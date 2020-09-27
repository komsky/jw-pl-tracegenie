using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using TraceGenie.Client;
using TraceGenie.Web.Models;

namespace TraceGenie.Web.Controllers
{
    public class HomeController : Controller
    {
        TraceGenieClient _client;

        static List<string> PolskieImiona;
        static HomeController()
        {
            PolskieImiona = System.IO.File.ReadAllLines(HostingEnvironment.MapPath(@"~/App_Data/lista_polskich_imion.txt")).Select(x=>x.ToLower()).ToList();
        }
        public ActionResult Index()
        {
            if (GetAuthCookie())
            {
                return Redirect("/login");
            }
            return View(new AuthModel());
        }

        private bool GetAuthCookie()
        {
            return Request.Cookies["authCookie"]?.Values["IsAuthorized"] == "true";
        }

        [HttpPost]
        public ActionResult Index(string password)
        {
            if (password == ConfigurationManager.AppSettings["AppPassword"])
            {
                SetAuthCookie();
                return Redirect("/login");
            }
            else if (GetAuthCookie())
            {
                return Redirect("/login");
            }
            return View(new AuthModel { ErrorMessage = "Nieprawidłowe hasło" });
        }

        private void SetAuthCookie()
        {
            HttpCookie authCookie = new HttpCookie("authCookie");
            authCookie["IsAuthorized"] = "true";
            authCookie.Expires.Add(new TimeSpan(30, 0, 0, 0, 0));
            Response.Cookies.Add(authCookie);
        }

        [AppAuthorize]
        public ActionResult Login()
        {
            return View(new LoginModel());
        }

        [AppAuthorize]
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                ObjectCache cache = MemoryCache.Default;
                var userHash = GetUserHash(model);
                if (cache.Contains(userHash))
                {
                    _client = cache[userHash] as TraceGenieClient;
                }
                else
                {
                    _client = new TraceGenieClient();
                    var loginSuccessful = await _client.Login(model.Login, model.Password);

                    if (!loginSuccessful)
                    {
                        model.LoginError = !loginSuccessful;
                        return View(model);
                    }
                    else
                    {
                        cache.Add(userHash, _client, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(10) });
                        
                    }
                }
                SetGenieCookie(userHash);
                return Redirect("/search");

            }
            return View(model);
        }

        private void SetGenieCookie(string userHash)
        {
            HttpCookie authCookie = new HttpCookie("genieCookie");
            authCookie.Value = userHash;
            authCookie.Expires.Add(new TimeSpan(0, 10, 0));
            Response.Cookies.Add(authCookie);
        }

        private string GetUserHash(LoginModel model)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(model.Login + model.Password));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        [AppAuthorize]
        public async Task<ActionResult> Search()
        {
            var userHash = GetGenieCookie();
            if (string.IsNullOrEmpty(userHash))
            {
                return Redirect("/login");
            }

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(userHash))
            {
                _client = cache[userHash] as TraceGenieClient;
                SetGenieCookie(userHash);
            }
            else
            {
                return Redirect("/login");
            }

            return View(new SearchModel());
        }

        [AppAuthorize]
        [HttpPost]
        public async Task<ActionResult> Search(SearchModel model)
        {
            var userHash = GetGenieCookie();
            if (string.IsNullOrEmpty(userHash))
            {
                return Redirect("/login");
            }

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(userHash))
            {
                _client = cache[userHash] as TraceGenieClient;
                SetGenieCookie(userHash);
            }
            else
            {
                return Redirect("/login");
            }
            if (ModelState.IsValid)
            {
                var result = await _client.MultiPostCodeSearchSingleYear(model.PostCode, "2020");
                if (model.FilterPolishNames)
                {
                    result = result.Where(x => PolskieImiona.Any(x.FullName.ToLower().Contains)).ToList();
                }
                return View("Found", new FoundModel { PostCode = model.PostCode, Records = result, RecordsFound = result.Count });
            }
            return View(model);
        }

        private string GetGenieCookie()
        {
            return Request.Cookies["genieCookie"]?.Value;
        }
    }
}