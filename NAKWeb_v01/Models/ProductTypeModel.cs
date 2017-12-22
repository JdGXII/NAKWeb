using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class ProductTypeModel
    {
        public int id { get; set; }
        public string name { get; set; }

        public ProductTypeModel()
        {
            id = 0;
            name = "Empty";
        }

        public ProductTypeModel(int id, string name)
        {
            this.id = id;
            this.name = name;

        }
    }
}