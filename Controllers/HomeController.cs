using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Twilio.Rest.Verify.V2.Service;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Stripe;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using Microsoft.AspNet.SignalR.Messaging;
using System.Threading.Tasks;

namespace juggle_joy.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        #region User Registration

        public ActionResult Register(string id = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                TempData["email_exists"] = id;

                UserValidation reg = new UserValidation();
                ViewBag.msg = TempData["RegistrationMsg"] == null ? "" : TempData["RegistrationMsg"] as string;
                ViewBag.contactmsg = TempData["contactinfoMsg"] == null ? "" : TempData["contactinfoMsg"] as string;
                ViewBag.DelegatingMsg = TempData["DelegatingMsg"] == null ? "" : TempData["DelegatingMsg"] as string;
                ViewBag.info = TempData["emailid"] == null ? "" : TempData["emailid"] as string;
                ViewBag.email_exists = TempData["email_exists"] == null ? "" : TempData["email_exists"] as string;


                ViewBag.emailbind = TempData["emailbind"] == null ? "" : TempData["emailbind"] as string;
                ViewBag.passwordbind = TempData["passwordbind"] == null ? "" : TempData["passwordbind"] as string;
                ViewBag.firstnamebind = TempData["firstname"] == null ? "" : TempData["firstname"] as string;
                ViewBag.phonebind = TempData["phone"] == null ? "" : TempData["phone"] as string;
                ViewBag.zipcodebind = TempData["zipcode"] == null ? "" : TempData["zipcode"] as string;
                ViewBag.heardfrombind = TempData["heardfrom"] == null ? "" : TempData["heardfrom"] as string;
                //ViewBag.assistant = TempData["assistant"] == null ? "" : TempData["assistant"] as string;
                ViewBag.partneremailbind = TempData["partneremailbind"] == null ? "" : TempData["partneremailbind"] as string;
                ViewBag.mobileMsg = TempData["mobileMsg"] == null ? "" : TempData["mobileMsg"] as string;

                ViewBag.Address = TempData["Address"] == null ? "" : TempData["Address"] as string;
                
                ViewBag.CountryName = TempData["CountryName"] == null ? "" : TempData["CountryName"] as string;
                



                if (ViewBag.email_exists == "contact-info")
                {
                    ViewBag.contact = "Email already exists please fill your contact info";

                }
                if (ViewBag.email_exists == "modile-verification")
                {
                    ViewBag.mobile_Msg = "Email already exists please verify your mobile number";

                }
                if (ViewBag.email_exists == "payment-details")
                {
                    ViewBag.contact = "Email already exists and contact information is also filled please fill your payment details";

                }
                return View(reg);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserValidation user)
        {
            try
            {
                var isCaptchaValid = await GlobalMethods.IsCaptchaValid(user.UserGoogleCaptchaToken, "userRegister", Request.UserHostAddress);
                if (isCaptchaValid)
                {
                    ModelState.Clear();
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {

                        Guid Salt = System.Guid.NewGuid();
                        var encrypted_password = GlobalMethods.HashSHA1(user.PasswordHash + Salt);

                        tbl_user tbluser = new tbl_user();

                        var users = 'U';

                        tbluser.EmailID = user.EmailID;

                        tbluser.PasswordHash = encrypted_password;
                        tbluser.Salt = Salt;
                        tbluser.PwdResetKey = Salt;
                        tbluser.Status = (int)UserStatus.IncompleteRegistration;
                        tbluser.RegDate = DateTime.Now;
                        tbluser.UserType = Convert.ToString(users);

                        ctx.tbl_user.Add(tbluser);


                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["emailid"] = "step1";
                            Session["Register"] = tbluser.UserID;
                            TempData["emailbind"] = tbluser.EmailID;
                            TempData["passwordbind"] = "********";
                            TempData["RegistrationMsg"] = "Registered successfully and please fill your contact information";
                            return RedirectToAction("Register");
                        }
                        else
                        {
                            TempData["RegistrationMsg"] = "Registration Failed";
                            return RedirectToAction("Register");
                        }
                    }
                }
                else
                {
                    TempData["RegistrationMsg"] = "Oops! Something went wrong please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["RegistrationMsg"] = ex.Message;
            }
            return RedirectToAction("Register");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactInfo(UserValidation obj)
        {

            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                //var email = TempData["emailid"];
                int userid = Convert.ToInt32(Session["Register"]);


                if (userid == 0)
                {
                    TempData["contactinfoMsg"] = "Please register with your EmailId";
                }
                else
                {

                    var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    user.FirstName = obj.FirstName;
                    user.PhoneNo = obj.PhoneNo;
                    user.ZipPostalCode = obj.ZipPostalCode;
                    user.PartnerEmail = obj.PartnerEmail;

                    user.HeardFrom = obj.HeardFrom;
                    int otp = GenerateOTP();
                    user.otp = otp;
                    SendOTPViaSMS(user.PhoneNo, otp);

                    user.Address = obj.Address;
                    
                    var coutryid = ctx.tbl_country.Where(x => x.Country == obj.CountryName).Select(x => x.CountryID).FirstOrDefault();
                    user.CountryID = coutryid;
                    
                    ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {

                        TempData["emailid"] = "verification";
                        TempData["emailbind"] = user.EmailID;
                        TempData["passwordbind"] = "********";

                        TempData["firstname"] = user.FirstName;
                        TempData["phone"] = user.PhoneNo;
                        TempData["zipcode"] = user.ZipPostalCode;

                        TempData["heardfrom"] = user.HeardFrom.ToString();
                        TempData["partneremailbind"] = user.PartnerEmail;

                        TempData["Address"] = user.Address;
                        
                        TempData["CountryName"] = obj.CountryName;
                       

                        TempData["mobileMsg"] = "Your contact information is saved and please verify your mobile number";
                    }
                    else
                    {
                        TempData["contactinfoMsg"] = "Failed to save your contact Information.";
                    }
                }


                return RedirectToAction("Register");
            }
        }

        [HttpPost]

        public ActionResult Delegating(string discountcode = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["Register"]);
                var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                if (userid == 0)
                {
                    TempData["DelegatingMsg"] = "Please register with your EmailId";
                    return RedirectToAction("Register");
                }
                else
                {
                    var tblgift = ctx.tbl_gifts.Where(x => x.GiftCode == discountcode).FirstOrDefault();
                    tblgift.Status = (int)GiftPaymentStatus.UsedCoupon;
                    ctx.Entry(tblgift).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { failed = true }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

        }


        public ActionResult registration_status_msg()
        {
            if (Session["Register"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    int userid = Convert.ToInt32(Session["Register"]);
                    var userdetails = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    userdetails.Status = 1;
                    userdetails.PwdResetKey = null;
                    ctx.Entry(userdetails).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        SendRegistrationSuccessMail(userdetails);
                        ViewBag.msg = "Registered successfully";
                        Session.Abandon();
                        Session.Clear();
                        return View();
                    }
                    else
                    {
                        TempData["RegistrationMsg"] = "Something went wrong.Please try again.";
                        return RedirectToAction("Register");
                    }

                }
            }
            else
            {
                TempData["RegistrationMsg"] = "something went wrong";
                return RedirectToAction("Register");
            }
        }

        public ActionResult RegVerification_succs_message(string id)
        {
            using (var ctx = new db_jugglejoyEntities())
            {
                try
                {
                    var _users = ctx.tbl_user.Where(x => x.PwdResetKey == new Guid(id)).FirstOrDefault();
                    if (_users != null)
                    {
                        _users.Status = 1;
                        _users.PwdResetKey = null;
                        ctx.Entry(_users).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            ViewBag.username = _users.FirstName + " " + _users.LastName;
                            ViewBag.msg = "Account verified successfully";
                            return View();
                        }
                        else
                        {
                            ViewBag.msg = "Account verified failed";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.msg = "Link is expired, please contact for support.";
                        return View();
                    }
                }
                catch (Exception)
                {
                    ViewBag.msg = "Oops! Something went wrong please try again.";
                    return View();
                }
            }


        }
        public JsonResult IsEmailIdExist(string EmailID = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                //return Json(!ctx.tbl_user.Any(x => x.EmailID.ToLower() == EmailID.Trim().ToLower() && x.UserType == "U"), JsonRequestBehavior.AllowGet);
                if (!ctx.tbl_user.Any(x => (x.EmailID.ToLower() == EmailID.Trim().ToLower() && x.UserType == "U") || (x.PartnerEmail.ToLower() == EmailID.Trim().ToLower() && x.UserType == "U")))
                {
                    //if emailID not exist
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //if exist
                    return Json(false, JsonRequestBehavior.AllowGet);
                }


            }
        }


        public JsonResult EmailExists(String id = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (id != "")
                {
                    var emailexists = ctx.tbl_user.Where(x => (x.EmailID == id && x.UserType == "U") || (x.PartnerEmail == id && x.UserType == "U")).FirstOrDefault();

                    if (emailexists != null && (emailexists.EmailID == id || emailexists.PartnerEmail == id))
                    {
                        var tblsubscription = ctx.tbl_subscription.Where(x => x.UserID == emailexists.UserID).FirstOrDefault();
                        //exists
                        if (emailexists.FirstName == null && emailexists.ZipPostalCode == null)
                        {
                            Session["Register"] = emailexists.UserID;
                            TempData["emailbind"] = emailexists.EmailID;

                            TempData["passwordbind"] = "********";
                            return Json(new { contact = true }, JsonRequestBehavior.AllowGet);
                        }
                        else if (emailexists.FirstName != null && emailexists.ZipPostalCode != null && emailexists.otp != null)
                        {
                            Session["Register"] = emailexists.UserID;
                            TempData["emailbind"] = emailexists.EmailID;
                            TempData["passwordbind"] = "********";
                            TempData["firstname"] = emailexists.FirstName;
                            TempData["phone"] = emailexists.PhoneNo;
                            TempData["zipcode"] = emailexists.ZipPostalCode;
                            TempData["heardfrom"] = emailexists.HeardFrom.ToString();
                            TempData["partneremailbind"] = emailexists.PartnerEmail;


                            TempData["Address"] = emailexists.Address;
                            TempData["City"] = emailexists.City;

                            var coutry = ctx.tbl_country.Where(x => x.CountryID == emailexists.CountryID).Select(x => x.Country).FirstOrDefault();

                            var state = ctx.tbl_state.Where(x => x.StateID == emailexists.State && x.CountryID == emailexists.CountryID).Select(x => x.State).FirstOrDefault();
                            TempData["CountryName"] = coutry;
                            TempData["StateName"] = state;
                            return Json(new { modileverification = true }, JsonRequestBehavior.AllowGet);
                        }
                        else if (emailexists.Status == (int)UserStatus.Active)
                        {
                            return Json(new { verification = true }, JsonRequestBehavior.AllowGet);
                        }
                        else if (tblsubscription != null)
                        {
                            Session["Register"] = emailexists.UserID;
                            TempData["RegistrationStatusMsg"] = "Registered successfully";
                            return Json(new { subscription = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Session["Register"] = emailexists.UserID;

                            TempData["emailbind"] = emailexists.EmailID;
                            TempData["passwordbind"] = "********";
                            TempData["firstname"] = emailexists.FirstName;
                            TempData["phone"] = emailexists.PhoneNo;
                            TempData["zipcode"] = emailexists.ZipPostalCode;
                            TempData["heardfrom"] = emailexists.HeardFrom.ToString();
                            TempData["partneremailbind"] = emailexists.PartnerEmail;

                            TempData["Address"] = emailexists.Address;
                            TempData["City"] = emailexists.City;

                            var coutry = ctx.tbl_country.Where(x => x.CountryID == emailexists.CountryID).Select(x => x.Country).FirstOrDefault();
                           
                            var state = ctx.tbl_state.Where(x => x.StateID == emailexists.State && x.CountryID == emailexists.CountryID).Select(x => x.State).FirstOrDefault();
                            TempData["CountryName"] = coutry;
                            TempData["StateName"] = state;
                            return Json(new { delgate = true }, JsonRequestBehavior.AllowGet);
                        }

                    }
                    else
                    {
                        //not exist
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

                    }

                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult IsPartnerEmailIdExist(string PartnerEmail = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var UserID = Convert.ToInt32(Session["Register"]);
                ////return Json(!ctx.tbl_user.Any(x => x.EmailID.ToLower() == EmailID.Trim().ToLower() && x.UserType == "U"), JsonRequestBehavior.AllowGet);


                //code for add method
                if (!ctx.tbl_user.Any(x => (x.EmailID.ToLower() == PartnerEmail.Trim().ToLower() && x.UserType == "U") || (x.PartnerEmail.ToLower() == PartnerEmail.Trim().ToLower() && x.UserID != UserID && x.UserType == "U")))
                {
                    //if emailID not exist
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //if exist
                    return Json(false, JsonRequestBehavior.AllowGet);
                }


            }
        }

        [HttpPost]
        public ActionResult discountcode(string id = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                var tbltempgift = ctx.tbl_tempgifts.Where(x => x.GiftCode == id).FirstOrDefault();

                var tblgift = ctx.tbl_gifts.Where(x => x.GiftCode == id).FirstOrDefault();

                if (tbltempgift == null && tblgift == null)
                {
                    return Json(new { giftcode = true }, JsonRequestBehavior.AllowGet);
                }
                else if (tbltempgift != null && tblgift == null)
                {
                    return Json(new
                    {
                        paymentNotDone = true,
                        DiscountedPrice = tbltempgift.DiscountedPrice,
                        ActualPrice = tbltempgift.ActualPrice
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (tblgift != null)
                {
                    if (tblgift.Status == (int)GiftPaymentStatus.UsedCoupon)
                    {
                        return Json(new { couponUsed = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { paymentDone = true }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]

        public ActionResult ResendOTP()
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["Register"]);
                var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                if (userid == 0)
                {
                    TempData["DelegatingMsg"] = "Please register with your EmailId";
                    return RedirectToAction("Register");
                }
                else
                {
                    int otp = GenerateOTP();
                    user.otp = otp;
                    ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        SendOTPViaSMS(user.PhoneNo, otp);
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { failed = true }, JsonRequestBehavior.AllowGet);
                    }



                }
            }
        }
        #endregion

        #region mobile verification

        public ActionResult mobileVerification(string digit_1, string digit_2, string digit_3, string digit_4, string digit_5, int userid)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {

                int newNumber = int.Parse(digit_1.ToString() + digit_2.ToString() + digit_3.ToString() + digit_4.ToString() + digit_5.ToString());



                var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                if (newNumber == tbluser.otp)
                {
                    tbluser.otp = null;
                    ctx.Entry(tbluser).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {

                        return Json(new { verified = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { failedotp = true }, JsonRequestBehavior.AllowGet);
                    }


                }
                else
                {
                    return Json(new { failedverification = true }, JsonRequestBehavior.AllowGet);
                }

            }

        }

        public ActionResult VerifiedContinue()
        {
            int userid = Convert.ToInt32(Session["Register"]);
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                TempData["emailid"] = "step2";
                TempData["emailbind"] = tbluser.EmailID;
                TempData["passwordbind"] = "********";

                TempData["firstname"] = tbluser.FirstName;
                TempData["phone"] = tbluser.PhoneNo;
                TempData["zipcode"] = tbluser.ZipPostalCode;

                TempData["heardfrom"] = tbluser.HeardFrom.ToString();
                TempData["partneremailbind"] = tbluser.PartnerEmail;
                TempData["contactinfoMsg"] = "Your contact information is saved and please fill your payment details";
                return Json(true, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Register");
            }


        }
        #endregion

        #region Login

        public ActionResult login(String RetUrl = "")
        {
            if (Session["UserID"] == null)
            {
                UserLoginValidation obj = new UserLoginValidation();

                //if (Request.Cookies["EmailID"] != null && Request.Cookies["password"] != null)
                //{
                //    //obj.EmailID = Request.Cookies["EmailID"].Value;
                //    //obj.PasswordHash = Request.Cookies["password"].Value;
                //    //return View(obj);
                //    ViewBag.EmailID = Request.Cookies["EmailID"].Value;
                //    ViewBag.password = Request.Cookies["password"].Value;
                //    ViewBag.remember_me = "on";
                //}
                ViewBag.login = "user-login";
                ViewBag.RetUrl = RetUrl;
                ViewBag.msg = TempData["UserLogInStatus"] == null ? "" : TempData["UserLogInStatus"] as string;
                return View();
            }
            else
            {
                return RedirectToAction("dashboard", "Task", new { RetUrl = Request.Url.ToString() });
            }
        }

        [HttpPost]
        public ActionResult login(UserLoginValidation user, String RetUrl = "", String remember_me = null)
        {
            if (ModelState.IsValid)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var authorizeduser = ctx.tbl_user.Where(x => (x.EmailID == user.EmailID && x.UserType == "U") || (x.PartnerEmail == user.EmailID && x.UserType == "U")).FirstOrDefault();
                    if (authorizeduser != null)
                    {
                        if (authorizeduser.Status == (int)UserStatus.Active)
                        {
                            string hashedpassword = GlobalMethods.HashSHA1(user.PasswordHash + authorizeduser.Salt);

                            if (authorizeduser.PasswordHash == hashedpassword)
                            {

                                if (remember_me != null && remember_me == "on")
                                {
                                    HttpCookie unameCookie = new HttpCookie("firstname");
                                    unameCookie.Value = authorizeduser.FirstName.ToString();
                                    HttpContext.Response.Cookies.Add(unameCookie);
                                    unameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie ufnameCookie = new HttpCookie("uname");
                                    ufnameCookie.Value = authorizeduser.FirstName[0].ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(ufnameCookie);
                                    ufnameCookie.Expires = DateTime.Now.AddDays(365);

                                    HttpCookie uidCookie = new HttpCookie("uid");
                                    uidCookie.Value = authorizeduser.UserID.ToString().ToUpper();
                                    HttpContext.Response.Cookies.Add(uidCookie);
                                    uidCookie.Expires = DateTime.Now.AddDays(365);
                                }


                                Session["UserID"] = authorizeduser.UserID;
                                //Session["UserName"] = authorizeduser.FirstName;
                                var uname = char.ToUpper(authorizeduser.FirstName[0]) + authorizeduser.FirstName.Substring(1);
                                Session["UserName"] = uname;
                                Session["FirstName"] = authorizeduser.FirstName[0].ToString().ToUpper();
                                if (authorizeduser.LastName != null)
                                {
                                    Session["LastName"] = authorizeduser.LastName[0].ToString().ToUpper();
                                }
                                if (RetUrl != "")
                                {
                                    return Redirect(RetUrl);
                                }
                                else
                                {
                                    return RedirectToAction("dashboard", "Task", new { RetUrl = Request.Url.ToString() });
                                }
                            }
                            else
                            {
                                TempData["UserLogInStatus"] = "EmailID/Password does not match";
                                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
                            }


                        }
                        else if (authorizeduser.Status == (int)UserStatus.IncompleteRegistration)
                        {
                            TempData["UserLogInStatus"] = "Your registration is incomplete please complete your registration.";
                        }
                        else if (authorizeduser.Status == (int)UserStatus.Blocked)
                        {
                            TempData["UserLogInStatus"] = "Your account is blocked, please contact for support";
                        }
                        else
                        {
                            TempData["UserLogInStatus"] = "Opps! Something went wrong please login again";
                        }
                        return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
                    }
                    else
                    {
                        TempData["UserLogInStatus"] = "EmailID or password does not exist";
                        return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
                    }
                }
            }
            else
            {
                ViewBag.RetUrl = RetUrl;
                return View("login", "Home");
            }
        }
        #endregion

        #region change Password
        public ActionResult changepassword()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.msg = TempData["changepass_status"] == null ? "" : TempData["changepass_status"] as string;
                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }

        }

        [HttpPost]
        public ActionResult changePassword(ChangeUPasswordValidation tmp)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (Session["UserID"] != null)
                {
                    if (ModelState.IsValid)
                    {
                        int userid = Convert.ToInt32(Session["UserID"]);
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
                                return RedirectToAction("changePassword");
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
                            return RedirectToAction("changePassword");
                        }
                    }
                    else
                    {
                        return RedirectToAction("changePassword");
                    }
                }
                else
                {
                    return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
                }
            }
        }
        #endregion

        #region profile
        public ActionResult profile()
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["updatemsg"] == null ? "" : TempData["updatemsg"] as string;
                    int userid = Convert.ToInt32(Session["UserID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    if (userdetails != null)
                    {
                        UpdateUProfileValidation profile = new UpdateUProfileValidation();
                        //profile.FirstName = userdetails.FirstName;
                        //profile.LastName = userdetails.LastName;
                        profile.UserID = userdetails.UserID;
                        profile.FullName = userdetails.FirstName + " " + userdetails.LastName;
                        profile.EmailID = userdetails.EmailID;
                        profile.PhoneNo = userdetails.PhoneNo;
                        profile.ZipPostalCode = userdetails.ZipPostalCode;
                        profile.HeardFrom = Convert.ToInt32(userdetails.HeardFrom);
                        profile.Partner_EmailID = userdetails.PartnerEmail;
                        profile.Address = userdetails.Address;
                        profile.City = userdetails.City;

                        ViewBag.CountryID = new SelectList(db.tbl_country.ToList(), "CountryID", "Country", userdetails.CountryID);
                        ViewBag.StateID = new SelectList(db.tbl_state.ToList(), "StateID", "State", userdetails.State);
                        return View(profile);
                    }
                    else
                    {
                        ViewBag.CountryID = new SelectList(db.tbl_country.ToList(), "CountryID", "Country");
                        ViewBag.StateID = new SelectList(db.tbl_state.ToList(), "StateID", "State");
                        return View(new UpdateUProfileValidation());
                    }
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        private int GenerateOTP()
        {
            Random rnd = new Random();
            int otpValue = rnd.Next(10000, 99999); // Generate a 4-digit OTP
            return otpValue;
        }

        [HttpPost]
        public ActionResult profile(UpdateUProfileValidation tmp)
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities db = new db_jugglejoyEntities())
                {
                    int userid = Convert.ToInt32(Session["UserID"]);
                    var userdetails = db.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                    //var user = tmp.FullName.Split("");
                    string[] fullname = tmp.FullName.Split(new char[0]);
                    userdetails.FirstName = fullname[0];
                    if (fullname[1] != "")
                        userdetails.LastName = fullname[1];
                    userdetails.EmailID = tmp.EmailID;
                    userdetails.PhoneNo = tmp.PhoneNo;
                    userdetails.ZipPostalCode = tmp.ZipPostalCode;
                    userdetails.HeardFrom = tmp.HeardFrom;
                    userdetails.PartnerEmail = tmp.Partner_EmailID;
                    userdetails.Address = tmp.Address;
                    userdetails.City = tmp.City;
                    userdetails.State = Convert.ToInt32(tmp.StateID);
                    userdetails.CountryID = Convert.ToInt32(tmp.CountryID);

                    db.Entry(userdetails).State = System.Data.Entity.EntityState.Modified;
                    if (db.SaveChanges() > 0)
                    {
                        if (userdetails.PartnerEmail != null)
                        {
                            SendEmailToPartnerEmailID(userdetails);
                        }
                        TempData["updatemsg"] = "Profile updated successfully";
                        return RedirectToAction("profile");
                    }
                    else
                    {
                        TempData["updatemsg"] = "Failed to update, Try Again !";
                        return RedirectToAction("profile");
                    }
                }
            }
            else
            {

                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("Login", "Home", new { RetUrl = Request.Url.ToString() });
            }


        }
        private void SendOTPViaSMS(string toPhoneNumber, int otp)
        {
            ///clien
            //var accountSid = "ACf437d22a95b8121fbf23b68181724f6c";
            //var authToken = "9662bcf976a6bda5c64877cce590b14e";
            //Amruta
            var accountSid = "AC0303797710834f66f6933599a1096bef";
            var authToken = "4347cb5369f315ea8850f1eb00df944b";


            TwilioClient.Init(accountSid, authToken);

            //client
            //var fromPhoneNumber = new PhoneNumber("+16185666104"); // Replace with your Twilio phone number
            //Amruta
            var fromPhoneNumber = new PhoneNumber("+12076055889");

            // Construct the message body with the OTP
            var messageBody = $"Your Jugglejoy OTP is: {otp}";

            ///client
            //var toPhoneNo = new PhoneNumber("+447446895475"); // Replace with the recipient's phone number
            //Amruta
            var toPhoneNo = new PhoneNumber("+919019240161");

            // Create message options
            var messageOptions = new CreateMessageOptions(toPhoneNo)
            {
                From = fromPhoneNumber,
                Body = messageBody
            };

            try
            {
                // Send the message
                var message = MessageResource.Create(messageOptions);
                Console.WriteLine(message.Body);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public JsonResult PartnerEmailIdExist(int UserID = 0, string Partner_EmailID = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                //if (UserID == 0)
                //{
                //    //code for add method
                //    if (!ctx.tbl_user.Any(x => x.PartnerEmail.ToLower() == Partner_EmailID.Trim().ToLower() && x.UserType == "U"))
                //    {
                //        return Json(true, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //        return Json(false, JsonRequestBehavior.AllowGet);
                //    }
                //}
                //else
                //{
                //    //code for edit
                //    if (!ctx.tbl_user.Any(x => x.PartnerEmail.ToLower() == Partner_EmailID.Trim().ToLower() && x.UserID != UserID && x.UserType == "U"))
                //    {
                //        return Json(true, JsonRequestBehavior.AllowGet);
                //    }
                //    else
                //    {
                //        return Json(false, JsonRequestBehavior.AllowGet);
                //    }
                //}

                if (!ctx.tbl_user.Any(x => (x.EmailID.ToLower() == Partner_EmailID.Trim().ToLower() && x.UserType == "U") || (x.PartnerEmail.ToLower() == Partner_EmailID.Trim().ToLower() && x.UserID != UserID && x.UserType == "U")))
                {
                    //if emailID not exist
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //if exist
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult GetStateList(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                List<tbl_state> state = ctx.tbl_state.Where(x => x.CountryID == id).ToList();
                var xr = new SelectList(state, "StateID", "State", id);
                return Json(xr, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Contact-Us
        public ActionResult contact_us()
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                ViewBag.msg = TempData["msg"] == null ? "" : TempData["msg"] as string;
                return View(new Contact_Us_Validation());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> contact_us(Contact_Us_Validation obj)
        {
            try
            {
                var isCaptchaValid = await GlobalMethods.IsCaptchaValid(obj.UserGoogleCaptchaToken, "userRegister", Request.UserHostAddress);
                if (isCaptchaValid)
                {
                    if (ModelState.IsValid)
                    {
                        using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                        {
                            tbl_contact contact = new tbl_contact();
                            contact.ContactName = obj.ContactName;
                            contact.ContactEmail = obj.ContactEmail;
                            contact.ContactPhone = obj.ContactPhone;
                            contact.ContactSubject = obj.ContactSubject;
                            contact.ContactMsg = obj.ContactMsg;
                            contact.ContactDate = DateTime.Now;
                            ctx.tbl_contact.Add(contact);

                            if (ctx.SaveChanges() > 0)
                            {
                                //SendContactMailToAdmin(contact);//mail sending to admin
                                TempData["msg"] = "Contact details submitted successfully";
                                return RedirectToAction("contact_us");
                            }
                            else
                            {
                                TempData["msg"] = "Failed to submit contact details";
                                return RedirectToAction("contact_us");
                            }
                        }

                    }
                }
                else
                {
                    TempData["msg"] = "Oops! Something went wrong please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
            }
            return RedirectToAction("contact_us");
        }
        #endregion

        #region Forgot Password and Reset Password
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
                var authorizeduser = ctx.tbl_user.Where(x => x.EmailID == emailid && x.UserType == "U").FirstOrDefault();
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
                return RedirectToAction("forgot_password", "Home");
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
                    var user = ctx.tbl_user.Where(x => x.PwdResetKey.ToString() == id && x.UserType == "U").FirstOrDefault();
                    if (user != null)
                    {
                        DateTime Datetime = Convert.ToDateTime(user.PwdKeyCreatedTime);
                        DateTime x1hourLater = Datetime.AddHours(1.0);
                        DateTime current_dateTime = System.DateTime.Now;

                        if (current_dateTime <= x1hourLater)
                        {
                            UserResetPasswordValidation Resetpassword = new UserResetPasswordValidation();
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
                                return RedirectToAction("reset_password_notification", "Home");
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
                        return RedirectToAction("reset_password_notification", "Home");
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordValidation obj)
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
                                return RedirectToAction("reset_password_notification", "Home");
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
                        return RedirectToAction("reset_password_notification", "Home");
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

        #region Dashboard and Logout

        public ActionResult logout()
        {
            Response.Cookies["firstname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["uname"].Expires = DateTime.Now.AddDays(-1);
            Response.Cookies["uid"].Expires = DateTime.Now.AddDays(-1);
            Session.Abandon();
            TempData["UserLogInStatus"] = "Please login here";
            return RedirectToAction("login", "Home");
        }

        #endregion

        #region Services

        public ActionResult our_services()
        {
            return View();
        }
        #endregion

        #region pricing

        public ActionResult pricing()
        {
            return View();
        }
        #endregion

        #region our story

        public ActionResult our_story()
        {
            return View();
        }
        #endregion

        #region press and media

        public ActionResult press()
        {
            return View();
        }
        #endregion

        # region book_a_call

        public ActionResult book_a_call()
        {
            return View();
        }
        #endregion


        #region gift_subscription
        public ActionResult gift_subscription()
        {
            ViewBag.msg = TempData["GiftMsg"] == null ? " " : TempData["GiftMsg"] as string;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> gift_subscription(string[] subscription_email, string subscription_recipient_name, string subscription_recipient_email, string UserGoogleCaptchaToken,
            string subscription_buyer_name, string subscription_buyer_email, string subscription_gift_message, string subscription, string percentage, string AP, string DP)
        {

            try
            {
                var isCaptchaValid = await GlobalMethods.IsCaptchaValid(UserGoogleCaptchaToken, "userRegister", Request.UserHostAddress);
                if (isCaptchaValid)
                {

                    var giftid = 0;
                    //if (Session["UserID"] != null)
                    //{
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {

                        Random generator = new Random();
                        String r = generator.Next(0, 1000000).ToString("D6");
                        var couponcode = "JJ" + r;

                        tbl_tempgifts tblgift = new tbl_tempgifts();
                        tblgift.GiftCode = couponcode;
                        tblgift.Duration = Convert.ToInt32(subscription);
                        tblgift.ActualPrice = Convert.ToDecimal(AP);
                        tblgift.DiscountedPrice = Convert.ToDecimal(DP);
                        tblgift.DiscountPercent = Convert.ToDecimal(percentage);
                        tblgift.RecepientName = subscription_recipient_name;
                        tblgift.RecepientEmail = subscription_recipient_email;
                        tblgift.SentDate = DateTime.Now;
                        tblgift.Message = subscription_gift_message;
                        tblgift.SenderEmail = subscription_buyer_email;
                        tblgift.SenderName = subscription_buyer_name;

                        int length = subscription_email.Length;
                        if (length == 2)
                        {
                            tblgift.MailReceiver = subscription_email[0] == "subscription_email_to_recipient" ? "Y" : null;
                            tblgift.MailSender = subscription_email[1] == "subscription_email_to_you" ? "Y" : null;
                        }
                        else
                        {
                            if (subscription_email[0] == "subscription_email_to_recipient")
                            {
                                tblgift.MailReceiver = subscription_email[0] == "subscription_email_to_recipient" ? "Y" : null;
                                tblgift.MailSender = null;
                            }
                            else if (subscription_email[0] == "subscription_email_to_you")
                            {
                                tblgift.MailSender = subscription_email[0] == "subscription_email_to_you" ? "Y" : null;
                                tblgift.MailReceiver = null;
                            }
                        }

                        ctx.tbl_tempgifts.Add(tblgift);
                        ctx.SaveChanges();
                        giftid = tblgift.TempGiftID;

                    }
                    return Json(new { giftid = giftid }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("gift_checkout", giftid);
                }
                else
                {
                    TempData["RegistrationMsg"] = "Oops! Something went wrong please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["RegistrationMsg"] = ex.Message;
            }
            return Json(new { failedCaptcha = true }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult gift_checkout(int id = 0)
        {
            ViewBag.giftid = id;
            return View();
        }

        [HttpPost]
        public ActionResult gift_Create(PaymentIntentCreateRequest request)
        {
            // Set the Stripe API key
            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;

            // Create customer options
            var customerOptions = new CustomerCreateOptions
            {
                Name = "Jenny Rosen",
                Address = new AddressOptions
                {
                    Line1 = "510 Townsend St",
                    PostalCode = "98140",
                    City = "San Francisco",
                    State = "CA",
                    Country = "US",
                },
            };

            // Create the customer
            var customerService = new CustomerService();
            var customer = customerService.Create(customerOptions);

            // Create payment intent options
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt32(Convert.ToDecimal(request.Items[2].Id) * 100), // CalculateOrderAmount(request.Items),
                Currency = "usd",
                Description = "Task Management",
                Customer = customer.Id,  // Associate the customer with the payment intent
                Metadata = new Dictionary<string, string>
                {
                     { "UserID", request.Items[0].Id },
                     { "ActualAmount", request.Items[1].Id },
                     { "PaidPrice", request.Items[2].Id },
                      { "DiscountCode", request.Items[3].Id },
                },
                PaymentMethodTypes = new List<string>
                {
                    "card",
                    //"apple_pay",
                    //"google_pay",
                    //"paypal",
                },

                // Uncomment if you want to enable automatic payment methods
                //AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                //{
                //    Enabled = true,
                //},
            };

            // Create the payment intent
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

            // Return the client secret
            return Json(new { clientSecret = paymentIntent.ClientSecret });
        }
        public ActionResult gift_success(string payment_intent, string payment_intent_client_secret, string redirect_status)
        {
            if (payment_intent != null && payment_intent_client_secret != null && redirect_status != null)
            {
                try
                {
                    StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
                    var service = new PaymentIntentService();
                    var res = service.Get(payment_intent);
                    if (res.Status == "succeeded")
                    {
                        int userid = Convert.ToInt16(Session["UserID"]);

                        int GiftID = Convert.ToInt16(res.Metadata["UserID"]);
                        var Subscriptionfees = Convert.ToDecimal(res.Metadata["PaidPrice"]);
                        var ActualAmount = res.Metadata["ActualAmount"];
                        var CouponCode = res.Metadata["DiscountCode"];
                        using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                        {
                            var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                            try
                            {
                                var gift = ctx.tbl_tempgifts.Where(x => x.TempGiftID == GiftID).FirstOrDefault();

                                tbl_gifts tblgift = new tbl_gifts();
                                tblgift.GiftCode = gift.GiftCode;
                                tblgift.Duration = gift.Duration;
                                tblgift.ActualPrice = gift.ActualPrice;
                                tblgift.DiscountedPrice = gift.DiscountedPrice;
                                tblgift.DiscountPercent = gift.DiscountPercent;
                                tblgift.RecepientName = gift.RecepientName;
                                tblgift.RecepientEmail = gift.RecepientEmail;
                                tblgift.SentDate = DateTime.Now;
                                tblgift.Message = gift.Message;
                                tblgift.SenderEmail = gift.SenderEmail;
                                tblgift.SenderName = gift.SenderName;
                                tblgift.MailReceiver = gift.MailReceiver;
                                tblgift.MailSender = gift.MailSender;
                                tblgift.Status = (int)GiftPaymentStatus.NotUsedCoupon;
                                ctx.tbl_gifts.Add(tblgift);
                                ctx.SaveChanges();

                                tbl_subscription objsuscription = new tbl_subscription();
                                objsuscription.UserID = userid;
                                objsuscription.SubscriptionFee = Subscriptionfees;
                                objsuscription.TransactionNo = res.ApplicationId;
                                objsuscription.TransactionStatus = res.Status;
                                objsuscription.TransactionMsg = "";

                                objsuscription.SubscriptionDate = DateTime.Now;
                                objsuscription.StripeOrderID = res.ApplicationId;
                                objsuscription.ExpirationDate = DateTime.Now.AddMonths(1);
                                objsuscription.StripePaymentType = "Stripe";
                                objsuscription.StripeReturnData = "";
                                objsuscription.GiftID = tblgift.GiftID;
                                ctx.tbl_subscription.Add(objsuscription);
                                ctx.tbl_tempgifts.Remove(ctx.tbl_tempgifts.Where(x => x.TempGiftID == GiftID).FirstOrDefault());
                                if (ctx.SaveChanges() > 0)
                                {
                                    if (tblgift.MailSender == "Y")
                                    {
                                        SendGiftMailToUser(user, tblgift);
                                    }
                                    if (tblgift.MailReceiver == "Y")
                                    {
                                        SendGiftMailToRecepient(tblgift);
                                    }
                                    TempData["GiftMsg"] = "payment successfull";
                                    return RedirectToAction("gift_subscription");
                                }
                                else
                                {
                                    TempData["PaymentMsg"] = "Oop's something went wrong try again !";
                                }

                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    else
                    {
                        TempData["PaymentMsg"] = "Payment Status: " + res.Status;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            else
            {
                TempData["PaymentMsg"] = "Oop's something went wrong try again !";
            }
            return RedirectToAction("payment_success");
        }
        #endregion

        #region Faq

        public ActionResult Faq(int id = 0)
        {
            return View();
        }

        public ActionResult Faqlist(int id = 0)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (id == 0)
                {
                    var initialfaq = ctx.tbl_faq_category.FirstOrDefault();
                    ViewBag.categoryid = initialfaq.Faq_Cat_id;
                    var faqs = ctx.tbl_faq.Where(x => x.Faq_categoryID == initialfaq.Faq_Cat_id).ToList();
                    return View(faqs);
                }
                else
                {
                    ViewBag.categoryid = id;
                    var faqs = ctx.tbl_faq.Where(x => x.Faq_categoryID == id).ToList();
                    return View(faqs);
                }
            }
        }

        #endregion

        #region Error 404

        public ActionResult Error()
        {
            return View();
        }
        #endregion

        #region Mail Sending


        public bool SendEmailToPartnerEmailID(tbl_user user)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string loginlink = GlobalMethods.webUrl + "login";


                string mailContent = @"<p>Congratulations!</p> " +
                    "<p>We are excited to welcome you to Juggle joy!.</p>" +
                    "<p>You can now access your account and start exploring our services by logging in here. </p>" +
                   "<p style='margin-top: 20px;'><a href='" + loginlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Login</a></p>";

                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, user.PartnerEmail, "", "", "Registration Successful-Welcome to Juggle Joy", strbody, true);
            }
        }

        public bool SendGiftMailToUser(tbl_user user, tbl_gifts gift)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string loginlink = GlobalMethods.webUrl + "login";

                string username = "";
                if (gift.SenderName != null)
                {
                    username = gift.SenderName;
                }
                else if (user != null)
                {
                    username = user.FirstName;
                }
                string mailContent = "Hi " + username + ",  <br /> " +
                 "<p>You have subscribed JuggleJoy account for the period of " + gift.Duration + " month.</P>" +
                 "<p>To avail this offer please use this coupon code " + gift.GiftCode + " while registering.</p>" + "<br/>" +
                 "<p style='color:#00c1ab !important;font-size:16px !important;font-family: Caveat, cursive !important;'>If you're juggling a million things, JuggleJoy is a game-changer!</p>" +
                 "<p style='color:#00c1ab !important;font-size:18px !important;font-family: Caveat, cursive !important;'>#LifeSimplified #JuggleJoyMagic #StressFree #Parenthack</p>" +
                  "<p style='margin-top: 20px;'><a href='" + loginlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Login</a></p>";

                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, gift.SenderEmail, "", "", "You have received gift from Jugglejoy", strbody, true);
            }
        }



        public bool SendGiftMailToRecepient(tbl_gifts gift)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string sendername = "";
                string mailContent = "";
                string loginlink = GlobalMethods.webUrl + "login";
                if (gift.SenderName != null)
                {
                    sendername = gift.SenderName;
                    mailContent = "Hi " + gift.RecepientName + ",  <br /> " +
               "<p>" + sendername + " sent amazing gift for you. </P>" +
             "<p>I just finished my to-do list effortlessly using JuggleJoy!. </P>" +
             "<p> Their amazing specialists took care of everything, so I could focus on what matters most.</p>" +
             "<p>To avail this offer for the period of " + gift.Duration + " month please use this coupon code " + gift.GiftCode + " while registering.</p>" + "<br/>" +
             "<p>" + gift.Message + "</p>" + "<br/>" +
             "<p style='color:#00c1ab !important;font-size:16px !important;font-family: Caveat, cursive !important;'> If you're juggling a million things, JuggleJoy is a game-changer! </p>" +
             "<p style='color:#00c1ab !important;font-size:18px !important;font-family: Caveat, cursive !important;'>#LifeSimplified  #JuggleJoyMagic  #StressFree  #Parenthack</p>" + "<br/>" +
              "<p style='margin-top: 20px;'><a href='" + loginlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Login</a></p>";
                }
                else
                {
                    sendername = "Jogglejoy";

                    mailContent = "Hi " + gift.RecepientName + ",  <br /> " +
             "<p>" + sendername + "  sent special gift. </P>" +
             "<p> Our amazing specialists take care of everything, so You can just focus on what matters most.</p>" +
             "<p>To avail this offer for the period of " + gift.Duration + " month please use this coupon code " + gift.GiftCode + " while registering.</p>" + "<br/>" +
             "<p>" + gift.Message + "</p>" + "<br/>" +
              "<p style='color:#00c1ab !important;font-size:16px !important;font-family: Caveat, cursive !important;'> If you're juggling a million things, JuggleJoy is a game-changer! </p>" +
             "<p style='color:#00c1ab !important;font-size:18px !important;font-family: Caveat, cursive !important;'>#LifeSimplified  #JuggleJoyMagic  #StressFree  #Parenthack</p>" +
              "<p style='margin-top: 20px;'><a href='" + loginlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>Login</a></p>";
                }

                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, gift.RecepientEmail, "", "", sendername + " sent gift to you", strbody, true);
            }
        }

        private bool SendRegistrationSuccessMail(tbl_user obj)
        {
            using (var db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                string verifyemailurl = GlobalMethods.webUrl + "login";
                //string verifyemailurl = GlobalMethods.webUrl + "RegVerification-succs-message/" + obj.Salt;

                //string verifyemailurl = GlobalMethods.webUrl + "/verifyaccount/" + obj.Salt;

                //string mailContent = @"Dear " + obj.FirstName + " " + obj.LastName + ",<br /><br />" +
                //    "<p>Congratulations! Your registration has been completed successfully.</p> " +
                //    "<p>Please verify your email address to further access your juggle joy account</p>" +
                //    "<p style='margin-top: 20px;'><a href='" + verifyemailurl + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background: #00c1ab;text-decoration: none;'>Verify Email</a></p>";

                string mailContent = @"Dear " + obj.FirstName + " " + obj.LastName + ",<br /><br />" +
                   "<p>Congratulations! Your registration has been completed successfully.</p> " +
                   "<p>We are excited to welcome you to Juggle joy!.</p>" +
                   "<p>You can now access your account and start exploring our services by logging in here. </p>" +
                    "<p style='margin-top: 20px;'><a href='" + verifyemailurl + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background: #00c1ab;text-decoration: none;'>Login</a></p>";
                string strBody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, obj.EmailID, "", "", "Registration Successful-Welcome to Juggle Joy", strBody, true);
            }
        }

        private bool SendResetPasswordMailToUser(tbl_user user)
        {
            using (db_jugglejoyEntities db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var pwdresetlink = GlobalMethods.webUrl + "/reset-password/" + user.PwdResetKey;

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

        public bool SendContactMailToAdmin(tbl_contact con)
        {
            using (var ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var admin_emailid = ctx.tbl_admin.Select(x => x.EmailID).FirstOrDefault();
                string mailContent = @"Dear Admin,<br /><br />" +
                "<p>This email is to inform you that, you have received a new contact.</p>" +
                "<p>Please find the details here.</p> " +
                "<p><b>Name :</b> " + con.ContactName + "<p/>" +
                "<p><b>Email Id :</b> " + con.ContactEmail + "<p/>" +
                "<p><b>Contact Number :</b> " + con.ContactPhone + "<p/>" +
                "<p><b>Subject :</b> " + con.ContactSubject + "<p/>" +
                "<p><b>Message :</b> " + con.ContactMsg + "<p/>";

                string strBody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, admin_emailid, "", "", "Jugglejoy - New contact received", strBody, true);
            }
        }

        #endregion

        #region CMS

        public ActionResult Terms_of_Service()
        {
            return View();
        }

        public ActionResult Privacy_Policy()
        {
            return View();
        }

        public ActionResult Cookie_Policy()
        {
            return View();
        }

        public ActionResult Your_Privacy_Choices()
        {
            return View();
        }

        public ActionResult Disclamer()
        {
            return View();
        }

        public ActionResult Acceptable_Use_Policy()
        {
            return View();
        }
        #endregion


        #region Notify Me

        public JsonResult notify_Me(String id = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (id != "")
                {
                    var emailexists = ctx.tbl_subscriber.Where(x => x.EmailId == id).FirstOrDefault();
                    if (emailexists != null && emailexists.EmailId == id)
                    {
                        return Json(new { error = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        tbl_subscriber obj = new tbl_subscriber();
                        obj.EmailId = id;
                        obj.SubscribedDate = DateTime.Now;
                        ctx.tbl_subscriber.Add(obj);
                        if (ctx.SaveChanges() > 0)
                        {
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);

        }


        #endregion

        #region Stripe implementation
        [HttpPost]
        public ActionResult Create(PaymentIntentCreateRequest request)
        {
            // Set the Stripe API key         
            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["Register"]);
                var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                // Create customer options
                var customerOptions = new CustomerCreateOptions
                {
                    Name = user.FirstName,
                    Address = new AddressOptions
                    {
                        Line1 = "510 Townsend St",
                        PostalCode = "98140",
                        City = "San Francisco",
                        State = "CA",
                        Country = "US",
                    },
                };
                // Create the customer
                var customerService = new CustomerService();
                var customer = customerService.Create(customerOptions);

                // Create a product and price
                var productService = new ProductService();
                var productObj = productService.Get("prod_QJFWVzd2U5d8O6"); // Replace with your product ID
                var priceOptions = new PriceCreateOptions
                {
                    Product = productObj.Id,
                    UnitAmount = Convert.ToInt64(Convert.ToDecimal(request.Items[2].Id) * 100), // Amount in cents
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions { Interval = "month" }, // Specify interval if recurring
                };

                var priceService = new PriceService();
                var price = priceService.Create(priceOptions);

                var paymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription",
                };

                // Create subscription options
                var subscriptionOptions = new SubscriptionCreateOptions
                {

                    Customer = customer.Id,
                    Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = price.Id }, // Use the created price ID
                },
                    PaymentSettings = paymentSettings,
                    PaymentBehavior = "default_incomplete",
                };
                subscriptionOptions.AddExpand("latest_invoice.payment_intent");

                // Create the subscription
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Create(subscriptionOptions);

                // Save subscription ID to your database
                SaveSubscriptionId(subscription.Id); // Implement this method to save to your database

                // Create payment intent options
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = Convert.ToInt32(Convert.ToDecimal(request.Items[2].Id) * 100), // Amount in cents
                    Currency = "usd",
                    Description = "Task Management",
                    Customer = customer.Id,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserID", request.Items[0].Id },
                        { "ActualAmount", request.Items[1].Id },
                        { "PaidPrice", request.Items[2].Id },
                        { "DiscountCode", request.Items[3].Id },

                    },
                    PaymentMethodTypes = new List<string> { "card" },
                };

                // Create the payment intent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

                return Json(new { clientSecret = paymentIntent.ClientSecret });
            }
            // Return the client secret

        }


        private int CalculateOrderAmount(Item[] items)
        {
            // Replace this constant with a calculation of the order's amount
            // Calculate the order total on the server to prevent
            // people from directly manipulating the amount on the client

            int amount = Convert.ToInt32(Convert.ToDecimal(items[0].Id) * 100);

            return Convert.ToInt32(Convert.ToDecimal(items[0].Id) * 100);
        }

        private void SaveSubscriptionId(string subscriptionId)
        {
            using (var ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["Register"]);
                var obj = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                obj.SubscriptionId = subscriptionId;
                obj.SubscriptionStatus = (int)SubscriptionStatus.Active;
                ctx.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
        }

        public class Item
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class PaymentIntentCreateRequest
        {
            [JsonProperty("items")]
            public Item[] Items { get; set; }

        }

        public ActionResult success(string payment_intent, string payment_intent_client_secret, string redirect_status)
        {
            if (payment_intent != null && payment_intent_client_secret != null && redirect_status != null)
            {
                try
                {
                    StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
                    var service = new PaymentIntentService();
                    var res = service.Get(payment_intent);
                    if (res.Status == "succeeded")
                    {
                        int UserID = Convert.ToInt16(res.Metadata["UserID"]);
                        var Subscriptionfees = Convert.ToDecimal(res.Metadata["PaidPrice"]);
                        var ActualAmount = res.Metadata["ActualAmount"];
                        //var CouponCode = res.Metadata["DiscountCode"];
                        var CouponCode = "";
                        using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                        {
                            try
                            {
                                if (CouponCode != null && CouponCode != "")
                                {
                                    var couponcode = ctx.tbl_gifts.Where(x => x.GiftCode == CouponCode).FirstOrDefault();
                                    couponcode.Status = 1;
                                    ctx.Entry(couponcode).State = System.Data.Entity.EntityState.Modified;
                                    if (ctx.SaveChanges() > 0)
                                    {
                                        SendPaymentDoneEmailToUser(UserID);
                                        SendNewPaymentEmailtoAdmin(UserID);
                                        TempData["PaymentMsg"] = "payment successfull";
                                        TempData["RegistrationStatusMsg"] = "Registered successfully";
                                        return RedirectToAction("registration_status_msg");
                                    }
                                    else
                                    {
                                        TempData["PaymentMsg"] = "Oop's something went wrong try again !";
                                    }
                                }
                                else
                                {
                                    var user = ctx.tbl_user.Where(x => x.UserID == UserID).FirstOrDefault();
                                    user.Status = (int)UserStatus.IncompleteRegistration;
                                    ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;


                                    tbl_subscription objsuscription = new tbl_subscription();
                                    objsuscription.UserID = UserID;
                                    objsuscription.SubscriptionFee = Subscriptionfees;
                                    objsuscription.TransactionNo = res.ApplicationId;
                                    objsuscription.TransactionStatus = res.Status;
                                    objsuscription.TransactionMsg = "";

                                    objsuscription.SubscriptionDate = DateTime.Now;
                                    objsuscription.StripeOrderID = res.ApplicationId;
                                    objsuscription.ExpirationDate = DateTime.Now.AddMonths(1);
                                    objsuscription.StripePaymentType = "Stripe";
                                    objsuscription.StripeReturnData = "";
                                    ctx.tbl_subscription.Add(objsuscription);

                                    if (ctx.SaveChanges() > 0)
                                    {
                                        SendPaymentDoneEmailToUser(UserID);
                                        SendNewPaymentEmailtoAdmin(UserID);
                                        TempData["PaymentMsg"] = "payment successfull";
                                        TempData["RegistrationStatusMsg"] = "Registered successfully";
                                        return RedirectToAction("registration_status_msg");
                                    }
                                    else
                                    {
                                        TempData["PaymentMsg"] = "Oop's something went wrong try again !";
                                    }
                                }

                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    else
                    {
                        TempData["PaymentMsg"] = "Payment Status: " + res.Status;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            else
            {
                TempData["PaymentMsg"] = "Oop's something went wrong try again !";
            }
            return RedirectToAction("payment_success");
        }

        public ActionResult CancelSubscription(string subscriptionId, CancelReasonValidationcs obj)
        {
            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
            var subscriptionService = new SubscriptionService();
            var canceledSubscription = subscriptionService.Cancel(obj.subscriptionId);
            if (canceledSubscription.Status == "incomplete_expired")
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var userid = Convert.ToInt32(Session["UserID"]);


                    tbl_CancelAnswer tblanswer = new tbl_CancelAnswer();

                    tblanswer.CancelID = obj.CancelID;
                    tblanswer.SpecificReaspn = obj.OtherReason;
                    tblanswer.UserID = userid;

                    ctx.tbl_CancelAnswer.Add(tblanswer);

                    if (ctx.SaveChanges() > 0)
                    {
                        var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();

                        tbluser.SubscriptionStatus = (int)SubscriptionStatus.Canceled;
                        ctx.Entry(tbluser).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            TempData["cancelmsg"] = "Your current plan is cancelled successfully";
                            return RedirectToAction("Subscription");
                        }
                        else
                        {
                            TempData["cancelmsg"] = "Something went wrong";
                            return RedirectToAction("subscription");
                        }
                    }
                    else
                    {
                        TempData["cancelmsg"] = "Something went wrong";
                        return RedirectToAction("subscription");
                    }

                }
            }
            else
            {
                TempData["cancelmsg"] = " Your current plan can't be cancelled";
                return RedirectToAction("Subscription");
            }


            //return RedirectToAction("UpdateSubscriptionStatus");
        }

        public ActionResult ResumelSubscription(string subscriptionId)
        {
            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;

            var subscriptionService = new SubscriptionService();
            var updateOptions = new SubscriptionUpdateOptions
            {
                PauseCollection = null
            };

            try
            {
                var resumedSubscription = subscriptionService.Update(subscriptionId, updateOptions);

                if (resumedSubscription.Status == "incomplete")
                {
                    using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                    {
                        var userid = Convert.ToInt32(Session["UserID"]);

                        var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();

                        tbluser.SubscriptionStatus = (int)SubscriptionStatus.Resumed;
                        ctx.Entry(tbluser).State = System.Data.Entity.EntityState.Modified;
                        if (ctx.SaveChanges() > 0)
                        {
                            return Json(new { resumed = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(false, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json(new { resumefailed = true }, JsonRequestBehavior.AllowGet);
                }



                //return RedirectToAction("UpdateSubscriptionStatus"); // Redirect to a confirmation page
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Subscription", "ResumeSubscription"));
            }
        }
        public ActionResult PauseSubscription(string subscriptionId)
        {

            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
            var subscriptionService = new SubscriptionService();
            var subscription = subscriptionService.Get(subscriptionId);
            var updateOptions = new SubscriptionUpdateOptions
            {
                PauseCollection = new SubscriptionPauseCollectionOptions
                {
                    Behavior = "mark_uncollectible"
                }
            };
            var pausedSubscription = subscriptionService.Update(subscriptionId, updateOptions);
            if (pausedSubscription.Status == "incomplete")
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var userid = Convert.ToInt32(Session["UserID"]);

                    var tbluser = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();

                    tbluser.SubscriptionStatus = (int)SubscriptionStatus.Paused;
                    ctx.Entry(tbluser).State = System.Data.Entity.EntityState.Modified;
                    if (ctx.SaveChanges() > 0)
                    {
                        TempData["pausemsg"] = "Your current plan is paused successfully.";
                        return Json(new { paused = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json(new { pausefailed = true }, JsonRequestBehavior.AllowGet);
            }


            //return RedirectToAction("UpdateSubscriptionStatus");
        }
        public ActionResult UpdateSubscriptionStatus()
        {
            ViewBag.SubStatus = TempData["SubStatus"] == null ? "" : TempData["SubStatus"] as string;
            return RedirectToAction("UpdateSubscriptionStatus");
        }
        private bool SendPaymentDoneEmailToUser(int UserID)
        {
            using (var db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var subscription = db.tbl_subscription.Where(x => x.UserID == UserID).FirstOrDefault();
                var user = db.tbl_user.Where(x => x.UserID == subscription.UserID).FirstOrDefault();
                string mailContent = "Hi " + user.FirstName + ",<br /><br />" +
                "<p>€" + subscription.SubscriptionFee + " commission has been paid to A24 </p> " +
                "<table  style='margin:30px auto;border-collapse:collapse;width:80%;min-width:300px'>" +
                "<tbody><tr>" +
                "<td style='border-bottom:1px solid #e7e7e7;padding:5px'>" +
                "<strong>Commission Amount </strong>" +
                "</td>" +
                "<td style='border-bottom:1px solid #e7e7e7;padding:5px;color:#008a00'>€" + subscription.SubscriptionFee + "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style = 'border-bottom:1px solid #e7e7e7;padding:5px' >" +
                "<strong> Deal Price</strong>" +
                "</td>" +
                "<td style='border-bottom:1px solid #e7e7e7;padding:5px'>€" + subscription.SubscriptionFee + "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style='border-bottom:1px solid #e7e7e7;padding:5px'>" +
                "<strong>Date</strong>" +
                "</td>" +
                "<td style = 'border-bottom:1px solid #e7e7e7;padding:5px'>" + GlobalMethods.ConvertToDate(subscription.SubscriptionDate) + "</td>" +
                "</tr>" +
                "</tbody></table>" +
                "<p style='margin-top: 20px;'></p>";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, user.EmailID, "", "", "Jugglejoy - Payment Success", strbody, true);
            }
        }
        private bool SendNewPaymentEmailtoAdmin(int userid)
        {
            using (var db = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var admin = db.tbl_admin.FirstOrDefault();
                var subscription = db.tbl_subscription.Where(x => x.UserID == userid).FirstOrDefault();
                string mailContent = "Hi " + admin.FullName + ",<br /><br />" +
                    "<p>You received commission amount of € " + subscription.SubscriptionFee + "</p> " +
                    "<p>Please login to view complete details</p>";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom, admin.EmailID, "", "", "JuggleJoy - Payment Received", strbody, true);
            }
        }
        #endregion

        #region Subscription

        public ActionResult Subscription(string msg = "")
        {
            if (Session["UserID"] != null)
            {
                ViewBag.pausemsg = TempData["pausemsg"] == null ? "" : TempData["pausemsg"] as string;
                ViewBag.cancelmsg = TempData["cancelmsg"] == null ? "" : TempData["cancelmsg"] as string;


                return View();
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }

        public ActionResult Subcancel()
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

        public ActionResult beforcancel()
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    CancelReasonValidationcs obj = new CancelReasonValidationcs();
                    ViewBag.CancelCatID = new SelectList(ctx.tbl_CancelCat.ToList(), "CancelCatID", "CancellationCategory");
                    ViewBag.CancelID = new SelectList(ctx.tbl_CancelReason.ToList(), "CancelID", "Reason");
                    return View(obj);
                }
            }
            else
            {
                TempData["UserLogInStatus"] = "Please login here";
                return RedirectToAction("login", "Home", new { RetUrl = Request.Url.ToString() });
            }
        }


        [HttpPost]
        public JsonResult GetCancelReasonlist(int id)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                List<tbl_CancelReason> state = ctx.tbl_CancelReason.Where(x => x.CancelCatID == id).ToList();
                var xr = new SelectList(state, "CancelID", "Reason", id);
                return Json(xr, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult FindCancelReasontextbox(int CancelID = 0)
        {
            if (Session["UserID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    var tblcancelreason = ctx.tbl_CancelReason.Where(x => x.CancelID == CancelID).FirstOrDefault();
                    if (tblcancelreason != null)
                    {
                        if (tblcancelreason.OptionType == "textbox")
                        {
                            return Json(new { textbox = true }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(false, JsonRequestBehavior.AllowGet);
                        }
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


        public ActionResult cancelSubscription_checkout()
        {
            return View();
        }

        public ActionResult Create_cancelsubscription(PaymentIntentCreateRequest request)
        {
            // Set the Stripe API key         
            StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["UserID"]);
                var user = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                // Create customer options
                var customerOptions = new CustomerCreateOptions
                {
                    Name = user.FirstName,
                    Address = new AddressOptions
                    {
                        Line1 = "510 Townsend St",
                        PostalCode = "98140",
                        City = "San Francisco",
                        State = "CA",
                        Country = "US",
                    },
                };
                // Create the customer
                var customerService = new CustomerService();
                var customer = customerService.Create(customerOptions);

                // Create a product and price
                var productService = new ProductService();
                var productObj = productService.Get("prod_QJFWVzd2U5d8O6"); // Replace with your product ID
                var priceOptions = new PriceCreateOptions
                {
                    Product = productObj.Id,
                    UnitAmount = Convert.ToInt64(Convert.ToDecimal(request.Items[1].Id) * 100), // Amount in cents
                    Currency = "usd",
                    Recurring = new PriceRecurringOptions { Interval = "month" }, // Specify interval if recurring
                };

                var priceService = new PriceService();
                var price = priceService.Create(priceOptions);

                var paymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription",
                };

                // Create subscription options
                var subscriptionOptions = new SubscriptionCreateOptions
                {

                    Customer = customer.Id,
                    Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = price.Id }, // Use the created price ID
                },
                    PaymentSettings = paymentSettings,
                    PaymentBehavior = "default_incomplete",
                };
                subscriptionOptions.AddExpand("latest_invoice.payment_intent");

                // Create the subscription
                var subscriptionService = new SubscriptionService();
                var subscription = subscriptionService.Create(subscriptionOptions);

                // Save subscription ID to your database
                SaveSubscriptionIdResume(subscription.Id); // Implement this method to save to your database

                // Create payment intent options
                var paymentIntentOptions = new PaymentIntentCreateOptions
                {
                    Amount = Convert.ToInt32(Convert.ToDecimal(request.Items[1].Id) * 100), // Amount in cents
                    Currency = "usd",
                    Description = "Task Management",
                    Customer = customer.Id,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserID", request.Items[0].Id },
                        { "PaidPrice", request.Items[1].Id },

                    },
                    PaymentMethodTypes = new List<string> { "card" },
                };

                // Create the payment intent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

                return Json(new { clientSecret = paymentIntent.ClientSecret });
            }
            // Return the client secret

        }

        private void SaveSubscriptionIdResume(string subscriptionId)
        {
            using (var ctx = new db_jugglejoyEntities())
            {
                int userid = Convert.ToInt32(Session["UserID"]);
                var obj = ctx.tbl_user.Where(x => x.UserID == userid).FirstOrDefault();
                obj.SubscriptionId = subscriptionId;
                ctx.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
        }
        public ActionResult success_cancelsubscription(string payment_intent, string payment_intent_client_secret, string redirect_status)
        {
            if (payment_intent != null && payment_intent_client_secret != null && redirect_status != null)
            {
                try
                {
                    StripeConfiguration.ApiKey = GlobalMethods.StripeSecretKey;
                    var service = new PaymentIntentService();
                    var res = service.Get(payment_intent);
                    if (res.Status == "succeeded")
                    {
                        int UserID = Convert.ToInt16(res.Metadata["UserID"]);
                        var Subscriptionfees = Convert.ToDecimal(res.Metadata["PaidPrice"]);

                        //var CouponCode = res.Metadata["DiscountCode"];

                        using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                        {
                            try
                            {

                                var user = ctx.tbl_user.Where(x => x.UserID == UserID).FirstOrDefault();
                                user.SubscriptionStatus = (int)SubscriptionStatus.Active;
                                ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;


                                tbl_subscription objsuscription = new tbl_subscription();
                                objsuscription.UserID = UserID;
                                objsuscription.SubscriptionFee = Subscriptionfees;
                                objsuscription.TransactionNo = res.ApplicationId;
                                objsuscription.TransactionStatus = res.Status;
                                objsuscription.TransactionMsg = "";

                                objsuscription.SubscriptionDate = DateTime.Now;
                                objsuscription.StripeOrderID = res.ApplicationId;
                                objsuscription.ExpirationDate = DateTime.Now.AddMonths(1);
                                objsuscription.StripePaymentType = "Stripe";
                                objsuscription.StripeReturnData = "";
                                ctx.tbl_subscription.Add(objsuscription);

                                if (ctx.SaveChanges() > 0)
                                {
                                    SendPaymentDoneEmailToUser(UserID);
                                    SendNewPaymentEmailtoAdmin(UserID);
                                    TempData["PaymentMsg"] = "payment successfull";
                                    TempData["cancelmsg"] = "Your Juggle joy account is active now";
                                    return RedirectToAction("subscription");
                                }
                                else
                                {
                                    TempData["PaymentMsg"] = "Oop's something went wrong try again !";
                                }


                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                    else
                    {
                        TempData["PaymentMsg"] = "Payment Status: " + res.Status;
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
            else
            {
                TempData["PaymentMsg"] = "Oop's something went wrong try again !";
            }
            return RedirectToAction("payment_success");
        }
        #endregion

    }
}