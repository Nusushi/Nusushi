using California.Creator.Service.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace California.Creator.Service.Models
{
    public class DbContentSafetyLock // TODO make one unique lock for all content to save space // TODO set roles/acces rights to lock table
    {
        public int DbContentSafetyLockId { get; set; }
        
        public int ContentAtomId { get; set; }
        public ContentAtom ContentAtom { get; set; }
    }
}
