using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using System.Data.Entity;

namespace RatherThis.Domain.Concrete
{
    public class EFQuestionOptionRepository : IQuestionOptionRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.QuestionOption> QuestionOptions
        {
            get { return context.QuestionOptions.Include(q => q.Question).Include(q => q.Answers); }
        }

        public void SaveQuestionOption(Entities.QuestionOption option)
        {
            if (option == null)
                return;

            if (option.QuestionOptionID == 0)
            {
                option.DateCreated = DateTime.Now;
                context.QuestionOptions.Add(option);
            }

            context.SaveChanges();
        }
    }
}
