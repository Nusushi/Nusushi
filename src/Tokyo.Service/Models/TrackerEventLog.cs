using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Models
{
    public class TrackerEventLog // TODO code duplication
    {
        public int TrackerEventLogId { get; set; }

        [Required]
        public TrackerEvent TrackerEvent { get; set; }

        public int NTimesCalled { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
