using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Extraction.UnnestingRules;

namespace OpsBro.Domain.Settings.JsonConverters
{
    public class UnnestingRuleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var rule = JObject.Load(reader);
            var extractionType = rule["type"].ToObject<UnnestingRuleType>();

            return extractionType switch
            {
                UnnestingRuleType.PerRegexMatch => rule.ToObject<UnnestingPerRegexMatchRule>(serializer),
                UnnestingRuleType.PerArrayEntry => rule.ToObject<UnnestingPerArrayEntryRule>(serializer),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UnnestingRule);
        }

        public override bool CanWrite => false;
    }
}