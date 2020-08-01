using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpsBro.Domain.Extraction.Validation;

namespace OpsBro.Domain.Settings
{
    public class SettingsFactory
    {
        public async Task<ISettings> CreateSettings()
        {
            var url = Environment.GetEnvironmentVariable("JSON_FILE_URL");

            string configuration;

            if (File.Exists(url))
            {
                configuration = File.ReadAllText(url, Encoding.UTF8);
            }
            else
            {
                using var httpClient = new HttpClient();
                configuration = await httpClient.GetStringAsync(url);
            }
            
            var settings = JsonConvert.DeserializeObject<Settings>(configuration, serializerSettings);
            ConfigValidationRule.Config = settings.Config;

            return settings;
        }

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ExtractionRuleConverter(),
                new StringEnumConverter(),
                new HttpMethodStringConverter(),
                new ValidationRuleConverter()
            },
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}