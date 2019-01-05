using California.Creator.Service.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class CaliforniaView
    {
        public int CaliforniaViewId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required] // TODO max query length? // TODO alternate key (read only + unique)
        public string QueryUrl { get; set; }

        public List<QueryViewLayoutBoxMapping> HostedByLayoutMappings { get; set; } = new List<QueryViewLayoutBoxMapping>();

        public List<LayoutRow> PlacedLayoutRows { get; set; } = new List<LayoutRow>();

        public int ViewSortOrderKey { get; set; } // default value => sql sequence // TODO document: care when copying view holders aka california projects

        [Required]
        public bool? IsInternal { get; set; } = false;

        [StringLength(CaliforniaServiceOptions.UserDefinedCssMaxLength)]
        public string UserDefinedCss { get; set; } = null;

        [NotMapped] // TODO this property should only be visible client side, but dont copy data => JsonIgnore not feasable // TODO calculation can be cached // TODO rework this
        public int DeepestLevel { get; }
        [NotMapped] // TODO calculation can be cached // TODO document invalid for internal views
        public int SpecialStyleViewStyleMoleculeId { get; }
        [NotMapped] // TODO calculation can be cached
        public int SpecialStyleBodyStyleMoleculeId { get; }
        [NotMapped] // TODO calculation can be cached
        public int SpecialStyleHtmlStyleMoleculeId { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleViewStyleMoleculeIdString { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleBodyStyleMoleculeIdString { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleHtmlStyleMoleculeIdString { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleViewStyleString { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleBodyStyleString { get; }
        [NotMapped, Required] // TODO calculation can be cached
        public string SpecialStyleHtmlStyleString { get; }
    }
}
