//#define SQLITE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace California.Creator.Service.Data
{
    class CaliforniaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CaliforniaDbContext>
    {
        public CaliforniaDesignTimeDbContextFactory()
        {

        }
        public CaliforniaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CaliforniaDbContext>();
            var devConfig = new ConfigurationBuilder().AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json")).Build(); // TODO
#if !SQLITE
            optionsBuilder.UseSqlServer(devConfig.GetConnectionString("CaliforniaConnectionString"));
#else
            optionsBuilder.UseSqlite("Data Source=californiasqlite.db");
#endif
            return new CaliforniaDbContext(optionsBuilder.Options);
        }
    }
}
