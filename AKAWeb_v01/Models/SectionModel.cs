using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //This class not only serves to model a section, said modelling helps the creation of the top menu
    //The top menu is the menu below the header on the main site. It is created dynamically by loading sections
    //Then listing the pages beneath the section
    public class SectionModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public bool isLive { get; set; }
        //optional. A list with the pages that belong to the section
        public List<PageModel> pages { get; set; }

        public SectionModel(int id, string title, bool isLive)
        {
            this.id = id;
            this.title = title;
            this.isLive = isLive;
        }

        public SectionModel(int id, string title, bool isLive, List<PageModel> pages)
        {
            this.id = id;
            this.title = title;
            this.isLive = isLive;
            this.pages = pages;
        }
    }
}