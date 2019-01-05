using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleAtom
    {
        public int StyleAtomId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required]
        public StyleAtomType? StyleAtomType { get; set; } = Core.StyleAtomType.Generic;

        public List<StyleValue> AppliedValues { get; set; } = new List<StyleValue>(); // TODO constraints to make sure all StyleValues that are mapped to same StyleAtom have different css properties
        public List<StyleAtomQuantumMapping> MappedQuantums { get; set; } = new List<StyleAtomQuantumMapping>();

        public int MappedToMoleculeId { get; set; }
        [JsonIgnore]
        public StyleMoleculeAtomMapping MappedToMolecule { get; set; }

        [NotMapped]
        public bool IsDeletable {
            get
            {
                return AppliedValues.Count == 0;
            }
        }
    }
}
