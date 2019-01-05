using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Builders
{
    public class TrackerQueueBuilder
    {
        public IServiceCollection Services { get; }
        public TrackerQueueBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
