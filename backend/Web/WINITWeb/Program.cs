global using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using System.Globalization;
using Winit.Modules.Auth.BL.Classes;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Common.UIState.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Route.BL.Classes;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIComponents.Common.Common.LocalStorage;
using WinIt;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string environment = builder.HostEnvironment.Environment;
//builder.Services.AddSingleton(new Winit.Shared.Models.Common.AppConfigs(environment));
builder.Services.AddSingleton<IAppConfig, AppConfigs>();
builder.Services.AddSingleton<Winit.Shared.CommonUtilities.Common.CommonFunctions>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddTransient<ISelectionItem, SelectionItem>();
builder.Services.AddTransient<WinitService>();
CultureInfo culture = new("ar");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.AddSingleton<WinIt.Shared.MemoryCleanupService>();
//builder.Services.AddSingleton<AppConfigs>(_ => new AppConfigs(environment));
builder.Services
    .AddTransient<Winit.Modules.Common.BL.Interfaces.ICommonMasterData,
        Winit.Modules.Common.BL.Classes.CommonMasterData>();

/// do not remove this line, it is required for ExcelPackage to work properly
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

//LocalStorage
builder.Services.AddScoped<Winit.Modules.Base.BL.ILocalStorageService, Winit.Modules.Base.BL.LocalStorageService>();
//PurChangeOrderHeader
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

// Register the PageStateService
builder.Services.AddSingleton<Winit.Modules.Common.UIState.Classes.BaseModuleState>();
builder.Services.AddSingleton<Winit.Modules.Common.UIState.Classes.NavigationHistoryService>();
builder.Services.AddTransient<WinIt.Pages.Base.PageStateHandler>();
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
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IQPSSchemePO, Winit.Modules.Scheme.Model.Classes.QPSSchemePO>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingSchemeDTO,
        Winit.Modules.Scheme.Model.Classes.StandingSchemeDTO>();

// Configure localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "LanguageResources");
builder.Services
    .AddSingleton<Winit.UIComponents.Common.Language.ILanguageService,
        Winit.UIComponents.Common.Language.LanguageService>();
builder.Services.AddSingleton<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus, Winit.Modules.Vehicle.Model.Classes.VehicleStatus>();
//Authentication
builder.Services.AddOptions();
builder.Services.AddTransient<Winit.Shared.CommonUtilities.Common.SHACommonFunctions>();
builder.Services.AddTransient<Winit.Modules.Auth.Model.Interfaces.IAuthMaster, Winit.Modules.Auth.Model.Classes.AuthMaster>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

//login
builder.Services
    .AddScoped<Winit.Modules.Auth.BL.Interfaces.ILoginViewModel, Winit.Modules.Auth.BL.Classes.LoginWebViewModel>();

//File Uploader
builder.Services
    .AddTransient<Winit.UIComponents.Common.FileUploader.IFileUploaderBaseViewModel,
        Winit.UIComponents.Common.FileUploader.FileUploaderBaseViewModel>();
builder.Services
    .AddTransient<Winit.FileUploader.IFileUploaderBaseViewModel, Winit.FileUploader.FileUploaderBaseViewModel>();

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

//CashDiscountExclude
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping,
        Winit.Modules.Scheme.Model.Classes.SchemeExcludeMapping>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeExcludeMappingViewModel,
        Winit.Modules.Scheme.BL.Classes.SchemeExcludeMappingWebViewModel>();

//storeasmmapping
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAsmMapping,
        Winit.Modules.Store.Model.Classes.StoreAsmMapping>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreAsmDivisionMappingViewModel,
        Winit.Modules.Store.BL.Classes.StoreAsmDivisionMappingWebViewModel>();

//tally
builder.Services
    .AddTransient<Winit.Modules.Tally.BL.Interfaces.ITallySKUMappingViewModel,
        Winit.Modules.Tally.BL.Classes.TallySKUMapping.TallySKUMappingWebViewModel>();


//Talley Master
builder.Services
    .AddTransient<Winit.Modules.Tally.BL.Interfaces.ITallyMasterViewModel,
        Winit.Modules.Tally.BL.Classes.TallyMasterWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyDealerMaster,
        Winit.Modules.Tally.Model.Classes.TallyDealerMaster>();
builder.Services
    .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallyInventoryMaster,
        Winit.Modules.Tally.Model.Classes.TallyInventoryMaster>();
builder.Services
    .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceMaster,
        Winit.Modules.Tally.Model.Classes.TallySalesInvoiceMaster>();
builder.Services
    .AddTransient<Winit.Modules.Tally.Model.Interfaces.ITallySalesInvoiceLineMaster,
        Winit.Modules.Tally.Model.Classes.TallySalesInvoiceLineMaster>();


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
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.ICustomerDetailsViewModel,
        Winit.Modules.Store.BL.Classes.CustomerDetailsWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.ISelfRegistrationViewModel,
        Winit.Modules.Store.BL.Classes.SelfRegistrationBaseViewModel>();
//citybranchmapping
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ICityBranch,
        Winit.Modules.Location.Model.Classes.CityBranch>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ICityBranchMapping,
        Winit.Modules.Location.Model.Classes.CityBranchMapping>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.ICityBranchMappingViewModel,
        Winit.Modules.Location.BL.Classes.CityBranchMappingWebViewModel>();
//Store
builder.Services
    .AddScoped<Winit.Modules.Store.BL.Interfaces.IStorBaseViewModelForWeb,
        Winit.Modules.Store.BL.Classes.StorWebViewModelForWeb>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreShowroom, Winit.Modules.Store.Model.Classes.StoreShowroom>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreBanking, Winit.Modules.Store.Model.Classes.StoreBanking>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreBrandDealingIn,
        Winit.Modules.Store.Model.Classes.StoreBrandDealingIn>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreSignatory,
        Winit.Modules.Store.Model.Classes.StoreSignatory>();
//AsmDivisionMapping
_ = builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping,
        Winit.Modules.Store.Model.Classes.AsmDivisionMapping>();

builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IOrgConfiguration,
        Winit.Modules.Store.Model.Classes.OrgConfiguration>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreCredit, Winit.Modules.Store.Model.Classes.StoreCredit>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IPurchaseOrderCreditLimitBufferRange,
        Winit.Modules.Store.Model.Classes.PurchaseOrderCreditLimitBufferRange>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreAttributes,
        Winit.Modules.Store.Model.Classes.StoreAttributes>();
builder.Services
    .AddScoped<Winit.Modules.Store.Model.Interfaces.IStoreMaster, Winit.Modules.Store.Model.Classes.StoreMaster>();
_ = builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCreditLimit,
        Winit.Modules.Store.Model.Classes.StoreCreditLimit>();

//Distributor
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

//Role
builder.Services
    .AddTransient<Winit.Modules.Role.BL.Interfaces.IMaintainUserRoleBaseViewMode,
        Winit.Modules.Role.BL.Classes.MaintainUserRoleBaseViewMode>();
builder.Services
    .AddTransient<Winit.Modules.Role.BL.Interfaces.ICreateUserRoleBaseViewModel,
        Winit.Modules.Role.BL.Classes.CreateUserRoleBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Role.BL.Interfaces.IMaintainWebMenuBaseViewModel,
        Winit.Modules.Role.BL.Classes.MaintainWebMenuBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Role.BL.Interfaces.IMaintainMobileMenuBaseViewModel,
        Winit.Modules.Role.BL.Classes.MaintainMobileMenuBaseViewModel>();
builder.Services.AddTransient<Winit.Modules.Role.BL.Interfaces.IMenu, Winit.Modules.Role.BL.Classes.WebMenu>();
builder.Services
    .AddTransient<Winit.Modules.Role.Model.Interfaces.IPermissionHeaderChecks,
        Winit.Modules.Role.Model.Classes.PermissionHeaderChecks>();
builder.Services
    .AddTransient<Winit.Modules.Role.Model.Interfaces.IModulesMasterView,
        Winit.Modules.Role.Model.Classes.ModulesMasterView>();
builder.Services
    .AddTransient<Winit.Modules.Role.Model.Interfaces.IModuleMaster, Winit.Modules.Role.Model.Classes.ModuleMaster>();
//builder.Services.AddSingleton<Winit.Modules.Role.Model.Interfaces.INavMenuModules, Winit.Modules.Role.Model.Classes.NavMenuModules>();
builder.Services
    .AddSingleton<Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView,
        Winit.Modules.Role.Model.Classes.MenuMasterHierarchyView>();
builder.Services
    .AddSingleton<Winit.Modules.Role.Model.Interfaces.IPermission, Winit.Modules.Role.Model.Classes.Permission>();
builder.Services
    .AddTransient<Winit.Modules.Role.BL.Interfaces.IPermissionDataHelper,
        Winit.Modules.Role.BL.Classes.PermissionDataHelper>();

//PRice List
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IMaintainCustomerPriceListBaseViewModel,
        Winit.Modules.SKU.BL.Classes.MaintainCustomerPriceListBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.ICreateCustomerPriceListBaseVieModel,
        Winit.Modules.SKU.BL.Classes.CreateCustomerPriceListWebVieModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IIndividualPriceListBaseViewModel,
        Winit.Modules.SKU.BL.Classes.IndividualPriceListBaseViewModel>();
builder.Services
    .AddTransient<Winit.UIComponents.Web.SalesManagement.PriceManagement.Services.ISKUPriceDataHelper,
        Winit.UIComponents.Web.SalesManagement.PriceManagement.Services.SKUPriceDataHelper>();
builder.Services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUPriceViewModel, Winit.Modules.SKU.BL.Classes.SKUPriceViewModel>();
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPriceView, Winit.Modules.SKU.Model.Classes.SKUPriceView>();
//SKUPriceLadder
builder.Services
    .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.ISKUPriceLadderingData,
        Winit.Modules.PriceLadder.Model.Classes.SKUPriceLadderingData>();
builder.Services
    .AddTransient<Winit.Modules.PriceLadder.BL.Interfaces.ILadderingCalculator,
        Winit.Modules.PriceLadder.BL.Classes.LadderingCalculator>();
builder.Services
    .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLadderingItemView,
        Winit.Modules.PriceLadder.Model.Classes.PriceLadderingItemView>();
builder.Services
    .AddTransient<Winit.Modules.PriceLadder.Model.Interfaces.IPriceLaddering,
        Winit.Modules.PriceLadder.Model.Classes.PriceLaddering>();
builder.Services
    .AddTransient<Winit.Modules.PriceLadder.BL.Interfaces.IPriceLadderingViewModel,
        Winit.Modules.PriceLadder.BL.Classes.PriceLadderingBaseViewModel>();

// for Routemanagement
builder.Services
    .AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteManagement,
        Winit.Modules.Route.BL.Classes.RouteManagementBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteMasterView,
        Winit.Modules.Route.Model.Classes.RouteMasterView>();
builder.Services.AddTransient<Winit.Modules.Route.Model.Interfaces.IRoute, Winit.Modules.Route.Model.Classes.Route>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteChangeLog,
        Winit.Modules.Route.Model.Classes.RouteChangeLog>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteCustomer,
        Winit.Modules.Route.Model.Classes.RouteCustomer>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteSchedule,
        Winit.Modules.Route.Model.Classes.RouteSchedule>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleDaywise,
        Winit.Modules.Route.Model.Classes.RouteScheduleDaywise>();
builder.Services
    .AddTransient<Winit.Modules.Route.Model.Interfaces.IRouteScheduleFortnight,
        Winit.Modules.Route.Model.Classes.RouteScheduleFortnight>();
builder.Services.AddTransient<Winit.Modules.Route.DL.Interfaces.IRouteDL, Winit.Modules.Route.DL.Classes.PGSQLRoute>();
builder.Services.AddTransient<Winit.Modules.Route.BL.Interfaces.IRouteBL, Winit.Modules.Route.BL.Classes.RouteBL>();


//ApprovalEngine
builder.Services
    .AddTransient<Winit.Modules.ApprovalEngine.BL.Interfaces.IApprovalEngineView,
        Winit.Modules.ApprovalEngine.BL.Classes.ApprovalEngineWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.ApprovalEngine.Model.Interfaces.IApprovalRuleMaster,
        Winit.Modules.ApprovalEngine.Model.Classes.ApprovalRuleMaster>();


builder.Services.AddSingleton<Winit.UIComponents.Common.IAlertService, Winit.UIComponents.Common.AlertService>();
//builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddTransient<HttpClient>();
builder.Services.AddHttpClient("WebAPI",
    client => client.BaseAddress = new Uri("https://netcoretest.winitsoftware.com/api/"));
// .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("WebAPI"));
builder.Services.AddScoped<Winit.Modules.Base.BL.ApiService>();


builder.Services
    .AddSingleton<Winit.Modules.Common.Model.Interfaces.IDataManager, Winit.Modules.Common.Model.Classes.DataManager>();
builder.Services.AddSingleton<Winit.Modules.Common.BL.Interfaces.IAppUser, Winit.Modules.Common.BL.Classes.AppUser>();

builder.Services
    .AddSingleton<Winit.Modules.JobPosition.Model.Interfaces.IJobPosition,
        Winit.Modules.JobPosition.Model.Classes.JobPosition>();
builder.Services.AddSingleton<IServiceProvider, ServiceProvider>();

//SnackBar
builder.Services.AddSingleton<Winit.UIComponents.SnackBar.IToast, Winit.UIComponents.SnackBar.Services.ToastService>();

// for loading
builder.Services
    .AddSingleton<Winit.UIComponents.Common.Services.ILoadingService,
        Winit.UIComponents.Common.Services.LoadingService>();

//Bread Crum
builder.Services
    .AddSingleton<Winit.UIModels.Web.Breadcrum.Interfaces.IDataService,
        Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel>();
builder.Services
    .AddTransient<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum,
        Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel>();

//need to remove later after all people changing there filter to pageheader and filter
builder.Services.AddTransient<WinIt.BreadCrum.Interfaces.IBreadCrum, WinIt.BreadCrum.Classes.BreadCrumModel>();
builder.Services.AddSingleton<WinIt.BreadCrum.Interfaces.IDataService, WinIt.BreadCrum.Classes.DataServiceModel>();

//DorpDownService
builder.Services
    .AddSingleton<Winit.UIComponents.Common.Services.IDropDownService,
        Winit.UIComponents.Common.Services.DropDownService>();

//return order
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder,
        Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine,
        Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderItemView,
        Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderItemView>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderWebViewModel,
        Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnSummaryWebViewModel,
        Winit.Modules.ReturnOrder.BL.Classes.ReturnSummaryWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView, Winit.Modules.SKU.Model.UIClasses.SKUUOMView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView,
        Winit.Modules.SKU.Model.UIClasses.SKUAttributeView>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderAmountCalculator,
        Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderAmountCalculator>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderViewModelFactory,
        Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderViewModelFactory>();
builder.Services
    .AddSingleton<Winit.Modules.Common.Model.Interfaces.IDataManager, Winit.Modules.Common.Model.Classes.DataManager>();

//PO return order
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IPOReturnOrderViewModel,
        Winit.Modules.ReturnOrder.BL.Classes.POReturnOrderBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.BL.Interfaces.IPOReturnOrderDataHelper,
        Winit.Modules.ReturnOrder.BL.Classes.POReturnOrderWebDataHelper>();
builder.Services
    .AddTransient<Winit.Modules.ReturnOrder.Model.Interfaces.IPOReturnOrderLineItem,
        Winit.Modules.ReturnOrder.Model.Classes.POReturnOrderLineItem>();

//Sales Order
//builder.Services.AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModel, Winit.Modules.SalesOrder.BL.UIClasses.VanSalesOrderViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModelFactory,
        Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderViewModelFactory>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView,
        Winit.Modules.SalesOrder.Model.UIClasses.SalesOrderItemView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUUOMView, Winit.Modules.SKU.Model.UIClasses.SKUUOMView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUAttributeView,
        Winit.Modules.SKU.Model.UIClasses.SKUAttributeView>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderAmountCalculator,
        Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderAmountCalculator>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine,
        Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.IOrderLevelCalculator,
        Winit.Modules.SalesOrder.BL.UIClasses.OrderLevelCalculator>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ICashDiscountCalculator,
        Winit.Modules.SalesOrder.BL.UIClasses.CashDiscountCalculator>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder,
        Winit.Modules.SalesOrder.Model.Classes.SalesOrder>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderDataHelper,
        Winit.Modules.SalesOrder.BL.UIClasses.SalesOrderDataWebHelper>();

//Purchase Order
builder.Services
    .AddTransient<Winit.Modules.PurchaseOrder.BL.Interfaces.IPurchaseOrderViewModel,
        Winit.Modules.PurchaseOrder.BL.Classes.PurchaseOrderBaseViewModel>();
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
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IAddProductPopUpDataHelper,
        Winit.Modules.SKU.BL.Classes.AddProductPopUpV1DataHelper>();
//User Master
builder.Services
    .AddTransient<Winit.Modules.User.Model.Interface.IUserMaster, Winit.Modules.User.Model.Classes.UserMaster>();
builder.Services
    .AddTransient<Winit.Modules.User.BL.Interface.IUserMasterBaseViewModel,
        Winit.Modules.User.BL.Classes.UserMasterWebViewModel>();
builder.Services.AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmp, Winit.Modules.Emp.Model.Classes.Emp>();
builder.Services
    .AddTransient<Winit.Modules.Currency.Model.Interfaces.IOrgCurrency,
        Winit.Modules.Currency.Model.Classes.OrgCurrency>();


//common helper
builder.Services
    .AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.IFilterHelper,
        Winit.Modules.Base.BL.Helper.Classes.FilterHelper>();
builder.Services
    .AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.ISortHelper,
        Winit.Modules.Base.BL.Helper.Classes.SortHelper>();
builder.Services
    .AddTransient<Winit.Modules.Base.BL.Helper.Interfaces.IListHelper,
        Winit.Modules.Base.BL.Helper.Classes.ListHelper>();

//DevliverySummery
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesSummaryViewModel,
        Winit.Modules.SalesOrder.BL.UIClasses.SalesSummaryBaseViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView,
        Winit.Modules.SalesOrder.Model.UIClasses.SalesSummaryItemView>();

//Store
builder.Services.AddTransient<Winit.Modules.Store.Model.Interfaces.IStore, Winit.Modules.Store.Model.Classes.Store>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreItemView,
        Winit.Modules.Store.Model.Classes.StoreItemView>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCustomer,
        Winit.Modules.Store.Model.Classes.StoreCustomer>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo,
        Winit.Modules.Store.Model.Classes.StoreAdditionalInfo>();

//Dashboard
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.ISalesPerformance,
    Winit.Modules.DashBoard.Model.Classes.SalesPerformance>();
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReportAsmview,
    Winit.Modules.DashBoard.Model.Classes.BranchSalesReportAsmview>();
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReportOrgview,
    Winit.Modules.DashBoard.Model.Classes.BranchSalesReportOrgview>();

builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.ICategoryPerformance,
    Winit.Modules.DashBoard.Model.Classes.CategoryPerformance>();
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IGrowthWiseChannelPartner,
    Winit.Modules.DashBoard.Model.Classes.GrowthWiseChannelPartner>();
builder.Services.AddTransient<Winit.Modules.DashBoard.BL.Interfaces.IDashboardReportViewmodel,
    Winit.Modules.DashBoard.BL.Classes.DashboardWebViewmodel>();
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IDistributorPerformance,
    Winit.Modules.DashBoard.Model.Classes.DistributorPerformance>();
builder.Services.AddTransient<Winit.Modules.DashBoard.BL.Interfaces.IBranchSalesReportViewModel,
    Winit.Modules.DashBoard.BL.Classes.BranchSalesReportWebViewModel>();
builder.Services.AddTransient<Winit.Modules.DashBoard.Model.Interfaces.IBranchSalesReport,
    Winit.Modules.DashBoard.Model.Classes.BranchSalesReport>();


//SkuPrice
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPrice, Winit.Modules.SKU.Model.Classes.SKUPrice>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUPriceList, Winit.Modules.SKU.Model.Classes.SKUPriceList>();

//Setting
builder.Services
    .AddSingleton<Winit.Modules.Setting.BL.Interfaces.IAppSetting, Winit.Modules.Setting.BL.Classes.AppSettings>();
builder.Services
    .AddTransient<Winit.Modules.Setting.Model.Interfaces.ISetting, Winit.Modules.Setting.Model.Classes.Setting>();

//SKU,SKUUOM
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKU, Winit.Modules.SKU.Model.Classes.SKUV1>();
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUV1, Winit.Modules.SKU.Model.Classes.SKUV1>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUConfig, Winit.Modules.SKU.Model.Classes.SKUConfig>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes, Winit.Modules.SKU.Model.Classes.SKUAttributes>();
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUUOM, Winit.Modules.SKU.Model.Classes.SKUUOM>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap, Winit.Modules.SKU.Model.Classes.TaxSkuMap>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUMaster, Winit.Modules.SKU.Model.Classes.SKUMaster>();
builder.Services.AddTransient<Winit.Modules.SKU.DL.Interfaces.ISKUDL, Winit.Modules.SKU.DL.Classes.PGSQLSKUDL>();
builder.Services.AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUBL, Winit.Modules.SKU.BL.Classes.SKUBL>();
builder.Services
    .AddTransient<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUField,
        Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUListView, Winit.Modules.SKU.Model.Classes.SKUListView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IMaintainSKUViewModel,
        Winit.Modules.SKU.BL.Classes.MaintainSKUWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IAddEditMaintainSKUViewModel,
        Winit.Modules.SKU.BL.Classes.AddEditMaintainSKUWebViewModel>();
//SKU mapping Template
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IManageUserSKUMappingTemplateBaseViewModel,
        Winit.Modules.SKU.BL.Classes.ManageUserSKUMappingTemplateWebViewModel>();


//Vehicle
builder.Services
    .AddTransient<Winit.Modules.Vehicle.Model.Interfaces.IVehicle, Winit.Modules.Vehicle.Model.Classes.Vehicle>();
builder.Services
    .AddTransient<Winit.Modules.Vehicle.BL.Interfaces.IMaintainVanViewModel,
        Winit.Modules.Vehicle.BL.Classes.MaintainVanWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Vehicle.BL.Interfaces.IAddEditMaintainVanViewModel,
        Winit.Modules.Vehicle.BL.Classes.AddEditMaintainVanWebViewModel>();

//WareHouse
builder.Services
    .AddTransient<Winit.Modules.Org.Model.Interfaces.IWarehouseItemView,
        Winit.Modules.Org.Model.Classes.WarehouseItemView>();
builder.Services
    .AddTransient<Winit.Modules.Org.BL.Interfaces.IMaintainWareHouseViewModel,
        Winit.Modules.Org.BL.Classes.MaintainWareHouseWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Org.BL.Interfaces.IAddEditMaintainWarehouseViewModel,
        Winit.Modules.Org.BL.Classes.AddEditMaintainWarehouseWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView,
        Winit.Modules.Org.Model.Classes.WarehouseStockItemView>();
builder.Services
    .AddTransient<Winit.Modules.Org.BL.Interfaces.IViewWareHouse_VanStockViewModel,
        Winit.Modules.Org.BL.Classes.ViewWareHouse_VanStockWebViewModel>();

//Currency
builder.Services
    .AddTransient<Winit.Modules.Currency.BL.Interfaces.IMaintainCurrencyViewModel,
        Winit.Modules.Currency.BL.Classes.MaintainCurrencyWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Currency.Model.Interfaces.ICurrency, Winit.Modules.Currency.Model.Classes.Currency>();


//Org
builder.Services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrg, Winit.Modules.Org.Model.Classes.Org>();
builder.Services
    .AddTransient<Winit.Modules.Org.BL.Interfaces.IOrgViewModel, Winit.Modules.Org.BL.Classes.OrgViewModel>();
builder.Services.AddTransient<Winit.Modules.Org.Model.Interfaces.IOrg, Winit.Modules.Org.Model.Classes.Org>();

//SKUGroup
builder.Services.AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroup, Winit.Modules.SKU.Model.Classes.SKUGroup>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView,
        Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUGroupViewModel,
        Winit.Modules.SKU.BL.Classes.SKUGroupWebViewModel>();
//IAddEditLoadRequest AddEditLoadRequestWebViewModel

//Tax
builder.Services
    .AddTransient<Winit.Modules.Tax.BL.Interfaces.ITaxCalculator, Winit.Modules.Tax.BL.Classes.TaxCalculator>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.UIInterfaces.ITaxItemView, Winit.Modules.Tax.Model.UIClasses.TaxItemView>();
builder.Services
    .AddTransient<Winit.Modules.Tax.BL.UIInterfaces.ITaxViewModel, Winit.Modules.Tax.BL.UIClasses.TaxWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap, Winit.Modules.Tax.Model.Classes.TaxSkuMap>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.UIInterfaces.ITaxSKUMapItemView,
        Winit.Modules.Tax.Model.UIClasses.TaxSKUMapItemView>();
builder.Services
    .AddTransient<Winit.Modules.Tax.BL.Interfaces.ITaxCalculator, Winit.Modules.Tax.BL.Classes.TaxCalculator>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.Interfaces.IAppliedTax, Winit.Modules.Tax.Model.Classes.AppliedTax>();
builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITax, Winit.Modules.Tax.Model.Classes.Tax>();


//TaxGroup
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.UIInterfaces.ITaxGroupItemView,
        Winit.Modules.Tax.Model.UIClasses.TaxGroupItemView>();
builder.Services
    .AddTransient<Winit.Modules.Tax.BL.UIInterfaces.ITaxGroupViewModel,
        Winit.Modules.Tax.BL.UIClasses.TaxGroupWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.UIInterfaces.ITaxGroupItemView,
        Winit.Modules.Tax.Model.UIClasses.TaxGroupItemView>();
builder.Services.AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroup, Winit.Modules.Tax.Model.Classes.TaxGroup>();
builder.Services
    .AddTransient<Winit.Modules.Tax.Model.Interfaces.ITaxGroupTaxes, Winit.Modules.Tax.Model.Classes.TaxGroupTaxes>();

//ListItem
builder.Services
    .AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListItem,
        Winit.Modules.ListHeader.Model.Classes.ListItem>();
builder.Services
    .AddTransient<Winit.Modules.ListHeader.Model.Interfaces.IListHeader,
        Winit.Modules.ListHeader.Model.Classes.ListHeader>();

//SKUClassGroup
builder.Services
    .AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup,
        Winit.Modules.SKUClass.Model.Classes.SKUClassGroup>();
builder.Services
    .AddTransient<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems,
        Winit.Modules.SKUClass.Model.Classes.SKUClassGroupItems>();
builder.Services
    .AddTransient<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView,
        Winit.Modules.SKUClass.Model.UIClasses.SKUClassGroupItemView>();
builder.Services
    .AddTransient<Winit.Modules.SKUClass.BL.UIInterfaces.ISKUClassGroupViewModel,
        Winit.Modules.SKUClass.BL.UIClasses.SKUClassGroupWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKUClass.BL.UIInterfaces.ISKUClassGroupItemsViewModel,
        Winit.Modules.SKUClass.BL.UIClasses.SKUClassGroupItemsWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKUClass.BL.UIInterfaces.ISKUClassGroupItemsViewModelV1,
        Winit.Modules.SKUClass.BL.UIClasses.SKUClassGroupItemsWebViewModelV1>();

//Location
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationItemView,
        Winit.Modules.Location.Model.Classes.LocationItemView>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationViewModel,
        Winit.Modules.Location.BL.Classes.LocationWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocation, Winit.Modules.Location.Model.Classes.Location>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationType,
        Winit.Modules.Location.Model.Classes.LocationType>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationMasterBaseViewModel,
        Winit.Modules.Location.BL.Classes.LocationMasterBaseViewModel>();
builder.Services
    .AddSingleton<Winit.UIComponents.Web.Location.Services.ILocationMasterService,
        Winit.UIComponents.Web.Location.Services.LocationMasterService>();
builder.Services
    .AddSingleton<Winit.UIComponents.Web.Location.Services.ILocationData,
        Winit.UIComponents.Web.Location.Services.LocationData>();

builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationMappingTemplateBaseViewModel,
        Winit.Modules.Location.BL.Classes.LocationMappingTemplateWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.IAddEditLocationMappingTemplateBaseViewModel,
        Winit.Modules.Location.BL.Classes.AddEditLocationMappingTemplateWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationTemplate,
        Winit.Modules.Location.Model.Classes.LocationTemplate>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationTemplateLine,
        Winit.Modules.Location.Model.Classes.LocationTemplateLine>();

//StoreGroup
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView,
        Winit.Modules.Store.Model.Classes.StoreGroupItemView>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreGroupViewModel,
        Winit.Modules.Store.BL.Classes.StoreGroupWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroup, Winit.Modules.Store.Model.Classes.StoreGroup>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupType,
        Winit.Modules.Store.Model.Classes.StoreGroupType>();

//SKuGroupType
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView,
        Winit.Modules.SKU.Model.Classes.SKUGroupTypeItemView>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.ISKUGroupTypeViewModel,
        Winit.Modules.SKU.BL.Classes.SKUGroupTypeWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType, Winit.Modules.SKU.Model.Classes.SKUGroupType>();

//LocationType
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView,
        Winit.Modules.Location.Model.Classes.LocationTypeItemView>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.ILocationTypeViewModel,
        Winit.Modules.Location.BL.Classes.LocationTypeWebViewModel>();

//StoreGroupType
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView,
        Winit.Modules.Store.Model.Classes.StoreGroupTypeItemView>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreGroupTypeViewModel,
        Winit.Modules.Store.BL.Classes.StoreGroupTypeWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.IMaintainCustomerViewModel,
        Winit.Modules.Store.BL.Classes.MaintainCustomerWebViewModel>();

//Promotions
builder.Services
    .AddTransient<Winit.Modules.Promotion.BL.UIInterfaces.IPromotionBase,
        Winit.Modules.Promotion.BL.UIClasses.PromotionBase>();
builder.Services
    .AddTransient<Winit.Modules.Promotion.BL.Interfaces.ICreatepromotionBaseViewModel,
        Winit.Modules.Promotion.BL.Classes.CreatePromotionBaseViewmodel>();
builder.Services
    .AddTransient<Winit.Modules.Promotion.Model.Interfaces.IPromoOrderForSlabs,
        Winit.Modules.Promotion.Model.Classes.PromoOrderForSlabs>();
builder.Services.AddTransient<PromoMasterView>();
builder.Services.AddTransient<PromotionView>();
builder.Services.AddTransient<PromoOrderView>();
builder.Services.AddTransient<PromoOrderItemView>();
builder.Services.AddTransient<PromoOfferView>();
builder.Services.AddTransient<PromoOfferItemView>();
builder.Services.AddTransient<PromoConditionView>();
builder.Services.AddTransient<ItemPromotionMapView>();

//Mapping
builder.Services
    .AddTransient<Winit.Modules.Mapping.BL.Interfaces.IMappingViewModel,
        Winit.Modules.Mapping.BL.Classes.MappingViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Mapping.Model.Interfaces.IMappingItemView,
        Winit.Modules.Mapping.Model.Classes.MappingItemView>();
//Price List

builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IMaintainStandardPriceListBaseViewModel,
        Winit.Modules.SKU.BL.Classes.MaintainStandardPriceListWebViewModel>();


//Route IRouteLoadTruckTemplateLine 
builder.Services
    .AddTransient<IRouteLoadTruckTemplateView, Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateView>();
builder.Services
    .AddTransient<IRouteLoadTruckTemplateLine, Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplateLine>();
builder.Services.AddTransient<IRouteLoadTruckTemplate, Winit.Modules.Route.Model.Classes.RouteLoadTruckTemplate>();
builder.Services
    .AddScoped<Winit.Modules.WHStock.DL.Interfaces.IWHStockDL, Winit.Modules.WHStock.DL.Classes.PGSQLWHStockDL>();
builder.Services.AddScoped<Winit.Modules.Route.BL.Interfaces.IRouteBL, Winit.Modules.Route.BL.Classes.RouteBL>();
builder.Services.AddScoped<Winit.Modules.Route.DL.Interfaces.IRouteDL, Winit.Modules.Route.DL.Classes.PGSQLRoute>();
//builder.Services.AddTransient<Winit.Modules.Route.BL.UIInterfaces.IRouteLoadBaseManagement, Winit.Modules.Route.BL.UIClasses.RouteLoadBaseManagement>();
builder.Services.AddTransient<IRouteLoadViewModel, RouteLoadBaseViewModel>();
builder.Services.AddScoped<IRouteLoadViewModel, RouteLoadViewModel>();
builder.Services.AddScoped<IAddEditRouteLoadViewModel, AddEditRouteLoadViewModel>();


//Product Sequencing
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IProductSequencingViewModel,
        Winit.Modules.SKU.BL.Classes.ProductSequencingViewModel>();


// Load Request

builder.Services
    .AddScoped<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView,
        Winit.Modules.WHStock.BL.Classes.LoadRequestWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.WHStock.BL.Interfaces.IAddEditLoadRequest,
        Winit.Modules.WHStock.BL.Classes.AddEditLoadRequestWebViewModel>();

//builder.Services.AddScoped<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView, Winit.Modules.WHStock.BL.Classes.LoadRequestBaseView>();

//builder.Services.AddScoped<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView, Winit.Modules.WHStock.BL.Classes.WebLoadRequestViewModel>();


builder.Services
    .AddScoped<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView,
        Winit.Modules.WHStock.BL.Classes.LoadRequestViewModel>();
builder.Services
    .AddScoped<Winit.Modules.StockUpdater.BL.Interfaces.IStockUpdaterBL,
        Winit.Modules.StockUpdater.BL.Classes.StockUpdaterBL>();
builder.Services
    .AddScoped<Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL,
        Winit.Modules.StockUpdater.DL.Classes.PGSQLStockUpdaterDL>();
//builder.Services.AddScoped<Winit.Modules.WHStock.BL.Interfaces.ILoadRequestView, Winit.Modules.WHStock.BL.Classes.LoadRequestWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.WHStock.BL.Interfaces.IAddEditLoadRequest,
        Winit.Modules.WHStock.BL.Classes.AddEditLoadRequestViewModel>();


//ManageSalesOrders
builder.Services
    .AddScoped<Winit.Modules.SalesOrder.BL.Interfaces.IManageSalesOrdersViewModel,
        Winit.Modules.SalesOrder.BL.Classes.ManageSalesOrdersWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales,
        Winit.Modules.SalesOrder.Model.Classes.DeliveredPreSales>();
//ClearData
builder.Services
    .AddTransient<Winit.Modules.Mobile.BL.Interfaces.IClearDataViewModel,
        Winit.Modules.Mobile.BL.Classes.ClearDataWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction,
        Winit.Modules.Mobile.Model.Classes.MobileAppAction>();
//DeviceManagement
builder.Services
    .AddScoped<Winit.Modules.Mobile.BL.Interfaces.IDeviceManagementViewModel,
        Winit.Modules.Mobile.BL.Classes.DeviceManagementWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.Mobile.Model.Interfaces.IAppVersionUser,
        Winit.Modules.Mobile.Model.Classes.AppVersionUser>();
builder.Services
    .AddTransient<Winit.Modules.WHStock.BL.Interfaces.IWHStockBL, Winit.Modules.WHStock.BL.Classes.WHStockBL>();

//News Activity
builder.Services
    .AddTransient<Winit.Modules.NewsActivity.BL.Interfaces.INewsActivityViewModel,
        Winit.Modules.NewsActivity.BL.Classes.NewsActivityWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.NewsActivity.Models.Interfaces.INewsActivity,
        Winit.Modules.NewsActivity.Models.Classes.NewsActivity>();
builder.Services
    .AddTransient<Winit.Modules.NewsActivity.BL.Interfaces.IManageNewsActivityViewModel,
        Winit.Modules.NewsActivity.BL.Classes.ManageNewsActivityWebViewModel>();

//MaintainSettings
builder.Services
    .AddTransient<Winit.Modules.Setting.BL.Interfaces.IMaintainSettingViewModel,
        Winit.Modules.Setting.BL.Classes.MaintainSettingWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.Setting.Model.Interfaces.ISetting, Winit.Modules.Setting.Model.Classes.Setting>();
//View Reasons
builder.Services
    .AddScoped<Winit.Modules.ListHeader.BL.Interfaces.IViewReasonsViewModel,
        Winit.Modules.ListHeader.BL.Classes.ViewReasonsWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.ListHeader.Model.Interfaces.IListHeader,
        Winit.Modules.ListHeader.Model.Classes.ListHeader>();
//View Today's Journey Plan
builder.Services
    .AddTransient<Winit.Modules.JourneyPlan.BL.Interfaces.IViewTodayJourneyPlanViewModel,
        Winit.Modules.JourneyPlan.BL.Classes.ViewTodayJourneyPlanWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.Model.Interfaces.IAssignedJourneyPlan,
        Winit.Modules.JourneyPlan.Model.Classes.AssignedJourneyPlan>();

//Maintain Users
builder.Services
    .AddTransient<Winit.Modules.User.BL.Interfaces.IMaintainUsersViewModel,
        Winit.Modules.User.BL.Classes.MaintainUsersWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.User.Model.Interfaces.IMaintainUser, Winit.Modules.User.Model.Classes.MaintainUser>();
builder.Services.AddTransient<Winit.Modules.User.Model.Interfaces.IEmpDTO, Winit.Modules.User.Model.Classes.EmpDTO>();
builder.Services.AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmp, Winit.Modules.Emp.Model.Classes.Emp>();
builder.Services.AddScoped<Winit.Modules.Emp.Model.Interfaces.IEmpInfo, Winit.Modules.Emp.Model.Classes.EmpInfo>();
builder.Services
    .AddTransient<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping, Winit.Modules.Emp.Model.Classes.EmpOrgMapping>();
builder.Services
    .AddScoped<Winit.Modules.User.BL.Interfaces.IAddEditEmployeeViewModel,
        Winit.Modules.User.BL.Classes.AddEditEmployeeWebViewModel>();
builder.Services.AddScoped<Winit.Modules.Role.Model.Interfaces.IRole, Winit.Modules.Role.Model.Classes.Role>();
builder.Services.AddScoped<Winit.Modules.Org.Model.Interfaces.IOrg, Winit.Modules.Org.Model.Classes.Org>();
builder.Services
    .AddScoped<Winit.Modules.User.Model.Interface.IUserLocationMapping,
        Winit.Modules.User.Model.Classes.UserLocationMapping>();
builder.Services
    .AddTransient<Winit.Modules.User.BL.Interface.IUserLocationMappingBaseViewModel,
        Winit.Modules.User.BL.Classes.UserLocationMappingWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.SKU.BL.Interfaces.IAddEditSKUMappingTemplateBaseViewModel,
        Winit.Modules.SKU.BL.Classes.AddEditSKUMappingTemplateWebViewModel>();

builder.Services
    .AddScoped<Winit.Modules.User.Model.Interfaces.IUserRoles, Winit.Modules.User.Model.Classes.UserRoles>();
builder.Services
    .AddScoped<Winit.Modules.User.Model.Interfaces.IUserFranchiseeMapping,
        Winit.Modules.User.Model.Classes.UserFranchiseeMapping>();
builder.Services
    .AddScoped<Winit.Modules.Auth.BL.Interfaces.ILoginViewModel, Winit.Modules.Auth.BL.Classes.LoginWebViewModel>();

//Re Open Journey Plan
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.BL.Interfaces.IReOpenJourneyPlanViewModel,
        Winit.Modules.JourneyPlan.BL.Classes.ReOpenJourneyPlanWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney,
        Winit.Modules.JourneyPlan.Model.Classes.UserJourney>();
//User Journey & Attendance Report
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.BL.Interfaces.IUserJourney_AttendanceReportViewModel,
        Winit.Modules.JourneyPlan.BL.Classes.UserJourney_AttendanceReportWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyGrid,
        Winit.Modules.JourneyPlan.Model.Classes.UserJourneyGrid>();
builder.Services
    .AddScoped<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourneyView,
        Winit.Modules.JourneyPlan.Model.Classes.UserJourneyView>();

//Store Check Report
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.IStoreCheckReportViewModel,
        Winit.Modules.Store.BL.Classes.StoreCheckReportWebViewModel>();

builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCheckReport,
        Winit.Modules.Store.Model.Classes.StoreCheckReport>();
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.IStoreCheckReportItem,
        Winit.Modules.Store.Model.Classes.StoreCheckReportItem>();

//View Bank Details
builder.Services
    .AddTransient<Winit.Modules.Bank.BL.Interfaces.IViewBankDetailsViewModel,
        Winit.Modules.Bank.BL.Classes.ViewBankDetailsWebViewModel>();
builder.Services.AddTransient<Winit.Modules.Bank.Model.Interfaces.IBank, Winit.Modules.Bank.Model.Classes.Bank>();
builder.Services
    .AddScoped<Winit.Modules.Auth.Model.Interfaces.ILoginResponse, Winit.Modules.Auth.Model.Classes.LoginResponse>();
builder.Services.AddScoped<ApiResponse<Winit.Modules.Auth.Model.Interfaces.ILoginResponse>>();

//View Error Details
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.BL.Interfaces.IViewErrorDetailsViewModel,
        Winit.Modules.ErrorHandling.BL.Classes.ViewErrorDetailsWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail,
        Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>();

//Error Description Details
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.BL.Interfaces.IErrorDescriptionViewModel,
        Winit.Modules.ErrorHandling.BL.Classes.ErrorDescriptionWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail,
        Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>();

//Add Edit Error Deetails Details
builder.Services
    .AddTransient<Winit.Modules.ErrorHandling.BL.Interfaces.IAddEditMaintainErrorViewModel,
        Winit.Modules.ErrorHandling.BL.Classes.AddEditMaintainErrorWebViewModel>();

//Add Edit Error Description Details
builder.Services
    .AddTransient<Winit.Modules.ErrorHandling.BL.Interfaces.IAddEditMaintainErrorDescriptionViewModel,
        Winit.Modules.ErrorHandling.BL.Classes.AddEditMaintainErrorDescriptionWebViewModel>();

//firebasereports
builder.Services
    .AddTransient<Winit.Modules.FirebaseReport.BL.Interfaces.IFirebaseReportViewModel,
        Winit.Modules.FirebaseReport.BL.Classes.FirebaseReportWebViewModel>();

//Tally
//builder.Services.AddScoped<Winit.Modules.Tally.BL.Interfaces.ITallySKUMappingViewModel, Winit.Modules.Tally.BL.Classes.TallySKUMapping.TallySKUMappingWebViewModel  >();


//collection
//builder.Services.AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.ICollectionViewModel, Winit.Modules.CollectionModule.BL.Classes.CollectionWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection,
        Winit.Modules.CollectionModule.Model.Classes.AccCollection>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IViewPaymentsViewModel,
        Winit.Modules.CollectionModule.BL.Classes.ViewPayments.ViewPaymentsWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.ICreatePaymentViewModel,
        Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.ICashSettlementViewModel,
        Winit.Modules.CollectionModule.BL.Classes.CashSettlement.CashSettlementWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.INonCashSettlementViewModel, Winit.Modules.CollectionModule.
        BL.Classes.NonCashSettlement.NonCashSettlementWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IPendingPaymentViewModel, Winit.Modules.CollectionModule.BL.
        Classes.NonCashSettlement.PendingPayment.PendingPaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.ISettlePaymentViewModel, Winit.Modules.CollectionModule.BL.
        Classes.NonCashSettlement.SettlePayment.SettlePaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IApprovePaymentViewModel, Winit.Modules.CollectionModule.BL.
        Classes.NonCashSettlement.ApprovePayment.ApprovePaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IBouncePaymentViewModel, Winit.Modules.CollectionModule.BL.
        Classes.NonCashSettlement.BouncePayment.BouncePaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IRejectPaymentViewModel, Winit.Modules.CollectionModule.BL.
        Classes.NonCashSettlement.RejectPayment.RejectPaymentWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IAccountStatementViewModel,
        Winit.Modules.CollectionModule.BL.Classes.Statement.AccountStatementWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IEarlyPaymentConfigurationViewModel, Winit.Modules.
        CollectionModule.BL.Classes.EarlyPaymentConfiguration.EarlyPaymentConfigurationWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.ICashCollectionDepositViewModel, Winit.Modules.
        CollectionModule.BL.Classes.CashCollectionDeposit.CashCollectionDepositWebViewModel>();
builder.Services
    .AddScoped<Winit.Modules.CollectionModule.BL.Interfaces.IBalanceConfirmationViewModel, Winit.Modules.
        CollectionModule.BL.Classes.BalanceConfirmation.BalanceConfirmationWebViewModel>();


//SelfRegistration
builder.Services
    .AddTransient<Winit.Modules.Store.Model.Interfaces.ISelfRegistration,
        Winit.Modules.Store.Model.Classes.SelfRegistration>();
builder.Services
    .AddTransient<Winit.Modules.Store.BL.Interfaces.ISelfRegistrationBL,
        Winit.Modules.Store.BL.Classes.SelfRegistrationBL>();


//Scheme
builder.Services
    .AddTransient<Winit.UIModels.Web.Schema.Interfaces.IAddSelloutSecondaryRealSchemeToggle,
        Winit.UIModels.Web.Schema.Classes.AddSelloutSecondaryRealSchemeToggle>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IManageStandingProvisionViewModel,
        Winit.Modules.Scheme.BL.Classes.ManageStandingProvisionWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISellInSchemeViewModel,
        Winit.Modules.Scheme.BL.Classes.SellInSchemeWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ICreateStandingConfigurationViewModel,
        Winit.Modules.Scheme.BL.Classes.CreateStandingConfigurationWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IQPSOrSelloutrealSecondarySchemeViewModel,
        Winit.Modules.Scheme.BL.Classes.QPSOrSelloutrealSecondarySchemeWebviewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeViewModelBase,
        Winit.Modules.Scheme.BL.Classes.SchemeViewModelBase>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.IManageSchemeViewModel,
        Winit.Modules.Scheme.BL.Classes.ManageSchemeWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISchemeApprovalEngineBaseViewModel,
        Winit.Modules.Scheme.BL.Classes.SchemeApprovalEngineBaseViewModel>();

builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeExtendHistory,
        Winit.Modules.Scheme.Model.Classes.SchemeExtendHistory>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeMaster,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeMaster>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionScheme,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionScheme>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeBranch,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeBranch>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeSlab, Winit.Modules.Scheme.Model.Classes.SchemeSlab>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IWallet, Winit.Modules.Scheme.Model.Classes.Wallet>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISalesPromotionScheme,
        Winit.Modules.Scheme.Model.Classes.SalesPromotionScheme>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeHeader,
        Winit.Modules.Scheme.Model.Classes.SellInSchemeHeader>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeLine,
        Winit.Modules.Scheme.Model.Classes.SellInSchemeLine>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutSchemeHeader,
        Winit.Modules.Scheme.Model.Classes.SellOutSchemeHeader>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutSchemeLine,
        Winit.Modules.Scheme.Model.Classes.SellOutSchemeLine>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger,
        Winit.Modules.Scheme.Model.Classes.WalletLedger>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemeDTO,
        Winit.Modules.Scheme.Model.Classes.SellInSchemeDTO>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IManageScheme,
        Winit.Modules.Scheme.Model.Classes.ManageScheme>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvision,
        Winit.Modules.Scheme.Model.Classes.StandingProvision>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IManageScheme,
        Winit.Modules.Scheme.Model.Classes.ManageScheme>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeDivision,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeDivision>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeOrg, Winit.Modules.Scheme.Model.Classes.SchemeOrg>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeBroadClassification,
        Winit.Modules.Scheme.Model.Classes.SchemeBroadClassification>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISchemeBranch,
        Winit.Modules.Scheme.Model.Classes.SchemeBranch>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellInSchemePO,
        Winit.Modules.Scheme.Model.Classes.SellInSchemePO>();

builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISellOutSchemeHeaderItemViewModel,
        Winit.Modules.Scheme.BL.Classes.SellOutSchemeWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISellOutMasterScheme,
        Winit.Modules.Scheme.Model.Classes.SellOutMasterScheme>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.BL.Interfaces.ISalesPromotionSchemeViewModel,
        Winit.Modules.Scheme.BL.Classes.SalesPromotionWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeApplicableOrg,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeApplicableOrg>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IStandingProvisionSchemeBroadClassification,
        Winit.Modules.Scheme.Model.Classes.StandingProvisionSchemeBroadClassification>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.IPreviousOrders,
        Winit.Modules.Scheme.Model.Classes.PreviousOrders>();
builder.Services
    .AddTransient<Winit.Modules.Scheme.Model.Interfaces.ISerialNumbers,
        Winit.Modules.Scheme.Model.Classes.SerialNumbers>();
builder.Services
    .AddTransient<Winit.Modules.Calender.Models.Interfaces.ICalender, Winit.Modules.Calender.Models.Classes.Calender>();


// Broad Classification
builder.Services
    .AddTransient<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader,
        Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeader>();
builder.Services
    .AddTransient<Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationHeaderViewModel,
        Winit.Modules.BroadClassification.BL.Classes.BroadClassificationHeaderWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.BroadClassification.BL.Interfaces.IBroadClassificationLineViewModel,
        Winit.Modules.BroadClassification.BL.Classes.BroadClassificationLineWebViewModel>();

//Branch Mapping
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.IBranch, Winit.Modules.Location.Model.Classes.Branch>();
builder.Services
    .AddTransient<Winit.Modules.Location.BL.Interfaces.IMaintainBranchMappingViewModel,
        Winit.Modules.Location.BL.Classes.MaintainBranchMappingWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Location.Model.Interfaces.ISalesOffice,
        Winit.Modules.Location.Model.Classes.SalesOffice>();

//Credit Limit
builder.Services
    .AddTransient<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit,
        Winit.Modules.CreditLimit.Model.Classes.TemporaryCredit>();
builder.Services
    .AddTransient<Winit.Modules.CreditLimit.BL.Interfaces.IMaintainTemporaryCreditEnhancementViewModel,
        Winit.Modules.CreditLimit.BL.Classes.MaintainTemporaryCreditEnhancementWebViewModel>();


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

//Outstanding Invouce -Collection 
builder.Services
    .AddTransient<Winit.Modules.CollectionModule.BL.Interfaces.IOutStandingInvoicesViewModel,
        Winit.Modules.CollectionModule.BL.Classes.OutStandingInvoice.OutStandingInvoicesWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableCMI,
        Winit.Modules.CollectionModule.Model.Classes.AccPayableCMI>();
builder.Services
    .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableMaster,
        Winit.Modules.CollectionModule.Model.Classes.AccPayableMaster>();
builder.Services
    .AddTransient<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayableView,
        Winit.Modules.CollectionModule.Model.Classes.AccPayableView>();


//builder.Services.AddTransient<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeaderViewModel, Winit.Modules.BroadClassification.Model.Classes.BroadClassificationHeaderWebViewModel>();


//ProvisioningCreditNote
builder.Services
    .AddTransient<Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView,
        Winit.Modules.Invoice.Model.Classes.ProvisioningCreditNoteView>();
builder.Services
    .AddTransient<Winit.Modules.Invoice.BL.Interfaces.IProvisioningCreditNoteViewModel,
        Winit.Modules.Invoice.BL.Classes.ProvisioningCreditNoteBaseViewModel>();


//Provisioning
builder.Services
    .AddTransient<Winit.Modules.Provisioning.BL.Interfaces.IProvisioningHeaderViewViewModel,
        Winit.Modules.Provisioning.BL.Classes.ProvisioningHeaderViewWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Provisioning.BL.Interfaces.IProvisioningItemViewViewModel,
        Winit.Modules.Provisioning.BL.Classes.ProvisioningItemViewWebViewModel>();
builder.Services
    .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionHeaderView,
        Winit.Modules.Provisioning.Model.Classes.ProvisionHeaderView>();
builder.Services
    .AddTransient<Winit.Modules.Provisioning.Model.Interfaces.IProvisionItemView,
        Winit.Modules.Provisioning.Model.Classes.ProvisionItemView>();

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

//AuditTrial Injection
builder.Services
    .AddTransient<Winit.Modules.AuditTrail.BL.Interfaces.IAuditTrailView,
        Winit.Modules.AuditTrail.BL.Classes.AuditTrailWebViewModel>();
//Provision Comparision Report Injection
builder.Services
    .AddTransient<Winit.Modules.ProvisionComparisonReport.BL.Interfaces.IProvisionComparisonReportsView,
        Winit.Modules.ProvisionComparisonReport.BL.Classes.ProvisionComparisonReportsWebViewModel>();


////indexedDb
//builder.Services.AddTransient<WinIt.Services.IndexedDbAccessor>();

//var host = builder.Build();
//using var scope = host.Services.CreateScope();
//await using var indexedDB = scope.ServiceProvider.GetService<IndexedDbAccessor>();

//if (indexedDB is not null
//{
//    await indexedDB.InitializeAsync();
//}
//await host.RunAsync();

////indexedDb
//builder.Services.AddTransient<WinIt.Services.IndexedDbAccessor>();

//var host = builder.Build();
//using var scope = host.Services.CreateScope();
//await using var indexedDB = scope.ServiceProvider.GetService<IndexedDbAccessor>();

//if (indexedDB is not null)
//{
//    await indexedDB.InitializeAsync();
//}
//await host.RunAsync();


//MobileApp
// Register Blazor.ECharts
// Add JS Interop for ECharts manually
builder.Services.AddScoped<Blazor.ECharts.JsInterop>();

#region KnowledgeBase

builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.Model.Interfaces.IErrorDetail,
        Winit.Modules.ErrorHandling.Model.Classes.ErrorDetail>();
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.DL.Interfaces.IKnowledgeBaseDL,
        Winit.Modules.ErrorHandling.DL.Classes.PGSQLKnowledgeBaseDL>();
builder.Services
    .AddScoped<Winit.Modules.ErrorHandling.BL.Interfaces.IKnowledgeBaseBL,
        Winit.Modules.ErrorHandling.BL.Classes.KnowledgeBaseBL>();
builder.Services
    .AddSingleton<Winit.Modules.ErrorHandling.BL.Interfaces.IErrorHandlerBL,
        Winit.Modules.ErrorHandling.BL.Classes.ErrorHandlerBL>();

//// Build the service provider
//var serviceProvider = builder.Services.BuildServiceProvider();

//// Retrieve an instance of KnowledgeBaseBL
//var knowledgeBaseBL = serviceProvider.GetRequiredService<IKnowledgeBaseBL>();

//// Load error details into dictionary at application startup
//await knowledgeBaseBL.GetErrorDetailsAsync();

#endregion

#region

#endregion

// Register your services
builder.Services.AddScoped<InterfaceTypeResolver>(); // Register the converter as a scoped service
builder.Services.AddScoped(serviceProvider =>
{
    var settings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Converters =
        {
            serviceProvider.GetRequiredService<InterfaceTypeResolver>()
        }
    };
    return settings;
});


// Register StateManagerService
builder.Services.AddScoped<IStateManagerService, StateManagerService>();
// Register module states
builder.Services.AddScoped<IClearableState, BaseModuleState>();
//builder.Services.AddScoped<IClearableState>(sp => sp.GetRequiredService<IPurchaseOrderModuleState>()); // If multiple class we can use this

// Repeat for other modules...


await builder.Build().RunAsync();