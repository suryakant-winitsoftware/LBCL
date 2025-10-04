using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.Classes;

namespace Winit.Modules.Tax.BL.UIClasses;

public class TaxWebViewModel : TaxBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private readonly Winit.Modules.Base.BL.ApiService _apiService;
    public TaxWebViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IListHelper listHelper,
           IAppUser appUser,
           IAppSetting appSetting,
           IDataManager dataManager,
           IAppConfig appConfigs,
           Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting, dataManager)
    {
        _appConfigs = appConfigs;
        _apiService = apiService;
    }

    #region Concrete Methods
    protected override async Task<bool> UpdateTaxMaster_Data(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster)
    {
        return await UpdateTaxMasterAPI(taxMaster);
    }
    protected override async Task<bool> CreateTaxMaster_Data(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster)
    {
        return await CreateTaxMasterAPI(taxMaster);
    }
    protected override async Task<TaxMasterDTO?> GetTaxMaster_Data(string TaxUID)
    {
        return await GetTaxMasterFromApi(TaxUID);
    }
    protected override async Task<List<ISKUMaster>> GetSKUMasterData_Data()
    {
        return await GetSKUMasterDataFromAPIAsync();
    }
    protected override async Task<List<ITax>> GetTaxs_Data()
    {
        return await getTaxsFromApi();
    }
    #endregion 
    #region API Calling Methods
    private async Task<ITax?> getTaxFromApi(string UID)
    {
        try
        {
            ApiResponse<Winit.Modules.Tax.Model.Classes.Tax> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.Tax.Model.Classes.Tax>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxByUID?UID{UID}",
                HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    private async Task<Winit.Modules.Tax.Model.Classes.TaxMasterDTO?> GetTaxMasterFromApi(string UID)
    {
        try
        {
            ApiResponse<Winit.Modules.Tax.Model.Classes.TaxMasterDTO> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.Tax.Model.Classes.TaxMasterDTO>(
                $"{_appConfigs.ApiBaseUrl}Tax/SelectTaxMasterViewByUID?UID={UID}",
                HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return null;
    }
    private async Task<List<Winit.Modules.Tax.Model.Interfaces.ITax>> getTaxsFromApi()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = PageSize;
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.IsCountRequired = true;
            pagingRequest.FilterCriterias = TaxFilterCriterias;
            pagingRequest.SortCriterias = TaxSortCriterials;
            ApiResponse<PagedResponse<Winit.Modules.Tax.Model.Classes.Tax>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Tax.Model.Classes.Tax>>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                TotalTaxItemsCount = apiResponse.Data.TotalCount;
                return apiResponse.Data.PagedData.OfType<Winit.Modules.Tax.Model.Interfaces.ITax>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<List<ISKUMaster>> GetSKUMasterDataFromAPIAsync()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = 10;
            pagingRequest.PageNumber = 1;
            ApiResponse<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.SKU.Model.Classes.SKUMasterData>>(
                $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",
                HttpMethod.Post, pagingRequest);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                List<ISKUMaster> sKUMastersFromApi = new List<ISKUMaster>();
                foreach (var skumaster in apiResponse.Data.PagedData.ToList())
                {
                    sKUMastersFromApi.Add(new SKUMaster()
                    {
                        SKU = skumaster.SKU,
                        SKUAttributes = (skumaster.SKUAttributes != null) ? skumaster.SKUAttributes.Cast<ISKUAttributes>().ToList() : new(),
                        SKUUOMs = skumaster.SKUUOMs != null ? skumaster.SKUUOMs.Cast<ISKUUOM>().ToList() : new(),
                        ApplicableTaxUIDs = skumaster.ApplicableTaxUIDs,
                        SKUConfigs = skumaster.SKUConfigs != null ? skumaster.SKUConfigs.OfType<ISKUConfig>().ToList() : new(),
                    });
                }
                return sKUMastersFromApi;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<bool> CreateTaxMasterAPI(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Tax/CreateTaxMaster",
                HttpMethod.Post, taxMaster);
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
    private async Task<bool> UpdateTaxMasterAPI(Winit.Modules.Tax.Model.Interfaces.ITaxMaster taxMaster)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Tax/UpdateTaxMaster",
                HttpMethod.Put, taxMaster);
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

