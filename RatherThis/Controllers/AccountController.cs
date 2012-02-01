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
using RatherThis.Code;
using System.Net;
using Newtonsoft.Json.Linq;
using RatherThis.Code;

namespace RatherThis.Controllers
{
    public class AccountController : Controller
    {
        private int _pageSize = 9;
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
                        FormsService.SignIn(model.Email, true);
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
            Session["greeting-msg"] = ""; //reset the greeting msg when you log out
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
                    MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName.Trim(), model.Password.Trim(), model.Email.Trim(), model.Gender.Trim());

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsService.SignIn(model.Email, true /* createPersistentCookie */);
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
                    model.Username = currentUser.Username;
                    //use the same random greeting message within the same session
                    string greetingMessage = (string)Session["greeting-msg"];
                    if (string.IsNullOrEmpty(greetingMessage))
                    {
                        greetingMessage = Utility.GetGreetingMessage(currentUser.Username);
                        Session["greeting-msg"] = greetingMessage;
                    }
                    model.GreetingMessage = greetingMessage;

                    return View("_UserSummary", model);
                }
            }
            return View("_UserSummary");
        }

        [Authorize]
        public ActionResult EditProfile()
        {
            Guid currentUserId = _membershipService.GetCurrentUserId();
            User currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).FirstOrDefault();

            EditProfileModel model = new EditProfileModel();
            model.UserName = currentUser.Username;
            model.Email = currentUser.Email;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditProfile(EditProfileModel model)
        {
            Guid currentUserId = _membershipService.GetCurrentUserId();

            if (string.IsNullOrEmpty(model.Password) || (model.Password.Length >= _membershipService.MinPasswordLength))
            {
                // Attempt to update the user
                MembershipCreateStatus updateStatus = MembershipService.UpdateUser(currentUserId, model.UserName.Trim(), model.Password, model.Email.Trim());

                if (updateStatus == MembershipCreateStatus.Success)
                {
                    ViewBag.Flash = model.UserName;
                    return View("EditProfile", model);
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(updateStatus));
                }
            }
            else
            {
                ModelState.AddModelError("", string.Format("Password must be at least {0} characters.", _membershipService.MinPasswordLength));
            }
            
            return View(model);
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

        public ActionResult UserDetail(string username, string filter = "", int page = 1)
        {
            UserDetailViewModel userDetailViewModel = new UserDetailViewModel();

            User user = _userRepo.GetUserWithQuestionsAnswersByUsername(username);
            if (user != null)
            {
                int numResults;
                IEnumerable<Question> questionResults;
                List<Question> results;

                Constants.Filter currentFilter = Constants.Filter.QUESTION;
                if (filter.ToLower() == Constants.Filter.ANSWER.ToString().ToLower())
                {
                    currentFilter = Constants.Filter.ANSWER;
                }

                if (currentFilter == Constants.Filter.ANSWER)
                {
                    questionResults = user.Answers.Select(a => a.Question);                    
                }
                else
                {
                    questionResults = user.Questions;
                }
                //count total number of results
                numResults = questionResults.Count();
                //get the actual list of results, accounting for page number and page size
                results = questionResults.OrderByDescending(q => q.DateCreated).Skip((page - 1) * _pageSize).Take(_pageSize).ToList();


                Dictionary<object, string> resultModel = new Dictionary<object, string>();
                //by 'CURRENT USER', i mean the user that is viewing this page (which could be different from the 'user' whose page is being displayed. e.g. im viewing a friend's user detail page)
                bool isLoggedIn = _membershipService.IsAuthenticated(); //check to see whether the current user viewing this page is authenticated
                Guid currentUserId = _membershipService.GetCurrentUserId(); //get current user's user id
                User currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).FirstOrDefault();
                foreach (var q in results)
                {
                    //get the question options
                    QuestionOption option1 = q.QuestionOptions.ElementAt(0);
                    QuestionOption option2 = q.QuestionOptions.ElementAt(1);

                    DateTime date = q.DateCreated;
                    String questionGender = q.Gender;
                    String name = q.User.Username;
                    String optionText1 = option1.OptionText;
                    String optionText2 = option2.OptionText;

                    //if user is not logged in, then display the question view since we don't know whether user has answered or not
                    if (!isLoggedIn)
                    {
                        QuestionDisplayViewModel model = new QuestionDisplayViewModel();
                        model.IsLoggedIn = false;
                        model.Date = date;
                        model.Gender = questionGender;
                        model.Name = name;
                        model.UserId = Guid.Empty;

                        model.OptionText1 = optionText1;
                        model.OptionText2 = optionText2;

                        model.OptionId1 = option1.QuestionOptionID;
                        model.OptionId2 = option2.QuestionOptionID;

                        model.QuestionId = q.QuestionID;
                        model.QuestionUserGender = q.User.Gender;
                        model.QuestionUsername = q.User.Username;

                        resultModel.Add(model, "question");
                    }
                    else
                    {
                        //if user is logged in, then check whether user has already answered the question. if so then render answer view, otherwise render the question view                
                        //or if the user doesn't match the gender restriction, then just display the answer format 
                        //(e.g. if current user is male, and this is a 'for ladies' question, since user can't answer it anyways, then just display the answer view

                        //check whether user has already answered this question
                        Answer userAnswer = q.Answers.Where(a => a.UserID == currentUserId).FirstOrDefault();
                        Guid questionUserId = q.UserID;

                        bool isAnswered = (userAnswer == null) ? false : true;
                        bool isGenderMismatch = (!string.IsNullOrEmpty(q.Gender) && q.Gender.ToLower() != currentUser.Gender.ToLower()) ? true : false;
                        if (isAnswered || (!isAnswered && isGenderMismatch))
                        {
                            AnswerDisplayViewModel model = new AnswerDisplayViewModel();
                            model.QuestionId = q.QuestionID;
                            model.Date = date;
                            model.Gender = questionGender;
                            model.Name = name;
                            model.UserId = questionUserId;
                            if (isAnswered)
                                model.AnsweredOptionId = userAnswer.QuestionOptionID;

                            model.OptionText1 = optionText1;
                            model.OptionText2 = optionText2;

                            model.OptionVotes1 = q.Answers.Where(a => a.QuestionOptionID == option1.QuestionOptionID).Count();
                            model.OptionVotes2 = q.Answers.Where(a => a.QuestionOptionID == option2.QuestionOptionID).Count();
                            model.TotalVotes = (model.OptionVotes1 + model.OptionVotes2);

                            model.OptionId1 = option1.QuestionOptionID;
                            model.OptionId2 = option2.QuestionOptionID;
                            model.QuestionUserGender = q.User.Gender;
                            model.QuestionUsername = q.User.Username;
                            /*
                            CommentListViewModel commentModel = new CommentListViewModel();
                            commentModel.OptionId1 = option1.QuestionOptionID;
                            commentModel.OptionId2 = option2.QuestionOptionID;
                            commentModel.Comments = q.Comments.ToList();
                            model.CommentModel = commentModel;
                             * */

                            model.NumComments = q.Comments.Count();
                            resultModel.Add(model, "answer");
                        }
                        else
                        {
                            QuestionDisplayViewModel model = new QuestionDisplayViewModel();
                            model.IsLoggedIn = true;
                            model.Date = date;
                            model.Gender = questionGender;
                            model.Name = name;
                            model.UserId = questionUserId;

                            model.OptionText1 = optionText1;
                            model.OptionText2 = optionText2;

                            model.OptionId1 = option1.QuestionOptionID;
                            model.OptionId2 = option2.QuestionOptionID;

                            model.QuestionId = q.QuestionID;
                            model.QuestionUserGender = q.User.Gender;
                            model.QuestionUsername = q.User.Username;

                            resultModel.Add(model, "question");
                        }
                    }
                }

                userDetailViewModel.CurrentPage = page;
                userDetailViewModel.Filter = currentFilter.ToString();
                userDetailViewModel.TotalPages = (int)Math.Ceiling((double)numResults / _pageSize);
                userDetailViewModel.ResultViewModels = resultModel;
                userDetailViewModel.Username = user.Username;
                userDetailViewModel.NumAsked = user.Questions.Count();
                userDetailViewModel.NumAnswered = user.Answers.Count();
                userDetailViewModel.UserGender = user.Gender;
            }
            return View(userDetailViewModel);
        }

        [HttpGet]
        public ActionResult FacebookLogin(string token)
        {
            WebClient client = new WebClient();
            string jsonResult = client.DownloadString(string.Concat(
                "https://graph.facebook.com/me?access_token=", token));

            JObject jsonUserInfo = JObject.Parse(jsonResult);

            string username = jsonUserInfo.Value<string>("username");
            string firstName = jsonUserInfo.Value<string>("first_name");
            string lastName = jsonUserInfo.Value<string>("last_name");
            if (string.IsNullOrEmpty(username))
            {
                //generate a username using first name and last name reversed
                lastName = lastName.ReverseString();
                username = string.Format("{0}-{1}", firstName, lastName);
            }
            string email = jsonUserInfo.Value<string>("email");
            Int64 facebook_userID = jsonUserInfo.Value<Int64>("id");
            string gender = jsonUserInfo.Value<string>("gender");

            //see if user already exists, if so, authenticate the user, otherwise create an account for the user using their fbID and authenticate them
            if (!string.IsNullOrEmpty(email))
            {
                User siteUser = _userRepo.Users.Where(u => u.Email.Equals(email)).FirstOrDefault();
                if (siteUser != null)
                {
                    FormsService.SignIn(email, true);
                }
                else
                {
                    string genderChar = "";
                    if(gender == "male"){
                        genderChar = "M";
                    }else if(gender == "female"){
                        genderChar = "F";
                    }
                    //use the overrided CreateUser method 
                    MembershipCreateStatus createStatus = MembershipService.CreateUser(username, DateTime.Now.ToString("ddMMMyyyyhhmmss") ,email, genderChar, "facebook", facebook_userID.ToString());

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsService.SignIn(email, true/* createPersistentCookie */);
                    }
                }
            }

            return RedirectToAction("Index", "Question");
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
