using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace California.Creator.Service.Middlewares
{
    public class CaliforniaProjectAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, CaliforniaProject>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, CaliforniaProject californiaProject)
        {
            var californiaStoreClaim = context.User.Claims.FirstOrDefault(c => c.Type == NusushiClaim.CaliforniaStoreClaimType);
            if (californiaStoreClaim == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                if (californiaProject.CaliforniaStoreId == californiaStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (californiaProject.SharedProjectInfos.Any(t => t.SharedWithCaliforniaStoreId == californiaStoreClaim.Value))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                if (californiaProject.CaliforniaStoreId == californiaStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (californiaProject.SharedProjectInfos.Any(t => (t.SharedWithCaliforniaStoreId == californiaStoreClaim.Value) && (t.IsEditAllowed.Value)))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                if (californiaProject.CaliforniaStoreId == californiaStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (californiaProject.SharedProjectInfos.Any(t => (t.SharedWithCaliforniaStoreId == californiaStoreClaim.Value) && (t.IsReshareAllowed.Value)))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
