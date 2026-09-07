using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class BlogsController : Controller
    {
        // GET: admin/Blogs
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult addblog(int id = 0)

        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msgforblog = TempData["msgforblog"] == null ? "" : TempData["msgforblog"] as string;
                    ViewBag.msgforbloglist = TempData["msgforbloglist"] == null ? "" : TempData["msgforbloglist"] as string;
                    ViewBag.msgforbloglists = TempData["msgforbloglists"] == null ? "" : TempData["msgforbloglists"] as string;
                    if (id > 0)
                    {
                        BlogValidation blogs = new BlogValidation();
                        var blog_details = ctx.tbl_blogs.Where(x => x.BlogID == id).FirstOrDefault();
                        ViewBag.CategoryID = new SelectList(ctx.tbl_blogcategory.ToList(), "CategoryID", "Category", blog_details.CategoryID);

                        blogs.BlogID = blog_details.BlogID;
                        blogs.CategoryID = Convert.ToInt32(blog_details.CategoryID);
                        blogs.BlogTitle = blog_details.BlogTitle;
                        ViewBag.BlogImage = blog_details.BlogImage;
                        blogs.BlogImage = blog_details.BlogImage;
                        blogs.BlogDescription = blog_details.BlogDescription;
                        blogs.Date = blog_details.Date;
                        return View(blogs);
                    }
                    else
                    {
                        ViewBag.CategoryID = new SelectList(ctx.tbl_blogcategory.ToList(), "CategoryID", "Category");
                        return View(new BlogValidation());
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
        public ActionResult addblog(BlogValidation blog, int id = 0)
        {
            //ViewBag.msgforblog = TempData["msgforblog"] == null ? "" : TempData["msgforblog"] as string;
            //ViewBag.msgforbloglist = TempData["msgforbloglist"] == null ? "" : TempData["msgforbloglist"] as string;

            //ViewBag.msgforbloglists = TempData["msgforbloglists"] == null ? "" : TempData["msgforbloglists"] as string;
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (blog.BlogID > 0)
                    { 
                        var _blogs = ctx.tbl_blogs.FirstOrDefault(x => x.BlogID == blog.BlogID);

                        _blogs.BlogTitle = blog.BlogTitle;
                        _blogs.BlogDescription = blog.BlogDescription;
                        _blogs.CategoryID = blog.CategoryID;
                        _blogs.Date = DateTime.Now;


                        var file = Request.Files["BlogImage"];
                        string Imagefilename = "";

                        if (file != null && file.FileName != "" && file.ContentLength > 0)
                        {
                            //Edited 20-12-23

                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Blogs/Images/" + blog.BlogImage)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Blogs/Images/" + blog.BlogImage));
                            }
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Blogs/Icons/" + blog.BlogImage)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Blogs/Icons/" + blog.BlogImage));
                            }

                            //till here

                            Imagefilename = "Blog" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(file.FileName);

                            //----- Rezize image with aspect ratio for large images------//
                            System.Drawing.Image MainImg = System.Drawing.Image.FromStream(file.InputStream);
                            MainImg = GlobalMethods.FixedSize(MainImg, 1000, 600);  // pass image, width
                            MainImg.Save(Server.MapPath("~/Assets/Blogs/Images/" + Imagefilename));
                            MainImg.Dispose();


                            //----- Rezize image with aspect ratio for small images------//
                            System.Drawing.Image SubImg = System.Drawing.Image.FromStream(file.InputStream);
                            SubImg = GlobalMethods.FixedSize(SubImg, 366, 261);  // pass image, width
                            SubImg.Save(Server.MapPath("~/Assets/Blogs/Icons/" + Imagefilename));
                            SubImg.Dispose();

                            _blogs.BlogImage = Imagefilename;

                        }

                        ctx.Entry(_blogs).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            RouteData.Values.Remove("id");
                            TempData["msgforblog"] = "Blog  updated successfully";
                            return RedirectToAction("bloglist");
                        }
                        else
                        {
                            RouteData.Values.Remove("id");
                            TempData["msgforbloglist"] = "Unable to update Blog";
                            return RedirectToAction("list");
                        }
                    }
                    else
                    {
                        var _blogs = new tbl_blogs();

                        _blogs.BlogTitle = blog.BlogTitle;
                        _blogs.BlogDescription = blog.BlogDescription;
                        _blogs.CategoryID = blog.CategoryID;
                        _blogs.Date = DateTime.Now;

                        var file = Request.Files["BlogImage"];
                        string Imagefilename = "";

                        if (file != null && file.FileName != "" && file.ContentLength > 0)
                        {
                            Imagefilename = "Blog" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(file.FileName);

                            //----- Rezize image with aspect ratio ------//
                            System.Drawing.Image MainImg = System.Drawing.Image.FromStream(file.InputStream);
                            MainImg = GlobalMethods.FixedSize(MainImg, 1000, 600);  // pass image, width
                            MainImg.Save(Server.MapPath("~/Assets/Blogs/Images/" + Imagefilename));
                            MainImg.Dispose();

                            //----- Rezize image with aspect ratio for small images------//
                            System.Drawing.Image SubImg = System.Drawing.Image.FromStream(file.InputStream);
                            SubImg = GlobalMethods.FixedSize(SubImg, 366, 261);  // pass image, width
                            SubImg.Save(Server.MapPath("~/Assets/Blogs/Icons/" + Imagefilename));
                            SubImg.Dispose();
                            _blogs.BlogImage = Imagefilename;
                        }

                        ctx.tbl_blogs.Add(_blogs);
                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["msgforblog"] = "Blog added successfully";
                            return RedirectToAction("bloglist");
                        }
                        else
                        {
                            TempData["msgforbloglist"] = "Unable to Add Blog";
                            return RedirectToAction("addblog");
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

        public ActionResult bloglist()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.deletemsg = TempData["delete"];

                    ViewBag.msgforblog = TempData["msgforblog"] == null ? "" : TempData["msgforblog"] as string;
                    ViewBag.msgforbloglist = TempData["msgforbloglist"] == null ? "" : TempData["msgforbloglist"] as string;

                    ViewBag.msgforbloglists = TempData["msgforbloglists"] == null ? "" : TempData["msgforbloglists"] as string;
                    var bloglist = ctx.tbl_blogs.Include("tbl_blogcategory").ToList();
                    return View(bloglist);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult blog_details(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.title = "blog";
                    return View(ctx.tbl_blogs.Include("tbl_blogcategory").Where(x => x.BlogID == id).FirstOrDefault());
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { Returl = Request.Url.ToString() });
            }
        }
        [HttpPost]
        public ActionResult delete_blog_list(IEnumerable<decimal> IdsToDelete)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AdminID"] != null)
                {
                    if (IdsToDelete.Count() > 0)
                    {

                        foreach (var item in IdsToDelete)
                        {
                            var OldImagefilename = ctx.tbl_blogs.Where(x => x.BlogID == item).FirstOrDefault();
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Blogs/Images/" + OldImagefilename.BlogImage)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Blogs/Images/" + OldImagefilename.BlogImage));
                            }

                            var OldImagefilenames = ctx.tbl_blogs.Where(x => x.BlogID == item).FirstOrDefault();
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/Blogs/Icons/" + OldImagefilenames.BlogImage)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/Blogs/Icons/" + OldImagefilenames.BlogImage));
                            }
                        }


                        ctx.tbl_blogs.RemoveRange(ctx.tbl_blogs.Where(x => IdsToDelete.Contains(x.BlogID)));
                        ctx.SaveChanges();
                        TempData["delete"] = "Record deleted successfully";
                    }
                    return RedirectToAction("bloglist");
                }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        public JsonResult Isblogquestion_Exists(int BlogID, string BlogTitle)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                if (BlogID == 0)
                {
                    return Json(!ctx.tbl_blogs.Any(x => x.BlogTitle.ToLower().Trim() == BlogTitle.ToLower().Trim()), JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(!ctx.tbl_blogs.Any(x => x.BlogTitle.ToLower().Trim() == BlogTitle.ToLower().Trim() && x.BlogID != BlogID), JsonRequestBehavior.AllowGet);

                }
            }
        }

        #region blog category

    

        #region manage 

        public ActionResult manageblogcategory(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.blogcatmsg = TempData["addblogcat_msg"] == null ? "" : TempData["addblogcat_msg"] as string;
                    if (id > 0)
                    {
                        var tblcat = ctx.tbl_blogcategory.Where(x => x.CategoryID == id).FirstOrDefault();

                        BlogCategoryValidation cat = new BlogCategoryValidation();
                        cat.CategoryID = tblcat.CategoryID;
                        cat.Category = tblcat.Category;

                        return View(cat);
                    }
                    else
                    {
                        return View(new BlogCategoryValidation());
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

        public ActionResult manageblogcategory(BlogCategoryValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid == true)
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        if (obj.CategoryID > 0)
                        {
                            var cat = ctx.tbl_blogcategory.Where(x => x.CategoryID == obj.CategoryID).FirstOrDefault();

                            if (cat != null)
                            {
                                cat.Category = obj.Category.Trim();

                                ctx.Entry(cat).State = System.Data.Entity.EntityState.Modified;
                                if (ctx.SaveChanges() > 0)
                                {
                                    TempData["addblogcat_msg"] = " Record updated successfully!";

                                }
                                else
                                {
                                    TempData["addblogcat_msg"] = "Record not updated";

                                }

                            }
                            else
                            {
                                TempData["addblogcat_msg"] = "Category does not exist";

                            }
                            RouteData.Values.Remove("id");
                            return RedirectToAction("manageblogcategory");
                        }
                        else
                        {
                            tbl_blogcategory cat = new tbl_blogcategory();
                            cat.Category = obj.Category;

                            ctx.tbl_blogcategory.Add(cat);
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["addblogcat_msg"] = "New record added successfully";

                            }
                            else
                            {
                                TempData["addblogcat_msg"] = "Unable to add new record";

                            }
                            return RedirectToAction("manageblogcategory");
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

        #endregion

        #region IsCategoryNameExists

        public JsonResult IsCategoryNameExists(int CategoryID = 0, string Category = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (CategoryID == 0)
                {
                    //code for add method
                    if (!ctx.tbl_blogcategory.Any(x => x.Category.ToLower() == Category.Trim().ToLower()))
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
                    if (!ctx.tbl_blogcategory.Any(x => x.Category.ToLower() == Category.Trim().ToLower() && x.CategoryID != CategoryID))
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

        #region grid

        public ActionResult blogcategory_list()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.delete_inuse = TempData["delete_blgcatlistinuse"] == null ? "" : TempData["delete_blgcatlistinuse"] as string;
                ViewBag.delete_notinuse = TempData["delete_blgcatnotinuse"] == null ? "" : TempData["delete_blgcatnotinuse"] as string;
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var list = ctx.tbl_blogcategory.ToList();

                    return View(list);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }


        }


        #endregion

        #region delete

        [HttpPost]
        public ActionResult delete_blgcategory_list(IEnumerable<decimal> IdsToDelete)
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
                            var subcategory = ctx.tbl_blogs.Where(x => x.CategoryID == item).FirstOrDefault();
                            if (subcategory != null)
                            {
                                var cat = ctx.tbl_blogcategory.Where(x => x.CategoryID == subcategory.CategoryID).Select(x => x.Category).FirstOrDefault();
                                catlistinuse = catlistinuse + cat + ",";
                            }
                            else
                            {
                                var res = ctx.tbl_blogcategory.Where(x => x.CategoryID == item).FirstOrDefault();
                                catnotinuse = catnotinuse + res.Category + ",";
                                ctx.tbl_blogcategory.Remove(res);
                            }
                        }

                        if (catlistinuse != "")
                        {
                            catlistinuse = catlistinuse.TrimEnd(',');
                            TempData["delete_blgcatlistinuse"] = "Category name (" + catlistinuse + ") is/are in use and can't be deleted";
                        }
                        if (catnotinuse != "")
                        {
                            catnotinuse = catnotinuse.TrimEnd(',');
                            TempData["delete_blgcatnotinuse"] = "Category name (" + catnotinuse + ") is/are deleted";
                        }

                        ctx.SaveChanges();

                    }
                    return RedirectToAction("manageblogcategory");
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

    }
}