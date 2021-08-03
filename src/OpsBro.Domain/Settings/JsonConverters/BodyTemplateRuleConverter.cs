using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events.Templates;
using OpsBro.Domain.Extraction.ExtractionRules;

namespace OpsBro.Domain.Settings.JsonConverters
{
    public class BodyTemplateRuleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var rule = JObject.Load(reader);
            var substringSpecification = rule["substring"];

            return substringSpecification == null 
                ? (BodyTemplateRule) rule.ToObject<BodyTokenTemplateRule>(serializer) 
                : rule.ToObject<BodySubstringTemplateRule>(serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BodyTemplateRule);
        }

        public override bool CanWrite => false;
    }
}