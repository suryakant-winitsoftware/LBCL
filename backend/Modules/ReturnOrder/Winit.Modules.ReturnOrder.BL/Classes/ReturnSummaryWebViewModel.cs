using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Classes;
public class ReturnSummaryWebViewModel : ReturnSummaryBaseViewModel, IReturnSummaryWebViewModel
{
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
    public string? RouteUID { get; set; }
    public ReturnSummaryWebViewModel
        (Winit.Shared.Models.Common.IAppConfig appConfig,
        Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser) : base(appUser)
    {
        _appConfigs = appConfig;
        _apiService = apiService;
        RouteSelectionItems = new List<ISelectionItem>();
        StoreSelectionItems = new List<ISelectionItem>();
        FilterCriterias = new List<FilterCriteria>();
    }
    public override async Task PopulateViewModel()
    {
        await PopulateReturnSummaryItemViewsDictionary();
        OnTabSelected(TabList?.First());
        RouteSelectionItems.Clear();
        RouteSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IRoute>
            (await GetAllRoutesAPIAsync(), new List<string> { "UID", "Code", "Name" }));
    }
    public async Task OnRouteSelect(string routeUID)
    {
        RouteUID = routeUID;
        StoreSelectionItems.Clear();
        StoreSelectionItems.AddRange(Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems
            <IStoreItemView>(await GetStoreItemViewsDataFromAPIAsync(routeUID), new List<string> { "UID", "Code", "Name" }));
    }
    public override async Task ApplyFilter()
    {
        await PopulateReturnSummaryItemViewsDictionary();
        SelectedTab!.IsSelected = false;
        OnTabSelected(SelectedTab);
    }
    #region Concrete Methods
    protected override async Task<bool> UpdateReturnOrderStatus_Data(List<string> returnOrderUIDs, string status)
    {
        return await UpdateReturnOrderStatus(returnOrderUIDs, status);
    }
    protected override async Task<List<Model.Interfaces.IReturnSummaryItemView>> GetReturnOrderSummaryItemViews_Data
        (DateTime startDate, DateTime endDate, string? storeUID = null)
    {
        return await GetReturnSummaryItemViewDataFromAPIAsync(startDate, endDate, storeUID);
    }
    #endregion
    #region API Calling Methods
    private async Task<bool> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string status)
    {
        try
        {
            ReturnOrderUpdateRequest returnOrderUpdateRequest = new ReturnOrderUpdateRequest
            {
                ReturnOrderUIDs = returnOrderUIDs,
                Status = status,
            };
            ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}ReturnOrder/UpdateReturnOrderStatus", HttpMethod.Put, returnOrderUpdateRequest);
            if (apiResponse != null)
            {
                return apiResponse.IsSuccess;
            }
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<Model.Interfaces.IReturnSummaryItemView>> GetReturnSummaryItemViewDataFromAPIAsync
        (DateTime startDate, DateTime endDate, string? storeUID = null)
    {
        try
        {
            ReturnSummaryItemApiRequest returnSummaryItemApiRequest = new ReturnSummaryItemApiRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                StoreUID = storeUID
            };
            if (FilterCriterias.Any()) returnSummaryItemApiRequest.FilterCriterias = FilterCriterias;
            ApiResponse<List<Model.Classes.ReturnSummaryItemView>> apiResponse =
                await _apiService.FetchDataAsync<List<Model.Classes.ReturnSummaryItemView>>(
                @$"{_appConfigs.ApiBaseUrl}ReturnOrder/GetReturnSummaryItemView",
                HttpMethod.Post, returnSummaryItemApiRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.OfType<Model.Interfaces.IReturnSummaryItemView>().ToList();
            }
            return new();
        }
        catch (Exception)
        {
            throw;
        }
    }
    protected async Task<List<Winit.Modules.Route.Model.Interfaces.IRoute>> GetAllRoutesAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            ApiResponse<PagedResponse<Winit.Modules.Route.Model.Classes.Route>> apiResponse =
            await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Route.Model.Classes.Route>>
            ($"{_appConfigs.ApiBaseUrl}Route/SelectAllRouteDetails?OrgUID={_appUser?.SelectedJobPosition?.OrgUID ?? "FR001"}",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.PagedData.ToList<Winit.Modules.Route.Model.Interfaces.IRoute>();
            }
            return new List<Route.Model.Interfaces.IRoute>();
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<List<IStoreItemView>> GetStoreItemViewsDataFromAPIAsync(string routeUID)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreItemView>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Store.Model.Classes.StoreItemView>>($"{_appConfigs.ApiBaseUrl}Store/GetStoreByRouteUID?routeUID={routeUID}",
                HttpMethod.Get, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.ToList<IStoreItemView>();
            }
            return new List<IStoreItemView>();
        }
        catch (Exception)
        {
            throw;
        }
    }
    #endregion
}

