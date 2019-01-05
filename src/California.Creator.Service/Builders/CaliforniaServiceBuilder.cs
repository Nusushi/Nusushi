using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Builders
{
    public class CaliforniaServiceBuilder
    {
        public IServiceCollection Services { get; }
        public CaliforniaServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
