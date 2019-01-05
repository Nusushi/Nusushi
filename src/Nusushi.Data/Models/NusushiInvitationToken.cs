using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Nusushi.Data.Models
{
    public class NusushiInvitationToken
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // unique id
        public string NusushiInvitationTokenId { get; set; } = Guid.NewGuid().ToString("D");
        public string Description { get; set; } = "You have been invited to register an account at http://xn--xckd0d.com/."; // TODO https
        public bool IsBeenUsed { get; set; } = false;
    }
}
