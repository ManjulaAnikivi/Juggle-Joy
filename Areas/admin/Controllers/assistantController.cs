using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace juggle_joy.Areas.admin.Controllers
{
    public class assistantController : Controller
    {
        // GET: admin/assistant
        public ActionResult Index()
        {
            return View();
        }


        #region assistant registration

        #region Register
        [HttpGet]
        public ActionResult Register(int id = 0)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.msg = TempData["RegistrationMsg"] == null ? "" : TempData["RegistrationMsg"] as string;
                    if (id > 0)
                    {
                        var user = ctx.tbl_user.Where(x => x.UserID == id).FirstOrDefault();

                        AssistantValidation reg = new AssistantValidation();
                        reg.UserID = user.UserID;
                        reg.FirstName = user.FirstName;
                        reg.LastName = user.LastName;
                        reg.EmailID = user.EmailID;
                        reg.UserName = user.UserName;
                        reg.HandlerID = Convert.ToInt32(user.HandlerID);
                        // reg.PasswordHash=user.PasswordHash;
                        ViewBag.HandlerID = new SelectList(ctx.tbl_user.Where(x => x.UserType == "H" && x.Status == (int)UserStatus.Active).ToList(), "UserID", "FirstName", user.HandlerID);
                        return View(reg);
                    }
                    else
                    {
                        ViewBag.HandlerID = new SelectList(ctx.tbl_user.Where(x => x.UserType == "H" && x.Status == (int)UserStatus.Active).ToList(), "UserID", "FirstName");
                        AssistantValidation reg = new AssistantValidation();

                        return View(reg);
                    }

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
        public ActionResult Register(AssistantValidation obj)
        {

            if (Session["AdminID"] != null)
            {
                ModelState.Clear();
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {

                    if (ModelState.IsValid == true)
                    {
                        if (obj.UserID > 0)
                        {
                            var user = ctx.tbl_user.Where(x => x.UserID == obj.UserID).FirstOrDefault();

                            if (user != null)
                            {
                                user.UserID = obj.UserID;
                                user.FirstName = obj.FirstName;
                                user.LastName = obj.LastName;
                                user.EmailID = obj.EmailID;
                                user.UserName = obj.UserName;
                                user.RegDate = DateTime.Now;
                                user.HandlerID = obj.HandlerID;
                                ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                                if (ctx.SaveChanges() > 0)
                                {
                                    var taskassistant = ctx.tbl_task.Where(x => x.AssistantID == obj.UserID).ToList();
                                    foreach (var item in taskassistant)
                                    {
                                        item.ToID = obj.HandlerID;
                                        ctx.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                        ctx.SaveChanges();
                                    }
                                    TempData["UpdateRegistrationMsg"] = "Record updated successfully";

                                    if (user.Status == 1)
                                    {
                                        RouteData.Values.Remove("id");
                                        return RedirectToAction("active");
                                    }
                                    else
                                    {
                                        RouteData.Values.Remove("id");
                                        return RedirectToAction("blocked");
                                    }
                                }
                                else
                                {
                                    RouteData.Values.Remove("id");
                                    TempData["UpdateRegistrationMsg"] = "Record not updated";
                                    return RedirectToAction("active");

                                }
                            }
                            else
                            {
                                RouteData.Values.Remove("id");
                                TempData["UpdateRegistrationMsg"] = "Record not found";
                                return RedirectToAction("active");
                            }
                        }
                        else
                        {
                            Guid Salt = System.Guid.NewGuid();
                            var encrypted_password = GlobalMethods.HashSHA1(obj.PasswordHash + Salt);


                            var assistant = 'A';
                            tbl_user user = new tbl_user();
                            user.FirstName = obj.FirstName;
                            user.LastName = obj.LastName;
                            user.EmailID = obj.EmailID;
                            user.UserName = obj.UserName;
                            user.UserType = Convert.ToString(assistant);
                            user.Status = (int)UserStatus.Active;
                            user.Salt = Salt;
                            user.PasswordHash = encrypted_password;
                            user.PwdResetKey = Salt;
                            user.RegDate = DateTime.Now;
                            user.HandlerID = obj.HandlerID;

                            ctx.tbl_user.Add(user);
                            if (ctx.SaveChanges() > 0)
                            {
                                TempData["RegistrationMsg"] = "Assistant added successfully";
                            }
                            else
                            {
                                TempData["RegistrationMsg"] = "Unable to add Assistant";
                            }

                            return RedirectToAction("Register");
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

        public JsonResult IsEmailIdExist(int UserID = 0, string EmailID = "")
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



        public JsonResult IsUserNameExixt(int UserID = 0, string UserName = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (UserID == 0)
                {
                    //code for add method
                    if (!ctx.tbl_user.Any(x => x.UserName.ToLower() == UserName.Trim().ToLower() && x.UserType == "A"))
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
                    if (!ctx.tbl_user.Any(x => x.UserName.ToLower() == UserName.Trim().ToLower() && x.UserID != UserID && x.UserType == "A"))
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
        #endregion


        #region Active Assistant
        public ActionResult active()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.UpdateRegistrationMsgActive = TempData["UpdateRegistrationMsg"] == null ? "" : TempData["UpdateRegistrationMsg"] as string;
                    ViewBag.ActiveAssistantListMsg = TempData["ActiveAssistantListMsg"] == null ? "" : TempData["ActiveAssistantListMsg"] as string;
                    ViewBag.AssistantInUse = TempData["AssistantInUse"] == null ? "" : TempData["AssistantInUse"] as string;
                    ViewBag.AssistantNotInUse = TempData["AssistantNotInUse"] == null ? "" : TempData["AssistantNotInUse"] as string;
                    var active_assistants = ctx.tbl_user.Where(x => x.Status == (int)UserStatus.Active && x.UserType == "A").OrderByDescending(x => x.UserID).ToList();
                    return View(active_assistants);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }



        #endregion

        #region Blocked Assistant
        public ActionResult blocked()
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    ViewBag.UpdateRegistrationMsgBlocked = TempData["UpdateRegistrationMsg"] == null ? "" : TempData["UpdateRegistrationMsg"] as string;
                    ViewBag.BlockedAssistantListMsg = TempData["BlockedAssistantListMsg"] == null ? "" : TempData["BlockedAssistantListMsg"] as string;
                    ViewBag.AssistantInUse = TempData["AssistantInUse"] == null ? "" : TempData["AssistantInUse"] as string;
                    ViewBag.AssistantNotInUse = TempData["AssistantNotInUse"] == null ? "" : TempData["AssistantNotInUse"] as string;
                    var bloked_Assistants = ctx.tbl_user.Where(x => x.Status == (int)UserStatus.Blocked && x.UserType == "A").OrderByDescending(x => x.UserID).ToList();
                    return View(bloked_Assistants);
                }
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }

        }
        #endregion

        #region action


        [HttpPost]
        public ActionResult action(IEnumerable<int> IdsToDelete, string action)
        {
            if (Session["AdminID"] != null)
            {
                using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                {
                    if (IdsToDelete != null && IdsToDelete.Count() > 0)
                    {
                        var user = ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).Select(x => new { status = x.Status }).FirstOrDefault();
                        string msg;
                        var assistantinUse = "";
                        var assistantnotUse = "";
                        if (action == "Delete")
                        {

                            var datauser = ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).ToList();
                            if (user.status == (int)UserStatus.Active)
                            {
                                foreach (var item in datauser)
                                {
                                    var assiatant = ctx.tbl_task.Where(x => x.AssistantID == item.UserID).FirstOrDefault();
                                    if (assiatant == null)
                                    {
                                        assistantnotUse = assistantnotUse + item.FirstName + ",";
                                        ctx.tbl_user.Remove(item);
                                        ctx.SaveChanges();
                                    }
                                    else
                                    {
                                        assistantinUse = assistantinUse + item.FirstName + ",";
                                    }
                                }
                                if (assistantinUse != "")
                                {
                                    assistantinUse = assistantinUse.TrimEnd(',');
                                    TempData["AssistantInUse"] = "Assistant (" + assistantinUse + ") is/are assigned tasks and can't be deleted";
                                }
                                if (assistantnotUse != "")
                                {
                                    assistantnotUse = assistantnotUse.TrimEnd(',');
                                    TempData["AssistantNotInUse"] = "Assistant (" + assistantnotUse + ") is/are deleted";
                                }
                                return RedirectToAction("active");
                            }
                            else
                            {
                                foreach (var item in datauser)
                                {
                                    ctx.tbl_user.Remove(item);

                                }

                                if (ctx.SaveChanges() > 0)
                                {
                                    msg = "Records deleted successfully";
                                }
                                else
                                {
                                    msg = "Failed to delete records";
                                }

                                TempData["BlockedAssistantListMsg"] = msg;
                                return RedirectToAction("blocked");
                            }

                        }
                        else if (action == "Un-Block")
                        {
                            ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).ToList().ForEach(x => { x.Status = (int)UserStatus.Active; });



                            if (ctx.SaveChanges() > 0)
                            {
                                //foreach (var item in Ids)
                                //{
                                //    User userdetail =  ctx.Users.Where(x => x.UserID == item).FirstOrDefault();
                                //    SendActivationMailToUser(user);
                                //}
                                TempData["ActiveAssistantListMsg"] = "Assistant account un-blocked successfully";
                                return RedirectToAction("active");
                            }
                            else
                            {
                                TempData["BlockedAssistantListMsg"] = "Failed to un-block assistant account";
                                return RedirectToAction("blocked");
                            }
                        }
                        else if (action == "Block")
                        {
                            //ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).ToList().ForEach(x => { x.Status = (int)UserStatus.Blocked; });

                            //if (ctx.SaveChanges() > 0)
                            //{
                            //    TempData["BlockedAssistantListMsg"] = "Assistant account blocked successfully";
                            //    return RedirectToAction("blocked");
                            //}
                            //else
                            //{
                            //    TempData["ActiveAssistantListMsg"] = "Failed to block assistant account";
                            //    return RedirectToAction("active");
                            //}

                            var datauser = ctx.tbl_user.Where(x => IdsToDelete.Contains(x.UserID)).ToList();
                            foreach (var item in datauser)
                            {
                                var assiatant = ctx.tbl_task.Where(x => x.AssistantID == item.UserID).FirstOrDefault();
                                if (assiatant == null)
                                {
                                    assistantnotUse = assistantnotUse + item.FirstName + ",";
                                    var tbluser = ctx.tbl_user.Where(x => x.UserID == item.UserID).FirstOrDefault();
                                    tbluser.Status = (int)UserStatus.Blocked;
                                    ctx.Entry(tbluser).State = System.Data.Entity.EntityState.Modified;
                                    ctx.SaveChanges();

                                }
                                else
                                {
                                    assistantinUse = assistantinUse + item.FirstName + ",";
                                }
                            }
                            if (assistantnotUse == "")
                            {
                                assistantinUse = assistantinUse.TrimEnd(',');
                                TempData["AssistantInUse"] = "Handlers (" + assistantinUse + ") can't be blocked,assigned with tasks";
                                return RedirectToAction("active");
                            }
                            else
                            {
                                assistantinUse = assistantinUse.TrimEnd(',');
                                assistantnotUse = assistantnotUse.TrimEnd(',');
                                if (assistantinUse != "")
                                {
                                    TempData["AssistantInUse"] = "Assistants (" + assistantinUse + ") can't be blocked,assigned with tasks";
                                }
                                if (assistantnotUse != "")
                                {
                                    TempData["AssistantNotInUse"] = "Assistants (" + assistantnotUse + ") is/are blocked";
                                }

                                return RedirectToAction("blocked");
                            }
                        }
                    }
                }
                return RedirectToAction("active");
            }
            else
            {
                TempData["AdminLogInStatus"] = "Please login here";
                return RedirectToAction("login", "account", new { RetUrl = Request.Url.ToString() });
            }
        }
        #endregion

        #endregion
    }
}