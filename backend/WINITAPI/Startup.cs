global using Microsoft.AspNetCore.Authorization;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using FirebaseNotificationServices.Classes;
//using FirebaseNotificationServices.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using Serilog;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text;
using WINITAPI.Middleware;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.Auth.BL.Classes;
using Winit.Modules.Initiative.BL.Extensions;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.Email.Model.Classes;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Int_CommonMethods.DL.Classes;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using WINITAPI.Controllers.SKU;
using WINITServices.Classes.Email;
using WINITServices.Classes.RabbitMQ;
using WINITAPI.Extensions;

namespace WINITAPI;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _environment = env;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment _environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddCors(options =>
        {
            options.AddPolicy("AllowAny",
                builder =>
                {
                    _ = builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        bool enableHostedService = Configuration.GetValue<bool>("ServiceSettings:EnableHostedService");

        //services.AddControllers()
        //    .AddJsonOptions(options =>
        //    {
        //        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        //        // Include null values during serialization
        //        //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        //    });

        _ = services.AddControllers().AddNewtonsoftJson(options =>
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            InterfaceTypeResolver modelConverter = serviceProvider.GetRequiredService<InterfaceTypeResolver>();
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.Converters.Add(modelConverter);
        });

        //used to Dapper model fileds mapping from snakecase to PascalCase
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        //ramana
        //services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        //{
        //    options.Filters.Add(new Microsoft.AspNetCore.Mvc.RequestFiltering.RequestFilteringAttribute
        //    {
        //        AllowUnlisted = true 
        //    });
        //});
        //_ = services.AddTransient<JsonSerializerOptions>((s) =>
        //{
        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        PropertyNameCaseInsensitive = true,
        //        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //        ReadCommentHandling = JsonCommentHandling.Skip,
        //        AllowTrailingCommas = true,
        //        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        //        ReferenceHandler = null,
        //        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        //        {
        //            Modifiers = { TypeResolvers.GetTypeResolvers(s) }
        //        }
        //    };

        //    options.Converters.Add(new JsonStringEnumConverter());

        //    return options;
        //});


        // Register your services
        _ = services.AddSingleton<InterfaceTypeResolver>(); // Register the converter as a scoped service

        // Register JsonSerializerSettings as a scoped service
        _ = services.AddSingleton(serviceProvider =>
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new DefaultContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters =
                {
                    serviceProvider.GetRequiredService<InterfaceTypeResolver>()
                }
            };

            return settings;
        });
        // Configure Serilog with dynamic configuration from appsettings.json


        // For Memory Caching
        _ = services.AddMemoryCache();

        _ = services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        _ = services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
        });

        //Hosted Services
        //services.AddHostedService<WINITAPI.HostedServices.CacheHostedService>();

        //Singleton
        _ = services.AddSingleton<ConnectionMultiplexer>(provider =>
        {
            try
            {
                ConfigurationOptions configuration = ConfigurationOptions.Parse(Configuration["RedisCache:HostName"]);
                configuration.SyncTimeout = 100000; // 10 seconds
                configuration.AbortOnConnectFail = false; // Allow retries when Redis is unavailable
                configuration.ConnectRetry = 3; // Number of times to retry connecting
                configuration.ConnectTimeout = 5000; // Connection timeout in milliseconds
                return ConnectionMultiplexer.Connect(configuration);
            }
            catch (Exception ex)
            {
                // Log the error but return null to allow the application to continue without Redis
                Log.Warning(ex, "Redis connection failed. Application will continue without caching.");
                return null;
            }
        });

        if (enableHostedService)
        {
            _ = services.AddHostedService<WorkerServices.Classes.SalesOrderWorkerService>();
            _ = services.AddHostedService<WorkerServices.Classes.ReturnOrderWorkerService>();
            _ = services.AddHostedService<WorkerServices.Classes.CollectionWorkerService>();
            _ = services.AddHostedService<WorkerServices.Classes.WHStockWorkerService>();
            _ = services.AddHostedService<WorkerServices.Classes.MasterWorkerService>();
            _ = services.AddHostedService<WorkerServices.Classes.CollectionDepositWorkerService>();
        }

        _ = services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));
        _ = services.Configure<Winit.Shared.Models.Configuration.SyncSettings>(Configuration.GetSection("SyncSettings"));
        _ = services.AddSingleton<Winit.Modules.Store.Model.Classes.ApiSettings>();
        // Configure HttpClient with proper BaseAddress to fix URI errors
        services.AddHttpClient();
        
        // Add named HttpClient for API calls with proper base address
        services.AddHttpClient("ApiClient", client =>
        {
            var request = services.BuildServiceProvider().GetService<IHttpContextAccessor>()?.HttpContext?.Request;
            if (request != null)
            {
                client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
            }
            else
            {
                // Fallback for background services
                client.BaseAddress = new Uri("https://multiplex-promotions.winitsoftware.com");
            }
        });
        _ = _ = services.AddTransient<Winit.Modules.Base.BL.ApiService>();
        _ = _ = services.AddTransient<Winit.Modules.Auth.BL.Interfaces.IAuthBL, Winit.Modules.Auth.BL.Classes.AuthBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Auth.DL.Interfaces.IAuthDL, Winit.Modules.Auth.DL.Classes.PGSQLAuthDL, Winit.Modules.Auth.DL.Classes.MSSQLAuthDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Auth.Model.Interfaces.IUserLogin, Winit.Modules.Auth.Model.Classes.UserLogin>();
        _ = _ = services.AddTransient<Winit.Modules.Auth.Model.Interfaces.IUser, Winit.Modules.Auth.Model.Classes.User>();
        _ = _ = services.AddTransient<WINITAPI.Common.RSAHelperMethods>();
        _ = _ = services.AddTransient<Winit.Shared.CommonUtilities.Common.SHACommonFunctions>();
        //commonfunctions
        _ = _ = services.AddTransient<CommonFunctions>();
        _ = services.AddTransient<OutstandingInvoiceView>();
        //services.AddSingleton<WINITAPI.HostedServices.CacheHostedService>();
        //services.AddSingleton<WINITServices.Interfaces.CacheHandler.ICacheService, WINITServices.Classes.CacheHandler.CacheService>();
        //services.AddSingleton<WINITServices.Interfaces.CacheHandler.ICacheService, WINITServices.Classes.CacheHandler.RedisCacheService>();
        _ = services
            .AddSingleton<WINITServices.Interfaces.CacheHandler.ICacheService,
                WINITServices.Classes.CacheHandler.RedisCacheServiceConnectionMultiplexer>();
        _ = services.AddSingleton<WINITServices.Classes.RabbitMQ.RabbitMQService>();
        _ = services.AddSingleton<WinitService>();

        _ = services
            .AddTransient<Winit.Modules.RabbitMQQueue.BL.Interfaces.IRabbitMQLogBL,
                Winit.Modules.RabbitMQQueue.BL.Classes.RabbitMQLogBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.RabbitMQQueue.DL.Interfaces.IRabbitMQLogDL, Winit.Modules.RabbitMQQueue.DL.Classes.PGSQLRabbitMQLogDL, Winit.Modules.RabbitMQQueue.DL.Classes.MSSQLRabbitMQLogDL>(Configuration);


        //Common
        _ = services
            .AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.ISortHelper,
                Winit.Modules.Base.BL.Helper.Classes.SortHelper>();

        //amit
        _ = services
            .AddTransient<WINITServices.Interfaces.RuleEngine.IRuleEngineService,
                WINITServices.Classes.RuleEngine.RuleEngineService>();
        _ = services
            .AddTransient<WINITRepository.Interfaces.RuleEngine.IRuleEngineRepository,
                WINITRepository.Classes.RuleEngine.PostgreSQLRuleEngineRepository>();
        _ = services.AddSingleton<EmailMessaging>();
        _ = services.AddSingleton<PublisherMQService>();
        _ = services.AddSingleton<SubscriberMQService>();
        //end

        // Initiative Module
        _ = services.AddInitiativeModule();

        ////Scoped
        //_ = services.AddTransient<WINITServices.Interfaces.ICommissionService, WINITServices.Classes.Commission.CommissionService>();
        ////_ = services.AddTransient<WINITRepository.Interfaces.Commission.ICommissionRepository, WINITRepository.Classes.Commission.PostgreSQLCommissionRepository>();
        //_ = services.AddTransient<WINITRepository.Interfaces.Commission.ICommissionRepository, WINITRepository.Classes.Commission.SQLServerCommissionRepository>();

        //Onboard Customer Information
        _ = services
            .AddTransient<Winit.Modules.Contact.Model.Interfaces.IContact,
                Winit.Modules.Contact.Model.Classes.Contact>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRetailingCityMonthlySales,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRetailingCityMonthlySales>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRACSalesByYear,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRACSalesByYear>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview,
                Winit.Modules.Store.Model.Classes.OnBoardGridview>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMI,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMI>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview,
                Winit.Modules.Store.Model.Classes.OnBoardGridview>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardEditCustomerDTO,
                Winit.Modules.Store.Model.Classes.OnBoardEditCustomerDTO>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreAdditionalInfoCMIDL, Winit.Modules.Store.DL.Classes.PGSQLStoreAdditionalInfoCMIDL, Winit.Modules.Store.DL.Classes.MSSQLStoreAdditionalInfoCMIDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoCMIBL,
                Winit.Modules.Store.BL.Classes.StoreAdditionalInfoCMIBL>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRACSalesByYear1,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRACSalesByYear1>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRetailingCityMonthlySales1,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRetailingCityMonthlySales1>();

        _ = services
            .AddTransient<WINITServices.Interfaces.ICustomerService, WINITServices.Classes.Customer.CustomerService>();
        //_ = services.AddTransient<WINITRepository.Interfaces.Customers.ICustomerRepository, WINITRepository.Classes.Customers.PostgreSQLCustomerRepository>();
        _ = services
            .AddTransient<WINITRepository.Interfaces.Customers.ICustomerRepository,
                WINITRepository.Classes.Customers.SQLServerCustomerRepository>();
        //_ = services.AddTransient<WINITRepository.Interfaces.Customers.ICustomerRepository, WINITRepository.Classes.Customers.SqliteCustomerRepository>();
        //  Products
        _ = services
            .AddTransient<WINITServices.Interfaces.IProductService, WINITServices.Classes.Products.ProductService>();
        // _ = services.AddTransient<WINITRepository.Interfaces.Products.IProductRepository, WINITRepository.Classes.Proucts.PostgreSQLProductsRepository>();
        _ = services
            .AddTransient<WINITRepository.Interfaces.Products.IProductRepository,
                WINITRepository.Classes.Proucts.SQLServerProductsRepository>();
        //  _ = services.AddTransient<WINITRepository.Interfaces.Products.IProductRepository, WINITRepository.Classes.Proucts.SqliteProductsRepository>();
        //Dash board
        _ = services
            .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.ICategoryPerformance,
                Winit.Modules.DashBoard.Model.Classes.CategoryPerformance>();
        _ = services
           .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReportAsmview,
               Winit.Modules.DashBoard.Model.Classes.BranchSalesReportAsmview>();
        _ = services
          .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReportOrgview,
              Winit.Modules.DashBoard.Model.Classes.BranchSalesReportOrgview>();

        _ = services
            .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReport,
                Winit.Modules.DashBoard.Model.Classes.BranchSalesReport>();

        _ = services
             .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IDistributorPerformance,
                 Winit.Modules.DashBoard.Model.Classes.DistributorPerformance>();
        _ = services
            .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.ISalesPerformance,
                Winit.Modules.DashBoard.Model.Classes.SalesPerformance>();
        _ = services
            .AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IGrowthWiseChannelPartner,
                Winit.Modules.DashBoard.Model.Classes.GrowthWiseChannelPartner>();
        _ = services
            .AddTransient<Winit.Modules.DashBoard.BL.Interfaces.IDashBoardBL,
                Winit.Modules.DashBoard.BL.Classes.DashBoardBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.DashBoard.DL.Interfaces.IDashBoardDL, Winit.Modules.DashBoard.DL.Classes.PGSQLDashBoardDL, Winit.Modules.DashBoard.DL.Classes.MSSQLDashBoardDL>(Configuration);

        // Target Module Services
        _ = services
            .AddTransient<Winit.Modules.Target.BL.Interfaces.ITargetBL,
                Winit.Modules.Target.BL.Classes.TargetBL>();
        _ = _ = services.AddTransient<Winit.Modules.Target.DL.Interfaces.ITargetDL, Winit.Modules.Target.DL.Classes.PGSQLTargetDL>();
        _ = services
            .AddTransient<Winit.Modules.Target.Model.Interfaces.ITarget,
                Winit.Modules.Target.Model.Classes.Target>();
        _ = services
            .AddTransient<Winit.Modules.Target.Model.Classes.Target>();
        _ = services
            .AddTransient<Winit.Modules.Target.Model.Classes.TargetFilter>();
        _ = services
            .AddTransient<Winit.Modules.Target.Model.Classes.TargetSummary>();
        _ = services
            .AddTransient<Winit.Modules.Target.Model.Classes.TargetUploadRequest>();

        // Store
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStorePromotionMap,
                Winit.Modules.Store.Model.Classes.StorePromotionMap>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreMaster,
                Winit.Modules.Store.Model.Classes.StoreMaster>();
        _ = services
            .AddTransient<Winit.Modules.StoreMaster.Model.Interfaces.IStoreViewModelDCO,
                Winit.Modules.StoreMaster.Model.Classes.StoreViewModelDCO1>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();

        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCustomer,
                Winit.Modules.Store.Model.Classes.StoreCustomer>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreDL, Winit.Modules.Store.DL.Classes.PGSQLStoreDL, Winit.Modules.Store.DL.Classes.MSSQLStoreDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreBL, Winit.Modules.Store.BL.Classes.StoreBL>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreItemView,
                Winit.Modules.Store.Model.Classes.StoreItemView>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCustomer,
                Winit.Modules.Store.Model.Classes.StoreCustomer>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupData,
                Winit.Modules.Store.Model.Classes.StoreGroupData>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreShowroom,
                Winit.Modules.Store.Model.Classes.StoreShowroom>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreSignatory,
                Winit.Modules.Store.Model.Classes.StoreSignatory>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreBrandDealingIn,
                Winit.Modules.Store.Model.Classes.StoreBrandDealingIn>();
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreOrgConfigurationBL,
                Winit.Modules.Store.BL.Classes.StoreOrgConfigurationBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreOrgConfigurationDL, Winit.Modules.Store.DL.Classes.PGSQLStoreOrgConfigurationDL, Winit.Modules.Store.DL.Classes.MSSQLStoreOrgConfigurationDL>(Configuration);



        //storecheck
        _ = services
         .AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView,
             Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView>();
        _ = services
         .AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistory,
             Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory>();
        _ = services
         .AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory,
             Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory>();
        _ = services
      .AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemUomQty,
          Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty>();
        _ = services
      .AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckGroupHistory,
          Winit.Modules.StoreCheck.Model.Classes.StoreCheckGroupHistory>();

        _ = services
     .AddTransient<Winit.Modules.StoreCheck.BL.Interfaces.IStoreCheckBL,
         Winit.Modules.StoreCheck.BL.Classes.StoreCheckBL>();

        _ = _ = services.AddTransient<Winit.Modules.StoreCheck.DL.Interfaces.IStoreCheckDL, Winit.Modules.StoreCheck.DL.Classes.PGSQLStoreCheckDL>();
        //StoreGroupData
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupData,
                Winit.Modules.Store.Model.Classes.StoreGroupData>();
        //approve engine
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalLog,
                Winit.Modules.ApprovalEngine.Model.Classes.ApprovalLog>();
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineBL,
                Winit.Modules.ApprovalEngine.BL.Classes.ApprovalEngineBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.ApprovalEngine.DL.Interfaces.IApprovalEngineDL, Winit.Modules.ApprovalEngine.DL.Classes.PGSQLApprovalEngineDL, Winit.Modules.ApprovalEngine.DL.Classes.MSSQLApprovalEngineDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMaster,
                Winit.Modules.ApprovalEngine.Model.Classes.ApprovalRuleMaster>();
        //Distributor
        _ = services.AddDatabaseProvider<Winit.Modules.Distributor.DL.Interfaces.IDistributorDL, Winit.Modules.Distributor.DL.Classes.PGSQLDIstributorDL, Winit.Modules.Distributor.DL.Classes.MSSQLDIstributorDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Distributor.BL.Interfaces.IDistributorBL,
                Winit.Modules.Distributor.BL.Classes.DistributorBL>();
        _ = services
            .AddTransient<Winit.Modules.Distributor.Model.Interfaces.IDistributor,
                Winit.Modules.Distributor.Model.Classes.Distributor>();
        _ = services
            .AddTransient<Winit.Modules.Distributor.Model.Interfaces.IDistributorAdminDTO,
                Winit.Modules.Distributor.Model.Classes.DistributorAdminDTO>();
        _ = services.AddSingleton<DataPreparationController>();
        //
        _ = services.AddDatabaseProvider<Winit.Modules.CreditLimit.DL.Interfaces.ITemporaryCreditDL, Winit.Modules.CreditLimit.DL.Classes.PGSQLTemporaryCreditDL, Winit.Modules.CreditLimit.DL.Classes.MSSQLTemporaryCreditDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.CreditLimit.BL.Interfaces.ITemporaryCreditBL,
                Winit.Modules.CreditLimit.BL.Classes.TemporaryCreditBL>();
        _ = services
            .AddTransient<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit,
                Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>();
        //Calender
        _ = services.AddDatabaseProvider<Winit.Modules.Calender.DL.Interface.ICalenderDL, Winit.Modules.Calender.DL.Classes.PGSQLCalenderDL, Winit.Modules.Calender.DL.Classes.MSSQLCalenderDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Calender.BL.Interfaces.ICalenderBL,
                Winit.Modules.Calender.BL.Classes.CalenderBL>();
        _ = services
            .AddTransient<Winit.Modules.Calender.Models.Interfaces.ICalender,
                Winit.Modules.Calender.Models.Classes.Calender>();
        //StoreAsmMapping
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAsmMapping,
                Winit.Modules.Store.Model.Classes.StoreAsmMapping>();
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAsmMappingBL,
                Winit.Modules.Store.BL.Classes.StoreAsmMappingBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreAsmMappingDL, Winit.Modules.Store.DL.Classes.PGSQLStoreAsmMappingDL, Winit.Modules.Store.DL.Classes.MSSQLStoreAsmMappingDL>(Configuration);

        //SMS
        _ = services.AddDatabaseProvider<Winit.Modules.SMS.DL.Interfaces.ISMSDL, Winit.Modules.SMS.DL.Classes.PGSQLSMSDL, Winit.Modules.SMS.DL.Classes.MSSQLSMSDL>(Configuration);

        _ = services
            .AddTransient<Winit.Modules.SMS.BL.Interfaces.ISMSBL,
                Winit.Modules.SMS.BL.Classes.SMSBL>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISMSRequest,
                Winit.Modules.SMS.Model.Classes.SMSRequest>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISMSApiResponse,
                Winit.Modules.SMS.Model.Classes.SMSApiResponse>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISMSSubRequest,
                Winit.Modules.SMS.Model.Classes.SMSSubRequest>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISMSSubApiResponse,
                Winit.Modules.SMS.Model.Classes.SMSSubApiResponse>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISmsModel,
                Winit.Modules.SMS.Model.Classes.SmsModel>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISmsModelReceiver,
                Winit.Modules.SMS.Model.Classes.SmsModelReceiver>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISmsRequestModel,
                Winit.Modules.SMS.Model.Classes.SmsRequestModel>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISmsResponseModel,
                Winit.Modules.SMS.Model.Classes.SmsResponseModel>();
        _ = services
            .AddTransient<Winit.Modules.SMS.Model.Interfaces.ISmsTemplateFields,
                Winit.Modules.SMS.Model.Classes.SmsTemplateFields>();


        //Email
        _ = services.AddDatabaseProvider<Winit.Modules.Email.DL.Interfaces.IEmailDL, Winit.Modules.Email.DL.Classes.PGSQLEmailDL, Winit.Modules.Email.DL.Classes.MSSQLEmailDL>(Configuration);

        _ = _ = services.AddTransient<Winit.Modules.Email.DL.UtilityClasses.EmailUtility>();
        _ = _ = services.AddTransient<EmailFromBodyModelDTO>();

        _ = services
            .AddTransient<Winit.Modules.Email.BL.Interfaces.IEmailBL,
                Winit.Modules.Email.BL.Classes.EmailBL>();
        _ = services
            .AddTransient<Winit.Modules.Notification.Model.Interfaces.Email.IMailRequest,
                Winit.Modules.Notification.Model.Classes.Email.MailRequest>();
        _ = services
            .AddTransient<Winit.Modules.Email.Model.Interfaces.IEmailModelReceiver,
                Winit.Modules.Email.Model.Classes.EmailModelReceiver>();
        _ = services
            .AddTransient<Winit.Modules.Email.Model.Interfaces.IEmailRequestModel,
                Winit.Modules.Email.Model.Classes.EmailRequestModel>();

        // StoreAttributes
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAttributes,
                Winit.Modules.Store.Model.Classes.StoreAttributes>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreAttributesDL, Winit.Modules.Store.DL.Classes.PGSQLStoreAttributesDL, Winit.Modules.Store.DL.Classes.MSSQLStoreAttributesDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAttributesBL,
                Winit.Modules.Store.BL.Classes.StoreAttributesBL>();

        // StoreAdditionalInfo
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfo>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreAdditionalInfoDL, Winit.Modules.Store.DL.Classes.PGSQLStoreAdditionalInfoDL, Winit.Modules.Store.DL.Classes.MSSQLStoreAdditionalInfoDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL,
                Winit.Modules.Store.BL.Classes.StoreAdditionalInfoBL>();

        // StoreGroup
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroup,
                Winit.Modules.Store.Model.Classes.StoreGroup>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreGroupDL, Winit.Modules.Store.DL.Classes.PGSQLStoreGroupDL, Winit.Modules.Store.DL.Classes.MSSQLStoreGroupDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreGroupBL,
                Winit.Modules.Store.BL.Classes.StoreGroupBL>();

        // StoreCredit
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCredit,
                Winit.Modules.Store.Model.Classes.StoreCredit>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreCreditDL, Winit.Modules.Store.DL.Classes.PGSQLStoreCreditDL, Winit.Modules.Store.DL.Classes.MSSQLStoreCreditDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreCreditBL,
                Winit.Modules.Store.BL.Classes.StoreCreditBL>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCreditLimit,
                Winit.Modules.Store.Model.Classes.StoreCreditLimit>();
        //PurchaseOrderCreditLimitBufferRange
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IPurchaseOrderCreditLimitBufferRange,
                Winit.Modules.Store.Model.Classes.PurchaseOrderCreditLimitBufferRange>();

        // StoreToGroupMapping
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreToGroupMapping,
                Winit.Modules.Store.Model.Classes.StoreToGroupMapping>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreToGroupMappingDL, Winit.Modules.Store.DL.Classes.PGSQLStoreToGroupMappingDL, Winit.Modules.Store.DL.Classes.MSSQLStoreToGroupMappingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreToGroupMappingBL,
                Winit.Modules.Store.BL.Classes.StoreToGroupMappingBL>();

        //SelfRegistration
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.ISelfRegistration,
                Winit.Modules.Store.Model.Classes.SelfRegistration>();
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.ISelfRegistrationBL,
                Winit.Modules.Store.BL.Classes.SelfRegistrationBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.ISelfRegistrationDL, Winit.Modules.Store.DL.Classes.PGSQLSelfRegistrationDL, Winit.Modules.Store.DL.Classes.MSSQLSelfRegistrationDL>(Configuration);

        //StoreGroupType
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupType,
                Winit.Modules.Store.Model.Classes.StoreGroupType>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreGroupTypeDL, Winit.Modules.Store.DL.Classes.PGSQLStoreGroupTypeDL, Winit.Modules.Store.DL.Classes.MSSQLStoreGroupTypeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeBL,
                Winit.Modules.Store.BL.Classes.StoreGroupTypeBL>();

        //StoreSpecialDay
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreSpecialDay,
                Winit.Modules.Store.Model.Classes.StoreSpecialDay>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreSpecialDayDL, Winit.Modules.Store.DL.Classes.PGSQLStoreSpecialDayDL, Winit.Modules.Store.DL.Classes.MSSQLStoreSpecialDayDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreSpecialDayBL,
                Winit.Modules.Store.BL.Classes.StoreSpecialDayBL>();

        //StoreHistory
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreHistory,
                Winit.Modules.Store.Model.Classes.StoreHistory>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreHistoryDL, Winit.Modules.Store.DL.Classes.PGSQLStoreHistoryDL, Winit.Modules.Store.DL.Classes.MSSQLStoreHistoryDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreHistoryBL,
                Winit.Modules.Store.BL.Classes.StoreHistoryBL>();

        //BroadClassification
        _ = services
            .AddTransient<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader,
                Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader>();
        _ = services
            .AddTransient<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine,
                Winit.Modules.BroadClassification.Model.Classes.BroadClassificationLine>();
        _ = services.AddDatabaseProvider<Winit.Modules.BroadClassification.DL.Interfaces.IBroadClassificationHeaderDL, Winit.Modules.BroadClassification.DL.Classes.PGSQLBroadClassificationHeaderDL, Winit.Modules.BroadClassification.DL.Classes.MSSQLBroadClassificationHeaderDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.BroadClassification.DL.Interfaces.IBroadClassificationLineDL, Winit.Modules.BroadClassification.DL.Classes.PGSQLBroadClassificationLineDL, Winit.Modules.BroadClassification.DL.Classes.MSSQLBroadClassificationLineDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationHeaderBL,
                Winit.Modules.BroadClassification.BL.Classes.BroadClassificationHeaderBL>();
        _ = services
            .AddTransient<Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationLineBL,
                Winit.Modules.BroadClassification.BL.Classes.BroadClassificationLineBL>();

        //Bank
        _ = _ = services.AddTransient<Winit.Modules.Bank.Model.Interfaces.IBank, Winit.Modules.Bank.Model.Classes.Bank>();
        _ = services.AddDatabaseProvider<Winit.Modules.Bank.DL.Interfaces.IBankDL, Winit.Modules.Bank.DL.Classes.PGSQLBankDL, Winit.Modules.Bank.DL.Classes.MSSQLBankDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Bank.BL.Interfaces.IBankBL, Winit.Modules.Bank.BL.Classes.BankBL>();

        //Survey
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurvey, Winit.Modules.Survey.Model.Classes.Survey>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveyResponse, Winit.Modules.Survey.Model.Classes.SurveyResponse>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel, Winit.Modules.Survey.Model.Classes.SurveyResponseModel>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveySection, Winit.Modules.Survey.Model.Classes.SurveySection>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.IActivityModule, Winit.Modules.Survey.Model.Classes.ActivityModule>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseViewDTO, Winit.Modules.Survey.Model.Classes.SurveyResponseViewDTO>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse, Winit.Modules.Survey.Model.Classes.ViewSurveyResponse>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponseExport, Winit.Modules.Survey.Model.Classes.ViewSurveyResponseExport>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.IServeyQuestions, Winit.Modules.Survey.Model.Classes.ServeyQuestions>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency, Winit.Modules.Survey.Model.Classes.StoreQuestionFrequency>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.DL.Interfaces.ISurveyDL, Winit.Modules.Survey.DL.Classes.PGSQLSurveyDL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.DL.Interfaces.ISurveyResponseDL, Winit.Modules.Survey.DL.Classes.PGSQLSurveyResponseDL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.DL.Interfaces.IActivityModuleDL, Winit.Modules.Survey.DL.Classes.PGSQLActivityModuleDL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.DL.Interfaces.IStoreandUserReportsDL, Winit.Modules.Survey.DL.Classes.PGSQLStoreandUserReportsDL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.BL.Interfaces.ISurveyBL, Winit.Modules.Survey.BL.Classes.SurveyBL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.BL.Interfaces.ISurveyResponseBL, Winit.Modules.Survey.BL.Classes.SurveyResponseBL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.BL.Interfaces.IActivityModuleBL, Winit.Modules.Survey.BL.Classes.ActivityModuleBL>();
        _ = _ = services.AddTransient<Winit.Modules.Survey.BL.Interfaces.IStoreandUserReportsBL, Winit.Modules.Survey.BL.Classes.StoreandUserReportsBL>();

        //TASK MANAGEMENT
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.Interfaces.ITask, Winit.Modules.Task.Model.Classes.Task>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.Interfaces.ITaskType, Winit.Modules.Task.Model.Classes.TaskType>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.Interfaces.ITaskSubType, Winit.Modules.Task.Model.Classes.TaskSubType>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.Interfaces.ITaskAssignment, Winit.Modules.Task.Model.Classes.TaskAssignment>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.DTOs.TaskDTO>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.DTOs.TaskAssignmentDTO>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.DTOs.TaskFilterRequest>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.DTOs.CreateTaskRequest>();
        _ = _ = services.AddTransient<Winit.Modules.Task.Model.DTOs.AssignTaskRequest>();
        _ = _ = services.AddTransient<Winit.Modules.Task.DL.Interfaces.ITaskDL, Winit.Modules.Task.DL.Classes.PGSQLTaskDL>();
        _ = _ = services.AddTransient<Winit.Modules.Task.BL.Interfaces.ITaskBL, Winit.Modules.Task.BL.Classes.TaskBL>();

        //PROVISIONING
        _ = services
            .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView,
                Winit.Modules.Provisioning.Model.Classes.ProvisionHeaderView>();
        _ = services
            .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView,
                Winit.Modules.Provisioning.Model.Classes.ProvisionItemView>();

        _ = services
            .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionApproval,
                Winit.Modules.Provisioning.Model.Classes.ProvisionApproval>();

        _ = services.AddDatabaseProvider<Winit.Modules.Provisioning.DL.Interfaces.IProvisioningHeaderViewDL, Winit.Modules.Provisioning.DL.Classes.PGSQLProvisioningHeaderViewDL, Winit.Modules.Provisioning.DL.Classes.MSSQLProvisioningHeaderViewDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Provisioning.DL.Interfaces.IProvisioningItemViewDL, Winit.Modules.Provisioning.DL.Classes.PGSQLProvisioningItemViewDL, Winit.Modules.Provisioning.DL.Classes.MSSQLProvisioningItemViewDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Provisioning.BL.Interfaces.IProvisioningHeaderViewBL,
                Winit.Modules.Provisioning.BL.Classes.ProvisioningHeaderViewBL>();
        _ = services
            .AddTransient<Winit.Modules.Provisioning.BL.Interfaces.IProvisioningItemViewBL,
                Winit.Modules.Provisioning.BL.Classes.ProvisioningItemViewBL>();

        //AsmDivisionMapping
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping,
                Winit.Modules.Store.Model.Classes.AsmDivisionMapping>();


        //Holiday
        _ = services
            .AddTransient<Winit.Modules.Holiday.Model.Interfaces.IHoliday,
                Winit.Modules.Holiday.Model.Classes.Holiday>();
        _ = services.AddDatabaseProvider<Winit.Modules.Holiday.DL.Interfaces.IHolidayDL, Winit.Modules.Holiday.DL.Classes.PGSQLHolidayDL, Winit.Modules.Holiday.DL.Classes.MSSQLHolidayDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Holiday.BL.Interfaces.IHolidayBL, Winit.Modules.Holiday.BL.Classes.HolidayBL>();

        //HolidayList
        _ = services
            .AddTransient<Winit.Modules.Holiday.Model.Interfaces.IHolidayList,
                Winit.Modules.Holiday.Model.Classes.HolidayList>();
        _ = services.AddDatabaseProvider<Winit.Modules.Holiday.DL.Interfaces.IHolidayListDL, Winit.Modules.Holiday.DL.Classes.PGSQLHolidayListDL, Winit.Modules.Holiday.DL.Classes.MSSQLHolidayListDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Holiday.BL.Interfaces.IHolidayListBL,
                Winit.Modules.Holiday.BL.Classes.HolidayListBL>();

        //HolidayListRole
        _ = services
            .AddTransient<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole,
                Winit.Modules.Holiday.Model.Classes.HolidayListRole>();
        _ = services.AddDatabaseProvider<Winit.Modules.Holiday.DL.Interfaces.IHolidayListRoleDL, Winit.Modules.Holiday.DL.Classes.PGSQLHolidayListRoleDL, Winit.Modules.Holiday.DL.Classes.MSSQLHolidayListRoleDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Holiday.BL.Interfaces.IHolidayListRoleBL,
                Winit.Modules.Holiday.BL.Classes.HolidayListRoleBL>();

        //Setting
        _ = services
            .AddTransient<Winit.Modules.Setting.Model.Interfaces.ISetting,
                Winit.Modules.Setting.Model.Classes.Setting>();
        _ = _ = services.AddTransient<Winit.Modules.Setting.DL.Interfaces.ISettingDL, Winit.Modules.Setting.DL.Classes.PGSQLSettingDL>();
        _ = services
            .AddTransient<Winit.Modules.Setting.BL.Interfaces.ISettingBL, Winit.Modules.Setting.BL.Classes.SettingBL>();

        //PurchangeOrderHeader
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeaderItem>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderHeaderBL,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderHeaderBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.PurchaseOrder.DL.Interfaces.IPurchaseOrderHeaderDL, Winit.Modules.PurchaseOrder.DL.Classes.PGSQLPurchaseOrderHeaderDL, Winit.Modules.PurchaseOrder.DL.Classes.MSSQLPurchaseOrderHeaderDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderMaster,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderMaster>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLine,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLine>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeader,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeader>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineQPS,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLineQPS>();
        _ = services.AddDatabaseProvider<Winit.Modules.PurchaseOrder.DL.Interfaces.IPurchaseOrderLineDL, Winit.Modules.PurchaseOrder.DL.Classes.PGSQLPurchaseOrderLineDL, Winit.Modules.PurchaseOrder.DL.Classes.MSSQLPurchaseOrderLineDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderLineBL,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderLineBL>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderApprovalDTO>();


        //PurchaseOrderTemplate
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateMaster,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateMaster>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateHeader,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateHeader>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateLine,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateLine>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateHeaderBL,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderTemplateHeaderBL>();
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateLineBL,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderTemplateLineBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.PurchaseOrder.DL.Interfaces.IPurchaseOrderTemplateLineDL, Winit.Modules.PurchaseOrder.DL.Classes.PGSQLPurchaseOrderTemplateLineDL, Winit.Modules.PurchaseOrder.DL.Classes.MSSQLPurchaseOrderTemplateLineDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.PurchaseOrder.DL.Interfaces.IPurchaseOrderTemplateHeaderDL, Winit.Modules.PurchaseOrder.DL.Classes.PGSQLPurchaseOrderTemplateHeaderDL, Winit.Modules.PurchaseOrder.DL.Classes.MSSQLPurchaseOrderTemplateHeaderDL>(Configuration);
        //purchase Order Line Provision 

        _ = services.AddDatabaseProvider<Winit.Modules.PurchaseOrder.DL.Interfaces.IPurchaseOrderLineProvisionDL, Winit.Modules.PurchaseOrder.DL.Classes.PGSQLPurchaseOrderLineProvisionDL, Winit.Modules.PurchaseOrder.DL.Classes.MSSQLPurchaseOrderLineProvisionDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineProvision,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLineProvision>();

        // Org
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IFOCStockItem,
                Winit.Modules.Org.Model.Classes.FOCStockItem>();
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView,
                Winit.Modules.Org.Model.Classes.WarehouseStockItemView>();
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IOrgType, Winit.Modules.Org.Model.Classes.OrgType>();
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView,
                Winit.Modules.Org.Model.Classes.WarehouseItemView>();
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IEditWareHouseItemView,
                Winit.Modules.Org.Model.Classes.EditWareHouseItemView>();
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IWareHouseStock,
                Winit.Modules.Org.Model.Classes.WareHouseStock>();
        ; // Org
        _ = _ = services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrg, Winit.Modules.Org.Model.Classes.Org>();
        _ = services.AddDatabaseProvider<Winit.Modules.Org.DL.Interfaces.IOrgDL, Winit.Modules.Org.DL.Classes.PGSQLOrgDL, Winit.Modules.Org.DL.Classes.MSSQLOrgDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Org.BL.Interfaces.IOrgBL, Winit.Modules.Org.BL.Classes.OrgBL>();
        //OrgHeirarchy
        _ = services
            .AddTransient<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy,
                Winit.Modules.Org.Model.Classes.OrgHeirarchy>();
        // Currency
        _ = services
            .AddTransient<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency,
                Winit.Modules.Currency.Model.Classes.OrgCurrency>();
        _ = services
            .AddTransient<Winit.Modules.Currency.Model.Interfaces.ICurrency,
                Winit.Modules.Currency.Model.Classes.Currency>();
        _ = _ = services.AddTransient<Winit.Modules.Currency.DL.Interfaces.ICurrencyDL, Winit.Modules.Currency.DL.Classes.PGSQLCurrencyDL>();
        _ = services
            .AddTransient<Winit.Modules.Currency.BL.Interfaces.ICurrencyBL,
                Winit.Modules.Currency.BL.Classes.CurrencyBL>();
        // Product
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProduct,
                Winit.Modules.Product.Model.Classes.Product>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductDL, Winit.Modules.Product.DL.Classes.PGSQLProductDL, Winit.Modules.Product.DL.Classes.MSSQLProductDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductBL, Winit.Modules.Product.BL.Classes.ProductBL>();

        // ProductConfig
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductConfig,
                Winit.Modules.Product.Model.Classes.ProductConfig>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductConfigDL, Winit.Modules.Product.DL.Classes.PGSQLProductConfigDL, Winit.Modules.Product.DL.Classes.MSSQLProductConfigDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductConfigBL,
                Winit.Modules.Product.BL.Classes.ProductConfigBL>();

        // ProductTypeBridge
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductTypeBridge,
                Winit.Modules.Product.Model.Classes.ProductTypeBridge>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductTypeBridgeDL, Winit.Modules.Product.DL.Classes.PGSQLProductTypeBridgeDL, Winit.Modules.Product.DL.Classes.MSSQLProductTypeBridgeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductTypeBridgeBL,
                Winit.Modules.Product.BL.Classes.ProductTypeBridgeBL>();
        // ProductAttributes
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductAttributes,
                Winit.Modules.Product.Model.Classes.ProductAttributes>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductAttributesDL, Winit.Modules.Product.DL.Classes.PGSQLProductAttributesDL, Winit.Modules.Product.DL.Classes.MSSQLProductAttributesDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductAttributesBL,
                Winit.Modules.Product.BL.Classes.ProductAttributesBL>();

        // ProductType
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductType,
                Winit.Modules.Product.Model.Classes.ProductType>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductTypeDL, Winit.Modules.Product.DL.Classes.PGSQLProductTypeDL, Winit.Modules.Product.DL.Classes.MSSQLProductTypeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductTypeBL,
                Winit.Modules.Product.BL.Classes.ProductTypeBL>();

        // Survey
        _ = services
            .AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurvey,
                Winit.Modules.Survey.Model.Classes.Survey>();
        _ = services
            .AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel,
                Winit.Modules.Survey.Model.Classes.SurveyResponseModel>();
        _ = services
            .AddTransient<Winit.Modules.Survey.Model.Interfaces.ISurveySection,
                Winit.Modules.Survey.Model.Classes.SurveySection>();

        // Emp
        _ = _ = services.AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmp, Winit.Modules.Emp.Model.Classes.Emp>();
        _ = services
            .AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmpView, Winit.Modules.Emp.Model.Classes.EmpView>();
        _ = services.AddDatabaseProvider<Winit.Modules.Emp.DL.Interfaces.IEmpDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Emp.BL.Interfaces.IEmpBL, Winit.Modules.Emp.BL.Classes.EmpBL>();
        _ = services
            .AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmpPassword,
                Winit.Modules.Emp.Model.Classes.EmpPassword>();

        // EmpInfo
        _ = services
            .AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmpInfo, Winit.Modules.Emp.Model.Classes.EmpInfo>();
        _ = services.AddDatabaseProvider<Winit.Modules.Emp.DL.Interfaces.IEmpInfoDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpInfoDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpInfoDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Emp.BL.Interfaces.IEmpInfoBL, Winit.Modules.Emp.BL.Classes.EmpInfoBL>();
        //EmpOrgMapping
        _ = services
            .AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping,
                Winit.Modules.Emp.Model.Classes.EmpOrgMapping>();
        _ = services.AddDatabaseProvider<Winit.Modules.Emp.DL.Interfaces.IEmpOrgMappingDL, Winit.Modules.Emp.DL.Classes.PGSQLEmpOrgMappingDL, Winit.Modules.Emp.DL.Classes.MSSQLEmpOrgMappingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Emp.BL.Interfaces.IEmpOrgMappingBL,
                Winit.Modules.Emp.BL.Classes.EmpOrgMappingBL>();
        //LocationData
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationData,
                Winit.Modules.Location.Model.Classes.LocationData>();
        // Location
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocation,
                Winit.Modules.Location.Model.Classes.Location>();
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationData,
                Winit.Modules.Location.Model.Classes.LocationData>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ILocationDL, Winit.Modules.Location.DL.Classes.PGSQLLocationDL, Winit.Modules.Location.DL.Classes.MSSQLLocationDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationBL,
                Winit.Modules.Location.BL.Classes.LocationBL>();

        //cityBranch
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ICityBranch,
                Winit.Modules.Location.Model.Classes.CityBranch>();
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping,
                Winit.Modules.Location.Model.Classes.CityBranchMapping>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ICityBranchMappingDL, Winit.Modules.Location.DL.Classes.PGSQLCityBranchMappingDL, Winit.Modules.Location.DL.Classes.MSSQLCityBranchMappingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ICityBranchMappingBL,
                Winit.Modules.Location.BL.Classes.CityBranchMappingBL>();
        //branch
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.IBranch,
                Winit.Modules.Location.Model.Classes.Branch>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.IBranchDL, Winit.Modules.Location.DL.Classes.PGSQLBranchDL, Winit.Modules.Location.DL.Classes.MSSQLBranchDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.IBranchBL, Winit.Modules.Location.BL.Classes.BranchBL>();
        //salesoffice

        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ISalesOffice,
                Winit.Modules.Location.Model.Classes.SalesOffice>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ISalesOfficeDL, Winit.Modules.Location.DL.Classes.PGSQLSalesOfficeDL, Winit.Modules.Location.DL.Classes.MSSQLSalesOfficeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ISalesOfficeBL,
                Winit.Modules.Location.BL.Classes.SalesOfficeBL>();


        // LocationMapping
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationMapping,
                Winit.Modules.Location.Model.Classes.LocationMapping>();
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationHierarchy,
                Winit.Modules.Location.Model.Classes.LocationHierarchy>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ILocationMappingDL, Winit.Modules.Location.DL.Classes.PGSQLLocationMappingDL, Winit.Modules.Location.DL.Classes.MSSQLLocationMappingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationMappingBL,
                Winit.Modules.Location.BL.Classes.LocationMappingBL>();

        // LocationType
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationType,
                Winit.Modules.Location.Model.Classes.LocationType>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ILocationTypeDL, Winit.Modules.Location.DL.Classes.PGSQLLocationTypeDL, Winit.Modules.Location.DL.Classes.MSSQLLocationTypeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationTypeBL,
                Winit.Modules.Location.BL.Classes.LocationTypeBL>();

        //LocationTemplate
        _ = services
            .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationTemplateBL,
                Winit.Modules.Location.BL.Classes.LocationTemplateBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Location.DL.Interfaces.ILocationTemplateDL, Winit.Modules.Location.DL.Classes.PGSQLLocationTemplateDL, Winit.Modules.Location.DL.Classes.MSSQLLocationTemplateDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationTemplate,
                Winit.Modules.Location.Model.Classes.LocationTemplate>();
        _ = services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationTemplateLine,
                Winit.Modules.Location.Model.Classes.LocationTemplateLine>();

        // Vehicle
        _ = services
            .AddTransient<Winit.Modules.Vehicle.Model.Interfaces.IVehicle,
                Winit.Modules.Vehicle.Model.Classes.Vehicle>();
        _ = services
            .AddTransient<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus,
                Winit.Modules.Vehicle.Model.Classes.VehicleStatus>();
        _ = services.AddDatabaseProvider<Winit.Modules.Vehicle.DL.Interfaces.IVehicleDL, Winit.Modules.Vehicle.DL.Classes.PGSQLVehicleDL, Winit.Modules.Vehicle.DL.Classes.MSSQLVehicleDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Vehicle.BL.Interfaces.IVehicleBL, Winit.Modules.Vehicle.BL.Classes.VehicleBL>();

        //RouteUser
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteUser,
                Winit.Modules.Route.Model.Classes.RouteUser>();
        _ = services.AddDatabaseProvider<Winit.Modules.Route.DL.Interfaces.IRouteUserDL, Winit.Modules.Route.DL.Classes.PGSQLRouteUserDL, Winit.Modules.Route.DL.Classes.MSSQLRouteUserDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteUserBL, Winit.Modules.Route.BL.Classes.RouteUserBL>();

        //SKUPrice

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPrice, Winit.Modules.SKU.Model.Classes.SKUPrice>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUPriceDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUPriceDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUPriceDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUPriceBL, Winit.Modules.SKU.BL.Classes.SKUPriceBL>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView,
                Winit.Modules.SKU.Model.Classes.SKUPriceView>();

        //SKUPriceLadder
        _ = services
            .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData,
                Winit.Modules.PriceLadder.Model.Classes.SKUPriceLadderingData>();
        _ = services
            .AddTransient<Winit.Modules.PriceLadder.BL.Interfaces.ISKUPriceLadderingBL,
                Winit.Modules.PriceLadder.BL.Classes.SKUPriceLadderingBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.PriceLadder.DL.Interfaces.ISkuPriceLadderingDL, Winit.Modules.PriceLadder.DL.Classes.PGSQLSKUPriceLadderingDL, Winit.Modules.PriceLadder.DL.Classes.MSSQLSKUPriceLadderingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLadderingItemView,
                Winit.Modules.PriceLadder.Model.Classes.PriceLadderingItemView>();
        _ = services
            .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering,
                Winit.Modules.PriceLadder.Model.Classes.PriceLaddering>();

        //SKUTemplate
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate,
                Winit.Modules.SKU.Model.Classes.SKUTemplate>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUTemplateDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUTemplateDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUTemplateDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUTemplateBL, Winit.Modules.SKU.BL.Classes.SKUTemplateBL>();
        //SKUTemplateLine
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine,
                Winit.Modules.SKU.Model.Classes.SKUTemplateLine>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUTemplateLineDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUTemplateLineDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUTemplateLineDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUTemplateLineBL,
                Winit.Modules.SKU.BL.Classes.SKUTemplateLineBL>();

        //SKUGroup
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroup, Winit.Modules.SKU.Model.Classes.SKUGroup>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView,
                Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUGroupDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUGroupDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUGroupDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUGroupBL, Winit.Modules.SKU.BL.Classes.SKUGroupBL>();

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupData,
                Winit.Modules.SKU.Model.Classes.SKUGroupData>();
        //HierarchyMapping
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.IGroupHierarchy,
                Winit.Modules.SKU.Model.Classes.GroupHierarchy>();

        //SKUGroupType
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView,
                Winit.Modules.SKU.Model.Classes.SKUGroupView>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView,
                Winit.Modules.SKU.Model.Classes.SKUGroupTypeView>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType,
                Winit.Modules.SKU.Model.Classes.SKUGroupType>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUAttributeLevel,
                Winit.Modules.SKU.Model.Classes.SKUAttributeLevel>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUGroupTypeDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUGroupTypeDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUGroupTypeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeBL,
                Winit.Modules.SKU.BL.Classes.SKUGroupTypeBL>();

        //SKUToGroupMapping
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping,
                Winit.Modules.SKU.Model.Classes.SKUToGroupMapping>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUToGroupMappingDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUToGroupMappingDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUToGroupMappingDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUToGroupMappingBL,
                Winit.Modules.SKU.BL.Classes.SKUToGroupMappingBL>();

        //SKUPriceList
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.IBuyPrice, Winit.Modules.SKU.Model.Classes.BuyPrice>();
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList,
                Winit.Modules.SKU.Model.Classes.SKUPriceList>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUPriceListDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUPriceListDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUPriceListDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUPriceListBL,
                Winit.Modules.SKU.BL.Classes.SKUPriceListBL>();
        //Route
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog,
                Winit.Modules.Route.Model.Classes.RouteChangeLog>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleDaywise,
                Winit.Modules.Route.Model.Classes.RouteScheduleDaywise>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleFortnight,
                Winit.Modules.Route.Model.Classes.RouteScheduleFortnight>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteMasterView,
                Winit.Modules.Route.Model.Classes.RouteMasterView>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteSchedule,
                Winit.Modules.Route.Model.Classes.RouteSchedule>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleConfig,
                Winit.Modules.Route.Model.Classes.RouteScheduleConfig>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleCustomerMapping,
                Winit.Modules.Route.Model.Classes.RouteScheduleCustomerMapping>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRoute, Winit.Modules.Route.Model.Classes.Route>();
        _ = services.AddDatabaseProvider<Winit.Modules.Route.DL.Interfaces.IRouteDL, Winit.Modules.Route.DL.Classes.PGSQLRoute, Winit.Modules.Route.DL.Classes.MSSQLRoute>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteBL, Winit.Modules.Route.BL.Classes.RouteBL>();

        //RouteCustomer
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteCustomer,
                Winit.Modules.Route.Model.Classes.RouteCustomer>();
        _ = services.AddDatabaseProvider<Winit.Modules.Route.DL.Interfaces.IRouteCustomerDL, Winit.Modules.Route.DL.Classes.PGSQLRouteCustomer, Winit.Modules.Route.DL.Classes.MSSQLRouteCustomer>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteCustomerBL,
                Winit.Modules.Route.BL.Classes.RouteCustomerBL>();
        //RouteLoadTruckTemplate
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateView,
                Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateView>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplateLine,
                Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateLine>();
        _ = services
            .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteLoadTruckTemplate,
                Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplate>();
        _ = services.AddDatabaseProvider<Winit.Modules.Route.DL.Interfaces.IRouteLoadTruckTemplateDL, Winit.Modules.Route.DL.Classes.PGSQLRouteLoadTruckTemplate, Winit.Modules.Route.DL.Classes.MSSQLRouteLoadTruckTemplate>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteLoadTruckTemplateBL,
                Winit.Modules.Route.BL.Classes.RouteLoadTruckTemplateBL>();

        //JobPosition
        _ = services
            .AddTransient<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition,
                Winit.Modules.JobPosition.Model.Classes.JobPosition>();
        _ = services
            .AddTransient<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance,
                Winit.Modules.JobPosition.Model.Classes.JobPositionAttendance>();
        _ = services.AddDatabaseProvider<Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL, Winit.Modules.JobPosition.DL.Classes.PGSQLJobPositionDL, Winit.Modules.JobPosition.DL.Classes.MSSQLJobPositionDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL,
                Winit.Modules.JobPosition.BL.Classes.JobPositionBL>();

        //Tally
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySKU, Winit.Modules.Tally.Model.Classes.TallySKU>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySKUMapping,
                Winit.Modules.Tally.Model.Classes.TallySKUMapping>();
        _ = services
            .AddTransient<Winit.Modules.Tally.BL.Interfaces.ITallyMappingBL,
                Winit.Modules.Tally.BL.Classes.TallyMappingBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Tally.DL.Interfaces.ITallyMappingDL, Winit.Modules.Tally.DL.Classes.PGSQLTallyMappingDL, Winit.Modules.Tally.DL.Classes.MSSQLTallyMappingDL>(Configuration);

        //Tally Master
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster,
                Winit.Modules.Tally.Model.Classes.TallyDealerMaster>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster,
                Winit.Modules.Tally.Model.Classes.TallyInventoryMaster>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster,
                Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster,
                Winit.Modules.Tally.Model.Classes.TallySalesInvoiceLineMaster>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceResult,
                Winit.Modules.Tally.Model.Classes.TallySalesInvoiceResult>();
        _ = services
            .AddTransient<Winit.Modules.Tally.BL.Interfaces.ITallyMasterBL,
                Winit.Modules.Tally.BL.Classes.TallyMasterBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Tally.DL.Interfaces.ITallyMasterDL, Winit.Modules.Tally.DL.Classes.PGSQLTallyMasterDL, Winit.Modules.Tally.DL.Classes.MSSQLTallyMasterDL>(Configuration);

        //TallyIntegration
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyConfigurationResponse,
                Winit.Modules.Tally.Model.Classes.TallyConfigurationResponse>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyConfiguration,
                Winit.Modules.Tally.Model.Classes.TallyConfiguration>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITaxConfiguration,
                Winit.Modules.Tally.Model.Classes.TaxConfiguration>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.IRetailersFromTally,
                Winit.Modules.Tally.Model.Classes.RetailersFromTally>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.IInventoryFromTally,
                Winit.Modules.Tally.Model.Classes.InventoryFromTally>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ISalesOrderHeaderFromTally,
                Winit.Modules.Tally.Model.Classes.SalesOrderHeaderFromTally>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ISalesOrderLineFromTally,
                Winit.Modules.Tally.Model.Classes.SalesOrderLineFromTally>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.IRetailersFromDB,
                Winit.Modules.Tally.Model.Classes.RetailersFromDB>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.IRetailerTallyStatus,
                Winit.Modules.Tally.Model.Classes.RetailerTallyStatus>();
        _ = services
            .AddTransient<Winit.Modules.Tally.Model.Interfaces.ISalesTallyStatus,
                Winit.Modules.Tally.Model.Classes.SalesTallyStatus>();


        //Collection Module
        //ACCPayableCMI
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableMaster,
                Winit.Modules.CollectionModule.Model.Classes.AccPayableMaster>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable,
                Winit.Modules.CollectionModule.Model.Classes.AccPayableView>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableView,
                Winit.Modules.CollectionModule.Model.Classes.AccPayableView>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI,
                Winit.Modules.CollectionModule.Model.Classes.AccPayableCMI>();
        _ = services.AddDatabaseProvider<Winit.Modules.CollectionModule.DL.Interfaces.IAccPayableCMIDL, Winit.Modules.CollectionModule.DL.Classes.PGSQLAccPayableCMIDL, Winit.Modules.CollectionModule.DL.Classes.MSSQLAccPayableCMIDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.IAccPayableCMIBL,
                Winit.Modules.CollectionModule.BL.Classes.AccPayableCMIBL>();

        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection,
                Winit.Modules.CollectionModule.Model.Classes.AccCollection>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionAmount,
                Winit.Modules.CollectionModule.Model.Classes.CollectionAmount>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IExchangeRate,
                Winit.Modules.CollectionModule.Model.Classes.ExchangeRate>();
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionAllotment>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger,
                Winit.Modules.CollectionModule.Model.Classes.AccStoreLedger>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable,
                Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccReceivable,
                Winit.Modules.CollectionModule.Model.Classes.AccReceivable>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionSettlement,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionSettlement>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionSettlementReceipts,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionSettlementReceipts>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollections,
                Winit.Modules.CollectionModule.Model.Classes.Collections>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccBank,
                Winit.Modules.CollectionModule.Model.Classes.AccBank>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccUser,
                Winit.Modules.CollectionModule.Model.Classes.AccUser>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule,
                Winit.Modules.CollectionModule.Model.Classes.CollectionModule>();
        _ = services.AddDatabaseProvider<Winit.Modules.CollectionModule.DL.Interfaces.ICollectionModuleDL, Winit.Modules.CollectionModule.DL.Classes.PGSQLCollectionModuleDL, Winit.Modules.CollectionModule.DL.Classes.MSSQLCollectionModuleDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL,
                Winit.Modules.CollectionModule.BL.Classes.CollectionModuleBL>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IEarlyPaymentDiscountConfiguration,
                Winit.Modules.CollectionModule.Model.Classes.EarlyPaymentDiscountConfiguration>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IEarlyPaymentDiscountAppliedDetails,
                Winit.Modules.CollectionModule.Model.Classes.EarlyPaymentDiscountAppliedDetails>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionCurrencyDetails,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionCurrencyDetails>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionDeposit,
                Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IStoreStatement,
                Winit.Modules.CollectionModule.Model.Classes.StoreStatement>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IBalanceConfirmation,
                Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation>();
        _ = services
            .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IBalanceConfirmationLine,
                Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmationLine>();


        _ = services.AddSingleton<DBServices.Interfaces.IDBService, DBServices.Classes.DBService>();
        if (IsRabbitMQServerAvailable())
        {
            _ = services
                .AddSingleton<RabbitMQService.Interfaces.IRabbitMQService, RabbitMQService.Classes.RabbitMQService>();
        }

        //services.AddSingleton<RabbitMQService.Interfaces.IRabbitMQService, RabbitMQService.Classes.RabbitMQService>();
        _ = services
            .AddSingleton<FirebaseNotificationServices.Interfaces.IFirebaseNotificationService,
                FirebaseNotificationServices.Classes.FirebaseNotificationService>();

        //SKUClass
        _ = services
            .AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClass,
                Winit.Modules.SKUClass.Model.Classes.SKUClass>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKUClass.DL.Interfaces.ISKUClassDL, Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassDL, Winit.Modules.SKUClass.DL.Classes.MSSQLSKUClassDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKUClass.BL.Interfaces.ISKUClassBL,
                Winit.Modules.SKUClass.BL.Classes.SKUClassBL>();
        _ = _ = services.AddTransient<SKUAttributeDropdownModel>();
        _ = _ = services.AddTransient<SKUGroupSelectionItem>();

        //SKUClassGroup
        _ = services
            .AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup,
                Winit.Modules.SKUClass.Model.Classes.SKUClassGroup>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupDL, Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassGroupDL, Winit.Modules.SKUClass.DL.Classes.MSSQLSKUClassGroupDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupBL,
                Winit.Modules.SKUClass.BL.Classes.SKUClassGroupBL>();

        //SKUClassGroupItems
        _ = services
            .AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems,
                Winit.Modules.SKUClass.Model.Classes.SKUClassGroupItems>();
        _ = services
            .AddTransient<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView,
                Winit.Modules.SKUClass.Model.UIClasses.SKUClassGroupItemView>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupItemsDL, Winit.Modules.SKUClass.DL.Classes.PGSQLSKUClassGroupItemsDL, Winit.Modules.SKUClass.DL.Classes.MSSQLSKUClassGroupItemsDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL,
                Winit.Modules.SKUClass.BL.Classes.SKUClassGroupItemsBL>();

        //ProductUOM
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductUOM,
                Winit.Modules.Product.Model.Classes.ProductUOM>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductUOMDL, Winit.Modules.Product.DL.Classes.PGSQLProductUOMDL, Winit.Modules.Product.DL.Classes.MSSQLProductUOMDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductUOMBL,
                Winit.Modules.Product.BL.Classes.ProductUOMBL>();
        //ProductType
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductType,
                Winit.Modules.Product.Model.Classes.ProductType>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductTypeDL, Winit.Modules.Product.DL.Classes.PGSQLProductTypeDL, Winit.Modules.Product.DL.Classes.MSSQLProductTypeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductTypeBL,
                Winit.Modules.Product.BL.Classes.ProductTypeBL>();
        //ProductDimension
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductDimension,
                Winit.Modules.Product.Model.Classes.ProductDimension>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductDimensionDL, Winit.Modules.Product.DL.Classes.PGSQLProductDimensionDL, Winit.Modules.Product.DL.Classes.MSSQLProductDimensionDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductDimensionBL,
                Winit.Modules.Product.BL.Classes.ProductDimensionBL>();
        //ProductDimensionBridge
        _ = services
            .AddTransient<Winit.Modules.Product.Model.Interfaces.IProductDimensionBridge,
                Winit.Modules.Product.Model.Classes.ProductDimensionBridge>();
        _ = services.AddDatabaseProvider<Winit.Modules.Product.DL.Interfaces.IProductDimensionBridgeDL, Winit.Modules.Product.DL.Classes.PGSQLProductDimensionBridgeDL, Winit.Modules.Product.DL.Classes.MSSQLProductDimensionBridgeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Product.BL.Interfaces.IProductDimensionBridgeBL,
                Winit.Modules.Product.BL.Classes.ProductDimensionBridgeBL>();
        //SalesOrder
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales,
                Winit.Modules.SalesOrder.Model.Classes.DeliveredPreSales>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrder>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales,
                Winit.Modules.SalesOrder.Model.Classes.ViewPreSales>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISKUViewPreSales,
                Winit.Modules.SalesOrder.Model.Classes.SKUViewPreSales>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderLinePrintView>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderPrintView>();
        //_ = services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder, Winit.Modules.SalesOrder.Model.Classes.SalesOrder>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderPrintView>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderLinePrintView>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderInvoice,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderInvoice>();
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLineInvoice,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderLineInvoice>();
        //_ = services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder, Winit.Modules.SalesOrder.Model.Classes.SalesOrder>();
        //_ = services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.Isales_order_info, Winit.Modules.SalesOrder.Model.Classes.sales_order_info>();
        //_ = services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine, Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>();
        //_ = services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel, Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModel>();
        _ = services.AddDatabaseProvider<Winit.Modules.SalesOrder.DL.Interfaces.ISalesOrderDL, Winit.Modules.SalesOrder.DL.Classes.PGSQLSalesOrderDL, Winit.Modules.SalesOrder.DL.Classes.MSSQLSalesOrderDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL,
                Winit.Modules.SalesOrder.BL.Classes.SalesOrderBL>();
        //_ = services.AddTransient<WorkerServices.Interfaces.ISalesOrderWorkerService, WorkerServices.Classes.SalesOrderWorkerService>();


        //Contact
        _ = services
            .AddTransient<Winit.Modules.Contact.Model.Interfaces.IContact,
                Winit.Modules.Contact.Model.Classes.Contact>();
        _ = services.AddDatabaseProvider<Winit.Modules.Contact.DL.Interfaces.IContactDL, Winit.Modules.Contact.DL.Classes.PGSQLContactDL, Winit.Modules.Contact.DL.Classes.MSSQLContactDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Contact.BL.Interfaces.IContactBL, Winit.Modules.Contact.BL.Classes.ContactBL>();

        //Address
        _ = services
            .AddTransient<Winit.Modules.Address.Model.Interfaces.IAddress,
                Winit.Modules.Address.Model.Classes.Address>();
        _ = services.AddDatabaseProvider<Winit.Modules.Address.DL.Interfaces.IAddressDL, Winit.Modules.Address.DL.Classes.PGSQLAddressDL, Winit.Modules.Address.DL.Classes.MSSQLAddressDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Address.BL.Interfaces.IAddressBL, Winit.Modules.Address.BL.Classes.AddressBL>();

        //StoreDocument
        _ = services
            .AddTransient<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument,
                Winit.Modules.StoreDocument.Model.Classes.StoreDocument>();
        _ = services.AddDatabaseProvider<Winit.Modules.StoreDocument.DL.Interfaces.IStoreDocumentDL, Winit.Modules.StoreDocument.DL.Classes.PGSQLStoreDocument, Winit.Modules.StoreDocument.DL.Classes.MSSQLStoreDocument>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.StoreDocument.BL.Interfaces.IStoreDocumentBL,
                Winit.Modules.StoreDocument.BL.Classes.StoreDocumentBL>();

        //FileSysBridge
        _ = services
            .AddTransient<Winit.Modules.FileSys.Model.Interfaces.ISKUImage,
                Winit.Modules.FileSys.Model.Classes.SKUImage>();
        _ = services
            .AddTransient<Winit.Modules.FileSys.Model.Interfaces.IFileSys,
                Winit.Modules.FileSys.Model.Classes.FileSys>();
        _ = services.AddDatabaseProvider<Winit.Modules.FileSys.DL.Interfaces.IFileSysDL, Winit.Modules.FileSys.DL.Classes.PGSQLFileSysDL, Winit.Modules.FileSys.DL.Classes.MSSQLFileSysDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.FileSys.BL.Interfaces.IFileSysBL, Winit.Modules.FileSys.BL.Classes.FileSysBL>();

        //FileSysTemplate
        _ = services
            .AddTransient<Winit.Modules.FileSys.Model.Interfaces.IFileSysTemplate,
                Winit.Modules.FileSys.Model.Classes.FileSysTemplate>();
        _ = services.AddDatabaseProvider<Winit.Modules.FileSys.DL.Interfaces.IFileSysTemplateDL, Winit.Modules.FileSys.DL.Classes.PGSQLFileSysTemplateDL, Winit.Modules.FileSys.DL.Classes.MSSQLFileSysTemplateDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.FileSys.BL.Interfaces.IFileSysTemplateBL,
                Winit.Modules.FileSys.BL.Classes.FileSysTemplateBL>();

        //StoreWeekOff
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreWeekOff,
                Winit.Modules.Store.Model.Classes.StoreWeekOff>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreWeekOffDL, Winit.Modules.Store.DL.Classes.PGSQLStoreWeekOffDL, Winit.Modules.Store.DL.Classes.MSSQLStoreWeekOffDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreWeekOffBL,
                Winit.Modules.Store.BL.Classes.StoreWeekOffBL>();

        //AwayPeriod
        _ = services
            .AddTransient<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod,
                Winit.Modules.AwayPeriod.Model.Classes.AwayPeriod>();
        _ = services.AddDatabaseProvider<Winit.Modules.AwayPeriod.DL.Interfaces.IAwayPeriodDL, Winit.Modules.AwayPeriod.DL.Classes.PGSQLAwayPeriodDL, Winit.Modules.AwayPeriod.DL.Classes.MSSQLAwayPeriodDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.AwayPeriod.BL.Interfaces.IAwayPeriodBL,
                Winit.Modules.AwayPeriod.BL.Classes.AwayPeriodBL>();


        //ListItemHeader
        _ = services
            .AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListItem,
                Winit.Modules.ListHeader.Model.Classes.ListItem>();
        _ = services
            .AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListHeader,
                Winit.Modules.ListHeader.Model.Classes.ListHeader>();
        _ = services.AddDatabaseProvider<Winit.Modules.ListHeader.DL.Interfaces.IListHeaderDL, Winit.Modules.ListHeader.DL.Classes.PGSQLListHeaderDL, Winit.Modules.ListHeader.DL.Classes.MSSQLListHeaderDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ListHeader.BL.Interfaces.IListHeaderBL,
                Winit.Modules.ListHeader.BL.Classes.ListHeaderBL>();

        //SKU
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUListView,
                Winit.Modules.SKU.Model.Classes.SKUListView>();
        _ = _ = services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKU, Winit.Modules.SKU.Model.Classes.SKUV1>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUV1DL, Winit.Modules.SKU.DL.Classes.MSSQLSKUV1DL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUBL, Winit.Modules.SKU.BL.Classes.SKUBL>();

        //SkuSequence
        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISkuSequence,
                Winit.Modules.SKU.Model.Classes.SkuSequence>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISkuSequenceDL, Winit.Modules.SKU.DL.Classes.PGSQLSkuSequenceDL, Winit.Modules.SKU.DL.Classes.MSSQLSkuSequenceDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISkuSequenceBL, Winit.Modules.SKU.BL.Classes.SkuSequenceBL>();

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUConfig, Winit.Modules.SKU.Model.Classes.SKUConfig>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUConfigDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUConfigDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUConfigDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUConfigBL, Winit.Modules.SKU.BL.Classes.SKUConfigBL>();

        _ = _ = services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUUOM, Winit.Modules.SKU.Model.Classes.SKUUOM>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUUOMDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUUOMDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUUOMDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUUOMBL, Winit.Modules.SKU.BL.Classes.SKUUOMBL>();

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes,
                Winit.Modules.SKU.Model.Classes.SKUAttributes>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ISKUAttributesDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUAttributesDL, Winit.Modules.SKU.DL.Classes.MSSQLSKUAttributesDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUAttributesBL,
                Winit.Modules.SKU.BL.Classes.SKUAttributesBL>();

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUMaster, Winit.Modules.SKU.Model.Classes.SKUMaster>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.IDataPreparationDL, Winit.Modules.SKU.DL.Classes.PGSQLDataPreparationDL, Winit.Modules.SKU.DL.Classes.MSSQLDataPreparationDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.IDataPreparationBL,
                Winit.Modules.SKU.BL.Classes.DataPreparationBL>();

        _ = services
            .AddTransient<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap, Winit.Modules.SKU.Model.Classes.TaxSkuMap>();
        _ = services.AddDatabaseProvider<Winit.Modules.SKU.DL.Interfaces.ITaxSkuMapDL, Winit.Modules.SKU.DL.Classes.PGSQLTaxSKUMapDL, Winit.Modules.SKU.DL.Classes.MSSQLTaxSKUMapDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.SKU.BL.Interfaces.ITaxSkuMapBL, Winit.Modules.SKU.BL.Classes.TaxSkuMapBL>();
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap, Winit.Modules.Tax.Model.Classes.TaxSkuMap>();
        //TaxDependencies
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxDependencies,
                Winit.Modules.Tax.Model.Classes.TaxDependencies>();


        //ReturnOrder
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster,
                Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMaster>();
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder,
                Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>();
        _ = services.AddDatabaseProvider<Winit.Modules.ReturnOrder.DL.Interfaces.IReturnOrderDL, Winit.Modules.ReturnOrder.DL.Classes.PGSQLReturnOrderDL, Winit.Modules.ReturnOrder.DL.Classes.MSSQLReturnOrderDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL,
                Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderBL>();
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView,
                Winit.Modules.ReturnOrder.Model.Classes.ReturnSummaryItemView>();

        //ReturnOrderLine
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine,
                Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine>();
        _ = services.AddDatabaseProvider<Winit.Modules.ReturnOrder.DL.Interfaces.IReturnOrderLineDL, Winit.Modules.ReturnOrder.DL.Classes.PGSQLReturnOrderLineDL, Winit.Modules.ReturnOrder.DL.Classes.MSSQLReturnOrderLineDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderLineBL,
                Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderLineBL>();

        //ReturnOrder
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderTax,
                Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderTax>();
        _ = services.AddDatabaseProvider<Winit.Modules.ReturnOrder.DL.Interfaces.IReturnOrderTaxDL, Winit.Modules.ReturnOrder.DL.Classes.PGSQLReturnOrderTaxDL, Winit.Modules.ReturnOrder.DL.Classes.MSSQLReturnOrderTaxDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderTaxBL,
                Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderTaxBL>();
        //TaxGroupTaxes
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes,
                Winit.Modules.Tax.Model.Classes.TaxGroupTaxes>();

        //TaxGroup
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroup, Winit.Modules.Tax.Model.Classes.TaxGroup>();
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSelectionItem,
                Winit.Modules.Tax.Model.Classes.TaxSelectionItem>();
        //taxMaster
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxMaster, Winit.Modules.Tax.Model.Classes.TaxMaster>();
        _ = _ = services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITax, Winit.Modules.Tax.Model.Classes.Tax>();
        _ = services
            .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSlab, Winit.Modules.Tax.Model.Classes.TaxSlab>();
        _ = services
            .AddTransient<Winit.Modules.Tax.DL.Interfaces.ITaxMasterDL, Winit.Modules.Tax.DL.Classes.PGSQTaxMasterDL>();
        _ = services
            .AddTransient<Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL, Winit.Modules.Tax.BL.Classes.TaxMasterBL>();
        //taxskumap
        //_ = services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap, Winit.Modules.Tax.Model.Classes.TaxSkuMap>();
        //UOMTYPE
        _ = services
            .AddTransient<Winit.Modules.UOM.Model.Interfaces.IUOMType, Winit.Modules.UOM.Model.Classes.UOMType>();
        _ = services.AddDatabaseProvider<Winit.Modules.UOM.DL.Interfaces.IUOMTypeDL, Winit.Modules.UOM.DL.Classes.PGSQLUOMTypeDL, Winit.Modules.UOM.DL.Classes.MSSQLUOMTypeDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.UOM.BL.Interfaces.IUOMTypeBL, Winit.Modules.UOM.BL.Classes.UOMTypeBL>();
        //PROMOORDER
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromotionData,
                Winit.Modules.Promotion.Model.Classes.PromotionData>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder,
                Winit.Modules.Promotion.Model.Classes.PromoOrder>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromotion,
                Winit.Modules.Promotion.Model.Classes.Promotion>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem,
                Winit.Modules.Promotion.Model.Classes.PromoOrderItem>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer,
                Winit.Modules.Promotion.Model.Classes.PromoOffer>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem,
                Winit.Modules.Promotion.Model.Classes.PromoOfferItem>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition,
                Winit.Modules.Promotion.Model.Classes.PromoCondition>();
        _ = services
            .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap,
                Winit.Modules.Promotion.Model.Classes.ItemPromotionMap>();
        _ = services.AddDatabaseProvider<Winit.Modules.Promotion.DL.Interfaces.IPromotionDL, Winit.Modules.Promotion.DL.Classes.PGSQLPromotionDL, Winit.Modules.Promotion.DL.Classes.MSSQLPromotionDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Promotion.BL.Interfaces.IPromotionBL,
                Winit.Modules.Promotion.BL.Classes.PromotionBL>();
        //WHStockAvialble
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockAvailable,
                Winit.Modules.WHStock.Model.Classes.WHStockAvailable>();
        //WHStockRequest
        //

        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest,
                Winit.Modules.WHStock.Model.Classes.WHStockRequest>();
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine,
                Winit.Modules.WHStock.Model.Classes.WHStockRequestLine>();
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView,
                Winit.Modules.WHStock.Model.Classes.WHStockRequestItemView>();
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLineItemView,
                Winit.Modules.WHStock.Model.Classes.WHStockRequestLineItemView>();
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView,
                Winit.Modules.WHStock.Model.Classes.ViewLoadRequestItemView>();
        _ = services.AddDatabaseProvider<Winit.Modules.WHStock.DL.Interfaces.IWHStockDL, Winit.Modules.WHStock.DL.Classes.PGSQLWHStockDL, Winit.Modules.WHStock.DL.Classes.MSSQLWHStockDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.WHStock.BL.Interfaces.IWHStockBL, Winit.Modules.WHStock.BL.Classes.WHStockBL>();

        //MobileApp
        _ = services
            .AddTransient<Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation,
                Winit.Modules.Mobile.Model.Classes.SqlitePreparation>();
        _ = services
            .AddTransient<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction,
                Winit.Modules.Mobile.Model.Classes.MobileAppAction>();
        _ = services
            .AddTransient<Winit.Modules.Mobile.Model.Interfaces.IUser, Winit.Modules.Mobile.Model.Classes.User>();
        _ = services
            .AddTransient<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser,
                Winit.Modules.Mobile.Model.Classes.AppVersionUser>();
        //services.AddSingleton<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser, Winit.Modules.Mobile.Model.Classes.AppVersionUser>();
        _ = _ = services.AddTransient<Winit.Modules.Mobile.DL.Interfaces.IMobileAppActionDL, Winit.Modules.Mobile.DL.Classes.PGSQLMobileAppActionDL>();
        _ = services
            .AddTransient<Winit.Modules.Mobile.BL.Interfaces.IMobileAppActionBL,
                Winit.Modules.Mobile.BL.Classes.MobileAppActionBL>();
        _ = _ = services.AddTransient<Winit.Modules.Mobile.DL.Interfaces.IAppVersionUserDL, Winit.Modules.Mobile.DL.Classes.PGSQLAppVersionUserDL>();
        _ = services
            .AddTransient<Winit.Modules.Mobile.BL.Interfaces.IAppVersionUserBL,
                Winit.Modules.Mobile.BL.Classes.AppVersionUserBL>();
        //services.AddSingleton<Winit.Modules.Mobile.DL.Interfaces.IAppVersionUserDL, Winit.Modules.Mobile.DL.Classes.PGSQLAppVersionUserDL>();
        //services.AddSingleton<Winit.Modules.Mobile.BL.Interfaces.IAppVersionUserBL, Winit.Modules.Mobile.BL.Classes.AppVersionUserBL>();
        //CustomSKUFields
        _ = services
            .AddTransient<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields,
                Winit.Modules.CustomSKUField.Model.Classes.CustomSKUFields>();
        _ = services
            .AddTransient<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUField,
                Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField>();
        _ = _ = services.AddTransient<Winit.Modules.CustomSKUField.DL.Interfaces.ICustomSKUFieldsDL, Winit.Modules.CustomSKUField.DL.Classes.PGSQLCustomSKUFieldsDL>();
        _ = services
            .AddTransient<Winit.Modules.CustomSKUField.BL.Interfaces.ICustomSKUFieldsBL,
                Winit.Modules.CustomSKUField.BL.Classes.CustomSKUFieldsBL>();

        //JourneyPlan
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory,
                Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>();
        
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistoryStats,
                Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats>();
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory,
                Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>();
        _ = services.AddDatabaseProvider<Winit.Modules.JourneyPlan.DL.Interfaces.IBeatHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.PGSQLBeatHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.MSSQLBeatHistoryDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL,
                Winit.Modules.JourneyPlan.BL.Classes.BeatHistoryBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.JourneyPlan.DL.Interfaces.IStoreHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.PGSQLStoreHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.MSSQLStoreHistoryDL>(Configuration);

        //UserJourney
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.ITodayJourneyPlanInnerGrid,
                Winit.Modules.JourneyPlan.Model.Classes.TodayJourneyPlanInnerGrid>();
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan,
                Winit.Modules.JourneyPlan.Model.Classes.AssignedJourneyPlan>();
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid,
                Winit.Modules.JourneyPlan.Model.Classes.UserJourneyGrid>();
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView,
                Winit.Modules.JourneyPlan.Model.Classes.UserJourneyView>();
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney,
                Winit.Modules.JourneyPlan.Model.Classes.UserJourney>();
        _ = services.AddDatabaseProvider<Winit.Modules.JourneyPlan.DL.Interfaces.IUserJourneyDL, Winit.Modules.JourneyPlan.DL.Classes.PGSQLUserJourneyDL, Winit.Modules.JourneyPlan.DL.Classes.MSSQLUserJourneyDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourneyBL,
                Winit.Modules.JourneyPlan.BL.Classes.UserJourneyBL>();
        //user
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interfaces.IEmpOrgMappingDDL,
                Winit.Modules.User.Model.Classes.EmpOrgMappingDDL>();
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping,
                Winit.Modules.User.Model.Classes.UserFranchiseeMapping>();
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interfaces.IEmpDTO, Winit.Modules.User.Model.Classes.EmpDTO>();
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interfaces.IMaintainUser,
                Winit.Modules.User.Model.Classes.MaintainUser>();
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interface.IUserHierarchy,
                Winit.Modules.User.Model.Classes.UserHierarchy>();
        _ = services
            .AddTransient<Winit.Modules.User.Model.Interfaces.IUserRoles, Winit.Modules.User.Model.Classes.UserRoles>();
        _ = services.AddDatabaseProvider<Winit.Modules.User.DL.Interfaces.IMaintainUserDL, Winit.Modules.User.DL.Classes.PGSQLMaintainUserDL, Winit.Modules.User.DL.Classes.MSSQLMaintainUserDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.User.BL.Interfaces.IMaintainUserBL,
                Winit.Modules.User.BL.Classes.MaintainUserBL>();
        _ = services
            .AddTransient<Winit.Modules.Common.BL.Interfaces.IAppUser,
                Winit.Modules.Common.BL.Classes.AppUser>();
        //Knowledge Base
        _ = services
            .AddTransient<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailModel,
                Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailModel>();
        _ = services
            .AddTransient<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetailsLocalization,
                Winit.Modules.ErrorHandling.Model.Classes.ErrorDetailsLocalization>();
        _ = services
            .AddTransient<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDescriptionDetails,
                Winit.Modules.ErrorHandling.Model.Classes.ErrorDescriptionDetails>();
        _ = services
            .AddTransient<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail,
                Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>();
        _ = services.AddDatabaseProvider<Winit.Modules.ErrorHandling.DL.Interfaces.IKnowledgeBaseDL, Winit.Modules.ErrorHandling.DL.Classes.PGSQLKnowledgeBaseDL, Winit.Modules.ErrorHandling.DL.Classes.MSSQLKnowledgeBaseDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ErrorHandling.BL.Interfaces.IKnowledgeBaseBL,
                Winit.Modules.ErrorHandling.BL.Classes.KnowledgeBaseBL>();

        //RSSMailQueue
        _ = services
            .AddTransient<Winit.Modules.RSSMailQueue.Model.Interfaces.IRSSMailQueue,
                Winit.Modules.RSSMailQueue.Model.Classes.RSSMailQueue>();
        _ = services.AddDatabaseProvider<Winit.Modules.RSSMailQueue.DL.Interfaces.IRSSMailQueueDL, Winit.Modules.RSSMailQueue.DL.Classes.PGSQLRSSMailQueueDL, Winit.Modules.RSSMailQueue.DL.Classes.MSSQLRSSMailQueueDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.RSSMailQueue.BL.Interfaces.IRSSMailQueueBL,
                Winit.Modules.RSSMailQueue.BL.Classes.RSSMailQueueBL>();

        //SelectionMapCriteria
        _ = services
            .AddTransient<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria,
                Winit.Modules.Mapping.Model.Classes.SelectionMapCriteria>();
        _ = services.AddDatabaseProvider<Winit.Modules.Mapping.DL.Interfaces.ISelectionMapCriteriaDL, Winit.Modules.Mapping.DL.Classes.PGSQLSelectionMapCriteriaDL, Winit.Modules.Mapping.DL.Classes.MSSQLSelectionMapCriteriaDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Mapping.BL.Interfaces.ISelectionMapCriteriaBL,
                Winit.Modules.Mapping.BL.Classes.SelectionMapCriteriaBL>();

        //SelectionMapDetails
        _ = services
            .AddTransient<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails,
                Winit.Modules.Mapping.Model.Classes.SelectionMapDetails>();
        _ = services.AddDatabaseProvider<Winit.Modules.Mapping.DL.Interfaces.ISelectionMapDetailsDL, Winit.Modules.Mapping.DL.Classes.PGSQLSelectionMapDetailsDL, Winit.Modules.Mapping.DL.Classes.MSSQLSelectionMapDetailsDL>(Configuration);
        //_ = services.AddTransient<Winit.Modules.Mapping.BL.Interfaces.ISelectionMapDetailsBL, Winit.Modules.Mapping.BL.Classes.SelectionMapDetailsBL>();

        //User Roles
        _ = _ = services.AddTransient<Winit.Modules.Role.BL.Interfaces.IRoleBL, Winit.Modules.Role.BL.Classes.RoleBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Role.DL.Interfaces.IRoleDL, Winit.Modules.Role.DL.Classes.PGSQLRoleDL, Winit.Modules.Role.DL.Classes.MSSQLRoleDL>(Configuration);
        _ = _ = services.AddTransient<Winit.Modules.Role.Model.Interfaces.IRole, Winit.Modules.Role.Model.Classes.Role>();
        _ = services
            .AddTransient<Winit.Modules.Role.Model.Interfaces.IModule, Winit.Modules.Role.Model.Classes.Module>();
        _ = services
            .AddTransient<Winit.Modules.Role.Model.Interfaces.IPermission,
                Winit.Modules.Role.Model.Classes.Permission>();
        _ = services
            .AddTransient<Winit.Modules.Role.Model.Interfaces.ISubModule, Winit.Modules.Role.Model.Classes.SubModule>();
        _ = services
            .AddTransient<Winit.Modules.Role.Model.Interfaces.ISubSubModules,
                Winit.Modules.Role.Model.Classes.SubSubModules>();
        _ = services
            .AddTransient<Winit.Modules.Role.Model.Interfaces.IModuleMaster,
                Winit.Modules.Role.Model.Classes.ModuleMaster>();
        //charts
        _ = services
            .AddTransient<Winit.Modules.Chart.BL.Interfaces.IChartBL,
                Winit.Modules.Chart.BL.Classes.ChartBL>();
        _ = _ = services.AddTransient<Winit.Modules.Chart.DL.Interfaces.IChartDL, Winit.Modules.Chart.DL.Classes.PGSQLChartDL>();
        _ = services
            .AddTransient<Winit.Modules.Chart.Models.Interfaces.IPOAndTallyDashBoard,
                Winit.Modules.Chart.Models.Classes.POAndTallyDashBoard>();

        // StoreCheckReport
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCheckReport,
                Winit.Modules.Store.Model.Classes.StoreCheckReport>();
        _ = services
           .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCheckReportItem,
               Winit.Modules.Store.Model.Classes.StoreCheckReportItem>();
        _ = _ = services.AddTransient<Winit.Modules.Store.DL.Interfaces.IStoreCheckReportDL, Winit.Modules.Store.DL.Classes.PGSQLStoreCheckReportDL>();
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreCheckReportBL,
                Winit.Modules.Store.BL.Classes.StoreCheckReportBL>();
        // Syncing
        _ = services
            .AddTransient<Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL,
                Winit.Modules.Syncing.BL.Classes.MobileDataSyncBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Syncing.DL.Interfaces.IMobileDataSyncDL, Winit.Modules.Syncing.DL.Classes.PGSQLMobileDataSyncDL, Winit.Modules.Syncing.DL.Classes.MSSQLMobileDataSyncDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ISyncLog,
                Winit.Modules.Syncing.Model.Classes.SyncLog>();
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroupEntityView,
                Winit.Modules.Syncing.Model.Classes.TableGroupEntityView>();
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroup,
                Winit.Modules.Syncing.Model.Classes.TableGroup>();
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableSyncDetail,
                Winit.Modules.Syncing.Model.Classes.TableSyncDetail>();
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableSyncDetail,
                Winit.Modules.Syncing.Model.Classes.TableSyncDetail>();
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroupEntity,
                Winit.Modules.Syncing.Model.Classes.TableGroupEntity>();
        //DyanmicQuery

        //_ = _ = services.AddTransient<Winit.Modules.Syncing.BL.Interfaces.IDynamicQueryBL, Winit.Modules.Syncing.BL.Classes.DynamicQueryBL>();
        //_ = _ = services.AddTransient<Winit.Modules.Syncing.DL.Interfaces.IDynamicQueryDL, Winit.Modules.Syncing.DL.Classes.PGSQLDynamicQueryDL>();
        //_ = _ = services.AddTransient<Winit.Modules.Syncing.Model.Classes.QueryRequest>();
        //_ = _ = services.AddTransient<Winit.Modules.Syncing.Model.Classes.QueryResponse>();

        //apprequest
        _ = services
            .AddTransient<Winit.Modules.Syncing.BL.Interfaces.IAppRequestBL,
                Winit.Modules.Syncing.BL.Classes.AppRequestBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Syncing.DL.Interfaces.IAppRequestDL, Winit.Modules.Syncing.DL.Classes.PGSQLAppRequestDL, Winit.Modules.Syncing.DL.Classes.MSSQLAppRequestDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Syncing.Model.Interfaces.IAppRequest,
                Winit.Modules.Syncing.Model.Classes.AppRequest>();
        //selectionItem
        _ = services
            .AddTransient<Winit.Shared.Models.Common.ISelectionItem, Winit.Shared.Models.Common.SelectionItem>();
        //Dropdown
        _ = services.AddDatabaseProvider<Winit.Modules.DropDowns.DL.Interfaces.IDropDownsDL, Winit.Modules.DropDowns.DL.Classes.PGSQLDropDownsDL, Winit.Modules.DropDowns.DL.Classes.MSSQLDropDownsDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.DropDowns.BL.Interfaces.IDropDownsBL,
                Winit.Modules.DropDowns.BL.Classes.DropDownsBL>();

        //FirebaseReport
        _ = services
            .AddTransient<Winit.Modules.FirebaseReport.BL.Interfaces.IFirebaseReportBL,
                Winit.Modules.FirebaseReport.BL.Classes.FirebaseReportBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.FirebaseReport.DL.Interfaces.IFirebaseReportDL, Winit.Modules.FirebaseReport.DL.Classes.PGSQLFirebaseReportDL, Winit.Modules.FirebaseReport.DL.Classes.MSSQLFirebaseReportDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.FirebaseReport.Models.Interfaces.IFirebaseReport,
                Winit.Modules.FirebaseReport.Models.Classes.FirebaseReport>();
        //ExchangeOrder
        _ = services
            .AddTransient<Winit.Modules.ExchangeOrder.Model.Interfaces.IExchangeOrder,
                Winit.Modules.ExchangeOrder.Model.Classes.ExchangeOrder>();
        _ = services
            .AddTransient<Winit.Modules.ExchangeOrder.Model.Interfaces.IExchangeOrderLine,
                Winit.Modules.ExchangeOrder.Model.Classes.ExchangeOrderLine>();
        //whstockconversion
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockConversion,
                Winit.Modules.WHStock.Model.Classes.WHStockConversion>();
        //whstockconversionline
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockConversionLine,
                Winit.Modules.WHStock.Model.Classes.WHStockConversionLine>();
        //whstockledger
        _ = services
            .AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockLedger,
                Winit.Modules.WHStock.Model.Classes.WHStockLedger>();
        //sequencenumber
        _ = services
            .AddTransient<Winit.Modules.SequenceNumber.Model.Interfaces.ISequenceNumber,
                Winit.Modules.SequenceNumber.Model.Classes.SequenceNumber>();
        //salesordertax
        _ = services
            .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderTax,
                Winit.Modules.SalesOrder.Model.Classes.SalesOrderTax>();
        //mobileerrorlog
        _ = services
            .AddTransient<Winit.Modules.Mobile.Model.Interfaces.IMobileErrorLog,
                Winit.Modules.Mobile.Model.Classes.MobileErrorLog>();
        //QPS
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IQPSSchemeBL,
                Winit.Modules.Scheme.BL.Classes.QPSSchemeBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IQPSSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLQPSSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLQPSSchemeDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IQPSSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLQPSSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLQPSSchemeDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IQPSSchemePO,
                Winit.Modules.Scheme.Model.Classes.QPSSchemePO>();

        //QPS
        //Scheme
        //Scheme
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISellInSchemeBL,
                Winit.Modules.Scheme.BL.Classes.SellInSchemeBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeOrgBL,
                Winit.Modules.Scheme.BL.Classes.SchemeOrgBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeBroadClassificationBL,
                Winit.Modules.Scheme.BL.Classes.SchemeBroadClassificationBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeBranchBL,
                Winit.Modules.Scheme.BL.Classes.SchemeBranchBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISalesPromotionSchemeBL,
                Winit.Modules.Scheme.BL.Classes.SalesPromotionSchemeBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISellOutSchemeHeaderBL,
                Winit.Modules.Scheme.BL.Classes.SellOutSchemeHeaderBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISellOutSchemeLineBL,
                Winit.Modules.Scheme.BL.Classes.SellOutSchemeLineBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IStandingProvisionSchemeBL,
                Winit.Modules.Scheme.BL.Classes.StandingProvisionSchemeBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IStandingProvisionSchemeBranchBL,
                Winit.Modules.Scheme.BL.Classes.StandingProvisionSchemeBranchBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IWalletBL, Winit.Modules.Scheme.BL.Classes.WalletBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IWalletLedgerBL,
                Winit.Modules.Scheme.BL.Classes.WalletLedgerBL>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemesBL, Winit.Modules.Scheme.BL.Classes.SchemesBL>();
        _ = _ = services.AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeExtendHistory,
            Winit.Modules.Scheme.Model.Classes.SchemeExtendHistory>();

        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISellInSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLSellInSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLSellInSchemeDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISellInSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLSellInSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLSellInSchemeDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISalesPromotionSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLSalesPromotionSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLSalesPromotionSchemeDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISellOutSchemeHeaderDL, Winit.Modules.Scheme.DL.Classes.PGSQLSellOutSchemeHeaderDL, Winit.Modules.Scheme.DL.Classes.MSSQLSellOutSchemeHeaderDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISellOutSchemeLineDL, Winit.Modules.Scheme.DL.Classes.PGSQLSellOutSchemeLineDL, Winit.Modules.Scheme.DL.Classes.MSSQLSellOutSchemeLineDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IStandingProvisionSchemeDL, Winit.Modules.Scheme.DL.Classes.PGSQLStandingProvisionSchemeDL, Winit.Modules.Scheme.DL.Classes.MSSQLStandingProvisionSchemeDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IStandingProvisionSchemeBranchDL, Winit.Modules.Scheme.DL.Classes.PGSQLStandingProvisionSchemeBranchDL, Winit.Modules.Scheme.DL.Classes.MSSQLStandingProvisionSchemeBranchDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingSchemePO,
                Winit.Modules.Scheme.Model.Classes.StandingSchemePO>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingSchemeDTO,
                Winit.Modules.Scheme.Model.Classes.StandingSchemeDTO>();
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IWalletDL, Winit.Modules.Scheme.DL.Classes.PGSQLWalletDL, Winit.Modules.Scheme.DL.Classes.MSSQLWalletDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.IWalletLedgerDL, Winit.Modules.Scheme.DL.Classes.PGSQLWalletLedgerDL, Winit.Modules.Scheme.DL.Classes.MSSQLWalletLedgerDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISchemesDL, Winit.Modules.Scheme.DL.Classes.PGSQLSchemesDL, Winit.Modules.Scheme.DL.Classes.MSSQLSchemesDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISchemeOrgDL, Winit.Modules.Scheme.DL.Classes.PGSQLSchemeOrgDL, Winit.Modules.Scheme.DL.Classes.MSSQLSchemeOrgDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISchemeBroadClassificationDL, Winit.Modules.Scheme.DL.Classes.PGSQLSchemeBroadClassificationDL, Winit.Modules.Scheme.DL.Classes.MSSQLSchemeBroadClassificationDL>(Configuration);
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISchemeBranchDL, Winit.Modules.Scheme.DL.Classes.PGSQLSchemeBranchDL, Winit.Modules.Scheme.DL.Classes.MSSQLSchemeBranchDL>(Configuration);

        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IWallet, Winit.Modules.Scheme.Model.Classes.Wallet>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme,
                Winit.Modules.Scheme.Model.Classes.SalesPromotionScheme>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeHeader,
                Winit.Modules.Scheme.Model.Classes.SellInSchemeHeader>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemePO,
                Winit.Modules.Scheme.Model.Classes.SellInSchemePO>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeLine,
                Winit.Modules.Scheme.Model.Classes.SellInSchemeLine>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeBranch,
                Winit.Modules.Scheme.Model.Classes.SchemeBranch>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeBroadClassification,
                Winit.Modules.Scheme.Model.Classes.SchemeBroadClassification>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeOrg,
                Winit.Modules.Scheme.Model.Classes.SchemeOrg>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutSchemeHeader,
                Winit.Modules.Scheme.Model.Classes.SellOutSchemeHeader>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutSchemeLine,
                Winit.Modules.Scheme.Model.Classes.SellOutSchemeLine>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger,
                Winit.Modules.Scheme.Model.Classes.WalletLedger>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionScheme,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionScheme>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeBranch,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeBranch>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeMaster,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeMaster>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeDTO,
                Winit.Modules.Scheme.Model.Classes.SellInSchemeDTO>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IManageScheme,
                Winit.Modules.Scheme.Model.Classes.ManageScheme>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutMasterScheme,
                Winit.Modules.Scheme.Model.Classes.SellOutMasterScheme>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme,
                Winit.Modules.Scheme.Model.Classes.SalesPromotionScheme>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvision,
                Winit.Modules.Scheme.Model.Classes.StandingProvision>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeApplicableOrg,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeApplicableOrg>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeBroadClassification,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeBroadClassification>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IPreviousOrders,
                Winit.Modules.Scheme.Model.Classes.PreviousOrders>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeDivision,
                Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeDivision>();

        // Cash Discount
        _ = services
            .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping,
                Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>();
        _ = services
            .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeExcludeMappingBL,
                Winit.Modules.Scheme.BL.Classes.SchemeExcludeMappingBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Scheme.DL.Interfaces.ISchemeExcludeMappingDL, Winit.Modules.Scheme.DL.Classes.PGSQLSchemeExcludeMappingDL, Winit.Modules.Scheme.DL.Classes.MSSQLSchemeExcludeMappingDL>(Configuration);

        // Store AsmMapping
        _ = services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAsmMapping,
                Winit.Modules.Store.Model.Classes.StoreAsmMapping>();
        _ = services
            .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAsmMappingBL,
                Winit.Modules.Store.BL.Classes.StoreAsmMappingBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Store.DL.Interfaces.IStoreAsmMappingDL, Winit.Modules.Store.DL.Classes.PGSQLStoreAsmMappingDL, Winit.Modules.Store.DL.Classes.MSSQLStoreAsmMappingDL>(Configuration);

        //Stock Updater
        _ = services
            .AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockRequestStock,
                Winit.Modules.StockUpdater.Model.Classes.WHStockRequestStock>();
        _ = services
            .AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger,
                Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>();
        _ = services
            .AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable,
                Winit.Modules.StockUpdater.Model.Classes.WHStockAvailable>();
        _ = services
            .AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary,
                Winit.Modules.StockUpdater.Model.Classes.WHStockSummary>();
        _ = services
            .AddTransient<Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL,
                Winit.Modules.StockUpdater.BL.Classes.StockUpdaterBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL, Winit.Modules.StockUpdater.DL.Classes.PGSQLStockUpdaterDL, Winit.Modules.StockUpdater.DL.Classes.MSSQLStockUpdaterDL>(Configuration);
        _ = services.AddTransient<Int_ApiService>();

        //Integration pending log
        _ = services
            .AddTransient<Winit.Modules.Int_CommonMethods.Model.Interfaces.IPendingDataRequest,
                Winit.Modules.Int_CommonMethods.Model.Classes.PendingDataRequest>();
        _ = services
            .AddTransient<Winit.Modules.Int_CommonMethods.BL.Interfaces.Iint_CommonMethodsBL,
                Winit.Modules.Int_CommonMethods.BL.Classes.int_CommonMethodsBL>();
        _ = _ = services.AddTransient<Winit.Modules.Int_CommonMethods.DL.Interfaces.IInt_CommonMethodsDL, Winit.Modules.Int_CommonMethods.DL.Classes.PGSQLint_CommonMethodsDL>();

        //Invoice
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceHeaderView,
                Winit.Modules.Invoice.Model.Classes.InvoiceHeaderView>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView,
                Winit.Modules.Invoice.Model.Classes.ProvisioningCreditNoteView>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceApprove,
                Winit.Modules.Invoice.Model.Classes.InvoiceApprove>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceLineView,
                Winit.Modules.Invoice.Model.Classes.InvoiceLineView>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceView,
                Winit.Modules.Invoice.Model.Classes.InvoiceView>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceMaster,
                Winit.Modules.Invoice.Model.Classes.InvoiceMaster>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.BL.Interfaces.IInvoiceBL, Winit.Modules.Invoice.BL.Classes.InvoiceBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.Invoice.DL.Interfaces.IInvoiceDL, Winit.Modules.Invoice.DL.Classes.PGSQLInvoiceDL, Winit.Modules.Invoice.DL.Classes.MSSQLInvoiceDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.NewsActivity.Models.Interfaces.INewsActivity,
                Winit.Modules.NewsActivity.Models.Classes.NewsActivity>();
        _ = services
            .AddTransient<Winit.Modules.NewsActivity.BL.Interfaces.INewsActivityBL,
                Winit.Modules.NewsActivity.BL.Classes.NewsActivityBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.NewsActivity.DL.Interfaces.INewsActivityDL, Winit.Modules.NewsActivity.DL.Classes.PGSQLNewsActivityDL, Winit.Modules.NewsActivity.DL.Classes.MSSQLNewsActivityDL>(Configuration);
        //Provisioning
        _ = services
            .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionDataDMS,
                Winit.Modules.Provisioning.Model.Classes.ProvisionDataDMS>();
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IProvisionComparisonView,
                Winit.Modules.Invoice.Model.Classes.ProvisionComparisonView>();
        //NewsActivity
        _ = services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IOutstandingInvoiceReport,
                Winit.Modules.Invoice.Model.Classes.OutstandingInvoiceReport>();

        //Service And Call Registration
        _ = services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.BL.Interfaces.IServiceAndCallRegistrationBL,
                Winit.Modules.ServiceAndCallRegistration.BL.Classes.ServiceAndCallRegistrationBL>();
        _ = services.AddDatabaseProvider<Winit.Modules.ServiceAndCallRegistration.DL.Interfaces.IServiceAndCallRegistrationDL, Winit.Modules.ServiceAndCallRegistration.DL.Classes.PGSQLServiceAndCallRegistrationDL, Winit.Modules.ServiceAndCallRegistration.DL.Classes.MSSQLServiceAndCallRegistrationDL>(Configuration);
        _ = services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration,
                Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration>();
        _ = services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.IServiceRequestStatus,
                Winit.Modules.ServiceAndCallRegistration.Model.Classes.ServiceRequestStatus>();
        //ProvisionComparisonReport
        _ = services
            .AddTransient<Winit.Modules.ProvisionComparisonReport.Model.Interfaces.IProvisionComparisonReportView,
                Winit.Modules.ProvisionComparisonReport.Model.Classes.ProvisionComparisonReportView>();

        //ApprovalEngine
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IViewChangeRequestApproval,
                Winit.Modules.ApprovalEngine.Model.Classes.ViewChangeRequestApproval>();
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IAllApprovalRequest,
                Winit.Modules.ApprovalEngine.Model.Classes.AllApprovalRequest>();
        _ = services
            .AddTransient<Winit.Shared.Models.Common.IApprovalHierarchy,
                Winit.Shared.Models.Common.ApprovalHierarchy>();

        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineHelper,
                Winit.Modules.ApprovalEngine.BL.Classes.ApprovalEngineHelper>();
        _ = services
            .AddTransient<Winit.Modules.ApprovalEngine.Model.Classes.ApprovalRequestItem>();

        _ = services
            .AddTransient<IAuditTrailHelper, AuditTrailHelper>();
        _ = services
            .AddTransient<IAuditTrailEntry, AuditTrailEntry>();

        //StoreActivity
        _ = services
            .AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivity, Winit.Modules.StoreActivity.Model.Classes.StoreActivity>();
        _ = services
            .AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityItem, Winit.Modules.StoreActivity.Model.Classes.StoreActivityItem>();
        _ = _ = services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityHistory, Winit.Modules.StoreActivity.Model.Classes.StoreActivityHistory>();
        _ = _ = services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityRoleMapping, Winit.Modules.StoreActivity.Model.Classes.StoreActivityRoleMapping>();
        //Capture Competitor
        _ = services
            .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor,
                Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>();
        _ = services
            .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandMapping,
                Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandMapping>();
        _ = services
             .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandCompetitorMapping,
                Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandCompetitorMapping>();
        
        // CaptureCompetitor BL and DL services (optional for now since we use direct DB access)
        // TODO: Add these back when BL/DL projects are fully working
        // _ = services
        //     .AddTransient<Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorBL,
        //         Winit.Modules.CaptureCompetitor.BL.Classes.CaptureCompetitorBL>();
        // _ = services
        //     .AddTransient<Winit.Modules.CaptureCompetitor.DL.Interfaces.ICaptureCompetitorDL,
        //         Winit.Modules.CaptureCompetitor.DL.Classes.PGSQLCaptureCompetitorDL>();


        //Planogram
        _ = _ = services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetupV1, Winit.Modules.Planogram.Model.Classes.PlanogramSetupV1>();


        _ = services
            .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetup,
                Winit.Modules.Planogram.Model.Classes.PlanogramSetup>();
        _ = services
            .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionHeader,
                Winit.Modules.Planogram.Model.Classes.PlanogramExecutionHeader>();
        _ = services
            .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionDetail,
                Winit.Modules.Planogram.Model.Classes.PlanogramExecutionDetail>();
        _ = services
            .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramCategory,
                Winit.Modules.Planogram.Model.Classes.PlanogramCategory>();
        _ = services
            .AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramRecommendation,
                Winit.Modules.Planogram.Model.Classes.PlanogramRecommendation>();
        
        // Planogram Data Layer
        _ = services.AddDatabaseProvider<Winit.Modules.Planogram.DL.Interfaces.IPlanogramDL, Winit.Modules.Planogram.DL.Classes.PGSQLPlanogramDL, Winit.Modules.Planogram.DL.Classes.MSSQLPlanogramDL>(Configuration);
        
        // Planogram Business Layer
        _ = services
            .AddTransient<Winit.Modules.Planogram.BL.Interfaces.IPlanogramBL,
                Winit.Modules.Planogram.BL.Classes.PlanogramBL>();
        
        
        _ = services
            .AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecution,
                Winit.Modules.PO.Model.Classes.POExecution>();
        
        _ = services
            .AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecutionLine,
                Winit.Modules.PO.Model.Classes.POExecutionLine>();

        _ = services
            .AddTransient<EscalationMatrixDto>();

        string environmentName = _environment.IsDevelopment()
            ? Winit.Shared.Models.Constants.EnvironmentName.Development
            : Winit.Shared.Models.Constants.EnvironmentName.Production;
        _ = _ = services.AddTransient<IAppConfig, AppConfigs>();
        // Configure Redis cache
        //var redis = ConnectionMultiplexer.Connect("10.20.53.121");
        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = redis.Configuration;
        //});

        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = "10.20.53.121"; // Replace with your Redis server configuration
        //    options.InstanceName = "WINITAPI"; // Replace with your desired instance name
        //});

        //Authentication
        _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });
        _ = services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "WINITAPI",
                Version = "v1"
            });
            // Add JWT Bearer Token Configuration
            OpenApiSecurityScheme securityScheme = new()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };
            c.AddSecurityDefinition("Bearer", securityScheme);
            OpenApiSecurityRequirement securityRequirement = new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme, Id = "Bearer"
                        }
                    },
                    new string[]
                    {
                    }
                }
            };

            c.AddSecurityRequirement(securityRequirement);
            // Use the custom schema ID generator to avoid conflicts
            c.CustomSchemaIds(CustomSchemaIdGenerator.Generate);

            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); // Workaround for conflicts
        });
        //Authorization
        //services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("AdminOnly", policy =>
        //        policy.RequireRole("admin"));
        //});
        _ = services.AddAuthorization();
        // Configure Serilog with dynamic configuration from appsettings.json
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration) // Read configuration from appsettings.json
            .CreateLogger();

        // Register the Serilog logger
        _ = services.AddLogging(loggingBuilder =>
        {
            _ = loggingBuilder.ClearProviders(); // Clear default logging providers
            _ = loggingBuilder.AddSerilog(); // Add Serilog as the logging provider
        });
    }

    private bool IsRabbitMQServerAvailable()
    {
        try
        {
            ConnectionFactory factory = new()
            {
                HostName = Configuration["RabbitMQ:HostName"],
                Port = Convert.ToInt32(Configuration["RabbitMQ:Port"]),
                UserName = Configuration["RabbitMQ:UserName"],
                Password = Configuration["RabbitMQ:Password"]
            };

            using IConnection connection = factory.CreateConnection();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            _ = app.UseDeveloperExceptionPage();
        }
        
        // Enable Swagger UI for all environments
        _ = app.UseSwagger();
        _ = app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WINITAPI v1");
            options.RoutePrefix = string.Empty; // Set the Swagger UI at the root URL
        });
        //app.UseCors("AllowSpecificOrigins");

        _ = app.UseCors(builder =>
        {
            _ = builder.AllowAnyOrigin();
            _ = builder.AllowAnyHeader();
            _ = builder.AllowAnyMethod();
        });
        _ = app.UseHttpsRedirection();

        _ = app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data")),
            RequestPath = "/data",
            ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings =
                {
                    [".sqlite"] = "application/x-sqlite3", [".db"] = "application/octet-stream"
                }
            }
        });

        // Add a second static file configuration for /Sqlite path to support mobile app compatibility
        // This will serve files from the Data directory and all its subdirectories
        _ = app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data")),
            RequestPath = "/Sqlite",
            ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings =
                {
                    [".sqlite"] = "application/x-sqlite3", 
                    [".db"] = "application/octet-stream",
                    [".zip"] = "application/zip"
                }
            },
            ServeUnknownFileTypes = true,
            DefaultContentType = "application/octet-stream"
        });

        _ = app.UseRouting();
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        
        // Enable custom error handling middleware
        _ = app.UseCustomErrorHandling();
        
        // Use Serilog for request logging
        //app.UseSerilogRequestLogging();
        _ = app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });
    }
}

public class CustomSchemaIdGenerator
{
    public static string Generate(Type type)
    {
        // Use the full name or any custom logic to generate unique schema IDs
        return type.FullName.Replace(".", "_");
    }
}