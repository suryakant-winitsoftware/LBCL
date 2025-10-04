using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.DL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Common.BL.Classes
{
    /// <summary>
    /// Service for validating data synchronization between SQLite and server
    /// </summary>
    public class DataSyncValidationServiceBL : IDataSyncValidationServiceBL
    {
        #region Constants
        private const string LastLoggedInUserKey = "LastLoggedInUser";
        private const string SyncPendingMessage = "Data synchronization is pending. Please complete sync before changing user.";
        private const string DatabaseNotFoundMessage = "SQLite database not found.";
        private const string SyncValidationErrorMessage = "Error occurred during sync validation";
        #endregion

        #region Fields
        private readonly IDataSyncValidationDL _dataSyncValidationDL;
        private readonly ILocalStorageService _localStorageService;
        private readonly CommonFunctions _commonFunctions;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DataSyncValidationService class
        /// </summary>
        /// <param name="dataSyncValidationDL">Data layer for sync validation operations</param>
        /// <param name="localStorageService">Local storage service for preferences</param>
        /// <param name="commonFunctions">Common utility functions</param>
        public DataSyncValidationServiceBL(
            IDataSyncValidationDL dataSyncValidationDL,
            ILocalStorageService localStorageService,
            CommonFunctions commonFunctions)
        {
            _dataSyncValidationDL = dataSyncValidationDL ?? throw new ArgumentNullException(nameof(dataSyncValidationDL));
            _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
            _commonFunctions = commonFunctions ?? throw new ArgumentNullException(nameof(commonFunctions));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if all SQLite data has been posted to the server
        /// </summary>
        /// <returns>True if all data is synchronized, false otherwise</returns>
        public async Task<bool> IsAllDataSynchronizedAsync()
        {
            try
            {
                if (!await _dataSyncValidationDL.IsDatabaseAvailableAsync())
                    return true; // No database means no pending data

                var pendingData = await _dataSyncValidationDL.GetPendingPushData();
                return pendingData?.Count == 0;
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                return false; // Assume not synchronized on error
            }
        }

        /// <summary>
        /// Gets pending push data with table-wise breakdown
        /// </summary>
        /// <returns>Dictionary with table names as keys and pending record counts as values</returns>
        public async Task<Dictionary<string, int>> GetPendingPushDataAsync()
        {
            try
            {
                return await _dataSyncValidationDL.GetPendingPushData() ?? new Dictionary<string, int>();
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Gets the last synchronization timestamp for a user
        /// </summary>
        /// <param name="userId">User ID to get timestamp for</param>
        /// <returns>Last sync timestamp or null if not available</returns>
        public async Task<DateTime?> GetLastSyncTimestampAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                return await _dataSyncValidationDL.GetLastSyncTimestampAsync(userId);
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                return null;
            }
        }

        /// <summary>
        /// Checks if the SQLite database file exists
        /// </summary>
        /// <returns>True if database exists, false otherwise</returns>
        public async Task<bool> IsDatabaseAvailableAsync()
        {
            try
            {
                return await _dataSyncValidationDL.IsDatabaseAvailableAsync();
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                return false;
            }
        }

        /// <summary>
        /// Handles user change validation and database cleanup if needed
        /// </summary>
        /// <param name="currentUserId">Current user ID</param>
        /// <param name="onDatabaseCleanup">Callback to execute database cleanup</param>
        /// <returns>True if user change is allowed, false if sync is pending</returns>
        public async Task<bool> HandleUserChangeValidationAsync(string currentUserId, Func<Task> onDatabaseCleanup = null)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("Current user ID cannot be empty", nameof(currentUserId));

            try
            {
                string lastLoggedInUser = await _localStorageService.GetItem<string>(LastLoggedInUserKey) ?? string.Empty;

                // If same user, no validation needed
                if (string.IsNullOrEmpty(lastLoggedInUser) || currentUserId == lastLoggedInUser)
                    return true;

                // Check if there's pending sync data
                bool isDataSynchronized = await IsAllDataSynchronizedAsync();

                if (!isDataSynchronized)
                {
                    // Data is not synchronized, prevent user change
                    return false;
                }

                // Data is synchronized, proceed with cleanup
                if (onDatabaseCleanup != null)
                {
                    await onDatabaseCleanup.Invoke();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                throw new Exception(SyncValidationErrorMessage, ex);
            }
        }
        #endregion
    }
} 