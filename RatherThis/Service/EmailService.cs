using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using RatherThis.Service.Interface;
using RatherThis.Code;
using System.Net.Mail;
using System.Net;

namespace RatherThis.Service
{
    public class EmailService : IEmailService
    {
        public void SendRegistrationCompleteEmail(string email)
        {
            StringBuilder body = new StringBuilder();
            body.AppendLine("Thank you for signing up with RatherThis.com!");
            body.AppendLine("Come out here, have some fun and start asking some questions!");
            body.AppendLine("http://www.ratherthis.com");
            body.AppendLine("");
            body.AppendLine("Thank you again!");
            body.AppendLine("From RatherThis");

            SendEmail(email, "Welcome to RatherThis!", body.ToString());
        }

        public void SendResetPasswordEmail(string email, string resetToken)
        {
            string resetPasswordLink = Constants.BaseUrl + "/Account/ResetPassword?resetToken=" + resetToken;
            StringBuilder body = new StringBuilder();
            body.AppendLine("Please click the following link to reset your password. The reset token will expire in 1 hour.");
            body.AppendLine(resetPasswordLink);
            body.AppendLine("");
            body.AppendLine("From RatherThis");

            SendEmail(email, "Password Reset | RatherThis", body.ToString());
        }

        public void SendContactUsEmail(string senderEmail, string contactType, string comment)
        {
            StringBuilder emailToSender = new StringBuilder();
            emailToSender.AppendLine("Thank you for taking the time to reach out to us!");
            emailToSender.Append("We will get back to you as soon as possible.");
            emailToSender.AppendLine("Please feel free to reach out to us any time!");
            emailToSender.Append("");
            emailToSender.AppendLine("Thank you!");
            emailToSender.AppendLine("From RatherThis");

            SendEmail(senderEmail, "Contact Us | RatherThis", emailToSender.ToString());

            StringBuilder emailToUs = new StringBuilder();
            emailToUs.AppendLine("Contact email: " + senderEmail);
            emailToUs.AppendLine("Contact type: " + contactType);
            emailToUs.AppendLine("Comment: " + comment);

            SendEmail("ratherthis@gmail.com", "Contact Us Email", emailToUs.ToString());
        }

        public void SendErrorEmail(string stackTrace)
        {
            SendEmail("ratherthis@gmail.com", "Error Email", stackTrace);
        }

        private void SendEmail(string toAddress, string subject, string body)
        {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("no-reply@ratherthis.com");
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;

                SmtpClient smtp = new SmtpClient(Config.EmailSettings.ServerName());
                NetworkCredential credentials = new NetworkCredential(Config.EmailSettings.Username(), Config.EmailSettings.Password());
                smtp.Credentials = credentials;

                if (Config.EmailSettings.WriteAsFile())
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtp.PickupDirectoryLocation = Config.EmailSettings.FileLocation();
                    smtp.EnableSsl = false;
                }

                if (Config.EmailSettings.WriteAsFile())
                {
                    mail.BodyEncoding = Encoding.ASCII;
                }
            
                smtp.Send(mail);
            
        }
    }
}