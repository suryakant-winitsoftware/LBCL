
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Modules.ReturnOrder.DL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.DL.Classes;

public class MSSQLReturnOrderDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IReturnOrderDL
{
    protected IStoreHistoryDL _storeHistoryDL;
    public MSSQLReturnOrderDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<IReturnOrder>> SelectAllReturnOrderDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@" Select * From (SELECT 
                                            id AS Id,
                                            uid AS Uid,
                                            created_by AS CreatedBy,
                                            created_time AS CreatedTime,
                                            modified_by AS ModifiedBy,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime,
                                            return_order_number AS ReturnOrderNumber,
                                            draft_order_number AS DraftOrderNumber,
                                            job_position_uid AS JobPositionUid,
                                            emp_uid AS EmpUid,
                                            org_uid AS OrgUid,
                                            distribution_channel_uid AS DistributionChannelUid,
                                            store_uid AS StoreUid,
                                            is_tax_applicable AS IsTaxApplicable,
                                            route_uid AS RouteUid,
                                            beat_history_uid AS BeatHistoryUid,
                                            store_history_uid AS StoreHistoryUid,
                                            status AS Status,
                                            order_type AS OrderType,
                                            order_date AS OrderDate,
                                            currency_uid AS CurrencyUid,
                                            total_amount AS TotalAmount,
                                            total_line_discount AS TotalLineDiscount,
                                            total_cash_discount AS TotalCashDiscount,
                                            total_header_discount AS TotalHeaderDiscount,
                                            total_discount AS TotalDiscount,
                                            total_excise_duty AS TotalExciseDuty,
                                            line_tax_amount AS LineTaxAmount,
                                            header_tax_amount AS HeaderTaxAmount,
                                            total_tax AS TotalTax,
                                            net_amount AS NetAmount,
                                            total_fake_amount AS TotalFakeAmount,
                                            line_count AS LineCount,
                                            qty_count AS QtyCount,
                                            notes AS Notes,
                                            is_offline AS IsOffline,
                                            latitude AS Latitude,
                                            longitude AS Longitude,
                                            delivered_by_org_uid AS DeliveredByOrgUid,
                                            promotion_uid AS PromotionUid,
                                            total_line_tax AS TotalLineTax,
                                            total_header_tax AS TotalHeaderTax
                                        FROM 
                                            return_order)As SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                            id AS Id,
                                            uid AS Uid,
                                            created_by AS CreatedBy,
                                            created_time AS CreatedTime,
                                            modified_by AS ModifiedBy,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime,
                                            return_order_number AS ReturnOrderNumber,
                                            draft_order_number AS DraftOrderNumber,
                                            job_position_uid AS JobPositionUid,
                                            emp_uid AS EmpUid,
                                            org_uid AS OrgUid,
                                            distribution_channel_uid AS DistributionChannelUid,
                                            store_uid AS StoreUid,
                                            is_tax_applicable AS IsTaxApplicable,
                                            route_uid AS RouteUid,
                                            beat_history_uid AS BeatHistoryUid,
                                            store_history_uid AS StoreHistoryUid,
                                            status AS Status,
                                            order_type AS OrderType,
                                            order_date AS OrderDate,
                                            currency_uid AS CurrencyUid,
                                            total_amount AS TotalAmount,
                                            total_line_discount AS TotalLineDiscount,
                                            total_cash_discount AS TotalCashDiscount,
                                            total_header_discount AS TotalHeaderDiscount,
                                            total_discount AS TotalDiscount,
                                            total_excise_duty AS TotalExciseDuty,
                                            line_tax_amount AS LineTaxAmount,
                                            header_tax_amount AS HeaderTaxAmount,
                                            total_tax AS TotalTax,
                                            net_amount AS NetAmount,
                                            total_fake_amount AS TotalFakeAmount,
                                            line_count AS LineCount,
                                            qty_count AS QtyCount,
                                            notes AS Notes,
                                            is_offline AS IsOffline,
                                            latitude AS Latitude,
                                            longitude AS Longitude,
                                            delivered_by_org_uid AS DeliveredByOrgUid,
                                            promotion_uid AS PromotionUid,
                                            total_line_tax AS TotalLineTax,
                                            total_header_tax AS TotalHeaderTax
                                        FROM 
                                            return_order)As SubQuery");
            }
            var parameters = new Dictionary<string, object?>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<IReturnOrder>(filterCriterias, sbFilterCriteria, parameters); ;
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

            IEnumerable<Model.Interfaces.IReturnOrder> returnOrders = await ExecuteQueryAsync<Model.Interfaces.IReturnOrder>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<IReturnOrder> pagedResponse = new PagedResponse<IReturnOrder>
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
    public async Task<IReturnOrder> SelectReturnOrderByUID(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"UID" , UID}
        };
        var sql = @"SELECT 
                                id AS Id,
                                uid AS Uid,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                return_order_number AS ReturnOrderNumber,
                                draft_order_number AS DraftOrderNumber,
                                job_position_uid AS JobPositionUid,
                                emp_uid AS EmpUid,
                                org_uid AS OrgUid,
                                distribution_channel_uid AS DistributionChannelUid,
                                store_uid AS StoreUid,
                                is_tax_applicable AS IsTaxApplicable,
                                route_uid AS RouteUid,
                                beat_history_uid AS BeatHistoryUid,
                                store_history_uid AS StoreHistoryUid,
                                status AS Status,
                                order_type AS OrderType,
                                order_date AS OrderDate,
                                currency_uid AS CurrencyUid,
                                total_amount AS TotalAmount,
                                total_line_discount AS TotalLineDiscount,
                                total_cash_discount AS TotalCashDiscount,
                                total_header_discount AS TotalHeaderDiscount,
                                total_discount AS TotalDiscount,
                                total_excise_duty AS TotalExciseDuty,
                                line_tax_amount AS LineTaxAmount,
                                header_tax_amount AS HeaderTaxAmount,
                                total_tax AS TotalTax,
                                net_amount AS NetAmount,
                                total_fake_amount AS TotalFakeAmount,
                                line_count AS LineCount,
                                qty_count AS QtyCount,
                                notes AS Notes,
                                is_offline AS IsOffline,
                                latitude AS Latitude,
                                longitude AS Longitude,
                                delivered_by_org_uid AS DeliveredByOrgUid,
                                promotion_uid AS PromotionUid,
                                total_line_tax AS TotalLineTax,
                                total_header_tax AS TotalHeaderTax
                            FROM 
                                return_order where uid=@UID;";
        Model.Interfaces.IReturnOrder ReturnOrderList = await ExecuteSingleAsync<Model.Interfaces.IReturnOrder>(sql, parameters);
        return ReturnOrderList;
    }
    public async Task<int> CreateReturnOrder(IReturnOrder returnOrder)
    {
        int count = -1;
        try
        {
            var sql = @"INSERT INTO return_order (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, return_order_number, 
                          draft_order_number, job_position_uid, emp_uid, org_uid, distribution_channel_uid, store_uid, is_tax_applicable, route_uid, 
                          beat_history_uid, store_history_uid, status, order_type, order_date, currency_uid, total_amount, total_line_discount, 
                          total_cash_discount, total_header_discount, total_discount, total_excise_duty, line_tax_amount, header_tax_amount, total_tax, 
                          net_amount, total_fake_amount, line_count, qty_count, notes, is_offline, latitude, longitude, delivered_by_org_uid, promotion_uid, 
                          total_line_tax, total_header_tax) VALUES (@UID, @CreatedBy,
                          @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @ReturnOrderNumber, @DraftOrderNumber, @JobPositionUID,
                          @EmpUID, @OrgUID, @DistributionChannelUID, @StoreUID, @IsTaxApplicable, @RouteUID, @BeatHistoryUID, @StoreHistoryUID, @Status, 
                          @OrderType, @OrderDate, @CurrencyUID, @TotalAmount, @TotalLineDiscount, @TotalCashDiscount, @TotalHeaderDiscount, @TotalDiscount, 
                          @TotalExciseDuty, @LineTaxAmount, @HeaderTaxAmount, @TotalTax, @NetAmount, @TotalFakeAmount, @LineCount, @QtyCount, @Notes, @IsOffline, 
                          @Latitude, @Longitude, @DeliveredByOrgUID,@PromotionUID,@TotalLineTax,@TotalHeaderTax)";

            count= await ExecuteNonQueryAsync(sql, returnOrder);
        }
        catch (Exception)
        {
            throw;
        }
        return count;
    }
    public async Task<int> UpdateReturnOrder(IReturnOrder returnOrder)
    {
        int count = -1;
        try
        {
            var sql = @"UPDATE return_order 
                                            SET modified_by = @ModifiedBy, 
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
                                            WHERE uid = @UID;";
            
            count= await ExecuteNonQueryAsync(sql, returnOrder);

        }
        catch (Exception)
        {
            throw;
        }
        return count;
    }
    public async Task<int> DeleteReturnOrder(string UID)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            {"UID" , UID}
        };
            var sql = @"DELETE  FROM return_order WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch
        {
            throw;
        }
        
    }

    public async Task<int> CreateReturnOrderMaster(ReturnOrderMasterDTO returnOrderMaster)
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
                        if (returnOrderMaster.ReturnOrder == null || returnOrderMaster.ReturnOrderLineList == null || !returnOrderMaster.ReturnOrderLineList.Any())
                            throw new Exception("Invalid data");
                        var returnOrderQuery =
                        """
                        INSERT INTO return_order (
                            uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                        return_order_number, draft_order_number, job_position_uid, emp_uid, org_uid, distribution_channel_uid, store_uid, 
                        is_tax_applicable, route_uid, beat_history_uid, store_history_uid, status, order_type, order_date, currency_uid, 
                        total_amount, total_line_discount, total_cash_discount, total_header_discount, total_discount, total_excise_duty, 
                        line_tax_amount, header_tax_amount, total_tax, net_amount, total_fake_amount, line_count, qty_count, notes, 
                        is_offline, latitude, longitude, delivered_by_org_uid, source, promotion_uid, total_line_tax, total_header_tax,
                        sales_order_uid
                        ) VALUES (
                            @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                        @ReturnOrderNumber, @DraftOrderNumber, @JobPositionUID, @EmpUID, @OrgUID, @DistributionChannelUID, @StoreUID, 
                        @IsTaxApplicable, @RouteUID, @BeatHistoryUID, @StoreHistoryUID, @Status, @OrderType, @OrderDate, @CurrencyUID, 
                        @TotalAmount, @TotalLineDiscount, @TotalCashDiscount, @TotalHeaderDiscount, @TotalDiscount, @TotalExciseDuty, 
                        @LineTaxAmount, @HeaderTaxAmount, @TotalTax, @NetAmount, @TotalFakeAmount, @LineCount, @QtyCount, @Notes, 
                        @IsOffline, @Latitude, @Longitude, @DeliveredByOrgUID, @Source, @PromotionUID, @TotalLineTax, @TotalHeaderTax,
                        @SalesOrderUID
                        );
                        """;
                        count += await ExecuteNonQueryAsync(returnOrderQuery, connection, transaction, returnOrderMaster.ReturnOrder);

                        if (returnOrderMaster.ReturnOrderLineList != null && returnOrderMaster.ReturnOrderLineList.Count > 0)
                        {
                            var returnOrderLineQuery = @"INSERT INTO return_order_line (
                            uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            return_order_uid, line_number, sku_uid, sku_code, sku_type, base_price, unit_price, fake_unit_price, 
                            base_uom, uom, multiplier, qty, qty_bu, approved_qty, returned_qty, total_amount, total_discount, 
                            total_excise_duty, total_tax, net_amount, sku_price_uid, sku_price_list_uid, reason_code, reason_text, 
                            expiry_date, batch_number, sales_order_uid, sales_order_line_uid, remarks, volume, volume_unit, 
                            promotion_uid, net_fake_amount, po_number,tax_data
                                ) VALUES (
                            @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                            @ReturnOrderUID, @LineNumber, @SKUUID, @SKUCode, @SKUType, @BasePrice, @UnitPrice, @FakeUnitPrice, 
                            @BaseUOM, @UoM, @Multiplier, @Qty, @QtyBU, @ApprovedQty, @ReturnedQty, @TotalAmount, @TotalDiscount, 
                            @TotalExciseDuty, @TotalTax, @NetAmount, @SKUPriceUID, @SKUPriceListUID, @ReasonCode, @ReasonText, 
                            @ExpiryDate, @BatchNumber, @SalesOrderUID, @SalesOrderLineUID, @Remarks, @Volume, @VolumeUnit, 
                            @PromotionUID, @NetFakeAmount, @PONumber,@TaxData
                        );";
                            count += await ExecuteNonQueryAsync(returnOrderLineQuery, connection, transaction, returnOrderMaster.ReturnOrderLineList);
                        }
                        if (returnOrderMaster.StoreHistory != null)
                        {

                            await _storeHistoryDL.CreateStoreHistory(returnOrderMaster.StoreHistory, connection, transaction);
                        }
                        transaction.Commit();

                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                return count;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateReturnOrderMaster(ReturnOrderMasterDTO updateReturnOrderMaster)
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
                        if (updateReturnOrderMaster.ReturnOrder != null)
                        {
                            var returnOrderQuery = @"UPDATE return_order 
                                            SET modified_by = @ModifiedBy, 
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
                                            WHERE uid = @UID;";
                            count = await ExecuteNonQueryAsync(returnOrderQuery, connection, transaction, updateReturnOrderMaster.ReturnOrder);
                        }
                        if (updateReturnOrderMaster.ReturnOrderLineList != null && updateReturnOrderMaster.ReturnOrderLineList.Count > 0)
                        {
                            var returnOrderLineQuery = @"UPDATE return_order_line SET 
                                                    modified_by = @ModifiedBy, 
                                                    modified_time = @ModifiedTime, 
                                                    server_modified_time = @ServerModifiedTime, 
                                                    return_order_uid = @ReturnOrderUID, 
                                                    line_number = @LineNumber, 
                                                    sku_uid = @SKUUID, 
                                                    sku_code = @SKUCode, 
                                                    sku_type = @SKUType, 
                                                    base_price = @BasePrice, 
                                                    unit_price = @UnitPrice, 
                                                    fake_unit_price = @FakeUnitPrice, 
                                                    base_uom = @BaseUOM, 
                                                    uom = @UoM, 
                                                    multiplier = @Multiplier, 
                                                    qty = @Qty, 
                                                    qty_bu = @QtyBU, 
                                                    approved_qty = @ApprovedQty, 
                                                    returned_qty = @ReturnedQty, 
                                                    total_amount = @TotalAmount, 
                                                    total_discount = @TotalDiscount, 
                                                    total_excise_duty = @TotalExciseDuty, 
                                                    total_tax = @TotalTax, 
                                                    net_amount = @NetAmount, 
                                                    sku_price_uid = @SKUPriceUID, 
                                                    sku_price_list_uid = @SKUPriceListUID, 
                                                    reason_code = @ReasonCode, 
                                                    reason_text = @ReasonText, 
                                                    expiry_date = @ExpiryDate, 
                                                    batch_number = @BatchNumber, 
                                                    sales_order_uid = @SalesOrderUID, 
                                                    sales_order_line_uid = @SalesOrderLineUID, 
                                                    remarks = @Remarks, 
                                                    volume = @Volume, 
                                                    volume_unit = @VolumeUnit, 
                                                    promotion_uid = @PromotionUID, 
                                                    net_fake_amount = @NetFakeAmount,
                                                    tax_data = @TaxData
                                                     WHERE uid = @UID";
                            count += await ExecuteNonQueryAsync(returnOrderLineQuery, connection, transaction, updateReturnOrderMaster.ReturnOrderLineList);
                        }
                        if (updateReturnOrderMaster.StoreHistory != null)
                        {
                            count += await _storeHistoryDL.UpdateStoreHistory(updateReturnOrderMaster.StoreHistory, connection, transaction);
                        }
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

    public async Task<List<IReturnSummaryItemView>> GetReturnSummaryItemView(DateTime startDate, DateTime endDate,
        string storeUID = "", List<FilterCriteria>? filterCriterias = null)
    {
        try
        {
            StringBuilder sql = new StringBuilder(@"SELECT 
                                                    S.code AS StoreCode, 
                                                    S.name AS StoreName, 
                                                    RO.return_order_number AS OrderNumber, 
                                                    A.line1 + ' ' + A.line2 AS Address, 
                                                    RO.order_type, 
                                                    RO.status AS OrderStatus, 
                                                    RO.order_date AS OrderDate, 
                                                    RO.net_amount AS OrderAmount, 
                                                    C.name AS CurrencyLabel, 
                                                    CASE 
                                                        WHEN RO.ss = 0 THEN CAST(1 AS BIT) 
                                                        ELSE CAST(0 AS BIT) 
                                                    END AS IsPosted, 
                                                    RO.uid AS UID 
                                                FROM 
                                                    return_order RO 
                                                    INNER JOIN store S ON RO.store_uid = S.uid 
                                                    INNER JOIN currency C ON RO.currency_uid = C.uid 
                                                    LEFT JOIN address A ON A.linked_item_type = 'Store' AND A.linked_item_uid = S.uid AND A.is_default = 1 
                                                WHERE 
                                                    RO.order_date BETWEEN @startDate AND @endDate");
            if (!storeUID.IsNullOrEmpty())
            {
                sql.Append(@"AND Store_UID = @storeUID");
            }
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "startDate", startDate },
                { "endDate", endDate },
                { "storeUID", storeUID },
            };
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                sql.Append(" AND ");
                StringBuilder sbFilterCriteria = new StringBuilder();
                AppendFilterCriteria<IReturnOrder>
                    (filterCriterias, sbFilterCriteria, parameters); ;
                sql.Append(sbFilterCriteria);
            }
            List<IReturnSummaryItemView> returnOrders = await ExecuteQueryAsync<IReturnSummaryItemView>(sql.ToString(), parameters);
            return returnOrders;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IReturnOrderMaster> GetReturnOrderMasterByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"ReturnOrderUID" , UID}
            };
            var sql = """
                        SELECT 
                        RO.id AS ReturnOrderId, RO.uid AS ReturnOrderUid, RO.created_by AS ReturnOrderCreatedBy,
                        RO.created_time AS ReturnOrderCreatedTime, RO.modified_by AS ReturnOrderModifiedBy, 
                        RO.modified_time AS ReturnOrderModifiedTime, RO.server_add_time AS ReturnOrderServerAddTime, 
                        RO.server_modified_time AS ReturnOrderServerModifiedTime, RO.return_order_number AS ReturnOrderNumber, 
                        RO.draft_order_number AS DraftOrderNumber, RO.job_position_uid AS JobPositionUid, RO.emp_uid AS EmpUid, 
                        RO.org_uid AS OrgUid, RO.distribution_channel_uid AS DistributionChannelUid,
                        RO.store_uid AS StoreUid, RO.is_tax_applicable AS IsTaxApplicable, RO.route_uid AS RouteUid, 
                        RO.beat_history_uid AS BeatHistoryUid, RO.store_history_uid AS StoreHistoryUid, RO.status AS Status, 
                        RO.order_type AS OrderType, RO.order_date AS OrderDate, RO.currency_uid AS CurrencyUid, 
                        RO.total_amount AS TotalAmount, RO.total_line_discount AS TotalLineDiscount, 
                        RO.total_cash_discount AS TotalCashDiscount, RO.total_header_discount AS TotalHeaderDiscount, 
                        RO.total_discount AS TotalDiscount, RO.total_excise_duty AS TotalExciseDuty, 
                        RO.line_tax_amount AS LineTaxAmount, RO.header_tax_amount AS HeaderTaxAmount, 
                        RO.total_tax AS TotalTax, RO.net_amount AS NetAmount,
                        RO.total_fake_amount AS TotalFakeAmount, RO.line_count AS LineCount, RO.qty_count AS QtyCount, 
                        RO.notes AS Notes, RO.is_offline AS IsOffline, RO.latitude AS Latitude, 
                        RO.longitude AS Longitude, RO.delivered_by_org_uid AS DeliveredByOrgUid, RO.ss AS SS, 
                        RO.source AS Source, RO.promotion_uid AS PromotionUid, RO.total_line_tax AS TotalLineTax, 
                        RO.total_header_tax AS TotalHeaderTax, ROL.id AS ReturnOrderLineId, ROL.uid AS ReturnOrderLineUid, 
                        ROL.created_by AS ReturnOrderLineCreatedBy, ROL.created_time AS ReturnOrderLineCreatedTime,
                        ROL.modified_by AS ReturnOrderLineModifiedBy, ROL.modified_time AS ReturnOrderLineModifiedTime,
                        ROL.server_add_time AS ReturnOrderLineServerAddTime, 
                        ROL.server_modified_time AS ReturnOrderLineServerModifiedTime, 
                        ROL.return_order_uid AS ReturnOrderLineReturnOrderUid, ROL.line_number AS LineNumber, 
                        ROL.sku_uid AS SkuUid, ROL.sku_code AS SkuCode, ROL.sku_type AS SkuType, 
                        ROL.base_price AS BasePrice, ROL.unit_price AS UnitPrice, ROL.fake_unit_price AS FakeUnitPrice, 
                        ROL.base_uom AS BaseUom, ROL.uom AS Uom, ROL.multiplier AS Multiplier, ROL.qty AS Qty, 
                        ROL.qty_bu AS QtyBu, ROL.approved_qty AS ApprovedQty, ROL.returned_qty AS ReturnedQty,
                        ROL.total_amount AS TotalAmountLine, ROL.total_discount AS TotalDiscountLine, 
                        ROL.total_excise_duty AS TotalExciseDutyLine, ROL.total_tax AS TotalTaxLine, 
                        ROL.net_amount AS NetAmountLine, ROL.sku_price_uid AS SkuPriceUid, 
                        ROL.sku_price_list_uid AS SkuPriceListUid, ROL.reason_code AS ReasonCode, 
                        ROL.reason_text AS ReasonText, ROL.expiry_date AS ExpiryDate, ROL.batch_number AS BatchNumber, 
                        ROL.sales_order_uid AS SalesOrderUid, ROL.sales_order_line_uid AS SalesOrderLineUid, 
                        ROL.remarks AS Remarks, ROL.volume AS Volume, ROL.volume_unit AS VolumeUnit, 
                        ROL.promotion_uid AS PromotionUidLine, ROL.net_fake_amount AS NetFakeAmount, 
                        ROT.id AS ReturnOrderTaxId, ROT.uid AS ReturnOrderTaxUid, ROT.created_by AS ReturnOrderTaxCreatedBy,
                        ROT.created_time AS ReturnOrderTaxCreatedTime, ROT.modified_by AS ReturnOrderTaxModifiedBy, 
                        ROT.modified_time AS ReturnOrderTaxModifiedTime, ROT.server_add_time AS ReturnOrderTaxServerAddTime, 
                        ROT.server_modified_time AS ReturnOrderTaxServerModifiedTime, ROT.return_order_uid AS ReturnOrderTaxReturnOrderUid, 
                        ROT.return_order_line_uid AS ReturnOrderLineUidTax, ROT.tax_uid AS TaxUid, ROT.tax_slab_uid AS TaxSlabUid, 
                        ROT.tax_amount AS TaxAmount, ROT.tax_name AS TaxName, ROT.applicable_at AS ApplicableAt, 
                        ROT.dependent_tax_uid AS DependentTaxUid, ROT.dependent_tax_name AS DependentTaxName, 
                        ROT.tax_calculation_type AS TaxCalculationType, ROT.base_tax_rate AS BaseTaxRate, 
                        ROT.range_start AS RangeStart, ROT.range_end AS RangeEnd, ROT.tax_rate AS TaxRate
                    FROM 
                        return_order RO
                        LEFT JOIN return_order_line ROL ON RO.uid = ROL.return_order_uid
                        LEFT JOIN return_order_tax ROT ON ROT.return_order_uid = RO.uid
                    WHERE 
                        RO.uid = @return_order_uid;
                    """;

            DataTable dt = await ExecuteQueryDataTableAsync(sql.ToString(), parameters);
            Dictionary<string, string> returnOrderColumnMappings = new Dictionary<string, string>
            {
                { "ReturnOrderId", "Id" },
                { "ReturnOrderUID", "UID" },
                { "ReturnOrderCreatedBy", "CreatedBy" },
                { "ReturnOrderCreatedTime", "CreatedTime" },
                { "ReturnOrderModifiedBy", "ModifiedBy" },
                { "ReturnOrderModifiedTime", "ModifiedTime" },
                { "ReturnOrderServerAddTime", "ServerAddTime" },
                { "ReturnOrderServerModifiedTime", "ServerModifiedTime" },
            };

            Dictionary<string, string> returnOrderLineColumnMappings = new Dictionary<string, string>
            {
                { "ReturnOrderLineId", "Id" },
                { "ReturnOrderLineUID", "UID" },
                { "ReturnOrderLineCreatedBy", "CreatedBy" },
                { "ReturnOrderLineCreatedTime", "CreatedTime" },
                { "ReturnOrderLineModifiedBy", "ModifiedBy" },
                { "ReturnOrderLineModifiedTime", "ModifiedTime" },
                { "ReturnOrderLineServerAddTime", "ServerAddTime" },
                { "ReturnOrderLineServerModifiedTime", "ServerModifiedTime" },
                { "ReturnOrderLineReturnOrderUID", "ReturnOrderUID" },
            };
            Dictionary<string, string> returnOrderTaxColumnMappings = new Dictionary<string, string>
            {
                { "ReturnOrderTaxId", "Id" },
                { "ReturnOrderTaxUID", "UID" },
                { "ReturnOrderTaxCreatedBy", "CreatedBy" },
                { "ReturnOrderTaxCreatedTime", "CreatedTime" },
                { "ReturnOrderTaxModifiedBy", "ModifiedBy" },
                { "ReturnOrderTaxModifiedTime", "ModifiedTime" },
                { "ReturnOrderTaxServerAddTime", "ServerAddTime" },
                { "ReturnOrderTaxServerModifiedTime", "ServerModifiedTime" },
                { "ReturnOrderTaxReturnOrderUID", "ReturnOrderUID" },
            };
            List<ReturnOrderMaster> returnOrderMasterList = new List<ReturnOrderMaster>();
            ReturnOrderMaster returnOrderMaster = null;
            foreach (DataRow row in dt.Rows)
            {
                Type returnOrderType = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrder>().GetType();
                Type returnOrderLineType = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrderLine>().GetType();
                Type returnOrderTaxType = _serviceProvider.GetRequiredService<Model.Interfaces.IReturnOrderTax>().GetType();

                var returnOrder = ConvertDataTableToObject<IReturnOrder>(row, returnOrderColumnMappings, returnOrderType);
                var returnOrderLine = ConvertDataTableToObject<IReturnOrderLine>(row, returnOrderLineColumnMappings, returnOrderLineType);
                var returnOrderTax = ConvertDataTableToObject<IReturnOrderTax>(row, returnOrderTaxColumnMappings, returnOrderTaxType);

                returnOrderMaster = returnOrderMasterList.Find(x => x.ReturnOrder.UID == returnOrder.UID);
                if (returnOrderMaster == null)
                {
                    returnOrderMaster = new ReturnOrderMaster
                    {
                        ReturnOrder = returnOrder,
                        ReturnOrderLineList = new List<IReturnOrderLine>(),
                        ReturnOrderTaxList = new List<IReturnOrderTax>()
                    };

                    returnOrderMasterList.Add(returnOrderMaster);
                }

                returnOrderMaster.ReturnOrderLineList.Add(returnOrderLine);
                returnOrderMaster.ReturnOrderTaxList.Add(returnOrderTax);

            }
            return returnOrderMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<(List<Model.Interfaces.IReturnOrder>, List<Model.Interfaces.IReturnOrderLine>)>
      SelectReturnOrderMasterByUID(string UID)
    {
        try
        {
            Dictionary<string, object?> Parameters = new Dictionary<string, object?>
            {
                { "UID", UID },
            };
            var returnOrderSql = new StringBuilder(@"SELECT 
                                        id AS Id,
                                        uid AS Uid,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        return_order_number AS ReturnOrderNumber,
                                        draft_order_number AS DraftOrderNumber,
                                        job_position_uid AS JobPositionUid,
                                        emp_uid AS EmpUid,
                                        org_uid AS OrgUid,
                                        distribution_channel_uid AS DistributionChannelUid,
                                        store_uid AS StoreUid,
                                        is_tax_applicable AS IsTaxApplicable,
                                        route_uid AS RouteUid,
                                        beat_history_uid AS BeatHistoryUid,
                                        store_history_uid AS StoreHistoryUid,
                                        status AS Status,
                                        order_type AS OrderType,
                                        order_date AS OrderDate,
                                        currency_uid AS CurrencyUid,
                                        total_amount AS TotalAmount,
                                        total_line_discount AS TotalLineDiscount,
                                        total_cash_discount AS TotalCashDiscount,
                                        total_header_discount AS TotalHeaderDiscount,
                                        total_discount AS TotalDiscount,
                                        total_excise_duty AS TotalExciseDuty,
                                        line_tax_amount AS LineTaxAmount,
                                        header_tax_amount AS HeaderTaxAmount,
                                        total_tax AS TotalTax,
                                        net_amount AS NetAmount,
                                        total_fake_amount AS TotalFakeAmount,
                                        line_count AS LineCount,
                                        qty_count AS QtyCount,
                                        notes AS Notes,
                                        is_offline AS IsOffline,
                                        latitude AS Latitude,
                                        longitude AS Longitude,
                                        delivered_by_org_uid AS DeliveredByOrgUid,
                                        ss AS Ss,
                                        source AS Source,
                                        promotion_uid AS PromotionUid,
                                        total_line_tax AS TotalLineTax,
                                        total_header_tax AS TotalHeaderTax,
                                        sales_order_uid AS SalesOrderUID
                                    FROM 
                                        return_order 
                                    WHERE 
                                        uid = @UID; ");

            List<Model.Interfaces.IReturnOrder> returnOrdersList = await ExecuteQueryAsync<Model.Interfaces.IReturnOrder>(returnOrderSql.ToString(), Parameters);

            var returnOrderLineSQL = new StringBuilder(@"SELECT 
                                            id AS Id,
                                            uid AS Uid,
                                            created_by AS CreatedBy,
                                            created_time AS CreatedTime,
                                            modified_by AS ModifiedBy,
                                            modified_time AS ModifiedTime,
                                            server_add_time AS ServerAddTime,
                                            server_modified_time AS ServerModifiedTime,
                                            return_order_uid AS ReturnOrderUid,
                                            line_number AS LineNumber,
                                            sku_uid AS SkuUuid,
                                            sku_code AS SkuCode,
                                            sku_type AS SkuType,
                                            base_price AS BasePrice,
                                            unit_price AS UnitPrice,
                                            fake_unit_price AS FakeUnitPrice,
                                            base_uom AS BaseUom,
                                            uom AS Uom,
                                            multiplier AS Multiplier,
                                            qty AS Qty,
                                            qty_bu AS QtyBu,
                                            approved_qty AS ApprovedQty,
                                            returned_qty AS ReturnedQty,
                                            total_amount AS TotalAmount,
                                            total_discount AS TotalDiscount,
                                            total_excise_duty AS TotalExciseDuty,
                                            total_tax AS TotalTax,
                                            net_amount AS NetAmount,
                                            sku_price_uid AS SkuPriceUid,
                                            sku_price_list_uid AS SkuPriceListUid,
                                            reason_code AS ReasonCode,
                                            reason_text AS ReasonText,
                                            expiry_date AS ExpiryDate,
                                            batch_number AS BatchNumber,
                                            sales_order_uid AS SalesOrderUid,
                                            sales_order_line_uid AS SalesOrderLineUid,
                                            remarks AS Remarks,
                                            volume AS Volume,
                                            volume_unit AS VolumeUnit,
                                            promotion_uid AS PromotionUid,
                                            net_fake_amount AS NetFakeAmount,
                                            tax_data AS TaxData
                                        FROM 
                                            return_order_line
                                        WHERE 
                                        return_order_uid = @UID ");
            List<Model.Interfaces.IReturnOrderLine> returnOrderLineList = await ExecuteQueryAsync<Model.Interfaces.IReturnOrderLine>(returnOrderLineSQL.ToString(), Parameters);
            return (returnOrdersList, returnOrderLineList);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateReturnOrderStatus(List<string> returnOrderUIDs, string Status)
    {
        try
        {
            var sql = @"UPDATE return_order 
                                SET 
                                    status = @Status 
                                WHERE 
                                    uid IN (SELECT value FROM STRING_SPLIT(@ReturnOrderUIDs, ','));";
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
