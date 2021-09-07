using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using PFMVC.Controllers;
using DLL.ViewModel;
using System.Net.Mime;
using System.Threading.Tasks;

namespace PFMVC.Models
{
    public static class EmailSender
    {
        public static void SendMails(string recepientEmail, string subject, string body, string path)
        {

            using (MailMessage mailMessage = new MailMessage())
            {

                Attachment attachment = new Attachment(path, MediaTypeNames.Application.Pdf);

                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"]);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.Attachments.Add(attachment);
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(recepientEmail));
                SmtpClient smtp = new SmtpClient();
                smtp.UseDefaultCredentials = true;

                #region FOR_EMAIL_AUTHENTICATION
                smtp.Host = ConfigurationManager.AppSettings["Host"];
                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = ConfigurationManager.AppSettings["UserName"];
                NetworkCred.Password = ConfigurationManager.AppSettings["Password"];
                smtp.Credentials = NetworkCred;
                #endregion

                smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                smtp.Send(mailMessage);                
            }

        }


        public static void SendEmail(string path, string empName, string email)
        {  
                if (email != null)
                {
                    string body = PopulateBody(empName);
                    SendMails(email, "Provident fund report", body, path);
                    Thread.Sleep(2000);
                }
        }

        private static string PopulateBody(string employeeName)
        {
            string body = string.Empty;
            string filePath = HostingEnvironment.MapPath("~/EmailTemplate.html");
            //var path = System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplate.html");
            using (StreamReader reader = new StreamReader(filePath))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{UserName}",employeeName);
            
            return body;
        }
    }


}

