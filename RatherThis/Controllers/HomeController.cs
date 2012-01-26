using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Entities;
using RatherThis.Models;
using RatherThis.Service.Interface;
using Ninject;

namespace RatherThis.Controllers
{
    public class HomeController : Controller
    {
        [Inject]
        public IEmailService EmailService { get; set; }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Contact(string msg = "")
        {

            if (!string.IsNullOrEmpty(msg))
            {
                ViewBag.Flash = msg; 
            }
            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                EmailService.SendContactUsEmail(model.Email, model.Type, model.Description);
                return RedirectToAction("Contact", new { msg = model.Email });
            }
            return View(model);
        }
    }
}
