using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Models
{
    public class PageModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string subheaderImage { get; set; }
        public List<string> leftMenu { get; set; }
        public string leftMenuTitle { get; set; }
        [AllowHtml]
        public string pageContent { get; set; }
        public int section { get; set; }
        public bool isLive { get; set; }
        //public DateTime createdAt { get; set; }
        //public DateTime modifiedAt { get; set; }
        public int sort_order { get; set; }
        //optional. a list for generating a dropdown element with the section's sort_order marked as default
        public List<SelectListItem> dropdown { get; set; }

        public PageModel()
        {

        }

        public PageModel(int id, string title, string subheaderImage, string pageContent, int section, bool isLive)
        {
            this.id = id;
            this.title = title;
            this.subheaderImage = subheaderImage;
            this.pageContent = pageContent;
            this.section = section;
            this.isLive = isLive;
            leftMenu = null;
            leftMenuTitle = null;

        }

    }
}