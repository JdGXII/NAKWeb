using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //class representing a row from the User_has_Product table in DB
    public class UserHasProductModel
    {
        public int id { get; set; }
        public UserModel user { get; set; }
        public ProductModel product { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public bool isValid { get; set; }

        public UserHasProductModel()
        {

        }

        public UserHasProductModel(UserModel user, ProductModel product, string start_date, string end_date, bool isLive)
        {
            this.user = user;
            this.product = product;
            this.start_date = start_date;
            this.end_date = end_date;
            this.isValid = isLive;
        }

        public UserHasProductModel(int id, UserModel user, ProductModel product, string start_date, string end_date, bool isLive)
        {
            this.id = id;
            this.user = user;
            this.product = product;
            this.start_date = start_date;
            this.end_date = end_date;
            this.isValid = isLive;
        }
    }
}