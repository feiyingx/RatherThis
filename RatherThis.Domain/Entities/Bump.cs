using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Domain.Entities
{
    public class Bump
    {
        public int BumpID { get; set; }
        public int QuestionID { get; set; }
        public Guid UserID { get; set; }
        public int BumpUpValue { get; set; }
        public int BumpDownValue { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }

        public virtual Question Question { get; set; }
        public virtual User User { get; set; }
    }
}
