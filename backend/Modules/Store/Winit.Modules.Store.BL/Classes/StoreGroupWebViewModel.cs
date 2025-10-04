using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.BL.Classes;

public class StoreGroupWebViewModel : StoreGroupBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public StoreGroupWebViewModel(
                IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
                Winit.Modules.Base.BL.ApiService apiService,
                IAppUser appUser) : base(serviceProvider, filter, sorter, listHelper, appUser)
    {

        _appConfigs = appConfigs;
        _apiService = apiService;
        StoreGroupItemViews = new List<IStoreGroupItemView>();
    }

    #region Concrete Methods
    protected override async Task<List<IStoreGroupType>> GetStoreGroupTypes_Data()
    {
        return await GetStoreGroupTypesFromAPI();
    }
    protected override async Task<bool> DeleteStoreGroup_Data(string storeGroupUID)
    {
        return await DeleteStoreGroupAPI(storeGroupUID);
    }
    protected override async Task<List<IStoreGroup>> GetStoreGroup_Data(string parentUID, int Level)
    {
        return await GetStoreGroupFromAPI(parentUID, Level);
    }
    protected override async Task<bool> UpdateStoreGroup_Data(IStoreGroup storeGroup)
    {
        return await UpdateStoreGroupAPI(storeGroup);
    }
    protected override async Task<bool> CreateStoreGroup_Data(IStoreGroup storeGroup)
    {
        return await CreateStoreGroupAPI(storeGroup);
    }
    protected override async Task CreateStoreGroupHierarchyApiCall(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
         await CallCreateStoreGroupHierarchyAPI(storeGroupItemView);
    }
    #endregion

    #region API Calling Methods
    public async Task<List<IStoreGroup>> GetStoreGroupFromAPI(string? ParentUID, int Level)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            if (ParentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Is));
            if (ParentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Equal));
            if (Level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ItemLevel", "" + Level, Shared.Models.Enums.FilterType.Equal));
            if (FilterCriterias != null && FilterCriterias.Any()) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroup>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroup>>(
                    $"{_appConfigs.ApiBaseUrl}StoreGroup/SelectAllStoreGroup",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.OfType<IStoreGroup>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new List<IStoreGroup>();
    }

    private async Task<bool> DeleteStoreGroupAPI(string StoreGroupUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroup/DeleteStoreGroup?UID={StoreGroupUID}",
                HttpMethod.Delete);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return false;
    }

    public async Task<bool> CreateStoreGroupAPI(IStoreGroup storeGroup)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroup/CreateStoreGroup",
                HttpMethod.Post, storeGroup);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return false;
    }

    public async Task<bool> UpdateStoreGroupAPI(IStoreGroup storeGroup)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(storeGroup);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroup/UpdateStoreGroup",
                HttpMethod.Put, storeGroup);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return false;
    }

    public async Task<List<IStoreGroupType>> GetStoreGroupTypesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                 $"{_appConfigs.ApiBaseUrl}StoreGroupType/SelectAllStoreGroupType",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroupType> skuGroups = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroupType>>(data);
                if (skuGroups.PagedData != null)
                {
                    return skuGroups.PagedData.OfType<IStoreGroupType>().ToList();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    public async Task CallCreateStoreGroupHierarchyAPI(Winit.Modules.Store.Model.Interfaces.IStoreGroupItemView storeGroupItemView)
    {
        try
        {
            string url = $"{_appConfigs.ApiBaseUrl}StoreGroup/InsertStoreGroupHierarchy?type={storeGroupItemView.StoreGroupTypeName}&uid={storeGroupItemView.UID}";
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(url, HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                //return true;
            }
        }
        catch (Exception)
        {
            throw;
        }
        //return false;
    }
    #endregion
}
