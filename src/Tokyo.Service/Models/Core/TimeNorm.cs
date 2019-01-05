using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Tokyo.Service.Extensions;

namespace Tokyo.Service.Models.Core
{
    public class TimeNorm
    {
        public int TimeNormId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; } = "Name...";
        public int WorkUnitId { get; set; }
        public WorkUnit WorkUnit { get; set; }
        public int? StartTimeId { get; set; }
        public TimeStamp StartTime { get; set; }
        public int? EndTimeId { get; set; }
        public TimeStamp EndTime { get; set; }
        public List<TimeNormTagMapping> TimeNormTagMappings { get; set; } = new List<TimeNormTagMapping>();
        public int ColorR { get; set; } = 50;
        public int ColorG { get; set; } = 148;
        public int ColorB { get; set; } = 163;
        public int? ProductivityRatingId { get; set; }
        public ProductivityRating ProductivityRating { get; set; }
        public int ManualSortOrderKey { get; set; } // default value => sql sequence // TODO make all keys Int64 in all projects (?)
        // TODO code duplication w/ work unit
        [NotMapped]
        public string DurationString
        {
            get
            {
                return Norm?.TotalMilliseconds.ToTrackerDuration(true, false) ?? "UNDEFINED_DURATION";
            }
        }
        [NotMapped]
        public TimeSpan? Norm
        {
            get
            {
                if (StartTime == null || EndTime == null)
                {
                    return null;
                }
                else
                {
                    return EndTime.TrackedTimeUTC.Subtract(StartTime.TrackedTimeUTC);
                }
            }
        }
        // TODO end code duplication
    }
}
