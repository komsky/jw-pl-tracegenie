using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceGenie.Client.Models;
using HtmlAgilityPack;
using System.Linq;
using TraceGenie.Client;

namespace TraceGenie.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileContent = File.ReadAllText("sampleEntries.html");
            List<TraceGenieEntry> lista = ConvertToTraceGenieEntries(fileContent);

            foreach (var item in lista)
            {
                System.Console.WriteLine($"Full name: {item.FullName}, Address: {item.Address}");
            }

            System.Console.ReadKey();
        }

        private static List<TraceGenieEntry> ConvertToTraceGenieEntries(string fileContent)
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
                    var name = body.SelectSingleNode("div/div/h2").InnerText.ClearFromNbsps();

                    var address = body.SelectSingleNode("div/div/table/tbody/tr/td").InnerHtml.ClearFromTags();

                    entries.Add(new TraceGenieEntry { FullName = name, Address = address });
                }
                

            }
            return entries;
        }

        private static string Cleanup(string fileContent)
        {
            return fileContent.Replace("<!DOCTYPE html>", "");
        }
    }
}
