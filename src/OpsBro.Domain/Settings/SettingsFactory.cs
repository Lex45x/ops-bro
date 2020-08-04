using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using OpsBro.Domain.Extraction.Validation;

namespace OpsBro.Domain.Settings
{
    public class SettingsFactory
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        public async Task<ISettings> CreateSettings()
        {
            var url = Environment.GetEnvironmentVariable("JSON_FILE_URL");

            logger.Info("Configuraiton file url: {url}", url);

            string configuration;

            if (File.Exists(url))
            {
                configuration = File.ReadAllText(url, Encoding.UTF8);
                logger.Info("Configuration file found and loaded!");
            }
            else
            {
                using var httpClient = new HttpClient();
                configuration = await httpClient.GetStringAsync(url);
                logger.Info("Configuration file downloaded!");
            }

            logger.Debug("Configuraiton file content: {configuration}", configuration);

            var settings = JsonConvert.DeserializeObject<Settings>(configuration, serializerSettings);
            ConfigValidationRule.Config = settings.Config;

            logger.Info("Settings object deserialized!");

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