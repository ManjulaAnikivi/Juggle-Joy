using juggle_joy.Hubs;
using juggle_joy.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
//using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace juggle_joy.Controllers
{
    public class HandlersController : Controller
    {
        // GET: Handlers
        #region Handler Login
        public ActionResult handler_login(string RetUrl)
        {
            if (Session["HandlerID"] == null)
            {
                AssistantLoginValidation obj = new AssistantLoginValidation();
                //if (Request.Cookies["Handler-Name"] != null && Request.Cookies["Handler-password"] != null)
                //{
                //    //obj.EmailID = Request.Cookies["EmailID"].Value;
                //    //obj.PasswordHash = Request.Cookies["password"].Value;
                //    //return View(obj);
                //    ViewBag.EmailID = Request.Cookies["Handler-Name"].Value;
                //    ViewBag.password = Request.Cookies["Handler-password"].Value;
                //    ViewBag.remember_me = "on";
                //}
                ViewBag.login = "handler-login";
                ViewBag.RetUrl = RetUrl;
                ViewBag.msg = TempData["UserLogInStatus"] == null ? "" : TempData["UserLogInStatus"] as string;
                return View();
            }
            else
            {
                return RedirectToAction("handler_dashboard", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult handler_login(HandlerLoginValidation handler, String RetUrl = "", String remember_me = null)
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var authorizeduser = ctx.tbl_user.Where(x => x.UserName == handler.UserName && x.UserType == "H").FirstOrDefault();
                    if (authorizeduser != null)
                    {
                        if (authorizeduser.Status == (int)UserStatus.Active)
                        {
                            string hashedpassword = GlobalMethods.HashSHA1(handler.PasswordHash + authorizeduser.Salt);

                            if (authorizeduser.PasswordHash == hashedpassword)
                            {
                                if (remember_me != null && remember_me == "on")
                                {
                                    HttpCookie unameCookie = new HttpCookie("hdlrfirstname");
                                    unameCookie.Value = authorizeduser.FirstName + authorizeduser.LastName.ToString();
                                    HttpContext.Response.Cookies.Add(unameCookie);
                                    unameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie ufnameCookie = new HttpCookie("hdlrname");
                                    ufnameCookie.Value = authorizeduser.FirstName[0].ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(ufnameCookie);
                                    ufnameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie uidCookie = new HttpCookie("hdlrid");
                                    uidCookie.Value = authorizeduser.UserID.ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(uidCookie);
                                    uidCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie ulnameCookie = new HttpCookie("hdlrlname");
                                    ulnameCookie.Value = authorizeduser.LastName[0].ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(ulnameCookie);
                                    ulnameCookie.Expires = DateTime.Now.AddDays(365);

                                }

                                Session["HandlerID"] = authorizeduser.UserID;
                                Session["HandlerUserName"] = authorizeduser.FirstName + authorizeduser.LastName;
                                Session["HandlerFirstName"] = authorizeduser.FirstName[0].ToString().ToUpper();
                                if (authorizeduser.LastName != null)
                                {
                                    Session["HandlerLastName"] = authorizeduser.LastName[0].ToString().ToUpper();
                                }
                                if (RetUrl != "")
                                {
                                    return Redirect(RetUrl);
                                }
                                else
                                {
                                    return RedirectToAction("handler_dashboard", "Handlers", new { RetUrl = Request.Url.ToString() });
                                }
                            }
                            else
                            {
                                TempData["UserLogInStatus"] = "Username/Password does not match";
                                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
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
                        return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
                    }
                    return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
                }
            }
            else
            {
                ViewBag.RetUrl = RetUrl;
                return View("handler_login", "Handlers");
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
                var authorizeduser = ctx.tbl_user.Where(x => x.EmailID == emailid && x.UserType == "H").FirstOrDefault();
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
                return RedirectToAction("forgot_password", "Handlers");
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
                    var user = ctx.tbl_user.Where(x => x.PwdResetKey.ToString() == id && x.UserType == "H").FirstOrDefault();
                    if (user != null)
                    {
                        DateTime Datetime = Convert.ToDateTime(user.PwdKeyCreatedTime);
                        DateTime x1hourLater = Datetime.AddHours(1.0);
                        DateTime current_dateTime = System.DateTime.Now;

                        if (current_dateTime <= x1hourLater)
                        {
                            HandlerResetPasswordValidation Resetpassword = new HandlerResetPasswordValidation();
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
                                return RedirectToAction("reset_password_notification", "Handlers");
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
                        return RedirectToAction("reset_password_notification", "Handlers");
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(HandlerResetPasswordValidation obj)
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
                                return RedirectToAction("reset_password_notification", "Handlers");
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
                        return RedirectToAction("reset_password_notification", "Handlers");
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

        #region Add Assistants
        public ActionResult add_assistants(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["RegistrationMsg"] == null ? "" : TempData["RegistrationMsg"] as string;
                    if (id > 0)
                    {
                        var assistant = ctx.tbl_user.Where(x => x.UserID == id).FirstOrDefault();
                        HandlerAssistantValidation obj = new HandlerAssistantValidation();
                        obj.UserID = assistant.UserID;
                        obj.FirstName = assistant.FirstName;
                        obj.LastName = assistant.LastName;
                        obj.EmailID = assistant.EmailID;
                        obj.UserName = assistant.UserName;
                        return View(obj);
                    }
                    else
                    {
                        HandlerAssistantValidation obj = new HandlerAssistantValidation();
                        return View(obj);
                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult add_assistants(HandlerAssistantValidation asst)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    int handlerid = Convert.ToInt32(Session["HandlerID"]);

                    if (asst.UserID > 0)
                    {
                        var user = ctx.tbl_user.Where(x => x.UserID == asst.UserID).FirstOrDefault();

                        if (user != null)
                        {
                            user.UserID = asst.UserID;
                            user.FirstName = asst.FirstName;
                            user.LastName = asst.LastName;
                            user.EmailID = asst.EmailID;
                            user.UserName = asst.UserName;
                            user.RegDate = DateTime.Now;
                            user.HandlerID = handlerid;

                            ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["RegistrationMsg"] = "Record updated successfully";
                                return RedirectToAction("add_assistants");
                            }
                            else
                            {
                                RouteData.Values.Remove("id");
                                TempData["RegistrationMsg"] = "Record not updated";
                                return RedirectToAction("add_assistants");

                            }
                        }
                        else
                        {
                            RouteData.Values.Remove("id");
                            TempData["RegistrationMsg"] = "Record not found";
                            return RedirectToAction("add_assistants");
                        }
                    }
                    else
                    {
                        Guid Salt = System.Guid.NewGuid();
                        var encrypted_password = GlobalMethods.HashSHA1(asst.PasswordHash + Salt);
                        var assistant = 'A';
                        tbl_user user = new tbl_user();
                        user.FirstName = asst.FirstName;
                        user.LastName = asst.LastName;
                        user.EmailID = asst.EmailID;
                        user.UserName = asst.UserName;
                        user.UserType = Convert.ToString(assistant);
                        user.Status = (int)UserStatus.Active;
                        user.Salt = Salt;
                        user.PasswordHash = encrypted_password;
                        user.PwdResetKey = Salt;
                        user.RegDate = DateTime.Now;
                        user.HandlerID = handlerid;

                        ctx.tbl_user.Add(user);
                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["RegistrationMsg"] = "Assistant added successfully";
                        }
                        else
                        {
                            TempData["RegistrationMsg"] = "Unable to add Assistant";
                        }

                        return RedirectToAction("add_assistants");
                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }

        public JsonResult IsAssistantEmailExists(int UserID = 0, string EmailID = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (UserID == 0)
                {
                    //code for add method
                    if (!ctx.tbl_user.Any(x => x.EmailID.ToLower() == EmailID.Trim().ToLower() && x.UserType == "A"))
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
                    if (!ctx.tbl_user.Any(x => x.EmailID.ToLower() == EmailID.Trim().ToLower() && x.UserID != UserID && x.UserType == "A"))
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

        public ActionResult manage_assistant()
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    int handlerid = Convert.ToInt32(Session["HandlerID"]);
                    var list_assistant = ctx.tbl_user.Where(x => x.HandlerID == handlerid && x.Status == (int)UserStatus.Active).ToList();
                    ViewBag.msg = TempData["del_assistant"] == null ? "" : TempData["del_assistant"] as string;
                    ViewBag.assistantinuse=TempData["assistantinuse"] == null ? "" : TempData["assistantinuse"] as string;
                    return View(list_assistant);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }


        [HttpPost]
        public ActionResult delete_assistant(IEnumerable<int> IdsToDelete, string action)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (IdsToDelete != null && IdsToDelete.Count() > 0)
                    {
                        var user = ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).Select(x => new { status = x.Status }).FirstOrDefault();
                        var datauser = ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).ToList();
                        var assistant = "";
                        foreach (var item in datauser)
                        {
                            var task = ctx.tbl_task.Where(x => x.AssistantID == item.UserID).FirstOrDefault();
                            if (task != null)
                            {
                                assistant = assistant + item.FirstName + ",";
                            }
                            else
                            {
                                ctx.tbl_user.Remove(item);
                                ctx.SaveChanges();
                                
                            }
                        }

                        if (assistant != "")
                        {
                            assistant = assistant.TrimEnd(',');
                            TempData["assistantinuse"] = "Assistants (" + assistant + ") is/are assigned with task and can't be deleted";
                        }

                        //if (ctx.SaveChanges() > 0)
                        //{
                        //    TempData["del_assistant"] = "Records deleted successfully";
                        //}
                        //else
                        //{
                        //    TempData["del_assistant"] = "Failed to delete records";
                        //}
                    }
                }
                return RedirectToAction("manage_assistant");
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion

        #region Handler-Profile
        public ActionResult handler_profile()
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["updatemsg"] == null ? "" : TempData["updatemsg"] as string;
                    int userid = Convert.ToInt32(Session["HandlerID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    if (userdetails != null)
                    {
                        HandlerProfileValidation profile = new HandlerProfileValidation();
                        profile.FirstName = userdetails.FirstName;
                        profile.LastName = userdetails.LastName;
                        profile.EmailID = userdetails.EmailID;
                        profile.PhoneNo = userdetails.PhoneNo;
                        return View(profile);
                    }
                    else
                    {
                        return View(new HandlerProfileValidation());
                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult handler_profile(HandlerProfileValidation tmp)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    int userid = Convert.ToInt32(Session["HandlerID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    userdetails.FirstName = tmp.FirstName;
                    userdetails.LastName = tmp.LastName;
                    userdetails.EmailID = tmp.EmailID;
                    userdetails.PhoneNo = tmp.PhoneNo;
                    db.Entry(userdetails).State = System.Data.Entity.EntityState.Modified;
                    if (db.SaveChanges() > 0)
                    {
                        TempData["updatemsg"] = "Profile updated successfully";
                        return RedirectToAction("handler_profile");
                    }
                    else
                    {
                        TempData["updatemsg"] = "Failed to update, Try Again !";
                        return RedirectToAction("handler_profile");
                    }
                }
            }
            else
            {

                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }


        }
        #endregion

        #region Handler Change Psssword
        public ActionResult handler_changepassword()
        {
            if (Session["HandlerID"] != null)
            {
                ViewBag.msg = TempData["changepass_status"] == null ? "" : TempData["changepass_status"] as string;
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult handler_changepassword(HandlerChangePasswordValidation tmp)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["HandlerID"] != null)
                {
                    if (ModelState.IsValid)
                    {
                        int userid = Convert.ToInt32(Session["HandlerID"]);
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
                                return RedirectToAction("handler_changepassword");
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
                            return RedirectToAction("handler_changepassword");
                        }
                    }
                    else
                    {
                        return RedirectToAction("handler_changepassword");
                    }
                }
                else
                {
                    return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
                }
            }
        }

        #endregion

        #region Assistant_dashboard and logout
        public ActionResult handler_dashboard()
        {
            if (Session["HandlerID"] != null)
            {
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }
        public ActionResult tasks_searchlist(string keywords)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.keywords = keywords;
                    //ViewBag.count = count;
                    int handlerid = Convert.ToInt32(Session["HandlerID"]);
                    //var task = ctx.tbl_task.OrderByDescending(x => x.Deadline != null).ThenBy(x => x.Deadline.HasValue).ToList();
                    
                    var today = DateTime.Today;
                     var tasks = ctx.tbl_task.Where(x=>(x.Status!=(int)TaskStatus.Draft) && (x.ToID==null|| x.ToID== handlerid)
                    && (x.TaskTitle.ToLower().Contains(keywords.ToString()) || x.tbl_category.Category.ToLower().Contains(keywords.ToString()) || x.tbl_subcategory.SubCategory.ToLower().Contains(keywords.ToString())))
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
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }
        public ActionResult handler_sidebar()
        {
            if (Session["HandlerID"] != null)
            {
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult task_list(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    //ViewBag.count = count;
                    int handlerid = Convert.ToInt32(Session["HandlerID"]);
                    //var task = ctx.tbl_task.OrderByDescending(x => x.Deadline != null).ThenBy(x => x.Deadline.HasValue).ToList();
                    
                    var today = DateTime.Today;
                   
                       var  task = ctx.tbl_task
                       .OrderBy(x => x.Deadline == null ? DateTime.MaxValue :
                                       (x.Deadline < today ? DateTime.MaxValue : x.Deadline))
                       .ThenBy(x => x.Deadline)
                       .ToList();
                        if (id == 0)
                        {
                            var next_page = task.Where(x => x.Status == (int)TaskStatus.Received).Take(10).ToList();
                            return View(next_page);
                        }
                        else
                        {
                            var next_page = task.Where(x => x.Status == (int)TaskStatus.Received).Skip(10 * (id - 1)).Take(10).ToList();
                            ViewBag.id = id;
                            return View(next_page);
                        }
                    
                    
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }

        public ActionResult task_pending(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    var assistantid = id;
                    ViewBag.assistantid = assistantid;
                    //var pending_tasks = ctx.tbl_task.Where(x => x.AssistantID == assistantid && x.Status == (int)TaskStatus.Received).OrderByDescending(x => x.Deadline != null).ThenBy(x => x.Deadline.HasValue).ToList();
                    var today = DateTime.Today;
                    var pending_tasks = ctx.tbl_task.Where(x => x.AssistantID == assistantid && x.Status == (int)TaskStatus.Received)
                        .OrderBy(x => x.Deadline == null ? DateTime.MaxValue :
                                        (x.Deadline < today ? DateTime.MaxValue : x.Deadline))
                        .ThenBy(x => x.Deadline)
                        .ToList();
                    return View(pending_tasks);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }


        public ActionResult task_ongoing(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var assistantid = id;
                    ViewBag.assistantid = assistantid;

                    var pending_tasks = ctx.tbl_task.Where(x => x.AssistantID == assistantid && x.Status == (int)TaskStatus.Ongoing).ToList();
                    return View(pending_tasks);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }
        public ActionResult task_completed(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var assistantid = id;
                    ViewBag.assistantid = assistantid;

                    var pending_tasks = ctx.tbl_task.Where(x => x.AssistantID == assistantid && x.Status == (int)TaskStatus.Complete).ToList();
                    return View(pending_tasks);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }


        public ActionResult assistant_info(int id = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var info = ctx.tbl_task.Where(x => x.AssistantID == id).ToList();
                    ViewBag.id = id;
                    return View(info);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult assistant_specific_task(int id, int notification = 0)
        {
            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var specific_task = ctx.tbl_task.Where(x => x.TaskID == id).FirstOrDefault();
                    ViewBag.id = specific_task.FromID;
                    ViewBag.adid = id;
                    return View(specific_task);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("handler_login", "Handlers", new { RetUrl = Request.Url.ToString() });
            }

        }

        public JsonResult notification(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (id > 0)
                {
                    var notifications = ctx.tbl_notification.Where(x => x.NotificationID == id).FirstOrDefault();
                    var Read = (int)ReadStatus.Read;
                    notifications.ReadStatus = Read.ToString();
                    ctx.Entry(notifications).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { error = false }, JsonRequestBehavior.AllowGet);

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
                        tbl_notification notify = new tbl_notification();
                        notify.ToID = assistantID;
                        notify.Notification = "New Task Assigned";
                        var unread = (int)ReadStatus.Unread;
                        notify.ReadStatus = unread.ToString();
                        notify.Url = "/assistant-specific-task/" + taskid.TaskID;
                        notify.Date = DateTime.Now;
                        ctx.tbl_notification.Add(notify);
                        if (ctx.SaveChanges() > 0)
                        {
                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            hubContext.Clients.All.SendNotification("New Task Assigned", "assistant", assistantID);
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }

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
        }

        public ActionResult handler_logout()
        {
            Response.Cookies["hdlrfirstname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["hdlrname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["hdlrid"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["hdlrlname"].Expires = DateTime.Now.AddDays(-1);
            Session.Abandon();
            TempData["UserLogInStatus"] = "Please login here";
            return RedirectToAction("handler_login", "Handlers");
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
                    notes.Date = DateTime.Now;
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
                    attach.Date = DateTime.Now;
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
                int HandlerID = Convert.ToInt32(Session["HandlerID"]);
                var notes = ctx.tbl_note.Where(x => x.HandlerID == HandlerID && x.TaskID == taskid).OrderByDescending(x=>x.NoteID).ToList();
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

            if (Session["HandlerID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var userid = Convert.ToInt32(Session["HandlerID"]);
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
                var taskfile = ctx.tbl_taskattachment.Where(x => x.TaskAttachmentID == id).FirstOrDefault();
                var taskid = taskfile.TaskID;
                var userid = Convert.ToInt32(Session["HandlerID"]);
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
        public ActionResult handlernotifications()
        {
            return PartialView("handler_notification");
        }
        #endregion

        #region chat notification using signalr
        public ActionResult chatnotification()
        {
            return PartialView("handler_chatnotification");
        }

        #endregion

        #region Mail
        private bool SendResetPasswordMailToUser(tbl_user user)
        {
            using (db_jugglejoyEntities db = new db_jugglejoyEntities())
            {
                string strFrom = "";

                var pwdresetlink = GlobalMethods.webUrl + "handler-reset-password/" + user.PwdResetKey;

                string mailContent = "Hi " + user.FirstName + " " + user.LastName + ",  <br /> " +
                    "<p>We received a request to reset the password for your account. </p> " +
                    "<p>If you requested a password reset for your account, please click the button below to reset your password. If you didn’t make this request, please ignore this email.</p> " +
                    "<p style='margin-top: 20px;'><a href='" + pwdresetlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Reset Password</a></p>";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom,user.EmailID, "", "", "Juggle joy Reset Password Link", strbody, true);
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
            return Mail.SendEmailToRecipients(strFrom,user.EmailID, "", "", "Juggle Joy Reset Password Confirmation", strbody, true);
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
                return Mail.SendEmailToRecipients(strFrom,user.EmailID, "", "", "Your Jugglejoy password has been changed", strbody, true);
            }
        }

        #endregion



    }
}