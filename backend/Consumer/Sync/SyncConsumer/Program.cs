using MassTransit;
using SyncConsumer.Common.Collection.Classes;
using SyncConsumer.Common.Collection.Interfaces;
using SyncConsumer.Consumers;
using Winit.Modules.CollectionModule.BL.Classes;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.DL.Classes;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.JourneyPlan.DL.Classes;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.DL.Classes;
using Winit.Modules.ReturnOrder.DL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Classes;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Modules.SalesOrder.DL.Classes;
using Winit.Modules.SalesOrder.DL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.StockUpdater.DL.Classes;
using Winit.Modules.StockUpdater.DL.Interfaces;
using Winit.Modules.StockUpdater.Model.Classes;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;
using Winit.Modules.StoreActivity.Model.Interfaces;
using Winit.Shared.Models.Constants.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

// Register RabbitMQ and MassTransit services
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CollectionSyncConsumer>();
    x.AddConsumer<MasterProcessConsumer>();
    x.AddConsumer<SalesOrderSyncConsumer>();
    x.AddConsumer<MerchandiserSyncConsumer>();
    x.AddConsumer<ReturnOrderSyncConsumer>();
    x.AddConsumer<FileSysProcessConsumer>();
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
        cfg.ReceiveEndpoint(QueueNames.SalesOrderQueue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<SalesOrderSyncConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.Sales_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
        cfg.ReceiveEndpoint(QueueNames.MerchandiserQueue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<MerchandiserSyncConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.Merchandiser_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
        cfg.ReceiveEndpoint(QueueNames.CollectionQueue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(15))); // Retry 3 times with 15-second intervals
            e.ConfigureConsumer<CollectionSyncConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.Collection_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
        cfg.ReceiveEndpoint(QueueNames.MasterQueue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<MasterProcessConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.Master_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
        cfg.ReceiveEndpoint(QueueNames.ReturnOrderQueue, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<ReturnOrderSyncConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.Return_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
        cfg.ReceiveEndpoint(QueueNames.FileSys, e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Durable = true; // Ensure the queue is durable
            e.AutoDelete = false; // Prevent the queue from being deleted automatically
            e.Lazy = true;
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 times with 5-second intervals
            e.ConfigureConsumer<FileSysProcessConsumer>(context);
            e.Bind(ExchangeNames.AppRequestExchange, x =>
            {
                x.ExchangeType = ExchangeTypes.Topic;
                x.RoutingKey = QueueRoute.FileSys_General;
                x.SetExchangeArgument("x-max-length-bytes", 10737418240);
            });
            e.UseConcurrencyLimit(100); // Allow up to 100 messages to be processed concurrently
        });
    });

    #region SalesOrder DI
    builder.Services.AddScoped<ISalesOrderBL, SalesOrderBL>();
    builder.Services.AddScoped<IStoreHistoryDL, PGSQLStoreHistoryDL>();
    builder.Services.AddScoped<ISalesOrderDL, PGSQLSalesOrderDL>();
    builder.Services.AddScoped<ICollectionModuleDL, PGSQLCollectionModuleDL>();
    builder.Services.AddScoped<IStockUpdaterDL, PGSQLStockUpdaterDL>();
    builder.Services.AddScoped<IWHStockAvailable, WHStockAvailable>();
    builder.Services.AddScoped<SalesOrderViewModelDCO>();
    #endregion

    #region Collection DI
    builder.Services.AddScoped<ICollectionMapper, CollectionMapper>();
    builder.Services.AddScoped<ICollectionModuleBL, CollectionModuleBL>();
    builder.Services.AddScoped<ICollectionModuleDL, PGSQLCollectionModuleDL>();
    builder.Services.AddScoped<CollectionDTO>();
    builder.Services.AddScoped<ICollections, Collections>();
    builder.Services.AddScoped<IAccCollection, AccCollection>();
    builder.Services.AddScoped<IAccCollectionPaymentMode, AccCollectionPaymentMode>();
    builder.Services.AddScoped<IAccStoreLedger, AccStoreLedger>();
    builder.Services.AddScoped<IAccCollectionAllotment, AccCollectionAllotment>();
    builder.Services.AddScoped<IAccPayable, AccPayable>();
    builder.Services.AddScoped<IAccReceivable, AccReceivable>();
    #endregion

    #region Master DI
    builder.Services.AddTransient<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>();
    builder.Services.AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL,
        Winit.Modules.JourneyPlan.BL.Classes.BeatHistoryBL>();
    builder.Services.AddTransient<Winit.Modules.JourneyPlan.DL.Interfaces.IBeatHistoryDL,
        Winit.Modules.JourneyPlan.DL.Classes.PGSQLBeatHistoryDL>();
    builder.Services.AddTransient<MasterDTO>();
    builder.Services.AddTransient<IExceptionLog, ExceptionLog>();
    builder.Services.AddTransient<IJobPositionAttendance, JobPositionAttendance>();
    builder.Services.AddTransient<IUserJourney, UserJourney>();
    builder.Services.AddTransient<IStoreHistory, StoreHistory>();
    builder.Services.AddTransient<IStoreHistoryStats, StoreHistoryStats>();
    builder.Services.AddTransient<IBeatHistory, BeatHistory>();
    builder.Services.AddTransient<IStoreActivityHistory, StoreActivityHistory>();
    #endregion
    #region ReturnOrder DI
    builder.Services.AddScoped<IReturnOrderBL, ReturnOrderBL>();
    builder.Services.AddScoped<IReturnOrderDL, PGSQLReturnOrderDL>();
    builder.Services.AddScoped<ReturnOrderMasterDTO>();
    builder.Services.AddScoped<IReturnOrderMaster, ReturnOrderMaster>();
    #endregion

    //Capture Competitor
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor,
            Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>();
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorBL,
            Winit.Modules.CaptureCompetitor.BL.Classes.CaptureCompetitorBL>();
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.DL.Interfaces.ICaptureCompetitorDL,
            Winit.Modules.CaptureCompetitor.DL.Classes.PGSQLCaptureCompetitorDL>();
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.Model.Classes.MerchandiserDTO>();
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandMapping,
            Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandMapping>();
    builder.Services
        .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandCompetitorMapping,
            Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandCompetitorMapping>();


    //Planogram


    builder.Services
        .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetup,
            Winit.Modules.Planogram.Model.Classes.PlanogramSetup>();
    builder.Services
        .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionHeader,
            Winit.Modules.Planogram.Model.Classes.PlanogramExecutionHeader>();
    builder.Services
        .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionDetail,
            Winit.Modules.Planogram.Model.Classes.PlanogramExecutionDetail>();
    
    //farmly
    builder.Services
        .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionV1,
            Winit.Modules.Planogram.Model.Classes.PlanogramExecutionV1>();
    builder.Services.AddTransient<Winit.Modules.ExpiryCheck.Model.Interfaces.IExpiryCheckExecution, Winit.Modules.ExpiryCheck.Model.Classes.ExpiryCheckExecution>();
    builder.Services.AddTransient<Winit.Modules.ExpiryCheck.Model.Interfaces.IExpiryCheckExecutionLine, Winit.Modules.ExpiryCheck.Model.Classes.ExpiryCheckExecutionLine>();
    builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IProductFeedback, Winit.Modules.Merchandiser.Model.Classes.ProductFeedback>();
    builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IBroadcastInitiative, Winit.Modules.Merchandiser.Model.Classes.BroadcastInitiative>();
    builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetupV1, Winit.Modules.Planogram.Model.Classes.PlanogramSetupV1>();
    builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IProductSampling, Winit.Modules.Merchandiser.Model.Classes.ProductSampling>();
    builder.Services.AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecution, Winit.Modules.PO.Model.Classes.POExecution>();
    builder.Services.AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecutionLine, Winit.Modules.PO.Model.Classes.POExecutionLine>();

    //filesys
    builder.Services.AddTransient<Winit.Modules.FileSys.Model.Interfaces.IFileSys, Winit.Modules.FileSys.Model.Classes.FileSys>();
    builder.Services.AddTransient<Winit.Modules.FileSys.BL.Interfaces.IFileSysBL,
        Winit.Modules.FileSys.BL.Classes.FileSysBL>();
    builder.Services.AddTransient<Winit.Modules.FileSys.DL.Interfaces.IFileSysDL,
        Winit.Modules.FileSys.DL.Classes.PGSQLFileSysDL>();

    var host = builder.Build();
    host.Run();
});
