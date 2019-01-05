using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class PreventDeveloperCodePropagation : Attribute // TODO sealed class? can't use in controller then...
    {
        // See the attribute guidelines at 
        // TODO http://go.microsoft.com/fwlink/?LinkId=85236

        public PreventDeveloperCodePropagation()
        {
            // TODO is this class necessary? vs JsonIgnore
        }
    }
}
