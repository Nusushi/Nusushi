using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Nusushi.Data.Models
{
    public class NusushiUser : IdentityUser<Guid>
    {
        [Required, DataType(DataType.Text), StringLength(255, MinimumLength = 1)]
        public string RegistrationReference { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDateTime { get; set; }
        public NusushiInvitationToken NusushiInvitationToken { get; set; }
    }
}
