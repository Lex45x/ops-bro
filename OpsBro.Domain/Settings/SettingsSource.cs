using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpsBro.Abstractions.Entities;
using OpsBro.Abstractions.Settings;

namespace OpsBro.Domain.Settings
{
    public class SettingsSource : ISettingsSource
    {
        public SettingsSource()
        {
            var url = Environment.GetEnvironmentVariable("JSON_FILE_URL");
            var httpClient = new HttpClient();
            httpClient
                .GetStringAsync(url)
                .ContinueWith(task => OnFileDownloaded(task, httpClient));
        }

        private void OnFileDownloaded(Task<string> task, HttpClient client)
        {
            client.Dispose();

            var root = JObject.Parse(task.Result);

            Listeners = root["listeners"].ToObject<List<Listener>>();
            EventDefinitions = root["eventDefinitions"].ToObject<List<EventDefinition>>();
        }


        public ICollection<Listener> Listeners { get; private set; }
        public ICollection<EventDefinition> EventDefinitions { get; private set; }
    }
}