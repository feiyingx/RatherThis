using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Domain.Entities
{
    public class QuestionOption
    {
        public int QuestionOptionID { get; set; }
        public int QuestionID { get; set; }
        public string OptionText { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Question Question { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
