using Microsoft.Data.Sqlite;
using Nest;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.StockUpdater.DL.Interfaces;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockUpdater.DL.Classes
{
    public class SQLiteStockUpdaterDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IStockUpdaterDL
    {
        public SQLiteStockUpdaterDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<int> UpdateStockAsync(List<IWHStockLedger> stockLedgers, IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            if (stockLedgers == null || stockLedgers.Count == 0)
            {
                return 0;
            }
            int retValue = -1;
            if (connection == null)
            {
                using (connection = SqliteConnection())
                {
                    await ((SqliteConnection)connection).OpenAsync();

                    using (transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (IWHStockLedger wHStockLedger in stockLedgers)
                            {
                                int noOfRecord = await CreateWHStockLedger(wHStockLedger, connection, transaction);
                            }
                            retValue = 1;
                            await ((SqliteTransaction)transaction).CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                // Rollback the transaction if it hasn't been completed
                                if (transaction?.Connection != null)
                                {
                                    transaction?.Rollback();
                                }
                            }
                            catch (Exception rollbackEx)
                            {
                                // Log or handle the rollback exception
                                Console.WriteLine($"Rollback exception: {rollbackEx.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            else
            {
                try
                {
                    foreach (IWHStockLedger wHStockLedger in stockLedgers)
                    {
                        int noOfRecord = await CreateWHStockLedger(wHStockLedger, connection, transaction);
                    }
                    retValue = 1;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return retValue;
        }
        private async Task<int> CreateWHStockLedger(IWHStockLedger wHStockLedger,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retValue = -1;
            try
            {
                if (wHStockLedger == null)
                {
                    return retValue;
                }
                // Code for inserting data into the sales_order_info table
                var salesOrderQuery = @"
                                INSERT INTO wh_stock_ledger (
                                    id,uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, warehouse_uid,
                                    org_uid, sku_uid, sku_code, type, reference_type, reference_uid, batch_number, expiry_date, qty, uom, stock_type, serial_no,
                                    version_no, parent_wh_uid, year_month
                                ) VALUES (
                                    0, @uid, 1, @createdBy, @createdTime, @modifiedBy, @modifiedTime, @serverAddTime, @serverModifiedTime, @companyUid, @warehouseUid,
                                    @orgUid, @skuUid, @skuCode, @type, @referenceType, @referenceUid, @batchNumber, @expiryDate, @qty, @uom, @stockType, @serialNo,
                                    @versionNo, @parentWhUid, @yearMonth
                                );";

                var parameters = new Dictionary<string, object?>
                            {
                                { "@uid", wHStockLedger.UID },
                                { "@createdBy", wHStockLedger.CreatedBy },
                                { "@createdTime", wHStockLedger.CreatedTime },
                                { "@modifiedBy", wHStockLedger.ModifiedBy },
                                { "@modifiedTime", wHStockLedger.ModifiedTime },
                                { "@serverAddTime", wHStockLedger.ServerAddTime },
                                { "@serverModifiedTime", wHStockLedger.ServerModifiedTime },
                                { "@companyUid", wHStockLedger.CompanyUID },
                                { "@warehouseUid", wHStockLedger.WarehouseUID },
                                { "@orgUid", wHStockLedger.OrgUID },
                                { "@skuUid", wHStockLedger.SKUUID },
                                { "@skuCode", wHStockLedger.SKUCode },
                                { "@type", wHStockLedger.Type },
                                { "@referenceType", wHStockLedger.ReferenceType },
                                { "@referenceUid", wHStockLedger.ReferenceUID },
                                { "@batchNumber", wHStockLedger.BatchNumber },
                                { "@expiryDate", wHStockLedger.ExpiryDate },
                                { "@qty", wHStockLedger.Qty },
                                { "@uom", wHStockLedger.UOM },
                                { "@stockType", wHStockLedger.StockType },
                                { "@serialNo", wHStockLedger.SerialNo },
                                { "@versionNo", wHStockLedger.VersionNo },
                                { "@parentWhUid", wHStockLedger.ParentWhUID },
                                { "@yearMonth", wHStockLedger.YearMonth }
                            };
                retValue = await ExecuteNonQueryAsync(salesOrderQuery, parameters, connection, transaction);
                if (retValue > 0)
                {
                    await CreateWHStockAvailableInDb(wHStockLedger.WarehouseUID, wHStockLedger.OrgUID, wHStockLedger.SKUUID, wHStockLedger.Type,
                        wHStockLedger.BatchNumber, wHStockLedger.ExpiryDate, wHStockLedger.Qty, wHStockLedger.UOM, wHStockLedger.StockType,
                        wHStockLedger.SerialNo, wHStockLedger.VersionNo, wHStockLedger.CreatedBy, wHStockLedger.UID,
                        wHStockLedger.YearMonth, connection, transaction);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return retValue;
        }
        private async Task CreateWHStockAvailableInDb(string wareHouseUID, string orgUID, string itemUID, int type,
            string batchNumber, DateTime? expiryDate, decimal qty, string uOM,
            string stockType, string? serialNo, string versionNo, string? createdBy, string lastWHStockLedgerUID, int yearMonth,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int wHStockAvailableStatus = 0;
            Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable? wHStockAvailable = await SelectWHStockAvailableIfExists(wareHouseUID,
                orgUID, itemUID, type, batchNumber, expiryDate, uOM, stockType, serialNo, versionNo, connection, transaction);
            if (wHStockAvailable != null)
            {
                wHStockAvailableStatus = await UpdateWHStockAvailableInDb(wHStockAvailable, type, qty, createdBy, lastWHStockLedgerUID, connection, transaction);
            }
            else
            {
                wHStockAvailableStatus = await InsertWHStockAvailableInDb(wareHouseUID, orgUID, itemUID, type, batchNumber, expiryDate,
                    qty, uOM, stockType, serialNo, versionNo, createdBy, lastWHStockLedgerUID, yearMonth, connection, transaction);
            }
        }
        private async Task<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable?> SelectWHStockAvailableIfExists(string wareHouseUID, string orgUID, string itemUID, int type,
            string batchNumber, DateTime? expiryDate, string uOM, string stockType, string? serialNo, string VersionNo,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable? wHStockAvailable = null;
            string expiryDateQuery = string.Empty;
            if (expiryDate == null)
            {
                expiryDateQuery = "AND expiry_date is null";
            }
            else
            {
                expiryDateQuery = "AND expiry_date = @ExpiryDate";
            }
            string query = string.Format($@"SELECT uid AS UID, qty AS Qty 
                                    FROM 
                                    wh_stock_available WHERE warehouse_uid = @WarehouseUID AND org_uid = @OrgUID 
                                    AND sku_uid = @SKUUID AND batch_number = @BatchNumber 
                                    AND uom = @UOM 
                                    AND stock_type = @StockType 
                                    AND COALESCE(serial_no,'') = COALESCE(@SerialNo, '') 
                                    {expiryDateQuery}
                                    AND version_no = @VersionNo");
            try
            {
                var parameters = new Dictionary<string, object?>
                            {
                                { "@WarehouseUID", wareHouseUID },
                                { "@OrgUID", orgUID },
                                { "@SKUUID", itemUID },
                                { "@BatchNumber", batchNumber },
                                { "@UOM", uOM },
                                { "@StockType", stockType },
                                { "@SerialNo", serialNo },
                                { "@ExpiryDate", expiryDate },
                                { "@VersionNo", VersionNo }
                            };
                wHStockAvailable = await ExecuteSingleAsync<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable>(query, parameters, null,
                    connection, transaction);
            }
            catch (Exception ex)
            {
                throw;
            }
            return wHStockAvailable;
        }
        private async Task<int> UpdateWHStockAvailableInDb(Winit.Modules.StockUpdater.Model.Interfaces.IWHStockAvailable wHStockAvailable,
            int type, decimal qty, string? createdBy, string lastWHStockLedgerUID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int returnValue = 0;
            decimal finalQty = wHStockAvailable.Qty + (qty * type);
            string query = string.Format(@"UPDATE wh_stock_available SET qty = @Qty, 
                                        modified_by = @ModifiedBy, modified_time = @ModifiedTime, last_wh_stock_ledger_uid = @LastWHStockLedgerUID
                                        WHERE uid = @UID");

            var parameters = new Dictionary<string, object?>
                            {
                                { "@Qty", finalQty },
                                { "@ModifiedBy", createdBy},
                                { "@ModifiedTime", DateTime.Now},
                                { "@LastWHStockLedgerUID", lastWHStockLedgerUID},
                                { "@UID", wHStockAvailable.UID }
                            };

            try
            {
                returnValue = await ExecuteNonQueryAsync(query, parameters, connection, transaction);
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnValue;
        }
        private async Task<int> InsertWHStockAvailableInDb(string wareHouseUID, string orgUID, string itemUID, int type,
            string batchNumber, DateTime? expiryDate, decimal qty, string uOM,
            string stockType, string? serialNo, string versionNo, string? createdBy, string lastWHStockLedgerUID, int yearMonth,
            IDbConnection?  connection = null, IDbTransaction? transaction = null)
        {
            int returnValue = 0;

            string query = string.Format(@"INSERT INTO wh_stock_available (id,uid, ss, created_by, created_time, modified_by, 
                                        modified_time, server_add_time, server_modified_time, company_uid, 
                                        org_uid, warehouse_uid, sku_uid, batch_number, expiry_date, qty, uom, stock_type, 
                                        serial_no, version_no, year_month, last_wh_stock_ledger_uid) 
                VALUES(0,@UID, 0, @CreatedBy, @CreatedTime, @ModifiedBy, 
                                    @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, 
                                    @OrgUID,@WarehouseUID,@SKUUID,@BatchNumber,@ExpiryDate,@Qty,@UOM,
                                    @StockType,@SerialNo,@VersionNo, @YearMonth, @LastWHStockLedgerUID)");
            // Here ss = 0 because we are not sending wh_stock_available to server

            var parameters = new Dictionary<string, object?>
                            {
                                { "@UID", Guid.NewGuid() },
                                { "@CreatedBy", createdBy },
                                { "@CreatedTime", DateTime.Now },
                                { "@ModifiedBy", createdBy },
                                { "@ModifiedTime", DateTime.Now },
                                { "@ServerAddTime", DateTime.Now },
                                { "@ServerModifiedTime", DateTime.Now },
                                { "@CompanyUID", null },
                                { "@OrgUID", orgUID },
                                { "@WarehouseUID", wareHouseUID },
                                { "@SKUUID", itemUID },
                                { "@BatchNumber", batchNumber },
                                { "@ExpiryDate", expiryDate },
                                { "@Qty", type * qty },
                                { "@UOM", uOM },
                                { "@StockType", stockType },
                                { "@SerialNo", serialNo },
                                { "@VersionNo", versionNo },
                                { "@YearMonth", yearMonth },
                                { "@LastWHStockLedgerUID", lastWHStockLedgerUID}
                            };

            try
            {
                returnValue = await ExecuteNonQueryAsync(query, parameters, connection, transaction);
            }
            catch (Exception ex)
            {
                throw;
            }
            return returnValue;
        }

        public async Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>?> GetWHStockSummary(string orgUID, string wareHouseUID)
        {
            if (wareHouseUID == null)
            {
                wareHouseUID = "";
            }
            List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>? wHStockSummaries = null;

            string query = string.Format(@"SELECT 
	                            WSA.org_uid AS OrgUID,
	                            WSA.warehouse_uid AS WarehouseUID,
	                            O.code AS WarehouseCode,
	                            O.name AS WarehouseName,
                                WSA.sku_uid AS SKUUID,
	                            S.code AS SKUCode,
	                            S.name as SKUName,
                                WSA.batch_number AS BatchNumber,
                                SUM(CASE WHEN WSA.stock_type = 'Salable' THEN WSA.qty ELSE 0 END) AS SalableQty,
                                SUM(CASE WHEN WSA.stock_type = 'NonSalable' THEN WSA.qty ELSE 0 END) AS NonSalableQty,
                                SUM(CASE WHEN WSA.stock_type = 'Reserved' THEN WSA.qty ELSE 0 END) AS ReservedQty,
                                SUM(WSA.qty) AS TotalQty
                            FROM 
                                wh_stock_available WSA
	                            INNER JOIN sku S ON S.uid = WSA.sku_uid
	                            INNER JOIN Org O ON O.uid = WSA.warehouse_uid
                            WHERE 
                                WSA.org_uid = @OrgUID 
                                AND (@WarehouseUID = '' OR WSA.warehouse_uid = @WarehouseUID)
                            GROUP BY 
                                WSA.org_uid, O.code, O.name, WSA.warehouse_uid, WSA.sku_uid, S.code, S.name, WSA.batch_number
                            ORDER BY 
                                WSA.org_uid, O.code, O.name, WSA.warehouse_uid, S.code, S.name, WSA.batch_number;");
            try
            {
                var parameters = new Dictionary<string, object?>
                            {
                                { "@OrgUID", orgUID },
                                { "@WarehouseUID", wareHouseUID }
                            };
                wHStockSummaries = await ExecuteQueryAsync<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockSummary>(query, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
            return wHStockSummaries;
        }
    }
}
