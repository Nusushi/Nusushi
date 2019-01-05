using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Builders;
using Tokyo.Service.Options.Jobs;

namespace Tokyo.Service.Workers.Jobs
{
    public class RemoveUserJobBuilder : TrackerJobBuilder
    {
        private readonly IOptions<RemoveUserJobOptions> _options;

        public RemoveUserJobBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _options = ServiceProvider.GetRequiredService<IOptions<RemoveUserJobOptions>>();
        }
        public override ITrackerJobBuilder Configure(IEnumerable<KeyValuePair<string, object>> optionValues)
        {
            var userId = optionValues.First(kvp => kvp.Key == "UserId").Value as string;
            return Configure(options => options.UserId = userId);
        }
        public RemoveUserJobBuilder Configure(Action<RemoveUserJobOptions> configure)
        {
            configure.Invoke(_options.Value);
            Validate();
            Job = new RemoveUserJob(ServiceProvider, _options.Value);
            return this;
        }
        public override bool Validate()
        {
            if (string.IsNullOrEmpty(_options.Value.UserId))
            {
                return false;
                // throw new ArgumentNullException(nameof(RemoveUserJobOptions.UserId)); TODO validation message result
            }
            return true;
        }
    }
}
