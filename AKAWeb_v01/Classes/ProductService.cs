using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

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
            
        }


    }
}