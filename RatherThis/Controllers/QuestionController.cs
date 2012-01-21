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

        private int _pageSize = 9;

        public QuestionController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo, IUserRepository userRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
        }

        public ActionResult New()
        {
            return View("_NewQuestion");
        }

        [HttpPost]
        public ActionResult New(NewQuestionViewModel model)
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
                option1.OptionText = model.Option1;
                option2.OptionText = model.Option2;
                _optionRepo.SaveQuestionOption(option1);
                _optionRepo.SaveQuestionOption(option2);

                return JavaScript("window.location.reload();");
            }

            return View("_NewQuestion", model);
        }

        public ActionResult Index(string sort, string gender)
        {
            List<Question> results;

            //default the sort to latest
            Constants.QuestionSort currentSort = Constants.QuestionSort.LATEST;

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

            if (currentSort == Constants.QuestionSort.TOP_VIEWED)
            {
                results = query.OrderByDescending(q => q.Answers.Count()).ThenByDescending(q => q.DateCreated).Take(_pageSize).ToList(); ;
            }
            else
            {
                results = query.OrderByDescending(q => q.DateCreated).Take(_pageSize).ToList();
            }
            
            
            return View(results);
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
                return RedirectToAction("RenderQuestion", currentQuestion);
            }
            return RedirectToAction("RenderQuestion", new { });
        }

        public PartialViewResult RenderQuestion(Question q)
        {
            if (q == null)
                throw new NullReferenceException("Trying to RenderQuestion with a null question");

            //need to re-retrieve the question from db in order to get eager loading for its relationships, otherwise the question passed in to our action only  has the question's base properties
            q = _questionRepo.Questions.Where(qe => qe.QuestionID == q.QuestionID).FirstOrDefault();
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

                    model.OptionText1 = optionText1;
                    model.OptionText2 = optionText2;

                    model.OptionVotes1 = q.Answers.Where(a => a.QuestionOptionID == option1.QuestionOptionID).Count();
                    model.OptionVotes2 = q.Answers.Where(a => a.QuestionOptionID == option2.QuestionOptionID).Count();
                    model.TotalVotes = (model.OptionVotes1 + model.OptionVotes2);

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

                    return PartialView("_QuestionDisplay", model);
                }
            }
        }
    }
}
