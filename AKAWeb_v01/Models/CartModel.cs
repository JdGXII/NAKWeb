using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //this class models what a row in the Cart table from the DB with
    //a join from the product table to get the price
    public class CartModel
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int product_id { get; set; }
        public string product_description { get; set; }
        public string product_cost { get; set; }

        public CartModel(int id, int user_id, int product_id, string product_description, string product_cost)
        {
            this.id = id;
            this.user_id = user_id;
            this.product_id = product_id;
            this.product_description = product_description;
            this.product_cost = product_cost;
        }
    }
}