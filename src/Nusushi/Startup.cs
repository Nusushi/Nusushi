//#define SQLITE // TURN OFF FOR MSSQL; APPLY SETTING IN DBCONTEXTS AS WELL; TODO document

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nusushi.Models;
using Nusushi.Services;
using Microsoft.Extensions.Logging;
using Nusushi.Data.Data;
using Nusushi.Data.Models;
using Tokyo.Service.Models;
using California.Creator.Service.Models;
using Tokyo.Service.Data;
using Tokyo.Service.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Tokyo.Service.Options;
using Microsoft.AspNetCore.Mvc.Razor;
using California.Creator.Service.Data;
using California.Creator.Service.Extensions;
using California.Creator.Service.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Nusushi
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, ILogger<Startup> logger, IHostingEnvironment env)
        {
            Configuration = configuration;
            _logger = logger;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO encrypt data with keys per user (like drop box)
            // TODO secure data with DPAPI when using local app
            // TODO sync service?! vs. cloudbackups and localapp+git
            // TODO signed updates for local app
            // TODO enable sqlite store depending on setting in TrackerServiceOptions for local app

            services.AddOptions();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            const string enUSCulture = "en-US"; // TODO use configuration file
            var allSupportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(culture => !culture.IsNeutralCulture).Skip(1).ToList(); // ignore invariant culture
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = allSupportedCultures;
                options.DefaultRequestCulture = new RequestCulture(culture: enUSCulture, uiCulture: enUSCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

#if SQLITE
            services.AddDbContext<NusushiDbContext>(options => options.UseSqlite("Data Source=nusushisqlite.db"));
#else
            services.AddDbContext<NusushiDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("NusushiConnectionString"));
                if (_env.IsDevelopment())
                {
                    options.ConfigureWarnings(warnings =>
                    {
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.QueryClientEvaluationWarning);
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.IncludeIgnoredWarning);
                    });
                }
            });
#endif

            services.AddIdentity<NusushiUser, NusushiRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 10; // TODO phone/sms routine etc to unlock sooner
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<NusushiDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<TrackerServiceOptions>(options =>
            {
                options.SupportedTimeZones = TimeZoneInfo.GetSystemTimeZones();
                options.CookieName = ".Nusushi.Tokyo";
                options.SupportedCultures = allSupportedCultures.ToList(); // TODO must match requestLocalizationOptions
            });

#if SQLITE
            services.AddDbContext<TokyoDbContext>(options => options.UseSqlite("Data Source=tokyosqlite.db"));
            services.AddTokyoServiceSqlite();
#else
            services.AddDbContext<TokyoDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("TokyoConnectionString"));
                if (_env.IsDevelopment())
                {
                    options.ConfigureWarnings(warnings =>
                    {
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.QueryClientEvaluationWarning);
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.IncludeIgnoredWarning);
                    });
                }
            });
            services.AddTokyoService();
#endif


            services.Configure<CaliforniaServiceOptions>(options =>
            {
                options.CookieName = ".Nusushi.California";
            });

#if SQLITE
            services.AddDbContext<CaliforniaDbContext>(options => options.UseSqlite("Data Source=californiasqlite.db"));
            services.AddCaliforniaServiceSqlite();
#else
            services.AddDbContext<CaliforniaDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("CaliforniaConnectionString"));
                if (_env.IsDevelopment())
                {
                    options.ConfigureWarnings(warnings =>
                    {
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.QueryClientEvaluationWarning);
                        warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.IncludeIgnoredWarning);
                    });
                    //options.EnableSensitiveDataLogging();
                }
            });
            services.AddCaliforniaService();
#endif


            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            if (_env.IsProduction())
            {
                var isAiEnabled = Configuration.GetValue<bool?>("Monitoring:IsEnableApplicationInsights", null);
                if (isAiEnabled.HasValue && isAiEnabled.Value == true)
                {
                    var instrumentationKey = Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");
                    if (!string.IsNullOrEmpty(instrumentationKey))
                    {
                        _logger.LogWarning("Connecting to Microsoft Online Application Insights Telemetry.");
                        services.AddApplicationInsightsTelemetry(options => // TODO failed in update 2018_06_10_02_17_00 to asp net core 2.0 => 2.1; 2018_06_10_21_43_00 reverted back
                        {
                            options.ApplicationVersion = "0.9";
                            options.DeveloperMode = false;
                            options.InstrumentationKey = instrumentationKey;
                        });
                    }
                    else
                    {
                        _logger.LogCritical("Could not connect to Microsoft Online Application Insights Telemetry. Instrumentation key missing.");
                    }
                }
            }

            services.AddLocalization(options => options.ResourcesPath = "Resources"); // TODO currently no localization files

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // app.UseCookiePolicy(); TODO this and more .net core 2.1 features https://docs.microsoft.com/en-us/aspnet/core/security/gdpr?view=aspnetcore-2.1 and https://docs.microsoft.com/en-us/aspnet/core/migration/20_21?view=aspnetcore-2.1

            app.UseAuthentication();

            app.UseRequestLocalization();

            app.UseTrackerCookieMiddleware();// TODO cookie middlewares are always both executed for each request
            app.UseCaliforniaCookieMiddleware();

            // TODO ensuremigrated/ensurecreated for all contexts
            // TODO staging
            // TODO this is run while testing/migrating!!
            
            app.UseMvc(routeBuilder =>
            {
                // TODO map routes to AccountController here instead of controller annotation
                // TODO map routes to ManageController here instead of controller annotation

                routeBuilder.MapRoute(NusushiRoutes.NusushiIndexRoute, "", new { controller = "Home", action = "Index" });

                routeBuilder.MapRoute(TrackerRoutes.TrackerBrowserRoute, "tokyo/{id:Guid?}", new { controller = "Tokyo", action = "Index" });
                routeBuilder.MapRoute(TrackerRoutes.TrackerRestApiRoute, "tokyo/{action}", new { controller = "Tokyo" });

                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaBrowserRoute, "california/{id:Guid?}", new { controller = "California", action = "Index" });
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaRestApiRoute, "california/{action}", new { controller = "California" });
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaPubRouteSpecific, "california/pub/{view}", new { controller = "California", action = "pub" });
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaPubRouteUnauthenticatedSpecific, "california/{id:Guid?}/pub/{view}", new { controller = "California", action = "pub" });
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaPubRouteDefault, "california/pub", new { controller = "California", action = "pub", view = "Home" });
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaExportCssRoute, "california/{id:Guid?}/pub.css", new { controller = "California", action = "StaticCss" }); // TODO hack
                routeBuilder.MapRoute(CaliforniaRoutes.CaliforniaExportJsRoute, "california/{id:Guid?}/pub.js", new { controller = "California", action = "StaticJs" }); // TODO hack
            });
        }
    }
}
