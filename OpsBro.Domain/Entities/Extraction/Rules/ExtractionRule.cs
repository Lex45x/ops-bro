using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Settings;

namespace OpsBro.Domain.Entities.Extraction.Rules
{
    /// <summary>
    ///  Base class for all extraction rules
    /// </summary>
    public abstract class ExtractionRule
    {
        protected ExtractionRule(ExtractionType type, string path, string property)
        {
            Type = type;
            Path = path;
            Property = property;
        }

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
        public abstract JObject ApplyRule(JObject eventData, JObject payload);
    }
}