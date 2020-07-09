﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction;
using OpsBro.Domain.Extraction.Validation;

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
            //todo: add config assertions
            var root = JObject.Parse(configuration);

            ListenersByListenerName = root["listeners"]
                .ToObject<List<Listener>>(serializer)
                .ToDictionary(listener => listener.Name);

            EventDispatcherByEventName = root["eventDispatchers"]
                .ToObject<List<EventDispatcher>>(serializer)
                .ToDictionary(dispatcher => dispatcher.EventName);

            Config = root["config"] as JObject;
            ConfigValidationRule.Config = Config;
        }
        
        private readonly JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ExtractionRuleConverter(),
                new StringEnumConverter(),
                new HttpMethodStringConverter(),
                new ValidationRuleConverter()
            },
            PreserveReferencesHandling = PreserveReferencesHandling.All
        });
        
        public Task Initialization { get; }
        public IDictionary<string, Listener> ListenersByListenerName { get; private set; }
        public IDictionary<string, EventDispatcher> EventDispatcherByEventName { get; private set; }
        public JObject Config { get; private set; }
    }

}