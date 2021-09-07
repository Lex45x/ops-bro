using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;
using OpsBro.Domain.Extraction.ValidationRules;
using OpsBro.Domain.Settings.JsonConverters;
using YamlDotNet.Serialization;

namespace OpsBro.Domain.Settings
{
    public class SettingsFactory
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        public async Task<ISettings> CreateSettings()
        {
            //JSON_FILE_URL is deprecated but has to be used for backward compatibility
            var configurationFileUrl = Environment.GetEnvironmentVariable("CONFIGURATION_FILE_URL");

            if (string.IsNullOrWhiteSpace(configurationFileUrl))
            {
                configurationFileUrl = Environment.GetEnvironmentVariable("JSON_FILE_URL");
                if (string.IsNullOrWhiteSpace(configurationFileUrl))
                {
                    logger.Warn("`JSON_FILE_URL` variable usage is deprecated! Use `CONFIGURATION_FILE_URL` instead");
                }
            }

            logger.Info("Configuration file url: {url}", configurationFileUrl);

            string configuration;

            if (File.Exists(configurationFileUrl))
            {
                configuration = await File.ReadAllTextAsync(configurationFileUrl, Encoding.UTF8);
                logger.Info("Configuration file found and loaded!");
            }
            else
            {
                using var httpClient = new HttpClient();
                configuration = await httpClient.GetStringAsync(configurationFileUrl);
                logger.Info("Configuration file downloaded!");
            }

            logger.Debug("Configuration file content: {configuration}", configuration);

            var configurationFileExtension = configurationFileUrl.Split('.').Last();

            switch (configurationFileExtension)
            {
                case "yaml":
                    logger.Debug("Configuraiton file type is YAML, converting to JSON");
                    var deserializer = new DeserializerBuilder()
                        .Build();

                    var yamlReader = new StringReader(configuration);
                    var yamlObject = deserializer.Deserialize(yamlReader);

                    var serializer = new SerializerBuilder()
                        .JsonCompatible()
                        .Build();

                    configuration = serializer.Serialize(yamlObject);

                    logger.Debug("Configuration file converted: {config}", configuration);
                    break;
                case "json":
                    break;
                default:
                    throw new InvalidOperationException("Unsupported configuration file extension! Only .json and .yaml supported.");
            }

            var configurationObject = JObject.Parse(configuration);

            var schema = GetConfigurationFileSchema();
            configurationObject.Validate(schema);

            var settings = configurationObject.ToObject<Settings>(JsonSerializer.Create(serializerSettings));

            logger.Info("Settings object deserialized!");

            return settings;
        }

        private JSchema GetConfigurationFileSchema()
        {
            var stream = GetType().Assembly.GetManifestResourceStream("OpsBro.Domain.settings-schema.json");
            using var streamReader = new StreamReader(stream);
            var jsonSchema = streamReader.ReadToEnd();
            var schema = JSchema.Parse(jsonSchema);

            return schema;
        }

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new ExtractionRuleConverter(),
                new StringEnumConverter(),
                new HttpMethodStringConverter(),
                new ValidationRuleConverter(),
                new UnnestingRuleConverter()
            },
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}