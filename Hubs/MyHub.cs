using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using juggle_joy.Models;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;

namespace juggle_joy.Hubs
{
    public class MyHub : Hub
    {
        db_jugglejoyEntities ctx = new db_jugglejoyEntities();

        [ValidateInput(false)]
        //public void SendMessage(string message, string adid, string toid, string fromid,string assistantid)
        //{
        //    //string name = Context.User.Identity.Name;
        //    //Clients.User(i).send(message);
        //    //Clients.All.AddMessageOnPage(message);
        //    //Clients.All.addNewMessageToPage(i, message);

        //    tbl_chat obj = new tbl_chat();
        //    //tbl_chatfile _file = new tbl_chatfile();
        //    var uid = Convert.ToInt32(toid);
        //    var fid = Convert.ToInt32(fromid);
        //    var pid = Convert.ToInt32(adid);
        //    var tuser = ctx.tbl_user.Where(x => x.UserID == uid).FirstOrDefault();
        //    var fuser = ctx.tbl_user.Where(x => x.UserID == fid).FirstOrDefault();
        //    //var prd = ctx.tbl_vehiclerecords.Where(x => x.vehicleid == pid).FirstOrDefault();
        //    var tchatdate = ctx.tbl_chat.Where(x => x.ToId == uid && x.TaskID == pid).OrderByDescending(x => x.ChatID).Select(x => x.ChatDate).FirstOrDefault();
        //    var fchatdate = ctx.tbl_chat.Where(x => x.FromId == fid).OrderByDescending(x => x.ChatID).Select(x => x.ChatDate).FirstOrDefault();
        //    obj.TaskID = Convert.ToInt32(adid);
        //    obj.ToId = Convert.ToInt32(toid);
        //    obj.FromId = Convert.ToInt32(fromid);
        //    obj.AssistantId = Convert.ToInt32(assistantid);
        //    obj.ChatText = message.Trim().Replace("\n", "<br/>");
        //    obj.ChatreadStatus = 0;
        //    obj.ChatDate = System.DateTime.Now;
        //    ctx.tbl_chat.Add(obj);
        //    ctx.SaveChanges();

        //    var chatfile = ctx.tbl_chatfile.OrderByDescending(x => x.FileId).FirstOrDefault();
        //    if (chatfile != null)
        //    {
        //        if (chatfile.ChatId == null)
        //        {
        //            chatfile.ChatId = obj.ChatID;
        //            ctx.Entry(chatfile).State = System.Data.Entity.EntityState.Modified;
        //            ctx.SaveChanges();
        //        }
        //    }

        //    if (tchatdate != null)
        //    {
        //        var ttimeSpan = DateTime.Now.Subtract((DateTime)tchatdate);
        //        //var ftimeSpan = DateTime.Now.Subtract((DateTime)fchatdate);
        //        //if (ttimeSpan >= TimeSpan.FromHours(2))
        //        //{
        //        //    SendmessagetoSeller(fuser, tuser);
        //        //}
        //        //else if (ttimeSpan >= TimeSpan.FromHours(2))
        //        //{
        //        //    SendMessageToUser(fuser, tuser);
        //        //}

        //    }           
        //    ctx.Database.Connection.Close();
        //    Clients.All.AddMessageOnPage(message);
        //}

        public void SendMessage(string message, int taskid,int fromid)
        {
            var tbltask=ctx.tbl_task.Where(x=>x.TaskID==taskid).FirstOrDefault();
            tbl_chat obj = new tbl_chat();
            
            obj.TaskID = taskid;
            obj.FromId = fromid;
            obj.ChatText = message.Trim().Replace("\n", "<br/>");
            obj.ChatreadStatus = 0;
            obj.ChatDate = System.DateTime.Now;
            ctx.tbl_chat.Add(obj);
            ctx.SaveChanges();

            var chatfile = ctx.tbl_chatfile.OrderByDescending(x => x.FileId).FirstOrDefault();
            if (chatfile != null)
            {
                if (chatfile.ChatId == null)
                {
                    chatfile.ChatId = obj.ChatID;
                    ctx.Entry(chatfile).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }

            if (tbltask.ToID==fromid)
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContext.Clients.All.SendNotification("chat msgs", "assistant", tbltask.AssistantID);

                IHubContext hubContextsUser = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContextsUser.Clients.All.SendNotification("chat msgs", "user", tbltask.FromID);
            }

            if (tbltask.AssistantID == fromid)
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContext.Clients.All.SendNotification("chat msgs", "handler", tbltask.AssistantID);

                IHubContext hubContextsUser = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContextsUser.Clients.All.SendNotification("chat msgs", "user", tbltask.FromID);
            }

            if (tbltask.FromID == fromid)
            {
                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContext.Clients.All.SendNotification("chat msgs", "assistant", tbltask.AssistantID);

                IHubContext hubContexts = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                hubContexts.Clients.All.SendNotification("chat msgs", "handler", tbltask.ToID);
            }

            ctx.Database.Connection.Close();
            //Clients.All.AddMessageOnPage(message);
            Clients.All.NewMassage(message);
            
        }

        private bool SendmessagetoSeller(tbl_user user, tbl_user tuser)
        {
            using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
            {
                string strFrom = "";
                var pwdresetlink = GlobalMethods.webUrl + "/chat/" + user.UserID + "/" + tuser.UserID;
                string strbody = @"<html>" +
                    "<head>" +
                    "</head>" +
                    "<body>" +
                    "<table border='0' cellpadding='0' cellspacing='0' width='100%'>" +
                    "<tr>" +
                    "<td bgcolor='#f5f5f5' style='padding: 4%;'>" +
                    "<div style='background-color: white;padding: 3%;border: 1px solid rgba(128, 128, 128, 0.38);box-shadow: 5px 5px 5px #888888;'>" +
                    "<div style='background-color: #eff3f5;padding: 2%;'>" +

                    "Hi " + tuser.FirstName + " " + tuser.LastName + ",<br />" +
                    "<p>One of your trainee has sent message(s). Please view and reply to the message(s).</p> " +
                    "<p style='margin-top: 20px;'><a href='" + pwdresetlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: #fff;background-color: #f05c26;border-color: #f05c26;text-decoration: none;'>Reply</a></p>" +
                    "<br />Thank you <br /> <br /> " +
                    "Regards<br /> " +
                    "<a href='" + GlobalMethods.webUrl + "'>Flow Trainers</a><br /> " +
                    "</div> " +
                    "</td> " +
                    "</tr> " +
                    "<tr>" +
                    "<td bgcolor='#FFFFFF' class='dotted_line'>&nbsp;</td>" +
                    "</tr> " +
                    "</table> " +
                    "</body> " +
                    "</html>";
                SendEmail Mail = new SendEmail();
                return Mail.SendEmailToRecipients(strFrom,tuser.EmailID, "", "", "Juggle Joy Message", strbody, true);
            }
        }

        //private bool SendMessageToUser(tbl_Trainers user, tbl_Trainers tuser)
        //{
        //    using (db_flowtrainersEntities ctx = new db_flowtrainersEntities())
        //    {
        //        //var name = Regex.Replace(prd.vehiclemake + prd.vehiclemodel, @"[^a-zA-Z0-9]+", "-").Trim('-').ToLower();
        //        var pwdresetlink = GlobalMethods.webUrl + "/chat/" + user.Trainerid + "/" + user.Trainerid;

        //        string strbody = @"<html>" +
        //            "<head>" +
        //            "</head>" +
        //            "<body>" +
        //            "<table border='0' cellpadding='0' cellspacing='0' width='100%'>" +
        //            "<tr>" +
        //            "<td bgcolor='#f5f5f5' style='padding: 4%;'>" +
        //            "<div style='background-color: white;padding: 3%;border: 1px solid rgba(128, 128, 128, 0.38);box-shadow: 5px 5px 5px #888888;'>" +
        //            "<div style='background-color: #eff3f5;padding: 2%;'>" +

        //            "Hi " + tuser.firstname + " " + tuser.lastname + ",<br />" +
        //            "<p> your have new message(s). Please view and reply to the message(s).</p> " +
        //            "<p style='margin-top: 20px;'><a href='" + pwdresetlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: #fff;background-color: #f05c26;border-color: #f05c26;text-decoration: none;'>Reply</a></p>" +
        //            "<br />Thank you <br /> <br /> " +
        //            "Regards<br /> " +
        //            "<a href='" + GlobalMethods.webUrl + "'>Flow Trainers</a><br /> " +
        //            "</div> " +
        //            "</td> " +
        //            "</tr> " +
        //            "<tr>" +
        //            "<td bgcolor='#FFFFFF' class='dotted_line'>&nbsp;</td>" +
        //            "</tr> " +
        //            "</table> " +
        //            "</body> " +
        //            "</html>";
        //        SendEmail Mail = new SendEmail();
        //        return Mail.SendEmailToRecipients(tuser.emailaddress, "", "", "Flow Trainers Trainer Message", strbody, true);
        //    }
        //}
    }
}