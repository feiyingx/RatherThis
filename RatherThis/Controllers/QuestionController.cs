using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RatherThis.Domain.Abstract;
using RatherThis.Models;
using RatherThis.Domain.Entities;
using RatherThis.Code;

namespace RatherThis.Controllers
{
    public class QuestionController : Controller
    {
        private IMembershipService _membershipService;
        private IQuestionRepository _questionRepo;
        private IQuestionOptionRepository _optionRepo;
        private IAnswerRepository _answerRepo;
        private IUserRepository _userRepo;
        private ICommentRepository _commentRepo;

        private int _pageSize = 9;

        public QuestionController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo, IUserRepository userRepo, ICommentRepository commentRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
            _commentRepo = commentRepo;
        }

        public ActionResult New()
        {
            return View("_NewQuestion");
        }

        [HttpPost]
        public ActionResult New(NewQuestionViewModel model)
        {
            //first check to see if this post request is meant for this action, if not, then don't do anything
            if (Request.Url.ToString().ToLower().Contains(Url.Action("New", "Question").ToLower()))
            {
                if (ModelState.IsValid)
                {
                    Question newQuestion = new Question();
                    newQuestion.Gender = model.Gender;
                    newQuestion.UserID = _membershipService.GetCurrentUserId();
                    _questionRepo.SaveQuestion(newQuestion);

                    QuestionOption option1 = new QuestionOption();
                    QuestionOption option2 = new QuestionOption();

                    option1.QuestionID = option2.QuestionID = newQuestion.QuestionID;
                    option1.OptionText = model.Option1.Trim('?'); //remove ? from the option if user added it in, we dont need it cuz our view will always automatically generate a '?' at the end
                    option2.OptionText = model.Option2.Trim('?');
                    _optionRepo.SaveQuestionOption(option1);
                    _optionRepo.SaveQuestionOption(option2);

                    return JavaScript("window.location.href = 'http://" + Request.Url.Authority + "';");
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

            return View("_NewQuestion", model);
        }

        public ActionResult Index(string sort, string gender, int page = 1)
        {
            List<Question> results;

            //default the sort to latest
            Constants.QuestionSort currentSort = Constants.QuestionSort.LATEST;
            string pageTitle = "";

            if (!string.IsNullOrEmpty(sort))
            {
                if (sort.ToLower() == Constants.QuestionSort.TOP_VIEWED.ToString().ToLower())
                    currentSort = Constants.QuestionSort.TOP_VIEWED;
            }

            IQueryable<Question> query = _questionRepo.Questions;

            //if there is no gender filter, then we can execute query directly without extra filtering
            if (!string.IsNullOrEmpty(gender))
            {
                gender = gender.ToLower(); //lower case it first to make it easier to compare
                var femaleGender = Constants.Gender.F.ToString().ToLower();
                var maleGender = Constants.Gender.M.ToString().ToLower();
                if (gender == femaleGender)
                {
                    query = query.Where(q => q.Gender.Equals(femaleGender, StringComparison.CurrentCultureIgnoreCase));
                    pageTitle = "Ladies Only Questions | RatherThis";
                }
                else if (gender == maleGender)
                {
                    query = query.Where(q => q.Gender.Equals(maleGender, StringComparison.CurrentCultureIgnoreCase));
                    pageTitle = "Guys Only Questions | RatherThis";
                }
            }

            int numResults = 0;
            if (currentSort == Constants.QuestionSort.TOP_VIEWED)
            {
                numResults = query.Count();
                results = query.OrderByDescending(q => q.Answers.Count()).ThenByDescending(q => q.DateCreated).Skip((page-1)*_pageSize).Take(_pageSize).ToList();

                //if we havent set a page title yet, that means it's not a gender page
                if (string.IsNullOrEmpty(pageTitle))
                {
                    pageTitle = "Top Viewed Questions | RatherThis";
                }
            }
            else
            {
                numResults = query.Count();
                results = query.OrderByDescending(q => q.DateCreated).Skip((page - 1) * _pageSize).Take(_pageSize).ToList();
                //if we havent set a page title yet, that means it's not a gender page
                if (string.IsNullOrEmpty(pageTitle))
                {
                    pageTitle = "Latest Questions | RatherThis";
                }
            }
            ViewBag.Title = pageTitle;


            Guid currentUserId = Guid.Empty; 
            User currentUser = null;
            bool isLoggedIn = _membershipService.IsAuthenticated();
            if(isLoggedIn){
                currentUserId = _membershipService.GetCurrentUserId();
                currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).First();
            }
             
            Dictionary<object, string> resultModel = new Dictionary<object, string>();
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

            QuestionIndexViewModel viewModel = new QuestionIndexViewModel();
            viewModel.ResultViewModels = resultModel;
            viewModel.CurrentPage = page;
            viewModel.Gender = gender;
            viewModel.Sort = sort;
            viewModel.TotalPages = (int)Math.Ceiling((double)numResults / _pageSize);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Answer(int qid = 0, int oid = -1)
        {
            if (qid > 0 && oid >= 0)
            {
                Question currentQuestion = _questionRepo.Questions.Where(q => q.QuestionID == qid).FirstOrDefault();

                if (_membershipService.IsAuthenticated())
                {
                    if (currentQuestion != null)
                    {
                        Answer newAnswer = new Answer();
                        newAnswer.QuestionID = qid;
                        newAnswer.QuestionOptionID = oid;
                        newAnswer.UserID = _membershipService.GetCurrentUserId();
                        _answerRepo.SaveAnswer(newAnswer);
                    }

                }
                return RedirectToAction("RenderQuestion", new { qid = currentQuestion.QuestionID });
            }
            return RedirectToAction("RenderQuestion", new { });
        }

        public PartialViewResult RenderQuestion(int qid, int commentListSize = 9, bool showAllComments = false)
        {
            Question q;

            //need to re-retrieve the question from db in order to get eager loading for its relationships, otherwise the question passed in to our action only  has the question's base properties
            q = _questionRepo.Questions.Where(qe => qe.QuestionID == qid).FirstOrDefault();
            //TODO: what happens if this is null (e.g. bad data)

            //get the question options
            QuestionOption option1 = q.QuestionOptions.ElementAt(0);
            QuestionOption option2 = q.QuestionOptions.ElementAt(1);

            DateTime date = q.DateCreated;
            String gender = q.Gender;
            String name = q.User.Username;
            String optionText1 = option1.OptionText;
            String optionText2 = option2.OptionText;

            bool isLoggedIn = _membershipService.IsAuthenticated();

            //if user is not logged in, then display the question view since we don't know whether user has answered or not
            if (!isLoggedIn)
            {
                QuestionDisplayViewModel model = new QuestionDisplayViewModel();
                model.IsLoggedIn = false;
                model.Date = date;
                model.Gender = gender;
                model.Name = name;
                model.UserId = Guid.Empty;

                model.OptionText1 = optionText1;
                model.OptionText2 = optionText2;

                model.OptionId1 = option1.QuestionOptionID;
                model.OptionId2 = option2.QuestionOptionID;

                model.QuestionId = q.QuestionID;
                model.QuestionUserGender = q.User.Gender;
                model.QuestionUsername = q.User.Username;

                return PartialView("_QuestionDisplay", model);
            }
            else 
            {
                //if user is logged in, then check whether user has already answered the question. if so then render answer view, otherwise render the question view                
                //or if the user doesn't match the gender restriction, then just display the answer format 
                //(e.g. if current user is male, and this is a 'for ladies' question, since user can't answer it anyways, then just display the answer view

                //check whether user has already answered this question
                Guid currentUserId = _membershipService.GetCurrentUserId();
                User currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).First();

                Answer userAnswer = _answerRepo.Answers.Where(a => a.UserID == currentUserId && a.QuestionID == q.QuestionID).FirstOrDefault();
                Guid questionUserId = q.UserID;

                bool isAnswered = (userAnswer == null) ? false : true;
                bool isGenderMismatch = (!string.IsNullOrEmpty(q.Gender) && q.Gender.ToLower() != currentUser.Gender.ToLower()) ? true : false;
                if (isAnswered || isGenderMismatch)
                {
                    AnswerDisplayViewModel model = new AnswerDisplayViewModel();
                    model.QuestionId = q.QuestionID;
                    model.Date = date;
                    model.Gender = gender;
                    model.Name = name;
                    model.UserId = questionUserId;
                    if(isAnswered)
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
                    */
                    model.NumComments = q.Comments.Count();
                    model.CommentListSize = commentListSize;
                    model.IsShowAllComment = showAllComments;

                    return PartialView("_AnswerDisplay", model);
                }
                else
                {
                    QuestionDisplayViewModel model = new QuestionDisplayViewModel();
                    model.IsLoggedIn = true;
                    model.Date = date;
                    model.Gender = gender;
                    model.Name = name;
                    model.UserId = questionUserId;

                    model.OptionText1 = optionText1;
                    model.OptionText2 = optionText2;

                    model.OptionId1 = option1.QuestionOptionID;
                    model.OptionId2 = option2.QuestionOptionID;

                    model.QuestionId = q.QuestionID;
                    model.QuestionUserGender = q.User.Gender;
                    model.QuestionUsername = q.User.Username;

                    return PartialView("_QuestionDisplay", model);
                }
            }
        }

        public ActionResult NewComment(int qid, int oid)
        {
            CommentFormViewModel model = new CommentFormViewModel();
            model.QuestionId = qid;
            model.AnsweredOptionId = oid;

            return View("_CommentsForm", model);
        }

        [HttpPost]
        public ActionResult NewComment(CommentFormViewModel model, int qid, int oid)
        {
            if (ModelState.IsValid && _membershipService.IsAuthenticated())
            {
                Comment newComment = new Comment();
                newComment.CommentText = model.Comment;
                newComment.QuestionID = qid;
                newComment.QuestionOptionID = oid;
                newComment.UserID = _membershipService.GetCurrentUserId();
                _commentRepo.SaveComment(newComment);

                return RedirectToAction("NewComment", new { qid = qid, oid = oid });
            }
            model.QuestionId = qid;
            model.AnsweredOptionId = oid;
            return View("_CommentsForm", model);
        }

        public ActionResult CommentList(int qid, int listSize = 0)
        {
            List<Comment> comments;
            Question q = _questionRepo.GetQuestionWithComments(qid);
            if (listSize > 0)
            {
                //if there is a list size param passed in, then take that number of comments, otherwise take all comments
                comments = q.Comments.OrderByDescending(c => c.DateCreated).Take(listSize).ToList();
            }
            else
            {
                comments = q.Comments.Where(c => c.QuestionID == qid).OrderByDescending(c => c.DateCreated).ToList();
            }

            CommentListViewModel model = new CommentListViewModel();
            model.Comments = comments;
            model.OptionId1 = q.QuestionOptions.ElementAt(0).QuestionOptionID;
            model.OptionId2 = q.QuestionOptions.ElementAt(1).QuestionOptionID;

            return View("_CommentList",model);
        }

        public ActionResult Detail(int qid)
        {
            Question q = _questionRepo.Questions.Where(qe => qe.QuestionID == qid).FirstOrDefault();
            QuestionDetailViewModel model = new QuestionDetailViewModel();
            model.Question = q;
            model.OptionText1 = q.QuestionOptions.ElementAt(0).OptionText;
            model.OptionText2 = q.QuestionOptions.ElementAt(1).OptionText;
            return View(model);
        }
    }
}
