using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;
using Prometheus;

namespace OpsBro.Domain.Events
{
    /// <summary>
    /// Handle and validate an extracted event.
    /// </summary>
    public class EventDispatcher
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private static readonly Counter eventsDispathcedCounter = Metrics.CreateCounter("events_dispatched", "Represent amount of dispatched events", new CounterConfiguration
        {
            LabelNames = new[] { "event_name" }
        });

        private static readonly Counter failedEventSubscriptionCounter = Metrics.CreateCounter("failed_event_subscription", "Represent amount of exceptions thrown by EventSubscriber", new CounterConfiguration
        {
            LabelNames = new[] { "event_name"}
        });

        private static readonly Counter eventsWithInvalidSchemaCounter = Metrics.CreateCounter("events_with_invalid_schema", "Represent amount of exceptions thrown by EventSubscriber", new CounterConfiguration
        {
            LabelNames = new[] { "event_name" }
        });

        public EventDispatcher(string eventName, JSchema schema, ICollection<EventSubscriber> subscribers)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Subscribers = subscribers ?? throw new ArgumentNullException(nameof(subscribers));
        }

        /// <summary>
        /// The name of an event
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Json schema to validate an event structure. <br/> See https://json-schema.org/understanding-json-schema/about.html
        /// </summary>
        public JSchema Schema { get; }

        /// <summary>
        /// A list of event subscribers
        /// </summary>
        public ICollection<EventSubscriber> Subscribers { get; }

        /// <summary>
        /// Handle an event to underlying subscribers.
        /// All subscribers exceptions will be suppressed.
        /// </summary>
        /// <param name="extractedEvent"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task Dispatch(Event extractedEvent, JObject config)
        {
            if (extractedEvent == null)
            {
                throw new ArgumentNullException(nameof(extractedEvent));
            }

            if (!string.Equals(extractedEvent.Name, EventName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Invalid event name. {EventName} expected, got {extractedEvent.Name}");
            }

            var eventValid = extractedEvent.Data.IsValid(Schema);

            if (!eventValid)
            {
                logger.Error("Event with name {name} and data {event} has invalid schema.", extractedEvent.Name, extractedEvent.Data);
                eventsWithInvalidSchemaCounter.WithLabels(extractedEvent.Name).Inc();
                return;
            }

            eventsDispathcedCounter.WithLabels(EventName).Inc();

            foreach (var subscriber in Subscribers)
            {
                try
                {
                    await subscriber.Handle(extractedEvent, config);
                }
                catch (Exception e)
                {
                    failedEventSubscriptionCounter.WithLabels(EventName).Inc();
                    logger.Error(e, "Event subscriber was unable to handle event!");
                }
            }
        }
    }
}