using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using California.Creator.Service.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nusushi.Data.Data;
using Nusushi.Data.Models;
using Nusushi.Models;
using Nusushi.Models.IndexViewModels;
using Tokyo.Service.Data;

namespace Nusushi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly NusushiDbContext _nusushiDbContext;
        private readonly TokyoDbContext _tokyoDbContext;
        private readonly CaliforniaDbContext _californiaDbContext;

        public HomeController(IHostingEnvironment hostingEnvironment, NusushiDbContext nusushiDbContext, TokyoDbContext tokyoDbContext, CaliforniaDbContext californiaDbContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _nusushiDbContext = nusushiDbContext;
            _tokyoDbContext = tokyoDbContext;
            _californiaDbContext = californiaDbContext;
        }

        public async Task<IActionResult> Index()
        {
            // TODO save and display last accessed, created dates, and also sort by last accessed date, and highlight last modified < configurable time with color
            var tokyoAccountsInUse = await _tokyoDbContext.TrackerStores.Include(t => t.TimeTables).Where(t => t.TimeStamps.Count() > 0).ToDictionaryAsync(t => t.TrackerStoreId);
            //var tokyoAccountsInUse = await _tokyoDbContext.TrackerStores.Include(t => t.TimeTables).Where(t => t.TimeStamps.Count() > 0).Select(t => new { TrackerStoreId = t.TrackerStoreId, TimeTables = t.TimeTables, StampCount = t.TimeStamps.Count() }).ToDictionaryAsync(t => t.TrackerStoreId);
            var californiaAccountsInUse = await _californiaDbContext.CaliforniaStores
                .Include(c => c.CaliforniaProjects)
                    .ThenInclude(cp => cp.CaliforniaViews)
                .Where(c => 
                    c.CaliforniaProjects.Count() > 0 
                    && c.CaliforniaProjects.Any(cp => 
                        cp.CaliforniaViews.Count() > 0 
                        && cp.CaliforniaViews.Any(vie => 
                            !vie.IsInternal.Value
                            && vie.PlacedLayoutRows.Any(lrow => 
                                lrow.AllBoxesBelowRow.Any(subox => 
                                    subox.PlacedInBoxAtoms.Count() > 0
                                )
                            )
                        )
                    )
                )
                .ToDictionaryAsync(c => c.CaliforniaStoreId);
            var indexVM = new IndexViewModel()
            {
                HostingEnvironment = _hostingEnvironment,
                NusushiAccounts = (await _nusushiDbContext.Set<NusushiUser>().ToArrayAsync()).Select(user =>
                {
                    tokyoAccountsInUse.TryGetValue(user.Id.ToString(), out var tokyoStore);
                    californiaAccountsInUse.TryGetValue(user.Id.ToString(), out var californiaStore);
                    var timeStampCount = tokyoStore != null ? _tokyoDbContext.Entry(tokyoStore).Collection(s => s.TimeStamps).Query().Count() : 0;
                    var tokyoDescription = tokyoStore != null ? (string.Join(",", tokyoStore.TimeTables.Take(Math.Min(tokyoStore.TimeTables.Count(), 50)).Select(t => t.Name)) + ", timestamps: " + timeStampCount.ToString()) : null;
                    var californiaDescription = californiaStore != null ? (string.Join(",", californiaStore?.CaliforniaProjects.First(/*TODO not first*/).CaliforniaViews.Where(v => !v.IsInternal.Value).Take(Math.Min(californiaStore.CaliforniaProjects.First(/*TODO not first*/).CaliforniaViews.Count(), 50)).Select(t => t.Name))) : null;
                    var result = new NusushiAccountData()
                    {
                        NusushiUserId = user.Id.ToString(),
                        TokyoUserId = tokyoStore?.TrackerStoreId ?? null,
                        TokyoDescription = tokyoDescription,
                        CaliforniaUserId = californiaStore?.CaliforniaStoreId ?? null,
                        CaliforniaDescription = californiaDescription,
                        TimeStampCount = timeStampCount
                    };
                    return result;
                })
            };
            return View(indexVM);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
