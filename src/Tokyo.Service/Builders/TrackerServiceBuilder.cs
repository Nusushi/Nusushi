using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Builders
{
    public class TrackerServiceBuilder
    {
        public IServiceCollection Services { get; }
        public TrackerServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
