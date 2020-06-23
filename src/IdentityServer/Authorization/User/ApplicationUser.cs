using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer.Authorization.User
{
    public class ApplicationUser : IdentityUser
    {
        public string Mote { get; set; }

        public ApplicationUser()
        {
          
        }
    }
}
