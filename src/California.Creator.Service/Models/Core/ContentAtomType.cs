using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public enum ContentAtomType
    {
        // integer values may not change in future (ef core)
        Text = 0,
        Html = 1,
        Picture = 2,
        Link = 3, // TODO email seems to be the same, but need mailto:
        NavLink = 4
    }
}
