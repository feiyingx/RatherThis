using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace RatherThis.Domain.Entities
{
    public class User
    {
        public Guid UserID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string AccountType { get; set; }
        public string ExternalUserID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public virtual ICollection<Question> Questions { get; private set; }
        public virtual ICollection<Answer> Answers { get; private set; }
        public virtual ICollection<Comment> Comments { get; private set; }
        public virtual ICollection<Bump> Bumps { get; private set; }

        public MembershipUser ConvertToMembershipUser()
        {
            return new MembershipUser(
                "CustomMembershipProvider",
                this.Email,
                this.UserID,
                this.Email,
                "",
                "",
                true,
                false,
                this.DateCreated,
                DateTime.Now,
                DateTime.Now,
                DateTime.Now,
                DateTime.Now);
        }
    }
}
