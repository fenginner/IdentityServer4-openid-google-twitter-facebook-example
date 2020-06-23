using IdentityServer.Authorization.Role;
using IdentityServer.Authorization.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.DbContext
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder) {

            base.OnModelCreating(builder);
        }

        public DbSet<ApplicationUser> ApplicationUsers { get;set; }
        public DbSet<IdentityRoleApp> IdentityRoleApps { get; set; }

    }

    

}
