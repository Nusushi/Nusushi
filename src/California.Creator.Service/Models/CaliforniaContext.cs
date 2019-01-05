using California.Creator.Service.Models.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models
{
    public class CaliforniaContext
    {
        public CaliforniaContext(CaliforniaEvent californiaEvent, string userId)
        {
            CaliforniaEvent = californiaEvent;
            UserId = userId;
        }
        public CaliforniaEvent CaliforniaEvent { get; set; }
        public string UserId { get; set; }
    }
}
