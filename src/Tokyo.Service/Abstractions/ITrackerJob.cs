using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Workers;

namespace Tokyo.Service.Abstractions
{
    public interface ITrackerJob
    {
        string Name { get; }
        Task<TrackerJobResult> RunAsync();
    }
}
