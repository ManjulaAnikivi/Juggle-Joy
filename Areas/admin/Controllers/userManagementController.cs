using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class userManagementController : Controller
    {
        // GET: admin/userManagement
        public ActionResult Index()
        {
            return View();
        }

        #region User Management

        public ActionResult incomplete_registration()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.IncompleteRegListMsg = TempData["IncompleteRegListMsg"] == null ? "" : TempData["IncompleteRegListMsg"] as string;
                    ViewBag.userInUse = TempData["userInUse"] == null ? "" : TempData["userInUse"] as string;
                    ViewBag.userNotInUse = TempData["userNotInUse"] == null ? "" : TempData["userNotInUse"] as string;
                    var incomplete_reg = ctx.tbl_user.Where(x => x.Status == (int)UserStatus.IncompleteRegistration && x.UserType == "U").OrderByDescending(x => x.UserID).ToList();
                    return View(incomplete_reg);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult active()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.ActiveUsersListMsg = TempData["ActiveUsersListMsg"] == null ? "" : TempData["ActiveUsersListMsg"] as string;
                    ViewBag.userInUse = TempData["userInUse"] == null ? "" : TempData["userInUse"] as string;
                    ViewBag.userNotInUse = TempData["userNotInUse"] == null ? "" : TempData["userNotInUse"] as string;
                    var active_users = ctx.tbl_user.Where(x => x.Status == (int)UserStatus.Active && x.UserType == "U").OrderByDescending(x => x.UserID).ToList();
                    return View(active_users);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult blocked()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.BlockedUsersListMsg = TempData["BlockedUsersListMsg"] == null ? "" : TempData["BlockedUsersListMsg"] as string;
                    ViewBag.userInUse = TempData["userInUse"] == null ? "" : TempData["userInUse"] as string;
                    ViewBag.userNotInUse = TempData["userNotInUse"] == null ? "" : TempData["userNotInUse"] as string;
                    var bloked_user = ctx.tbl_user.Where(x => x.Status == (int)UserStatus.Blocked && x.UserType == "U").OrderByDescending(x => x.UserID).ToList();
                    return View(bloked_user);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }

        }

        [HttpPost]
        public ActionResult action(IEnumerable<int> Ids, string action)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (Ids != null && Ids.Count() > 0)
                    {
                        var user = ctx.tbl_user.Where(x => Ids.Contains(x.UserID)).Select(x => new { status = x.Status }).FirstOrDefault();

                        
                        var userinUse = "";
                        var usernotUse = "";
                        if (action == "Delete")
                        {
                            foreach (var item in Ids)
                            {
                                var tbluser = ctx.tbl_user.Where(x => x.UserID == item).FirstOrDefault();
                                var task = ctx.tbl_task.Where(x => x.FromID == item).FirstOrDefault();

                                if (task == null)
                                {
                                   
                                    usernotUse = usernotUse + tbluser.FirstName + ",";
                                    ctx.tbl_user.Remove(tbluser);
                                    ctx.SaveChanges();
                                }
                                else
                                {
                                    userinUse = userinUse + tbluser.FirstName + ",";
                                }
                            }

                            if (userinUse != "")
                            {
                                userinUse = userinUse.TrimEnd(',');
                                TempData["userInUse"] = "Users (" + userinUse + ") is/are can't be deleted";
                            }
                            if(usernotUse!="")
                            {
                                usernotUse = usernotUse.TrimEnd(',');
                                TempData["userNotInUse"] = "Users (" + usernotUse + ") is/are deleted";
                            }

                           
                           

                            switch (user.status)
                            {
                                case (int)UserStatus.IncompleteRegistration:  return RedirectToAction("incomplete_registration");
                                case (int)UserStatus.Active:  return RedirectToAction("active");
                                case (int)UserStatus.Blocked:  return RedirectToAction("blocked");
                                default: break;
                            }
                        }
                        else if (action == "Un-Block")
                        {
                            ctx.tbl_user.Where(x => Ids.Contains(x.UserID)).ToList().ForEach(x => { x.Status = (int)UserStatus.Active; });

                            if (ctx.SaveChanges() > 0)
                            {
                                //foreach (var item in Ids)
                                //{
                                //    User userdetail = ctx.Users.Where(x => x.UserID == item).FirstOrDefault();
                                //    SendActivationMailToUser(user);
                                //}
                                TempData["ActiveUsersListMsg"] = "Users account un-blocked successfully";
                                return RedirectToAction("active");
                            }
                            else
                            {
                                TempData["BlockedUsersListMsg"] = "Failed to un-block users account";
                                return RedirectToAction("blocked");
                            }
                        }
                        else if (action == "Block")
                        {
                            ctx.tbl_user.Where(x => Ids.Contains(x.UserID)).ToList().ForEach(x => { x.Status = (int)UserStatus.Blocked; });
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["BlockedUsersListMsg"] = "Users account blocked successfully";
                                return RedirectToAction("blocked");
                            }
                            else
                            {
                                TempData["ActiveUsersListMsg"] = "Failed to block users account";
                                return RedirectToAction("active");
                            }
                        }
                        else if (action == "Activate")
                        {
                            ctx.tbl_user.Where(x => Ids.Contains(x.UserID)).ToList().ForEach(x => { x.Status = (int)UserStatus.Active; });
                            var users = ctx.tbl_user.Where(x => Ids.Contains(x.UserID)).ToList();
                            if (ctx.SaveChanges() > 0)
                            {
                                foreach (var items in users)
                                {
                                    SendActivationSuccessMail(items);
                                }
                                TempData["ActiveUsersListMsg"] = "Users account activated successfully";
                                return RedirectToAction("active");
                            }
                            else
                            {
                                TempData["IncompleteRegListMsg"] = "Failed to activate users account";
                                return RedirectToAction("incomplete_registration");
                            }
                        }
                    }
                }
                return RedirectToAction("incomplete_registration_");
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult user_details(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var detail = ctx.tbl_user.Where(x => x.UserID == id).OrderByDescending(x => x.UserID).FirstOrDefault();
                return View(detail);
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Export(string GridHtml)
        {
            return File(Encoding.ASCII.GetBytes(GridHtml), "application/vnd.ms-excel", "Users.xls");
        }

        #endregion

        #region mail
        private bool SendActivationSuccessMail(tbl_user obj)
        {
            using (var db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string verifyemailurl = GlobalMethods.webUrl + "login";
                //string verifyemailurl = GlobalMethods.webUrl + "/verifyaccount/" + obj.Salt;

                string mailContent = @"Dear " + obj.FirstName + " " + obj.LastName + ",<br />" +
                    "<p>Congratulations! Your account has been activated by the admin successfully.</p> " +
                    "<p>You can access your juggle joy account by logging in.</p>" +
                    "<p style='margin-top: 20px;'><a href='" + verifyemailurl + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background: #00c1ab;text-decoration: none;'>Login</a></p>";
                string strBody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom,obj.EmailID, "", "", "Juggle joy", strBody, true);
            }
        }
        #endregion
    }
}