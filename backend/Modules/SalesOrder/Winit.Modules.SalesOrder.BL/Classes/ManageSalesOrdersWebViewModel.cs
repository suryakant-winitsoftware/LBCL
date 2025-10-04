using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.Classes
{
    public class ManageSalesOrdersWebViewModel:ManageSalesOrdersBaseViewModel
    {
        private IServiceProvider _serviceProvider;
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private List<string> _propertiesToSearch = new List<string>();
        private readonly IListHelper _listHelper;
        private readonly IAppUser _appUser;
        private readonly IAppSetting _appSetting;
        private readonly IDataManager _dataManager;
        private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        public ManageSalesOrdersWebViewModel(IServiceProvider serviceProvider,
               IFilterHelper filter,
               ISortHelper sorter,
               IListHelper listHelper,
               IAppUser appUser,
               IAppSetting appSetting,
               IDataManager dataManager,
               IAppConfig appConfigs,
               Base.BL.ApiService apiService
           ) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager, appConfigs, apiService)
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _appUser = appUser;
            _appSetting = appSetting;
            _dataManager = dataManager;
            _appConfigs = appConfigs;
            _apiService = apiService;
            ManageSalesOrderList = new List<Model.Interfaces.IDeliveredPreSales>();
            salesOrderFilterCriterials = new List<FilterCriteria>();
            SKUViewPreSalesList = new List<IViewPreSales>();
            RouteSelectionItems = new List<ISelectionItem>();
        }
        public override async Task PopulateViewModel(string selectedTab=null)
        {
            await base.PopulateViewModel(selectedTab);
            //RoutesListDD = await GetRouteDropdownDataFromAPIAsync(OrgUID);
            //if (RoutesListDD != null)
            //{
            //    RouteSelectionItems.Clear();
            //    RouteSelectionItems.AddRange(ConvertRouteToSelectionItem(RoutesListDD));
            //}
        }
        #region Business Logics  
        //private List<ISelectionItem> ConvertRouteToSelectionItem(IEnumerable<Winit.Modules.Route.Model.Interfaces.IRoute> routeitem)
        //{
        //    List<ISelectionItem> selectionItems = new List<ISelectionItem>();
        //    foreach (var route in routeitem)
        //    {
        //        SelectionItem si = new SelectionItem();
        //        // si.Code = org.Code;
        //        si.Label = route.Name;
        //        si.UID = route.UID;
        //        selectionItems.Add(si);
        //    }
        //    return selectionItems;
        //}
        #endregion
        #region Database or Services Methods
        public override async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales>> GetManageSalesOrdersData(string selectedtab)
        {
            return await GetManageSalesOrdersDataFromAPIAsnyc(selectedtab);
        }
        public override async Task<Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales> GetManageSalesOrdersDataforView(string salesorderUID)
        {
            return await GetManageSalesOrdersDataforViewFromAPIAsnyc(salesorderUID);
        }
        public override async Task<List<ISelectionItem>> GetDistributorData()
        {
            return await GetDistributorDataFromAPIAsync();
        }
        public override async Task<List<ISelectionItem>> GetSalesmanData(string OrgUID)
        {
            return await GetSalesmanDataFromAPIAsync(OrgUID);
        }
        public override async Task<List<ISelectionItem>> GetRouteData(string OrgUID)
        {
            return await GetRouteDataFromAPIAsync(OrgUID);
        }
        public override async Task<List<ISelectionItem>> GetCustomerData(string UID)
        {
            return await GetCustomerDataFromAPIAsync(UID);
        }
        #endregion
        #region Api Calling Methods
        private async Task<List<ISelectionItem>> GetDistributorDataFromAPIAsync()
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetDistributorDropDown",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        private async Task<List<ISelectionItem>> GetSalesmanDataFromAPIAsync(string OrgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetEmpDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        private async Task<List<ISelectionItem>> GetRouteDataFromAPIAsync(string OrgUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetRouteDropDown?orgUID={OrgUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        private async Task<List<ISelectionItem>> GetCustomerDataFromAPIAsync(string RouteUID)
        {
            try

            {
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                    $"{_appConfigs.ApiBaseUrl}Dropdown/GetCustomersByRouteUIDDropDown?routeUID={RouteUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    ApiResponse<List<SelectionItem>> Response = JsonConvert.DeserializeObject<ApiResponse<List<SelectionItem>>>(apiResponse.Data);
                    return Response.Data.ToList<ISelectionItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales>> GetManageSalesOrdersDataFromAPIAsnyc(string selectedtab)
        {
            try
            {
                DateTime endDate = DateTime.Now.AddDays(4);
                DateTime startDate = DateTime.Now.AddDays(-120);
                PagingRequest pagingRequest = new PagingRequest();
                pagingRequest.PageSize = PageSize;
                pagingRequest.PageNumber = PageNumber;
                pagingRequest.FilterCriterias = salesOrderFilterCriterials;
                pagingRequest.SortCriterias = new List<SortCriteria>
                {
                    new SortCriteria("SalesOrderModifiedTime",SortDirection.Desc)
                };
                pagingRequest.SortCriterias = this.SortCriterias;
                pagingRequest.IsCountRequired = true;
                ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                   $"{_appConfigs.ApiBaseUrl}SalesOrder/SelectDeliveredPreSales?startdate={startDate.ToString("MM/dd/yyyy")}&enddate={endDate.ToString("MM/dd/yyyy")}&Status={selectedtab}",
                   HttpMethod.Post, pagingRequest);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                    PagedResponse<Winit.Modules.SalesOrder.Model.Classes.DeliveredPreSales> selectionORGs = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.SalesOrder.Model.Classes.DeliveredPreSales>>(data);
                    if (selectionORGs.PagedData != null)
                    {
                        TotalItemsCount = selectionORGs.TotalCount;
                        return selectionORGs.PagedData.OfType<IDeliveredPreSales>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
        //private async Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRoute>> GetRouteDropdownDataFromAPIAsync(string orgUID)
        //{
        //    try
        //    {
        //        PagingRequest pagingRequest = new PagingRequest();
        //        pagingRequest.PageNumber = 1;
        //        pagingRequest.PageSize = int.MaxValue;
        //        pagingRequest.FilterCriterias = new List<FilterCriteria>();
        //        pagingRequest.IsCountRequired = true;
        //        ApiResponse<PagedResponse<Winit.Modules.Route.Model.Classes.Route>> apiResponse =
        //             await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>(
        //             $"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={orgUID}",
        //             HttpMethod.Post, pagingRequest);
        //        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null
        //            && apiResponse.Data.PagedData.Any())
        //        {
        //            return apiResponse.Data.PagedData;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return null;
        //}
        private async Task<Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales> GetManageSalesOrdersDataforViewFromAPIAsnyc(string salesorderUID)
        {
            try
            {
                ApiResponse<Winit.Modules.SalesOrder.Model.Classes.ViewPreSalesDTO> apiResponse =
                    await _apiService.FetchDataAsync<Winit.Modules.SalesOrder.Model.Classes.ViewPreSalesDTO>(
                    $"{_appConfigs.ApiBaseUrl}SalesOrder/SelectDeliveredPreSalesBySalesOrderUID?SalesOrderUID={salesorderUID}",
                    HttpMethod.Post);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales viewPreSales = new ViewPreSales();
                    viewPreSales.CustomerNumber = apiResponse.Data.CustomerNumber;
                    viewPreSales.CustomerName = apiResponse.Data.CustomerName;
                    viewPreSales.PONumber = apiResponse.Data.PONumber;
                    viewPreSales.SalesOrderNumber = apiResponse.Data.SalesOrderNumber;
                    viewPreSales.DraftOrderNumber = apiResponse.Data.DraftOrderNumber;
                    viewPreSales.SalesREP = apiResponse.Data.SalesREP;
                    viewPreSales.OrderType = apiResponse.Data.OrderType;
                    viewPreSales.TotalSKUCount = apiResponse.Data.TotalSKUCount;
                    viewPreSales.OrderStatus = apiResponse.Data.OrderStatus;
                    viewPreSales.PaymentType = apiResponse.Data.PaymentType;
                    viewPreSales.RouteName = apiResponse.Data.RouteName;
                    viewPreSales.OrderDate = apiResponse.Data.OrderDate;
                    viewPreSales.ExpectedDeliveryDate = apiResponse.Data.ExpectedDeliveryDate;
                    viewPreSales.DeliveredDateTime = apiResponse.Data.DeliveredDateTime;
                    viewPreSales.QtyCount = apiResponse.Data.QtyCount;
                    viewPreSales.ReferenceNumber = apiResponse.Data.ReferenceNumber;
                    viewPreSales.ReferenceUID = apiResponse.Data.ReferenceUID;
                    viewPreSales.Notes = apiResponse.Data.Notes;
                    viewPreSales.TotalAmount = apiResponse.Data.TotalAmount;
                    viewPreSales.TotalDiscount = apiResponse.Data.TotalDiscount;
                    viewPreSales.TotalTax = apiResponse.Data.TotalTax;
                    viewPreSales.NetAmount = apiResponse.Data.NetAmount;
                    viewPreSales.sKUViewPreSalesList = apiResponse.Data.sKUViewPreSalesList.OfType<ISKUViewPreSales>().ToList();
                    return viewPreSales;
                }
            }
            catch (Exception)
            {
                // Handle exceptions
                // Handle exceptions
            }
            return null;
        }
        #endregion
    }
}
