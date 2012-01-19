using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IAnswerRepository
    {
        IQueryable<Answer> Answers { get; }
        void SaveAnswer(Answer answer);
    }
}
