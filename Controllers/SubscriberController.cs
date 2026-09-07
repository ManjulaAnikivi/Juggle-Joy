using juggle_joy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace juggle_joy.Controllers
{
    public class SubscriberController : Controller
    {
        // GET: Subscriber
        public JsonResult subscriber(String id = "")
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                if (id != "")
                {
                    var emailexists = ctx.tbl_subscriber.Where(x => x.EmailId == id).FirstOrDefault();
                    if( emailexists != null && emailexists.EmailId == id)
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
                            SendSubscribeMailToUser(obj);
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

        private bool SendSubscribeMailToUser(tbl_subscriber obj)
        {
            using (db_jugglejoyEntities db = new db_jugglejoyEntities())
            {
                string strFrom = " blog@marketing.jugglejoy.com";
                string mailContent = "Hi,<br /> " +
                    "<p>This is to inform you that you will receive further information at the email address: "+obj.EmailId+". </p> " +
                    "<p>Please ensure that you check this email address regularly for any updates and important information.</p> ";
                string strbody = GlobalMethods.MailBody(mailContent);
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom,obj.EmailId, "", "", "Juggle joy notification", strbody, true);
            }
        }

    }
}
