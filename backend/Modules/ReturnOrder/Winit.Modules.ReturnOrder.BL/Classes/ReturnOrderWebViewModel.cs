using Microsoft.IdentityModel.Tokens;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Classes;

public class ReturnOrderWebViewModel : ReturnOrderBaseViewModel, IReturnOrderWebViewModel
{
    protected Winit.Modules.Base.BL.ApiService _apiService;
    public List<IStoreCredit> StoreCredits { get; set; }
    public List<ISelectionItem> StoreCreditsSelectionItems { get; set; }
    public ReturnOrderWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           Interfaces.IReturnOrderAmountCalculator amountCalculator,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService,
           IAppUser appUser) : base(serviceProvider,
           filter, sorter, amountCalculator, listHelper, appConfigs, apiService, appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
        StoreCredits = [];
        StoreCreditsSelectionItems = [];
    }

    public override async Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "")
    {
        IsNewOrder = isNewOrder;
        ReturnOrderUID = returnOrderUID;
        Source = source;
        Status = Shared.Models.Constants.SalesOrderStatus.DRAFT;
        RouteSelectionItems.Clear();
        RouteSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IRoute>
            (await GetAllRoutesAPIAsync(), ["UID", "Code", "Name"]));
        await PopulateReasons();
        InitializeOptionalDependencies();
        await SetSystemSettingValues();
        await PopulateSKUMaster();
        await PopulatePriceMaster();
        await ApplyFilter([], FilterMode.And);
        List<SortCriteria> sortCriteriaList = [new SortCriteria("SKUName", SortDirection.Asc)];
        await ApplySort(sortCriteriaList);
        if (!returnOrderUID.IsNullOrEmpty())
        {
            await PopulateReturnOrder();
        }
        else
        {
            ReturnOrderUID = Guid.NewGuid().ToString();
        }
        IsTaxable = true;
    }
    protected override async Task PopulateReturnOrder()
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(ReturnOrderUID))
        {
            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = await GetReturnOrder_Data();
            if (returnOrderMaster != null)
            {
                ConvertReturnOrderToReturnOrderViewModel(returnOrderMaster.ReturnOrder);
                await OnRouteSelect(RouteUID);
                RouteSelectionItems.FirstOrDefault(e => e.UID == RouteUID)!.IsSelected = true;
                await OnStoreItemViewSelected(returnOrderMaster.ReturnOrder.StoreUID);
                StoreSelectionItems.FirstOrDefault(e => e.UID == StoreUID)!.IsSelected = true;
                foreach (IReturnOrderLine returnOrderLine in returnOrderMaster.ReturnOrderLineList)
                {
                    OverideReturnOrderItemView(returnOrderLine);
                }
            }
        }
    }

    protected override async Task<List<IListItem>> Reasons_Data(string reason)
    {
        return await GetReasonDataFromAPIAsync(reason);
    }
    public override async Task OnRouteSelect(string routeUID)
    {
        RouteUID = routeUID;
        StoreItemViews.Clear();
        StoreItemViews.AddRange(await GetStoreItemViewsDataFromAPIAsync(routeUID));
        StoreSelectionItems.Clear();
        StoreSelectionItems.AddRange(Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems
            <IStoreItemView>(StoreItemViews, ["UID", "Code", "Name"]));
    }
    public override async Task OnStoreItemViewSelected(string storeItemViewUID)
    {
        IStoreItemView? storeItemView = StoreItemViews.Find(e => e.UID == storeItemViewUID);
        if (storeItemView != null)
        {
            SelectedStoreViewModel = storeItemView;
            await PopulateStoreData(SelectedStoreViewModel);
            ReturnOrderItemViewsRawdata.ForEach(e => e.IsTaxable = SelectedStoreViewModel.IsTaxApplicable);
        }
    }
    public void OnStoreDistributionChannelSelect(string distributionChannelUID)
    {
        SelectedStoreViewModel.SelectedDistributionChannelUID = distributionChannelUID;
        DistributionChannelOrgUID = distributionChannelUID;
    }
    protected override async Task<List<ISKUMaster>> SKUMasters_Data(List<string> orgs, List<string>? skuUIDs = null)
    {
        return await GetSKUDataFromAPIAsync(orgs, skuUIDs);
    }
    protected override async Task<List<ISKUPrice>> GetSKuPrices_Data()
    {
        return await GetSKuPricesFromAPI();
    }
    protected override async Task<IReturnOrderMaster> GetReturnOrder_Data()
    {
        return await GetReturnOrderDataFromAPIAsync();
    }
    protected override async Task<bool> PostData_ReturnOrder(ReturnOrderMasterDTO returnOrderViewModel)
    {
        return await PostDataToReturnOrderAPIAsync(returnOrderViewModel) > 0;
    }
    protected override async Task PopulateStoreData(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel)
    {
        StoreCredits.Clear();
        StoreCreditsSelectionItems.Clear();
        List<StoreMasterDTO> storeMaster = await GetStoreMastersbyStoreUIDs([storeViewModel.UID]);
        if (storeMaster != null && storeMaster.Any() && storeMaster.First().storeCredits != null &&
            storeMaster.First().storeCredits.Any())
        {
            if (storeMaster.First().storeCredits.Count == 1)
            {
                SelectedStoreViewModel.SelectedDistributionChannelUID = storeMaster.First().storeCredits.First().DistributionChannelUID;
            }
            else
            {
                StoreCredits.AddRange(storeMaster.First().storeCredits);
                StoreCreditsSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IStoreCredit>
                    (StoreCredits, ["UID", "DistributionChannelUID", "CreditType"]));
            }
        }
        OrgUID = _appUser.SelectedJobPosition.OrgUID;
        DistributionChannelOrgUID = SelectedStoreViewModel.SelectedDistributionChannelUID;
        StoreUID = storeViewModel.UID;
    }
    #region api calling Methods
    protected async Task<List<Winit.Modules.Route.Model.Interfaces.IRoute>> GetAllRoutesAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new();
            //pagingRequest.PageSize = int.MaxValue;
            //pagingRequest.PageNumber = 1;
            ApiResponse<PagedResponse<Winit.Modules.Route.Model.Classes.Route>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>
            ($"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={_appUser?.SelectedJobPosition?.OrgUID ?? "FR001"}",
                HttpMethod.Post, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null
                ? apiResponse.Data.PagedData.ToList<Winit.Modules.Route.Model.Interfaces.IRoute>()
                : ([]);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<IListItem>> GetReasonDataFromAPIAsync(string reasonType)
    {
        try
        {
            Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new()
            {
                Codes = [reasonType]
            };
            ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> apiResponse = await _apiService.
                FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>(
                $"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes",
                HttpMethod.Post, listItemRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null && apiResponse.Data.PagedData.Any()
                ? apiResponse.Data.PagedData.ToList<IListItem>()
                : ([]);
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while retirving the Reasons");
        }
    }
    private async Task<List<ISKUMaster>> GetSKUDataFromAPIAsync(List<string> orgs, List<string>? skuUIDs = null)
    {
        try
        {
            SKUMasterRequest sKUMasterRequest = new()
            {
                SKUUIDs = skuUIDs,
                OrgUIDs = orgs
            };
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>> apiResponse = await _apiService.FetchDataAsync
                <PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>($"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData", HttpMethod.Post,
                sKUMasterRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                List<ISKUMaster> sKUMastersFromApi = [];
                foreach (SKUMasterData? skumaster in apiResponse.Data.PagedData.ToList())
                {
                    if (skumaster != null)
                    {
                        sKUMastersFromApi.Add(new SKUMaster()
                        {
                            SKU = skumaster.SKU,
                            SKUAttributes = (skumaster.SKUAttributes != null) ? skumaster.SKUAttributes.Cast<ISKUAttributes>().ToList() : [],
                            SKUUOMs = skumaster.SKUUOMs != null ? skumaster.SKUUOMs.Cast<ISKUUOM>().ToList() : [],
                            ApplicableTaxUIDs = skumaster.ApplicableTaxUIDs,
                            SKUConfigs = skumaster.SKUConfigs != null ? skumaster.SKUConfigs.OfType<ISKUConfig>().ToList() : [],
                        }); ;
                    }
                }
                return sKUMastersFromApi;
            }
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while retriving the skumaster data from api");
        }
        return [];
    }
    private async Task<List<ISKUPrice>> GetSKuPricesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>> apiResponse = await _apiService.FetchDataAsync
                <PagedResponse<Winit.Modules.SKU.Model.Classes.SKUPrice>>(
                    $"{_appConfigs.ApiBaseUrl}SKUPrice/SelectAllSKUPriceDetails",
                    HttpMethod.Post, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data.PagedData.ToList<ISKUPrice>() : ([]);
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while Retriving the skuprices data from api");
        }
    }
    private async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderDataFromAPIAsync()
    {
        try
        {
            ApiResponse<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO> apiResponse = await _apiService.FetchDataAsync
                <Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>($"{_appConfigs.ApiBaseUrl}" +
                $"ReturnOrder/SelectReturnOrderMasterByUID?UID={ReturnOrderUID}",
                HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = new ReturnOrderMaster
                {
                    ReturnOrder = apiResponse.Data.ReturnOrder,
                    ReturnOrderLineList = (apiResponse.Data.ReturnOrderLineList != null) ?
                    apiResponse.Data.ReturnOrderLineList.Cast<IReturnOrderLine>().ToList() : [],
                    ReturnOrderTaxList = (apiResponse.Data.ReturnOrderTaxList != null) ?
                    apiResponse.Data.ReturnOrderTaxList.Cast<IReturnOrderTax>().ToList() : []
                };
                return returnOrderMaster;
            }
            return new ReturnOrderMaster();
        }
        catch (Exception)
        {
            throw new Exception("Exception occured while retiving the exising returnorde from api");
        }
    }
    private async Task<int> PostDataToReturnOrderAPIAsync(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO
        returnOrderMasterDTO)
    {
        try
        {
            ApiResponse<string> apiResponse = IsNewOrder
                ? await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}ReturnOrder/CreateReturnOrderMaster", HttpMethod.Post, returnOrderMasterDTO)
                : await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}ReturnOrder/UpdateReturnOrderMaster", HttpMethod.Post, returnOrderMasterDTO);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? 1 : 0;
        }
        catch (Exception)
        {
            throw new Exception("Exception Occured while saving the return order api");
        }
    }
    private async Task<List<IStoreItemView>> GetStoreItemViewsDataFromAPIAsync(string routeUID)
    {
        try
        {
            PagingRequest pagingRequest = new();
            ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreItemView>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.StoreItemView>>($"{_appConfigs.ApiBaseUrl}Store/GetStoreByRouteUID?routeUID={routeUID}",
                HttpMethod.Get, pagingRequest);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data.ToList<IStoreItemView>() : ([]);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<StoreMasterDTO>> GetStoreMastersbyStoreUIDs(List<string> storeUIDs)
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


    #endregion
}

