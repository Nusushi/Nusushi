using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokyo.Service.Extensions;
using Tokyo.Service.Models;
using Tokyo.Service.Options;

namespace Tokyo.Service.Middlewares
{
    public class TrackerCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TrackerServiceOptions _serviceOptions;

        public TrackerCookieMiddleware(RequestDelegate next, IOptions<TrackerServiceOptions> serviceOptions)
        {
            _next = next;
            _serviceOptions = serviceOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            TrackerClientSession clientSession = new TrackerClientSession();
            if (context.Request.Cookies.TryGetValue(_serviceOptions.CookieName, out var cookie))
            {
                try
                {
                    JsonConvert.PopulateObject(cookie, clientSession);
                    // restore managed code objects
                    clientSession.DeviceTimeZone = clientSession.DeviceTimeZoneCookieValue != null ? TimeZoneInfo.FindSystemTimeZoneById(clientSession.DeviceTimeZoneCookieValue) : null;
                    clientSession.ViewTimeZone = clientSession.ViewTimeZoneCookieValue != null ? TimeZoneInfo.FindSystemTimeZoneById(clientSession.ViewTimeZoneCookieValue) : null;
                    if (clientSession.Revision < TrackerClientSession.TargetRevision)
                    {
                        clientSession.UpdateTrackerOptionsCookie(context.Response, _serviceOptions.CookieName);
                    }
                }
                catch (JsonSerializationException)
                {
                    // bad cookie format
                    context.Response.Cookies.Delete(_serviceOptions.CookieName);
                    clientSession = new TrackerClientSession();
                }
            }
            context.Features.Set<TrackerClientSession>(clientSession);

            await this._next.Invoke(context);
        }
    }
}
