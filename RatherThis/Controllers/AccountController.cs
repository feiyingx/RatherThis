using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RatherThis.Models;
using RatherThis.Domain.Abstract;
using domain = RatherThis.Domain.Entities;
using RatherThis.Domain.Entities;
using Ninject;
using RatherThis.Service.Interface;

namespace RatherThis.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        private IMembershipService _membershipService;
        private IQuestionRepository _questionRepo;
        private IQuestionOptionRepository _optionRepo;
        private IAnswerRepository _answerRepo;
        private IUserRepository _userRepo;
        private ICommentRepository _commentRepo;
        private IResetPasswordTokenRepository _resetTokenRepo;
        [Inject]
        public IEmailService EmailService { get; set; }

        public AccountController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo, IUserRepository userRepo, ICommentRepository commentRepo, IResetPasswordTokenRepository resetTokenRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
            _commentRepo = commentRepo;
            _resetTokenRepo = resetTokenRepo;
        }

        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult LogOn()
        {
            return View("_LoginForm");
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            //first check to see if this post request is meant for this action, if not, then don't do anything
            if (Request.Url.ToString().ToLower().Contains(Url.Action("LogOn", "Account").ToLower()))
            {
                if (ModelState.IsValid)
                {
                    if (MembershipService.ValidateUser(model.Email, model.Password))
                    {
                        FormsService.SignIn(model.Email, model.RememberMe);
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return JavaScript("window.location = \"" + returnUrl + "\";");
                        }
                        else
                        {
                            return JavaScript("window.location.reload();");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }
            else
            {
                //clear the model state errors for this request since this httppost was meant for another action
                //e.g. when we post back on 'forgot password', it will also hit this action because the form is also
                //on the page, but we know the request is not meant for this action so just clear the errors manually
                //so it doesn't get rendered onto the page
                ModelState.Clear();
            }

            // If we got this far, something failed, redisplay form
            return View("_LoginForm", model);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult LogOff()
        {
            FormsService.SignOut();

            return RedirectToAction("Index", "Question");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View("_RegisterForm");
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            //first check to see if this post request is meant for this action, if not, then don't do anything
            if (Request.Url.ToString().ToLower().Contains(Url.Action("Register", "Account").ToLower())){
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email, model.Gender);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsService.SignIn(model.Email, false /* createPersistentCookie */);
                        return JavaScript("window.location.reload();");
                    }
                    else
                    {
                        ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                    }
                }
            }
            else
            {
                //clear the model state errors for this request since this httppost was meant for another action
                //e.g. when we post back on 'forgot password', it will also hit this action because the form is also
                //on the page, but we know the request is not meant for this action so just clear the errors manually
                //so it doesn't get rendered onto the page
                ModelState.Clear();
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View("_RegisterForm", model);
        }

        public ActionResult UserSummary()
        {
            if (_membershipService.IsAuthenticated())
            {
                UserSummaryViewModel model = new UserSummaryViewModel();
                Guid userId = _membershipService.GetCurrentUserId();
                domain.User currentUser = _userRepo.GetUserWithQuestionsAnswers(userId);
                if (currentUser != null)
                {
                    model.Name = currentUser.Username;
                    model.NumAnswers = currentUser.Answers.Count();
                    model.NumQuestions = currentUser.Questions.Count();

                    return View("_UserSummary", model);
                }
            }
            return View("_UserSummary");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                domain.User userMatch = _userRepo.Users.Where(u => u.Email.ToLower().Equals(model.Email.ToLower())).FirstOrDefault();
                if (userMatch == null)
                {
                    ModelState.AddModelError("", "Email was not found. Please check the email address.");
                    return View(model);
                }

                //create reset password token
                //then send reset password link to email
                ResetPasswordToken newToken = new ResetPasswordToken();
                newToken.UserID = userMatch.UserID;
                newToken.IsUsed = false;
                _resetTokenRepo.AddToken(newToken);

                EmailService.SendResetPasswordEmail(model.Email, newToken.ResetPasswordTokenID.ToString());

                return View("ForgotPasswordThankYou");
            }
            return View(model);
        }

        public ActionResult ForgotPasswordThankYou()
        {
            return View();
        }

        public ActionResult ResetPassword(string resetToken = "")
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();

            //first make sure we ahve a valid token before proceeding
            Boolean badToken = false;
            if (string.IsNullOrEmpty(resetToken))
            {
                badToken = true;
            }

            Guid token = Guid.Empty;
            if (!Guid.TryParse(resetToken, out token))
            {
                badToken = true;
            }
            else if (token == Guid.Empty)
            {
                badToken = true;
            }

            if (badToken)
            {
                model.IsTokenExpired = true;
                return View(model);
            }

            //if we are here, then the token has passed the first test
            //check to see if the token is valid and not expired
            ResetPasswordToken tokenMatch = _resetTokenRepo.Tokens.Where(t => t.ResetPasswordTokenID == token).FirstOrDefault();

            if (tokenMatch == null || tokenMatch.DateExpired < DateTime.Now || tokenMatch.IsUsed)
            {
                model.IsTokenExpired = true;
            }
            else
            {
                //here means the token is good to go
                model.IsTokenExpired = false;
                User userMatch = _userRepo.Users.Where(u => u.UserID == tokenMatch.UserID).FirstOrDefault();
                if (userMatch != null)
                {
                    model.Email = userMatch.Email;
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model, string resetToken = "")
        {
            if (ModelState.IsValid)
            {
                //first make sure we ahve a valid token before proceeding
                Boolean badToken = false;
                if (string.IsNullOrEmpty(resetToken))
                {
                    badToken = true;
                }

                Guid token = Guid.Empty;
                if (!Guid.TryParse(resetToken, out token))
                {
                    badToken = true;
                }
                else if (token == Guid.Empty)
                {
                    badToken = true;
                }

                if (badToken)
                {
                    model.IsTokenExpired = true;
                    return View(model);
                }

                //if we are here, then the token has passed the first test
                //check to see if the token is valid and not expired
                ResetPasswordToken tokenMatch = _resetTokenRepo.Tokens.Where(t => t.ResetPasswordTokenID == token).FirstOrDefault();

                if (tokenMatch == null || tokenMatch.IsUsed)
                {
                    model.IsTokenExpired = true;
                }
                else
                {
                    //here means the token is good to go

                    User userMatch = _userRepo.Users.Where(u => u.UserID == tokenMatch.UserID).FirstOrDefault();
                    if (userMatch != null)
                    {
                        userMatch.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                        _userRepo.SaveUser(userMatch);

                        tokenMatch.IsUsed = true;
                        _resetTokenRepo.AddToken(tokenMatch);

                        return RedirectToAction("ResetPasswordThankYou");
                    }
                    //if we end up here, that means we ran into some issues
                    model.IsTokenExpired = true;
                }
            }

            return View(model);
        }

        public ActionResult ResetPasswordThankYou()
        {
            return View();
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
    }
}
