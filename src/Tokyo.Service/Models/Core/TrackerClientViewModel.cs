using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TrackerClientViewModel
    {
        // TODO json response large because all variable names are rendered as well... pass data in object array and generate cast functions?
        // status
        public string StatusText { get; set; }
        public int CurrentRevision { get; set; }
        public string UrlToReadOnly { get; set; }
        public string UrlToReadAndEdit { get; set; }
        public int TrackerEvent { get; set; }
        public int TargetWorkUnitId { get; set; }
        public List<TrackerTimeZone> TimeZones { get; set; }
        public List<TrackerCultureInfo> CultureIds { get; set; } // TODO cache data (current client size with small data table: 384KB)
        public string[] WeekDayLetters { get; set; }
        public string[] AbbreviatedMonths { get; set; }
        public int StartDayOfWeekIndex { get; set; } // TODO should be number|undefined
        public string SelectedCultureId { get; set; }
        public string SelectedTimeZoneIdTimeStamps { get; set; }
        public string SelectedTimeZoneIdView { get; set; }
        public bool[] TrackerClientFlags { get; set; }
        public List<TimeTableViewModel> TimeTables { get; set; }
        public int SelectedTimeTableId { get; set; }
        public TimeTableViewModel SelectedTimeTable { get; } = null; // object managed client side
        public List<TimeStamp> UnboundTimeStamps { get; set; }
        public int TargetId { get; set; }
        public TimeTableViewModel TimeTable { get; set; }
        public TimeTableViewModel TimeTableSecondary { get; set; }
        public int WorkUnitIdSource { get; set; }
        public int WorkUnitIdTarget { get; set; }
        public WorkUnit WorkUnit { get; set; }
        public TimeStamp TimeStamp { get; set; }
        public TimeNorm TimeNormNoChildren { get; set; }
        public string UpdatedName { get; set; }
        public TimeNormTag TimeNormTag { get; set; }
        public ProductivityRating UpdatedRating { get; set; }
        public string ClientTimelineOffset { get; set; } // timeline is displayed with client device timezone
        public string TimeNormDurationString { get; set; }
        public string WorkUnitDurationString { get; set; }
    }
}
