using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class LayoutAtom : LayoutBase
    {
        public override int LayoutBaseId { get; set; } // overrides for client app code generation
        public override StyleMolecule StyleMolecule { get; set; }
        public override int LayoutSortOrderKey { get; set; } // default value => sql sequence
        public override LayoutType LayoutType { get { return LayoutType.Atom; } }

        public int PlacedAtomInBoxId { get; set; }
        public LayoutBox PlacedAtomInBox { get; set; }
        
        public ContentAtom HostedContentAtom { get; set; }

        public List<LayoutStyleInteraction> LayoutStyleInteractions { get; set; }

        [NotMapped] // TODO this property should only be visible client side
        public int Level { get; } = 0;
    }
}
