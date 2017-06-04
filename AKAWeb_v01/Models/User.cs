using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{

    public class User
    {
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int access { get; set; }
    }
}