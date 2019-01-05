using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TimeNormTag
    {
        public int TimeNormTagId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; } = "UNSET_TIMENORMTAG";
        public List<TimeNormTagMapping> TimeNormTagMappings { get; set; } = new List<TimeNormTagMapping>();
        [Required, StringLength(255)]
        public string Color { get; set; } = "red";
        [Required]
        public string TrackerStoreId { get; set; }
        public TrackerStore TrackerStore { get; set; }
    }
}
