using Nest;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.BL.Classes;

public class AddProductPopUpV1DataHelper : IAddProductPopUpDataHelper
{
    private IAppUser _appUser { get; }
    private IAppSetting _appSetting { get; }
    private IAppConfig _appConfigs { get; }
    private ApiService _apiService { get; }

    public AddProductPopUpV1DataHelper(
            IAppUser appUser,
            IAppSetting appSetting,
            IAppConfig appConfigs,
            ApiService apiService)
    {
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _apiService = apiService;
    }
    public async Task<List<SKUAttributeDropdownModel>?> GetSKUAttributeData()
    {
        List<SKUAttributeDropdownModel> sKUAttributeDropdownModels = await GetSKUAttributeDropDownData();
        if (sKUAttributeDropdownModels == null || !sKUAttributeDropdownModels.Any())
        {
            return default;
        }
        for (int i = 0; i < sKUAttributeDropdownModels.Count; i++)
        {
            SKUAttributeDropdownModel item = sKUAttributeDropdownModels[i];
            for (int j = 0; j < sKUAttributeDropdownModels.Count; j++)
            {
                if (item.UID == sKUAttributeDropdownModels[j].ParentUID)
                {
                    SKUAttributeDropdownModel childItem = sKUAttributeDropdownModels[j];
                    sKUAttributeDropdownModels.RemoveAt(j);
                    sKUAttributeDropdownModels.Insert(i + 1, childItem);
                    i++;
                    break;
                }
            }
        }
        List<Task> tasks = [];
        foreach (SKUAttributeDropdownModel item in sKUAttributeDropdownModels)
        {
            tasks.Add(AddDataSourceToDD(item));
        }

        var divisions = GetProductDivisionSelectionItems();
        await Task.WhenAll(tasks);
        await Task.WhenAll(divisions);
        divisions.Result?.ForEach(e => e.Code = e.UID);
        sKUAttributeDropdownModels.Add(new SKUAttributeDropdownModel
        {
            UID = "Division",
            Code = "Division",
            DropDownTitle = "Division",
            DropDownDataSource = divisions.Result
        });
        return sKUAttributeDropdownModels;
    }

    public async Task<List<ISelectionItem>?> GetProductDivisionSelectionItems()
    {
        ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetProductDivisionSelectionItems",
                HttpMethod.Get);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }
    public async Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID)
    {
        List<SKUGroupSelectionItem> data = await GetSKUGroupSelectionItemBySKUGroupTypeUID(null, selectedItemUID);
        return data != null && data.Any() ? data.ToList<ISelectionItem>() : [];
    }
    public bool FilterAction(List<FilterCriteria> filterCriterias, ISKU sKU)
    {
        if (filterCriterias == null || !filterCriterias.Any())
        {
            return true;
        }

        if (sKU is SKUV1 sKUV1) // Check if sKU can be cast to SKUV1
        {
            foreach (FilterCriteria filter in filterCriterias)
            {
                if (filter.Value is List<string> filterValues) // Check if the filter value is a list of strings
                {
                    if (!sKUV1.FilterKeys.Any(filterValues.Contains)) // Check if any filter values match the filter keys
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }

    private async Task AddDataSourceToDD(SKUAttributeDropdownModel item)
    {
        List<SKUGroupSelectionItem> data = await GetSKUGroupSelectionItemBySKUGroupTypeUID(item.UID, null);
        if (data != null && data.Any())
        {
            item.DropDownDataSource.AddRange(data.ToList<ISelectionItem>());
        }
    }
    private async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
    {
        try
        {
            ApiResponse<List<SKUGroupSelectionItem>> apiResponse =
               await _apiService.FetchDataAsync<List<SKUGroupSelectionItem>>(
               $"{_appConfigs.ApiBaseUrl}SKUGroup/GetSKUGroupSelectionItemBySKUGroupTypeUID?skuGroupTypeUID={skuGroupTypeUID}&parentUID={parentUID}",
               HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    private async Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData()
    {
        try
        {
            ApiResponse<List<SKUAttributeDropdownModel>> apiResponse =
               await _apiService.FetchDataAsync<List<SKUAttributeDropdownModel>>(
               $"{_appConfigs.ApiBaseUrl}SKUAttributes/GetSKUGroupTypeForSKuAttribute",
               HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    public async Task<List<ISKUV1>> GetAllSKUs(PagingRequest pagingRequest)
    {
        try
        {
            ApiResponse<PagedResponse<SKUV1>> apiResponse =
               await _apiService.FetchDataAsync<PagedResponse<SKUV1>>(
               $"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetails",
               HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList<ISKUV1>();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }

    public async Task<List<ISKUGroup>> GetSKUGroup(PagingRequest pagingRequest)
    {
        try
        {
            ApiResponse<PagedResponse<ISKUGroup>> apiResponse = 
                await _apiService.FetchDataAsync<PagedResponse<ISKUGroup>>($"{_appConfigs.ApiBaseUrl}" +
                $"SKUGroup/SelectAllSKUGroupDetails", HttpMethod.Post, pagingRequest);
            if(apiResponse!=null && apiResponse.IsSuccess&& apiResponse.Data!=null&&apiResponse.Data.PagedData!=null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }
    public async Task<List<ISKUGroupType>> GetSKUGroupType(PagingRequest pagingRequest)
    {
        try
        {
            Shared.Models.Common.ApiResponse<PagedResponse<ISKUGroupType>> apiResponse = 
                await _apiService.FetchDataAsync<PagedResponse<ISKUGroupType>>($"{_appConfigs.ApiBaseUrl}" +
                $"SKUGroupType/SelectAllSKUGroupTypeDetails", HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                return apiResponse.Data.PagedData.ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return [];
    }

}
