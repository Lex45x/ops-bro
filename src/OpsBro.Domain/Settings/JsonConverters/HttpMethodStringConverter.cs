using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace OpsBro.Domain.Settings
{
    internal class HttpMethodStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var httpMethod = value as HttpMethod;

            writer.WriteValue(httpMethod?.Method);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var httpMethodString = reader.Value as string;
            return new HttpMethod(httpMethodString);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpMethod);
        }
    }
}