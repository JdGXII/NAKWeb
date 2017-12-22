using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class MainMenuItem
    {
        public int id { get; set; }
        public string item_name { get; set; }
        public bool islive { get; set; }
        public int submenu_id { get; set; }
        public MainMenuItem submenu_item { get; set; }
    }
}