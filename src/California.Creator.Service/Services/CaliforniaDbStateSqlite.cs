using California.Creator.Service.Data;
using California.Creator.Service.Models.Core;
using California.Creator.Service.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace California.Creator.Service.Services
{
    public class CaliforniaDbStateSqlite
    {
        private int _layoutBaseOrderSequence = -1;
        private int _viewUnitOrderSequence = -1;
        private bool isActive = false;

        public CaliforniaDbStateSqlite(IOptions<CaliforniaServiceOptions> californiaServiceOptions, IServiceProvider serviceProvider)
        {
            if (californiaServiceOptions.Value.DatabaseTechnology == CaliforniaServiceOptions.DbEngineSelector.Sqlite)
            {
                isActive = true;
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    using (var californiaDbContext = serviceScope.ServiceProvider.GetRequiredService<CaliforniaDbContext>())
                    {
                        var layoutBaseSortKeys = californiaDbContext.Set<LayoutBase>().ToList().Select(l => l.LayoutSortOrderKey);
                        _layoutBaseOrderSequence = layoutBaseSortKeys.Count() > 0 ? layoutBaseSortKeys.Max() + 1 : 1;
                        var viewUnitSortKeys = californiaDbContext.Set<CaliforniaView>().ToList().Select(v => v.ViewSortOrderKey);
                        _viewUnitOrderSequence = viewUnitSortKeys.Count() > 0 ? viewUnitSortKeys.Max() + 1 : 1;
                    }
                }
            }
        }

        public int GetAndIncrementLayoutBaseSortKey() // TODO document not threaded with sqlite
        {
            if (!isActive)
            {
                return 0;
            }
            var result = _layoutBaseOrderSequence;
            _layoutBaseOrderSequence += 1;
            return result;
        }

        public int GetAndIncrementCaliforniaViewSortKey() // TODO document not threaded with sqlite
        {
            if (!isActive)
            {
                return 0;
            }
            var result = _viewUnitOrderSequence;
            _viewUnitOrderSequence += 1;
            return result;
        }
    }
}
