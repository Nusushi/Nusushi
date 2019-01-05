using System;
using System.Collections.Generic;
using System.Text;
using Tokyo.Service.Abstractions;

namespace Tokyo.Service.Builders
{
    public abstract class TrackerJobBuilder : ITrackerJobBuilder
    {
        public IServiceProvider ServiceProvider { get; }
        protected TrackerJobBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        public ITrackerJob Job { get; protected set; }
        public virtual ITrackerJob Build()
        {
            if (Job == null)
            {
                throw new ArgumentNullException($"{nameof(Job)}: Configure builder.");
            }
            return Job;
        }
        public abstract ITrackerJobBuilder Configure(IEnumerable<KeyValuePair<string, object>> optionValues);
        public abstract bool Validate();
    }
}
