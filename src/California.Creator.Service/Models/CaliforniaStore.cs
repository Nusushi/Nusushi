using California.Creator.Service.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models
{
    public class CaliforniaStore
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // matches nusushi user ID TODO documentation
        public string CaliforniaStoreId { get; set; }
        // TODO date created vs nusushi user date created / TODO last accessed time(s)

        public int CaliforniaUserDefaultsId { get; set; }
        public CaliforniaUserDefaults CaliforniaUserDefaults { get; set; } = new CaliforniaUserDefaults();

        public List<CaliforniaProject> CaliforniaProjects { get; set; } = new List<CaliforniaProject>();

        public List<SharedProjectInfo> ForeignSharedProjectInfos { get; set; }
        public List<SharedProjectInfo> OwnedSharedProjectInfos { get; set; }
    }
}
