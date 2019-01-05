using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class LayoutRow : LayoutBase
    {
        public override int LayoutBaseId { get; set; } // overrides for client app code generation
        public override StyleMolecule StyleMolecule { get; set; }
        public override int LayoutSortOrderKey { get; set; } // default value => sql sequence

        public override LayoutType LayoutType { get { return LayoutType.Row; } }

        public List<LayoutBox> AllBoxesBelowRow { get; set; } = new List<LayoutBox>();

        public int PlacedOnViewId { get; set; }
        [JsonIgnore]
        public CaliforniaView PlacedOnView { get; set; }

        [NotMapped] // TODO this property should only be visible client side, but dont copy data => JsonIgnore not feasable
        public int DeepestLevel { get; } = 0;
    }
}
