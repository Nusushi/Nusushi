//#define SQLITE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Tokyo.Service.Data
{
    class TokyoDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TokyoDbContext>
    {
        public TokyoDesignTimeDbContextFactory()
        {

        }
        public TokyoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TokyoDbContext>();
            var devConfig = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json")).Build(); // TODO
#if !SQLITE
            optionsBuilder.UseSqlServer(devConfig.GetConnectionString("TokyoConnectionString"));
#else
            optionsBuilder.UseSqlite("Data Source=tokyosqlite.db"); // TODO
#endif

            return new TokyoDbContext(optionsBuilder.Options);
        }
    }
}
