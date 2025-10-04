using AuditTrailConsumer;
using AuditTrailConsumer.BL;
using AuditTrailConsumer.Consumers;
using MassTransit;
using Microsoft.Extensions.Options;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.DL.Classes;
using Winit.Modules.AuditTrail.DL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

// Configure MongoDB settings
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

// Register RabbitMQ and MassTransit services
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditTrailProcessConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        //x.SetKebabCaseEndpointNameFormatter();
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
        cfg.ReceiveEndpoint(QueueNames.AuditTrail_Queue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<AuditTrailProcessConsumer>(context);
            e.Bind(ExchangeNames.AuditTrailExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.AuditTrail_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
    });
});

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<IAuditTrailEntry, AuditTrailEntry>();
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

var host = builder.Build();
host.Run();
