using Newtonsoft.Json;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using WINITSharedObjects.Enums;

namespace Winit.Modules.SKU.BL.Classes;
public class SKUGroupWebViewModel : SKUGroupBaseViewModel
{
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public SKUGroupWebViewModel(
                IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
                Winit.Modules.Base.BL.ApiService apiService,
                IAppUser appUser
           ) : base(serviceProvider, filter, sorter, listHelper, appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
        SKUGroupItemViews = new List<ISKUGroupItemView>();
    }
    #region Concrete Methods
    protected override async Task<List<ISKUGroupType>> GetSKUGroupTypes_Data()
    {
        return await GetSKUGroupTypesFromAPI();
    }
    protected override async Task<List<ISKUGroupItemView>> GetSKUGroup_Data(string? ParentUID, int level)
    {
        return await GetSKUGroupFromAPI(ParentUID, level);
    }
    protected override async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSuppliers_Data()
    {
        return await GetSuppliersFromAPI();
    }
    protected override async Task<bool> UpdateSKUGroup_Data(ISKUGroup sKUGroup)
    {
        return await UpdateSKUGroupAPI(sKUGroup);
    }
    protected override async Task<bool> CreateSKUGroup_Data(ISKUGroup sKUGroup)
    {
        return await CreateSKUGroupAPI(sKUGroup);
    }
    protected override async Task<bool> DeleteSKUGroup_Data(string sKUGroupUID)
    {
        return await DeleteSKUGroupAPI(sKUGroupUID);
    }
    protected override async Task CreateSKUHierarchyAPICall(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        await CallCreateSKUHierarchyAPI(sKUGroupItemView);
    }
    #endregion
    #region API Calling Methods
    public async Task<List<ISKUGroupItemView>> GetSKUGroupFromAPI(string parentUID, int level)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            if (parentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", parentUID, Shared.Models.Enums.FilterType.Is));
            if (parentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", parentUID, Shared.Models.Enums.FilterType.Equal));
            if (level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ItemLevel", "" + level, Shared.Models.Enums.FilterType.Equal));
            if (FilterCriterias != null && FilterCriterias.Any() && string.IsNullOrEmpty(parentUID)) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            ApiResponse<List<Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.SKU.Model.UIClasses.SKUGroupItemView>>(
            $"{_appConfigs.ApiBaseUrl}SKUGroup/SelectAllSKUGroupItemViews",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null )
            {
               return apiResponse.Data.OfType<ISKUGroupItemView>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<bool> DeleteSKUGroupAPI(string SKUGroupUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroup/DeleteSKUGroup?UID={SKUGroupUID}",
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
    private async Task<bool> CreateSKUGroupAPI(ISKUGroup sKUGroup)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroup/CreateSKUGroup",
                HttpMethod.Post, sKUGroup);
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
    private async Task<bool> UpdateSKUGroupAPI(ISKUGroup sKUGroup)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(sKUGroup);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroup/UpdateSKUGroup",
                HttpMethod.Put, sKUGroup);
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
    private async Task<List<ISKUGroupType>> GetSKUGroupTypesFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>>(
                 $"{_appConfigs.ApiBaseUrl}SKUGroupType/SelectAllSKUGroupTypeDetails",
                 HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.OfType<ISKUGroupType>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new List<ISKUGroupType>();
    }
    private async Task<List<Winit.Modules.Org.Model.Interfaces.IOrg>> GetSuppliersFromAPI()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>
                {
                    new Shared.Models.Enums.FilterCriteria("OrgTypeUID","Supplier",Shared.Models.Enums.FilterType.Equal)
                };
            ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.OfType<Winit.Modules.Org.Model.Interfaces.IOrg>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    public async Task CallCreateSKUHierarchyAPI(Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView sKUGroupItemView)
    {
        try
        {
            string url = $"{_appConfigs.ApiBaseUrl}SKUGroup/InsertSKUGroupHierarchy?type={sKUGroupItemView.SKUGroupTypeName}&uid={sKUGroupItemView.UID}";
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

