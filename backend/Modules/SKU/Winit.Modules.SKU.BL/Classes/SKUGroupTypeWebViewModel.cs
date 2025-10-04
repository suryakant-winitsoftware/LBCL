using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.BL.Classes;

public class SKUGroupTypeWebViewModel : SKUGroupTypeBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public SKUGroupTypeWebViewModel(
            IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser
       ) : base(serviceProvider, filter, sorter, listHelper, appUser)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }

    #region Concrete Methods
    protected override async Task<List<ISKUGroupType>> GetSKUGroupType_Data(string parentUID, int level)
    {
        return await GetSKUGroupTypeFromAPI(parentUID, level);
    }
    protected override async Task<bool> UpdateSKUGroupType_Data(ISKUGroupType sKUGroupType)
    {
        return await UpdateSKUGroupTypeAPI(sKUGroupType);
    }
    protected override async Task<bool> DeleteSKUGroupType_Data(string sKUGroupTypeUID)
    {
        return await DeleteSKUGroupTypeAPI(sKUGroupTypeUID);
    }
    protected override async Task<bool> CreateSKUGroup_Data(ISKUGroupType sKUGroupType)
    {
        return await CreateSKUGroupTypeAPI(sKUGroupType);
    }
    #endregion
    #region API Calling Methods
    public async Task<List<ISKUGroupType>> GetSKUGroupTypeFromAPI(string ParentUID, int Level)
    {
        try
        {
            Console.WriteLine($"[SKUGroupType] Fetching data - ParentUID: {ParentUID ?? "NULL"}, Level: {Level}");
            
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.FilterCriterias = new List<Shared.Models.Enums.FilterCriteria>();

            if (ParentUID == null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Is));
            if (ParentUID != null) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ParentUID", ParentUID, Shared.Models.Enums.FilterType.Equal));
            if (Level != 0) pagingRequest.FilterCriterias.Add(new Shared.Models.Enums.FilterCriteria("ItemLevel", "" + Level, Shared.Models.Enums.FilterType.Equal));

            if (FilterCriterias != null && FilterCriterias.Any() && string.IsNullOrEmpty(ParentUID)) pagingRequest.FilterCriterias.AddRange(FilterCriterias);
            
            Console.WriteLine($"[SKUGroupType] API URL: {_appConfigs.ApiBaseUrl}SKUGroupType/SelectAllSKUGroupTypeDetails");
            Console.WriteLine($"[SKUGroupType] Filter count: {pagingRequest.FilterCriterias?.Count ?? 0}");
            
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUGroupType>>(
                $"{_appConfigs.ApiBaseUrl}SKUGroupType/SelectAllSKUGroupTypeDetails",
                HttpMethod.Post, pagingRequest);
                
            if (apiResponse != null)
            {
                Console.WriteLine($"[SKUGroupType] API Response - Success: {apiResponse.IsSuccess}, StatusCode: {apiResponse.StatusCode}");
                if (!apiResponse.IsSuccess)
                {
                    Console.WriteLine($"[SKUGroupType] API Error: {apiResponse.ErrorMessage}");
                }
                
                if (apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
                {
                    var result = apiResponse.Data.PagedData.OfType<ISKUGroupType>().ToList();
                    Console.WriteLine($"[SKUGroupType] Retrieved {result.Count} items");
                    return result;
                }
            }
            else
            {
                Console.WriteLine("[SKUGroupType] API Response is null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SKUGroupType] Exception: {ex.Message}");
            Console.WriteLine($"[SKUGroupType] StackTrace: {ex.StackTrace}");
            throw;
        }
        Console.WriteLine("[SKUGroupType] Returning empty list");
        return new();
    }
    private async Task<bool> DeleteSKUGroupTypeAPI(string SKUGroupTypeUID)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroupType/DeleteSKUGroupTypeByUID?UID={SKUGroupTypeUID}",
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
    public async Task<bool> CreateSKUGroupTypeAPI(ISKUGroupType sKUGroupType)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroupType/CreateSKUGroupType",
                HttpMethod.Post, sKUGroupType);
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
    public async Task<bool> UpdateSKUGroupTypeAPI(ISKUGroupType sKUGroupType)
    {
        try
        {
            string jsonBody = JsonConvert.SerializeObject(sKUGroupType);
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}SKUGroupType/UpdateSKUGroupType",
                HttpMethod.Put, sKUGroupType);
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

