using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models
{
    public class TimeTablePlotData
    {
        // python data exchange: dictionary columns / data sets TODO keep in sync with python plot script
        public string[] X_AXIS_TICKS { get; set; }
        public int TIME_TABLE_ID { get; set; }
        // TODO make sure all user input that is used in python scripts is sanitized!
    }
}
