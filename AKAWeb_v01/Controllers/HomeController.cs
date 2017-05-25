using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKAWeb_v01.Classes;

namespace AKAWeb_v01.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DBConnection testconn = new DBConnection();
            string query = "select image_number from carousel where id = 1";
            SqlDataReader dataReader;
            dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            ViewBag.CarouselImg = dataReader.GetValue(0);
            testconn.CloseDataReader();
            testconn.CloseDataReader();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Privacy()
        {
            ViewBag.Message = "Privacy Page";
            return View();
        }

        public ActionResult TermsConditions()
        {
            ViewBag.Message = "Terms and Conditions Page";
            return View();
        }
    }
}