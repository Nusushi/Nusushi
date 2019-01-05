//#define SQLITE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nusushi.Data.Data
{
    public class NusushiDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NusushiDbContext>
    {
        public NusushiDesignTimeDbContextFactory()
        {
        }
        public NusushiDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NusushiDbContext>();
            var devConfig = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json")).Build(); // TODO
#if !SQLITE
            optionsBuilder.UseSqlServer(devConfig.GetConnectionString("NusushiConnectionString"));
#else
            optionsBuilder.UseSqlite("Data Source=nusushisqlite.db"); // TODO
#endif
            return new NusushiDbContext(optionsBuilder.Options);
        }
    }
}
