using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction.UnnestingRules;
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

        public Listener(string name, ICollection<Extractor> extractors, ICollection<UnnestingRule> unnestingRules)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
            UnnestingRules = unnestingRules ?? throw new ArgumentNullException(nameof(unnestingRules));
        }

        /// <summary>
        /// A list of associated Extractors
        /// </summary>
        public ICollection<Extractor> Extractors { get; }

        /// <summary>
        /// A list of unnesting rules for the request.
        /// </summary>
        public ICollection<UnnestingRule> UnnestingRules { get; }

        /// <summary>
        /// Extract all possible events from the payload
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

            IEnumerable<JObject> payloads = Enumerable.Repeat(payload, 1);

            if (UnnestingRules.Any())
            {
                payloads = UnnestingRules.Aggregate(payloads, (payloads, rule) => payloads.SelectMany(payload => rule.Unnest(payload)));
            }

            foreach (var unnestedPayload in payloads)
            {
                foreach (var extractor in Extractors)
                {
                    if (extractor.TryExtract(unnestedPayload, out var extractedEvent))
                    {
                        extractedEventsCounter.WithLabels(Name, extractedEvent.Name).Inc();

                        yield return extractedEvent;
                    }
                }
            }
        }
    }
}