using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RatherThis.Models;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Entities;

namespace RatherThis.Controllers
{
    public class WkysController : Controller
    {
        private IMembershipService _membershipService;
        private IQuestionRepository _questionRepo;
        private IQuestionOptionRepository _optionRepo;
        private IAnswerRepository _answerRepo;
        private IUserRepository _userRepo;
        private ICommentRepository _commentRepo;

        public WkysController(IMembershipService membershipService, IQuestionRepository questionRepo, IQuestionOptionRepository optionRepo, IAnswerRepository answerRepo, IUserRepository userRepo, ICommentRepository commentRepo)
        {
            _membershipService = membershipService;
            _questionRepo = questionRepo;
            _optionRepo = optionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
            _commentRepo = commentRepo;
        }

        public ActionResult ToDoQuestions()
        {
            //restrict access to calvin and i
            if (!_membershipService.IsAuthenticated())
            {
                return RedirectToAction("404", "Error");

                Guid currentUserId = _membershipService.GetCurrentUserId();
                User currentUser = _userRepo.Users.Where(u => u.UserID == currentUserId).FirstOrDefault();
                if (currentUser.Email.ToLower() != "feiyingx@gmail.com" || currentUser.Email.ToLower() != "calvinthenwang@gmail.com")
                {
                    return RedirectToAction("404", "Error");
                }
            }

            List<Question> questions = _questionRepo.Questions.Where(q => q.Category == null).ToList();
            return View(questions);
        }

        public ActionResult Edit(int qid = 0)
        {
            if (qid == 0)
            {
                return RedirectToAction("404", "Error");
            }

            Question question = _questionRepo.Questions.Where(q => q.QuestionID == qid).FirstOrDefault();
            EditQuestionViewModel model = new EditQuestionViewModel();
            model.Category = question.Category.HasValue ? question.Category.Value : 0;
            model.Gender = question.Gender;
            model.Option1 = question.QuestionOptions.ElementAt(0).OptionText;
            model.Option2 = question.QuestionOptions.ElementAt(1).OptionText;
            model.QuestionID = question.QuestionID;
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(EditQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                Question edittedQuestion = _questionRepo.Questions.Where(q => q.QuestionID == model.QuestionID).FirstOrDefault();
                edittedQuestion.Category = model.Category;
                edittedQuestion.Gender = model.Gender;
                edittedQuestion.QuestionOptions.ElementAt(0).OptionText = model.Option1;
                edittedQuestion.QuestionOptions.ElementAt(1).OptionText = model.Option2;
                _questionRepo.SaveQuestion(edittedQuestion);

                return RedirectToAction("ToDoQuestions");
            }

            return View(model);
        }
    }
}
