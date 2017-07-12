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
    public class ProductsController : Controller
    {
        
        // GET: Products
        public ActionResult ListProducts(string type)
        {
            return View();
        }

        public ActionResult Memberships()
        {
            var model = getMemberships();
            return View(model);
        }

        private List<ProductModel> getMemberships()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length FROM Products WHERE isLive = 1 AND type = 'Membership'";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ProductModel> memberships = new List<ProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                ProductModel membership = new ProductModel(id, cost, type, description, length, true);
                memberships.Add(membership);
            }

            return memberships;
        }
    }
}