using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Builders;
using Tokyo.Service.Models;
using Tokyo.Service.Options;
using Tokyo.Service.Workers;
using static Tokyo.Service.Models.TrackerLogMessages;

namespace Tokyo.Service.Services
{
    // TODO all public members and methods are thread safe
    public class TrackerQueue : ITrackerQueue, IDisposable
    {
        private readonly IOptions<TrackerQueueOptions> _options;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memCache;
        private readonly Timer _pollTimer;
        private readonly IServiceProvider _serviceProvider;
        public TrackerQueue(IServiceProvider serviceProvider, ILoggerFactory logger, IMemoryCache memCache, IOptions<TrackerQueueOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger.CreateLogger<TrackerQueue>();
            LogEvent(TrackerBackendEvent.RegisterQueue);
            _memCache = memCache;
            _options = options;
            _options.Value.Validate();
            int pollingRate = (int)options.Value.PollInterval.TotalMilliseconds;
            if (pollingRate <= 0 || pollingRate > 100000)
            {
                throw new ArgumentOutOfRangeException(nameof(pollingRate));
            }
            _pollTimer = new Timer(async (evt) =>
            {
                await PollQueueAndRunJobsAsync();
            }, null, 0, pollingRate);
        }
        public void LogEvent(TrackerBackendEvent trackerBackendEvent, string info = null) => TrackerLogMessages.BackendMessages[trackerBackendEvent].Invoke(_logger, info, null);
        public async Task<TrackerJobResult> EnqueueAndWaitJobAsync(Type trackerJobBuilderType, IEnumerable<KeyValuePair<string, object>> optionValues, uint jobPrioritySmallerFirst)
        {
            return await BuildAndExecuteJob(new TrackerEnqueuedJob() { OptionValues = new Dictionary<string, object>(optionValues), RunAfterDate = DateTime.Now, JobBuilderTypeName = trackerJobBuilderType.ToString(), Priority = jobPrioritySmallerFirst });

        }
        public async Task<TrackerEnqueuedJob> EnqueueJobAsync(Type trackerJobBuilderType, IEnumerable<KeyValuePair<string, object>> optionValues, TimeSpan runAfterTime, uint jobPrioritySmallerFirst)
        {
            var jobQueue = await GetJobQueueAsync();
            var result = new TrackerEnqueuedJob() { OptionValues = new Dictionary<string, object>(optionValues), RunAfterDate = DateTime.Now.Add(runAfterTime), JobBuilderTypeName = trackerJobBuilderType.ToString(), Priority = jobPrioritySmallerFirst };
            jobQueue.Enqueue(result);
            _logger.LogDebug($"{result.CreatedDate} Added job to queue (priority {result.Priority}), scheduled for {result.RunAfterDate}.");
            return result;
        }
        // 0 => not polling, 1 => polling, shared by multiple threads
        private static int _isPolling = 0;
        public async Task PollQueueAndRunJobsAsync()
        {
            int hasBeenPolling = Interlocked.CompareExchange(ref _isPolling, 1, 0);
            if (hasBeenPolling == 1)
            {
                return;
            }
            try
            {
                // prioritize jobs that are scheduled with 0 < 1 < 2 < ... < jobPrioritySmallerFirst_max priority
                // same priority => run in FIFO order
                // strict order per user
                var jobQueue = await GetJobQueueAsync();
                var runningJobs = await GetRunningJobsAsync();
                var runningCount = runningJobs.Count;
                while (jobQueue.TryPeek(out var enqueuedJob))
                {
                    if (runningCount <= _options.Value.MaxJobs && enqueuedJob.RunAfterDate < DateTime.Now && enqueuedJob.Priority == 0) // TODO other priorities are not handled
                    {
                        if (!jobQueue.TryDequeue(out enqueuedJob))
                        {
                            throw new ApplicationException("Could not remove job from job queue.");
                        }
                        var executedTask = BuildAndExecuteJob(enqueuedJob);
                        runningJobs.Enqueue(executedTask);
                    }
                }
                var completedJobs = new Queue<Task<TrackerJobResult>>() { };
                var stillRunningQueue = new Queue<Task<TrackerJobResult>>();
                while (runningJobs.TryDequeue(out var runningJob))
                {
                    if (runningJob.IsCompleted)
                    {
                        completedJobs.Enqueue(runningJob);
                    }
                    else
                    {
                        stillRunningQueue.Enqueue(runningJob);
                    }
                }
                while (completedJobs.TryDequeue(out var completedJob))
                {
                    if (completedJob.Result.Success)
                    {
                        _logger.LogInformation($"Job finished with success: {completedJob.Result.StatusMessage}");
                    }
                    else
                    {
                        _logger.LogError($"Job finished with error: {completedJob.Result.StatusMessage}");
                    }
                }
                while (stillRunningQueue.TryDequeue(out var stillRunningJob))
                {
                    runningJobs.Enqueue(stillRunningJob);
                }
            }
            catch (Exception ex)
            {
                // catch exceptions outside webb app exception flow
                // loop is halted when unhandled exception is thrown
                _logger.LogCritical($"Unhandled exception while polling jobs: {ex.Message}");
            }
            finally
            {
                Interlocked.CompareExchange(ref _isPolling, 0, 1);
            }
        }

        private Task<TrackerJobResult> BuildAndExecuteJob(TrackerEnqueuedJob enqueuedJob)
        {
            var targetBuilderType = Type.GetType(enqueuedJob.JobBuilderTypeName);
            if (targetBuilderType == null)
            {
                throw new ApplicationException($"Could not find builder type {enqueuedJob.JobBuilderTypeName}.");
            }
            var jobBuilder = _serviceProvider.GetService(targetBuilderType);
            if (jobBuilder == null)
            {
                throw new ApplicationException($"Could not get instance of {targetBuilderType.ToString()} from DI.");
            }
            (jobBuilder as TrackerJobBuilder).Configure(enqueuedJob.OptionValues);
            var createdJob = (jobBuilder as TrackerJobBuilder).Build();
            _logger.LogInformation($"Starting job {createdJob.Name}.");
            return Task.Run(async () =>
            {
                try
                {
                    // TODO check access rights when process is executed
                    // TODO check System.Threading.ThreadPool
                    return await createdJob.RunAsync();
                }
                catch (Exception ex)
                {
                    return new TrackerJobResult() { Success = false, StatusMessage = ex.Message, Name = createdJob.Name };
                }
            });
        }

        private async Task<ConcurrentQueue<TrackerEnqueuedJob>> GetJobQueueAsync()
        {
            return await _memCache.GetOrCreateAsync("TrackerWorkerQueue", e => Task.FromResult(new ConcurrentQueue<TrackerEnqueuedJob>()));
        }
        private async Task<ConcurrentQueue<Task<TrackerJobResult>>> GetRunningJobsAsync()
        {
            return await _memCache.GetOrCreateAsync("TrackerWorkerRunning", e => Task.FromResult(new ConcurrentQueue<Task<TrackerJobResult>>()));
        }
        public void Dispose()
        {
            _memCache.Dispose();
            _pollTimer.Dispose();
        }
    }
}
