﻿using System;
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
        private ProductService product_service = new ProductService();

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

        public ActionResult Conferences()
        {
            var model = product_service.getConferences();
            return View(model);
        }

        /*[ChildActionOnly]
        public PartialViewResult Memberships()
        {
            var model = getMemberships();
            return PartialView("_Memberships", model);
        }*/

        private List<ProductModel> getMemberships()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details FROM Products WHERE isLive = 1 AND type = 'Membership' AND stock > 0";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ProductModel> memberships = new List<ProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                ProductModel membership = new ProductModel(id, cost, type, description, length, true, details, null);
                memberships.Add(membership);
            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            return memberships;
        }
    }
}