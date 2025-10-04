using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Auth.BL.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Shared.Models.Constants;
using WINITMobile.Pages.Base;
using Winit.Modules.Base.Model.Constants;
namespace WINITMobile.Pages;

/// <summary>
/// Serializable DTO for LoginResponse that contains only concrete types.
/// Used for JSON serialization/deserialization in secure storage.
/// </summary>
public class SerializableLoginResponse
{
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string Token { get; set; } = string.Empty;
    public SerializableAuthMaster? AuthMaster { get; set; }
}

/// <summary>
/// Serializable DTO for AuthMaster that contains only concrete types.
/// </summary>
public class SerializableAuthMaster
{
    public Winit.Modules.Emp.Model.Classes.Emp? Emp { get; set; }
    public Winit.Modules.JobPosition.Model.Classes.JobPosition? JobPosition { get; set; }
    public Winit.Modules.Role.Model.Classes.Role? Role { get; set; }
    public List<Winit.Modules.Vehicle.Model.Classes.VehicleStatus>? VehicleStatuses { get; set; }
}

public partial class Index : WINITMobile.Pages.Base.BaseComponentBase
{
    private string version { get; set; }
    private string releaseDate = "2025-07-03";
    private string buildType;
    #region Constants

    // Database Constants
    private const string SqliteDatabaseName = "WINITSQLite.db";
    private const string TempFileExtension = ".old";

    // Storage Keys
    //private const string SelectedCultureKey = "SelectedCulture";
    //private const string RememberMeKey = "RememberMe";
    //private const string RememberedUsernameKey = "RememberedUsername";
    //private const string RememberedPasswordKey = "RememberedPassword";
    //private const string ExpirationDateKey = "ExpirationDate";


    // Default Values
    private const string DefaultCulture = "en-GB";
    private const string DevEnvironment = "dev";
    private const string DevBuildType = "Dev";
    private const string LiveBuildType = "Live";
    private const string ReleaseDate = "2025-04-14";

    // Input Types
    private const string UserIdInputType = "UserId";
    private const string PasswordInputType = "Password";

    // Key Constants
    private const string EnterKey = "Enter";

    // Error Messages
    private const string ValidationErrorTitle = "Validation Error";
    private const string UserIdRequiredMessage = "User ID is required.";
    private const string PasswordRequiredMessage = "Password is required.";
    private const string DeviceErrorTitle = "Device Error";
    private const string DeviceIdNotFoundMessage = "Device ID not found.";
    private const string NetworkErrorTitle = "Network Error";
    private const string NetworkErrorMessage = "No internet connection. Please check your network settings.";
    private const string LoginFailedTitle = "Login failed";
    private const string AuthenticationFailedMessage = "Authentication failed.";

    // Offline login constants
    private const string OfflineLoginErrorTitle = "Offline Login Failed";
    private const string OfflineCredentialsNotFoundMessage = "No offline credentials available. Please connect to internet for first login.";
    private const string OfflineCredentialsExpiredMessage = "Offline credentials have expired. Please connect to internet to refresh.";
    private const string OfflineCredentialsInvalidMessage = "Invalid offline credentials. Please try again.";
    private const string OfflineLoginSuccessTitle = "Offline Login";
    private const string OfflineLoginSuccessMessage = "Logged in successfully using offline credentials.";
    private const int OfflineCredentialsValidityDays = 7; // Offline credentials valid for 7 days
    private const string TimeoutTitle = "Timeout";
    private const string TimeoutMessage = "Request timed out. Please try again.";
    private const string ErrorTitle = "Error";
    private const string UnexpectedErrorMessage = "An unexpected error occurred during login.";
    private const string SqliteDialogTitle = "Sqlite";
    private const string SqliteDialogMessage = "Sqlite database is missing. Would you like to download it?";
    private const string YesButton = "Yes";
    private const string CancelButton = "Cancel";
    private const string DataErrorTitle = "Data Error";
    private const string InsufficientDataMessage = "Insufficient data to proceed. Please try again.";
    private const string SqliteDownloadFailedMessage = "SQLite database download failed.";
    private const string SyncPendingTitle = "Sync Pending";
    private const string SyncPendingMessage = "You have unsynchronized data. Please complete synchronization before changing user.";
    private const string SyncButton = "Sync Now";
    private const string ContinueAnywayButton = "Continue Anyway";

    // Retry Constants
    private const int MaxRetries = 5;
    private const int RetryDelayBase = 2000;
    private const int RememberMeExpirationDays = 30;

    // Console Messages
    private const string SqliteDeletedMessage = "SQLite database deleted successfully.";
    private const string SqliteRenamedMessage = "Could not delete SQLite database, but renamed it for later deletion.";

    // JavaScript Functions
    private const string GetFirebaseTokenFunction = "androidInterop.getFirebaseToken";
    private const string LogToConsoleFunction = "logToConsole";

    // Route Constants
    private const string DashboardRoute = "DashBoard";
    private const string ProfileRoute = "profile";

    // Confirmation Messages
    private const string QuitConfirmationTitle = "Confirm?";
    private const string QuitConfirmationMessage = "Are you sure, You want to Quit the App?";

    #endregion

    #region Private Fields

    

    #endregion

    #region Public Properties

    public bool IsForgotPasswordPopUpOpen { get; set; }
    public bool IsShowLangPopUp { get; set; } = false;
    public bool IsLoginInProgress { get; set; } = false;

    #endregion

    #region Private Properties

    private bool RememberMe { get; set; } = false;
    private string _firebaseToken { get; set; }
    private string _userID { get; set; }
    private string _password { get; set; }
    private string _errorMsg { get; set; }
    private ElementReference _passWordElement { get; set; }
    private string _selectedCulture { get; set; } = DefaultCulture;
    private string _selectedLanguage { get; set; } = "";
    private string _androidId { get; set; }
    private bool _isOfflineMode { get; set; } = false; // Indicates if user is currently in offline mode

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Initializes the component with culture settings and user preferences.
    /// </summary>
    protected override void OnInitialized()
    {
        string savedCulture = _storageHelper.GetStringFromPreferences(LocalStorageKeys.SelectedCulture);

        buildType = _appConfigs.AppEnvironment; //.ApiBaseUrl.Contains(DevEnvironment) ? DevBuildType : LiveBuildType;

        _backbuttonhandler.SetCurrentPage(this);

        SetupCulture(savedCulture);
        LoadResources(null, _languageService.SelectedCulture);
        _languageService.OnLanguageChanged += LoadResources;

        InitializeRememberMeSettings();
    }

    /// <summary>
    /// Initializes asynchronous components like Firebase token and device ID.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
#if ANDROID
        //await CheckAndRequestPermissions();
        await CheckAndRequestNotificationPermissions();
        await SetFirebaseToken();
#endif

        await GetAndroidFromDevice();
        version = AppInfo.Current.VersionString;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Sets up culture configuration based on saved preferences.
    /// </summary>
    /// <param name="savedCulture">Previously saved culture setting</param>
    private void SetupCulture(string savedCulture)
    {
        if (!string.IsNullOrEmpty(savedCulture))
        {
            _languageService.SelectedCulture = savedCulture;
        }
        else
        {
            _languageService.SelectedCulture = DefaultCulture;
        }
    }

    /// <summary>
    /// Initializes remember me settings and handles auto-login if applicable.
    /// </summary>
    private void InitializeRememberMeSettings()
    {
        RememberMe = _storageHelper.GetBooleanFromPreference(LocalStorageKeys.RememberMe, false);
        string lastLoggedInUser = _storageHelper.GetStringFromPreferences(LocalStorageKeys.LastLoginId, string.Empty);

        if (!RememberMe)
            return;

        string storedUsername = _storageHelper.GetStringFromPreferences(LocalStorageKeys.RememberedUsername, string.Empty);
        DateTime expirationDate = _storageHelper.GetDateTimeFromPreference(LocalStorageKeys.ExpirationDate, DateTime.MinValue);

        if (expirationDate > DateTime.Now && storedUsername == lastLoggedInUser)
        {
            _userID = storedUsername;
        }
        else
        {
            ClearRememberedCredentials();
        }
    }

    /// <summary>
    /// Clears all stored authentication credentials from secure storage.
    /// </summary>
    private void ClearRememberedCredentials()
    {
        _storageHelper.RemoveKey(LocalStorageKeys.RememberedUsername);
        _storageHelper.RemoveKey(LocalStorageKeys.RememberMe);
        _storageHelper.RemoveKey(LocalStorageKeys.ExpirationDate);
    }

    /// <summary>
    /// Retrieves Android device ID for authentication purposes.
    /// </summary>
    protected async Task GetAndroidFromDevice()
    {
        if (string.IsNullOrEmpty(_androidId))
        {
            _androidId = _deviceService.GetDeviceId();
        }
        await Task.CompletedTask;
    }



    #endregion

    #region Localization Methods

    /// <summary>
    /// Loads localization resources for the specified culture.
    /// </summary>
    /// <param name="sender">Event sender (not used)</param>
    /// <param name="culture">Culture code to load resources for</param>
    private void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }

    /// <summary>
    /// Selects a language for the application.
    /// </summary>
    /// <param name="language">Language code to select</param>
    private void SelectLanguage(string language)
    {
        _selectedLanguage = language;
    }

    /// <summary>
    /// Proceeds with language change if a language has been selected.
    /// </summary>
    private void Proceed()
    {
        if (string.IsNullOrEmpty(_selectedLanguage))
            return;

        ChangeLanguage(_selectedLanguage);
        IsShowLangPopUp = false;
    }

    /// <summary>
    /// Changes the application culture and saves the preference.
    /// </summary>
    /// <param name="culture">Culture code to set</param>
    private void ChangeLanguage(string culture)
    {
        _languageService.SelectedCulture = culture;
        _selectedCulture = culture;
        _storageHelper.SaveStringToPreference(LocalStorageKeys.SelectedCulture, culture);
    }

    /// <summary>
    /// Opens the language selection popup.
    /// </summary>
    protected void OpenLanguage()
    {
        IsShowLangPopUp = true;
    }

    /// <summary>
    /// Checks if the specified language is currently selected.
    /// </summary>
    /// <param name="language">Language code to check</param>
    /// <returns>True if the language is selected, false otherwise</returns>
    private bool IsLanguageSelected(string language)
    {
        return _selectedLanguage == language;
    }

    #endregion

    #region Login Event Handlers

    /// <summary>
    /// Handles button click for login with proper async execution and UI state management.
    /// </summary>
    private async Task OnLoginButtonClick()
    {
        if (IsLoginInProgress)
            return;

        await InvokeAsync(async () => await ProcessLogin()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles keyboard events for login form inputs with Enter key navigation.
    /// </summary>
    /// <param name="e">Keyboard event arguments</param>
    /// <param name="inputBox">Name of the input box that triggered the event</param>
    private async Task KeyHandler(KeyboardEventArgs e, string inputBox = PasswordInputType)
    {
        if (e.Key != EnterKey)
            return;

        if (inputBox == UserIdInputType)
        {
            await _passWordElement.FocusAsync();
            return;
        }

        if (!IsLoginInProgress)
        {
            await InvokeAsync(async () => await ProcessLogin()).ConfigureAwait(false);
        }
    }

    #endregion

    #region Main Login Logic

    /// <summary>
    /// Handles user login with improved early return pattern and reduced complexity.
    /// Validates input, authenticates user, and navigates to dashboard upon success.
    /// </summary>
    public async Task OnLoginClick()
    {
        try
        {
            if (!await ValidateLoginInputs())
                return;

            ShowLoader("Checking connectivity...");
            // Check internet connectivity silently - if no internet, try offline login
            // Use silent connectivity check to avoid showing alerts when offline login is available
            bool hasInternet = await CheckInternetConnectivity(showAlert: false, timeoutMs: 800);
            if (!hasInternet)
            {
                // Attempt offline login
                await AttemptOfflineLogin();
                return;
            }

            var tokenData = await AuthenticateUser();
            if (tokenData == null)
                return;

            // Validate device access with token (but don't store token yet)
            if (!await ValidateDeviceAccess(tokenData))
                return;

            await HandleUserChangeWithSyncValidation(tokenData);

            // Only store token after ALL validations pass
            await StoreAuthenticationToken(tokenData);

            // Execute ClearData action based on server configuration
            await HandleClearDataAction(tokenData);

            await HandleRememberMe();

            await SetupUserSession(tokenData);

            // Save user context for future upload operations (for user change scenarios)
            await SaveUserContextToPreferences();

            // Store offline credentials after successful online login
            await StoreOfflineCredentials();

            await HandleDatabaseAndNavigation();
        }
        catch (HttpRequestException)
        {
            await _alertService.ShowErrorAlert(NetworkErrorTitle, "Failed to connect to server. Please check your internet connection.");
        }
        catch (TaskCanceledException)
        {
            await _alertService.ShowErrorAlert(TimeoutTitle, TimeoutMessage);
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(ErrorTitle, ex.Message);
        }
        finally
        {
            //_password = string.Empty;
        }
    }

    /// <summary>
    /// Validates login inputs with early returns for validation errors.
    /// </summary>
    /// <returns>True if all validations pass, false otherwise</returns>
    private async Task<bool> ValidateLoginInputs()
    {
        if (string.IsNullOrEmpty(_userID))
        {
            await _alertService.ShowErrorAlert(ValidationErrorTitle, UserIdRequiredMessage);
            return false;
        }

        if (string.IsNullOrEmpty(_password))
        {
            await _alertService.ShowErrorAlert(ValidationErrorTitle, PasswordRequiredMessage);
            return false;
        }

        if (string.IsNullOrEmpty(_androidId))
        {
            await _alertService.ShowErrorAlert(DeviceErrorTitle, DeviceIdNotFoundMessage);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Handles user change validation with synchronization check
    /// </summary>
    /// <param name="tokenData">Authentication token data containing user info</param>
    private async Task HandleUserChangeWithSyncValidation(ILoginResponse tokenData)
    {
        try
        {
            ShowLoader("Validating user change");

            bool isDatabaseAvailable = await _dataSyncValidationService.IsDatabaseAvailableAsync();
            if (!isDatabaseAvailable)
            {
                // If database not available then no need to check same or different user 
                return;
            }

            // Extract current user UID from token
            string currentEmpUID = tokenData.AuthMaster?.Emp?.UID;

            if (string.IsNullOrEmpty(currentEmpUID))
            {
                await _alertService.ShowErrorAlert(ErrorTitle, "Unable to retrieve user information from token.");
                return;
            }

            // Get last logged in user UID from local storage
            string lastLoggedInUserEmpUID = await _localStorageService.GetItem<string>(LocalStorageKeys.LastEmpUID) ?? string.Empty;

            // If nothing came from local storage or current user UID is same as stored UID, no need to check sync
            if (string.IsNullOrEmpty(lastLoggedInUserEmpUID) || currentEmpUID == lastLoggedInUserEmpUID)
            {
                return; // Move to next step
            }

            // User is different, check for pending sync data
            var pendingData = await _dataSyncValidationService.GetPendingPushDataAsync();

            if (pendingData != null && pendingData.Count > 0)
            {
                // Show alert that system will try to sync data
                await _alertService.ShowErrorAlert(
                    "Data Sync Required",
                    "You have unsynchronized data. System will try to sync the data then you can try to login again.");

                // Handle upload with proper loader and progress reporting
                await HandleOldUserDataUpload();

                // After sync attempt, stop login process
                // User needs to try login again
                return;
            }

            // No pending data, can proceed with user change
            // Delete SQLite database for user change
            await DeleteSQLiteDatabase();
        }
        catch (Exception ex)
        {
            // Log exception and show generic error
            await _alertService.ShowErrorAlert(ErrorTitle, "Error during user validation. Please try again.");
            Console.WriteLine($"User validation error: {ex}");
        }
    }



    /// <summary>
    /// Saves current user context to preferences for future upload operations.
    /// This should be called after successful login to store the user context.
    /// </summary>
    /// <param name="tokenData">Authentication token data containing user info</param>
    private async Task SaveUserContextToPreferences()
    {
        try
        {
            string orgUID = _appUser.SelectedJobPosition.OrgUID;
            string empUID = _appUser.Emp.UID;
            string jobPositionUID = _appUser.SelectedJobPosition.UID;

            if (!string.IsNullOrEmpty(orgUID) && !string.IsNullOrEmpty(empUID) && !string.IsNullOrEmpty(jobPositionUID))
            {
                await _localStorageService.SetItem(LocalStorageKeys.LastOrgUID, orgUID);
                await _localStorageService.SetItem(LocalStorageKeys.LastEmpUID, empUID);
                await _localStorageService.SetItem(LocalStorageKeys.LastJobPositionUID, jobPositionUID);

                Console.WriteLine($"Saved user context - EmpUID: {empUID}, OrgUID: {orgUID}, JobPositionUID: {jobPositionUID}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save user context: {ex.Message}");
        }
    }

    /// <summary>
    /// Authenticates user and returns token data.
    /// </summary>
    /// <returns>Token data if authentication successful, null otherwise</returns>
    private async Task<ILoginResponse> AuthenticateUser()
    {
        ShowLoader("Authenticating user...");
        _userID = _userID.Trim();
        string challengeCode = _sHACommonFunctions.GenerateChallengeCode();
        _password = _password.Trim();
        string encryptedPassword = _sHACommonFunctions.EncryptPasswordWithChallenge(_password, challengeCode);

        var tokenData = await _loginViewModel.GetTokenByLoginCredentials(_userID, encryptedPassword, challengeCode, _androidId);

        if (tokenData == null || string.IsNullOrEmpty(tokenData.Token))
        {
            await _alertService.ShowErrorAlert(LoginFailedTitle, tokenData?.ErrorMessage ?? AuthenticationFailedMessage);
            return null;
        }

        return tokenData;
    }

    /// <summary>
    /// Stores authentication token in localStorage after all validations pass
    /// </summary>
    /// <param name="tokenData">Authentication token data</param>
    private async Task StoreAuthenticationToken(ILoginResponse tokenData)
    {
        // Convert interface to serializable DTO for storage
        var serializableTokenData = ConvertToSerializableLoginResponse(tokenData);
        await _localStorageService.SetItem(LocalStorageKeys.Token, serializableTokenData.Token);
        //await _localStorageService.SetItem(LocalStorageKeys.TokenData, serializableTokenData);
        await _localStorageService.SetItem(LocalStorageKeys.FirebaseKey, _firebaseToken);
        await _localStorageService.SetItem(LocalStorageKeys.LastLoginId, _userID);
    }

    /// <summary>
    /// Sets up user session data after successful authentication.
    /// </summary>
    /// <param name="tokenData">Authentication token data</param>
    private async Task SetupUserSession(ILoginResponse tokenData)
    {
        ShowLoader("Initializing user session");
        // Tokens are already set earlier in the login flow
        await _authStateProvider.GetAuthenticationStateAsync();

        //_userID = tokenData.AuthMaster?.Emp?.LoginId; // This will be the data entered by user
        _appUser.Emp = tokenData.AuthMaster?.Emp;
        _appUser.SelectedJobPosition = tokenData.AuthMaster?.JobPosition;
        _appUser.Vehicles = tokenData.AuthMaster?.VehicleStatuses;

        var vehicleStatuses = tokenData.AuthMaster?.VehicleStatuses;
        _appUser.Vehicle = vehicleStatuses != null ? System.Linq.Enumerable.FirstOrDefault(vehicleStatuses) : null;

        _appUser.Role = tokenData.AuthMaster?.Role;

        // Vishal commented on 7th Jun 2025 don't delete below line
        //await _userMasterDataViewModel.GetUserMasterData(_userID);
        StateHasChanged();
    }

    /// <summary>
    /// Validates device access by orchestrating between API service and business logic
    /// </summary>
    /// <param name="tokenData">Authentication token data containing employee information</param>
    /// <returns>True if device access is valid, false otherwise</returns>
    private async Task<bool> ValidateDeviceAccess(ILoginResponse tokenData)
    {
        try
        {
            ShowLoader("Validating device registration");
            // Extract employee UID from token data
            string empUID = tokenData.AuthMaster?.Emp?.UID;

            if (string.IsNullOrEmpty(empUID))
            {
                await _alertService.ShowErrorAlert(ErrorTitle, "Unable to retrieve employee information from authentication token.");
                return false;
            }

            if (string.IsNullOrEmpty(_androidId))
            {
                await _alertService.ShowErrorAlert(DeviceErrorTitle, DeviceIdNotFoundMessage);
                return false;
            }

            // Extract token for temporary use in API calls
            string token = tokenData.Token;
            if (string.IsNullOrEmpty(token))
            {
                await _alertService.ShowErrorAlert(ErrorTitle, "Authentication token is missing.");
                return false;
            }

            // Step 1: Get app version user data via API service using temporary token
            var appVersionUser = await _appVersionApiService.GetAppVersionDetailsByEmpUIDAsync(empUID, token);

            // Step 2: Validate device access using business logic
            var validationResult = await _deviceValidationService.ValidateDeviceAccessAsync(appVersionUser, _androidId);

            if (!validationResult.IsValid)
            {
                await _alertService.ShowErrorAlert(validationResult.ErrorTitle, validationResult.ErrorMessage);
                return false;
            }

            // Step 3: Handle data operations based on validation result
            if (validationResult.RequiresInsert)
            {
                // Insert new app version user record for first-time device registration using temporary token
                bool insertSuccess = await _appVersionApiService.InsertAppVersionUserAsync(
                    empUID: empUID,
                    deviceId: _androidId,
                    temporaryToken: token,
                    appVersion: GetCurrentAppVersion(),
                    appVersionNumber: GetCurrentAppVersionNumber(),
                    orgUID: _appUser.SelectedJobPosition?.OrgUID ?? string.Empty,
                    gcmKey: _firebaseToken ?? string.Empty,
                    deviceType: GetDeviceType(),
                    isTest: false
                );

                if (!insertSuccess)
                {
                    await _alertService.ShowErrorAlert("Device Registration Error", "Failed to register device. Please try again.");
                    return false;
                }
            }
            else if (validationResult.RequiresUpdate)
            {
                // Update existing app version user record using temporary token
                bool updateSuccess = await _appVersionApiService.UpdateAppVersionDetailsAsync(
                    empUID,
                    _androidId,
                    GetCurrentAppVersion(),
                    GetCurrentAppVersionNumber(),
                    _firebaseToken ?? string.Empty,
                    token
                );

                // Note: Update failure is not critical, just log it
                if (!updateSuccess)
                {
                    Console.WriteLine($"Warning: Failed to update app version details for user {empUID}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(ErrorTitle, $"Device validation failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current application version
    /// </summary>
    /// <returns>Current app version string</returns>
    private string GetCurrentAppVersion()
    {
        try
        {
            return AppInfo.Current.VersionString ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }

    /// <summary>
    /// Gets the current application version number
    /// </summary>
    /// <returns>Current app version number string</returns>
    private string GetCurrentAppVersionNumber()
    {
        try
        {
            // Extract numeric version from version string (e.g., "1.2.3" -> "123")
            string version = AppInfo.Current.VersionString ?? "1.0.0";
            return version.Replace(".", "");
        }
        catch
        {
            return "100";
        }
    }

    /// <summary>
    /// Gets the current device type
    /// </summary>
    /// <returns>Device type string from DeviceInfo.Platform</returns>
    private string GetDeviceType()
    {
        try
        {
            return DeviceInfo.Platform.ToString();
        }
        catch
        {
            return DevicePlatform.Android.ToString(); // Default to Android platform string
        }
    }

    #endregion

    #region Database and Navigation Methods

    /// <summary>
    /// Handles database verification and navigation with early returns.
    /// </summary>
    private async Task HandleDatabaseAndNavigation()
    {
        ShowLoader("Checking DB availability");
        string sqlLitePath = Path.Combine(_appConfigs.BaseFolderPath, "DB", SqliteDatabaseName);

        if (File.Exists(sqlLitePath))
        {
            await HandleExistingDatabase();
            return;
        }

        await HandleMissingDatabase();
    }

    /// <summary>
    /// Handles scenario when SQLite database exists.
    /// </summary>
    private async Task HandleExistingDatabase()
    {
        await ProcessDatabaseSync();
        await MoveToNextScreen();
    }
    /// <summary>
    /// Processes database synchronization after login.
    /// Downloads all table groups and tables from server to local SQLite database.
    /// </summary>
    private async Task ProcessDatabaseSync()
    {
        // Use silent sync for login flow - no success messages, seamless user experience
        await ProcessDatabaseSyncSilent();

        // Alternative examples for different scenarios:
        // await ProcessDatabaseSyncSilent(new List<string> { "Master", "Sales" });  // Silent sync for specific groups
        // await ProcessDatabaseSyncInteractive();  // Interactive sync with success message (for Settings)
        // await ProcessDatabaseSyncAll(showTableLevelErrors: true, showSuccessMessage: false);  // Detailed errors but no success message
    }

    /// <summary>
    /// Handles scenario when SQLite database is missing.
    /// </summary>
    private async Task HandleMissingDatabase()
    {
        await ProcessDatabaseDownload();
    }

    /// <summary>
    /// Processes SQLite database download and setup.
    /// </summary>
    private async Task ProcessDatabaseDownload()
    {
        ShowLoader("Initializing DB");
        bool downloadSuccess = await InitiateDbCreation();

        if (downloadSuccess)
        {
            await MoveToNextScreen();
            return;
        }
    }

    #endregion

    #region User Session Management

    /// <summary>
    /// Handles remember me functionality by storing or clearing credentials.
    /// </summary>
    protected async Task HandleRememberMe()
    {
        if (RememberMe)
        {
            _storageHelper.SaveStringToPreference(LocalStorageKeys.RememberedUsername, _userID);
            _storageHelper.SaveBooleanToPreference(LocalStorageKeys.RememberMe, true);
            DateTime expirationDate = DateTime.Now.AddDays(RememberMeExpirationDays);
            _storageHelper.SaveDateTimeToPreference(LocalStorageKeys.ExpirationDate, expirationDate);
        }
        else
        {
            ClearRememberedCredentials();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets Firebase token for push notifications.
    /// </summary>
    protected async Task SetFirebaseToken()
    {
        try
        {
            _firebaseToken = await jsRuntime.InvokeAsync<string>(GetFirebaseTokenFunction, Array.Empty<object>());
            await jsRuntime.InvokeVoidAsync(LogToConsoleFunction, "Firebase Token:" + _firebaseToken);
        }
        catch (Exception)
        {
            // Firebase token retrieval failed - continue without it
        }
    }

    #endregion

    #region Permission Methods (Future Implementation)
    private async Task DisplayPermissionDeniedAlert(string permission)
    {
        await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert("Permission Denied", $"{permission} permission is required to use this feature. The app will now close.", "OK");
        CloseApp();
    }
    
    // Note: These methods are currently commented out but kept for future implementation
    // They should be uncommented and properly implemented when permission handling is required
    private async Task CheckAndRequestNotificationPermissions()
    {
        try
        {
            // Check and request Notification permission
            PermissionStatus notificationStatus = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (notificationStatus != PermissionStatus.Granted)
            {
                notificationStatus = await Permissions.RequestAsync<Permissions.PostNotifications>();
                if (notificationStatus != PermissionStatus.Granted)
                {
                    await DisplayPermissionDeniedAlert("Notifications");
                    return;
                }
            }
            // Check and request Location permission
            PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    await DisplayPermissionDeniedAlert("Location");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayPermissionDeniedAlert(ex.Message);
        }

    }
    /*
    private async Task CheckAndRequestNotificationPermissions()
    {
        // Implementation for notification permissions
    }

    private async Task CheckAndRequestPermissions()
    {
        // Implementation for various app permissions
    }

    private async Task DisplayPermissionDeniedAlert(string permission)
    {
        await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
            "Permission Denied", 
            $"{permission} permission is required to use this feature. The app will now close.", 
            "OK");
        CloseApp();
    }
    */

    #endregion
    private async Task MoveToNextScreen()
    {
        ShowLoader("Initializing system");
        bool isDatabaseAvailable = await _dataSyncValidationService.IsDatabaseAvailableAsync();
        if (!isDatabaseAvailable)
        {
            await _alertService.ShowErrorAlert(ErrorTitle, "Database not initialized with required data.");
            return;
        }
        await GetEmpdataAndBindAppuser();
        await MoveToDashboard();
    }
    private async Task MoveToDashboard()
    {
        _appUser.IsCheckedIn = false;

        if (_appUser.Emp?.IsMandatoryChangePassword == true)
        {
            HideLoader();
            _navigationManager.NavigateTo(ProfileRoute);
            return;
        }

        NavigateTo(DashboardRoute);
    }

    /// <summary>
    /// Initiates database creation with comprehensive progress reporting and proper error handling.
    /// This method replaces the old implementation with the new refactored SyncDbInit API.
    /// </summary>
    /// <returns>True if database creation and download succeeded, false otherwise</returns>
    private async Task<bool> InitiateDbCreation()
    {
        bool isOperationComplete = false;

        try
        {
            // Create progress callback for real-time UI updates
            var progressCallback = new Progress<Winit.Modules.Auth.BL.Classes.DatabaseInitializationProgress>(progress =>
            {
                // Only update if operation is still in progress
                if (!isOperationComplete)
                {
                    // Update loader with current progress and percentage
                    //string progressMessage = $"{progress.Message} ({progress.PercentageComplete}%)";
                    string progressMessage = $"{progress.Message}";
                    ShowLoader(progressMessage);

                    // Force UI update to show progress immediately
                    InvokeAsync(() => StateHasChanged());
                }

                // Log progress for debugging
                Console.WriteLine($"DB Init Progress: {progress.Stage} - {progress.PercentageComplete}% - {progress.Message}");
            });

            // Create cancellation token with timeout for safety
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(10)); // 10-minute timeout

            // Start database creation with progress reporting
            bool success = await _syncDbHelper.InitiateDbCreationAsync(
                progressCallback,
                cancellationTokenSource.Token);

            if (!success)
            {
                isOperationComplete = true; // Stop progress callbacks

                // Get detailed error message from SyncDbInit
                string errorMessage = !string.IsNullOrEmpty(_syncDbHelper.ErrorMessage)
                    ? _syncDbHelper.ErrorMessage
                    : "Database creation failed for an unknown reason.";

                // Show error alert to user
                await _alertService.ShowErrorAlert(
                    "Database Creation Failed",
                    $"Unable to create the database.<br/>Reason: {errorMessage}<br/>Please try again.");

                Console.WriteLine($"Database creation failed: {errorMessage}");
                return false;
            }

            // Verify that we have a valid database path
            if (string.IsNullOrEmpty(_syncDbHelper.SqlitePath))
            {
                isOperationComplete = true; // Stop progress callbacks

                await _alertService.ShowErrorAlert(
                    "Database Creation Error",
                    "Database creation appeared to succeed but no valid database path was returned. Please try again.");

                Console.WriteLine("Database creation succeeded but no valid path was returned");
                return false;
            }

            // Success! Database is ready for use
            isOperationComplete = true; // Stop progress callbacks
            Console.WriteLine($"Database initialization completed successfully: {_syncDbHelper.SqlitePath}");

            // The _syncViewModel.SqlitePath has already been set by SyncDbInit
            // and _syncViewModel.IsValid has been set to true
            return true;
        }
        catch (OperationCanceledException)
        {
            isOperationComplete = true; // Stop progress callbacks

            await _alertService.ShowErrorAlert(
                "Database Creation Timeout",
                "Database creation was cancelled due to timeout (10 minutes). This may be due to a slow internet connection or server issues.<br/>Please try again later.");

            Console.WriteLine("Database creation was cancelled due to timeout");
            _syncViewModel.IsValid = false;
            _syncViewModel.ErrorMessage = "Database creation timed out. Please try again.";

            return false;
        }
        catch (Exception ex)
        {
            isOperationComplete = true; // Stop progress callbacks

            await _alertService.ShowErrorAlert(
                "Unexpected Error",
                $"An unexpected error occurred during database creation: {ex.Message}<br/>Please try again or contact support if the problem persists.");

            Console.WriteLine($"Unexpected error during database creation: {ex}");
            _syncViewModel.IsValid = false;
            _syncViewModel.ErrorMessage = "An unexpected error occurred during database creation. Please try again.";
            return false;
        }
    }



    public async Task GetEmpdataAndBindAppuser()
    {
        await _userConfig.InitiateData(_userID);
        await Task.Run((_loginViewModel as LoginAppViewModel).GenerateFilterData).ConfigureAwait(false);
    }

    private async Task OnForgotPasswordOkClick()
    {
        try
        {
            ShowLoader();
            (bool isSuccessResponse, string msg) = await _loginViewModel.VerifyUserIdAndSendRandomPassword(_userID);

            if (isSuccessResponse)
            {
                await _alertService.ShowSuccessAlert("Success", msg);
                _errorMsg = string.Empty;
                IsForgotPasswordPopUpOpen = false;
            }
            else
            {
                _errorMsg = msg;
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            HideLoader();
        }
    }

    public override async Task OnBackClick()
    {
        if (await _alertService.ShowConfirmationReturnType(QuitConfirmationTitle, QuitConfirmationMessage))
        {
            Application.Current.Quit();
        }
    }

    #region Login Process Methods

    /// <summary>
    /// Processes the complete login workflow with proper state management.
    /// </summary>
    private async Task ProcessLogin()
    {
        IsLoginInProgress = true;
        StateHasChanged();
        ShowLoader("Loading...");

        try
        {
            await OnLoginClick();
        }
        catch (Exception ex)
        {
            await _alertService.ShowErrorAlert(ErrorTitle, UnexpectedErrorMessage);
            Console.WriteLine($"Login error: {ex}");
        }
        finally
        {
            HideLoader();
            IsLoginInProgress = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Deletes the SQLite database with retry mechanism for file locking issues.
    /// </summary>
    private async Task DeleteSQLiteDatabase()
    {
        string sqlLitePath = Path.Combine(_appConfigs.BaseFolderPath, "DB", SqliteDatabaseName);

        try
        {
            await _sqlite.EnsureDatabaseReleasedAsync(sqlLitePath);
            await AttemptDatabaseDeletion(sqlLitePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting SQLite database: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to delete the SQLite database with retry logic.
    /// </summary>
    /// <param name="sqlLitePath">Path to the SQLite database file</param>
    private async Task AttemptDatabaseDeletion(string sqlLitePath)
    {
        int currentRetry = 0;
        bool deleted = false;

        while (currentRetry < MaxRetries && !deleted)
        {
            try
            {
                if (File.Exists(sqlLitePath))
                {
                    await TryDeleteDatabaseFile(sqlLitePath);
                    Console.WriteLine(SqliteDeletedMessage);
                    deleted = true;
                }
                else
                {
                    deleted = true;
                }
            }
            catch (IOException ex)
            {
                currentRetry++;
                Console.WriteLine($"Attempt {currentRetry}: Failed to delete SQLite file: {ex.Message}");

                if (currentRetry >= MaxRetries)
                {
                    await HandleFinalDeletionAttempt(sqlLitePath);
                    deleted = true;
                }
                else
                {
                    await PrepareForRetry(currentRetry);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to delete the database file using file stream approach.
    /// </summary>
    /// <param name="sqlLitePath">Path to the SQLite database file</param>
    private async Task TryDeleteDatabaseFile(string sqlLitePath)
    {
        using (FileStream fs = new FileStream(sqlLitePath, FileMode.Open, FileAccess.Read, FileShare.Delete))
        {
            // Opening with FileShare.Delete helps release locks
        }

        File.Delete(sqlLitePath);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles final deletion attempt by renaming the file if deletion fails.
    /// </summary>
    /// <param name="sqlLitePath">Path to the SQLite database file</param>
    private async Task HandleFinalDeletionAttempt(string sqlLitePath)
    {
        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("All deletion retries failed");

        try
        {
            string tempPath = sqlLitePath + TempFileExtension;

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            File.Move(sqlLitePath, tempPath);
            Console.WriteLine(SqliteRenamedMessage);
        }
        catch (Exception moveEx)
        {
            Console.WriteLine($"Failed to rename SQLite file: {moveEx.Message}");
            throw;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Prepares the system for retry by cleaning up connections and waiting.
    /// </summary>
    /// <param name="currentRetry">Current retry attempt number</param>
    private async Task PrepareForRetry(int currentRetry)
    {
        await Task.Delay(RetryDelayBase * currentRetry);

        _sqlite.CloseConnection();
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    #endregion

    #region Offline Login Methods

    /// <summary>
    /// Attempts to perform offline login using stored credentials.
    /// </summary>
    private async Task AttemptOfflineLogin()
    {
        try
        {
            ShowLoader("Attempting offline authentication...");

            // Validate offline credentials exist and are not expired
            var validationResult = ValidateOfflineCredentials();
            if (!validationResult.IsValid)
            {
                await _alertService.ShowErrorAlert(OfflineLoginErrorTitle, validationResult.ErrorMessage);
                return;
            }

            // Verify provided credentials against stored offline credentials
            if (!await VerifyOfflineCredentials())
            {
                await _alertService.ShowErrorAlert(OfflineLoginErrorTitle, OfflineCredentialsInvalidMessage);
                return;
            }

            // Load offline user data and proceed
            await LoadOfflineUserDataAndProceed();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Offline login error: {ex}");
            await _alertService.ShowErrorAlert(OfflineLoginErrorTitle, "An error occurred during offline login. Please try again.");
        }
    }

    /// <summary>
    /// Validates that offline credentials exist and are not expired.
    /// </summary>
    /// <returns>Validation result with success status and error message</returns>
    private (bool IsValid, string ErrorMessage) ValidateOfflineCredentials()
    {
        // Check if offline credentials exist
        string offlineUsername = _storageHelper.GetStringFromPreferences(LocalStorageKeys.OfflineUsername);
        string offlinePasswordHash = _storageHelper.GetStringFromPreferences(LocalStorageKeys.OfflinePasswordHash);

        if (string.IsNullOrEmpty(offlineUsername) || string.IsNullOrEmpty(offlinePasswordHash))
        {
            return (false, OfflineCredentialsNotFoundMessage);
        }

        // Check if credentials have expired
        DateTime credentialsDate = _storageHelper.GetDateTimeFromPreference(LocalStorageKeys.OfflineCredentialsDate, DateTime.MinValue);
        if (credentialsDate == DateTime.MinValue ||
            DateTime.Now > credentialsDate.AddDays(OfflineCredentialsValidityDays))
        {
            return (false, OfflineCredentialsExpiredMessage);
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Verifies provided credentials against stored offline credentials.
    /// </summary>
    /// <returns>True if credentials match, false otherwise</returns>
    private async Task<bool> VerifyOfflineCredentials()
    {
        try
        {
            // Get stored offline credentials
            string storedUsername = _storageHelper.GetStringFromPreferences(LocalStorageKeys.OfflineUsername);
            string storedPasswordHash = _storageHelper.GetStringFromPreferences(LocalStorageKeys.OfflinePasswordHash);

            // Verify username matches
            if (!string.Equals(_userID.Trim(), storedUsername, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // For offline verification, we need to store a different hash that doesn't depend on challenge code
            // Let's create a simple hash of the original password for offline comparison
            string offlinePasswordHash = _sHACommonFunctions.HashPasswordWithSHA256(_password.Trim());

            // Compare hashes
            return string.Equals(offlinePasswordHash, storedPasswordHash, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying offline credentials: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Stores offline credentials after successful online login.
    /// </summary>
    private async Task StoreOfflineCredentials()
    {
        try
        {
            // Store username
            _storageHelper.SaveStringToPreference(LocalStorageKeys.OfflineUsername, _userID.Trim());

            // Store password hash (using simple hash without challenge for offline verification)
            string offlinePasswordHash = _sHACommonFunctions.HashPasswordWithSHA256(_password.Trim());
            _storageHelper.SaveStringToPreference(LocalStorageKeys.OfflinePasswordHash, offlinePasswordHash);

            // Store credentials creation date
            _storageHelper.SaveDateTimeToPreference(LocalStorageKeys.OfflineCredentialsDate, DateTime.Now);

            Console.WriteLine("Offline credentials stored successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing offline credentials: {ex}");
            // Don't throw - this shouldn't break the login flow
        }
    }

    /// <summary>
    /// Loads offline user data and proceeds with offline login flow.
    /// </summary>
    private async Task LoadOfflineUserDataAndProceed()
    {
        try
        {
            // Retrieve the serializable DTO from secure storage
            var serializableTokenData = await _localStorageService.GetItem<SerializableLoginResponse>(LocalStorageKeys.TokenData);
            if (serializableTokenData == null)
            {
                throw new InvalidOperationException("Failed to load offline user data. Please try online login.");
            }

            // Convert DTO back to interface type
            var tokenData = ConvertFromSerializableLoginResponse(serializableTokenData);

            // Set offline mode flag
            _isOfflineMode = true;

            await SetupUserSession(tokenData);

            // Proceed to database and navigation (should use existing local database)
            await MoveToNextScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading offline user data: {ex}");
            throw new InvalidOperationException("Failed to load offline user data. Please try online login.");
        }
    }

    /// <summary>
    /// Clears all offline credentials from storage.
    /// Call this when user logs out or credentials expire.
    /// </summary>
    private void ClearOfflineCredentials()
    {
        _storageHelper.RemoveKey(LocalStorageKeys.OfflineUsername);
        _storageHelper.RemoveKey(LocalStorageKeys.OfflinePasswordHash);
        _storageHelper.RemoveKey(LocalStorageKeys.OfflineCredentialsDate);
        _storageHelper.RemoveKey(LocalStorageKeys.OfflineUserData);
        Console.WriteLine("Offline credentials cleared");
    }

    /// <summary>
    /// Converts ILoginResponse interface to serializable DTO for storage.
    /// </summary>
    /// <param name="loginResponse">Login response interface</param>
    /// <returns>Serializable SerializableLoginResponse object</returns>
    private SerializableLoginResponse ConvertToSerializableLoginResponse(ILoginResponse loginResponse)
    {
        return new SerializableLoginResponse
        {
            StatusCode = loginResponse.StatusCode,
            ErrorMessage = loginResponse.ErrorMessage,
            Token = loginResponse.Token,
            AuthMaster = loginResponse.AuthMaster != null ? ConvertToSerializableAuthMaster(loginResponse.AuthMaster) : null
        };
    }

    /// <summary>
    /// Converts IAuthMaster interface to serializable DTO.
    /// </summary>
    /// <param name="authMaster">Auth master interface</param>
    /// <returns>Serializable SerializableAuthMaster object</returns>
    private SerializableAuthMaster ConvertToSerializableAuthMaster(IAuthMaster authMaster)
    {
        return new SerializableAuthMaster
        {
            Emp = authMaster.Emp as Winit.Modules.Emp.Model.Classes.Emp,
            JobPosition = authMaster.JobPosition as Winit.Modules.JobPosition.Model.Classes.JobPosition,
            Role = authMaster.Role as Winit.Modules.Role.Model.Classes.Role,
            VehicleStatuses = authMaster.VehicleStatuses?.Cast<Winit.Modules.Vehicle.Model.Classes.VehicleStatus>().ToList()
        };
    }

    /// <summary>
    /// Converts serializable DTO back to ILoginResponse interface.
    /// </summary>
    /// <param name="dto">Serializable SerializableLoginResponse</param>
    /// <returns>LoginResponse implementing ILoginResponse</returns>
    private ILoginResponse ConvertFromSerializableLoginResponse(SerializableLoginResponse dto)
    {
        return new LoginResponse
        {
            StatusCode = dto.StatusCode,
            ErrorMessage = dto.ErrorMessage,
            Token = dto.Token,
            AuthMaster = dto.AuthMaster != null ? ConvertFromSerializableAuthMaster(dto.AuthMaster) : null
        };
    }

    /// <summary>
    /// Converts serializable DTO back to IAuthMaster interface.
    /// </summary>
    /// <param name="dto">Serializable SerializableAuthMaster</param>
    /// <returns>AuthMaster implementing IAuthMaster</returns>
    private IAuthMaster ConvertFromSerializableAuthMaster(SerializableAuthMaster dto)
    {
        return new AuthMaster
        {
            Emp = dto.Emp,
            JobPosition = dto.JobPosition,
            Role = dto.Role,
            VehicleStatuses = dto.VehicleStatuses?.Cast<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>().ToList()
        };
    }

    #endregion

    /// <summary>
    /// Closes the application.
    /// </summary>
    private void CloseApp()
    {
        Application.Current.Quit();
    }

    /// <summary>
    /// Handles ClearData actions based on server configuration
    /// </summary>
    /// <param name="tokenData">Authentication token data</param>
    private async Task HandleClearDataAction(ILoginResponse tokenData)
    {
        try
        {
            ShowLoader("Checking cleardata actions");

            bool isDatabaseAvailable = await _dataSyncValidationService.IsDatabaseAvailableAsync();
            if (!isDatabaseAvailable)
            {
                // If database available then only check cleardata permission else ignore
                return;
            }

            string empUID = tokenData.AuthMaster?.Emp?.UID;

            if (string.IsNullOrEmpty(empUID))
            {
                // If no UID, proceed with normal flow
                return;
            }

            // Get determination of whether database should be deleted
            var result = await _clearDataService.ShouldDeleteDatabaseAsync(empUID, async () =>
            {
                ShowLoader("Uploading data");
                // Use the dedicated method for current user data upload
                await HandleCurrentUserDataUpload(
                    baseMessage: "Uploading data for clear data action",
                    showAlerts: false  // Don't show alerts in clear data flow
                );
            });

            // Handle result and execute database deletion if needed
            if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                await _alertService.ShowErrorAlert("ClearData Action", result.ErrorMessage);
                return;
            }

            // Simple check: if service says delete database, then delete it
            if (result.ShouldDeleteDatabase)
            {
                await DeleteSQLiteDatabase();
            }
        }
        catch (Exception ex)
        {
            // Log exception and continue with normal flow
            Console.WriteLine($"ClearData action error: {ex}");
        }
    }
}
