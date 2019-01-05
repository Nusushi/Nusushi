using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Workers;

namespace Tokyo.Service.Abstractions
{
    public interface ITrackerQueue
    {
        Task<TrackerEnqueuedJob> EnqueueJobAsync(Type trackerJobBuilderType, IEnumerable<KeyValuePair<string, object>> optionValues, TimeSpan runAfterTime, uint jobPrioritySmallerFirst);
        Task<TrackerJobResult> EnqueueAndWaitJobAsync(Type type, IEnumerable<KeyValuePair<string, object>> optionValues, uint jobPrioritySmallerFirst);
        // TODO wait all enqueud tasks to finish / persist tasks for app restart (with version number)
        Task PollQueueAndRunJobsAsync();
    }
}
