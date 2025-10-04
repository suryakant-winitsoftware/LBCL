using Winit.Modules.Auth.BL.Interfaces;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Auth.Model.Constants;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Mobile.Model.Classes;
using Winit.Modules.Mobile.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.BL.Classes
{
    /// <summary>
    /// Service to determine ClearData actions based on server configuration
    /// </summary>
    public class ClearDataServiceBL : IClearDataServiceBL
    {
        #region Constants
        private const string MobileAppActionApiEndpoint = "MobileAppAction/GetMobileAppAction";
        private const string ApiErrorMessage = "Failed to retrieve mobile app action from server";
        #endregion

        #region Fields
        private readonly IAppConfig _appConfig;
        private readonly ApiService _apiService;
        private readonly IDataSyncValidationServiceBL _dataSyncValidationService;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the ClearDataServiceBL class
        /// </summary>
        /// <param name="appConfig">Application configuration</param>
        /// <param name="apiService">API service for server communication</param>
        /// <param name="dataSyncValidationService">Data sync validation service</param>
        public ClearDataServiceBL(
            IAppConfig appConfig,
            ApiService apiService,
            IDataSyncValidationServiceBL dataSyncValidationService)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _dataSyncValidationService = dataSyncValidationService ?? throw new ArgumentNullException(nameof(dataSyncValidationService));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines whether database should be deleted based on server configuration
        /// </summary>
        /// <param name="empUID">Employee UID</param>
        /// <param name="onDataUpload">Callback to execute data upload if needed</param>
        /// <returns>ClearDataResult indicating whether database should be deleted</returns>
        public async Task<IClearDataResult> ShouldDeleteDatabaseAsync(string empUID, Func<Task> onDataUpload = null)
        {
            if (string.IsNullOrWhiteSpace(empUID))
                throw new ArgumentException("Employee UID cannot be empty", nameof(empUID));

            try
            {
                // Get mobile app action from server
                var mobileAppAction = await GetMobileAppActionByEmpUIDAsync(empUID);
                string serverAction = GetActionFromResponse(mobileAppAction);

                // Determine whether database should be deleted
                return await ProcessServerActionAsync(serverAction, onDataUpload);
            }
            catch (Exception ex)
            {
                // Log exception and return default (no deletion)
                return new ClearDataResult
                {
                    ShouldDeleteDatabase = false,
                    ServerAction = DBActions.NO_ACTION,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets mobile app action from server API
        /// </summary>
        /// <param name="empUID">Employee UID</param>
        /// <returns>Mobile app action response</returns>
        private async Task<IMobileAppAction?> GetMobileAppActionByEmpUIDAsync(string empUID)
        {
            try
            {
                string apiUrl = $"{_appConfig.ApiBaseUrl}{MobileAppActionApiEndpoint}?empUID={empUID}";
                
                ApiResponse<MobileAppAction> apiResponse = await _apiService.FetchDataAsync<MobileAppAction>(
                    apiUrl, 
                    HttpMethod.Get);

                return apiResponse?.IsSuccess == true && apiResponse.Data != null 
                    ? apiResponse.Data 
                    : null;
            }
            catch (Exception ex)
            {
                throw new Exception(ApiErrorMessage, ex);
            }
        }

        /// <summary>
        /// Extracts action from mobile app action response
        /// </summary>
        /// <param name="mobileAppAction">Mobile app action response</param>
        /// <returns>Action string</returns>
        private static string GetActionFromResponse(IMobileAppAction? mobileAppAction)
        {
            return mobileAppAction?.Status == 0 
                ? mobileAppAction.Action ?? DBActions.NO_ACTION
                : DBActions.NO_ACTION;
        }

        /// <summary>
        /// Processes server action and determines whether database should be deleted
        /// </summary>
        /// <param name="serverAction">Server action response</param>
        /// <param name="onDataUpload">Data upload callback if needed</param>
        /// <returns>ClearData result</returns>
        private async Task<IClearDataResult> ProcessServerActionAsync(string serverAction, Func<Task> onDataUpload)
        {
            switch (serverAction)
            {
                case DBActions.CLEAR_DATA:
                    // Delete database without any check
                    return CreateResult(true, serverAction);

                case DBActions.CLEAR_DATA_AFTER_UPLOAD:
                    // Check if anything pending, handle upload if needed, then decide deletion
                    return await HandleClearDataAfterUploadAsync(serverAction, onDataUpload);

                case DBActions.NO_ACTION:
                default:
                    // No database deletion, proceed with sync
                    return CreateResult(false, serverAction);
            }
        }

        /// <summary>
        /// Handles CLEAR_DATA_AFTER_UPLOAD logic
        /// </summary>
        /// <param name="serverAction">Server action</param>
        /// <param name="onDataUpload">Data upload callback</param>
        /// <returns>ClearData result</returns>
        private async Task<IClearDataResult> HandleClearDataAfterUploadAsync(string serverAction, Func<Task> onDataUpload)
        {
            try
            {
                // Check if anything pending to be posted
                bool isAllDataSynced = await _dataSyncValidationService.IsAllDataSynchronizedAsync();

                if (isAllDataSynced)
                {
                    // Nothing pending, delete database
                    return CreateResult(true, serverAction);
                }

                // Data pending, upload first if callback provided
                if (onDataUpload != null)
                {
                    await onDataUpload.Invoke();
                    
                    // Check again after upload
                    isAllDataSynced = await _dataSyncValidationService.IsAllDataSynchronizedAsync();
                    
                    if (isAllDataSynced)
                    {
                        // All posted after upload, delete database
                        return CreateResult(true, serverAction);
                    }
                }

                // Still pending data or no upload callback, don't delete
                return CreateResult(false, serverAction, "Data upload incomplete, database will not be deleted");
            }
            catch (Exception ex)
            {
                // Error during process, don't delete database
                return CreateResult(false, serverAction, $"Error during upload process: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a ClearDataResult with specified parameters
        /// </summary>
        /// <param name="shouldDeleteDatabase">Whether database should be deleted</param>
        /// <param name="serverAction">Original server action</param>
        /// <param name="errorMessage">Error message if any</param>
        /// <returns>ClearDataResult</returns>
        private static IClearDataResult CreateResult(
            bool shouldDeleteDatabase,
            string serverAction,
            string errorMessage = "")
        {
            return new ClearDataResult
            {
                ShouldDeleteDatabase = shouldDeleteDatabase,
                ServerAction = serverAction,
                Success = string.IsNullOrEmpty(errorMessage),
                ErrorMessage = errorMessage
            };
        }
        #endregion
    }
} 