using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKAWeb_v01.Models;
using AKAWeb_v01.Classes;
using System.Data.SqlClient;

namespace AKAWeb_v01.Controllers
{
    public class SubPagesController : Controller
    {

        public ActionResult Pages(string id)
        {
            if(id != null)
            {
                 var model = getPage(id);
                 if (model.isLive)
                 {
                     return View(model);
                 }
                 else
                 {
                     return View("Error");
                 }
                

            }
            else
            {
                return View("Error");
            }
            
        }

        private PageModel getPage(string title)
        {
            title = HttpUtility.UrlDecode(title);
            DBConnection testconn = new DBConnection();
            SqlDataReader datareader;
            //this query selects the page to load by title, if it exists i.e. title matches the functions parameters
            string query = "select id, title, subheader_image, content, section, isAlive from Pages where title ='" + title+"'";

            //assign data reader to first query
            datareader = testconn.ReadFromTest(query);
            //check if there are matching pages
            if (datareader.Read())
            {
                //this gets the section of the page and stores it in a string to be used multiple times
                string section = datareader.GetValue(4).ToString();
                //this query gets all page titles that belong to the same section as the requested page
                string query2 = "select title from Pages where section = " + section+" AND isAlive = 1";
                List<string> menu = new List<string>();
                //read from query2
                datareader = testconn.ReadFromTest(query2);
                while (datareader.Read())
                {
                    string menutitle = datareader.GetValue(0).ToString();
                    menu.Add(menutitle);

                }
                //this query gets the title of the section to which the page(s) belong
                string query3 = "select name from Sections where id = " + section;
                //use datareader to read from query3
                datareader = testconn.ReadFromTest(query3);
                datareader.Read();
                string leftMenuTitle = datareader.GetValue(0).ToString();
                //read from first query again
                datareader = testconn.ReadFromTest(query);
                datareader.Read();
                PageModel page = new PageModel();
                page.id = Int32.Parse(datareader.GetValue(0).ToString());
                page.title = datareader.GetValue(1).ToString();
                page.subheaderImage = datareader.GetValue(2).ToString();
                page.pageContent = datareader.GetValue(3).ToString();
                page.leftMenu = menu;
                page.leftMenuTitle = leftMenuTitle;
                page.isLive = (bool)datareader.GetValue(5);

                return page;
            }
            else
            {
                PageModel page = new PageModel();
                              page.id = 1;
                page.title = "Test";
                page.subheaderImage = "";
                page.pageContent = "<div>hola</div>";
                page.leftMenu = null;
                return page;
            }


        }

    }
}