using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class SectionModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool isLive { get; set; }

        public SectionModel(int id, string title, bool isLive)
        {
            this.id = id;
            this.title = title;
            this.isLive = isLive;
        }
    }
}