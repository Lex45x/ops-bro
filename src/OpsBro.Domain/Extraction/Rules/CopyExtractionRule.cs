using System;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.Rules
{
    /// <summary>
    /// Extraction rule of <see cref="ExtractionType.Copy"/> type.
    /// </summary>
    public class CopyExtractionRule : ExtractionRule
    {
        public CopyExtractionRule(string path, string property) : base(ExtractionType.Copy, path, property)
        {
        }

        /// <inheritdoc />
        public override JObject ApplyRule(JObject eventData, JObject payload)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var selectedValue = payload.SelectToken(Path);

            if (selectedValue == null)
            {
                return eventData;
            }

            eventData[Property] = selectedValue;

            return eventData;
        }
    }
}