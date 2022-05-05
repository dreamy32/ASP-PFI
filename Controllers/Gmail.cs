using System;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

namespace Gmail
{
    public static class SMTP
    {
        private static SmtpClient client = null;
        private static MailMessage mail = null;
        public static string OwnerName;
        public static string OwnerEmail;

        static SMTP()
        {
            OwnerName = WebConfigurationManager.AppSettings["SMTPOwnerName"];
            OwnerEmail = WebConfigurationManager.AppSettings["SMTPOwnerEmail"];

            client = new SmtpClient(WebConfigurationManager.AppSettings["SMTPHost"],
                                    int.Parse(WebConfigurationManager.AppSettings["SMTPPort"]))
            {
                Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SMTPOwnerEmail"],
                                                    WebConfigurationManager.AppSettings["SMTPOwnerPassword"]),

                EnableSsl = bool.Parse(WebConfigurationManager.AppSettings["SMTPEnableSSL"]),
                Timeout = int.Parse(WebConfigurationManager.AppSettings["SMTPTimeOut"])
            };

            mail = new MailMessage
            {
                From = new MailAddress(OwnerEmail, OwnerName, System.Text.Encoding.UTF8),
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
        }

        public static void SendEmail(string toName, string toEmail, string subject, string body)
        {
            mail.To.Clear();
            mail.To.Add(new MailAddress(toEmail, toName, System.Text.Encoding.UTF8));
            mail.Subject = subject;
            mail.Body = body;
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                // return to sender
                mail.Subject = "Problème d'envoi de courriel:" + mail.Subject;
                mail.Body = "<h4>problème d'envoi de courriel. Message du serveur :" + 
                            e.Message + "</h4><br/> <h4>Message original envoyé à :" + 
                            mail.To[0].Address + "</h4><hr/>" + mail.Body;
                mail.Attachments.Clear();
                mail.To.Clear();
                mail.To.Add(new MailAddress(mail.From.Address, mail.From.DisplayName, System.Text.Encoding.UTF8));
                client.Send(mail);
            }
        }
    }
}