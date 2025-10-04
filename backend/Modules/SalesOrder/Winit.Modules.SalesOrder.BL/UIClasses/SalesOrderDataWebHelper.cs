using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesOrderDataWebHelper : ISalesOrderDataHelper
{
    private IAppUser _appUser { get; }
    private IAppSetting _appSetting { get; }
    private IAppConfig _appConfigs { get; }
    private ApiService _apiService { get; }
    public SalesOrderDataWebHelper(
            IAppUser appUser,
            IAppSetting appSetting,
            Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService)
    {
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _apiService = apiService;
    }
    public async Task<List<Winit.Modules.Route.Model.Interfaces.IRoute>> GetAllRoutesAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<PagedResponse<Winit.Modules.Route.Model.Classes.Route>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>
            ($"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={_appUser?.SelectedJobPosition?.OrgUID}",
                HttpMethod.Post, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
                ? apiResponse.Data.PagedData.ToList<Winit.Modules.Route.Model.Interfaces.IRoute>()
                : [];
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<IStoreItemView>> GetStoreItemViewsDataFromAPIAsync(string routeUID)
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreItemView>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.StoreItemView>>($"{_appConfigs.ApiBaseUrl}Store/GetStoreByRouteUID?routeUID={routeUID}",
                HttpMethod.Get, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
            ? apiResponse.Data.ToList<IStoreItemView>()
                : [];
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<ISKUV1>> GetSKUs(PagingRequest pagingRequest)
    {
        try
        {

            ApiResponse<PagedResponse<ISKUV1>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<ISKUV1>>($"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetails",
                HttpMethod.Post, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData is not null
                ? apiResponse.Data.PagedData.ToList()
                : [];
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<ISKUMaster>> PopulateSKUMaster(List<string> orgUID)
    {
        try
        {
            PagingRequest pagingRequest = new()
            {
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            ApiResponse<PagedResponse<ISKUMaster>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISKUMaster>>(
                $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData is not null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    public async Task<List<ISKUMaster>> PopulateSKUMaster(SKUMasterRequest request)
    {
        try
        {
            PagingRequest pagingRequest = new()
            {
                PageSize = int.MaxValue,
                PageNumber = 1
            };
            ApiResponse<PagedResponse<ISKUMaster>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISKUMaster>>(
                $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",
                HttpMethod.Post, request);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData is not null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    public async Task<PagedResponse<ISKUPrice>> PopulatePriceMaster(List<SortCriteria>? sortCriterias = null,
        int? pageNumber = null, int? pageSize = null, List<FilterCriteria>? filterCriterias = null,
        bool? isCountRequired = null, string skuPriceList = null)
    {
        PagingRequest pagingRequest = new()
        {
            PageSize = pageSize ?? 0,
            PageNumber = pageNumber ?? 0,
            SortCriterias = sortCriterias,
            FilterCriterias = filterCriterias,
        };
        if (filterCriterias != null && !filterCriterias.Any(p => p.Name.Contains("SkuPriceListUid")))
        {
            pagingRequest.FilterCriterias.Add(new FilterCriteria("SkuPriceListUid", !string.IsNullOrEmpty(skuPriceList) ? skuPriceList : "DefaultPriceList", FilterType.Equal));
        }
        ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>>(
                $"{_appConfigs.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetailsV1",
                HttpMethod.Post, pagingRequest);

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.PagedData != null && apiResponse.Data.PagedData.Any()
            ? new PagedResponse<ISKUPrice> { PagedData = apiResponse.Data.PagedData, TotalCount = apiResponse.Data.TotalCount }
            : new PagedResponse<ISKUPrice>();
    }
    public async Task<bool> SaveSalesOrder_Order(SalesOrderViewModelDCO salesOrderViewModelDCO)
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<int> apiResponse = await _apiService.FetchDataAsync<int>(
                    $"{_appConfigs.ApiBaseUrl}SalesOrder/InsertorUpdate_SalesOrders",
                    HttpMethod.Post, salesOrderViewModelDCO);

            return apiResponse != null && apiResponse.IsSuccess;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured while Creating SalesOrder", ex.Message);
            return false;
        }
    }
    public async Task<ISalesOrder?> GetSalesOrderByUID(string SalesOrderUID)
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<Winit.Modules.SalesOrder.Model.Classes.SalesOrder> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.SalesOrder.Model.Classes.SalesOrder>
                ($"{_appConfigs.ApiBaseUrl}SalesOrder/SelectSalesOrderByUID?SalesOrderUID={SalesOrderUID}",
                HttpMethod.Get, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data : (ISalesOrder?)default;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error Occured While executing Get Sales Order By Sales Order UID From API Async Sales Order UID {SalesOrderUID} exection is {ex.Message}");
        }
    }
    public async Task<SalesOrderViewModelDCO?> GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID)
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<SalesOrderViewModelDCO> apiResponse =
                await _apiService.FetchDataAsync<SalesOrderViewModelDCO>
                ($"{_appConfigs.ApiBaseUrl}SalesOrder/GetSalesOrderMasterDataBySalesOrderUID?salesOrderUID={SalesOrderUID}",
                HttpMethod.Get, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data : default;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error Occured While executing Get Sales Order By Sales Order UID From API Async Sales Order UID  {SalesOrderUID}  exection is {ex.Message}");
        }
    }
    public async Task<List<StoreMasterDTO>> GetStoreMastersbyStoreUIDs(List<string> storeUIDs)
    {
        try
        {
            ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreMasterDTO>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.StoreMasterDTO>>
                ($"{_appConfigs.ApiBaseUrl}Store/GetStoreMastersByStoreUIDs",
                HttpMethod.Post, storeUIDs);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
                ? apiResponse.Data.ToList<StoreMasterDTO>()
                : [];
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IStoreHistory> GetStoreHistoryAPI(string routeUID, string visitDate, string storeUID)
    {
        try
        {
            ApiResponse<StoreHistory> apiResponse =
            await _apiService.FetchDataAsync<StoreHistory>
            ($"{_appConfigs.ApiBaseUrl}StoreHistory/GetStoreHistoryByRouteUIDVisitDateAndStoreUID?" +
            $"routeUID={routeUID}&visitDate={visitDate}&storeUID={storeUID}",
                HttpMethod.Get);
            return apiResponse.Data;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> UpdateSalesOrderStatusApiAsync(Winit.Modules.SalesOrder.Model.Classes.SalesOrderStatusModel salesOrderStatusModel)
    {
        try
        {
            ApiResponse<int> apiResponse =
                await _apiService.FetchDataAsync<int>
                ($"{_appConfigs.ApiBaseUrl}SalesOrder/UpdateSalesOrderStatus",
                HttpMethod.Put, salesOrderStatusModel);
            return apiResponse != null && apiResponse.IsSuccess;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error Occured While executing UpdateSalesOrderStatusApiAsync SalesOrderUID{salesOrderStatusModel.UID} exection is {ex.Message}");
        }
    }

    public async Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        try
        {
            ApiResponse<List<ISalesOrderLine>> apiResponse =
            await _apiService.FetchDataAsync<List<ISalesOrderLine>>
            ($"{_appConfigs.ApiBaseUrl}SalesOrder/GetSalesOrderLinesBySalesOrderUID?SalesOrderUID={salesOrderUID}",
                HttpMethod.Get);
            return apiResponse.Data;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<IEnumerable<IWarehouseStockItemView>> GetVanStockItems(string vanUID, string orgUID, StockType stockType)
    {
        throw new NotImplementedException();
    }

    public async Task<List<IFileSys>?> GetFileSys(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs = null)
    {
        return await Task.FromResult(new List<IFileSys>());
    }

    public Task<ISalesOrderPrintView> GetSalesOrderHeaderDetails(string salesOrderUID)
    {
        throw new NotImplementedException();
    }

    public Task<List<ISalesOrderLinePrintView>> GetSalesOrderPrintViewslines(string salesOrderUID)
    {
        throw new NotImplementedException();
    }

    public List<AppliedPromotionView> ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView,
        Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateFileSysForBulk(List<IFileSys> fileSysList)
    {
        return default;
    }
    public async Task<List<string>?> GetOrgHierarchyParentUIDsByOrgUID(string orgUID)
    {
        ApiResponse<List<string>> apiResponse = await _apiService.FetchDataAsync<List<string>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetOrgHierarchyParentUIDsByOrgUID",
            HttpMethod.Post, new List<string>
            {
                orgUID
            });
        return apiResponse.Data;
    }
    
    public async Task<List<IOrg>?> GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
    {
        ApiResponse<List<IOrg>> apiResponse = await _apiService.FetchDataAsync<List<IOrg>>(
            $"{_appConfigs.ApiBaseUrl}Org/GetDeliveryDistributorsByOrgUID?storeUID={storeUID}&orgUID={orgUID}",
            HttpMethod.Get);
        return apiResponse.Data;
    }

    public Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData()
    {
        throw new NotImplementedException();
    }

    public Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
    {
        throw new NotImplementedException();
    }

    public Task<List<IOrg>> GetWareHouseData(string orgTypeUID)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IOrg>> GetDistributorslistByType(string type)
    {
        throw new NotImplementedException();
    }

    //Task<ISalesOrder?> ISalesOrderDataHelper.GetSalesOrderByUID(string salesOrderUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<ISalesOrderLine>?> ISalesOrderDataHelper.GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<ISKUMaster>> ISalesOrderDataHelper.PopulateSKUMaster(List<string> orgUIDs)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<ISKUV1>> ISalesOrderDataHelper.GetSKUs(PagingRequest pagingRequest)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<ISKUMaster>> ISalesOrderDataHelper.PopulateSKUMaster(SKUMasterRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<PagedResponse<ISKUPrice>> ISalesOrderDataHelper.PopulatePriceMaster(List<SortCriteria>? sortCriterias, int? pageNumber, int? pageSize, List<FilterCriteria>? filterCriterias, bool? isCountRequired, string skuPriceList)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<bool> ISalesOrderDataHelper.SaveSalesOrder_Order(SalesOrderViewModelDCO salesOrderViewModelDCO)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<IEnumerable<IWarehouseStockItemView>> ISalesOrderDataHelper.GetVanStockItems(string vanUID, string orgUID, StockType stockType)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<IFileSys>?> ISalesOrderDataHelper.GetFileSys(string linkedItemType, string fileSysType, List<string>? linkedItemUIDs)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<ISalesOrderPrintView> ISalesOrderDataHelper.GetSalesOrderHeaderDetails(string salesOrderUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<ISalesOrderLinePrintView>> ISalesOrderDataHelper.GetSalesOrderPrintViewslines(string salesOrderUID)
    //{
    //    throw new NotImplementedException();
    //}

    //List<AppliedPromotionView> ISalesOrderDataHelper.ApplyPromotion(string applicablePromotionUIDs, PromotionHeaderView promoHeaderView, Dictionary<string, DmsPromotion> promoDictionary, PromotionPriority promotionPriority)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<int> ISalesOrderDataHelper.CreateFileSysForBulk(List<IFileSys> fileSysList)
    //{
    //    throw new NotImplementedException();
    //}

    ////Task<List<IRoute>> ISalesOrderDataHelper.GetAllRoutesAPIAsync()
    ////{
    ////    throw new NotImplementedException();
    ////}

    //Task<SalesOrderViewModelDCO?> ISalesOrderDataHelper.GetSalesOrderMasterDataBySalesOrderUID(string SalesOrderUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<IStoreItemView>> ISalesOrderDataHelper.GetStoreItemViewsDataFromAPIAsync(string routeUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<StoreMasterDTO>> ISalesOrderDataHelper.GetStoreMastersbyStoreUIDs(List<string> storeUIDs)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<IStoreHistory> ISalesOrderDataHelper.GetStoreHistoryAPI(string routeUID, string visitDate, string storeUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<bool> ISalesOrderDataHelper.UpdateSalesOrderStatusApiAsync(SalesOrderStatusModel salesOrderStatusModel)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<string>?> ISalesOrderDataHelper.GetOrgHierarchyParentUIDsByOrgUID(string orgUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<IOrg>?> ISalesOrderDataHelper.GetDeliveryDistributorsByOrgUID(string orgUID, string storeUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<SKUAttributeDropdownModel>> ISalesOrderDataHelper.GetSKUAttributeDropDownData()
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<SKUGroupSelectionItem>> ISalesOrderDataHelper.GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<List<IOrg>> ISalesOrderDataHelper.GetWareHouseData(string orgTypeUID)
    //{
    //    throw new NotImplementedException();
    //}

    //Task<IEnumerable<IOrg>> ISalesOrderDataHelper.GetDistributorslistByType(string type)
    //{
    //    throw new NotImplementedException();
    //}
}
