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



        //public bool SendEmailToRecipients(string strTo, string strCcList, string strBccList, string strSubject, string strBody, bool IsBodyHtml)
        //{
        //    bool status = false;
        //    this.strFrom = "info@a24.com";
        //    this.strCustomerList = strTo;
        //    this.strCcList = strCcList;
        //    this.strBccList = strBccList;
        //    this.strSubject = strSubject.ToString();
        //    this.strBody = strBody;
        //    this.IsBodyHtml = IsBodyHtml;

        //    SmtpClient smtpSendMail = new SmtpClient();

        //    MailMessage objEmail = new MailMessage();

        //    objEmail.From = new MailAddress(strFrom, "Jugglejoy");
        //    objEmail.Subject =  strSubject.ToString();
        //    //objEmail.Subject = strSubject.ToString();
        //    objEmail.Body = strBody;
        //    objEmail.IsBodyHtml = IsBodyHtml;
        //    objEmail.To.Add(strCustomerList);
        //    //objEmail.Priority = MailPriority.High;

        //    //'if CC list not null then send CC to all recipients in this list
        //    if (strCcList != string.Empty)
        //    {
        //        objEmail.CC.Add(strCcList);
        //    }

        //    if (this.strBccList != string.Empty)
        //    {
        //        objEmail.Bcc.Add(strBccList);
        //    }

        //    //smtpSendMail.Host = "smtp-relay.sendinblue.com";
        //    //smtpSendMail.Port = 587;
        //    //smtpSendMail.Credentials = new System.Net.NetworkCredential("contact@aalpha.net", "xsmtpsib-3d9c8d363a171f7d39c7f76f7025199edc950ead0caaaffe83c3f55a2c1e4135-3mHSNKYBGkMqsc56");

        //    //smtpSendMail.Send(objEmail);


        //    smtpSendMail.Host = "smtp-relay.sendinblue.com";
        //    smtpSendMail.Port = 587;
        //    //smtpSendMail.Credentials = new system.net.networkcredential("webadmin@comparateur-assurance-dommage-ouvrage.fr", "Aalpha@100");
        //    smtpSendMail.Credentials = new System.Net.NetworkCredential("dandinsiddu@gmail.com", "ByxWhCacjbPU1MLH");
        //    smtpSendMail.Send(objEmail);

        //    status = true;
        //    return status;
        //}


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
