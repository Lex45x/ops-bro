using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Entities.Extraction.Rules
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
            Pattern = pattern;
        }

        /// <inheritdoc />
        public override JObject ApplyRule(JObject eventData, JObject payload)
        {
            var selectedValue = payload.SelectToken(Path);

            var match = Regex.Match(selectedValue.ToObject<string>(), Pattern);

            eventData[Property] = match.Value;

            return eventData;
        }
    }
}