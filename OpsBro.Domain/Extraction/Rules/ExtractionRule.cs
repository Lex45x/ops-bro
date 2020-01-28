using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.Rules
{
    /// <summary>
    ///  Base class for all extraction rules
    /// </summary>
    public abstract class ExtractionRule
    {
        protected ExtractionRule(ExtractionType type, string path, string property)
        {
            if (!Enum.IsDefined(typeof(ExtractionType), type))
            {
                throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(ExtractionType));
            }

            Type = type;
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        /// <summary>
        /// Type of extraction.
        /// </summary>
        public ExtractionType Type { get; }

        /// <summary>
        /// Json path to property in the payload
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The name of Event property
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Extract value from the property <paramref name="payload"/> and set result to <paramref name="eventData"/>
        /// </summary>
        /// <param name="eventData">Data of the event being processing</param>
        /// <param name="payload">Json with value to be extracted</param>
        /// <returns>Modified <paramref name="eventData"/></returns>
        public abstract JObject ApplyRule([NotNull] JObject eventData, [NotNull] JObject payload);
    }
}