using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleMoleculeAtomMapping
    {
        public int StyleMoleculeAtomMappingId { get; set; }

        public int StyleMoleculeId { get; set; }
        public StyleMolecule StyleMolecule { get; set; }
        
        [JsonIgnore]
        public StyleAtom StyleAtom { get; set; }  // foreign key on style atom because main delete path is layout => style molecule => mapping => style atom => values

        public int ResponsiveDeviceId { get; set; }
        public ResponsiveDevice ResponsiveDevice { get; set; }

        [StringLength(255)]
        public string StateModifier { get; set; }
    }
}
