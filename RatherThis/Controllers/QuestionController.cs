using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RatherThis.Domain.Abstract;
using RatherThis.Models;
using RatherThis.Domain.Entities;

namespace RatherThis.Controllers
{
    public class QuestionController : Controller
    {
        private IMembershipService _membershipService;
        private IQuestionRepository _questionRepo;
        private IQuestionOptionRepository _optionRepo;
        private IAnswerRepository _answerRepo;

        public QuestionController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
        }

        public ActionResult New()
        {
            return View();
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

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public ActionResult Latest()
        {
            List<Question> questions = _questionRepo.Questions.OrderByDescending(q => q.DateCreated).Take(10).ToList();
            
            return View(questions);
        }

        [HttpPost]
        public PartialViewResult Answer(int qid = 0, int oid = -1)
        {
            QuestionDisplayViewModel model = new QuestionDisplayViewModel();   

            if (_membershipService.IsAuthenticated())
            {
                if (qid > 0 && oid >= 0)
                {
                    Question currentQuestion = _questionRepo.Questions.Where(q => q.QuestionID == qid).FirstOrDefault();
                    model.IsLoggedIn = true;

                    if (currentQuestion != null)
                    {
                        Answer newAnswer = new Answer();
                        newAnswer.QuestionID = qid;
                        newAnswer.QuestionOptionID = oid;
                        newAnswer.UserID = _membershipService.GetCurrentUserId();
                        _answerRepo.SaveAnswer(newAnswer);

                        model.Question = currentQuestion;
                        model.HasAnsweredQuestion = true;
                    }
                }
            }
            return PartialView("_QuestionDisplayPartial", model);   
        }

        public PartialViewResult RenderQuestion(Question q)
        {
            if (q == null)
                throw new NullReferenceException("Trying to RenderQuestion with a null question");

            q = _questionRepo.Questions.Where(qe => qe.QuestionID == q.QuestionID).FirstOrDefault();
            //check whether user has already answered this question
            Guid currentUserId = _membershipService.GetCurrentUserId();
            Answer userAnswer = _answerRepo.Answers.Where(a => a.UserID == currentUserId && a.QuestionID == q.QuestionID).FirstOrDefault();

            bool isAnswered = (userAnswer == null) ? false : true;
            if (isAnswered)
            {
                AnswerDisplayViewModel model = new AnswerDisplayViewModel();
                model.Date = q.DateCreated;
                model.Gender = q.Gender;
                model.Name = q.User.Username;

                //get the question options
                QuestionOption option1 = q.QuestionOptions.ElementAt(0);
                QuestionOption option2 = q.QuestionOptions.ElementAt(1);
                model.OptionText1 = option1.OptionText;
                model.OptionText2 = option2.OptionText;

                model.OptionVotes1 = q.Answers.Where(a => a.QuestionOptionID == option1.QuestionOptionID).Count();
                model.OptionVotes2 = q.Answers.Where(a => a.QuestionOptionID == option2.QuestionOptionID).Count();
                model.TotalVotes = (model.OptionVotes1 + model.OptionVotes2);
                
                return PartialView("_AnswerDisplay", model);
            }
            else
            {
                return PartialView("_QuestionDisplay");
            }
        }
    }
}
