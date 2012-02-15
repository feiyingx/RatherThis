using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Domain.Entities
{
    public class Question
    {
        public int QuestionID { get; set; }
        public Guid UserID { get; set; }
        public DateTime DateCreated { get; set; }
        public string Gender { get; set; }
        public int? Category { get; set; }
        public int BumpUpValue { get; set; } //purpose of a field for bump up and bump down is so we can quickly get the bump count without makign another query when we have the question
        public int BumpDownValue { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<QuestionOption> QuestionOptions { get; private set; }
        public virtual ICollection<Answer> Answers { get; private set; }
        public virtual ICollection<Comment> Comments { get; private set; }
        public virtual ICollection<Bump> Bumps { get; private set; }
    }
}
