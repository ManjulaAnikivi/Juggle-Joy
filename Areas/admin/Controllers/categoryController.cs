using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static juggle_joy.Models.GlobalMethods;

namespace juggle_joy.Areas.admin.Controllers
{
    public class categoryController : Controller
    {
        // GET: admin/category


        #region category


        #region manage

        public ActionResult manage(int id=0)
        {
            if (Session["AdminID"] != null)
            {
                using(db_jugglejoyEntities ctx=new db_jugglejoyEntities())
                {
                    ViewBag.catmsg = TempData["addcat_msg"] == null ? "" : TempData["addcat_msg"] as string;
                    if (id > 0)
                    {
                        var tblcat=ctx.tbl_category.Where(x=>x.CategoryID==id).FirstOrDefault();

                        CategoryValidation cat = new CategoryValidation();
                        cat.CategoryID = tblcat.CategoryID;
                        cat.Category = tblcat.Category;
                        cat.CategoryImage=tblcat.CategoryImage;
                        return View(cat);
                    }
                    else
                    {
                        return View(new CategoryValidation());
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

        public ActionResult manage(CategoryValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid == true)
                {
                    using(db_jugglejoyEntities ctx=new db_jugglejoyEntities())
                    {

                        if (obj.CategoryID > 0)
                        {
                            var cat=ctx.tbl_category.Where(x=>x.CategoryID==obj.CategoryID).FirstOrDefault();

                            if (cat != null)
                            {
                                cat.Category=obj.Category.Trim();


                                var file = Request.Files["CategoryImage"];
                                string Imagefilename = "";

                                if (file != null && file.FileName != "" && file.ContentLength > 0)
                                {
                                    //Edited 20-12-23

                                    if (System.IO.File.Exists(Server.MapPath("~/Assets/Category/" + obj.CategoryImage)))
                                    {
                                        System.IO.File.Delete(Server.MapPath("~/Assets/Category/" + obj.CategoryImage));
                                    }
                                  

                                    //till here

                                    Imagefilename = "Category" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(file.FileName);

                                    //----- Rezize image with aspect ratio for large images------//
                                    System.Drawing.Image MainImg = System.Drawing.Image.FromStream(file.InputStream);
                                    MainImg = GlobalMethods.FixedSize(MainImg, 50, 30);  // pass image, width
                                    MainImg.Save(Server.MapPath("~/Assets/Category/" + Imagefilename));
                                    MainImg.Dispose();




                                    cat.CategoryImage = Imagefilename;

                                }

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
                            return RedirectToAction("manage");
                        }
                        else
                        {
                            tbl_category cat = new tbl_category();
                            cat.Category=obj.Category;
                            var file = Request.Files["CategoryImage"];
                            string Imagefilename = "";

                            if (file != null && file.FileName != "" && file.ContentLength > 0)
                            {
                               

                                Imagefilename = "Category" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(file.FileName);

                                //----- Rezize image with aspect ratio for large images------//
                                System.Drawing.Image MainImg = System.Drawing.Image.FromStream(file.InputStream);
                                MainImg = GlobalMethods.FixedSize(MainImg, 50, 30);  // pass image, width
                                MainImg.Save(Server.MapPath("~/Assets/Category/" + Imagefilename));
                                MainImg.Dispose();




                                cat.CategoryImage = Imagefilename;

                            }
                            ctx.tbl_category.Add(cat);
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["addcat_msg"] = "New record added successfully";
                               
                            }
                            else
                            {
                                TempData["addcat_msg"] = "Unable to add new record";
                                
                            }
                            return RedirectToAction("manage");
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

        public JsonResult IsCategoryNameExists(int CategoryID = 0,string Category = "")
        {
            using(db_jugglejoyEntities ctx=new db_jugglejoyEntities())
            {
                if(CategoryID == 0)
                {
                    //code for add method
                    if (!ctx.tbl_category.Any(x => x.Category.ToLower() == Category.Trim().ToLower()))
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false,JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    //code for edit
                    if(!ctx.tbl_category.Any(x=>x.Category.ToLower()== Category.Trim().ToLower() && x.CategoryID != CategoryID))
                     {
                        return Json(true,JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false,JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }
        #endregion

        #region grid

        public ActionResult category_list()
        {
            if (Session["AdminID"] != null)
            {
                ViewBag.delete_inuse = TempData["delete_catlistinuse"] == null ? "" : TempData["delete_catlistinuse"] as string;
                ViewBag.delete_notinuse = TempData["delete_catnotinuse"] == null ? "" : TempData["delete_catnotinuse"] as string;
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var list = ctx.tbl_category.ToList();

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
        public ActionResult delete_category_list(IEnumerable<decimal> IdsToDelete)
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
                            var subcategory = ctx.tbl_subcategory.Where(x => x.CategoryID == item).FirstOrDefault();
                            if (subcategory != null)
                            {
                                var cat = ctx.tbl_category.Where(x => x.CategoryID == subcategory.CategoryID).Select(x => x.Category).FirstOrDefault();
                                catlistinuse = catlistinuse + cat + ",";
                            }
                            else
                            {
                                var res = ctx.tbl_category.Where(x => x.CategoryID == item).FirstOrDefault();
                                catnotinuse = catnotinuse + res.Category + ",";
                                ctx.tbl_category.Remove(res);
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
                    return RedirectToAction("manage");
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


        #region subcategory

        #region subcat

        public ActionResult subcategory(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.subcatmsg = TempData["subcategory"] == null ? "" : TempData["subcategory"] as string;
                    if (id > 0)
                    {
                        SubcategoryValidation obj = new SubcategoryValidation();
                        var subcat = ctx.tbl_subcategory.Where(x => x.SubCategoryID == id).FirstOrDefault();
                        if (subcat != null)
                        {
                            ViewBag.CategoryID = new SelectList(ctx.tbl_category.ToList(), "CategoryID", "Category", subcat.CategoryID);
                            obj.SubCategoryID = subcat.SubCategoryID;
                            obj.CategoryID = Convert.ToInt32(subcat.CategoryID);
                            obj.SubCategory = subcat.SubCategory;
                            return View(obj);
                        }
                    }
                    ViewBag.CategoryID = new SelectList(ctx.tbl_category.ToList(), "CategoryID", "Category");
                    return View(new SubcategoryValidation());

                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }


        [HttpPost]
        public ActionResult subcategory(SubcategoryValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid)
                {

                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        if (obj.SubCategoryID > 0)
                        {
                            var subcat = ctx.tbl_subcategory.Where(x => x.SubCategoryID == obj.SubCategoryID).FirstOrDefault();
                            subcat.CategoryID = obj.CategoryID;
                            subcat.SubCategory = obj.SubCategory;
                            ctx.Entry(subcat).State = System.Data.Entity.EntityState.Modified;

                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["subcategory"] = "Record updated successfully";
                            }
                            else
                            {
                                TempData["subcategory"] = "Failed to update record";
                            }
                            RouteData.Values.Remove("id");

                            return RedirectToAction("subcategory");
                        }
                        else
                        {
                            tbl_subcategory subcat = new tbl_subcategory();
                            subcat.CategoryID = obj.CategoryID;
                            subcat.SubCategory = obj.SubCategory;
                            ctx.tbl_subcategory.Add(subcat);
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["subcategory"] = "Record added successfully";
                            }
                            else
                            {
                                TempData["subcategory"] = "Failed to add record";
                            }
                            return RedirectToAction("subcategory");
                        }
                    }

                }
                else
                {
                    //if you wont pass parameter in view then model state will not work
                    return View(obj);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("Login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        #endregion

        #region grid
        public ActionResult subcategorylist()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.subcaterror = TempData["subcat_error"] == null ? "" : TempData["subcat_error"] as string;

                    return View(ctx.tbl_subcategory.Include("tbl_category").ToList());

                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }

        }
        #endregion


        #region delete subcat

        [HttpPost]
        public ActionResult deletesubCat(IEnumerable<int> IdsToDelete)
        {
            try
            {
                if (Session["AdminID"] != null)
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {

                        if (IdsToDelete.Count() > 0)
                        {
                            ctx.tbl_subcategory.RemoveRange(ctx.tbl_subcategory.Where(x => IdsToDelete.Contains(x.SubCategoryID)));
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["subcat_error"] = "Record(s) deleted successfully";
                            }
                            else
                            {
                                TempData["subcat_error"] = "Failed to delete records";
                            }
                        }
                        return RedirectToAction("subcategory");
                    }
                }
                else
                {
                    TempData["AdminLogInStatus"] = "Please login here";
                    return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                }
            }
            catch
            {

                TempData["subcat_error"] = "subcatagory is in use";
            }
            return RedirectToAction("subcategory");

        }

        #endregion


        #region IsSubCategoryNameExists
        public JsonResult IsSubCategoryNameExists(int SubCategoryID = 0, string SubCategory = "", int CategoryID = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (SubCategoryID == 0 && CategoryID > 0)
                {
                    if (!ctx.tbl_subcategory.Any(x => x.SubCategory.ToLower() == SubCategory.Trim().ToLower() && x.CategoryID == CategoryID))
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
                    if (!ctx.tbl_subcategory.Any(x => x.SubCategory.ToLower() == SubCategory.Trim().ToLower() && x.CategoryID == CategoryID && x.SubCategoryID != SubCategoryID))
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

        #endregion

    }
}