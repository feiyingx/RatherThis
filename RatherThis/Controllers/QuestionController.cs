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
        private IBumpRepository _bumpRepo;

        private int _pageSize = 9;

        public QuestionController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo, IUserRepository userRepo, ICommentRepository commentRepo, IBumpRepository bumpRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
            _commentRepo = commentRepo;
            _bumpRepo = bumpRepo;
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
                    newQuestion.Category = model.Category;
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

        public ActionResult Index(string sort, string gender, int page = 1, int qcat = -1, bool onlyUnanswered = false)
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
                }
                else if (gender == maleGender)
                {
                    query = query.Where(q => q.Gender.Equals(maleGender, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            //check if there is a category
            if (qcat >= 0)
            {
                //if category is the default 'unknown' category, then also check for questions that has 'null' as the category
                if (qcat == 0)
                    query = query.Where(q => q.Category == qcat || q.Category == null);
                else
                    query = query.Where(q => q.Category == qcat);
            }

            Guid currentUserId = Guid.Empty;
            User currentUser = null;
            List<int> answeredQuestionIds = new List<int>();
            bool isLoggedIn = _membershipService.IsAuthenticated();
            if (isLoggedIn)
            {
                currentUserId = _membershipService.GetCurrentUserId();
                currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).First();
                answeredQuestionIds = currentUser.Answers.Select(ans => ans.QuestionID).ToList();

                //check to see if we dont want to include answered questions, which means we should exclude questions 
                if (onlyUnanswered)
                {
                    query = query.Where(q => !answeredQuestionIds.Contains(q.QuestionID)).Where(q => string.IsNullOrEmpty(q.Gender) || q.Gender == currentUser.Gender);
                }
            }

            

            int numResults = 0;
            if (currentSort == Constants.QuestionSort.TOP_VIEWED)
            {
                numResults = query.Count();
                results = query.OrderByDescending(q => q.Answers.Count()).ThenByDescending(q => q.DateCreated).Skip((page-1)*_pageSize).Take(_pageSize).ToList();

            }
            else
            {
                numResults = query.Count();
                results = query.OrderByDescending(q => q.DateCreated).Skip((page - 1) * _pageSize).Take(_pageSize).ToList();
            }

            //set pagetitle based on the question category
            //if qcat is -1, then it means we're on the view all page
            if (qcat == -1)
            {
                pageTitle = "'Would You Rather' Questions For Everyone - RatherThis.com";
            }
            else
            {
                CategoryItem currentCat = Constants.QuestionCategories.Where(c => c.CategoryID == qcat).First();
                pageTitle = "Fun And Interesting Questions About " + currentCat.Name + " - RatherThis.com";
            }

            ViewBag.Title = pageTitle;
             
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

                //if category is null, then default to the 'unknown' category
                CategoryItem questionCat = q.Category ==  null ? Constants.QuestionCategories.Where(cat => cat.CategoryID == 0).First() : Constants.QuestionCategories.Where(cat => cat.CategoryID == q.Category).First();  //default to 'unknown'

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
                    model.QuestionCategory = questionCat.Name;
                    model.QuestionCategoryId = questionCat.CategoryID;

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

                        model.QuestionCategory = questionCat.Name;
                        model.QuestionCategoryId = questionCat.CategoryID;
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
                        model.QuestionCategory = questionCat.Name;
                        model.QuestionCategoryId = questionCat.CategoryID;

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
            viewModel.CurrentCategoryId = qcat;
            viewModel.IsOnlyUnanswered = onlyUnanswered;

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
                        Guid currentUserId = _membershipService.GetCurrentUserId();
                        //check if user has already answered, COURTESY OF JERM
                        Answer existingAnswer = _answerRepo.Answers.Where(a => a.QuestionID == qid && a.UserID == currentUserId).FirstOrDefault();
                        if (existingAnswer == null)
                        {
                            Answer newAnswer = new Answer();
                            newAnswer.QuestionID = qid;
                            newAnswer.QuestionOptionID = oid;
                            newAnswer.UserID = currentUserId;
                            _answerRepo.SaveAnswer(newAnswer);
                        }
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

        public ActionResult Bump(int qid, string direction)
        {
            //make sure 'direction'is valid
            if (direction == "up" || direction == "down" || direction == "reset")
            {

                Guid currentUserId = _membershipService.GetCurrentUserId();

                //check whether question exists
                Question question = _questionRepo.Questions.Where(q => q.QuestionID == qid).FirstOrDefault();
                if (question != null)
                {

                    //check whether the user has already bumped this question
                    Bump alreadyBumped = _bumpRepo.Bumps.Where(b => b.QuestionID == qid && b.UserID == currentUserId).FirstOrDefault();
                    if (alreadyBumped == null && direction != "reset")
                    {
                        if (question != null)
                        {
                            //hasnt been bumped, so record the bump
                            Bump newBump = new Bump();
                            newBump.QuestionID = qid;
                            newBump.UserID = currentUserId;

                            if (direction == "up")
                            {
                                newBump.BumpUpValue = 1;
                                //update question value as well
                                question.BumpUpValue += 1;
                            }
                            else
                            {
                                newBump.BumpDownValue = 1;
                                //update question value as well
                                question.BumpDownValue += 1;
                            }

                            _bumpRepo.SaveBump(newBump);
                            _questionRepo.SaveQuestion(question);
                            
                        }
                    }
                    else
                    {
                        //check what the previous bump value was, if user already bumped up, then do nothing since already been bumped up
                        //if user has bumped it down before, then update the bump to a bump up
                        if (direction == "up")
                        {
                            if (alreadyBumped.BumpDownValue > 0)
                            {
                                alreadyBumped.BumpDownValue = 0;
                                alreadyBumped.BumpUpValue = 1;
                                _bumpRepo.SaveBump(alreadyBumped);

                                question.BumpDownValue -= 1; //remove 1 bump down since user changed their bump to a bump up
                                question.BumpUpValue += 1;
                            }
                            else if (alreadyBumped.BumpUpValue == 0)
                            {
                                //this case checks whether the user may have 'reset'their bump so now it's back to zero
                                alreadyBumped.BumpDownValue = 0;
                                alreadyBumped.BumpUpValue = 1;
                                _bumpRepo.SaveBump(alreadyBumped);

                                question.BumpUpValue += 1;
                            }
                        }
                        else if (direction == "down")
                        {
                            //check whether user had previously bumped it up, if so, we'll need to subtract 1 from the dump value
                            if (alreadyBumped.BumpUpValue > 0)
                            {
                                alreadyBumped.BumpUpValue = 0;
                                alreadyBumped.BumpDownValue = 1;
                                _bumpRepo.SaveBump(alreadyBumped);

                                question.BumpUpValue -= 1;
                                question.BumpDownValue += 1;
                            }
                            else if (alreadyBumped.BumpDownValue == 0)
                            {
                                //this case checks whether the user may have 'reset'their dump so now it's back to zero
                                alreadyBumped.BumpUpValue = 0;
                                alreadyBumped.BumpDownValue = 1;
                                _bumpRepo.SaveBump(alreadyBumped);

                                question.BumpDownValue += 1;
                            }
                        }
                        else
                        {
                            if (alreadyBumped.BumpUpValue > 0)
                            {
                                question.BumpUpValue -= 1;
                            }
                            else if(alreadyBumped.BumpDownValue > 0)
                            {
                                question.BumpDownValue -= 1;
                            }

                            alreadyBumped.BumpUpValue = alreadyBumped.BumpDownValue = 0;
                            _bumpRepo.SaveBump(alreadyBumped);
                        }
                        _questionRepo.SaveQuestion(question);
                    }

                    return Json("success", JsonRequestBehavior.AllowGet);
                }
            }

            return Json("failed", JsonRequestBehavior.AllowGet);
        }

        public ActionResult BumpControl(int qid)
        {
            BumpControlViewModel model = new BumpControlViewModel();
            model.QuestionID = qid;

            if (_membershipService.IsAuthenticated())
            {
                Guid userId = _membershipService.GetCurrentUserId();
                Bump userBump = _bumpRepo.Bumps.Where(b => b.UserID == userId && b.QuestionID == qid).FirstOrDefault();
                if (userBump == null)
                {
                    model.IsBumped = model.IsDumped = false;
                    model.NetBumps = 0;
                }
                else
                {
                    model.IsBumped = userBump.BumpUpValue > 0;
                    model.IsDumped = userBump.BumpDownValue > 0;
                    model.NetBumps = userBump.BumpUpValue - userBump.BumpDownValue;
                }

                model.IsLoggedIn = true;
            }
            else
            {
                model.IsLoggedIn = false;
            }

            return View("_BumpControl", model);
        }
    }
}
