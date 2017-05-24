using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace AKAWeb_v01.Controllers
{
    public class BackendController : Controller
    {
        private string username = "admin";
        private string password = "admin";

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string cnnString = WebConfigurationManager.ConnectionStrings["Test"].ToString();
            SqlConnection cnn;
            SqlCommand command;
            cnn = new SqlConnection(cnnString);
            string query = "Select name, email, password, access from Users Where email ='"+username+"' AND password ='"+password+"'";
            SqlDataReader dataReader;

            try
            {
                cnn.Open();
                command = new SqlCommand(query, cnn);
                dataReader = command.ExecuteReader();            
                dataReader.Read();
                

                if (dataReader.GetValue(1) != null)  //(username == this.username) && (password == this.password))
                {
                    System.Web.HttpContext.Current.Session["userpermission"] = dataReader.GetValue(3).ToString();
                    System.Web.HttpContext.Current.Session["username"] = dataReader.GetValue(0).ToString();

                    //ViewData["sessionString"] = System.Web.HttpContext.Current.Session["userpermission"];
                    
                    return RedirectToAction("EditCarousel");

                }
                else
                {
                    ViewBag.Message = "Wrong Credentials";
                    return RedirectToAction("Index");
                }

            }
            catch (Exception e)
            {
                //System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                return RedirectToAction("Index");
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
        public ActionResult ChangeCarouselNumber(String number)
        {
            ConfigurationManager.AppSettings["CarouselImageNumber"] = number;
            var d = ConfigurationManager.AppSettings["CarouselImageNumber"];
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
                ViewBag.CarouselImageNumber = WebConfigurationManager.AppSettings["CarouselImageNumber"];
                ViewBag.CarouselDropdown = this.GenerateViewBagList();
        
                return View();
            }
            else
                return RedirectToAction("Index");
        }
        // GET: Backend
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(int picnum)
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
                }
                catch(Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }

            return RedirectToAction("EditCarousel");
        }

        private String PictureName (int picnum)
        {
            return "carouselpicture" + picnum.ToString()+ ".jpg";
        }

        public ActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Test(HttpPostedFileBase file)
        {



            if (file != null && file.ContentLength > 0)
                try
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/CarouselUploads"), fileName);
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }


            return View();
        }

    }
}
