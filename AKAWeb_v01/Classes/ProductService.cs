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

        //returns a product model (representing a product) by id
        private ProductModel getProduct(string product_id)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id, type, cost, description, length, details FROM Products WHERE id = " + product_id;
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

        public List<ConferenceModel> getConferences()
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT title, start_date, end_date, isLive, conference_code, tagline, processing_fee, members_only FROM Conference WHERE getdate() < start_date AND isLive = 1 AND max_attendees > attendees";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            List<ConferenceModel> conferences = new List<ConferenceModel>();
            if (dataReader.HasRows)
            {

                while (dataReader.Read())
                {

                    string title = dataReader.GetValue(0).ToString();
                    string start_date = dataReader.GetValue(1).ToString();
                    string end_date = dataReader.GetValue(2).ToString();
                    bool isLive = (bool)dataReader.GetValue(3);
                    int conference_code = Int32.Parse(dataReader.GetValue(4).ToString());
                    string tagline = dataReader.GetValue(5).ToString();
                    string processing_fee = dataReader.GetValue(6).ToString();
                    bool members_only = (bool)dataReader.GetValue(7);

                    ConferenceModel conference = new ConferenceModel(title, start_date, end_date, isLive, conference_code);
                    conference.tagline = tagline;
                    conference.tickets = getAssociatedTickets(conference_code);
                    conference.location = getAssociatedLocation(conference_code);
                    conference.processing_fee = processing_fee;
                    conference.members_only = members_only;
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



    }
}