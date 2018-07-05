using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace SKtimeManagement
{
    public class EmailService
    {
        private const string _mailserver = "";
        public static void Send(string to, string from, string subject, string body, string[] cc = null, string[] bcc = null)
        {
            try
            {
                MailMessage message = new MailMessage(from, to, subject, body);
                message.IsBodyHtml = true;
                if (bcc != null && bcc.Count() > 0)
                {
                    Array.ForEach(bcc, m => message.Bcc.Add(new MailAddress(m)));
                }
                if (cc != null && cc.Count() > 0)
                {
                    Array.ForEach(cc, m => message.CC.Add(new MailAddress(m)));
                }
                SmtpClient client = new SmtpClient(_mailserver);
                client.Send(message);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public static void SendErrorEmail2Admin(string errorAction, string errorMsg)
        {
            Send(SiteConfiguration.AdminEmail, "error@tpf.com.au", "E-commerce Tracking Error: " + errorAction, errorMsg);
        }

        public static async Task SendAsync(string to, string from, string subject, string body, string[] cc = null, string[] bcc = null)
        {
            try
            {
                MailMessage message = new MailMessage(from, to, subject, body);
                message.IsBodyHtml = true;
                if (bcc != null && bcc.Count() > 0)
                {
                    Array.ForEach(bcc, m => message.Bcc.Add(new MailAddress(m)));
                }
                if (cc != null && cc.Count() > 0)
                {
                    Array.ForEach(cc, m => message.CC.Add(new MailAddress(m)));
                }
                SmtpClient client = new SmtpClient(_mailserver);
                await client.SendMailAsync(message);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public static string RenderException(Exception e)
        {
            var result = e.Message;
            var messages = new Dictionary<string, string>();
            messages.Add("Exception", e.Message);
            if (!String.IsNullOrEmpty(e.Source))
                messages.Add("Exception source", e.Source);
            if (!String.IsNullOrEmpty(e.StackTrace))
                messages.Add("Exception stack trace", e.StackTrace);
            if (e.TargetSite != null && !String.IsNullOrEmpty(e.TargetSite.Name))
                messages.Add("Exception target site name", e.TargetSite.Name);
            if (e.InnerException != null)
            {
                if (!String.IsNullOrEmpty(e.InnerException.Message))
                    messages.Add("Inner exception", e.InnerException.Message);
                if (!String.IsNullOrEmpty(e.InnerException.Source))
                    messages.Add("Inner exception source", e.Source);
                if (!String.IsNullOrEmpty(e.InnerException.StackTrace))
                    messages.Add("Inner exception stack trace", e.StackTrace);
                if (e.InnerException.TargetSite != null && !String.IsNullOrEmpty(e.InnerException.TargetSite.Name))
                    messages.Add("Inner exception target site name", e.TargetSite.Name);
            }
            return String.Join("<br/>", messages.Select(m => String.Format("{0}: {1}", m.Key, m.Value)));
        }
    }
}