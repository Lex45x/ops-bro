using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpsBro.Domain.Settings;
using Prometheus;

namespace OpsBro.Api.Controllers
{
    [Route("api/{listenerName}")]
    public class ListenerController : ControllerBase
    {
        private static readonly Counter eventsWithoutDispatcherCounter = Metrics.CreateCounter("events_without_dispatcher", "Represent amount of events that don't have a dispatcher", new CounterConfiguration
        {
            LabelNames = new[] { "listener_name", "event_name" }
        });

        private static readonly Counter callsWithoutListenerCounter = Metrics.CreateCounter("calls_without_listener", "Represent amount of requests where listener with selected name was not found");

        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly ISettings settings;

        public ListenerController(ISettings settings)
        {
            this.settings = settings;
        }

        [HttpPost]
        public async Task Call([FromRoute] string listenerName, [FromBody] JObject body)
        {
            logger.Debug("Received call to listener {listener}.", listenerName);

            var listenerFound = settings.ListenersByListenerName.TryGetValue(listenerName, out var listener);

            if (!listenerFound)
            {
                callsWithoutListenerCounter.Inc();
                logger.Warn("Listener {listener} not found.", listenerName);
                return;
            }

            var payload = new JObject
            {
                ["query"] = JToken.FromObject(Request.Query.ToDictionary(entry => entry.Key, entry => entry.Value)),
                ["body"] = body,
                ["headers"] = JToken.FromObject(Request.Headers),
                ["config"] = settings.Config
            };

            foreach (var @event in listener.ExtractAll(payload))
            {
                logger.Debug("Event {eventName} was extracted.", @event.Name);

                var eventDispatcher = settings.EventDispatcherByEventName[@event.Name];

                if (eventDispatcher == null)
                {
                    eventsWithoutDispatcherCounter.WithLabels(listenerName, @event.Name).Inc();
                    logger.Warn("Dispatcher for event {event} was not found!", @event.Name);
                    continue;
                }

                await eventDispatcher.Dispatch(@event, settings.Config);
            }
        }
    }
}