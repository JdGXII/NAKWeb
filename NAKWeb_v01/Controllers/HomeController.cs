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
            string query2 = "select link1, link2, link3, link4, link5, link6 from carousel_links where id = 1";
            dataReader = testconn.ReadFromTest(query2);
            dataReader.Read();
            ViewData["link1"] = dataReader.GetValue(0).ToString();
            ViewData["link2"] = dataReader.GetValue(1).ToString();
            ViewData["link3"] = dataReader.GetValue(2).ToString();
            ViewData["link4"] = dataReader.GetValue(3).ToString();
            ViewData["link5"] = dataReader.GetValue(4).ToString();
            ViewData["link6"] = dataReader.GetValue(5).ToString();

            testconn.CloseDataReader();
            testconn.CloseConnection();

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