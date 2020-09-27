using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TraceGenie.Client.Interfaces;
using TraceGenie.Client.Models;

namespace TraceGenie.Client
{
    public class TraceGenieClient : ITraceGenieClient, IDisposable
    {
        //http://www.tracegenie.com/amember4/amember/1DAY/allpcs.php?s=100&q6=EN4%208sj

        bool _isLoggedIn;
        HttpClient _client;
        string _initialPage = "https://www.tracegenie.com/";
        string _loginLocation = "https://www.tracegenie.com/amember4/amember/member";

        public TraceGenieClient()
        {
            _client = new HttpClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _isLoggedIn = false;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<bool> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Nie podałeś użytkownika lub hasła");
            }
            var response = await _client.GetAsync(_initialPage);

            response.EnsureSuccessStatusCode();

            var formContent = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("amember_login", username),
                                    new KeyValuePair<string, string>("amember_pass", password)
                                });

            var loginResult = await _client.PostAsync(_loginLocation, formContent);

            loginResult.EnsureSuccessStatusCode();

            loginResult = await _client.GetAsync(_loginLocation);

            loginResult.EnsureSuccessStatusCode();

            var loginContent = await loginResult.Content.ReadAsStringAsync();
            if (loginContent.Contains("Your Membership Information"))
            {
                _isLoggedIn = true;
                return true;
            }
            return false;
        }

        public async Task<List<TraceGenieEntry>> SearchAllYears(string postcode)
        {
            if (!_isLoggedIn)
            {
                throw new NotLoggedInException();
            }

            if (postcode.Contains(Environment.NewLine)) return await MultiPostCodeSearchAllYears(postcode);

            string encodedPostCode = WebUtility.HtmlEncode(postcode);
            int position = 0;
            var list = new List<TraceGenieEntry>();

            var entries = new List<TraceGenieEntry>();

            do
            {
                string searchPage = $"http://www.tracegenie.com/amember4/amember/1DAY/allpcs.php?s={position}&q6={encodedPostCode}";

                var searchResult = await _client.GetAsync(searchPage);
                searchResult.EnsureSuccessStatusCode();

                entries = ConvertToTraceGenieEntries(await searchResult.Content.ReadAsStringAsync());
                list.AddRange(entries);
                position += 10;
            } while (entries.Count == 10);
            
            return list;
        }

        public async Task<List<TraceGenieEntry>> MultiPostCodeSearchAllYears(string postcode)
        {
            var postcodes = postcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var fullList = new List<TraceGenieEntry>();

            foreach (var code in postcodes)
            {
                fullList.AddRange(await SearchForAddresses(code));
            }

            return fullList;
        }

        public async Task<List<TraceGenieEntry>> MultiPostCodeSearchSingleYear(string postcode, string year)
        {
            var postcodes = postcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var fullList = new List<TraceGenieEntry>();

            foreach (var code in postcodes)
            {
                fullList.AddRange(await SearchForAddressesSingleYear(code, year));
            }

            return fullList;
        }

        public async Task<List<TraceGenieEntry>> SearchForAddresses(string postcode)
        {
            string encodedPostCode = WebUtility.HtmlEncode(postcode);
            int position = 0;
            var list = new List<TraceGenieEntry>();

            var entries = new List<TraceGenieEntry>();

            do
            {
                string searchPage = $"http://www.tracegenie.com/amember4/amember/1DAY/allpcs.php?s={position}&q6={encodedPostCode}";

                var searchResult = await _client.GetAsync(searchPage);
                searchResult.EnsureSuccessStatusCode();

                entries = ConvertToTraceGenieEntries(await searchResult.Content.ReadAsStringAsync());
                ExtractPostcodeToProperty(postcode, entries);
                list.AddRange(entries);
                position += 10;
            } while (entries.Count == 10);

            return list;

        }

        private static void ExtractPostcodeToProperty(string postcode, List<TraceGenieEntry> entries)
        {
            foreach (var entry in entries)
            {
                entry.PostCode = postcode;
                entry.Address = entry.Address.Remove(entry.Address.IndexOf(postcode), postcode.Length);
            }
        }

        public async Task<List<TraceGenieEntry>> SearchForAddressesSingleYear(string postcode, string year)
        {
            string encodedPostCode = WebUtility.HtmlEncode(postcode);
            int position = 0;
            var list = new List<TraceGenieEntry>();

            var entries = new List<TraceGenieEntry>();

            do
            {
                string searchPage = GetSearchPage(encodedPostCode, position, year);

                var searchResult = await _client.GetAsync(searchPage);
                searchResult.EnsureSuccessStatusCode();
                entries = ConvertToTraceGenieEntriesSingleYear(await searchResult.Content.ReadAsStringAsync());
                ExtractPostcodeToProperty(postcode, entries);
                list.AddRange(entries);
                position += 10;
            } while (entries.Count == 10);

            return list;

        }

        private static string GetSearchPage(string encodedPostCode, int position, string year)
        {
            string baseAddres = "http://www.tracegenie.com/amember4/amember/1DAY/";
            switch (year)
            {
                case "2018":
                    return $"{baseAddres}postcode.php?s={position}&q6={encodedPostCode}&D99=2018vr";
                case "2017":
                    return $"{baseAddres}postcode.php?s={position}&q6={encodedPostCode}&D89=2017vr";
                case "2020":
                    return $"{baseAddres}postcode.php?s={position}&q6={encodedPostCode}&D119=2020vr";
                case "ALL":
                default:
                    return $"{baseAddres}allpcs.php?s={position}&q6={encodedPostCode}";
            }
            
        }

        public List<TraceGenieEntry> ConvertToTraceGenieEntries(string fileContent)
        {
            fileContent = Cleanup(fileContent);

            var doc = new HtmlDocument();
            doc.LoadHtml(fileContent);
            var htmls = doc.DocumentNode.ChildNodes.Where(x => x.Name == "html").ToList();
            var entries = new List<TraceGenieEntry>();

            foreach (var item in htmls)
            {
                var body = item.ChildNodes.SingleOrDefault(x => x.Name == "body");
                if (body != null)
                {
                    var name = body.SelectSingleNode("div/div/h2").InnerText.ClearFromNbsps().ClearFromDoubleSpaces();

                    var address = body.SelectSingleNode("div/div/table/tbody/tr/td").InnerHtml.ClearFromTags();

                    entries.Add(new TraceGenieEntry { FullName = name, Address = address });
                }


            }
            return entries;
        }
        public List<TraceGenieEntry> ConvertToTraceGenieEntriesSingleYear(string fileContent)
        {
            fileContent = Cleanup(fileContent);

            var doc = new HtmlDocument();
            doc.LoadHtml(fileContent);
            var mainElement = doc.DocumentNode.ChildNodes.Single(x => x.Name == "font");
            var elements = mainElement.SelectNodes("//div[contains(@class, 'panel panel-default')]/div"); ///div[contains(@class, 'container')
            var entries = new List<TraceGenieEntry>();
            if (elements == null)
            {
                return entries;
            }

            foreach (var entry in elements)
            {
                if (entry != null)
                {
                    var name = entry.SelectSingleNode("h2").InnerText?.ClearFromNbsps();

                    var address = entry.SelectSingleNode("table/tbody/tr/td").InnerHtml.ClearFromTags();

                    entries.Add(new TraceGenieEntry { FullName = name, Address = address });
                }
                
            }
            return entries;
        }

        private static string Cleanup(string fileContent)
        {
            return fileContent.Replace("<!DOCTYPE html>", "");
        }

        public async Task<List<TraceGenieEntry>> SearchSingleYear(string postcode, string year)
        {

            if (!_isLoggedIn)
            {
                throw new NotLoggedInException();
            }

            if (postcode.Contains(Environment.NewLine)) return await MultiPostCodeSearchAllYears(postcode);

            string encodedPostCode = WebUtility.HtmlEncode(postcode);
            int position = 0;
            var list = new List<TraceGenieEntry>();

            var entries = new List<TraceGenieEntry>();

            do
            {
                string searchPage = $"http://www.tracegenie.com/amember4/amember/1DAY/allpcs.php?s={position}&q6={encodedPostCode}";

                var searchResult = await _client.GetAsync(searchPage);
                searchResult.EnsureSuccessStatusCode();

                entries = ConvertToTraceGenieEntries(await searchResult.Content.ReadAsStringAsync());
                ExtractPostcodeToProperty(postcode, entries);
                list.AddRange(entries);
                position += 10;
            } while (entries.Count == 10);

            return list;
        }
    }
}
