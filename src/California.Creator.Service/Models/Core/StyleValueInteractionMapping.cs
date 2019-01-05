using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleValueInteractionMapping
    {
        public int StyleValueInteractionMappingId { get; set; }

        public int StyleValueId { get; set; }
        [JsonIgnore]
        public StyleValue StyleValue { get; set; }

        public int LayoutStyleInteractionId { get; set; }
        [JsonIgnore]
        public LayoutStyleInteraction LayoutStyleInteraction { get; set; }

        [Required] // TODO max css length from standard => apply to all css value strings for db efficiency
        public string CssValue { get; set; }
    }
}