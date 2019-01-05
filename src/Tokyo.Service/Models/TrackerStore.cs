using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Models
{
    public class TrackerStore
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // created when registering on Nusushi Services Global TODO documentation
        public string TrackerStoreId { get; set; }
        // TODO date created vs nusushi user date created / TODO last accessed time(s)
        public Collection<TimeStamp> TimeStamps { get; set; }
        public List<TimeTable> TimeTables { get; set; } = new List<TimeTable>();
        public List<SharedTimeTableInfo> ForeignSharedTimeTableInfos { get; set; }
        public List<SharedTimeTableInfo> OwnedSharedTimeTableInfos { get; set; }
        public int TrackerUserDefaultsId { get; set; }
        public TrackerUserDefaults TrackerUserDefaults { get; set; } = new TrackerUserDefaults();
        public int TrackerInsightsId { get; set; }
        public TrackerInsights TrackerInsights { get; set; }
        public List<TimeNormTag> TimeNormTags { get; set; }
        [NotMapped]
        [JsonIgnore]
        public IEnumerable<TimeStamp> UnboundTimeStamps // TODO rework
        {
            get
            {
                return TimeStamps.Where(t => !t.IsBound);
            }
        }
    }
}
