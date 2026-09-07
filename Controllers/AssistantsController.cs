using juggle_joy.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
//using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Twilio.TwiML.Messaging;

//these two required for signalR
using Microsoft.AspNet.SignalR;
using juggle_joy.Hubs;

namespace juggle_joy.Controllers
{
    public class AssistantsController : Controller
    {
        // GET: chk
        public ActionResult Index()
        {
            return View();
        }

        #region Assistant Login
        public ActionResult assistant_login(string RetUrl)
        {
            if (Session["AssistantID"] == null)
            {
                AssistantLoginValidation obj = new AssistantLoginValidation();
                //if (Request.Cookies["Assistant-Name"] != null && Request.Cookies["Assistant-password"] != null)
                //{
                //    //obj.EmailID = Request.Cookies["EmailID"].Value;
                //    //obj.PasswordHash = Request.Cookies["password"].Value;
                //    //return View(obj);
                //    ViewBag.EmailID = Request.Cookies["Assistant-Name"].Value;
                //    ViewBag.password = Request.Cookies["Assistant-password"].Value;
                //    ViewBag.remember_me = "on";
                //}
                ViewBag.login = "assistant-login";
                ViewBag.RetUrl = RetUrl;
                ViewBag.msg = TempData["UserLogInStatus"] == null ? "" : TempData["UserLogInStatus"] as string;
                return View();
            }
            else
            {
                return RedirectToAction("assistant_dashboard", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult assistant_login(AssistantLoginValidation assistant, String RetUrl = "", String remember_me = null)
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {


                    var authorizeduser = ctx.tbl_user.Where(x => x.UserName == assistant.UserName && x.UserType == "A").FirstOrDefault();
                    if (authorizeduser != null)
                    {
                        if (authorizeduser.Status == (int)UserStatus.Active)
                        {
                            string hashedpassword = GlobalMethods.HashSHA1(assistant.PasswordHash + authorizeduser.Salt);

                            if (authorizeduser.PasswordHash == hashedpassword)
                            {

                                if (remember_me != null && remember_me == "on")
                                {
                                    HttpCookie unameCookie = new HttpCookie("astfirstname");
                                    unameCookie.Value = authorizeduser.FirstName + authorizeduser.LastName.ToString();
                                    HttpContext.Response.Cookies.Add(unameCookie);
                                    unameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie ufnameCookie = new HttpCookie("astname");
                                    ufnameCookie.Value = authorizeduser.FirstName[0].ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(ufnameCookie);
                                    ufnameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie uidCookie = new HttpCookie("astid");
                                    uidCookie.Value = authorizeduser.UserID.ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(uidCookie);
                                    uidCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie ulnameCookie = new HttpCookie("astlname");
                                    ulnameCookie.Value = authorizeduser.LastName[0].ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(ulnameCookie);
                                    ulnameCookie.Expires = DateTime.Now.AddDays(365);
                                }

                                Session["AssistantID"] = authorizeduser.UserID;
                                Session["AssistantName"] = authorizeduser.FirstName + authorizeduser.LastName;
                                Session["AssistantFirstName"] = authorizeduser.FirstName[0].ToString().ToUpper();
                                if (authorizeduser.LastName != null)
                                {
                                    Session["AssistantLastName"] = authorizeduser.LastName[0].ToString().ToUpper();
                                }
                                if (RetUrl != "")
                                {
                                    return Redirect(RetUrl);
                                }
                                else
                                {
                                    return RedirectToAction("assistant_dashboard", "Assistants", new { RetUrl = Request.Url.ToString() });
                                }
                            }
                            else
                            {
                                TempData["UserLogInStatus"] = "Username/Password does not match";
                                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
                            }

                        }
                        else if (authorizeduser.Status == (int)UserStatus.Blocked)
                        {
                            TempData["UserLogInStatus"] = "Your account is blocked, please contact for support";

                        }
                        else
                        {
                            TempData["UserLogInStatus"] = "Opps! Something went wrong please login again";
                        }

                    }
                    else
                    {
                        TempData["UserLogInStatus"] = "Username or password does not exist";
                        return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
                    }
                    return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });

                }
            }
            else
            {
                ViewBag.RetUrl = RetUrl;
                return View("assistant_login", "Assistants");
            }
        }

        #endregion

        #region Forgot and Reset password
        public ActionResult forgot_password()
        {
            ViewBag.msg = TempData["forgotStatus"] == null ? "" : TempData["forgotStatus"] as string;
            return View();
        }

        [HttpPost]
        public ActionResult forgot_password(string emailid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var authorizeduser = ctx.tbl_user.Where(x => x.EmailID == emailid && x.UserType == "A").FirstOrDefault();
                if (authorizeduser != null)
                {
                    Guid userGuid = System.Guid.NewGuid();
                    authorizeduser.PwdResetKey = userGuid;
                    authorizeduser.PwdKeyCreatedTime = DateTime.Now;
                    ctx.Entry(authorizeduser).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        SendResetPasswordMailToUser(authorizeduser);
                        TempData["forgotStatus"] = "Reset password link has been sent to your registered email id";
                    }
                    else
                    {
                        TempData["forgotStatus"] = "Something went wrong ! Try again";
                    }
                }
                else
                {
                    TempData["forgotStatus"] = "Email ID does not exists";
                }
                return RedirectToAction("forgot_password", "Assistants");
            }
        }

        public ActionResult ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                RouteData.Values.Remove("id");
                return RedirectToAction("reset_password_notification");
            }
            else
            {
                ViewBag.msg = TempData["resetpass_status"] == null ? "" : TempData["resetpass_status"] as string;
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var user = ctx.tbl_user.Where(x => x.PwdResetKey.ToString() == id && x.UserType == "A").FirstOrDefault();
                    if (user != null)
                    {
                        DateTime Datetime = Convert.ToDateTime(user.PwdKeyCreatedTime);
                        DateTime x1hourLater = Datetime.AddHours(1.0);
                        DateTime current_dateTime = System.DateTime.Now;

                        if (current_dateTime <= x1hourLater)
                        {
                            AssistantResetPasswordValidation Resetpassword = new AssistantResetPasswordValidation();
                            Resetpassword.ResetCode = id;
                            return View(Resetpassword);
                        }
                        else
                        {
                            user.PwdResetKey = null;
                            user.PwdKeyCreatedTime = null;
                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                return RedirectToAction("reset_password_notification", "Assistants");
                            }
                            else
                            {
                                ViewBag.msg = "Something went wrong! Try again";
                                return View();
                            }
                        }
                    }
                    else
                    {
                        RouteData.Values.Remove("id");
                        return RedirectToAction("reset_password_notification", "Assistants");
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(AssistantResetPasswordValidation obj)
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var user = ctx.tbl_user.Where(x => x.PwdResetKey.ToString() == obj.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        DateTime Datetime = Convert.ToDateTime(user.PwdKeyCreatedTime);
                        DateTime x1hourLater = Datetime.AddHours(1);
                        DateTime current_dateTime = System.DateTime.Now;

                        if (current_dateTime <= x1hourLater)
                        {
                            // First create a new Guid for the user. This will be unique for each user
                            Guid userGuid = System.Guid.NewGuid();

                            // Hash the new password together with unique userGuid
                            string hashedPassword = GlobalMethods.HashSHA1(obj.NewPassword + userGuid.ToString());

                            user.PasswordHash = hashedPassword;
                            user.Salt = userGuid;
                            user.PwdResetKey = null;
                            user.PwdKeyCreatedTime = null;
                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;

                            if (ctx.SaveChanges() > 0)
                            {
                                SendResetPasswordConfirmationMailToUser(user);
                                RouteData.Values.Remove("id");
                                TempData["resetpassnotification"] = "Reset password successfully";
                                return RedirectToAction("reset_password_notification", "Assistants");
                            }
                            else
                            {
                                ViewBag.msg = "Something went wrong! Try again";
                                return View(obj);
                            }
                        }
                        else
                        {
                            user.PwdResetKey = null;
                            user.PwdKeyCreatedTime = null;
                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                return RedirectToAction("reset_password_notification");
                            }
                            else
                            {
                                ViewBag.msg = "Something went wrong! Try again";
                                return View();
                            }
                        }
                    }
                    else
                    {
                        RouteData.Values.Remove("id");
                        return RedirectToAction("reset_password_notification", "Assistants");
                    }
                }
            }
            else
            {
                ViewBag.msg = "Something went wrong! Try again";
                return View(obj);
            }
        }

        public ActionResult reset_password_notification()
        {
            ViewBag.msg = TempData["resetpassnotification"] == null ? "" : TempData["resetpassnotification"] as string;
            return View();
        }

        #endregion

        #region Assistant-Profile
        public ActionResult assistant_profile()
        {
            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["updatemsg"] == null ? "" : TempData["updatemsg"] as string;
                    int userid = Convert.ToInt32(Session["AssistantID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    if (userdetails != null)
                    {
                        AssistantProfileValidation profile = new AssistantProfileValidation();
                        profile.FirstName = userdetails.FirstName;
                        profile.LastName = userdetails.LastName;
                        profile.EmailID = userdetails.EmailID;
                        profile.PhoneNo = userdetails.PhoneNo;
                        return View(profile);
                    }
                    else
                    {
                        return View(new AssistantProfileValidation());
                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult assistant_profile(HandlerProfileValidation tmp)
        {
            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    int userid = Convert.ToInt32(Session["AssistantID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    userdetails.FirstName = tmp.FirstName;
                    userdetails.LastName = tmp.LastName;
                    userdetails.EmailID = tmp.EmailID;
                    userdetails.PhoneNo = tmp.PhoneNo;
                    db.Entry(userdetails).State = System.Data.Entity.EntityState.Modified;
                    if (db.SaveChanges() > 0)
                    {
                        TempData["updatemsg"] = "Profile updated successfully";
                        return RedirectToAction("assistant_profile");
                    }
                    else
                    {
                        TempData["updatemsg"] = "Failed to update, Try Again !";
                        return RedirectToAction("assistant_profile");
                    }
                }
            }
            else
            {

                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }

        }
        #endregion

        #region Assistant Change Password
        public ActionResult assistant_changepassword()
        {
            if (Session["AssistantID"] != null)
            {
                ViewBag.msg = TempData["changepass_status"] == null ? "" : TempData["changepass_status"] as string;
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult assistant_changepassword(AssistantChangePasswordValidation tmp)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["AssistantID"] != null)
                {
                    if (ModelState.IsValid)
                    {
                        int userid = Convert.ToInt32(Session["AssistantID"]);
                        var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                        string oldpassword = GlobalMethods.HashSHA1(tmp.OldPassword + user.Salt);
                        if (user.PasswordHash == oldpassword)
                        {
                            // First create a new Guid for the user. This will be unique for each user
                            Guid userGuid = System.Guid.NewGuid();

                            // Hash the new password together with unique userGuid
                            string hashedPassword = GlobalMethods.HashSHA1(tmp.NewPassword + userGuid.ToString());
                            user.PasswordHash = hashedPassword;
                            user.Salt = userGuid;
                            user.PwdResetKey = null;
                            user.PwdKeyCreatedTime = null;
                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;

                            if (ctx.SaveChanges() > 0)
                            {
                                SendPasswordChangedMailToUser(user);
                                TempData["changepass_status"] = "Password changed successfully";
                                return RedirectToAction("assistant_changepassword");
                            }
                            else
                            {
                                ViewBag.msg = "Something went wrong! Try again";
                                return View(tmp);
                            }
                        }
                        else
                        {
                            TempData["changepass_status"] = "Old Password does not match";
                            return RedirectToAction("assistant_changepassword");
                        }
                    }
                    else
                    {
                        return RedirectToAction("assistant_changepassword");
                    }
                }
                else
                {
                    return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        #endregion

        #region Assistant_dashboard and logout
        public ActionResult assistant_dashboard()
        {
            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    return View();
                }

            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult tasks_list(string keywords)
        {
            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.keywords = keywords;
                    int assistantid = Convert.ToInt32(Session["AssistantID"]);
                    var today = DateTime.Today;
                    var tasks = ctx.tbl_task
            .Where(x => (x.Status != (int)TaskStatus.Draft) && (x.AssistantID == null || x.AssistantID == assistantid) && (x.TaskTitle.ToLower().Contains(keywords.ToString()) || x.tbl_category.Category.ToLower().Contains(keywords.ToString()) || x.tbl_subcategory.SubCategory.ToLower().Contains(keywords.ToString())))
            .OrderBy(x => x.Deadline == null ? DateTime.MaxValue :
                            (x.Deadline < today ? DateTime.MaxValue : x.Deadline))
            .ThenBy(x => x.Deadline)
            .ToList();
                    return View(tasks);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult new_tasks()
        {
            if (Session["AssistantID"] != null)
            {
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult pending_tasks()
        {
            if (Session["AssistantID"] != null)
            {
                ViewBag.msg = TempData["task_acceptance"] == null ? "" : TempData["task_acceptance"] as string;
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult accepting_task(int id = 0)
        {
            if (id > 0)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    int assistantid = Convert.ToInt32(Session["AssistantID"]);
                    var task = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                    var assistant = ctx.tbl_user.Where(x => x.UserID == assistantid).FirstOrDefault();
                    task.Status = (int)TaskStatus.Ongoing;
                    task.AssistantID = assistantid;
                    task.ToID = assistant.HandlerID;
                    ctx.Entry(task).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        var logtbl = ctx.tbl_log.ToList();
                        var assttbl = ctx.tbl_user.Where(x => x.UserID == assistantid).FirstOrDefault();
                        //var existsID = logtbl.Where(x => (x.TaskID != null) && (x.TaskID == id)).FirstOrDefault();
                        //var logid = logtbl.Where(x => (existsID != null) && (x.LogID == existsID.LogID)).FirstOrDefault();
                        tbl_log obj = new tbl_log();
                        obj.TaskID = id;
                        obj.LogStatus = "Assigned to" + " " + assttbl.FirstName;
                        obj.LogDate = DateTime.Now;
                        ctx.tbl_log.Add(obj);
                        if (ctx.SaveChanges() > 0)
                        {
                            tbl_notification notify = new tbl_notification();
                            notify.ToID = task.ToID;
                            notify.Notification = "Task accepted by assistant";
                            char received = '0';
                            notify.ReadStatus = received.ToString();
                            notify.Url = "/assistant-specific-tasks/" + task.TaskID;
                            notify.Date = DateTime.Now;
                            ctx.tbl_notification.Add(notify);
                            ctx.SaveChanges();

                            notify.ToID = task.FromID;
                            notify.Notification = "Task in progress";
                            var unread = (int)ReadStatus.Unread;
                            notify.ReadStatus = unread.ToString();
                            notify.Url = "/taskDetails/" + task.TaskID;
                            notify.Date = DateTime.Now;
                            ctx.tbl_notification.Add(notify);
                            //ctx.SaveChanges();
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["task_acceptance"] = "Task accepted successfully";
                                RouteData.Values.Remove("id");

                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                                hubContext.Clients.All.SendNotification("Task accepted", "user", task.FromID);

                                return RedirectToAction("ongoing_tasks");
                            }
                            TempData["task_acceptance"] = "Task accepted successfully";
                            RouteData.Values.Remove("id");
                            return RedirectToAction("ongoing_tasks");

                        }
                        else
                        {
                            TempData["task_acceptance"] = "Failed to accept task";
                            RouteData.Values.Remove("id");
                            return RedirectToAction("ongoing_tasks");
                        }

                    }
                    else
                    {
                        TempData["task_acceptance"] = "Failed to accept task";
                        return RedirectToAction("ongoing_tasks");
                    }
                }
            }
            else
            {
                TempData["task_acceptance"] = "Something went wrong!";
                return RedirectToAction("ongoing_tasks");
            }

        }
        public ActionResult ongoing_tasks()
        {
            if (Session["AssistantID"] != null)
            {
                ViewBag.msg = TempData["task_acceptance"] == null ? "" : TempData["task_acceptance"] as string;
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }
        public ActionResult completed_tasks()
        {
            if (Session["AssistantID"] != null)
            {
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult assistant_specific_task(int id)


        {
            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.step1 = TempData["step1"] == null ? "" : TempData["step1"] as string;
                    var specific_task = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                    ViewBag.id = specific_task.FromID;
                    ViewBag.adid = id;
                    return View(specific_task);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("assistant_login", "Assistants", new { RetUrl = Request.Url.ToString() });
            }

        }


        [HttpPost]
        public JsonResult recommendation(RecommendationsValidation rcom, string notification = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                if (rcom.RecID > 0)
                {
                    var recommendations = ctx.tbl_recommendations.Where(x => x.RecID == rcom.RecID).FirstOrDefault();
                    recommendations.RecID = rcom.RecID;
                    recommendations.TaskID = Convert.ToInt32(rcom.TaskID);
                    recommendations.AssistantID = Convert.ToInt32(rcom.AssistantID);
                    recommendations.Name = rcom.Name;
                    recommendations.Rating = rcom.Rating;
                    recommendations.Note = rcom.Note;
                    recommendations.Price = rcom.Price;
                    recommendations.Availability = rcom.Availability;
                    recommendations.NextStep = rcom.NextStep;
                    recommendations.MiniTaskID = Convert.ToInt32(rcom.MiniTaskID);
                    recommendations.TaskStatus = (int)TaskStatus.Draft;
                    ctx.Entry(recommendations).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        if (notification != "")
                        {
                            int taskid = Convert.ToInt32(rcom.TaskID);
                            var userid = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                            tbl_notification notify = new tbl_notification();
                            notify.ToID = userid.FromID;
                            //notify.Notification = "New options added";
                            if (rcom.MiniTaskID != "" && rcom.MiniTaskID != null)
                            {
                                notify.Notification = "New options added to mini task";
                            }
                            else
                            {
                                notify.Notification = "New options added";
                            }
                            var read = (int)ReadStatus.Unread;
                            notify.ReadStatus = read.ToString();
                            notify.Url = "/taskDetails/" + rcom.TaskID;
                            notify.Date = DateTime.Now;
                            ctx.tbl_notification.Add(notify);
                            ctx.SaveChanges();

                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContext.Clients.All.SendNotification("Task accepted", "user", userid.FromID);
                        }
                        return Json(recommendations.RecID, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    tbl_recommendations obj = new tbl_recommendations();
                    obj.RecID = rcom.RecID;
                    obj.TaskID = Convert.ToInt32(rcom.TaskID);
                    obj.AssistantID = Convert.ToInt32(rcom.AssistantID);
                    obj.Name = rcom.Name;
                    obj.Rating = rcom.Rating;
                    obj.Note = rcom.Note;
                    obj.Price = rcom.Price;
                    obj.Availability = rcom.Availability;
                    obj.NextStep = rcom.NextStep;
                    obj.TaskStatus = (int)TaskStatus.Draft;
                    obj.MiniTaskID = Convert.ToInt32(rcom.MiniTaskID);
                    ctx.tbl_recommendations.Add(obj);
                    if (ctx.SaveChanges() > 0)
                    {
                        if (notification != "")
                        {
                            int taskid = Convert.ToInt32(rcom.TaskID);
                            var userid = ctx.tbl_task.Where(x => x.TaskID == taskid).FirstOrDefault();
                            tbl_notification notify = new tbl_notification();
                            notify.ToID = userid.FromID;
                            //notify.Notification = "Options added";
                            if (rcom.MiniTaskID != "" && rcom.MiniTaskID != null)
                            {
                                notify.Notification = "Options added to mini task";
                            }
                            else
                            {
                                notify.Notification = "Options added";
                            }
                            var read = (int)ReadStatus.Unread;
                            notify.ReadStatus = read.ToString();
                            notify.Url = "/taskDetails/" + rcom.TaskID;
                            notify.Date = DateTime.Now;
                            ctx.tbl_notification.Add(notify);
                            ctx.SaveChanges();

                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContext.Clients.All.SendNotification("Task accepted", "user", userid.FromID);
                        }
                        return Json(obj.RecID, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public ActionResult assistant_logout()
        {
            Response.Cookies["astfirstname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["astname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["astid"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["astlname"].Expires = DateTime.Now.AddDays(-1);
            Session.Abandon();
            TempData["UserLogInStatus"] = "Please login here";
            return RedirectToAction("assistant_login", "Assistants");
        }

        #endregion

        #region Note
        [HttpPost]
        public JsonResult Note(FormCollection note)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var id = 0;
                if (note.Get("NoteID") != "")
                {
                    id = int.Parse(note.Get("NoteID"));
                }

                if (id > 0)
                {
                    var notes = ctx.tbl_note.Where(x => x.NoteID == id).FirstOrDefault();
                    HttpPostedFileBase file = Request.Files["FilePath"];
                    if (file != null && file.FileName != "" && file.ContentLength > 0)
                    {
                        notes.Filepath = Path.GetFileName(file.FileName);
                        file.SaveAs(Server.MapPath("~/Assets/Notes/" + notes.Filepath));
                    }
                    notes.Note = note.Get("NoteID");
                    notes.Note = note.Get("Note").Trim();
                    notes.TaskID = Convert.ToInt32(note.Get("TaskID"));
                    notes.AssistantID = Convert.ToInt32(note.Get("AssistantID"));
                    //attach.Date = DateTime.Now;
                    notes.HandlerID = Convert.ToInt32(note.Get("HandlerID"));
                    ctx.Entry(notes).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        return Json(notes, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    HttpPostedFileBase file = Request.Files["FilePath"];
                    tbl_note attach = new tbl_note();
                    if (file != null && file.FileName != "" && file.ContentLength > 0)
                    {
                        attach.Filepath = Path.GetFileName(file.FileName);
                        file.SaveAs(Server.MapPath("~/Assets/Notes/" + attach.Filepath));
                    }
                    attach.NoteID = 0;
                    attach.Note = note.Get("Note").Trim();
                    attach.TaskID = Convert.ToInt32(note.Get("TaskID"));
                    attach.AssistantID = Convert.ToInt32(note.Get("AssistantID"));
                    //attach.Date = DateTime.Now;
                    attach.HandlerID = Convert.ToInt32(note.Get("HandlerID"));
                    ctx.tbl_note.Add(attach);
                    if (ctx.SaveChanges() > 0)
                    {
                        return Json(attach, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

            }
        }

        public ActionResult notelist(int taskid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                int HandlerID = Convert.ToInt32(Session["Assistant"]);
                var notes = ctx.tbl_note.Where(x => x.HandlerID == HandlerID && x.TaskID == taskid).ToList();
                ViewBag.taskid = taskid;
                return View(notes);
            }
        }

        public ActionResult Edit(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var Edit_note = ctx.tbl_note.Where(x => x.NoteID == id).FirstOrDefault();
                return Json(Edit_note, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteNote(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var note = ctx.tbl_note.Where(x => x.NoteID == id).FirstOrDefault();
                var noteid = note.NoteID;
                if (System.IO.File.Exists(Server.MapPath("~/Assets/Notes/" + note.Filepath)))
                {
                    System.IO.File.Delete(Server.MapPath("~/Assets/Notes/" + note.Filepath));
                }
                ctx.tbl_note.Remove(note);
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

        #region file upload

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult fileupload(HttpPostedFileBase fileattachment, int taskid = 0)
        {

            if (Session["AssistantID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var userid = Convert.ToInt32(Session["AssistantID"]);
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



        public ActionResult deletefile(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var userid = Convert.ToInt32(Session["AssistantID"]);
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
        public ActionResult ast_notifications()
        {
            return PartialView("assistant_notification");
        }



        #endregion

        #region chat notification using signalr
        public ActionResult chatnotification()
        {
            return PartialView("assistant_chatnotification");
        }
        #endregion
        #region Mail
        private bool SendResetPasswordMailToUser(tbl_user user)
        {
            using (db_jugglejoyEntities db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var pwdresetlink = GlobalMethods.webUrl + "assistant-reset-password/" + user.PwdResetKey;

                string mailContent = "Hi " + user.FirstName + " " + user.LastName + ",  <br /> " +
                    "<p>We received a request to reset the password for your account. </p> " +
                    "<p>If you requested a password reset for your account, please click the button below to reset your password. If you didn’t make this request, please ignore this email.</p> " +
                    "<p style='margin-top: 20px;'><a href='" + pwdresetlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Reset Password</a></p>";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, user.EmailID, "", "", "Juggle joy Reset Password Link", strbody, true);
            }
        }


        private bool SendResetPasswordConfirmationMailToUser(tbl_user user)
        {
            string strFrom = "";
            string mailContent = "Hi " + user.FirstName + " " + user.LastName + ",  <br /> " +
                    "<p>You recently changed the password associated with your juggle joy account.</p> " +
                    "<p>If you did not make this change, please reset your password.</p> ";
            string strbody = GlobalMethods.MailBody(mailContent);
            SendEmail Mail = new SendEmail();
            return Mail.SendEmailToRecipients(strFrom, user.EmailID, "", "", "Juggle Joy Reset Password Confirmation", strbody, true);
        }

        public bool SendPasswordChangedMailToUser(tbl_user user)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string mailContent = "Hi " + user.FirstName + " " + user.LastName + ",  <br /> " +
                "<br /> " +
                "<p>You recently changed the password associated with your Jugglejoy account.</p> " +
                "<p>If you did not make this change, please reset your password.</p> ";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, user.EmailID, "", "", "Your Jugglejoy password has been changed", strbody, true);
            }
        }

        #endregion
    }
}