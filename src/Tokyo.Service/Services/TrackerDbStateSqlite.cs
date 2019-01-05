using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokyo.Service.Data;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Options;

namespace Tokyo.Service.Services
{
    public class TrackerDbStateSqlite
    {
        private int _workUnitOrderSequence = -1;
        private int _timeNormOrderSequence = -1;
        private bool isActive = false;

        public TrackerDbStateSqlite(IOptions<TrackerServiceOptions> trackerServiceOptions, IServiceProvider serviceProvider)
        {
            if (trackerServiceOptions.Value.DatabaseTechnology == TrackerServiceOptions.DbEngineSelector.Sqlite)
            {
                isActive = true;
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    using (var tokyoDbContext = serviceScope.ServiceProvider.GetRequiredService<TokyoDbContext>())
                    {
                        var workUnitSortKeys = tokyoDbContext.Set<WorkUnit>().ToList().Select(w => w.ManualSortOrderKey);
                        _workUnitOrderSequence = workUnitSortKeys.Count() > 0 ? workUnitSortKeys.Max() + 1 : 1;
                        var timeNormSortKeys = tokyoDbContext.Set<TimeNorm>().ToList().Select(t => t.ManualSortOrderKey);
                        _timeNormOrderSequence = timeNormSortKeys.Count() > 0 ? timeNormSortKeys.Max() + 1 : 1;
                    }
                }
            }
        }

        public int GetAndIncrementWorkUnitSortKey() // TODO document not threaded with sqlite
        {
            if (!isActive)
            {
                return 0;
            }
            var result = _workUnitOrderSequence;
            _workUnitOrderSequence += 1;
            return result;
        }

        public int GetAndIncrementTimeNormSortKey() // TODO document not threaded with sqlite
        {
            if (!isActive)
            {
                return 0;
            }
            var result = _timeNormOrderSequence;
            _timeNormOrderSequence += 1;
            return result;
        }
    }
}
