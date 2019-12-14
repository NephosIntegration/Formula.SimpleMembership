using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Formula.SimpleMembership
{
    public class SimpleMembershipDbContext : IdentityDbContext<ApplicationUser>
    {
        public SimpleMembershipDbContext(DbContextOptions<SimpleMembershipDbContext> options) : base(options)
        {
        }

        protected SimpleMembershipDbContext()
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

    }
}