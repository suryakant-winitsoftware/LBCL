global using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Dapper;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.DashBoard.BL.Classes;
using Winit.Modules.DashBoard.BL.Interfaces;
using Winit.Modules.Emp.BL.Classes;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.DL.Classes;
using Winit.Modules.Emp.DL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Route.BL.Classes;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SalesOrder.BL.UIClasses;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Modules.WHStock.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using WINITMobile.Data;
using WINITMobile.Pages.Collection;
using WINITMobile.State;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Constants;
using Winit.Modules.StoreActivity.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;

namespace WINITMobile;

public static class MauiProgram
{
    /// <summary>
    /// Current environment setting - Change this value to switch environments as needed.
    /// This allows debugging with different environment URLs (e.g., Production URL during development).
    /// Available values: EnvironmentName.Development, EnvironmentName.Production
    /// </summary>
    private static readonly string CurrentEnvironment = EnvironmentName.Development;
    public static readonly string CurrentProjectName = "WINIT";

    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        builder.Services.AddSingleton<WeatherForecastService>();

        #endif

        // Register your services
        builder.Services.AddScoped<InterfaceTypeResolver>(); // Register the converter as a scoped service
        builder.Services.AddScoped(serviceProvider =>
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { serviceProvider.GetRequiredService<InterfaceTypeResolver>() }
            };
            return settings;
        });

        // test Niranjan
        builder.Services.AddSingleton<WINITMobile.State.IBackButtonHandler, WINITMobile.State.BackButtonHandler>();
        builder.Services.AddScoped<StopwatchService>();
        builder.Services.AddScoped<DatabaseUploadService>();
        builder.Services.AddScoped<IImageUploadService, ImageUploadService>();


        //shareofshelf
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.BL.Interfaces.IShareOfShelfViewModel, Winit.Modules.ShareOfShelf.BL.Classes.ShareOfShelfAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.BL.Interfaces.IShareOfShelfBL, Winit.Modules.ShareOfShelf.BL.Classes.ShareOfShelfBL>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.DL.Interfaces.IShareOfShelfDL, Winit.Modules.ShareOfShelf.DL.Classes.SQLiteShareOfShelfDL>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosHeader, Winit.Modules.ShareOfShelf.Model.Classes.SosHeader>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosHeaderCategory, Winit.Modules.ShareOfShelf.Model.Classes.SosHeaderCategory>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosHeaderCategoryItemView, Winit.Modules.ShareOfShelf.Model.Classes.SosHeaderCategoryItemView>();
        builder.Services.AddTransient<Winit.Modules.ShareOfShelf.Model.Interfaces.ISosLine, Winit.Modules.ShareOfShelf.Model.Classes.SosLine>();


        //CaptureCompetitorBL
        builder.Services.AddTransient<Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorViewModel, Winit.Modules.CaptureCompetitor.BL.Classes.CaptureCompetitorAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.CaptureCompetitor.BL.Interfaces.ICaptureCompetitorBL, Winit.Modules.CaptureCompetitor.BL.Classes.CaptureCompetitorBL>();
        builder.Services.AddTransient<Winit.Modules.CaptureCompetitor.DL.Interfaces.ICaptureCompetitorDL, Winit.Modules.CaptureCompetitor.DL.Classes.SQLiteCaptureCompetitorDL>();
        builder.Services.AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor, Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>();


        //planogram
        builder.Services.AddTransient<Winit.Modules.Planogram.BL.Interfaces.IPlanogramViewModel, Winit.Modules.Planogram.BL.Classes.PlanogramAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.Planogram.BL.Interfaces.IPlanogramBL, Winit.Modules.Planogram.BL.Classes.PlanogramBL>();
        builder.Services.AddTransient<Winit.Modules.Planogram.DL.Interfaces.IPlanogramDL, Winit.Modules.Planogram.DL.Classes.SQLitePlanogramDL>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetup, Winit.Modules.Planogram.Model.Classes.PlanogramSetup>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionHeader, Winit.Modules.Planogram.Model.Classes.PlanogramExecutionHeader>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionDetail, Winit.Modules.Planogram.Model.Classes.PlanogramExecutionDetail>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramCategory, Winit.Modules.Planogram.Model.Classes.PlanogramCategory>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramRecommendation, Winit.Modules.Planogram.Model.Classes.PlanogramRecommendation>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramSetupV1, Winit.Modules.Planogram.Model.Classes.PlanogramSetupV1>();
        builder.Services.AddTransient<Winit.Modules.Planogram.DL.Interfaces.IPlanogramV1DL, Winit.Modules.Planogram.DL.Classes.SQLitePlanogramV1DL>();
        builder.Services.AddTransient<Winit.Modules.Planogram.BL.Interfaces.IPlanogramV1BL, Winit.Modules.Planogram.BL.Classes.PlanogramV1BL>();
        builder.Services.AddTransient<Winit.Modules.Planogram.Model.Interfaces.IPlanogramExecutionV1, Winit.Modules.Planogram.Model.Classes.PlanogramExecutionV1>();


        // Register the Foreground Service
#if ANDROID
        builder.Services.AddSingleton<IForegroundService, WINITMobile.Platforms.Android.ForegroundServiceAndroid>();
        //builder.Services.AddSingleton<ICameraService, WINITMobile.Platforms.Android.CameraService>();
        //builder.Services.AddSingleton<IForegroundServiceForStopwatch, WINITMobile.Platforms.Android.StopwatchForegroundServiceAndroid>();
#endif


        // Configure localization services
        builder.Services.AddLocalization(options => options.ResourcesPath = "LanguageResources");
        builder.Services.AddSingleton<Winit.UIComponents.Common.Language.ILanguageService, Winit.UIComponents.Common.Language.LanguageService>();
        builder.Services.AddSingleton<Winit.Modules.Common.BL.Interfaces.IAndroidBlazorCommunicationBridge, AndroidBlazorCommunicationBridge>();
        builder.Services.AddSingleton<WeatherForecastService>();

        builder.Services.AddSingleton<WINITMobile.Services.SecureStorageHelper>();
        // for loading
        builder.Services.AddSingleton<Winit.UIComponents.Common.Services.ILoadingService, Winit.UIComponents.Common.Services.LoadingService>();

        //LocalStorage - Use secure mobile implementation
        builder.Services.AddTransient<Winit.Modules.Base.BL.ILocalStorageService, WINITMobile.Services.MobileLocalStorageService>();

        //File Uploader
        builder.Services.AddScoped<Winit.UIComponents.Common.FileUploader.IFileUploaderBaseViewModel, Winit.UIComponents.Common.FileUploader.FileUploaderBaseViewModel>();
        //Auth
        builder.Services.AddTransient<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddTransient<Winit.Shared.CommonUtilities.Common.SHACommonFunctions>();
        builder.Services.AddTransient<Winit.Modules.Auth.Model.Interfaces.IAuthMaster, Winit.Modules.Auth.Model.Classes.AuthMaster>();
        builder.Services.AddSingleton<Winit.Modules.Auth.BL.Interfaces.ISyncViewModel, Winit.Modules.Auth.BL.Classes.SyncBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.Auth.BL.Classes.SyncDbInit>();
        builder.Services.AddScoped<Winit.Modules.Auth.Model.Interfaces.ILoginResponse, Winit.Modules.Auth.Model.Classes.LoginResponse>();
        builder.Services.TryAddTransient<Winit.Modules.Auth.Model.Interfaces.IClearDataResult, Winit.Modules.Auth.Model.Classes.ClearDataResult>();
        builder.Services.TryAddTransient<Winit.Modules.Auth.BL.Interfaces.IClearDataServiceBL, Winit.Modules.Auth.BL.Classes.ClearDataServiceBL>();

        // for JourneyPlan
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IJourneyPlanViewModelFactory, Winit.Modules.JourneyPlan.BL.Classes.JourneyPlanViewModelFactory>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory, Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney, Winit.Modules.JourneyPlan.Model.Classes.UserJourney>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryBL, Winit.Modules.JourneyPlan.BL.Classes.BeatHistoryBL>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.DL.Interfaces.IBeatHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.SQLiteBeatHistoryDL>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory, Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistoryStats, Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.DL.Interfaces.IStoreHistoryDL, Winit.Modules.JourneyPlan.DL.Classes.SQLiteStoreHistoryDL>();
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IBeatHistoryViewModel, Winit.Modules.JourneyPlan.BL.Classes.BeatHistoryAppViewModel>();

        //TaxDependencies
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxDependencies, Winit.Modules.Tax.Model.Classes.TaxDependencies>();

        //Org
        builder.Services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrgType, Winit.Modules.Org.Model.Classes.OrgType>();
        builder.Services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrg, Winit.Modules.Org.Model.Classes.Org>();


        // for Vanstock
        builder.Services.AddScoped<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView, Winit.Modules.Org.Model.Classes.WarehouseStockItemView>();
        builder.Services.AddTransient<Winit.Modules.Org.DL.Interfaces.IOrgDL, Winit.Modules.Org.DL.Classes.SQLiteOrgDL>();
        builder.Services.AddTransient<Winit.Modules.Org.BL.Interfaces.IOrgBL, Winit.Modules.Org.BL.Classes.OrgBL>();
        builder.Services.AddTransient<Winit.Modules.Org.BL.Interfaces.IViewWareHouse_VanStockViewModel, Winit.Modules.Org.BL.Classes.ViewWareHouse_VanStockWebViewModel>();

        //Sqlite Service
        builder.Services.AddScoped<Winit.Modules.Base.DL.DBManager.SqliteDBManager>();

        //CDF
        builder.Services.AddScoped<Winit.Modules.JourneyPlan.BL.Interfaces.ICFDViewModel, Winit.Modules.JourneyPlan.BL.Classes.CFDBaseViewModel>();

        //StoreActivity
        builder.Services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityHistory, Winit.Modules.StoreActivity.Model.Classes.StoreActivityHistory>();
        builder.Services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityRoleMapping, Winit.Modules.StoreActivity.Model.Classes.StoreActivityRoleMapping>();
        builder.Services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivity, Winit.Modules.StoreActivity.Model.Classes.StoreActivity>();
        builder.Services.AddTransient<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityItem, Winit.Modules.StoreActivity.Model.Classes.StoreActivityItem>();
        builder.Services.AddTransient<Winit.Modules.StoreActivity.DL.Interfaces.IStoreActivitDL, Winit.Modules.StoreActivity.DL.Classes.SQLiteStoreActivitDL>();
        builder.Services.AddTransient<Winit.Modules.StoreActivity.BL.Interfaces.IStoreActivitBL, Winit.Modules.StoreActivity.BL.Classes.StoreActivityBL>();
        builder.Services.AddScoped<Winit.Modules.StoreActivity.BL.Interfaces.IStoreActivityViewModel, Winit.Modules.StoreActivity.BL.Classes.StoreActivityBaseViewModel>();

        // Start-Day
        builder.Services.AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourneyBL, Winit.Modules.JourneyPlan.BL.Classes.UserJourneyBL>();
        builder.Services.AddScoped<Winit.Modules.JourneyPlan.BL.Interfaces.IStartDayViewModel, Winit.Modules.JourneyPlan.BL.Classes.StartDayBaseViewModel>();


        // for Share
        builder.Services.AddTransient<IShareService, ShareService>();

        //navigation
        builder.Services.AddSingleton<NavigationService>();
        builder.Services.AddSingleton<IServiceProvider, ServiceProvider>();

        //login service
        builder.Services.AddTransient<Winit.Modules.Auth.BL.Interfaces.ILoginViewModel, Winit.Modules.Auth.BL.Classes.LoginAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.Auth.DL.Interfaces.ICommonDataDL, Winit.Modules.Auth.DL.Classes.SQLiteCommonDataDL>();
        builder.Services.AddTransient<SKUGroupSelectionItem>();

        //for printservice

        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView, Winit.Modules.SalesOrder.Model.Classes.SalesOrderPrintView>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView, Winit.Modules.SalesOrder.Model.Classes.SalesOrderLinePrintView>();
        builder.Services.AddSingleton<Winit.Modules.Printing.Model.Classes.BluetoothDeviceInfo>();


        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddSingleton<WINITMobile.Pages.Base.BaseComponentBase>();
        //top
        builder.Services.AddSingleton<WINITMobile.Data.SideBarService>();
        /* Unmerged change from project 'WINITMobile (net7.0-android)'
        Before:
                    builder.Services.AddSingleton<WINITMobile.TopBarButtonClasses.Service>();
        After:
                    builder.Services.AddSingleton<Service>();)
        */
        builder.Services.AddSingleton<Models.TopBar.Service>();
        // Use the configured environment setting
        builder.Services.AddSingleton<IAppConfig>(new MobileAppConfigs(CurrentEnvironment, Path.Combine(FileSystem.AppDataDirectory, CurrentProjectName, "Data")));
        builder.Services.AddSingleton<Winit.Shared.CommonUtilities.Common.CommonFunctions>();
        builder.Services.AddSingleton<Winit.Modules.Common.BL.Interfaces.IAppUser, Winit.Modules.Common.BL.Classes.AppUser>();

        //Menu Handling
        builder.Services.AddSingleton<Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView, Winit.Modules.Role.Model.Classes.MenuMasterHierarchyView>();
        builder.Services.AddTransient<Winit.Modules.Role.BL.Interfaces.IMenu, Winit.Modules.Role.BL.Classes.MobileMenu>();


        builder.Services.AddSingleton<Winit.Modules.Common.Model.Interfaces.IDataManager, Winit.Modules.Common.Model.Classes.DataManager>();
        builder.Services.AddSingleton<Winit.UIComponents.Common.IAlertService, Winit.UIComponents.Common.AlertService>();
        builder.Services.AddSingleton<Winit.UIComponents.Common.Services.IDropDownService, Winit.UIComponents.Common.Services.DropDownService>();

        // Configure HttpClient
        builder.Services.AddScoped(sp => 
        {
            var appConfig = sp.GetRequiredService<IAppConfig>();
            var apiBaseUrl = appConfig.ApiBaseUrl;

            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                throw new InvalidOperationException("API Base URL is not configured in MobileAppConfigs");
            }

            return new HttpClient { BaseAddress = new Uri(apiBaseUrl), Timeout = TimeSpan.FromMinutes(5) };
        });
        builder.Services.AddScoped<Winit.Modules.Base.BL.ApiService>();
        builder.Services.AddScoped<Winit.Modules.Base.BL.NetworkErrorHandler>();
        builder.Services.AddScoped<WINITMobile.Services.NetworkConnectivityService>();

        //Common Helper
        builder.Services.AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.IFilterHelper, Winit.Modules.Base.BL.Helper.Classes.FilterHelper>();
        builder.Services.AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.ISortHelper, Winit.Modules.Base.BL.Helper.Classes.SortHelper>();
        builder.Services.AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.IListHelper, Winit.Modules.Base.BL.Helper.Classes.ListHelper>();
        builder.Services.AddTransient<Winit.Shared.CommonUtilities.Common.FileService>();

        //collection
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection, Winit.Modules.CollectionModule.Model.Classes.AccCollection>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode, Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment, Winit.Modules.CollectionModule.Model.Classes.AccCollectionAllotment>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger, Winit.Modules.CollectionModule.Model.Classes.AccStoreLedger>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable, Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccReceivable, Winit.Modules.CollectionModule.Model.Classes.AccReceivable>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionSettlement, Winit.Modules.CollectionModule.Model.Classes.AccCollectionSettlement>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionSettlementReceipts, Winit.Modules.CollectionModule.Model.Classes.AccCollectionSettlementReceipts>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollections, Winit.Modules.CollectionModule.Model.Classes.Collections>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IExchangeRate, Winit.Modules.CollectionModule.Model.Classes.ExchangeRate>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule, Winit.Modules.CollectionModule.Model.Classes.CollectionModule>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.DL.Interfaces.ICollectionModuleDL, Winit.Modules.CollectionModule.DL.Classes.SQLiteCollectionModuleDL>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL, Winit.Modules.CollectionModule.BL.Classes.CollectionModuleBL>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleViewModel, Winit.Modules.CollectionModule.BL.Classes.CollectionAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.ICreatePaymentViewModel, Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.ICashCollectionDepositViewModel, Winit.Modules.CollectionModule.BL.Classes.CashCollectionDeposit.CashCollectionDepositAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IEarlyPaymentDiscountConfiguration, Winit.Modules.CollectionModule.Model.Classes.EarlyPaymentDiscountConfiguration>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IEarlyPaymentDiscountAppliedDetails, Winit.Modules.CollectionModule.Model.Classes.EarlyPaymentDiscountAppliedDetails>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionPrint, Winit.Modules.CollectionModule.Model.Classes.CollectionPrint>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionPrintDetails, Winit.Modules.CollectionModule.Model.Classes.CollectionPrintDetails>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionCurrencyDetails, Winit.Modules.CollectionModule.Model.Classes.AccCollectionCurrencyDetails>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IPaymentSummary, Winit.Modules.CollectionModule.Model.Classes.PaymentSummary>();
        builder.Services.AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionDeposit, Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>();




        //ListHeader
        builder.Services.AddTransient<Winit.Modules.ListHeader.BL.Interfaces.IListHeaderBL, Winit.Modules.ListHeader.BL.Classes.ListHeaderBL>();
        builder.Services.AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListHeader, Winit.Modules.ListHeader.Model.Classes.ListHeader>();
        builder.Services.AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListItem, Winit.Modules.ListHeader.Model.Classes.ListItem>();
        builder.Services.AddTransient<Winit.Modules.ListHeader.DL.Interfaces.IListHeaderDL, Winit.Modules.ListHeader.DL.Classes.SQLiteListHeaderDL>();

        //Location
        builder.Services.AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationBL, Winit.Modules.Location.BL.Classes.LocationBL>();
        builder.Services.AddTransient<Winit.Modules.Location.DL.Interfaces.ILocationDL, Winit.Modules.Location.DL.Classes.SQLiteLocationDL>();

        //Address
        builder.Services.AddTransient<Winit.Modules.Address.BL.Interfaces.IAddressBL, Winit.Modules.Address.BL.Classes.AddressBL>();


        //Store
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo, Winit.Modules.Store.Model.Classes.StoreAdditionalInfo>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();
        builder.Services.AddTransient<Winit.Modules.Store.DL.Interfaces.IStoreDL, Winit.Modules.Store.DL.Classes.SQLiteStoreDL>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreBL, Winit.Modules.Store.BL.Classes.StoreBL>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreItemView, Winit.Modules.Store.Model.Classes.StoreItemView>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreViewModel, Winit.Modules.Store.BL.Classes.StoreViewModel>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCredit, Winit.Modules.Store.Model.Classes.StoreCredit>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAttributes, Winit.Modules.Store.Model.Classes.StoreAttributes>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreMaster, Winit.Modules.Store.Model.Classes.StoreMaster>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStandardListSource, Winit.Modules.Store.Model.Classes.StandardListSource>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStorBaseViewModelForMobile, Winit.Modules.Store.BL.Classes.StorBaseViewModelForMobile>();

        //RouteCustomer
        builder.Services.AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteCustomer, Winit.Modules.Route.Model.Classes.RouteCustomer>();



        // OrgCurrency
        builder.Services.AddTransient<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency, Winit.Modules.Currency.Model.Classes.OrgCurrency>();
        builder.Services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrgHeirarchy, Winit.Modules.Org.Model.Classes.OrgHeirarchy>();

        //currency
        builder.Services.AddTransient<Winit.Modules.Currency.BL.Interfaces.ICurrencyBL, Winit.Modules.Currency.BL.Classes.CurrencyBL>();
        builder.Services.AddTransient<Winit.Modules.Currency.DL.Interfaces.ICurrencyDL, Winit.Modules.Currency.DL.Classes.SQLiteCurrencyDL>();
        builder.Services.AddTransient<Winit.Modules.Currency.Model.Interfaces.ICurrency, Winit.Modules.Currency.Model.Classes.Currency>();


        //Contact 
        builder.Services.AddTransient<Winit.Modules.Contact.Model.Interfaces.IContact, Winit.Modules.Contact.Model.Classes.Contact>();
        builder.Services.AddTransient<Winit.Modules.Contact.BL.Interfaces.IContactBL, Winit.Modules.Contact.BL.Classes.ContactBL>();
        builder.Services.AddTransient<Winit.Modules.Contact.DL.Interfaces.IContactDL, Winit.Modules.Contact.DL.Classes.SQLiteContactDL>();

        //Address 
        builder.Services.AddTransient<Winit.Modules.Address.Model.Interfaces.IAddress, Winit.Modules.Address.Model.Classes.Address>();
        builder.Services.AddTransient<Winit.Modules.Address.BL.Interfaces.IAddressBL, Winit.Modules.Address.BL.Classes.AddressBL>();
        builder.Services.AddTransient<Winit.Modules.Address.DL.Interfaces.IAddressDL, Winit.Modules.Address.DL.Classes.SQLiteAddressDL>();



        //AdditionalInfo for payment details 
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoBL, Winit.Modules.Store.BL.Classes.StoreAdditionalInfoBL>();
        builder.Services.AddTransient<Winit.Modules.Store.DL.Interfaces.IStoreAdditionalInfoDL, Winit.Modules.Store.DL.Classes.SQLiteStoreAdditionalInfoDL>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IPayment, Winit.Modules.Store.Model.Classes.Payment>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IWeekDays, Winit.Modules.Store.Model.Classes.WeekDays>();

        //document

        builder.Services.AddTransient<Winit.Modules.StoreDocument.BL.Interfaces.IStoreDocumentBL, Winit.Modules.StoreDocument.BL.Classes.StoreDocumentBL>();
        builder.Services.AddTransient<Winit.Modules.StoreDocument.DL.Interfaces.IStoreDocumentDL, Winit.Modules.StoreDocument.DL.Classes.SQLiteStoreDocument>();
        builder.Services.AddTransient<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument, Winit.Modules.StoreDocument.Model.Classes.StoreDocument>();

        //Bank
        builder.Services.AddTransient<Winit.Modules.Bank.BL.Interfaces.IBankBL, Winit.Modules.Bank.BL.Classes.BankBL>();
        builder.Services.AddTransient<Winit.Modules.Bank.Model.Interfaces.IBank, Winit.Modules.Bank.Model.Classes.Bank>();

        //Tax
        builder.Services.AddTransient<Winit.Modules.Tax.Model.UIInterfaces.ITaxView, Winit.Modules.Tax.Model.UIClasses.TaxView>();

        //TaxGroupTaxes
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes, Winit.Modules.Tax.Model.Classes.TaxGroupTaxes>();

        //TaxGroup
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroup, Winit.Modules.Tax.Model.Classes.TaxGroup>();


        //Sales Order
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderAppViewModel, Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView, Winit.Modules.SalesOrder.Model.UIClasses.SalesOrderItemView>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView, Winit.Modules.SKU.Model.UIClasses.SKUUOMView>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView, Winit.Modules.SKU.Model.UIClasses.SKUAttributeView>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderAmountCalculator, Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderAmountCalculator>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder, Winit.Modules.SalesOrder.Model.Classes.SalesOrder>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine, Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.DL.Interfaces.ISalesOrderDL, Winit.Modules.SalesOrder.DL.Classes.SQLiteSalesOrderDL>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.Interfaces.ISalesOrderBL, Winit.Modules.SalesOrder.BL.Classes.SalesOrderBL>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.IOrderLevelCalculator, Winit.Modules.SalesOrder.BL.UIClasses.OrderLevelCalculator>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ICashDiscountCalculator, Winit.Modules.SalesOrder.BL.UIClasses.CashDiscountCalculator>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView, Winit.Modules.SalesOrder.Model.Classes.SalesOrderPrintView>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView, Winit.Modules.SalesOrder.Model.Classes.SalesOrderLinePrintView>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderInvoice, Winit.Modules.SalesOrder.Model.Classes.SalesOrderInvoice>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLineInvoice, Winit.Modules.SalesOrder.Model.Classes.SalesOrderLineInvoice>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderDataHelper, Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderDataAppHelper>();

        //Return Order
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderWithInvoiceViewModel, Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderWithInvoiceAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView, Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderItemView>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderAmountCalculator, Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderAmountCalculator>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnSummaryViewModel, Winit.Modules.ReturnOrder.BL.Classes.ReturnSummaryAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView, Winit.Modules.ReturnOrder.Model.Classes.ReturnSummaryItemView>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderBL, Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderBL>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.DL.Interfaces.IReturnOrderDL, Winit.Modules.ReturnOrder.DL.Classes.SQLiteReturnOrderDL>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder, Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine, Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderLineBL, Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderLineBL>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.DL.Interfaces.IReturnOrderLineDL, Winit.Modules.ReturnOrder.DL.Classes.SQLiteReturnOrderLineDL>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderViewModelFactory, Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderViewModelFactory>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderInvoice, Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderInvoice>();
        builder.Services.AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLineInvoice, Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLineInvoice>();


        //DevliverySummery
        builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesSummaryViewModel, Winit.Modules.SalesOrder.BL.UIClasses.SalesSummaryBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView, Winit.Modules.SalesOrder.Model.UIClasses.SalesSummaryItemView>();

        //SkuPrice
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPrice, Winit.Modules.SKU.Model.Classes.SKUPrice>();
        builder.Services.AddTransient<Winit.Modules.SKU.DL.Interfaces.ISKUPriceDL, Winit.Modules.SKU.DL.Classes.SQLiteSKUPriceDL>();
        builder.Services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUPriceBL, Winit.Modules.SKU.BL.Classes.SKUPriceBL>();

        //Setting
        builder.Services.AddSingleton<Winit.Modules.Setting.BL.Interfaces.IAppSetting, Winit.Modules.Setting.BL.Classes.AppSettings>();
        builder.Services.AddTransient<Winit.Modules.Setting.Model.Interfaces.ISetting, Winit.Modules.Setting.Model.Classes.Setting>();
        builder.Services.AddTransient<Winit.Modules.Setting.DL.Interfaces.ISettingDL, Winit.Modules.Setting.DL.Classes.SQLiteSettingDL>();
        builder.Services.AddTransient<Winit.Modules.Setting.BL.Interfaces.ISettingBL, Winit.Modules.Setting.BL.Classes.SettingBL>();

        //SKU,SKUUOM
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKU, Winit.Modules.SKU.Model.Classes.SKU>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Classes.SKU>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUConfig, Winit.Modules.SKU.Model.Classes.SKUConfig>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes, Winit.Modules.SKU.Model.Classes.SKUAttributes>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUUOM, Winit.Modules.SKU.Model.Classes.SKUUOM>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap, Winit.Modules.SKU.Model.Classes.TaxSkuMap>();
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUMaster, Winit.Modules.SKU.Model.Classes.SKUMaster>();
        builder.Services.AddTransient<Winit.Modules.SKU.DL.Interfaces.ISKUDL, Winit.Modules.SKU.DL.Classes.SqliteSKUDL>();
        builder.Services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUBL, Winit.Modules.SKU.BL.Classes.SKUBL>();
        builder.Services.AddTransient<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields, Winit.Modules.CustomSKUField.Model.Classes.CustomSKUFields>();

        //SKUGroup
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroup, Winit.Modules.SKU.Model.Classes.SKUGroup>();

        //SKUGroupType
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType, Winit.Modules.SKU.Model.Classes.SKUGroupType>();


        builder.Services.AddTransient<Winit.Modules.Common.BL.Interfaces.IScopedService, Winit.Modules.Common.BL.Classes.ScopedService>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreBL, Winit.Modules.Store.BL.Classes.StoreBL>();

        //WHStock




        builder.Services.AddTransient<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView, Winit.Modules.WHStock.BL.Classes.LoadRequestAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.WHStock.BL.Interfaces.IAddEditLoadRequest, Winit.Modules.WHStock.BL.Classes.AddEditLoadRequestAppViewModel>();

        builder.Services.AddTransient<IWHStockBL, Winit.Modules.WHStock.BL.Classes.WHStockBL>();
        builder.Services.AddTransient<Winit.Modules.WHStock.DL.Interfaces.IWHStockDL, Winit.Modules.WHStock.DL.Classes.SQLiteWHStockDL>();
        builder.Services.AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView, Winit.Modules.WHStock.Model.Classes.WHStockRequestItemView>();
        // In your ConfigureServices method 
        builder.Services.AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest, Winit.Modules.WHStock.Model.Classes.WHStockRequest>();
        builder.Services.AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLineItemView, Winit.Modules.WHStock.Model.Classes.WHStockRequestLineItemView>();

        builder.Services.AddTransient<Winit.Modules.WHStock.Model.Interfaces.IViewLoadRequestItemView, Winit.Modules.WHStock.Model.Classes.ViewLoadRequestItemView>();
        builder.Services.AddTransient<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine, Winit.Modules.WHStock.Model.Classes.WHStockRequestLine>();

        //StoreGroupData
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupData, Winit.Modules.Store.Model.Classes.StoreGroupData>();

        //SKUGroupData
        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupData, Winit.Modules.SKU.Model.Classes.SKUGroupData>();

        // Syncing
        builder.Services.AddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroupEntity, Winit.Modules.Syncing.Model.Classes.TableGroupEntity>();

        //PromotionData
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromotionData, Winit.Modules.Promotion.Model.Classes.PromotionData>();


        builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList, Winit.Modules.SKU.Model.Classes.SKUPriceList>();


        //Route
        builder.Services.AddScoped<IRouteLoadViewModel, RouteLoadBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.Common.BL.Interfaces.IScopedService, Winit.Modules.Common.BL.Classes.ScopedService>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreBL, Winit.Modules.Store.BL.Classes.StoreBL>();
        builder.Services.AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteBL, Winit.Modules.Route.BL.Classes.RouteBL>();
        builder.Services.AddTransient<Winit.Modules.Route.Model.Interfaces.IRoute, Winit.Modules.Route.Model.Classes.Route>();
        builder.Services.AddTransient<Winit.Modules.Route.DL.Interfaces.IRouteDL, Winit.Modules.Route.DL.Classes.SQLiteRouteDL>();

        //LocationData
        builder.Services.AddScoped<Winit.Modules.Location.Model.Interfaces.ILocationData, Winit.Modules.Location.Model.Classes.LocationData>();


        // Emp
        builder.Services.AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmp, Winit.Modules.Emp.Model.Classes.Emp>();
        builder.Services.AddTransient<Winit.Modules.Emp.BL.Interfaces.IEmpBL, Winit.Modules.Emp.BL.Classes.EmpBL>();
        builder.Services.AddTransient<Winit.Modules.Emp.DL.Interfaces.IEmpDL, Winit.Modules.Emp.DL.Classes.SQLiteEmpDL>();


        // JobPosition
        builder.Services.AddTransient<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition, Winit.Modules.JobPosition.Model.Classes.JobPosition>();
        builder.Services.AddTransient<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance, Winit.Modules.JobPosition.Model.Classes.JobPositionAttendance>();
        builder.Services.AddTransient<Winit.Modules.JobPosition.BL.Interfaces.IJobPositionBL, Winit.Modules.JobPosition.BL.Classes.JobPositionBL>();
        builder.Services.AddTransient<Winit.Modules.JobPosition.DL.Interfaces.IJobPositionDL, Winit.Modules.JobPosition.DL.Classes.SQLiteJobPositionDL>();

        //DashBoard
        builder.Services.AddTransient<IDashBoardViewModel, Winit.Modules.DashBoard.BL.DashboardViewModel>();
        builder.Services.AddTransient<IEmpInfoBL, EmpInfoBL>();
        builder.Services.AddTransient<IEmpInfo, EmpInfo>();
        builder.Services.AddTransient<IEmpInfoDL, SQLiteEmpInfoDL>();
        builder.Services.AddTransient<IJPBeatHistory, JPBeatHistory>();
        builder.Services.AddTransient<UserConfig>();
        builder.Services.AddTransient<Winit.Modules.Vehicle.Model.Interfaces.IVehicle, Winit.Modules.Vehicle.Model.Classes.Vehicle>();
        builder.Services.AddTransient<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus, Winit.Modules.Vehicle.Model.Classes.VehicleStatus>();


        //StockAudit AddEditStockAuditAppViewModel
        builder.Services.AddTransient<Winit.Modules.StockAudit.BL.Interfaces.IAddEditStockAudit, Winit.Modules.StockAudit.BL.Classes.AddEditStockAuditAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.StockAudit.BL.Interfaces.IWHStockAuditBL, Winit.Modules.StockAudit.BL.Classes.WHStockAuditBL>();
        builder.Services.AddSingleton<Winit.Modules.StockAudit.DL.Interfaces.IWHStockAuditDL, Winit.Modules.StockAudit.DL.Classes.SQLiteStockAuditDL>();

        //Store
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreBL, Winit.Modules.Store.BL.Classes.StoreBL>();
        builder.Services.AddTransient<Winit.Modules.Store.DL.Interfaces.IStoreDL, Winit.Modules.Store.DL.Classes.SQLiteStoreDL>();

        //CommonServices
        //builder.Services.AddSingleton<Winit.UIComponents.Common.IAlertService, Winit.UIComponents.Common.AlertService>();

        //FileSys
        builder.Services.AddTransient<Winit.Modules.FileSys.BL.Interfaces.IFileSysBL, Winit.Modules.FileSys.BL.Classes.FileSysBL>();
        builder.Services.AddTransient<Winit.Modules.FileSys.DL.Interfaces.IFileSysDL, Winit.Modules.FileSys.DL.Classes.SQLiteFileSysDL>();
        builder.Services.AddTransient<Winit.Modules.FileSys.Model.Interfaces.IFileSys, Winit.Modules.FileSys.Model.Classes.FileSys>();

        //Promotion
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOrder, Winit.Modules.Promotion.Model.Classes.PromoOrder>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromotion, Winit.Modules.Promotion.Model.Classes.Promotion>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderItem, Winit.Modules.Promotion.Model.Classes.PromoOrderItem>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOffer, Winit.Modules.Promotion.Model.Classes.PromoOffer>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOfferItem, Winit.Modules.Promotion.Model.Classes.PromoOfferItem>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoCondition, Winit.Modules.Promotion.Model.Classes.PromoCondition>();
        builder.Services.AddTransient<Winit.Modules.Promotion.Model.Interfaces.IItemPromotionMap, Winit.Modules.Promotion.Model.Classes.ItemPromotionMap>();
        builder.Services.AddTransient<Winit.Modules.Promotion.DL.Interfaces.IPromotionDL, Winit.Modules.Promotion.DL.Classes.SQlitePromotionDL>();
        builder.Services.AddTransient<Winit.Modules.Promotion.BL.Interfaces.IPromotionBL, Winit.Modules.Promotion.BL.Classes.PromotionBL>();


        //Store Check 

        builder.Services.AddTransient<IStoreCheckHistoryView, Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView>();
        builder.Services.AddTransient<IStoreCheckItemHistoryViewList, Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistoryViewList>();
        builder.Services.AddTransient<IStoreCheckItemUomQty, Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty>();
        builder.Services.AddTransient<IStoreCheckItemExpiryDREHistory, Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory>();

        builder.Services.AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistory, Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory>();
        builder.Services.AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckGroupHistory, Winit.Modules.StoreCheck.Model.Classes.StoreCheckGroupHistory>();

        builder.Services.AddTransient<Winit.Modules.StoreCheck.BL.Interfaces.IAddEditStoreCheck, Winit.Modules.StoreCheck.BL.Classes.AddEditStoreCheckAppViewModel>();
        builder.Services.AddTransient<Winit.Modules.StoreCheck.BL.Interfaces.IStoreCheckBL, Winit.Modules.StoreCheck.BL.Classes.StoreCheckBL>();
        builder.Services.AddTransient<Winit.Modules.StoreCheck.DL.Interfaces.IStoreCheckDL, Winit.Modules.StoreCheck.DL.Classes.SQLiteStoreCheckDL>();
        builder.Services.AddTransient<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemView, Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemView>();


        // Tax
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITax, Winit.Modules.Tax.Model.Classes.Tax>();
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSlab, Winit.Modules.Tax.Model.Classes.TaxSlab>();
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxMaster, Winit.Modules.Tax.Model.Classes.TaxMaster>();
        builder.Services.AddTransient<Winit.Modules.Tax.BL.Interfaces.ITaxMasterBL, Winit.Modules.Tax.BL.Classes.TaxMasterBL>();
        builder.Services.AddTransient<Winit.Modules.Tax.DL.Interfaces.ITaxMasterDL, Winit.Modules.Tax.DL.Classes.SQLiteTaxMasterDL>();
        builder.Services.AddTransient<Winit.Modules.Tax.BL.Interfaces.ITaxCalculator, Winit.Modules.Tax.BL.Classes.TaxCalculator>();
        builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.IAppliedTax, Winit.Modules.Tax.Model.Classes.AppliedTax>();

        //Role
        builder.Services.TryAddTransient<Winit.Modules.Role.Model.Interfaces.IRole, Winit.Modules.Role.Model.Classes.Role>();
        builder.Services.TryAddTransient<Winit.Modules.Role.BL.Interfaces.IRoleBL, Winit.Modules.Role.BL.Classes.RoleBL>();
        builder.Services.TryAddTransient<Winit.Modules.Role.DL.Interfaces.IRoleDL, Winit.Modules.Role.DL.Classes.SQLiteRoleDL>();


        //Syncing
        builder.Services.TryAddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroup, Winit.Modules.Syncing.Model.Classes.TableGroup>();
        builder.Services.TryAddTransient<Winit.Modules.Syncing.Model.Interfaces.ITableGroupEntityView, Winit.Modules.Syncing.Model.Classes.TableGroupEntityView>();
        builder.Services.TryAddTransient<Winit.Modules.Syncing.Model.Interfaces.ISyncRequest, Winit.Modules.Syncing.Model.Classes.SyncRequest>();
        builder.Services.TryAddTransient<Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL, Winit.Modules.Syncing.BL.Classes.MobileDataSyncBL>();
        builder.Services.TryAddTransient<Winit.Modules.Syncing.DL.Interfaces.IMobileDataSyncDL, Winit.Modules.Syncing.DL.Classes.SqliteMobileDataSyncDL>();
        builder.Services.TryAddTransient<Winit.Modules.Common.BL.Interfaces.IDataSyncValidationServiceBL, Winit.Modules.Common.BL.Classes.DataSyncValidationServiceBL>();
        builder.Services.TryAddTransient<Winit.Modules.Common.DL.Interfaces.IDataSyncValidationDL, Winit.Modules.Common.DL.Classes.SQLiteDataSyncValidationDL>();
        


        //apprequest
        builder.Services.AddTransient<Winit.Modules.Syncing.BL.Interfaces.IAppRequestBL, Winit.Modules.Syncing.BL.Classes.AppRequestBL>();
        builder.Services.AddTransient<Winit.Modules.Syncing.DL.Interfaces.IAppRequestDL, Winit.Modules.Syncing.DL.Classes.SqliteAppRequestDL>();
        builder.Services.AddTransient<Winit.Modules.Syncing.Model.Interfaces.IAppRequest, Winit.Modules.Syncing.Model.Classes.AppRequest>();


        //Stock Updater
        builder.Services.AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger, Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>();
        builder.Services.AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable, Winit.Modules.StockUpdater.Model.Classes.WHStockAvailable>();
        builder.Services.AddTransient<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary, Winit.Modules.StockUpdater.Model.Classes.WHStockSummary>();
        builder.Services.AddTransient<Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL, Winit.Modules.StockUpdater.BL.Classes.StockUpdaterBL>();
        builder.Services.AddTransient<Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL, Winit.Modules.StockUpdater.DL.Classes.SQLiteStockUpdaterDL>();

        //Approval Engine
        builder.Services.AddTransient<Winit.Modules.ApprovalEngine.DL.Interfaces.IApprovalEngineDL, Winit.Modules.ApprovalEngine.DL.Classes.MSSQLApprovalEngineDL>();
        builder.Services.AddTransient<Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineBL, Winit.Modules.ApprovalEngine.BL.Classes.ApprovalEngineBL>();
        builder.Services.AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMaster, Winit.Modules.ApprovalEngine.Model.Classes.ApprovalRuleMaster>();

        //calender
        builder.Services.AddTransient<Winit.Modules.Calender.Models.Interfaces.ICalender, Winit.Modules.Calender.Models.Classes.Calender>();

        //PurChangeOrderHeader
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeaderItem>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderHeaderBL, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderHeaderBL>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IViewPurchaseOrderStatusViewModel, Winit.Modules.PurchaseOrder.BL.Classes.ViewPurchaseOrderStatusBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderLevelCalculator, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderLevelCalculator>();

        //PurchaseOrderTemplate
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IMaintainPurchaseOrderTemplateViewModel, Winit.Modules.PurchaseOrder.BL.Classes.MaintainPurchaseOrderTemplateBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IMaintainPurchaseOrderTemplateDataHelper, Winit.Modules.PurchaseOrder.BL.Classes.MaintainPurchaseOrderTemplateWebDataHelper>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateHeader, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateHeader>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateLine, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateLine>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateMaster, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateMaster>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IAddEditPurchaseOrderTemplateViewModel, Winit.Modules.PurchaseOrder.BL.Classes.AddEditPurchaseOrderTemplateBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IAddEditPurchaseOrderTemplateDataHelper, Winit.Modules.PurchaseOrder.BL.Classes.AddEditPurchaseOrderTemplateDataWebHelper>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderTemplateItemView>();

        //Purchase Order
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderViewModel, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderBaseViewModel>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderItemView>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderDataHelper, Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderDataWebHelper>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeader, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeader>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderMaster, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderMaster>();
        builder.Services.AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLine, Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLine>();
        builder.Services.AddTransient<Winit.Modules.SKU.BL.Interfaces.IAddProductPopUpDataHelper, Winit.Modules.SKU.BL.Classes.AddProductPopUpV1DataHelper>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreMaster, Winit.Modules.Store.Model.Classes.StoreMaster>();
        builder.Services.AddTransient<ISelectionItem, SelectionItem>();
        builder.Services.AddTransient<ISellInSchemeDTO, SellInSchemeDTO>();
        builder.Services.AddTransient<Winit.Modules.User.BL.Interface.IUserMasterBaseViewModel, Winit.Modules.User.BL.Classes.UserMasterWebViewModel>();
        builder.Services.AddTransient<Winit.Modules.User.Model.Interface.IUserMaster, Winit.Modules.User.Model.Classes.UserMaster>();

        //Customer Details
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardCustomerDTO,
                Winit.Modules.Store.Model.Classes.OnBoardCustomerDTO>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardEditCustomerDTO,
                Winit.Modules.Store.Model.Classes.OnBoardEditCustomerDTO>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview,
                Winit.Modules.Store.Model.Classes.OnBoardGridview>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRACSalesByYear1,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRACSalesByYear1>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRetailingCityMonthlySales1,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRetailingCityMonthlySales1>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IOnBoardGridview,
                Winit.Modules.Store.Model.Classes.OnBoardGridview>();

        builder.Services
    .AddTransient<Winit.Modules.FileSys.BL.Interfaces.IFilesysViewModel,
        Winit.Modules.FileSys.BL.Classes.FilesysWebviewModel>();

        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRetailingCityMonthlySales,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRetailingCityMonthlySales>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMIRACSalesByYear,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMIRACSalesByYear>();
        builder.Services
            .AddTransient<Winit.Modules.Contact.Model.Interfaces.IContact, Winit.Modules.Contact.Model.Classes.Contact>();
        builder.Services
            .AddTransient<Winit.Modules.Address.Model.Interfaces.IAddress, Winit.Modules.Address.Model.Classes.Address>();
        builder.Services
            .AddTransient<Winit.Modules.FileSys.Model.Interfaces.IFileSys, Winit.Modules.FileSys.Model.Classes.FileSys>();
        builder.Services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationData,
                Winit.Modules.Location.Model.Classes.LocationData>();
        builder.Services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationType,
                Winit.Modules.Location.Model.Classes.LocationType>();
        builder.Services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocation, Winit.Modules.Location.Model.Classes.Location>();
        builder.Services
            .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationMaster,
                Winit.Modules.Location.Model.Classes.LocationMaster>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfoCMI,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMI>();
        builder.Services
            .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo,
                Winit.Modules.Store.Model.Classes.StoreAdditionalInfo>();
        builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.ICustomerDetailsViewModel,
                Winit.Modules.Store.BL.Classes.CustomerDetailsWebViewModel>();
        builder.Services.AddTransient<Winit.Modules.Store.BL.Interfaces.ISelfRegistrationViewModel,
                Winit.Modules.Store.BL.Classes.SelfRegistrationBaseViewModel>();

        //builder.Services
        //    .AddScoped<Winit.Modules.Store.BL.Interfaces.IStorBaseViewModelForWeb, Winit.Modules.Store.BL.Classes.StorWebViewModelForWeb>();
        builder.Services.AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreShowroom, Winit.Modules.Store.Model.Classes.StoreShowroom>();
        builder.Services.AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreBanking, Winit.Modules.Store.Model.Classes.StoreBanking>();
        builder.Services.AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreBrandDealingIn,
                Winit.Modules.Store.Model.Classes.StoreBrandDealingIn>();
        builder.Services.AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreSignatory,
                Winit.Modules.Store.Model.Classes.StoreSignatory>();

        builder.Services
    .AddTransient<Winit.Modules.Distributor.BL.Interfaces.ICreateDistributorBaseViewModel,
        Winit.Modules.Distributor.BL.Classes.CreateDistributorBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.Distributor.BL.Interfaces.IMaintainDistributorBaseViewModel,
                Winit.Modules.Distributor.BL.Classes.MaintainDistributorBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.Distributor.BL.Interfaces.IDistributorAdminBaseViewModel,
                Winit.Modules.Distributor.BL.Classes.DistributorAdminBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.Distributor.Model.Interfaces.IDistributorToggle,
                Winit.Modules.Distributor.Model.Classes.DistributorToggle>();
        builder.Services
            .AddTransient<Winit.Modules.Distributor.Model.Interfaces.IDistributorAdminDTO,
                Winit.Modules.Distributor.Model.Classes.DistributorAdminDTO>();
        builder.Services.AddSingleton<Winit.UIComponents.SnackBar.IToast, Winit.UIComponents.SnackBar.Services.ToastService>();

        //po
        builder.Services
    .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeaderItem,
        Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeaderItem>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderHeaderBL,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderHeaderBL>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IViewPurchaseOrderStatusViewModel,
                Winit.Modules.PurchaseOrder.BL.Classes.ViewPurchaseOrderStatusBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderLevelCalculator,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderLevelCalculator>();

        //PurchaseOrderTemplate
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IMaintainPurchaseOrderTemplateViewModel,
                Winit.Modules.PurchaseOrder.BL.Classes.MaintainPurchaseOrderTemplateBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IMaintainPurchaseOrderTemplateDataHelper,
                Winit.Modules.PurchaseOrder.BL.Classes.MaintainPurchaseOrderTemplateWebDataHelper>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateHeader,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateHeader>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateLine,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateLine>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderTemplateMaster,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderTemplateMaster>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IAddEditPurchaseOrderTemplateViewModel,
                Winit.Modules.PurchaseOrder.BL.Classes.AddEditPurchaseOrderTemplateBaseViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IAddEditPurchaseOrderTemplateDataHelper,
                Winit.Modules.PurchaseOrder.BL.Classes.AddEditPurchaseOrderTemplateDataWebHelper>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderTemplateItemView,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderTemplateItemView>();
        builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IPurchaseOrderCreditLimitBufferRange,
        Winit.Modules.Store.Model.Classes.PurchaseOrderCreditLimitBufferRange>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderItemView,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderItemView>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderDataHelper,
                Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderDataWebHelper>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderHeader,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderHeader>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderMaster,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderMaster>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLine,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLine>();
        builder.Services
            .AddTransient<Winit.Modules.PurchaseOrder.Model.Interfaces.IPurchaseOrderLineProvision,
                Winit.Modules.PurchaseOrder.Model.Classes.PurchaseOrderLineProvision>();

        //invoice
        _ = builder.Services
    .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceHeaderView,
        Winit.Modules.Invoice.Model.Classes.InvoiceHeaderView>();
        _ = builder.Services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView,
                Winit.Modules.Invoice.Model.Classes.ProvisioningCreditNoteView>();
        _ = builder.Services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceLineView,
                Winit.Modules.Invoice.Model.Classes.InvoiceLineView>();
        _ = builder.Services
            .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IInvoiceMaster,
                Winit.Modules.Invoice.Model.Classes.InvoiceMaster>();
        _ = builder.Services
            .AddTransient<Winit.Modules.Invoice.BL.Interfaces.IInvoiceDataHelper,
                Winit.Modules.Invoice.BL.Classes.InvoiceWebDataHelper>();
        _ = builder.Services
            .AddTransient<Winit.Modules.Invoice.BL.Interfaces.IInvoiceViewModel,
                Winit.Modules.Invoice.BL.Classes.InvoiceBaseViewModel>();


        //Service And Call Registration
        builder.Services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.ICallRegistration,
                Winit.Modules.ServiceAndCallRegistration.Model.Classes.CallRegistration>();
        builder.Services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.BL.Interfaces.IServiceAndCallRegistrationViewModel,
                Winit.Modules.ServiceAndCallRegistration.BL.Classes.ServiceAndCallRegistrationWebViewModel>();
        builder.Services
            .AddTransient<Winit.Modules.ServiceAndCallRegistration.Model.Interfaces.IServiceRequestStatus,
                Winit.Modules.ServiceAndCallRegistration.Model.Classes.ServiceRequestStatus>();

        //Credit Limit
        builder.Services
            .AddTransient<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit,
                Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>();
        builder.Services.AddTransient<Winit.Modules.CreditLimit.BL.Interfaces.IMaintainTemporaryCreditEnhancementViewModel,
                Winit.Modules.CreditLimit.BL.Classes.MaintainTemporaryCreditEnhancementWebViewModel>();

        //Capture Competitor
        builder.Services
            .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor,
                Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>();
        builder.Services
            .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandMapping,
                Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandMapping>();
        builder.Services
            .AddTransient<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICategoryBrandCompetitorMapping,
                Winit.Modules.CaptureCompetitor.Model.Classes.CategoryBrandCompetitorMapping>();


        //ProductFeedback
        builder.Services
            .AddTransient<Winit.Modules.ProductFeedback.BL.Interfaces.IProductFeedbackViewModel,
                Winit.Modules.ProductFeedback.BL.Classes.ProductFeedbackAppViewModel>();


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

        // Selection Map Criteria
        builder.Services.AddTransient<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria, Winit.Modules.Mapping.Model.Classes.SelectionMapCriteria>();
        builder.Services.AddTransient<Winit.Modules.Mapping.DL.Interfaces.ISelectionMapCriteriaDL, Winit.Modules.Mapping.DL.Classes.SQLiteSelectionMapCriteriaDL>();
        builder.Services.AddTransient<Winit.Modules.Mapping.BL.Interfaces.ISelectionMapCriteriaBL, Winit.Modules.Mapping.BL.Classes.SelectionMapCriteriaBL>();
        builder.Services.AddTransient<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails, Winit.Modules.Mapping.Model.Classes.SelectionMapDetails>();
        builder.Services.AddTransient<Winit.Modules.Mapping.DL.Interfaces.ISelectionMapDetailsDL, Winit.Modules.Mapping.DL.Classes.SQLiteSelectionMapDetailsDL>();
        builder.Services.AddTransient<Winit.Modules.Mapping.BL.Interfaces.ISelectionMapDetailsBL, Winit.Modules.Mapping.BL.Classes.SelectionMapDetailsBL>();

        //Expiry Check
        builder.Services.AddTransient<Winit.Modules.ExpiryCheck.Model.Interfaces.IExpiryCheckExecution, Winit.Modules.ExpiryCheck.Model.Classes.ExpiryCheckExecution>();
        builder.Services.AddTransient<Winit.Modules.ExpiryCheck.Model.Interfaces.IExpiryCheckExecutionLine, Winit.Modules.ExpiryCheck.Model.Classes.ExpiryCheckExecutionLine>();
        builder.Services.AddTransient<Winit.Modules.ExpiryCheck.DL.Interfaces.IExpiryCheckExecutionDL, Winit.Modules.ExpiryCheck.DL.Classes.SQLiteExpiryCheckExecutionDL>();
        builder.Services.AddTransient<Winit.Modules.ExpiryCheck.BL.Interfaces.IExpiryCheckExecutionBL, Winit.Modules.ExpiryCheck.BL.Classes.ExpiryCheckExecutionBL>();
        builder.Services.AddTransient<Winit.Modules.ExpiryCheck.BL.Interfaces.IExpiryCheckViewModel, Winit.Modules.ExpiryCheck.BL.Classes.ExpiryCheckAppViewModel>();


        //Expiry Check
        builder.Services.AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecution, Winit.Modules.PO.Model.Classes.POExecution>();
        builder.Services.AddTransient<Winit.Modules.PO.Model.Interfaces.IPOExecutionLine, Winit.Modules.PO.Model.Classes.POExecutionLine>();
        builder.Services.AddTransient<Winit.Modules.PO.DL.Interfaces.IPOExecutionDL, Winit.Modules.PO.DL.Classes.SQLitePOExecutionDL>();
        builder.Services.AddTransient<Winit.Modules.PO.BL.Interfaces.IPOExecutionBL, Winit.Modules.PO.BL.Classes.POExecutionBL>();

        //Expiry Check
        builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IProductFeedback, Winit.Modules.Merchandiser.Model.Classes.ProductFeedback>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.DL.Interfaces.IProductFeedbackDL, Winit.Modules.Merchandiser.DL.Classes.SQLiteProductFeedbackDL>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.BL.Interfaces.IProductFeedbackBL, Winit.Modules.Merchandiser.BL.Classes.ProductFeedbackBL>();

        //Broadcast Initiative
        builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IBroadcastInitiative, Winit.Modules.Merchandiser.Model.Classes.BroadcastInitiative>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.DL.Interfaces.IBroadcastInitiativeDL, Winit.Modules.Merchandiser.DL.Classes.SQLiteBroadcastInitiativeDL>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.BL.Interfaces.IBroadcastInitiativeBL, Winit.Modules.Merchandiser.BL.Classes.BroadcastInitiativeBL>();

        //Product Sampling
        builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IProductSampling, Winit.Modules.Merchandiser.Model.Classes.ProductSampling>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.DL.Interfaces.IProductSamplingDL, Winit.Modules.Merchandiser.DL.Classes.SQLiteProductSamplingDL>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.BL.Interfaces.IProductSamplingBL, Winit.Modules.Merchandiser.BL.Classes.ProductSamplingBL>();

        //ROTA
        builder.Services.AddTransient<Winit.Modules.Merchandiser.Model.Interfaces.IROTAActivity, Winit.Modules.Merchandiser.Model.Classes.ROTAActivity>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.DL.Interfaces.IROTAActivityDL, Winit.Modules.Merchandiser.DL.Classes.SQLiteROTAActivityDL>();
        builder.Services.AddTransient<Winit.Modules.Merchandiser.BL.Interfaces.IROTAActivityBL, Winit.Modules.Merchandiser.BL.Classes.ROTAActivityBL>();

        //SKUClassGroupItems
        builder.Services.AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup, Winit.Modules.SKUClass.Model.Classes.SKUClassGroup>();
        builder.Services.AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems, Winit.Modules.SKUClass.Model.Classes.SKUClassGroupItems>();
        builder.Services.AddTransient<Winit.Modules.SKUClass.DL.Interfaces.ISKUClassGroupItemsDL, Winit.Modules.SKUClass.DL.Classes.SQLiteSKUClassGroupItemsDL>();
        builder.Services.AddTransient<Winit.Modules.SKUClass.BL.Interfaces.ISKUClassGroupItemsBL, Winit.Modules.SKUClass.BL.Classes.SKUClassGroupItemsBL>();

        builder.Services.AddTransient<IDeviceService,DeviceService>();
        builder.Services.AddTransient<IExceptionLog, ExceptionLog>();
        builder.Services.AddTransient<IStoreActivityHistory, StoreActivityHistory>();

        // Mobile App Version User Services
        builder.Services.AddTransient<Winit.Modules.Mobile.DL.Interfaces.IAppVersionUserDL, Winit.Modules.Mobile.DL.Classes.MSSQLAppVersionUserDL>();
        builder.Services.AddTransient<Winit.Modules.Mobile.BL.Interfaces.IAppVersionUserBL, Winit.Modules.Mobile.BL.Classes.AppVersionUserBL>();
        
        // API Services  
        builder.Services.AddTransient<WINITMobile.Services.AppVersionApiService>(serviceProvider =>
        {
            var apiService = serviceProvider.GetRequiredService<Winit.Modules.Base.BL.ApiService>();
            var appConfig = serviceProvider.GetRequiredService<Winit.Shared.Models.Common.IAppConfig>();
            return new WINITMobile.Services.AppVersionApiService(apiService, appConfig);
        });
        
        // Auth Services
        builder.Services.AddTransient<Winit.Modules.Auth.Model.Interfaces.IDeviceValidationResult, Winit.Modules.Auth.Model.Classes.DeviceValidationResult>();
        builder.Services.AddTransient<Winit.Modules.Auth.BL.Interfaces.IDeviceValidationServiceBL, Winit.Modules.Auth.BL.Classes.DeviceValidationServiceBL>();

        _ = builder.Services
            .AddTransient<EscalationMatrixDto>();

        //PageState
        SqlMapper.AddTypeHandler<decimal>(new DecimalTypeHandler());
        builder.Services.AddSingleton<PageState>();
        
        return builder.Build();
    }
}