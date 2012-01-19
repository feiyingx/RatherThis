using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IQuestionRepository
    {
        IQueryable<Question> Questions { get; }
        void SaveQuestion(Question question);
    }
}
