using Microsoft.Extensions.Configuration;
using System.Text;
using Newtonsoft.Json;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Vehicle.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Auth.BL.Classes
{
    /// <summary>
    /// Login view model implementation for web authentication
    /// </summary>
    public class LoginWebViewModel : LoginBaseViewModel
    {
        #region Constants
        private const string LoginIdEmptyMessage = "Login ID cannot be empty";
        private const string PasswordEmptyMessage = "Password cannot be empty";
        private const string NotImplementedMessage = "GetOrgByOrgUID is not implemented for the web version";
        #endregion

        #region Fields
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the LoginWebViewModel class
        /// </summary>
        /// <param name="appConfig">Application configuration</param>
        /// <param name="apiService">API service for making HTTP requests</param>
        /// <param name="appUser">Current application user</param>
        /// <param name="modulesMasterHierarchy">Modules master hierarchy view</param>
        /// <param name="configuration">Configuration settings</param>
        /// <param name="jsonSerializerSettings">JSON serializer settings</param>
        /// <param name="commonFunctions">Common utility functions</param>
        public LoginWebViewModel(Winit.Shared.Models.Common.IAppConfig appConfig,
            Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser,
            Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView modulesMasterHierarchy,
            IConfiguration configuration, JsonSerializerSettings jsonSerializerSettings, CommonFunctions commonFunctions) : 
            base(appConfig, apiService, appUser, modulesMasterHierarchy, commonFunctions, jsonSerializerSettings)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Authenticates user and retrieves login token for web application
        /// </summary>
        /// <param name="loginID">User login ID</param>
        /// <param name="password">User password</param>
        /// <param name="challengeCode">Challenge code for additional security</param>
        /// <param name="androidId">Android device ID (not used in web version)</param>
        /// <returns>Login response containing token and auth master data</returns>
        public override async Task<ILoginResponse?> GetTokenByLoginCredentials(string loginID, string password,
            string challengeCode, string androidId)
        {
            if (string.IsNullOrWhiteSpace(loginID))
                throw new ArgumentException(LoginIdEmptyMessage, nameof(loginID));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(PasswordEmptyMessage, nameof(password));

            UserLogin userLogin = new() 
            { 
                UserID = loginID, 
                Password = password, 
                ChallengeCode = challengeCode 
            };

            ApiResponse<ILoginResponse> apiResponse =
                await _apiService.FetchDataAsync<ILoginResponse>(
                    $"{_appConfigs.ApiBaseUrl}{GetTokenEndpoint}",
                    HttpMethod.Post, userLogin);

            return ModifyLoginResponse(apiResponse);
        }

        /// <summary>
        /// Retrieves organization by organization UID (not implemented in web version)
        /// </summary>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <returns>Organization data</returns>
        /// <exception cref="NotImplementedException">This method is not implemented for the web version</exception>
        public override Task<Winit.Modules.Org.Model.Interfaces.IOrg?> GetOrgByOrgUID(string orgUID)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }
        #endregion
    }
}