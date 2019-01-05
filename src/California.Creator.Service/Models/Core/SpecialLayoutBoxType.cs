using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public enum SpecialLayoutBoxType // TODO test if numbering can be changed (ef core?)
    {
        Default = 0,
        CaliforniaViewHolder = 1,
        Navigation = 2,
        UnsortedList = 3,
        SortedList = 4,
        ListItem = 5,
        RichText = 6
    }
}
