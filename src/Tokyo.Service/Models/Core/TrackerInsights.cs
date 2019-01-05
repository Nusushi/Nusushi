using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace Tokyo.Service.Models.Core
{
    // TODO make pro features optional and enable service for local/web app
    public class TrackerInsights
    {
        // key index
        public int TrackerInsightsId { get; set; }
        public string TrackerStoreId { get; set; }
        [Required]
        public TrackerStore TrackerStore { get; set; }
        // members
        public DateTimeOffset? NextSuggestedStartTime { get; set; }
        public DateTimeOffset? NextSuggestedEndTime { get; set; }
        public Task GetMostProductiveTimeRange()
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
