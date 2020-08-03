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
        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
               .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
            
            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("Welcome to OpsBro! -- this message means that logging is configured and working correctly");

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

            BuildWebHost(args).Run();
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
