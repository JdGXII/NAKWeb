using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class PageModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string subheaderImage { get; set; }
        public List<string> leftMenu { get; set; }
        public string leftMenuTitle { get; set; }
        public string pageContent { get; set; }
        //public DateTime createdAt { get; set; }
        //public DateTime modifiedAt { get; set; }

    }
}