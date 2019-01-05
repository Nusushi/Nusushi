using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class PictureContent
    {
        public int PictureContentId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        public List<ContentAtom> DisplayedOnAtoms { get; set; } = new List<ContentAtom>();
    }
}
