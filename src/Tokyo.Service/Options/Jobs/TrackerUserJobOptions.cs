using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Options.Jobs
{
    public abstract class TrackerUserJobOptions
    {
        public string UserId { get; set; }
        // TODO version number for serialized options
        // TODO usage with care (also derived classes): all values are converted to string? e.g. int?
        // TODO abstract?
    }
}
