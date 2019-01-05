using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class CaliforniaClientViewModel
    {
        // TODO json response large because all variable names are rendered as well... pass data in object array and generate cast functions?
        // status
        public string StatusText { get; set; } // empty when request is successful, error message when request fails
        public int CurrentRevision { get; set; }

        public int CaliforniaEvent { get; set; }

        public CaliforniaProject CaliforniaProject { get; set; }

        // public StyleQuantum StyleQuantum { get; set; } only full data transfers

        // static data
        public Dictionary<string, List<string>> StyleAtomCssPropertyMapping { get; set; }
        public List<string> AllCssProperties { get; set; }
        public List<string> ThirdPartyFonts { get; set; }
        public string UrlToReadOnly { get; set; }
        public string UrlToReadAndEdit { get; set; }

        public CaliforniaClientPartialData PartialUpdate { get; set; }
    }
}
