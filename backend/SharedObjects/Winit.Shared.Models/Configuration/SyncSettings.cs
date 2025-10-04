namespace Winit.Shared.Models.Configuration
{
    /// <summary>
    /// Configuration settings for sync operations
    /// </summary>
    public class SyncSettings
    {
        /// <summary>
        /// Enable parallel processing of tables during sync
        /// </summary>
        public bool EnableParallelSync { get; set; } = true;

        /// <summary>
        /// Maximum number of tables to sync concurrently
        /// </summary>
        public int MaxConcurrentTables { get; set; } = 4;

        /// <summary>
        /// Maximum number of batches to process concurrently within a table
        /// </summary>
        public int MaxConcurrentBatches { get; set; } = 2;

        /// <summary>
        /// Number of records per batch for SQLite operations
        /// </summary>
        public int BatchSize { get; set; } = 5000;

        /// <summary>
        /// Enable detailed sync logging
        /// </summary>
        public bool EnableSyncLogging { get; set; } = true;
    }
}