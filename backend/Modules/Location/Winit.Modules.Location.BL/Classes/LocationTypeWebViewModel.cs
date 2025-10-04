using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Location.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Location.BL.Classes;

public class LocationTypeWebViewModel : LocationTypeBaseViewModel
{
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public LocationTypeWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService,
            Winit.Modules.Common.BL.Interfaces.IAppUser appUser
       ) : base(serviceProvider, filter, sorter, listHelper,appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }

    #region Concrete implemeted Methods
    protected override async Task<List<ILocationType>> GetLocationType_Data(string? ParentUID, int Level)
    {
        return await GetLocationTypeFromAPI(ParentUID, Level);
    }
    protected override async Task<bool> UpdateLocationType_Data(ILocationType locationType)
    {
        return await UpdateLocationTypeAPI(locationType);
    }
    protected override async Task<bool> DeleteLocationType_Data(string locationTypeUID)
    {
        return await DeleteLocationTypeAPI(locationTypeUID);
    }
    protected override async Task<bool> CreateLocationType_Data(ILocationType locationType)
    {
        return await CreateLocationTypeAPI(locationType);
    }
    #endregion

    #region Api Calling Methods
    public async Task<List<ILocationType>> GetLocationTypeFromAPI(string? ParentUID, int Level)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();

            if (ParentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Is));
            if (ParentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Equal));
            if (Level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("LevelNo", "" + Level, Shared.Models.Enums.FilterType.Equal));

            if (FilterCriterias != null && FilterCriterias.Any() && string.IsNullOrEmpty(ParentUID)) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}LocationType/SelectAllLocationTypeDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                string data = new Winit.Shared.CommonUtilities.Common.CommonFunctions().GetDataFromResponse(apiResponse.Data);
                PagedResponse<Winit.Modules.Location.Model.Classes.LocationType> skuGroups = JsonConvert.DeserializeObject<PagedResponse<Winit.Modules.Location.Model.Classes.LocationType>>(data);
                if (skuGroups.PagedData != null)
                {
                    return skuGroups.PagedData.OfType<ILocationType>().ToList();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    private async Task<bool> DeleteLocationTypeAPI(string LocationTypeUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}LocationType/DeleteLocationTypeDetails?UID={LocationTypeUID}",
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
    public async Task<bool> CreateLocationTypeAPI(ILocationType locationType)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}LocationType/CreateLocationType",
                HttpMethod.Post, locationType);
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
    public async Task<bool> UpdateLocationTypeAPI(ILocationType locationType)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(locationType);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}LocationType/UpdateLocationTypeDetails",
                HttpMethod.Put, locationType);
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
    #endregion
}
