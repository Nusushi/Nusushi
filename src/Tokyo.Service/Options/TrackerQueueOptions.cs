using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Options
{
    public class TrackerQueueOptions
    {
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(1000);
        public int MaxJobs { get; set; } = Environment.ProcessorCount;

        public void Validate()
        {
            const int minThreads = 1;
            const int maxThreads = 24;
            if (MaxJobs < minThreads || MaxJobs > maxThreads)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxJobs));
            }
        }
    }
}
