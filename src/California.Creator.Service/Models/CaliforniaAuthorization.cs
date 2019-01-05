using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models
{
    public class CaliforniaAuthorization
    {
        public static OperationAuthorizationRequirement ReadRequirement = new OperationAuthorizationRequirement() { Name = "Read" };
        public static OperationAuthorizationRequirement ShareRequirement = new OperationAuthorizationRequirement() { Name = "Share" };
        public static OperationAuthorizationRequirement EditRequirement = new OperationAuthorizationRequirement() { Name = "Edit" };
    }
}
