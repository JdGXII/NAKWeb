using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    public class MembershipModel : ProductModel
    {
        public string type { get; set; }
        public string description { get; set; }

        public MembershipModel(int id, int cost, string type, string description) : base (id, cost)
        {
            this.type = type;
            this.description = description;

        }
    }
}