using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events;

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

            foreach (var extractor in Extractors)
            {
                if (extractor.TryExtract(payload, out var extractedEvent))
                {
                    yield return extractedEvent;
                }
            }
        }
    }
}