using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class SharedTimeTableInfo  // TODO security problem: store id of other store is transmitted => use email or some other key/token
    {
        public int SharedTimeTableInfoId { get; set; }

        [Required]
        public string SharedWithTrackerStoreId { get; set; }
        public TrackerStore SharedWithTrackerStore { get; set; }

        [Required]
        public string OwnerTrackerStoreId { get; set; }
        public TrackerStore OwnerTrackerStore { get; set; }

        public int TimeTableId { get; set; }
        public TimeTable TimeTable { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public DateTimeOffset ShareEnabledTime { get; set; } // value generated in db
        [Required]
        public bool? IsReshareAllowed { get; set; }
        [Required]
        public bool? IsEditAllowed { get; set; }
    }
}
