﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Extraction.ExtractionRules;

namespace OpsBro.Domain.Settings.JsonConverters
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
                ExtractionType.FirstRegexMatch => rule.ToObject<FirstRegexMatchExtractorRule>(serializer),
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