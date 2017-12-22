using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class AddressModel
    {
        public int user_id { get; set; }
        public string country { get; set; }
        [Display(Name = "State")]
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string street_address { get; set; }

        public AddressModel()
        {
            this.country = " ";
            this.state = " ";
            this.city = " ";
            this.zip = " ";
            this.street_address = " ";

        }

        public AddressModel(string country, string state, string city, string zip, string street_address)
        {
            this.country = country;
            this.state = state;
            this.city = city;
            this.zip = zip;
            this.street_address = street_address;
        }
    }
}