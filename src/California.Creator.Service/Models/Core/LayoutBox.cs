using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class LayoutBox : LayoutBase
    {
        public override int LayoutBaseId { get; set; } // overrides for client app code generation
        public override StyleMolecule StyleMolecule { get; set; }
        public override int LayoutSortOrderKey { get; set; } // default value => sql sequence
        public override LayoutType LayoutType { get { return LayoutType.Box; } }

        public List<LayoutAtom> PlacedInBoxAtoms { get; set; } = new List<LayoutAtom>();
        public List<LayoutBox> PlacedInBoxBoxes { get; set; } = new List<LayoutBox>();

        public int? PlacedBoxInBoxId { get; set; }
        public LayoutBox PlacedBoxInBox { get; set; }

        public int BoxOwnerRowId { get; set; }
        public LayoutRow BoxOwnerRow { get; set; }

        public List<QueryViewLayoutBoxMapping> HostedViewMappings { get; set; }

        [Required]
        public SpecialLayoutBoxType? SpecialLayoutBoxType { get; set; } = Core.SpecialLayoutBoxType.Default;

        [NotMapped] // TODO this property should only be visible client side, but dont copy data => JsonIgnore not feasable
        public int Level { get; } = 0;
    }
}
