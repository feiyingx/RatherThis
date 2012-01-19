using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface ICommentRepository
    {
        IQueryable<Comment> Comments { get; }
        void SaveComment(Comment comment);
    }
}
