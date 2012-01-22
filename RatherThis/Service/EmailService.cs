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
            body.AppendLine("");
            body.AppendLine("Thank you again!");
            body.AppendLine("From RatherThis");

            SendEmail("welcome@RatherThis.com", email, "Welcome to RatherThis!", body.ToString());
        }

        public void SendResetPasswordEmail(string email, string resetToken)
        {
            string resetPasswordLink = Constants.BaseUrl + "/Account/ResetPassword?resetToken=" + resetToken;
            StringBuilder body = new StringBuilder();
            body.AppendLine("Please click the following link to reset your password. The reset token will expire in 1 hour.");
            body.AppendLine(resetPasswordLink);
            body.AppendLine("");
            body.AppendLine("From RatherThis");

            SendEmail("noreply@RatherThis.com", email, "Password Reset | RatherThis", body.ToString());
        }

        public void SendContactUsEmail(string senderEmail, string contactType, string comment)
        {
            StringBuilder emailToSender = new StringBuilder();
            emailToSender.AppendLine("Thank you for taking the time to reach out to us! We will get back to you as soon as possible.");
            emailToSender.AppendLine("Please feel free to reach out to us any time!");
            emailToSender.AppendLine("Thank you!");
            emailToSender.AppendLine("From RatherThis");

            SendEmail("noreply@RatherThis.com", senderEmail, "Contact Us | RatherThis", emailToSender.ToString());

            StringBuilder emailToUs = new StringBuilder();
            emailToUs.AppendLine("Contact email: " + senderEmail);
            emailToUs.AppendLine("Contact type: " + contactType);
            emailToUs.AppendLine("Comment: " + comment);

            SendEmail("contact-us@RatherThis.com", "contact-us@RatherThis.com", "Contact Us Email", emailToUs.ToString());
        }

        public void SendErrorEmail(string stackTrace)
        {
            SendEmail("error@RatherThis.com", "ratherthis@gmail.com", "Error Email", stackTrace);
        }

        private void SendEmail(string fromAddress, string toAddress, string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = Config.EmailSettings.UseSsl();
                smtpClient.Host = Config.EmailSettings.ServerName();
                smtpClient.Port = Config.EmailSettings.ServerPort();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(Config.EmailSettings.Username(), Config.EmailSettings.Password());

                if (Config.EmailSettings.WriteAsFile())
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = Config.EmailSettings.FileLocation();
                    smtpClient.EnableSsl = false;
                }

                MailMessage mail = new MailMessage(fromAddress, toAddress, subject, body);

                if (Config.EmailSettings.WriteAsFile())
                {
                    mail.BodyEncoding = Encoding.ASCII;
                }

                smtpClient.Send(mail);
            }
        }
    }
}