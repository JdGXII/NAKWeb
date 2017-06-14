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
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        private PageModel getPage(string title)
        {
            title = HttpUtility.UrlDecode(title);
            DBConnection testconn = new DBConnection();
            SqlDataReader datareader;
            string query = "select id, title, subheader_image, content from Pages where title ='" + title+"'"; 
            //submenu_id is actually the id of the parent menu. So read it as "parent id" in your head
            //the following query basically returns the submenus of a page given the title of the page
            string query2 = "select id, item_name, islive, submenu_id from main_menu where submenu_id = (select id from main_menu where title ='" + title + "')";

            if (testconn.ReadFromTest(query).Read())
            {
                List<MainMenuItem> menu = new List<MainMenuItem>();
                datareader = testconn.ReadFromTest(query2);
                while (datareader.Read())
                {
                    MainMenuItem menuitem = new MainMenuItem();
                    menuitem.id = Int32.Parse(datareader.GetValue(0).ToString());
                    menuitem.item_name = datareader.GetValue(1).ToString();
                    menuitem.islive = (bool)datareader.GetValue(2);
                    menuitem.submenu_id = Int32.Parse(datareader.GetValue(3).ToString());
                    menu.Add(menuitem);

                }

                datareader = testconn.ReadFromTest(query);
                datareader.Read();
                PageModel page = new PageModel();
                page.id = Int32.Parse(datareader.GetValue(0).ToString());
                page.title = datareader.GetValue(1).ToString();
                page.subheaderImage = datareader.GetValue(2).ToString();
                page.pageContent = datareader.GetValue(3).ToString();
                page.leftMenu = menu;

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