using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using System.Data.Entity;

namespace RatherThis.Domain.Concrete
{
    public class EFAnswerRepository : IAnswerRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.Answer> Answers
        {
            get { return context.Answers.Include(a => a.Question).Include(a => a.QuestionOption).Include(a => a.User); }
        }

        public void SaveAnswer(Entities.Answer answer)
        {
            if (answer == null)
                return;

            if (answer.AnswerID == 0)
            {
                answer.DateCreated = DateTime.Now;
                context.Answers.Add(answer);
            }

            context.SaveChanges();
        }
    }
}
