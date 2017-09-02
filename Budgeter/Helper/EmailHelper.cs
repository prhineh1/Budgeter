using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Budgeter.Helper
{
    public class EmailMessage
    {
        public string CustomSource { get; set; }
        public string SourceName { get; set; }
        public string SourceId { get; set; }

        [DataType(DataType.EmailAddress)]
        public string DestinationEmail { get; set; }

        public string Subject { get; set; }

        [AllowHtml]
        public string Body { get; set; }
    }

    public class EmailHelper
    {
        public async Task SendNotificationEmailAsync(EmailMessage message)
        {
            //Steps for composing the CustomSource 
            message.CustomSource = message.SourceName + WebConfigurationManager.AppSettings["emailsource"];

            //Get data from our private.Config for usage within the SMTP object
            var gmailUsername = WebConfigurationManager.AppSettings["username"];
            var gmailPassword = WebConfigurationManager.AppSettings["password"];
            var gmailHost = WebConfigurationManager.AppSettings["host"];
            var gmailPort = Convert.ToInt32(WebConfigurationManager.AppSettings["port"]);

            using (var smtp = new SmtpClient()
            {
                Host = gmailHost,
                Port = gmailPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(gmailUsername, gmailPassword)
            })

                try
                {
                    await smtp.SendMailAsync(message.CustomSource, message.DestinationEmail, message.Subject, message.Body);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.FromResult(0);
                }
        }
    }
}