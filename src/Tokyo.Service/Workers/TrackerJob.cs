using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Abstractions;

namespace Tokyo.Service.Workers
{
    public abstract class TrackerJob : ITrackerJob // TODO is sealed needed in this/derived classes
    {
        // TODO check access rights when process is executed
        private string _name = null;
        // returns derived job class (RemoveUserJob, ...)
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = this.GetType().ToString();
                }
                return _name;
            }
        }
        protected ILogger Logger { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected TrackerJob(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TrackerJob>(); // TODO carry over original request ID to match logs when job execution is deferred
        }
        public abstract Task<TrackerJobResult> RunAsync();
    }
}
