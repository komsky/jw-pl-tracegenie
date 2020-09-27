using System;
using System.Web;
using System.Web.Mvc;

namespace TraceGenie.Web
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Request.Cookies["authCookie"]?.Values["IsAuthorized"] == "true";

        }
    }
}
