using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Abstractions
{
    public interface ITrackerJobBuilder// TODO <TUserJob> where TUserJob : ITrackerJob
    {
        ITrackerJob Build();
        ITrackerJobBuilder Configure(IEnumerable<KeyValuePair<string, object>> optionValues);
        bool Validate();
    }
}
