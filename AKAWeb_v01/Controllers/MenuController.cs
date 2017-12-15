using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
            string query = "SELECT id, title, subheader_image, content, section, isAlive, sort_order from Pages WHERE isAlive = 1 AND section = @sectionId";

            Dictionary<string, Object> query_params = new Dictionary<string, Object>();
            query_params.Add("@sectionId", section_id);

            SqlDataReader dataReader = testconn.ReadFromProduction(query, query_params);
            //while there are records in the datareader
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string subheaderImage = dataReader.GetValue(2).ToString();
                string pageContent = dataReader.GetValue(3).ToString();
                int section = Int32.Parse(dataReader.GetValue(4).ToString());
                bool isAlive = (bool)dataReader.GetValue(5);
                int sort_order = Int32.Parse(dataReader.GetValue(6).ToString());
                PageModel page = new PageModel(id, title, subheaderImage, pageContent, section, isAlive, sort_order);
                page_list.Add(page);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            List<PageModel> sorted_pages = page_list.OrderBy(p => p.sort_order).ToList();
            return sorted_pages;

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
                int sort_order = getSortOrder(id);

                SectionModel section = new SectionModel(id, title, isLive, pages,sort_order);
                sections.Add(section);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            //sort sections
            List<SectionModel> sorted_sections = sections.OrderBy(s => s.sort_order).ToList();
            return sorted_sections;
        }

        //returns the sort order number belonging to a section
        //
        private int getSortOrder(int section_id)
        {
            DBConnection testconn = new DBConnection();
            //get the sort order from the Section_Sorting table
            StringBuilder sort_query = new StringBuilder("SELECT section_sortnumber FROM Section_Sorting WHERE section_id = ");
            sort_query.Append("@sectionId");

            Dictionary<string, Object> query_params = new Dictionary<string, Object>();
            query_params.Add("@sectionId", section_id);

            SqlDataReader sortOrderReader = testconn.ReadFromProduction(sort_query.ToString(), query_params);
            sortOrderReader.Read();
            int sort_order = Int32.Parse(sortOrderReader.GetValue(0).ToString());

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return sort_order;

        }

        [ChildActionOnly]
        public PartialViewResult MainMenu()
        {
            var model = getSections();

            return PartialView("_MainMenu", model);
        }
    }
}