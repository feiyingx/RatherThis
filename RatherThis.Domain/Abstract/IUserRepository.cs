using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IUserRepository
    {
        IQueryable<User> Users { get; }
        void SaveUser(User user);
        User GetUserWithQuestionsAnswers(Guid userId);
        User GetUserWithQuestionsAnswersByUsername(string username);
    }
}
