using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using AKAWeb_v01.Classes;
using AKAWeb_v01.Models;

namespace AKAWeb_v01.Controllers
{
    public class BackendController : Controller
    {
        private string username = "admin";
        private string password = "admin";

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            DBConnection testconn = new DBConnection();

            string query = "Select name, email, id, access from Users Where email ='" + username + "' AND password ='" + password + "'";


            try
            {
                SqlDataReader dataReader;
                dataReader = testconn.ReadFromTest(query);
                dataReader.Read();


                if (dataReader.GetValue(1) != null)  //(username == this.username) && (password == this.password))
                {
                    System.Web.HttpContext.Current.Session["userpermission"] = dataReader.GetValue(3).ToString();
                    System.Web.HttpContext.Current.Session["username"] = dataReader.GetValue(0).ToString();
                    System.Web.HttpContext.Current.Session["userid"] = dataReader.GetValue(2).ToString();

                    //ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                    testconn.CloseDataReader();
                    testconn.CloseConnection();
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    ViewBag.Message = "Wrong Credentials";
                    testconn.CloseDataReader();
                    testconn.CloseConnection();
                    return RedirectToAction("Index");
                }

            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                return RedirectToAction("Index", "Home");
            }

        }

        private List<SelectListItem> GenerateViewBagList()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT * from Sections";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SelectListItem> list = new List<SelectListItem>();
            
            while (dataReader.Read())
            {
                SelectListItem item = new SelectListItem();
                item.Value = dataReader.GetValue(0).ToString();
                item.Text = dataReader.GetValue(1).ToString();

                list.Add(item);

            }
            return list;
        }

        [HttpPost]
        public ActionResult DeleteImage(int picnum)
        {
            DBConnection dbconnect = new DBConnection();
            string query = "Update Carousel set image_number = (select image_number from Carousel where id =1) -1 where id = 1";
            int carousel_number = getCurrentCarouselNumber();
            string path = Server.MapPath("~/Content/Images/CarouselUploads/carouselpicture" + picnum.ToString() + ".jpg");
            UpdateLinks("", picnum);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            dbconnect.WriteToTest(query);
            dbconnect.CloseDataReader();
            dbconnect.CloseConnection();
            while (carousel_number - picnum >= 0)
            {
                path = Server.MapPath("~/Content/Images/CarouselUploads/carouselpicture" + picnum.ToString() + ".jpg");
                string newpath = Server.MapPath("~/Content/Images/CarouselUploads/carouselpicture" + (picnum + 1).ToString() + ".jpg");
                if (System.IO.File.Exists(newpath))
                {
                    System.IO.File.Move(newpath, path);
                }


                picnum++;
            }
            //}



            return RedirectToAction("EditCarousel");

        }


        [HttpPost]
        public ActionResult ChangeCarouselNumber()
        {

            DBConnection dbconnect = new DBConnection();
            string query = "Update Carousel set image_number = (select image_number from Carousel where id =1) +1 where id = 1";
            dbconnect.WriteToTest(query);
            dbconnect.CloseConnection();
            return RedirectToAction("EditCarousel");
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
                //creates the left menu in the backend page
                ViewData["BackendPages"] = getBackendPages();
                DBConnection testconn = new DBConnection();
                string query = "select * from pivoted_car_link"; //queries view with unpivoted external links
                string query2 = "select * from pivoted_car_image"; //queries view with unpivoted image urls
                SqlDataReader dataReader;
                SqlDataReader dataReader2;
                dataReader = testconn.ReadFromTest(query);
                dataReader2 = testconn.ReadFromTest(query2);
                var model = new List<CarouselImage>();

                while (dataReader.Read())
                {
                    dataReader2.Read();
                    CarouselImage image = new CarouselImage();
                    image.externalLink = dataReader.GetValue(2).ToString();
                    image.url = dataReader2.GetValue(2).ToString();
                    model.Add(image);
                }
                string query3 = "select image_number from carousel where id = 1";

                dataReader = testconn.ReadFromTest(query3);
                dataReader.Read();
                ViewBag.CarouselImageNumber = dataReader.GetValue(0);
                return View(model);
            }
            else
                return RedirectToAction("Index");
        }

        private int getCurrentCarouselNumber()
        {
            DBConnection testconn = new DBConnection();
            string query3 = "select image_number from carousel where id = 1";
            SqlDataReader dataReader;
            dataReader = testconn.ReadFromTest(query3);
            dataReader.Read();
            int number = Int32.Parse(dataReader.GetValue(0).ToString());
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return number;
        }
        // GET: Backend
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Upload(int picnum, string link)
        {


            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                    try
                    {
                        var fileName = PictureName(picnum);
                        var path = Path.Combine(Server.MapPath("~/Content/Images/CarouselUploads"), fileName);
                        file.SaveAs(path);
                        ViewBag.Message = "File uploaded successfully";
                        UpdateLinks(link, picnum);

                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }

            }

            return RedirectToAction("EditCarousel");
        }

        private String PictureName(int picnum)
        {
            return "carouselpicture" + picnum.ToString() + ".jpg";
        }



        private void UpdateLinks(string url, int picnum)
        {
            DBConnection testconn = new DBConnection();
            string query = "Update carousel_links set link" + picnum.ToString() + " = '" + url + "' where id = 1";
            testconn.WriteToTest(query);
            
        }

        public ActionResult EditMenu()
        {

            var model = new List<MainMenuItem>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT * FROM main_menu";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            while (dataReader.Read())
            {
                var item = new MainMenuItem();
                item.id = Convert.ToInt32(dataReader.GetValue(0));
                item.item_name = dataReader.GetValue(1).ToString();
                item.islive = (bool)dataReader.GetValue(2);
                item.submenu_item = dataReader.GetValue(3) as MainMenuItem;

                model.Add(item);
            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddMainMenuItem(string menu_name)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO main_menu (item_name, islive) VALUES ('" + menu_name + "', 1)";
            testconn.WriteToTest(query);
            testconn.CloseConnection();



            return RedirectToAction("EditMenu");
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult RegisterUser(string name, string email, string password)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Users (name, email, password, access) VALUES ('" + name + "', '" + email + "', '" + password + "',  1)";
            testconn.WriteToTest(query);
            testconn.CloseConnection();
            System.Web.HttpContext.Current.Session["userpermission"] = "1";
            System.Web.HttpContext.Current.Session["username"] = name;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult MyProfile()
        {
            return View();
        }

        public ActionResult Main()
        {
            ViewData["BackendPages"] = getBackendPages();
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        //controller action that manages the edit pages page. Lists all the pages
        public ActionResult ListPages()
        {
            ViewData["BackendPages"] = getBackendPages();
            var model = getPages();
            return View(model);
        }

        //gets all pages in the db and return a list with them 
        private List<AKAWeb_v01.Models.PageModel> getPages()
        {
            List<AKAWeb_v01.Models.PageModel> page_list = new List<PageModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, subheader_image, content, section, isAlive from Pages";
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
                PageModel page = new PageModel(id, title, subheaderImage, pageContent,section, isAlive);
                page_list.Add(page);

            }

            return page_list;

        }
        //gets a single page by id
        private PageModel getPage(int pageId)
        {
            PageModel page = null;
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, subheader_image, content, section, isAlive from Pages WHERE id =" + pageId.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string subheaderImage = dataReader.GetValue(2).ToString();
                string pageContent = dataReader.GetValue(3).ToString();
                int section = Int32.Parse(dataReader.GetValue(4).ToString());
                bool isAlive = (bool)dataReader.GetValue(5);
                page = new PageModel(id, title, subheaderImage, pageContent, section, isAlive);
                
            }

            return page;
        }

        //gets and individual section from which a page belongs to and returns it as a SectionModel to be used by views
        private SectionModel getSection(string id)
        {
            DBConnection testconn = new DBConnection();
            //get which section
            string query = "SELECT section from Pages where id = " + id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            //change query to get selected sections
            query = "SELECT id, name, isAlive from Sections where id = " + dataReader.GetValue(0).ToString();
            dataReader = testconn.ReadFromTest(query);
            dataReader.Read();

            int section_id = Int32.Parse(dataReader.GetValue(0).ToString());
            string name = dataReader.GetValue(1).ToString();
            bool isLive = (bool)dataReader.GetValue(2);
            SectionModel section = new SectionModel(section_id, name, isLive);

            return section;

        }

        public ActionResult ChangePageSection(string id)
        {
            ViewData["BackendPages"] = getBackendPages();
            var model = getSection(id);
            ViewData["SectionList"] = GenerateViewBagList();
            PageModel page = getPage(Int32.Parse(id));
            ViewData["PageTitle"] = page.title;
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePageSection(string SectionList, string page_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Pages SET section=" + SectionList + " WHERE id =" + page_id;
            bool t = testconn.WriteToTest(query);
            if (t) { }
            return RedirectToAction("ChangePageSection", new { id = page_id });
        }

        public ActionResult EditPage(string id)
        {
            
            var model = getPage(Int32.Parse(id));
            if(model != null)
            {
                ViewData["BackendPages"] = getBackendPages();
                return View(model);
            }
            else
            {
                return RedirectToAction("ListPages", "Backend");
            }
            
        }

        [HttpPost, ValidateInput(false)]     
        public ActionResult EditPage(string id, string content, string title)
        {
            //uncomment the next line to check content being passed through form
            //System.Web.HttpContext.Current.Session["debug"] = Request.Files.Count;
            
            DBConnection testconn = new DBConnection();
            //original query to be executed. It will change if an image is being updated
            string query = "UPDATE Pages SET content ='" + content + "', title = '"+title+"' where id =" + id;
            //check if user is updating the page's subheader image by looking for the file in the request
            //if he is, this will change the query to be executed
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                    try
                    {
                        var fileName = file.FileName;
                        var path = Path.Combine(Server.MapPath("~/Content/Images/Subheaders"), fileName);
                        file.SaveAs(path);
                        string pathForDB = "~/Content/Images/Subheaders/" + fileName.ToString(); 
                        query = "UPDATE Pages SET content ='" + content + "', subheader_image ='"+ pathForDB + "', title = '" + title + "' where id =" + id;
                        ViewBag.Message = "File uploaded successfully";
                        

                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }

            }
            
            testconn.WriteToTest(query);
            testconn.CloseConnection();

            return RedirectToAction("ListPages", "Backend");

        }

        public ActionResult CreatePage()
        {
            ViewData["SectionList"] = GenerateViewBagList();
            ViewData["BackendPages"] = getBackendPages();
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreatePage(string title, string content, string SectionList)
        {
            //uncomment the next line to check content being passed through form
            //System.Web.HttpContext.Current.Session["debug"] = Request.Files.Count;

            DBConnection testconn = new DBConnection();
            //original query to be executed. It will change if an image is being updated
            string query = "INSERT into Pages (title, subheader_image, content, created_at, modified_at, section, isAlive)" +
                "VALUES ('" + title + "', ' ', '" + content + "', " + "(select getdate()), (select getdate()), " + SectionList + ", 1)";
            //check if user is updating the page's subheader image by looking for the file in the request
            //if he is, this will change the query to be executed
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                    try
                    {
                        var fileName = file.FileName;
                        var path = Path.Combine(Server.MapPath("~/Content/Images/Subheaders"), fileName);
                        file.SaveAs(path);
                        string pathForDB = "~/Content/Images/Subheaders/" + fileName.ToString();
                        //query = "INSERT into Pages (title, subheader_image, content, created_at, modified_at, section) VALUES (" + title + ", " + pathForDB + ", " + content + ", getdate(), getdate(), " + SectionList + ")";
                        query = "INSERT into Pages(title, subheader_image, content, created_at, modified_at, section, isAlive) VALUES('" + title + "', '" + pathForDB + "', '" + content + "', getdate(), getdate(), " + SectionList + ", 1)";
                        ViewBag.Message = "File uploaded successfully";


                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }

            }

            testconn.WriteToTest(query);
            testconn.CloseConnection();

            return RedirectToAction("ListPages", "Backend");

        }

        public ActionResult ToggleIsLivePage(string id)
        {
            DBConnection testconn = new DBConnection();
            //see what is the current state of the page if it's alive or not
            string query = "SELECT isAlive from Pages where id =" + id;
            //this query will set a page isAlive to 0 
            string query2 = "UPDATE Pages SET isAlive = 0 WHERE id =" + id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            //if the page is NOT live, change the query and turn the page to live
            if (!(bool)dataReader.GetValue(0))
            {
                query2 = "UPDATE Pages SET isAlive = 1 WHERE id =" + id;
            }
            testconn.WriteToTest(query2);
            return RedirectToAction("ListPages", "Backend");
        }
        //for now this just redirects to the regular page i.e. SubPages Controller
        //I'm leaving it here in case we want to do something different with previews.
        public ActionResult PreviewPage(string id)
        {
            return RedirectToAction("Pages", "SubPages", new { id = id });
        }

        //retrieves all sections from db and returns in list form
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

        //Lists all sections
        public ActionResult ListSections()
        {
            ViewData["BackendPages"] = getBackendPages();
            var model = getSections();
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSection(string sectionid, string sectiontitle)
        {

            DBConnection testconn = new DBConnection();
            string query = "UPDATE Sections SET name = '" + sectiontitle + "' WHERE id = " + sectionid;
            testconn.WriteToTest(query);
            return RedirectToAction("ListSections", "Backend");
        }

        public ActionResult ToggleIsLiveSection(string id)
        {
            DBConnection testconn = new DBConnection();
            //see what is the current state of the section if it's alive or not
            string query = "SELECT isAlive from Sections where id =" + id;
            //this query will set a section isAlive to 0 
            string query2 = "UPDATE Sections SET isAlive = 0 WHERE id =" + id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            //if the section is NOT live, change the query and turn the page to live
            if (!(bool)dataReader.GetValue(0))
            {
                query2 = "UPDATE Sections SET isAlive = 1 WHERE id =" + id;
            }
            testconn.WriteToTest(query2);
            testconn.CloseConnection();
            return RedirectToAction("ListSections", "Backend");
        }

        public ActionResult CreateSection()
        {
            ViewData["BackendPages"] = getBackendPages();
            return View();
        }

        [HttpPost]
        public ActionResult CreateSection(string sectiontitle)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Sections (name, isAlive) VALUES ('" + sectiontitle + "', 1)";
            testconn.WriteToTest(query);
            testconn.CloseConnection();
            return RedirectToAction("ListSections", "Backend");
        }

        //this function returns the list of backend pages from the DB to populate the backend menu
        private List<BackendMenuModel> getBackendPages()
        {

            List<BackendMenuModel> backend_pages = new List<BackendMenuModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, controller, action, isLive FROM BackendPages where isLive = 1";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string controller = dataReader.GetValue(2).ToString();
                string action = dataReader.GetValue(3).ToString();
                bool isLive = (bool)dataReader.GetValue(4);
                BackendMenuModel backendPage = new BackendMenuModel(id, title, controller, action, isLive);
                backend_pages.Add(backendPage);
            }

            return backend_pages;
        }

        [HttpPost]
        public ActionResult RecoverPassword(string email)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, email from Users where email = '" + email+"'";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            if (dataReader.Read())
            {
                string id = dataReader.GetValue(0).ToString();
                string mail = dataReader.GetValue(1).ToString(); 
                RandomPassword(id);
                string message = "An email has been sent to " + mail + " with a new temporary password.";
                TempData["passwordmessage"] = message;
                return RedirectToAction("PasswordRecoveryMessage");
            }
            else
            {
                string message = "Email provided doesn't match any records.";
                TempData["passwordmessage"] = message;
                return RedirectToAction("PasswordRecoveryMessage");
            }

        }

        private void RandomPassword(string id)
        {
            string password = Membership.GeneratePassword(8, 3);
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Users SET password = '" + password + "' WHERE id = " + id;
            testconn.WriteToTest(query);
            testconn.CloseConnection();
        }


        public ActionResult PasswordRecoveryMessage()
        {
            ViewBag.Message = TempData["passwordmessage"].ToString();
            return View();
   
        }

        public ActionResult Test()
        {
            return View();
        }

    }


}
