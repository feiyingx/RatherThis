using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Domain.Entities
{
    public class ResetPasswordToken
    {
        public Guid ResetPasswordTokenID { get; set; }
        public Guid UserID { get; set; }
        public DateTime DateExpired { get; set; }
        public Boolean IsUsed { get; set; }
    }
}
