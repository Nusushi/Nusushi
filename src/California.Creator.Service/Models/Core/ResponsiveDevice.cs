using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class ResponsiveDevice
    {
        public int ResponsiveDeviceId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required, StringLength(5)]
        public string NameShort { get; set; }

        [Required]
        public int? WidthThreshold { get; set; } // TODO document convention: -1 <=> default/no selection + direct css, 0 <=> direct css, >0 <=> media query wrap, CSS device priority is from small to large

        // TODO HeightThreshold AND/OR Mixed Height+Width

        public List<StyleMoleculeAtomMapping> AppliedToMappings { get; set; } = new List<StyleMoleculeAtomMapping>();
    }
}
