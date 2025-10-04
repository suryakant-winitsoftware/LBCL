using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.DL.Interfaces
{
    /// <summary>
    /// Data layer interface for sync validation operations
    /// </summary>
    public interface IDataSyncValidationDL
    {
        /// <summary>
        /// Gets the last synchronization timestamp for a user
        /// </summary>
        /// <param name="userId">User ID to get timestamp for</param>
        /// <returns>Last sync timestamp or null if not available</returns>
        Task<DateTime?> GetLastSyncTimestampAsync(string userId);

        /// <summary>
        /// Checks if the SQLite database file exists
        /// </summary>
        /// <returns>True if database exists, false otherwise</returns>
        Task<bool> IsDatabaseAvailableAsync();

        /// <summary>
        /// Gets pending push data from all tables that need to be synchronized
        /// </summary>
        /// <returns>Dictionary with table names as keys and pending record counts as values</returns>
        Task<Dictionary<string, int>> GetPendingPushData();
    }
} 