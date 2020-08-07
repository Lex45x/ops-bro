using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events;
using Prometheus;

namespace OpsBro.Domain.Extraction
{
    /// <summary>
    /// Represent an entry-point for extraction process
    /// </summary>
    public class Listener
    {
        /// <summary>
        /// The name of a listener
        /// </summary>
        public string Name { get; }

        private static readonly Counter listenerCallsCounter = Metrics.CreateCounter("listener_calls", "Represent amount of calls, listener received", new CounterConfiguration
        {
            LabelNames = new[] { "listener_name" }
        });

        private static readonly Counter extractedEventsCounter = Metrics.CreateCounter("listener_events_extracted", "Represent amount of extracted events", new CounterConfiguration
        {
            LabelNames = new[] { "listener_name", "event_name" }
        });

        public Listener(string name, ICollection<Extractor> extractors)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
        }

        /// <summary>
        /// A list of associated Extractors
        /// </summary>
        public ICollection<Extractor> Extractors { get; }

        /// <summary>
        /// Extract all possible event from the payload
        /// </summary>
        /// <param name="payload">Payload to extract from</param>
        /// <returns>All extracted events</returns>
        public IEnumerable<Event> ExtractAll(JObject payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            listenerCallsCounter.WithLabels(Name).Inc();

            foreach (var extractor in Extractors)
            {
                if (extractor.TryExtract(payload, out var extractedEvent))
                {
                    extractedEventsCounter.WithLabels(Name, extractedEvent.Name).Inc();

                    yield return extractedEvent;
                }
            }
        }
    }
}