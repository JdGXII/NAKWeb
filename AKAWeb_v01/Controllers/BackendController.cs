using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using AKAWeb_v01.Classes;
using AKAWeb_v01.Models;
using reCAPTCHA.MVC;

namespace AKAWeb_v01.Controllers
{
    public class BackendController : Controller
    {
        //private string username = "admin";
        //private string password = "admin";
        private HashService hash_service = new HashService();

        //perform the actual login steps
        //returns a boolean true if login was a success, false if it was not.
        private bool doLogin(string email, string password)
        {
            bool login = false;
            DBConnection testconn = new DBConnection();

            string query = "Select name, email, id, access, password from Users Where email ='" + email + "'";


            try
            {
                SqlDataReader dataReader;
                dataReader = testconn.ReadFromTest(query);


                //if email exists in db
                if (dataReader.Read())  
                {
                    //get password from db where it is hashed
                    string hashedPassword = dataReader.GetValue(4).ToString();
                    //if password matches, login is succesful
                    if(hash_service.VerifyPassword(hashedPassword, password))
                    {
                        System.Web.HttpContext.Current.Session["userpermission"] = dataReader.GetValue(3).ToString();
                        System.Web.HttpContext.Current.Session["username"] = dataReader.GetValue(0).ToString();
                        System.Web.HttpContext.Current.Session["userid"] = dataReader.GetValue(2).ToString();

                        //ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                        testconn.CloseDataReader();
                        testconn.CloseConnection();

                        login = true;
                      

                    }



                }


            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                login = false;
            }

            return login;

        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {



            bool login = doLogin(username, password);

                if (login)  //(username == this.username) && (password == this.password))
                {

                    return RedirectToAction("MyProfile");

                }
                else
                {

                    TempData["failedlogin"] = "Sign in failed. Password or email not recognized.";
                    return RedirectToAction("Index");
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
            //check if coming back from a failed login
            if(TempData["failedlogin"] != null)
            {
                ViewBag.LoginFailed = TempData["failedlogin"].ToString();
                return View();
            }
            else
            {
                return View();
            }
            
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
            if(TempData["captchafailed"]  != null)
            {
                ViewBag.Captchafail = TempData["captchafailed"].ToString();
                return View();
            }
            else
            {
                return View();
            }
            
        }

        //asynchronously validate the register form to enable the register button
        [HttpPost]
        public string validateRegisterForm(string name, string email, string password)
        {
            string form = "false";
            //if name and password are not empty and email return code is true (meaning no problems with email address)
            if(validateNotEmpty(name) && validateNotEmpty(password) && (validateEmail(email) == 2))
            {
                //form is true/valid
                form = "true";
            }

            return form;

        }

        //action that sees if an email is valid and returns a message to be rendered by the view if it is not valid
        [HttpPost]
        public string checkEmail(string email)
        {
            int validation = validateEmail(email);
            //if email is valid, don't return an error message
            if(validation == 2)
            {
                return "";
            }
            else if(validation == 1)
            {
                return "already exists. Please try a different email address.";
            }
            else if(validation == 3)
            {
                return "format not accepted. Please input a valid email address.";
            }
            else
            {
                return "";
            }
        }

        //check if name is valid/not empty and returns a message if there's something wrong
        [HttpPost]
        public string checkName(string name)
        {
            //if it's not empty, it's ok
            if (validateNotEmpty(name))
            {
                return "";
            }
            else
            {
                return "can't be empty.";
            }
        }

        //check if password is valid and returns a message
        [HttpPost]
        public string checkPassword(string password)
        {
            //if it's not empty, it's ok
            if (validateNotEmpty(password))
            {
                return "";
            }
            else
            {
                return "can't be empty.";
            }
        }

        //checks if a string is not empty, returns true if true, false if empty
        private bool validateNotEmpty(string str)
        {
            if(String.IsNullOrEmpty(str))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //validates that an email is not already in the DB or that it has an email format
        private int validateEmail(string email)
        {
            if (email.Contains("@"))
            {


                DBConnection testconn = new DBConnection();
                string query = "SELECT id FROM Users WHERE email = '" + email + "'";
                SqlDataReader dataReader = testconn.ReadFromTest(query);
                if (dataReader.Read())
                {
                    //address already exists in db
                    return 1; //"already exists. Please try a different email address.";
                }
                else
                {
                    //everything ok
                    return 2;
                }

            }
            else
            {
                //problem with format missing @ sign
                return 3; //"format not accepted. Please input a valid email address.";
            }
        }


        [HttpPost]
        [CaptchaValidator]
        public ActionResult RegisterUser(string name, string email, string password, bool captchaValid)
        {
            if (captchaValid)
            {
                if((validateRegisterForm(name, email, password) == "true"))
                {
                    //hash password before inserting in db
                    string hashed_password = hash_service.HashPassword(password);
                    DBConnection testconn = new DBConnection();
                    string query = "INSERT INTO Users (name, email, password, access) VALUES ('" + name + "', '" + email + "', '" + hashed_password + "',  1)";
                    testconn.WriteToTest(query);
                    testconn.CloseConnection();

                    //email message
                    string message = "Thank you for registering with an account.";
                    //send registration confirmation email.
                    sendEmail(message, email, "AKA Registration");

                    bool login = doLogin(email, password);
                    if (login)
                    {
                        return RedirectToAction("AccountInfo", "Backend");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Backend");
                    }

                    

                }
                else
                {
                    TempData["captchafailed"] = "There was something wrong with something you input. Please try again.";
                    return RedirectToAction("Register");
                }

            }
            else
            {
                TempData["captchafailed"] = "We couldn't properly validate you are human. Please try again.";
                return RedirectToAction("Register");
            }
        }


        public ActionResult MyProfile()
        {
            if(System.Web.HttpContext.Current.Session["username"] != null)
            {
                ViewData["BackendPages"] = getBackendPages();
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
            
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

        //this function returns a list with the MyProfile pages
        //it uses a backendmenumodel because the information is very similar, but source is different
        private List<BackendMenuModel> getMyProfilePages()
        {

            List<BackendMenuModel> backend_pages = new List<BackendMenuModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, controller, action, isLive FROM MyProfilePages where isLive = 1";
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

        //Checks if the email the user provided exists
        //If it does it sets messages for the email and website calls the RandomPassword function and
        //Sends the email with the password
        //If the email is not recognized a message with the notice of failure is set
        [HttpPost]
        public ActionResult RecoverPassword(string email)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, email from Users where email = '" + email+"'";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            //if email exists
            if (dataReader.Read())
            {
                string id = dataReader.GetValue(0).ToString();
                string mail = dataReader.GetValue(1).ToString(); 
                string password = RandomPassword(id);
                //message for the website
                string webmessage = "An email has been sent to " + mail + " with a new temporary password.";
                //message for email
                string emailmessage = setEmailString(password);
                string subject = "Password Recovery";
                //this forwards the webmessage to the view that will display it and can then access it
                TempData["passwordmessage"] = webmessage;
                
                //send the email with the new password
                sendEmail(emailmessage, mail, subject);
                return RedirectToAction("PasswordRecoveryMessage");
            }
            else
            {
                //message for the website. No email message is necessary
                string message = "Email provided doesn't match any records.";
                //this forwards the webmessage to the view that will display it and can then access it
                TempData["passwordmessage"] = message;
                return RedirectToAction("PasswordRecoveryMessage");
            }

        }

        //Generates a random password and sets in the database
        //Receives the user's id whose password will be randomized
        private string RandomPassword(string id)
        {
            string password = Membership.GeneratePassword(8, 3);
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Users SET password = '" + password + "' WHERE id = " + id;
            testconn.WriteToTest(query);
            testconn.CloseConnection();
            return password;
        }


        public ActionResult PasswordRecoveryMessage()
        {
            ViewBag.Message = TempData["passwordmessage"].ToString();
            return View();
   
        }

        //Builds the message for the email containing the random password
        //the user requested when recovering his password
        private string setEmailString(string password)
        {
            string message = "Your new temporary password is: " +
                            password + 
                            " You can set a new password from your MyProfile page after signing in.";
            return message;
        }

        //function to send email, takes in the message, the email address to mail to and subject
        private void sendEmail(string emailmessage, string mailTo, string subject)
        {
            try
            {
                string password = WebConfigurationManager.AppSettings.Get("emailPassword");
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("jralzaibar@gmail.com");
                mail.To.Add(mailTo);
                mail.Subject = subject;
                mail.Body = emailmessage;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("jralzaibar", password);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
               
            }
            catch (Exception ex)
            {
                //set exception in session for debugging purposes
                System.Web.HttpContext.Current.Session["mailfail"] = ex;
            }
        }

        //returns View for Account info, can only be accessed if user is logged in
        public ActionResult AccountInfo()
        {
            //we're checking if the user has logged in and there's a session variable set
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                string user_id = System.Web.HttpContext.Current.Session["userid"].ToString();
                var model = getUserModel(user_id);
                //if coming from an update operation and there's a message, set it to viewbag
                if(TempData["updatedresult"] != null)
                {
                    ViewBag.Result = TempData["updatedresult"].ToString();
                }
                //pass information for left menu
                ViewData["MyProfilePages"] = getMyProfilePages();
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        //gets the information from one user given the user's id and returns it as a UserModel object
        //it assumes the user will be found because there's already been a
        //login and user verification process
        //so the id being passed is valid
        private UserModel getUserModel(string user_id)
        {
            DBConnection testconn = new DBConnection();
            //get user by id
            string query = "SELECT id, name, email, password, access FROM Users WHERE id = " + user_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            if (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string name = dataReader.GetValue(1).ToString();
                string email = dataReader.GetValue(2).ToString();
                string password = dataReader.GetValue(3).ToString();
                int access = Int32.Parse(dataReader.GetValue(4).ToString());
                AddressModel address = getAddressModel(user_id);

                UserModel user = new UserModel(id, name, email, password, access, address);

                return user;
            }
            else
            {
                return new UserModel();
            }

        }

        //returns an AddressModel object given a user id for the address we're looking for. 
        private AddressModel getAddressModel(string user_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT country, state, city, street_address, zip FROM User_has_Address WHERE user_id =" + user_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            if (dataReader.Read())
            {
                string country = dataReader.GetValue(0).ToString();
                string state = dataReader.GetValue(1).ToString();
                string city = dataReader.GetValue(2).ToString();
                string street_address = dataReader.GetValue(3).ToString();
                string zip = dataReader.GetValue(4).ToString();

                AddressModel address = new AddressModel(country, state, city, zip, street_address);
                return address;
            }
            else
            {
                AddressModel address = new AddressModel();
                return address;
            }
        }


        [HttpPost]
        public ActionResult UpdateProfile(string name, string email, string country, string state, string city, string street_address, string zip, string id)
        {
            bool updateUserInfo = UpdateUserInfo(name, email, id);
            
            bool updateUserAddress = UpdateUserAddress(country, state, city, street_address, zip, id);
            
            if (updateUserInfo && updateUserAddress)
            {
                //set success message before redirecting
                TempData["updatedresult"] = "Information succesfully updated";
                return RedirectToAction("AccountInfo");
            }
            else
            {
                //set failure message before redirecting
                TempData["updatedresult"] = "Something went wrong. Information was not updated";
                return RedirectToAction("AccountInfo");
            }
        }

        //Updates the user's info, right now just name and email
        //Password update has its own function
        private bool UpdateUserInfo(string name, string email, string user_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Users SET name = '" + name + "', email = '" + email + "' WHERE id = " + user_id;

            return testconn.WriteToTest(query);
        }

        //Updates user address, takes in relevant address info and a user id
        private bool UpdateUserAddress(string country, string state, string city, string street_address, string zip, string user_id)
        {
            DBConnection testconn = new DBConnection();
            string check_user_exists = "SELECT user_id FROM User_Has_Address  WHERE user_id = " + user_id;
            SqlDataReader dataReader = testconn.ReadFromTest(check_user_exists);
            string query = "";
            if (dataReader.Read())
            {
                query = "UPDATE User_Has_Address SET country = '" + country + "', state = '" + state + "'," +
    "city = '" + city + "', street_address = '" + street_address + "', zip = '" + zip + "' WHERE user_id = " + user_id;
            }
            else
            {
                query = "INSERT INTO User_Has_Address (country, state, city, street_address, zip, user_id) VALUES('" + country + "', '" + state + "'," +
"'" + city + "','" + street_address + "', '" + zip + "', " + user_id+")";
            }



            return testconn.WriteToTest(query);
        }

        //Returns view with form to UpdatePassword
        //Assumes user has logged in
        public ActionResult UpdatePassword()
        {
            //we're checking if the user has logged in and there's a session variable set
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                string user_id = System.Web.HttpContext.Current.Session["userid"].ToString();
                var model = getUserModel(user_id);
                //if password update message has been set, add it to viewbag
                if(TempData["passwordupdatemessage"] != null)
                {
                    ViewBag.Result = TempData["passwordupdatemessage"].ToString();
                }

                ViewData["MyProfilePages"] = getMyProfilePages();
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public ActionResult UpdatePassword(string current_password, string new_password, string id)
        {
            //check if current_password is same as one stored in DB
            bool passwordMatch = MatchPassword(current_password, id);

            //if passwords matched
            if (passwordMatch)
            {
                //call function that updates password
                bool success = UpdateUserPassword(new_password, id);
                //if operation is a success set success message
                if (success)
                {
                    TempData["passwordupdatemessage"] = "Password has been succesfully updated.";
                }
                else
                {
                    TempData["passwordupdatemessage"] = "Something went wrong. Password not updated.";
                }
                return RedirectToAction("UpdatePassword");
            }
            else
            {
                TempData["passwordupdatemessage"] = "Password not updated. Current Password field did not match with account's records.";
                return RedirectToAction("UpdatePassword");
            }

        }

        //checks if submitted password matches current stored password and returns true or false
        private bool MatchPassword(string submitted_password, string user_id)
        {
            bool match = false;
            DBConnection testconn = new DBConnection();
            string query = "SELECT password from Users WHERE id = " + user_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            if (dataReader.Read())
            {
                string hashedPasswordFromDB = dataReader.GetValue(0).ToString();

                //true if they match, false if they don't
                match = hash_service.VerifyPassword(hashedPasswordFromDB, submitted_password);
            }

            return match;
        }

        //function that actually performes the password update 
        private bool UpdateUserPassword(string new_password, string user_id)
        {
            DBConnection testconn = new DBConnection();
            //hashes new password
            string hashNewPassword = hash_service.HashPassword(new_password);
            //saved hashed password to db
            string query = "UPDATE Users SET password = '" + hashNewPassword + "' WHERE id = " + user_id;

            //if the Write function was succesful returns true, false if otherwise
            return testconn.WriteToTest(query);
        }

        //returns a product model (representing a product) by id
        private ProductModel getProduct(string product_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details FROM Products WHERE isLive = 1 AND id = "+ product_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            ProductModel product = new ProductModel();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                product = new ProductModel(id, cost, type, description, length, true, details, null);
                
            }

            return product;
        }

        //returns a userhasproduct model list of products purchased by a user
        //it takes a user id and retrieves the products he owns from the db
        //this functions is meant to be called for actions that occur once the user has logged in
        //in that sense  it assumes validation has already happened and the id being passed is valid
        private List<UserHasProductModel> getUserHasProducts(string user_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, user_id, product_id, product_start, product_end, isValid FROM User_Has_Product WHERE user_id = " + user_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<UserHasProductModel> userhasproducts = new List<UserHasProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string userid = dataReader.GetValue(1).ToString();
                string product_id = dataReader.GetValue(2).ToString();
                string product_start = dataReader.GetValue(3).ToString();
                string product_end = dataReader.GetValue(4).ToString();
                bool isValid = (bool)dataReader.GetValue(5);

                UserModel user = getUserModel(userid);
                ProductModel product = getProduct(product_id);

                UserHasProductModel userhasproduct = new UserHasProductModel(id, user, product, product_start, product_end, isValid);
                userhasproducts.Add(userhasproduct);
            }

            return userhasproducts;
        }

        //populates the purchase history view
        public ActionResult PurchaseHistory()
        {
            //check if user is logged in, if not, redirect them to login
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                string user_id = System.Web.HttpContext.Current.Session["userid"].ToString();
                var model = getUserHasProducts(user_id);
                ViewData["MyProfilePages"] = getMyProfilePages();
                return View(model);

            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public ActionResult Test()
        {
            return View();
        }

    }


}
