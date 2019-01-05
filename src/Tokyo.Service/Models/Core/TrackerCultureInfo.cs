using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class TrackerCultureInfo
    {
        public int Key { get; set; } // TODO use array index in client app
        public string IdDotNet { get; set; }
        public string DisplayName { get; set; }
    }
}
