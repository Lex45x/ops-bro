using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;

namespace OpsBro.Domain.Settings
{
    public class Settings : ISettings
    {
        public Settings()
        {
            var url = Environment.GetEnvironmentVariable("JSON_FILE_URL");

            if (File.Exists(url))
            {
                var configuration = File.ReadAllText(url, Encoding.UTF8);
                Initialize(configuration);
                Initialization = Task.CompletedTask;
            }
            else
            {
                var httpClient = new HttpClient();
                Initialization = httpClient
                    .GetStringAsync(url)
                    .ContinueWith(task =>
                    {
                        Initialize(task.Result);
                        httpClient.Dispose();
                    });
            }
        }

        private void Initialize(string configuration)
        {
            var root = JObject.Parse(configuration);

            Listeners = root["listeners"].ToObject<List<Listener>>(serializer);
            EventDispatchers = root["eventDispatchers"].ToObject<List<EventDispatcher>>(serializer);
        }

        private readonly JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ExtractionRuleConverter(),
                new StringEnumConverter(),
                new HttpMethodStringConverter()
            }
        });

        public Task Initialization { get; }
        public ICollection<Listener> Listeners { get; private set; }
        public ICollection<EventDispatcher> EventDispatchers { get; private set; }
    }

    internal class HttpMethodStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var httpMethod = value as HttpMethod;

            writer.WriteValue(httpMethod?.Method);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
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