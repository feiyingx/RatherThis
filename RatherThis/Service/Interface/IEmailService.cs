using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Service.Interface
{
    public interface IEmailService
    {
        void SendRegistrationCompleteEmail(string email);
        void SendResetPasswordEmail(string email, string resetToken);
        void SendContactUsEmail(string senderEmail, string contactType, string comment);
        void SendErrorEmail(string stackTrace);
    }
}
