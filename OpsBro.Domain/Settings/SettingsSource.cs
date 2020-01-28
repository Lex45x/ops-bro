using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            ListenersByListenerName = root["listeners"]
                .ToObject<List<Listener>>(serializer)
                .ToDictionary(listener => listener.Name);

            EventDispatcherByEventName = root["eventDispatchers"]
                .ToObject<List<EventDispatcher>>(serializer)
                .ToDictionary(dispatcher => dispatcher.EventName);
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
        public IDictionary<string, Listener> ListenersByListenerName { get; private set; }
        public IDictionary<string, EventDispatcher> EventDispatcherByEventName { get; private set; }
    }

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