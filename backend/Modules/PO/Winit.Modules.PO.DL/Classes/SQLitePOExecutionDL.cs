using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.PO.DL.Interfaces;
using Winit.Modules.PO.Model.Classes;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.DL.Classes
{
    public class SQLitePOExecutionDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IPOExecutionDL
    {
        public SQLitePOExecutionDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IPOExecution> GetByUIDAsync(string uid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, execution_time, po_number, line_count, qty_count, total_amount
                    FROM po_execution 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                var header = await ExecuteSingleAsync<IPOExecution>(sql, parameters);

                if (header != null)
                {
                    header.Lines = await GetLinesByHeaderUIDAsync(uid);
                }

                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO execution by UID", ex);
            }
        }

        public async Task<string> CreateAsync(IPOExecution poExecution)
        {
            using SqliteConnection connection = GetConnection();
            await connection.OpenAsync();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                // Insert header
                var headerSql = @"
                    INSERT INTO po_execution 
                    (id, uid, ss, created_by, created_time, modified_by, modified_time,
                     server_add_time, server_modified_time, beat_history_uid,
                     store_history_uid, route_uid, store_uid, job_position_uid,
                     emp_uid, execution_time, po_number, line_count, qty_count, total_amount)
                    VALUES 
                    (@Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                     @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID,
                     @StoreHistoryUID, @RouteUID, @StoreUID, @JobPositionUID,
                     @EmpUID, @ExecutionTime, @PONumber, @LineCount, @QtyCount, @TotalAmount)";

                var headerParameters = new Dictionary<string, object>
                {
                    { "Id", poExecution.Id },
                    { "UID", poExecution.UID },
                    { "SS", poExecution.SS },
                    { "CreatedBy", poExecution.CreatedBy },
                    { "CreatedTime", poExecution.CreatedTime },
                    { "ModifiedBy", poExecution.ModifiedBy },
                    { "ModifiedTime", poExecution.ModifiedTime },
                    { "ServerAddTime", poExecution.ServerAddTime },
                    { "ServerModifiedTime", poExecution.ServerModifiedTime },
                    { "BeatHistoryUID", poExecution.BeatHistoryUID },
                    { "StoreHistoryUID", poExecution.StoreHistoryUID },
                    { "RouteUID", poExecution.RouteUID },
                    { "StoreUID", poExecution.StoreUID },
                    { "JobPositionUID", poExecution.JobPositionUID },
                    { "EmpUID", poExecution.EmpUID },
                    { "ExecutionTime", poExecution.ExecutionTime },
                    { "PONumber", poExecution.PONumber },
                    { "LineCount", poExecution.LineCount },
                    { "QtyCount", poExecution.QtyCount },
                    { "TotalAmount", poExecution.TotalAmount }
                };

                await ExecuteNonQueryAsync(headerSql, headerParameters, connection, transaction);

                // Insert lines in batch if any exist
                if (poExecution.Lines != null && poExecution.Lines.Any())
                {
                    var lineSql = @"
                        INSERT INTO po_execution_line 
                        (id, uid, ss, line_number, sku_uid, qty, price, total_amount, po_execution_uid)
                        VALUES 
                        (@Id, @UID, @SS, @LineNumber, @SKUUID, @Qty, @Price, @TotalAmount, @POExecutionUID)";

                    var lineParameters = poExecution.Lines.Select(line => new Dictionary<string, object>
                    {
                        { "Id", line.Id },
                        { "UID", line.UID },
                        { "SS", line.SS },
                        { "LineNumber", line.LineNumber },
                        { "SKUUID", line.SKUUID },
                        { "Qty", line.Qty },
                        { "Price", line.Price },
                        { "TotalAmount", line.TotalAmount },
                        { "POExecutionUID", poExecution.UID }
                    }).ToList();

                    await ExecuteNonQueryAsync(lineSql, lineParameters, connection, transaction);
                }

                await transaction.CommitAsync();
                return poExecution.UID;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error creating PO execution", ex);
            }
        }

        public async Task<bool> UpdateAsync(IPOExecution poExecution)
        {
            using SqliteConnection connection = GetConnection();
            await connection.OpenAsync();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                // Update header
                var headerSql = @"
                    UPDATE po_execution 
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
                        execution_time = @ExecutionTime,
                        po_number = @PONumber,
                        line_count = @LineCount,
                        qty_count = @QtyCount,
                        total_amount = @TotalAmount
                    WHERE uid = @UID";

                var headerParameters = new Dictionary<string, object>
                {
                    { "UID", poExecution.UID },
                    { "SS", poExecution.SS },
                    { "ModifiedBy", poExecution.ModifiedBy },
                    { "ModifiedTime", poExecution.ModifiedTime },
                    { "ServerModifiedTime", poExecution.ServerModifiedTime },
                    { "BeatHistoryUID", poExecution.BeatHistoryUID },
                    { "StoreHistoryUID", poExecution.StoreHistoryUID },
                    { "RouteUID", poExecution.RouteUID },
                    { "StoreUID", poExecution.StoreUID },
                    { "JobPositionUID", poExecution.JobPositionUID },
                    { "EmpUID", poExecution.EmpUID },
                    { "ExecutionTime", poExecution.ExecutionTime },
                    { "PONumber", poExecution.PONumber },
                    { "LineCount", poExecution.LineCount },
                    { "QtyCount", poExecution.QtyCount },
                    { "TotalAmount", poExecution.TotalAmount }
                };

                await ExecuteNonQueryAsync(headerSql, headerParameters, connection, transaction);

                // Delete existing lines
                var deleteLinesSql = "DELETE FROM po_execution_line WHERE po_execution_uid = @HeaderUID";
                await ExecuteNonQueryAsync(deleteLinesSql, new Dictionary<string, object> { { "HeaderUID", poExecution.UID } }, connection, transaction);

                // Insert updated lines
                if (poExecution.Lines != null && poExecution.Lines.Any())
                {
                    var lineSql = @"
                        INSERT INTO po_execution_line 
                        (id, uid, ss, line_number, sku_uid, qty, price, total_amount, po_execution_uid)
                        VALUES 
                        (@Id, @UID, @SS, @LineNumber, @SKUUID, @Qty, @Price, @TotalAmount, @POExecutionUID)";

                    var lineParameters = poExecution.Lines.Select(line => new Dictionary<string, object>
                    {
                        { "Id", line.Id },
                        { "UID", line.UID },
                        { "SS", line.SS },
                        { "LineNumber", line.LineNumber },
                        { "SKUUID", line.SKUUID },
                        { "Qty", line.Qty },
                        { "Price", line.Price },
                        { "TotalAmount", line.TotalAmount },
                        { "POExecutionUID", poExecution.UID }
                    }).ToList();

                    await ExecuteNonQueryAsync(lineSql, lineParameters, connection, transaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error updating PO execution", ex);
            }
        }

        public async Task<bool> DeleteAsync(string uid)
        {
            using SqliteConnection connection = GetConnection();
            await connection.OpenAsync();
            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                // Delete lines first due to foreign key constraint
                var deleteLinesSql = "DELETE FROM po_execution_line WHERE po_execution_uid = @UID";
                await ExecuteNonQueryAsync(deleteLinesSql, new Dictionary<string, object> { { "UID", uid } }, connection, transaction);

                // Delete header
                var deleteHeaderSql = "DELETE FROM po_execution WHERE uid = @UID";
                await ExecuteNonQueryAsync(deleteHeaderSql, new Dictionary<string, object> { { "UID", uid } }, connection, transaction);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Error deleting PO execution", ex);
            }
        }

        public async Task<List<IPOExecution>> GetByStoreUIDAsync(string storeUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, execution_time, po_number, line_count, qty_count, total_amount
                    FROM po_execution 
                    WHERE store_uid = @StoreUID
                    ORDER BY execution_time DESC";

                var parameters = new Dictionary<string, object> { { "StoreUID", storeUid } };
                var headers = await ExecuteQueryAsync<IPOExecution>(sql, parameters);

                foreach (var header in headers)
                {
                    header.Lines = await GetLinesByHeaderUIDAsync(header.UID);
                }

                return headers;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO executions by store UID", ex);
            }
        }

        private async Task<List<IPOExecutionLine>> GetLinesByHeaderUIDAsync(string headerUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, ss, line_number, sku_uid, qty, price, total_amount, po_execution_uid
                    FROM po_execution_line 
                    WHERE po_execution_uid = @HeaderUID
                    ORDER BY line_number";

                var parameters = new Dictionary<string, object> { { "HeaderUID", headerUid } };
                return await ExecuteQueryAsync<IPOExecutionLine>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO execution lines", ex);
            }
        }
    }
}