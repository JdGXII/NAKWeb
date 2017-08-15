using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using AKAWeb_v01.Models;
using ExcelDataReader;

namespace AKAWeb_v01.Classes
{
    public class ExcelService
    {
        HashService hash_service = new HashService();
        public ExcelService()
        {
            string filePath = "C:\\MembershipData7-19-17.xlsx";
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var reader = ExcelReaderFactory.CreateReader(stream);
            reader.Read();
            do
            {
                while (reader.Read())
                {
                    /* string name = reader.GetString(1) + " "+ reader.GetString(2);
                     string email = reader.GetString(16);
                     string password = hash_service.HashPassword("AKA_Temp_Password");
                     int access = 1;

                     string country = "US";
                     string state = reader.GetString(9);
                     string city = reader.GetString(8);
                     string zip = reader.GetString(10);
                     string street_address = reader.GetString(5) + " " + reader.GetString(6);

                     AddressModel address = new AddressModel(country, state, city, zip, street_address);

                     UserModel user = new UserModel(0, name, email, password, access, address);

                     CreateUser(user);*/
                    string email = reader.GetString(16);
                    string price = reader.GetDouble(23).ToString();
                    //string date = reader.GetString(26);
                    DateTime date = reader.GetDateTime(26);
                    string end_date = date.ToString("yyyy-mm-dd");
                    end_date = end_date.Replace("-00-", "-12-");
                    int product_id = GetMembershipIdByPrice(price);
                    int user_id = GetUserIdByEmail(email);

                    
                    AddProductToUser(user_id, product_id, end_date);
                    



                }
            } while (reader.NextResult());
        }

        private bool CreateUser(UserModel user)
        {
            bool created = false;
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO Users(name, email, password, access) VALUES ('" + user.name + "', '" + user.email + "', '" + user.password + "'," + user.access + ")";
            string getId = "SELECT IDENT_CURRENT('Users')";
            SqlDataReader dataReader;
            if (testconn.WriteToTest(query))
            {
                dataReader = testconn.ReadFromTest(getId);
                dataReader.Read();
                int id = Int32.Parse(dataReader.GetValue(0).ToString());
                user.id = id;
                AddAddressToUser(user);
                created = true;
            }

            return created;

        }

        private void AddAddressToUser(UserModel user)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO User_Has_Address (country, state, city, street_address, zip, user_id) VALUES('" + user.address.country + "', '" + user.address.state + "'," +
"'" + user.address.city + "','" + user.address.street_address + "', '" + user.address.zip + "', " + user.id + ")";
            testconn.WriteToTest(query);


        }

        private int GetMembershipIdByPrice(string price)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id from Products where type = 'Membership' AND cost = '" + price+"'";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            int product_id = Int32.Parse(dataReader.GetValue(0).ToString());

            return product_id;
        }

        private int GetUserIdByEmail(string email)
        {
            DBConnection testconn = new DBConnection();
            string query = "SELECT id from Users WHERE email = '" + email+"'";
            SqlDataReader dataReader = testconn.ReadFromTest(query);
            dataReader.Read();
            int user_id = Int32.Parse(dataReader.GetValue(0).ToString());
            return user_id;
        }

     

        private void AddProductToUser(int user_id, int product_id, string end_date)
        {
            DBConnection testconn = new DBConnection();
            string query = "INSERT INTO User_Has_Product (user_id, product_id, product_end, isValid) VALUES(" + user_id.ToString() + "," + product_id.ToString() + ", '" + end_date + "',1)";
            string b = "";
            testconn.WriteToTest(query);

        }



    }
}