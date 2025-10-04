using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Constants;
using Winit.Modules.Base.BL;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.BL.Classes;
public class ClearDataValidate
{
    private readonly ISyncViewModel _syncViewModel;
    protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
    protected Winit.Modules.Base.BL.ApiService _apiService;

    public ClearDataValidate(ISyncViewModel syncViewModel, IAppConfig appConfig, ApiService apiService)
    {
        _syncViewModel = syncViewModel;
        _appConfigs = appConfig;
        _apiService = apiService;
    }
    public async Task Execute()
    {
        try
        {
            IMobileAppAction? mobileAppAction = await GetMobileAppActionByLoginId(_syncViewModel.UserName);
            _syncViewModel.AppAction = mobileAppAction != null && mobileAppAction.Status == 0 ? mobileAppAction.Action : DBActions.NO_ACTION;
        }
        catch (Exception)
        {
            _syncViewModel.AppAction = DBActions.NO_ACTION;
        }
    }
    public async Task SetNext()
    {
        //Shared.sFAModel.Interface.IProcess next = null;
        _syncViewModel.ClearData = false;
        if ((_syncViewModel.AppAction ?? "").Equals(DBActions.CLEAR_DATA))
        {
            _syncViewModel.ClearData = true;
            //next = new DebugLogSync(_syncViewModel);
        }
        else if ((_syncViewModel.AppAction ?? "").Equals(DBActions.CLEAR_DATA_AFTER_UPLOAD) && _syncViewModel.IsSyncPushPending)
        {
            // If data pending first try to post data then call DebugLogSync
            _syncViewModel.Mode = SyncMode.Upload;
            //new SyncDbFull(_syncViewModel).Process();

            /*var dpush = Sync.BLayer.DeltaSyncManager.IsDataPendingToPush();
            _syncViewModel.IsSyncPushPending = dpush.IsPending;
            // If after sync no data is pending then do cleardata
            if (!_syncViewModel.IsSyncPushPending)
            {
                _syncViewModel.ClearData = true;
            }
            next = new DebugLogSync(_syncViewModel);*/
        }
        else if ((_syncViewModel.AppAction ?? "").Equals(DBActions.CLEAR_DATA_AFTER_UPLOAD))
        {
            _syncViewModel.ClearData = true;
            _syncViewModel.Mode = SyncMode.Download;
            //next = new DebugLogSync(_syncViewModel);
        }
        else if ((_syncViewModel.AppAction ?? "").Equals(DBActions.NO_ACTION))
        {
            _syncViewModel.Mode = SyncMode.Download;
            //next = _syncViewModel.IsMainDbExist ? (Shared.sFAModel.Interface.IProcess)new SyncDbFull(_syncViewModel)
            //: (Shared.sFAModel.Interface.IProcess)new SyncDbInit(_syncViewModel);
        }
        //else if ((_syncViewModel.AppAction ?? "").Equals(MobileAppActionType.CLEAR_DATA_AFTER_UPLOAD) && !_syncViewModel.IsSyncPushPending)
        //{
        //    next = new DebugLogSync(_syncViewModel);
        //}
        //return next;
    }

    private async Task<IMobileAppAction?> GetMobileAppActionByLoginId(string? loginId)
    {
        try
        {
            ApiResponse<MobileAppAction> apiResponse =
            await _apiService.FetchDataAsync<MobileAppAction>(
                $"{_appConfigs.ApiBaseUrl}MobileAppAction/GetMobileAppAction?userName={loginId}",
                HttpMethod.Get);
            return apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null ? apiResponse.Data : (IMobileAppAction?)default;
        }
        catch (Exception)
        {
            throw;
        }
    }
}