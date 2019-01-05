using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TimeTable
    {
        // TODO sync per table
        public int TimeTableId { get; set; }
        public List<WorkUnit> WorkUnits { get; set; } = new List<WorkUnit>();
        [Required, StringLength(255)]
        public string Name { get; set; } = "Table";
        public string TrackerStoreId { get; set; }
        [Required]
        public TrackerStore TrackerStore { get; set; }
        public List<SharedTimeTableInfo> SharedTimeTableInfos { get; set; } = new List<SharedTimeTableInfo>();
        public bool IsFrozen { get; set; } = false;
        public int? TargetWeeklyTimeId { get; set; }
        public TimeRange TargetWeeklyTime { get; set; } = new TimeRange(40);
        // TODO create backups/undo log??
        // TODO check LOB reference in resource allocator
        //[JsonIgnore]
        public Plot Plot { get; set; }
    }
}
