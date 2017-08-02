using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;


namespace AKAWeb_v01.Classes
{
    public class HashService
    {
        public string HashString(string str)
        {
            return Crypto.SHA256(str);

        }

        public string HashPassword(string password)
        {
            
            return Crypto.HashPassword(password);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            return Crypto.VerifyHashedPassword(hashedPassword, password);
        }



    }
}
    
