using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Controllers
{
    public class SubPagesController : Controller
    {
        // GET: SubPages
        public ActionResult KinToday()
        {
            return View("~/Views/SubPages/Publications/KinesiologyToday.cshtml");
        }

        public ActionResult Workshop()
        {
            return View("~/Views/SubPages/EventsServices/WorkshopRegistration.cshtml");
        }
    }
}