using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TimeNormTagMapping
    {
        public int TimeNormTagMappingId { get; set; }
        public int TimeNormId { get; set; }
        public TimeNorm TimeNorm { get; set; }
        public int TimeNormTagId { get; set; }
        public TimeNormTag TimeNormTag { get; set; }
    }
}
