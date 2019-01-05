using System;
using System.Collections.Generic;
using System.Text;
using Tokyo.Service.Extensions;

namespace Tokyo.Service.Models.Core
{
    public class TimeTableViewModel
    {
        public TimeTableViewModel(TrackerClientSession clientSession, TimeTable timeTableAndWorkUnits, string tableOwnerUserName)
        {
            TableOwnerUserName = tableOwnerUserName;
            TimeTableAndWorkUnits = timeTableAndWorkUnits.TranformTrackedTimesForView(clientSession);

            var tableDateRange = timeTableAndWorkUnits.GetStartAndEndDate();
            if (tableDateRange != null) // TODO default value
            {
                TimeTableDateRange = $"{tableDateRange.Item1.Date.ToLongDateString()} - {tableDateRange.Item2.Date.ToLongDateString()}";
                EarliestDateTimeInData = tableDateRange.Item1;
                LatestDateTimeInData = tableDateRange.Item2;
            }
            /*TimeZoneInfo clientTimeZone = clientSession.DeviceTimeZone; TODO unused
            DateTimeOffset currentDateTimeUtc = DateTimeOffset.UtcNow;
            DateTimeOffset clientDateTimeAtDayStart;
            if (clientTimeZone.Id != "UTC")
            {
                DateTimeOffset currentClientTime = TimeZoneInfo.ConvertTime(currentDateTimeUtc, clientTimeZone);
                DateTime clientDateTimeAtDayStartAsDateTimeLocal = new DateTime(currentClientTime.Year, currentClientTime.Month, currentClientTime.Day, 0, 0, 0, 1, DateTimeKind.Unspecified);
                DateTime clientUtcAtDayStartAsDateTimeUtc = TimeZoneInfo.ConvertTime(clientDateTimeAtDayStartAsDateTimeLocal, clientTimeZone, TimeZoneInfo.Utc);
                TimeSpan clientOffsetAtDayStart = clientTimeZone.GetUtcOffset(clientUtcAtDayStartAsDateTimeUtc);
                clientDateTimeAtDayStart = new DateTimeOffset(currentClientTime.Year, currentClientTime.Month, currentClientTime.Day, 0, 0, 0, 1, clientOffsetAtDayStart);
            }
            else
            {
                clientDateTimeAtDayStart = new DateTimeOffset(currentDateTimeUtc.Year, currentDateTimeUtc.Month, currentDateTimeUtc.Day, 0, 0, 0, 1, TimeSpan.Zero);
            }*/
            // ReferenceForTimeline = clientDateTimeAtDayStart;
        }
        public string TableOwnerUserName { get; set; }
        public TimeTable TimeTableAndWorkUnits { get; set; }
        public string TimeTableDateRange { get; set; }
        public DateTimeOffset EarliestDateTimeInData { get; set; }
        public DateTimeOffset LatestDateTimeInData { get; set; }
    }
}
