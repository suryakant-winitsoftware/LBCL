using Azure.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Winit.Modules.Auth.DL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.BL.Classes
{
    /// <summary>
    /// Login view model implementation for mobile app authentication
    /// </summary>
    public class LoginAppViewModel : LoginBaseViewModel
    {
        #region Constants
        private const string SourceHeaderKey = "Source";
        private const string AppHeaderValue = "App";
        private const string LoginIdEmptyMessage = "Login ID cannot be empty";
        private const string PasswordEmptyMessage = "Password cannot be empty";
        private const string SkuAttributesKey = "skuAttributes";
        private const string SkuTypesKey = "skuTypes";
        private const string FilterDataErrorMessage = "Error occurred while generating filter data";
        private const string NotImplementedMessage = "GetOrgByOrgUID is not implemented for the app version";
        #endregion

        #region Fields
        private readonly ICommonDataDL _commonDataDL;
        private readonly IDataManager _dataManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the LoginAppViewModel class
        /// </summary>
        /// <param name="appConfig">Application configuration</param>
        /// <param name="apiService">API service for making HTTP requests</param>
        /// <param name="commonDataDL">Common data access layer</param>
        /// <param name="appUser">Current application user</param>
        /// <param name="dataManager">Data manager for local storage</param>
        /// <param name="modulesMasterHierarchy">Modules master hierarchy view</param>
        /// <param name="jsonSerializerSettings">JSON serializer settings</param>
        /// <param name="commonFunctions">Common utility functions</param>
        public LoginAppViewModel(Winit.Shared.Models.Common.IAppConfig appConfig,
            Winit.Modules.Base.BL.ApiService apiService, ICommonDataDL commonDataDL, IAppUser appUser, 
            IDataManager dataManager, Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView modulesMasterHierarchy, 
            JsonSerializerSettings jsonSerializerSettings, CommonFunctions commonFunctions) :
            base(appConfig, apiService, appUser, modulesMasterHierarchy, commonFunctions, jsonSerializerSettings)
        {
            _commonDataDL = commonDataDL ?? throw new ArgumentNullException(nameof(commonDataDL));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Authenticates user and retrieves login token for mobile app
        /// </summary>
        /// <param name="loginID">User login ID</param>
        /// <param name="password">User password</param>
        /// <param name="challengeCode">Challenge code for additional security</param>
        /// <param name="androidId">Android device ID</param>
        /// <returns>Login response containing token and auth master data</returns>
        public override async Task<ILoginResponse?> GetTokenByLoginCredentials(string loginID, string password, string challengeCode, string androidId)
        {
            if (string.IsNullOrWhiteSpace(loginID))
                throw new ArgumentException(LoginIdEmptyMessage, nameof(loginID));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(PasswordEmptyMessage, nameof(password));

            UserLogin userLogin = new UserLogin 
            { 
                UserID = loginID, 
                Password = password, 
                ChallengeCode = challengeCode, 
                DeviceId = androidId 
            };

            var customHeaders = new Dictionary<string, string>
            {
                { SourceHeaderKey, AppHeaderValue }
            };

            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}{GetTokenEndpoint}", 
                HttpMethod.Post, 
                userLogin);

            return ModifyLoginResponse(apiResponse);
        }

        /// <summary>
        /// Generates and caches filter data for the application
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task GenerateFilterData()
        {
            if (_appUser?.OrgUIDs == null)
                return;

            try
            {
                await GenerateSkuAttributesData();
                await GenerateSkuTypesData();
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                throw new Exception(FilterDataErrorMessage, ex);
            }
        }

        /// <summary>
        /// Retrieves organization by organization UID (not implemented in app version)
        /// </summary>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <returns>Organization data</returns>
        /// <exception cref="NotImplementedException">This method is not implemented for the app version</exception>
        public override Task<IOrg?> GetOrgByOrgUID(string orgUID)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Generates and caches SKU attributes data
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task GenerateSkuAttributesData()
        {
            var skuAttributes = await _commonDataDL.GetAllSKUAttibutes(_appUser.OrgUIDs);
            
            if (skuAttributes != null && skuAttributes.Count() > 0)
            {
                _dataManager.SetData(SkuAttributesKey, skuAttributes);
            }
        }

        /// <summary>
        /// Generates and caches SKU types data
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task GenerateSkuTypesData()
        {
            var skuTypes = await _commonDataDL.GetAttributeTypes(_appUser.OrgUIDs);
            
            if (skuTypes != null && skuTypes.Any())
            {
                var groupedData = skuTypes
                    .GroupBy(e => e.UID)
                    .ToDictionary(
                        group => group.Key, 
                        group => group.OrderBy(item => item.UID).ToList());
                
                _dataManager.SetData(SkuTypesKey, groupedData);
            }
        }
        #endregion
    }
}


