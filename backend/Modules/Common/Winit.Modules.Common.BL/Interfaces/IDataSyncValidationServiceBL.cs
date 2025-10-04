using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Classes;

namespace Winit.Modules.Common.BL.Interfaces
{
    /// <summary>
    /// Interface for data synchronization validation service
    /// </summary>
    public interface IDataSyncValidationServiceBL
    {
        /// <summary>
        /// Checks if all SQLite data has been posted to the server
        /// </summary>
        /// <returns>True if all data is synchronized, false otherwise</returns>
        Task<bool> IsAllDataSynchronizedAsync();

        /// <summary>
        /// Gets pending push data with table-wise breakdown
        /// </summary>
        /// <returns>Dictionary with table names as keys and pending record counts as values</returns>
        Task<Dictionary<string, int>> GetPendingPushDataAsync();

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
    }

    
} 