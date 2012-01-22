using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Entities;

namespace RatherThis.Domain.Abstract
{
    public interface IResetPasswordTokenRepository
    {
        IQueryable<ResetPasswordToken> Tokens { get; }
        void AddToken(ResetPasswordToken token);
    }
}
