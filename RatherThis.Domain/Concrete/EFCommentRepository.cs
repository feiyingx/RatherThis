using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using System.Data.Entity;

namespace RatherThis.Domain.Concrete
{
    public class EFCommentRepository : ICommentRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.Comment> Comments
        {
            get { return context.Comments.Include(c => c.User); }
        }

        public void SaveComment(Entities.Comment comment)
        {
            if (comment == null)
                return;

            if (comment.CommentID == 0)
            {
                comment.DateCreated = DateTime.Now;
                context.Comments.Add(comment);
            }

            context.SaveChanges();
        }
    }
}
