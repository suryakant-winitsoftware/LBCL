using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.Location.BL.Classes;

public class LocationWebViewModel : LocationBaseViewModel
{
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public LocationWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser) : base(serviceProvider, filter, sorter, listHelper,appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }

    #region Concrete Methods
    protected override async Task<List<ILocationType>> GetLocationTypes_Data()
    {
        return await GetLocationTypesFromAPI();
    }
    protected override async Task<List<ILocation>> GetLocations_Data(string? ParentUID, int level)
    {
        return await GetLocationFromAPI(ParentUID, level);
    }
    protected override async Task<bool> UpdateLocation_Data(ILocation location)
    {
        return await UpdateLocationAPI(location);
    }
    protected override async Task<bool> CreateLocation_Data(ILocation location)
    {
        return await CreateLocationAPI(location);
    }
    protected override async Task CreateLocationHierarchyApiCall(ILocationItemView location)
    {
        await CallCreateLocationHierarchyApi(location);
    }
    protected override async Task<bool> DeleteLocation_Data(string locationUID)
    {
        return await DeleteLocationAPI(locationUID);
    }
    #endregion

    #region Api Calling Methods
    public async Task<List<ILocation>> GetLocationFromAPI(string? ParentUID, int Level)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            if (ParentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Is));
            if (ParentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Equal));
            if (Level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ItemLevel", "" + Level, Shared.Models.Enums.FilterType.Equal));
            if (FilterCriterias != null && FilterCriterias.Any() && string.IsNullOrEmpty(ParentUID)) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            ApiResponse<PagedResponse<Winit.Modules.Location.Model.Classes.Location>> apiResponse = await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Location.Model.Classes.Location>>(
                $"{_appConfigs.ApiBaseUrl}Location/SelectAllLocationDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.OfType<ILocation>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<bool> DeleteLocationAPI(string LocationUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Location/DeleteLocationDetails?UID={LocationUID}",
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
    public async Task<bool> CreateLocationAPI(ILocation location)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Location/CreateLocation",
                HttpMethod.Post, location);
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

    public async Task CallCreateLocationHierarchyApi(ILocationItemView location)
    {
        try
        {
            string url = $"{_appConfigs.ApiBaseUrl}LocationMapping/InsertLocationHierarchy?type={location.LocationTypeName}&uid={location.UID}";
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
    public async Task<bool> UpdateLocationAPI(ILocation location)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(location);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Location/UpdateLocationDetails",
                HttpMethod.Put, location);
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
    public async Task<List<ILocationType>> GetLocationTypesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            ApiResponse<PagedResponse<Winit.Modules.Location.Model.Classes.LocationType>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Location.Model.Classes.LocationType>>(
                 $"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.OfType<ILocationType>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    #endregion
}

