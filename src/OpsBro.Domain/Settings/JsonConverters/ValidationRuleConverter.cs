using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpsBro.Domain.Extraction.Validation;

namespace OpsBro.Domain.Settings
{
    public class ValidationRuleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var rule = JObject.Load(reader);
            Type ruleType = null;

            if (rule.ContainsKey("value"))
            {
                ruleType = typeof(ValueValidationRule);
            }

            if (rule.ContainsKey("configPath"))
            {
                ruleType = typeof(ConfigValidationRule);
            }

            if (ruleType == null)
            {
                throw new JsonSerializationException("Could not retrieve validation rule type from existing keys.");
            }

            var deserializedRule = rule.ToObject(ruleType, serializer);

            return deserializedRule;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ValidationRule);
        }

        public override bool CanWrite => false;
    }
}