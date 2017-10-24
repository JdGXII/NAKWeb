using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class DepartmentModel
    {
        public int id { get; set; }
        [Display(Name = "Department")]
        public string name { get; set; }
        public string website { get; set; }

        public DepartmentModel()
        {
            id = 0;
            name = "Empty";
            website = "Empty";
        }

        public DepartmentModel(int id, string name, string website)
        {
            this.id = id;
            this.name = name;
            this.website = website;
        }
    }
}