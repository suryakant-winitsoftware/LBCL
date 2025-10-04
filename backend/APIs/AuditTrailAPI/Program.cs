using AuditTrailAPI3.BL;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.DL.Classes;
using Winit.Modules.AuditTrail.DL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Notification.BL.Classes.Common;
using Winit.Modules.Notification.BL.Interfaces.Common;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });


// Configure MongoDB settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

// Add MongoDB client as a singleton
//builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
//{
//    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();
//    return new MongoClient(settings.ConnectionString);
//});

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        int port = 5672; // Default port if not set in configuration
        if (!string.IsNullOrEmpty(configuration["RabbitMQ:Port"]))
        {
            port = Convert.ToInt32(configuration["RabbitMQ:Port"]);
        }
        cfg.Host(configuration["RabbitMQ:HostName"], 5672, "/", h =>
        {
            h.Username(configuration["RabbitMQ:UserName"] ?? "");
            h.Password(configuration["RabbitMQ:Password"] ?? "");
        });
        // For Topic exchange
        cfg.Message<IAuditTrailEntry>(config =>
        {
            config.SetEntityName(ExchangeNames.AuditTrailExchange); // Use the same exchange name
        });
        cfg.Publish<IAuditTrailEntry>(publishConfig =>
        {
            publishConfig.AutoDelete = false; // Ensure exchange persists
            publishConfig.Durable = true;     // Make the exchange durable
            publishConfig.ExchangeType = ExchangeTypes.Topic;
        });
        cfg.Message<AuditTrailEntry>(config =>
        {
            config.SetEntityName(ExchangeNames.AuditTrailExchange); // Use the same exchange name
        });
        cfg.Publish<AuditTrailEntry>(publishConfig =>
        {
            publishConfig.AutoDelete = false; // Ensure exchange persists
            publishConfig.Durable = true;     // Make the exchange durable
            publishConfig.ExchangeType = ExchangeTypes.Topic;
        });
    });
});

builder.Services.AddSingleton<IAuditTrailServiceDL, AuditTrailServiceDL>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;

    return new AuditTrailServiceDL(
        settings.ConnectionString,
        settings.DatabaseName,
        "CMIAuditTrail"
    );
});

builder.Services.AddScoped<IAuditTrailServiceBL, AuditTrailServiceBL>();
builder.Services.AddScoped<AuditChangeProcessor>();
builder.Services.AddScoped<INotificationPublisherService, NotificationPublisherService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod());
app.UseAuthorization();

app.MapControllers();

app.Run();
