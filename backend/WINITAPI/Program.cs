using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
//using Serilog;
//using Serilog.Events;
//using Serilog.Formatting.Compact;
//using Serilog;
//using Serilog.AspNetCore;
//using Serilog.Formatting.Compact;
//using Serilog.Events;

namespace WINITAPI
{

    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var urls = configuration["urls"] ?? configuration["ASPNETCORE_URLS"] ?? "https://multiplex-promotions.winitsoftware.com";
            
            logger.LogInformation("Starting WINITAPI application...");
            logger.LogInformation("Application will listen on: {Urls}", urls);
            
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenLocalhost(8000);
                    });
                });
    }

    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        CreateHostBuilder(args).Build().Run();

    //    }
    //    public static IHostBuilder CreateHostBuilder(string[] args) =>
    //        Host.CreateDefaultBuilder(args)
    //         .UseSerilog((hostingContext, loggerConfiguration) =>
    //         {
    //             loggerConfiguration
    //                 .MinimumLevel.Information()
    //                 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    //                 .MinimumLevel.Override("System", LogEventLevel.Warning)
    //                 .Enrich.FromLogContext()
    //                 .WriteTo.Console(new CompactJsonFormatter())
    //                 .WriteTo.File("C:\\Logs\\logs.txt", rollingInterval: RollingInterval.Day);
    //         })
    //        .ConfigureWebHostDefaults(webBuilder =>
    //        {
    //            webBuilder.UseStartup<Startup>();

    //        });
    //}
}

