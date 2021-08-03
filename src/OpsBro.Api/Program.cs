using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using OpsBro.Domain.Settings;
using System;
using System.Threading.Tasks;

namespace OpsBro.Api
{
    public class Program
    {
        public static async Task Main(params string[] args)
        {
            var logger = InitializeLogging();

            await ConfigureApplication(logger);

            await BuildWebHost(args)
                .RunAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        private static async Task ConfigureApplication(Logger logger)
        {
            try
            {
                var settingsFactory = new SettingsFactory();
                Settings = await settingsFactory.CreateSettings();
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Settings deserialization is failed!");
                throw;
            }
        }

        private static Logger InitializeLogging()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

            var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");

            LogManager.GlobalThreshold = logLevel == null 
                ? NLog.LogLevel.Info 
                : NLog.LogLevel.FromString(logLevel);

            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("Welcome to OpsBro! -- this message means that logging is configured and working properly");
            return logger;
        }

        public static ISettings Settings { get; private set; }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog()
                .Build();
    }
}