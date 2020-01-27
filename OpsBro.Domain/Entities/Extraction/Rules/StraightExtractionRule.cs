using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Entities.Extraction.Rules
{
    /// <summary>
    /// Extraction rule of <see cref="ExtractionType.Straight"/> type.
    /// </summary>
    public class CopyExtractionRule : ExtractionRule
    {
        public CopyExtractionRule(string path, string property) : base(ExtractionType.Copy, path, property)
        {
        }

        /// <inheritdoc />
        public override JObject ApplyRule(JObject eventData, JObject payload)
        {
            var selectedValue = payload.SelectToken(Path);
            eventData[Property] = selectedValue;

            return eventData;
        }
    }
}