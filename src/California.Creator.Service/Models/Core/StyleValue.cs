using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleValue
    {
        public int StyleValueId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        public int StyleAtomId { get; set; }
        [JsonIgnore]
        public StyleAtom StyleAtom { get; set; }

        [Required]
        public string CssProperty { get; set; } // TODO convert css properties from string to primary object and share for all users to save storage space
        [Required]
        public string CssValue { get; set; } // TODO convert css values to equivalent/base representation, e.g. color black = #000000 = rgb(0,0,0)... for equivalent comparisons

        [JsonIgnore]
        public List<StyleValueInteractionMapping> AppliedStyleValueInteractions { get; set; } = new List<StyleValueInteractionMapping>();
    }
}
