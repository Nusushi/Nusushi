using California.Creator.Service.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models
{
    public class CaliforniaEventLog
    {
        public int CaliforniaEventLogId { get; set; }

        [Required]
        public CaliforniaEvent CaliforniaEvent { get; set; }

        public int NTimesCalled { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
