﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using System.Data.Entity;

namespace RatherThis.Domain.Concrete
{
    public class EFUserRepository : IUserRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.User> Users
        {
            get { return context.Users; }
        }
        
        /// <summary>
        /// Will save the user object if the object's id is guid.empty, otherwise it will update the user object in the db
        /// </summary>
        /// <param name="user"></param>
        public void SaveUser(Entities.User user)
        {
            if (user == null)
                return;

            if (user.UserID == Guid.Empty)
            {
                user.UserID = Guid.NewGuid();
                user.DateCreated = DateTime.Now;
                context.Users.Add(user);
            }
            else
            {
                user.DateModified = DateTime.Now;
            }
            context.SaveChanges();
        }
    }
}