using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Tokyo.Service.Extensions;

namespace Tokyo.Service.Models.Core
{
    public class WorkUnit // SQL
    {
        public int WorkUnitId { get; set; }
        public List<TimeNorm> TimeNorms { get; set; } = new List<TimeNorm>();
        public int TimeTableId { get; set; } // SQL // TODO document: SQL tag in this project means parameter name is used in raw SQL query
        public TimeTable TimeTable { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; } = "Times";
        public int? ActiveTrackerUserDefaultsId { get; set; }
        public TrackerUserDefaults ActiveTrackerUserDefaults { get; set; }
        public int ManualSortOrderKey { get; set; } // SQL // TODO getter has 0 check with sqlite for integrity
        public bool IsDisplayAgendaTimeZone { get; set; } = false;
        [NotMapped]
        public TimeSpan UtcOffsetAgenda
        {
            get
            {
                return IsNegativeUtcOffsetAgenda ? AbsoluteOfUtcOffsetAgenda.Negate() : AbsoluteOfUtcOffsetAgenda;
            }
            set
            {
                if (value.Ticks > 0)
                {
                    AbsoluteOfUtcOffsetAgenda = value;
                    IsNegativeUtcOffsetAgenda = false;
                }
                else
                {
                    AbsoluteOfUtcOffsetAgenda = value.Negate();
                    IsNegativeUtcOffsetAgenda = true;
                }
            }
        }
        public TimeSpan AbsoluteOfUtcOffsetAgenda { get; set; } = TimeSpan.Zero;
        public bool IsNegativeUtcOffsetAgenda { get; set; } = false;
        [Required, StringLength(255)]
        public string TimeZoneIdAgenda { get; set; } = "UTC";
        // TODO code duplication w/ time norm
        [NotMapped]
        public string DurationString
        {
            get
            {
                return Norm?.TotalMilliseconds.ToTrackerDuration(false, false) ?? "UNDEFINED_DURATION";
            }
        }
        [NotMapped]
        public TimeSpan? Norm // TODO calculate client side => no update mechanism required / no confusion with not loaded time norms
        {
            get
            {
                // sum over all associated time norms that are valid and positive
                return new TimeSpan(TimeNorms.Sum(tn => (tn.Norm.HasValue && tn.Norm.Value.Ticks > 0 && tn.Norm.Value.TotalSeconds > 1) ? tn.Norm.Value.Ticks : 0));
            }
        }
        // TODO end code duplication
    }
}
