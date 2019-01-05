using California.Creator.Service.Extensions;
using California.Creator.Service.Models;
using California.Creator.Service.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace California.Creator.Service.Middlewares
{
    public class CaliforniaCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CaliforniaServiceOptions _serviceOptions;

        public CaliforniaCookieMiddleware(RequestDelegate next, IOptions<CaliforniaServiceOptions> serviceOptions)
        {
            _next = next;
            _serviceOptions = serviceOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            CaliforniaClientSession clientSession = new CaliforniaClientSession();
            if (context.Request.Cookies.TryGetValue(_serviceOptions.CookieName, out var cookie))
            {
                try
                {
                    JsonConvert.PopulateObject(cookie, clientSession);
                    if (clientSession.Revision < CaliforniaClientSession.TargetRevision)
                    {
                        clientSession.UpdateCaliforniaOptionsCookie(context.Response, _serviceOptions.CookieName);
                    }
                }
                catch (JsonSerializationException)
                {
                    // bad cookie format
                    context.Response.Cookies.Delete(_serviceOptions.CookieName);
                    clientSession = new CaliforniaClientSession();
                }
            }
            context.Features.Set<CaliforniaClientSession>(clientSession);

            await this._next.Invoke(context);
        }
    }
}
