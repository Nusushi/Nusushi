using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tokyo.Service.Models.Core;

namespace Tokyo.Service.Models
{
    public static class TrackerLogMessages // TODO rework
    {
        public enum TrackerBackendEvent
        {
            RegisterQueue = 0,
            RegisterThrottler = 1,
        }
        public static Dictionary<TrackerEvent, Action<ILogger, string, Exception>> SuccessMessages = new Dictionary<TrackerEvent, Action<ILogger, string, Exception>>(
            Enum.GetNames(typeof(TrackerEvent)).Select((name, index) => CreateSuccessMessageKvp((TrackerEvent)index, $"{name}[{index}] Success: {{0}}")));

        public static Dictionary<TrackerBackendEvent, Action<ILogger, string, Exception>> BackendMessages = new Dictionary<TrackerBackendEvent, Action<ILogger, string, Exception>>(
            new KeyValuePair<TrackerBackendEvent, Action<ILogger, string, Exception>>[]
            {
                CreateBackendMessageKvp(TrackerBackendEvent.RegisterQueue, "Registering tracker queue: {0}"),
                CreateBackendMessageKvp(TrackerBackendEvent.RegisterThrottler, "Registering tracker throttler: {0}"),
            });

        private static KeyValuePair<TrackerEvent, Action<ILogger, string, Exception>> CreateSuccessMessageKvp(TrackerEvent trackerEvent, string messageWithPlaceholder)
            => new KeyValuePair<TrackerEvent, Action<ILogger, string, Exception>>(trackerEvent, LoggerMessage.Define<string>(LogLevel.Information, (int)trackerEvent, messageWithPlaceholder));

        private static KeyValuePair<TrackerBackendEvent, Action<ILogger, string, Exception>> CreateBackendMessageKvp(TrackerBackendEvent trackerBackendEvent, string messageWithPlaceholder)
            => new KeyValuePair<TrackerBackendEvent, Action<ILogger, string, Exception>>(trackerBackendEvent, LoggerMessage.Define<string>(LogLevel.Warning, (int)trackerBackendEvent, messageWithPlaceholder));
    }
}
