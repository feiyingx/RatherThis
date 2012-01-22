using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RatherThis.Domain.Abstract;

namespace RatherThis.Domain.Concrete
{
    public class EFResetPasswordTokenRepository : IResetPasswordTokenRepository
    {
        private EFDbContext context = new EFDbContext();
        public IQueryable<Entities.ResetPasswordToken> Tokens
        {
            get
            {
                return context.ResetPasswordTokens;
            }
        }

        public void AddToken(Entities.ResetPasswordToken token)
        {
            if (token.ResetPasswordTokenID == Guid.Empty)
            {
                token.ResetPasswordTokenID = Guid.NewGuid();
                //password token expires in one hour
                token.DateExpired = DateTime.Now.AddHours(1);  //token duration is 1 hour
                context.ResetPasswordTokens.Add(token);
            }

            context.SaveChanges();
        }
    }
}
