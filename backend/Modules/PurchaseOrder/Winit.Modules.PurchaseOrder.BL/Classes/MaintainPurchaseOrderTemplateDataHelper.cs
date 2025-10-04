using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class MaintainPurchaseOrderTemplateWebDataHelper : IMaintainPurchaseOrderTemplateDataHelper
{
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;

    public MaintainPurchaseOrderTemplateWebDataHelper(IAppUser appUser,
        IAppSetting appSetting,
        IAppConfig appConfigs,
        ApiService apiService)
    {
        this._appUser = appUser;
        this._appSetting = appSetting;
        this._appConfigs = appConfigs;
        this._apiService = apiService;
    }
    public async Task<PagedResponse<IPurchaseOrderTemplateHeader>?> GetAllPurchaseOrderTemplateHeader(int pageNumber, int pageSize,
        List<SortCriteria> sortCriterias, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        PagingRequest pagingRequest = new PagingRequest();
        pagingRequest.SortCriterias = sortCriterias;
        pagingRequest.FilterCriterias = filterCriterias;
        pagingRequest.PageSize = pageSize;
        pagingRequest.PageNumber = pageNumber;
        pagingRequest.IsCountRequired = isCountRequired;
        ApiResponse<PagedResponse<IPurchaseOrderTemplateHeader>> apiResponse = await _apiService
            .FetchDataAsync<PagedResponse<IPurchaseOrderTemplateHeader>>(
            $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateHeader/GetAllPurchaseOrderTemplateHeaders",
            HttpMethod.Post, pagingRequest);

        if (apiResponse != null && apiResponse.Data != null)
        {
            return apiResponse.Data;
        }
        return default;
    }
    public async Task<bool> DeletePurchaseOrderHeaderByUIDs(List<string> purchaseOrderTemplateHeaderUids)
    {
        try
        {
            ApiResponse<string> apiResponse =
                await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}PurchaseOrderTemplateHeader/DeletePurchaseOrderHeaderByUIDs",
                HttpMethod.Delete, purchaseOrderTemplateHeaderUids);

            if (apiResponse != null && apiResponse.IsSuccess) return true;
            else
            {
                throw new Exception(apiResponse.ErrorMessage);
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
}
