﻿using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public enum StyleAtomType
    {
        // integer values may not change in future (ef core)
        Generic = 0,
        Font = 1,
        Typography = 2,
        Divider = 3,
        Background = 4,
        Spacing = 5,
        Picture = 6,
        Grid = 7,
        Row = 8,
        Navbar = 9,
        List = 10,
        Box = 11
    }
}
