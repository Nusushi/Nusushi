using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Data;
using Tokyo.Service.Options.Jobs;

namespace Tokyo.Service.Workers.Jobs
{
    public class RemoveUserJob : TrackerJob
    {
        public RemoveUserJob(IServiceProvider serviceProvider, RemoveUserJobOptions options) : base(serviceProvider)
        {
            Options = options;
        }
        public RemoveUserJobOptions Options { get; }

        public override async Task<TrackerJobResult> RunAsync()
        {
            return await RemoveUserStoresAsync(Options.UserId);
        }
        private async Task<TrackerJobResult> RemoveUserStoresAsync(string userId)
        {
            var serviceScope = ServiceProvider.CreateScope();
            var data = serviceScope.ServiceProvider.GetRequiredService<TokyoDbContext>();
            var statusMessages = new List<string>();
            var userStore = await data.TrackerStores.SingleOrDefaultAsync(t => t.TrackerStoreId == userId);
            if (userStore != null)
            {
                data.TrackerStores.Remove(userStore);
                await data.SaveChangesAsync();
                statusMessages.Add($"Deleted {userStore.TrackerStoreId}"); // TODO check null ref
            }
            else
            {
                Logger.LogWarning($"User store of user {userId} could not be found.");
            }
            serviceScope.Dispose();
            return new TrackerJobResult() { Success = true, StatusMessage = $"{string.Join(", ", statusMessages)}.", Name = Name };
        }
    }
}
