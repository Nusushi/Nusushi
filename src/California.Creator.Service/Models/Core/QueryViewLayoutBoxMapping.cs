using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class QueryViewLayoutBoxMapping
    {
        public int QueryViewLayoutBoxMappingId { get; set; }

        public int LayoutBoxId { get; set; }
        public LayoutBox LayoutBox { get; set; }

        public int CaliforniaViewId { get; set; }
        public CaliforniaView CaliforniaView { get; set; }

        [Required, StringLength(255)]
        public string QueryString { get; set; }
    }
}
