using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AKAWeb_v01.Models;

namespace AKAWeb_v01.Classes
{
    //service that provides and updates different information on products like stock level alerts
    public class ProductService
    {
        public int low_stock_alert { get; set; }

        public ProductService()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT stock_alert FROM Low_Stock_Alert WHERE id = 1";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            low_stock_alert = Int32.Parse(dataReader.GetValue(0).ToString());
            testconn.CloseDataReader();
            testconn.CloseConnection();

        }

        public async Task<List<ProductModel>> getTickets()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details, isLive FROM Products WHERE type = 'Ticket' AND stock > 0";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ProductModel> tickets = new List<ProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                bool isLive = (bool)dataReader.GetValue(6);
                ProductModel ticket = new ProductModel(id, cost, type, description, length, isLive, details, null);
                tickets.Add(ticket);
            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            return tickets;
        }

        public List<ProductModel> getConferenceAddOns()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details, isLive FROM Products WHERE type = 'ConferenceProduct' AND stock > 0";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ProductModel> conference_products = new List<ProductModel>();
            while (dataReader.Read())
            {
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                string type = dataReader.GetValue(1).ToString();
                int cost = Int32.Parse(dataReader.GetValue(2).ToString());
                string description = dataReader.GetValue(3).ToString();
                string length = dataReader.GetValue(4).ToString();
                string details = dataReader.GetValue(5).ToString();
                bool isLive = (bool)dataReader.GetValue(6);
                ProductModel conference_product = new ProductModel(id, cost, type, description, length, isLive, details, null);
                conference_products.Add(conference_product);
            }
            testconn.CloseDataReader();
            testconn.CloseConnection();

            return conference_products;
        }


    }
}