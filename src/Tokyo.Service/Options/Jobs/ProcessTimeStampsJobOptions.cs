using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Options.Jobs
{
    public class ProcessTimeStampsJobOptions : TrackerUserJobOptions
    {
        public int? TargetWorkUnitId { get; set; } = null;
    }
}
