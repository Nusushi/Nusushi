using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public abstract class LayoutBase // SQL // TODO inheritance/discriminator mapping seems to be super expensive in EF Core
    {
        public abstract int LayoutBaseId { get; set; } // members are declared as abstract for client app code generation (TODO find a way to use inheritance and remove abstract + overrides)
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; } // SQL
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        public abstract StyleMolecule StyleMolecule { get; set; }

        public abstract int LayoutSortOrderKey { get; set; } // SQL // default value => sql sequence

        public abstract LayoutType LayoutType { get; } // for client app, hardcoded TODO can be removed?
    }
}
