using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WINITSyncDataProcessor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Build the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // 2. Create a service collection
            var serviceCollection = new ServiceCollection();

            // 3. Add configuration to the services
            serviceCollection.AddSingleton<IConfiguration>(config);

            // 4. Register services (add your services here)
            ConfigureServices(serviceCollection);

            // 5. Build the service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(config)
                        .CreateLogger();
            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
            //start services  app
            var app = serviceProvider.GetRequiredService<App>();
            await app.Run();

            //Console.WriteLine("Hello, World!");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register interface with implementation

            _ = services.AddTransient<App>();  // Register main App class
        }
    }
}
