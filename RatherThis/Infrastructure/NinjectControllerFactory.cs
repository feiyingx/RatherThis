﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Concrete;
using System.Web.Security;
using RatherThis.Models;
using RatherThis.Service.Interface;
using RatherThis.Service;

namespace RatherThis.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel ninjectKernel;

        public NinjectControllerFactory()
        {
            ninjectKernel = new StandardKernel();
            AddBindings();
        }

        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            return controllerType == null ? null : (IController)ninjectKernel.Get(controllerType);
        }

        private void AddBindings()
        {
            //additional bindings
            ninjectKernel.Bind<IUserRepository>().To<EFUserRepository>();
            ninjectKernel.Bind<IAnswerRepository>().To<EFAnswerRepository>();
            ninjectKernel.Bind<ICommentRepository>().To<EFCommentRepository>();
            ninjectKernel.Bind<IQuestionOptionRepository>().To<EFQuestionOptionRepository>();
            ninjectKernel.Bind<IQuestionRepository>().To<EFQuestionRepository>();
            ninjectKernel.Bind<IResetPasswordTokenRepository>().To<EFResetPasswordTokenRepository>();
            ninjectKernel.Bind<IEmailService>().To<EmailService>();
            ninjectKernel.Bind<IMembershipService>().To<AccountMembershipService>();
            ninjectKernel.Bind<IBumpRepository>().To<EFBumpRepository>();

            //inject membership
            ninjectKernel.Inject(Membership.Provider);
        }
    }
}