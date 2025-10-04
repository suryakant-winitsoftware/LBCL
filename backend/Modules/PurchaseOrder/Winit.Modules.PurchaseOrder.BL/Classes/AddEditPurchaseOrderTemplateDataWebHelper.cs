using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class AddEditPurchaseOrderTemplateDataWebHelper : IAddEditPurchaseOrderTemplateDataHelper
{
    private IAppUser _appUser { get; }
    private IAppSetting _appSetting { get; }
    private IAppConfig _appConfigs { get; }
    private ApiService _apiService { get; }

    public AddEditPurchaseOrderTemplateDataWebHelper(
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
    public async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string? skuGroupTypeUID, string? parentUID)
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
    public async Task<List<SKUAttributeDropdownModel>> GetSKUAttributeDropDownData()
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
    public async Task<List<ISelectionItem>?> GetProductOrgSelectionItems()
    {
        ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetProductOrgSelectionItems",
                HttpMethod.Get);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }

    public async Task<List<ISelectionItem>?> GetProductDivisionSelectionItems()
    {
        ApiResponse<List<ISelectionItem>> apiResponse = await _apiService.FetchDataAsync<List<ISelectionItem>>(
                $"{_appConfigs.ApiBaseUrl}Org/GetProductDivisionSelectionItems",
                HttpMethod.Get);

        return apiResponse != null && apiResponse.IsSuccess ? apiResponse.Data : default;
    }
    public async Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest)
    {
        try
        {
            ApiResponse<PagedResponse<ISKUMaster>> apiResponse =
               await _apiService.FetchDataAsync<PagedResponse<ISKUMaster>>(
               $"{_appConfigs.ApiBaseUrl}SKU/GetAllSKUMasterData",
               HttpMethod.Post, sKUMasterRequest);

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
    
    public async Task<IPurchaseOrderTemplateMaster?> GetPOTemplateMasterByUID(string purchaseOrderTemplateHeaderUID)
    {
        try
        {
            ApiResponse<IPurchaseOrderTemplateMaster> apiResponse =
               await _apiService.FetchDataAsync<IPurchaseOrderTemplateMaster>(
               $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateHeader/GetPurchaseOrderTemplateMasterByUID?uID={purchaseOrderTemplateHeaderUID}",
               HttpMethod.Get);

            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null )
            {
                return apiResponse.Data;
            }
        }
        catch (Exception)
        {
            throw;
        }
        return default;
    }

    public async Task<bool> CUD_POTemplate(IPurchaseOrderTemplateMaster purchaseOrderTemplateMaster)
    {
        try
        {
            ApiResponse<string> apiResponse =
               await _apiService.FetchDataAsync<string>(
               $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateHeader/CUD_PurchaseOrderTemplate",
               HttpMethod.Post, purchaseOrderTemplateMaster);

            return apiResponse != null && apiResponse.IsSuccess;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> DeletePurchaseOrderTemplateLinesByUIDs(List<string> purchaseOrderTemplateLineUids)
    {
        try
        {
            ApiResponse<string> apiResponse =
               await _apiService.FetchDataAsync<string>(
               $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateLine/DeletePurchaseOrderTemplateLinesByUIDs",
               HttpMethod.Delete, purchaseOrderTemplateLineUids);

            return apiResponse != null && apiResponse.IsSuccess;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
