using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Data;
using Tokyo.Service.Extensions;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Options;
using Tokyo.Service.Options.Jobs;
using Tokyo.Service.Workers;
using Tokyo.Service.Workers.Jobs;
using static Tokyo.Service.Models.TrackerLogMessages;

namespace Tokyo.Service.Services
{
    // TODO check all the generated SQL queries
    // TODO make sure null returned when error is logged everywhere
    // TODO all db calls async
    public class TrackerService
    {
        private readonly ILogger _logger;
        private readonly TokyoDbContext _data;
        private readonly ITrackerQueue _queue;
        private readonly IAuthorizationService _authorizationService;
        private readonly TrackerDbStateSqlite _trackerDbStateSqlite;
        private readonly TrackerServiceOptions _trackerServiceOptions;

        public TrackerService(ILoggerFactory logger, TokyoDbContext data, ITrackerQueue queue, 
            IAuthorizationService authorizationService, IOptions<TrackerServiceOptions> trackerServiceOptions, TrackerDbStateSqlite trackerDbStateSqlite)
        {
            _logger = logger.CreateLogger<TrackerService>();
            _data = data;
            _queue = queue;
            _authorizationService = authorizationService;
            _trackerDbStateSqlite = trackerDbStateSqlite;
            _trackerServiceOptions = trackerServiceOptions.Value;
        }
        public async Task<WorkUnit> GetDefaultTargetWorkUnitAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .AsNoTracking()
                .Include(t => t.TrackerUserDefaults)
                    .ThenInclude(tu => tu.TargetWorkUnit)
                .SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
            if (userStore.TrackerUserDefaults.TargetWorkUnit == null)
            {
                userStore = await _data.TrackerStores
                    .Include(t => t.TimeTables)
                        .ThenInclude(tt => tt.WorkUnits)
                    .Include(t => t.TrackerUserDefaults)
                        .ThenInclude(tu => tu.TargetWorkUnit)
                    .SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
                userStore.TrackerUserDefaults.TargetWorkUnit = userStore.TimeTables.First().WorkUnits.First();
                await _data.SaveChangesAsync();
            }
            return userStore.TrackerUserDefaults.TargetWorkUnit;
        }

        public async Task<TimeZoneInfo> GetDefaultTimeZoneAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .Include(t => t.TrackerUserDefaults)
                .SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
            if (!string.IsNullOrEmpty(userStore.TrackerUserDefaults.TimeZoneId))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(userStore.TrackerUserDefaults.TimeZoneId);
            }
            else
            {
                userStore.TrackerUserDefaults.TimeZoneId = _trackerServiceOptions.DefaultUserTimeZone.Id;
                await _data.SaveChangesAsync();
                return _trackerServiceOptions.DefaultUserTimeZone;
            }
        }

        public async Task LogTrackerContextAsync(TrackerContext trackerContext) // TODO code duplication
        {
            // TODO per user/project count?
            // TODO auto generate entities and store in cache
            // TODO parallelization issues with such type of counter
            // TODO order of calls also interesting and later on relations between targeted data
            // TODO front end calls are not logged
            var trackerEventLog = await _data.TrackerEventLogs.FirstOrDefaultAsync(e => e.TrackerEvent == trackerContext.TrackerEvent);
            if (trackerEventLog == null)
            {
                trackerEventLog = new TrackerEventLog() { TrackerEvent = trackerContext.TrackerEvent };
            }
            trackerEventLog.NTimesCalled++;
            await _data.SaveChangesAsync();
        }

        public async Task<List<SharedTimeTableInfo>> GetSharedByOtherTableInfosAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .AsNoTracking()
                .Include(t => t.ForeignSharedTimeTableInfos)
                .SingleOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            return userStore.ForeignSharedTimeTableInfos;
        }
        public async Task<List<SharedTimeTableInfo>> GetSharedWithOtherTableInfosAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .AsNoTracking()
                .Include(t => t.OwnedSharedTimeTableInfos)
                .SingleOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            return userStore.OwnedSharedTimeTableInfos;
        }

        public async Task<Tuple<int, int>> MoveTimeNormToWorkUnit(int moveTimeNormId, int targetWorkUnitId)  // TODO fix
        {
            var moveTimeNorm = await _data.FindAsync<TimeNorm>(moveTimeNormId);
            var originalWorkUnitId = moveTimeNorm.WorkUnitId;
            if (moveTimeNorm.WorkUnitId == targetWorkUnitId)
            {
                throw new InvalidOperationException("Move time norm already in target work unit.");
            }
            var targetWorkUnit = await _data.Set<WorkUnit>().Include(w => w.TimeNorms).FirstAsync(w => w.WorkUnitId == targetWorkUnitId);
            if (targetWorkUnit.WorkUnitId == moveTimeNorm.WorkUnitId)
            {
                // same owner table
                targetWorkUnit.TimeNorms.Remove(moveTimeNorm);
            }
            else
            {
                // TODO is this tracked correctly?
                var originalWorkUnit = await _data
                    .Set<WorkUnit>()
                    .Include(w => w.TimeNorms)
                    .FirstAsync(w => w.WorkUnitId == moveTimeNorm.WorkUnitId);
                originalWorkUnit.TimeNorms.Remove(moveTimeNorm);
            }
            targetWorkUnit.TimeNorms.Add(moveTimeNorm);
            await _data.SaveChangesAsync();
            return new Tuple<int, int>(originalWorkUnitId, targetWorkUnitId);
        }

        public async Task<Tuple<int, int>> MoveWorkUnitAsync(TrackerContext trackerContext, int moveWorkUnitId, int targetWorkUnitId, bool IsInsertBeforeTarget)
        {
            // TODO also add no change case where target is already behind move work unit
            if (moveWorkUnitId == targetWorkUnitId)
            {
                throw new InvalidOperationException("Move and target id must be different.");
            }

            // TODO compare Serializable to dbContext.UpdateRange()
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                var moveWorkUnit = await _data.Set<WorkUnit>().Include(w => w.TimeTable).FirstAsync(w => w.WorkUnitId == moveWorkUnitId);
                var targetWorkUnit = await _data.Set<WorkUnit>().Include(w => w.TimeTable).FirstAsync(w => w.WorkUnitId == targetWorkUnitId);

                // move work unit to different time table
                var originalTable = moveWorkUnit.TimeTable;
                var targetTable = targetWorkUnit.TimeTable;
                if (originalTable != targetTable)
                {
                    await _data.Entry<TimeTable>(targetTable).Collection(t => t.WorkUnits).LoadAsync();
                    originalTable.WorkUnits.Remove(moveWorkUnit);
                    targetTable.WorkUnits.Add(moveWorkUnit);
                }

                // update work unit position key: example: user (A) moves item 5 in front of item 2; example*: user (A) moves item 2 in front of item 6
                // element => positionKey
                // state1:             |     state2:                |     state2*:
                // (B)item 1 => 1      |     (B)item 1 => 1         |     (B)item 1 => 1
                // (A)item 2 => 2      |     (A)item 2 => -4-       |     (A)item 2 => -5-
                // (B)item 3 => 3      |     (B)item 3 => 3         |     (B)item 3 => 3
                // (A)item 4 => 4      |     (A)item 4 => -5-       |     (A)item 4 => -2-
                // (A)item 5 => 5      |     (A)item 5 => -2-       |     (A)item 5 => -4-
                // (A)item 6 => 6      |     (A)item 6 => 6         |     (A)item 6 => 6
                // (A)item 7 => 7      |     (A)item 7 => 7         |     (A)item 7 => 7
                // (A)item 8 => 8      |     (A)item 8 => 8         |     (A)item 8 => 8
                // (B)item 9 => 9      |     (B)item 9 => 9         |     (B)item 9 => 9
                // (B)item 10 => 10    |     (B)item 10 => 10       |     (B)item 10 => 10

                bool isMoveDirectionUp = moveWorkUnit.ManualSortOrderKey > targetWorkUnit.ManualSortOrderKey;
                var minAffectedSortKey = isMoveDirectionUp ? targetWorkUnit.ManualSortOrderKey : moveWorkUnit.ManualSortOrderKey; // TODO assert with math min
                var maxAffectedSortKey = isMoveDirectionUp ? moveWorkUnit.ManualSortOrderKey : (targetWorkUnit.ManualSortOrderKey - 1);

                List<WorkUnit> affectedWorkUnits;
                if (_trackerServiceOptions.DatabaseTechnology == TrackerServiceOptions.DbEngineSelector.Mssql)
                {
                    affectedWorkUnits = await _data.Set<WorkUnit>().FromSql($"SELECT * FROM dbo.WorkUnit WITH (TABLOCKX, HOLDLOCK) WHERE (TimeTableId={targetTable.TimeTableId} AND ManualSortOrderKey>={minAffectedSortKey} AND ManualSortOrderKey<={maxAffectedSortKey})").OrderBy(w => w.ManualSortOrderKey).ToListAsync();
                }
                else if (_trackerServiceOptions.DatabaseTechnology == TrackerServiceOptions.DbEngineSelector.Sqlite)
                {
                    affectedWorkUnits = await _data.Set<WorkUnit>().FromSql($"SELECT * FROM WorkUnit WHERE (TimeTableId={targetTable.TimeTableId} AND ManualSortOrderKey>={minAffectedSortKey} AND ManualSortOrderKey<={maxAffectedSortKey})").OrderBy(w => w.ManualSortOrderKey).ToListAsync();
                }
                else
                {
                    throw new NotImplementedException(_trackerServiceOptions.DatabaseTechnology.ToString());
                }
                /*var affectedWorkUnits = await _data.Set<WorkUnit>().FromSql("SELECT * FROM dbo.{0} WITH (TABLOCKX, HOLDLOCK) "
                    + "WHERE ({1}={2} "
                        + "AND {3}>={4} "
                        + "AND {3}<={5}) "
                     + "ORDER BY {3};", nameof(WorkUnit), nameof(WorkUnit.TimeTableId), targetTable.TimeTableId, nameof(WorkUnit.ManualSortOrderKey), minAffectedSortKey, maxAffectedSortKey).ToListAsync();*/
                //var affectedWorkUnits = await _data.Set<WorkUnit>().FromSql($"SELECT * FROM dbo.{nameof(WorkUnit)} WITH (TABLOCKX) "
                //var affectedWorkUnits = await _data.Set<WorkUnit>().FromSql($"SELECT * FROM dbo.{nameof(WorkUnit)} WITH (XLOCK, ROWLOCK) "
                //var affectedWorkUnits = await _data.Set<WorkUnit>().FromSql($"SELECT * FROM dbo.{nameof(WorkUnit)} WITH (UPDLOCK) "


                var affectedCount = affectedWorkUnits.Count;
                var previousIndexes = affectedWorkUnits.Select(w => w.ManualSortOrderKey).ToList();

                for (int i = 0; i < affectedCount; i++)
                {
                    var takeFromIndex = isMoveDirectionUp ? (i + 1) : (i - 1);
                    if (takeFromIndex < 0)
                    {
                        takeFromIndex = affectedCount - 1;
                    }
                    else if (takeFromIndex == affectedCount)
                    {
                        takeFromIndex = 0;
                    }
                    affectedWorkUnits[i].ManualSortOrderKey = previousIndexes[takeFromIndex];
                }
                //await Task.Delay(10000); TODO tests
                var affectedRows = await _data.SaveChangesAsync();
                if (affectedRows != affectedCount)
                {
                    _logger.LogError((int)trackerContext.TrackerEvent, $"Changing position for work unit {targetWorkUnitId} failed.");
                    throw new ApplicationException();
                }

                transaction.Commit();
                // TODO is rollback called automatically if something goes wrong?
                return new Tuple<int, int>(originalTable.TimeTableId, targetTable.TimeTableId);
            }
        }

        public async Task<TimeTable> GetFullTimeTableAsync(TrackerContext trackerContext, int timeTableId)
        {
            var timeTable = await _data.Set<TimeTable>()
                .AsNoTracking()
                .Include(t => t.WorkUnits)
                    .ThenInclude(wu => wu.TimeNorms)
                        .ThenInclude(tn => tn.StartTime)
                .Include(t => t.WorkUnits)
                    .ThenInclude(wu => wu.TimeNorms)
                        .ThenInclude(tn => tn.EndTime)
                .Include(t => t.SharedTimeTableInfos)
                .Include(t => t.TargetWeeklyTime)
                .SingleAsync(t => t.TimeTableId == timeTableId);
            return timeTable;
        }

        public async Task<string[]> GetTableOwnerNamesAsync(TrackerContext trackerContext, IEnumerable<int> timeTableIds)
        {
            var timeTables = await _data.Set<TimeTable>()
                .AsNoTracking()
                .Where(t => timeTableIds.Contains(t.TimeTableId))
                .Include(t => t.TrackerStore)
                .ToListAsync();
            return timeTables.Select(t => t.TrackerStore.TrackerStoreId).ToArray();
        }

        public async Task<bool> RemoveTimeNormTagAsync(TrackerContext trackerContext, int timeNormTagId)
        {
            var timeNormTag = await _data.FindAsync<TimeNormTag>(timeNormTagId);
            _data.Remove(timeNormTag);
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not delete time norm tag {timeNormTagId}.");
                return false;
            }
            return true;
        }

        // TODO authorize access to resources / only query data from user store
        public async Task ShareTablesAsync(TrackerContext trackerContext, IEnumerable<int> timeTableIds, string shareWithStoreId)
        {
            var logMessages = new List<string>() { $"For owner {trackerContext.UserId}:" };
            var shareWithStore = await _data.TrackerStores.SingleAsync(t => t.TrackerStoreId == shareWithStoreId); // TODO test case: delete target store while sharing tables
            if (shareWithStoreId == trackerContext.UserId)
            {
                throw new InvalidOperationException($"Owner store and share with store {shareWithStoreId} may not be equal."); // TODO testcase
            }
            foreach (var timeTableId in timeTableIds)
            {
                var timeTableToShare = await _data.FindAsync<TimeTable>(timeTableId);
                if (timeTableToShare == null)
                {
                    _logger.LogError($"Could not find table {timeTableId}.");
                    continue;
                }
                if (!timeTableToShare.SharedTimeTableInfos.Any(si => si.SharedWithTrackerStore == shareWithStore))
                {
                    var sharedTimeTableInfo = new SharedTimeTableInfo() { OwnerTrackerStore = timeTableToShare.TrackerStore, SharedWithTrackerStore = shareWithStore, TimeTable = timeTableToShare };
                    timeTableToShare.SharedTimeTableInfos.Add(sharedTimeTableInfo);
                    logMessages.Add($"Shared table {timeTableId} with {shareWithStore.TrackerStoreId}.");
                }
                else
                {
                    logMessages.Add($"Table {timeTableId} already shared with {shareWithStore.TrackerStoreId}.");
                }
            }
            await _data.SaveChangesAsync();
            LogEvent(trackerContext.TrackerEvent, string.Join(" ", logMessages));
        }
        public async Task UnShareTablesAsync(TrackerContext trackerContext, IEnumerable<int> timeTableIds)
        {
            var userStore = await _data.TrackerStores
                .Include(t => t.TimeTables)
                .ThenInclude(tt => tt.SharedTimeTableInfos)
                .SingleOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            foreach (var timeTableId in timeTableIds)
            {
                var timeTableToUnShare = await _data.FindAsync<TimeTable>(timeTableId);
                if (timeTableToUnShare == null)
                {
                    _logger.LogError($"Could not find table {timeTableId}.");
                    continue;
                }
                timeTableToUnShare.SharedTimeTableInfos.Clear();
            }
            await _data.SaveChangesAsync();
        }

        public async Task<TimeStamp> DuplicateTimeStampAsync(TrackerContext trackerContext, int timeStampId)
        {
            // TODO only read requirement on other timestamp => is query tracked?
            var userStore = await _data.TrackerStores.Include(t => t.TimeStamps).SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);

            var referenceTimeStamp = await _data.Set<TimeStamp>().FirstAsync(t => t.TimeStampId == timeStampId);

            var newTimeStamp = new TimeStamp()
            {
                TrackedTimeUTC = referenceTimeStamp.TrackedTimeUTC,
                UtcOffsetAtCreation = referenceTimeStamp.UtcOffsetAtCreation,
                TimeZoneIdAtCreation = referenceTimeStamp.TimeZoneIdAtCreation
            };

            userStore.TimeStamps.Add(newTimeStamp);
            var rowsChanged = await _data.SaveChangesAsync();
            LogEvent(trackerContext.TrackerEvent, $"UTC: {newTimeStamp.TrackedTimeUTC.ToUniversalTime()}");
            if (rowsChanged != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Duplicate timestamp for user {trackerContext.UserId} failed.");
            }
            return newTimeStamp;
        }
        public async Task<TimeStamp> CreateTimeStampAsync(TrackerContext trackerContext, TimeZoneInfo localTimeZone)
        {
            var userStore = await _data.TrackerStores.Include(t => t.TimeStamps).SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
            var newTimeStamp = new TimeStamp()
            {
                UtcOffsetAtCreation = localTimeZone.GetUtcOffset(DateTime.UtcNow), // TODO code duplication w/ http get parse
                TimeZoneIdAtCreation = localTimeZone.Id
            };

            userStore.TimeStamps.Add(newTimeStamp);
            var rowsChanged = await _data.SaveChangesAsync(); // TODO DbUpdateConcurrencyException // TODO check if default value is sent back (generated in MSSQL DB)
            LogEvent(trackerContext.TrackerEvent, $"UTC: {newTimeStamp.TrackedTimeUTC.ToUniversalTime()}");
            if (rowsChanged != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Create timestamp for user {trackerContext.UserId} failed.");
            }
            return newTimeStamp;
        }

        public async Task<ProductivityRating> EditProductivityRatingAsync(TrackerContext trackerContext, int timeNormId, double productivityPercentage)
        {
            var productivityRating = await _data.Set<ProductivityRating>().FirstOrDefaultAsync(p => p.TimeNormId == timeNormId);
            if (productivityRating == null)
            {
                productivityRating = new ProductivityRating() { ProductivityPercentage = productivityPercentage };
                var timeNorm = await _data.Set<TimeNorm>().Include(t => t.ProductivityRating).FirstAsync(t => t.TimeNormId == timeNormId);
                timeNorm.ProductivityRating = productivityRating;
            }
            else
            {
                if (Math.Abs(productivityRating.ProductivityPercentage - productivityPercentage) < 1e-15)
                {
                    return productivityRating;
                }
                productivityRating.ProductivityPercentage = productivityPercentage;
            }
            await _data.SaveChangesAsync();
            return productivityRating;
        }

        public async Task<TimeStamp> AddTimeStampAsync(TrackerContext trackerContext, TimeStamp timeStamp)
        {
            var userStore = await _data.TrackerStores.Include(t => t.TimeStamps).SingleOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            userStore.TimeStamps.Add(timeStamp);
            var rowsChanged = await _data.SaveChangesAsync();
            LogEvent(trackerContext.TrackerEvent, $"UTC: {timeStamp.TrackedTimeUTC.ToUniversalTime()}");
            if (rowsChanged != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Create timestamp for user {trackerContext.UserId} failed.");
                return null;
            }
            return timeStamp;
        }

        public async Task<TimeNormTag> CreateTimeNormTagAsync(TrackerContext trackerContext, string name, string color)
        {
            var userStore = await _data.TrackerStores.Include(t => t.TimeNormTags).SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
            var createdTag = new TimeNormTag() { Color = color, Name = name };
            userStore.TimeNormTags.Add(createdTag);
            var rowsChanged = await _data.SaveChangesAsync();
            if (rowsChanged != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not create time norm tag.");
                return null;
            }
            return createdTag;
        }

        public async Task CreateUserStoreAndInitializeAsync(TrackerContext trackerContext)
        {
            var defaultTable = new TimeTable()
            {
                Name = "Table"
            };
            var defaultWorkUnit = new WorkUnit()
            {
                Name = "Times", // TODO same default value as in work unit
                ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementWorkUnitSortKey()
            };
            var createdStore = new TrackerStore()
            {
                TrackerStoreId = trackerContext.UserId
            };
            createdStore.TrackerUserDefaults.TargetWorkUnit = defaultWorkUnit;
            defaultTable.WorkUnits.Add(defaultWorkUnit);
            createdStore.TimeTables.Add(defaultTable);
            await _data.TrackerStores.AddAsync(createdStore);
            await _data.SaveChangesAsync();
        }

        private Dictionary<string, object> CreateUserJobOptionsDictionary(TrackerContext trackerContext)
        {
            if (string.IsNullOrEmpty(trackerContext.UserId))
            {
                throw new ArgumentNullException("User Id not set in Tracker Context.");
            }
            return new Dictionary<string, object>() { { "UserId", trackerContext.UserId } };
        }
        public async Task RemoveWorkUnitAsync(TrackerContext trackerContext, int workUnitId)
        {
            var workUnit = await _data.Set<WorkUnit>()
                .Include(w => w.TimeNorms)
                .FirstAsync(w => w.WorkUnitId == workUnitId);

            if (workUnit.TimeNorms.Count > 0)
            {
                throw new InvalidOperationException("Work unit can not be deleted while time norms are set.");
            }

            _data.Remove(workUnit);

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Delete work unit {workUnitId} of user {trackerContext.UserId} failed.");
            }
        }
        public async Task<bool> RemoveTimeTableAsync(TrackerContext trackerContext, int timeTableId)
        {
            var timeTable = await _data.Set<TimeTable>()
                .Include(t => t.WorkUnits)
                .FirstAsync(t => t.TimeTableId == timeTableId);
            if (timeTable.WorkUnits.Count > 0)
            {
                throw new InvalidOperationException("Time table may not contain work units.");
            }
            _data.Remove(timeTable);
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Delete time table {timeTableId} of user {trackerContext.UserId} failed.");
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveTimeStampAsync(TrackerContext trackerContext, int timeStampId)
        {
            var targetStore = await _data.TrackerStores
                .Include(t => t.TimeStamps)
                    .ThenInclude(ts => ts.BoundNormStart)
                .Include(t => t.TimeStamps)
                    .ThenInclude(ts => ts.BoundNormEnd)
                .FirstOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            var timeStamp = targetStore.TimeStamps.FirstOrDefault(ts => ts.TimeStampId == timeStampId);
            if (timeStamp == null)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not find timestamp {timeStampId}.");
                return false;
            }
            if (timeStamp.IsBound)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not remove timestamp {timeStampId} (unbind first).");
                return false;
            }
            _data.Remove(timeStamp);
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Delete timestamp {timeStampId} of user {trackerContext.UserId} failed.");
            }
            return affectedRows == 1;
        }

        public async Task<Tuple<TimeStamp, TimeNorm, string, string>> EditTimeStampAsync(TrackerContext trackerContext, int timeStampId, TimeStamp updatedTimeStamp)
        {
            // TODO if necessary, swap start and end time and notify client to update ID (update start timestamp + end timestamp)

            var timeStamp = await _data.Set<TimeStamp>()
                .Include(t => t.BoundNormStart)
                    .ThenInclude(tn => tn.EndTime) // required for duration calculation
                .Include(t => t.BoundNormStart)
                    .ThenInclude(tn => tn.ProductivityRating) // TODO required for UI?
                .Include(t => t.BoundNormEnd)
                    .ThenInclude(tn => tn.StartTime) // required for duration calculation
                .Include(t => t.BoundNormEnd)
                    .ThenInclude(tn => tn.ProductivityRating)
                .Include(t => t.BoundNormStart)
                    .ThenInclude(tn => tn.EndTime) // required for duration calculation
                // next four blocks are only to calculate duration TODO cache
                .Include(t => t.BoundNormStart)
                    .ThenInclude(no => no.WorkUnit)
                        .ThenInclude(wou => wou.TimeNorms)
                            .ThenInclude(tino => tino.StartTime)
                .Include(t => t.BoundNormStart)
                    .ThenInclude(no => no.WorkUnit)
                        .ThenInclude(wou => wou.TimeNorms)
                            .ThenInclude(tino => tino.EndTime)
                .Include(t => t.BoundNormEnd)
                    .ThenInclude(no => no.WorkUnit)
                        .ThenInclude(wou => wou.TimeNorms)
                            .ThenInclude(tino => tino.StartTime)
                .Include(t => t.BoundNormEnd)
                    .ThenInclude(no => no.WorkUnit)
                        .ThenInclude(wou => wou.TimeNorms)
                            .ThenInclude(tino => tino.EndTime)
                .FirstAsync(t => t.TimeStampId == timeStampId);

            TimeNorm boundNorm = null;
            if (timeStamp.BoundNormStart != null)
            {
                boundNorm = timeStamp.BoundNormStart;
            }
            else if (timeStamp.BoundNormEnd != null)
            {
                boundNorm = timeStamp.BoundNormEnd;
            }

            timeStamp.TrackedTimeUTC = updatedTimeStamp.TrackedTimeUTC;
            timeStamp.Name = updatedTimeStamp.Name;

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Edit timestamp {timeStampId} of user {trackerContext.UserId} failed.");
            }

            return new Tuple<TimeStamp, TimeNorm, string, string>(timeStamp, boundNorm, boundNorm != null ? boundNorm.DurationString : null, boundNorm != null ? boundNorm.WorkUnit.DurationString : null);
        }



        public async Task<bool> EditTimeTableAsync(TrackerContext trackerContext, int timeTableId, string updatedTimeTableName)
        {
            var timeTable = await _data.FindAsync<TimeTable>(timeTableId);
            if (timeTable.Name == updatedTimeTableName)
            {
                return true;
            }
            timeTable.Name = updatedTimeTableName;
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not edit time table {timeTableId}");
                return false;
            }
            return true;
        }

        public async Task<TimeNorm> EditTimeNormAsync(TrackerContext trackerContext, int timeNormId, string updatedTimeNormName, int updatedTimeNormColorR, int updatedTimeNormColorG, int updatedTimeNormColorB)
        {
            // TODO nice time norm name handling
            if (updatedTimeNormColorR < 0 || updatedTimeNormColorR > 255
                || updatedTimeNormColorG < 0 || updatedTimeNormColorG > 255
                || updatedTimeNormColorB < 0 || updatedTimeNormColorB > 255)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Invalid color values supplied for {timeNormId}.");
                return null;
            }
            var timeNorm = await _data.Set<TimeNorm>()
                .Include(t => t.StartTime) // required for duration calculation
                .Include(t => t.EndTime)
                .Include(t => t.ProductivityRating)
                .FirstAsync(t => t.TimeNormId == timeNormId);
            if (timeNorm == null)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not find time norm {timeNormId}.");
                return null;
            }

            if ((!string.IsNullOrEmpty(updatedTimeNormName) && (timeNorm.Name == updatedTimeNormName))
                && timeNorm.ColorR == updatedTimeNormColorR
                && timeNorm.ColorG == updatedTimeNormColorG
                && timeNorm.ColorB == updatedTimeNormColorB)
            {
                return timeNorm;
            }

            if (!string.IsNullOrEmpty(updatedTimeNormName))
            {
                timeNorm.Name = updatedTimeNormName;
            }

            timeNorm.ColorR = updatedTimeNormColorR;
            timeNorm.ColorG = updatedTimeNormColorG;
            timeNorm.ColorB = updatedTimeNormColorB;

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not edit time norm {timeNormId}");
                return null;
            }

            return timeNorm;
        }

        public async Task<bool> EditWorkUnitAsync(TrackerContext trackerContext, int workUnitId, string updatedWorkUnitName)
        {
            var workUnit = await _data.FindAsync<WorkUnit>(workUnitId);

            if (workUnit.Name == updatedWorkUnitName)
            {
                return true;
            }

            workUnit.Name = updatedWorkUnitName;

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not edit work unit {workUnitId}");
                return false;
            }
            return true;
        }

        public async Task<TimeTable> CreateTimeTableAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .Include(t => t.TimeTables)
                .FirstAsync(t => t.TrackerStoreId == trackerContext.UserId);
            var newTimeTable = new TimeTable();
            var newWorkUnit = new WorkUnit()
            {
                ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementWorkUnitSortKey()
            };
            newTimeTable.WorkUnits.Add(newWorkUnit);
            userStore.TimeTables.Add(newTimeTable);
            await _data.SaveChangesAsync();
            return newTimeTable;
        }

        public async Task<WorkUnit> CreateWorkUnitAsync(TrackerContext trackerContext, int timeTableId)
        {
            var timeTable = await _data.Set<TimeTable>().Include(t => t.WorkUnits).FirstAsync(t => t.TimeTableId == timeTableId);

            var newWorkUnit = new WorkUnit()
            {
                ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementWorkUnitSortKey()
            };
            timeTable.WorkUnits.Add(newWorkUnit);

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Create work unit in table {timeTableId} of user {trackerContext.UserId} failed.");
                return null;
            }

            return newWorkUnit;
        }

        /*public async Task<WorkUnit> CreateWorkUnitAndMoveAsync(TrackerContext trackerContext, int timeTableId, int beforeWorkUnitId) TODO unused => client side operation
        {
            var timeTable = await _data.Set<TimeTable>().Include(t => t.WorkUnits).FirstAsync(t => t.TimeTableId == timeTableId);
            // TODO need full table lock from start to end
            var newWorkUnit = new WorkUnit()
            {
                ManualSortOrderKey = _trackerDbStateSqlite.GetAndIncrementWorkUnitSortKey()
            };
            timeTable.WorkUnits.Add(newWorkUnit);

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Create work unit in table {timeTableId} of user {trackerContext.UserId} failed.");
                return null;
            }
            await MoveWorkUnitAsync(trackerContext, newWorkUnit.WorkUnitId, beforeWorkUnitId, true); // TODO roll back add workunit if move failed
            // TODO test other work units moved in the meantime etc
            return newWorkUnit;
        }*/

        public async Task<(string TableName, byte[] Data)> ExportTimeTableToCSVAsync(TrackerContext trackerContext, TrackerClientSession clientSession, int timeTableId)
        {
            var timeTable = await _data.Set<TimeTable>()
                .AsNoTracking()
                .Include(t => t.WorkUnits)
                    .ThenInclude(wu => wu.TimeNorms)
                        .ThenInclude(tim => tim.StartTime)
                .Include(t => t.WorkUnits)
                    .ThenInclude(wu => wu.TimeNorms)
                        .ThenInclude(tim => tim.EndTime)
                .FirstAsync(t => t.TimeTableId == timeTableId);

            timeTable.TranformTrackedTimesForView(clientSession);

            var timeNorms = timeTable.WorkUnits.SelectMany(w => w.TimeNorms);
            var resultCSV = new List<string>();

            //if (timeNorms.Count > 10000) TODO MB limit

            foreach (var timeNorm in timeNorms)
            {
                string dataLine = $"{timeNorm.Name.Replace(",", ";")/*TODO document*/},{timeNorm.StartTime.TrackedTimeUTC.ToString()},{timeNorm.EndTime.TrackedTimeUTC.ToString()},{timeNorm.StartTime.TrackedTimeAtCreation.ToString()},{timeNorm.EndTime.TrackedTimeAtCreation.ToString()},{timeNorm.StartTime.TrackedTimeForView.ToString()},{timeNorm.EndTime.TrackedTimeForView.ToString()}";
                resultCSV.Add(dataLine);
            }

            return (timeTable.Name, Encoding.UTF8.GetBytes(string.Join('\n', resultCSV)));
        }

        public async Task<Plot> GetTimeTablePlotAsync(TrackerContext trackerContext, int timeTableId)
        {
            var targetTimeTable = await _data.Set<TimeTable>().Include(t => t.Plot).FirstOrDefaultAsync(t => t.TimeTableId == timeTableId);
            if (targetTimeTable == null)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Could not find time table {timeTableId}.");
                return null;
            }

            if (targetTimeTable.Plot == null)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Plot for time table {timeTableId} does not exist.");
                return null;
            }

            return targetTimeTable.Plot;
        }

        public async Task<WorkUnit> SetWorkUnitAsDefaultTargetAsync(TrackerContext trackerContext, int? workUnitId)
        {
            // TODO should lock when moveWorkUnit has TABLOCKX => try with raw SELECT statement (circumvent changeTracker/context)
            var userStore = await _data.TrackerStores
                .Include(t => t.TrackerUserDefaults)
                    .ThenInclude(tu => tu.TargetWorkUnit)
                .SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);

            if (workUnitId.HasValue)
            {
                if (userStore.TrackerUserDefaults.TargetWorkUnit?.WorkUnitId == workUnitId)
                {
                    return userStore.TrackerUserDefaults.TargetWorkUnit;
                }
                var targetWorkUnit = await _data.FindAsync<WorkUnit>(workUnitId);
                if (targetWorkUnit == null)
                {
                    _logger.LogError((int)trackerContext.TrackerEvent, $"Could not find work unit {workUnitId}.");
                    return null;
                }
                userStore.TrackerUserDefaults.TargetWorkUnit = targetWorkUnit; // TODO lock table required (rapid changes of html select with keyboard => concurrencyEx)
            }
            else
            {
                if (userStore.TrackerUserDefaults.TargetWorkUnit == null)
                {
                    return null;
                }
                userStore.TrackerUserDefaults.TargetWorkUnit = null;
            }
            /*TODO had exceptions when fast changes were triggered by selectinput => a) concurrency because of a [timestamp] in workunit b) unique constraint: active workunit => tracker store*/
            var affectedRows = await _data.SaveChangesAsync();
            return userStore.TrackerUserDefaults.TargetWorkUnit;
        }

        public async Task<TimeNormTag> EditTimeNormTagAsync(TrackerContext trackerContext, int timeNormTagId, TimeNormTag updatedTimeNormTag)
        {
            var timeNormTag = await _data.FindAsync<TimeNormTag>(timeNormTagId);

            timeNormTag.Name = updatedTimeNormTag.Name;
            timeNormTag.Color = updatedTimeNormTag.Color;

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Edit time norm tag {timeNormTagId} failed.");
                return null;
            }
            return timeNormTag;
        }
        public async Task<string> UnbindTimeNormAsync(TrackerContext trackerContext, int timeNormId, bool isDeleteTimeStamps)
        {
            TimeNorm targetTimeNorm;
            int expectedRowChanges = 1;
            if (isDeleteTimeStamps)
            {
                targetTimeNorm = await _data.Set<TimeNorm>()
                    .Include(t => t.StartTime)
                    .Include(t => t.EndTime)
                    .Include(t => t.WorkUnit)
                        .ThenInclude(wu => wu.TimeNorms)
                            .ThenInclude(tin => tin.StartTime)
                    .Include(t => t.WorkUnit)
                        .ThenInclude(wu => wu.TimeNorms)
                            .ThenInclude(tin => tin.EndTime)
                    .FirstAsync(t => t.TimeNormId == timeNormId);
                _data.Remove(targetTimeNorm.StartTime);
                _data.Remove(targetTimeNorm.EndTime);
                expectedRowChanges = 3;
            }
            else
            {
                targetTimeNorm = await _data.Set<TimeNorm>()
                    .Include(t => t.WorkUnit)
                        .ThenInclude(wu => wu.TimeNorms)
                            .ThenInclude(tin => tin.StartTime)
                    .Include(t => t.WorkUnit)
                        .ThenInclude(wu => wu.TimeNorms)
                            .ThenInclude(tin => tin.EndTime)
                    .FirstAsync(t => t.TimeNormId == timeNormId);
            }

            _data.Remove(targetTimeNorm);

            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != expectedRowChanges) // TODO FIXME PRODUCTION exception is thrown wrong amount of expected row changes for isDeleteTimeStamps == false
            {
                _logger.LogError((int)trackerContext.TrackerEvent, $"Delete time norm {timeNormId} of user {trackerContext.UserId} failed.");
            }

            return targetTimeNorm.WorkUnit.DurationString;
        }
        /*public async Task ClearTimeStampsAsync(TrackerContext trackerContext) TODO not needed anymore?
        {
            var userStore = _data.TrackerStores.SingleOrDefault(t => t.TrackerStoreId == trackerContext.UserId);
            if (userStore == null)
            {
                _logger.LogError($"User store #{trackerContext.UserId} could not be found."); // TODO default error for user store not found
            }
            var timeStamps = await _data.Entry(userStore).Collection(t => t.TimeStamps).Query()
                .Include(ts => ts.BoundNormStart)
                .Include(ts => ts.BoundNormEnd)
                .ToListAsync();
            foreach(var timeStamp in timeStamps)
            {
                if (timeStamp.BoundNormStart != null)
                {
                    _data.Remove(timeStamp.BoundNormStart);
                }
                if (timeStamp.BoundNormEnd != null)
                {
                    _data.Remove(timeStamp.BoundNormEnd);
                }
                _data.Remove(timeStamp);
            }
            var affectedRows = await _data.SaveChangesAsync();
            LogEvent(trackerContext.TrackerEvent, $"Affected {affectedRows} rows.");
        }*/

        public async Task<List<TimeTable>> GetTimeTablesAndWorkUnitsAsync(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.WorkUnits)
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.TargetWeeklyTime)
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.TrackerStoreId == trackerContext.UserId);
            if (userStore == null)
            {
                throw new ArgumentNullException($"{nameof(TrackerStore)} is null for user {trackerContext.UserId}.");
            }
            var workUnits = userStore.TimeTables.SelectMany(t => t.WorkUnits);
            LogEvent(trackerContext.TrackerEvent, $"Retrieved {userStore.TimeTables.Count} tables with {workUnits.Count()} work units");
            return userStore.TimeTables;
        }

        public async Task<List<TimeTable>> GetAllTimeTables(TrackerContext trackerContext)
        {
            var userStore = await _data.TrackerStores
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.WorkUnits)
                        .ThenInclude(wwu => wwu.TimeNorms)
                            .ThenInclude(norm => norm.StartTime)
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.WorkUnits)
                        .ThenInclude(wwu => wwu.TimeNorms)
                            .ThenInclude(norm => norm.EndTime)
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.WorkUnits)
                        .ThenInclude(wwu => wwu.TimeNorms)
                            .ThenInclude(norm => norm.ProductivityRating)
                .Include(t => t.TimeTables)
                    .ThenInclude(tt => tt.TargetWeeklyTime)
                .AsNoTracking()
                .SingleAsync(t => t.TrackerStoreId == trackerContext.UserId);
            return userStore.TimeTables;
        }
        public async Task<TimeStamp> GetTimeStampAsync(TrackerContext trackerContext, int timeStampId)
        {
            var userStore = _data.TrackerStores.SingleOrDefault(t => t.TrackerStoreId == trackerContext.UserId);
            return await _data.FindAsync<TimeStamp>(timeStampId) ?? throw new ArgumentOutOfRangeException(nameof(timeStampId));
        }
        public async Task<List<TimeStamp>> GetTimeStampsAsync(TrackerContext trackerContext)
        {
            // TODO benchmark and measure performance depending on .AsNoTracking() location / repeated use etc.
            var userStore = _data.TrackerStores.SingleOrDefault(t => t.TrackerStoreId == trackerContext.UserId);
            var userStamps = await _data.Entry(userStore)
                .Collection(t => t.TimeStamps)
                .Query()
                .AsNoTracking()
                .Include(st => st.BoundNormStart)
                .Include(st => st.BoundNormEnd)
                .ToListAsync();
            LogEvent(trackerContext.TrackerEvent, $"Count: {userStamps.Count}");
            return userStamps;
        }

        public async Task<List<TimeNormTag>> GetTimeNormTagsAsync(TrackerContext trackerContext)
        {
            var userStore = _data.TrackerStores.SingleOrDefault(t => t.TrackerStoreId == trackerContext.UserId);
            var userTags = await _data.Entry(userStore)
                .Collection(t => t.TimeNormTags)
                .Query()
                .AsNoTracking()
                .ToListAsync();
            LogEvent(trackerContext.TrackerEvent, $"Count: {userTags.Count}");
            return userTags;
        }

        public async Task AuthorizeTimeTableAsync(ClaimsPrincipal user, int timeTableId, OperationAuthorizationRequirement requirement) // TODO same function with timeTable instead of timeTableId when already tracked
        {
            TimeTable timeTable;
            if (requirement.Name == TrackerAuthorization.ReadRequirement.Name)
            {
                timeTable = await _data
                    .Set<TimeTable>()
                    .AsNoTracking()
                    .Include(t => t.SharedTimeTableInfos)
                    .FirstAsync(t => t.TimeTableId == timeTableId);
            }
            else if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                timeTable = await _data
                    .Set<TimeTable>()
                    .Include(t => t.SharedTimeTableInfos)
                    .FirstAsync(t => t.TimeTableId == timeTableId);
            }
            else if (requirement.Name == TrackerAuthorization.ShareRequirement.Name)
            {
                timeTable = await _data
                    .Set<TimeTable>()
                    .Include(t => t.SharedTimeTableInfos)
                    .Include(t => t.TrackerStore)
                    .FirstAsync(t => t.TimeTableId == timeTableId);
            }
            else
            {
                throw new NotImplementedException($"Missing query for time table requirement {requirement.Name}.");
            }
            var authResult = await _authorizationService.AuthorizeAsync(user, timeTable, requirement);
            if (!authResult.Succeeded)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public async Task AuthorizeWorkUnitAsync(ClaimsPrincipal user, int workUnitId, OperationAuthorizationRequirement requirement)
        {
            WorkUnit workUnit;
            if (requirement.Name == TrackerAuthorization.ReadRequirement.Name)
            {
                workUnit = await _data.Set<WorkUnit>()
                    .Include(t => t.TimeTable)
                    .AsNoTracking()
                    .FirstAsync(t => t.WorkUnitId == workUnitId);
            }
            else if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                workUnit = await _data.Set<WorkUnit>()
                    .Include(t => t.TimeTable)
                    .FirstAsync(t => t.WorkUnitId == workUnitId);
            }
            else if (requirement.Name == TrackerAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for work unit requirement {requirement.Name}.");
            }
            await AuthorizeTimeTableAsync(user, workUnit.TimeTableId, requirement);
        }

        public async Task AuthorizeTimeNormAsync(ClaimsPrincipal user, int timeNormId, OperationAuthorizationRequirement requirement)
        {
            TimeNorm timeNorm;
            if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                timeNorm = await _data.Set<TimeNorm>()
                    .Include(t => t.WorkUnit)
                    .Include(t => t.ProductivityRating)
                    .AsTracking() 
                    .FirstAsync(t => t.TimeNormId == timeNormId);
            }
            else
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            var timeTableId = timeNorm.WorkUnit.TimeTableId;
            await AuthorizeTimeTableAsync(user, timeTableId, requirement);
        }

        public async Task AuthorizeTimeStampAsync(ClaimsPrincipal user, int timeStampId, OperationAuthorizationRequirement requirement)
        {
            TimeStamp timeStamp;
            if (requirement.Name == TrackerAuthorization.ReadRequirement.Name)
            {
                timeStamp = await _data.Set<TimeStamp>()
                    .FirstAsync(t => t.TimeStampId == timeStampId); // TODO should be no tracking but needed for duplicate timestamp?
            }
            else if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                timeStamp = await _data.Set<TimeStamp>()
                    .FirstAsync(t => t.TimeStampId == timeStampId);
            }
            else
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            // TODO auth chain problem: duplicate timestamp needs read timestamp (no tracking and write table (tracking))
            
            var authResult = await _authorizationService.AuthorizeAsync(user, timeStamp, requirement);
            if (!authResult.Succeeded)
            {
                throw new UnauthorizedAccessException();
            }

        }

        public void LogEvent(TrackerEvent trackerEvent, string info = null) => TrackerLogMessages.SuccessMessages[trackerEvent].Invoke(_logger, info, null);
        public void LogEvent(TrackerBackendEvent trackerBackendEvent, string info = null) => TrackerLogMessages.BackendMessages[trackerBackendEvent].Invoke(_logger, info, null);

        public async Task<TrackerJobResult> ProcessTimeStampsAsync(TrackerContext trackerContext, int targetWorkUnitId)
        {
            var jobOptions = CreateUserJobOptionsDictionary(trackerContext);
            jobOptions[nameof(ProcessTimeStampsJobOptions.TargetWorkUnitId)] = targetWorkUnitId.ToString();
            return await _queue.EnqueueAndWaitJobAsync(typeof(ProcessTimeStampsJobBuilder), jobOptions, 0);
        }
        public async Task<TrackerEnqueuedJob> ScheduleRemoveUserStoresAsync(TrackerContext trackerContext)
        {
            return await _queue.EnqueueJobAsync(typeof(RemoveUserJobBuilder), CreateUserJobOptionsDictionary(trackerContext), TimeSpan.FromSeconds(1), 0); // TODO serialize IOptions?
        }
        public async Task<TrackerEnqueuedJob> ScheduleProcessTimeStampsAsync(TrackerContext trackerContext, int? targetWorkUnitId)
        {
            var jobOptions = CreateUserJobOptionsDictionary(trackerContext);
            jobOptions[nameof(ProcessTimeStampsJobOptions.TargetWorkUnitId)] = targetWorkUnitId;
            return await _queue.EnqueueJobAsync(typeof(ProcessTimeStampsJobBuilder), jobOptions, TimeSpan.FromMilliseconds(100), 0);
        }
    }
}
