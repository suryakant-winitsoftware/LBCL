
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Modules.ReturnOrder.DL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Classes;

public class SQLiteReturnOrderDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IReturnOrderDL
{
     protected IStoreHistoryDL _storeHistoryDL;
    public SQLiteReturnOrderDL(IServiceProvider serviceProvider, IStoreHistoryDL storeHistoryDL) : base(serviceProvider)
    {
        _storeHistoryDL = storeHistoryDL;
    }
    public async Task<PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>> SelectAllReturnOrderDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, 
	                server_modified_time, return_order_number, draft_order_number, job_position_uid, 
	                emp_uid, org_uid, distribution_channel_uid, store_uid, is_tax_applicable, route_uid,
	                beat_history_uid, store_history_uid, status, order_type, order_date, currency_uid, 
	                total_amount, total_line_discount, total_cash_discount, total_header_discount, 
	                total_discount, total_excise_duty, line_tax_amount, header_tax_amount, total_tax, 
	                net_amount, total_fake_amount, line_count, qty_count, notes, is_offline, latitude, 
	                longitude, delivered_by_org_uid, ss, source, promotion_uid, total_line_tax, 
	                total_header_tax 
	                FROM return_order");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM return_order");
            }
            var parameters = new Dictionary<string, object?>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

                sql.Append(sbFilterCriteria);
                // If count required then add filters to count
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

            //Data
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrder>().GetType();
            IEnumerable<Model.Interfaces.IReturnOrder> returnOrders = await ExecuteQueryAsync<Model.Interfaces.IReturnOrder>(sql.ToString(), parameters, type);
            //Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder> pagedResponse = new PagedResponse<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>
            {
                PagedData = returnOrders,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder> SelectReturnOrderByUID(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"UID" , UID}
        };
        var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, 
	                server_modified_time, return_order_number, draft_order_number, job_position_uid, 
	                emp_uid, org_uid, distribution_channel_uid, store_uid, is_tax_applicable, route_uid,
	                beat_history_uid, store_history_uid, status, order_type, order_date, currency_uid, 
	                total_amount, total_line_discount, total_cash_discount, total_header_discount, 
	                total_discount, total_excise_duty, line_tax_amount, header_tax_amount, total_tax, 
	                net_amount, total_fake_amount, line_count, qty_count, notes, is_offline, latitude, 
	                longitude, delivered_by_org_uid, ss, source, promotion_uid, total_line_tax, 
	                total_header_tax
	                FROM public.return_order";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrder>().GetType();
        Model.Interfaces.IReturnOrder ReturnOrderList = await ExecuteSingleAsync<Model.Interfaces.IReturnOrder>(sql, parameters, type);
        return ReturnOrderList;
    }
    public async Task<int> CreateReturnOrder(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder returnOrder)
    {
        try
        {
            var sql = @"INSERT INTO ReturnOrder (
    uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, 
    server_modified_time AS ServerModifiedTime, return_order_number AS ReturnOrderNumber, draft_order_number AS DraftOrderNumber, job_position_uid AS JobPositionUID, 
    emp_uid AS EmpUID, org_uid AS OrgUID, distribution_channel_uid AS DistributionChannelUID, store_uid AS StoreUID, is_tax_applicable AS IsTaxApplicable, 
    route_uid AS RouteUID, beat_history_uid AS BeatHistoryUID, store_history_uid AS StoreHistoryUID, status AS Status, order_type AS OrderType, order_date AS OrderDate, 
    currency_uid AS CurrencyUID, total_amount AS TotalAmount, total_line_discount AS TotalLineDiscount, total_cash_discount AS TotalCashDiscount, 
    total_header_discount AS TotalHeaderDiscount, total_discount AS TotalDiscount, total_excise_duty AS TotalExciseDuty, line_tax_amount AS LineTaxAmount, 
    header_tax_amount AS HeaderTaxAmount, total_tax AS TotalTax, net_amount AS NetAmount, total_fake_amount AS TotalFakeAmount, line_count AS LineCount,
    qty_count AS QtyCount, notes AS Notes, is_offline AS IsOffline, latitude AS Latitude, longitude AS Longitude, delivered_by_org_uid AS DeliveredByOrgUID, 
    promotion_uid AS PromotionUID, total_line_tax AS TotalLineTax, total_header_tax AS TotalHeaderTax) 
    VALUES ( @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReturnOrderNumber, @DraftOrderNumber, @JobPositionUID,
    @EmpUID, @OrgUID, @DistributionChannelUID, @StoreUID, @IsTaxApplicable, @RouteUID, @BeatHistoryUID,@StoreHistoryUID, @Status, @OrderType, @OrderDate, @CurrencyUID, 
    @TotalAmount, @TotalLineDiscount, @TotalCashDiscount, @TotalHeaderDiscount, @TotalDiscount, @TotalExciseDuty, @LineTaxAmount, @HeaderTaxAmount, @TotalTax, 
    @NetAmount, @TotalFakeAmount,  @LineCount, @QtyCount, @Notes, @IsOffline, @Latitude, @Longitude, @DeliveredByOrgUID, @PromotionUID, @TotalLineTax, @TotalHeaderTax)";
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "@UID", returnOrder.UID },
                { "@CreatedBy", returnOrder.CreatedBy },
                { "@CreatedTime", returnOrder.CreatedTime },
                { "@ModifiedBy", returnOrder.ModifiedBy },
                { "@ModifiedTime", returnOrder.ModifiedTime },
                { "@ServerAddTime", returnOrder.ServerAddTime },
                { "@ServerModifiedTime", returnOrder.ServerModifiedTime },
                { "@ReturnOrderNumber", returnOrder.ReturnOrderNumber },
                { "@DraftOrderNumber", returnOrder.DraftOrderNumber },
                { "@JobPositionUID", returnOrder.JobPositionUID },
                { "@EmpUID", returnOrder.EmpUID },
                { "@OrgUID", returnOrder.OrgUID },
                { "@DistributionChannelUID", returnOrder.DistributionChannelUID },
                { "@StoreUID", returnOrder.StoreUID },
                { "@IsTaxApplicable", returnOrder.IsTaxApplicable },
                { "@RouteUID", returnOrder.RouteUID },
                { "@BeatHistoryUID", returnOrder.BeatHistoryUID },
                { "@StoreHistoryUID", returnOrder.StoreHistoryUID },
                { "@Status", returnOrder.Status },
                { "@OrderType", returnOrder.OrderType },
                { "@OrderDate", returnOrder.OrderDate },
                { "@CurrencyUID", returnOrder.CurrencyUID },
                { "@TotalAmount", returnOrder.TotalAmount },
                { "@TotalLineDiscount", returnOrder.TotalLineDiscount },
                { "@TotalCashDiscount", returnOrder.TotalCashDiscount },
                { "@TotalHeaderDiscount", returnOrder.TotalHeaderDiscount },
                { "@TotalDiscount", returnOrder.TotalDiscount },
                { "@TotalExciseDuty", returnOrder.TotalExciseDuty },
                { "@LineTaxAmount", returnOrder.LineTaxAmount },
                { "@HeaderTaxAmount", returnOrder.HeaderTaxAmount },
                { "@TotalTax", returnOrder.TotalTax },
                { "@NetAmount", returnOrder.NetAmount },
                { "@TotalFakeAmount", returnOrder.TotalFakeAmount },
                { "@LineCount", returnOrder.LineCount },
                { "@QtyCount", returnOrder.QtyCount },
                { "@Notes", returnOrder.Notes },
                { "@IsOffline", returnOrder.IsOffline },
                { "@Latitude", returnOrder.Latitude },
                { "@Longitude", returnOrder.Longitude },
                { "@DeliveredByOrgUID", returnOrder.DeliveredByOrgUID },
                  { "@PromotionUID", returnOrder.PromotionUID },
                { "@TotalLineTax", returnOrder.TotalLineTax },
                { "@TotalHeaderTax", returnOrder.TotalHeaderTax },
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateReturnOrder(Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder returnOrder)
    {
        try
        {
            var sql = @"UPDATE ReturnOrder SET
                            modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            return_order_number = @ReturnOrderNumber, 
                            draft_order_number = @DraftOrderNumber, 
                            job_position_uid = @JobPositionUID, 
                            emp_uid = @EmpUID, 
                            org_uid = @OrgUID, 
                            distribution_channel_uid = @DistributionChannelUID, 
                            store_uid = @StoreUID, 
                            is_tax_applicable = @IsTaxApplicable, 
                            route_uid = @RouteUID, 
                            beat_history_uid = @BeatHistoryUID, 
                            store_history_uid = @StoreHistoryUID, 
                            status = @Status, 
                            order_type = @OrderType, 
                            order_date = @OrderDate, 
                            currency_uid = @CurrencyUID, 
                            total_amount = @TotalAmount, 
                            total_line_discount = @TotalLineDiscount, 
                            total_cash_discount = @TotalCashDiscount, 
                            total_header_discount = @TotalHeaderDiscount, 
                            total_discount = @TotalDiscount, 
                            total_excise_duty = @TotalExciseDuty, 
                            line_tax_amount = @LineTaxAmount, 
                            header_tax_amount = @HeaderTaxAmount, 
                            total_tax = @TotalTax, 
                            net_amount = @NetAmount, 
                            total_fake_amount = @TotalFakeAmount, 
                            line_count = @LineCount, 
                            qty_count = @QtyCount, 
                            notes = @Notes, 
                            is_offline = @IsOffline, 
                            latitude = @Latitude, 
                            longitude = @Longitude, 
                            delivered_by_org_uid = @DeliveredByOrgUID,
                            promotion_uid = @PromotionUID,
                            total_line_tax = @TotalLineTax,
                            total_header_tax = @TotalHeaderTax
                            WHERE uid = @UID";
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
               { "@UID", returnOrder.UID },
               { "@ModifiedBy", returnOrder.ModifiedBy },
               { "@ModifiedTime", returnOrder.ModifiedTime },
               { "@ServerModifiedTime", returnOrder.ServerModifiedTime },
               { "@ReturnOrderNumber", returnOrder.ReturnOrderNumber },
               { "@DraftOrderNumber", returnOrder.DraftOrderNumber },
               { "@JobPositionUID", returnOrder.JobPositionUID },
               { "@EmpUID", returnOrder.EmpUID },
               { "@OrgUID", returnOrder.OrgUID },
               { "@DistributionChannelUID", returnOrder.DistributionChannelUID },
               { "@StoreUID", returnOrder.StoreUID },
               { "@IsTaxApplicable", returnOrder.IsTaxApplicable },
               { "@RouteUID", returnOrder.RouteUID },
               { "@BeatHistoryUID", returnOrder.BeatHistoryUID },
               { "@StoreHistoryUID", returnOrder.StoreHistoryUID },
               { "@Status", returnOrder.Status },
               { "@OrderType", returnOrder.OrderType },
               { "@OrderDate", returnOrder.OrderDate },
               { "@CurrencyUID", returnOrder.CurrencyUID },
               { "@TotalAmount", returnOrder.TotalAmount },
               { "@TotalLineDiscount", returnOrder.TotalLineDiscount },
               { "@TotalCashDiscount", returnOrder.TotalCashDiscount },
               { "@TotalHeaderDiscount", returnOrder.TotalHeaderDiscount },
               { "@TotalDiscount", returnOrder.TotalDiscount },
               { "@TotalExciseDuty", returnOrder.TotalExciseDuty },
               { "@LineTaxAmount", returnOrder.LineTaxAmount },
               { "@HeaderTaxAmount", returnOrder.HeaderTaxAmount },
               { "@TotalTax", returnOrder.TotalTax },
               { "@NetAmount", returnOrder.NetAmount },
               { "@TotalFakeAmount", returnOrder.TotalFakeAmount },
               { "@LineCount", returnOrder.LineCount },
               { "@QtyCount", returnOrder.QtyCount },
               { "@Notes", returnOrder.Notes },
               { "@IsOffline", returnOrder.IsOffline },
               { "@Latitude", returnOrder.Latitude },
               { "@Longitude", returnOrder.Longitude },
               { "@DeliveredByOrgUID", returnOrder.DeliveredByOrgUID },
               { "@PromotionUID", returnOrder.PromotionUID },
               { "@TotalLineTax", returnOrder.TotalLineTax },
               { "@TotalHeaderTax", returnOrder.TotalHeaderTax },
            };
            return await ExecuteNonQueryAsync(sql, parameters);

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteReturnOrder(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"UID" , UID}
        };
        var sql = @"DELETE  FROM return_order WHERE uid = @UID";
        return await ExecuteNonQueryAsync(sql, parameters);
    }

    public async Task<int> CreateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMaster)
    {
        int count= 0;
        try
        {
            using (var connection = SqliteConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (returnOrderMaster.ReturnOrder != null) {

                            var returnOrderQuery = @"INSERT INTO return_order (
                                     id, uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                    return_order_number, draft_order_number, job_position_uid, emp_uid, org_uid, distribution_channel_uid, store_uid, 
                                    is_tax_applicable, route_uid, beat_history_uid, store_history_uid, status, order_type, order_date, currency_uid, 
                                    total_amount, total_line_discount, total_cash_discount, total_header_discount, total_discount, total_excise_duty, 
                                    line_tax_amount, header_tax_amount, total_tax, net_amount, total_fake_amount, line_count, qty_count, notes, 
                                    is_offline, latitude, longitude, delivered_by_org_uid, source, promotion_uid, total_line_tax, total_header_tax,Sales_Order_UID
                                ) VALUES (
                                     @Id, @UID, 1, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                    @ReturnOrderNumber, @DraftOrderNumber, @JobPositionUID, @EmpUID, @OrgUID, @DistributionChannelUID, @StoreUID, 
                                    @IsTaxApplicable, @RouteUID, @BeatHistoryUID, @StoreHistoryUID, @Status, @OrderType, @OrderDate, @CurrencyUID, 
                                    @TotalAmount, @TotalLineDiscount, @TotalCashDiscount, @TotalHeaderDiscount, @TotalDiscount, @TotalExciseDuty, 
                                    @LineTaxAmount, @HeaderTaxAmount, @TotalTax, @NetAmount, @TotalFakeAmount, @LineCount, @QtyCount, @Notes, 
                                    @IsOffline, @Latitude, @Longitude, @DeliveredByOrgUID, @Source, @PromotionUID, @TotalLineTax, @TotalHeaderTax,@SalesOrderUID
                                );";
                            count = await ExecuteNonQueryAsync(returnOrderQuery, returnOrderMaster.ReturnOrder, connection, transaction);
                        }
                        if (returnOrderMaster.ReturnOrderLineList != null && returnOrderMaster.ReturnOrderLineList.Any())
                        {
                            var returnOrderLineQuery = @"INSERT INTO return_order_line (
                            id, uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            return_order_uid, line_number, sku_uid, sku_code, sku_type, base_price, unit_price, fake_unit_price, 
                            base_uom, uom, multiplier, qty, qty_bu, approved_qty, returned_qty, total_amount, total_discount, 
                            total_excise_duty, total_tax, net_amount, sku_price_uid, sku_price_list_uid, reason_code, reason_text, 
                            expiry_date, batch_number, sales_order_uid, sales_order_line_uid, remarks, volume, volume_unit, 
                            promotion_uid, net_fake_amount, po_number
                                ) VALUES (
                            @Id,@UID, 1, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @ReturnOrderUID, @LineNumber, @SKUUID, @SKUCode, @SKUType, @BasePrice, @UnitPrice, @FakeUnitPrice, 
                            @BaseUOM, @UoM, @Multiplier, @Qty, @QtyBU, @ApprovedQty, @ReturnedQty, @TotalAmount, @TotalDiscount, 
                            @TotalExciseDuty, @TotalTax, @NetAmount, @SKUPriceUID, @SKUPriceListUID, @ReasonCode, @ReasonText, 
                            @ExpiryDate, @BatchNumber, @SalesOrderUID, @SalesOrderLineUID, @Remarks, @Volume, @VolumeUnit, 
                            @PromotionUID, @NetFakeAmount, @PONumber
                        );";

                            count += await ExecuteNonQueryAsync(returnOrderLineQuery, returnOrderMaster.ReturnOrderLineList,connection, transaction);
                        }
                        //if (returnOrderMaster.StoreHistory != null) {

                        //     await _storeHistoryDL.CreateStoreHistory(returnOrderMaster.StoreHistory, connection, transaction);
                        //}
                        transaction.Commit();
                       
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return count;
    }

    public async Task<int> UpdateReturnOrderMaster(Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO updateReturnOrderMaster)
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
                        if(updateReturnOrderMaster.ReturnOrder != null)
                        {
                            var returnOrderQuery = @"Update return_order 
                                    Set 
                                    ss = 2,
                                    modified_by AS ModifiedBy=@ModifiedBy,
                                    modified_time AS ModifiedTime=@ModifiedTime,
                                    server_modified_time AS ServerModifiedTime=@ServerModifiedTime,
                                    is_tax_applicable AS IsTaxApplicable=@IsTaxApplicable,
                                    status AS Status=@Status,
                                    order_type AS OrderType=@OrderType 
                                    WHERE uid=@UID";
                          

                            count += await ExecuteNonQueryAsync(returnOrderQuery, updateReturnOrderMaster, connection, transaction);
                        }
                        if (updateReturnOrderMaster.ReturnOrderLineList != null && updateReturnOrderMaster.ReturnOrderLineList.Any()) {

                            var returnOrderLineQuery = @"Update return_order_line 
                                Set 
                                ss = 2,
                                modified_by AS ModifiedBy=@ModifiedBy,
                                modified_time AS ModifiedTime=@ModifiedTime,
                                server_modified_time AS ServerModifiedTime=@ServerModifiedTime,
                                reason_code AS ReasonCode=@ReasonCode,
                                reason_text AS ReasonText=@ReasonText 
                                WHERE 
                                return_order_uid=@ReturnOrderUID";
                            count += await ExecuteNonQueryAsync(returnOrderLineQuery, updateReturnOrderMaster, connection, transaction);
                           
                        }
                        //if (updateReturnOrderMaster.StoreHistory != null)
                        //{
                        //    count += await _storeHistoryDL.UpdateStoreHistory(updateReturnOrderMaster.StoreHistory, connection, transaction);
                        //}
                        transaction.Commit();
                        
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        return count;
    }

    public async Task<List<IReturnSummaryItemView>> GetReturnSummaryItemView(DateTime startDate, DateTime endDate, string storeUID = "", List<FilterCriteria>? filterCriterias = null)
    {
        StringBuilder sql = new StringBuilder("""
            Select RO.uid AS UID, S.code as StoreCode, S.[name] as StoreName, RO.return_order_number as OrderNumber, 
            (COALESCE(A.Line1, '') || ' ' || COALESCE(A.Line2, '')) AS 'address', RO.order_type, RO.status as OrderStatus,RO.order_date, 
            Ro.order_date as date, RO.net_amount as OrderAmount, C.name as CurrencyLabel,CASE WHEN RO.ss = 0 THEN 1 ELSE 0 END AS IsPosted, 
            RO.line_count as LineCount, SO.sales_order_number As SalesOrderNumber
            from return_order RO 
            inner join  store S ON RO.store_uid = S.UID
            inner join currency C ON Ro.currency_uid = C.UID
            LEFT JOIN [address] A ON A.Linked_Item_Type = 'Store' AND A.Linked_Item_UID = S.uid AND A.Is_Default = 1 Left join Sales_Order SO
            ON RO.Sales_order_uid = SO.UID
            WHERE RO.order_date  BETWEEN @startDate AND @endDate
            """);
        if (!storeUID.IsNullOrEmpty())
        {
            sql.Append(@"AND store_uid = @storeUID");
        }
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            { "startDate", startDate },
            { "endDate", endDate },
            { "storeUID", storeUID },
        };

        return await ExecuteQueryAsync<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnSummaryItemView>(sql.ToString(), parameters);
    }

    //public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderMasterByUID(string UID)
    //{
    //    try
    //    {
    //        Dictionary<string, object?> parameters = new Dictionary<string, object?>
    //        {
    //            {" @return_order_uid" , UID}
    //        };
    //        var sql = @"SELECT RO.id AS ReturnOrderId, RO.uid AS ReturnOrderUID, RO.created_by AS ReturnOrderCreatedBy,
    //            RO.created_time AS ReturnOrderCreatedTime, RO.modified_by AS ReturnOrderModifiedBy, RO.modified_time AS ReturnOrderModifiedTime,
    //            RO.server_add_time AS ReturnOrderServerAddTime, RO.server_modified_time AS ReturnOrderServerModifiedTime,
    //            RO.return_order_number, RO.draft_order_number, RO.job_position_uid, RO.emp_uid, RO.org_uid, RO.distribution_channel_uid,
    //            RO.store_uid, RO.is_tax_applicable, RO.route_uid, RO.beat_history_uid, RO.store_history_uid, RO.status, RO.order_type,
    //            RO.order_date, RO.currency_uid, RO.total_amount, RO.total_line_discount, RO.total_cash_discount, RO.total_header_discount,
    //            RO.total_discount, RO.total_excise_duty, RO.line_tax_amount, RO.header_tax_amount, RO.total_tax, RO.net_amount,
    //            RO.total_fake_amount, RO.line_count, RO.qty_count, RO.notes, RO.is_offline, RO.latitude, RO.longitude,
    //            RO.delivered_by_org_uid, RO.ss, RO.source, RO.promotion_uid, RO.total_line_tax, RO.total_header_tax 
    //            FROM return_order RO
    //            WHERE RO.uid = @return_order_uid";
    //        IReturnOrder returnOrder = await ExecuteSingleAsync<IReturnOrder>(sql, parameters);
    //        string sqlLine = $@"select * from return_order_line where return_order_uid = @ReturnOrderUID";
    //        List<IReturnOrderLine> returnOrderLines = await ExecuteQueryAsync<IReturnOrderLine>(sqlLine, parameters);
    //        IReturnOrderMaster returnOrderMaster = new ReturnOrderMaster();
    //        returnOrderMaster.ReturnOrder = returnOrder;
    //        returnOrderMaster.ReturnOrderLineList = returnOrderLines;
    //        return returnOrderMaster;
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}
    public async Task<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster> GetReturnOrderMasterByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"@return_order_uid", UID} // Ensure the parameter key matches the SQL placeholder
        };

            var sql = @"SELECT RO.id AS ReturnOrderId, RO.uid AS ReturnOrderUID, RO.created_by AS ReturnOrderCreatedBy,
            RO.created_time AS ReturnOrderCreatedTime, RO.modified_by AS ReturnOrderModifiedBy, RO.modified_time AS ReturnOrderModifiedTime,
            RO.server_add_time AS ReturnOrderServerAddTime, RO.server_modified_time AS ReturnOrderServerModifiedTime,
            RO.return_order_number, RO.draft_order_number, RO.job_position_uid, RO.emp_uid, RO.org_uid, RO.distribution_channel_uid,
            RO.store_uid, RO.is_tax_applicable, RO.route_uid, RO.beat_history_uid, RO.store_history_uid, RO.status, RO.order_type,
            RO.order_date, RO.currency_uid, RO.total_amount, RO.total_line_discount, RO.total_cash_discount, RO.total_header_discount,
            RO.total_discount, RO.total_excise_duty, RO.line_tax_amount, RO.header_tax_amount, RO.total_tax, RO.net_amount,
            RO.total_fake_amount, RO.line_count, RO.qty_count, RO.notes, RO.is_offline, RO.latitude, RO.longitude,
            RO.delivered_by_org_uid, RO.ss, RO.source, RO.promotion_uid, RO.total_line_tax, RO.total_header_tax 
            FROM return_order RO
            WHERE RO.uid = @return_order_uid";

            IReturnOrder returnOrder = await ExecuteSingleAsync<IReturnOrder>(sql, parameters);

            string sqlLine = @"SELECT * FROM return_order_line WHERE return_order_uid = @return_order_uid"; // Ensure consistency in parameter name
            List<IReturnOrderLine> returnOrderLines = await ExecuteQueryAsync<IReturnOrderLine>(sqlLine, parameters);

            IReturnOrderMaster returnOrderMaster = new ReturnOrderMaster
            {
                ReturnOrder = returnOrder,
                ReturnOrderLineList = returnOrderLines
            };

            return returnOrderMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<(List<IReturnOrder>, List<IReturnOrderLine>)> SelectReturnOrderMasterByUID(string UID)
    {
        throw new NotImplementedException();
    }
    public async Task<int> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string Status)
    {
        try
        {
            var sql = @"UPDATE return_order SET 
                            status AS Status = @Status 
                             WHERE uid IN (@ReturnOrderUIDs);";
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"Status",Status },
                {"ReturnOrderUIDs",string.Join(",",returnOrderUIDs) }
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<IReturnOrderInvoiceMaster> GetReturnOrderInvoiceMasterByUID(string returnOrderUID)
    {
        try
        {
            IReturnOrderInvoiceMaster returnOrderInvoiceMaster = new ReturnOrderInvoiceMaster();
            string returnOrderSql = """
                        SELECT RO.UID AS ReturnOrderUID, RO.return_order_number AS ReturnOrderNumber, RO.order_date as OrderDate,
                        SO.Sales_Order_Number AS SalesOrderNumber, Ro.order_type as OrderType, RO.Status as OrderStatus, Ro.line_count as SKUCount
                        from Return_Order RO Left join Sales_Order SO on RO.Sales_Order_uid = SO.uid where RO.UID = @ReturnOrderUID
                        """;

            string returnOrderLinesql = """
                                        select s.code as SKUCode, s.name as SKUName,rol.uom as UOM, rol.qty as  orderqty, rol.sku_type as ItemType, 
                                        rol.reason_text as Reason  from return_order_line rol INNER join sku s on rol.sku_uid = s.uid
                                        where rol.return_order_uid = @ReturnOrderUID
                                        """;
            returnOrderInvoiceMaster.ReturnOrderInvoice = await ExecuteSingleAsync<IReturnOrderInvoice>(returnOrderSql, new { ReturnOrderUID = returnOrderUID });
            returnOrderInvoiceMaster.ReturnOrderLineInvoices = await ExecuteQueryAsync<IReturnOrderLineInvoice>(returnOrderLinesql, new { ReturnOrderUID = returnOrderUID });
            return returnOrderInvoiceMaster;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
