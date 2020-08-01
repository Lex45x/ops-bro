using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OpsBro.Domain.Settings;
using System.Threading.Tasks;

namespace OpsBro.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var settingsFactory = new SettingsFactory();
            Settings = await settingsFactory.CreateSettings();

            BuildWebHost(args).Run();
        }

        public static ISettings Settings { get; private set; }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
