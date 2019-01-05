using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nusushi.Models.TokyoViewModels
{
    public class EditTimeStampPostViewModel
    {
        public int TimeStampId { get; set; }
        [StringLength(255)]
        public string StatusMessage { get; set; } = "";
        [StringLength(255)]
        public string TimeStampName { get; set; } = "MANUAL";
        [Range(1, 9999)]
        public int? Year { get; set; } // TODO correct range limits
        [Range(1, 12)]
        public int? Month { get; set; }
        [Range(1, 31)]
        public int? Day { get; set; }
        [Range(0, 23)]
        public int? Hour { get; set; }
        [Range(0, 59)]
        public int? Minute { get; set; }
        [Range(0, 59)]
        public int? Second { get; set; }
        [Range(0, 999)]
        public int? Millisecond { get; set; }
        [Range(1, 9999)]
        public int? YearOld { get; set; }
        [Range(1, 12)]
        public int? MonthOld { get; set; }
        [Range(1, 31)]
        public int? DayOld { get; set; }
        [Range(0, 23)]
        public int? HourOld { get; set; }
        [Range(0, 59)]
        public int? MinuteOld { get; set; }
        [Range(0, 59)]
        public int? SecondOld { get; set; }
        [Range(0, 999)]
        public int? MillisecondOld { get; set; }
        public long? ExactTicks { get; set; }
        [StringLength(255)]
        public string JsTimeString { get; set; }
    }
}
