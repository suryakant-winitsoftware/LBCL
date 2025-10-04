using MassTransit;
using Microsoft.OpenApi.Models;
using Winit.Modules.Notification.Model.Interfaces;
using Winit.Modules.Notification.Model.Constant;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.BL.Classes.Common;
using Winit.Modules.Notification.BL.Classes.DataServices.Order;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Order;
using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.DL.Classes;
using Winit.Modules.Email.DL.UtilityClasses;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.DL.Classes;
using Winit.Shared.Models.Common;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification API",                     // API title displayed in Swagger UI.
        Version = "v1",                                 // API version identifier.
        Description = "API to send notification",               // Short description of the API.
        Contact = new OpenApiContact                    // Contact information block.
        {
            Name = "WINIT"
        }
    });
});
builder.Services.AddTransient<INotificationOrderDataServiceBL, NotificationOrderDataServiceBL>();
builder.Services.AddTransient<IEmailDL, MSSQLEmailDL>();
builder.Services.AddTransient<ISMSDL, MSSQLSMSDL>();
builder.Services.AddTransient<EmailUtility>();
builder.Services.AddTransient<HttpClient>();
builder.Services.AddTransient<IAppConfig, AppConfigs>();
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
        cfg.Host(configuration["RabbitMQ:HostName"], 5672, configuration["RabbitMQ:VirtualHost"] ?? "/", h =>
        {
            h.Username(configuration["RabbitMQ:UserName"]??"");
            h.Password(configuration["RabbitMQ:Password"]??"");
        });
        // For Topic exchange
        cfg.Message<INotificationRequest>(config =>
        {
            config.SetEntityName(ExchangeNames.NotificationExchange); // Use the same exchange name
        });
        cfg.Publish<INotificationRequest>(publishConfig =>
        {
            publishConfig.AutoDelete = false; // Ensure exchange persists
            publishConfig.Durable = true;     // Make the exchange durable
            publishConfig.ExchangeType = ExchangeTypes.Topic;
        });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny",
    builder =>
    {
        _ = builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddScoped<INotificationPublisherService, NotificationPublisherService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
_ = app.UseCors(builder =>
{
    _ = builder.AllowAnyOrigin();
    _ = builder.AllowAnyHeader();
    _ = builder.AllowAnyMethod();
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
