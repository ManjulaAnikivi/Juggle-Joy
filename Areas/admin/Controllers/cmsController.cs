using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class cmsController : Controller
    {
        // GET: admin/faq
        #region FAQ Category Management

        public ActionResult Faq_category(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.catmsg = TempData["addcat_msg"] == null ? "" : TempData["addcat_msg"] as string;
                    if (id > 0)
                    {
                        var tblcat = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == id).FirstOrDefault();

                        Faq_CategoryValidation cat = new Faq_CategoryValidation();
                        cat.Faq_Cat_id = tblcat.Faq_Cat_id;
                        cat.Category = tblcat.Category;

                        return View(cat);
                    }
                    else
                    {
                        return View(new Faq_CategoryValidation());
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
        [ValidateAntiForgeryToken]
        public ActionResult Faq_category(Faq_CategoryValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid == true)
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        if (obj.Faq_Cat_id > 0)
                        {
                            var cat = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == obj.Faq_Cat_id).FirstOrDefault();

                            if (cat != null)
                            {
                                cat.Category = obj.Category.Trim();

                                ctx.Entry(cat).State = System.Data.Entity.EntityState.Modified;
                                if (ctx.SaveChanges() > 0)
                                {
                                    TempData["addcat_msg"] = " Record updated successfully!";

                                }
                                else
                                {
                                    TempData["addcat_msg"] = "Record not updated";

                                }

                            }
                            else
                            {
                                TempData["addcat_msg"] = "Category does not exist";

                            }
                            RouteData.Values.Remove("id");
                            return RedirectToAction("Faq_category");
                        }
                        else
                        {
                            tbl_faq_category cat = new tbl_faq_category();
                            cat.Category = obj.Category;

                            ctx.tbl_faq_category.Add(cat);
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["addcat_msg"] = "New record added successfully";

                            }
                            else
                            {
                                TempData["addcat_msg"] = "Unable to add new record";

                            }
                            return RedirectToAction("Faq_category");
                        }
                    }
                }
                else
                {
                    return View(obj);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });

            }
        }


        public ActionResult Faq_categortlist()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.delete_inuse = TempData["delete_catlistinuse"] == null ? "" : TempData["delete_catlistinuse"] as string;
                ViewBag.delete_notinuse = TempData["delete_catnotinuse"] == null ? "" : TempData["delete_catnotinuse"] as string;
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var list = ctx.tbl_faq_category.ToList();

                    return View(list);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }     

      

        #region IsCategoryNameExists

        public JsonResult IsFAQCategoryNameExists(int Faq_Cat_id = 0, string Category = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Faq_Cat_id == 0)
                {
                    //code for add method
                    if (!ctx.tbl_faq_category.Any(x => x.Category.ToLower() == Category.Trim().ToLower()))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    //code for edit
                    if (!ctx.tbl_faq_category.Any(x => x.Category.ToLower() == Category.Trim().ToLower() && x.Faq_Cat_id != Faq_Cat_id))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        #endregion

        #region delete

        [HttpPost]
        public ActionResult delete_faqcategory_list(IEnumerable<decimal> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {

                        var catlistinuse = "";
                        var catnotinuse = "";
                        foreach (var item in IdsToDelete)
                        {
                            //var subcategory = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == item).FirstOrDefault();
                            //if (subcategory != null)
                            //{
                            //    var cat = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == subcategory.Faq_Cat_id).Select(x => x.Category).FirstOrDefault();
                            //    catlistinuse = catlistinuse + cat + ",";
                            //}
                            //else
                            //{
                            //    var res = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == item).FirstOrDefault();
                            //    catnotinuse = catnotinuse + res.Category + ",";
                            //    ctx.tbl_faq_category.Remove(res);
                            //}

                            var faq = ctx.tbl_faq.Where(x => x.Faq_categoryID == item).FirstOrDefault();
                            if (faq != null)
                            {
                                var cat = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == faq.Faq_categoryID).Select(x => x.Category).FirstOrDefault();
                                catlistinuse = catlistinuse + cat + ",";
                            }
                            else
                            {
                                var res = ctx.tbl_faq_category.Where(x => x.Faq_Cat_id == item).FirstOrDefault();
                                catnotinuse = catnotinuse + res.Category + ",";
                                ctx.tbl_faq_category.Remove(res);
                            }
                        }

                        if (catlistinuse != "")
                        {
                            catlistinuse = catlistinuse.TrimEnd(',');
                            TempData["delete_catlistinuse"] = "Category name (" + catlistinuse + ") is/are in use and can't be deleted";
                        }
                        if (catnotinuse != "")
                        {
                            catnotinuse = catnotinuse.TrimEnd(',');
                            TempData["delete_catnotinuse"] = "Category name (" + catnotinuse + ") is/are deleted";
                        }

                        ctx.SaveChanges();

                    }
                    return RedirectToAction("Faq_category");
                }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        #endregion

        #endregion


        #region Add-FAQ
        public ActionResult manage_faq(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    ViewBag.msgforfaq = TempData["msgforfaq"] == null ? "" : TempData["msgforfaq"] as string;
                    ViewBag.msgforfaqlist = TempData["msgforfaqlist"] == null ? "" : TempData["msgforfaqlist"] as string;
                    ViewBag.msgforfaqlists = TempData["msgforfaqlists"] == null ? "" : TempData["msgforfaqlists"] as string;
                    if (id > 0)
                    {
                        FaqValidation faq = new FaqValidation();
                        var faq_details = ctx.tbl_faq.Where(x => x.FaqID == id).FirstOrDefault();
                        faq.FaqID = faq_details.FaqID;
                        faq.FaqQuestion = faq_details.FaqQuestion;
                        faq.FaqAnswer = faq_details.FaqAnswer;
                        ViewBag.Faq_Cat_id = new SelectList(ctx.tbl_faq_category.ToList(), "Faq_Cat_id", "Category", faq_details.Faq_categoryID);

                        return View(faq);

                    }
                    else
                    {
                        ViewBag.Faq_Cat_id = new SelectList(ctx.tbl_faq_category.ToList(), "Faq_Cat_id", "Category");
                        return View(new FaqValidation());
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
        public ActionResult manage_faq(FaqValidation _faq)
        {

            ViewBag.msgforfaq = TempData["msgforfaq"] == null ? "" : TempData["msgforfaq"] as string;
            ViewBag.msgforfaqlist = TempData["msgforfaqlist"] == null ? "" : TempData["msgforfaqlist"] as string;

            ViewBag.msgforfaqlists = TempData["msgforfaqlists"] == null ? "" : TempData["msgforfaqlists"] as string;
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {


                    if (_faq.FaqID > 0)
                    {
                        var _fq = ctx.tbl_faq.FirstOrDefault(x => x.FaqID == _faq.FaqID);

                        _fq.FaqQuestion = _faq.FaqQuestion;
                        _fq.FaqAnswer = _faq.FaqAnswer;
                        _fq.Faq_categoryID = Convert.ToInt32(_faq.Faq_Cat_id);

                        ctx.Entry(_fq).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            RouteData.Values.Remove("id");
                            TempData["msgforfaq"] = "Faq  updated successfully";
                            return RedirectToAction("faq_list");
                        }
                        else
                        {
                            RouteData.Values.Remove("id");
                            TempData["msgforfaqlist"] = "Unable to update Faq";
                            return RedirectToAction("faq_list");
                        }
                    }
                    else
                    {
                        var _fa = new tbl_faq();

                        _fa.FaqQuestion = _faq.FaqQuestion;
                        _fa.FaqAnswer = _faq.FaqAnswer;
                        _fa.Faq_categoryID = Convert.ToInt32(_faq.Faq_Cat_id);
                        ctx.tbl_faq.Add(_fa);
                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["msgforfaq"] = "Faq added successfully";
                            return RedirectToAction("faq_list");
                        }
                        else
                        {
                            TempData["msgforfaqlist"] = "Unable to add Faq";
                            return RedirectToAction("manage_faq");
                        }
                    }
                }

            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult faq_list()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msgforfaq = TempData["msgforfaq"] == null ? "" : TempData["msgforfaq"] as string;
                    ViewBag.msgforfaqlist = TempData["msgforfaqlist"] == null ? "" : TempData["msgforfaqlist"] as string;

                    ViewBag.msgforfaqlists = TempData["msgforfaqlists"] == null ? "" : TempData["msgforfaqlists"] as string;
                    var faqlist = ctx.tbl_faq.ToList();
                    return View(faqlist);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult delete_faq(IEnumerable<int> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {
                        foreach (var ids in IdsToDelete)
                        {
                            var res = ctx.tbl_faq.Where(x => x.FaqID == ids).FirstOrDefault();
                            ctx.tbl_faq.Remove(res);
                            ctx.SaveChanges();
                        }
                        TempData["msgforfaqlists"] = "Records deleted successfully";
                        return RedirectToAction("faq_list");
                    }
                    return RedirectToAction("faq_list");
                }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }

            }


        }


        public JsonResult Isfaqquestion_Exists(int FaqID, string FaqQuestion)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                if (FaqID == 0)
                {
                    return Json(!ctx.tbl_faq.Any(x => x.FaqQuestion.ToLower().Trim() == FaqQuestion.ToLower().Trim()), JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(!ctx.tbl_faq.Any(x => x.FaqQuestion.ToLower().Trim() == FaqQuestion.ToLower().Trim() && x.FaqID != FaqID), JsonRequestBehavior.AllowGet);

                }
            }
        }

        public JsonResult update_preference(string faqlist = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                faqlist = faqlist.TrimStart(',');
                var faqlistids = faqlist.Split(',').ToList();
                for (int i = 0; i < faqlistids.Count(); i++)
                {
                    int faqid = Convert.ToInt32(faqlistids[i]);
                    var faq = ctx.tbl_faq.Where(x => x.FaqID == faqid).FirstOrDefault();
                    faq.SortPreference = i;
                    ctx.Entry(faq).State = System.Data.Entity.EntityState.Modified;
                }
                if (ctx.SaveChanges() > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        #region cms

        public ActionResult manage(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.cmsmsg = TempData["cmsmsg"] == null ? "" : TempData["cmsmsg"] as string;

                    if (id > 0)
                    {
                        CMSValidation cms = new CMSValidation();

                        var pagecontent = ctx.tbl_cms.Where(x => x.CmsID == id).FirstOrDefault();

                        cms.CmsID = id;
                        cms.CmsText = pagecontent.CmsText;
                        ViewBag.CmsID = new SelectList(ctx.tbl_cms.Where(x => x.CmsID != null).ToList(), "CmsID", "CmsPageName", pagecontent.CmsID);
                        return View(cms);
                    }
                    else
                    {
                        var pagecontent = ctx.tbl_cms.ToList();
                        ViewBag.CmsID = new SelectList(pagecontent, "CmsID", "CmsPageName");

                        return View(new CMSValidation());
                    }
                }
            }
            else
            {
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult manage(CMSValidation cms)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid)
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        if (cms.CmsID > 0)
                        {
                            tbl_cms pagecontent = ctx.tbl_cms.Where(x => x.CmsID == cms.CmsID).FirstOrDefault();
                            pagecontent.CmsText = cms.CmsText;
                            //pagecontent.CmsText_DE = cms.CmsText_DE;

                            ctx.Entry(pagecontent).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                ViewBag.cmsmsg = TempData["cmsmsg"] = "Content updated successfully!";
                                RouteData.Values.Remove("id");
                                return RedirectToAction("manage", "cms");
                            }
                            else
                            {
                                ViewBag.cmsmsg = TempData["cmsmsg"] = "Content not Updated";
                                return View(new CMSValidation());
                            }
                        }
                        else
                        {
                            tbl_cms pagecontent = new tbl_cms();
                            pagecontent.CmsText = cms.CmsText;
                            //pagecontent.CmsText_DE = cms.CmsText_DE;
                            ctx.tbl_cms.Add(pagecontent);
                            if (ctx.SaveChanges() > 0)
                            {
                                ViewBag.cmsmsg = TempData["cmsmsg"] = "Content added successfully!";
                                RouteData.Values.Remove("id");
                                return RedirectToAction("manage", "cms");
                            }
                            else
                            {
                                ViewBag.cmsmsg = TempData["cmsmsg"] = "Content adding failed";
                                return View(new CMSValidation());
                            }
                        }
                    }
                }
                else
                {
                    TempData["cmsmsg"] = "Failed to add content";
                    return RedirectToAction("manage");
                }
            }
            else
            {
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public JsonResult _GetPageContent(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var pgcontent = ctx.tbl_cms.Where(x => x.CmsID == id).FirstOrDefault();
                return Json(pgcontent, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}