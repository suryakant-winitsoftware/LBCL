using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Role.Model.Classes;
using Winit.Modules.Role.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;


namespace Winit.Modules.Common.BL.Classes;

public class CommonMasterData : ICommonMasterData
{
    public CommonMasterData(IAppUser appUser, ILocalStorageService localStorageService, ApiService apiService, IAppConfig appConfigs, CommonFunctions commonFunctions,
        NavigationManager navigationManager, Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IMenuMasterHierarchyView menuMasterHierarchyView)
    {
        _apiService = apiService;
        _appConfigs = appConfigs;
        _commonFunctions = commonFunctions;
        _navigationManager = navigationManager;
        _dataManager = dataManager;
        _localStorageService = localStorageService;
        _appUser = appUser;
        _MenuMasterHierarchyView = menuMasterHierarchyView;
    }
    public IMenuMasterHierarchyView _MenuMasterHierarchyView { get; set; }
    protected IAppUser _appUser;
    protected ILocalStorageService _localStorageService { get; private set; }
    protected ApiService _apiService { get; set; }
    protected IAppConfig _appConfigs { get; set; }
    protected CommonFunctions _commonFunctions { get; set; }
    protected NavigationManager _navigationManager { get; set; }
    protected Winit.Modules.Common.Model.Interfaces.IDataManager _dataManager { get; set; }

    PagingRequest pagingRequest;

    public async Task PageLoadevents()
    {

        Task.Run(() => SyncSKUGroupType());
        Task.Run(() => SyncSKUGroup());
        Task.Run(() => GetLocationMaster());
        Task.Run(() => GetChannelMasterData());

        //await GetLocationMaster();
        //await GetChannelMasterData();
    }

    public async Task SyncSKUGroupType()
    {
        pagingRequest = new PagingRequest();
        //pagingRequest.PageNumber = 1;
        //pagingRequest.PageSize = Int32.MaxValue;
        pagingRequest.IsCountRequired = true;

        Shared.Models.Common.ApiResponse<PagedResponse<ISKUGroupType>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISKUGroupType>>($"{_appConfigs.ApiBaseUrl}SKUGroupType/SelectAllSKUGroupTypeDetails", HttpMethod.Post, pagingRequest);
        if (apiResponse != null)
        {
            if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
            {
                DatamanagerGeneric<List<ISKUGroupType>>.Remove(Shared.Models.Constants.CommonMasterDataConstants.SKUGroupType);
                DatamanagerGeneric<List<ISKUGroupType>>.Set(Shared.Models.Constants.CommonMasterDataConstants.SKUGroupType, apiResponse?.Data?.PagedData?.ToList());

            }
        }

    }
    public async Task SyncSKUGroup()
    {
        ApiResponse<PagedResponse<ISKUGroup>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<ISKUGroup>>($"{_appConfigs.ApiBaseUrl}SKUGroup/SelectAllSKUGroupDetails", HttpMethod.Post, pagingRequest);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.StatusCode == 200)
        {
            DatamanagerGeneric<List<ISKUGroup>>.Remove(Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
            DatamanagerGeneric<List<ISKUGroup>>.Set(Shared.Models.Constants.CommonMasterDataConstants.SKUGroup,
                apiResponse?.Data?.PagedData?.ToList());
        }
    }
    private async Task GetLocationMaster()
    {
        var LocationMasterForUIs = new List<Winit.Modules.Location.Model.Classes.LocationData>();
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
         $"{_appConfigs.ApiBaseUrl}LocationMapping/GetLocationMasterByLocationName", HttpMethod.Get);
        if (apiResponse.Data != null)
        {
            // string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
            ApiResponse<List<Location.Model.Classes.LocationData>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Location.Model.Classes.LocationData>>>(apiResponse.Data);
            if (apiResponse1 != null && apiResponse1.Data != null)
            {
                LocationMasterForUIs = apiResponse1.Data;
                DatamanagerGeneric<List<Winit.Modules.Location.Model.Classes.LocationData>>.Set(Model.Constants.CommonConstants.LocationMasterData, LocationMasterForUIs);
                //_dataManager.DeleteData(Winit.Modules.Common.Model.Constants.CommonConstants.LocationMasterData);
                //_dataManager.SetData(Model.Constants.CommonConstants.LocationMasterData, LocationMasterForUIs);
            }
        }

    }
    private async Task GetChannelMasterData()
    {
        var ChannelMasterData = new List<Winit.Modules.Store.Model.Classes.StoreGroupData>();
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
         $"{_appConfigs.ApiBaseUrl}StoreToGroupMapping/SelectAllChannelMasterData", HttpMethod.Get);
        if (apiResponse.Data != null)
        {
            // string data = _commonFunctions.GetDataFromResponse(apiResponse.Data);
            ApiResponse<List<Winit.Modules.Store.Model.Classes.StoreGroupData>>? apiResponse1 = JsonConvert.DeserializeObject<ApiResponse<List<Store.Model.Classes.StoreGroupData>>>(apiResponse.Data);
            if (apiResponse1 != null && apiResponse1.Data != null)
            {
                ChannelMasterData = apiResponse1.Data;
                DatamanagerGeneric<List<Store.Model.Classes.StoreGroupData>>.
                    Set(Model.Constants.CommonConstants.LocationMasterData, ChannelMasterData);

                //_dataManager.DeleteData(Winit.Modules.Common.Model.Constants.CommonConstants.ChannelMasterData);
                //_dataManager.SetData(Model.Constants.CommonConstants.ChannelMasterData, ChannelMasterData);
            }
        }

    }
    public async Task GetAppUserData()
    {

    }
}
