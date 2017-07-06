using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKAWeb_v01.Classes;
using AKAWeb_v01.Models;

namespace AKAWeb_v01.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }

        private List<SectionModel> getSections()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT * from Sections";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SectionModel> sections = new List<SectionModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                bool isLive = (bool)dataReader.GetValue(2);
                SectionModel section = new SectionModel(id, title, isLive);
                sections.Add(section);

            }

            return sections;
        }
    }
}