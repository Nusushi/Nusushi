using System;
using System.Collections.Generic;
using System.Text;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Models
{
    public class TrackerContext
    {
        public TrackerContext(TrackerEvent trackerEvent, string userId)
        {
            TrackerEvent = trackerEvent;
            UserId = userId;
        }
        public TrackerEvent TrackerEvent { get; set; }
        public string UserId { get; set; }
    }
}
