using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class priceController : Controller
    {
        // GET: admin/price
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult addPrice()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.PriceMsg = TempData["PriceMsg"] == null ? "" : TempData["PriceMsg"] as string;
                   
                        var price = ctx.tbl_price.FirstOrDefault();

                        PriceValidation pri = new PriceValidation();
                        pri.PriceID = price.PriceID;
                        pri.ActualPrice = price.ActualPrice;
                        pri.DiscountedPrice = price.DiscountedPrice;
                       

                        return View(pri);
                    

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
        public ActionResult addPrice(PriceValidation obj)
        {
            if (Session["AdminID"] != null)
            {

                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (ModelState.IsValid == true)
                    {
                       
                            var price = ctx.tbl_price.Where(x => x.PriceID == obj.PriceID).FirstOrDefault();

                            if (price != null)
                            {
                                price.PriceID = obj.PriceID;
                                price.ActualPrice = obj.ActualPrice;
                                price.DiscountedPrice = obj.DiscountedPrice;


                                ctx.Entry(price).State = System.Data.Entity.EntityState.Modified;
                                if (ctx.SaveChanges() > 0)
                                {
                                    TempData["PriceMsg"] = "Record updated successfully";

                                }
                                else
                                {
                                    RouteData.Values.Remove("id");
                                    TempData["PriceMsg"] = "Record not updated";

                                }
                            return RedirectToAction("addPrice");
                        }
                            else
                            {
                                RouteData.Values.Remove("id");
                                TempData["PriceMsg"] = "Record not found";
                            return RedirectToAction("addPrice");
                        }
                    }
                    else
                    {
                        return View(obj);
                    }
                    
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }
    }
}