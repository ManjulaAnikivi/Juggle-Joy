using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using juggle_joy.Models;

namespace juggle_joy.Areas.admin.Controllers
{
    public class accountController : Controller
    {
        // GET: admin/account


        #region Login

        [HttpGet]
        public ActionResult login(string RetUrl = "")
        {
            if (Session["AdminID"] == null)
            {
                ViewBag.RetUrl = RetUrl;
                ViewBag.msg = TempData["AdminLogInStatus"] == null ? "" : TempData["AdminLogInStatus"] as string;
                return View();
            }
            else
            {
                return RedirectToAction("dashboard");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login(LoginValidation admin, string RetUrl = "")
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var authorizeduser = ctx.tbl_admin.Where(x => x.UserName == admin.username).FirstOrDefault();
                    if (authorizeduser != null)
                    {
                        string hashedpassword = GlobalMethods.HashSHA1(admin.password + authorizeduser.Salt);

                        if (authorizeduser.PasswordHash == hashedpassword)
                        {
                            Session["AdminID"] = authorizeduser.AdminID;
                            Session["AdminName"] = authorizeduser.FullName;

                            if (RetUrl != "")
                            {
                                return Redirect(RetUrl);
                            }
                            else
                            {
                                return RedirectToAction("dashboard");
                            }

                        }
                        else
                        {
                            TempData["AdminLogInStatus"] = "Username/Password does not match";
                            return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                        }

                    }
                    else
                    {
                        TempData["AdminLogInStatus"] = "Username or password does not exist";
                        return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
                    }
                }
            }
            else
            {
                ViewBag.RetUrl = RetUrl;
                return View(admin);
            }

        }

        #endregion

        #region Forgot Password

        public ActionResult forgot_password()
        {
            ViewBag.msg = TempData["forgotPasswordStatus"] == null ? "" : TempData["forgotPasswordStatus"] as string;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult forgot_password(string EmailId)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var authorizeduser = ctx.tbl_admin.Where(x => x.EmailID == EmailId).FirstOrDefault();
                if (authorizeduser != null)
                {
                    Guid userguid = System.Guid.NewGuid();
                    authorizeduser.PwdResetKey = userguid;
                    ctx.Entry(authorizeduser).State = System.Data.Entity.EntityState.Modified;

                    if (ctx.SaveChanges() > 0)
                    {

                        SendResetPasswordMailToAdmin(authorizeduser);
                        TempData["forgotPasswordStatus"] = "Reset password link is sent to your registered email id";

                    }
                    else
                    {
                        TempData["forgotPasswordStatus"] = "Something went wrong ! Try again";
                    }
                }
                else
                {
                    TempData["forgotPasswordStatus"] = "Email ID does not exists";

                }
            }
            return RedirectToAction("forgot_password");
        }
        #endregion

        #region updateprofile
        public ActionResult update_profile()
        {

            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["update_profile"] == null ? "" : TempData["update_profile"] as string;
                    int admin_id = Convert.ToInt32(Session["AdminID"]);
                    UpdateProfileValidation obj = new UpdateProfileValidation();
                    var admin = ctx.tbl_admin.Where(x => x.AdminID == admin_id).FirstOrDefault();
                    obj.AdminID = admin.AdminID;
                    obj.FullName = admin.FullName;
                    obj.EmailID = admin.EmailID;
                    obj.PhoneNo = admin.PhoneNo;
                    return View(obj);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }
        [HttpPost]
        public ActionResult update_profile(UpdateProfileValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var result = ctx.tbl_admin.Where(x => x.AdminID == obj.AdminID).FirstOrDefault();
                    result.FullName = obj.FullName;
                    result.EmailID = obj.EmailID;
                    result.PhoneNo = obj.PhoneNo;
                    ctx.Entry(result).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        Session["AdminName"] = obj.FullName;
                        TempData["update_profile"] = "Profile updated successfully";
                    }
                    else
                    {
                        TempData["update_profile"] = "Failed to update your profile";
                    }
                    return RedirectToAction("update_profile");
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion

        #region changepassword
        public ActionResult change_password()
        {
            if (Session["AdminId"] != null)
            {
                ViewBag.msg = TempData["changepass_status"] == null ? "" : TempData["changepass_status"] as string;
                return View();
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });

            }


        }
        [HttpPost]
        public ActionResult change_password(ChangePasswordValidation obj)
        {
            if (Session["AdminID"] != null)
            {
                if (ModelState.IsValid)
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        int AdminID = Convert.ToInt32(Session["AdminID"]);
                        var LoggedUser = ctx.tbl_admin.Where(x => x.AdminID == AdminID).FirstOrDefault();

                        string hashedPassword = HashSHA1(obj.OldPassword + LoggedUser.Salt);

                        if (LoggedUser.PasswordHash == hashedPassword)
                        {
                            // First create a new Guid for the user. This will be unique for each user
                            Guid userGuid = System.Guid.NewGuid();

                            // Hash the new password together with unique userGuid
                            hashedPassword = HashSHA1(obj.NewPassword + userGuid.ToString());

                            var res = ctx.tbl_admin.Where(x => x.AdminID == AdminID).FirstOrDefault();
                            res.PasswordHash = hashedPassword;
                            res.Salt = userGuid;
                            ctx.Entry(res).State = System.Data.Entity.EntityState.Modified;

                            if (ctx.SaveChanges() > 0)
                            {
                                SendPasswordChangedMailToAdmin(LoggedUser);
                                TempData["changepass_status"] = "Password changed successfully";
                                return RedirectToAction("change_password");
                            }
                            else
                            {
                                ViewBag.msg = "Password change failed";
                                return View(obj);
                            }

                        }
                        else
                        {
                            TempData["changepass_status"] = "Old password does not match";
                            return RedirectToAction("change_password");
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

        #region Reset-password

        public ActionResult reset_password(string id = "")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                RouteData.Values.Remove("id");
                return RedirectToAction("reset_password_notification");
            }
            else
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var admin = ctx.tbl_admin.Where(x => x.PwdResetKey.ToString() == id).FirstOrDefault();
                    if (admin != null)
                    {
                        ResetPasswordValidation resetpassword = new ResetPasswordValidation();
                        resetpassword.ResetCode = id;
                        return View(resetpassword);
                    }
                    else
                    {
                        RouteData.Values.Remove("id");
                        return RedirectToAction("reset_password_notification");
                    }
                }
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult reset_password(ResetPasswordValidation obj)
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var user = ctx.tbl_admin.Where(x => x.PwdResetKey.ToString() == obj.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        // First create a new Guid for the user. This will be unique for each user
                        Guid userGuid = System.Guid.NewGuid();

                        // Hash the new password together with unique userGuid
                        string hashedPassword = GlobalMethods.HashSHA1(obj.NewPassword + userGuid.ToString());

                        user.PasswordHash = hashedPassword;
                        user.Salt = userGuid;
                        //user.PwdResetKey = null;

                        ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            RouteData.Values.Remove("id");
                            TempData["resetpass_status"] = "Reset password successfull";
                            return RedirectToAction("reset_password_notification");
                        }
                        else
                        {
                            ViewBag.msg = "Something went wrong! Try again";
                            return View(obj);
                        }
                    }
                    else
                    {
                        RouteData.Values.Remove("id");
                        return RedirectToAction("reset_password_notification");
                    }
                }
            }
            else
            {
                ViewBag.msg = "Something went wrong";
                return View(obj);
            }
        }
        public ActionResult reset_password_notification()
        {
            ViewBag.msg = TempData["resetpass_status"] == null ? "" : TempData["resetpass_status"] as string;
            return View();
        }

        #endregion

        #region dashboard and signout

        public ActionResult dashboard()
        {
            if (Session["AdminID"] != null)
            {
                return View();
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult signout()
        {
            Session.Abandon();
            TempData["AdminLogInStatus"] = "You have logged out successfully";
            return RedirectToAction("login");
        }
        #endregion

        #region HashSHA1

        public static string HashSHA1(string value)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hash = sha1.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion

        #region mail sending code

        public bool SendResetPasswordMailToAdmin(tbl_admin user)
        {
            string strFrom = "";
            var pwdresetlink = GlobalMethods.webUrl + "admin/reset-password/" + user.PwdResetKey;
            //var pwdresetlink = GlobalMethods.webUrl + "/admin/account/reset_password/" + user.PwdResetKey;
            string mailContent = "Hi " + user.FullName + ",  <br /> " +
            "<br /> " +
            "<p>We received a request to reset the password for your account. </p> " +
            "<p>If you requested a password reset for your account, please click the button below to reset your password. If you didn’t make this request, please ignore this email.</p> " +
            "<p style='margin-top: 20px;'><a href='" + pwdresetlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: #fff;background-color: #f05c26;border-color: #f05c26;text-decoration: none;'>Reset Password</a></p>";

            string strbody = GlobalMethods.MailBody(mailContent);
            SendEmail Mail = new SendEmail();
            return Mail.SendEmailToRecipients(strFrom,user.EmailID, "", "", "Your JuggleJoy password has been changed", strbody, true);
        }

        public bool SendPasswordChangedMailToAdmin(tbl_admin user)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string mailContent = "Hi " + user.FullName + ",  <br /> " +
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