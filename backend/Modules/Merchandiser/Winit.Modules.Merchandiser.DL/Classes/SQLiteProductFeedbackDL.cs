using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.Sqlite;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Merchandiser.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Microsoft.Extensions.Configuration;

namespace Winit.Modules.Merchandiser.DL.Classes
{
    public class SQLiteProductFeedbackDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IProductFeedbackDL
    {
        public SQLiteProductFeedbackDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {
        }

        public async Task<IProductFeedback> GetByUID(string uid)
        {
            try
            {
                var sql = @"
                    SELECT * FROM product_feedback 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IProductFeedback>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product feedback by UID", ex);
            }
        }

        public async Task<List<IProductFeedback>> GetAll()
        {
            try
            {
                var sql = "SELECT * FROM product_feedback";
                return await ExecuteQueryAsync<IProductFeedback>(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all product feedback", ex);
            }
        }

        public async Task<bool> Insert(IProductFeedback productFeedback)
        {
            try
            {
                var sql = @"
                    INSERT INTO product_feedback (
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid, route_uid,
                        job_position_uid, emp_uid, execution_time, store_uid, sku_uid,
                        feedback_on, end_customer_name, mobile_no
                    ) VALUES (
                        @Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                        @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID, @RouteUID,
                        @JobPositionUID, @EmpUID, @ExecutionTime, @StoreUID, @SKUUID,
                        @FeedbackOn, @EndCustomerName, @MobileNo
                    )";

                var parameters = new Dictionary<string, object>
                {
                    { "Id", productFeedback.Id },
                    { "UID", productFeedback.UID },
                    { "SS", productFeedback.SS },
                    { "CreatedBy", productFeedback.CreatedBy },
                    { "CreatedTime", productFeedback.CreatedTime },
                    { "ModifiedBy", productFeedback.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", productFeedback.ModifiedTime },
                    { "ServerAddTime", productFeedback.ServerAddTime },
                    { "ServerModifiedTime", productFeedback.ServerModifiedTime },
                    { "BeatHistoryUID", productFeedback.BeatHistoryUID ?? (object)DBNull.Value },
                    { "RouteUID", productFeedback.RouteUID },
                    { "JobPositionUID", productFeedback.JobPositionUID },
                    { "EmpUID", productFeedback.EmpUID },
                    { "ExecutionTime", productFeedback.ExecutionTime },
                    { "StoreUID", productFeedback.StoreUID },
                    { "SKUUID", productFeedback.SKUUID },
                    { "FeedbackOn", productFeedback.FeedbackOn ?? (object)DBNull.Value },
                    { "EndCustomerName", productFeedback.EndCustomerName ?? (object)DBNull.Value },
                    { "MobileNo", productFeedback.MobileNo ?? (object)DBNull.Value }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting product feedback", ex);
            }
        }

        public async Task<bool> Update(IProductFeedback productFeedback)
        {
            try
            {
                var sql = @"
                    UPDATE product_feedback SET
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
                        feedback_on = @FeedbackOn,
                        end_customer_name = @EndCustomerName,
                        mobile_no = @MobileNo
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", productFeedback.UID },
                    { "SS", productFeedback.SS },
                    { "ModifiedBy", productFeedback.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", productFeedback.ModifiedTime },
                    { "ServerModifiedTime", productFeedback.ServerModifiedTime },
                    { "BeatHistoryUID", productFeedback.BeatHistoryUID ?? (object)DBNull.Value },
                    { "RouteUID", productFeedback.RouteUID },
                    { "JobPositionUID", productFeedback.JobPositionUID },
                    { "EmpUID", productFeedback.EmpUID },
                    { "ExecutionTime", productFeedback.ExecutionTime },
                    { "StoreUID", productFeedback.StoreUID },
                    { "SKUUID", productFeedback.SKUUID },
                    { "FeedbackOn", productFeedback.FeedbackOn ?? (object)DBNull.Value },
                    { "EndCustomerName", productFeedback.EndCustomerName ?? (object)DBNull.Value },
                    { "MobileNo", productFeedback.MobileNo ?? (object)DBNull.Value }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating product feedback", ex);
            }
        }

        public async Task<bool> Delete(string uid)
        {
            try
            {
                var sql = "DELETE FROM product_feedback WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting product feedback", ex);
            }
        }

        public async Task<List<IProductFeedback>> GetByStoreUID(string storeUID)
        {
            try
            {
                var sql = "SELECT * FROM product_feedback WHERE store_uid = @StoreUID";
                var parameters = new Dictionary<string, object> { { "StoreUID", storeUID } };
                return await ExecuteQueryAsync<IProductFeedback>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product feedback by store UID", ex);
            }
        }

        public async Task<List<IProductFeedback>> GetByRouteUID(string routeUID)
        {
            try
            {
                var sql = "SELECT * FROM product_feedback WHERE route_uid = @RouteUID";
                var parameters = new Dictionary<string, object> { { "RouteUID", routeUID } };
                return await ExecuteQueryAsync<IProductFeedback>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product feedback by route UID", ex);
            }
        }

        public async Task<List<IProductFeedback>> GetByEmpUID(string empUID)
        {
            try
            {
                var sql = "SELECT * FROM product_feedback WHERE emp_uid = @EmpUID";
                var parameters = new Dictionary<string, object> { { "EmpUID", empUID } };
                return await ExecuteQueryAsync<IProductFeedback>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product feedback by employee UID", ex);
            }
        }
    }
} 