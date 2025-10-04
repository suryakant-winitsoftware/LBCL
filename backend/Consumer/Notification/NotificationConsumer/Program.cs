using MassTransit;
using NotificationConsumer.Consumers.General;
using NotificationConsumer.Consumers.Order;
using NotificationConsumer.Consumers.OTP;
using NotificationConsumer.Consumers.Return;
using Winit.Modules.Notification.Model.Constant;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Modules.Notification.BL.Classes.Common;
using Winit.Modules.Notification.BL.Interfaces.DataServices.General;
using Winit.Modules.Notification.BL.Classes.DataServices.General;
using Winit.Modules.Notification.BL.Classes.DataServices.Order;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Order;
using Winit.Modules.Notification.BL.Classes.DataServices.Return;
using Winit.Modules.Notification.BL.Interfaces.DataServices.Return;
using Winit.Modules.Base.BL;
using Winit.Modules.Email.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Modules.Email.DL.UtilityClasses;
using Winit.Modules.Email.BL.Interfaces;
using Winit.Modules.Email.BL.Classes;
using Winit.Modules.Email.DL.Interfaces;
using Winit.Modules.Email.DL.Classes;
using Microsoft.JSInterop;
using Winit.Modules.SMS.Model.Interfaces;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.Email.Model.Interfaces;
using Winit.Modules.Notification.Model.Interfaces.Order;
using Winit.Modules.Notification.Model.Classes.Order;
using Winit.Modules.SMS.DL.Interfaces;
using Winit.Modules.SMS.DL.Classes;
using Serilog;
using Serilog.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog.Core;
using Serilog.Events;

namespace NotificationConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);
            var configuration = builder.Configuration;

            var infoLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);
            var errorLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Error);

            // Configure Serilog with separate log files
            Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(infoLevelSwitch) // Default level
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // Console logs everything
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information) // Log only Information level
        .WriteTo.File("C:\\Logs\\NotificationNoDataLog.txt", rollingInterval: RollingInterval.Day)
    )
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error) // Log only Errors
        .WriteTo.File("C:\\Logs\\NotificationErrorLog.txt", rollingInterval: RollingInterval.Day)
    )
    .CreateLogger();

            // Register Serilog as the logging provider
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); // Clear default logging providers
                loggingBuilder.AddSerilog(); // Add Serilog as the logging provider
            });


            // Register RabbitMQ and MassTransit services
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<NotificationOrderEmailConsumer>();
                x.AddConsumer<NotificationOrderSMSConsumer>();
                x.AddConsumer<NotificationOTPEmailConsumer>();
                x.AddConsumer<NotificationOTPSMSConsumer>();
                x.AddConsumer<NotificationGeneralEmailConsumer>();
                x.AddConsumer<NotificationGeneralSMSConsumer>();
                x.AddConsumer<NotificationReturnEmailConsumer>();
                x.AddConsumer<NotificationReturnSMSConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    //x.SetKebabCaseEndpointNameFormatter();
                    int port = 5672; // Default port if not set in configuration
                    if (!string.IsNullOrEmpty(configuration["RabbitMQ:Port"]))
                    {
                        port = Convert.ToInt32(configuration["RabbitMQ:Port"]);
                    }
                    cfg.Host(configuration["RabbitMQ:HostName"], 5672, configuration["RabbitMQ:VirtualHost"] ?? "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:UserName"] ?? "");
                        h.Password(configuration["RabbitMQ:Password"] ?? "");
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_Order_Email_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationOrderEmailConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_Order_Email;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_Order_SMS_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationOrderSMSConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_Order_SMS;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_OTP_Email_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationOTPEmailConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_OTP_Email;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_OTP_SMS_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationOTPSMSConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_OTP_SMS;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_General_Email_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationGeneralEmailConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_General_Email;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    cfg.ReceiveEndpoint(QueueNames.Notification_General_SMS_Queue, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Durable = true; // Ensure the queue is durable
                        e.AutoDelete = false; // Prevent the queue from being deleted automatically
                        e.Lazy = true;
                        e.SetQueueArgument("x-queue-type", "classic");
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
                        e.ConfigureConsumer<NotificationGeneralSMSConsumer>(context);
                        e.Bind(ExchangeNames.NotificationExchange, x =>
                        {
                            x.ExchangeType = ExchangeTypes.Topic;
                            x.RoutingKey = NotificationRoutes.Notification_General_SMS;
                            x.SetExchangeArgument("x-max-length-bytes", 10737418240);
                        });
                        e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
                    });
                    /*
                    // Fanout exchange Queue
                    cfg.ReceiveEndpoint("fanout_sms_queue", e =>
                    {
                        e.Bind("fanout_exchange_name");
                    });

                    // Topic exchange Queue
                    cfg.ReceiveEndpoint("topic_sms_queue", e =>
                    {
                        e.Bind("topic_exchange_name", x => x.RoutingKey = "sms.*");
                    });

                    // Header Exchange Queue
                    cfg.ReceiveEndpoint("headers_sms_queue", e =>
                    {
                        e.Bind("headers_exchange_name", x =>
                        {
                            x.Argument("type", "transactional");
                            x.Argument("priority", "high");
                        });
                    });
                    */
                });
            });


            builder.Services.AddHostedService<Worker>();

            builder.Services.AddScoped<INotificationGeneralDataServiceBL, NotificationGeneralDataServiceBL>();
            builder.Services.AddScoped<INotificationOrderDataServiceBL, NotificationOrderDataServiceBL>();
            builder.Services.AddScoped<INotificationReturnDataServiceBL, NotificationReturnDataServiceBL>();
            builder.Services.AddScoped<INotificationEmailServiceBL, NotificationEmailServiceBL>();
            builder.Services.AddScoped<INotificationSMSServiceBL, NotificationSMSServiceBL>();
            builder.Services.AddTransient<ApiService>();
            builder.Services.AddTransient<HttpClient>();
            builder.Services.AddTransient<EmailUtility>();
            builder.Services.AddTransient<EmailFromBodyModelDTO>();
            builder.Services.AddTransient<EmailRequestDTO>();
            builder.Services.AddTransient<IEmailRequestModel, EmailRequestModel>();
            builder.Services.AddTransient<IAppConfig, AppConfigs>();
            builder.Services.AddTransient<IEmailBL, EmailBL>();
            builder.Services.AddTransient<IEmailDL, MSSQLEmailDL>();
            builder.Services.AddTransient<ISMSDL, MSSQLSMSDL>();
            builder.Services.AddTransient<ISmsTemplateFields, SmsTemplateFields>();
            builder.Services.AddTransient<Winit.Modules.Email.Model.Interfaces.INotificationPOData, Winit.Modules.Email.Model.Classes.NotificationPOData>();
            builder.Services.AddTransient<Winit.Modules.SMS.Model.Interfaces.INotificationPOData, Winit.Modules.SMS.Model.Classes.NotificationPOData>();
            builder.Services.AddTransient<Winit.Modules.Notification.Model.Interfaces.Order.INotificationPOData, Winit.Modules.Notification.Model.Classes.Order.NotificationPOData>();


            var host = builder.Build();
            host.Run();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();
        }
    }
}