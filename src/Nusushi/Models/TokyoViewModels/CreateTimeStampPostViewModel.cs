using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nusushi.Models.TokyoViewModels
{
    public class CreateTimeStampPostViewModel
    {
        public string StatusMessage { get; set; } = "";
        [StringLength(255)]
        public string ManualDateTimeString { get; set; }
        [StringLength(255)]
        public string TimeStampName { get; set; } = "MANUAL";
        public int? Year { get; set; }
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
        [StringLength(255)]
        public string JsTimeString { get; set; }
    }
}
