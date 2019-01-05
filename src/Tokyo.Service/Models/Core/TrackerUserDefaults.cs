using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TrackerUserDefaults
    {
        public int TrackerUserDefaultsId { get; set; }
        public string TrackerStoreId { get; set; }
        [Required]
        public TrackerStore TrackerStore { get; set; }
        public int? TargetWorkUnitId { get; set; }
        public WorkUnit TargetWorkUnit { get; set; }
        [Required]
        public string TimeZoneId { get; set; }
    }
}
