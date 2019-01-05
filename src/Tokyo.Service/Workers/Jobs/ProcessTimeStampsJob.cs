using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Data;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Options.Jobs;
using Tokyo.Service.Services;

namespace Tokyo.Service.Workers.Jobs
{
    public class ProcessTimeStampsJob : TrackerJob
    {
        private readonly TrackerDbStateSqlite _trackerDbStateSqlite;

        public ProcessTimeStampsJob(IServiceProvider serviceProvider, ProcessTimeStampsJobOptions options) : base(serviceProvider)
        {
            Options = options;
            _trackerDbStateSqlite = ServiceProvider.GetRequiredService<TrackerDbStateSqlite>(); ;
        }

        public ProcessTimeStampsJobOptions Options { get; } // TODO override base class of options?
        public override async Task<TrackerJobResult> RunAsync()
        {
            var serviceScope = ServiceProvider.CreateScope();
            var data = serviceScope.ServiceProvider.GetRequiredService<TokyoDbContext>();
            // TODO check access rights when process is executed
            // TODO parallelization issues => lock tables (similar to edit timenorm position)
            var result = new TrackerJobResult() { Success = false, StatusMessage = $"No unprocessed timestamps for user {Options.UserId}.", Name = Name }; // TODO User id should be printed in every log message
            var userStore = await data.TrackerStores
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.WorkUnits)
                        .ThenInclude(www => www.TimeNorms)
                .Include(t => t.TimeStamps)
                .Include(t => t.TrackerUserDefaults)
                    .ThenInclude(tu => tu.TargetWorkUnit)
                .SingleOrDefaultAsync(t => t.TrackerStoreId == Options.UserId);

            if (userStore == null)
            {
                result.StatusMessage = $"Could not find tracker store for user ${Options.UserId}.";
            }
            var boundNormPropertyName = $"{nameof(TimeNorm)}Id";
            var unprocessedTimeStamps = userStore.UnboundTimeStamps.OrderBy(t => t.TrackedTimeUTC.UtcDateTime).ToList();


            var targetTable = userStore.TimeTables.LastOrDefault();
            if (unprocessedTimeStamps.Count > 1)
            {
                if (targetTable == null)
                {
                    targetTable = new TimeTable();
                    userStore.TimeTables.Add(targetTable);
                }
                // process timestamps: most recent timetable, most recent work unit, norm without end time gets first timestamp
                // bind every pair of 2 timestamps to a norm and add to work unit
                var createdNorms = new List<TimeNorm>();
                WorkUnit workUnit = null;
                while (unprocessedTimeStamps.Count >= 2)
                {
                    if (workUnit == null)
                    {
                        if (Options.TargetWorkUnitId.HasValue)
                        {
                            workUnit = await data.FindAsync<WorkUnit>(Options.TargetWorkUnitId);
                        }
                        if (workUnit == null)
                        {
                            workUnit = targetTable.WorkUnits.LastOrDefault();
                            if (workUnit == null)
                            {
                                workUnit = new WorkUnit()
                                {
                                    ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementWorkUnitSortKey()
                                };
                                targetTable.WorkUnits.Add(workUnit);
                            }
                        }
                        userStore.TrackerUserDefaults.TargetWorkUnit = workUnit;
                    }
                    var timeNorm = new TimeNorm()
                    {
                        ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementTimeNormSortKey()
                    };
                    var startTimeStamp = unprocessedTimeStamps.ElementAt(0);
                    var endTimeStamp = unprocessedTimeStamps.ElementAt(1);
                    timeNorm.StartTime = startTimeStamp;
                    startTimeStamp.BoundNormStart = timeNorm;
                    timeNorm.EndTime = endTimeStamp;
                    endTimeStamp.BoundNormEnd = timeNorm;
                    workUnit.TimeNorms.Add(timeNorm);

                    createdNorms.Add(timeNorm);
                    unprocessedTimeStamps.Remove(startTimeStamp);
                    unprocessedTimeStamps.Remove(endTimeStamp);
                }
                /*if (workUnit != null && workUnit.Name == "UNSET_WORKUNIT") // TODO default value handling is not good / should be null(able) and const default value client side
                {
                    var startEndDate = workUnit.GetStartAndEndDate();
                    workUnit.Name = $"WorkUnit {startEndDate.Item1.ToString("R")} - {startEndDate.Item2.ToString("R")}";
                } */
                await data.SaveChangesAsync();
                result.StatusMessage = $"Created {createdNorms.Count} time norms for user {Options.UserId}.";
            }
            result.Success = true;
            return result;
        }
    }
}
