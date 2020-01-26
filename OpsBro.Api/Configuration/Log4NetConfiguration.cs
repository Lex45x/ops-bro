namespace OpsBro.Api.Configuration
{
    public class Log4NetConfiguration : AbstractConfiguration, ILog4NetConfiguration
    {
        public string ConfigurationFile { get; set; }

        public Log4NetConfiguration(IConfigurationRoot root)
            : base(root)
        {
        }
    }
}