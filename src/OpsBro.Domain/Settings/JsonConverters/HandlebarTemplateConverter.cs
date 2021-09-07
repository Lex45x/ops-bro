using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events.Templates;

namespace OpsBro.Domain.Settings.JsonConverters
{
    public class HandlebarTemplateConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var template = JToken.Load(reader);

            return template.Type == JTokenType.String
                ? new HandlebarTemplate(template.ToObject<string>())
                : null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HandlebarTemplate);
        }

        public override bool CanWrite => false;
    }
}