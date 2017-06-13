using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Controllers
{
    public class SubPagesController : Controller
    {

        public ActionResult Pages(string id)
        {
            return RedirectToAction("Index", "Home");
        }

    }
}