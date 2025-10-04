using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Winit.Modules.Base.Model;
using Winit.Modules.WHStock.DL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using System.Linq;

namespace Winit.Modules.WHStock.DL.Classes
{
    public class SQLiteWHStockDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IWHStockDL
    {
        public Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL _stockUpdaterDL;
        public SQLiteWHStockDL(IServiceProvider serviceProvider, StockUpdater.DL.Interfaces.IStockUpdaterDL stockUpdaterDL) : base(serviceProvider)
        {
            _stockUpdaterDL = stockUpdaterDL;
        }
        //public async Task<int> CUDWHStockMobile(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel)
        //{
        //    int count = 0;
        //    try
        //    {
        //        if (wHRequestTempleteModel != null)
        //        {
        //            count += await CUDWHStockRequest(wHRequestTempleteModel.WHStockRequest);
        //            transaction.Commit();


        //        }

        //        //count += await CUDWHStockRequestLine(wHRequestTempleteModel.WHStockRequestLines);
        //        //transaction.Commit();
        //    }
        //    catch
        //    {
        //        transaction.Rollback();
        //        throw;
        //    }
        //    finally { connection.Close(); }
        //}


        //public async Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel)
        //{
        //    int retValue = -1;
        //    using (var connection = SqliteConnection())
        //    {
        //        await connection.OpenAsync();

        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                if (salesOrderViewModel.IsNewOrder)
        //                {
        //                    //Create SalesOrder
        //                    await CreateSalesOrder(salesOrderViewModel.SalesOrder, connection, transaction);
        //                    //Create SalesOrderInfo
        //                    await CreateSalesOrderInfo(salesOrderViewModel.SalesOrderInfo, connection, transaction);
        //                }
        //                else
        //                {
        //                    //Update SalesOrder
        //                    await UpdateSalesOrder(salesOrderViewModel.SalesOrder, connection, transaction);
        //                    //Update SalesOrderInfo
        //                    await UpdateSalesOrderInfo(salesOrderViewModel.SalesOrderInfo, connection, transaction);
        //                }
        //                // Process SalesOrderLine
        //                if (salesOrderViewModel.SalesOrderLines != null)
        //                {
        //                    foreach (SalesOrderLine salesOrderLine in salesOrderViewModel.SalesOrderLines)
        //                    {
        //                        if (string.IsNullOrEmpty(salesOrderLine.SalesOrderLineUID))
        //                        {
        //                            await CreateSalesOrderLine(salesOrderLine, connection, transaction);
        //                        }
        //                        else
        //                        {
        //                            await UpdateSalesOrderLine(salesOrderLine, connection, transaction);
        //                        }
        //                    }
        //                }
        //                retValue = 1;
        //                await transaction.CommitAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                throw ex;
        //            }
        //        }
        //    }
        //    return retValue;
        //}

        public async Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel)
        {
            int count = 0;
            try
            {
                using (var connection = SqliteConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (wHRequestTempleteModel != null && wHRequestTempleteModel.WHStockRequest != null)
                            {
                                count += await CUDWHStockRequest(wHRequestTempleteModel.WHStockRequest, connection, transaction);

                            }
                            if (wHRequestTempleteModel != null && wHRequestTempleteModel.WHStockRequest != null)
                            {
                                count += await CUDWHStockRequestLine(wHRequestTempleteModel.WHStockRequestLines, connection, transaction);
                            }
                            if (wHRequestTempleteModel?.WHStockLedgerList != null && wHRequestTempleteModel.WHStockLedgerList.Count > 0)
                            {
                                count += await _stockUpdaterDL.UpdateStockAsync(wHRequestTempleteModel.WHStockLedgerList
                                    .ToList<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>(), connection, transaction);
                            }
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        
        public async Task<int> CUDWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            try
            {
                if (wHStockRequestLines == null || wHStockRequestLines.Count == 0)
                {
                    return count;
                }
                List<string> uidList = wHStockRequestLines.Select(po => po.UID).ToList();
                List<string> deletedUidList = wHStockRequestLines.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

                //IEnumerable<IWHStockRequestLine> existingRec = await SelectWHStockRequestLineByUID(uidList);

                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.WHStockRequestLine, uidList, connection, transaction);

                List<WHStockRequestLine>? newRecords = null;
                List<WHStockRequestLine>? existingRecords = null;
                if (existingUIDs != null && existingUIDs.Count > 0)
                {
                    newRecords = wHStockRequestLines.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                    existingRecords = wHStockRequestLines.Where(e => existingUIDs.Contains(e.UID)).ToList();
                }
                else
                {
                    newRecords = wHStockRequestLines;
                }

                if (existingRecords != null && existingRecords.Any())
                {
                    count += await UpdateWHStockRequestLine(existingRecords, connection, transaction);
                }
                if (newRecords != null && newRecords.Any())
                {
                    count += await CreateWHStockRequestLine(newRecords, connection, transaction);
                }

                // Delete
                // Insert data in delete log table and it should be deleted server end too
                await DeleteByUID(DbTableName.WHStockRequestLine, deletedUidList, connection, transaction);

                /*
                foreach (WHStockRequestLine wHStockRequestLine in wHStockRequestLines)
                {
                    switch (wHStockRequestLine.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            if (existingUIDs != null && existingUIDs.Count > 0)
                            {

                            }
                            bool exists = existingRec.Any(po => po.UID == wHStockRequestLine.UID);
                            count += exists ?
                                await UpdateWHStockRequestLine(wHStockRequestLine, connection, transaction) :
                                await CreateWHStockRequestLine(wHStockRequestLine, connection, transaction);
                            break;

                        case Shared.Models.Enums.ActionType.Delete:
                            count += await DeleteWHStockRequestLine(deletedUidList);
                            break;
                    }
                }
                */
            }
            catch
            {
                throw;
            }

            return count;
        }
        public async Task<int> CUDWHStockRequest(Winit.Modules.WHStock.Model.Classes.WHStockRequest wHStockRequest,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            try
            {
                string? uid = await CheckIfUIDExistsInDB(DbTableName.WHStockRequest, wHStockRequest.UID, connection, transaction);
                if (!string.IsNullOrEmpty(uid))
                {
                    count += await UpdateWHStocKRequest(wHStockRequest, connection, transaction);
                }
                else
                {
                    count += await CreateWHStocKRequest(wHStockRequest, connection, transaction);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest> SelectWHStockRequestByUID(string UID,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                            id AS Id,
                            uid AS UID,
                            company_uid AS CompanyUID,
                            source_org_uid AS SourceOrgUID,
                            source_wh_uid AS SourceWHUID,
                            target_org_uid AS TargetOrgUID,
                            target_wh_uid AS TargetWHUID,
                            route_uid AS RouteUID,
                            code AS Code,
                            request_type AS RequestType,
                            request_by_emp_uid AS RequestByEmpUID,
                            job_position_uid AS JobPositionUID,
                            required_by_date AS RequiredByDate,
                            status AS Status,
                            remarks AS Remarks,
                            stock_type AS StockType,
                            ss AS SS,
                            created_time AS CreatedTime,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            org_uid AS OrgUID,
                            warehouse_uid AS WarehouseUID,
                            year_month AS YearMonth
                        FROM 
                            wh_stock_request WHERE uid = @UID";
            return await ExecuteSingleAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest>(sql, parameters, null, connection, transaction);
        }
        private async Task<int> UpdateWHStocKRequest(Winit.Modules.WHStock.Model.Classes.WHStockRequest wHStockRequest,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"UPDATE WH_stock_request 
                                        SET 
                                            ss = 2,
                                            status = @Status, 
                                            modified_time = @ModifiedTime, 
                                            server_modified_time = @ServerModifiedTime
                                        WHERE uid = @UID;
                                        ";
            return await ExecuteNonQueryAsync(Query, wHStockRequest, connection, transaction);
        }
        private async Task<int> CreateWHStocKRequest(Winit.Modules.WHStock.Model.Classes.WHStockRequest wHStockRequest,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {            
            var Query = @"INSERT INTO WH_stock_request (
                            uid, company_uid, source_org_uid, source_wh_uid,  
                            target_org_uid, target_wh_uid, code, request_type, request_by_emp_uid, job_position_uid, required_by_date, status, 
                            remarks, stock_type, ss, created_time, modified_time, server_add_time, server_modified_time, 
                            route_uid ,org_uid,warehouse_uid,year_month
                        ) VALUES (
                            @UID, @CompanyUID, @SourceOrgUID, @SourceWHUID, @TargetOrgUID, @TargetWHUID,
                            @Code, @RequestType, @RequestByEmpUID, @JobPositionUID, @RequiredByDate, @Status, @Remarks, @StockType, 1, 
                            @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID,@OrgUID,@WareHouseUID,@YearMonth
                        );
                        ";
            
            return await ExecuteNonQueryAsync(Query, wHStockRequest, connection, transaction);
        }
        private async Task<int> DeleteWHStocKRequest(string UID)
        {
            var Query = @"
                    DELETE FROM wh_stock_request
                    WHERE uid = @UID;

                    DELETE FROM wh_stock_request_line
                    WHERE wh_stock_request_uid = @UID;";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", UID },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        //private async Task<IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>> SelectWHStockRequestLineByUID(List<string> UIDs)
        //{
        //    UIDs = UIDs.Select(code => code.Trim()).ToList();
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    for (int i = 0; i < UIDs.Count; i++)
        //    {
        //        parameters.Add($"@UID{i}", UIDs[i]);
        //    }
        //    var sql = $"SELECT * FROM wh_stock_request_line WHERE UID IN  ({string.Join(",", UIDs.Select((_, i) => $"@UID{i}"))})";
        //    Type type = _serviceProvider.GetRequiredService<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>().GetType();
        //    IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine> WHStockRequestLineDetails = await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>(sql, parameters, type);
        //    return WHStockRequestLineDetails;
        //}
        private async Task<IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>> SelectWHStockRequestLineByUID(List<string> UIDs)
        {
            UIDs = UIDs.Select(code => code.Trim()).ToList();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < UIDs.Count; i++)
            {
                parameters.Add($"@UID{i}", UIDs[i]);
            }
            var sql = $"SELECT * FROM wh_stock_request_line WHERE uid IN ({string.Join(",", UIDs.Select((_, i) => $"@UID{i}"))})";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>().GetType();
            IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine> WHStockRequestLineDetails = await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>(sql, parameters, type);
            return WHStockRequestLineDetails;
        }
        private async Task<int> CreateWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var query = @"INSERT INTO wh_stock_request_line (
                    uid, wh_stock_request_uid, stock_sub_type, sku_uid, uom1, uom2, uom, uom1_cnf, uom2_cnf, 
                    requested_qty1, requested_qty2, cpe_approved_qty1, cpe_approved_qty2, 
                    cpe_approved_qty, approved_qty1, approved_qty2, approved_qty, forward_qty1, 
                    forward_qty2, forward_qty, collected_qty1, collected_qty2, collected_qty, wh_qty, ss, 
                    created_time, modified_time, server_add_time, server_modified_time, template_qty1, 
                    template_qty2, sku_code, line_number, org_uid, warehouse_uid, year_month
                ) VALUES (
                    @UID, @WHStockRequestUID, @StockSubType, @SKUUID, @UOM1, @UOM2, @UOM, @UOM1CNF, @UOM2CNF, 
                    @RequestedQty1, @RequestedQty2, @CPEApprovedQty1, @CPEApprovedQty2, 
                    @CPEApprovedQty, @ApprovedQty1, @ApprovedQty2, @ApprovedQty, @ForwardQty1, 
                    @ForwardQty2, @ForwardQty, @CollectedQty1, @CollectedQty2, @CollectedQty, @WHQty, 1, 
                    @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @TemplateQty1, 
                    @TemplateQty2, @SKUCode, @LineNumber, @OrgUID, @WareHouseUID, @YearMonth
                );
                ";
                return await ExecuteNonQueryAsync(query, wHStockRequestLines, connection, transaction);
            }
            catch (Exception Ex)
            {
                throw;
            }
        }
        private async Task<int> CreateWHStockRequestLine(Winit.Modules.WHStock.Model.Classes.WHStockRequestLine wHStockRequestLine,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            // int yearMonth = int.Parse(DateTime.Now.ToString("yyMM"));
            try
            {
                int yearMonth = int.Parse(DateTime.Now.ToString("yyMM"));

                var Query = @"INSERT INTO wh_stock_request_line (
                    uid, wh_stock_request_uid, stock_sub_type, sku_uid, uom1, uom2, uom, uom1_cnf, uom2_cnf, 
                    requested_qty1, requested_qty2, cpe_approved_qty1, cpe_approved_qty2, 
                    cpe_approved_qty, approved_qty1, approved_qty2, approved_qty, forward_qty1, 
                    forward_qty2, forward_qty, collected_qty1, collected_qty2, collected_qty, wh_qty, ss, 
                    created_time, modified_time, server_add_time, server_modified_time, template_qty1, 
                    template_qty2, sku_code, line_number, org_uid, warehouse_uid, year_month
                ) VALUES (
                    @UID, @WHStockRequestUID, @StockSubType, @SKUUID, @UOM1, @UOM2, @UOM, @UOM1CNF, @UOM2CNF, 
                    @RequestedQty1, @RequestedQty2, @CPEApprovedQty1, @CPEApprovedQty2, 
                    @CPEApprovedQty, @ApprovedQty1, @ApprovedQty2, @ApprovedQty, @ForwardQty1, 
                    @ForwardQty2, @ForwardQty, @CollectedQty1, @CollectedQty2, @CollectedQty, @WHQty, 1, 
                    @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @TemplateQty1, 
                    @TemplateQty2, @SKUCode, @LineNumber, @OrgUID, @WareHouseUID, @YearMonth
                );
                ";

                var Parameters = new Dictionary<string, object>
                {
                    { "@UID", wHStockRequestLine.UID },
                    { "@WHStockRequestUID", wHStockRequestLine.WHStockRequestUID },
                    { "@StockSubType", wHStockRequestLine.StockSubType },
                    { "@SKUUID", wHStockRequestLine.SKUUID },
                    { "@UOM1", wHStockRequestLine.UOM1 },
                    { "@UOM2", wHStockRequestLine.UOM2 },
                    { "@UOM", wHStockRequestLine.UOM },
                    { "@UOM1CNF", wHStockRequestLine.UOM1CNF },
                    { "@UOM2CNF", wHStockRequestLine.UOM2CNF },
                    { "@RequestedQty1", wHStockRequestLine.RequestedQty1 },
                    { "@RequestedQty2", wHStockRequestLine.RequestedQty2 },
                    { "@CPEApprovedQty1", wHStockRequestLine.CPEApprovedQty1 },
                    { "@CPEApprovedQty2", wHStockRequestLine.CPEApprovedQty2 },
                    { "@CPEApprovedQty", wHStockRequestLine.CPEApprovedQty },
                    { "@ApprovedQty1", wHStockRequestLine.ApprovedQty1 },
                    { "@ApprovedQty2", wHStockRequestLine.ApprovedQty2 },
                    { "@ApprovedQty", wHStockRequestLine.ApprovedQty },
                    { "@ForwardQty1", wHStockRequestLine.ForwardQty1 },
                    { "@ForwardQty2", wHStockRequestLine.ForwardQty2 },
                    { "@ForwardQty", wHStockRequestLine.ForwardQty },
                    { "@CollectedQty1", wHStockRequestLine.CollectedQty1 },
                    { "@CollectedQty2", wHStockRequestLine.CollectedQty2 },
                    { "@CollectedQty", wHStockRequestLine.CollectedQty },
                    { "@WHQty", wHStockRequestLine.WHQty },
                    { "@CreatedTime", wHStockRequestLine.CreatedTime },
                    { "@ModifiedTime", wHStockRequestLine.ModifiedTime },
                    { "@ServerAddTime", wHStockRequestLine.ServerAddTime },
                    { "@ServerModifiedTime", wHStockRequestLine.ServerModifiedTime },
                    { "@TemplateQty1", wHStockRequestLine.TemplateQty1 },
                    { "@TemplateQty2", wHStockRequestLine.TemplateQty2 },
                    { "@SKUCode", wHStockRequestLine.SKUCode },
                    { "@LineNumber", wHStockRequestLine.LineNumber },
                    { "@OrgUID", wHStockRequestLine.OrgUID },
                    { "@WareHouseUID", wHStockRequestLine.WareHouseUID },
                    { "@YearMonth", yearMonth }
                   };
                return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
            }
            catch (Exception Ex)
            {
                throw;
            }


        }
        private async Task<int> UpdateWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {

            var Query = @"UPDATE wh_stock_request_line
                        SET cpe_approved_qty1 = @CPEApprovedQty1, 
                            cpe_approved_qty2 = @CPEApprovedQty2, 
                            cpe_approved_qty = @CPEApprovedQty, 
                            approved_qty1 = @ApprovedQty1, 
                            approved_qty2 = @ApprovedQty2, 
                            approved_qty = @ApprovedQty, 
                            requested_qty2 = @RequestedQty2,
                            requested_qty1 = @RequestedQty1,
                            requested_qty = @RequestedQty,
                            ss = 2,
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime,
                            collected_qty1 = @CollectedQty1,
                            collected_qty2 = @CollectedQty2,
                            collected_qty = @CollectedQty,
                            line_number = @LineNumber
                        WHERE uid = @UID";

            return await ExecuteNonQueryAsync(Query, wHStockRequestLines, connection, transaction);
        }
        private async Task<int> UpdateWHStockRequestLine(Winit.Modules.WHStock.Model.Classes.WHStockRequestLine wHStockRequestLine,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {

            var Query = @"UPDATE wh_stock_request_line
                        SET cpe_approved_qty1 = @CPEApprovedQty1, 
                            cpe_approved_qty2 = @CPEApprovedQty2, 
                            cpe_approved_qty = @CPEApprovedQty, 
                            approved_qty1 = @ApprovedQty1, 
                            approved_qty2 = @ApprovedQty2, 
                            approved_qty = @ApprovedQty, 
                            requested_qty2 = @RequestedQty2,
                            requested_qty1 = @RequestedQty1,
                            requested_qty = @RequestedQty,
                            ss = 2,
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime,
                            collected_qty1 = @CollectedQty1,
                            collected_qty2 = @CollectedQty2,
                            collected_qty = @CollectedQty,
                            line_number = @LineNumber
                        WHERE uid = @UID";
            //var Parameters = new Dictionary<string, object>
            //            {
            //               { "@UID", wHStockRequestLine.UID },
            //               { "@RequestedQty1", wHStockRequestLine.RequestedQty1 },
            //               { "@RequestedQty2", wHStockRequestLine.RequestedQty2 },
            //               { "@RequestedQty", wHStockRequestLine.RequestedQty },
            //               { "@CPEApprovedQty1", wHStockRequestLine.CPEApprovedQty1 },
            //               { "@CPEApprovedQty2", wHStockRequestLine.CPEApprovedQty2 },
            //               { "@CPEApprovedQty", wHStockRequestLine.CPEApprovedQty },
            //               { "@ApprovedQty1", wHStockRequestLine.ApprovedQty1 },
            //               { "@ApprovedQty2", wHStockRequestLine.ApprovedQty2 },
            //               { "@ApprovedQty", wHStockRequestLine.ApprovedQty },
            //               { "@ModifiedTime", wHStockRequestLine.ModifiedTime },
            //               { "@ServerModifiedTime", wHStockRequestLine.ServerModifiedTime },
            //               { "@CollectedQty1", wHStockRequestLine.CollectedQty1 },
            //               { "@CollectedQty2", wHStockRequestLine.CollectedQty2 },
            //               { "@CollectedQty", wHStockRequestLine.CollectedQty },
            //               { "@LineNumber", wHStockRequestLine.LineNumber },
            //            };
            return await ExecuteNonQueryAsync(Query, wHStockRequestLine, connection, transaction);
        }
        private async Task<int> DeleteWHStockRequestLine(List<string> UIDs)
        {
            var sql = @"DELETE FROM wh_stock_request_line WHERE  uid IN @UIDs";
            var parameters = new { UIDs = UIDs };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StockType)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                WSR.uid AS UID,
                                WSR.modified_time AS ModifiedTime,
                                WSR.request_type AS RequestType,
                                WSR.code AS RequestCode,
                                R.code AS RouteCode,
                                R.name AS RouteName,
                                WSR.created_time AS RequestedTime,
                                WSR.required_by_date AS RequiredByDate,
                                WSR.status AS Status,
                                WSR.remarks AS Remarks
                            FROM 
                                wh_stock_request WSR
                            INNER JOIN 
                                route R ON R.uid = WSR.route_uid 
                            WHERE 
                                WSR.status = @StockType");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt  
                                                   FROM Wh_stock_request WSR
                                                   INNER JOIN Route R ON R.UID = WSR.RouteUID 
                                                   INNER JOIN Org S ON S.UID = WSR.SourceWHUID
                                                   INNER JOIN Org T ON T.UID = WSR.TargetWHUID  
                                                   WHERE WSR.Status = @StockType;");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "StockType", StockType },
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IWHStockRequestItemView>().GetType();

                IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView> LoadRequestDetails = await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView> pagedResponse = new PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>
                {
                    PagedData = LoadRequestDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<(List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>, List<IWHStockRequestLineItemView>)> SelectLoadRequestDataByUID(string UID)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object>
                {
                    { "UID", UID },
                };
                var whStockSql = new StringBuilder(@"SELECT 
                            WSR.uid AS UID,
                            WSR.source_org_uid AS SourceOrgUID,
                            WSR.target_org_uid  AS TargetOrgUID ,
                            WSR.source_wh_uid AS SourceWHUID,
                            WSR.target_wh_uid  AS TargetWHUID,
                            WSR.warehouse_uid AS WareHouseUID,
                            R.uid AS RouteUID,
                            R.code AS RouteCode,
                            R.name AS RouteName,
                            S.code AS SourceCode,
                            S.name AS SourceName,
                            T.code AS TargetCode,
                            T.name AS TargetName,
                            WSR.code AS RequestCode,
                            WSR.request_type AS RequestType,
                            WSR.required_by_date AS RequiredByDate,
                            WSR.status AS Status,
                            WSR.stock_type AS StockType,
                            WSR.remarks AS Remarks,
                            WSR.org_uid AS OrgUID,
                            WSR.warehouse_uid AS WarehouseUID,
                            WSR.year_month AS YearMonth,
                            WSR.created_time AS RequestedTime,
                            WSR.modified_time AS ModifiedTime
                        FROM 
                            wh_stock_request WSR
                        LEFT JOIN 
                            Route R ON R.uid = WSR.route_uid 
                        LEFT JOIN 
                            Org S ON S.uid = WSR.source_wh_uid
                        LEFT JOIN 
                            Org T ON T.uid = WSR.target_wh_uid 
                        WHERE 
                            WSR.uid = @UID ");
                Type whStockType = _serviceProvider.GetRequiredService<Model.Interfaces.IWHStockRequestItemView>().GetType();
                List<Model.Interfaces.IWHStockRequestItemView> WHStockRequest = await ExecuteQueryAsync<Model.Interfaces.IWHStockRequestItemView>(whStockSql.ToString(), Parameters, whStockType);
                var whStockLineSQL = new StringBuilder(@"SELECT 
                                            WSRL.sku_uid AS SKUUID, 
                                            S.code AS SKUCode, 
                                            S.name AS SKUName, 
                                            WSRL.uid AS UID,
                                            WSRL.modified_time AS ModifiedTime, 
                                            WSRL.uom1 AS UOM1, 
                                            WSRL.uom2 AS UOM2, 
                                            WSRL.uom AS UOM, 
                                            WSRL.requested_qty1 AS RequestedQty1, 
                                            WSRL.requested_qty2 AS RequestedQty2, 
                                            WSRL.requested_qty AS RequestedQty,
                                            WSRL.cpe_approved_qty1 AS CPEApprovedQty1, 
                                            WSRL.cpe_approved_qty2 AS CPEApprovedQty2, 
                                            WSRL.cpe_approved_qty AS CPEApprovedQty,
                                            WSRL.approved_qty1 AS ApprovedQty1, 
                                            WSRL.approved_qty2 AS ApprovedQty2, 
                                            WSRL.approved_qty AS ApprovedQty,
                                            WSRL.forward_qty1 AS ForwardQty1, 
                                            WSRL.forward_qty2 AS ForwardQty2, 
                                            WSRL.forward_qty AS ForwardQty,
                                            WSRL.template_qty1 AS TemplateQty1, 
                                            WSRL.template_qty2 AS TemplateQty2,
                                            WSRL.collected_qty1 AS CollectedQty1, 
                                            WSRL.collected_qty2 AS CollectedQty2, 
                                            WSRL.collected_qty AS CollectedQty, 
                                            WSRL.uom2_cnf AS UOM2CNF,
                                            WSRL.line_number AS LineNumber,
                                            WSRL.org_uid AS OrgUID,
                                            WSRL.warehouse_uid AS WarehouseUID,
                                            WSRL.year_month AS YearMonth
                                        FROM 
                                            wh_stock_request_line WSRL
                                        INNER JOIN 
                                            sku S ON S.uid = WSRL.sku_uid
                                        WHERE 
                                            WSRL.wh_stock_request_uid = @UID  
                                        ORDER BY 
                                            WSRL.line_number ASC");
                Type whStockLineType = _serviceProvider.GetRequiredService<Model.Interfaces.IWHStockRequestLineItemView>().GetType();
                List<Model.Interfaces.IWHStockRequestLineItemView> WHStockLineList = await ExecuteQueryAsync<Model.Interfaces.IWHStockRequestLineItemView>(whStockLineSQL.ToString(), Parameters, whStockLineType);
                return (WHStockRequest, WHStockLineList);
            }
            catch
            {
                throw;
            }
            finally { }

        }
    }
}
