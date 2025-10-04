using System;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.UIComponents.Common.Extensions
{
    /// <summary>
    /// Extension methods for sync validation functionality
    /// </summary>
    public static class SyncValidationExtensions
    {
        #region Constants
        private const string SyncPendingTitle = "Sync Pending";
        private const string SyncPendingMessage = "You have unsynchronized data. Please complete synchronization before changing user.";
        private const string SyncButton = "Sync Now";
        private const string ContinueAnywayButton = "Continue Anyway";
        private const string ErrorTitle = "Error";
        private const string ValidationErrorMessage = "Error during user validation. Please try again.";
        #endregion

        /// <summary>
        /// Handles user change validation with sync check and user interaction
        /// </summary>
        /// <param name="dataSyncService">Data sync validation service</param>
        /// <param name="currentUserId">Current user ID</param>
        /// <param name="onDatabaseCleanup">Database cleanup callback</param>
        /// <param name="onSyncRequired">Sync required callback</param>
        /// <param name="onShowAlert">Alert display callback</param>
        /// <param name="onShowConfirmation">Confirmation dialog callback</param>
        /// <returns>True if user change is allowed, false otherwise</returns>
        public static async Task<bool> HandleUserChangeWithUIAsync(
            this IDataSyncValidationService dataSyncService,
            string currentUserId,
            Func<Task> onDatabaseCleanup = null,
            Func<Task> onSyncRequired = null,
            Func<string, string, Task> onShowAlert = null,
            Func<string, string, string, string, Task<bool>> onShowConfirmation = null)
        {
            try
            {
                bool canProceed = await dataSyncService.HandleUserChangeValidationAsync(
                    currentUserId, 
                    onDatabaseCleanup);

                if (!canProceed)
                {
                    // Get sync info for display
                    var syncInfo = await dataSyncService.GetSyncStatusInfoAsync();
                    
                    string message = $"{SyncPendingMessage}\n\nPending records: {syncInfo.PendingRecordsCount}";
                    
                    if (onShowConfirmation != null)
                    {
                        bool shouldSync = await onShowConfirmation.Invoke(
                            SyncPendingTitle, 
                            message, 
                            SyncButton, 
                            ContinueAnywayButton);

                        if (shouldSync && onSyncRequired != null)
                        {
                            await onSyncRequired.Invoke();
                        }
                        else if (!shouldSync && onDatabaseCleanup != null)
                        {
                            // User chose to continue anyway, force cleanup
                            await onDatabaseCleanup.Invoke();
                        }
                    }
                    
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (onShowAlert != null)
                {
                    await onShowAlert.Invoke(ErrorTitle, ValidationErrorMessage);
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a formatted sync status message
        /// </summary>
        /// <param name="dataSyncService">Data sync validation service</param>
        /// <returns>Formatted sync status message</returns>
        public static async Task<string> GetFormattedSyncStatusAsync(this IDataSyncValidationService dataSyncService)
        {
            var syncInfo = await dataSyncService.GetSyncStatusInfoAsync();
            
            if (syncInfo.IsFullySynchronized)
            {
                return "All data is synchronized.";
            }

            var message = $"Synchronization pending:\n";
            message += $"Total records: {syncInfo.PendingRecordsCount}\n";
            
            if (syncInfo.PendingRecordsByTable.Count > 0)
            {
                message += "Breakdown by table:\n";
                foreach (var table in syncInfo.PendingRecordsByTable)
                {
                    message += $"  {table.Key}: {table.Value} records\n";
                }
            }

            if (syncInfo.LastSyncTimestamp.HasValue)
            {
                message += $"Last sync: {syncInfo.LastSyncTimestamp.Value:yyyy-MM-dd HH:mm:ss}";
            }

            return message;
        }

        /// <summary>
        /// Quick check if user change is safe (no UI interaction)
        /// </summary>
        /// <param name="dataSyncService">Data sync validation service</param>
        /// <param name="currentUserId">Current user ID</param>
        /// <returns>True if safe to change user, false if sync is needed</returns>
        public static async Task<bool> IsSafeToChangeUserAsync(
            this IDataSyncValidationService dataSyncService,
            string currentUserId)
        {
            try
            {
                return await dataSyncService.HandleUserChangeValidationAsync(currentUserId);
            }
            catch
            {
                return false;
            }
        }
    }
} 