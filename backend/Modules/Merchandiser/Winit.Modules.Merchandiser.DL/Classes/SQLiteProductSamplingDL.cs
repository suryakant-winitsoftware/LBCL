using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.DL.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.DL.Classes
{
    public class SQLiteProductSamplingDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IProductSamplingDL
    {
        public SQLiteProductSamplingDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<IProductSampling> GetByUID(string uid)
        {
            try
            {
                var sql = @"
                    SELECT * FROM product_sampling 
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", uid } };
                return await ExecuteSingleAsync<IProductSampling>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product sampling by UID", ex);
            }
        }

        public async Task<List<IProductSampling>> GetAll()
        {
            try
            {
                var sql = "SELECT * FROM product_sampling";
                return await ExecuteQueryAsync<IProductSampling>(sql, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all product samplings", ex);
            }
        }

        public async Task<bool> Insert(IProductSampling productSampling)
        {
            try
            {
                var sql = @"
                    INSERT INTO product_sampling (
                        id, uid, ss, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time, beat_history_uid, route_uid,
                        job_position_uid, emp_uid, execution_time, store_uid, sku_uid,
                        selling_price, unit_used, unit_sold, no_of_customer_approached
                    ) VALUES (
                        @Id, @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                        @ServerAddTime, @ServerModifiedTime, @BeatHistoryUID, @RouteUID,
                        @JobPositionUID, @EmpUID, @ExecutionTime, @StoreUID, @SKUUID,
                        @SellingPrice, @UnitUsed, @UnitSold, @NoOfCustomerApproached
                    )";

                var parameters = new Dictionary<string, object>
                {
                    { "Id", productSampling.Id },
                    { "UID", productSampling.UID },
                    { "SS", productSampling.SS },
                    { "CreatedBy", productSampling.CreatedBy },
                    { "CreatedTime", productSampling.CreatedTime },
                    { "ModifiedBy", productSampling.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", productSampling.ModifiedTime },
                    { "ServerAddTime", productSampling.ServerAddTime },
                    { "ServerModifiedTime", productSampling.ServerModifiedTime },
                    { "BeatHistoryUID", productSampling.BeatHistoryUID ?? (object)DBNull.Value },
                    { "RouteUID", productSampling.RouteUID },
                    { "JobPositionUID", productSampling.JobPositionUID },
                    { "EmpUID", productSampling.EmpUID },
                    { "ExecutionTime", productSampling.ExecutionTime },
                    { "StoreUID", productSampling.StoreUID },
                    { "SKUUID", productSampling.SKUUID },
                    { "SellingPrice", productSampling.SellingPrice },
                    { "UnitUsed", productSampling.UnitUsed },
                    { "UnitSold", productSampling.UnitSold },
                    { "NoOfCustomerApproached", productSampling.NoOfCustomerApproached }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting product sampling", ex);
            }
        }

        public async Task<bool> Update(IProductSampling productSampling)
        {
            try
            {
                var sql = @"
                    UPDATE product_sampling SET
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
                        selling_price = @SellingPrice,
                        unit_used = @UnitUsed,
                        unit_sold = @UnitSold,
                        no_of_customer_approached = @NoOfCustomerApproached
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", productSampling.UID },
                    { "SS", productSampling.SS },
                    { "ModifiedBy", productSampling.ModifiedBy ?? (object)DBNull.Value },
                    { "ModifiedTime", productSampling.ModifiedTime },
                    { "ServerModifiedTime", productSampling.ServerModifiedTime },
                    { "BeatHistoryUID", productSampling.BeatHistoryUID ?? (object)DBNull.Value },
                    { "RouteUID", productSampling.RouteUID },
                    { "JobPositionUID", productSampling.JobPositionUID },
                    { "EmpUID", productSampling.EmpUID },
                    { "ExecutionTime", productSampling.ExecutionTime },
                    { "StoreUID", productSampling.StoreUID },
                    { "SKUUID", productSampling.SKUUID },
                    { "SellingPrice", productSampling.SellingPrice },
                    { "UnitUsed", productSampling.UnitUsed },
                    { "UnitSold", productSampling.UnitSold },
                    { "NoOfCustomerApproached", productSampling.NoOfCustomerApproached }
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating product sampling", ex);
            }
        }

        public async Task<bool> Delete(string uid)
        {
            try
            {
                var sql = "DELETE FROM product_sampling WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", uid } };
                await ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting product sampling", ex);
            }
        }

        public async Task<List<IProductSampling>> GetByStoreUID(string storeUID)
        {
            try
            {
                var sql = "SELECT * FROM product_sampling WHERE store_uid = @StoreUID";
                var parameters = new Dictionary<string, object> { { "StoreUID", storeUID } };
                return await ExecuteQueryAsync<IProductSampling>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product samplings by store UID", ex);
            }
        }

        public async Task<List<IProductSampling>> GetByRouteUID(string routeUID)
        {
            try
            {
                var sql = "SELECT * FROM product_sampling WHERE route_uid = @RouteUID";
                var parameters = new Dictionary<string, object> { { "RouteUID", routeUID } };
                return await ExecuteQueryAsync<IProductSampling>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product samplings by route UID", ex);
            }
        }

        public async Task<List<IProductSampling>> GetByEmpUID(string empUID)
        {
            try
            {
                var sql = "SELECT * FROM product_sampling WHERE emp_uid = @EmpUID";
                var parameters = new Dictionary<string, object> { { "EmpUID", empUID } };
                return await ExecuteQueryAsync<IProductSampling>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching product samplings by employee UID", ex);
            }
        }
    }
} 