using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models
{
    public class CaliforniaClientSession // date is transfered to client via HTTP cookie and readable from javascript
    {
        public const int TargetRevision = 1; // TODO current app version
        public int Revision { get; set; } = 0;

        public int ProjectId { get; set; } = 0;
    }
}
