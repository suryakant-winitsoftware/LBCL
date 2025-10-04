using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Hangfire.Storage;
using Serilog;
using WINITSyncManager;
using WINITSyncManager.Common;
using WINITSyncManager.Constants;
using WINITSyncManager.CustomAttributes;
public static class Program
{
    public static IServiceProvider serviceProvider { get; private set; }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //  Register services (add your services here)
        builder.Services.ConfigureServices();
        // Build the configuration
        var config = builder.Configuration;
        // Configure Serilog with dynamic configuration from appsettings.json
        var connectionString = CommonFunctions.GetStringValue(config.GetConnectionString("SqlServer")) ?? "";
        var sinkOptions = new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
        {
            AutoCreateSqlTable = true,
            TableName = $"int_error_log{DateTime.UtcNow:yyMM}",
            BatchPostingLimit = 100,
        };
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config) // Read configuration from appsettings.json
            .WriteTo.MSSqlServer(connectionString, sinkOptions, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();
        // Register the Serilog logger
        _ = builder.Services.AddLogging(loggingBuilder =>
        {
            _ = loggingBuilder.ClearProviders();
            _ = loggingBuilder.AddSerilog();
        });
        // Add Hangfire services with SQL Server storage
        builder.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseConsole()
                  .UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                  {
                      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                      QueuePollInterval = TimeSpan.Zero,
                      UseRecommendedIsolationLevel = true,
                      DisableGlobalLocks = true,
                  });
            // Add global filters and job configurations
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 }); // Retry failed jobs
            GlobalJobFilters.Filters.Add(new CustomExpirationAttribute(TimeSpan.FromDays(2))); // Expire jobs after 2  
        });
        builder.Services.AddHangfireServer();
        var app = builder.Build();
        serviceProvider = app.Services;
        // Enable Hangfire dashboard
        app.UseHangfireDashboard("/IntegrationDashBoard");
        //to remove old jobs 
        var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
        var manager = new RecurringJobManager();
        foreach (var job in recurringJobs)
        {
            manager.RemoveIfExists(job.Id);
        }
        // add jobs here 
        var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
        ProcessJobs.RegisterJobs(recurringJobManager, serviceProvider, config);
        RecurringJob.AddOrUpdate("CheckAndCancelLongRunningJobs", () => CheckAndCancelLongRunningJobs(null, CancellationToken.None), CronExpressions.EveryFiveMinutes);
        RecurringJob.AddOrUpdate("CleanupFailedJobs", () => CleanupFailedJobs(), CronExpressions.EveryOneDay);
        var options = new BackgroundJobServerOptions
        {
            Queues = new[] { "long-running-queue", "push-queue", "default", "out-standing-invoice" }, // Adjust queues as needed
            WorkerCount = 4
        };
        //using (var server = new BackgroundJobServer(options))
        //{
        //    app.Run();
        //}
        var server = new BackgroundJobServer(options);
        app.Run();
    }
    //public static void CheckAndCancelLongRunningJobs()
    //{
    //    // Get all processing jobs
    //    var monitoringApi = JobStorage.Current.GetMonitoringApi();
    //    var processingJobs = monitoringApi.ProcessingJobs(0, 100);
    //    foreach (var job in processingJobs)
    //    {
    //        var jobDetails = monitoringApi.JobDetails(job.Key);
    //        if (jobDetails == null)
    //        {
    //            continue; // Skip if job details could not be found
    //        }
    //        // Calculate job duration
    //        var jobDuration = DateTime.UtcNow - jobDetails.CreatedAt;
    //        if (jobDuration > TimeSpan.FromMinutes(5)) // Timeout limit of 10 minutes
    //        {
    //            BackgroundJob.Delete(job.Key); // Cancel the job
    //        }
    //    }
    //}
    public static async Task CheckAndCancelLongRunningJobs(PerformContext context, CancellationToken cancellationToken)
    {
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(120), cancellationToken);
        var jobTask = Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++) // Simulate work
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Job canceled due to timeout.");
                }
                await Task.Delay(500); // Simulating processing
            }
        });
        if (await Task.WhenAny(jobTask, timeoutTask) == timeoutTask)
        {
            throw new OperationCanceledException("Job exceeded the 15-minute limit and was terminated.");
        }
    }
    public static void CleanupFailedJobs()
    {
        var monitor = JobStorage.Current.GetMonitoringApi();
        var failedJobs = monitor.FailedJobs(0, 1000 /* limit */);
        while (failedJobs.Count > 0)
        {
            foreach (var job in failedJobs)
            {
                BackgroundJob.Delete(job.Key);
            }
            failedJobs = monitor.FailedJobs(0, 1000 /* limit */);
        }
    }
}
