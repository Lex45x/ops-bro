using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpsBro.Domain.Entities.Extraction.Rules;

namespace OpsBro.Domain.Settings
{
    public class ExtractionRuleConverter : JsonConverter
    {
        private static JsonSerializerSettings _specifiedSubclassConversion = new JsonSerializerSettings
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter()
        };

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var rule = JObject.Load(reader);
            var extractionType = rule["type"].ToObject<ExtractionType>();

            return extractionType switch
            {
                ExtractionType.Copy => rule.ToObject<CopyExtractionRule>(serializer),
                ExtractionType.FirstRegexMatch => rule.ToObject<FirstRegexMatchExtractorRule>(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ExtractionRule);
        }

        public override bool CanWrite => false;
    }

    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(ExtractionRule).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
}