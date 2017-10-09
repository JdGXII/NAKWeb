using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class ConferenceModel
    {
        public int id { get; set; }
        [Display(Name = "Title")]
        public string title { get; set; }
        public string tagline { get; set; }
        public string external_url { get; set; }
        [Display(Name = "Start Date")]
        public string start_date { get; set; }
        [Display(Name = "End Date")]
        public string end_date { get; set; }
        public string processing_fee { get; set; }
        public int max_attendees { get; set; }
        public int attendees { get; set; }
        public bool members_only { get; set; }
        [Display(Name = "Is Live")]
        public bool isLive { get; set; }
        public List<ProductModel> tickets { get; set; }
        [Display(Name = "Conference Code")]
        public int conference_code { get; set; }

        public ConferenceModel()
        {
            id = 0;
            title = "Empty";
            tagline = "Empty";
            external_url = "Empty";
            start_date = "Empty";
            end_date = "Empty";
            processing_fee = "Empty";
            max_attendees = 0;
            attendees = 0;
            members_only = true;
            isLive = false;

        }

        public ConferenceModel(string title, string start_date, string end_date, bool isLive, int conference_code)
        {
            this.title = title;
            this.start_date = start_date;
            this.end_date = end_date;
            this.isLive = isLive;
            this.conference_code = conference_code;
        }

        public ConferenceModel(string title, string tagline, string external_url, string start_date, string end_date, string processing_fee, int max_attendees, bool members_only)
        {

            this.title = title;
            this.tagline = tagline;
            this.external_url = external_url;
            this.start_date = start_date;
            this.end_date = end_date;
            this.processing_fee = processing_fee;
            this.max_attendees = max_attendees;
            this.members_only = members_only;

        }

        public ConferenceModel(int id, string title, string tagline, string external_url, string start_date, string end_date, string processing_fee, int max_attendees, int attendees, bool members_only, bool isLive)
        {
            this.id = id;
            this.title = title;
            this.tagline = tagline;
            this.external_url = external_url;
            this.start_date = start_date;
            this.end_date = end_date;
            this.processing_fee = processing_fee;
            this.max_attendees = max_attendees;
            this.attendees = attendees;
            this.members_only = members_only;
            this.isLive = isLive;
        }

        public ConferenceModel(int id, string title, string tagline, string external_url, string start_date, string end_date, string processing_fee, int max_attendees, int attendees, bool members_only, bool isLive, List<ProductModel> tickets)
        {
            this.id = id;
            this.title = title;
            this.tagline = tagline;
            this.external_url = external_url;
            this.start_date = start_date;
            this.end_date = end_date;
            this.processing_fee = processing_fee;
            this.max_attendees = max_attendees;
            this.attendees = attendees;
            this.members_only = members_only;
            this.isLive = isLive;
            this.tickets = tickets;
        }

        public ConferenceModel(int id, string title, string tagline, string external_url, string start_date, string end_date, string processing_fee, int max_attendees, int attendees, bool members_only, bool isLive, List<ProductModel> tickets, int conference_code)
        {
            this.id = id;
            this.title = title;
            this.tagline = tagline;
            this.external_url = external_url;
            this.start_date = start_date;
            this.end_date = end_date;
            this.processing_fee = processing_fee;
            this.max_attendees = max_attendees;
            this.attendees = attendees;
            this.members_only = members_only;
            this.isLive = isLive;
            this.tickets = tickets;
            this.conference_code = conference_code;
        }

    }
}