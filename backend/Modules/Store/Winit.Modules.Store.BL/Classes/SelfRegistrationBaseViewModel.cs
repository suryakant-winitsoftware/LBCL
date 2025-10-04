using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.util;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Notification.Model.Classes;
using Winit.Modules.Notification.Model.Constant;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Store.BL.Classes;

public class SelfRegistrationBaseViewModel : ISelfRegistrationViewModel
{
    private IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Common.BL.Interfaces.IAppUser _appuser;
    private Winit.Modules.Base.BL.ApiService _apiService;
    public ISelfRegistration selfRegistration { get; set; }
    public SelfRegistrationBaseViewModel(IServiceProvider serviceProvider,
          IFilterHelper filter,
          ISortHelper sorter,
          IAppUser appuser,
          IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _apiService = apiService;
        _appConfigs = appConfigs;
        _appuser = appuser;
        selfRegistration = new SelfRegistration();
    }
    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool IsUIDRequired = true)
    {
        baseModel.CreatedBy = _appuser?.Emp?.UID;
        baseModel.ModifiedBy = _appuser?.Emp?.UID;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    public async Task<bool> HandleSelfRegistration()
    {
        try
        {
            AddCreateFields(selfRegistration);
            return await SaveAndUpdateSelfRegistration(selfRegistration);
        }
        catch (Exception ex)
        { throw; }
    }
    public async Task<bool> VerifyOTP()
    {
        try
        {
            return await HandleVerifyOTP(selfRegistration);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task SendSms(string Otp, string MobileNumber)
    {
        try
        {
            await InsertSmsIntoRabbitMQ(Otp, MobileNumber);
        }
        catch (Exception ex)
        {
            // Log Exception
        }
    }
    public async Task<bool> InsertSmsIntoRabbitMQ(string Otp, string MobileNumber)
    {
        try
        {
            List<NotificationRequest> notificationRequests = new List<NotificationRequest>();
            notificationRequests.Add(new NotificationRequest
            {
                UniqueUID = Guid.NewGuid().ToString(),
                LinkedItemType = "SelfRegistration",
                LinkedItemUID = Otp,
                TemplateName = "General",
                Receiver = new List<string> { MobileNumber },
                NotificationRoute = NotificationRoutes.Notification_General_SMS
            });

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
               $"{_appConfigs.NotificationApiUrl}Notification/PublishMessagesByRoutingKey",
               HttpMethod.Post, notificationRequests);
            return apiResponse.IsSuccess;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> SaveAndUpdateSelfRegistration(ISelfRegistration selfRegistration)
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
               $"{_appConfigs.ApiBaseUrl}SelfRegistration/CrudSelfRegistration",
               HttpMethod.Post, selfRegistration);

        return apiResponse != null && apiResponse.IsSuccess;
    }

    public async Task<bool> HandleVerifyOTP(ISelfRegistration selfRegistration)
    {
        ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
            $"{_appConfigs.ApiBaseUrl}SelfRegistration/VerifyOtp",
            HttpMethod.Post, selfRegistration);

        if (apiResponse != null && apiResponse.IsSuccess)
        {
            selfRegistration.UID = apiResponse.Data;
            return true;
        }

        return false;
    }

    public async Task<ILoginResponse?> GetToken()
    {
        try
        {
            return await GetTokenWithoutLoginCredintials();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public async Task<IStore> GetStatusFromStore(string UID)
    {
        try
        {
            return await GetStatusFromStoreWithoutLoginCredintials(UID);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<ILoginResponse?> GetTokenWithoutLoginCredintials()
    {
        ApiResponse<LoginResponseDTO> apiResponse =
            await _apiService.FetchDataAsync<LoginResponseDTO>(
                $"{_appConfigs.ApiBaseUrl}Auth/GetTokenWithoutCredentials",
                HttpMethod.Post, null);
        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            ILoginResponse loginResponse = new LoginResponse
            {
                Token = apiResponse.Data.Token,
                AuthMaster = new AuthMaster(),
            };
            return loginResponse;
        }
        return default;
    }
    public async Task<IStore?> GetStatusFromStoreWithoutLoginCredintials(string UID)
    {
        ApiResponse<IStore> apiResponse =
            await _apiService.FetchDataAsync<IStore>(
                $"{_appConfigs.ApiBaseUrl}Store/SelectStoreByUIDWithoutCache?UID={UID}",
                HttpMethod.Get, null);
        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            return apiResponse.Data;
        }
        return default;
    }
}