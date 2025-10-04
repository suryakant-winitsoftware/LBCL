using Azure;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Common.BL.Classes;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.BL.Classes;

/// <summary>
/// Base class for login view model providing common authentication functionality
/// </summary>
public abstract class LoginBaseViewModel : ILoginViewModel
{
    #region Constants
    private const string VerifyUserIdEndpoint = "Auth/VerifyUserIdAndSendRandomPassword?LoginId=";
    private const string UpdatePasswordEndpoint = "Auth/UpdateExistingPasswordWithNewPassword";
    private const string EncryptTextEndpoint = "Auth/GetEncryptedText?text=";
    protected const string GetTokenEndpoint = "Auth/GetToken";
    
    // Error Messages
    private const string UserIdEmptyMessage = "User ID cannot be empty";
    private const string NewPasswordEmptyMessage = "New password cannot be empty";
    private const string RequestFailedMessage = "Failed to send the request";
    private const string MailFailedMessage = "Failed to send mail";
    private const string PasswordChangeErrorMessage = "Error occurred while changing the password";
    private const string EncryptFailedMessage = "Failed to encrypt text";
    private const string NoResponseMessage = "No response received";
    private const string UnknownErrorMessage = "Unknown error occurred";
    private const string ServerDownMessage = "Unable to connect to server. Please try after sometime.";
    private const string ParseResponseFailedMessage = "Failed to parse response";
    private const string ParseErrorResponseFailedMessage = "Failed to parse error response";
    
    // Status Codes
    private const int SuccessStatusCode = 200;
    private const int InternalServerErrorStatusCode = 500;
    private const int ServiceUnavailableStatusCode = 503;
    #endregion

    #region Fields
    protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
    protected Winit.Modules.Base.BL.ApiService _apiService;
    protected IAppUser _appUser;
    protected CommonFunctions _commonFunctions;
    protected JsonSerializerSettings _jsonSerializerSettings;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the modules master hierarchy view
    /// </summary>
    public Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView ModulesMasterHierarchy { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the LoginBaseViewModel class
    /// </summary>
    /// <param name="appConfig">Application configuration</param>
    /// <param name="apiService">API service for making HTTP requests</param>
    /// <param name="appUser">Current application user</param>
    /// <param name="modulesMasterHierarchy">Modules master hierarchy view</param>
    /// <param name="commonFunctions">Common utility functions</param>
    /// <param name="jsonSerializerSettings">JSON serializer settings</param>
    public LoginBaseViewModel(Winit.Shared.Models.Common.IAppConfig appConfig,
        Winit.Modules.Base.BL.ApiService apiService, IAppUser appUser,
        Winit.Modules.Role.Model.Interfaces.IMenuMasterHierarchyView modulesMasterHierarchy, 
        CommonFunctions commonFunctions, JsonSerializerSettings jsonSerializerSettings)
    {
        _appConfigs = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _appUser = appUser ?? throw new ArgumentNullException(nameof(appUser));
        ModulesMasterHierarchy = modulesMasterHierarchy ?? throw new ArgumentNullException(nameof(modulesMasterHierarchy));
        _commonFunctions = commonFunctions ?? throw new ArgumentNullException(nameof(commonFunctions));
        _jsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));
    }
    #endregion

    #region Abstract Methods
    /// <summary>
    /// Authenticates user and retrieves login token
    /// </summary>
    /// <param name="loginID">User login ID</param>
    /// <param name="password">User password</param>
    /// <param name="challengeCode">Challenge code for additional security</param>
    /// <param name="androidId">Android device ID</param>
    /// <returns>Login response containing token and auth master data</returns>
    public abstract Task<ILoginResponse?> GetTokenByLoginCredentials(string loginID, string password, string challengeCode, string androidId);
    
    /// <summary>
    /// Retrieves organization by organization UID
    /// </summary>
    /// <param name="orgUID">Organization unique identifier</param>
    /// <returns>Organization data</returns>
    public abstract Task<Winit.Modules.Org.Model.Interfaces.IOrg?> GetOrgByOrgUID(string orgUID);
    #endregion

    #region Public Methods
    /// <summary>
    /// Verifies user ID and sends random password via email
    /// </summary>
    /// <param name="userId">User ID to verify</param>
    /// <returns>Success status and message</returns>
    public virtual async Task<(bool IsSuccessResponse, string Msg)> VerifyUserIdAndSendRandomPassword(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return (false, UserIdEmptyMessage);

        try
        {
            ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}{VerifyUserIdEndpoint}{userId}",
                HttpMethod.Post);
            
            return apiResponse != null 
                ? (apiResponse.IsSuccess, apiResponse.Data) 
                : (false, RequestFailedMessage);
        }
        catch (Exception ex)
        {
            // Log exception here if logging is available
            return (false, MailFailedMessage);
        }
    }

    /// <summary>
    /// Updates existing password with new password
    /// </summary>
    /// <param name="changePassword">Password change request data</param>
    /// <returns>Result message</returns>
    public virtual async Task<string> UpdateExistingPasswordWithNewPassword(IChangePassword changePassword)
    {
        if (changePassword == null)
            throw new ArgumentNullException(nameof(changePassword));

        if (string.IsNullOrWhiteSpace(changePassword.NewPassword))
            throw new ArgumentException(NewPasswordEmptyMessage, nameof(changePassword));

        try
        {
            changePassword.NewPassword = await GetEncryptedText(changePassword.NewPassword);
            
            ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}{UpdatePasswordEndpoint}",
                HttpMethod.Post, changePassword);
            
            if (apiResponse == null)
                return RequestFailedMessage;

            return apiResponse.IsSuccess 
                ? apiResponse.Data 
                : throw new Exception(apiResponse.ErrorMessage);
        }
        catch (Exception ex)
        {
            throw new Exception(PasswordChangeErrorMessage, ex);
        }
    }

    /// <summary>
    /// Encrypts the provided text
    /// </summary>
    /// <param name="text">Text to encrypt</param>
    /// <returns>Encrypted text</returns>
    public async Task<string> GetEncryptedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        try
        {
            ApiResponse<string> apiResponse =
            await _apiService.FetchDataAsync(
                $"{_appConfigs.ApiBaseUrl}{EncryptTextEndpoint}{text}",
                HttpMethod.Get);
            
            if (apiResponse == null)
                return RequestFailedMessage;

            return apiResponse.IsSuccess 
                ? apiResponse.Data 
                : apiResponse.ErrorMessage;
        }
        catch (Exception ex)
        {
            // Log exception here if logging is available
            return EncryptFailedMessage;
        }
    }
    #endregion

    #region Protected Methods
    /// <summary>
    /// Modifies login response from API response
    /// </summary>
    /// <param name="apiResponse">API response containing login data</param>
    /// <returns>Modified login response</returns>
    protected ILoginResponse ModifyLoginResponse(ApiResponse<ILoginResponse> apiResponse)
    {
        if (apiResponse == null)
            return new LoginResponse { StatusCode = InternalServerErrorStatusCode, ErrorMessage = NoResponseMessage };

        if (apiResponse.IsSuccess && apiResponse.Data != null)
        {
            apiResponse.Data.StatusCode = apiResponse.StatusCode;
            return apiResponse.Data;
        }

        return new LoginResponse()
        {
            StatusCode = apiResponse.StatusCode,
            ErrorMessage = apiResponse?.ErrorMessage ?? UnknownErrorMessage
        };
    }

    /// <summary>
    /// Modifies login response from string API response
    /// </summary>
    /// <param name="apiResponse">String API response</param>
    /// <returns>Modified login response</returns>
    protected ILoginResponse ModifyLoginResponse(ApiResponse<string> apiResponse)
    {
        if (apiResponse == null)
            return new LoginResponse { StatusCode = InternalServerErrorStatusCode, ErrorMessage = NoResponseMessage };

        if (apiResponse.IsSuccess && !string.IsNullOrEmpty(apiResponse.Data))
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ILoginResponse>(
                    _commonFunctions.GetDataFromResponse(apiResponse.Data), 
                    _jsonSerializerSettings);
                
                if (response != null)
                {
                    response.StatusCode = SuccessStatusCode;
                    return response;
                }
            }
            catch (JsonException ex)
            {
                // Log exception here if logging is available
                return new LoginResponse()
                {
                    StatusCode = InternalServerErrorStatusCode,
                    ErrorMessage = ParseResponseFailedMessage
                };
            }
        }

        if (!apiResponse.IsSuccess && apiResponse.Data == null)
        {
            return new LoginResponse()
            {
                StatusCode = ServiceUnavailableStatusCode,
                ErrorMessage = ServerDownMessage
            };
        }

        try
        {
            var errorResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(
                _commonFunctions.GetDataFromResponse(apiResponse.Data), 
                _jsonSerializerSettings);
            
            return new LoginResponse()
            {
                StatusCode = errorResponse?.StatusCode ?? InternalServerErrorStatusCode,
                ErrorMessage = errorResponse?.ErrorMessage ?? UnknownErrorMessage
            };
        }
        catch (JsonException ex)
        {
            // Log exception here if logging is available
            return new LoginResponse()
            {
                StatusCode = InternalServerErrorStatusCode,
                ErrorMessage = ParseErrorResponseFailedMessage
            };
        }
    }
    #endregion
}

