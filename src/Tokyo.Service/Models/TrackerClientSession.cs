using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models
{
    // TODO data structures changed for following comment:
    // ----
    // combines options for the current http request in the following order: (initialized in TrackerOptionsMiddleware)
    // 1) hardcoded fallback
    // 2) configured by appsettings.{$$$}.json => IOptions<TrackerServiceOptions>
    // 3) configured by environment variable
    // 4) stored per user in DB
    // 5) stored per user in cookie TODO
    // 6) overridden by model structure / parameters (e.g. agenda mode)
    // 7) stored per user in browser application store
    // ----
    // TODO lazy loading such that <=1 DB call
    // TODO sync data with browser app store so cookie gets smaller
    // TODO all values accessible from one class TrackerSettings.SettingName
    // TODO decide where to store (DB/cache/perUser/global/...) by DataAnnotation
    // TODO could contain tracker event context
    // TODO should be checkable for consistency
    // TODO granularity complicated
    // main purpose: a consistent way to decide which timezones to display and store values with
    // e.g. company HQ timezone => external contractor timezone => preparing for agenda / travel log => using VPN
    public class TrackerClientSession
    {
        public const int TargetRevision = 1; // TODO current app version
        public int Revision { get; set; } = 0;
        private TimeZoneInfo _deviceTimeZone = null;
        [JsonIgnore]
        public TimeZoneInfo DeviceTimeZone
        {
            get
            {
                return _deviceTimeZone;
            }
            set
            {
                DeviceTimeZoneCookieValue = value?.Id;
                _deviceTimeZone = value;
            }
        }
        public string DeviceTimeZoneCookieValue { get; set; } = null;
        private TimeZoneInfo _viewTimeZone = null;
        [JsonIgnore]
        public TimeZoneInfo ViewTimeZone
        {
            get
            {
                return _viewTimeZone;
            }
            set
            {
                ViewTimeZoneCookieValue = value?.Id;
                _viewTimeZone = value;
            }
        }
        public string ViewTimeZoneCookieValue { get; set; } = null;
        public bool IsDisplayAsTracked { get; set; } = false; // TODO check implementation
        public bool IsCsvDirectionHorizontal { get; set; } = true;
        public int? LastViewedTableId { get; set; } = null; // TODO
        public enum SortOrder { Default, ByDatabaseOrder, ByTimeAdded, ByTimeTracked, Alphabetically, ByClassifier, ByTimeSpanDuration };
        public SortOrder LastSortOrder { get; set; } = SortOrder.Default;
    }
}
