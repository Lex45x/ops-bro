using OpsBro.Abstractions.Bind;
using OpsBro.Abstractions.Contracts.Events;
using OpsBro.Abstractions.Settings;

namespace OpsBro.Api.Controllers
{
    [ControllerName("Listener")]
    [Route("api/{listener}")]
    public class ListenerController : Controller, ITrigger<GenericEvent>
    {
        private readonly IBindingService bindingService;
        private readonly IEventBus eventBus;
        private readonly IQueryBus queryBus;
        private readonly ISettingsSource settings;
        private readonly IGenericLogger<ListenerController> logger;

        public ListenerController(IBindingService bindingService, IEventBus eventBus, IQueryBus queryBus,
            ISettingsSource settings, IGenericLogger<ListenerController> logger)
        {
            this.bindingService = bindingService;
            this.eventBus = eventBus;
            this.queryBus = queryBus;
            this.settings = settings;
            this.logger = logger;
        }

        [HttpPost]
        public async Task Call([FromRoute] string listener, [FromBody] JObject body)
        {
            listener = Uri.UnescapeDataString(listener);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Step"] = "Beginning",
                    ["NextStep"] = "Find customer",
                    ["Listener"] = listener,
                    ["RequestBody"] = body,
                    ["RequestId"] = Request.HttpContext.TraceIdentifier
                });
            }
            
            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Step"] = "Find customer",
                    ["NextStep"] = "Find listener",
                    ["RequestId"] = Request.HttpContext.TraceIdentifier
                });
            }

            var listeners = settings.Listeners.First(customerListener => customerListener.Name == listener);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Step"] = "Find listener",
                    ["NextStep"] = "Create payload",
                    ["RequestId"] = Request.HttpContext.TraceIdentifier
                });
            }

            var payload = new JObject
            {
                ["body"] = body,
                ["headers"] = JToken.FromObject(Request.Headers)
            };

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Step"] = "Create payload",
                    ["NextStep"] = "Execute binding",
                    ["RequestId"] = Request.HttpContext.TraceIdentifier,
                    ["Payload"] = payload
                });
            }

            var events = bindingService.Bind(listeners, payload);

            if (logger.IsDebugEnabled)
            {
                logger.Debug(new JObject
                {
                    ["Step"] = "Execute binding",
                    ["NextStep"] = "::LOOP for Events",
                    ["RequestId"] = Request.HttpContext.TraceIdentifier,
                    ["Events"] = JToken.FromObject(events)
                });
            }

            foreach (var @event in events)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Step"] = "Process event::BEGIN_ITERATION",
                        ["NextStep"] = "Find event definition",
                        ["RequestId"] = Request.HttpContext.TraceIdentifier,
                        ["Event"] = JToken.FromObject(@event)
                    });
                }

                var eventDefinition =
                    settings.EventDefinitions.FirstOrDefault(definition => definition.Name == @event.Name);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Step"] = "Find event definition",
                        ["NextStep"] = "Raise event",
                        ["RequestId"] = Request.HttpContext.TraceIdentifier,
                        ["EventDefinition"] = JToken.FromObject(eventDefinition)
                    });
                }

                var customerEvent = new GenericEvent
                {
                    Event = @event,
                    Definition = eventDefinition
                };

                await eventBus.Raise(this, customerEvent);

                if (logger.IsDebugEnabled)
                {
                    logger.Debug(new JObject
                    {
                        ["Step"] = "Raise event",
                        ["NextStep"] = "::END_ITERATION",
                        ["RequestId"] = Request.HttpContext.TraceIdentifier,
                        ["Event"] = JToken.FromObject(@events)
                    });
                }
            }
        }
    }
}