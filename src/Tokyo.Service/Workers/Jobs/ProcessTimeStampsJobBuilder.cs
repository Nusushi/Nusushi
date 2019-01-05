using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Builders;
using Tokyo.Service.Options.Jobs;
using Tokyo.Service.Services;

namespace Tokyo.Service.Workers.Jobs
{
    public class ProcessTimeStampsJobBuilder : TrackerJobBuilder
    {
        private readonly IOptions<ProcessTimeStampsJobOptions> _options;

        public ProcessTimeStampsJobBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _options = ServiceProvider.GetRequiredService<IOptions<ProcessTimeStampsJobOptions>>();
        }

        public override ITrackerJobBuilder Configure(IEnumerable<KeyValuePair<string, object>> optionValues)
        {
            var userId = optionValues.First(kvp => kvp.Key == nameof(ProcessTimeStampsJobOptions.UserId)).Value as string;
            int targetWorkUnitId = 0;
            int.TryParse(optionValues.First(kvp => kvp.Key == nameof(ProcessTimeStampsJobOptions.TargetWorkUnitId)).Value as string, out targetWorkUnitId);
            return Configure(options =>
            {
                options.UserId = userId;
                options.TargetWorkUnitId = (targetWorkUnitId != 0) ? (int?)targetWorkUnitId : null;
            });
        }
        private ITrackerJobBuilder Configure(Action<ProcessTimeStampsJobOptions> configure)
        {
            configure.Invoke(_options.Value);
            Validate(); // TODO validate string,string dictionary (before setting options)
            // Job = new RemoveUserJob(ServiceProvider, _processTimeStampsJobOptions.Value); TODO bug fixed prevent in future / write test case
            Job = new ProcessTimeStampsJob(ServiceProvider, _options.Value);
            return this;
        }
        public override bool Validate()
        {
            if (string.IsNullOrEmpty(_options.Value.UserId))
            {
                return false;
            }
            return true;
        }
    }
}
