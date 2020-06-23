using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Authorization.Role
{
    public class IdentityRoleApp : IdentityRole
    {
        public string MoteRole { get; set; }
    }
}
