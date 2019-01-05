using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Builders;
using Tokyo.Service.Options;
using Tokyo.Service.Services;
using Tokyo.Service.Workers.Jobs;

namespace Tokyo.Service.Extensions
{
    public static class TrackerQueueExtensions
    {
        public static TrackerQueueBuilder AddTokyoQueue(this IServiceCollection services) => services.AddTokyoQueue(options => { });
        public static TrackerQueueBuilder AddTokyoQueue(this IServiceCollection services, Action<TrackerQueueOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            services.AddMemoryCache();
            // configuration
            services.AddOptions();
            services.Configure<TrackerQueueOptions>(options => configure(options));
            // register DI
            services.AddSingleton<ITrackerQueue, TrackerQueue>();
            // register JobBuilders with Options
            services.AddTransient<RemoveUserJobBuilder, RemoveUserJobBuilder>();
            services.AddTransient<ProcessTimeStampsJobBuilder, ProcessTimeStampsJobBuilder>();
            /*services.AddTransient<RemoveUserJob, RemoveUserJob>(serviceProvider =>
            {
                var builder = serviceProvider.GetService<RemoveUserJobBuilder>();
                // TODO how can we pass options here before building
                return builder.Configure(new Dictionary<string, string>()).Build() as RemoveUserJob;
            });*/
            // TODO initialize service message to get a separate scope in AI (currently messages are bound to first request operation id)
            return new TrackerQueueBuilder(services);
        }
    }
}
