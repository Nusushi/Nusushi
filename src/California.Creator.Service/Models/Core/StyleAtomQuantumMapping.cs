using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleAtomQuantumMapping
    {
        public int StyleAtomQuantumMappingId { get; set; }

        public int StyleAtomId { get; set; }
        [JsonIgnore]
        public StyleAtom StyleAtom { get; set; }

        public int StyleQuantumId { get; set; }
        public StyleQuantum StyleQuantum { get; set; }
    }
}
