using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Auth.BL.Interfaces
{
    /// <summary>
    /// Interface for synchronization view model containing properties for database sync operations.
    /// After refactoring SyncDbInit, several properties are no longer needed.
    /// </summary>
    public interface ISyncViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether clear data operation is required.
        /// Used by ClearDataValidate class for determining data clearing strategy.
        /// </summary>
        public bool ClearData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sync push operations are pending.
        /// Used by ClearDataValidate to determine upload strategy.
        /// </summary>
        public bool IsSyncPushPending { get; set; }

        /// <summary>
        /// Gets or sets the username for sync operations.
        /// Used by ClearDataValidate to retrieve MobileAppAction data.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the application action to be performed (e.g., CLEAR_DATA, NO_ACTION).
        /// Used by ClearDataValidate for determining sync workflow.
        /// </summary>
        public string? AppAction { get; set; }

        /// <summary>
        /// Gets or sets the sync mode (Upload/Download).
        /// Used by ClearDataValidate to set operation mode.
        /// </summary>
        public string? Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation completed successfully.
        /// Used by SyncDbInit to report operation status.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the title for sync operation (rarely used).
        /// Used by SyncDbInit for error scenarios.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the error message if operation failed.
        /// Used by SyncDbInit to report detailed error information.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the SQLite database file path after successful creation.
        /// Used by SyncDbInit to store the final database path.
        /// </summary>
        public string? SqlitePath { get; set; }

        // REMOVED PROPERTIES (No longer needed after refactoring):
        // - IsMainDbExist: Database existence is now checked directly by file system
        // - IsOnline: Network connectivity is checked directly by network services  
        // - SqliteZipPath: ZIP path is handled internally by SyncDbInit download logic
    }
}
