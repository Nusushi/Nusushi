using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class LayoutStyleInteraction
    {
        public int LayoutStyleInteractionId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        public int LayoutAtomId { get; set; }
        [JsonIgnore]
        public LayoutAtom LayoutAtom { get; set; }

        public List<StyleValueInteractionMapping> StyleValueInteractions { get; set; } = new List<StyleValueInteractionMapping>(); 

        [Required]
        public LayoutStyleInteractionType? LayoutStyleInteractionType { get; set; }

        //[Required]
        //public InteractionTriggerType? InteractionTriggerType { get; set; } // TODO possible sources: html element triggered by event (click, scroll, ...); possible targets: css styles, dom properties, ...; possible modes: toggle/smooth per fraction+inverse with reference value for fraction calc and reference values for style calc
    }
}
