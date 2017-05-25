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
        private string testString = WebConfigurationManager.ConnectionStrings["Test"].ToString();

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



                return true; ;

            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["exception"] = e.ToString();
                return false;
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