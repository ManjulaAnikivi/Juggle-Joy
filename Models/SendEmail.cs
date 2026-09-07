using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Mail;

namespace juggle_joy.Models
{
    public class SendEmail
    {
        #region members

        string strFrom;
        string strCustomerList;
        string strCcList;
        string strBccList;
        string strSubject;
        string strBody;
        bool IsBodyHtml;
        #endregion

        #region methods

        public SendEmail()
        {
            strFrom = string.Empty;
            strCustomerList = string.Empty;
            strCcList = string.Empty;
            strBccList = string.Empty;
            strSubject = string.Empty;
            strBody = string.Empty;
        }




        public bool SendEmailToRecipients(string strFrom,string strTo, string strCcList, string strBccList, string strSubject, string strBody, bool IsBodyHtml)
        {
            bool status = false;
            if (strFrom == "")
            {
                this.strFrom = "notifications@www.jugglejoy.com";
            }
            else
            {
                this.strFrom = strFrom;

            }
            this.strCustomerList = strTo;
            this.strCcList = strCcList;
            this.strBccList = strBccList;
            this.strSubject = strSubject.ToString();
            this.strBody = strBody;
            this.IsBodyHtml = IsBodyHtml;

            try
            {
                SmtpClient smtpSendMail = new SmtpClient("smtp.postmarkapp.com", 2525)
                {
                    Credentials = new System.Net.NetworkCredential("7beaf1be-4588-4d29-b390-d1e38ae5909f", "7beaf1be-4588-4d29-b390-d1e38ae5909f"),
                    EnableSsl = true // Use TLS
                };

                MailMessage objEmail = new MailMessage
                {
                    From = new MailAddress(strFrom, "Jugglejoy"),
                    Subject = strSubject,
                    Body = strBody,
                    IsBodyHtml = IsBodyHtml
                };

                // Add recipients
                objEmail.To.Add(strCustomerList);

                //'if CC list not null then send CC to all recipients in this list
                if (strCcList != string.Empty)
                {
                    objEmail.CC.Add(strCcList);
                }

                if (this.strBccList != string.Empty)
                {
                    objEmail.Bcc.Add(strBccList);
                }
                smtpSendMail.Send(objEmail);
                status = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return status;
        }
        #endregion
    }
}
