using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IBumpRepository
    {
        IQueryable<Bump> Bumps { get; }
        void SaveBump(Bump bump);
    }
}
