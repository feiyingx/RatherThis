﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RatherThis.Domain.Entities
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int QuestionID { get; set; }
        public int QuestionOptionID { get; set; }
        public Guid UserID { get; set; }
        public string CommentText { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Question Question { get; set; }
        public virtual User User { get; set; }
        public virtual QuestionOption QuestionOption { get; set; }
    }
}
