using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    /// <summary>
    /// Stores a clock time. Internal handling as UTC time stamp.
    /// Additional information (time zone id, offset at creation) allows for
    /// a) conversion to user local time
    /// b) keep past time stamps consistent when the time zone offset changes in the future
    /// c) react to time zone offset changes for time stamps, that are set in the future
    /// </summary>
    public class TimeStamp
    {
        public int TimeStampId { get; set; }
        public TimeSpan AbsoluteOfUtcOffset { get; set; } = TimeSpan.Zero;
        public bool IsNegativeUtcOffset { get; set; } = false;
        [Required, StringLength(255)]
        public string TimeZoneIdAtCreation { get; set; } = "UTC";
        [JsonIgnore]
        public DateTimeOffset TrackedTimeUTC { get; set; } = DateTimeOffset.UtcNow; // default value for SQLite, set in MSSQL on row create
        [NotMapped]
        public DateTimeOffset TrackedTimeAtCreation // TODO check every usage and supply TrackedTimeForView for request timezone
        {
            get
            {
                var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneIdAtCreation);
                var result = TimeZoneInfo.ConvertTimeFromUtc(TrackedTimeUTC.UtcDateTime, targetTimeZone); // TODO cache
                return new DateTimeOffset(result, targetTimeZone.GetUtcOffset(result));
            }
        }
        [NotMapped]
        public DateTimeOffset? TrackedTimeForView { get; set; } = null;
        [NotMapped]
        public TimeSpan UtcOffsetAtCreation
        {
            get
            {
                return IsNegativeUtcOffset ? AbsoluteOfUtcOffset.Negate() : AbsoluteOfUtcOffset;
            }
            set
            {
                if (value.Ticks > 0)
                {
                    AbsoluteOfUtcOffset = value;
                    IsNegativeUtcOffset = false;
                }
                else
                {
                    AbsoluteOfUtcOffset = value.Negate();
                    IsNegativeUtcOffset = true;
                }
            }
        }
        [NotMapped]
        public string TimeString { get; set; } = "UNSET_TIMESTRING";
        [NotMapped]
        public string DateString { get; set; } = "UNSET_DATESTRING";
        [Required, StringLength(255)]
        public string Name { get; set; } = "UNSET_TIMESTAMP";
        [Required]
        public string TrackerStoreId { get; set; }
        public TimeNorm BoundNormStart { get; set; }
        public TimeNorm BoundNormEnd { get; set; }
        // TODO is mapping safe?
        public bool IsBound
        {
            get
            {
                return (BoundNormStart != null || BoundNormEnd != null);
            }
        }
    }
}
