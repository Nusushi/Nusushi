using California.Creator.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class CaliforniaSelectionOverlay
    {
        public CaliforniaSelectionOverlay(CaliforniaProject californiaProject)
        {
            if (californiaProject.CurrentSelection != null)
            {
                throw new InvalidOperationException("Overlay already defined on target project.");
            }
            CaliforniaProject = californiaProject;
            californiaProject.CurrentSelection = this;
        }

        public readonly CaliforniaProject CaliforniaProject; // set in constructor
        public Dictionary<int, List<string>> StyleMoleculeCssClassMapping { get; set; } = new Dictionary<int, List<string>>();
        public List<StyleMolecule> StyleMolecules { get; set; } = new List<StyleMolecule>();
        public Dictionary<string, List<int>> Fonts { get; set; } = new Dictionary<string, List<int>>();

        // TODO renumber starting from 0 and fill holes, renumber later on request to refill holes and adjust to order in most recent version 
    }
}
