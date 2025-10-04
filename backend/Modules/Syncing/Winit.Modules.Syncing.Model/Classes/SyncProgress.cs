using System;

namespace Winit.Modules.Syncing.Model.Classes
{
    /// <summary>
    /// Represents progress information for sync operations.
    /// Used to report real-time sync progress to the UI.
    /// </summary>
    public class SyncProgress
    {
        /// <summary>
        /// Current stage of the sync operation
        /// </summary>
        public string Stage { get; set; } = string.Empty;

        /// <summary>
        /// Current group being synced
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Current table being synced
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Number of records processed in current table
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Current group index (0-based)
        /// </summary>
        public int CurrentGroupIndex { get; set; }

        /// <summary>
        /// Total number of groups to process
        /// </summary>
        public int TotalGroups { get; set; }

        /// <summary>
        /// Current table index within group (0-based)
        /// </summary>
        public int CurrentTableIndex { get; set; }

        /// <summary>
        /// Total number of tables in current group
        /// </summary>
        public int TotalTablesInGroup { get; set; }

        /// <summary>
        /// Overall percentage complete (0-100)
        /// </summary>
        public int PercentageComplete { get; set; }

        /// <summary>
        /// Detailed progress message for display
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the operation completed successfully
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Error message if sync failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Creates a formatted progress message for display
        /// </summary>
        /// <returns>Formatted progress message</returns>
        public string GetFormattedMessage()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                return $"Error: {ErrorMessage}";
            }

            if (IsCompleted)
            {
                return "Sync completed successfully";
            }

            if (!string.IsNullOrEmpty(TableName) && RecordCount > 0)
            {
                return $"Syncing group: {GroupName} table: {TableName} - {RecordCount} records";
            }

            if (!string.IsNullOrEmpty(GroupName))
            {
                return $"Syncing group: {GroupName}";
            }

            return Message;
        }
    }
} 