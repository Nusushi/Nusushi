using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Tokyo.Service.Builders;
using Tokyo.Service.Middlewares;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Options;
using Tokyo.Service.Services;
using static Tokyo.Service.Models.TrackerLogMessages;

namespace Tokyo.Service.Extensions
{
    public static class TrackerServiceExtensions
    {
        public static IServiceCollection ConfigureTokyoService(this IServiceCollection services, Action<TrackerServiceOptions> configure)
            => services.Configure(nameof(TrackerServiceOptions), configure);

        public static IApplicationBuilder UseTrackerCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TrackerCookieMiddleware>();
        }

        public static TrackerServiceBuilder AddTokyoServiceSqlite(this IServiceCollection services) => services.AddTokyoServiceSqlite(options => { });
        public static TrackerServiceBuilder AddTokyoService(this IServiceCollection services) => services.AddTokyoService(options => { });
        public static TrackerServiceBuilder AddTokyoServiceSqlite(this IServiceCollection services, Action<TrackerServiceOptions> configure)
        {
            // TODO document: database context filters do not work with sqlite => test unique constraints for affected properties (optional foreign key)
            return services.AddTokyoService(options =>
            {
                configure(options);
                options.DatabaseTechnology = TrackerServiceOptions.DbEngineSelector.Sqlite;
            });
        }
        public static TrackerServiceBuilder AddTokyoService(this IServiceCollection services, Action<TrackerServiceOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            // LogEvent(TrackerBackendEvent.RegisterService); // TODO include build number / git parent to match log files <=> source code
            // service configuration
            services.AddOptions();
            services.Configure<TrackerServiceOptions>(options => configure(options));
            services.Configure<TrackerQueueOptions>(options => { });
            // add worker queue for low priority / delayed tasks=TrackerJobs
            services.AddTokyoQueue();
            services.AddAuthentication();
            services.AddAuthorization(options =>
            {
                options.AddTokyoServicePolicies();
            });
            // register DI
            services.AddSingleton<IAuthorizationHandler, TimeTableAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, TimeStampAuthorizationHandler>();
            services.AddSingleton<TrackerDbStateSqlite, TrackerDbStateSqlite>(); // TODO only when using sqlite
            services.AddTransient<TrackerService, TrackerService>();
            // TODO options must be checked somewhere
            // check system compatibility TODO
            // check properly initialized service
            var eventCount = Enum.GetValues(typeof(TrackerEvent)).Length;
            for (int i = 0; i < eventCount; i++)
            {
                if (!TrackerLogMessages.SuccessMessages.TryGetValue((TrackerEvent)i, out var msg))
                {
                    throw new NotImplementedException($"Log information message missing for event: {i}");
                }
            }
            var backendEventCount = Enum.GetValues(typeof(TrackerBackendEvent)).Length;
            for (int i = 0; i < backendEventCount; i++)
            {
                if (!TrackerLogMessages.BackendMessages.TryGetValue((TrackerBackendEvent)i, out var msg))
                {
                    throw new NotImplementedException($"Log information message missing for backend event: {i}");
                }
            }
            return new TrackerServiceBuilder(services);
        }

        public static string MakeCookieValue(this TrackerClientSession currentSession)
            => JsonConvert.SerializeObject(currentSession, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

        public static void UpdateTrackerOptionsCookie(this TrackerClientSession currentSession, HttpResponse response, string cookieName)
        {
            var updatedCookieValue = MakeCookieValue(currentSession);

            if (!string.IsNullOrEmpty(updatedCookieValue))
            {
                response.Cookies.Append(cookieName, updatedCookieValue,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        SameSite = SameSiteMode.Strict,
                        HttpOnly = false // TODO security documentation
                    });
            }
            else
            {
                response.Cookies.Delete(cookieName);
            }
        }

        public static Tuple<DateTimeOffset, DateTimeOffset> GetStartAndEndDate(this TimeTable timeTableAndWorkUnits)
        {
            DateTimeOffset? earliestDate = null;
            DateTimeOffset? latestDate = null;
            foreach (var workUnit in timeTableAndWorkUnits.WorkUnits)
            {
                foreach (var timeNorm in workUnit.TimeNorms)
                {
                    if (timeNorm.StartTime == null || timeNorm.EndTime == null)
                    {
                        throw new ArgumentNullException("TimeNorm not well defined");
                    }
                    if (earliestDate == null || timeNorm.StartTime.TrackedTimeUTC.CompareTo(earliestDate.Value) < 0)
                    {
                        earliestDate = timeNorm.StartTime.TrackedTimeUTC;
                    }
                    if (timeNorm.EndTime.TrackedTimeUTC.CompareTo(earliestDate.Value) < 0)
                    {
                        earliestDate = timeNorm.EndTime.TrackedTimeUTC;
                        // TODO make sure starttime always < endtime
                        // TODO general development guideline: check model for invalid usage possibilities
                    }
                    if (latestDate == null || timeNorm.StartTime.TrackedTimeUTC.CompareTo(latestDate.Value) > 0)
                    {
                        latestDate = timeNorm.StartTime.TrackedTimeUTC;
                    }
                    if (timeNorm.EndTime.TrackedTimeUTC.CompareTo(latestDate.Value) > 0)
                    {
                        latestDate = timeNorm.EndTime.TrackedTimeUTC;
                    }
                }
            }
            if (earliestDate.HasValue && latestDate.HasValue)
            {
                return new Tuple<DateTimeOffset, DateTimeOffset>(earliestDate.Value, latestDate.Value);
            }
            else
            {
                return null;
            }
        }

        public static AuthorizationOptions AddTokyoServicePolicies(this AuthorizationOptions authorizationOptions)
        {
            authorizationOptions.AddPolicy(NusushiPolicy.AdminOnly, policy => policy.RequireClaim(ClaimTypes.Role, NusushiClaim.AdministratorRoleValue)); // TODO check if IssuerInstance must be checked for security
            return authorizationOptions;
        }

        public static Tuple<DateTimeOffset, DateTimeOffset> GetStartAndEndDate(this WorkUnit workUnit) // TODO combine with other stardAndEndDateFunctions
        {
            DateTimeOffset? earliestDate = null;
            DateTimeOffset? latestDate = null;
            foreach (var timeNorm in workUnit.TimeNorms)
            {
                if (timeNorm.StartTime == null || timeNorm.EndTime == null)
                {
                    throw new ArgumentNullException("TimeNorm not well defined");
                }
                if (earliestDate == null || timeNorm.StartTime.TrackedTimeUTC.CompareTo(earliestDate.Value) < 0)
                {
                    earliestDate = timeNorm.StartTime.TrackedTimeUTC;
                }
                if (timeNorm.EndTime.TrackedTimeUTC.CompareTo(earliestDate.Value) < 0)
                {
                    earliestDate = timeNorm.EndTime.TrackedTimeUTC;
                    // TODO make sure starttime always < endtime
                    // TODO general development guideline: check model for invalid usage possibilities
                }
                if (latestDate == null || timeNorm.StartTime.TrackedTimeUTC.CompareTo(latestDate.Value) > 0)
                {
                    latestDate = timeNorm.StartTime.TrackedTimeUTC;
                }
                if (timeNorm.EndTime.TrackedTimeUTC.CompareTo(latestDate.Value) > 0)
                {
                    latestDate = timeNorm.EndTime.TrackedTimeUTC;
                }
            }
            if (earliestDate.HasValue && latestDate.HasValue)
            {
                return new Tuple<DateTimeOffset, DateTimeOffset>(earliestDate.Value, latestDate.Value);
            }
            else
            {
                return null;
            }
        }

        // converts a time span to a human readable string (<1s, ..., n days m hours)
        // TODO translation
        public static string ToTrackerDuration(this double ms, bool isConvertHoursToDays, bool isAllowNegative)
        {
            var durationSign = "+";
            if (ms < 0.0)
            {
                if (!isAllowNegative)
                {
                    return "<0?";
                }
                else
                {
                    durationSign = "-";
                    ms *= -1.0; // TODO test for max range and exception behaviour when used with allowNegative
                }
            }
            var durationInitial = ms / 1000.0;
            var durationString = "";
            if (durationInitial < 1.0)
            {
                durationString = "< 1s";
            }
            else if (durationInitial < 60.0)
            {
                durationString = $"{Math.Round(durationInitial)}s";
            }
            else
            { //if (durationInitial >= 60.0)
                var durationSec = durationInitial % 60.0;
                var durationMin = (durationInitial - durationSec) / 60.0;
                if (durationMin < 60.0)
                {
                    var durationSecRounded = Math.Round(durationSec);
                    var durationSecString = durationSecRounded >= 1.0 ? $" {durationSecRounded}s" : "";
                    durationString = $"{durationMin}min{durationSecString}";
                }
                else
                {
                    var durationMinWhenH = durationMin % 60.0;
                    var durationH = (durationMin - durationMinWhenH) / 60.0;
                    if (!isConvertHoursToDays || durationH < 24.0)
                    {
                        var durationMinWhenHRounded = Math.Round(durationMinWhenH);
                        var durationMinWhenHString = durationMinWhenHRounded >= 1.0 ? $" {durationMinWhenH}min" : "";
                        durationString = $"{durationH}h{durationMinWhenHString}";
                    }
                    else
                    {
                        var durationHWhenDays = durationH % 24.0;
                        var durationHWhenDaysRounded = Math.Round(durationHWhenDays);
                        var durationDays = (durationH - durationHWhenDays) / 24.0;
                        var durationHWhenDaysString = durationHWhenDaysRounded >= 1.0 ? $" {durationHWhenDaysRounded}h" : "";
                        durationString = $"{durationDays}d{durationHWhenDaysString}";
                    }
                }
            }
            if (isAllowNegative)
            {
                durationString = durationSign + durationString;
            }
            return durationString;
        }

        /*public static TrackerService GetTestSession(this IStartup startup)
        {
            startup.
            var session = new TrackerService()
            {
                Name = "Test Session 1",
                DateCreated = new DateTime(2016, 8, 1)
            };
            var idea = new Idea()
            {
                DateCreated = new DateTime(2016, 8, 1),
                Description = "Totally awesome idea",
                Name = "Awesome idea"
            };
            session.AddIdea(idea);
            return session;
        }*/

        /*public static ITrackerServiceBuilder AddEntityFrameworkDatabases(this ITrackerServiceBuilder builder, TrackerContext context)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            
            var services = builder.Services;

            services.AddScoped<TrackerContext, TrackerContext>();

            return builder;
        }*/

        public static TimeStamp SetTrackedTimeForView(this TimeStamp timeStamp, TrackerClientSession clientSession)
            => SetTrackedTimeForView(timeStamp, clientSession.ViewTimeZone);

        public static TimeStamp SetTrackedTimeForView(this TimeStamp timeStamp, TimeZoneInfo requestTimeZone)
        {
            if (requestTimeZone == null || timeStamp.TimeZoneIdAtCreation == requestTimeZone.Id)
            {
                timeStamp.TrackedTimeForView = timeStamp.TrackedTimeAtCreation;
            }
            else
            {
                //var requestOffset = requestTimeZone.GetUtcOffset(timeNorm.StartTime.TrackedTimeUTC.UtcDateTime);
                //timeNorm.StartTime.TrackedTimeForView = new DateTimeOffset(timeNorm.StartTime.TrackedTimeUTC.DateTime, requestOffset); // TODO check getUTCOffset everywhere

                var result = TimeZoneInfo.ConvertTimeFromUtc(timeStamp.TrackedTimeUTC.UtcDateTime, requestTimeZone); // TODO cache
                timeStamp.TrackedTimeForView = new DateTimeOffset(result, requestTimeZone.GetUtcOffset(result));
            }
            timeStamp.TimeString = timeStamp.TrackedTimeForView.Value.DateTime.ToLongTimeString();
            timeStamp.DateString = timeStamp.TrackedTimeForView.Value.DateTime.ToLongDateString();
            return timeStamp;
        }

        public static TimeTable TranformTrackedTimesForView(this TimeTable timeTable, TrackerClientSession clientSession)
        {
            foreach (var workUnit in timeTable.WorkUnits)
            {
                TimeZoneInfo targetTimeZone = null;
                if (workUnit.IsDisplayAgendaTimeZone)
                {
                    targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(workUnit.TimeZoneIdAgenda);
                }
                else if (!clientSession.IsDisplayAsTracked)
                {
                    targetTimeZone = clientSession.ViewTimeZone;
                }
                foreach (var timeNorm in workUnit.TimeNorms)
                {
                    if (timeNorm.StartTime != null)
                    {
                        timeNorm.StartTime = timeNorm.StartTime.SetTrackedTimeForView(targetTimeZone);
                    }
                    if (timeNorm.EndTime != null)
                    {
                        timeNorm.EndTime = timeNorm.EndTime.SetTrackedTimeForView(targetTimeZone);
                    }
                }
            }
            return timeTable;
        }

        public static DateTimeOffset FirstTimeOfDay(this DateTimeOffset timeOfDay)
        {
            // TODO what if "today" was DST adjustment => must be in user time zone
            var targetTimeZone = TimeZoneInfo.Utc;
            return new DateTimeOffset(timeOfDay.Year, timeOfDay.Month, timeOfDay.Day, 0, 0, 0, 1, TimeSpan.Zero); // TODO
        }
    }
}
