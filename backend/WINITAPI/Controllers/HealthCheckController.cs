using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WINITAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(IConfiguration configuration, ILogger<HealthCheckController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet("")]
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "3.0.0",
                service = "WINIT API"
            });
        }

        /// <summary>
        /// Detailed health check with component status
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> DetailedHealth()
        {
            var healthStatus = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["service"] = "WINIT API",
                ["version"] = "3.0.0"
            };

            var componentChecks = new Dictionary<string, object>();

            // Check API Status
            componentChecks["api"] = new
            {
                status = "healthy",
                responseTime = "1ms"
            };

            // Check Database
            var dbCheck = await CheckDatabase();
            componentChecks["database"] = dbCheck;

            // Check V3 Tables
            var tableCheck = await CheckV3Tables();
            componentChecks["v3_tables"] = tableCheck;

            // Check SQLite Database
            var sqliteCheck = await CheckSQLiteDatabase();
            componentChecks["sqlite_database"] = sqliteCheck;

            // Overall status
            bool isHealthy = (bool)((dynamic)dbCheck).healthy && 
                           (bool)((dynamic)tableCheck).healthy && 
                           (bool)((dynamic)sqliteCheck).healthy;
            healthStatus["status"] = isHealthy ? "healthy" : "unhealthy";
            healthStatus["components"] = componentChecks;

            return isHealthy ? Ok(healthStatus) : StatusCode(503, healthStatus);
        }

        /// <summary>
        /// Mobile sync specific health check
        /// </summary>
        [HttpGet("mobile-sync")]
        public async Task<IActionResult> MobileSyncHealth()
        {
            var syncHealth = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["service"] = "Mobile Sync Health Check"
            };

            var componentChecks = new Dictionary<string, object>();

            // Check PostgreSQL Database
            var postgresCheck = await CheckDatabase();
            componentChecks["postgresql"] = postgresCheck;

            // Check SQLite Database
            var sqliteCheck = await CheckSQLiteDatabase();
            componentChecks["sqlite"] = sqliteCheck;

            // Check Essential Tables for Mobile Sync
            var mobileTablesCheck = await CheckMobileSyncTables();
            componentChecks["mobile_tables"] = mobileTablesCheck;

            // Check File System Access
            var fileSystemCheck = await CheckFileSystemAccess();
            componentChecks["file_system"] = fileSystemCheck;

            // Overall mobile sync health
            bool isMobileSyncHealthy = (bool)((dynamic)postgresCheck).healthy && 
                                     (bool)((dynamic)sqliteCheck).healthy && 
                                     (bool)((dynamic)mobileTablesCheck).healthy &&
                                     (bool)((dynamic)fileSystemCheck).healthy;

            syncHealth["status"] = isMobileSyncHealthy ? "healthy" : "unhealthy";
            syncHealth["components"] = componentChecks;
            syncHealth["mobile_sync_ready"] = isMobileSyncHealthy;

            return isMobileSyncHealthy ? Ok(syncHealth) : StatusCode(503, syncHealth);
        }

        /// <summary>
        /// Check if system is ready to accept traffic
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Check database
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgreSQL"));
                await connection.OpenAsync();
                
                // Check if essential tables exist
                var essentialTablesExist = await connection.QueryFirstAsync<bool>(@"
                    SELECT 
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'promotion') AND
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'product_promotion_config') AND
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'promotion_volume_cap')
                ");

                if (essentialTablesExist)
                {
                    return Ok(new { ready = true, timestamp = DateTime.UtcNow });
                }
                else
                {
                    return StatusCode(503, new { ready = false, reason = "Essential tables not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { ready = false, reason = ex.Message });
            }
        }

        /// <summary>
        /// Simple liveness check
        /// </summary>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { alive = true, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Check database connectivity and performance
        /// </summary>
        private async Task<object> CheckDatabase()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgreSQL"));
                
                await connection.OpenAsync();
                var result = await connection.QueryFirstAsync<int>("SELECT 1");
                
                stopwatch.Stop();

                return new
                {
                    healthy = true,
                    status = "connected",
                    responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                    database = connection.Database
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return new
                {
                    healthy = false,
                    status = "disconnected",
                    error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check V3 tables existence
        /// </summary>
        private async Task<object> CheckV3Tables()
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgreSQL"));
                await connection.OpenAsync();
                
                var tablesExist = await connection.QueryFirstAsync<bool>(@"
                    SELECT 
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'promotion') AND
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'product_promotion_config') AND
                        EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'promotion_volume_cap')
                ");

                return new
                {
                    healthy = tablesExist,
                    status = tablesExist ? "tables_exist" : "missing_tables",
                    tables = new[] { "promotion", "product_promotion_config", "promotion_volume_cap" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "V3 tables health check failed");
                return new
                {
                    healthy = false,
                    status = "error",
                    error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check SQLite database health
        /// </summary>
        private async Task<object> CheckSQLiteDatabase()
        {
            try
            {
                var sqlitePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "Sqlite", "Base", "WINITSQLite.db");
                
                if (!System.IO.File.Exists(sqlitePath))
                {
                    return new
                    {
                        healthy = false,
                        status = "file_not_found",
                        path = sqlitePath
                    };
                }

                var fileInfo = new System.IO.FileInfo(sqlitePath);
                var fileSize = fileInfo.Length;

                return new
                {
                    healthy = true,
                    status = "available",
                    path = sqlitePath,
                    size = $"{fileSize / 1024 / 1024}MB",
                    lastModified = fileInfo.LastWriteTimeUtc
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite database health check failed");
                return new
                {
                    healthy = false,
                    status = "error",
                    error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check mobile sync essential tables
        /// </summary>
        private async Task<object> CheckMobileSyncTables()
        {
            try
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("PostgreSQL"));
                await connection.OpenAsync();
                
                var essentialTables = new[] { 
                    "store_group_data", "org_hierarchy", "list_item", "org_currency", "tax_group_taxes",
                    "tax", "sku", "vehicle", "currency"
                };

                var tableChecks = new List<object>();
                bool allTablesExist = true;

                foreach (var table in essentialTables)
                {
                    var exists = await connection.QueryFirstAsync<bool>($@"
                        SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{table}')
                    ");
                    
                    tableChecks.Add(new { table, exists });
                    if (!exists) allTablesExist = false;
                }

                return new
                {
                    healthy = allTablesExist,
                    status = allTablesExist ? "all_tables_exist" : "missing_tables",
                    tables = tableChecks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mobile sync tables health check failed");
                return new
                {
                    healthy = false,
                    status = "error",
                    error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check file system access for mobile sync
        /// </summary>
        private async Task<object> CheckFileSystemAccess()
        {
            try
            {
                var dataPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data");
                var sqlitePath = System.IO.Path.Combine(dataPath, "Sqlite", "Base");
                
                var dataPathExists = System.IO.Directory.Exists(dataPath);
                var sqlitePathExists = System.IO.Directory.Exists(sqlitePath);
                var canWrite = false;

                try
                {
                    var testFile = System.IO.Path.Combine(sqlitePath, "test_write.tmp");
                    await System.IO.File.WriteAllTextAsync(testFile, "test");
                    System.IO.File.Delete(testFile);
                    canWrite = true;
                }
                catch
                {
                    canWrite = false;
                }

                return new
                {
                    healthy = dataPathExists && sqlitePathExists && canWrite,
                    status = canWrite ? "read_write_access" : "read_only_access",
                    dataPath = dataPath,
                    sqlitePath = sqlitePath,
                    dataPathExists = dataPathExists,
                    sqlitePathExists = sqlitePathExists,
                    canWrite = canWrite
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File system access health check failed");
                return new
                {
                    healthy = false,
                    status = "error",
                    error = ex.Message
                };
            }
        }
    }
}