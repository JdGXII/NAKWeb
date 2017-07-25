using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //class representing a row from the User_has_Product table in DB
    public class UserHasProductModel
    {
        public int user_id { get; set; }
        public int product_id { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public bool isLive { get; set; }

        public UserHasProductModel()
        {

        }

        public UserHasProductModel(int user_id, int product_id, string start_date, string end_date, bool isLive)
        {
            this.user_id = user_id;
            this.product_id = product_id;
            this.start_date = start_date;
            this.end_date = end_date;
            this.isLive = isLive;
        }
    }
}