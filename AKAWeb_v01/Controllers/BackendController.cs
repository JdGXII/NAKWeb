using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Controllers
{
    public class BackendController : Controller
    {
        private string username = "admin";
        private string password = "admin";

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                if((username == this.username) && (password == this.password))
                {
                    System.Web.HttpContext.Current.Session["userpermission"] = "3";
                    
                    ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                    
                    return RedirectToAction("EditCarousel");

                }
                else
                {
                    ViewBag.Message = "Wrong Credentials";
                    return RedirectToAction("Index");
                }
                
            }
            catch
            {
                ViewBag.Message = "Something went wrong while validating";
                return View("~/Views/Backend/Index.cshtml");
            }
            
        }
        public ActionResult EditCarousel()
        {
            String userpermission = ""; 
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
           
                return View();
            }
            else
                return RedirectToAction("Index");
        }
        // GET: Backend
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload()
        {


            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                try
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/CarouselUploads"), fileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch(Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }

            return RedirectToAction("EditCarousel");
        }

        public ActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Test(HttpPostedFileBase file)
        {



            if (file != null && file.ContentLength > 0)
                try
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/CarouselUploads"), fileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }


            return View();
        }

    }
}
