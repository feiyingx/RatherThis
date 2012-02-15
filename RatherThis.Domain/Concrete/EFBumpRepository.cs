using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Concrete
{
    public class EFBumpRepository : IBumpRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Bump> Bumps
        {
            get { return context.Bumps; }
        }

        public void SaveBump(Bump bump)
        {
            if (bump == null)
                return;

            //if bump id is 0, it means this is a new bump obj
            if (bump.BumpID == 0)
            {
                bump.DateCreated = DateTime.Now;
                context.Bumps.Add(bump);
            }
            else
            {
                bump.DateModified = DateTime.Now;
            }

            context.SaveChanges();
        }
    }
}
