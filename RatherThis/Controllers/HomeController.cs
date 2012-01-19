using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Entities;
using RatherThis.Models;

namespace RatherThis.Controllers
{
    public class HomeController : Controller
    {
        private IQuestionRepository _questionRepo;
        private int _pageSize = 10;

        public HomeController(IQuestionRepository questionRepo)
        {
            _questionRepo = questionRepo;
        }

        public ActionResult Index(int page = 1)
        {
            List<Question> questions = _questionRepo.Questions.OrderByDescending(q => q.DateCreated).Skip((page - 1) * _pageSize).Take(_pageSize).ToList();
            HomeIndexViewModel model = new HomeIndexViewModel();
            model.Questions = questions;
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
