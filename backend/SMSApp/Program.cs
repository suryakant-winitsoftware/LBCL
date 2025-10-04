using Hangfire;
using Hangfire.Console;
using Hangfire.Storage;
using SMSApp;
using SMSApp.CustomAttributes;
using Winit.Modules.Email.BL.Classes;
using Winit.Modules.Email.BL.Interfaces;
using Winit.Modules.Email.DL.Classes;
using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.DL.UtilityClasses;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.SMS.BL.Classes;
using Winit.Modules.SMS.BL.Interfaces;
using Winit.Modules.SMS.DL.Classes;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); 

var config = builder.Configuration;
var connectionString = CommonFunctions.GetStringValue(config.GetConnectionString("SqlServer")) ?? "";

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
              DisableGlobalLocks = true
          });
    // Add global filters and job configurations
    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 }); // Retry failed jobs
    GlobalJobFilters.Filters.Add(new CustomExpirationAttribute(TimeSpan.FromDays(4))); // Expire jobs after 4 days
});
builder.Services.AddHangfireServer();
builder.Services.AddTransient<HttpClient>();
//Sms services
builder.Services.AddTransient<ISMSBL, SMSBL>();
builder.Services.AddTransient<ISMSDL, MSSQLSMSDL>();
builder.Services.AddTransient<ISmsRequestModel, SmsRequestModel>();
builder.Services.AddTransient<SmsRequestDTO>();
//Email Services
builder.Services.AddTransient<EmailUtility>();
builder.Services.AddTransient<IEmailBL, EmailBL>();
builder.Services.AddTransient<IEmailDL, MSSQLEmailDL>();
builder.Services.AddTransient<IEmailRequestModel, EmailRequestModel>();
builder.Services.AddTransient<EmailRequestDTO>();


var app = builder.Build();

var serviceProvider = app.Services;
// Enable Hangfire dashboard
app.UseHangfireDashboard("/SMSApp");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();


//to remove old jobs 
var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
var manager = new RecurringJobManager();
foreach (var job in recurringJobs)
{
    manager.RemoveIfExists(job.Id);
}

// add jobs here 
var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
var smsBL = serviceProvider.GetRequiredService<ISMSBL>();
var emailBL = serviceProvider.GetRequiredService<IEmailBL>();
ProcessJobs.RegisterJobs(recurringJobManager, serviceProvider, smsBL, emailBL);

//RecurringJob.AddOrUpdate("CheckAndCancelLongRunningJobs", () => CheckAndCancelLongRunningJobs(), CronExpressions.EveryFiveMinutes);

var options = new BackgroundJobServerOptions
{
    Queues = new[] { "long-running-queue", "push-queue", "default" } // Adjust queues as needed
};

using (var server = new BackgroundJobServer(options))
{
    app.Run();
}

//app.Run();

