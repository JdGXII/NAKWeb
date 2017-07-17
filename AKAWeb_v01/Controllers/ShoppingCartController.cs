using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AKAWeb_v01.Classes;

namespace AKAWeb_v01.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        public ActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddToCart(string product_id)
        {
            string userid = "";
            //if the user is logged in
            if (System.Web.HttpContext.Current.Session["userid"] != null)
            {
                userid = System.Web.HttpContext.Current.Session["userid"].ToString();
                DBConnection testconn = new DBConnection();
                string query = "INSERT INTO Cart(user_id, product_id) VALUES("+userid+","+product_id+")";
                testconn.WriteToTest(query);
                testconn.CloseConnection();
                return RedirectToAction("Cart");


            }
            //if not redirect him to log in
            else
            {
                return RedirectToAction("Index", "Backend");
            }
        }
    }
}