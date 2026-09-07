using juggle_joy.Hubs;
using juggle_joy.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

//using System.Xml;


namespace juggle_joy.Controllers
{
    public class TaskController : Controller
    {
        // GET: Task
        #region dashboards
        public ActionResult dashboard()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.draftMsg = TempData["draftMsg"] == null ? "" : TempData["draftMsg"] as string;
                return View();

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }
        public ActionResult dashboard_tasklist(int id = 0, int catid = 0, string sortby = "")
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var userid = Convert.ToInt32(Session["UserID"]);
                    var tbltasklistDraft = ctx.tbl_task.Include("tbl_category").Include("tbl_subcategory").Where(x => x.Status == (int)TaskStatus.Draft && x.FromID == userid).OrderByDescending(x => x.TaskID).ToList();
                    //var catids = Convert.ToInt32(catid);
                    if (catid != 0)
                    {
                        if (catid == 100)
                        {
                            if (sortby == "" || sortby == "N")
                            {
                                tbltasklistDraft = tbltasklistDraft.Where(x => x.CategoryID == null && x.TaskTitle != null).OrderByDescending(x => x.TaskID).ToList();
                            }
                            else if (sortby != "" && sortby == "O")
                            {
                                tbltasklistDraft = tbltasklistDraft.Where(x => x.CategoryID == null && x.TaskTitle != null).OrderBy(x => x.TaskID).ToList();
                            }
                        }
                        else
                        {
                            if (sortby == "" || sortby == "N")
                            {
                                tbltasklistDraft = tbltasklistDraft.Where(x => x.CategoryID == catid).OrderByDescending(x => x.TaskID).ToList();
                            }
                            else if (sortby != "" && sortby == "O")
                            {
                                tbltasklistDraft = tbltasklistDraft.Where(x => x.CategoryID == catid).OrderBy(x => x.TaskID).ToList();
                            }
                        }


                        ViewBag.count = tbltasklistDraft.Count;

                    }
                    else if (sortby != "")
                    {
                        if (sortby == "N")
                        {
                            tbltasklistDraft = tbltasklistDraft.OrderByDescending(x => x.TaskID).ToList();
                        }
                        else
                        {
                            tbltasklistDraft = tbltasklistDraft.OrderBy(x => x.TaskID).ToList();
                        }

                        ViewBag.count = tbltasklistDraft.Count;

                    }



                    if (id == 0)
                    {
                        var next_page = tbltasklistDraft.Take(10).ToList();
                        return View(next_page);
                    }
                    else
                    {
                        var next_page = tbltasklistDraft.Skip(10 * (id - 1)).Take(10).ToList();

                        return View(next_page);
                    }

                }

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }
        public ActionResult pending()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.taskcreatedMsg = TempData["taskcreatedMsg"] == null ? "" : TempData["taskcreatedMsg"] as string;
                return View();

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult pending_tasklist(int id = 0, int catid = 0, string sortby = "")
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    var userid = Convert.ToInt32(Session["UserID"]);

                    int pageSize = 10; // Number of records per page


                    var tasksQuery = ctx.tbl_task.Include("tbl_category")
                        .Where(x => x.Status == (int)TaskStatus.Received && x.FromID == userid)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();

                    if (catid != 0)
                    {

                        if (sortby == "" || sortby == "N")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else if (sortby != "" && sortby == "O")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }
                    else if (sortby != "")
                    {
                        if (sortby == "N")
                        {
                            tasksQuery = tasksQuery.ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else
                        {
                            tasksQuery = tasksQuery.OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }


                    if (id == 0)
                    {
                        id = 1;
                    }

                    var paginatedTasks = tasksQuery
                        .Skip(pageSize * (id - 1))
                        .Take(pageSize)
                        .ToList();


                    var groupedTasks = paginatedTasks
                        .GroupBy(x => x.CreatedDate.Value.Date)
                        .OrderByDescending(g => g.Key)
                        .ToList();

                    return View(groupedTasks);



                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }

        public ActionResult ongoing()
        {
            if (Session["UserID"] != null)
            {
                return View();

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult ongoing_tasklist(int id = 0, int catid = 0, string sortby = "")
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    var userid = Convert.ToInt32(Session["UserID"]);

                    int pageSize = 10; // Number of records per page


                    var tasksQuery = ctx.tbl_task.Include("tbl_category")
                        .Where(x => x.Status == (int)TaskStatus.Ongoing && x.FromID == userid)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();


                    if (catid != 0)
                    {

                        if (sortby == "" || sortby == "N")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else if (sortby != "" && sortby == "O")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }
                    else if (sortby != "")
                    {
                        if (sortby == "N")
                        {
                            tasksQuery = tasksQuery.ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else
                        {
                            tasksQuery = tasksQuery.OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }




                    if (id == 0)
                    {
                        id = 1;
                    }

                    var paginatedTasks = tasksQuery
                        .Skip(pageSize * (id - 1))
                        .Take(pageSize)
                        .ToList();


                    var groupedTasks = paginatedTasks
                        .GroupBy(x => x.CreatedDate.Value.Date)
                        .OrderByDescending(g => g.Key)
                        .ToList();

                    return View(groupedTasks);



                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult completed()
        {
            if (Session["UserID"] != null)
            {
                return View();

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult completed_tasklist(int id = 0, int catid = 0, string sortby = "")
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    var userid = Convert.ToInt32(Session["UserID"]);

                    int pageSize = 10; // Number of records per page


                    var tasksQuery = ctx.tbl_task.Include("tbl_category")
                        .Where(x => x.Status == (int)TaskStatus.Complete && x.FromID == userid)
                        .OrderByDescending(x => x.CreatedDate)
                        .ToList();

                    if (catid != 0)
                    {

                        if (sortby == "" || sortby == "N")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else if (sortby != "" && sortby == "O")
                        {
                            tasksQuery = tasksQuery.Where(x => x.CategoryID == catid).OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }
                    else if (sortby != "")
                    {
                        if (sortby == "N")
                        {
                            tasksQuery = tasksQuery.ToList();
                            ViewBag.count = tasksQuery.Count;
                        }
                        else
                        {
                            tasksQuery = tasksQuery.OrderBy(x => x.TaskID).ToList();
                            ViewBag.count = tasksQuery.Count;
                            if (id == 0)
                            {
                                id = 1;
                            }

                            var paginatedTask = tasksQuery
                                .Skip(pageSize * (id - 1))
                                .Take(pageSize)
                                .ToList();


                            var groupedTask = paginatedTask
                                .GroupBy(x => x.CreatedDate.Value.Date)
                                .OrderBy(g => g.Key)
                                .ToList();

                            return View(groupedTask);
                        }


                    }


                    if (id == 0)
                    {
                        id = 1;
                    }

                    var paginatedTasks = tasksQuery
                        .Skip(pageSize * (id - 1))
                        .Take(pageSize)
                        .ToList();


                    var groupedTasks = paginatedTasks
                        .GroupBy(x => x.CreatedDate.Value.Date)
                        .OrderByDescending(g => g.Key)
                        .ToList();

                    return View(groupedTasks);



                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        #endregion

        #region task details
        public ActionResult taskDetails(int id = 0)
        {

            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (id > 0)
                    {
                        ViewBag.minitask = TempData["minitask"] == null ? "" : TempData["minitask"] as string;
                        ViewBag.activeminitab = TempData["activeminitab"] == null ? "" : TempData["activeminitab"] as string;
                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                        ViewBag.id = tbltask.ToID;
                        ViewBag.adid = id;
                        return View(tbltask);
                    }
                    else
                    {
                        return RedirectToAction("dashboard", "Task");
                    }

                }

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion


        #region add task

        public ActionResult addtask(int id = 0)
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (id > 0)
                    {
                        //ViewBag.slideactive = TempData["slideactive"] == null ? "" : TempData["slideactive"] as string;

                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                        TaskValidation obj = new TaskValidation();
                        obj.TaskID = tbltask.TaskID;
                        obj.CategoryID = tbltask.CategoryID;
                        obj.subcategory = tbltask.SubCategoryID;
                        obj.TaskTitle = tbltask.TaskTitle;
                        obj.Budget = tbltask.Budget;
                        obj.Description = tbltask.Description;
                        obj.Repeating = tbltask.Repeating;
                        obj.Status = tbltask.Status;
                        obj.DeadlineType = tbltask.DeadlineType;
                        obj.Image = tbltask.Fileattachment;
                        if (tbltask.DeadlineType == "S")
                        {
                            obj.specificDate = tbltask.Deadline;
                        }
                        Session["taskid"] = tbltask.TaskID;
                        return View(obj);
                    }
                    else
                    {

                        TaskValidation obj = new TaskValidation();
                        return View(obj);
                    }
                }


            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }





        public JsonResult subcategory(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {


                var subcat = ctx.tbl_subcategory.Where(x => x.CategoryID == id).ToList();

                if (subcat.Count() > 0)
                {

                    //var list = JsonConvert.SerializeObject(subcat, Formatting.None,new JsonSerializerSettings
                    //    {
                    //        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    //    });

                    //var list = subcat.Select(x => new { SubCategory= x.SubCategory, SubCategoryID=x.SubCategoryID }).ToList();
                    var list = subcat.Select(x => new { x.SubCategory, x.SubCategoryID }).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }



        [HttpPost]
        public ActionResult questions(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                // var test = ctx.tbl_question.Include("tbl_options").Where(x => x.SubCategoryID.Contains(id.ToString())).ToList();

                var strid = id.ToString();
                if (id == 0)
                {
                    var tblque = ctx.tbl_question.Where(x => x.QuestionID == 1).ToList();
                    return PartialView("questionPartial", tblque);
                }
                else
                {
                    var tblque = ctx.tbl_question
                            .AsEnumerable() // Switch to LINQ to Objects
                            .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                            .ToList();
                    return PartialView("questionPartial", tblque);
                }




            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult add_task_slide1(TaskValidation obj, string taskaction1)
        {
            if (Session["UserID"] != null)
            {
                //ModelState.Clear();
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    int userid = Convert.ToInt32(Session["UserID"]);
                    var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    if (obj.TaskID > 0)
                    {

                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == obj.TaskID).FirstOrDefault();
                        tbltask.FromID = tbluser.UserID;
                        tbltask.CategoryID = obj.CategoryID;
                        tbltask.SubCategoryID = obj.subcategory;
                        tbltask.TaskTitle = obj.TaskTitle;

                        ctx.Entry(tbltask).State = System.Data.Entity.EntityState.Modified;

                        if (taskaction1 == "Draft")
                        {
                            if (ctx.SaveChanges() > 0)
                            {
                                tbl_log tbllog = new tbl_log();
                                tbllog.TaskID = obj.TaskID;
                                tbllog.LogStatus = "Edited by" + " " + tbluser.FirstName;
                                tbllog.LogDate = DateTime.Now;
                                ctx.tbl_log.Add(tbllog);
                                ctx.SaveChanges();
                                TempData["draftMsg"] = "Task is saved as Draft";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                TempData["draftMsg"] = "Failed to add Task";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else if (taskaction1 == "Next")
                        {
                            if (ctx.SaveChanges() > 0)
                            {
                                ViewBag.slideactive = "step2";

                                return Json(new { taskid = tbltask.TaskID }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                TempData["draftMsg"] = "Failed to add Task";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        tbl_task task = new tbl_task();

                        task.FromID = tbluser.UserID;
                        task.CategoryID = obj.CategoryID;
                        task.SubCategoryID = obj.subcategory;
                        task.TaskTitle = obj.TaskTitle;


                        task.Status = (int)TaskStatus.Draft;
                        ctx.tbl_task.Add(task);



                        if (taskaction1 == "Draft")
                        {

                            if (ctx.SaveChanges() > 0)
                            {
                                tbl_log log = new tbl_log();
                                log.TaskID = task.TaskID;
                                log.LogStatus = "Task created as draft";
                                log.LogDate = DateTime.Now;
                                ctx.tbl_log.Add(log);
                                ctx.SaveChanges();

                                TempData["draftMsg"] = "Task is saved as Draft";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                TempData["draftMsg"] = "Failed to add Task";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        else if (taskaction1 == "Next")
                        {
                            if (ctx.SaveChanges() > 0)
                            {
                                ViewBag.slideactive = "step1";

                                return Json(new { taskid = task.TaskID }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                TempData["draftMsg"] = "Failed to add Task";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    TempData["draftMsg"] = "something went wrong,please try again";
                    return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);

                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult add_task_slide2(string[] answerCounter, string[] answerOther, string taskaction2, string address,
            Dictionary<string, string> answerOptions, Dictionary<string, string[]> answercheck, Dictionary<string, string> optiontyperadio, Dictionary<string, string> optiontypecheckbox,
             Dictionary<string, string> optiontext, Dictionary<string, string> optiondate, Dictionary<string, string> optiontime,
             Dictionary<string, string> optiontextarea, Dictionary<string, string> optioncheckbox, Dictionary<string, string> optioncounter, Dictionary<string, string> optionradio, Dictionary<string, string> optionddtime, string TaskId)
        {
            if (Session["UserID"] != null)
            {

                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var taskid = Convert.ToInt32(TaskId);

                    if (taskid == 0)
                    {
                        TempData["draftMsg"] = "Please add task";
                        return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {


                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();


                        var catid = tbltask.CategoryID;
                        var strid = "0";
                        var tblque = ctx.tbl_question.ToList();
                        if (catid != 10)
                        {
                            strid = tbltask.SubCategoryID.ToString();
                            tblque = tblque
                                  .AsEnumerable() // Switch to LINQ to Objects
                                  .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                                  .ToList();
                        }
                        else
                        {
                            tblque = tblque.Where(x => x.QuestionID == 1).ToList();
                        }
                        //var strid = tbltask.SubCategoryID.ToString();

                        //var tblque = ctx.tbl_question
                        //            .AsEnumerable() // Switch to LINQ to Objects
                        //            .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                        //            .ToList();



                        int i = 0;
                        //int j = 0;
                        int k = 0;

                        var tblanswers = ctx.tbl_answer.ToList();
                        foreach (var item in tblque)
                        {

                            var tblanswer = tblanswers.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid).FirstOrDefault();
                            if (item.OptionType == "Counter")
                            {
                                if (answerCounter != null)
                                {
                                    if (tblanswer != null)
                                    {
                                        tblanswer.TaskID = tbltask.TaskID;
                                        tblanswer.QuestionID = item.QuestionID;
                                        tblanswer.Answer = answerCounter[i];
                                        ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        if (!(answerCounter[i] == "" || answerCounter[i] == "0" || answerCounter[i] == null))
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = answerCounter[i];
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();

                                        }
                                    }
                                    i++;
                                }
                            }
                            else if (item.OptionType == "Radio")
                            {

                                var keyToFind = item.QuestionID.ToString();
                                if (answerOptions.ContainsKey(keyToFind))
                                {
                                    var myValueLookup = answerOptions[keyToFind];


                                    if (optiontyperadio.ContainsKey(myValueLookup))
                                    {
                                        var value = optiontyperadio[myValueLookup];
                                        if (tblanswer != null)
                                        {
                                            tblanswer.TaskID = tblanswer.TaskID;
                                            tblanswer.QuestionID = Convert.ToInt32(keyToFind);
                                            tblanswer.Answer = myValueLookup;
                                            tblanswer.OtherAnswer = value;
                                            ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;

                                        }
                                        else
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValueLookup;
                                            tblans.OtherAnswer = value;
                                            ctx.tbl_answer.Add(tblans);

                                        }
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        if (tblanswer != null)
                                        {
                                            tblanswer.TaskID = tblanswer.TaskID;
                                            tblanswer.QuestionID = Convert.ToInt32(keyToFind);
                                            tblanswer.Answer = myValueLookup;

                                            if (tblanswer.OtherAnswer != null)
                                            {
                                                tblanswer.OtherAnswer = null;
                                            }
                                            ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;

                                        }
                                        else
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValueLookup;
                                            ctx.tbl_answer.Add(tblans);

                                        }
                                        ctx.SaveChanges();
                                    }


                                }


                            }
                            else if (item.OptionType == "Textarea")
                            {
                                if (item.Question == "What is the address?" || item.Question == "What is the collection address?" || item.Question == "What is the delivery address?")
                                {
                                    if (tblanswer != null)
                                    {


                                        tblanswer.TaskID = tbltask.TaskID;
                                        tblanswer.QuestionID = item.QuestionID;
                                        tblanswer.Answer = address;
                                        ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        if (address != "")
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = address;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                        }
                                    }


                                }
                                else if (answerOther != null)
                                {
                                    if (tblanswer != null)
                                    {


                                        tblanswer.TaskID = tbltask.TaskID;
                                        tblanswer.QuestionID = item.QuestionID;
                                        tblanswer.Answer = answerOther[k];
                                        ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        if (!(answerOther[k] == "" || answerOther[k] == null))
                                        {

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = answerOther[k];
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();

                                        }
                                    }

                                    k++;
                                }
                            }
                            else if (item.OptionType == "CheckBox")
                            {
                                if (tblanswer != null)
                                {
                                    var lists = ctx.tbl_answer.Where(x => x.QuestionID == tblanswer.QuestionID && x.TaskID == tblanswer.TaskID).ToList();
                                    foreach (var list in lists)
                                    {
                                        ctx.tbl_answer.Remove(list);
                                        ctx.SaveChanges();
                                    }
                                }

                                var keyToFind = item.QuestionID.ToString();
                                if (answercheck.ContainsKey(keyToFind))
                                {

                                    var myValueLookup = answercheck[keyToFind];


                                    foreach (var myValue in myValueLookup)
                                    {


                                        if (optiontypecheckbox.ContainsKey(myValue))
                                        {
                                            var value = optiontypecheckbox[myValue];

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValue;
                                            tblans.OtherAnswer = value;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValue;
                                            ctx.tbl_answer.Add(tblans);


                                            ctx.SaveChanges();
                                        }

                                    }

                                }


                            }
                            else if (item.OptionType == "Option")
                            {
                                var tblopt = ctx.tbl_options.Where(x => x.QuestionID == item.QuestionID).ToList();
                                foreach (var opts in tblopt)
                                {
                                    if (opts.OptionType == "text")
                                    {
                                        if (optiontext != null)
                                        {

                                            if (optiontext.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();

                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == tbltask.TaskID && x.Answer == key.ToString()).FirstOrDefault();

                                                if (tbl_ans != null)
                                                {

                                                    tbl_ans.TaskID = tbltask.TaskID;
                                                    tbl_ans.QuestionID = item.QuestionID;
                                                    tbl_ans.Answer = opts.OptionID.ToString();
                                                    tbl_ans.OtherAnswer = optiontext[key];
                                                    ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    ctx.SaveChanges();
                                                }
                                                else
                                                {
                                                    if (optiontext[key] != "")
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontext[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }
                                                }


                                            }




                                        }
                                    }
                                    else if (opts.OptionType == "date")
                                    {
                                        if (optiondate != null)
                                        {

                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optiondate.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optiondate[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optiondate[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiondate[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }
                                                    ctx.SaveChanges();

                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }
                                            }
                                            else if (tbl_ans != null)
                                            {
                                                tbl_ans.OtherAnswer = null;
                                                ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                ctx.SaveChanges();
                                            }



                                        }
                                    }
                                    else if (opts.OptionType == "time")
                                    {
                                        if (optiontime != null)
                                        {

                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optiontime.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optiontime[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optiontime[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontime[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }

                                                    ctx.SaveChanges();
                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }
                                            }
                                            else if (tbl_ans != null)
                                            {
                                                tbl_ans.OtherAnswer = null;
                                                ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                ctx.SaveChanges();
                                            }

                                        }
                                    }
                                    else if (opts.OptionType == "textarea")
                                    {
                                        if (optiontextarea != null)
                                        {

                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optiontextarea.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optiontextarea[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optiontextarea[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontextarea[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }

                                                    ctx.SaveChanges();
                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }
                                            }
                                            else if (tbl_ans != null)
                                            {
                                                ctx.tbl_answer.Remove(tbl_ans);
                                                ctx.SaveChanges();
                                            }

                                        }
                                    }
                                    else if (opts.OptionType == "checkbox")
                                    {
                                        if (optioncheckbox != null)
                                        {
                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optioncheckbox.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optioncheckbox[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optioncheckbox[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optioncheckbox[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }

                                                    ctx.SaveChanges();
                                                }


                                            }
                                            else
                                            {
                                                if (tbl_ans != null)
                                                {
                                                    tbl_ans.OtherAnswer = null;
                                                    ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    ctx.SaveChanges();
                                                }

                                            }

                                        }
                                    }
                                    else if (opts.OptionType == "counter")
                                    {
                                        if (optioncounter != null)
                                        {
                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optioncounter.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optioncounter[key] != "0")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optioncounter[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optioncounter[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }

                                                    ctx.SaveChanges();
                                                }



                                            }


                                        }
                                    }
                                    else if (opts.OptionType == "radio")
                                    {

                                        if (optionradio != null)
                                        {
                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();


                                            if (optionradio.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optionradio[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optionradio[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optionradio[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }

                                                    ctx.SaveChanges();
                                                }



                                            }

                                        }
                                    }
                                    else if (opts.OptionType == "ddtime")
                                    {
                                        if (optionddtime != null)
                                        {

                                            var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == taskid && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                            if (optionddtime.ContainsKey(opts.OptionID.ToString()))
                                            {
                                                var key = opts.OptionID.ToString();
                                                if (optionddtime[key] != "")
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optionddtime[key];
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optionddtime[key];
                                                        ctx.tbl_answer.Add(tblans);
                                                    }
                                                    ctx.SaveChanges();

                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }
                                            }



                                        }
                                    }
                                }



                            }

                        }



                    }
                    if (taskaction2 == "Next")
                    {
                        ViewBag.slideactive = "step2";
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }

                    if (taskaction2 == "Draft")
                    {
                        var log = ctx.tbl_log.Where(x => x.TaskID == taskid && x.LogStatus == "Task created as draft").FirstOrDefault();
                        if (log == null)
                        {
                            tbl_log tbllog = new tbl_log();
                            tbllog.TaskID = taskid;
                            tbllog.LogStatus = "Task created as draft";
                            tbllog.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(tbllog);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            int userid = Convert.ToInt32(Session["UserID"]);
                            var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                            tbl_log tbllog = new tbl_log();
                            tbllog.TaskID = taskid;
                            tbllog.LogStatus = "Edited by" + " " + tbluser.FirstName;
                            tbllog.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(tbllog);
                            ctx.SaveChanges();
                        }

                        TempData["draftMsg"] = "Task is saved as Draft";
                        return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["UserLogInStatus"] = "Please login here";
                    return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult add_task_slide3(TaskValidation obj, DateTime? specific_date, string taskaction3)
        {

            if (Session["UserID"] != null)
            {

                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    if (obj.TaskID == 0)
                    {
                        TempData["draftMsg"] = "Please add task";
                        return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == obj.TaskID).FirstOrDefault();
                        //tbltask.WorkTime = obj.WorkTime;

                        if (obj.Deadline == "S")
                        {
                            tbltask.Deadline = specific_date;
                            tbltask.DeadlineType = "S";
                        }
                        else if (obj.Deadline == "N")
                        {
                            tbltask.Deadline = DateTime.Now.AddDays(7);
                            tbltask.DeadlineType = "N";
                        }
                        else if (obj.Deadline == "C")
                        {
                            tbltask.Deadline = DateTime.Now.AddDays(2 * 7);
                            tbltask.DeadlineType = "C";
                        }
                        else if (obj.Deadline == "M")
                        {
                            tbltask.Deadline = DateTime.Now.AddMonths(2);
                            tbltask.DeadlineType = "M";
                        }
                        else if (obj.Deadline == "F")
                        {
                            tbltask.Deadline = null;
                            tbltask.DeadlineType = "F";
                        }


                        tbltask.Budget = obj.Budget;

                        ctx.Entry(tbltask).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            if (taskaction3 == "Next")
                            {
                                ViewBag.slideactive = "step3";
                                return Json(true, JsonRequestBehavior.AllowGet);
                            }
                            if (taskaction3 == "Draft")
                            {
                                var log = ctx.tbl_log.Where(x => x.TaskID == obj.TaskID && x.LogStatus == "Task created as draft").FirstOrDefault();
                                if (log == null)
                                {
                                    tbl_log tbllog = new tbl_log();
                                    tbllog.TaskID = obj.TaskID;
                                    tbllog.LogStatus = "Task created as draft";
                                    tbllog.LogDate = DateTime.Now;
                                    ctx.tbl_log.Add(tbllog);
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    int userid = Convert.ToInt32(Session["UserID"]);
                                    var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                                    tbl_log tbllog = new tbl_log();
                                    tbllog.TaskID = tbltask.TaskID;
                                    tbllog.LogStatus = "Edited by" + " " + tbluser.FirstName;
                                    tbllog.LogDate = DateTime.Now;
                                    ctx.tbl_log.Add(tbllog);
                                    ctx.SaveChanges();
                                }
                                TempData["draftMsg"] = "Task is saved as Draft";
                                return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                            }

                            return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            TempData["draftMsg"] = "something went wrong";
                            return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                        }

                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }



        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult add_task_slide4(TaskValidation obj, string taskaction4)
        {
            if (Session["UserID"] != null)
            {


                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (obj.TaskID == 0)
                    {
                        TempData["draftMsg"] = "Please add task";
                        return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var tbltask = ctx.tbl_task.Where(x => x.TaskID == obj.TaskID).FirstOrDefault();

                        tbltask.Description = obj.Description;

                        var file = Request.Files["Image"];
                        string Imagefilename = "";

                        if (tbltask.Fileattachment != null)
                        {
                            if (System.IO.File.Exists(Server.MapPath("~/Assets/TaskPicture/" + tbltask.Fileattachment)))
                            {
                                System.IO.File.Delete(Server.MapPath("~/Assets/TaskPicture/" + tbltask.Fileattachment));
                            }
                            tbltask.Fileattachment = null;
                        }

                        if (file != null && file.FileName != "" && file.ContentLength > 0)
                        {

                            //till here

                            Imagefilename = "Task" + DateTime.Now.ToString("ddMMyyHHmmss") + System.IO.Path.GetExtension(file.FileName);

                            //----- Rezize image with aspect ratio for large images------//
                            System.Drawing.Image MainImg = System.Drawing.Image.FromStream(file.InputStream);
                            MainImg = GlobalMethods.FixedSize(MainImg, 1000, 600);  // pass image, width
                            MainImg.Save(Server.MapPath("~/Assets/TaskPicture/" + Imagefilename));
                            MainImg.Dispose();


                            tbltask.Fileattachment = Imagefilename;

                        }


                        //pending
                        tbltask.Repeating = obj.Repeating;

                        if (taskaction4 == "Create")
                        {
                            tbltask.Status = (int)TaskStatus.Received;
                            if (tbltask.CreatedDate == null)
                            {
                                tbltask.CreatedDate = DateTime.Now;
                            }

                            ctx.Entry(tbltask).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                var tblLog = ctx.tbl_log.Where(x => x.TaskID == obj.TaskID && x.LogStatus == "Task moved to Pending").FirstOrDefault();
                                if (tblLog != null)
                                {
                                    int userid = Convert.ToInt32(Session["UserID"]);
                                    var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                                    tbl_log log = new tbl_log();
                                    log.TaskID = tbltask.TaskID;
                                    log.LogStatus = "Edited by" + " " + tbluser.FirstName;
                                    log.LogDate = DateTime.Now;
                                    ctx.tbl_log.Add(log);
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    tbl_log log = new tbl_log();
                                    log.TaskID = tbltask.TaskID;
                                    log.LogStatus = "Task moved to Pending";
                                    log.LogDate = DateTime.Now;
                                    ctx.tbl_log.Add(log);
                                    ctx.SaveChanges();


                                }


                                TempData["taskcreatedMsg"] = "Task created successfully";
                            }
                            return Json(new { pending = true }, JsonRequestBehavior.AllowGet);

                        }

                        if (taskaction4 == "Draft")
                        {
                            var log = ctx.tbl_log.Where(x => x.TaskID == obj.TaskID && x.LogStatus == "Task created as draft").FirstOrDefault();
                            if (log == null)
                            {
                                tbl_log tbllog = new tbl_log();
                                tbllog.TaskID = obj.TaskID;
                                tbllog.LogStatus = "Task created as draft";
                                tbllog.LogDate = DateTime.Now;
                                ctx.tbl_log.Add(tbllog);
                                ctx.SaveChanges();
                            }
                            else
                            {
                                int userid = Convert.ToInt32(Session["UserID"]);
                                var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                                tbl_log tbllog = new tbl_log();
                                tbllog.TaskID = tbltask.TaskID;
                                tbllog.LogStatus = "Edited by" + " " + tbluser.FirstName;
                                tbllog.LogDate = DateTime.Now;
                                ctx.tbl_log.Add(tbllog);
                                ctx.SaveChanges();
                            }
                            ctx.Entry(tbltask).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["draftMsg"] = "Task is saved as Draft";
                            }
                            return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    TempData["draftMsg"] = "something went wrong";
                    return Json(new { dashboard = true }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }

        [HttpPost]
        public ActionResult toDoList(string taskTitle = "")
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    tbl_task task = new tbl_task();

                    task.FromID = Convert.ToInt32(Session["UserID"]);
                    //task.CategoryID = obj.CategoryID;
                    //task.SubCategoryID = obj.subcategory;
                    task.TaskTitle = taskTitle;
                    task.Status = (int)TaskStatus.Draft;
                    ctx.tbl_task.Add(task);
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
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }


        [HttpPost]
        public ActionResult submitDraftTask(int taskId = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var tbltask = ctx.tbl_task.Where(x => x.TaskID == taskId).FirstOrDefault();
                if (tbltask.CategoryID != null && tbltask.SubCategoryID != null)
                {
                    tbltask.Status = (int)TaskStatus.Received;
                    tbltask.CreatedDate = DateTime.Now;
                    ctx.Entry(tbltask).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        tbl_log log = new tbl_log();
                        log.TaskID = taskId;
                        log.LogStatus = "Task moved to Pending";
                        log.LogDate = DateTime.Now;
                        ctx.tbl_log.Add(log);
                        ctx.SaveChanges();

                        TempData["taskcreatedMsg"] = "Task created successfully";
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                        //return RedirectToAction("pending");
                    }
                    else
                    {
                        return Json(new { failed = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Incomplete = true }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        #region add_mini_task

        public ActionResult addminitask(int id = 0, int minitaskid = 0)
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    var tbltask = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                    var strid = tbltask.SubCategoryID.ToString();

                    TempData["taskID"] = id;
                    ViewBag.TaskID = id;

                    TempData["minitaskid"] = minitaskid;
                    ViewBag.minitaskid = minitaskid;

                    var catid = tbltask.CategoryID;
                    var tblque = ctx.tbl_question.ToList();
                    if (catid == 10)
                    {
                        tblque = ctx.tbl_question.Where(x => x.QuestionID == 1).ToList();
                    }
                    else
                    {
                        tblque = ctx.tbl_question
                               .AsEnumerable() // Switch to LINQ to Objects
                               .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                               .ToList();
                    }

                    //var tblque = ctx.tbl_question
                    //            .AsEnumerable() // Switch to LINQ to Objects
                    //            .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                    //            .ToList();
                    return View(tblque);


                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult addminitask(string tasktitle, string[] answerCounter, string[] answerOther, string address,
            Dictionary<string, string> answerOptions, Dictionary<string, string[]> answercheck, Dictionary<string, string> optiontyperadio, Dictionary<string, string> optiontypecheckbox,
             Dictionary<string, string> optiontext, Dictionary<string, string> optiondate, Dictionary<string, string> optiontime,
             Dictionary<string, string> optiontextarea, Dictionary<string, string> optioncheckbox, Dictionary<string, string> optioncounter, Dictionary<string, string> optionradio, Dictionary<string, string> optionddtime)
        {


            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    int TaskID = Convert.ToInt32(TempData["taskID"]);
                    int minitaskid = Convert.ToInt32(TempData["minitaskid"]);

                    var isexistminitask = ctx.tbl_minitask.Where(x => x.TaskID == TaskID && x.MinitaskID == minitaskid).FirstOrDefault();

                    var tbltask = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();

                    var catid = tbltask.CategoryID;
                    var strid = tbltask.SubCategoryID.ToString();
                    var tblque = ctx.tbl_question.ToList();
                    if (catid == 10)
                    {
                        tblque = ctx.tbl_question.Where(x => x.QuestionID == 1).ToList();
                    }
                    else
                    {
                        tblque = ctx.tbl_question
                               .AsEnumerable() // Switch to LINQ to Objects
                               .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                               .ToList();
                    }
                    //var strid = tbltask.SubCategoryID.ToString();

                    //var tblque = ctx.tbl_question
                    //            .AsEnumerable() // Switch to LINQ to Objects
                    //            .Where(x => x.SubCategoryID.Split(',').Any(z => z.Trim() == strid))
                    //            .ToList();

                    int i = 0;
                    int k = 0;
                    if (isexistminitask == null)
                    {
                        tbl_minitask mini = new tbl_minitask();
                        mini.TaskID = TaskID;
                        mini.Title = tasktitle;
                        mini.PostedDate = DateTime.Now;
                        ctx.tbl_minitask.Add(mini);
                        if (ctx.SaveChanges() > 0)
                        {


                            foreach (var item in tblque)
                            {
                                if (item.OptionType == "Counter")
                                {
                                    if (answerCounter != null)
                                    {

                                        if (!(answerCounter[i] == "" || answerCounter[i] == "0" || answerCounter[i] == null))
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = answerCounter[i];
                                            tblans.MinitaskID = mini.MinitaskID;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();

                                        }

                                        i++;
                                    }
                                }
                                else if (item.OptionType == "Radio")
                                {
                                    //doing it separatly

                                    var keyToFind = item.QuestionID.ToString();
                                    if (answerOptions.ContainsKey(keyToFind))
                                    {
                                        var myValueLookup = answerOptions[keyToFind];
                                        // do work...



                                        if (optiontyperadio.ContainsKey(myValueLookup))
                                        {
                                            var value = optiontyperadio[myValueLookup];//where to store this value

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValueLookup;
                                            tblans.OtherAnswer = value;
                                            tblans.MinitaskID = mini.MinitaskID;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = Convert.ToInt32(keyToFind);
                                            tblans.Answer = myValueLookup;
                                            tblans.MinitaskID = mini.MinitaskID;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                        }


                                    }



                                }
                                else if (item.OptionType == "Textarea")
                                {
                                    if (item.Question == "What is the address?" || item.Question == "What is the collection address?" || item.Question == "What is the delivery address?")
                                    {

                                        if (address != "")
                                        {
                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = address;
                                            tblans.MinitaskID = mini.MinitaskID;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                        }



                                    }
                                    else if (answerOther != null)
                                    {

                                        if (!(answerOther[k] == "" || answerOther[k] == null))
                                        {

                                            tbl_answer tblans = new tbl_answer();

                                            tblans.TaskID = tbltask.TaskID;
                                            tblans.QuestionID = item.QuestionID;
                                            tblans.Answer = answerOther[k];
                                            tblans.MinitaskID = mini.MinitaskID;
                                            ctx.tbl_answer.Add(tblans);
                                            ctx.SaveChanges();
                                            k++;
                                        }


                                        
                                    }
                                }
                                else if (item.OptionType == "CheckBox")
                                {


                                    var keyToFind = item.QuestionID.ToString();
                                    if (answercheck.ContainsKey(keyToFind))
                                    {

                                        var myValueLookup = answercheck[keyToFind];


                                        foreach (var myValue in myValueLookup)
                                        {


                                            if (optiontypecheckbox.ContainsKey(myValue))
                                            {
                                                var value = optiontypecheckbox[myValue];//where to store this value

                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValue;
                                                tblans.OtherAnswer = value;
                                                tblans.MinitaskID = mini.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {

                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValue;
                                                tblans.MinitaskID = mini.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);


                                                ctx.SaveChanges();
                                            }

                                        }

                                    }


                                }
                                else if (item.OptionType == "Option")
                                {
                                    var tblopt = ctx.tbl_options.Where(x => x.QuestionID == item.QuestionID).ToList();
                                    foreach (var opts in tblopt)
                                    {
                                        if (opts.OptionType == "text")
                                        {
                                            if (optiontext != null)
                                            {

                                                if (optiontext.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();


                                                    if (optiontext[key] != "")
                                                    {
                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontext[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }

                                                }




                                            }
                                        }
                                        else if (opts.OptionType == "date")
                                        {
                                            if (optiondate != null)
                                            {

                                                if (optiondate.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiondate[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiondate[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();

                                                    }

                                                }



                                            }
                                        }
                                        else if (opts.OptionType == "time")
                                        {
                                            if (optiontime != null)
                                            {


                                                if (optiontime.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiontime[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontime[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }

                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "textarea")
                                        {
                                            if (optiontextarea != null)
                                            {


                                                if (optiontextarea.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiontextarea[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optiontextarea[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }


                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "checkbox")
                                        {
                                            if (optioncheckbox != null)
                                            {

                                                if (optioncheckbox.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optioncheckbox[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optioncheckbox[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }


                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "counter")
                                        {
                                            if (optioncounter != null)
                                            {

                                                if (optioncounter.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optioncounter[key] != "0")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optioncounter[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);
                                                        ctx.SaveChanges();
                                                    }



                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "radio")
                                        {

                                            if (optionradio != null)
                                            {



                                                if (optionradio.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optionradio[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optionradio[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);

                                                        ctx.SaveChanges();
                                                    }



                                                }

                                            }
                                        }
                                        else if (opts.OptionType == "ddtime")
                                        {
                                            if (optionddtime != null)
                                            {


                                                if (optionddtime.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optionddtime[key] != "")
                                                    {

                                                        tbl_answer tblans = new tbl_answer();

                                                        tblans.TaskID = tbltask.TaskID;
                                                        tblans.QuestionID = item.QuestionID;
                                                        tblans.Answer = opts.OptionID.ToString();
                                                        tblans.OtherAnswer = optionddtime[key];
                                                        tblans.MinitaskID = mini.MinitaskID;
                                                        ctx.tbl_answer.Add(tblans);

                                                        ctx.SaveChanges();

                                                    }
                                                }



                                            }
                                        }
                                    }



                                }



                            }
                            TempData["minitask"] = "Mini task is added successfully";
                            tbl_notification notification = new tbl_notification();
                            var unread = (int)ReadStatus.Unread;

                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "Added minitask";
                            notification.Url = "/assistant-specific-task/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = tbltask.AssistantID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();


                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "Added minitask";
                            notification.Url = "/assistant-specific-tasks/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = tbltask.ToID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();

                            //log 
                            var user = ctx.tbl_user.Where(x => x.UserID == tbltask.FromID).FirstOrDefault();
                            tbl_log log = new tbl_log();

                            log.TaskID = TaskID;
                            log.LogStatus = user.FirstName + " added minitask " + tasktitle;
                            log.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(log);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            TempData["minitask"] = "Failed to add mini task";

                        }
                        //if (tbltask.Status == (int)TaskStatus.Ongoing)
                        //{
                        //    return RedirectToAction("ongoing", "Task");
                        //}
                        //else
                        //{
                        //    return RedirectToAction("pending", "Task");
                        //}
                        TempData["activeminitab"] = "minitab";
                        return RedirectToAction("taskDetails", new { id = TaskID });
                    }
                    else
                    {

                        isexistminitask.Title = tasktitle;
                        isexistminitask.PostedDate = DateTime.Now;
                        ctx.Entry(isexistminitask).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {


                            var tblanswers = ctx.tbl_answer.ToList();
                            foreach (var item in tblque)
                            {
                                var tblanswer = tblanswers.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID).FirstOrDefault();

                                if (item.OptionType == "Counter")
                                {
                                    if (answerCounter != null)
                                    {
                                        if (tblanswer != null)
                                        {
                                            tblanswer.TaskID = tbltask.TaskID;
                                            tblanswer.QuestionID = item.QuestionID;
                                            tblanswer.Answer = answerCounter[i];
                                            tblanswer.MinitaskID = isexistminitask.MinitaskID;
                                            ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            if (!(answerCounter[i] == "" || answerCounter[i] == "0" || answerCounter[i] == null))
                                            {
                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = item.QuestionID;
                                                tblans.Answer = answerCounter[i];
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);
                                                ctx.SaveChanges();

                                            }
                                        }


                                        i++;
                                    }
                                }
                                else if (item.OptionType == "Radio")
                                {
                                    //doing it separatly

                                    var keyToFind = item.QuestionID.ToString();
                                    if (answerOptions.ContainsKey(keyToFind))
                                    {
                                        var myValueLookup = answerOptions[keyToFind];
                                        // do work...




                                        if (optiontyperadio.ContainsKey(myValueLookup))
                                        {
                                            var value = optiontyperadio[myValueLookup];//where to store this value
                                            if (tblanswer != null)
                                            {
                                                tblanswer.TaskID = tblanswer.TaskID;
                                                tblanswer.QuestionID = Convert.ToInt32(keyToFind);
                                                tblanswer.Answer = myValueLookup;
                                                tblanswer.OtherAnswer = value;
                                                tblanswer.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;

                                            }
                                            else
                                            {
                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValueLookup;
                                                tblans.OtherAnswer = value;
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);

                                            }
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            if (tblanswer != null)
                                            {
                                                tblanswer.TaskID = tblanswer.TaskID;
                                                tblanswer.QuestionID = Convert.ToInt32(keyToFind);
                                                tblanswer.Answer = myValueLookup;

                                                if (tblanswer.OtherAnswer != null)
                                                {
                                                    tblanswer.OtherAnswer = null;
                                                }
                                                tblanswer.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;

                                            }
                                            else
                                            {
                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValueLookup;
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);

                                            }
                                            ctx.SaveChanges();
                                        }


                                    }



                                }
                                else if (item.OptionType == "Textarea")
                                {
                                    if (item.Question == "What is the address?" || item.Question == "What is the collection address?" || item.Question == "What is the delivery address?")
                                    {
                                        if (tblanswer != null)
                                        {


                                            tblanswer.TaskID = tbltask.TaskID;
                                            tblanswer.QuestionID = item.QuestionID;
                                            tblanswer.Answer = address;
                                            tblanswer.MinitaskID = isexistminitask.MinitaskID;
                                            ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                            ctx.SaveChanges();
                                        }
                                        else
                                        {
                                            if (address != "")
                                            {
                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = item.QuestionID;
                                                tblans.Answer = address;
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);
                                                ctx.SaveChanges();
                                            }
                                        }


                                    }
                                    else if (answerOther != null)
                                    {
                                        if (tblanswer != null)
                                        {


                                            tblanswer.TaskID = tbltask.TaskID;
                                            tblanswer.QuestionID = item.QuestionID;
                                            tblanswer.Answer = answerOther[k];
                                            tblanswer.MinitaskID = isexistminitask.MinitaskID;
                                            ctx.Entry(tblanswer).State = System.Data.Entity.EntityState.Modified;
                                            ctx.SaveChanges();
                                            k++;
                                        }
                                        else
                                        {
                                            if (!(answerOther[k] == "" || answerOther[k] == null))
                                            {

                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = item.QuestionID;
                                                tblans.Answer = answerOther[k];
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);
                                                ctx.SaveChanges();
                                                k++;

                                            }
                                        }

                                       
                                    }
                                }
                                else if (item.OptionType == "CheckBox")
                                {
                                    if (tblanswer != null)
                                    {
                                        var lists = ctx.tbl_answer.Where(x => x.QuestionID == tblanswer.QuestionID && x.TaskID == tblanswer.TaskID && x.MinitaskID == isexistminitask.MinitaskID).ToList();
                                        foreach (var list in lists)
                                        {
                                            ctx.tbl_answer.Remove(list);
                                            ctx.SaveChanges();
                                        }
                                    }

                                    var keyToFind = item.QuestionID.ToString();
                                    if (answercheck.ContainsKey(keyToFind))
                                    {

                                        var myValueLookup = answercheck[keyToFind];
                                        //var datavalue = "";


                                        foreach (var myValue in myValueLookup)
                                        {


                                            if (optiontypecheckbox.ContainsKey(myValue))
                                            {
                                                var value = optiontypecheckbox[myValue];//where to store this value

                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValue;
                                                tblans.OtherAnswer = value;
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);
                                                ctx.SaveChanges();
                                            }
                                            else
                                            {

                                                tbl_answer tblans = new tbl_answer();

                                                tblans.TaskID = tbltask.TaskID;
                                                tblans.QuestionID = Convert.ToInt32(keyToFind);
                                                tblans.Answer = myValue;
                                                tblans.MinitaskID = isexistminitask.MinitaskID;
                                                ctx.tbl_answer.Add(tblans);


                                                ctx.SaveChanges();
                                            }

                                        }

                                    }


                                }
                                else if (item.OptionType == "Option")
                                {
                                    var tblopt = ctx.tbl_options.Where(x => x.QuestionID == item.QuestionID).ToList();
                                    foreach (var opts in tblopt)
                                    {
                                        if (opts.OptionType == "text")
                                        {
                                            if (optiontext != null)
                                            {

                                                if (optiontext.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();

                                                    var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == tbltask.TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == key.ToString()).FirstOrDefault();

                                                    if (tbl_ans != null)
                                                    {

                                                        tbl_ans.TaskID = tbltask.TaskID;
                                                        tbl_ans.QuestionID = item.QuestionID;
                                                        tbl_ans.Answer = opts.OptionID.ToString();
                                                        tbl_ans.OtherAnswer = optiontext[key];
                                                        tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        if (optiontext[key] != "")
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optiontext[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                            ctx.SaveChanges();
                                                        }
                                                    }


                                                }




                                            }
                                        }
                                        else if (opts.OptionType == "date")
                                        {
                                            if (optiondate != null)
                                            {

                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optiondate.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiondate[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optiondate[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optiondate[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }
                                                        ctx.SaveChanges();

                                                    }
                                                    else
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.OtherAnswer = null;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                            ctx.SaveChanges();
                                                        }

                                                    }
                                                }
                                                else if (tbl_ans != null)
                                                {
                                                    tbl_ans.OtherAnswer = null;
                                                    ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    ctx.SaveChanges();
                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "time")
                                        {
                                            if (optiontime != null)
                                            {

                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optiontime.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiontime[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optiontime[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optiontime[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }

                                                        ctx.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.OtherAnswer = null;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                            ctx.SaveChanges();
                                                        }

                                                    }
                                                }
                                                else if (tbl_ans != null)
                                                {
                                                    tbl_ans.OtherAnswer = null;
                                                    ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    ctx.SaveChanges();
                                                }

                                            }
                                        }
                                        else if (opts.OptionType == "textarea")
                                        {
                                            if (optiontextarea != null)
                                            {

                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optiontextarea.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optiontextarea[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optiontextarea[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optiontextarea[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }

                                                        ctx.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.OtherAnswer = null;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                            ctx.SaveChanges();
                                                        }

                                                    }
                                                }
                                                else if (tbl_ans != null)
                                                {
                                                    tbl_ans.OtherAnswer = null;
                                                    ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                    ctx.SaveChanges();
                                                }

                                            }
                                        }
                                        else if (opts.OptionType == "checkbox")
                                        {
                                            if (optioncheckbox != null)
                                            {
                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optioncheckbox.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optioncheckbox[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optioncheckbox[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optioncheckbox[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }

                                                        ctx.SaveChanges();
                                                    }


                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }

                                            }
                                        }
                                        else if (opts.OptionType == "counter")
                                        {
                                            if (optioncounter != null)
                                            {
                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optioncounter.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optioncounter[key] != "0")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optioncounter[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optioncounter[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }

                                                        ctx.SaveChanges();
                                                    }


                                                }


                                            }
                                        }
                                        else if (opts.OptionType == "radio")
                                        {

                                            if (optionradio != null)
                                            {
                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();


                                                if (optionradio.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optionradio[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optionradio[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optionradio[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }

                                                        ctx.SaveChanges();
                                                    }



                                                }

                                            }
                                        }
                                        else if (opts.OptionType == "ddtime")
                                        {
                                            if (optionddtime != null)
                                            {


                                                var tbl_ans = ctx.tbl_answer.Where(x => x.QuestionID == item.QuestionID && x.TaskID == TaskID && x.MinitaskID == isexistminitask.MinitaskID && x.Answer == opts.OptionID.ToString()).FirstOrDefault();
                                                if (optionddtime.ContainsKey(opts.OptionID.ToString()))
                                                {
                                                    var key = opts.OptionID.ToString();
                                                    if (optionddtime[key] != "")
                                                    {
                                                        if (tbl_ans != null)
                                                        {
                                                            tbl_ans.TaskID = tbltask.TaskID;
                                                            tbl_ans.QuestionID = item.QuestionID;
                                                            tbl_ans.Answer = opts.OptionID.ToString();
                                                            tbl_ans.OtherAnswer = optionddtime[key];
                                                            tbl_ans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            tbl_answer tblans = new tbl_answer();

                                                            tblans.TaskID = tbltask.TaskID;
                                                            tblans.QuestionID = item.QuestionID;
                                                            tblans.Answer = opts.OptionID.ToString();
                                                            tblans.OtherAnswer = optionddtime[key];
                                                            tblans.MinitaskID = isexistminitask.MinitaskID;
                                                            ctx.tbl_answer.Add(tblans);
                                                        }
                                                        ctx.SaveChanges();

                                                    }
                                                }
                                                else
                                                {
                                                    if (tbl_ans != null)
                                                    {
                                                        tbl_ans.OtherAnswer = null;
                                                        ctx.Entry(tbl_ans).State = System.Data.Entity.EntityState.Modified;
                                                        ctx.SaveChanges();
                                                    }

                                                }


                                            }
                                        }
                                    }



                                }

                            }
                            TempData["minitask"] = "Mini task is edited successfully";

                            tbl_notification notification = new tbl_notification();
                            var unread = (int)ReadStatus.Unread;

                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "Edited minitask";
                            notification.Url = "/assistant-specific-task/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = tbltask.AssistantID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();


                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "Edited minitask";
                            notification.Url = "/assistant-specific-tasks/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = tbltask.ToID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();

                            //log 
                            var user = ctx.tbl_user.Where(x => x.UserID == tbltask.FromID).FirstOrDefault();
                            tbl_log log = new tbl_log();

                            log.TaskID = TaskID;
                            log.LogStatus = user.FirstName + " edited minitask " + tasktitle;
                            log.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(log);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            TempData["minitask"] = "Failed to add mini task";

                        }
                        TempData["activeminitab"] = "minitab";
                        return RedirectToAction("taskDetails", new { id = TaskID });
                    }


                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion


        #region file upload

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult fileupload(HttpPostedFileBase fileattachment, int taskid = 0, int userid = 0)
        {

            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    //var postedFile = Request.Files["fileattachment"];
                    var files = ctx.tbl_taskattachment.Where(x => x.TaskID == taskid).ToList();
                    var filecount = files.Count();
                    int? size = 0;
                    if (filecount >= 6)
                    {
                        return Json(new { filecount = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var maxFileSize = 30 * 1024 * 1024; // 5MB
                        foreach (var item in files)
                        {
                            size = item.FileSize;
                        }
                        if (size > maxFileSize)
                        {
                            return Json(new { filesize = true }, JsonRequestBehavior.AllowGet);
                        }
                        else if (fileattachment != null && fileattachment.FileName != "" && fileattachment.ContentLength > 0)
                        {
                            tbl_taskattachment attach = new tbl_taskattachment();
                            attach.FileName = Path.GetFileName(fileattachment.FileName);

                            fileattachment.SaveAs(Server.MapPath("~/Assets/TaskFiles/" + attach.FileName));
                            attach.TaskID = taskid;
                            attach.UserID = userid;
                            attach.Date = DateTime.Now;
                            attach.FileSize = fileattachment.ContentLength;
                            ctx.tbl_taskattachment.Add(attach);
                            ctx.SaveChanges();

                            var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                            tbl_log log = new tbl_log();
                            log.TaskID = taskid;
                            log.LogStatus = user.FirstName + " uploads a file";
                            log.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(log);
                            ctx.SaveChanges();



                            return Json(new { taskid = taskid }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(false, JsonRequestBehavior.AllowGet);
                        }
                    }


                }




            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }



        public ActionResult deletefile(int id = 0, int userid = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var taskfile = ctx.tbl_taskattachment.Where(x => x.TaskAttachmentID == id).FirstOrDefault();
                var taskid = taskfile.TaskID;
                if (taskfile.UserID == userid)
                {

                    if (System.IO.File.Exists(Server.MapPath("~/Assets/TaskFiles/" + taskfile.FileName)))
                    {
                        System.IO.File.Delete(Server.MapPath("~/Assets/TaskFiles/" + taskfile.FileName));
                    }

                    ctx.tbl_taskattachment.Remove(taskfile);
                    if (ctx.SaveChanges() > 0)
                    {
                        var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                        tbl_log log = new tbl_log();
                        log.TaskID = taskid;
                        log.LogStatus = user.FirstName + " deleted a file";
                        log.LogDate = DateTime.Now;
                        ctx.tbl_log.Add(log);
                        ctx.SaveChanges();

                        return Json(new { taskid = taskid });
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { notvaliduser = true }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult loadfiles(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var data = ctx.tbl_taskattachment.Where(x => x.TaskID == id).OrderByDescending(x => x.TaskAttachmentID).ToList();
                return View(data);
            }
        }
        #endregion

        #region notification

        public ActionResult notification(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var notification = ctx.tbl_notification.Where(x => x.NotificationID == id).FirstOrDefault();
                var read = (int)ReadStatus.Read;
                notification.ReadStatus = read.ToString();
                ctx.Entry(notification).State = System.Data.Entity.EntityState.Modified;

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

        #region recommendation

        public ActionResult userrecommendation(int id = 0, int TaskID = 0, string user_note = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var recommendation = ctx.tbl_recommendations.Where(x => x.RecID == id).FirstOrDefault();
                recommendation.TaskStatus = 1;
                ctx.Entry(recommendation).State = System.Data.Entity.EntityState.Modified;
                if (ctx.SaveChanges() > 0)
                {
                    var task = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();

                    if (task != null)
                    {
                        task.UserNote = user_note;
                        //task.Status = (int)TaskStatus.Complete;
                        //task.CompletionDate = DateTime.Now;
                        ctx.Entry(task).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            //notification
                            tbl_notification notification = new tbl_notification();
                            var unread = (int)ReadStatus.Unread;

                            notification.ReadStatus = unread.ToString();
                            //notification.Notification = "Task completed";
                            notification.Notification = "User has picked an option to proceed with";
                            notification.Url = "/assistant-specific-task/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = task.AssistantID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();


                            notification.ReadStatus = unread.ToString();
                            //notification.Notification = "Task completed";
                            notification.Notification = "User has picked an option to proceed with";
                            notification.Url = "/assistant-specific-tasks/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = task.ToID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();

                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContext.Clients.All.SendNotification("User has picked an option to proceed with", "assistant", task.AssistantID);

                            IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContexts.Clients.All.SendNotification("User has picked an option to proceed with", "handler", task.ToID);
                            //log 
                            var user = ctx.tbl_user.Where(x => x.UserID == task.FromID).FirstOrDefault();
                            tbl_log log = new tbl_log();

                            log.TaskID = TaskID;
                            log.LogStatus = user.FirstName + " picks Opition " + recommendation.Name;
                            log.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(log);
                            ctx.SaveChanges();

                            //log.TaskID = TaskID;
                            //log.LogStatus = user.FirstName + " marks task as completed";
                            //log.LogDate = DateTime.Now;
                            //ctx.tbl_log.Add(log);
                            //ctx.SaveChanges();
                        }
                    }



                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }


            }

        }

        public ActionResult completetask(int taskid = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var task = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                task.Status = (int)TaskStatus.Complete;
                task.CompletionDate = DateTime.Now;
                ctx.Entry(task).State = System.Data.Entity.EntityState.Modified;
                if (ctx.SaveChanges() > 0)
                {
                    //notification
                    tbl_notification notification = new tbl_notification();
                    var unread = (int)ReadStatus.Unread;

                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Task completed successfully";
                    notification.Url = "/assistant-specific-task/" + taskid;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.AssistantID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();


                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Task completed successfully";
                    notification.Url = "/assistant-specific-tasks/" + taskid;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.ToID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();

                    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContext.Clients.All.SendNotification("Task completed successfully", "assistant", task.AssistantID);

                    IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContexts.Clients.All.SendNotification("Task completed successfully", "handler", task.ToID);
                    //log 
                    var user = ctx.tbl_user.Where(x => x.UserID == task.FromID).FirstOrDefault();
                    tbl_log log = new tbl_log();

                    log.TaskID = taskid;
                    log.LogStatus = user.FirstName + " marks task as completed";
                    log.LogDate = DateTime.Now;
                    ctx.tbl_log.Add(log);
                    ctx.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult morerecommendation(int TaskID = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var task = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();
                task.MoreOptions = "Y";
                ctx.Entry(task).State = System.Data.Entity.EntityState.Modified;
                if (ctx.SaveChanges() > 0)
                {
                    tbl_notification notification = new tbl_notification();
                    var unread = (int)ReadStatus.Unread;

                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Request for more recommendation";
                    notification.Url = "/assistant-specific-task/" + TaskID;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.AssistantID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();


                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Request for more recommendation";
                    notification.Url = "/assistant-specific-tasks/" + TaskID;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.ToID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();

                    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContext.Clients.All.SendNotification("Task completed", "assistant", task.AssistantID);

                    IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContexts.Clients.All.SendNotification("Task completed", "handler", task.ToID);

                    //log 
                    var user = ctx.tbl_user.Where(x => x.UserID == task.FromID).FirstOrDefault();
                    tbl_log log = new tbl_log();

                    log.TaskID = TaskID;
                    log.LogStatus = user.FirstName + " request for more options ";
                    log.LogDate = DateTime.Now;
                    ctx.tbl_log.Add(log);
                    ctx.SaveChanges();

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
        }

        [HttpPost]
        public ActionResult survey(Dictionary<string, string> ratings, string feedback_comments = "", int TaskID = 0, int comments_surveyId = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var tbltask = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();

                List<tbl_survey> surveylist = new List<tbl_survey>();

                foreach (string key in ratings.Keys)
                {
                    if (ratings[key] != "0")
                    {
                        tbl_survey survey = new tbl_survey();
                        survey.SurveyQueID = int.Parse(key);
                        survey.Rate = ratings[key];
                        survey.AssistantID = tbltask.AssistantID;
                        survey.TaskID = TaskID;
                        surveylist.Add(survey);
                    }

                }

                if (feedback_comments != "")
                {
                    tbl_survey survey = new tbl_survey();
                    survey.SurveyQueID = comments_surveyId;
                    survey.Comments = feedback_comments;
                    survey.AssistantID = tbltask.AssistantID;
                    survey.TaskID = TaskID;
                    surveylist.Add(survey);
                }

                ctx.tbl_survey.AddRange(surveylist);
                if (ctx.SaveChanges() > 0)
                {
                    int userid = Convert.ToInt32(Session["UserID"]);
                    var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    tbl_log log = new tbl_log();
                    log.TaskID = TaskID;
                    log.LogStatus = user.FirstName + " provides feedback";
                    log.LogDate = DateTime.Now;
                    ctx.tbl_log.Add(log);
                    ctx.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }



        }


        public ActionResult minitaskuserrecommendation(int id = 0, int TaskID = 0, string user_note = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var recommendation = ctx.tbl_recommendations.Where(x => x.RecID == id).FirstOrDefault();
                recommendation.TaskStatus = 1;
                ctx.Entry(recommendation).State = System.Data.Entity.EntityState.Modified;
                if (ctx.SaveChanges() > 0)
                {
                    var task = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();
                    var minitask = ctx.tbl_minitask.Where(x => x.MinitaskID == recommendation.MiniTaskID).FirstOrDefault();
                    if (minitask != null)
                    {
                        minitask.UserNote = user_note;
                        //task.Status = (int)TaskStatus.Complete;
                        ctx.Entry(minitask).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            //notification
                            tbl_notification notification = new tbl_notification();
                            var unread = (int)ReadStatus.Unread;

                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "User has picked an option to proceed with";
                            notification.Url = "/assistant-specific-task/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = task.AssistantID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();


                            notification.ReadStatus = unread.ToString();
                            notification.Notification = "User has picked an option to proceed with";
                            notification.Url = "/assistant-specific-tasks/" + TaskID;
                            notification.Date = DateTime.Now;
                            notification.ToID = task.ToID;
                            ctx.tbl_notification.Add(notification);
                            ctx.SaveChanges();

                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContext.Clients.All.SendNotification("User has picked an option to proceed with", "assistant", task.AssistantID);

                            IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContexts.Clients.All.SendNotification("User has picked an option to proceed with", "handler", task.ToID);

                            //log 
                            var user = ctx.tbl_user.Where(x => x.UserID == task.FromID).FirstOrDefault();
                            tbl_log log = new tbl_log();

                            log.TaskID = TaskID;
                            log.LogStatus = user.FirstName + " picks minitask opition " + recommendation.Name;
                            log.LogDate = DateTime.Now;
                            ctx.tbl_log.Add(log);
                            ctx.SaveChanges();

                            //log.TaskID = TaskID;
                            //log.LogStatus = user.FirstName + " marks task as completed";
                            //log.LogDate = DateTime.Now;
                            //ctx.tbl_log.Add(log);
                            //ctx.SaveChanges();
                        }
                    }



                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }


            }

        }

        public ActionResult minitaskmorerecommendation(int TaskID = 0, int miniTaskID = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var task = ctx.tbl_task.Where(x => x.TaskID == TaskID).FirstOrDefault();
                var minitask = ctx.tbl_minitask.Where(x => x.MinitaskID == miniTaskID).FirstOrDefault();
                minitask.MoreOptions = "Y";
                ctx.Entry(minitask).State = System.Data.Entity.EntityState.Modified;
                if (ctx.SaveChanges() > 0)
                {
                    tbl_notification notification = new tbl_notification();
                    var unread = (int)ReadStatus.Unread;

                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Request for more recommendation";
                    notification.Url = "/assistant-specific-task/" + TaskID;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.AssistantID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();


                    notification.ReadStatus = unread.ToString();
                    notification.Notification = "Request for more recommendation";
                    notification.Url = "/assistant-specific-tasks/" + TaskID;
                    notification.Date = DateTime.Now;
                    notification.ToID = task.ToID;
                    ctx.tbl_notification.Add(notification);
                    ctx.SaveChanges();

                    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContext.Clients.All.SendNotification("Task completed", "assistant", task.AssistantID);

                    IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    hubContexts.Clients.All.SendNotification("Task completed", "handler", task.ToID);
                    //log 
                    var user = ctx.tbl_user.Where(x => x.UserID == task.FromID).FirstOrDefault();
                    tbl_log log = new tbl_log();

                    log.TaskID = TaskID;
                    log.LogStatus = user.FirstName + " request for more options ";
                    log.LogDate = DateTime.Now;
                    ctx.tbl_log.Add(log);
                    ctx.SaveChanges();

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
        }
        #endregion

        #region stopRecurring

        public ActionResult stopRecurring(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var stoprecurring = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                stoprecurring.Repeating = Convert.ToString((int)RecurringStatus.Stop);
                ctx.Entry(stoprecurring).State = System.Data.Entity.EntityState.Modified;

                if (ctx.SaveChanges() > 0)
                {
                    tbl_log log = new tbl_log();
                    log.TaskID = id;
                    log.LogStatus = "Stopped task from recurring";
                    log.LogDate = DateTime.Now;
                    ctx.tbl_log.Add(log);
                    ctx.SaveChanges();


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion



        #region Log Events

        public ActionResult log_Events(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                ViewBag.taskid = id;
                return View();
            }
        }
        #endregion

        #region notification using signalr
        public ActionResult notifications()
        {
            return PartialView("dashboard_notification");
        }

        #endregion

        #region chat notification using signalr
        public ActionResult chatnotification()
        {
            return PartialView("dashboard_chatnotification");
        }

        #endregion
    }
}