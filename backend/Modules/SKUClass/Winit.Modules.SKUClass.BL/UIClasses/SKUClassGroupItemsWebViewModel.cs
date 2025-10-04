using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.BL.UIClasses;
public class SKUClassGroupItemsWebViewModel : SKUClassGroupItemsBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public SKUClassGroupItemsWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            Shared.Models.Common.IAppConfig appConfigs,
            Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }



    protected override async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupMaster?> GetSKUClassGroupMaster(string skuClassGroupUID)
    {
        try
        {
            ApiResponse<SKUClassGroupDTO> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.SKUClass.Model.Classes.SKUClassGroupDTO>(
                $"{_appConfigs.ApiBaseUrl}SKUClassGroup/GetSKUClassGroupMaster?sKUClassGroupUID={skuClassGroupUID}",
                HttpMethod.Post);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                ISKUClassGroupMaster sKUClassGroupMaster = new SKUClassGroupMaster();
                sKUClassGroupMaster.SKUClassGroup = apiResponse.Data.SKUClassGroup;
                if(apiResponse.Data.SKUClassGroupItems is not null)
                sKUClassGroupMaster.SKUClassGroupItems = apiResponse.Data.SKUClassGroupItems.
                        OfType<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView>().ToList();
                return sKUClassGroupMaster;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return default;
    }
    protected async override Task<List<IOrg>> GetOrgs(List<FilterCriteria> filterCriterias)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = filterCriterias;
            ApiResponse<PagedResponse<Winit.Modules.Org.Model.Classes.Org>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Org.Model.Classes.Org>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetOrgDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
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

    protected async override Task<List<ISKUMaster>?> GetSKUMasters(List<string> orgs)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = 10;
            pagingRequest.PageNumber = 1;
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>> apiResponse = await _apiService.FetchDataAsync
                <PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>($"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData", HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                List<ISKUMaster> sKUMastersFromApi = new List<ISKUMaster>();
                foreach (var skumaster in apiResponse.Data.PagedData.ToList())
                {
                    if (skumaster != null)
                    {
                        sKUMastersFromApi.Add(new SKUMaster()
                        {
                            SKU = skumaster.SKU,
                            SKUAttributes = (skumaster.SKUAttributes != null) ? skumaster.SKUAttributes.Cast<ISKUAttributes>().ToList() : new(),
                            SKUUOMs = skumaster.SKUUOMs != null ? skumaster.SKUUOMs.Cast<ISKUUOM>().ToList() : new(),
                            ApplicableTaxUIDs = skumaster.ApplicableTaxUIDs,
                            SKUConfigs = skumaster.SKUConfigs != null ? skumaster.SKUConfigs.OfType<ISKUConfig>().ToList() : new(),
                        }); ;
                    }
                }
                return sKUMastersFromApi;
            }
            return default;
        }
        catch (Exception)
        {
            throw;
        }
    }
    protected override async Task<bool> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster)
    {
        try
        {
            
            ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUClassGroup/CUD_SKUClassGroupMaster",
                HttpMethod.Post,sKUClassGroupMaster);
            if (apiResponse != null )
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
}

