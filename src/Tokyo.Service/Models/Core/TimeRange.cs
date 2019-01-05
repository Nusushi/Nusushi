using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TimeRange
    {
        public TimeRange() { } // EF Core
        public int TimeRangeId { get; set; }
        public int Days { get; set; }
        public TimeSpan WithinDayTimeSpan { get; set; }
        public TimeRange(int hours, int minutes = 0)
        {
            var hoursRemainder = hours % 24;
            var days = (hours - hoursRemainder) / 24;
            Days = days;
            WithinDayTimeSpan = new TimeSpan(hoursRemainder, minutes, 0);
        }
        public int TimeTableId { get; set; }
        public TimeTable TimeTable { get; set; }
    }
}
