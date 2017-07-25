using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class ProductModel
    {
        public int id { get; set; }
        public int cost { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string length { get; set; }
        public bool isLive { get; set; }
        public string details { get; set; }
        public string image { get; set; }

        public ProductModel() {

            this.id = 0;
            this.cost = 0;
            this.type = "Empty";
            this.description = "Empty";
            this.length = "Empty";
            this.isLive = false;
            this.details = "Empty";
            this.image = "Empty";

        }
        
        public ProductModel(int id, int cost, string type, string description, string length, bool isLive)
        {
            this.id = id;
            this.cost = cost;
            this.type = type;
            this.description = description;
            this.length = length;
            this.isLive = isLive;

        }

        public ProductModel(int id, int cost, string type, string description, string length, bool isLive, string details, string image)
        {
            this.id = id;
            this.cost = cost;
            this.type = type;
            this.description = description;
            this.length = length;
            this.isLive = isLive;
            this.details = details;
            this.image = image;

        }

        public ProductModel(int id, int cost)
        {
            this.id = id;
            this.cost = cost;
          

        }
    }
}