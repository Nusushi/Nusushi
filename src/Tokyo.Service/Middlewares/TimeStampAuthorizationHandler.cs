using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Middlewares
{
    public class TimeStampAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, TimeStamp>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TimeStamp timeStamp)
        {
            var trackerStoreClaim = context.User.Claims.FirstOrDefault(c => c.Type == NusushiClaim.TrackerStoreClaimType);
            if (trackerStoreClaim == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (requirement.Name == TrackerAuthorization.ReadRequirement.Name)
            {
                if (timeStamp.TrackerStoreId == trackerStoreClaim.Value) // TODO security audit: not clear if table auth implies timestamp auth
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                if (timeStamp.TrackerStoreId == trackerStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == TrackerAuthorization.ShareRequirement.Name)
            {
                if (timeStamp.TrackerStoreId == trackerStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
