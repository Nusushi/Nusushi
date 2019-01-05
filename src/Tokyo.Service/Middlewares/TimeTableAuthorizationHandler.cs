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
    public class TimeTableAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, TimeTable>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, TimeTable timeTable)
        {
            var trackerStoreClaim = context.User.Claims.FirstOrDefault(c => c.Type == NusushiClaim.TrackerStoreClaimType);
            if (trackerStoreClaim == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (requirement.Name == TrackerAuthorization.ReadRequirement.Name)
            {
                if (timeTable.TrackerStoreId == trackerStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (timeTable.SharedTimeTableInfos.Any(t => t.SharedWithTrackerStoreId == trackerStoreClaim.Value))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == TrackerAuthorization.EditRequirement.Name)
            {
                if (timeTable.TrackerStoreId == trackerStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (timeTable.SharedTimeTableInfos.Any(t => (t.SharedWithTrackerStoreId == trackerStoreClaim.Value) && (t.IsEditAllowed.Value)))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement.Name == TrackerAuthorization.ShareRequirement.Name)
            {
                if (timeTable.TrackerStoreId == trackerStoreClaim.Value)
                {
                    context.Succeed(requirement);
                }
                if (timeTable.SharedTimeTableInfos.Any(t => (t.SharedWithTrackerStoreId == trackerStoreClaim.Value) && (t.IsReshareAllowed.Value)))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
