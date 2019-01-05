using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using California.Creator.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nusushi.Data.Models;
using Nusushi.Models.TokyoViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokyo.Service.Abstractions;
using Tokyo.Service.Data;
using Tokyo.Service.Extensions;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Options;
using Tokyo.Service.Services;

namespace Nusushi.Controllers
{
    // TODO automatically shorten links/parameter names
    // TODO automatically move binding parameters to view models + [FromBody]
    // TODO test cases per api routine: auth failure, 
    // TODO ValidateAntiForgeryToken
    [Authorize]
    public class TokyoController : Controller
    {
        private readonly UserManager<NusushiUser> _userManager;
        private readonly ILogger _logger;
        private readonly TrackerService _trackerService;
        //private readonly IStringLocalizer<ClientApp> _clientLocalizer;
        private readonly SignInManager<NusushiUser> _signInManager;
        private readonly TrackerServiceOptions _trackerServiceOptions;
        private readonly CaliforniaService _californiaService; // TODO object reference + store routines just for link to different project?

        private static List<TrackerTimeZone> TrackerTimeZones = TimeZoneInfo.GetSystemTimeZones().Select((tz, index) => new TrackerTimeZone() { Key = index, IdDotNet = tz.Id, DisplayName = tz.DisplayName }).ToList(); // TODO client should be consistent with backend

        public TokyoController(UserManager<NusushiUser> userManager, ILoggerFactory loggerFactory, TrackerService trackerService, 
            IOptions<TrackerServiceOptions> trackerServiceOptions, SignInManager<NusushiUser> signInManager, CaliforniaService californiaService)
        {
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<TokyoController>();
            _trackerService = trackerService;
            //_clientLocalizer = clientLocalizer;
            _signInManager = signInManager;
            _californiaService = californiaService;
            _trackerServiceOptions = trackerServiceOptions.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(string id, string token)
        {
            // TODO logout button in client UI doesnt work after some cases? application restarted in other IDE?
            // TODO redirect to https!
            if (!string.IsNullOrEmpty(id))
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null)
                {
                    // check if user matches id
                    var currentUserId = await _userManager.GetUserIdAsync(currentUser);
                    if (currentUserId != id)
                    {
                        await _signInManager.SignOutAsync();
                        currentUser = null;
                    }
                }
                if (currentUser != null)
                {
                    var currentUserId = await _userManager.GetUserIdAsync(currentUser);
                    // user logged in, check if claim was already created (TODO developer only, merge in nusushi registration)
                    if (User.Claims.FirstOrDefault(c => c.Type == NusushiClaim.TrackerStoreClaimType) == null)
                    {
                        var storeClaim = new Claim(NusushiClaim.TrackerStoreClaimType, currentUserId);
                        var storeClaimResult = await _userManager.AddClaimAsync(currentUser, storeClaim);
                        if (!storeClaimResult.Succeeded)
                        {
                            throw new ApplicationException("Could not add claim to user.");
                        }
                        await _signInManager.SignOutAsync();
                        currentUser = null;
                    }
                    else
                    {
                        // user logged in, check if store exists
                        // TODO
                        /* var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ReadStore);
                        if (await _trackerService.CreateUserStoreAndInitializeAsync(californiaContext, false, false) == null)
                        {
                            await _trackerService.CreateUserStoreAndInitializeAsync(trackerContext); // TODO remove for production, needed when dropping table often
                        }*/
                    }
                }
                if (currentUser == null)
                {
                    var existingRandomUser = await _userManager.GetUsersForClaimAsync(new Claim(NusushiClaim.CaliforniaStoreClaimType, id));
                    if (existingRandomUser.Count == 0)
                    {
                        existingRandomUser = await _userManager.GetUsersForClaimAsync(new Claim(NusushiClaim.TrackerStoreClaimType, id));
                    }

                    if (existingRandomUser.Count == 0)
                    {
                        return NotFound();
                    }
                    else if (existingRandomUser.Count > 1)
                    {
                        throw new ApplicationException("Duplicate users with claim to the same stores");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(existingRandomUser[0], isPersistent: true); // TODO make sure this is TURNED OFF FOR PRODUCTION OR MATCH WITH HARDCODED PASSWORD
                    }
                }
            }
            else
            {
                if (!_signInManager.IsSignedIn(HttpContext.User))
                {
                    // anonymous user, new project / no id => create and sign in user
                    // TODO duplicate code with account controller
                    var rng = new Random();
                    var randomUserName = "tokyouser" + rng.Next(100000000); // TODO check name is unique
                    var randomUser = new NusushiUser { UserName = randomUserName, Email = $"{randomUserName}@{_trackerServiceOptions.EmailDomainName}", RegistrationDateTime = DateTime.Now, RegistrationReference = "autogenerated", NusushiInvitationToken = new NusushiInvitationToken(), EmailConfirmed = true, PhoneNumber = "" };
                    var result = await _userManager.CreateAsync(randomUser, "ShouldBeRandomPass1*" /*TODO randomPass*/);
                    if (!result.Succeeded)
                    {
                        throw new ApplicationException("Random user could not be created.");
                    }
                    var userId = await _userManager.GetUserIdAsync(randomUser);
                    var storeClaim = new Claim(NusushiClaim.TrackerStoreClaimType, userId);
                    var storeClaimResult = await _userManager.AddClaimAsync(randomUser, storeClaim);
                    if (!storeClaimResult.Succeeded)
                    {
                        throw new ApplicationException("Could not add claim to user.");
                    }
                    var otherStoreClaim = new Claim(NusushiClaim.CaliforniaStoreClaimType, userId);
                    var otherStoreClaimResult = await _userManager.AddClaimAsync(randomUser, otherStoreClaim);
                    if (!otherStoreClaimResult.Succeeded)
                    {
                        throw new ApplicationException("Could not add other claim to user.");
                    }
                    await _signInManager.SignInAsync(randomUser, isPersistent: true);
                    // TODO code duplication
                    var trackerContext = new TrackerContext(TrackerEvent.CreateProfile, userId);
                    var californiaContext = new CaliforniaContext(CaliforniaEvent.CreateStore, userId);
                    await _trackerService.CreateUserStoreAndInitializeAsync(trackerContext);  // TODO can fail while db is updating => need rollback of user creation or check when accessing tokyo store
                    await _californiaService.CreateUserStoreAndInitializeAsync(californiaContext); // TODO can fail while db is updating => need rollback of user creation or check when accessing california store
                    await _californiaService.CreateDefaultProjectDataAsync(californiaContext); // TODO 2nd db save action
                    // TODO code duplication end
                    // TODO add public table share as read only
                }
                return RedirectToRoute(TrackerRoutes.TrackerBrowserRoute, new { id = _userManager.GetUserId(HttpContext.User) }); // TODO alternative pushState() => value maybe in browser history
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet] // TODO should not be cacheable, only works well because time string probably is always different
        public async Task<JsonResult> InitialClientData(string jsTimeString)
        {
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ShowInitialClientData);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var jsTimeZone = GetRequestTimeZone(null, jsTimeString); // TODO client side self check if time adjustments are compatible
            bool isCookieUpdateRequired = false;
            if (clientSession.DeviceTimeZone == null || clientSession.DeviceTimeZone != jsTimeZone)
            {
                clientSession.DeviceTimeZone = jsTimeZone; // TODO probably has to get updated after each request / location change while app is running
                isCookieUpdateRequired = true;
            }
            if (clientSession.ViewTimeZone == null)
            {
                clientSession.ViewTimeZone = await _trackerService.GetDefaultTimeZoneAsync(trackerContext);
                isCookieUpdateRequired = true;
            }
            if (isCookieUpdateRequired)
            {
                clientSession.UpdateTrackerOptionsCookie(Response, _trackerServiceOptions.CookieName);
            }
            /*var trackerClientFlags = new bool[Enum.GetNames(typeof(TrackerClientFlag)).Length]; TODO was just an example, currently unused
            if (!jsTimeZone.Equals(clientSession.DeviceTimeZone.Id))
            {
                trackerClientFlags[(int)TrackerClientFlag.IsDefaultDeviceTimeZone] = false;
            }
            else
            {
                trackerClientFlags[(int)TrackerClientFlag.IsDefaultDeviceTimeZone] = true;
            }*/
            // --- TODO code duplication
            var ownedTimeTables = await _trackerService.GetAllTimeTables(trackerContext);
            if (ownedTimeTables.Count == 0)
            {
                throw new ApplicationException("No user tables.");
            }
            await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement);
            var clientOwnerName = await GetTableOwnerNameAsync(ownedTimeTables.First().TimeTableId);
            var allTimeTableVMs = ownedTimeTables.Select(t =>
            {
                return new TimeTableViewModel(clientSession, t, clientOwnerName);
            }).ToList();
            // TODO title = $"Table {sharedTimeTableInfo.TimeTableId}, Shared by {timeTableOwnerName} ({ sharedTimeTableInfo.ShareEnabledTime })";
            var sharedTimeTables = await _trackerService.GetSharedByOtherTableInfosAsync(trackerContext);
            var sharedTimeTableVMs = sharedTimeTables.Select(async s =>
            {
                await _trackerService.AuthorizeTimeTableAsync(User, s.TimeTable.TimeTableId, TrackerAuthorization.ReadRequirement);
                var sharedTableOwnerName = await GetTableOwnerNameAsync(s.TimeTableId);
                return new TimeTableViewModel(clientSession, s.TimeTable, sharedTableOwnerName);
            }).Select(t => t.Result);
            allTimeTableVMs.Concat(sharedTimeTableVMs);
            var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
            var unboundTimeStamps = timeStamps.Where(t => !t.IsBound).Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
            var utcTimeForCalculations = DateTimeOffset.UtcNow;
            var clientTimelineOffset = clientSession.ViewTimeZone.GetUtcOffset(utcTimeForCalculations).Subtract(clientSession.DeviceTimeZone.GetUtcOffset(utcTimeForCalculations)); // TODO more efficient with local time?
            var clientTimelineOffsetString = clientTimelineOffset == TimeSpan.Zero ? "" : clientTimelineOffset.TotalMilliseconds.ToTrackerDuration(false, true);
            /// --- TODO code duplication end
            int selectedTimeTableId;
            if (clientSession.LastViewedTableId.HasValue && allTimeTableVMs.Any(t => t.TimeTableAndWorkUnits.TimeTableId == clientSession.LastViewedTableId))
            {
                selectedTimeTableId = clientSession.LastViewedTableId.Value;
            }
            else
            {
                selectedTimeTableId = ownedTimeTables.First().TimeTableId;
            }
            var defaulTargetWorkUnit = await _trackerService.GetDefaultTargetWorkUnitAsync(trackerContext);
            // TODO make sure everywhere that currentthread.culture and uiculture is taken into account, especially for day of week (server/client side); idea: force invariantCulture on server (what about server timezone, language, ...)
            // TODO localization
            // create data for client timeline: localized day names independent of server timezone
            var requestCulture = CultureInfo.CurrentCulture;
            var refDateMonday = new DateTimeOffset(new DateTime(2018, 01, 01), TimeSpan.Zero); // 01 jan 2018 is monday
            var fullDayNamesLocalized = requestCulture.DateTimeFormat.DayNames.ToList(); // localized day names, order depends on server timezone

            var fullDayNamesServer = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.ToList();
            var mondayIndexInNamesServer = fullDayNamesServer.IndexOf(refDateMonday.DayOfWeek.ToString());
            var firstDayOfWeekIndexInNamesServer = fullDayNamesServer.IndexOf(requestCulture.DateTimeFormat.FirstDayOfWeek.ToString()); // first day of week for request culture, in server language

            var localizedWeekDaysStartingMonday = new List<string>();
            var startDayOfWeekIndexStartingMonday = 0;
            for (int i = mondayIndexInNamesServer; i < 7; i++)
            {
                if (i == firstDayOfWeekIndexInNamesServer)
                {
                    startDayOfWeekIndexStartingMonday = localizedWeekDaysStartingMonday.Count;
                }
                localizedWeekDaysStartingMonday.Add(fullDayNamesLocalized[i]);
            }
            for (int i = 0; i < mondayIndexInNamesServer; i++)
            {
                if (i == firstDayOfWeekIndexInNamesServer)
                {
                    startDayOfWeekIndexStartingMonday = localizedWeekDaysStartingMonday.Count;
                }
                localizedWeekDaysStartingMonday.Add(fullDayNamesLocalized[i]);
            }
            
            //var translationsGER = _clientLocalizer.WithCulture(new System.Globalization.CultureInfo("de-DE")).GetAllStrings(true).Select(locString => new KeyValuePair<string, string>(locString.Name, locString.Value));
            //var translationsENU = _clientLocalizer.WithCulture(new System.Globalization.CultureInfo("en-US")).GetAllStrings(true).Select(locString => new KeyValuePair<string, string>(locString.Name, locString.Value));

            var clientVM = new TrackerClientViewModel()
            {
                StatusText = "",
                CurrentRevision = TrackerClientSession.TargetRevision,
                SelectedTimeTableId = selectedTimeTableId,
                UnboundTimeStamps = unboundTimeStamps,
                TrackerEvent = (int)trackerContext.TrackerEvent,
                TargetWorkUnitId = defaulTargetWorkUnit.WorkUnitId,
                TimeTables = allTimeTableVMs,
                SelectedTimeZoneIdTimeStamps = clientSession.DeviceTimeZone.Id,
                SelectedTimeZoneIdView = clientSession.ViewTimeZone.Id,
                TimeZones = TrackerTimeZones,
                CultureIds = _trackerServiceOptions.SupportedCultures.Select((culture, index) => new TrackerCultureInfo() { Key = index, IdDotNet = culture.Name, DisplayName = culture.NativeName }).ToList(), // TODO Cache
                WeekDayLetters = localizedWeekDaysStartingMonday.Select(dayName => dayName.Substring(0, 1)).ToArray(), // TODO cache
                AbbreviatedMonths = requestCulture.DateTimeFormat.AbbreviatedMonthNames, // TODO cache
                StartDayOfWeekIndex = startDayOfWeekIndexStartingMonday,
                SelectedCultureId = requestCulture.Name,
                TrackerClientFlags = new bool[0], // trackerClientFlags, TODO
                UrlToReadOnly = Url.Link(TrackerRoutes.TrackerBrowserRoute, new { id = trackerContext.UserId, action = nameof(Index) }),
                UrlToReadAndEdit = Url.Link(TrackerRoutes.TrackerBrowserRoute, new { id = trackerContext.UserId, action = nameof(Index), token = Guid.NewGuid().ToString("d") }), // TODO
                ClientTimelineOffset = clientTimelineOffsetString
            };
            // TODO use protobuf instead of json
            return Json(clientVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); // TODO use default routine for json response
        }

        [HttpPost]
        public async Task<JsonResult> EditProductivityRating([Required] int? timeNormId, [Required] double? ratingPercentage)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeNormAsync(User, timeNormId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditProductivityRating);
            var productivityRating = await _trackerService.EditProductivityRatingAsync(trackerContext, timeNormId.Value, ratingPercentage.Value);
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { UpdatedRating = productivityRating, TargetId = timeNormId.Value }, trackerContext);
        }

        private void ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                // TODO expected to throw exception when invalid data is passed but actually annoying / control flow by exceptions...?
                throw new InvalidOperationException();
            }
        }

        [HttpPost]
        public async Task<JsonResult> CreateTimeTable()
        {
            ValidateModelState();
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateTimeTable);
            var newTimeTable = await _trackerService.CreateTimeTableAsync(trackerContext);
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel()
            {
                TimeTable = new TimeTableViewModel(clientSession, newTimeTable, await GetTableOwnerNameAsync(newTimeTable.TimeTableId))
            }, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> MoveWorkUnitBeforeWorkUnit([Required] int? moveWorkUnitId, [Required] int? moveBeforeWorkUnitId)
        {
            ValidateModelState();
            if (moveWorkUnitId == moveBeforeWorkUnitId)
            {
                throw new InvalidOperationException("Work unit ids must be different.");
            }
            await _trackerService.AuthorizeWorkUnitAsync(User, moveWorkUnitId.Value, TrackerAuthorization.EditRequirement);
            await _trackerService.AuthorizeWorkUnitAsync(User, moveBeforeWorkUnitId.Value, TrackerAuthorization.EditRequirement);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.MoveWorkUnit);
            var affectedTableIds = await _trackerService.MoveWorkUnitAsync(trackerContext, moveWorkUnitId.Value, moveBeforeWorkUnitId.Value, true);
            var affectedTable = await _trackerService.GetFullTimeTableAsync(trackerContext, affectedTableIds.Item1);
            var tableOwnerName = await GetTableOwnerNameAsync(affectedTableIds.Item1);
            TimeTableViewModel timeTableVM = new TimeTableViewModel(clientSession, affectedTable, tableOwnerName);
            TimeTableViewModel timeTableSecondaryVM = null;
            if (affectedTableIds.Item2 != affectedTableIds.Item1)
            {
                affectedTable = await _trackerService.GetFullTimeTableAsync(trackerContext, affectedTableIds.Item2);
                tableOwnerName = await GetTableOwnerNameAsync(affectedTableIds.Item2);
                timeTableSecondaryVM = new TimeTableViewModel(clientSession, affectedTable, tableOwnerName);
            }
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeTable = timeTableVM, TimeTableSecondary = timeTableSecondaryVM }, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> SetWorkUnitAsDefaultTarget([Required] int? workUnitId)
        {
            // TODO fast change in UI possible => implement OperationCanceled Exception handler
            ValidateModelState();
            await _trackerService.AuthorizeWorkUnitAsync(User, workUnitId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.SetDefaultTargetWorkUnit);
            var newActiveWorkUnit = await _trackerService.SetWorkUnitAsDefaultTargetAsync(trackerContext, workUnitId.Value);
            if (newActiveWorkUnit == null)
            {
                return CreateJsonErrorForUser("Could not set default target work unit.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetWorkUnitId = workUnitId.Value }, trackerContext);
            }
        }

        /*[HttpPost] should rather be UpdateDeviceTimeZone for current jsTime OR rework device timezone to override and sync with client device time zone (which is currently fixed)
        public async Task<JsonResult> SetDeviceTimeZone([Required, MinLength(1)] string timeZoneIdDotNet)
        {
            ValidateModelState();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.SetDeviceTimeZone);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdDotNet);
            clientSession.DeviceTimeZone = timeZone;
            clientSession.UpdateTrackerOptionsCookie(Response, _trackerServiceOptions.CookieName);
            var trackerClientVM = new TrackerClientViewModel()
            {
                SelectedTimeZoneIdTimeStamps = timeZone.Id
            };
            return CreateJsonResultNoLoopsPascalCase(trackerClientVM, trackerContext);
        }*/

        [HttpPost]
        public async Task<JsonResult> SetViewTimeZone([Required, MinLength(1)] string timeZoneIdDotNet)
        {
            ValidateModelState();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.SetViewTimeZone);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdDotNet);
            clientSession.ViewTimeZone = timeZone;
            clientSession.UpdateTrackerOptionsCookie(Response, _trackerServiceOptions.CookieName);

            // --- TODO code duplication
            var ownedTimeTables = await _trackerService.GetAllTimeTables(trackerContext);
            if (ownedTimeTables.Count == 0)
            {
                throw new ApplicationException("No user tables.");
            }
            await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement);
            var clientOwnerName = await GetTableOwnerNameAsync(ownedTimeTables.First().TimeTableId);
            var allTimeTableVMs = ownedTimeTables.Select(t =>
            {
                return new TimeTableViewModel(clientSession, t, clientOwnerName);
            }).ToList();
            // TODO title = $"Table {sharedTimeTableInfo.TimeTableId}, Shared by {timeTableOwnerName} ({ sharedTimeTableInfo.ShareEnabledTime })";
            var sharedTimeTables = await _trackerService.GetSharedByOtherTableInfosAsync(trackerContext);
            var sharedTimeTableVMs = sharedTimeTables.Select(async s =>
            {
                await _trackerService.AuthorizeTimeTableAsync(User, s.TimeTable.TimeTableId, TrackerAuthorization.ReadRequirement);
                var sharedTableOwnerName = await GetTableOwnerNameAsync(s.TimeTableId);
                return new TimeTableViewModel(clientSession, s.TimeTable, sharedTableOwnerName);
            }).Select(t => t.Result);
            allTimeTableVMs.Concat(sharedTimeTableVMs);
            var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
            var unboundTimeStamps = timeStamps.Where(t => !t.IsBound).Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
            var utcTimeForCalculations = DateTimeOffset.UtcNow;
            var clientTimelineOffset = clientSession.ViewTimeZone.GetUtcOffset(utcTimeForCalculations).Subtract(clientSession.DeviceTimeZone.GetUtcOffset(utcTimeForCalculations)); // TODO more efficient with local time?
            var clientTimelineOffsetString = clientTimelineOffset == TimeSpan.Zero ? "" : clientTimelineOffset.TotalMilliseconds.ToTrackerDuration(false, true);
            /// --- TODO code duplication end

            var trackerClientVM = new TrackerClientViewModel()
            {
                SelectedTimeZoneIdView = timeZone.Id,
                TimeTables = allTimeTableVMs,
                UnboundTimeStamps = unboundTimeStamps,
                ClientTimelineOffset = clientTimelineOffsetString
            };
            return CreateJsonResultNoLoopsPascalCase(trackerClientVM, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> SetCulture([Required, MinLength(1)] string cultureIdDotNet)
        {
            ValidateModelState();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.SetCulture);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var newCulture = new RequestCulture(cultureIdDotNet);
            CultureInfo.CurrentCulture = newCulture.Culture; // TODO audit
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(newCulture),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), SameSite = SameSiteMode.Strict }); // TODO different cookie, TODO cookie expiration time?

            // --- TODO code duplication
            var ownedTimeTables = await _trackerService.GetAllTimeTables(trackerContext);
            if (ownedTimeTables.Count == 0)
            {
                throw new ApplicationException("No user tables.");
            }
            await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement);
            var clientOwnerName = await GetTableOwnerNameAsync(ownedTimeTables.First().TimeTableId);
            var allTimeTableVMs = ownedTimeTables.Select(t =>
            {
                return new TimeTableViewModel(clientSession, t, clientOwnerName);
            }).ToList();
            // TODO title = $"Table {sharedTimeTableInfo.TimeTableId}, Shared by {timeTableOwnerName} ({ sharedTimeTableInfo.ShareEnabledTime })";
            var sharedTimeTables = await _trackerService.GetSharedByOtherTableInfosAsync(trackerContext);
            var sharedTimeTableVMs = sharedTimeTables.Select(async s =>
            {
                await _trackerService.AuthorizeTimeTableAsync(User, s.TimeTable.TimeTableId, TrackerAuthorization.ReadRequirement);
                var sharedTableOwnerName = await GetTableOwnerNameAsync(s.TimeTableId);
                return new TimeTableViewModel(clientSession, s.TimeTable, sharedTableOwnerName);
            }).Select(t => t.Result);
            allTimeTableVMs.Concat(sharedTimeTableVMs);
            var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
            var unboundTimeStamps = timeStamps.Where(t => !t.IsBound).Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
            /// --- TODO code duplication end
            /// TODO code duplication
            var requestCulture = newCulture.Culture;
            var refDateMonday = new DateTimeOffset(new DateTime(2018, 01, 01), TimeSpan.Zero); // 01 jan 2018 is monday
            var fullDayNamesLocalized = requestCulture.DateTimeFormat.DayNames.ToList(); // localized day names, order depends on server timezone

            var fullDayNamesServer = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.ToList();
            var mondayIndexInNamesServer = fullDayNamesServer.IndexOf(refDateMonday.DayOfWeek.ToString());
            var firstDayOfWeekIndexInNamesServer = fullDayNamesServer.IndexOf(requestCulture.DateTimeFormat.FirstDayOfWeek.ToString()); // first day of week for request culture, in server language

            var localizedWeekDaysStartingMonday = new List<string>();
            var startDayOfWeekIndexStartingMonday = 0;
            for (int i = mondayIndexInNamesServer; i < 7; i++)
            {
                if (i == firstDayOfWeekIndexInNamesServer)
                {
                    startDayOfWeekIndexStartingMonday = localizedWeekDaysStartingMonday.Count;
                }
                localizedWeekDaysStartingMonday.Add(fullDayNamesLocalized[i]);
            }
            for (int i = 0; i < mondayIndexInNamesServer; i++)
            {
                if (i == firstDayOfWeekIndexInNamesServer)
                {
                    startDayOfWeekIndexStartingMonday = localizedWeekDaysStartingMonday.Count;
                }
                localizedWeekDaysStartingMonday.Add(fullDayNamesLocalized[i]);
            }
            /// TODO code duplication end

            var trackerClientVM = new TrackerClientViewModel()
            {
                SelectedCultureId = cultureIdDotNet,
                TimeTables = allTimeTableVMs,
                UnboundTimeStamps = unboundTimeStamps,
                WeekDayLetters = localizedWeekDaysStartingMonday.Select(dayName => dayName.Substring(0, 1)).ToArray(), // TODO cache + duplicated code
                AbbreviatedMonths = requestCulture.DateTimeFormat.AbbreviatedMonthNames, // TODO cache  + duplicated code
                StartDayOfWeekIndex = startDayOfWeekIndexStartingMonday,
            };

            return CreateJsonResultNoLoopsPascalCase(trackerClientVM, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> MoveTimeNormToWorkUnit([Required] int? timeNormId, [Required] int? targetWorkUnitId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeNormAsync(User, timeNormId.Value, TrackerAuthorization.EditRequirement);
            await _trackerService.AuthorizeTimeNormAsync(User, targetWorkUnitId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.MoveTimeNorm);
            var affectedWorkUnitIds = await _trackerService.MoveTimeNormToWorkUnit(timeNormId.Value, targetWorkUnitId.Value);
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { WorkUnitIdSource = affectedWorkUnitIds.Item1, WorkUnitIdTarget = affectedWorkUnitIds.Item2 }, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> ProcessTimeStamps()
        {
            ValidateModelState();
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ProcessTimeStamps);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var targetWorkUnit = await _trackerService.GetDefaultTargetWorkUnitAsync(trackerContext);
            await _trackerService.AuthorizeWorkUnitAsync(User, targetWorkUnit.WorkUnitId, TrackerAuthorization.EditRequirement);
            //var scheduledTask = await _trackerService.ScheduleProcessTimeStampsAsync(trackerContext, targetWorkUnit?.WorkUnitId); // TODO use signalR ? or periodically update all clients
            await _trackerService.ProcessTimeStampsAsync(trackerContext, targetWorkUnit.WorkUnitId);

            // --- TODO code duplication
            var ownedTimeTables = await _trackerService.GetAllTimeTables(trackerContext);
            if (ownedTimeTables.Count == 0)
            {
                throw new ApplicationException("No user tables.");
            }
            await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement);
            var clientOwnerName = await GetTableOwnerNameAsync(ownedTimeTables.First().TimeTableId);
            var allTimeTableVMs = ownedTimeTables.Select(t =>
            {
                return new TimeTableViewModel(clientSession, t, clientOwnerName);
            }).ToList();
            // TODO title = $"Table {sharedTimeTableInfo.TimeTableId}, Shared by {timeTableOwnerName} ({ sharedTimeTableInfo.ShareEnabledTime })";
            var sharedTimeTables = await _trackerService.GetSharedByOtherTableInfosAsync(trackerContext);
            var sharedTimeTableVMs = sharedTimeTables.Select(async s =>
            {
                await _trackerService.AuthorizeTimeTableAsync(User, s.TimeTable.TimeTableId, TrackerAuthorization.ReadRequirement);
                var sharedTableOwnerName = await GetTableOwnerNameAsync(s.TimeTableId);
                return new TimeTableViewModel(clientSession, s.TimeTable, sharedTableOwnerName);
            }).Select(t => t.Result);
            allTimeTableVMs.Concat(sharedTimeTableVMs);
            var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
            var unboundTimeStamps = timeStamps.Where(t => !t.IsBound).Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
            /// --- TODO code duplication end
            string targetDurationString = null;
            foreach (var table in allTimeTableVMs)
            {
                var isFound = false;
                foreach (var workUnit in table.TimeTableAndWorkUnits.WorkUnits)
                {
                    if (workUnit.WorkUnitId == targetWorkUnit.WorkUnitId)
                    {
                        targetDurationString = workUnit.DurationString;
                        break;
                    }
                }
                if (isFound)
                {
                    break;
                }
            }
            if (targetDurationString == null)
            {
                throw new ApplicationException("Could not find target work unit.");
            }
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeTables = allTimeTableVMs, UnboundTimeStamps = unboundTimeStamps, WorkUnitDurationString = targetDurationString, TargetWorkUnitId = targetWorkUnit.WorkUnitId }, trackerContext);
        }

        [HttpPost] // TODO rgba?
        public async Task<JsonResult> EditTimeNorm([Required]int? timeNormId, [MinLength(1)] string newTimeNormName, [Required] int? newColorR, [Required] int? newColorG, [Required] int? newColorB)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeNormAsync(User, timeNormId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditTimeNorm);
            var updatedTimeNorm = await _trackerService.EditTimeNormAsync(trackerContext, timeNormId.Value, newTimeNormName, newColorR.Value, newColorG.Value, newColorB.Value);
            if (updatedTimeNorm == null)
            {
                return CreateJsonErrorForUser("Could not edit time norm.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeNormId.Value, TimeNormNoChildren = updatedTimeNorm }, trackerContext);
            }
        }
        // TODO bug: loaded from cache on ipad although HttpPost
        // TODO HttpPost everywhere!! and body
        // TODO everywhere (where HttpGet)  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public async Task<JsonResult> CreateWorkUnit([Required] int? timeTableId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeTableAsync(User, timeTableId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateWorkUnit);
            var newWorkUnit = await _trackerService.CreateWorkUnitAsync(trackerContext, timeTableId.Value);
            if (newWorkUnit == null)
            {
                return CreateJsonErrorForUser("Work unit could not be created.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { WorkUnit = newWorkUnit }, trackerContext);
            }
        }

        /*[HttpPost]
        public async Task<JsonResult> RemoveTimeNorm([Required] int? timeNormId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeNormAsync(User, timeNormId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.RemoveTimeNorm);
            await _trackerService.RemoveTimeNormAsync(trackerContext, timeNormId.Value, true);
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeNormId.Value }, trackerContext);
        }*/

        [HttpPost]
        public async Task<JsonResult> UnbindTimeNorm([Required] int? timeNormId)
        {
            ValidateModelState();
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            await _trackerService.AuthorizeTimeNormAsync(User, timeNormId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.UnbindTimeNorm);
            var updatedDuration = await _trackerService.UnbindTimeNormAsync(trackerContext, timeNormId.Value, false);
            var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
            var unboundTimeStamps = timeStamps.Where(t => !t.IsBound).Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeNormId.Value, UnboundTimeStamps = unboundTimeStamps, WorkUnitDurationString = updatedDuration }, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveWorkUnit([Required] int? workUnitId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeWorkUnitAsync(User, workUnitId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.RemoveWorkUnit);
            await _trackerService.RemoveWorkUnitAsync(trackerContext, workUnitId.Value);
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = workUnitId.Value }, trackerContext);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveTimeTable([Required] int? timeTableId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeTableAsync(User, timeTableId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.RemoveTimeTable);
            var isSuccess = await _trackerService.RemoveTimeTableAsync(trackerContext, timeTableId.Value);
            if (!isSuccess)
            {
                return CreateJsonErrorForUser("Removing time table failed.");
            }
            return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeTableId.Value }, trackerContext);
        }

        //TODO [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<JsonResult> EditTimeStampPost([FromBody] EditTimeStampPostViewModel editTimeStampPostViewModel)
        {
            ValidateModelState();
            var clientSession = HttpContext.Features.Get<TrackerClientSession>(); // TODO
            await _trackerService.AuthorizeTimeStampAsync(User, editTimeStampPostViewModel.TimeStampId, TrackerAuthorization.EditRequirement);
            // await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement); TODO is this required when operating with time stamps?
            TimeStamp updatedTimeStamp = null;
            DateTime? updatedTime = null;
            if (editTimeStampPostViewModel.ExactTicks.HasValue && /* TODO fix typewriter output for nullable int*/ editTimeStampPostViewModel.ExactTicks.Value != 0)
            {
                // TODO implement in javascript client
                if (editTimeStampPostViewModel.YearOld == null
                || editTimeStampPostViewModel.MonthOld == null
                || editTimeStampPostViewModel.DayOld == null
                || editTimeStampPostViewModel.HourOld == null
                || editTimeStampPostViewModel.MinuteOld == null
                || editTimeStampPostViewModel.SecondOld == null
                || editTimeStampPostViewModel.MillisecondOld == null)
                {
                    throw new ArgumentNullException("Old time data");
                }
                // can't use values from client UI directly => create timestamp with exact ticks and adjust by difference t_diff = t_ref - t_new
                updatedTime = new DateTime(editTimeStampPostViewModel.ExactTicks.Value, DateTimeKind.Utc); // TODO fix => use local time
                updatedTime = updatedTime.Value.AddYears(editTimeStampPostViewModel.Year.Value - editTimeStampPostViewModel.YearOld.Value);
                updatedTime = updatedTime.Value.AddMonths(editTimeStampPostViewModel.Month.Value - editTimeStampPostViewModel.MonthOld.Value);
                updatedTime = updatedTime.Value.AddDays(editTimeStampPostViewModel.Day.Value - editTimeStampPostViewModel.DayOld.Value);
                updatedTime = updatedTime.Value.AddHours(editTimeStampPostViewModel.Hour.Value - editTimeStampPostViewModel.HourOld.Value);
                updatedTime = updatedTime.Value.AddMinutes(editTimeStampPostViewModel.Minute.Value - editTimeStampPostViewModel.MinuteOld.Value);
                updatedTime = updatedTime.Value.AddSeconds(editTimeStampPostViewModel.Second.Value - editTimeStampPostViewModel.SecondOld.Value);
                updatedTime = updatedTime.Value.AddMilliseconds(editTimeStampPostViewModel.Millisecond.Value - editTimeStampPostViewModel.MillisecondOld.Value);
            }
            else
            {
                updatedTime = new DateTime(
                    editTimeStampPostViewModel.Year.Value,
                    editTimeStampPostViewModel.Month.Value,
                    editTimeStampPostViewModel.Day.Value,
                    editTimeStampPostViewModel.Hour.Value,
                    editTimeStampPostViewModel.Minute.Value,
                    editTimeStampPostViewModel.Second.Value,
                    editTimeStampPostViewModel.Millisecond.Value,
                    DateTimeKind.Unspecified); // TODO test
            }
            //var targetTimeZone = GetRequestTimeZone(updatedTime.Value, editTimeStampPostViewModel.JsTimeString);
            if (clientSession.ViewTimeZone.IsAmbiguousTime(updatedTime.Value))
            { // TODO duplicated code where time is added/updated
                var ambiguousTimeOffsets = clientSession.ViewTimeZone.GetAmbiguousTimeOffsets(updatedTime.Value); // TODO audit
                return CreateJsonErrorForUser("Ambiguous time."); // TODO set/check absoluteOffset after client decision in timeStamp
            }
            else if (clientSession.ViewTimeZone.IsInvalidTime(updatedTime.Value))
            { // TODO invalidTime is not triggered? need try catch
                return CreateJsonErrorForUser("Invalid time dotnet.");
            }
            DateTimeOffset? dateTimeOffset = null;
            try
            {
                dateTimeOffset = new DateTimeOffset(updatedTime.Value, clientSession.ViewTimeZone.GetUtcOffset(updatedTime.Value));
            }
            catch (ArgumentException e)
            {
                if (e.Message.StartsWith("The UTC Offset of the local dateTime parameter does not match the offset argument."))
                {
                    return CreateJsonErrorForUser("Invalid time.");
                }
                else
                {
                    throw e;
                }
            }
            updatedTimeStamp = new TimeStamp()
            {
                TimeStampId = editTimeStampPostViewModel.TimeStampId,
                Name = editTimeStampPostViewModel.TimeStampName,
                TrackedTimeUTC = dateTimeOffset.Value.UtcDateTime,
                UtcOffsetAtCreation = clientSession.ViewTimeZone.GetUtcOffset(dateTimeOffset.Value.DateTime), // TODO code duplication w/ http get parse
                TimeZoneIdAtCreation = clientSession.ViewTimeZone.Id
            };
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditTimeStamp);
            var timeStampAndTimeNormAndSummedDuration = await _trackerService.EditTimeStampAsync(trackerContext, editTimeStampPostViewModel.TimeStampId, updatedTimeStamp); // TODO ef core convention => editStamp == addStampManually?
            var clientVM = new TrackerClientViewModel()
            {
                TimeStamp = timeStampAndTimeNormAndSummedDuration.Item1.SetTrackedTimeForView(clientSession),
                TimeNormNoChildren = timeStampAndTimeNormAndSummedDuration.Item2,
                TimeNormDurationString = timeStampAndTimeNormAndSummedDuration.Item3,
                WorkUnitDurationString = timeStampAndTimeNormAndSummedDuration.Item4
            };
            return CreateJsonResultNoLoopsPascalCase(clientVM, trackerContext);
        }

        [HttpPost]
        public JsonResult UndoLastUserAction()
        {
            // TODO
            // implementation example:
            // track list of modifying vs. navigating user actions per user
            // undo routines similar to client partial update routine: switch every tracker event and handle
            // vs. creating full undo rules similar to background jobs
            // sync with client UI browser (page back navigation description)
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<JsonResult> RemoveTimeStamp([Required] int? timeStampId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeStampAsync(User, timeStampId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.RemoveTimeStamp);
            var deleteSuccess = await _trackerService.RemoveTimeStampAsync(trackerContext, timeStampId.Value); // TODO use some task cancellation procedure // TODO benchmark and compare

            if (deleteSuccess)
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeStampId.Value }, trackerContext);
            }
            else
            {
                return CreateJsonErrorForUser("Timestamp could not be removed.");
            }
        }

        /* TODO workaround => clean cookies when info: Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationHandler[7]
        Identity.Application was not authenticated. Failure message: Unprotect ticket failed
        info: Microsoft.AspNetCore.Authorization.DefaultAuthorizationService[2]*/


        [HttpPost]
        public async Task<JsonResult> EditTimeTable(int? timeTableId, [MinLength(1)] string newTimeTableName)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeTableAsync(User, timeTableId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditTimeTable);
            var editSuccess = await _trackerService.EditTimeTableAsync(trackerContext, timeTableId.Value, newTimeTableName);
            if (editSuccess)
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeTableId.Value, UpdatedName = newTimeTableName }, trackerContext);
            }
            else
            {
                return CreateJsonErrorForUser("Could not edit time table.");
            }
        }

        [HttpPost]
        public async Task<JsonResult> EditWorkUnit(int? workUnitId, [MinLength(1)] string newWorkUnitName)
        {
            ValidateModelState();
            await _trackerService.AuthorizeWorkUnitAsync(User, workUnitId.Value, TrackerAuthorization.EditRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditWorkUnit);
            var editSuccess = await _trackerService.EditWorkUnitAsync(trackerContext, workUnitId.Value, newWorkUnitName);
            if (editSuccess)
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = workUnitId.Value, UpdatedName = newWorkUnitName }, trackerContext);
            }
            else
            {
                return CreateJsonErrorForUser("Could not edit work unit.");
            }
        }

        /*[HttpGet] // TODO not required anymore
        public async Task<IActionResult> EditTimeStamp(EditTimeStampViewModel editTimeStampViewModel)
        {
            // TODO fix for usage from javascript client i.e. define common expectations for CSHTML-UI and JS-UI
            if (!ModelState.IsValid)
            {
                editTimeStampViewModel.StatusMessage = StatusMessageFromModelState();
                return View(editTimeStampViewModel);
            }
            if (editTimeStampViewModel == null)
            {
                editTimeStampViewModel = new EditTimeStampViewModel();
            }
            if (editTimeStampViewModel.Year == null)
            {
                if (!editTimeStampViewModel.TimeStampId.HasValue)
                {
                    editTimeStampViewModel.StatusMessage = "Select timestamp for edit."; // TODO redirect to timestamps list?
                    return View(editTimeStampViewModel);
                }
                var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ShowTimeStamp);
                var timeStamp = await _trackerService.GetTimeStampAsync(trackerContext, editTimeStampViewModel.TimeStampId.Value);
                editTimeStampViewModel.TimeStampId = timeStamp.TimeStampId;
                editTimeStampViewModel.Year = timeStamp.TrackedTimeUTC.Year;
                editTimeStampViewModel.Month = timeStamp.TrackedTimeUTC.Month;
                editTimeStampViewModel.Day = timeStamp.TrackedTimeUTC.Day;
                editTimeStampViewModel.Hour = timeStamp.TrackedTimeUTC.Hour;
                editTimeStampViewModel.Minute = timeStamp.TrackedTimeUTC.Minute;
                editTimeStampViewModel.Second = timeStamp.TrackedTimeUTC.Second;
                editTimeStampViewModel.Millisecond = timeStamp.TrackedTimeUTC.Millisecond;
                editTimeStampViewModel.ExactTicks = timeStamp.TrackedTimeUTC.Ticks;
                editTimeStampViewModel.StatusMessage = $"Time Stamp {timeStamp.TimeStampId}";
            }
            return View(editTimeStampViewModel);
        }*/

        /*[HttpGet] // TODO not required anymore
        public IActionResult CreateTimeStampManually(CreateTimeStampViewModel suggestedTimeStampViewModel = null)
        {
            DateTime suggestedTime = DateTime.Now;
            var statusMessage = "";
            if (!string.IsNullOrEmpty(suggestedTimeStampViewModel?.ManualDateTimeString))
            {
                if (DateTime.TryParse(suggestedTimeStampViewModel.ManualDateTimeString, out suggestedTime)) // TODO parse with user localization setting
                {
                    statusMessage = "Parsed successfully.";
                }
                else
                {
                    suggestedTime = DateTime.Now;
                    suggestedTimeStampViewModel.ManualDateTimeString = null;
                    statusMessage = "Could not parse time string.";
                }
            }
            var createTimeStampViewModel = new CreateTimeStampViewModel()
            {
                ManualDateTimeString = suggestedTimeStampViewModel?.ManualDateTimeString,
                Year = suggestedTime.Year,
                Month = suggestedTime.Month,
                Day = suggestedTime.Day,
                Hour = suggestedTime.Hour,
                Minute = suggestedTime.Minute,
                Second = suggestedTime.Second,
                Millisecond = suggestedTime.Millisecond,
                StatusMessage = statusMessage,
            };
            return View(createTimeStampViewModel);
        }*/

        [HttpPost]
        public JsonResult CreateTimeNormTag(string name, string color)
        {
            throw new NotImplementedException();
            /*if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(color))
            {
                throw new ArgumentNullException(nameof(color));
            }
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateTimeNormTag);
            var createdTag = await _trackerService.CreateTimeNormTagAsync(trackerContext, name, color);
            if (createdTag == null)
            {
                return CreateJsonErrorForUser("Could not create time norm tag.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeNormTag = createdTag }, trackerContext);
            }*/
        }

        [HttpPost]
        public JsonResult EditTimeNormTag(int? timeNormTagId, string name, string color)
        {
            throw new NotImplementedException();
            /*
            if (!timeNormTagId.HasValue)
            {
                throw new ArgumentNullException(nameof(timeNormTagId));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(color))
            {
                throw new ArgumentNullException(nameof(color));
            }
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.EditTimeNormTag);
            TimeNormTag updatedTimeNormTag = new TimeNormTag()
            {
                Name = name,
                Color = color
            };
            var updatedTag = await _trackerService.EditTimeNormTagAsync(trackerContext, timeNormTagId.Value, updatedTimeNormTag);
            if (updatedTag == null)
            {
                return CreateJsonErrorForUser("Could not edit time norm tag.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeNormTag = updatedTag }, trackerContext);
            }*/
        }

        [HttpPost]
        public JsonResult RemoveTimeNormTag(int? timeNormTagId)
        {
            throw new NotImplementedException();
            /*
            if (!timeNormTagId.HasValue)
            {
                throw new ArgumentNullException(nameof(timeNormTagId));
            }
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.RemoveTimeNormTag);
            var success = await _trackerService.RemoveTimeNormTagAsync(trackerContext, timeNormTagId.Value);
            if (!success)
            {
                return CreateJsonErrorForUser("Could not delete time norm tag.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TargetId = timeNormTagId.Value }, trackerContext);
            }*/
        }

        [HttpGet]
        public async Task<IActionResult> ExportTimeTableToCSV([Required] int? timeTableId) // TODO alternative to using request time zone => send file name with local time by js
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeTableAsync(User, timeTableId.Value, TrackerAuthorization.ReadRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ExportTimeTableToCSV);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>(); // TODO client session can be null => use default values
            if (clientSession.DeviceTimeZone == null || clientSession.ViewTimeZone == null)
            {
                return BadRequest("Invalid session.");
            }
            var dataCSV = await _trackerService.ExportTimeTableToCSVAsync(trackerContext, clientSession, timeTableId.Value);
            if (dataCSV.Data.Length == 0)
            {
                return BadRequest("Time table is empty.");
            }
            else if (dataCSV.Data.Length > 51200) // 50MB // TODO document
            {
                return BadRequest("Exported data too large.");
            }
            var currentTimeOfUser = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientSession.DeviceTimeZone);
            var fileNameDateTimeString = currentTimeOfUser.ToString("yyyy_MM_dd-HH_mm_ss", CultureInfo.InvariantCulture); // TODO check all .toString if invariant is required
            var fileName = $"{timeTableId}_{dataCSV.TableName}_{fileNameDateTimeString}.csv";

            return File(dataCSV.Data, "text/csv;charset=UTF-8", fileName);
        }

        [HttpPost]
        public FileResult TimeTablePlot([Required] int? timeTableId)
        {
            throw new NotImplementedException();
            /*if (!timeTableId.HasValue)
            {
                throw new ArgumentNullException(nameof(timeTableId));
            }
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ShowPlot);
            // TODO one unnecessary roundtrip to DB => use blob storage and query file directly
            var plot = await _trackerService.GetTimeTablePlotAsync(trackerContext, timeTableId.Value);
            if (plot == null)
            {
                throw new NotImplementedException();
            }
            return File(plot.ImageData, plot.MediaType, $"plot.png?v={System.Convert.ToBase64String(plot.Version)}");*/
        }

        private TimeZoneInfo GetRequestTimeZone(DateTime? targetTimeNumbers, string jsTimeString) // TODO search for all implicit casts DateTime=>DateTimeOffset
        {
            TimeZoneInfo targetTimeZone = null;
            // expected format: "Fri Oct 20 2017 06:05:19 GMT+0200 (W. Europe Daylight Time)"
            var timeZoneIdStart = jsTimeString.IndexOf('(');
            var timeZoneIdEnd = jsTimeString.IndexOf(')');
            if (timeZoneIdStart >= 6 && timeZoneIdEnd > (timeZoneIdStart + 1))
            {
                bool parseOffsetSuccess = true;
                TimeSpan? parsedTimeSpan = null;
                var offsetElement = jsTimeString.Substring(timeZoneIdStart - 6, 5);
                // parse time zone id
                var timeZoneString = jsTimeString.Substring(timeZoneIdStart + 1, timeZoneIdEnd - timeZoneIdStart - 1);

                // parse time zone offset
                var signs = new char[] { '+', '-' };
                var offsetSignPosition = offsetElement.IndexOfAny(signs);
                var lastOffsetIndex = offsetElement.Length - 1;
                if (offsetSignPosition > -1)
                {
                    if ((lastOffsetIndex - offsetSignPosition) == 4)
                    {
                        var parsedOffsetSign = offsetElement.Substring(offsetSignPosition, 1);
                        var parsedOffsetHours = offsetElement.Substring(offsetSignPosition + 1, 2);
                        var parsedOffsetMinutes = offsetElement.Substring(offsetSignPosition + 3, 2);
                        // TODO let user decide when offset from client doesn't match offset in system (time zone data mismatch)
                        if (int.TryParse(parsedOffsetHours, out var parsedHours) && int.TryParse(parsedOffsetMinutes, out var parsedMinutes))
                        {
                            int sign = parsedOffsetSign == "+" ? 1 : -1;
                            if (parsedHours != 0)
                            {
                                parsedHours *= sign;
                            }
                            else if (parsedMinutes != 0)
                            {
                                parsedMinutes *= sign;
                            }
                            else
                            {
                                parseOffsetSuccess = false;
                            }
                            parsedTimeSpan = new TimeSpan(parsedHours, parsedMinutes, 0);
                        }
                        else
                        {
                            parseOffsetSuccess = false;
                        }
                    }
                }

                // TODO test for all languages
                // TODO add right to left version for arabic languages
                if (parseOffsetSuccess) // TODO enable && targetTimeNumbers.HasValue) // TODO fix for targetLocalTime==null
                {
                    targetTimeZone = _trackerServiceOptions.SupportedTimeZones.FirstOrDefault(tz => tz.DaylightName.Contains(timeZoneString) || tz.StandardName.Contains(timeZoneString)); // TODO probably not necessary
                    if (targetTimeZone == null)
                    {
                        // TODO fix
                        // targetTimeZone = _trackerServiceOptions.SupportedTimeZones.FirstOrDefault(tz => tz.GetUtcOffset(targetTimeNumbers.Value) == parsedTimeSpan); // TODO performance
                    }
                    /* TODO enable var supposedOffset = targetTimeZone.GetUtcOffset(targetTimeNumbers.Value);
                    if (parsedTimeSpan != supposedOffset) // TODO kind of assert for the case time zone is detected by name
                    {
                        targetTimeZone = null;
                    }*/
                }
            }
            if (targetTimeZone == null)
            {
                return TimeZoneInfo.Local;
                // TODO throw new InvalidOperationException("TimeZone not supported.");
            }
            return targetTimeZone;
        }

        public JsonResult CreateTimeStamps(string[] timeStampStrings, int? count)
        {
            // TODO create multiple time stamps at once
            throw new NotImplementedException();
        }

        // TODO CreateTimeStampAndFinish / CreateTimeStampBookmark => LoaderOptimization mini.js and create timestamp from js / ...
        // TODO CHECK 100000 request/s

        [ResponseCache(NoStore = true)] // TODO validationTokens
        [HttpGet] // TODO rename to Timestamp
        public async Task<JsonResult> CreateTimeStamp(/*TODO stringlength*/string jsTimeString = null) // TODO add timestamp description at start/stop // TODO provide target time and count down/send notification
        {
            ValidateModelState();
            // TODO
            //var timeZoneFeature = HttpContext.;
            //bool isValidTimeZone = availableTimeZones.Contains(localTimeZone);

            // TODO catch CancelOperation exceptions from dependencies (IdentityStore, TrackerService)
            TrackerClientViewModel clientVM = null;
            try
            {
                var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateTimeStamp);
                var clientSession = HttpContext.Features.Get<TrackerClientSession>(); // TODO
                /*var throttlerResult = await _trackerThrottler.ThrottleRequestAsync(HttpContext, trackerContext); TODO
                if (!throttlerResult.ContinueExecution)
                {
                    return BadRequest("Too many requests"); // TODO
                }*/
                var targetTimeZone = jsTimeString != null ? GetRequestTimeZone(null, jsTimeString) : TimeZoneInfo.Utc; // TODO this should not default to utc, make separate action
                var createdTimeStamp = await _trackerService.CreateTimeStampAsync(trackerContext, targetTimeZone);
                if (targetTimeZone != TimeZoneInfo.Utc)
                {
                    // TODO check view time zone is set at all
                    if (clientSession.ViewTimeZone.IsAmbiguousTime(createdTimeStamp.TrackedTimeUTC))
                    {
                        throw new NotImplementedException();
                        // TODO
                    }
                }
                //await throttlerResult.FinishRequest();
                clientVM = new TrackerClientViewModel()
                {
                    TimeStamp = createdTimeStamp.SetTrackedTimeForView(clientSession)
                };
                return CreateJsonResultNoLoopsPascalCase(clientVM, trackerContext);
            }
            catch (OperationCanceledException)
            {
                return CreateJsonErrorForUser("cancelled"); // TODO catch OperationCanceledException at critical operations? or is EF core enough
            }
        }

        // TODO validationTokens
        [HttpPost]
        public async Task<JsonResult> CreateTimeNorm([Required] int? workUnitId, /*TODO stringlength*/string jsTimeString = null)
        {
            /* TODO
            repro: start in production mode, open tokyo with one unbound stamp, duplicate that stamp, manual edit stamp, select different target next to stamps, then rename created stamp, change table, then -->rapidly create 2 notes<--

            fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
                  Failed executing DbCommand (21ms) [Parameters=[@p0='?', @p1='?', @p2='?', @p3='?', @p4='?' (Size = 255), @p5='?', @p6='?', @p7='?', @p8='?', @p9='?', @p10='?', @p11='?', @p12='?' (Size = 255), @p13='?', @p14='?', @p15='?', @p17='?', @p16='?'], CommandType='Text', CommandTimeout='30']
                  SET NOCOUNT ON;
                  DECLARE @inserted0 TABLE ([TimeNormId] int, [_Position] [int]);
                  MERGE [TimeNorm] USING (
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, 0),
                  (@p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, 1)) AS i ([ColorB], [ColorG], [ColorR], [EndTimeId], [Name], [ProductivityRatingId], [StartTimeId], [WorkUnitId], _Position) ON 1=0
                  WHEN NOT MATCHED THEN
                  INSERT ([ColorB], [ColorG], [ColorR], [EndTimeId], [Name], [ProductivityRatingId], [StartTimeId], [WorkUnitId])
                  VALUES (i.[ColorB], i.[ColorG], i.[ColorR], i.[EndTimeId], i.[Name], i.[ProductivityRatingId], i.[StartTimeId], i.[WorkUnitId])
                  OUTPUT INSERTED.[TimeNormId], i._Position
                  INTO @inserted0;

                  SELECT [t].[TimeNormId], [t].[ManualSortOrderKey] FROM [TimeNorm] t
                  INNER JOIN @inserted0 i ON ([t].[TimeNormId] = [i].[TimeNormId])
                  ORDER BY [i].[_Position];

                  UPDATE [TrackerUserDefaults] SET [TargetWorkUnitId] = @p16
                  WHERE [TrackerUserDefaultsId] = @p17;
                  SELECT @@ROWCOUNT;
            System.Data.SqlClient.SqlException (0x80131904): Cannot insert duplicate key row in object 'dbo.TimeNorm' with unique index 'IX_TimeNorm_EndTimeId'. The duplicate key value is (3937).
            The statement has been terminated.
               at System.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__108_0(Task`1 result)
               at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
               at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
               at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommand.<ExecuteAsync>d__17.MoveNext()
            ClientConnectionId:eae541bc-4c34-4e0a-9fe5-58f40859add3
            Error Number:2601,State:1,Class:14
            fail: Microsoft.EntityFrameworkCore.Update[10000]
                  An exception occurred in the database while saving changes for context type 'Tokyo.Service.Data.TokyoDbContext'.
                  Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. See the inner exception for details. ---> System.Data.SqlClient.SqlException: Cannot insert duplicate key row in object 'dbo.TimeNorm' with unique index 'IX_TimeNorm_EndTimeId'. The duplicate key value is (3937).
                  The statement has been terminated.
                     at System.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__108_0(Task`1 result)
                     at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
                     at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
                     at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
                     at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommand.<ExecuteAsync>d__17.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
                     at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__32.MoveNext()
                     --- End of inner exception stack trace ---
                     at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__32.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__10.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerExecutionStrategy.<ExecuteAsync>d__7`2.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
                     at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__61.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
                     at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__59.MoveNext()
                  --- End of stack trace from previous location where exception was thrown ---
                     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
                     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                     at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
                     at Microsoft.EntityFrameworkCore.DbContext.<SaveChangesAsync>d__48.MoveNext()
            Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries. See the inner exception for details. ---> System.Data.SqlClient.SqlException: Cannot insert duplicate key row in object 'dbo.TimeNorm' with unique index 'IX_TimeNorm_EndTimeId'. The duplicate key value is (3937).
            The statement has been terminated.
               at System.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__108_0(Task`1 result)
               at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
               at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
               at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommand.<ExecuteAsync>d__17.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__32.MoveNext()
               --- End of inner exception stack trace ---
               at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.<ExecuteAsync>d__32.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.<ExecuteAsync>d__10.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerExecutionStrategy.<ExecuteAsync>d__7`2.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__61.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.<SaveChangesAsync>d__59.MoveNext()
            --- End of stack trace from previous location where exception was thrown ---
               at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
               at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
               at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
               at Microsoft.EntityFrameworkCore.DbContext.<SaveChangesAsync>d__48.MoveNext()
             */
            ValidateModelState();
            await _trackerService.AuthorizeWorkUnitAsync(User, workUnitId.Value, TrackerAuthorization.EditRequirement);
            try
            {
                var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateTimeNorm);
                var clientSession = HttpContext.Features.Get<TrackerClientSession>(); // TODO

                var timeStamps = await _trackerService.GetTimeStampsAsync(trackerContext);
                var unboundTimeStamps = timeStamps.Where(t => !t.IsBound); //.Select(t => t.SetTrackedTimeForView(clientSession)).ToList();
                if (unboundTimeStamps.Count() > 0) // TODO locking with create time stamp and process
                {
                    return CreateJsonErrorForUser("Can't use while there are unbound timestamps.");
                }

                var targetTimeZone = GetRequestTimeZone(null, jsTimeString);
                var createdTimeStamp = await _trackerService.CreateTimeStampAsync(trackerContext, targetTimeZone);
                if (clientSession.ViewTimeZone.IsAmbiguousTime(createdTimeStamp.TrackedTimeUTC))
                {
                    throw new NotImplementedException();
                    // TODO
                }
                var createdTimeStamp2 = await _trackerService.CreateTimeStampAsync(trackerContext, targetTimeZone);
                if (clientSession.ViewTimeZone.IsAmbiguousTime(createdTimeStamp2.TrackedTimeUTC))
                {
                    throw new NotImplementedException();
                    // TODO
                }

                await _trackerService.ProcessTimeStampsAsync(trackerContext, workUnitId.Value);

                // --- TODO code duplication
                var ownedTimeTables = await _trackerService.GetAllTimeTables(trackerContext);
                if (ownedTimeTables.Count == 0)
                {
                    throw new ApplicationException("No user tables.");
                }
                await _trackerService.AuthorizeTimeTableAsync(User, ownedTimeTables.First().TimeTableId, TrackerAuthorization.ReadRequirement);
                var clientOwnerName = await GetTableOwnerNameAsync(ownedTimeTables.First().TimeTableId);
                var allTimeTableVMs = ownedTimeTables.Select(t =>
                {
                    return new TimeTableViewModel(clientSession, t, clientOwnerName);
                }).ToList();
                // TODO title = $"Table {sharedTimeTableInfo.TimeTableId}, Shared by {timeTableOwnerName} ({ sharedTimeTableInfo.ShareEnabledTime })";
                var sharedTimeTables = await _trackerService.GetSharedByOtherTableInfosAsync(trackerContext);
                var sharedTimeTableVMs = sharedTimeTables.Select(async s =>
                {
                    await _trackerService.AuthorizeTimeTableAsync(User, s.TimeTable.TimeTableId, TrackerAuthorization.ReadRequirement);
                    var sharedTableOwnerName = await GetTableOwnerNameAsync(s.TimeTableId);
                    return new TimeTableViewModel(clientSession, s.TimeTable, sharedTableOwnerName);
                }).Select(t => t.Result);
                allTimeTableVMs.Concat(sharedTimeTableVMs);
                /// --- TODO code duplication end
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeTables = allTimeTableVMs, UnboundTimeStamps = new List<TimeStamp>(), TargetWorkUnitId = workUnitId.Value }, trackerContext); // unbound timestamps must be empty
            }
            catch (OperationCanceledException)
            {
                return CreateJsonErrorForUser("cancelled"); // TODO catch OperationCanceledException at critical operations? or is EF core enough
            }
        }

        [HttpPost]
        public async Task<JsonResult> DuplicateTimeStamp([Required] int? timeStampId)
        {
            ValidateModelState();
            await _trackerService.AuthorizeTimeStampAsync(User, timeStampId.Value, TrackerAuthorization.ReadRequirement);
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.DuplicateTimeStamp);
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            var duplicatedTimeStamp = await _trackerService.DuplicateTimeStampAsync(trackerContext, timeStampId.Value);
            if (duplicatedTimeStamp == null)
            {
                return CreateJsonErrorForUser("Could not duplicate timestamp.");
            }
            else
            {
                return CreateJsonResultNoLoopsPascalCase(new TrackerClientViewModel() { TimeStamp = duplicatedTimeStamp.SetTrackedTimeForView(clientSession) }, trackerContext);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CreateTimeStampManuallyPost([FromBody] CreateTimeStampPostViewModel createTimeStampViewModel)
        {
            if (createTimeStampViewModel == null)
            {
                throw new ArgumentNullException(nameof(createTimeStampViewModel));
            }
            ValidateModelState(); // TODO does this check view model for null?
            var clientSession = HttpContext.Features.Get<TrackerClientSession>();
            if (createTimeStampViewModel.Year == null || createTimeStampViewModel.Month == null || createTimeStampViewModel.Day == null ||
                createTimeStampViewModel.Hour == null || createTimeStampViewModel.Minute == null || createTimeStampViewModel.Second == null ||
                createTimeStampViewModel.Millisecond == null)
            {
                throw new ArgumentNullException("Time data");
            }

            var manualDateTime = new DateTime(
                createTimeStampViewModel.Year.Value,
                createTimeStampViewModel.Month.Value,
                createTimeStampViewModel.Day.Value,
                createTimeStampViewModel.Hour.Value,
                createTimeStampViewModel.Minute.Value,
                createTimeStampViewModel.Second.Value,
                createTimeStampViewModel.Millisecond.Value,
                DateTimeKind.Unspecified); // TODO fix fix fix

            //var targetTimeZone = GetRequestTimeZone(manualDateTime, createTimeStampViewModel.JsTimeString);
            if (clientSession.ViewTimeZone.IsAmbiguousTime(manualDateTime))
            { // TODO duplicated code where time is added/updated
                var ambiguousTimeOffsets = clientSession.ViewTimeZone.GetAmbiguousTimeOffsets(manualDateTime); // TODO audit
                return CreateJsonErrorForUser("Ambiguous time."); // TODO set/check absoluteOffset after client decision in timeStamp
            }
            else if (clientSession.ViewTimeZone.IsInvalidTime(manualDateTime))
            { // TODO invalidTime is not triggered? need try catch
                return CreateJsonErrorForUser("Invalid time dotnet.");
            }
            DateTimeOffset? dateTimeOffset = null;
            try
            {
                dateTimeOffset = new DateTimeOffset(manualDateTime, clientSession.ViewTimeZone.GetUtcOffset(manualDateTime));
            }
            catch (ArgumentException e)
            {
                if (e.Message.StartsWith("The UTC Offset of the local dateTime parameter does not match the offset argument."))
                {
                    return CreateJsonErrorForUser("Invalid time.");
                }
                else
                {
                    throw e;
                }
            }
            var manualTimeStamp = new TimeStamp()
            {
                Name = createTimeStampViewModel.TimeStampName,
                TrackedTimeUTC = dateTimeOffset.Value.UtcDateTime,
                UtcOffsetAtCreation = clientSession.ViewTimeZone.GetUtcOffset(dateTimeOffset.Value.DateTime), // TODO code duplication w/ http get parse
                TimeZoneIdAtCreation = clientSession.ViewTimeZone.Id
            };
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.CreateTimeStampManually);
            var createdTimeStamp = await _trackerService.AddTimeStampAsync(trackerContext, manualTimeStamp);

            var clientVM = new TrackerClientViewModel()
            {
                TimeStamp = createdTimeStamp.SetTrackedTimeForView(clientSession)
            };

            return CreateJsonResultNoLoopsPascalCase(clientVM, trackerContext);
        }

        private IActionResult RedirectToAccessDenied() => RedirectToAction(nameof(AccountController.AccessDenied), "Account");
        private JsonResult CreateJsonAccessDenied() // TODO use instead of redirect
        {
            var errorVM = new TrackerClientViewModel()
            {
                StatusText = "Access denied."
            };
            return Json(errorVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        private JsonResult CreateJsonErrorForUser(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }
            var errorVM = new TrackerClientViewModel()
            {
                StatusText = errorMessage
            };
            return Json(errorVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
        }

        private JsonResult CreateJsonResultNoLoopsPascalCase(TrackerClientViewModel clientVM, TrackerContext trackerContext)
        {
            clientVM.StatusText = "";
            clientVM.TrackerEvent = (int)trackerContext.TrackerEvent;
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            JsonResult serializedJson = null;
            try
            {
                // TODO use protobuf instead of json
                var requestLocalizationFeature = HttpContext.Features.Get<IRequestCultureFeature>();
                Thread.CurrentThread.CurrentCulture = requestLocalizationFeature.RequestCulture.Culture;
                // child properties which are set to null are not ignored
                serializedJson = Json(clientVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
            return serializedJson;
        }


        //[ActionName("s")] TODO
        [HttpPost]
        public JsonResult SetSortOrder(int? sortOrderType, bool? isAscending)
        {
            // TODO
            var currentSortOrder = HttpContext.Features.Get<TrackerClientSession>().LastSortOrder;
            throw new NotImplementedException();
        }

        [HttpPost]
        public JsonResult ShareTimeTablesWithToken(int[] timeTableIds)
        {
            // TODO
            throw new NotImplementedException();
        }

        [HttpPost]
        public JsonResult ShareTimeTables(int[] timeTableIds, string shareWithEmail)
        {
            throw new NotImplementedException();
            /*if (!ModelState.IsValid)
            {
                throw new NotImplementedException(); // TODO
            }
            if (timeTableIds.Count() == 0)
            {
                throw new ArgumentException("Provide at least one timeTableId.");
            }
            if (string.IsNullOrEmpty(shareWithEmail))
            {
                throw new ArgumentNullException(shareWithEmail);
            }
            // check if email owner has already registered
            var otherUser = await _userManager.FindByEmailAsync(shareWithEmail);
            if (otherUser == null)
            {
                throw new NotImplementedException();
                // send invite with token to get access
                // TODO
            }
            else
            {
                // share tables
                var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.ShareTimeTables);
                foreach (var timeTableId in timeTableIds)
                {
                    await _trackerService.AuthorizeTimeTableAsync(User, timeTableId, TrackerAuthorization.ShareRequirement);
                }
                var otherUserId = await _userManager.GetUserIdAsync(otherUser);
                await _trackerService.ShareTablesAsync(trackerContext, timeTableIds, otherUserId);
            }
            return RedirectToAction(nameof(Index));*/
        }

        [HttpPost]
        public JsonResult UnShareTimeTables(int[] timeTableIds)
        {
            throw new NotImplementedException();
            /*if (!ModelState.IsValid)
            {
                throw new NotImplementedException(); // TODO
            }
            if (timeTableIds.Count() == 0)
            {
                throw new ArgumentException("Provide at least one timeTableId.");
            }
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.UnshareTimeTables);
            foreach (var timeTableId in timeTableIds)
            {
                await _trackerService.AuthorizeTimeTableAsync(User, timeTableId, TrackerAuthorization.ShareRequirement);
            }
            await _trackerService.UnShareTablesAsync(trackerContext, timeTableIds);
            return RedirectToAction(nameof(Index));*/
        }

        private async Task<string> GetTableOwnerNameAsync(int? timeTableId)
        {
            if (timeTableId == null)
            {
                throw new ArgumentNullException(nameof(timeTableId));
            }
            var tableOwnerArray = await GetTableOwnerNamesAsync(new int[] { timeTableId.Value });
            return tableOwnerArray[0];
        }

        private async Task<string[]> GetTableOwnerNamesAsync(IEnumerable<int> timeTableIds)
        {
            var trackerContext = await CreateUserTrackerContextAsync(TrackerEvent.QueryTableOwners);
            var owningUserIds = await _trackerService.GetTableOwnerNamesAsync(trackerContext, timeTableIds);
            var ownerUserNames = new List<string>();
            foreach (var userId in owningUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userName = await _userManager.GetUserNameAsync(user);
                ownerUserNames.Add(userName);
            }
            return ownerUserNames.ToArray();
        }

        private async Task<TrackerContext> CreateUserTrackerContextAsync(TrackerEvent trackerEvent)
        {
            var trackerContext = new TrackerContext(trackerEvent, await GetUserIdStringAsync());
            await _trackerService.LogTrackerContextAsync(trackerContext);
            return trackerContext;
        }

        private async Task<string> GetUserIdStringAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            return await _userManager.GetUserIdAsync(user);
        }
        private string StatusMessageFromModelState() => string.Join(",", ModelState.Where(kvp => kvp.Value.Errors.Count > 0).Select(kvp => kvp.Value.Errors.First().ErrorMessage)); // TODO unused
    }
}
