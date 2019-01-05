using AngleSharp.Css;
using California.Creator.Service.Extensions;
using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using California.Creator.Service.Options;
using California.Creator.Service.Services;
using Google.Apis.Services;
using Google.Apis.Webfonts.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tokyo.Service.Models;
using Tokyo.Service.Models.Core;
using Tokyo.Service.Services;

namespace Nusushi.Controllers
{
    // TODO transfer todos of tracker service to california service
    [Authorize]
    public class CaliforniaController : Controller
    {
        private readonly CaliforniaService _californiaService;
        private readonly UserManager<NusushiUser> _userManager;
        private readonly SignInManager<NusushiUser> _signInManager;
        private readonly IHostingEnvironment _env;
        private readonly TrackerService _trackerService; // TODO object reference + store routines just for link to different project?
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IConfiguration _configuration;
        private readonly CaliforniaServiceOptions _californiaServiceOptions;
        private readonly ILogger<CaliforniaController> _logger;

        // static content for client
        private static List<string> _allCssProperties;
        private static Dictionary<string, List<string>> _styleAtomCssPropertyMapping;
        private static int _staticContentInitialized = 0;
        private static string _controllerShortName = nameof(CaliforniaController).Substring(0, nameof(CaliforniaController).Length - "Controller".Length); // TODO should be a constant
        private static List<string> _availableFonts = new List<string>(); // TODO

        public CaliforniaController(CaliforniaService californiaService, UserManager<NusushiUser> userManager, 
            SignInManager<NusushiUser> signInManager, IOptions<CaliforniaServiceOptions> californiaServiceOptions, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment,
            TrackerService trackerService, ICompositeViewEngine viewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider, IConfiguration configuration)
        {
            _californiaService = californiaService;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = hostingEnvironment;
            _trackerService = trackerService;
            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
            _configuration = configuration;
            _californiaServiceOptions = californiaServiceOptions.Value;
            _logger = loggerFactory.CreateLogger<CaliforniaController>();
            if (Interlocked.CompareExchange(ref _staticContentInitialized, 1, 0) == 0)
            {
                _styleAtomCssPropertyMapping = new Dictionary<string, List<string>>(californiaServiceOptions.Value.StyleAtomDefaultProperties.Select(kvp => new KeyValuePair<string, List<string>>(kvp.Key.ToString(), kvp.Value)));
                var allProps = typeof(PropertyNames).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static); // TEST list for completeness and TEST detect api changes
                _allCssProperties = allProps.Select(p => p.GetValue(null).ToString()).ToList(); // TODO sort alphabetically
                _allCssProperties.Add("outline-offset"); // TODO manual
                _allCssProperties.Add("user-select"); // TODO manual
                _allCssProperties.Add("-webkit-user-select"); // TODO manual
            }
            _availableFonts = _californiaService.ReadAvailableFonts();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet, AllowAnonymous, ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
                    if (User.Claims.FirstOrDefault(c => c.Type == NusushiClaim.CaliforniaStoreClaimType) == null)
                    {
                        var storeClaim = new Claim(NusushiClaim.CaliforniaStoreClaimType, currentUserId);
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
                        var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.ReadStore);
                        if (await _californiaService.ReadCaliforniaStoreAsync(californiaContext, false, false) == null)
                        {
                            await _californiaService.CreateUserStoreAndInitializeAsync(californiaContext); // TODO remove for production, needed when dropping table often
                        }
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
                    var randomUserName = "californiauser" + rng.Next(100000000); // TODO check name is unique
                    var randomUser = new NusushiUser { UserName = randomUserName, Email = $"{randomUserName}@{_californiaServiceOptions.EmailDomainName}", RegistrationDateTime = DateTime.Now, RegistrationReference = "autogenerated", NusushiInvitationToken = new NusushiInvitationToken(), EmailConfirmed = true, PhoneNumber = "" };
                    var result = await _userManager.CreateAsync(randomUser, "ShouldBeRandomPass1*" /*TODO randomPass*/); // TODO duplicate users are added here when claim was not found, but user id existed
                    if (!result.Succeeded)
                    {
                        throw new ApplicationException("Random user could not be created.");
                    }
                    var userId = await _userManager.GetUserIdAsync(randomUser);
                    var storeClaim = new Claim(NusushiClaim.CaliforniaStoreClaimType, userId);
                    var storeClaimResult = await _userManager.AddClaimAsync(randomUser, storeClaim);
                    if (!storeClaimResult.Succeeded)
                    {
                        throw new ApplicationException("Could not add claim to user.");
                    }
                    var otherStoreClaim = new Claim(NusushiClaim.TrackerStoreClaimType, userId);
                    var otherStoreClaimResult = await _userManager.AddClaimAsync(randomUser, otherStoreClaim);
                    if (!otherStoreClaimResult.Succeeded)
                    {
                        throw new ApplicationException("Could not add other claim to user.");
                    }
                    await _signInManager.SignInAsync(randomUser, isPersistent: true);
                    // TODO code duplication
                    var californiaContext = new CaliforniaContext(CaliforniaEvent.CreateStore, userId);
                    var trackerContext = new TrackerContext(TrackerEvent.CreateProfile, userId);
                    await _trackerService.CreateUserStoreAndInitializeAsync(trackerContext);  // TODO can fail while db is updating => need rollback of user creation or check when accessing tokyo store
                    await _californiaService.CreateUserStoreAndInitializeAsync(californiaContext); // TODO can fail while db is updating => need rollback of user creation or check when accessing california store
                    await _californiaService.CreateDefaultProjectDataAsync(californiaContext); // TODO 2nd db save action
                    // TODO code duplication end
                    // TODO add default styles and $$$ as read only
                }
                return RedirectToRoute(CaliforniaRoutes.CaliforniaBrowserRoute, new { id = _userManager.GetUserId(HttpContext.User) }); // TODO alternative pushState() => value maybe in browser history // TODO project authorized/loaded multiple times just to change URL
            }
            return View();
        }
        
        [HttpPost]
        public async Task<JsonResult> DeleteLayoutStyleInteraction([Required] int? layoutStyleInteractionId)
        {
            ValidateModelState();
            var layoutStyleInteraction = await _californiaService.AuthorizeLayoutStyleInteractionAsync(User, layoutStyleInteractionId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteLayoutStyleInteraction);
            await _californiaService.DeleteLayoutStyleInteractionAsync(layoutStyleInteraction);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DeleteStyleValueInteraction([Required] int? layoutStyleInteractionId, [Required] int? styleValueId)
        {
            ValidateModelState();
            var layoutStyleInteraction = await _californiaService.AuthorizeLayoutStyleInteractionAsync(User, layoutStyleInteractionId.Value, CaliforniaAuthorization.EditRequirement);
            var styleValue = await _californiaService.AuthorizeStyleValueAsync(User, styleValueId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteStyleValueInteraction);
            await _californiaService.DeleteStyleValueFromLayoutStyleInteractionAsync(layoutStyleInteraction, styleValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateLayoutStyleInteractionForLayoutAtom([Required] int? layoutAtomId)
        {
            ValidateModelState();
            var layoutAtom = await _californiaService.AuthorizeLayoutAtomAsync(User, layoutAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateLayoutStyleInteraction);
            await _californiaService.CreateLayoutStyleInteractionForLayoutAtomAsync(layoutAtom);
            return await InitialClientData();
        }

        [HttpPost, PreventDeveloperCodePropagation]
        public async Task<IActionResult> UploadFiles(ICollection<Microsoft.AspNetCore.Http.IFormFile> formFiles)
        {
            try
            {
                foreach (var file in formFiles)
                {
                    if (file.Length > 0)
                    {
                        throw new NotImplementedException();
                        var filePath = Path.GetTempFileName(); // TODO can throw IOException if more than 65535 files
                        var fileExtension = System.IO.Path.GetExtension(file.FileName);
                        var fileNameCleaned = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                        // TODO System.IO.Path.GetInvalidFileNameChars
                        // TODO System.IO.Path.GetInvalidPathChars
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO
            }
            // TODO create thumbnail
            // TODO matching for responsive variants?
            return Ok();
        }

        public const string TargetCaliforniaViewToken = "TargetCaliforniaView";
        public const string OptimizedProjectToken = "OptimizedProject";
        public const string UserIdForManualRenderToken = "UserIdForManualRender";
        public const string InteractionLayoutIdsToken = "InteractionLayoutIds";
        public const string MathJaxContentInProjectToken = "MathJaxContentInProject";
        public const string CaliforniaSelectionOverlayToken = "CaliforniaSelectionOverlay";
        public const string InlineCssToken = "InlineCss";
        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> pub(string view = "Home", string id = null) // TODO view different name in function than request
        {
            var targetUserId = id;
            if (id == null)
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (currentUser != null)
                {
                    return RedirectToRoute(CaliforniaRoutes.CaliforniaPubRouteUnauthenticatedSpecific, new { id = _userManager.GetUserId(HttpContext.User), view = view });
                }
                else
                {
                    throw new ArgumentNullException(nameof(id));
                }
            }
            // TODO error in html: expected : in line.. <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            if (targetUserId == null || !cachedExportedHtml.ContainsKey(targetUserId)) // TODO document that pub does not require auth
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                string currentUserId = null;
                if (currentUser != null)
                {
                    currentUserId = await _userManager.GetUserIdAsync(currentUser);
                    if (targetUserId != null && currentUserId != targetUserId)
                    {
                        await _signInManager.SignOutAsync();
                        currentUser = null;
                    }
                }

                if (targetUserId != null && currentUserId != targetUserId)
                {
                    var existingRandomUser = await _userManager.GetUsersForClaimAsync(new Claim(NusushiClaim.CaliforniaStoreClaimType, targetUserId)); // TODO code duplication with index
                    if (existingRandomUser.Count == 0)
                    {
                        existingRandomUser = await _userManager.GetUsersForClaimAsync(new Claim(NusushiClaim.TrackerStoreClaimType, targetUserId));
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
                        return RedirectToRoute(CaliforniaRoutes.CaliforniaPubRouteUnauthenticatedSpecific, new { id = targetUserId, view = view });
                    }
                }

                var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.View);
                targetUserId = californiaContext.UserId;
                if (!cachedExportedHtml.ContainsKey(targetUserId))
                {
                    var clientSession = HttpContext.Features.Get<CaliforniaClientSession>();
                    if (clientSession.ProjectId == 0 || id != null)
                    {
                        var store = await _californiaService.ReadCaliforniaStoreAsync(californiaContext, true, false);
                        clientSession.ProjectId = store.CaliforniaProjects.First().CaliforniaProjectId; // TODO document
                        clientSession.UpdateCaliforniaOptionsCookie(this.Response, _californiaServiceOptions.CookieName);
                    }
                    await PublishAuthorizeAndCacheAsync(clientSession.ProjectId, view);
                }
            }
            return Content(cachedExportedHtml[targetUserId].Value, "text/html");
        }

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> StaticCss(string id = null)
        {
            ValidateModelState();
            /*var clientSession = HttpContext.Features.Get<CaliforniaClientSession>();
            await _californiaService.AuthorizeCaliforniaProjectAsync(User, clientSession.ProjectId, CaliforniaAuthorization.ReadRequirement); TODO document that pub does not require auth */
            if (id == null)
            {
                var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.ViewCss);
                id = californiaContext.UserId;
            }
            return File(cachedExportedCss[id], "text/css;charset=UTF-8");
        }

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> StaticJs(string id = null)
        {
            ValidateModelState();
            /*var clientSession = HttpContext.Features.Get<CaliforniaClientSession>();
            await _californiaService.AuthorizeCaliforniaProjectAsync(User, clientSession.ProjectId, CaliforniaAuthorization.ReadRequirement); TODO document that pub does not require auth */
            if (id == null)
            {
                var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.ViewJs);
                id = californiaContext.UserId;
            }
            return File(cachedExportedJs[id], "application/javascript;charset=UTF-8");
        }

        private static Dictionary<string, HtmlString> cachedExportedHtml = new Dictionary<string, HtmlString>();// TODO not thread safe!! // will take too much RAM
        private static Dictionary<string, byte[]> cachedExportedCss = new Dictionary<string, byte[]>();// TODO not thread safe!! // will take too much RAM
        private static Dictionary<string, byte[]> cachedExportedJs = new Dictionary<string, byte[]>();// TODO not thread safe!! // will take too much RAM

        [HttpPost]
        public async Task<IActionResult> Publish([Required] int? californiaProjectId, string view = "Home")
        {
            ValidateModelState();
            await PublishAuthorizeAndCacheAsync(californiaProjectId.Value, view);
            return Ok();
        }

        private async Task PublishAuthorizeAndCacheAsync(int californiaProjectId, string view = "Home")
        {
            var californiaProject = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.Publish);
            var exportedProject = await _californiaService.PublishCaliforniaProjectAsync(californiaContext, californiaProject);
            var currentSelection = new CaliforniaSelectionOverlay(exportedProject); // TODO filter style molecules and layouts by target view / user selection
            // render view manually TODO think about a more storage efficient way to cache data (server side cache if project is expected to be accessed by multiple clients)
            var taskHtml = Task.Run<HtmlString>(async () =>
            {
                var targetView = exportedProject.CaliforniaViews.FirstOrDefault(v => v.Name == view);
                if (targetView == null) // TODO rework not found (e.g. create)
                {
                    throw new InvalidOperationException();
                }
                var routeData = new RouteData();
                routeData.Values["controller"] = _controllerShortName;
                routeData.Values["action"] = nameof(CaliforniaController.Publish);
                var controllerContext = new DefaultHttpContext() { RequestServices = _serviceProvider };
                var actionContext = new ActionContext(controllerContext, routeData, new ControllerActionDescriptor());
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState);
                var tempData = new TempDataDictionary(controllerContext, _tempDataProvider);
                using (StringWriter sw = new StringWriter())
                {
                    ViewEngineResult viewResult = _viewEngine.FindView(actionContext, nameof(CaliforniaController.Publish), false);
                    // attention: viewData != ViewData
                    viewData[TargetCaliforniaViewToken] = view;
                    viewData[OptimizedProjectToken] = exportedProject;
                    viewData[UserIdForManualRenderToken] = californiaContext.UserId;
                    var interactionLayoutIds = exportedProject.LayoutStyleInteractions.SelectMany(i => new int[] { i.LayoutAtomId }.Concat(i.StyleValueInteractions.Select(iv => iv.StyleValue.StyleAtom.MappedToMolecule.StyleMolecule.StyleForLayoutId)));// TODO only add interactions that target or are sourced in targetView
                    viewData[InteractionLayoutIdsToken] = interactionLayoutIds;
                    viewData[CaliforniaSelectionOverlayToken] = currentSelection;
                    viewData[InlineCssToken] = exportedProject.CurrentSelection.GenerateOptimizedCss();
                    viewData[MathJaxContentInProjectToken] = (bool) exportedProject.ContentAtoms.Any(c =>
                    {
                        // TODO copy test from mathjax, for now just prevent false negatives
                        if (c.ContentAtomType == ContentAtomType.Text)
                        {
                            var firstIndex = c.TextContent.IndexOf("$");
                            if (firstIndex > -1)
                            {
                                return c.TextContent.LastIndexOf("$") != firstIndex;
                            }
                        }
                        return false;
                    });
                    ViewContext viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());
                    await viewResult.View.RenderAsync(viewContext);
                    return new HtmlString(sw.GetStringBuilder().ToString());
                }
            });
            var taskCss = Task.Run<Byte[]>(() =>
            {
                //return Encoding.UTF8.GetBytes(exportedProject.GeneratePedanticCss());
                //return Encoding.UTF8.GetBytes(exportedProject.CurrentSelection.GenerateOptimizedCss());
                // do nothing, style is inlined
                return Encoding.UTF8.GetBytes("");
            });
            var taskJs = Task.Run<Byte[]>(() =>
            {
                return Encoding.UTF8.GetBytes(exportedProject.GenerateJs());
            });
            try
            {
                await Task.WhenAll();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            // only save data after all tasks finished with no error
            cachedExportedHtml[californiaContext.UserId] = taskHtml.Result;
            cachedExportedCss[californiaContext.UserId] = taskCss.Result;
            cachedExportedJs[californiaContext.UserId] = taskJs.Result;
        }

        [HttpPost]
        public async Task<JsonResult> InitialClientData(string jsTimeString = null /*prevent caching*/)
        {
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.ReadInitialClientData);
            var clientSession = HttpContext.Features.Get<CaliforniaClientSession>();
            bool isCookieUpdateRequired = false;

            if (clientSession.ProjectId == 0)
            {
                var californiaStore = await _californiaService.ReadCaliforniaStoreAsync(californiaContext, true, false);
                clientSession.ProjectId = californiaStore.CaliforniaProjects.First().CaliforniaProjectId;
                isCookieUpdateRequired = true;
            }
            else
            {
                var californiaStore = await _californiaService.ReadCaliforniaStoreAsync(californiaContext, true, false);
                if (!californiaStore.CaliforniaProjects.Any(project => project.CaliforniaProjectId == clientSession.ProjectId))
                {
                    clientSession.ProjectId = californiaStore.CaliforniaProjects.First().CaliforniaProjectId;
                    isCookieUpdateRequired = true;
                }
            }
            
            if (isCookieUpdateRequired)
            {
                clientSession.UpdateCaliforniaOptionsCookie(Response, _californiaServiceOptions.CookieName);
            }

            await _californiaService.AuthorizeCaliforniaProjectAsync(User, clientSession.ProjectId, CaliforniaAuthorization.ReadRequirement); // TODO project is loaded twice
            var californiaProject = await _californiaService.ReadCaliforniaProjectForClientAsync(californiaContext, clientSession.ProjectId, false);

            var clientVM = new CaliforniaClientViewModel()
            {
                StatusText = "",
                CurrentRevision = CaliforniaClientSession.TargetRevision,
                CaliforniaProject = californiaProject,
                UrlToReadOnly = Url.Link(CaliforniaRoutes.CaliforniaBrowserRoute, new { id = californiaContext.UserId, action = nameof(Index) }), // TODO only in initial
                UrlToReadAndEdit = Url.Link(CaliforniaRoutes.CaliforniaBrowserRoute, new { id = californiaContext.UserId, action = nameof(Index), token = Guid.NewGuid().ToString("d") }), // TODO
            };
            /*if (_env.IsDevelopment())
            {
                // TODO optimize size by turning off unused parameters in client (JsonIgnore) // TODO short names for parameters save approx. half size
                var jsonString = JsonConvert.SerializeObject(clientVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
                _logger.LogCritical($"Estimated size of transmitted client view model: {(jsonString.Length * 2) / 1000} KB."); // TODO 2x serialization just to calculate size
            }*/
            return CreateJsonResultNoLoopsPascalCase(clientVM, californiaContext, true);
            /*TODO exception Connection id "0HLDC9HF89VKI", Request id "0HLDC9HF89VKI:00000003": the connection was closed becuase the response was not read by the client at the specified minimum data rate.
                info: Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv[14]
                      Connection id "0HLDC9HF89VKI" communication error.
                Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking.UvException: Error -4081 ECANCELED operation canceled */
        }

        [HttpPost]
        public async Task<JsonResult> SetSpecialLayoutBoxType([Required] int? layoutBoxId, [Required] int? specialLayoutBoxType)  // TEST ide-test function name change is propagated to client app (.js)
        {
            ValidateModelState();
            var layoutBox = await _californiaService.AuthorizeLayoutBoxAsync(User, layoutBoxId.Value, CaliforniaAuthorization.EditRequirement, false); // TODO load recursive and perform checks (no sub boxes, ...) or change content types / layout
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SetSpecialLayoutBoxType);
            if (specialLayoutBoxType.Value < 0 || specialLayoutBoxType.Value > SpecialLayoutBoxTypeCount)
            {
                throw new ArgumentOutOfRangeException("Invalid special layout box type.");
            }
            var specialLayoutBoxTypeEnum = (SpecialLayoutBoxType)specialLayoutBoxType.Value;
            await _californiaService.SetSpecialLayoutBoxTypeAsync(layoutBox, specialLayoutBoxTypeEnum);
            return await InitialClientData();
        }

        private static int SpecialLayoutBoxTypeCount = Enum.GetValues(typeof(SpecialLayoutBoxType)).Length;

        // TODO sidebar => special row type?

        [HttpPost]
        public async Task<JsonResult> DeleteStyleQuantum([Required] int? styleQuantumId)
        {
            ValidateModelState();
            await _californiaService.AuthorizeStyleQuantumAsync(User, styleQuantumId.Value, false, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteStyleQuantum);
            await _californiaService.DeleteStyleQuantumAsync(californiaContext, styleQuantumId.Value);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DeleteLayout([Required] int? layoutBaseId, [Required] bool? isOnlyBelow)
        {
            ValidateModelState();
            var targetLayout = await _californiaService.AuthorizeLayoutBaseAsync(User, layoutBaseId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteLayout);
            if (targetLayout is LayoutAtom)
            {
                if (isOnlyBelow.Value)
                {
                    throw new InvalidOperationException("Layout type must be layout box or layout row.");
                }
                await _californiaService.DeleteLayoutAtomAsync(californiaContext, layoutBaseId.Value, true);
            }
            else if (targetLayout is LayoutBox)
            {
                await _californiaService.DeleteLayoutBoxOrBelowRecursiveAsync(californiaContext, layoutBaseId.Value, true, isOnlyBelow.Value);
            }
            else if (targetLayout is LayoutRow)
            {
                await _californiaService.DeleteLayoutRowOrBelowRecursiveAsync(californiaContext, layoutBaseId.Value, isOnlyBelow.Value);
            }
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SetStyleMoleculeAsReferenceStyle([Required] int? styleMoleculeId)
        {
            throw new NotImplementedException(); // TODO must rather create new style
            ValidateModelState();
            var styleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, false);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SetStyleMoleculeAsReference);
            await _californiaService.SetStyleMoleculeAsReferenceStyleAsync(styleMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SetStyleMoleculeReference([Required] int? styleMoleculeId, [Required] int? referenceStyleMoleculeId)
        {
            // TODO layout html tag could also be affected (text => h1 => h2 ...)
            // TODO prevent changing clone reference style of listed/default layouts
            // TODO styles which belong to instantiated layout molecules (i.e. not listed in the instantiable section) should not be made reference => either create instantiable layout mol. or set new clone ref style when layout is deleted
            ValidateModelState();
            var styleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, true);
            var referenceStyleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, referenceStyleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, false);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SetStyleMoleculeReference);
            await _californiaService.SetStyleMoleculeReferenceAsync(styleMolecule, referenceStyleMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SyncStyleMoleculeToReferenceStyle([Required] int? styleMoleculeId)
        {
            throw new NotImplementedException();
            ValidateModelState();
            var styleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.ReadRequirement, false);
            var referenceStyleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, false);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SyncStyleMoleculeToReference);
            await _californiaService.SyncStyleMoleculeToReferenceStyleAsync(styleMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SyncStyleMoleculeFromReferenceStyle([Required] int? styleMoleculeId)
        {
            ValidateModelState();
            var styleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, false);
            var referenceStyleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.ReadRequirement, false);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SyncStyleMoleculeFromReference);
            await _californiaService.SyncStyleMoleculeFromReferenceStyleAsync(styleMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SyncLayoutStylesImitatingReferenceLayout([Required] int? targetLayoutMoleculeId, [Required] int? referenceLayoutMoleculeId)
        {
            ValidateModelState();
            LayoutBase targetLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, targetLayoutMoleculeId.Value, CaliforniaAuthorization.EditRequirement);
            LayoutBase referenceLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, referenceLayoutMoleculeId.Value, CaliforniaAuthorization.ReadRequirement);
            if (targetLayoutMolecule.LayoutType != referenceLayoutMolecule.LayoutType)
            {
                throw new InvalidOperationException("Layout types must be equal.");
            }
            if (targetLayoutMolecule.LayoutType == LayoutType.Row)
            {
                targetLayoutMolecule = await _californiaService.AuthorizeLayoutRowAsync(User, targetLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.EditRequirement, true);
                referenceLayoutMolecule = await _californiaService.AuthorizeLayoutRowAsync(User, referenceLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.ReadRequirement, true);
            }
            else if (targetLayoutMolecule.LayoutType == LayoutType.Box)
            {
                targetLayoutMolecule = await _californiaService.AuthorizeLayoutBoxAsync(User, targetLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.EditRequirement, true);
                referenceLayoutMolecule = await _californiaService.AuthorizeLayoutBoxAsync(User, referenceLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.ReadRequirement, true);
            }
            else // targetLayoutMolecule.LayoutType == LayoutType.Atom
            {
                targetLayoutMolecule = await _californiaService.AuthorizeLayoutAtomAsync(User, targetLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.EditRequirement);
                referenceLayoutMolecule = await _californiaService.AuthorizeLayoutAtomAsync(User, referenceLayoutMolecule.LayoutBaseId, CaliforniaAuthorization.EditRequirement);
            }
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SyncLayoutStylesImitatingReference);
            await _californiaService.SyncLayoutStylesImitatingReferenceLayoutAsync(targetLayoutMolecule, referenceLayoutMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SetLayoutBoxCountForRowOrBox([Required] int? layoutRowId, [Required] int? boxStyleMoleculeId, [Required] int? targetBoxCount, [Required] bool? isFitWidth)
        {
            ValidateModelState();
            if (targetBoxCount.Value < 1 || targetBoxCount.Value > 12) // TODO do we need upper limit?
            {
                throw new ArgumentOutOfRangeException($"Provided {nameof(targetBoxCount)} must be between 1 and 12.");
            }
            var targetLayout = await _californiaService.AuthorizeLayoutBaseAsync(User, layoutRowId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SetBoxCount);
            if (targetLayout is LayoutRow)
            {
                await _californiaService.SetLayoutBoxCountForRowAsync(californiaContext, targetLayout.LayoutBaseId, boxStyleMoleculeId.Value, targetBoxCount.Value, isFitWidth.Value);
            }
            else if (targetLayout is LayoutBox)
            {
                throw new NotImplementedException();
            }
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateStyleValueForAtom([Required] int? styleAtomId, [Required, MinLength(1)] string cssProperty)
        {
            ValidateModelState();
            if (!_allCssProperties.Contains(cssProperty))
            {
                throw new InvalidOperationException("Not a valid css property.");
            }
            var styleAtom = await _californiaService.AuthorizeStyleAtomAsync(User, styleAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateStyleValueForAtom);
            await _californiaService.CreateStyleValueForAtomAsync(styleAtom, cssProperty);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateCaliforniaView([Required] int? californiaProjectId, [Required, MinLength(1)] string californiaViewName)
        {
            ValidateModelState();
            var californiaProject = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateCaliforniaView);
            await _californiaService.CreateCaliforniaViewAsync(californiaProject, californiaViewName);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateCaliforniaViewFromReferenceView([Required] int? californiaProjectId, [Required, MinLength(1)] string californiaViewName, [Required] int? referenceCaliforniaViewId)
        {
            ValidateModelState();
            var californiaProject = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId.Value, CaliforniaAuthorization.EditRequirement);
            var referenceCaliforniaView = await _californiaService.AuthorizeCaliforniaViewAsync(User, referenceCaliforniaViewId.Value, CaliforniaAuthorization.ReadRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateCaliforniaViewFromReferenceView);
            await _californiaService.CreateCaliforniaViewFromReferenceViewAsync(californiaProject, californiaViewName, referenceCaliforniaView);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DeleteCaliforniaView([Required] int? californiaViewId)
        {
            ValidateModelState();
            var californiaView = await _californiaService.AuthorizeCaliforniaViewAsync(User, californiaViewId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteCaliforniaView);
            await _californiaService.DeleteCaliforniaViewAsync(californiaView);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateStyleValueInteraction([Required] int? layoutStyleInteractionId, [Required] int? styleValueId, [Required, MinLength(1)] string cssValue)
        {
            ValidateModelState();
            var layoutStyleInteraction = await _californiaService.AuthorizeLayoutStyleInteractionAsync(User, layoutStyleInteractionId.Value, CaliforniaAuthorization.EditRequirement);
            var styleValue = await _californiaService.AuthorizeStyleValueAsync(User, styleValueId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateStyleValueInteraction);
            await _californiaService.CreateStyleValueInteractionAsync(layoutStyleInteraction, styleValue, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateStyleAtomForMolecule([Required] int? styleMoleculeId, [Required] int? styleAtomType, [Required] int? responsiveDeviceId, string stateModifier)
        {
            ValidateModelState();
            StyleAtomType targetType = (StyleAtomType)styleAtomType;
            var styleMolecule = await _californiaService.AuthorizeStyleMoleculeAsync(User, styleMoleculeId.Value, CaliforniaAuthorization.EditRequirement, false);
            var responsiveDevice = await _californiaService.AuthorizeResponsiveDeviceAsync(User, responsiveDeviceId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateStyleAtomForMolecule);
            await _californiaService.CreateStyleAtomForMoleculeAsync(styleMolecule, targetType, responsiveDevice, stateModifier, true);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DeleteStyleAtom([Required] int? styleAtomId)
        {
            ValidateModelState();
            var styleAtom = await _californiaService.AuthorizeStyleAtomAsync(User, styleAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteStyleAtom);
            await _californiaService.DeleteStyleAtomAsync(californiaContext, styleAtom);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> ApplyStyleQuantumToAtom([Required] int? styleAtomId, [Required] int? styleQuantumId)
        {
            ValidateModelState();
            var styleAtom = await _californiaService.AuthorizeStyleAtomAsync(User, styleAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var styleQuantum = await _californiaService.AuthorizeStyleQuantumAsync(User, styleQuantumId.Value, true, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.ApplyStyleQuantumToAtom);
            await _californiaService.SetStyleQuantumToAtomAsync(styleAtom, styleQuantum);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateStyleQuantum([Required] int? californiaProjectId, [Required, MinLength(1)] string quantumName, [Required, MinLength(1)] string cssProperty, [Required, MinLength(1)] string cssValue)
        {
            ValidateModelState();
            if (!_allCssProperties.Contains(cssProperty))
            {
                throw new InvalidOperationException("Not a valid css property.");
            }
            var project = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateStyleQuantum);
            await _californiaService.CreateStyleQuantumAsync(californiaContext, californiaProjectId.Value, quantumName, cssProperty, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> UpdateTextContentAtom([Required] int? contentAtomId, string updatedTextContent)
        {
            ValidateModelState();
            if (updatedTextContent == null)
            {
                updatedTextContent = "";
            }
            var contentAtom = await _californiaService.AuthorizeContentAtomAsync(User, contentAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.UpdateContentAtom);
            await _californiaService.UpdateContentAtomAsync(californiaContext, contentAtom, updatedTextContent);
            return CreateJsonResultNoLoopsPascalCase(new CaliforniaClientViewModel() { PartialUpdate = new CaliforniaClientPartialData() { ContentAtom = contentAtom } }, californiaContext, true); // TODO current revision
        }

        [HttpPost]
        public async Task<JsonResult> UpdateStyleQuantum([Required] int? styleQuantumId, [Required, MinLength(1)] string cssValue)
        {
            ValidateModelState();
            var styleQuantum = await _californiaService.AuthorizeStyleQuantumAsync(User, styleQuantumId.Value, true, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.UpdateStyleQuantum);
            await _californiaService.UpdateStyleQuantumAsync(styleQuantum, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> UpdateStyleValue([Required] int? styleValueId, [Required, MinLength(1)] string cssValue)
        {
            ValidateModelState();
            var styleValue = await _californiaService.AuthorizeStyleValueAsync(User, styleValueId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.UpdateStyleValue);
            await _californiaService.UpdateStyleValueAsync(styleValue, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> UpdateUserDefinedCssForProject([Required] int? californiaProjectId, [Required] string cssValue)
        {
            ValidateModelState();
            var californiaProject = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.UpdateUserDefinedCssForProject);
            await _californiaService.UpdateUserDefinedCssAsync(californiaProject, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> UpdateUserDefinedCssForView([Required] int? californiaViewId, [Required] string cssValue)
        {
            ValidateModelState();
            var californiaView = await _californiaService.AuthorizeCaliforniaViewAsync(User, californiaViewId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.UpdateUserDefinedCssForView);
            await _californiaService.UpdateUserDefinedCssAsync(californiaView, cssValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DeleteStyleValue([Required] int? styleValueId)
        {
            ValidateModelState();
            var styleValue = await _californiaService.AuthorizeStyleValueAsync(User, styleValueId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DeleteStyleValue);
            await _californiaService.DeleteStyleValueAsync(styleValue);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> DuplicateStyleQuantum([Required] int? styleQuantumId)
        {
            // var parsedStyleQuantumIds = styleQuantumIdsString.Split(',').Select(partialString => int.Parse(partialString)); // TEST test all browser have same output rules // TODO pass multiple quantums as array/,-joined-string-of-ints and overwrite array to string function in client javascript
            ValidateModelState();
            var clientSession = HttpContext.Features.Get<CaliforniaClientSession>();
            if (clientSession.ProjectId == 0)
            {
                throw new ArgumentNullException("Project id not set.");
            }
            await _californiaService.AuthorizeStyleQuantumAsync(User, styleQuantumId.Value, false, CaliforniaAuthorization.ReadRequirement);
            await _californiaService.AuthorizeCaliforniaProjectAsync(User, clientSession.ProjectId, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.DuplicateStyleQuantum);
            var clonedQuantum = await _californiaService.CloneAndRegisterStyleQuantumAsync(californiaContext, clientSession.ProjectId, styleQuantumId.Value);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateLayoutAtomForBox([Required] int? targetLayoutBoxId, [Required] int? referenceLayoutAtomId)
        {
            ValidateModelState();
            var layoutAtom = await _californiaService.AuthorizeLayoutAtomAsync(User, referenceLayoutAtomId.Value, CaliforniaAuthorization.ReadRequirement);
            var targetLayoutBox = await _californiaService.AuthorizeLayoutBoxAsync(User, targetLayoutBoxId.Value, CaliforniaAuthorization.EditRequirement, isLoadRecursive: false);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateLayoutAtomForBox);
            await _californiaService.CreateLayoutAtomForBoxAsync(californiaContext, targetLayoutBox, layoutAtom);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateLayoutBoxForBoxOrRow([Required] int? targetLayoutBoxOrRowId, [Required] int? referenceLayoutBoxId)
        {
            ValidateModelState();
            var layoutBox = await _californiaService.AuthorizeLayoutBoxAsync(User, referenceLayoutBoxId.Value, CaliforniaAuthorization.ReadRequirement, isLoadRecursive: true);
            var targetLayoutBase = await _californiaService.AuthorizeLayoutBaseAsync(User, targetLayoutBoxOrRowId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateLayoutBoxForBoxOrRow);
            if (targetLayoutBase.LayoutType == LayoutType.Box)
            {
                var targetLayoutBox = await _californiaService.AuthorizeLayoutBoxAsync(User, targetLayoutBoxOrRowId.Value, CaliforniaAuthorization.EditRequirement, isLoadRecursive: true);
                var targetContainerRow = await _californiaService.AuthorizeLayoutRowAsync(User, targetLayoutBox.BoxOwnerRowId, CaliforniaAuthorization.EditRequirement, false); // TODO is necessary??
                await _californiaService.CreateLayoutBoxForBoxAsync(californiaContext, targetLayoutBox, layoutBox);
            }
            else if (targetLayoutBase.LayoutType == LayoutType.Row)
            {
                var targetLayoutRow = await _californiaService.AuthorizeLayoutRowAsync(User, targetLayoutBoxOrRowId.Value, CaliforniaAuthorization.EditRequirement, false);
                await _californiaService.CreateLayoutBoxForRowAsync(californiaContext, targetLayoutRow, layoutBox);
            }
            else
            {
                throw new InvalidOperationException("Target layout type must be layout box or row.");
            }
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateLayoutBoxForAtomInPlace([Required] int? targetLayoutAtomId, [Required] int? referenceLayoutBoxId)
        {
            ValidateModelState();
            var layoutBox = await _californiaService.AuthorizeLayoutBoxAsync(User, referenceLayoutBoxId.Value, CaliforniaAuthorization.ReadRequirement, isLoadRecursive: true);
            var layoutAtom = await _californiaService.AuthorizeLayoutAtomAsync(User, targetLayoutAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var atomOwnerBox = await _californiaService.AuthorizeLayoutBoxAsync(User, layoutAtom.PlacedAtomInBoxId, CaliforniaAuthorization.EditRequirement, isLoadRecursive: true); // TODO this stuff should be inside or outside transaction?
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateLayoutBoxForAtomInPlace);
            await _californiaService.CreateLayoutBoxForAtomInPlaceAsync(californiaContext, layoutAtom, layoutBox, atomOwnerBox);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> CreateLayoutRowForView([Required] int? targetCaliforniaViewId, [Required] int? referenceLayoutRowId)
        {
            ValidateModelState();
            var referenceLayoutRow = await _californiaService.AuthorizeLayoutRowAsync(User, referenceLayoutRowId.Value, CaliforniaAuthorization.ReadRequirement, true);
            var californiaView = await _californiaService.AuthorizeCaliforniaViewAsync(User, targetCaliforniaViewId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.CreateLayoutRowForView);
            await _californiaService.CreateLayoutRowForViewAsync(californiaContext, californiaView, referenceLayoutRow);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> SetLayoutRowOrBoxAsInstanceable([Required] int? californiaProjectId, [Required] int? layoutRowOrBoxId)
        {
            ValidateModelState();
            var targetProject = await _californiaService.AuthorizeCaliforniaProjectAsync(User, californiaProjectId.Value, CaliforniaAuthorization.EditRequirement);
            var targetLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, layoutRowOrBoxId.Value, CaliforniaAuthorization.ReadRequirement);
            if (targetLayoutMolecule.LayoutType == LayoutType.Box)
            {
                targetLayoutMolecule = await _californiaService.AuthorizeLayoutBoxAsync(User, layoutRowOrBoxId.Value, CaliforniaAuthorization.ReadRequirement, true);
            }
            else if (targetLayoutMolecule.LayoutType == LayoutType.Row)
            {
                targetLayoutMolecule = await _californiaService.AuthorizeLayoutRowAsync(User, layoutRowOrBoxId.Value, CaliforniaAuthorization.ReadRequirement, true);
            }
            else
            {
                throw new InvalidOperationException("Layout molecule must have row or box type.");
            }
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.SetLayoutMoleculeAsInstanceable);
            await _californiaService.SetLayoutMoleculeAsInstanceableAsync(targetProject, targetLayoutMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> MoveStyleAtomToResponsiveDevice([Required] int? styleAtomId, [Required] int? targetResponsiveDeviceId)
        {
            ValidateModelState();
            var styleAtom = await _californiaService.AuthorizeStyleAtomAsync(User, styleAtomId.Value, CaliforniaAuthorization.EditRequirement);
            var responsiveDevice = await _californiaService.AuthorizeResponsiveDeviceAsync(User, targetResponsiveDeviceId.Value, CaliforniaAuthorization.EditRequirement);
            if (styleAtom.MappedToMolecule.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId)
            {
                return CreateJsonErrorForUser("Style atom is already mapped to target reponsive device.");
            }
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.MoveStyleAtomToResponsiveDevice);
            await _californiaService.MoveStyleAtomToResponsiveDevice(styleAtom, responsiveDevice);
            return await InitialClientData();
        }

        [HttpGet]
        public async Task<IActionResult> RefreshExternalApis() // TODO require admin rights
        {
            var isGoogleFontsApiEnabled = _configuration.GetValue<bool?>("ThirdParty:IsEnableGoogleFontsApi", null);
            if (isGoogleFontsApiEnabled.HasValue && isGoogleFontsApiEnabled.Value == true)
            {
                var googleFontsApiKey = _configuration.GetValue<string>("ThirdParty:GoogleFontsApiKey");
                if (!string.IsNullOrEmpty(googleFontsApiKey))
                {
                    _logger.LogWarning("Connecting to Google Fonts Api.");
                    var service = new WebfontsService(new BaseClientService.Initializer
                    {
                        ApplicationName = CaliforniaServiceOptions.ServerApplicationName,
                        ApiKey = googleFontsApiKey,
                    });
                    var result = await service.Webfonts.List().ExecuteAsync();
                    if (result.Items != null)
                    {
                        await _californiaService.CreateOrUpdateFontsAsync(result.Items.Select(f => new Webfont() { Family = f.Family, Version = f.Version }));
                        _availableFonts = _californiaService.ReadAvailableFonts();
                    }
                    else
                    {
                        throw new ApplicationException("Google Fonts Api returned empty list.");
                    }
                }
                else
                {
                    _logger.LogCritical("Could not connect to Google Fonts Api. Google font api key missing.");
                }
            }
            else
            {
                _logger.LogError("Google Fonts Api needs to be setup in appsettings.");
            }
            return Ok();
        }

        [HttpPost]
        public async Task<JsonResult> MoveLayoutMoleculeIntoLayoutMolecule([Required] int? movedLayoutMoleculeId, [Required] int? targetContainerLayoutMoleculeId)
        {
            // TODO guard internal molecules from moving
            // TODO make sure sort order is such that it is inserted at end! // TODO or at start? different UI action
            ValidateModelState();
            var movedLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, movedLayoutMoleculeId.Value, CaliforniaAuthorization.EditRequirement);
            if (movedLayoutMolecule.LayoutType == LayoutType.Box)
            {
                movedLayoutMolecule = await _californiaService.AuthorizeLayoutBoxAsync(User, movedLayoutMoleculeId.Value, CaliforniaAuthorization.EditRequirement, true);
            }
            var targetContainerLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, targetContainerLayoutMoleculeId.Value, CaliforniaAuthorization.EditRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.MoveLayoutMoleculeIntoLayoutMolecule);
            await _californiaService.MoveLayoutMoleculeIntoLayoutMoleculeAsync(movedLayoutMolecule, targetContainerLayoutMolecule);
            return await InitialClientData();
        }

        [HttpPost]
        public async Task<JsonResult> MoveLayoutMoleculeNextToLayoutMolecule([Required] int? movedLayoutMoleculeId, [Required] int? targetNeighborLayoutMoleculeId, [Required] bool? isMoveBefore)
        {
            // TODO guard internal molecules from moving
            ValidateModelState();
            var movedLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, movedLayoutMoleculeId.Value, CaliforniaAuthorization.EditRequirement);
            var targetNeighborLayoutMolecule = await _californiaService.AuthorizeLayoutBaseAsync(User, targetNeighborLayoutMoleculeId.Value, CaliforniaAuthorization.ReadRequirement);
            var californiaContext = await CreateUserCaliforniaContextAsync(CaliforniaEvent.MoveLayoutMoleculeNextToLayoutMolecule);
            await _californiaService.MoveLayoutMoleculeNextToLayoutMoleculeAsync(movedLayoutMolecule, targetNeighborLayoutMolecule, isMoveBefore.Value);
            return await InitialClientData();
        }

        private JsonResult CreateJsonAccessDenied() // TODO use instead of redirect
        {
            var errorVM = new CaliforniaClientViewModel()
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
            var errorVM = new CaliforniaClientViewModel()
            {
                StatusText = errorMessage
            };
            return Json(errorVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
        }

        private JsonResult CreateJsonResultNoLoopsPascalCase(CaliforniaClientViewModel clientVM, CaliforniaContext californiaContext, bool isInitial = false)
        {
            clientVM.StatusText = "";
            clientVM.CaliforniaEvent = (int)californiaContext.CaliforniaEvent;
            if (isInitial)
            {
                clientVM.AllCssProperties = _allCssProperties;
                clientVM.StyleAtomCssPropertyMapping = _styleAtomCssPropertyMapping;
                clientVM.ThirdPartyFonts = _availableFonts;
            }
            // TODO child properties which are set to null are not ignored
            JsonResult serializedJson = Json(clientVM, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });
            return serializedJson;
        }

        private async Task<CaliforniaContext> CreateUserCaliforniaContextAsync(CaliforniaEvent californiaEvent)
        {
            var californiaContext = new CaliforniaContext(californiaEvent, await GetUserIdStringAsync());
            await _californiaService.LogCaliforniaContextAsync(californiaContext);
            return californiaContext;
        }

        private async Task<string> GetUserIdStringAsync()
        {
            var user = await _userManager.GetUserAsync(User); // TEST was different cookie after some period of time without changing app from californiauser => tokyouser // TODO store in database which app was used last by ip, client app device etc. and security notification if cookie-app changed in server but not in client and vice versa
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            return await _userManager.GetUserIdAsync(user);
        }

        private void ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                // TODO expected to throw exception when invalid data is passed but actually annoying / control flow by exceptions...?
                throw new InvalidOperationException();
            }
        }
    }
}
