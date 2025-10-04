using Winit.Modules.Base.BL;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Shared.Models.Common;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Winit.Modules.Role.BL.Interfaces;

namespace WINITMobile.Services
{
    /// <summary>
    /// API service for AppVersion operations
    /// </summary>
    public class AppVersionApiService
    {
        #region Constants
        private const string GetAppVersionDetailsByEmpUIDEndpoint = "AppVersion/GetAppVersionDetailsByEmpUID";
        private const string InsertAppVersionUserEndpoint = "AppVersion/InsertAppVersionUser";
        private const string UpdateAppVersionDetailsEndpoint = "AppVersion/UpdateAppVersionDetails";
        #endregion

        #region Fields
        private readonly ApiService _apiService;
        private readonly IAppConfig _appConfig;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the AppVersionApiService class
        /// </summary>
        /// <param name="apiService">Base API service for HTTP operations</param>
        /// <param name="appConfig">Application configuration for API base URL</param>
        public AppVersionApiService(ApiService apiService, IAppConfig appConfig)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets app version details by employee UID
        /// </summary>
        /// <param name="empUID">Employee unique identifier</param>
        /// <param name="temporaryToken">Temporary authentication token to use for API call (optional)</param>
        /// <returns>App version user details or null if not found</returns>
        public async Task<IAppVersionUser> GetAppVersionDetailsByEmpUIDAsync(string empUID, string temporaryToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(empUID))
                    return null;

                string apiUrl = $"{_appConfig.ApiBaseUrl}{GetAppVersionDetailsByEmpUIDEndpoint}?empUID={empUID}";

                // Use concrete class for deserialization, then return as interface
                ApiResponse<AppVersionUser> apiResponse = await _apiService.FetchDataAsync<AppVersionUser>(
                    apiUrl, 
                    HttpMethod.Get,
                    temporaryToken: temporaryToken);

                return apiResponse?.IsSuccess == true && apiResponse.Data != null 
                    ? apiResponse.Data 
                    : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting app version details: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Inserts a new app version user record
        /// </summary>
        /// <param name="empUID">Employee unique identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="appVersion">Application version</param>
        /// <param name="appVersionNumber">Application version number</param>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <param name="gcmKey">Firebase GCM key</param>
        /// <param name="deviceType">Device type (Android/IOS)</param>
        /// <param name="isTest">Whether this is a test user</param>
        /// <param name="temporaryToken">Temporary authentication token to use for API call (optional)</param>
        /// <returns>True if insert was successful, false otherwise</returns>
        public async Task<bool> InsertAppVersionUserAsync(
            string empUID, 
            string deviceId, 
            string appVersion = "", 
            string appVersionNumber = "", 
            string orgUID = "", 
            string gcmKey = "", 
            string deviceType = "Android", 
            bool isTest = false,
            string temporaryToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(empUID) || string.IsNullOrEmpty(deviceId))
                    return false;

                // Parse appVersionNumber to int, default to 0 if parsing fails
                int parsedAppVersionNumber = 0;
                if (!string.IsNullOrEmpty(appVersionNumber))
                {
                    int.TryParse(appVersionNumber, out parsedAppVersionNumber);
                }

                // Use concrete AppVersionUser class instead of anonymous object
                var appVersionUser = new AppVersionUser
                {
                    UID = Guid.NewGuid().ToString(),
                    EmpUID = empUID,
                    DeviceType = deviceType,
                    DeviceId = deviceId,
                    AppVersion = appVersion,
                    AppVersionNumber = parsedAppVersionNumber,
                    ApiVersion = null,
                    DeploymentDateTime = DateTime.MinValue,
                    NextAppVersion = null,
                    NextAppVersionNumber = 0,
                    PublishDate = DateTime.MinValue,
                    IsTest = isTest,
                    IMEINo = null,
                    OrgUID = orgUID,
                    GcmKey = gcmKey,
                    IMEINo2 = null,
                    CreatedTime = DateTime.Now,
                    CreatedBy = empUID
                };

                string apiUrl = $"{_appConfig.ApiBaseUrl}{InsertAppVersionUserEndpoint}";

                // Pass the raw object directly to FetchDataAsync - let ApiService handle serialization
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>(
                    apiUrl, 
                    HttpMethod.Post, 
                    appVersionUser,
                    temporaryToken: temporaryToken);

                return apiResponse?.IsSuccess == true && apiResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting app version user: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates app version user details
        /// </summary>
        /// <param name="empUID">Employee unique identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="appVersion">Application version</param>
        /// <param name="temporaryToken">Temporary authentication token to use for API call (optional)</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public async Task<bool> UpdateAppVersionDetailsAsync(string empUID, string deviceId, string appVersion = "", string appVersionNumber = "", string gcmKey = "", 
            string temporaryToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(empUID) || string.IsNullOrEmpty(deviceId))
                    return false;

                // First get the existing record to get the UID
                var existingRecord = await GetAppVersionDetailsByEmpUIDAsync(empUID, temporaryToken);
                if (existingRecord == null)
                    return false;

                // Parse appVersionNumber to int, default to 0 if parsing fails
                int parsedAppVersionNumber = 0;
                if (!string.IsNullOrEmpty(appVersionNumber))
                {
                    int.TryParse(appVersionNumber, out parsedAppVersionNumber);
                }

                existingRecord.DeviceId = deviceId;
                existingRecord.AppVersion = appVersion;
                existingRecord.AppVersionNumber = parsedAppVersionNumber;
                existingRecord.IMEINo = null;
                existingRecord.GcmKey = gcmKey;
                existingRecord.IMEINo2 = null;
                existingRecord.ModifiedBy = empUID;
                existingRecord.ModifiedTime = DateTime.Now;
                existingRecord.ServerModifiedTime = DateTime.Now;

                string apiUrl = $"{_appConfig.ApiBaseUrl}{UpdateAppVersionDetailsEndpoint}";

                // Pass the raw object directly to FetchDataAsync - let ApiService handle serialization
                ApiResponse<bool> apiResponse = await _apiService.FetchDataAsync<bool>(
                    apiUrl, 
                    HttpMethod.Post,
                    existingRecord,
                    temporaryToken: temporaryToken);

                return apiResponse?.IsSuccess == true && apiResponse.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating app version details: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
} 