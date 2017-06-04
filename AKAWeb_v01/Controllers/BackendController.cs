using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
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

            string query = "Select name, email, password, access from Users Where email ='" + username + "' AND password ='" + password + "'";


            try
            {
                SqlDataReader dataReader;
                dataReader = testconn.ReadFromTest(query);
                dataReader.Read();


                if (dataReader.GetValue(1) != null)  //(username == this.username) && (password == this.password))
                {
                    System.Web.HttpContext.Current.Session["userpermission"] = dataReader.GetValue(3).ToString();
                    System.Web.HttpContext.Current.Session["username"] = dataReader.GetValue(0).ToString();

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
            List<SelectListItem> list = new List<SelectListItem>();
            int carouselnum = int.Parse(WebConfigurationManager.AppSettings["CarouselImageNumber"]);
            for (int i = carouselnum; i < 7; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Value = i.ToString();
                item.Text = i.ToString();

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


    }
}
