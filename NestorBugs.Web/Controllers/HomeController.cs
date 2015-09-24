using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Konamiman.NestorBugs.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Faq()
        {
            ViewBag.MainContentTitle = "Frequently Asked Questions";

            return View();
        }

        public ActionResult Index()
        {
            return RedirectToActionPermanent("List", "Bugs");
        }

        public ActionResult Markdown()
        {
            ViewBag.MainContentTitle = "Makrdown syntax";

            return View();
        }

    }
}
