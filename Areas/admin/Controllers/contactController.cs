using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class contactController : Controller
    {
        // GET: admin/contact
        public ActionResult Index()
        {
            return View();
        }

        #region contact_us
        public ActionResult list()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["deletemsg"] == null ? "" : TempData["deletemsg"] as string;
                    return View(ctx.tbl_contact.OrderByDescending(x => x.ContactID).ToList());
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult contact_details(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    Session["id"] = id.ToString();
                    return View(ctx.tbl_contact.Where(x => x.ContactID == id).FirstOrDefault());
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { Returl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult delete_contact_list(IEnumerable<decimal> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {
                        ctx.tbl_contact.RemoveRange(ctx.tbl_contact.Where(x => IdsToDelete.Contains(x.ContactID)));
                        ctx.SaveChanges();
                        TempData["deletemsg"] = "Record deleted successfully";
                    }
                    return RedirectToAction("list");
                }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        #endregion

        #region subscribers
        public ActionResult subscriber()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["deletemsg"] == null ? "" : TempData["deletemsg"] as string;
                    return View(ctx.tbl_subscriber.OrderByDescending(x => x.SubscriberId).ToList());
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult delete_subscriber(IEnumerable<decimal> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {
                        ctx.tbl_subscriber.RemoveRange(ctx.tbl_subscriber.Where(x => IdsToDelete.Contains(x.SubscriberId)));
                        ctx.SaveChanges();
                        TempData["deletemsg"] = "Record deleted successfully";
                    }
                    return RedirectToAction("subscriber");
                }
                else
                {

                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        #endregion
    }
}