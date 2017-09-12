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

        private List<AKAWeb_v01.Models.PageModel> getPages(int section_id)
        {
            List<AKAWeb_v01.Models.PageModel> page_list = new List<PageModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, subheader_image, content, section, isAlive from Pages WHERE isAlive = 1 AND section = "+section_id.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            //while there are records in the datareader
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string subheaderImage = dataReader.GetValue(2).ToString();
                string pageContent = dataReader.GetValue(3).ToString();
                int section = Int32.Parse(dataReader.GetValue(4).ToString());
                bool isAlive = (bool)dataReader.GetValue(5);
                PageModel page = new PageModel(id, title, subheaderImage, pageContent, section, isAlive);
                page_list.Add(page);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            return page_list;

        }

        private List<SectionModel> getSections()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT * from Sections WHERE isAlive = 1";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SectionModel> sections = new List<SectionModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                bool isLive = (bool)dataReader.GetValue(2);
                List<PageModel> pages = getPages(id);
                SectionModel section = new SectionModel(id, title, isLive, pages);
                sections.Add(section);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            return sections;
        }

        [ChildActionOnly]
        public PartialViewResult MainMenu()
        {
            var model = getSections();

            return PartialView("_MainMenu", model);
        }
    }
}