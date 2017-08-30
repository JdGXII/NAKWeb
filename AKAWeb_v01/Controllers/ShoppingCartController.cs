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
    public class ShoppingCartController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Cart");
        }
        // GET: ShoppingCart
        public ActionResult Cart()
        {
            if(System.Web.HttpContext.Current.Session["userid"] != null)
            {
                var model = getModel();
                return View(model);
            }
            else
            {
                return View();
            }

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

                if(TempData["paid"] != null)
                {
                    ViewBag.Done = TempData["paid"].ToString();
                }

                return RedirectToAction("Cart");


            }
            //if not redirect him to log in
            else
            {
                return RedirectToAction("Index", "Backend");
            }
        }

        //this function removes an item from the Cart by deleting the entry
        //from the DB by matching the id parameter with the id in the DB
        
        public ActionResult RemoveFromCart(int id)
        {
            DBConnection testconn = new DBConnection();
            string query = "DELETE FROM Cart WHERE id = " + id.ToString();
            testconn.WriteToTest(query);
            return RedirectToAction("Cart");
        }

        //This function returns all the items in someone's cart from the Cart table.
        //it assumes it will only be called if the user has logged in.
        //Verification for that should come from the method that calls this function.
        private List<CartModel> getCartItems()
        {
            //gets the user id from the session which we assume exists
            string userid = System.Web.HttpContext.Current.Session["userid"].ToString();
            List<CartModel> cart_list = new List<CartModel>();
            DBConnection testconn = new DBConnection();
            string query = "SELECT c.id, c.user_id, c.product_id, p.description, p.cost from Cart c, Products p WHERE p.id = product_id AND c.user_id = "+userid;
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                int user_id = Int32.Parse(dataReader.GetValue(1).ToString());
                int product_id = Int32.Parse(dataReader.GetValue(2).ToString());
                string product_description = dataReader.GetValue(3).ToString();
                string product_cost = dataReader.GetValue(4).ToString();
                CartModel cart = new CartModel(id, user_id, product_id, product_description, product_cost);
                cart_list.Add(cart);

            }
            testconn.CloseConnection();
            return cart_list;

        }

        //finishes the purchasing process after payment has gone through
        //Assumes user has been logged in
        [HttpPost]
        public string Purchase()
        {
            approveProductsForUser();
            generateInvoice();
            updateStock();
            deleteFromCart();
            return "Purchase complete";
        }

        //updates the product stock, decreasing the current stock by one unit
        private bool updateStock()
        {
            DBConnection testconn = new DBConnection();
            List<CartModel> cart = getCartItems();
            bool success = true;
            foreach (CartModel item in cart)
            {
                string user_id = item.user_id.ToString();
                string product_id = item.product_id.ToString();

                string query = "UPDATE Products SET stock = ((SELECT stock from Products WHERE id = " + product_id + ") -1) WHERE id = " + product_id;


                
                bool flag = testconn.WriteToTest(query);
                //if flag is false the stock for one of the items was not complete and function will return false
                if (!flag)
                {
                    success = false;
                }
            }

            return success;
        }

        //deletes from cart
        private bool deleteFromCart()
        {
            string userid = System.Web.HttpContext.Current.Session["userid"].ToString();
            DBConnection testconn = new DBConnection();
            string query = "DELETE FROM Cart WHERE user_id = " + userid;
            return testconn.WriteToTest(query);

        }

        //if payment goes through send cart contents to proper tables in DB
        //to give the buyer access to the products he bought
        private void approveProductsForUser()
        {
            DBConnection testconn = new DBConnection();
            List<CartModel> cart = getCartItems();
           
            foreach (CartModel item in cart)
            {
                string user_id = item.user_id.ToString();
                string product_id = item.product_id.ToString();


                string query = "INSERT INTO User_Has_Product (user_id, product_id, product_start, product_end, isValid) VALUES (" + user_id + ", " + product_id + ", getdate(), dateadd(year,1,getdate()), 1)";
                testconn.WriteToTest(query);
            }
        }

        //records sale in invoice table and sends email to buyer with invoice information
        //assumes user has been logged in
        private void generateInvoice()
        {
            DBConnection testconn = new DBConnection();
            CartViewModel cartmodel = getModel();
            string total = cartmodel.total;
            string user_id = System.Web.HttpContext.Current.Session["userid"].ToString();
            string query = "INSERT INTO Invoice (user_id, total, date) VALUES (" + user_id + ", " + total + ", " + " getdate())";
            testconn.WriteToTest(query);
            string get_latest_invoice_id = "SELECT IDENT_CURRENT('Invoice')";
            SqlDataReader dataReader = testconn.ReadFromTest(get_latest_invoice_id);
            dataReader.Read();
            string latest_invoice_id = dataReader.GetValue(0).ToString();

            string invoice_id = dataReader.GetValue(0).ToString();

            foreach(CartModel item in cartmodel.cart)
            {
                string query_for_invoice = "INSERT INTO Invoice_Has_Product (invoice_id, product_id, product_cost) VALUES (" + latest_invoice_id + ", " + item.product_id + ", '" + item.product_cost + "')";
                testconn.WriteToTest(query_for_invoice);
            }

        }

        //this function returns a CartViewModel which is the actual Cart page model
        private CartViewModel getModel()
        {
            List<CartModel> cart = getCartItems();
            CartViewModel pageModel = new CartViewModel(cart);
            return pageModel;
        }
    }
}