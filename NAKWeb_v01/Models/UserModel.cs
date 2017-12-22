using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKAWeb_v01.Models
{
    //describes a user
    public class UserModel
    {
        public int id  { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int access { get; set; }
        //user has an address represented by an address model
        public AddressModel address { get; set; }

        public UserModel()
        {
            id = 0;
            name = "Empty User";
            email = "Empty User";
            password = "Empty User";
            access = 0;
            address = new AddressModel();

        }

        public UserModel (int id, string name, string email, string password, int access)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.password = password;
            this.access = access;
            this.address = new AddressModel();
        }

        public UserModel(int id, string name, string email, string password, int access, AddressModel address)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.password = password;
            this.access = access;
            this.address = address;
        }


    }
}