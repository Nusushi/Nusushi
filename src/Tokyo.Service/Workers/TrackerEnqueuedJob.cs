using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tokyo.Service.Workers
{
    public class TrackerEnqueuedJob
    {
        // ID not used in memory cache
        public int TrackerEnqueuedJobId { get; set; } = -1;
        public DateTime CreatedDate { get; private set; } = DateTime.Now;
        public DateTime? RunAfterDate { get; set; } = null;
        public DateTime? FinishedDate { get; set; } = null;
        public string JobBuilderTypeName { get; set; } = null;
        public uint Priority { get; set; } = 0; // TODO priority != 0 are not handled
        [NotMapped] // TODO
        public Dictionary<string, object> OptionValues { get; set; } = new Dictionary<string, object>();
    }
}
