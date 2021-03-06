﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpsBro.Domain.Extraction.Rules;

namespace OpsBro.Domain.Settings
{
    public class ExtractionRuleConverter : JsonConverter
    {
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
}