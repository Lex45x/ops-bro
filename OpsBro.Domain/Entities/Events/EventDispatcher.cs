using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using OpsBro.Domain.ValueObjects;

namespace OpsBro.Domain.Entities.Events
{
    /// <summary>
    /// Handle and validate an extracted event.
    /// </summary>
    public class EventDispatcher
    {
        public EventDispatcher(string eventName, JSchema schema, ICollection<EventSubscriber> subscribers)
        {
            EventName = eventName;
            Schema = schema;
            Subscribers = subscribers;
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
        /// </summary>
        /// <param name="extractedEvent"></param>
        /// <returns></returns>
        public async Task Dispatch(Event extractedEvent)
        {
            if (!string.Equals(extractedEvent.Name, EventName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Invalid event name. {EventName} expected, got {extractedEvent.Name}");
            }

            extractedEvent.Data.Validate(Schema);

            foreach (var subscriber in Subscribers)
            {
                try
                {
                    await subscriber.Handle(extractedEvent);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}