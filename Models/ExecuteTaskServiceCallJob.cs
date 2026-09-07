using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
//to delete file from server
using System.Web.Hosting;
using System.IO;


namespace juggle_joy.Models
{
    public class ExecuteTaskServiceCallJob : IJob
    {
        public static readonly string SchedulingStatus = ConfigurationManager.AppSettings["ExecuteTaskServiceCallSchedulingStatus"];
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                if (SchedulingStatus.Equals("ON"))
                {
                    try
                    {
                        //Do whatever stuff you want
                        using (db_jugglejoyEntities ctx = new db_jugglejoyEntities())
                        {
                            //code to delete file records 
                            var lists = ctx.tbl_task.Where(x => x.Status == (int)TaskStatus.Complete && x.CompletionDate != null).ToList();
                            var list = lists.Where(x => ((DateTime.Now.Date - x.CompletionDate.Value.Date).TotalDays) >= 90).ToList();
                            if (list.Count() > 0)
                            {
                                foreach (var item in list)
                                {
                                    var tblattachment = ctx.tbl_taskattachment.Where(x => x.TaskID == item.TaskID).ToList();
                                    foreach (var items in tblattachment)
                                    {
                                        String filepath = Path.Combine("~/Assets/TaskFiles/", items.FileName);
                                        if (System.IO.File.Exists(HostingEnvironment.MapPath("~/Assets/TaskFiles/" + items.FileName)))
                                        {
                                            System.IO.File.Delete(HostingEnvironment.MapPath("~/Assets/TaskFiles/" + items.FileName));
                                        }
                                        //if (System.IO.File.Exists(Server.MapPath("~/Assets/TaskFiles/" + items.FileName)))
                                        //{
                                        //    System.IO.File.Delete(Server.MapPath("~/Assets/TaskFiles/" + items.FileName));
                                        //}

                                        ctx.tbl_taskattachment.Remove(items);
                                        ctx.SaveChanges();
                                    }
                                }

                            }

                            //code to generate new task
                            var tasklist = ctx.tbl_task.Where(x => x.Status != (int)TaskStatus.Draft && x.CreatedDate != null).ToList();
                            var tbltask = tasklist.Where(x =>
                                            x.Repeating == Convert.ToString((int)RecurringStatus.Weekly) 
                                            || x.Repeating == Convert.ToString((int)RecurringStatus.Fortnightly)
                                            || x.Repeating == Convert.ToString((int)RecurringStatus.Monthly)
                                            ).ToList();


                            if (tbltask.Count() > 0)
                            {
                                foreach (var item in tbltask)
                                {


                                    DateTime deadline = new DateTime();

                                    if (item.Deadline != null)
                                    {
                                        if (item.Deadline.HasValue)
                                        {
                                            deadline = item.Deadline.Value.Date;
                                        }
                                    }
                                    else if (item.CreatedDate != null)
                                    {
                                        if (item.CreatedDate.HasValue)
                                        {
                                            deadline = item.CreatedDate.Value.Date;
                                        }
                                    }

                                    var currentDate = DateTime.Now.Date;

                                    var diff = deadline - currentDate;

                                    var daysDiff = diff.TotalDays;


                                    var DiffInt = (int)daysDiff;
                                    tbl_task newtask = new tbl_task();

                                    if ((DiffInt <= 3 && item.Repeating == Convert.ToString((int)RecurringStatus.Weekly)) || (DiffInt <= 7 && item.Repeating == Convert.ToString((int)RecurringStatus.Fortnightly)) || (DiffInt <= 7 && item.Repeating == Convert.ToString((int)RecurringStatus.Monthly)))
                                    {

                                        if (item.Repeating == Convert.ToString((int)RecurringStatus.Weekly))
                                        {
                                            newtask.Deadline = deadline.AddDays(7);
                                        }
                                        else if (item.Repeating == Convert.ToString((int)RecurringStatus.Monthly))
                                        {
                                            newtask.Deadline = deadline.AddMonths(1);
                                        }
                                        else if (item.Repeating == Convert.ToString((int)RecurringStatus.Fortnightly))
                                        {
                                            newtask.Deadline = deadline.AddDays(14);
                                        }

                                        newtask.FromID = item.FromID;
                                        newtask.CategoryID = item.CategoryID;
                                        newtask.SubCategoryID = item.SubCategoryID;
                                        newtask.CreatedDate = DateTime.Now;
                                        newtask.Repeating = item.Repeating;
                                        newtask.Budget = item.Budget;
                                        newtask.Status = (int)TaskStatus.Received;
                                        newtask.TaskTitle = item.TaskTitle;
                                        newtask.DeadlineType = item.DeadlineType;
                                        newtask.Description = item.Description;
                                        ctx.tbl_task.Add(newtask);

                                        if (ctx.SaveChanges() > 0)
                                        {
                                            tbl_log log = new tbl_log();
                                            log.TaskID = newtask.TaskID;
                                            log.LogStatus = "Task moved to Pending";
                                            log.LogDate = DateTime.Now;
                                            ctx.tbl_log.Add(log);
                                            ctx.SaveChanges();

                                            //answer 
                                            var tblans = ctx.tbl_answer.Where(x => x.TaskID == item.TaskID && x.MinitaskID == null).ToList();
                                            foreach (var anw in tblans)
                                            {
                                                tbl_answer ans = new tbl_answer();
                                                ans.TaskID = newtask.TaskID;
                                                ans.QuestionID = anw.QuestionID;
                                                ans.Answer = anw.Answer;
                                                ans.OtherAnswer = anw.OtherAnswer;
                                                ctx.tbl_answer.Add(ans);
                                                ctx.SaveChanges();
                                            }

                                            item.Repeating = Convert.ToString((int)RecurringStatus.Recurring);
                                            ctx.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                            ctx.SaveChanges();

                                            string strFrom = "";
                                            var user = ctx.tbl_user.Where(x => x.UserID == item.FromID).FirstOrDefault();
                                            string loginlink = GlobalMethods.webUrl + "pending";
                                            string mailContent = "Hi " + user.FirstName + ",  <br /> " +
                                            "<p>As per the schedule of your ticket " + item.TaskTitle + " has been regenerated.</p>" + "<br /> " +
                                            "<p>Please click below link,if you want to stop any upcoming occurrences of this task so that ticket will not automatically regenerate again.</P>" +
                                            "<p style='margin-top: 20px;'><a href='" + loginlink + "' style='font-size: 16px;padding: 8px;cursor: pointer;color: white;background-color: #00c1ab;border-color: #00c1ab;text-decoration: none;'>STOP</a></p>";
                                            string strbody = GlobalMethods.MailBody(mailContent);
                                            SendEmail Mail = new SendEmail();
                                            Mail.SendEmailToRecipients(strFrom,user.EmailID, "", "", "Jugglejoy Task Generated", strbody, true);
                                        }

                                    }
                                }


                            }

                        }




                    }
                    catch (Exception ex)
                    {
                    }
                }
            });
            return task;
        }




    }
}