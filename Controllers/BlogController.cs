using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace juggle_joy.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog

        public ActionResult Blogs()
        {

            return View();

        }
        public static class IpAddressHelper
        {
            public static string GetClientIpAddress(HttpRequestBase request)
            {
                string ipAddress = null;

                try
                {
                    ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        // In case of multiple addresses, get the first one
                        string[] addresses = ipAddress.Split(',');
                        if (addresses.Length != 0)
                        {
                            ipAddress = addresses[0];
                        }
                    }

                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = request.ServerVariables["REMOTE_ADDR"];
                    }

                    // Additional checks for local requests or IPv6 addresses
                    if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
                    {
                        ipAddress = Dns.GetHostAddresses(Dns.GetHostName())
                                       .FirstOrDefault(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                                       .ToString();
                    }
                }
                catch
                {
                    ipAddress = "Unable to determine IP address";
                }

                return ipAddress;
            }
        }
        public ActionResult BlogDetail(int id=0)
        {
            using(db_jugglejoyEntities ctx= new db_jugglejoyEntities())
            {
                // Getting ip address using host name 
                string clientIpAddress = IpAddressHelper.GetClientIpAddress(Request);

                var tblviews = ctx.tbl_blogviews.Where(x => x.IPaddress == clientIpAddress && x.BlogID==id).FirstOrDefault();

                if (tblviews == null)
                {
                    tbl_blogviews views = new tbl_blogviews();
                    views.BlogID= id;
                    views.IPaddress= clientIpAddress;
                    ctx.tbl_blogviews.Add(views);
                    ctx.SaveChanges();
                }
              

                var blog_detail=ctx.tbl_blogs.Where(x=>x.BlogID==id).FirstOrDefault();
                BlogValidation blg=new BlogValidation();
                blg.BlogID= blog_detail.BlogID;
                blg.CategoryID = blog_detail.CategoryID;
                blg.BlogTitle= blog_detail.BlogTitle;
                blg.BlogImage= blog_detail.BlogImage;
                blg.BlogDescription= blog_detail.BlogDescription;
                return View(blg);

            }
        }


        public ActionResult blogscategorylist(int id=0)
        {
            using(db_jugglejoyEntities ctx=new db_jugglejoyEntities())
            {
                var list = ctx.tbl_blogs.Include("tbl_blogcategory").Where(x => x.CategoryID == id).ToList();
                return View(list);
            }
        }
    }
}