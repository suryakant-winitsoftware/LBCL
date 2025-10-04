using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.DL.Classes
{
    public class SQLitePlanogramV1DL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IPlanogramV1DL
    {
        public SQLitePlanogramV1DL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {
        }

        public async Task<IPlanogramSetupV1> GetPlanogramSetupV1ByUIDAsync(string uid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, ss, description, image_url
                    FROM planogram_setup_v1 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IPlanogramSetupV1>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching planogram setup v1 by UID", ex);
            }
        }

        public async Task<string> CreatePlanogramSetupV1Async(IPlanogramSetupV1 setup)
        {
            try
            {
                var sql = @"
                    INSERT INTO planogram_setup_v1 
                    (uid, created_by, created_time, modified_by, modified_time,
                     server_add_time, server_modified_time, ss, description, image_url)
                    VALUES 
                    (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                     @ServerAddTime, @ServerModifiedTime, @SS, @Description, @ImageUrl)";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", setup.UID },
                    { "CreatedBy", setup.CreatedBy },
                    { "CreatedTime", setup.CreatedTime },
                    { "ModifiedBy", setup.ModifiedBy },
                    { "ModifiedTime", setup.ModifiedTime },
                    { "ServerAddTime", setup.ServerAddTime },
                    { "ServerModifiedTime", setup.ServerModifiedTime },
                    { "SS", setup.SS },
                    { "Description", setup.Description },
                    { "ImageUrl", setup.ImageUrl }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return setup.UID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating planogram setup v1", ex);
            }
        }

        public async Task<bool> UpdatePlanogramSetupV1Async(IPlanogramSetupV1 setup)
        {
            try
            {
                var sql = @"
                    UPDATE planogram_setup_v1 
                    SET modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        ss = @SS,
                        description = @Description,
                        image_url = @ImageUrl
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", setup.UID },
                    { "ModifiedBy", setup.ModifiedBy },
                    { "ModifiedTime", setup.ModifiedTime },
                    { "ServerModifiedTime", setup.ServerModifiedTime },
                    { "SS", setup.SS },
                    { "Description", setup.Description },
                    { "ImageUrl", setup.ImageUrl }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating planogram setup v1", ex);
            }
        }

        public async Task<bool> DeletePlanogramSetupV1Async(string uid)
        {
            try
            {
                var sql = "DELETE FROM planogram_setup_v1 WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting planogram setup v1", ex);
            }
        }

        public async Task<List<IPlanogramSetupV1>> GetAllPlanogramSetupV1Async()
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, ss, description, image_url
                    FROM planogram_setup_v1 
                    ORDER BY created_time DESC";

                return await ExecuteQueryAsync<IPlanogramSetupV1>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all planogram setup v1", ex);
            }
        }

        public async Task<IPlanogramExecutionV1> GetPlanogramExecutionV1ByUIDAsync(string uid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, screen_name, planogram_setup_v1_uid, execution_time
                    FROM planogram_execution_v1 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IPlanogramExecutionV1>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching planogram execution v1 by UID", ex);
            }
        }

        public async Task<string> CreatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution)
        {
            try
            {
                var sql = @"
                    INSERT INTO planogram_execution_v1 
                    (id, uid, ss, created_by, created_time, modified_by, modified_time,
                     server_add_time, server_modified_time, beat_history_uid,
                     store_history_uid, route_uid, store_uid, job_position_uid,
                     emp_uid, screen_name, planogram_setup_v1_uid, execution_time)
                    VALUES 
                    (@Id,@UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                     @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID,
                     @StoreHistoryUID, @RouteUID, @StoreUID, @JobPositionUID,
                     @EmpUID, @ScreenName, @PlanogramSetupV1UID, @ExecutionTime)";

                var parameters = new Dictionary<string, object>
                {
                    { "Id", execution.Id },
                    { "UID", execution.UID },
                    { "SS", execution.SS ?? 0 },
                    { "CreatedBy", execution.CreatedBy },
                    { "CreatedTime", execution.CreatedTime },
                    { "ModifiedBy", execution.ModifiedBy },
                    { "ModifiedTime", execution.ModifiedTime },
                    { "ServerAddTime", execution.ServerAddTime },
                    { "ServerModifiedTime", execution.ServerModifiedTime },
                    { "BeatHistoryUID", execution.BeatHistoryUID },
                    { "StoreHistoryUID", execution.StoreHistoryUID },
                    { "RouteUID", execution.RouteUID },
                    { "StoreUID", execution.StoreUID },
                    { "JobPositionUID", execution.JobPositionUID },
                    { "EmpUID", execution.EmpUID },
                    { "ScreenName", execution.ScreenName },
                    { "PlanogramSetupV1UID", execution.PlanogramSetupV1UID },
                    { "ExecutionTime", execution.ExecutionTime }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return execution.UID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating planogram execution v1", ex);
            }
        }

        public async Task<bool> UpdatePlanogramExecutionV1Async(IPlanogramExecutionV1 execution)
        {
            try
            {
                var sql = @"
                    UPDATE planogram_execution_v1 
                    SET modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = @ServerModifiedTime,
                        beat_history_uid = @BeatHistoryUID,
                        store_history_uid = @StoreHistoryUID,
                        route_uid = @RouteUID,
                        store_uid = @StoreUID,
                        job_position_uid = @JobPositionUID,
                        emp_uid = @EmpUID,
                        screen_name = @ScreenName,
                        planogram_setup_v1_uid = @PlanogramSetupV1UID,
                        execution_time = @ExecutionTime
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", execution.UID },
                    { "ModifiedBy", execution.ModifiedBy },
                    { "ModifiedTime", execution.ModifiedTime },
                    { "ServerModifiedTime", execution.ServerModifiedTime },
                    { "BeatHistoryUID", execution.BeatHistoryUID },
                    { "StoreHistoryUID", execution.StoreHistoryUID },
                    { "RouteUID", execution.RouteUID },
                    { "StoreUID", execution.StoreUID },
                    { "JobPositionUID", execution.JobPositionUID },
                    { "EmpUID", execution.EmpUID },
                    { "ScreenName", execution.ScreenName },
                    { "PlanogramSetupV1UID", execution.PlanogramSetupV1UID },
                    { "ExecutionTime", execution.ExecutionTime }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating planogram execution v1", ex);
            }
        }

        public async Task<bool> DeletePlanogramExecutionV1Async(string uid)
        {
            try
            {
                var sql = "DELETE FROM planogram_execution_v1 WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting planogram execution v1", ex);
            }
        }

        public async Task<List<IPlanogramExecutionV1>> GetPlanogramExecutionV1ByStoreUIDAsync(string storeUid)
        {
            try
            {
                var sql = @"
                    SELECT 
                        id, uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid,
                        store_history_uid, route_uid, store_uid, job_position_uid,
                        emp_uid, screen_name, planogram_setup_v1_uid, execution_time
                    FROM planogram_execution_v1 
                    WHERE store_uid = @StoreUID
                    ORDER BY execution_time DESC";

                var parameters = new Dictionary<string, object> { { "StoreUID", storeUid } };
                return await ExecuteQueryAsync<IPlanogramExecutionV1>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching planogram executions v1 by store UID", ex);
            }
        }
    }
} 