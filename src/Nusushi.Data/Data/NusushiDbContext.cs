using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nusushi.Data.Data
{
    public class NusushiDbContext : IdentityDbContext<NusushiUser, NusushiRole, Guid>
    {
        public NusushiDbContext(DbContextOptions<NusushiDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
        public DbSet<NusushiInvitationToken> NusushiInvitationTokens { get; set; }
    }
}
