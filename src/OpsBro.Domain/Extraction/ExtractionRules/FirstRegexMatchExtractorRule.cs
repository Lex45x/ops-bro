using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.ExtractionRules
{
    /// <summary>
    /// Represent extractor of <see cref="ExtractionType.FirstRegexMatch"/> type.
    /// </summary>
    public class FirstRegexMatchExtractorRule : ExtractionRule
    {
        public string Pattern { get; }

        public FirstRegexMatchExtractorRule(string path, string property, string pattern)
            : base(ExtractionType.FirstRegexMatch, path, property)
        {
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        /// <inheritdoc />
        public override JObject ApplyRule(JObject eventData, JObject payload)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var selectedValue = payload.SelectToken(Path);

            if (selectedValue?.Type != JTokenType.String)
            {
                return eventData;
            }

            var match = Regex.Match(selectedValue.ToObject<string>(), Pattern);

            if (!match.Success)
            {
                return eventData;
            }

            eventData[Property] = match.Value;

            return eventData;
        }
    }
}