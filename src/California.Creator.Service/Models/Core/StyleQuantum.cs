using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleQuantum
    {
        public int StyleQuantumId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [JsonIgnore]
        public List<StyleAtomQuantumMapping> MappedToAtoms { get; set; } = new List<StyleAtomQuantumMapping>();

        [Required]
        public string CssProperty { get; set; } // TODO convert css properties from string to primary object and share for all users to save storage space
        [Required]
        public string CssValue { get; set; }  // TEST convert css values from string to primary objects TEST legal user consent, can measure if already exists by speed

        [NotMapped]
        public bool IsDeletable {
            get
            {
                return MappedToAtoms.Count == 0;
            }
        }
    }
}
