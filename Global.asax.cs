using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace juggle_joy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string project_name = "Jugglejoy";
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ExecuteTaskServiceCallScheduler.StartAsync().GetAwaiter().GetResult();
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {

            if (Request.Cookies["firstname"] != null && Request.Cookies["uname"] != null && Request.Cookies["uid"] != null)
            {
                HttpContext.Current.Session["UserName"] = Request.Cookies["firstname"].Value;
                HttpContext.Current.Session["FirstName"] = Request.Cookies["uname"].Value;
                HttpContext.Current.Session["UserID"] = Convert.ToInt32(Request.Cookies["uid"].Value);
            }

            if (Request.Cookies["astfirstname"] != null && Request.Cookies["astname"] != null && Request.Cookies["astid"] != null && Request.Cookies["astlname"] != null)
            {
                HttpContext.Current.Session["AssistantName"] = Request.Cookies["astfirstname"].Value;
                HttpContext.Current.Session["AssistantFirstName"] = Request.Cookies["astname"].Value;
                HttpContext.Current.Session["AssistantID"] = Convert.ToInt32(Request.Cookies["astid"].Value);
                HttpContext.Current.Session["AssistantLastName"] = Request.Cookies["astlname"].Value;
            }

            if (Request.Cookies["hdlrfirstname"] != null && Request.Cookies["hdlrname"] != null && Request.Cookies["hdlrid"] != null && Request.Cookies["hdlrlname"] != null)
            {
                HttpContext.Current.Session["HandlerUserName"] = Request.Cookies["hdlrfirstname"].Value;
                HttpContext.Current.Session["HandlerFirstName"] = Request.Cookies["hdlrname"].Value;
                HttpContext.Current.Session["HandlerID"] = Convert.ToInt32(Request.Cookies["hdlrid"].Value);
                HttpContext.Current.Session["HandlerLastName"] = Request.Cookies["hdlrlname"].Value;
            }
        }
    }
}
