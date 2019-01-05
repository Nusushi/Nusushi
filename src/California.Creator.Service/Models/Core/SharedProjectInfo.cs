using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class SharedProjectInfo // TODO security problem: store id of other store is transmitted => use email or some other key/token
    {
        public int SharedProjectInfoId { get; set; }

        [Required]
        public string SharedWithCaliforniaStoreId { get; set; }
        public CaliforniaStore SharedWithCaliforniaStore { get; set; }

        [Required]
        public string OwnerCaliforniaStoreId { get; set; }
        public CaliforniaStore OwnerCaliforniaStore { get; set; }

        public int CaliforniaProjectId { get; set; }
        public CaliforniaProject CaliforniaProject { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public DateTimeOffset ShareEnabledTime { get; set; } // value generated in db
        [Required]
        public bool? IsReshareAllowed { get; set; }
        [Required]
        public bool? IsEditAllowed { get; set; }
    }
}
