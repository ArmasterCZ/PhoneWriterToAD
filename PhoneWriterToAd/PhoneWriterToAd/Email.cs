using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using System.Reflection;

namespace telefonyDoAD
{
    public class Email
    {
        /// <summary>
        /// Class for sending Email.
        /// Data needed for it take from App.onfig
        /// </summary>
        /// <param name="userData">Text in email body</param>


        public Email(string userData)
        {
            loadMailData();
            createMailBody(userData);
        }

        protected string smtpHost = null;
        protected Nullable<int> smtpPort = null;
        protected string emailFrom = null;
        protected string emailFromPassword = null;
        protected string emailFromMasked = null;
        protected string emailTo = null;
        protected string emailHead = null;
        protected string emailBody = null;

        protected virtual void loadMailData()
        {
            //load data from App.config
            //need reference system.configuration

            try
            {
                smtpHost = ConfigurationManager.AppSettings["SmtpServerHost"];
                smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpServerPort"]);
                emailFrom = ConfigurationManager.AppSettings["SmtpServerUserName"];
                emailFromPassword = ConfigurationManager.AppSettings["SmtpServerPassword"];
                emailFromMasked = ConfigurationManager.AppSettings["emailFrom"];
                emailHead = ConfigurationManager.AppSettings["emailHead"];
                emailTo = ConfigurationManager.AppSettings["emailTo"];
                emailBody = ConfigurationManager.AppSettings["emailBody"];
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (smtpHost != null & smtpPort != null & emailFrom != null & emailFromPassword != null & emailFromMasked != null & emailHead != null & emailTo != null & emailBody != null)
            {
                Console.WriteLine("Email data loaded.");
            }
            else
            {
                Exception ex = new Exception("Canot load server information for Email send. Check App.config file.");
                throw ex;
            }
        }

        protected virtual void createMailBody(string userData)
        {
            emailBody += $"{Environment.NewLine}Date   : {DateTime.Now}";
            emailBody += $"{Environment.NewLine}PC name: {Environment.MachineName}";
            emailBody += $"{Environment.NewLine}Program: {typeof(Program).Namespace}";
            emailBody += $"{Environment.NewLine}Path   : {System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}";
            emailBody += $"{Environment.NewLine}User   : {Environment.UserDomainName + "\\" + Environment.UserName}";
            emailBody += $"{Environment.NewLine}Data   : {userData}";
        }

        public void send()
        {
            // Command line argument must the the SMTP host.
            SmtpClient client = new SmtpClient();
            client.Port = smtpPort.Value;
            client.Host = smtpHost;
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(emailFrom, emailFromPassword);

            MailMessage mm = new MailMessage(emailFromMasked, emailTo, emailHead, emailBody);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
            Console.WriteLine("Email odeslán.");
        }
    }


    public class SendEmailError : Email
    {
        public SendEmailError(string userDataError, string userDataReport) : base(userDataError)
        {
            addReportData(userDataReport);

            if (!emailTo.Equals(""))
            {
                send();
            }
        }

        protected void addReportData (string userDataReport)
        {
            if (!userDataReport.Equals(""))
            {
                emailBody += $"{Environment.NewLine}Report Log: {userDataReport}";
            }
        }

        protected override void loadMailData()
        {

            //load data from App.config
            //need reference system.configuration

            try
            {
                base.smtpHost = ConfigurationManager.AppSettings["SmtpServerHost"];
                smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpServerPort"]);
                emailFrom = ConfigurationManager.AppSettings["SmtpServerUserName"];
                emailFromPassword = ConfigurationManager.AppSettings["SmtpServerPassword"];
                emailFromMasked = ConfigurationManager.AppSettings["emailFrom"];
                emailHead = ConfigurationManager.AppSettings["emailHead"];
                emailBody = ConfigurationManager.AppSettings["emailBodyError"];
                emailTo = ConfigurationManager.AppSettings["emailToError"];

            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (smtpHost != null & smtpPort != null & emailFrom != null & emailFromPassword != null & emailFromMasked != null & emailHead != null & emailTo != null & emailBody != null)
            {
                Console.WriteLine("Načteny data pro email.");
            }
            else
            {
                Exception ex = new Exception("Canot load server information for Email send. Check App.config file.");
                throw ex;
            }
        }

        protected override void createMailBody(string userDataError)
        {
            emailHead = "Error - " + emailHead;
            emailBody += $"{Environment.NewLine}Date     : {DateTime.Now}";
            emailBody += $"{Environment.NewLine}PC name  : {Environment.MachineName}";
            emailBody += $"{Environment.NewLine}Program  : {typeof(Program).Namespace}";
            emailBody += $"{Environment.NewLine}Path     : {System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}";
            emailBody += $"{Environment.NewLine}User     : {Environment.UserDomainName + "\\" + Environment.UserName}";
            emailBody += $"{Environment.NewLine}Error Log: {userDataError}";
        }
    }


    public class SendEmailReport : Email
    {
        public SendEmailReport(string userData) : base(userData)
        {
            if (!emailTo.Equals(""))
            {
                send();
            }
        }

        protected override void loadMailData()
        {

            //load data from App.config
            //need reference system.configuration

            try
            {
                base.smtpHost = ConfigurationManager.AppSettings["SmtpServerHost"];
                smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpServerPort"]);
                emailFrom = ConfigurationManager.AppSettings["SmtpServerUserName"];
                emailFromPassword = ConfigurationManager.AppSettings["SmtpServerPassword"];
                emailFromMasked = ConfigurationManager.AppSettings["emailFrom"];
                emailHead = ConfigurationManager.AppSettings["emailHead"];
                emailBody = ConfigurationManager.AppSettings["emailBodyReport"];
                emailTo = ConfigurationManager.AppSettings["emailToReport"];

            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (smtpHost != null & smtpPort != null & emailFrom != null & emailFromPassword != null & emailFromMasked != null & emailHead != null & emailTo != null & emailBody != null)
            {
                Console.WriteLine("Načteny data pro email.");
            }
            else
            {
                Exception ex = new Exception("Canot load server information for Email send. Check App.config file.");
                throw ex;
            }
        }

        protected override void createMailBody(string userData)
        {
            emailHead = "Report - " + emailHead;
            emailBody += $"{Environment.NewLine}Date   : {DateTime.Now}";
            emailBody += $"{Environment.NewLine}PC name: {Environment.MachineName}";
            emailBody += $"{Environment.NewLine}Program: {typeof(Program).Namespace}";
            emailBody += $"{Environment.NewLine}Path   : {System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}";
            emailBody += $"{Environment.NewLine}User   : {Environment.UserDomainName + "\\" + Environment.UserName}";
            emailBody += $"{Environment.NewLine}Report : {userData}";
        }

    }
}
