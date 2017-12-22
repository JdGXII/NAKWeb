using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //This class models the PAGE for the cart
    //it basically has the list of cart items plus the total amount of the transaction
    public class CartViewModel
    {
        public List<CartModel> cart { get; set; }
        public string total { get; set; }

        public CartViewModel(List<CartModel> cart)
        {
            this.cart = cart;
            this.total = Total();
        }

        private string Total()
        {
            float total = 0;
            foreach(CartModel item in this.cart)
            {
                total = total +  float.Parse(item.product_cost);
            }
            //format the total to only two decimal places
            return total.ToString("0.00");

        }

    }
}