using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace AKAWeb_v01.Classes
{
    public class DBConnection
    {
        private SqlDataReader dataReader = null;
        private SqlConnection cnn;
        private string testString = WebConfigurationManager.ConnectionStrings["A2String"].ToString();

        public bool WriteToTest(string query)
        {

            cnn = new SqlConnection(testString);
            SqlCommand command;


            try
            {
                cnn.Open();
                command = cnn.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
                command.Dispose();



                return true; ;

            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                return false;
            }
        }

        //safely inserts or updates DB. Receives query with dictionary with name of parameter and its value
        public bool WriteToProduction(string query, Dictionary<string, Object> parameters)
        {

            int rows_affected = 0;
            using(cnn = new SqlConnection(testString))
            {
                cnn.Open();
                using (SqlCommand command = new SqlCommand(query, cnn))
                {
                    foreach(KeyValuePair<string, Object> paramater in parameters)
                    {
                        command.Parameters.AddWithValue(paramater.Key.ToString(), paramater.Value);
                    }

                    rows_affected = command.ExecuteNonQuery();
                    
                }
            }

            if(rows_affected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

            
        }

        //safely inserts or updates to a DB table and returns the id of the last inserted row
        public int WriteToProductionReturnID(string query, Dictionary<string, Object> parameters)
        {
            int id = -1;
            using (cnn = new SqlConnection(testString))
            {
                cnn.Open();
                using (SqlCommand command = new SqlCommand(query, cnn))
                {
                    foreach (KeyValuePair<string, Object> paramater in parameters)
                    {
                        command.Parameters.AddWithValue(paramater.Key.ToString(), paramater.Value);
                    }

                    command.CommandText = query + "; SELECT SCOPE_IDENTITY();";
                    //Object ob = command.ExecuteScalar();
                    //ob.ToString();
                   id = Int32.Parse(command.ExecuteScalar().ToString());

                }
            }

            return id;
        }

        public int WriteToTestReturnID(string query)
        {

            cnn = new SqlConnection(testString);
            SqlCommand command;


            try
            {
                cnn.Open();
                command = cnn.CreateCommand();
                command.CommandText = query + "; SELECT SCOPE_IDENTITY();";
                int id = Int32.Parse(command.ExecuteScalar().ToString());
                command.Dispose();

                cnn.Close();

                return id; 

            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                return -1;
            }
        }

        public SqlDataReader ReadFromTest(string query)
        {

            
            
            cnn = new SqlConnection(testString);
            SqlCommand command;
            

            try
            {
                cnn.Open();
                command = new SqlCommand(query, cnn);
                dataReader = command.ExecuteReader();
                
                
                
                return dataReader;

            }
            catch(Exception e)
            {
                return dataReader;
            }
           
        }

        //Read safely from DB. Dictionary with parameters have key = param name and value = value of the param
        public SqlDataReader ReadFromProduction(string query, Dictionary<string, Object> parameters)
        {
            try
            {
                cnn = new SqlConnection(testString);
                cnn.Open();
                SqlCommand command = new SqlCommand(query, cnn);
                //iterate through the dictionary and add the parameters to the query
                foreach (KeyValuePair<string, Object> param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key.ToString(), param.Value);
                }
                dataReader = command.ExecuteReader();
                return dataReader;
            }
            catch (Exception e)
            {
                return dataReader;
            }



        }

        public void CloseDataReader()
        {
            if (dataReader != null)
            {
                dataReader.Close();
            }
        }

        public void CloseConnection()
        {
            if (cnn != null)
            {
                cnn.Close();
            }
        }
    }

   
}