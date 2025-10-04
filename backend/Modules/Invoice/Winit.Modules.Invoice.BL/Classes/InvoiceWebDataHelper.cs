using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Classes;

public class InvoiceWebDataHelper : IInvoiceDataHelper
{
    private IAppUser _appUser { get; }
    private IAppSetting _appSetting { get; }
    private IAppConfig _appConfigs { get; }
    private ApiService _apiService { get; }

    public InvoiceWebDataHelper(
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

    public async Task<PagedResponse<IInvoiceHeaderView>?> GetAllInvoices(List<SortCriteria>? sortCriterias,
        int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            PagingRequest pagingRequest = new PagingRequest
            {
                FilterCriterias = filterCriterias,
                SortCriterias = sortCriterias,
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsCountRequired = isCountRequired,
            };
            ApiResponse<PagedResponse<IInvoiceHeaderView>> apiResponse =
                await _apiService.FetchDataAsync<PagedResponse<IInvoiceHeaderView>>(
                    $"{_appConfigs.ApiBaseUrl}Invoice/GetAllInvoices/{_appUser.SelectedJobPosition.UID}",
                    HttpMethod.Post, pagingRequest);
            if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
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

    public async Task<IInvoiceMaster?> GetInvoiceMasterByInvoiceUID(string invoiceUID)
    {
        try
        {
            ApiResponse<IInvoiceMaster> apiResponse =
                await _apiService.FetchDataAsync<IInvoiceMaster>(
                    $"{_appConfigs.ApiBaseUrl}Invoice/GetInvoiceMasterByInvoiceUID/{invoiceUID}",
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
        return default;
    }

    public async Task<List<IStore>?> GetChannelPartner(string jobPositionUid)
    {
        ApiResponse<List<IStore>> apiResponse = await _apiService.FetchDataAsync<List<IStore>>(
            $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={jobPositionUid}",
            HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null ? apiResponse.Data : null;
    }
}
