using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using System.Data.Entity;

namespace RatherThis.Domain.Concrete
{
    public class EFQuestionRepository : IQuestionRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.Question> Questions
        {
            get { return context.Questions.Include(q => q.User).Include(q => q.QuestionOptions).Include(q => q.Answers); }
        }

        public void SaveQuestion(Entities.Question question)
        {
            if (question == null)
                return;

            if (question.QuestionID == 0)
            {
                question.DateCreated = DateTime.Now;
                context.Questions.Add(question);
            }

            context.SaveChanges();
        }
    }
}
