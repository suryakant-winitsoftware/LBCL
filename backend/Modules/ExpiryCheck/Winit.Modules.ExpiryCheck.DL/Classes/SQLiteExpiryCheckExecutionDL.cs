using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.ExpiryCheck.DL.Interfaces;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.DL.Classes
{
    public class SQLiteExpiryCheckExecutionDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IExpiryCheckExecutionDL
    {
        public SQLiteExpiryCheckExecutionDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IExpiryCheckExecution> GetByUIDAsync(string uid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, execution_time
                    FROM expiry_check_execution 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                var header = await ExecuteSingleAsync<IExpiryCheckExecution>(sql, parameters);

                if (header != null)
                {
                    header.Lines = await GetLinesByHeaderUIDAsync(uid);
                }

                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching expiry check execution by UID", ex);
            }
        }

        public async Task<string> CreateAsync(IExpiryCheckExecution expiryCheck)
        {

            using SqliteConnection connection = GetConnection();
            await connection.OpenAsync();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                // Insert header
                var headerSql = @"
                        INSERT INTO expiry_check_execution 
                        (id, uid, ss, created_by, created_time, modified_by, modified_time,
                         server_add_time, server_modified_time, beat_history_uid,
                         store_history_uid, route_uid, store_uid, job_position_uid,
                         emp_uid, execution_time)
                        VALUES 
                        (@Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                         @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID,
                         @StoreHistoryUID, @RouteUID, @StoreUID, @JobPositionUID,
                         @EmpUID, @ExecutionTime)";

                var headerParameters = new Dictionary<string, object>
                    {
                        { "Id", expiryCheck.Id },
                        { "UID", expiryCheck.UID },
                        { "SS", expiryCheck.SS },
                        { "CreatedBy", expiryCheck.CreatedBy },
                        { "CreatedTime", expiryCheck.CreatedTime },
                        { "ModifiedBy", expiryCheck.ModifiedBy },
                        { "ModifiedTime", expiryCheck.ModifiedTime },
                        { "ServerAddTime", expiryCheck.ServerAddTime },
                        { "ServerModifiedTime", expiryCheck.ServerModifiedTime },
                        { "BeatHistoryUID", expiryCheck.BeatHistoryUID },
                        { "StoreHistoryUID", expiryCheck.StoreHistoryUID },
                        { "RouteUID", expiryCheck.RouteUID },
                        { "StoreUID", expiryCheck.StoreUID },
                        { "JobPositionUID", expiryCheck.JobPositionUID },
                        { "EmpUID", expiryCheck.EmpUID },
                        { "ExecutionTime", expiryCheck.ExecutionTime }
                    };

                await ExecuteNonQueryAsync(headerSql, headerParameters, connection, transaction);

                // Insert lines in batch if any exist
                if (expiryCheck.Lines != null && expiryCheck.Lines.Any())
                {
                    var lineSql = @"
                            INSERT INTO expiry_check_execution_line 
                            (id, uid, ss, line_number, sku_uid, qty, expiry_date, expiry_check_execution_uid)
                            VALUES 
                            (@Id, @UID, @SS, @LineNumber, @SKUUID, @Qty, @ExpiryDate, @ExpiryCheckExecutionUID)";

                    var lineParameters = expiryCheck.Lines.Select(line => new Dictionary<string, object>
                        {
                            { "Id", line.Id },
                            { "UID", line.UID },
                            { "SS", line.SS },
                            { "LineNumber", line.LineNumber },
                            { "SKUUID", line.SKUUID },
                            { "Qty", line.Qty },
                            { "ExpiryDate", line.ExpiryDate },
                            { "ExpiryCheckExecutionUID", expiryCheck.UID }
                        }).ToList();

                    await ExecuteNonQueryAsync(lineSql, lineParameters, connection, transaction);
                }

                // Commit transaction only if both operations succeed
                await transaction.CommitAsync();
                return expiryCheck.UID;
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                throw new Exception("Error creating expiry check execution", ex);
            }
        }

        public async Task<bool> UpdateAsync(IExpiryCheckExecution expiryCheck)
        {
            try
            {
                // Update header
                var headerSql = @"
                    UPDATE expiry_check_execution 
                    SET modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        ss = @SS,
                        beat_history_uid = @BeatHistoryUID,
                        store_history_uid = @StoreHistoryUID,
                        route_uid = @RouteUID,
                        store_uid = @StoreUID,
                        job_position_uid = @JobPositionUID,
                        emp_uid = @EmpUID,
                        execution_time = @ExecutionTime
                    WHERE uid = @UID";

                var headerParameters = new Dictionary<string, object>
                {
                    { "UID", expiryCheck.UID },
                    { "SS", expiryCheck.SS },
                    { "ModifiedBy", expiryCheck.ModifiedBy },
                    { "ModifiedTime", expiryCheck.ModifiedTime },
                    { "ServerModifiedTime", expiryCheck.ServerModifiedTime },
                    { "BeatHistoryUID", expiryCheck.BeatHistoryUID },
                    { "StoreHistoryUID", expiryCheck.StoreHistoryUID },
                    { "RouteUID", expiryCheck.RouteUID },
                    { "StoreUID", expiryCheck.StoreUID },
                    { "JobPositionUID", expiryCheck.JobPositionUID },
                    { "EmpUID", expiryCheck.EmpUID },
                    { "ExecutionTime", expiryCheck.ExecutionTime }
                };

                await ExecuteNonQueryAsync(headerSql, headerParameters);

                // Delete existing lines
                var deleteLinesSql = "DELETE FROM expiry_check_execution_line WHERE expiry_check_execution_uid = @HeaderUID";
                await ExecuteNonQueryAsync(deleteLinesSql, new Dictionary<string, object> { { "HeaderUID", expiryCheck.UID } });

                // Insert updated lines
                if (expiryCheck.Lines != null && expiryCheck.Lines.Any())
                {
                    var lineSql = @"
                        INSERT INTO expiry_check_execution_line 
                        (uid, ss, line_number, sku_uid, qty, expiry_date, expiry_check_execution_uid)
                        VALUES 
                        (@UID, @SS, @LineNumber, @SKUUID, @Qty, @ExpiryDate, @ExpiryCheckExecutionUID)";

                    foreach (var line in expiryCheck.Lines)
                    {
                        var lineParameters = new Dictionary<string, object>
                        {
                            { "UID", line.UID },
                            { "SS", line.SS },
                            { "LineNumber", line.LineNumber },
                            { "SKUUID", line.SKUUID },
                            { "Qty", line.Qty },
                            { "ExpiryDate", line.ExpiryDate },
                            { "ExpiryCheckExecutionUID", expiryCheck.UID }
                        };

                        await ExecuteNonQueryAsync(lineSql, lineParameters);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating expiry check execution", ex);
            }
        }

        public async Task<bool> DeleteAsync(string uid)
        {
            try
            {
                // Delete lines first due to foreign key constraint
                var deleteLinesSql = "DELETE FROM expiry_check_execution_line WHERE expiry_check_execution_uid = @UID";
                await ExecuteNonQueryAsync(deleteLinesSql, new Dictionary<string, object> { { "UID", uid } });

                // Delete header
                var deleteHeaderSql = "DELETE FROM expiry_check_execution WHERE uid = @UID";
                await ExecuteNonQueryAsync(deleteHeaderSql, new Dictionary<string, object> { { "UID", uid } });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting expiry check execution", ex);
            }
        }

        public async Task<List<IExpiryCheckExecution>> GetByStoreUIDAsync(string storeUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, execution_time
                    FROM expiry_check_execution 
                    WHERE store_uid = @StoreUID
                    ORDER BY execution_time DESC";

                var parameters = new Dictionary<string, object> { { "StoreUID", storeUid } };
                var headers = await ExecuteQueryAsync<IExpiryCheckExecution>(sql, parameters);

                foreach (var header in headers)
                {
                    header.Lines = await GetLinesByHeaderUIDAsync(header.UID);
                }

                return headers;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching expiry check executions by store UID", ex);
            }
        }

        public async Task<List<IExpiryCheckExecution>> GetByBeatHistoryUIDAsync(string beatHistoryUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, execution_time
                    FROM expiry_check_execution 
                    WHERE beat_history_uid = @BeatHistoryUID
                    ORDER BY execution_time DESC";

                var parameters = new Dictionary<string, object> { { "BeatHistoryUID", beatHistoryUid } };
                var headers = await ExecuteQueryAsync<IExpiryCheckExecution>(sql, parameters);

                foreach (var header in headers)
                {
                    header.Lines = await GetLinesByHeaderUIDAsync(header.UID);
                }

                return headers;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching expiry check executions by beat history UID", ex);
            }
        }

        public async Task<List<IExpiryCheckExecutionLine>> GetLinesByHeaderUIDAsync(string headerUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, line_number, sku_uid, qty, expiry_date, expiry_check_execution_uid
                    FROM expiry_check_execution_line 
                    WHERE expiry_check_execution_uid = @HeaderUID
                    ORDER BY line_number";

                var parameters = new Dictionary<string, object> { { "HeaderUID", headerUid } };
                return await ExecuteQueryAsync<IExpiryCheckExecutionLine>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching expiry check execution lines", ex);
            }
        }
    }
}