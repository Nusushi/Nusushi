using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class CaliforniaUserDefaults
    {
        public int CaliforniaUserDefaultsId { get; set; }
        [PreventDeveloperCodePropagation]
        public string CaliforniaStoreId { get; set; }
        [Required, PreventDeveloperCodePropagation]
        public CaliforniaStore CaliforniaStore { get; set; }
    }
}
