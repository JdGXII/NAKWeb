using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AKAWeb_v01.Models
{
    public class JobModel
    {
        [Display(Name = "Name")]
        public string senders_name { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        [Display(Name = "Institution Name")]
        public string instintution_name { get; set; }
        [Display(Name = "Department Name")]
        public string department_name { get; set; }
        [Display(Name = "Title of position to advertise")]
        public string title_position { get; set; }
        [Display(Name = "Category to place it in")]
        public string category { get; set; }
        [Display(Name = "Closing date")]
        public DateTime closing_date { get; set; }
        [Display(Name = "URL of complete job description")]
        public string job_url { get; set; }

        public List<SelectListItem> category_options { get; set; }

        public List<SelectListItem> getCategoriesList()
        {
            List<SelectListItem> myList = new List<SelectListItem>();
            var data = new[]{
                 new SelectListItem{ Value="1",Text="Admin (Chairs/Deans)"},
                 new SelectListItem{ Value="2",Text="Tenure"},
                 new SelectListItem{ Value="3",Text="Non Tenure"}
             };
            myList = data.ToList();
            return myList;
        }

        public JobModel()
        {

        }

        
    }
}