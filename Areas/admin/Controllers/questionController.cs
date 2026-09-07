using juggle_joy.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class questionController : Controller
    {
        #region Addquestion
        // GET: admin/question
        public ActionResult Addquestion(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["msgquestion"] == null ? "" : TempData["msgquestion"] as string;

                    if (id > 0)
                    {
                        var question = ctx.tbl_question.Where(x => x.QuestionID == id).FirstOrDefault();
                        QuestionValidation obj = new QuestionValidation();
                        obj.QuestionID = question.QuestionID; 
                        obj.Question = question.Question;
                        obj.SubCategoryID = new string[] { question.SubCategoryID };
                        obj.OptionType = question.OptionType;
                        ViewBag.SubCategoryID = new SelectList(ctx.tbl_subcategory.OrderBy(x => x.SubCategoryID).ToList(), "SubCategoryID", "SubCategory",obj.SubCategoryID);
                        return View(obj);
                    }
                    else
                    {
                        ViewBag.SubCategoryID = new SelectList(ctx.tbl_subcategory.OrderBy(x => x.SubCategoryID).ToList(), "SubCategoryID", "SubCategory");
                        return View(new QuestionValidation());
                    }
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new {RetUrl = Request.Url.ToString()});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Addquestion(QuestionValidation ques)
        {
            if (Session["AdminID"] != null)
            {
                using(db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var queid = ctx.tbl_question.Where(x => x.QuestionID == ques.QuestionID).FirstOrDefault();
                    if (queid != null)
                    {
                        queid.QuestionID = ques.QuestionID;
                        queid.Question = ques.Question;
                        
                        queid.OptionType=ques.OptionType;
                         var type = "";
                        if (ques.SubCategoryID != null)
                        {
                            foreach (var data in ques.SubCategoryID)
                            {
                                type = type + data + ",";
                            }
                            type = type.TrimEnd(',');
                        }

                        queid.SubCategoryID = type;

                        ctx.Entry(queid).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {

                            if (queid.OptionType == "Radio" || queid.OptionType == "CheckBox")
                            {
                            List<tbl_options> objoptlist = new List<tbl_options>();

                                foreach (var item in ques.options)
                                {

                                    var deleteditems = ctx.tbl_options.Where(x => x.OptionID == item.OptionID).FirstOrDefault();
                                    if (item.OptionID > 0 && deleteditems != null)
                                    {
                                        tbl_options opt = ctx.tbl_options.Where(x => x.OptionID == item.OptionID).FirstOrDefault();
                                        opt.OptionID = item.OptionID;
                                        opt.QuestionID = queid.QuestionID;
                                        opt.Question = item.Options;
                                        ctx.Entry(opt).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        if (item.Options != null)
                                        {
                                            tbl_options opt = new tbl_options();
                                            opt.OptionID = item.OptionID;
                                            opt.QuestionID = queid.QuestionID;
                                            opt.Question = item.Options;
                                            objoptlist.Add(opt);
                                        }
                                    }


                                }
                                ctx.tbl_options.AddRange(objoptlist);

                                if (ctx.SaveChanges() > 0)
                                {
                                    TempData["msgquestion"] = "Question updated successfully";

                                }
                            }
                            else if (queid.OptionType == "Counter" || queid.OptionType == "TextArea")
                            {
                                var del_opt = ctx.tbl_options.Where(x => x.QuestionID == queid.QuestionID).ToList();
                                if (del_opt.Count()>0)
                                {
                                    ctx.tbl_options.RemoveRange(del_opt);
                                    if (ctx.SaveChanges() > 0)
                                    {
                                        TempData["msgquestion"] = "Question updated successfully";

                                    }
                                }
                               
                            }
                                // else
                                //{
                                //    TempData["msgquestion"] = "Failed to update question";

                                //}
                                TempData["msgquestion"] = "Question updated successfully";
                            return RedirectToAction("Addquestion", "question");


                        }
                        else
                        {
                            TempData["msgquestion"] = "Failed to update question";
                            return RedirectToAction("Addquestion", "question");
                        }
                    }
                    else
                    {
                        tbl_question obj = new tbl_question();
                        var type = "";
                        if(ques.SubCategoryID != null)
                {
                            foreach (var data in ques.SubCategoryID)
                            {
                                type = type + data + ",";
                            }
                            type = type.TrimEnd(',');
                        }

                        obj.SubCategoryID = type;
                        obj.Question = ques.Question;

                        obj.OptionType = ques.OptionType;
                        ctx.tbl_question.Add(obj);
                        if(ctx.SaveChanges() > 0)
                        {
                            if (obj.OptionType == "Radio" || obj.OptionType == "CheckBox")
                            {
                                foreach (var item in ques.options)
                                {
                                    if (item.Options != null)
                                    {
                                        tbl_options opt = new tbl_options();
                                        opt.OptionID = ques.OptionID;
                                        opt.QuestionID = obj.QuestionID;
                                        opt.Question = item.Options;
                                        ctx.tbl_options.Add(opt);
                                        if (ctx.SaveChanges() > 0)
                                        {
                                            TempData["msgquestion"] = "Question added successfully";
                                        }
                                        //else
                                        //{
                                        //    TempData["msgquestion"] = "Failed to add question";
                                        //}
                                    }
                                }
                               
                            }

                            TempData["msgquestion"] = "Question added successfully";
                            //return RedirectToAction("Addquestion", "question");


                        }
                        else
                        {
                            TempData["msgquestion"] = "Failed to add question";
                        }
                        return RedirectToAction("Addquestion", "question");

                    }
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { Returl = Request.Url.ToString() });
            }
        }


        public JsonResult IsquestionExist(int QuestionID = 0, string Question = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (QuestionID == 0)
                {
                    //code for add method
                    if (!ctx.tbl_question.Any(x => x.Question.ToLower() == Question.Trim().ToLower()))
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
                    if (!ctx.tbl_question.Any(x => x.Question.ToLower() == Question.Trim().ToLower() && x.QuestionID != QuestionID))
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
    

        public ActionResult question_details(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var obj = ctx.tbl_question.Where(x => x.QuestionID == id).FirstOrDefault();
                return View(obj);
            }
        }

        [HttpPost]
        public ActionResult Delete_question(IEnumerable<decimal> Ids)
        {
            if (Ids.Count() > 0 )
            {
                using(db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    foreach (var item in Ids)
                    {
                        var del_opt = ctx.tbl_options.Where(x => x.QuestionID == item).ToList();
                        ctx.tbl_options.RemoveRange(del_opt);
                    }

                    foreach (var items in Ids)
                    {
                        var del_ans = ctx.tbl_answer.Where(x => x.QuestionID == items).ToList();
                        ctx.tbl_answer.RemoveRange(del_ans);
                    }

                    ctx.tbl_question.RemoveRange(ctx.tbl_question.Where(x => Ids.Contains(x.QuestionID)));
                    if(ctx.SaveChanges() > 0)
                    {
                        TempData["delete_productMSg"] = "Question(s) deleted successfully";
                        return RedirectToAction("managequestion", "question");
                    }
                    else
                    {
                        TempData["delete_productMSg"] = "Failed to delete Question(s)";
                        return RedirectToAction("managequestion", "question");
                    }
                }
            }
            else
            {
                TempData["msgquestion"] = "Something went wrong";
                return RedirectToAction("managequestion", "question");
            }
        }

        public JsonResult delete_que(int id)
        {
            using (var ctx = new db_jugglejoyEntities())
            {
                var del_opt = ctx.tbl_options.Where(x => x.OptionID == id).FirstOrDefault();
                if (del_opt != null)
                {
                    ctx.tbl_options.Remove(del_opt);
                    if (ctx.SaveChanges() > 0)
                    {
                        return Json("true", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("false", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("false", JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        #region Managequestions

        public ActionResult managequestion()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["delete_productMSg"] == null ? "" : TempData["delete_productMSg"] as string;
                    return View(ctx.tbl_question.ToList().OrderBy(x => x.QuestionID));
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion

    }
}