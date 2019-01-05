using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class Webfont
    {
        public int WebfontId { get; set; }
        
        [Required]
        public string Family { get; set; }

        [Required]
        public string Version { get; set; }
    }
}
