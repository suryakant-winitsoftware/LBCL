using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;

namespace Winit.Modules.Location.BL.Classes;

public class CityBranchMappingWebViewModel : CityBranchMappingBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public CityBranchMappingWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IListHelper listHelper,
           IAppUser appUser,
           IDataManager dataManager,
           IAppConfig appConfigs,
           Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, dataManager)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }
    protected override async Task<List<ICityBranch>> GetCityBranchDetails()
    {
        return await GetCityBranchFromApi();
    }
    protected override async Task<List<ISelectionItem>> GetBranchDetails(string UID)
    {
        return await GetBranchDetailsFromApi();
    }
    protected override async Task<bool> CreateCityBranchMapping(List<ICityBranchMapping> cityBranchMappings)
    {
        return await InsertCityBranchMappingFromApi(cityBranchMappings);
    }
    private async Task<List<Winit.Modules.Location.Model.Interfaces.ICityBranch>> GetCityBranchFromApi()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = PageSize;
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.IsCountRequired = true;
            pagingRequest.FilterCriterias = CityBranchFilterCriterias;
            pagingRequest.SortCriterias = CityBranchSortCriterials;
            ApiResponse<PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Location.Model.Interfaces.ICityBranch>>(
                $"{_appConfigs.ApiBaseUrl}CityBranch/SelectCityBranchDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                TotalItemsCount = apiResponse.Data.TotalCount;
                return apiResponse.Data.PagedData.OfType<Winit.Modules.Location.Model.Interfaces.ICityBranch>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<List<ISelectionItem>> GetBranchDetailsFromApi()
    {
        try
        {
            ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}CityBranch/SelectBranchDetails", HttpMethod.Post);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }

        return new List<ISelectionItem>();
    }
    private async Task<bool> InsertCityBranchMappingFromApi(List<ICityBranchMapping> cityBranchMappings)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}CityBranch/CreateCityBranchMapping", HttpMethod.Post, cityBranchMappings);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }



}

