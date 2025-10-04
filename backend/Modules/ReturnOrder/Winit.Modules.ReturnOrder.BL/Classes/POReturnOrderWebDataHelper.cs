using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
namespace Winit.Modules.ReturnOrder.BL.Classes;

public class POReturnOrderWebDataHelper : IPOReturnOrderDataHelper
{
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IAppConfig _appConfigs;
    private readonly ApiService _apiService;
    public POReturnOrderWebDataHelper(
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
    public async Task<List<IStore>?> GetChannelPartner(string jobPositionUid)
    {
        ApiResponse<List<IStore>> apiResponse = await _apiService.FetchDataAsync<List<IStore>>(
        $"{_appConfigs.ApiBaseUrl}Store/GetChannelPartner?jobPositionUid={jobPositionUid}",
        HttpMethod.Get);

        return apiResponse != null && apiResponse.Data != null ?
            apiResponse.Data : null;
    }
    public async Task<IStoreMaster?> GetStoreMasterByStoreUID(string storeUID)
    {
        ApiResponse<List<IStoreMaster>> apiResponse = await _apiService.FetchDataAsync<List<IStoreMaster>>(
        $"{_appConfigs.ApiBaseUrl}Store/GetStoreMastersByStoreUIDs",
        HttpMethod.Post, new List<string>
        {
            storeUID
        });

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any() ? apiResponse.Data.FirstOrDefault() : null;
    }
    public async Task<List<IInvoiceView>> GetInvoicesForReturnOrder(InvoiceListRequest invoiceListRequest)
    {
        ApiResponse<List<IInvoiceView>> apiResponse = await _apiService.FetchDataAsync<List<IInvoiceView>>(
        $"{_appConfigs.ApiBaseUrl}Invoice/GetInvoicesForReturnOrder",
        HttpMethod.Post,
        invoiceListRequest);

        return apiResponse != null && apiResponse.Data != null && apiResponse.Data.Any() ? apiResponse.Data : null;
    }
}
