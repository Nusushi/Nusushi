using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nusushi.Data.Models
{
    public class NusushiRole : IdentityRole<Guid>
    {
        public NusushiRole()
        {
        }

        public NusushiRole(string roleName) : base(roleName)
        {
        }
    }
}
