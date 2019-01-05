using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Workers
{
    public class TrackerJobResult
    {
        public bool Success { get; set; }

        public string Name { get; set; }
        // job summary
        public string StatusMessage { get; set; }
        // TODO duration, start finish date, job creator
    }
}
