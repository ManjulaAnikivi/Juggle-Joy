
using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class bannerController : Controller
    {

        #region add-banner
        // GET: admin/banner
        public ActionResult addbanner(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["add_banner_msg"] == null ? "" : TempData["add_banner_msg"] as string;
                    ViewBag.deletemsg = TempData["deletemsg"] == null ? "" : TempData["deletemsg"] as string;
                    if (id > 0)
                    {
                        var main_banner = ctx.tbl_banner.Where(x => x.BannerId == id).FirstOrDefault();
                        BannerValidation _banner = new BannerValidation();
                        _banner.BannerId = main_banner.BannerId;
                        _banner.BannerVideo = main_banner.BannerVideo;
                        _banner.BannerText = main_banner.BannerText;
                        _banner.UrlLink = main_banner.UrlLink;
                        _banner.Description = main_banner.Description;
                        return View(_banner);
                    }
                    else
                    {
                        return View(new BannerValidation());
                    }
                }

            }
            else
           {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult addbanner(BannerValidation main_banner, int id = 0)
        {
           

                if (Session["adminid"] != null)
                {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var postedFile = Request.Files["BannerVideo"];

                    string Videofilename = "";
                    if (id == 0)
                    {
                        if (postedFile != null && postedFile.FileName != "" && postedFile.ContentLength > 0)
                        {
                            tbl_banner _banner = new tbl_banner();

                            _banner.ContentType = postedFile.ContentType;

                            Videofilename = "br" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(postedFile.FileName);

                            _banner.BannerVideo = Path.GetFileName(Videofilename);

                            postedFile.SaveAs(Server.MapPath("~/Assets/Videofiles/" + Videofilename));

                            _banner.BannerText = main_banner.BannerText;
                            _banner.UrlLink = main_banner.UrlLink;
                            _banner.Description = main_banner.Description;

                            ctx.tbl_banner.Add(_banner);

                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["add_banner_msg"] = "Banner added successfully";
                            }
                            else
                            {
                                TempData["add_banner_msg"] = "Failed to add Banner";
                            }
                        }
                        else
                        {
                            TempData["add_banner_msg"] = "Please choose banner image";
                        }
                    }
                    else
                    {

                        tbl_banner _banner = ctx.tbl_banner.Where(x => x.BannerId == id).FirstOrDefault();

                        if (postedFile != null && postedFile.FileName != "" && postedFile.ContentLength > 0)
                        {
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Videofiles/" + _banner.BannerVideo)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Videofiles/" + _banner.BannerVideo));
                            }
                            _banner.ContentType = postedFile.ContentType;

                            Videofilename = "br" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(postedFile.FileName);

                            _banner.BannerVideo = Path.GetFileName(Videofilename);

                            postedFile.SaveAs(Server.MapPath("~/Assets/Videofiles/" + Videofilename));

                        }
                        _banner.BannerText = main_banner.BannerText;
                        _banner.UrlLink = main_banner.UrlLink;
                        _banner.Description = main_banner.Description;

                        ctx.Entry(_banner).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["add_banner_msg"] = "Banner updated successfully";
                        }
                        else
                        {
                            TempData["add_banner_msg"] = "Failed to update Banner Details";
                        }
                    }
                    RouteData.Values.Remove("id");
                    return RedirectToAction("addbanner");

                }
            }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("Login", "Account", new { RetUrl = Request.Url.ToString() });
                }

           

        }
        #endregion

        #region banner-list

        public ActionResult banner_List()
        {
            //if (Session["AdminID"] != null)
            //{
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var bannerlist = ctx.tbl_banner.OrderByDescending(x => x.BannerId).ToList();
                    return View(bannerlist);

                }
            //}
            //else
            //{
            //    TempData["AdminLogInStatus"] = "Please login here";
            //    return RedirectToAction("Login", "Account", new { RetUrl = Request.Url.ToString() });
            //}


        }
        [HttpPost]
        public ActionResult delete_banner_list(IEnumerable<decimal> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {
                        //Deletting Image
                        foreach (var item in IdsToDelete)
                        {
                            var OldImagefilename = ctx.tbl_banner.Where(x => x.BannerId == item).FirstOrDefault();
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Videofiles/" + OldImagefilename.BannerVideo)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Videofiles/" + OldImagefilename.BannerVideo));
                            }
                        }
                        ctx.tbl_banner.RemoveRange(ctx.tbl_banner.Where(x => IdsToDelete.Contains(x.BannerId)));
                        ctx.SaveChanges();
                        TempData["deletemsg"] = "Record deleted successfully";
                    }
                    return RedirectToAction("addbanner");
                }
                else
                {
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
        }

        //public JsonResult update_banner_preference(string bnrlist = "")
        //{
        //    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
        //    {
        //        bnrlist = bnrlist.TrimStart(',');
        //        var bnrids = bnrlist.Split(',').Select(int.Parse).ToList();
        //        for (int i = 0; i < bnrids.Count(); i++)
        //        {
        //            int bnrid = bnrids[i];
        //            var banner = ctx.tbl_banner.Where(x => x.BannerId == bnrid).FirstOrDefault();
        //            banner.SortPreference = i;
        //            ctx.Entry(banner).State = System.Data.Entity.EntityState.Modified;
        //        }
        //        if (ctx.SaveChanges() > 0)
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(false, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        #endregion
    }
}