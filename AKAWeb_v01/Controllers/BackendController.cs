using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        //Uncomment only if you need to do bulk upload to db from an excel sheet
        //private ExcelService excel_service = new ExcelService();
        //this uses product service that does things like check what the low stock level alert number is
        private ProductService product_service = new ProductService();
       

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
                        System.Web.HttpContext.Current.Session["email"] = dataReader.GetValue(1).ToString();

                        //ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                        testconn.CloseDataReader();
                        testconn.CloseConnection();

                        login = true;
                      

                    }



                }



            }
            catch (Exception e)
            {
                testconn.CloseDataReader();
                testconn.CloseConnection();
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                login = false;
            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
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

        //returns a list of all the sections as a SelectListItem list
        //This is to fill dropdowns with the pertinent information in whatever view requires the sections in dropdown form
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
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return list;
        }

        //this returns a generic SelectListItem with productTypes to fill a Dropdown
        private List<SelectListItem> getProductTypes()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, name FROM Product_Type";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SelectListItem> list = new List<SelectListItem>();

            while (dataReader.Read())
            {
                SelectListItem item = new SelectListItem();
                item.Value = dataReader.GetValue(1).ToString();
                item.Text = dataReader.GetValue(1).ToString();

                list.Add(item);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return list;
        }

        //this overloads the getProductTypes it receives the type of a product
        //and sets that type as a default for the dropwdown
        private List<SelectListItem> getProductTypes(string product_type)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, name FROM Product_Type";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SelectListItem> list = new List<SelectListItem>();

            while (dataReader.Read())
            {
                SelectListItem item = new SelectListItem();
                item.Value = dataReader.GetValue(1).ToString();
                item.Text = dataReader.GetValue(1).ToString();
                if(dataReader.GetValue(1).ToString() == product_type)
                {
                    item.Selected = true;
                    
                }
                

                list.Add(item);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return list;
        }

        //this receives a sort number of a section, returns a list for a dropdown cotaining
        //the sort numbers for sections with the
        //sort number received as the default element
        private List<SelectListItem> getSectionSort(int sort_number)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT section_sortnumber FROM Section_Sorting";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<SelectListItem> list = new List<SelectListItem>();

            while (dataReader.Read())
            {
                SelectListItem item = new SelectListItem();
                item.Value = dataReader.GetValue(0).ToString();
                item.Text = dataReader.GetValue(0).ToString();
                if (Int32.Parse(dataReader.GetValue(0).ToString()) == sort_number)
                {
                    item.Selected = true;

                }


                list.Add(item);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
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


            dbconnect.CloseDataReader();
            dbconnect.CloseConnection();
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

                testconn.CloseDataReader();
                testconn.CloseConnection();
                return View(model);
            }
            else
                return RedirectToAction("MyProfile");
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


            testconn.CloseConnection();
            testconn.WriteToTest(query);
            
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
                    testconn.CloseDataReader();
                    testconn.CloseConnection();
                    //address already exists in db
                    return 1; //"already exists. Please try a different email address.";
                }
                else
                {
                    testconn.CloseDataReader();
                    testconn.CloseConnection();
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
                if(System.Web.HttpContext.Current.Session["userpermission"].ToString() == "3")
                {
                    ViewData["BackendPages"] = getBackendPages();
                    ViewBag.LowStock = alertLowStock();
                    return View();
                }
                else
                {
                    return View();
                }

            }
            else
            {
                return RedirectToAction("Index");
            }
            
        }

        //returns true if there is a live product with low stock, false if no live product has low stock
        //comparing its stock with the low_stock_alert number which is the minimum stock number
        private bool alertLowStock()
        {
            var products = getProducts();
            bool low_stock = false;
            foreach(ProductModel product in products)
            {
                if(product.isLive && product.stock <= product_service.low_stock_alert)
                {
                    low_stock = true;
                }
            }

            return low_stock;
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
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                if(TempData["pageDeletionSuccess"] != null)
                {
                    ViewBag.PageAlert = TempData["pageDeletionSuccess"].ToString();
                }


                if(TempData["PageCreation"] != null)
                {
                    ViewBag.PageAlert = TempData["PageCreation"].ToString();
                }

                ViewData["BackendPages"] = getBackendPages();
                var model = getPages();

                return View(model);
            }
            else
            {
                return RedirectToAction("MyProfile");
            }

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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return section;

        }

        public ActionResult ChangePageSection(string id)
        {
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                ViewData["BackendPages"] = getBackendPages();
                var model = getSection(id);
                ViewData["SectionList"] = GenerateViewBagList();
                PageModel page = getPage(Int32.Parse(id));
                ViewData["PageTitle"] = page.title;
                return View(model);
            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult ChangePageSection(string SectionList, string page_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Pages SET section=" + SectionList + " WHERE id =" + page_id;
            bool t = testconn.WriteToTest(query);


            testconn.CloseConnection();
            return RedirectToAction("ChangePageSection", new { id = page_id });
        }

        public ActionResult EditPage(string id)
        {
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                var model = getPage(Int32.Parse(id));
                if (model != null)
                {
                    ViewData["BackendPages"] = getBackendPages();
                    if(TempData["EditSuccess"] != null)
                    {
                        ViewBag.EditSuccess = TempData["EditSuccess"].ToString();
                    }
                    
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ListPages", "Backend");
                }

            }
            else
            {
                return RedirectToAction("MyProfile");
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
                        
                        

                    }
                    catch (Exception ex)
                    {
                        //ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        TempData["EditSuccess"] = "Something went wrong. Page was not saved.";
                    }

            }
            
            bool success = testconn.WriteToTest(query);
            if (success)
            {
                TempData["EditSuccess"] = "Page succesfully edited.";
            }
            else
            {
                TempData["EditSuccess"] = "Something went wrong. Page was not saved.";
            }

            testconn.CloseConnection();

            return RedirectToAction("EditPage", new { id = id});

        }

        public ActionResult CreatePage()
        {
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                ViewData["SectionList"] = GenerateViewBagList();
                ViewData["BackendPages"] = getBackendPages();
                return View();

            }
            else
            {
                return RedirectToAction("MyProfile");
            }

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

            bool success = testconn.WriteToTest(query);

            if (success)
            {
                TempData["PageCreation"] = "Page succesfully created.";
            }
            else
            {
                TempData["PageCreation"] = "Something went wrong. Page was not created.";
            }

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

 
            testconn.CloseConnection();
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
                //get the sort order from the Section_Sorting table
                StringBuilder sort_query = new StringBuilder("SELECT section_sortnumber FROM Section_Sorting WHERE section_id = ");
                sort_query.Append(id);
                SqlDataReader sortOrderReader = testconn.ReadFromTest(sort_query.ToString());
                sortOrderReader.Read();
                int sort_order = Int32.Parse(sortOrderReader.GetValue(0).ToString());

                List<SelectListItem> dropdown = getSectionSort(sort_order);
                SectionModel section = new SectionModel(id, title, isLive, sort_order, dropdown);
                sections.Add(section);

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            //sort sections
            List<SectionModel> sorted_sections = sections.OrderBy(s => s.sort_order).ToList();
            return sorted_sections;
        }

        //Lists all sections
        public ActionResult ListSections()
        {
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                ViewData["BackendPages"] = getBackendPages();
                var model = getSections();
                return View(model);

            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult EditSection(string sectionid, string sectiontitle, string sort_order)
        {

            DBConnection testconn = new DBConnection();
            string query = "UPDATE Sections SET name = '" + sectiontitle + "' WHERE id = " + sectionid;
            testconn.WriteToTest(query);
            testconn.CloseConnection();
            saveSectionSorting(sectionid, sort_order);
            return RedirectToAction("ListSections", "Backend");
        }

        //takes the id of a section and a sort number to sort the section
        //if the sort number is the one it already has it does nothing
        //if it's different it switches the section sort order with that number and
        private void saveSectionSorting(string sectionid, string sort_order)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT section_sortnumber FROM Section_Sorting WHERE section_id = "+sectionid;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            string sort_number = "";
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    sort_number = dataReader.GetValue(0).ToString();

                }
            }

            //if the section's current sort number is different, switch the numbers
            if(sort_number != sort_order)
            {
                query = "SELECT section_id FROM Section_Sorting WHERE section_sortnumber = " + sort_order;
                dataReader = testconn.ReadFromTest(query);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        string section_tobe_switched_id = dataReader.GetValue(0).ToString();
                        string first_update = "UPDATE Section_Sorting SET section_sortnumber = " + sort_order + " WHERE section_id = " + sectionid;
                        string second_update = "UPDATE Section_Sorting SET section_sortnumber = " + sort_number + " WHERE section_id = " + section_tobe_switched_id;

                        bool first_success = testconn.WriteToTest(first_update);
                        bool second_success = testconn.WriteToTest(second_update);

                    }
                }

            }

            testconn.CloseDataReader();
            testconn.CloseConnection();

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

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return RedirectToAction("ListSections", "Backend");
        }

        public ActionResult CreateSection()
        {
            String userpermission = "";
            if (System.Web.HttpContext.Current.Session["userpermission"] != null)
            {
                userpermission = System.Web.HttpContext.Current.Session["userpermission"] as String;
            }
            if (userpermission.Equals("3"))
            {
                ViewData["BackendPages"] = getBackendPages();
                return View();
            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult CreateSection(string sectiontitle)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Sections (name, isAlive) VALUES ('" + sectiontitle + "', 1)";
            int id = testconn.WriteToTestReturnID(query);
            query = "INSERT INTO Section_Sorting (section_id, section_sortnumber) VALUES(" + id.ToString() + "," + id.ToString() + ")";
            bool success = testconn.WriteToTest(query);
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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

                testconn.CloseDataReader();
                testconn.CloseConnection();
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

        //Generates a random password emails it to the user, then the password hash is set in the database
        //Receives the user's id whose password will be randomized
        private string RandomPassword(string id)
        {
            string password = Membership.GeneratePassword(8, 3);
            string hashedPassword = hash_service.HashPassword(password);
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Users SET password = '" + hashedPassword + "' WHERE id = " + id;

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
            EmailService email = new EmailService(emailmessage, mailTo, subject, false);
            bool success = email.sendEmail();

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

                testconn.CloseDataReader();
                testconn.CloseConnection();
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

                testconn.CloseDataReader();
                testconn.CloseConnection();
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
            bool result = testconn.WriteToTest(query);

            testconn.CloseConnection();
            return result;
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


            bool result = testconn.WriteToTest(query);

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return result;
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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
            bool result = testconn.WriteToTest(query);

            testconn.CloseConnection();
            return result;
        }

        //returns a product model (representing a product) by id
        private ProductModel getProduct(string product_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details FROM Products WHERE id = "+ product_id;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            ProductModel product = new ProductModel();
            if (dataReader.HasRows)
            {
                dataReader.Read();
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                product = new ProductModel(id, cost, type, description, length, true, details, null);
                
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return product;
        }

        //returns a list with all the products in the DB
        private List<ProductModel> getProducts()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details, isLive, image, stock FROM Products";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ProductModel> product_list = new List<ProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                bool isLive = (bool)dataReader.GetValue(6);
                string image = dataReader.GetValue(7).ToString();
                List<SelectListItem> dropdown = getProductTypes(type);
                int stock = Int32.Parse(dataReader.GetValue(8).ToString());
                ProductModel product = new ProductModel(id, cost, type, description, length, isLive, details, image, dropdown, stock);
                product_list.Add(product);
                

            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return product_list;
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

            testconn.CloseDataReader();
            testconn.CloseConnection();
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

        //get the list of images uploaded to the folder containing
        //the images for editing subpages and returns it
        private List<AKAWeb_v01.Models.ImageModel> getImageList()
        {
            List<AKAWeb_v01.Models.ImageModel> image_list = new List<ImageModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, title, url FROM SubPages_Images";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            //while there are records in the datareader
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string url = dataReader.GetValue(2).ToString();
           
                ImageModel image = new ImageModel(id, title, url);
                image_list.Add(image);

            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return image_list;


        }

        //action that makes the Image list from GetImaList() available as images for easy copy and paste
        public ActionResult ReturnImageList()
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                if (TempData["imageUploadSuccess"] != null)
                {
                    ViewBag.UploadSuccess = TempData["imageUploadSuccess"].ToString();
                }
                //this condition is to set an element of the viewbag with the
                //exception if failure to upload the image happens
                //it's for debugging purposes
                else if (TempData["imageUploadException"] != null)
                {
                    ViewBag.ShowImageUploadExcepcion = TempData["imageUploadException"].ToString();
                }
                ViewData["BackendPages"] = getBackendPages();
                var model = getImageList();
                return View(model);
            }
            else
            {
                return RedirectToAction("MyProfile");

            }

        }


        //method to upload images to subpages
        [HttpPost]
        public ActionResult UploadSubPageImage(string title)
        {

            
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                    try
                    {
                        var fileName = file.FileName;
                        var path = Path.Combine(Server.MapPath("~/Content/Images/SubPagesUploads"), fileName);
                        file.SaveAs(path);
                        string pathForDB = "~/Content/Images/SubPagesUploads/" + fileName.ToString();
                        DBConnection testconn = new DBConnection();
                        string query = "INSERT INTO SubPages_Images(title, url) VALUES('" + title + "', '" + pathForDB + "')";

                        testconn.WriteToTest(query);
                        testconn.CloseConnection();
                        TempData["imageUploadSuccess"] = "Image uploaded succesfully!";
                        



                    }
                    catch (Exception ex)
                    {
                        
                        TempData["imageUploadSuccess"] = "Something went wrong. Image did not upload.";
                        TempData["imageUploadException"] = ex;
                    }

            }

            return RedirectToAction("ReturnImageList");
        }

        //edits the title of an image.
        //title is NOT the name of the file, it is an attribute we use for models
        //not the file name.
        [HttpPost]
        public ActionResult EditSubPageImageTitle(string imageid, string imagetitle)
        {
            DBConnection testconn = new DBConnection();
            string query = "UPDATE SubPages_Images SET title = '"+ imagetitle + "' WHERE id = " + imageid;
            if (testconn.WriteToTest(query))
            {
                TempData["imageUploadSuccess"] = "Image title succesfully edited.";
            }
            else
            {
                TempData["imageUploadSuccess"] = "Something went wrong. Title not updated.";
            }

            testconn.CloseConnection();
            return RedirectToAction("ReturnImageList");
        }

        //deletes an image, both from the db and the file system
        public ActionResult DeleteSubPageImage(string id, string url)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                string query = "DELETE FROM SubPages_Images WHERE id = " + id;

                //get the path of the image from the server
                string path = Server.MapPath(url);

                //if file exists delete it
                if (System.IO.File.Exists(path))
                {
                    //delete from file system
                    System.IO.File.Delete(path);
                    //delete from DB

                   
                    TempData["imageUploadSuccess"] = "Image succesfully deleted.";


                }
                else
                {
                    TempData["imageUploadSuccess"] = "Something went wrong. Image not deleted.";
                }


                testconn.CloseConnection();
                return RedirectToAction("ReturnImageList");

            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        //this function returns a Json array to be fed into the text/html editor
        //to serve as a list for existing images for easier editing
        public JsonResult ImageJsonList()
        {
            /*var builder = new StringBuilder();
            builder.Append("[");
            foreach (var item in getImageList())
            {
                builder.Append("{\"title\": \"" + item.title + "\", \"value\": \"" + Server.MapPath(item.url) + "\"},");
                
            }
                
            var result = builder.ToString().TrimEnd(new char[] { ',', ' ' }) + "]";
            return result;*/
            //List<ImagePair> list = getImagesForJson();
            List<ImagePair> list = getImagesForJson();
            //list.Add(new ImagePair { title = "20150326_180444_resized.jpg", value = "/Images/Images/20150326_180444_resized.jpg" });
            //list.Add(new ImagePair { title = "20150326_180444_resized.jpg", value = "/Images/Images/20150326_180444_resized.jpg" });
            //ImagePair[] img = new ImagePair[2] { };
            return Json(list, JsonRequestBehavior.AllowGet);

        }

        private List<ImagePair> getImagesForJson()
        {

            List<AKAWeb_v01.Models.ImagePair> image_list = new List<ImagePair>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT title, url FROM SubPages_Images";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            //while there are records in the datareader
            while (dataReader.Read())
            {
                
                string title = dataReader.GetValue(0).ToString();
                string url = dataReader.GetValue(1).ToString();
                string value = Url.Content(url);
                //ImagePair image = new ImagePair(title, url);
                image_list.Add(new ImagePair { title = title, value = value });

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return image_list;

        }

        public ActionResult ListProducts()
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                if (TempData["productEditSuccess"] != null)
                {
                    ViewBag.ProductEditSuccess = TempData["productEditSuccess"].ToString();
                }
                else if (TempData["productCreationSuccess"] != null)
                {
                    ViewBag.ProductEditSuccess = TempData["productCreationSuccess"].ToString();
                }
                ViewData["BackendPages"] = getBackendPages();
                ViewData["ProductTypes"] = getProductTypes();
                ViewBag.LowStockNumber = product_service.low_stock_alert;
                var model = getProducts();
                return View(model);

            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult EditProduct (string cost, string type, string description, string length, string details, string productid, string stock)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                string query = "UPDATE Products SET cost = '" + cost + "', type= '" + type + "', description = '" + description + "', length = '" + length + "', details = '" + details + "', stock = "+stock+" WHERE id = " + productid;
                if (testconn.WriteToTest(query))
                {
                    TempData["productEditSuccess"] = "Product edited succesfully.";

                }
                else
                {
                    TempData["productEditSuccess"] = "Something went wrong, product did not update.";
                }

                testconn.CloseConnection();
                return RedirectToAction("ListProducts");

            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        public ActionResult ToggleIsLiveProduct(string id)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                //see what is the current state of the product if it's alive or not
                string query = "SELECT islive from Products where id =" + id;
                //this query will set a product isAlive to 0 
                string query2 = "UPDATE Products SET islive = 0 WHERE id =" + id;
                SqlDataReader dataReader = testconn.ReadFromTest(query);
                dataReader.Read();
                //if the product is NOT live, change the query and turn the product to live
                if (!(bool)dataReader.GetValue(0))
                {
                    query2 = "UPDATE Products SET islive = 1 WHERE id =" + id;
                }
                testconn.WriteToTest(query2);
                testconn.CloseDataReader();
                testconn.CloseConnection();
                return RedirectToAction("ListProducts", "Backend");
            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult CreateProductType(string prodtype)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                string query = "INSERT INTO Product_Type (name) VALUES('" + prodtype + "')";
                if (testconn.WriteToTest(query))
                {
                    TempData["productCreationSuccess"] = "Product Type successfully created!";
                }
                else
                {
                    TempData["productCreationSuccess"] = "Something went wrong. Product Type not created";
                }
                testconn.CloseDataReader();
                testconn.CloseConnection();
                return RedirectToAction("ListProducts");
            }
            else
            {
                return RedirectToAction("MyProfile");
            }
        }

        [HttpPost]
        public ActionResult CreateProduct(string cost, string ProductTypes, string description, string details, string duration, string stock)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                string query = "INSERT INTO Products(cost, type, description, details, length, isLive, stock) VALUES('" + cost + "', '" + ProductTypes + "', '" + description + "', '" + details + "', '" + duration + "', 0, "+stock+")";
                if (testconn.WriteToTest(query))
                {
                    TempData["productCreationSuccess"] = "Product successfully created!";
                }
                else
                {
                    TempData["productCreationSuccess"] = "Something went wrong. Product not created";
                }

                testconn.CloseConnection();
                return RedirectToAction("ListProducts");
            }
            else
            {
                return RedirectToAction("MyProfile");
            }

        }

        [HttpPost]
        public ActionResult DeletePage(string page_id)
        {
            if (System.Web.HttpContext.Current.Session["username"] != null)
            {
                DBConnection testconn = new DBConnection();
                string query = "DELETE FROM Pages WHERE id = "+page_id;
                if (testconn.WriteToTest(query))
                {
                    TempData["pageDeletionSuccess"] = "Page has been succesfully deleted";
                }
                else
                {
                    TempData["pageDeletionSuccess"] = "Something went wrong. Page wasn't deleted";
                }
                testconn.CloseDataReader();
                testconn.CloseConnection();
                return RedirectToAction("ListPages");
            }
            else
            {
                return RedirectToAction("MyProfile");
            }
        }

        [HttpGet]
        public async Task<ActionResult> CreateConference()
        {
            if (TempData["ConferenceCreationSuccess"] != null)
            {
                ViewBag.ConferenceCreationFeedback = TempData["ConferenceCreationSuccess"].ToString();
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetTicketsForConferencePartialView()
        {
            var model = await product_service.getTickets();
            
            return PartialView("_AsyncTickets", model);

        }

        [HttpGet]
        public async Task<ActionResult> GetTicketsForConferencePartialViewEdit(int conference_code)
        {
            var model = await product_service.getTickets();
            ViewBag.AlreadyBoundTickets = getAssociatedTickets(conference_code);
            return PartialView("_AsyncTickets", model);

        }

        /*[HttpGet]
        public async Task<ActionResult> GetAddOnsForConferencePartialView()
        {
            var model = await product_service.getConferenceAddOns();
            return PartialView("_AsyncTickets", model);

        }*/

        //creates conference from form in CreateConference view
        [HttpPost]
        public ActionResult CreateConference(ICollection<string> addon, ICollection<ProductModel> tickets, ConferenceModel conference, AddressModel location)
        {
            int conference_code = storeNewConference(conference);
            if(conference_code != -1)
            {
                bool tickets_success = bindTicketsToConference(conference_code, addon, tickets);
                bool location_success = bindAddressToConference(conference_code, location);

                if(tickets_success && location_success)
                {
                    TempData["ConferenceCreationSuccess"] = "Conference succesfully created";
                    return RedirectToAction("ListConferences");
                }
                else
                {
                    bool deletion_success = revertConferenceCreation(conference_code);
                    TempData["ConferenceCreationSuccess"] = "Something went wrong. Conference could not be created.";
                    return RedirectToAction("CreateConference");
                }
            }
            else
            {
                TempData["ConferenceCreationSuccess"] = "Something went wrong. Conference could not be created.";
                return RedirectToAction("CreateConference");
            }

            
        }

        //Creates/saves a new conference in the Conference DB
        //Step 1 of properly configuring a new conference
        //returns the conference code or -1 if the the creation process failed
        private int storeNewConference(ConferenceModel conference)
        {
            int members_only = 0;
            Random rnd = new Random();
            int conference_code = rnd.Next();
            if (conference.members_only)
            {
                members_only = 1;
            }
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Conference(title, tagline, external_url, start_date, end_date, processing_fee, max_attendees, attendees, members_only, isLive, conference_code)" +
                "VALUES('" + conference.title + "','" + conference.tagline + "','" + conference.external_url + "','" + conference.start_date + "','" + conference.end_date + "','" + conference.processing_fee + "'," + conference.max_attendees + ", 0," + members_only + ", 1, "+conference_code+")";
            bool success = testconn.WriteToTest(query);
            testconn.CloseConnection();
         

            if (success)
            {
                return conference_code;
            }
            else
            {
                return -1;

            }
  
     
        }

        //If anything goes wrong with the multiple database write operations this function is called and deletes the records
        //takes in the failed inserted conference  conference_code
        private bool revertConferenceCreation(int conference_code)
        {
            DBConnection testconn = new DBConnection();
            string delete_location = "DELETE FROM Conference_Has_Location WHERE conference_code = " + conference_code.ToString();
            string delete_products = "DELETE FROM Conference_Has_Product WHERE conference_code = " + conference_code.ToString();
            string delete_conference = "DELETE FROM Conference WHERE conference_code = " + conference_code.ToString();

            bool success_location = testconn.WriteToTest(delete_location);
            bool success_products = testconn.WriteToTest(delete_products);
            bool success_conference = testconn.WriteToTest(delete_conference);

            testconn.CloseConnection();
            return success_conference && success_location && success_products;
        }

        //takes in the code of a conference a collection of ticket/product ids and saves them to Conference_Has_Product
        //also sets isLive to true on Products table and updates the tickets
        private bool bindTicketsToConference(int conference_code, ICollection<string> ticket_ids, ICollection<ProductModel> tickets)
        {
            bool success_insert = true;
            bool success_update = updateTicketsFromConference(ticket_ids, tickets);
            bool success = true;
            DBConnection testconn = new DBConnection();
            //first delete tickets (in case the function is called from an edit) then insert the selected tickets
            string delete_query = "DELETE FROM Conference_Has_Product WHERE conference_code = " + conference_code.ToString();
            testconn.WriteToTest(delete_query);

            foreach (string ticket_id in ticket_ids)
            {
                string insert = "INSERT INTO Conference_Has_Product(conference_code, product_id)" +
                "VALUES(" + conference_code + "," + ticket_id + ")";
                
                success_insert = testconn.WriteToTest(insert);
               
                success = (success_update && success_insert);
                if (!success)
                {

                    testconn.CloseConnection();
                    return success;
                }
                
            }

            testconn.CloseConnection();
            return success;
            
            

        }

        //sets conference location
        private bool bindAddressToConference(int conference_code, AddressModel address)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Conference_Has_Location(state, city, street_address, zip, conference_code) VALUES('" + address.state + "'," +
"'" + address.city + "','" + address.street_address + "', '" + address.zip + "', " + conference_code + ")";
            bool success = testconn.WriteToTest(query);
            
            testconn.CloseConnection();

            return success;


        }

        //This action gets called asynchronously from the CreateConference View to create a new ticket type product from that page
        [HttpPost]
        public string CreateTicketConference(string cost, string description, string details, string duration)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Products(cost, type, description, details, length, isLive, stock) VALUES('" + cost + "', 'Ticket', '" + description + "', '" + details + "', '" + duration + "', 0, 100000)";
            bool success = testconn.WriteToTest(query);
            testconn.CloseConnection();
            if (success)
            {

                return "Success";
            }
            else
            {

                return "Fail";
            }

        }

        public ActionResult ListConferences()
        {
            
            if (TempData["ConferenceCreationSuccess"] != null)
            {
                ViewBag.ConferenceCreationFeedback = TempData["ConferenceCreationSuccess"].ToString();
            }

            var model = getConferences();
            return View(model);
        }

        private List<ConferenceModel> getConferences()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT title, start_date, end_date, isLive, conference_code FROM Conference";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ConferenceModel> conferences = new List<ConferenceModel>();
            if (dataReader.HasRows)
            {

                while (dataReader.Read()) { 
              
                    string title = dataReader.GetValue(0).ToString();
                    string start_date = dataReader.GetValue(1).ToString();
                    string end_date = dataReader.GetValue(2).ToString();
                    bool isLive = (bool)dataReader.GetValue(3);
                    int conference_code = Int32.Parse(dataReader.GetValue(4).ToString());

                    ConferenceModel conference = new ConferenceModel(title, start_date, end_date, isLive, conference_code);
                    conferences.Add(conference);

                }

            }
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return conferences;
        }

        //get the associated tickets/products associated to a conference
        private List<ProductModel> getAssociatedTickets(int conference_code)
        {
            DBConnection testconn = new DBConnection();
            List<ProductModel> tickets = new List<ProductModel>();
            string conference_tickets = "SELECT product_id from Conference_Has_Product WHERE conference_code = " + conference_code.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(conference_tickets);

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    int product_id = Int32.Parse(dataReader.GetValue(0).ToString());
                    ProductModel ticket = getProduct(product_id.ToString());
                    tickets.Add(ticket);


                }

            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return tickets;

        }

        //get the location associated to a conference
        private AddressModel getAssociatedLocation(int conference_code)
        {
            DBConnection testconn = new DBConnection();
            AddressModel location = new AddressModel(); 
            string conference_location = "SELECT city, state, zip, street_address FROM Conference_Has_Location WHERE conference_code = " + conference_code.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(conference_location);

            dataReader = testconn.ReadFromTest(conference_location);

            if (dataReader.HasRows)
            {
                dataReader.Read();
                string city = dataReader.GetValue(0).ToString();
                string state = dataReader.GetValue(1).ToString();
                string zip = dataReader.GetValue(2).ToString();
                string street_address = dataReader.GetValue(3).ToString();

                location = new AddressModel(" ", state, city, zip, street_address);
            }

            testconn.CloseDataReader();
            testconn.CloseConnection();
            return location;

        }



        //get a specific conference from conference code
        private ConferenceModel getConference(int conference_code)
        {
            ConferenceModel conference = new ConferenceModel();
            List<ProductModel> tickets = getAssociatedTickets(conference_code);
            AddressModel location = getAssociatedLocation(conference_code);
            DBConnection testconn = new DBConnection();
            string conference_info = "SELECT id, title, tagline, external_url, start_date, end_date, processing_fee, max_attendees, attendees, members_only, isLive, conference_code FROM Conference WHERE conference_code =" + conference_code.ToString();
 

            SqlDataReader dataReader = testconn.ReadFromTest(conference_info);

            if (dataReader.HasRows)
            {
                //id, title, tagline, external_url, start_date, end_date, processing_fee, max_attendees, attendees, members_only, isLive, conference_code
                dataReader.Read();
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string title = dataReader.GetValue(1).ToString();
                string tagline = dataReader.GetValue(2).ToString();
                string external_url = dataReader.GetValue(3).ToString();
                string start_date = dataReader.GetValue(4).ToString();
                string end_date = dataReader.GetValue(5).ToString();
                string processing_fee = dataReader.GetValue(6).ToString();
                int max_attendees = Int32.Parse(dataReader.GetValue(7).ToString());
                int attendees = Int32.Parse(dataReader.GetValue(8).ToString());
                bool members_only = (bool)dataReader.GetValue(9);
                bool isLive = (bool)dataReader.GetValue(10);
                int conf_code = Int32.Parse(dataReader.GetValue(11).ToString());

                conference = new ConferenceModel(id, title, tagline, external_url, start_date, end_date, processing_fee, max_attendees, attendees, members_only, isLive, tickets, conf_code, location);



            }


            testconn.CloseDataReader();
            testconn.CloseConnection();
            return conference;
            
        }

        public ActionResult ToggleIsLiveConference(int id)
        {


            DBConnection testconn = new DBConnection();
            //see what is the current state of the product if it's alive or not
            string query = "SELECT isLive from Conference where conference_code =" + id.ToString();
            //this query will set a product isAlive to 0 
            string query2 = "UPDATE Conference SET isLive = 0 WHERE conference_code =" + id.ToString();
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            //if the product is NOT live, change the query and turn the product to live
            if (!(bool)dataReader.GetValue(0))
            {
                query2 = "UPDATE Conference SET isLive = 1 WHERE conference_code =" + id.ToString();
            }
            testconn.WriteToTest(query2);
            testconn.CloseDataReader();
            testconn.CloseConnection();
            return RedirectToAction("ListConferences", "Backend");

            

        }

        public ActionResult EditConference(int id)
        {
            if (TempData["ConferenceUpdateSuccess"] != null)
            {
                ViewBag.EdiSuccess = TempData["ConferenceUpdateSuccess"].ToString();
            }
            var model = getConference(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditConference(ICollection<string> addon, ICollection<ProductModel> tickets, ConferenceModel conference, AddressModel location)
        {
            bool update_conference_success = updateConference(conference);
            bool tickets_success = bindTicketsToConference(conference.conference_code, addon, tickets);
            bool location_success = bindAddressToConference(conference.conference_code, location);
            if (tickets_success && location_success && update_conference_success)
            {
                TempData["ConferenceUpdateSuccess"] = "Conference succesfully edited";
                return RedirectToAction("EditConference", conference.conference_code);

            }
            else
            {
                TempData["ConferenceUpdateSuccess"] = "Something went wrong. Conference could not be updated.";
                return RedirectToAction("EditConference", conference.conference_code); 
            }

        }

        private bool updateConference(ConferenceModel conference)
        {
            DBConnection testconn = new DBConnection();
            int members_only = 0;
            if (conference.members_only)
            {
                members_only = 1;
            }
            string query = "UPDATE Conference SET title = '" + conference.title + "', tagline = '" + conference.tagline + "', external_url = '" + conference.external_url + "',start_date= '" + conference.start_date + "', end_date = '" + conference.end_date + "', processing_fee= '" + conference.processing_fee + "', max_attendees =" + conference.max_attendees + ", members_only =" + members_only.ToString()+" WHERE conference_code = "+conference.conference_code.ToString();
            bool success = testconn.WriteToTest(query);

            testconn.CloseConnection();
            return success;


        }

        private bool updateConferenceLocation(int conference_code, AddressModel address)
        {
            DBConnection testconn = new DBConnection();
            string query = "UPDATE Conference_Has_Location SET state = '" + address.state + "', city = '" + address.city + "', street_address = '" + address.street_address + "', zip = '" + address.zip + "' WHERE conference_code = " + conference_code;
            bool success = testconn.WriteToTest(query);

            testconn.CloseConnection();

            return success;

        }

        //addon represents a collection with the ids of products/tickets that were marked to be associated/bound to a conference
        //since the conference tool allows for the update of old tickets or creation of new ones it needs to handle that
        //so this function takes the list of selected tickets in addon and updates to the DB the tickets in the tickets Collection that have the same id
        //it also always changes isLive to 1
        private bool updateTicketsFromConference(ICollection<string> addon, ICollection<ProductModel> tickets)
        {
            DBConnection testconn = new DBConnection();
            List<ProductModel> shell_tickets = new List<ProductModel>();
            bool success = true;
            foreach (string id in addon)
            {
                ProductModel shell_ticket = new ProductModel();
                shell_ticket.id = Int32.Parse(id);
                shell_tickets.Add(shell_ticket);
                
            }

            foreach(ProductModel ticket in tickets)
            {
                if (shell_tickets.Contains(ticket))
                {
                    string update_ticket = "UPDATE Products SET description = '" + ticket.description + "', details = '" + ticket.details + "', cost = '" + ticket.cost + "', isLive = 1 WHERE id = " + ticket.id.ToString();
                    success = testconn.WriteToTest(update_ticket);

                    if (!success)
                    {
                        testconn.CloseConnection();
                        return false;
                    }
                }
            }

            testconn.CloseConnection();
            return success;

        }


        public ActionResult Test()
        {
            //ViewBag.List = GetImageList();
            return View();
        }

    }


}
