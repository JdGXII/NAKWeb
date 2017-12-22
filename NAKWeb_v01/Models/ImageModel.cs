using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class ImageModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string url { get; set; }

        public ImageModel()
        {
            id = 0;
            title = "Empty Image";
            string url = "Empty Image";

        }
        
        public ImageModel(int id, string title, string url)
        {
            this.id = id;
            this.title = title;
            this.url = url;
        }
    }

}