using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.Model.Classes
{
    /// <summary>
    /// Contains information about synchronization status
    /// </summary>
    public class SyncStatusInfo
    {
        /// <summary>
        /// Gets or sets whether all data is synchronized
        /// </summary>
        public bool IsFullySynchronized { get; set; }

        /// <summary>
        /// Gets or sets the total count of pending records
        /// </summary>
        public int PendingRecordsCount { get; set; }

        /// <summary>
        /// Gets or sets the breakdown of pending records by table
        /// </summary>
        public Dictionary<string, int> PendingRecordsByTable { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the last synchronization timestamp
        /// </summary>
        public DateTime? LastSyncTimestamp { get; set; }

        /// <summary>
        /// Gets or sets any error messages
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
