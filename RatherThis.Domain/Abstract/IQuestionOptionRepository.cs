using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IQuestionOptionRepository
    {
        IQueryable<QuestionOption> QuestionOptions { get; }
        void SaveQuestionOption(QuestionOption option);
    }
}
