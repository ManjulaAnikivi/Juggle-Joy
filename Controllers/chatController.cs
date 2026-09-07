using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using juggle_joy.Hubs;
using juggle_joy.Models;
using Microsoft.AspNet.SignalR;

namespace juggle_joy.Controllers
{
    public class chatController : Controller
    {
        // GET: chat
     
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult inbox(int id = 0,int aid=0)
        {
            HttpCookie aCookie = Request.Cookies["Name"];
            if (Session["UserID"] != null || aCookie != null)
            {

                int userid = 0;
                if (Session["UserID"] != null)
                {
                    userid = Convert.ToInt32(Session["UserID"]);
                }
                else
                {
                    userid = Convert.ToInt32(aCookie.Value);
                }
                if(userid==id)
                {
                    return RedirectToAction("dashboard", "my_account");
                }
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.id = id;
                    ViewBag.adid = aid;
                    //ViewBag.SportsId = new SelectList(ctx.tbl_sportscategory.OrderBy(x => x.SportsName).ToList(), "SportsId", "SportsName");
                    //ViewBag.cSportsId = new SelectList(ctx.tbl_sportscategory.OrderBy(x => x.SportsName).ToList(), "SportsId", "SportsName");
                    ViewBag.msg = TempData["chatstatus"] == null ? "" : TempData["chatstatus"] as string;
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Log_In", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        //public ActionResult getchatlist(int id = 0, int aid = 0)
        //{            
        //    HttpCookie aCookie = Request.Cookies["Name"];
        //    int? fromid = 0;
        //    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
        //    {
        //        if (Session["UserID"] != null || aCookie != null || Session["HandlerID"] != null || Session["AssistantID"] != null)
        //    {
        //        if (aCookie != null)
        //        {
        //            fromid = Convert.ToInt32(aCookie.Value);
        //        }
        //        if (Session["UserID"] != null)
        //        {
        //            var udr= Session["UserID"];
        //            fromid = Convert.ToInt32(Session["UserID"]);
        //        }
        //        if (Session["HandlerID"] != null)
        //        {
        //            var udr = Session["HandlerID"];
        //            fromid = Convert.ToInt32(Session["HandlerID"]);
        //        }
        //        if (Session["AssistantID"] != null)
        //        {
        //            var assid = Convert.ToInt32(Session["AssistantID"]);
        //            fromid = ctx.tbl_user.Where(x => x.UserID == assid).Select(x => x.HandlerID).FirstOrDefault();
        //        }
        //    }

        //    ViewBag.id = id;
        //    ViewBag.adid = aid;
        //    int addid = Convert.ToInt32(ViewBag.adid);

        //        if (id > 0)
        //        {
        //            var chatmsgread = ctx.tbl_chat.Where(x => x.ToId == fromid && x.TaskID == aid && x.FromId == id && x.ChatreadStatus == 0).ToList();
        //            if (chatmsgread.Count > 0)
        //            {
        //                chatmsgread.ForEach(x => x.ChatreadStatus = 1);
        //                ctx.SaveChanges();
        //            }
        //        }

        //        return View(ctx.tbl_chat.Where(x => x.TaskID == addid && (x.ToId == id || x.FromId == id) && (x.ToId == fromid || x.FromId == fromid)).OrderBy(x => x.ChatDate).ToList());
        //    }

        //}



        public ActionResult getchatlist(int taskid = 0, int fromid = 0)
        {
            HttpCookie aCookie = Request.Cookies["Name"];
          
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var tbltask = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                if (Session["UserID"] != null || aCookie != null || Session["HandlerID"] != null || Session["AssistantID"] != null)
                {
                    if (aCookie != null)
                    {
                        fromid = Convert.ToInt32(aCookie.Value);
                    }
                    if (Session["UserID"] != null)
                    {
                        fromid = Convert.ToInt32(Session["UserID"]);
                    }
                    if (Session["HandlerID"] != null)
                    { 
                        fromid = Convert.ToInt32(Session["HandlerID"]);
                    }
                    if (Session["AssistantID"] != null)
                    {
                        fromid = Convert.ToInt32(Session["AssistantID"]);
                    }
                }

                if (Session["HandlerID"] != null|| Session["AssistantID"] != null)
                {
                   
                    var chatmsgread = ctx.tbl_chat.Where(x => x.FromId == tbltask.FromID && x.ChatreadStatus == 0 && x.TaskID == taskid).ToList();
                    if (chatmsgread.Count() > 0)
                    {
                        chatmsgread.ForEach(x => x.ChatreadStatus = 1);
                        ctx.SaveChanges();
                    }
                }
                if (Session["UserID"] != null)
                {
                    var chatmsgread = ctx.tbl_chat.Where(x => x.FromId != fromid  && x.ChatreadStatus == 0 && x.TaskID==taskid).ToList();
                    if (chatmsgread.Count() > 0)
                    {
                        chatmsgread.ForEach(x => x.ChatreadStatus = 1);
                        ctx.SaveChanges();
                    }
                }

                ViewBag.taskid = taskid;
                var chatlist=ctx.tbl_chat.Where(x => x.TaskID==taskid).OrderBy(x => x.ChatDate).ToList();

                return View(chatlist);
            }

        }

        public JsonResult getchatmsgcount(int taskid = 0)
        {
            HttpCookie aCookie = Request.Cookies["Name"];
            int uid = Convert.ToInt32(Session["UserID"]);
            int chatmsgcount = 0;
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["UserID"] != null || aCookie != null)
                {
                     chatmsgcount = ctx.tbl_chat.Where(x => x.ChatreadStatus == 0 && x.TaskID == taskid && x.FromId!= uid).ToList().Count();
                }
                if (Session["HandlerID"] != null || Session["AssistantID"] != null)
                {
                    var tbltask = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                    chatmsgcount = ctx.tbl_chat.Where(x => x.ChatreadStatus == 0 && x.TaskID == taskid && (x.FromId != tbltask.ToID && x.FromId != tbltask.AssistantID)).ToList().Count();
                }
                return Json(chatmsgcount, JsonRequestBehavior.AllowGet);

            }
        }

        //public JsonResult getchatmsgcount(int taskid=0)
        //{
        //    HttpCookie aCookie = Request.Cookies["Name"];
        //    int? uid = 0;
        //    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
        //    {
        //        if (Session["UserID"] != null || aCookie != null)
        //    {
        //        if (aCookie != null)
        //        {
        //            uid = Convert.ToInt32(aCookie.Value);
        //        }
        //        if (Session["UserID"] != null)
        //        {
        //            uid = Convert.ToInt32(Session["UserID"]);
        //        }
        //        if (Session["HandlerID"] != null)
        //        {
        //            uid = Convert.ToInt32(Session["HandlerID"]);
        //        }
        //        if (Session["AssistantID"] != null)
        //        {
        //            var assid = Convert.ToInt32(Session["AssistantID"]);
        //            uid = ctx.tbl_user.Where(x => x.UserID == assid).Select(x => x.HandlerID).FirstOrDefault();
        //        }
        //    }          

        //        int chatmsgcount = ctx.tbl_chat.Where(x => x.ToId == uid && x.ChatreadStatus == 0 && x.TaskID== taskid).ToList().Count();
        //        return Json(chatmsgcount, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult uploadfile()
        {
            HttpCookie aCookie = Request.Cookies["Name"];
            if (Session["UserID"] != null || Session["AssistantID"] != null || Session["HandlerID"] != null || aCookie != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    tbl_chatfile obj = new tbl_chatfile();
                    var file = Request.Files[0];
                    if (file != null)
                    {
                        string Imagefilename = "_" + DateTime.Now.ToString("ddMMyy_HHmmss") + System.IO.Path.GetExtension(file.FileName);
                        string fileName = Path.GetFileName(Imagefilename);
                        FileInfo fi = new FileInfo(fileName);
                        string ext = fi.Extension;
                        string orgfilename = file.FileName;
                        if (ext.ToLower() == ".pdf")
                        {
                            file.SaveAs(Server.MapPath("~/Assets/chatfiles/" + orgfilename));
                            obj.FileName = orgfilename;
                            ctx.tbl_chatfile.Add(obj);
                            ctx.SaveChanges();
                        }
                        else if (ext.ToLower() == ".doc" || ext.ToLower() == ".docx")
                        {
                            file.SaveAs(Server.MapPath("~/Assets/chatfiles/" + orgfilename));
                            obj.FileName = orgfilename;
                            ctx.tbl_chatfile.Add(obj);
                            ctx.SaveChanges();
                        }
                        else if (ext.ToLower() == ".xlsx" || ext.ToLower() == ".xls")
                        {
                            file.SaveAs(Server.MapPath("~/Assets/chatfiles/" + orgfilename));
                            obj.FileName = orgfilename;
                            ctx.tbl_chatfile.Add(obj);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            Image imgPhoto = System.Drawing.Image.FromStream(file.InputStream);
                            imgPhoto.Save(Server.MapPath("~/Assets/chatfiles/" + orgfilename));
                            obj.FileName = orgfilename;
                            ctx.tbl_chatfile.Add(obj);
                            ctx.SaveChanges();
                        }

                        return Json(1, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(0, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult chatlist(int taskid = 0, int fromid = 0)
        {
            HttpCookie aCookie = Request.Cookies["Name"];

            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var tbltask = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                if (Session["UserID"] != null || aCookie != null || Session["HandlerID"] != null || Session["AssistantID"] != null)
                {
                    if (aCookie != null)
                    {
                        fromid = Convert.ToInt32(aCookie.Value);
                    }
                    if (Session["UserID"] != null)
                    {
                        fromid = Convert.ToInt32(Session["UserID"]);
                    }
                    if (Session["HandlerID"] != null)
                    {
                        fromid = Convert.ToInt32(Session["HandlerID"]);
                    }
                    if (Session["AssistantID"] != null)
                    {
                        fromid = Convert.ToInt32(Session["AssistantID"]);
                    }
                }

                if (Session["HandlerID"] != null || Session["AssistantID"] != null)
                {

                    var chatmsgread = ctx.tbl_chat.Where(x => x.FromId == tbltask.FromID && x.ChatreadStatus == 0 && x.TaskID == taskid).ToList();
                    if (chatmsgread.Count() > 0)
                    {
                        chatmsgread.ForEach(x => x.ChatreadStatus = 1);
                        ctx.SaveChanges();
                    }
                }
                if (Session["UserID"] != null)
                {
                    var chatmsgread = ctx.tbl_chat.Where(x => x.FromId != fromid && x.ChatreadStatus == 0 && x.TaskID == taskid).ToList();
                    if (chatmsgread.Count() > 0)
                    {
                        chatmsgread.ForEach(x => x.ChatreadStatus = 1);
                        ctx.SaveChanges();
                    }
                }

                ViewBag.taskid = taskid;
                var chatlist = ctx.tbl_chat.Where(x => x.TaskID == taskid).OrderBy(x => x.ChatDate).ToList();

                return View(chatlist);
            }

        }
    }
}