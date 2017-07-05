using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class BackendMenuModel
    {

        public int id { get; set; }
        public string title { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public bool isLive { get; set; }

        public BackendMenuModel() { }

        public BackendMenuModel(int id, string title, string controller, string action, bool isLive)
        {
            this.id = id;
            this.title = title;
            this.controller = controller;
            this.action = action;
            this.isLive = isLive;
        }
    }
}