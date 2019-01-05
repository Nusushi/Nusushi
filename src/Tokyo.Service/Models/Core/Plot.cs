using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class Plot
    {
        public int PlotId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; } = "UNSET_PLOT";
        public byte[] ImageData { get; set; } // TODO save path to file in (Azure) blob store
        public int TimeTableId { get; set; }
        public TimeTable TimeTable { get; set; }
        public string MediaType { get; set; } = "image/png";
        [Timestamp]
        public byte[] Version { get; set; }
    }
}
