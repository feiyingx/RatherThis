using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RatherThis.Controllers
{
    public class ErrorController : Controller
    {
        [ActionName("404")]
        public ActionResult _404()
        {
            return View("404");
        }

        [ActionName("500")]
        public ActionResult _500()
        {
            return View("500");
        }

    }
}
