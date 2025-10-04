using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.WHStock.DL.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.DL.Classes
{
    public class MSSQLWHStockDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IWHStockDL
    {
        IServiceProvider _serviceProvider = null;
        public Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL _stockUpdaterDL;
        public MSSQLWHStockDL(IServiceProvider serviceProvider, IConfiguration config, 
            StockUpdater.DL.Interfaces.IStockUpdaterDL stockUpdaterDL) : base(serviceProvider, config)
        {
            _serviceProvider = serviceProvider;
            _stockUpdaterDL = stockUpdaterDL;
        }
        public async Task<int> CUDWHStock(Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel wHRequestTempleteModel)
        {
            int count = 0;
            try
            {
                using (var connection = CreateConnection())
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
                            if (wHRequestTempleteModel?.WHStockLedgerList != null && wHRequestTempleteModel.WHStockLedgerList.Count == 0)
                            {
                                count += await _stockUpdaterDL.UpdateStockAsync(wHRequestTempleteModel.WHStockLedgerList
                                    .ToList<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>(), 
                                    connection, transaction);
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
                List<string> deletedUidList = new List<string>(); // Get this list from delete log

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
                   count +=await UpdateWHStockRequestLine(existingRecords, connection, transaction);
                }
                if (newRecords.Any())
                {
                    count += await CreateWHStockRequestLine(newRecords, connection, transaction);
                }

                // Delete 
                //await DeleteByUID(DbTableName.WHStockRequestLine, deletedUidList, connection, transaction);
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
                  count +=  await UpdateWHStocKRequest(wHStockRequest, connection, transaction);
                }
                else
                {
                    count +=await CreateWHStocKRequest(wHStockRequest, connection, transaction);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        public async Task<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest> SelectWHStockRequestByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT
                        id AS Id,
                        uid AS UID,
                        company_uid AS CompanyUid,
                        source_org_uid AS SourceOrgUid,
                        source_wh_uid AS SourceWhUid,
                        target_org_uid AS TargetOrgUid,
                        target_wh_uid AS TargetWhUid,
                        code AS Code,
                        request_type AS RequestType,
                        request_by_emp_uid AS RequestByEmpUid,
                        job_position_uid AS JobPositionUid,
                        required_by_date AS RequiredByDate,
                        status AS Status,
                        remarks AS Remarks,
                        stock_type AS StockType,
                        ss AS Ss,
                        created_time AS CreatedTime,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        route_uid AS RouteUid
                        FROM wh_stock_request 
                        WHERE uid = @UID;
                    ";
            Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest whStockRequestDetails = await ExecuteSingleAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest>(sql, parameters);
            return whStockRequestDetails;
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
            return await ExecuteNonQueryAsync(Query, connection, transaction, wHStockRequest);
        }
        private async Task<int> CreateWHStocKRequest(Winit.Modules.WHStock.Model.Classes.WHStockRequest wHStockRequest,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try {
                
                var Query = @"INSERT INTO WH_stock_request (
                  uid, company_uid, source_org_uid, source_wh_uid,  
                  target_org_uid, target_wh_uid, code, request_type, request_by_emp_uid, job_position_uid, required_by_date, status, 
                  remarks, stock_type, ss, created_time, modified_time, server_add_time, server_modified_time, 
                  route_uid ,org_uid,warehouse_uid,year_month
              ) VALUES (
                  @UID, @CompanyUID, @SourceOrgUID, @SourceWHUID, @TargetOrgUID, @TargetWHUID,
                  @Code, @RequestType, @RequestByEmpUID, @JobPositionUID, @RequiredByDate, @Status, @Remarks, @StockType, @SS, 
                  @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @RouteUID,@OrgUID,@WareHouseUID,@YearMonth
              );
              ";

                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, wHStockRequest);
            }
            catch(Exception ex )
            {

                throw;
            }
            return retVal;


        }
        private async Task<int> DeleteWHStocKRequest(string UID)
        {
            var Query = @"
                    DELETE FROM WH_stock_request
                        WHERE uid = @UID;

                        DELETE FROM WH_stock_request_line
                        WHERE wh_stock_request_uid = @UID;
                        ";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@UID", UID },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters);
        }
        private async Task<IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>> SelectWHStockRequestLineByUID(List<string> uids)
        {
            try
            {
                var parameters = new { UIDs = uids };


            var sql = @"SELECT 
                    id AS Id, 
                    uid AS UID, 
                    wh_stock_request_uid AS WhStockRequestUid, 
                    stock_sub_type AS StockSubType, 
                    sku_uid AS SkuUid, 
                    uom1 AS Uom1, 
                    uom2 AS Uom2, 
                    uom AS Uom, 
                    uom1_cnf AS Uom1Cnf, 
                    uom2_cnf AS Uom2Cnf, 
                    requested_qty1 AS RequestedQty1, 
                    requested_qty2 AS RequestedQty2, 
                    requested_qty AS RequestedQty,
                    cpe_approved_qty1 AS CpeApprovedQty1, 
                    cpe_approved_qty2 AS CpeApprovedQty2, 
                    cpe_approved_qty AS CpeApprovedQty, 
                    approved_qty1 AS ApprovedQty1, 
                    approved_qty2 AS ApprovedQty2, 
                    approved_qty AS ApprovedQty, 
                    forward_qty1 AS ForwardQty1, 
                    forward_qty2 AS ForwardQty2, 
                    forward_qty AS ForwardQty, 
                    collected_qty1 AS CollectedQty1, 
                    collected_qty2 AS CollectedQty2, 
                    collected_qty AS CollectedQty, 
                    wh_qty AS WhQty, 
                    ss AS SS, 
                    created_time AS CreatedTime, 
                    modified_time AS ModifiedTime, 
                    server_add_time AS ServerAddTime, 
                    server_modified_time AS ServerModifiedTime, 
                    template_qty1 AS TemplateQty1, 
                    template_qty2 AS TemplateQty2, 
                    sku_code AS SkuCode, 
                    line_number AS LineNumber
                FROM 
                    wh_stock_request_line 
                WHERE
                    uid IN @UIDs;";
                IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine> WHStockRequestLineDetails = await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>(sql, parameters);
                return WHStockRequestLineDetails;
            }
            catch
            {
                throw;
            }
            
        }
        private async Task<int> CreateWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
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
                    @ForwardQty2, @ForwardQty, @CollectedQty1, @CollectedQty2, @CollectedQty, @WHQty, 0, 
                    @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @TemplateQty1, 
                    @TemplateQty2, @SKUCode, @LineNumber, @OrgUID, @WareHouseUID, @YearMonth
                );
                ";
                retVal = await ExecuteNonQueryAsync(query, connection, transaction, wHStockRequestLines);
            }
            catch (Exception Ex)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> UpdateWHStockRequestLine(List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine> wHStockRequestLines,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {int retVal=-1;
            try
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
                            ss = 0,
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime,
                            collected_qty1 = @CollectedQty1,
                            collected_qty2 = @CollectedQty2,
                            collected_qty = @CollectedQty,
                            line_number = @LineNumber
                        WHERE uid = @UID";

                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, wHStockRequestLines);
            }
            catch
            {
                throw;
            }
            return retVal;
            
        }
        private async Task<int> DeleteWHStockRequestLine(List<string> UIDs)
        {
            var sql = @"delete from wh_stock_request_line where  uid  in @UIDs";
            var parameters = new { UIDs = UIDs };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<PagedResponse<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>> SelectLoadRequestData(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,string StockType)
        {
                    try
                    {
                       var sql = new StringBuilder(@"SELECT * FROM (
                                                SELECT
                                                wsr.id AS Id,
                                                wsr.uid AS UID,
                                                wsr.code AS RequestCode,
                                                wsr.request_type AS RequestType,
                                                r.code AS RouteCode,
                                                r.name AS RouteName,
                                                s.code AS SourceCode,
                                                s.uid AS OrgUID,
                                                s.name AS SourceName,
                                                t.code AS TargetCode,
                                                t.name AS TargetName,
                                                wsr.modified_time AS ModifiedTime,
                                                wsr.created_time AS RequestedTime,
                                                wsr.required_by_date AS RequiredByDate,
                                                wsr.status AS Status,
                                                wsr.remarks AS Remarks
                                                -- CASE WHEN wsr.remarks = 'Auto Generated' THEN 0 ELSE 1 END AS IsDeleteEnabled
                                            FROM
                                                wh_stock_request wsr
                                            LEFT JOIN
                                                route r ON r.uid = wsr.route_uid
                                            LEFT JOIN
                                                org s ON s.uid = wsr.source_wh_uid
                                            LEFT JOIN
                                                org t ON t.uid = wsr.target_wh_uid
                                            WHERE
                                                wsr.status = @StockType
                                        ) AS subquery");
                
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (
                                                SELECT 
                                                    wsr.id AS Id,
                                                    wsr.uid AS UID,
                                                    wsr.code AS RequestCode,
                                                    wsr.request_type AS RequestType,
                                                    r.code AS RouteCode, 
                                                    r.name AS RouteName, 
                                                    s.code AS SourceCode, 
                                                    s.uid AS OrgUID, 
                                                    s.name AS SourceName,
                                                    t.code AS TargetCode, 
                                                    t.name AS TargetName, 
                                                    wsr.modified_time AS ModifiedTime,
                                                    wsr.created_time AS RequestedTime,
                                                    wsr.required_by_date AS RequiredByDate,
                                                    wsr.status AS Status,
                                                    wsr.remarks AS Remarks
                                                    -- CASE WHEN wsr.remarks = 'Auto Generated' THEN 0 ELSE 1 END AS IsDeleteEnabled
                                                FROM 
                                                    wh_stock_request wsr
                                                LEFT JOIN 
                                                    route r ON r.uid = wsr.route_uid 
                                                LEFT JOIN 
                                                    org s ON s.uid = wsr.source_wh_uid
                                                LEFT JOIN 
                                                    org t ON t.uid = wsr.target_wh_uid 
                                                WHERE 
                                                    wsr.status = @StockType
                                            ) AS subquery");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "StockType", StockType },
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>(filterCriterias, sbFilterCriteria, parameters);
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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }
                IEnumerable<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView> LoadRequestDetails = await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestItemView>(sql.ToString(), parameters);
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
                var whStockSql = new StringBuilder(@"select 
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
                                        where 
                                            WSR.uid = @uid
                                        ");
                Type whStockType = _serviceProvider.GetRequiredService<Model.Interfaces.IWHStockRequestItemView>().GetType();
                List<Model.Interfaces.IWHStockRequestItemView> WHStockRequest = await ExecuteQueryAsync<Model.Interfaces.IWHStockRequestItemView>(whStockSql.ToString(), Parameters, whStockType);
                var whStockLineSQL = new StringBuilder(@"select 
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
                                                from 
                                                    wh_stock_request_line wsrl
                                                inner join 
                                                    sku s on s.uid = wsrl.sku_uid
                                                where 
                                                    wsrl.wh_stock_request_uid = @uid
                                                order by 
                                                    wsrl.modified_time desc
                                                ");
                List<Model.Interfaces.IWHStockRequestLineItemView> WHStockLineList = await ExecuteQueryAsync<Model.Interfaces.IWHStockRequestLineItemView>(whStockLineSQL.ToString(), Parameters);
                return (WHStockRequest, WHStockLineList);
            }
            catch
            {
                throw;
            }
          
        }


    }
}
