using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tokyo.Service.Options
{
    public class TrackerServiceOptions
    {
        // internal
        public enum DbEngineSelector { Mssql, Sqlite }
        public DbEngineSelector DatabaseTechnology { get; set; } = DbEngineSelector.Mssql; // value is selected by chosing appropriate service register routines
        // configurable by user
        public TimeZoneInfo DefaultUserTimeZone { get; set; } = TimeZoneInfo.Utc;
        public IList<TimeZoneInfo> SupportedTimeZones { get; set; } = new List<TimeZoneInfo>() { TimeZoneInfo.Utc };
        public List<CultureInfo> SupportedCultures { get; set; } = new List<CultureInfo>() { new CultureInfo("en-US") };
        public string CookieName { get; set; } = ".TokyoService.UNSET";
        public string PublicAccountUserName { get; set; } = "tokyo@nusushi.com"; // TODO
        public string PublicAccountPassword { get; set; } = "tokyo@nusushi.com"; // TODO
        public string EmailDomainName { get; set; } = "xn--xckd0d.com"; // TODO
    }
}
