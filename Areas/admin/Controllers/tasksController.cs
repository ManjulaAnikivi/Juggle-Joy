using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle_joy.Models;

namespace juggle_joy.Areas.admin.Controllers
{
    public class tasksController : Controller
    {
        #region Task Management

        public ActionResult received()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.ReceivedMsg = TempData["ReceivedMsg"] == null ? "" : TempData["ReceivedMsg"] as string;
                    var received = ctx.tbl_task.Where(x => x.Status == (int)TaskStatus.Received).ToList().OrderByDescending(x => x.TaskID);
                    return View(received);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult ongoing()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.ongoingMsg = TempData["ongoingMsg"] == null ? "" : TempData["ongoingMsg"] as string;
                    var active_users = ctx.tbl_task.Where(x => x.Status == (int)TaskStatus.Ongoing).ToList();
                    return View(active_users);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult completed()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.completedMsg = TempData["completedMsg"] == null ? "" : TempData["completedMsg"] as string;
                    var bloked_user = ctx.tbl_task.Where(x => x.Status == (int)TaskStatus.Complete).ToList();
                    return View(bloked_user);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult task_details(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var detail = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                return View(detail);
            }
        }




        public JsonResult saveassistant(int id, string asstID)
        {

            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var taskid = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                if (taskid != null)
                {
                    var assistantID = Convert.ToInt32(asstID);
                    taskid.AssistantID = assistantID;
                    var assistant = ctx.tbl_user.Where(x => x.UserID == assistantID).FirstOrDefault();
                    taskid.ToID = assistant.HandlerID;
                    taskid.Status = (int)TaskStatus.Ongoing;
                    //taskid.ToID = Convert.ToInt32(asstID);
                    ctx.Entry(taskid).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        //var logtbl = ctx.tbl_log.ToList();
                        //var assttbl = ctx.tbl_user.Where(x => x.UserID.ToString() == asstID).FirstOrDefault();
                        //var existsID = logtbl.Where(x => (x.TaskID != null) && (x.TaskID == id)).FirstOrDefault();
                        ////var logid = logtbl.Where(x => (existsID != null) && (x.LogID == existsID.LogID)).FirstOrDefault();
                        //tbl_log obj = new tbl_log();
                        //obj.TaskID = id;
                        //if (existsID == null)
                        //{
                        //    obj.LogStatus = "Assigned to" + " " + assttbl.UserName;
                        //    obj.LogDate = DateTime.Now;
                        //}
                        //else
                        //{
                        //    obj.LogStatus = "Re-Assigned to" + " " + assttbl.UserName;
                        //    obj.LogDate = DateTime.Now;
                        //}
                        //ctx.tbl_log.Add(obj);
                        //if (ctx.SaveChanges() > 0)
                        //{
                        //    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        //}
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);


                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }
            #endregion




        }
    }
}