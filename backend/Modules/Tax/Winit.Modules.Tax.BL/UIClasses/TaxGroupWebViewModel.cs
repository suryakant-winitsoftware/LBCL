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
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Newtonsoft.Json;
using Winit.Modules.Tax.Model.Classes;

namespace Winit.Modules.Tax.BL.UIClasses;
public class TaxGroupWebViewModel : TaxGroupBaseViewModel
{
    private readonly Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public TaxGroupWebViewModel(IServiceProvider serviceProvider,
                IFilterHelper filter,
                ISortHelper sorter,
                IListHelper listHelper,
                IAppUser appUser,
                IAppSetting appSetting,
                IDataManager dataManager,
                IAppConfig appConfigs,
                Base.BL.ApiService apiService) : base(serviceProvider, filter, sorter, listHelper, appUser, appSetting,
                    dataManager)
    {

        _appConfigs = appConfigs;
        _apiService = apiService;
    }
    #region Concrete Methods
    protected override async Task<List<ITaxGroup>> GetTaxGroups_Data()
    {
        return await GetTaxGroupsFromApi();
    }
    protected override async Task<ITaxGroup?> GetTaxGroupByUID_Data(string TaxGroupUID)
    {
        return await GetTaxGroupByUIDFromApi(TaxGroupUID);
    }
    protected override async Task<List<ITaxSelectionItem>> GetTaxSelectionItems_Data(string TaxGroupUID)
    {
        return await GetTaxSelectionItemsFromAPIAsync(TaxGroupUID);
    }
    protected override async Task<bool> CreateTaxGroupMaster_Data(TaxGroupMasterDTO taxGroupMasterDTO)
    {
        return await CreateTaxGroupMasterAPI(taxGroupMasterDTO);
    }
    protected override async Task<bool> UpdateTaxGroupMaster_Data(TaxGroupMasterDTO taxGroupMasterDTO)
    {
        return await UpdateTaxGroupMasterAPI(taxGroupMasterDTO);
    }
    #endregion

    #region API Calling Methods
    private async Task<Winit.Modules.Tax.Model.Interfaces.ITaxGroup?> GetTaxGroupByUIDFromApi(string UID)
    {
        try
        {
            ApiResponse<Winit.Modules.Tax.Model.Classes.TaxGroup> apiResponse =
                await _apiService.FetchDataAsync<Winit.Modules.Tax.Model.Classes.TaxGroup>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxGroupByUID?UID={UID}",
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
    private async Task<List<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>> GetTaxGroupsFromApi()
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest();
            pagingRequest.PageSize = PageSize;
            pagingRequest.PageNumber = PageNumber;
            pagingRequest.IsCountRequired = true;
            pagingRequest.FilterCriterias = TaxGroupFilterCriterias;
            pagingRequest.SortCriterias = TaxGroupSortCriterials;
            ApiResponse<PagedResponse<Winit.Modules.Tax.Model.Classes.TaxGroup>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<Winit.Modules.Tax.Model.Classes.TaxGroup>>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxGroupDetails",
                HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
            {
                TotalTaxGroupItemsCount = apiResponse.Data.TotalCount;
                return apiResponse.Data.PagedData.OfType<Winit.Modules.Tax.Model.Interfaces.ITaxGroup>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<List<ITaxSelectionItem>> GetTaxSelectionItemsFromAPIAsync(string TaxGroupUID)
    {
        try
        {

            ApiResponse<List<Winit.Modules.Tax.Model.Classes.TaxSelectionItem>> apiResponse =
                await _apiService.FetchDataAsync<List<Winit.Modules.Tax.Model.Classes.TaxSelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Tax/GetTaxSelectionItems?UID={TaxGroupUID}",
                HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
            {
                return apiResponse.Data.OfType<ITaxSelectionItem>().ToList();
            }
        }
        catch (Exception)
        {
            throw;
        }
        return new();
    }
    private async Task<bool> CreateTaxGroupMasterAPI(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMaster)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Tax/CreateTaxGroupMaster",
                HttpMethod.Post, taxGroupMaster);
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
    private async Task<bool> UpdateTaxGroupMasterAPI(Winit.Modules.Tax.Model.Classes.TaxGroupMasterDTO taxGroupMaster)
    {
        try
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}Tax/UpdateTaxGroupMaster",
                HttpMethod.Put, taxGroupMaster);
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


