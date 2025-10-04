using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Classes
{
    public class SQLiteBroadcastInitiativeDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IBroadcastInitiativeDL
    {
        public SQLiteBroadcastInitiativeDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IBroadcastInitiative> GetByUID(string uid)
        {
            try
            {
                var sql = @"
                    SELECT * FROM broadcast_initiative 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IBroadcastInitiative>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching broadcast initiative by UID", ex);
            }
        }

        public async Task<List<IBroadcastInitiative>> GetAll()
        {
            try
            {
                var sql = "SELECT * FROM broadcast_initiative";
                return await ExecuteQueryAsync<IBroadcastInitiative>(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all broadcast initiatives", ex);
            }
        }

        public async Task<bool> Insert(IBroadcastInitiative broadcastInitiative)
        {
            try
            {
                var sql = @"
                    INSERT INTO broadcast_initiative (
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid, route_uid,
                        job_position_uid, emp_uid, execution_time, store_uid, sku_uid,
                        gender, end_customer_name, mobile_no, ftb_rc
                    ) VALUES (
                        @Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                        @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID, @RouteUID,
                        @JobPositionUID, @EmpUID, @ExecutionTime, @StoreUID, @SKUUID,
                        @Gender, @EndCustomerName, @MobileNo, @FtbRc
                    )";

                var parameters = new Dictionary<string, object>
                {
                    { "Id", broadcastInitiative.Id },
                    { "UID", broadcastInitiative.UID },
                    { "SS", broadcastInitiative.SS },
                    { "CreatedBy", broadcastInitiative.CreatedBy },
                    { "CreatedTime", broadcastInitiative.CreatedTime },
                    { "ModifiedBy", broadcastInitiative.ModifiedBy ?? null},
                    { "ModifiedTime", broadcastInitiative.ModifiedTime },
                    { "ServerAddTime", broadcastInitiative.ServerAddTime },
                    { "ServerModifiedTime", broadcastInitiative.ServerModifiedTime },
                    { "BeatHistoryUID", broadcastInitiative.BeatHistoryUID ?? null },
                    { "RouteUID", broadcastInitiative.RouteUID },
                    { "JobPositionUID", broadcastInitiative.JobPositionUID },
                    { "EmpUID", broadcastInitiative.EmpUID },
                    { "ExecutionTime", broadcastInitiative.ExecutionTime },
                    { "StoreUID", broadcastInitiative.StoreUID },
                    { "SKUUID", broadcastInitiative.SKUUID },
                    { "Gender", broadcastInitiative.Gender ?? null },
                    { "EndCustomerName", broadcastInitiative.EndCustomerName ?? null },
                    { "MobileNo", broadcastInitiative.MobileNo ?? null },
                    { "FtbRc", broadcastInitiative.FtbRc ?? null }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting broadcast initiative", ex);
            }
        }

        public async Task<bool> Update(IBroadcastInitiative broadcastInitiative)
        {
            try
            {
                var sql = @"
                    UPDATE broadcast_initiative SET
                        ss = @SS,
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        beat_history_uid = @BeatHistoryUID,
                        route_uid = @RouteUID,
                        job_position_uid = @JobPositionUID,
                        emp_uid = @EmpUID,
                        execution_time = @ExecutionTime,
                        store_uid = @StoreUID,
                        sku_uid = @SKUUID,
                        gender = @Gender,
                        end_customer_name = @EndCustomerName,
                        mobile_no = @MobileNo,
                        ftb_rc = @FtbRc
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", broadcastInitiative.UID },
                    { "SS", broadcastInitiative.SS },
                    { "ModifiedBy", broadcastInitiative.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", broadcastInitiative.ModifiedTime },
                    { "ServerModifiedTime", broadcastInitiative.ServerModifiedTime },
                    { "BeatHistoryUID", broadcastInitiative.BeatHistoryUID ?? (object)DBNull.Value },
                    { "RouteUID", broadcastInitiative.RouteUID },
                    { "JobPositionUID", broadcastInitiative.JobPositionUID },
                    { "EmpUID", broadcastInitiative.EmpUID },
                    { "ExecutionTime", broadcastInitiative.ExecutionTime },
                    { "StoreUID", broadcastInitiative.StoreUID },
                    { "SKUUID", broadcastInitiative.SKUUID },
                    { "Gender", broadcastInitiative.Gender ?? (object)DBNull.Value },
                    { "EndCustomerName", broadcastInitiative.EndCustomerName ?? (object)DBNull.Value },
                    { "MobileNo", broadcastInitiative.MobileNo ?? (object)DBNull.Value },
                    { "FtbRc", broadcastInitiative.FtbRc ?? (object)DBNull.Value }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating broadcast initiative", ex);
            }
        }

        public async Task<bool> Delete(string uid)
        {
            try
            {
                var sql = "DELETE FROM broadcast_initiative WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting broadcast initiative", ex);
            }
        }

        public async Task<List<IBroadcastInitiative>> GetByStoreUID(string storeUID)
        {
            try
            {
                var sql = "SELECT * FROM broadcast_initiative WHERE store_uid = @StoreUID";
                var parameters = new Dictionary<string, object> { { "StoreUID", storeUID } };
                return await ExecuteQueryAsync<IBroadcastInitiative>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching broadcast initiatives by store UID", ex);
            }
        }

        public async Task<List<IBroadcastInitiative>> GetByRouteUID(string routeUID)
        {
            try
            {
                var sql = "SELECT * FROM broadcast_initiative WHERE route_uid = @RouteUID";
                var parameters = new Dictionary<string, object> { { "RouteUID", routeUID } };
                return await ExecuteQueryAsync<IBroadcastInitiative>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching broadcast initiatives by route UID", ex);
            }
        }

        public async Task<List<IBroadcastInitiative>> GetByEmpUID(string empUID)
        {
            try
            {
                var sql = "SELECT * FROM broadcast_initiative WHERE emp_uid = @EmpUID";
                var parameters = new Dictionary<string, object> { { "EmpUID", empUID } };
                return await ExecuteQueryAsync<IBroadcastInitiative>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching broadcast initiatives by employee UID", ex);
            }
        }
    }
} 