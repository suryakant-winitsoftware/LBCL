using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Winit.Modules.Base.Model.Constants;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Constants;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
using Winit.UIComponents.Common.Services;
using WINITMobile.State;
using Winit.Modules.Base.BL;
using Winit.Shared.CommonUtilities;
using WINITMobile.Services;

namespace WINITMobile.Pages.Base;

/// <summary>
/// Context containing user-specific information required for data upload operations.
/// </summary>
public class UploadContext
{
    public string OrgUID { get; set; }
    public string EmpUID { get; set; }
    public string JobPositionUID { get; set; }

    /// <summary>
    /// Creates an UploadContext from the current app user.
    /// </summary>
    /// <param name="appUser">Current application user</param>
    /// <returns>UploadContext with current user's information</returns>
    public static UploadContext FromAppUser(IAppUser appUser)
    {
        return new UploadContext
        {
            OrgUID = appUser?.SelectedJobPosition?.OrgUID,
            EmpUID = appUser?.Emp?.UID,
            JobPositionUID = appUser?.SelectedJobPosition?.UID
        };
    }

    /// <summary>
    /// Creates an UploadContext with explicit parameters.
    /// </summary>
    /// <param name="orgUID">Organization UID</param>
    /// <param name="empUID">Employee UID</param>
    /// <param name="jobPositionUID">Job Position UID</param>
    /// <returns>UploadContext with specified parameters</returns>
    public static UploadContext Create(string orgUID, string empUID, string jobPositionUID)
    {
        return new UploadContext
        {
            OrgUID = orgUID,
            EmpUID = empUID,
            JobPositionUID = jobPositionUID
        };
    }

    /// <summary>
    /// Validates that all required fields are present.
    /// </summary>
    /// <returns>True if context is valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(OrgUID) &&
               !string.IsNullOrEmpty(EmpUID) &&
               !string.IsNullOrEmpty(JobPositionUID);
    }

    /// <summary>
    /// Gets validation error message if context is invalid.
    /// </summary>
    /// <returns>Error message describing what's missing</returns>
    public string GetValidationError()
    {
        var missingFields = new List<string>();

        if (string.IsNullOrEmpty(OrgUID)) missingFields.Add("OrgUID");
        if (string.IsNullOrEmpty(EmpUID)) missingFields.Add("EmpUID");
        if (string.IsNullOrEmpty(JobPositionUID)) missingFields.Add("JobPositionUID");

        return missingFields.Any()
            ? $"Missing required fields: {string.Join(", ", missingFields)}"
            : string.Empty;
    }
}

/// <summary>
/// Options for controlling upload behavior
/// </summary>
public class UploadOptions
{
    public bool ShowLoading { get; set; } = false;
    public string LoadingMessage { get; set; } = "Data uploading";
    public bool ShowSuccessAlert { get; set; } = false;
    public bool ShowErrorAlert { get; set; } = false;
    public bool RunInBackground { get; set; } = false;

    /// <summary>
    /// Callback to report progress during upload operation.
    /// Parameters: (currentTableGroup, currentIndex, totalCount)
    /// </summary>
    public Action<string, int, int> OnProgress { get; set; }
}

/// <summary>
/// Result of upload operation
/// </summary>
public class UploadResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> UploadedTableGroups { get; set; } = new List<string>();
    public List<string> FailedTableGroups { get; set; } = new List<string>();
    public Dictionary<string, string> TableGroupErrors { get; set; } = new Dictionary<string, string>();
    public Exception Exception { get; set; }

    /// <summary>
    /// True if all table groups were uploaded successfully
    /// </summary>
    public bool IsCompleteSuccess => Success && !FailedTableGroups.Any();

    /// <summary>
    /// True if some table groups succeeded and some failed
    /// </summary>
    public bool IsPartialSuccess => UploadedTableGroups.Any() && FailedTableGroups.Any();

    /// <summary>
    /// True if no table groups were uploaded successfully
    /// </summary>
    public bool IsCompleteFailure => !UploadedTableGroups.Any() && FailedTableGroups.Any();
}

/// <summary>
/// Base component class that provides comprehensive network error handling and loading management.
/// All pages in the mobile app should inherit from this class to get automatic network error handling.
/// 
/// NETWORK ERROR HANDLING FEATURES:
/// ================================
/// 
/// 1. MANUAL vs AUTOMATIC LOADER CONTROL:
///    - ShowLoader(message) - Marks loader as MANUALLY controlled (protected from auto-hide)
///    - HideLoader() - Clears manual control and hides loader
///    - Automatic loaders (from NetworkErrorHandler) - Only work when NO manual control
///    - Manual loaders are PROTECTED from network error auto-hide
/// 
/// 2. SAFE LOADER MANAGEMENT:
///    - ForceHideLoader() - Only hides if not manually controlled
///    - IsLoaderManuallyControlled() - Check if loader is under manual control
///    - ShowNetworkErrorSafe() - Show error without affecting manual loaders
/// 
/// 3. USAGE EXAMPLES:
/// 
///    PROTECTED MANUAL LOADER (Login/Important Operations):
///    ===================================================
///    ShowLoader("Logging in..."); // Manual control - PROTECTED
///    try 
///    {
///        // Multiple API calls during login
///        var authResult = await _apiService.FetchDataAsync<AuthData>("api/auth");
///        var deviceResult = await _apiService.FetchDataAsync<DeviceData>("api/device");
///        // Even if network errors occur, loader stays visible
///        // Process results...
///    }
///    finally 
///    {
///        HideLoader(); // Only YOU control when to hide
///    }
///    
///    AUTOMATIC LOADER (Simple Data Fetch):
///    ====================================
///    // No manual ShowLoader() call
///    try 
///    {
///        var result = await _apiService.FetchDataAsync<SomeData>("api/endpoint");
///        // NetworkErrorHandler auto-manages loader
///        // On error: auto-hides loader and shows alert
///    }
///    catch (Exception ex)
///    {
///        // Handle if needed, loader already hidden
///    }
///    
///    SAFE NETWORK ERROR (During Manual Operations):
///    ==============================================
///    ShowLoader("Processing...");
///    try 
///    {
///        var result = await _apiService.FetchDataAsync<Data>("api/endpoint");
///    }
///    catch (NetworkException ex)
///    {
///        // Show error but keep loader (we'll hide it manually)
///        await ShowNetworkErrorSafe("Error", ex.Message, hideLoaderOnError: false);
///        // Continue processing or hide loader manually
///    }
///    finally 
///    {
///        HideLoader(); // Manual control
///    }
/// 
/// 4. AUTOMATIC FEATURES:
///    - Network errors show appropriate alerts but respect manual loader control
///    - All API calls through _apiService are automatically monitored
///    - Automatic loader management only when NO manual control active
///    - User-friendly error messages with icons based on error type
/// 
/// 5. LOADER STATE METHODS:
///    - IsLoaderVisible() - Check if loader is currently showing
///    - IsLoaderManuallyControlled() - Check if under manual control
///    - ForceHideLoader() - Emergency hide (only if not manual)
/// </summary>
public class BaseComponentBase : ComponentBase, ICurrentPageHandler, IDisposable
{
    [Inject] protected NavigationManager _navigationManager { get; set; }
    [Inject] protected WINITMobile.State.NavigationService navigationService { get; set; }
    [Inject] protected IDataManager _dataManager { get; set; }
    [Inject] protected Winit.UIComponents.Common.Services.ILoadingService _loadingService { get; set; }
    [Inject] protected WINITMobile.Data.SideBarService _sideBarService { get; set; }
    [Inject] protected Winit.UIComponents.Common.IAlertService _alertService { get; set; }
    [Inject] protected Winit.UIComponents.Common.Services.IDropDownService _dropdownService { get; set; }
    [Inject]
    protected AuthenticationStateProvider? _authStateProvider { get; set; }
    [Inject]
    protected Winit.Modules.Base.BL.ILocalStorageService? _localStorageService { get; set; }
    [Inject] protected Winit.Modules.Common.BL.Interfaces.IAppUser _appUser { get; set; }
    [Inject] protected WINITMobile.State.IBackButtonHandler _backbuttonhandler { get; set; }
    [Inject] protected IStringLocalizer<LanguageKeys>? Localizer { get; set; }
    [Inject] protected Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL _mobileDataSyncBL { get; set; }
    [Inject] protected Winit.UIComponents.Common.Language.ILanguageService _languageService { get; set; }
    [Inject] protected NetworkErrorHandler _networkErrorHandler { get; set; }
    [Inject] protected ApiService _apiService { get; set; }
    [Inject] protected NetworkConnectivityService _networkConnectivityService { get; set; }

    protected bool HasValidationErrors = false;
    protected List<string> ValidationErrors = new List<string>();
    private bool _isNetworkErrorSubscribed = false;
    private bool _isManualLoaderActive = false; // Track if loader was manually shown

    protected override async Task OnInitializedAsync()
    {
        // Initialize NetworkErrorHandler with ApiService
        if (_networkErrorHandler != null && _apiService != null)
        {
            _networkErrorHandler.Initialize(_apiService);
            SubscribeToNetworkErrors();
        }
        
        await base.OnInitializedAsync();
    }

    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    public void Navigate(string navigationURL, string queryString, object data, bool forceLoad = false)
    {
        if (data != null)
        {
            _dataManager.SetData(navigationURL, data);
        }
        _navigationManager.NavigateTo($"{navigationURL}{queryString}", forceLoad);
    }

    public void NavigateTo(string uri)
    {
        navigationService.NavigateTo(_navigationManager, uri, this);
    }

    public async Task NavigateBack(int NoOfBackSteps)
    {
        await navigationService.NavigateBack(NoOfBackSteps);
    }
    public void ClearValidationError()
    {
        HasValidationErrors = false;
        ValidationErrors.Clear();
    }

    /// <summary>
    /// Shows loader with network error handling integration.
    /// </summary>
    /// <param name="message">Loading message to display</param>
    public void ShowLoader(string message = "")
    {
        _isManualLoaderActive = true; // Mark as manually controlled
        InvokeAsync(() => _loadingService.ShowLoading(message));
    }

    /// <summary>
    /// Hides loader with network error handling integration.
    /// </summary>
    public void HideLoader()
    {
        _isManualLoaderActive = false; // Clear manual control flag
        InvokeAsync(() => _loadingService.HideLoading());
    }

    /// <summary>
    /// Force hides loader in emergency situations (network errors, etc.)
    /// Only works if loader is not manually controlled.
    /// </summary>
    public void ForceHideLoader()
    {
        if (!_isManualLoaderActive) // Only hide if not manually controlled
        {
            InvokeAsync(() => 
            {
                _loadingService.HideLoading();
                _networkErrorHandler?.ForceHideLoader();
            });
        }
    }

    /// <summary>
    /// Subscribes to network error events from NetworkErrorHandler.
    /// </summary>
    private void SubscribeToNetworkErrors()
    {
        if (_isNetworkErrorSubscribed || _networkErrorHandler == null) return;

        _networkErrorHandler.OnLoaderStateChanged += HandleLoaderStateChanged;
        _networkErrorHandler.OnShowAlert += HandleShowAlert;
        _isNetworkErrorSubscribed = true;
    }

    /// <summary>
    /// Unsubscribes from network error events.
    /// </summary>
    private void UnsubscribeFromNetworkErrors()
    {
        if (!_isNetworkErrorSubscribed || _networkErrorHandler == null) return;

        _networkErrorHandler.OnLoaderStateChanged -= HandleLoaderStateChanged;
        _networkErrorHandler.OnShowAlert -= HandleShowAlert;
        _isNetworkErrorSubscribed = false;
    }

    /// <summary>
    /// Handles loader state changes from NetworkErrorHandler.
    /// Respects manual loader control and doesn't interfere with manually shown loaders.
    /// </summary>
    /// <param name="shouldShow">Whether loader should be shown</param>
    private async void HandleLoaderStateChanged(bool shouldShow)
    {
        await InvokeAsync(() =>
        {
            // Only auto-manage loader if not manually controlled
            if (!_isManualLoaderActive)
            {
                if (shouldShow)
                {
                    _loadingService.ShowLoading();
                }
                else
                {
                    _loadingService.HideLoading();
                }
                StateHasChanged();
            }
            // If manually controlled, let the manual control handle the loader
        });
    }

    /// <summary>
    /// Handles show alert events from NetworkErrorHandler.
    /// </summary>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message</param>
    private async void HandleShowAlert(string title, string message)
    {
        await InvokeAsync(async () =>
        {
            await _alertService.ShowErrorAlert(title, message);
            StateHasChanged();
        });
    }

    /// <summary>
    /// Shows custom network error alert.
    /// </summary>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message</param>
    public async Task ShowNetworkError(string title, string message)
    {
        if (_networkErrorHandler != null)
        {
            await _networkErrorHandler.ShowAlertAsync(title, message);
        }
        else
        {
            await _alertService.ShowErrorAlert(title, message);
        }
    }

    /// <summary>
    /// Shows network confirmation dialog.
    /// </summary>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Dialog message</param>
    /// <returns>True if confirmed, false otherwise</returns>
    public async Task<bool> ShowNetworkConfirmation(string title, string message)
    {
        if (_networkErrorHandler != null)
        {
            return await _networkErrorHandler.ShowConfirmationAsync(title, message);
        }
        else
        {
            // Fallback to alert service (no direct confirmation method)
            await _alertService.ShowErrorAlert(title, message);
            return true;
        }
    }

    /// <summary>
    /// Checks if network error handler is properly initialized.
    /// </summary>
    /// <returns>True if network handling is available</returns>
    public bool IsNetworkHandlingAvailable()
    {
        return _networkErrorHandler != null && _apiService != null;
    }

    /// <summary>
    /// Checks if loader is currently visible.
    /// </summary>
    /// <returns>True if loader is visible</returns>
    public bool IsLoaderVisible()
    {
        return _networkErrorHandler?.IsLoaderVisible() ?? false;
    }

    /// <summary>
    /// Checks if loader is currently under manual control.
    /// </summary>
    /// <returns>True if loader is manually controlled</returns>
    public bool IsLoaderManuallyControlled()
    {
        return _isManualLoaderActive;
    }

    /// <summary>
    /// Shows network error without interfering with manual loader control.
    /// </summary>
    /// <param name="title">Alert title</param>
    /// <param name="message">Alert message</param>
    /// <param name="hideLoaderOnError">Whether to hide loader on error (only if not manually controlled)</param>
    public async Task ShowNetworkErrorSafe(string title, string message, bool hideLoaderOnError = true)
    {
        // Show the error alert
        await ShowNetworkError(title, message);
        
        // Only hide loader if not manually controlled and requested
        if (hideLoaderOnError && !_isManualLoaderActive)
        {
            ForceHideLoader();
        }
    }

    /// <summary>
    /// Core method to upload data from SQLite to server for the specified table groups.
    /// This method handles only the business logic without any UI concerns.
    /// Continues processing all table groups even if some fail.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="uploadContext">Context containing user-specific information for upload</param>
    /// <param name="onProgress">Optional progress callback to report current table group being processed</param>
    /// <returns>UploadResult containing success status and detailed results for each table group</returns>
    protected async Task<UploadResult> UploadDataToServerCore(List<string> dbTableGroups, UploadContext uploadContext, Action<string, int, int> onProgress = null)
    {
        var result = new UploadResult();

        if (dbTableGroups == null || !dbTableGroups.Any())
        {
            result.Success = false;
            result.ErrorMessage = "No table groups specified for upload.";
            return result;
        }

        if (!uploadContext.IsValid())
        {
            result.Success = false;
            result.ErrorMessage = $"Invalid upload context: {uploadContext.GetValidationError()}";
            return result;
        }

        var allErrors = new List<string>();
        int totalCount = dbTableGroups.Count;

        for (int i = 0; i < dbTableGroups.Count; i++)
        {
            string tableGroup = dbTableGroups[i];

            // Report progress
            onProgress?.Invoke(tableGroup, i + 1, totalCount);

            try
            {
                await _mobileDataSyncBL.SyncDataFromSQLiteToServer(
                    uploadContext.OrgUID,
                    tableGroup,
                    uploadContext.EmpUID,
                    uploadContext.JobPositionUID);

                result.UploadedTableGroups.Add(tableGroup);
            }
            catch (Exception ex)
            {
                result.FailedTableGroups.Add(tableGroup);
                result.TableGroupErrors[tableGroup] = ex.Message;
                allErrors.Add($"{tableGroup}: {ex.Message}");

                // Store the first exception for backward compatibility
                if (result.Exception == null)
                {
                    result.Exception = ex;
                }
            }
        }

        // Determine overall success status
        result.Success = result.UploadedTableGroups.Any();

        // Build comprehensive error message
        if (result.FailedTableGroups.Any())
        {
            if (result.UploadedTableGroups.Any())
            {
                result.ErrorMessage = $"Partial upload completed. {result.UploadedTableGroups.Count} succeeded, {result.FailedTableGroups.Count} failed. Failures: {string.Join("; ", allErrors)}";
            }
            else
            {
                result.ErrorMessage = $"All uploads failed. Errors: {string.Join("; ", allErrors)}";
            }
        }

        return result;
    }

    /// <summary>
    /// Flexible upload method with options to control UI behavior.
    /// Uses current app user context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="options">Options controlling UI behavior</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataToServer(List<string> dbTableGroups, UploadOptions options = null)
    {
        return await UploadDataToServer(dbTableGroups, UploadContext.FromAppUser(_appUser), options);
    }

    /// <summary>
    /// Flexible upload method with options to control UI behavior.
    /// Uses specified upload context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="uploadContext">Context containing user-specific information for upload</param>
    /// <param name="options">Options controlling UI behavior</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataToServer(List<string> dbTableGroups, UploadContext uploadContext, UploadOptions options = null)
    {
        options ??= new UploadOptions();
        UploadResult result;

        // Show loading if requested
        if (options.ShowLoading)
        {
            ShowLoader(options.LoadingMessage);
        }

        try
        {
            if (options.RunInBackground)
            {
                result = await Task.Run(async () => await UploadDataToServerCore(dbTableGroups, uploadContext, options.OnProgress));
            }
            else
            {
                result = await UploadDataToServerCore(dbTableGroups, uploadContext, options.OnProgress);
            }

            // Handle different success scenarios
            if (options.ShowSuccessAlert)
            {
                if (result.IsCompleteSuccess)
                {
                    await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["upload_completed_successfully"]);
                }
                else if (result.IsPartialSuccess)
                {
                    await _alertService.ShowSuccessAlert(@Localizer["partial_success"],
                        $"Partial upload completed: {result.UploadedTableGroups.Count} succeeded, {result.FailedTableGroups.Count} failed.");
                }
                // Note: Complete failure will be handled by error alert below
            }

            // Show error alert if requested and there were failures
            if (options.ShowErrorAlert && (result.IsCompleteFailure || result.IsPartialSuccess))
            {
                string errorTitle = result.IsCompleteFailure ? @Localizer["error"] : @Localizer["partial_failure"];
                await _alertService.ShowErrorAlert(errorTitle, result.ErrorMessage);
            }
        }
        finally
        {
            // Hide loading if it was shown
            if (options.ShowLoading)
            {
                _loadingService.HideLoading();
            }
        }

        return result;
    }

    /// <summary>
    /// Convenience method for silent upload (no UI interactions).
    /// Uses current app user context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataSilent(List<string> dbTableGroups)
    {
        return await UploadDataToServer(dbTableGroups, UploadContext.FromAppUser(_appUser), new UploadOptions());
    }

    /// <summary>
    /// Convenience method for silent upload (no UI interactions).
    /// Uses specified upload context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="uploadContext">Context containing user-specific information for upload</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataSilent(List<string> dbTableGroups, UploadContext uploadContext)
    {
        return await UploadDataToServer(dbTableGroups, uploadContext, new UploadOptions());
    }

    /// <summary>
    /// Convenience method for background upload with full UI feedback.
    /// Uses current app user context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="loadingMessage">Custom loading message</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataWithUI(List<string> dbTableGroups, string loadingMessage = "Data uploading")
    {
        return await UploadDataToServer(dbTableGroups, UploadContext.FromAppUser(_appUser), new UploadOptions
        {
            ShowLoading = true,
            LoadingMessage = loadingMessage,
            ShowSuccessAlert = true,
            ShowErrorAlert = true,
            RunInBackground = true
        });
    }

    /// <summary>
    /// Convenience method for background upload with full UI feedback.
    /// Uses specified upload context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="uploadContext">Context containing user-specific information for upload</param>
    /// <param name="loadingMessage">Custom loading message</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataWithUI(List<string> dbTableGroups, UploadContext uploadContext, string loadingMessage = "Data uploading")
    {
        return await UploadDataToServer(dbTableGroups, uploadContext, new UploadOptions
        {
            ShowLoading = true,
            LoadingMessage = loadingMessage,
            ShowSuccessAlert = true,
            ShowErrorAlert = true,
            RunInBackground = true
        });
    }

    /// <summary>
    /// Convenience method for background upload without UI alerts (loading only).
    /// Uses current app user context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="loadingMessage">Custom loading message</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataWithLoadingOnly(List<string> dbTableGroups, string loadingMessage = "Data uploading")
    {
        return await UploadDataToServer(dbTableGroups, UploadContext.FromAppUser(_appUser), new UploadOptions
        {
            ShowLoading = true,
            LoadingMessage = loadingMessage,
            ShowSuccessAlert = false,
            ShowErrorAlert = false,
            RunInBackground = true
        });
    }

    /// <summary>
    /// Convenience method for background upload without UI alerts (loading only).
    /// Uses specified upload context.
    /// </summary>
    /// <param name="dbTableGroups">List of database table group names to upload</param>
    /// <param name="uploadContext">Context containing user-specific information for upload</param>
    /// <param name="loadingMessage">Custom loading message</param>
    /// <returns>UploadResult containing success status and details</returns>
    protected async Task<UploadResult> UploadDataWithLoadingOnly(List<string> dbTableGroups, UploadContext uploadContext, string loadingMessage = "Data uploading")
    {
        return await UploadDataToServer(dbTableGroups, uploadContext, new UploadOptions
        {
            ShowLoading = true,
            LoadingMessage = loadingMessage,
            ShowSuccessAlert = false,
            ShowErrorAlert = false,
            RunInBackground = true
        });
    }

    /// <summary>
    /// Handles uploading user data with proper loader management and progress reporting.
    /// Generic method that can be used for any user context across all pages.
    /// </summary>
    /// <param name="uploadContext">User context for the upload (mandatory)</param>
    /// <param name="baseMessage">Base message to show in loader (e.g., "Uploading user data", "Syncing data")</param>
    /// <param name="tableGroups">Specific table groups to upload. If null, uses default set</param>
    /// <param name="showAlerts">Whether to show success/error alerts to user</param>
    /// <returns>UploadResult containing the result of the upload operation</returns>
    protected async Task<UploadResult> HandleUserDataUpload(
        UploadContext uploadContext,
        string baseMessage = "Uploading data",
        List<string> tableGroups = null,
        bool showAlerts = true)
    {
        // Validate mandatory context
        if (uploadContext == null || !uploadContext.IsValid())
        {
            var errorResult = new UploadResult
            {
                Success = false,
                ErrorMessage = uploadContext == null
                    ? "Upload context is required and cannot be null."
                    : $"Invalid upload context: {uploadContext.GetValidationError()}",
                FailedTableGroups = tableGroups ?? new List<string>()
            };

            if (showAlerts)
            {
                await _alertService.ShowErrorAlert("Invalid Context", errorResult.ErrorMessage);
            }

            return errorResult;
        }

        // Use default table groups if none provided
        tableGroups ??= new List<string>
        {
            DbTableGroup.Master,
            //DbTableGroup.SurveyResponse,
            DbTableGroup.FileSys,
            DbTableGroup.Merchandiser,
            DbTableGroup.Sales

        };

        UploadResult result = null;

        try
        {
            // Show initial loader
            ShowLoader($"{baseMessage}...");

            // Create progress callback to update loader message
            Action<string, int, int> progressCallback = (tableGroup, current, total) =>
            {
                string progressMessage = $"{baseMessage}... ({current}/{total}) {tableGroup}";

                // Update loader message on UI thread
                ShowLoader(progressMessage);
            };

            // Perform upload with progress reporting
            Console.WriteLine($"Uploading data for user: EmpUID={uploadContext.EmpUID}, OrgUID={uploadContext.OrgUID}");

            var options = new UploadOptions
            {
                ShowLoading = false,        // We handle loading externally
                ShowSuccessAlert = false,   // We handle alerts externally
                ShowErrorAlert = false,     // We handle alerts externally
                RunInBackground = true,
                OnProgress = progressCallback
            };

            result = await UploadDataToServer(tableGroups, uploadContext, options);

            // Handle results and show appropriate messages
            if (showAlerts)
            {
                await HandleUploadResultAlerts(result, baseMessage);
            }
        }
        catch (Exception ex)
        {
            if (showAlerts)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], "Sync failed. Please try again.");
            }
            Console.WriteLine($"Upload data error: {ex}");

            result = new UploadResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Exception = ex,
                FailedTableGroups = tableGroups
            };
        }
        finally
        {
            // HideLoader();
            // Caller should handle hide loader
        }

        return result;
    }

    /// <summary>
    /// Handles displaying alerts based on upload results.
    /// </summary>
    /// <param name="result">Upload result to process</param>
    /// <param name="baseMessage">Base message for context</param>
    protected async Task HandleUploadResultAlerts(UploadResult result, string baseMessage)
    {
        if (result.IsCompleteSuccess)
        {
            /*
            await _alertService.ShowSuccessAlert("Upload Successful",
                $"Successfully uploaded all data: {string.Join(", ", result.UploadedTableGroups)}");
            */
            await _alertService.ShowSuccessAlert("Upload Successful",
                $"Successfully uploaded all data.");
        }
        else if (result.IsPartialSuccess)
        {
            await _alertService.ShowErrorAlert("Partial Upload",
                $"Some data uploaded successfully. Failed to upload: {string.Join(", ", result.FailedTableGroups)}. Error: {result.ErrorMessage}");
        }
        else
        {
            await _alertService.ShowErrorAlert("Upload Failed",
                $"Failed to upload data: {result.ErrorMessage}");
        }
    }
    /// <summary>
    /// Handles uploading old user data during user change validation.
    /// Explicitly determines old user context and calls the base method.
    /// </summary>
    public async Task HandleOldUserDataUpload()
    {
        // Explicitly get old user context from preferences
        var oldUserContext = await TryGetUploadContextFromPreferences(
            LocalStorageKeys.LastOrgUID,
            LocalStorageKeys.LastEmpUID,
            LocalStorageKeys.LastJobPositionUID);

        // Validate that we have a valid old user context
        if (oldUserContext == null || !oldUserContext.IsValid())
        {
            await _alertService.ShowErrorAlert("Context Error",
                "Unable to find old user context in preferences. Cannot upload old user data.");
            return;
        }

        // Call the base method with explicit context
        await HandleUserDataUpload(
            uploadContext: oldUserContext,
            baseMessage: "Uploading old user data",
            tableGroups: null, // Use default table groups
            showAlerts: true
        );
    }

    /// <summary>
    /// Handles uploading current user data for various scenarios like clear data action.
    /// Explicitly determines current user context and calls the base method.
    /// </summary>
    /// <param name="baseMessage">Base message for the upload operation</param>
    /// <param name="showAlerts">Whether to show alerts to the user</param>
    /// <returns>UploadResult containing the result of the upload operation</returns>
    public async Task<UploadResult> HandleCurrentUserDataUpload(string baseMessage = "Uploading current user data", bool showAlerts = false)
    {
        // Explicitly get current user context
        var currentUserContext = UploadContext.FromAppUser(_appUser);

        // Validate that we have a valid current user context
        if (currentUserContext == null || !currentUserContext.IsValid())
        {
            string errorMessage = "Invalid current user context for data upload.";

            if (showAlerts)
            {
                HideLoader();
                await _alertService.ShowErrorAlert("Context Error", errorMessage);
            }
            else
            {
                Console.WriteLine($"Warning: {errorMessage}");
            }

            return new UploadResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                FailedTableGroups = null
            };
        }

        // Call the base method with explicit context
        return await HandleUserDataUpload(
            uploadContext: currentUserContext,
            baseMessage: baseMessage,
            tableGroups: null, // Use default table groups
            showAlerts: showAlerts
        );
    }

    #region Upload Usage Examples
    /* 
    USAGE EXAMPLES WITH MANDATORY CONTEXT:

    1. CURRENT USER UPLOAD (Settings Page):
    ========================================
    private async Task OnUploadClick()
    {
        var tableGroups = new List<string> { DbTableGroup.Master, DbTableGroup.Sales };
        
        // Explicitly get current user context
        var currentUserContext = UploadContext.FromAppUser(_appUser);
        
        // Validate before proceeding
        if (currentUserContext?.IsValid() == true)
        {
            await HandleUserDataUpload(
                uploadContext: currentUserContext,
                baseMessage: "Uploading current user data",
                tableGroups: tableGroups,
                showAlerts: true
            );
        }
        else
        {
            await _alertService.ShowErrorAlert("Error", "Invalid user context");
        }
    }

    2. OLD USER UPLOAD (Login Scenario):
    ====================================
    private async Task UploadOldUserData()
    {
        // Explicitly get old user context from preferences
        var oldUserContext = await TryGetUploadContextFromPreferences(
            "LastOrgUID", "LastEmpUID", "LastJobPositionUID");
            
        if (oldUserContext?.IsValid() == true)
        {
            await HandleUserDataUpload(
                uploadContext: oldUserContext,
                baseMessage: "Uploading old user data",
                tableGroups: null,  // Use default
                showAlerts: true
            );
        }
        else
        {
            await _alertService.ShowErrorAlert("Error", "No valid old user context found");
        }
    }

    3. SPECIFIC USER UPLOAD (Admin Scenario):
    =========================================
    private async Task UploadForSpecificUser(string orgUID, string empUID, string jobPositionUID)
    {
        // Explicitly create context for specific user
        var specificUserContext = UploadContext.Create(orgUID, empUID, jobPositionUID);
        
        if (specificUserContext.IsValid())
        {
            await HandleUserDataUpload(
                uploadContext: specificUserContext,
                baseMessage: "Uploading data for specific user",
                tableGroups: new List<string> { DbTableGroup.Master },
                showAlerts: false  // Silent operation
            );
        }
    }

    4. BACKGROUND SYNC (Silent):
    =============================
    private async Task<UploadResult> BackgroundSync()
    {
        var currentUserContext = UploadContext.FromAppUser(_appUser);
        
        if (currentUserContext?.IsValid() == true)
        {
            return await HandleUserDataUpload(
                uploadContext: currentUserContext,
                baseMessage: "Background sync",
                tableGroups: null,
                showAlerts: false  // Silent operation
            );
        }
        
        return new UploadResult { Success = false, ErrorMessage = "Invalid context" };
    }

    5. CHECKOUT FLOW (No Success Alerts):
    =====================================
    private async Task CheckoutUpload()
    {
        var currentUserContext = UploadContext.FromAppUser(_appUser);
        
        var result = await HandleUserDataUpload(
            uploadContext: currentUserContext,
            baseMessage: "Uploading data during checkout",
            tableGroups: new List<string> { DbTableGroup.Sales, DbTableGroup.FileSys },
            showAlerts: false  // Handle alerts manually
        );
        
        // Custom error handling for checkout context
        if (!result.Success)
        {
            await ShowCheckoutErrorDialog(result.ErrorMessage);
        }
    }

    6. MANUAL CONTEXT VALIDATION:
    ==============================
    private async Task SafeUpload()
    {
        var context = UploadContext.FromAppUser(_appUser);
        
        // Explicit validation
        if (context == null)
        {
            await _alertService.ShowErrorAlert("Error", "Context is null");
            return;
        }
        
        if (!context.IsValid())
        {
            await _alertService.ShowErrorAlert("Error", context.GetValidationError());
            return;
        }
        
        await HandleUserDataUpload(
            uploadContext: context,
            baseMessage: "Safe upload with validation",
            tableGroups: null,
            showAlerts: true
        );
    }
    */
    #endregion

    //Test__Niranjan
    // Virtual method to handle back button click
    public virtual Task OnBackClick()
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose() 
    {
        // Cleanup NetworkErrorHandler
        if (_networkErrorHandler != null && _apiService != null)
        {
            _networkErrorHandler.Cleanup(_apiService);
        }
        
        UnsubscribeFromNetworkErrors();
        UnSubscribeEvent();
    }

    public virtual void UnSubscribeEvent()
    {

    }

    public void CleanDataManager(params string[] keys)
    {
        foreach (string key in keys)
        {
            _dataManager.DeleteData(key);
        }
    }

    /// <summary>
    /// Helper method to safely retrieve upload context from preferences.
    /// Returns null if any required preference is missing.
    /// </summary>
    /// <param name="orgUIDKey">Key for organization UID in preferences</param>
    /// <param name="empUIDKey">Key for employee UID in preferences</param>
    /// <param name="jobPositionUIDKey">Key for job position UID in preferences</param>
    /// <returns>UploadContext if all preferences exist, null otherwise</returns>
    protected async Task<UploadContext> TryGetUploadContextFromPreferences(string orgUIDKey, string empUIDKey, string jobPositionUIDKey)
    {
        try
        {
            string orgUID = await _localStorageService.GetItem<string>(orgUIDKey);
            string empUID = await _localStorageService.GetItem<string>(empUIDKey);
            string jobPositionUID = await _localStorageService.GetItem<string>(jobPositionUIDKey);

            if (string.IsNullOrEmpty(orgUID) || string.IsNullOrEmpty(empUID) || string.IsNullOrEmpty(jobPositionUID))
            {
                return null;
            }

            return UploadContext.Create(orgUID, empUID, jobPositionUID);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void ShowAlert(string? header = null, string? body = null)
    {
        _alertService.ShowSuccessAlert(header, body);
    }

    public void MoveToJP()
    {
        if (_appUser.StartDayStatus.Equals(StartDayStatus.START_DAY))
        {
            _alertService.ShowErrorAlert("", "Please start your day first to continue to Journey Plan.");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE))
        {
            _alertService.ShowErrorAlert("", "CFD has been completed for the day. You can't perform any transaction Today.");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT))
        {
            _navigationManager.NavigateTo("/CloseOfTheDay");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.BLANK))
        {
            _alertService.ShowErrorAlert("", "You have already started future date and now you can't start back date.");
        }
        else if (_appUser.SelectedRoute == null)
        {
            _alertService.ShowErrorAlert("", "Please select route to continue.");
        }
        else
        {
            NavigateTo("JourneyPlan");
        }
    }

    public void MoveAfterJourneyCheck(string path)
    {
        if (_appUser.StartDayStatus.Equals(StartDayStatus.START_DAY))
        {
            _alertService.ShowErrorAlert("", "Please start your day first to continue to Journey Plan.");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT_DONE))
        {
            _alertService.ShowErrorAlert("", "CFD has been completed for the day. You can't perform any transaction Today.");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.EOT))
        {
            _navigationManager.NavigateTo("/CloseOfTheDay");
        }
        else if (_appUser.StartDayStatus.Equals(StartDayStatus.BLANK))
        {
            _alertService.ShowErrorAlert("", "You have already started future date and now you can't start back date.");
        }
        else if (_appUser.SelectedRoute == null)
        {
            _alertService.ShowErrorAlert("", "Please select route to continue.");
        }
        else
        {
            NavigateTo(path);
        }
    }

    private async Task HandleSkipClicked()
    {

    }

    public IFileSys ConvertFileSys(string linkedItemType, string linkedItemUID,
            string fileSysType, string fileType, string fileName, string userName, string folderPath)
    {
        IFileSys fileSy = null;
        // Create FileSys List
        fileSy = CreateFileSys(Guid.NewGuid().ToString(), linkedItemType,
            linkedItemUID, fileSysType, fileType,
            fileName, userName, 0, folderPath, "0", "0",
            _appUser.SelectedJobPosition.UID, _appUser.Emp.UID);
        return fileSy;
    }

    public IFileSys CreateFileSys(string uniqueUID, string linkedItemType, string linkedItemUID, string fileSysType, string fileType, string fileName,
           string displayName, int fileSize, string relativePath, string latitude, string longitude, string jobPositionUID, string empUID)
    {
        IFileSys fileSy = new FileSys();
        fileSy.UID = uniqueUID;
        fileSy.LinkedItemType = linkedItemType;
        fileSy.LinkedItemUID = linkedItemUID;
        fileSy.FileSysType = fileSysType;
        fileSy.FileType = fileType;
        fileSy.FileName = fileName;
        fileSy.DisplayName = displayName;
        fileSy.FileSize = fileSize;
        fileSy.RelativePath = relativePath;
        fileSy.Latitude = latitude;
        fileSy.Longitude = longitude;
        fileSy.CreatedByJobPositionUID = jobPositionUID;
        fileSy.CreatedByEmpUID = empUID;
        fileSy.CreatedBy = empUID;
        fileSy.ModifiedBy = empUID;
        fileSy.CreatedTime = DateTime.Now;
        fileSy.ModifiedTime = DateTime.Now;
        return fileSy;
    }

    #region Database Synchronization

    /// <summary>
    /// Processes database synchronization with comprehensive error handling and user feedback.
    /// Supports both full sync (all groups) and selective sync (specific groups).
    /// </summary>
    /// <param name="groupNames">List of group names to sync. If null or empty, syncs all groups.</param>
    /// <param name="showTableLevelErrors">Whether to show detailed table-level error popups to users</param>
    /// <param name="showSuccessMessage">Whether to show success confirmation message on completion</param>
    /// <returns>Task that completes when sync operation finishes (including user acknowledgment of any errors)</returns>
    protected async Task ProcessDatabaseSync(List<string> groupNames = null, bool showTableLevelErrors = false, bool showSuccessMessage = false)
    {
        bool isSyncOperationComplete = false;
        bool isProgressCompletionSourceSet = false;
        TaskCompletionSource<bool> progressCompletionSource = new TaskCompletionSource<bool>();
        
        try
        {
            ShowLoader("Syncing data");
            // Create progress callback for real-time UI updates
            var progressCallback = new Progress<Winit.Modules.Syncing.Model.Classes.SyncProgress>(async progress =>
            {
                try
                {
                    // Handle error acknowledgment scenarios - these bypass the completion flag
                    if (progress.Stage == "Completed with Errors" && !string.IsNullOrEmpty(progress.ErrorMessage))
                    {
                        // Hide loader first
                        HideLoader();
                        
                        // Show error popup and wait for user acknowledgment
                        await _alertService.ShowConfirmationReturnType("Sync Completed with Errors", 
                            $"{progress.Message}<br/>Details: {progress.ErrorMessage}", "Ok", null);
                        
                        // Now mark operation as complete after user acknowledgment
                        isSyncOperationComplete = true;
                        if (!isProgressCompletionSourceSet)
                        {
                            isProgressCompletionSourceSet = true;
                            progressCompletionSource.SetResult(true);
                        }
                        return;
                    }
                    
                    // Handle critical errors that require user acknowledgment
                    if (progress.Stage == "Error" && !string.IsNullOrEmpty(progress.ErrorMessage))
                    {
                        // Hide loader first
                        HideLoader();
                        
                        // Show error popup and wait for user acknowledgment
                        await _alertService.ShowConfirmationReturnType("Sync Failed", 
                            $"{progress.Message}<br/>Error: {progress.ErrorMessage}","Ok", null);
                        
                        // Now mark operation as complete after user acknowledgment
                        isSyncOperationComplete = true;
                        if (!isProgressCompletionSourceSet)
                        {
                            isProgressCompletionSourceSet = true;
                            progressCompletionSource.SetResult(true);
                        }
                        return;
                    }
                    
                    // Handle table-level processing (when showTableLevelErrors is enabled)
                    if (progress.Stage == "Processing Table")
                    {
                        if (!string.IsNullOrEmpty(progress.ErrorMessage))
                        {
                            // Show table error popup and continue
                            await _alertService.ShowConfirmationReturnType("Table Processing Error", 
                                $"Table: {progress.TableName}<br/>{progress.Message}<br/>Error: {progress.ErrorMessage}","Continue", null);
                        }
                        else
                        {
                            // Show table processing progress (success case)
                            await _alertService.ShowConfirmationReturnType("Table Processing", 
                                $"Table: {progress.TableName}<br/>{progress.Message}","Continue", null);
                        }
                        
                        // Don't mark as complete - let sync continue
                        return;
                    }
                        
                    // Only update progress if sync operation is still in progress
                    if (!isSyncOperationComplete)
                    {
                        // Update loader with current progress information
                        string progressMessage = progress.GetFormattedMessage();
                        ShowLoader(progressMessage);
                        
                        // Force UI update to show progress immediately
                        InvokeAsync(() => StateHasChanged());

                        // Log progress for debugging
                        Console.WriteLine($"Sync Progress: {progress.Stage} - {progressMessage}");
                        
                        // Only complete on final successful completion (stage = "Completed" AND IsCompleted = true AND no errors)
                        if (progress.IsCompleted && progress.Stage == "Completed" && string.IsNullOrEmpty(progress.ErrorMessage))
                        {
                            // Show success message if requested
                            if (showSuccessMessage)
                            {
                                // Hide loader first
                                HideLoader();
                                
                                // Show success confirmation message
                                await _alertService.ShowSuccessAlert("Sync Completed", 
                                    "Database synchronization completed successfully!");
                            }
                            
                            isSyncOperationComplete = true;
                            if (!isProgressCompletionSourceSet)
                            {
                                isProgressCompletionSourceSet = true;
                                progressCompletionSource.SetResult(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors in progress callback
                    Console.WriteLine($"Error in progress callback: {ex.Message}");
                    isSyncOperationComplete = true;
                    if (!isProgressCompletionSourceSet)
                    {
                        isProgressCompletionSourceSet = true;
                        progressCompletionSource.SetException(ex);
                    }
                }
            });

            // Choose sync method based on groupNames parameter
            if (groupNames == null || groupNames.Count == 0)
            {
                // Sync all groups and all tables with progress reporting
                await _mobileDataSyncBL.SyncDataForTableGroup("", "", _appUser.Emp.UID, _appUser.SelectedJobPosition.UID,
                                _appUser.SelectedJobPosition.UserRoleUID, _appUser?.Vehicle?.UID, _appUser.SelectedJobPosition.OrgUID, _appUser.Emp.Code,
                                progressCallback, showTableLevelErrors);
            }
            else
            {
                // Sync specific groups with progress reporting
                await _mobileDataSyncBL.SyncDataForTableGroups(groupNames, "", _appUser.Emp.UID, _appUser.SelectedJobPosition.UID,
                                _appUser.SelectedJobPosition.UserRoleUID, _appUser?.Vehicle?.UID, _appUser.SelectedJobPosition.OrgUID, _appUser.Emp.Code,
                                progressCallback, showTableLevelErrors);
            }
            
            // Wait for progress callback to complete (either success or error acknowledgment)
            await progressCompletionSource.Task;
        }
        catch (Exception ex)
        {
            // Mark operation as complete to stop progress callbacks
            isSyncOperationComplete = true;
            
            // Hide loader and show error to user
            HideLoader();
            await _alertService.ShowErrorAlert("Sync Failed", $"Database synchronization failed: {ex.Message}");
            
            Console.WriteLine($"Database sync failed: {ex.Message}");
            
            // Ensure completion source is set
            if (!isProgressCompletionSourceSet)
            {
                isProgressCompletionSourceSet = true;
                progressCompletionSource.SetException(ex);
            }
            
            throw; // Re-throw to let calling method handle the error
        }
    }

    /// <summary>
    /// Convenience method to sync all database groups with standard settings.
    /// </summary>
    /// <param name="showTableLevelErrors">Whether to show detailed table-level errors (default: false)</param>
    /// <param name="showSuccessMessage">Whether to show success confirmation message (default: false)</param>
    protected async Task ProcessDatabaseSyncAll(bool showTableLevelErrors = false, bool showSuccessMessage = false)
    {
        await ProcessDatabaseSync(groupNames: null, showTableLevelErrors: showTableLevelErrors, showSuccessMessage: showSuccessMessage);
    }

    /// <summary>
    /// Convenience method to sync specific database groups.
    /// </summary>
    /// <param name="groupNames">Specific group names to sync</param>
    /// <param name="showTableLevelErrors">Whether to show detailed table-level errors (default: false)</param>
    /// <param name="showSuccessMessage">Whether to show success confirmation message (default: false)</param>
    protected async Task ProcessDatabaseSyncGroups(List<string> groupNames, bool showTableLevelErrors = false, bool showSuccessMessage = false)
    {
        await ProcessDatabaseSync(groupNames: groupNames, showTableLevelErrors: showTableLevelErrors, showSuccessMessage: showSuccessMessage);
    }

    /// <summary>
    /// Convenience method to sync critical groups (typically Master, Sales, StoreCheck).
    /// </summary>
    /// <param name="showTableLevelErrors">Whether to show detailed table-level errors (default: false)</param>
    /// <param name="showSuccessMessage">Whether to show success confirmation message (default: false)</param>
    protected async Task ProcessDatabaseSyncCritical(bool showTableLevelErrors = false, bool showSuccessMessage = false)
    {
        var criticalGroups = new List<string> { "Master", "Sales", "StoreCheck" };
        await ProcessDatabaseSync(groupNames: criticalGroups, showTableLevelErrors: showTableLevelErrors, showSuccessMessage: showSuccessMessage);
    }

    /// <summary>
    /// Convenience method for silent sync (used in login flow) - no success messages, no table-level errors.
    /// </summary>
    /// <param name="groupNames">List of group names to sync. If null or empty, syncs all groups.</param>
    protected async Task ProcessDatabaseSyncSilent(List<string> groupNames = null)
    {
        await ProcessDatabaseSync(groupNames: groupNames, showTableLevelErrors: false, showSuccessMessage: false);
    }

    /// <summary>
    /// Convenience method for interactive sync (used in Settings/manual sync) - shows success messages.
    /// </summary>
    /// <param name="groupNames">List of group names to sync. If null or empty, syncs all groups.</param>
    /// <param name="showTableLevelErrors">Whether to show detailed table-level errors (default: false)</param>
    protected async Task ProcessDatabaseSyncInteractive(List<string> groupNames = null, bool showTableLevelErrors = false)
    {
        await ProcessDatabaseSync(groupNames: groupNames, showTableLevelErrors: showTableLevelErrors, showSuccessMessage: true);
    }

    #endregion

    #region Network Connectivity Helper Methods

    /// <summary>
    /// Checks if internet connectivity is available using platform-independent NetworkHelper.
    /// </summary>
    /// <param name="timeoutMs">Timeout for ping operations in milliseconds</param>
    /// <returns>True if internet is available, false otherwise</returns>
    protected async Task<bool> IsInternetAvailableAsync(int timeoutMs = 3000)
    {
        return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
    }

    /// <summary>
    /// Checks if a specific host is reachable using platform-independent NetworkHelper.
    /// </summary>
    /// <param name="host">Host to test (IP address or domain name)</param>
    /// <param name="timeoutMs">Timeout for ping operation in milliseconds</param>
    /// <returns>True if host is reachable, false otherwise</returns>
    protected async Task<bool> IsHostReachableAsync(string host, int timeoutMs = 3000)
    {
        return await NetworkHelper.IsHostReachableAsync(host, timeoutMs);
    }

    /// <summary>
    /// Gets the device's network access status using MAUI-specific NetworkConnectivityService.
    /// This is a platform-specific feature that checks device network interface status.
    /// </summary>
    /// <returns>Network access status from device</returns>
    protected NetworkAccess GetDeviceNetworkStatus()
    {
        try
        {
            return _networkConnectivityService?.GetDeviceNetworkStatus() ?? NetworkAccess.Unknown;
        }
        catch
        {
            return NetworkAccess.Unknown;
        }
    }

    /// <summary>
    /// Fast connectivity check optimized for login and critical operations.
    /// Uses device check + fast ping test with very short timeout for quick feedback.
    /// INCLUDES PING TEST for reliability - device status alone is not sufficient.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds for ping test (default: 800ms for very fast response)</param>
    /// <returns>True if connected, false otherwise</returns>
    protected async Task<bool> IsFastConnectivityAvailable(int timeoutMs = 800)
    {
        try
        {
            // STEP 1: Quick device network status check first (instant)
            var deviceNetworkStatus = GetDeviceNetworkStatus();
            
            // If device has no network interface, return false immediately
            if (deviceNetworkStatus == NetworkAccess.None)
                return false;

            // STEP 2: Device has network interface - now do PING TEST for actual internet
            // This ping test is critical - device status can show "Connected" with no internet
            if (deviceNetworkStatus == NetworkAccess.Unknown)
            {
                // Device status unknown - do ping test to verify
                return await NetworkHelper.IsInternetAvailableAsync(timeoutMs);
            }

            // Device shows network access - still need ping test to verify actual internet
            return _networkConnectivityService != null
                ? await _networkConnectivityService.IsConnectedAsync(timeoutMs)  // This includes ping test
                : await NetworkHelper.IsInternetAvailableAsync(timeoutMs);      // Direct ping test
        }
        catch
        {
            // On error, assume no connection for safety in critical operations
            return false;
        }
    }

    /// <summary>
    /// Main internet connectivity check with configurable behavior.
    /// Supports both silent and alert modes with customizable timeout.
    /// Optimized for login and critical operations with fast response times.
    /// </summary>
    /// <param name="showAlert">Whether to show alert on connectivity failure (default: true)</param>
    /// <param name="title">Alert title (used only if showAlert is true)</param>
    /// <param name="message">Alert message (used only if showAlert is true)</param>
    /// <param name="timeoutMs">Timeout in milliseconds for connectivity test (default: 800ms for fast response)</param>
    /// <returns>True if connected, false if no connection</returns>
    protected async Task<bool> CheckInternetConnectivity(
        bool showAlert = true,
        string title = "Network Error", 
        string message = "No internet connection. Please check your network settings and try again.",
        int timeoutMs = 800)
    {
        try
        {
            if (await IsFastConnectivityAvailable(timeoutMs))
                return true;

            // Show alert only if requested
            if (showAlert && _alertService != null)
                await _alertService.ShowErrorAlert(title, message);
            
            return false;
        }
        catch
        {
            // On error, assume connected to avoid blocking user
            return true;
        }
    }

    #endregion
}

