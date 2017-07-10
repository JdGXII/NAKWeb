using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public abstract class ProductModel
    {
        public int id { get; set; }
        public int cost { get; set; }
        
        public ProductModel(int id, int cost)
        {
            this.id = id;
            this.cost = cost;
        }
    }
}