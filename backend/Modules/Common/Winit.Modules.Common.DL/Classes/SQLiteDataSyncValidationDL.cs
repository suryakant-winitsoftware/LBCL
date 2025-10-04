using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Common.DL.Interfaces;
using Winit.Shared.Models.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Interfaces;

namespace Winit.Modules.Common.DL.Classes
{
    /// <summary>
    /// Data layer repository for sync validation operations
    /// </summary>
    public class SQLiteDataSyncValidationDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IDataSyncValidationDL
    {
        #region Constants
        private const string SqliteDatabaseName = "WINITSQLite.db";
        private const string PendingTablesQuery = @"SELECT DISTINCT table_name FROM table_group_entity WHERE has_upload = 1 
                 AND table_name NOT IN ('sales_order_line', 'sales_order_tax', 'sales_order_discount', 
                 'return_order_line', 'return_order_tax', 'return_order_discount',
                 'exchange_order_line')";
        private const string PendingRecordsForTableQuery = @"SELECT COUNT(1) AS NoOfRecords FROM {0} WHERE SS IN (-1,1,2)";
        #endregion

        #region Fields
        private readonly IAppConfig _appConfig;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DataSyncValidationDL class
        /// </summary>
        /// <param name="sqliteManager">SQLite database manager</param>
        /// <param name="appConfig">Application configuration</param>
        public SQLiteDataSyncValidationDL(
            IServiceProvider serviceProvider, IConfiguration config,
            IAppConfig appConfig) : base(serviceProvider)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the last synchronization timestamp for a user
        /// </summary>
        /// <param name="userId">User ID to get timestamp for</param>
        /// <returns>Last sync timestamp or null if not available</returns>
        public async Task<DateTime?> GetLastSyncTimestampAsync(string userId)
        {
            try
            {
                if (!await IsDatabaseAvailableAsync() || string.IsNullOrEmpty(userId))
                    return null;

                // TODO: Implement if you have a sync timestamp table
                // For now, return null as this might not be required for your use case
                return null;
            }
            catch (Exception ex)
            {
                // Log exception here if logging is available
                return null;
            }
        }

        /// <summary>
        /// Checks if the SQLite database is available and properly initialized with essential tables
        /// </summary>
        /// <returns>True if database exists and is properly initialized, false otherwise</returns>
        public async Task<bool> IsDatabaseAvailableAsync()
        {
            try
            {
                string sqlitePath = Path.Combine(_appConfig.BaseFolderPath, "DB", SqliteDatabaseName);
                
                // First check if file exists
                if (!File.Exists(sqlitePath))
                {
                    return false;
                }

                // Check if file has content (not zero bytes)
                var fileInfo = new FileInfo(sqlitePath);
                if (fileInfo.Length == 0)
                {
                    Console.WriteLine("Warning: SQLite database file exists but is empty (0 bytes)");
                    return false;
                }

                // Check if database is properly initialized with essential tables
                return await IsDatabaseProperlyInitializedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking database availability: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies that the database is properly initialized by checking for essential tables
        /// </summary>
        /// <returns>True if database contains essential tables, false otherwise</returns>
        private async Task<bool> IsDatabaseProperlyInitializedAsync()
        {
            try
            {
                // check: Verify table_group_entity exists (used for sync validation)
                string tableGroupEntityQuery = @"SELECT count(1) AS Count FROM sqlite_master WHERE type='table' AND name='table_group_entity'";
                int tableGroupEntityCount = await ExecuteScalarAsync<int>(tableGroupEntityQuery);
                
                if (tableGroupEntityCount <= 0)
                {
                    Console.WriteLine("Warning: Database exists but 'table_group_entity' table is missing");
                    return false;
                }

                Console.WriteLine("Database is properly initialized with essential tables");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying database initialization: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets pending push data from all tables that need to be synchronized
        /// </summary>
        /// <returns>Dictionary with table names as keys and pending record counts as values</returns>
        public async Task<Dictionary<string, int>> GetPendingPushData()
        {
            var result = new Dictionary<string, int>();
            
            try
            {
                // First get the list of tables from table_group_entity
                DataSet ds = await ExecuteQueryDataSetAsync(new string[] { PendingTablesQuery });
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return result;
                }

                // Get list of existing tables in SQLite database
                var existingTables = await GetExistingTablesAsync();
                
                // Build UNION ALL query only for tables that actually exist
                var unionQueries = new List<string>();
                var skippedTables = new List<string>();
                
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string tableName = row["table_name"].ToString();
                    
                    // Check if table exists before adding to union query
                    if (existingTables.Contains(tableName.ToLower()))
                    {
                        unionQueries.Add($"SELECT '{tableName}' AS TableName, COUNT(1) AS NoOfRecords FROM {tableName} WHERE SS IN (-1,1,2)");
                    }
                    else
                    {
                        skippedTables.Add(tableName);
                    }
                }

                // Log skipped tables for debugging
                if (skippedTables.Count > 0)
                {
                    Console.WriteLine($"Warning: Skipped {skippedTables.Count} missing tables: {string.Join(", ", skippedTables)}");
                }

                // Execute efficient single query only if we have existing tables
                if (unionQueries.Count > 0)
                {
                    string combinedQuery = string.Join(" UNION ALL ", unionQueries);
                    DataSet countsDs = await ExecuteQueryDataSetAsync(new string[] { combinedQuery });
                    
                    if (countsDs != null && countsDs.Tables.Count > 0 && countsDs.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow countRow in countsDs.Tables[0].Rows)
                        {
                            string tableName = countRow["TableName"].ToString();
                            int noOfRecords = Convert.ToInt32(countRow["NoOfRecords"]);
                            if (noOfRecords > 0)
                            {
                                result[tableName] = noOfRecords;
                                Console.WriteLine($"Found {noOfRecords} pending records in table '{tableName}'");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Warning: No existing tables found for pending data check");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending push data: {ex.Message}");
                // Return partial results instead of empty dictionary
            }
            
            return result;
        }

        /// <summary>
        /// Gets a list of existing table names in the SQLite database
        /// </summary>
        /// <returns>HashSet of existing table names (lowercase)</returns>
        private async Task<HashSet<string>> GetExistingTablesAsync()
        {
            var existingTables = new HashSet<string>();
            
            try
            {
                const string existingTablesQuery = "SELECT name FROM sqlite_master WHERE type='table'";
                DataSet ds = await ExecuteQueryDataSetAsync(new string[] { existingTablesQuery });
                
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string tableName = row["name"].ToString().ToLower();
                        existingTables.Add(tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting existing tables: {ex.Message}");
            }
            
            return existingTables;
        }
        #endregion
    }
} 