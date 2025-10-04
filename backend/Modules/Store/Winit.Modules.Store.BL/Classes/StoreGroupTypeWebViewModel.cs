using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Store.Model.Interfaces;
using Newtonsoft.Json;
using Winit.Shared.Models.Common;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.Store.BL.Classes;

public class StoreGroupTypeWebViewModel : StoreGroupTypeBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public StoreGroupTypeWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser
       ) : base(serviceProvider, filter, sorter, listHelper, appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
        StoreGroupTypeItemViews = new List<IStoreGroupTypeItemView>();
    }

    #region Concrete Methods
    protected override async Task<bool> CreateStoreGroup_Data(IStoreGroupType storeGroupType)
    {
        return await CreateStoreGroupTypeAPI(storeGroupType);
    }
    protected override async Task<bool> DeleteStoreGroupType_Data(string storeGropTypeUID)
    {
        return await DeleteStoreGroupTypeAPI(storeGropTypeUID);
    }
    protected override async Task<bool> UpdateStoreGroupType_Data(IStoreGroupType storeGroupType)
    {
        return await UpdateStoreGroupTypeAPI(storeGroupType);
    }
    protected override async Task<List<IStoreGroupType>> GetStoreGroupType_Data(string? parentUID, int itemLevel)
    {
        return await GetStoreGroupTypeFromAPI(parentUID, itemLevel);
    }
    #endregion

    #region Api Calling Methods
    public async Task<List<IStoreGroupType>> GetStoreGroupTypeFromAPI(string ParentUID, int Level)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();

            if (ParentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Is));
            if (ParentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Equal));
            if (Level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("LevelNo", "" + Level, Shared.Models.Enums.FilterType.Equal));

            if (FilterCriterias != null && FilterCriterias.Any() && string.IsNullOrEmpty(ParentUID)) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            ApiResponse<PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroupType>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Store.Model.Classes.StoreGroupType>>(
                $"{_appConfigs.ApiBaseUrl}StoreGroupType/SelectAllStoreGroupType",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData!=null)
            {
                return apiResponse.Data.PagedData.OfType<IStoreGroupType>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<bool> DeleteStoreGroupTypeAPI(string StoreGroupTypeUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroupType/DeleteStoreGroupType?UID={StoreGroupTypeUID}",
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
    public async Task<bool> CreateStoreGroupTypeAPI(IStoreGroupType storeGroupType)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroupType/CreateStoreGroupType",
                HttpMethod.Post, storeGroupType);
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
    public async Task<bool> UpdateStoreGroupTypeAPI(IStoreGroupType storeGroupType)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(storeGroupType);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}StoreGroupType/UpdateStoreGroupType",
                HttpMethod.Put, storeGroupType);
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

