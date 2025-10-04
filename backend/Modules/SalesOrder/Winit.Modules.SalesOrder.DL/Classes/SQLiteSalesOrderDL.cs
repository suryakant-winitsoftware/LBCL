using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.SalesOrder.DL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.StockUpdater.DL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.DL.Classes;

public class SQLiteSalesOrderDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISalesOrderDL
{
    public Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL _stockUpdaterDL;
    protected ICollectionModuleDL _collectionModuleDL;
    public SQLiteSalesOrderDL(IServiceProvider serviceProvider, Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL stockUpdaterDL,
        ICollectionModuleDL collectionModuleDL) : base(serviceProvider)
    {
        _stockUpdaterDL = stockUpdaterDL;
        _collectionModuleDL = collectionModuleDL;
    }
    public async Task<IEnumerable<ISalesOrderViewModel>> SelectSalesOrderDetailsAll(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias)
    {
        try
        {
            StringBuilder sql = new(@"SELECT SO.id AS SalesOrderId, SO.uid AS SalesOrderUID, SO.created_by As SalesOrderCreatedBy, 
                SO.created_time AS SalesOrderCreatedTime, SO.modified_by As SalesOrderModifiedBy, SO.modified_time As SalesOrderModifiedTime, 
                SO.server_add_time As SalesOrderServerAddTime, SO.server_modified_time As SalesOrderServerModifiedTime, 
                SO.sales_order_number AS SalesOrderNumber, SO.draft_order_number AS DraftOrderNumber, SO.org_uid AS OrgUID, 
                SO.distribution_channel_uid AS DistributionChannelUID, SO.delivered_by_org_uid AS DeliveredByOrgUID,
                SO.store_uid AS StoreUID, SO.status AS Status, SO.order_type AS OrderType, SO.order_date AS SalesOrderOrderDate, 
                SO.customer_po AS SalesOrderCustomerPO, SO.currency_uid AS CurrencyUID, SO.payment_type AS PaymentType, 
                SO.total_amount AS TotalAmount, SO.total_discount AS TotalDiscount, SO.total_tax AS TotalTax, SO.net_amount AS SalesOrderNetAmount, 
                SO.line_count AS LineCount, SO.qty_count AS QtyCount, SO.total_fake_amount AS TotalFakeAmount, 
                SO.reference_number AS SalesOrderReferenceNumber, SO.source AS Source, SO.cash_sales_customer AS CashSalesCustomer,
                SO.cash_sales_address AS CashSalesAddress, so.id AS Id, so.uid AS UID, 
                so.created_by As CreatedBy,so.created_time As CreatedTime, so.modified_by As ModifiedBy,
                so.modified_time As ModifiedTime,so.server_add_time AS ServerAddTime, 
                so.server_modified_time As ServerModifiedTime, so.sales_order_uid AS SalesOrderUID, 
                so.job_position_uid AS JobPositionUID, so.emp_uid AS EmpUID, so.beat_history_uid AS BeatHistoryUID, so.route_uid AS RouteUID, 
                so.store_history_uid AS StoreHistoryUID, so.total_credit_limit AS TotalCreditLimit, so.available_credit_limit AS AvailableCreditLimit, 
                so.expected_delivery_date AS ExpectedDeliveryDate, so.delivered_date_time AS DeliveredDateTime, so.latitude AS Latitude,
                so.longitude AS Longitude, so.is_offline AS IsOffline, so.credit_days AS CreditDays, so.notes AS Notes, 
                so.delivery_instructions AS DeliveryInstructions, so.remarks AS Remarks, so.is_temperature_check_enabled AS IsTemperatureCheckEnabled,
                so.always_printed_flag AS AlwaysPrintedFlag, so.purchase_order_no_required_flag AS PurchaseOrderNoRequiredFlag, 
                so.is_with_printed_invoices_flag AS IsWithPrintedInvoicesFlag
                FROM sales_order SO ");
            Dictionary<string, object?> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                _ = sql.Append(" WHERE ");
                AppendFilterCriteria(filterCriterias, sql, parameters);
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }
            DataTable dt = await ExecuteQueryDataTableAsync(sql.ToString(), parameters);

            // Create mappings for SalesOrder properties
            Dictionary<string, string> salesOrderColumnMappings = new()
            {
                { "SalesOrderId", "Id" },
                { "SalesOrderUID", "UID" },
                { "SalesOrderCreatedBy", "CreatedBy" },
                { "SalesOrderCreatedTime", "CreatedTime" },
                { "SalesOrderModifiedBy", "ModifiedBy" },
                { "SalesOrderModifiedTime", "ModifiedTime" },
                { "SalesOrderServerAddTime", "ServerAddTime" },
                { "SalesOrderServerModifiedTime", "ServerModifiedTime" },
            };

            List<ISalesOrderViewModel>? salesOrderViewModelList = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                salesOrderViewModelList = new List<ISalesOrderViewModel>();
                foreach (DataRow row in dt.Rows)
                {
                    IFactory salesOrderFactory = new Model.Classes.SalesOrderFactory(_serviceProvider.GetRequiredService<Model.Interfaces.ISalesOrder>().GetType().Name);

                    ISalesOrder salesOrder = ConvertDataTableToObject<SalesOrder.Model.Interfaces.ISalesOrder>(row, salesOrderFactory, salesOrderColumnMappings);


                    SalesOrderViewModel salesOrderDetailsModel = new()
                    {
                        SalesOrder = salesOrder,
                    };
                    salesOrderViewModelList.Add(salesOrderDetailsModel);
                }
            }

            return salesOrderViewModelList;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderViewModel>> SelectSalesOrderByUID(string SalesOrderUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
                {"SalesOrderUID" , SalesOrderUID}
            };
            string sql = @"SELECT SO.Id AS SalesOrderId, SO.UID AS SalesOrderUID, SO.CreatedBy AS SalesOrderCreatedBy, SO.CreatedTime AS SalesOrderCreatedTime, 
                            SO.ModifiedBy AS SalesOrderModifiedBy, SO.ModifiedTime AS SalesOrderModifiedTime, SO.ServerAddTime AS SalesOrderServerAddTime,
                            SO.ServerModifiedTime AS SalesOrderServerModifiedTime,SO.company_uid AS CompanyUID, SO.sales_order_number AS SalesOrderNumber, 
                            SO.draft_order_number AS DraftOrderNumber, SO.org_uid AS OrgUID, SO.distribution_channel_uid AS DistributionChannelUID, 
                            SO.delivered_by_org_uid AS DeliveredByOrdUID, SO.store_uid AS StoreUID, SO.status AS Status, SO.order_type AS OrderType, 
                            SO.order_date AS OrderDate, SO.customer_po AS CustomerPO, SO.currency_uid AS CurrencyUID, SO.payment_type AS PaymentType, 
                            SO.total_amount AS TotalAmount, SO.total_discount AS TotalDiscount, SO.total_tax AS TotalTax, SO.net_amount AS NetAmount, 
                            SO.line_count AS LineCount, SO.qty_count AS QtyCount, SO.total_fake_amount AS TotalFakeAmount, SO.reference_number AS ReferenceNumber, 
                            SO.source AS Source,SO.cash_sales_customer AS CashSalesCustomer,SO.cash_sales_address AS CashSalesAddress, so.id AS Id, 
                            so.uid AS UID, so.created_by AS CreatedBy,so.created_time AS CreatedTime,
                            so.modified_by AS ModifiedBy,so.modified_time AS ModifiedTime,so.server_add_time AS ServerAddTime,
                            so.server_modified_time AS ServerModifiedTime,so.sales_order_uid AS SalesOrderUID, so.job_position_uid AS JobPositionUID, 
                            so.emp_uid AS EmpUID, so.beat_history_uid AS BeatHistoryUID, so.route_uid AS RouteUID, so.store_history_uid AS StoreHistoryUID,
                            so.total_credit_limit AS TotalCreditLimit, so.available_credit_limit AS AvailableCreditLimit, so.expected_delivery_date AS ExpectedDeliveryDate, 
                            so.delivered_date_time AS DeliveredDateTime, so.latitude AS Latitude, so.longitude AS Longitude, so.is_offline AS IsOffline, 
                            so.credit_days AS CreditDays, so.notes AS Notes, so.delivery_instructions AS DeliveryInstructions, so.remarks AS Remarks, 
                            so.is_temperature_check_enabled AS IsTemperatureCheckEnabled, so.always_printed_flag AS AlwaysPrintedFlag, 
                            so.purchase_order_no_required_flag AS PurchaseOrderNoRequiredFlag, so.is_with_printed_invoices_flag AS IsWithPrintedInvoices,
                            SOL.id AS SalesOrderLineId ,SOL.uid AS SalesOrderLineUID, SOL.created_by AS SalesOrderLineCreatedBy,SOL.created_time AS SalesOrderLineCreatedTime,
                            SOL.modified_by AS SalesOrderLineModifiedBy, SOL.modified_time AS SalesOrderLineModifiedTime,SOL.server_add_time AS SalesOrderLineServerAddTime,
			                SOL.server_modified_time AS SalesOrderLineServerModifiedTime,SOL.sales_order_uid AS SalesOrderLineSalesOrderUID,SOL.line_number AS LineNumber,
                            SOL.item_code AS ItemCode, SOL.item_type AS ItemType,SOL.base_price AS BasePrice,SOL.unit_price AS UnitPrice, SOL.fake_unit_price AS FakeUnitPrice,
                            SOL.base_uom AS BaseUOM,SOL.uom AS UoM,SOL.uom_conversion_to_bu AS UOMConversionToBU,SOL.reco_uom AS RecoUOM,SOL.reco_qty AS RecoQty,
                            SOL.reco_uom_conversion_to_bu AS RecoUOMConversionToBU,SOL.reco_qty_bu AS RecoQtyBU,SOL.model_qty_bu AS ModelQtyBU,SOL.qty AS Qty,
                            SOL.qty_bu AS QtyBU,SOL.van_qty_bu AS VanQtyBU,SOL.delivered_qty AS DeliveredQty,SOL.missed_qty AS MissedQty,SOL.returned_qty AS ReturnedQty,
                            SOL.total_amount AS SalesOrderLineTotalAmount,SOL.total_discount AS TotalDiscount,SOL.line_tax_amount AS LineTaxAmount,SOL.prorata_tax_amount AS ProrataTaxAmount,
                            SOL.total_tax AS SalesOrderLineTotalTax,SOL.net_amount AS NetAmount,SOL.net_fake_amount AS NetFakeAmount,SOL.sku_price_uid AS SKUPriceUID,
			                SOL.prorata_discount_amount AS ProrataDiscountAmount,SOL.line_discount_amount AS LineDiscountAmount,SOL.mrp AS MRP,SOL.cost_unit_price AS CostUnitPrice,
                            SOL.parent_uid AS ParentUID,SOL.is_promotion_applied AS IsPromotionApplied,SOL.volume AS Volume,SOL.volume_unit AS VolumeUnit,
			                SOL.weight AS Weight,SOL.weight_unit AS WeightUnit,SOL.stock_type AS StockType,SOL.remarks AS Remarks
                            FROM sales_order SO
			                INNER JOIN sales_order_line SOL ON SOL.sales_order_uid=so.sales_order_uid WHERE SOL.sales_order_uid=@SalesOrderUID";
            DataTable dt = await ExecuteQueryDataTableAsync(sql.ToString(), parameters);
            Dictionary<string, string> salesOrderColumnMappings = new()
            {
                { "SalesOrderId", "Id" },
                { "SalesOrderUID", "UID" },
                { "SalesOrderCreatedBy", "CreatedBy" },
                { "SalesOrderCreatedTime", "CreatedTime" },
                { "SalesOrderModifiedBy", "ModifiedBy" },
                { "SalesOrderModifiedTime", "ModifiedTime" },
                { "SalesOrderServerAddTime", "ServerAddTime" },
                { "SalesOrderServerModifiedTime", "ServerModifiedTime" },
            };


            Dictionary<string, string> salesOrderLineColumnMappings = new()
            {
                { "SalesOrderLineId", "Id" },
                { "SalesOrderLineUID", "UID" },
                { "SalesOrderLineCreatedBy", "CreatedBy" },
                { "SalesOrderLineCreatedTime", "CreatedTime" },
                { "SalesOrderLineModifiedBy", "ModifiedBy" },
                { "SalesOrderLineModifiedTime", "ModifiedTime" },
                { "SalesOrderLineServerAddTime", "ServerAddTime" },
                { "SalesOrderLineServerModifiedTime", "ServerModifiedTime" },
            };
            List<ISalesOrderViewModel>? salesOrderViewModelList = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                salesOrderViewModelList = new List<ISalesOrderViewModel>();
                foreach (DataRow row in dt.Rows)
                {
                    IFactory salesOrderFactory = new Model.Classes.SalesOrderFactory(_serviceProvider.GetRequiredService<Model.Interfaces.ISalesOrder>().GetType().Name);
                    IFactory salesOrderLineFactory = new Model.Classes.SalesOrderFactory(_serviceProvider.GetRequiredService<Model.Interfaces.ISalesOrderLine>().GetType().Name);
                    ISalesOrder salesOrder = ConvertDataTableToObject<SalesOrder.Model.Interfaces.ISalesOrder>(row, salesOrderFactory, salesOrderColumnMappings);
                    ISalesOrderLine salesOrderLine = ConvertDataTableToObject<SalesOrder.Model.Interfaces.ISalesOrderLine>(row, salesOrderLineFactory, salesOrderLineColumnMappings);
                    SalesOrderViewModel salesOrderDetailsModel = new()
                    {
                        SalesOrder = salesOrder,
                        SalesOrderLine = new List<ISalesOrderLine> { salesOrderLine }
                    };
                    salesOrderViewModelList.Add(salesOrderDetailsModel);
                }
            }
            return salesOrderViewModelList;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel)
    {
        int retValue = -1;
        if(salesOrderViewModel == null || salesOrderViewModel.SalesOrder == null)
        {
            return retValue;
        }
        using (SqliteConnection connection = SqliteConnection())
        {
            await connection.OpenAsync();

            using SqliteTransaction transaction = connection.BeginTransaction();
            try
            {
                _ = salesOrderViewModel.IsNewOrder
                    ? await CreateSalesOrder(salesOrderViewModel.SalesOrder, connection, transaction)
                    : await UpdateSalesOrder(salesOrderViewModel.SalesOrder, connection, transaction);
                if (salesOrderViewModel.SalesOrderLines != null)
                {
                    List<SalesOrderLine> salesOrderLinesCreate = salesOrderViewModel.SalesOrderLines
                        .Where(e => string.IsNullOrEmpty(e.SalesOrderLineUID))
                        .ToList();
                    List<SalesOrderLine> salesOrderLinesUpdate = salesOrderViewModel.SalesOrderLines
                        .Where(e => !string.IsNullOrEmpty(e.SalesOrderLineUID))
                        .ToList();
                    // Create
                    if(salesOrderLinesCreate != null && salesOrderLinesCreate.Count > 0)
                    {
                        await CreateSalesOrderLine(salesOrderLinesCreate.ToList<ISalesOrderLine>(), connection, transaction);
                    }
                    // Update
                    if (salesOrderLinesUpdate != null && salesOrderLinesUpdate.Count > 0)
                    {
                        await UpdateSalesOrderLine(salesOrderLinesUpdate.ToList<ISalesOrderLine>(), connection, transaction);
                    }

                    //foreach (SalesOrderLine salesOrderLine in salesOrderViewModel.SalesOrderLines)
                    //{
                    //    _ = string.IsNullOrEmpty(salesOrderLine.SalesOrderLineUID)
                    //        ? await CreateSalesOrderLine(salesOrderLine, connection, transaction)
                    //        : await UpdateSalesOrderLine(salesOrderLine, connection, transaction);
                    //}
                }
                if (salesOrderViewModel.SalesOrder.IsStockUpdateRequired)
                {
                    await UpdateStock(salesOrderViewModel, connection, transaction);
                }
                if (salesOrderViewModel.SalesOrder.IsInvoiceGenerationRequired)
                {
                    if (salesOrderViewModel.AccPayable != null)
                    {
                        await _collectionModuleDL.CUDAccPayable(new List<CollectionModule.Model.Interfaces.IAccPayable> { salesOrderViewModel.AccPayable }, connection, transaction);
                    }
                }
                retValue = 1;
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        return retValue;
    }
    public async Task<int> CreateSalesOrder(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = -1;
        try
        {
            if (salesOrder != null)
            {
                string salesOrderQuery = @"
                                INSERT INTO sales_order (uid, ss,created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                                sales_order_number, draft_order_number,company_uid, org_uid, distribution_channel_uid, delivered_by_org_uid, store_uid, Status, Order_Type, 
                                order_date, customer_po, currency_uid, total_amount, total_discount, total_tax,net_amount, line_count, qty_count, 
                                total_fake_amount, reference_number, source,total_line_discount,total_cash_discount,total_header_discount,total_excise_duty,
                                total_line_tax,total_header_tax,cash_sales_customer,cash_sales_address,reference_uid,reference_type,
                                job_position_uid, emp_uid, beat_history_uid, route_uid, store_history_uid, total_credit_limit, available_credit_limit, 
                                expected_delivery_date, delivered_date_time, latitude, longitude, is_offline, credit_days, notes, delivery_instructions, remarks, 
                                is_temperature_check_enabled, always_printed_flag, purchase_order_no_required_flag, is_with_printed_invoices_flag, vehicle_uid)
                                VALUES
                                (@UID, @SS,@CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SalesOrderNumber, 
                                @DraftOrderNumber,@CompanyUID, @OrgUID, @DistributionChannelUID, @DeliveredByOrgUID, @StoreUID, @Status, @OrderType, @OrderDate, 
                                @CustomerPO, @CurrencyUID, @TotalAmount, @TotalDiscount, @TotalTax,@NetAmount, @LineCount, @QtyCount, @TotalFakeAmount, 
                                @ReferenceNumber, @Source,@TotalLineDiscount,@TotalCashDiscount,@TotalHeaderDiscount,@TotalExciseDuty,
                                @TotalLineTax,@TotalHeaderTax,@CashSalesCustomer,@CashSalesAddress,@ReferenceUID,@ReferenceType,
                                @JobPositionUID, @EmpUID, @BeatHistoryUID, @RouteUID, @StoreHistoryUID, @TotalCreditLimit, @AvailableCreditLimit, 
                                @ExpectedDeliveryDate, @DeliveredDateTime, @Latitude, @Longitude, @IsOffline, @CreditDays, @Notes, @DeliveryInstructions, 
                                @Remarks, @IsTemperatureCheckEnabled, @AlwaysPrintedFlag, @PurchaseOrderNoRequiredFlag, @IsWithPrintedInvoicesFlag,@VehicleUID);";

                /*
                Dictionary<string, object?> salesOrderParameters = new()
                {
                    { "UID", salesOrder.UID },
                    { "SS", salesOrder.SS },
                    { "CreatedBy", salesOrder.CreatedBy },
                    { "CreatedTime", salesOrder.CreatedTime },
                    { "ModifiedBy", salesOrder.ModifiedBy },
                    { "ModifiedTime", salesOrder.ModifiedTime },
                    { "ServerAddTime", DateTime.Now },
                    { "ServerModifiedTime", DateTime.Now },
                    { "SalesOrderNumber", salesOrder.SalesOrderNumber },
                    { "DraftOrderNumber", salesOrder.DraftOrderNumber },
                    { "CompanyUID",salesOrder.CompanyUID },
                    { "OrgUID", salesOrder.OrgUID },
                    { "DistributionChannelUID", salesOrder.DistributionChannelUID },
                    { "DeliveredByOrgUID", salesOrder.DeliveredByOrgUID },
                    { "StoreUID", salesOrder.StoreUID },
                    { "Status", salesOrder.Status },
                    { "OrderType", salesOrder.OrderType },
                    { "OrderDate", salesOrder.OrderDate },
                    { "CustomerPO", salesOrder.CustomerPO },
                    { "CurrencyUID", salesOrder.CurrencyUID },
                    { "TotalAmount", salesOrder.TotalAmount },
                    { "TotalDiscount", salesOrder.TotalDiscount },
                    { "TotalTax", salesOrder.TotalTax },
                    { "NetAmount", salesOrder.NetAmount },
                    { "LineCount", salesOrder.LineCount },
                    { "QtyCount", salesOrder.QtyCount },
                    { "TotalFakeAmount", salesOrder.TotalFakeAmount },
                    { "ReferenceNumber", salesOrder.ReferenceNumber },
                    { "Source", salesOrder.Source },
                    { "TotalLineDiscount", salesOrder.TotalLineDiscount },
                    { "TotalCashDiscount", salesOrder.TotalCashDiscount },
                    { "TotalHeaderDiscount", salesOrder.TotalHeaderDiscount },
                    { "TotalExciseDuty", salesOrder.TotalExciseDuty },
                    { "TotalLineTax", salesOrder.TotalLineTax },
                    { "TotalHeaderTax", salesOrder.TotalHeaderTax },
                    { "CashSalesCustomer", salesOrder.CashSalesCustomer },
                    { "CashSalesAddress", salesOrder.CashSalesAddress },
                    { "ReferenceUID", salesOrder.ReferenceUID },
                    { "ReferenceType", salesOrder.ReferenceType },
                    { "JobPositionUID", salesOrder.JobPositionUID },
                    { "EmpUID", salesOrder.EmpUID },
                    { "BeatHistoryUID", salesOrder.BeatHistoryUID },
                    { "RouteUID", salesOrder.RouteUID },
                    { "StoreHistoryUID", salesOrder.StoreHistoryUID },
                    { "TotalCreditLimit", salesOrder.TotalCreditLimit },
                    { "AvailableCreditLimit", salesOrder.AvailableCreditLimit },
                    { "ExpectedDeliveryDate", salesOrder.ExpectedDeliveryDate },
                    { "DeliveredDateTime",CommonFunctions.GetDateTimeInFormatForSqlite(salesOrder.DeliveredDateTime) },
                    { "Latitude", salesOrder.Latitude },
                    { "Longitude", salesOrder.Longitude },
                    { "IsOffline", salesOrder.IsOffline },
                    { "CreditDays", salesOrder.CreditDays },
                    { "Notes", salesOrder.Notes },
                    { "DeliveryInstructions", salesOrder.DeliveryInstructions },
                    { "Remarks", salesOrder.Remarks },
                    { "IsTemperatureCheckEnabled", salesOrder.IsTemperatureCheckEnabled },
                    { "AlwaysPrintedFlag", salesOrder.AlwaysPrintedFlag },
                    { "PurchaseOrderNoRequiredFlag", salesOrder.PurchaseOrderNoRequiredFlag },
                    { "IsWithPrintedInvoicesFlag", salesOrder.IsWithPrintedInvoicesFlag },
                    { "VehicleUID", salesOrder.VehicleUID },
                };
                */
                retValue = await ExecuteNonQueryAsync(salesOrderQuery, salesOrder, connection, transaction);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }

    public async Task<int> CreateSalesOrderLine_Old(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine salesOrderLine,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = -1;
        try
        {
            if (salesOrderLine != null)
            {
                // Code for inserting data into the SalesOrderLine table
                string salesOrderLineQuery = @"
                                    INSERT INTO sales_order_line (uid, ss,created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                    sales_order_uid, line_number, item_code, item_type, base_price, unit_price, fake_unit_price, base_uom, uom, uom_conversion_to_bu, 
                                    reco_uom, reco_qty, reco_uom_conversion_to_bu, reco_qty_bu, model_qty_bu, qty, qty_bu, van_qty_bu, delivered_qty, missed_qty, returned_qty, 
                                    total_amount, total_discount, line_tax_amount, prorata_tax_amount, total_tax, net_amount, net_fake_amount, sku_price_uid, 
                                    prorata_discount_amount, line_discount_amount, mrp, cost_unit_price, parent_uid, is_promotion_applied, volume, volume_unit, weight, 
                                    weight_unit, stock_type, remarks, total_cash_discount,total_excise_duty,sku_uid,approved_qty,tax_data)
                                    VALUES
                                    ( @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SalesOrderUID, 
                                    @LineNumber, @ItemCode, @ItemType, @BasePrice, @UnitPrice, @FakeUnitPrice, @BaseUOM, @UoM, @UOMConversionToBU, @RecoUOM, 
                                    @RecoQty, @RecoUOMConversionToBU, @RecoQtyBU, @ModelQtyBU, @Qty, @QtyBU, @VanQtyBU, @DeliveredQty, @MissedQty, @ReturnedQty, 
                                    @TotalAmount, @TotalDiscount, @LineTaxAmount, @ProrataTaxAmount, @TotalTax, @NetAmount, @NetFakeAmount, @SKUPriceUID, 
                                    @ProrataDiscountAmount, @LineDiscountAmount, @MRP, @CostUnitPrice, @ParentUID, @IsPromotionApplied, @Volume, @VolumeUnit, 
                                    @Weight, @WeightUnit, @StockType, @Remarks,@TotalCashDiscount,@TotalExciseDuty,@SKUUID,@ApprovedQty,@TaxData); ";
                /*
                Dictionary<string, object?> salesOrderLineParameters = new()
                {
                    { "UID", salesOrderLine.UID },
                    { "SS", salesOrderLine.SS },
                    { "CreatedBy", salesOrderLine.CreatedBy },
                    { "CreatedTime", salesOrderLine.CreatedTime },
                    { "ModifiedBy", salesOrderLine.ModifiedBy },
                    { "ModifiedTime", salesOrderLine.ModifiedTime },
                    { "ServerAddTime", DateTime.Now },
                    { "ServerModifiedTime", DateTime.Now },
                    { "SalesOrderUID", salesOrderLine.SalesOrderUID },
                    { "LineNumber", salesOrderLine.LineNumber },
                    { "ItemCode", salesOrderLine.ItemCode },
                    { "ItemType", salesOrderLine.ItemType },
                    { "BasePrice", salesOrderLine.BasePrice },
                    { "UnitPrice", salesOrderLine.UnitPrice },
                    { "FakeUnitPrice", salesOrderLine.FakeUnitPrice },
                    { "BaseUOM", salesOrderLine.BaseUOM },
                    { "UoM", salesOrderLine.UoM },
                    { "UOMConversionToBU", salesOrderLine.UOMConversionToBU },
                    { "RecoUOM", salesOrderLine.RecoUOM },
                    { "RecoQty", salesOrderLine.RecoQty },
                    { "RecoUOMConversionToBU", salesOrderLine.RecoUOMConversionToBU },
                    { "RecoQtyBU", salesOrderLine.RecoQtyBU },
                    { "ModelQtyBU", salesOrderLine.ModelQtyBU },
                    { "Qty", salesOrderLine.Qty },
                    { "QtyBU", salesOrderLine.QtyBU },
                    { "VanQtyBU", salesOrderLine.VanQtyBU },
                    { "DeliveredQty", salesOrderLine.DeliveredQty },
                    { "MissedQty", salesOrderLine.MissedQty },
                    { "ReturnedQty", salesOrderLine.ReturnedQty },
                    { "TotalAmount", salesOrderLine.TotalAmount },
                    { "TotalDiscount", salesOrderLine.TotalDiscount },
                    { "LineTaxAmount", salesOrderLine.LineTaxAmount },
                    { "ProrataTaxAmount", salesOrderLine.ProrataTaxAmount },
                    { "TotalTax", salesOrderLine.TotalTax },
                    { "NetAmount", salesOrderLine.NetAmount },
                    { "NetFakeAmount", salesOrderLine.NetFakeAmount },
                    { "SKUPriceUID", salesOrderLine.SKUPriceUID },
                    { "ProrataDiscountAmount", salesOrderLine.ProrataDiscountAmount },
                    { "LineDiscountAmount", salesOrderLine.LineDiscountAmount },
                    { "MRP", salesOrderLine.MRP },
                    { "CostUnitPrice", salesOrderLine.CostUnitPrice },
                    { "ParentUID", salesOrderLine.ParentUID },
                    { "IsPromotionApplied", salesOrderLine.IsPromotionApplied },
                    { "Volume", salesOrderLine.Volume },
                    { "VolumeUnit", salesOrderLine.VolumeUnit },
                    { "Weight", salesOrderLine.Weight },
                    { "WeightUnit", salesOrderLine.WeightUnit },
                    { "StockType", salesOrderLine.StockType },
                    { "Remarks", salesOrderLine.Remarks },
                    { "TotalCashDiscount", salesOrderLine.TotalCashDiscount },
                    { "TotalExciseDuty", salesOrderLine.TotalExciseDuty },
                    { "SKUUID", salesOrderLine.SKUUID },
                    { "ApprovedQty", salesOrderLine.ApprovedQty },
                    { "TaxData", salesOrderLine.TaxData },
                };
                */
                retValue = await ExecuteNonQueryAsync(salesOrderLineQuery, salesOrderLine, connection, transaction);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> CreateSalesOrderLine(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine> salesOrderLines,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrderLines == null || salesOrderLines.Count == 0)
            {
                return retValue;
            }
            // Code for inserting data into the SalesOrderLine table
            string salesOrderLineQuery = @"
                                    INSERT INTO sales_order_line (uid, ss,created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                                    sales_order_uid, line_number, item_code, item_type, base_price, unit_price, fake_unit_price, base_uom, uom, uom_conversion_to_bu, 
                                    reco_uom, reco_qty, reco_uom_conversion_to_bu, reco_qty_bu, model_qty_bu, qty, qty_bu, van_qty_bu, delivered_qty, missed_qty, returned_qty, 
                                    total_amount, total_discount, line_tax_amount, prorata_tax_amount, total_tax, net_amount, net_fake_amount, sku_price_uid, 
                                    prorata_discount_amount, line_discount_amount, mrp, cost_unit_price, parent_uid, is_promotion_applied, volume, volume_unit, weight, 
                                    weight_unit, stock_type, remarks, total_cash_discount,total_excise_duty,sku_uid,approved_qty,tax_data)
                                    VALUES
                                    ( @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SalesOrderUID, 
                                    @LineNumber, @ItemCode, @ItemType, @BasePrice, @UnitPrice, @FakeUnitPrice, @BaseUOM, @UoM, @UOMConversionToBU, @RecoUOM, 
                                    @RecoQty, @RecoUOMConversionToBU, @RecoQtyBU, @ModelQtyBU, @Qty, @QtyBU, @VanQtyBU, @DeliveredQty, @MissedQty, @ReturnedQty, 
                                    @TotalAmount, @TotalDiscount, @LineTaxAmount, @ProrataTaxAmount, @TotalTax, @NetAmount, @NetFakeAmount, @SKUPriceUID, 
                                    @ProrataDiscountAmount, @LineDiscountAmount, @MRP, @CostUnitPrice, @ParentUID, @IsPromotionApplied, @Volume, @VolumeUnit, 
                                    @Weight, @WeightUnit, @StockType, @Remarks,@TotalCashDiscount,@TotalExciseDuty,@SKUUID,@ApprovedQty,@TaxData); ";
            retValue = await ExecuteNonQueryAsync(salesOrderLineQuery, salesOrderLines, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateSalesOrder(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = -1;
        try
        {
            if (salesOrder != null)
            {
                string salesOrderQuery = @"UPDATE sales_order SET ss= @SS,
                                            created_by= @CreatedBy, 
                                            created_time= @CreatedTime, 
                                            modified_by= @ModifiedBy, 
                                            modified_time= @ModifiedTime,
                                            server_add_time= @ServerAddTime,      
                                            server_modified_time= @ServerModifiedTime, 
                                            sales_order_number= @SalesOrderNumber, 
                                            draft_order_number= @DraftOrderNumber,                           
                                            company_uid= @CompanyUID, org_uid= @OrgUID, 
                                            distribution_channel_uid= @DistributionChannelUID,                                    
                                            delivered_by_org_uid= @DeliveredByOrgUID, 
                                            store_uid= @StoreUID, 
                                            status= @Status, 
                                            order_type= @OrderType, 
                                            order_date= @OrderDate,
                                            customer_po= @CustomerPO, 
                                            currency_uid= @CurrencyUID, 
                                            payment_type= @PaymentType, 
                                            total_amount= @TotalAmount, 
                                            total_discount= @TotalDiscount, 
                                            total_tax= @TotalTax, 
                                            net_amount= @NetAmount, 
                                            line_count= @LineCount, 
                                            qty_count= @QtyCount, 
                                            total_fake_amount= @TotalFakeAmount,
                                            reference_number= @ReferenceNumber, 
                                            source= @Source,
                                            total_line_discount = @TotalLineDiscount, 
                                            total_cash_discount = @TotalCashDiscount, 
                                            total_header_discount = @TotalHeaderDiscount,
                                            total_excise_duty = @TotalExciseDuty,
                                            total_line_tax = @TotalLineTax,
                                            total_header_tax = @TotalHeaderTax,
                                            cash_sales_customer=@CashSalesCustomer,
                                            cash_sales_address=@CashSalesAddress,
                                            reference_uid=@ReferenceUID,
                                            reference_type=@ReferenceType,
                                            job_position_uid= @JobPositionUID, 
                                            emp_uid= @EmpUID, 
                                            beat_history_uid= @BeatHistoryUID,
                                            route_uid= @RouteUID,
                                            store_history_uid= @StoreHistoryUID, 
                                            total_credit_limit= @TotalCreditLimit,
                                            available_credit_limit= @AvailableCreditLimit, 
                                            expected_delivery_date= @ExpectedDeliveryDate,
                                            delivered_date_time= @DeliveredDateTime,
                                            latitude= @Latitude, 
                                            longitude= @Longitude,
                                            is_offline= @IsOffline,
                                            credit_days= @CreditDays,
                                            notes= @Notes, 
                                            is_temperature_check_enabled= @IsTemperatureCheckEnabled, 
                                            always_printed_flag= @AlwaysPrintedFlag,
                                            purchase_order_no_required_flag= @PurchaseOrderNoRequiredFlag, 
                                            is_with_printed_invoices_flag= @IsWithPrintedInvoicesFlag,
                                            vehicle_uid = @VehicleUID
                                            WHERE uid = @UID;";
                /*
                Dictionary<string, object?> salesOrderParameters = new()
                {
                            { "UID", salesOrder.UID },
                            { "SS", salesOrder.SS },
                            { "CreatedBy", salesOrder.CreatedBy },
                            { "CreatedTime", salesOrder.CreatedTime },
                            { "ModifiedBy", salesOrder.ModifiedBy },
                            { "ModifiedTime", salesOrder.ModifiedTime },
                            { "ServerAddTime", DateTime.Now },
                            { "ServerModifiedTime", DateTime.Now },
                            { "SalesOrderNumber", salesOrder.SalesOrderNumber },
                            { "DraftOrderNumber", salesOrder.DraftOrderNumber },
                            { "CompanyUID",salesOrder.CompanyUID },
                            { "OrgUID", salesOrder.OrgUID },
                            { "DistributionChannelUID", salesOrder.DistributionChannelUID },
                            { "DeliveredByOrgUID", salesOrder.DeliveredByOrgUID },
                            { "StoreUID", salesOrder.StoreUID },
                            { "Status", salesOrder.Status },
                            { "OrderType", salesOrder.OrderType },
                            { "OrderDate", salesOrder.OrderDate },
                            { "CustomerPO", salesOrder.CustomerPO },
                            { "CurrencyUID", salesOrder.CurrencyUID },
                            { "PaymentType", salesOrder.PaymentType },
                            { "TotalAmount", salesOrder.TotalAmount },
                            { "TotalDiscount", salesOrder.TotalDiscount },
                            { "TotalTax", salesOrder.TotalTax },
                            { "NetAmount", salesOrder.NetAmount },
                            { "LineCount", salesOrder.LineCount },
                            { "QtyCount", salesOrder.QtyCount },
                            { "TotalFakeAmount", salesOrder.TotalFakeAmount },
                            { "ReferenceNumber", salesOrder.ReferenceNumber },
                            { "Source", salesOrder.Source },
                            { "TotalLineDiscount", salesOrder.TotalLineDiscount },
                            { "TotalCashDiscount", salesOrder.TotalCashDiscount },
                            { "TotalHeaderDiscount", salesOrder.TotalHeaderDiscount },
                            { "TotalExciseDuty", salesOrder.TotalExciseDuty },
                            { "TotalLineTax", salesOrder.TotalLineTax },
                            { "TotalHeaderTax", salesOrder.TotalHeaderTax },
                            { "CashSalesCustomer", salesOrder.CashSalesCustomer },
                            { "CashSalesAddress", salesOrder.CashSalesAddress },
                            { "ReferenceUID", salesOrder.ReferenceUID },
                            { "ReferenceType", salesOrder.ReferenceType },
                            { "JobPositionUID", salesOrder.JobPositionUID },
                            { "EmpUID", salesOrder.EmpUID },
                            { "BeatHistoryUID", salesOrder.BeatHistoryUID },
                            { "RouteUID", salesOrder.RouteUID },
                            { "StoreHistoryUID", salesOrder.StoreHistoryUID },
                            { "TotalCreditLimit", salesOrder.TotalCreditLimit },
                            { "AvailableCreditLimit", salesOrder.AvailableCreditLimit },
                            { "ExpectedDeliveryDate", salesOrder.ExpectedDeliveryDate },
                            { "DeliveredDateTime", salesOrder.DeliveredDateTime },
                            { "Latitude", salesOrder.Latitude },
                            { "Longitude", salesOrder.Longitude },
                            { "IsOffline", salesOrder.IsOffline },
                            { "CreditDays", salesOrder.CreditDays },
                            { "Notes", salesOrder.Notes },
                            { "DeliveryInstructions", salesOrder.DeliveryInstructions },
                            { "Remarks", salesOrder.Remarks },
                            { "IsTemperatureCheckEnabled", salesOrder.IsTemperatureCheckEnabled },
                            { "AlwaysPrintedFlag", salesOrder.AlwaysPrintedFlag },
                            { "PurchaseOrderNoRequiredFlag", salesOrder.PurchaseOrderNoRequiredFlag },
                            { "IsWithPrintedInvoicesFlag", salesOrder.IsWithPrintedInvoicesFlag },
                            { "VehicleUID", salesOrder.VehicleUID }
                        };
                */
                retValue = await ExecuteNonQueryAsync(salesOrderQuery, salesOrder, connection, transaction);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }

    public async Task<int> UpdateSalesOrderLine_Old(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine salesOrderLine,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = -1;
        try
        {
            if (salesOrderLine != null)
            {
                string query = @"
                UPDATE Sales_Order_Line SET id= @Id, ss = @SS, created_by= @CreatedBy, created_time= @CreatedTime, modified_by= @ModifiedBy, 
                modified_time= @ModifiedTime, server_add_time= @ServerAddTime, server_modified_time= @ServerModifiedTime, 
                sales_order_uid= @SalesOrderUID, line_number= @LineNumber, item_code= @ItemCode, item_type= @ItemType, 
                base_price= @BasePrice, unit_price= @UnitPrice, fake_unit_price= @FakeUnitPrice, base_uom= @BaseUOM, 
                uom= @UoM, uom_conversion_to_bu= @UOMConversionToBU, reco_uom= @RecoUOM, reco_qty= @RecoQty, 
                Reco_UOM_Conversion_To_BU= @RecoUOMConversionToBU, Reco_Qty_BU= @RecoQtyBU, Model_Qty_BU= @ModelQtyBU, 
                qty= @Qty, qty_bu= @QtyBU, van_qty_bu= @VanQtyBU, delivered_qty= @DeliveredQty, missed_qty= @MissedQty, 
                returned_qty= @ReturnedQty, total_amount= @TotalAmount, total_discount= @TotalDiscount, 
                line_tax_amount= @LineTaxAmount, prorata_tax_amount= @ProrataTaxAmount, total_tax= @TotalTax, 
                net_amount= @NetAmount, net_fake_amount= @NetFakeAmount, sku_price_uid= @SKUPriceUID, 
                prorata_discount_amount= @ProrataDiscountAmount, line_discount_amount= @LineDiscountAmount, 
                mrp= @MRP, cost_unit_price= @CostUnitPrice, parent_uid= @ParentUID, is_promotion_applied= @IsPromotionApplied, 
                volume= @Volume, volume_unit= @VolumeUnit, weight= @Weight, weight_unit= @WeightUnit, 
                stock_type= @StockType, remarks= @Remarks, total_cash_discount = @TotalCashDiscount,
                total_excise_duty = @TotalExciseDuty,sku_uid = @SKUUID,approved_qty=@ApprovedQty,tax_data=@TaxData
                WHERE UID = @UID;";
                /*
                Dictionary<string, object?> salesOrderLineParameters = new()
                {
                                { "Id", salesOrderLine.Id },
                                { "UID", salesOrderLine.UID },
                                { "SS", salesOrderLine.SS },
                                { "CreatedBy", salesOrderLine.CreatedBy },
                                { "CreatedTime", salesOrderLine.CreatedTime },
                                { "ModifiedBy", salesOrderLine.ModifiedBy },
                                { "ModifiedTime", salesOrderLine.ModifiedTime },
                                { "ServerAddTime", DateTime.Now },
                                { "ServerModifiedTime", DateTime.Now },
                                { "SalesOrderUID", salesOrderLine.SalesOrderUID },
                                { "LineNumber", salesOrderLine.LineNumber },
                                { "ItemCode", salesOrderLine.ItemCode },
                                { "ItemType", salesOrderLine.ItemType },
                                { "BasePrice", salesOrderLine.BasePrice },
                                { "UnitPrice", salesOrderLine.UnitPrice },
                                { "FakeUnitPrice", salesOrderLine.FakeUnitPrice },
                                { "BaseUOM", salesOrderLine.BaseUOM },
                                { "UoM", salesOrderLine.UoM },
                                { "UOMConversionToBU", salesOrderLine.UOMConversionToBU },
                                { "RecoUOM", salesOrderLine.RecoUOM },
                                { "RecoQty", salesOrderLine.RecoQty },
                                { "RecoUOMConversionToBU", salesOrderLine.RecoUOMConversionToBU },
                                { "RecoQtyBU", salesOrderLine.RecoQtyBU },
                                { "ModelQtyBU", salesOrderLine.ModelQtyBU },
                                { "Qty", salesOrderLine.Qty },
                                { "QtyBU", salesOrderLine.QtyBU },
                                { "VanQtyBU", salesOrderLine.VanQtyBU },
                                { "DeliveredQty", salesOrderLine.DeliveredQty },
                                { "MissedQty", salesOrderLine.MissedQty },
                                { "ReturnedQty", salesOrderLine.ReturnedQty },
                                { "TotalAmount", salesOrderLine.TotalAmount },
                                { "TotalDiscount", salesOrderLine.TotalDiscount },
                                { "LineTaxAmount", salesOrderLine.LineTaxAmount },
                                { "ProrataTaxAmount", salesOrderLine.ProrataTaxAmount },
                                { "TotalTax", salesOrderLine.TotalTax },
                                { "NetAmount", salesOrderLine.NetAmount },
                                { "NetFakeAmount", salesOrderLine.NetFakeAmount },
                                { "SKUPriceUID", salesOrderLine.SKUPriceUID },
                                { "ProrataDiscountAmount", salesOrderLine.ProrataDiscountAmount },
                                { "LineDiscountAmount", salesOrderLine.LineDiscountAmount },
                                { "MRP", salesOrderLine.MRP },
                                { "CostUnitPrice", salesOrderLine.CostUnitPrice },
                                { "ParentUID", salesOrderLine.ParentUID },
                                { "IsPromotionApplied", salesOrderLine.IsPromotionApplied },
                                { "Volume", salesOrderLine.Volume },
                                { "VolumeUnit", salesOrderLine.VolumeUnit },
                                { "Weight", salesOrderLine.Weight },
                                { "WeightUnit", salesOrderLine.WeightUnit },
                                { "StockType", salesOrderLine.StockType },
                                { "Remarks", salesOrderLine.Remarks },
                                { "TotalCashDiscount", salesOrderLine.TotalCashDiscount },
                                { "TotalExciseDuty", salesOrderLine.TotalExciseDuty },
                                { "SKUUID", salesOrderLine.SKUUID },
                                { "ApprovedQty", salesOrderLine.ApprovedQty },
                                { "TaxData", salesOrderLine.TaxData },
                            };
                */
                retValue = await ExecuteNonQueryAsync(query, salesOrderLine, connection, transaction);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }

    public async Task<int> UpdateSalesOrderLine(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine> salesOrderLines,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrderLines == null || salesOrderLines.Count == 0)
            {
                return retValue;
            }
            string query = @"
                UPDATE Sales_Order_Line SET id= @Id, ss = @SS, created_by= @CreatedBy, created_time= @CreatedTime, modified_by= @ModifiedBy, 
                modified_time= @ModifiedTime, server_add_time= @ServerAddTime, server_modified_time= @ServerModifiedTime, 
                sales_order_uid= @SalesOrderUID, line_number= @LineNumber, item_code= @ItemCode, item_type= @ItemType, 
                base_price= @BasePrice, unit_price= @UnitPrice, fake_unit_price= @FakeUnitPrice, base_uom= @BaseUOM, 
                uom= @UoM, uom_conversion_to_bu= @UOMConversionToBU, reco_uom= @RecoUOM, reco_qty= @RecoQty, 
                Reco_UOM_Conversion_To_BU= @RecoUOMConversionToBU, Reco_Qty_BU= @RecoQtyBU, Model_Qty_BU= @ModelQtyBU, 
                qty= @Qty, qty_bu= @QtyBU, van_qty_bu= @VanQtyBU, delivered_qty= @DeliveredQty, missed_qty= @MissedQty, 
                returned_qty= @ReturnedQty, total_amount= @TotalAmount, total_discount= @TotalDiscount, 
                line_tax_amount= @LineTaxAmount, prorata_tax_amount= @ProrataTaxAmount, total_tax= @TotalTax, 
                net_amount= @NetAmount, net_fake_amount= @NetFakeAmount, sku_price_uid= @SKUPriceUID, 
                prorata_discount_amount= @ProrataDiscountAmount, line_discount_amount= @LineDiscountAmount, 
                mrp= @MRP, cost_unit_price= @CostUnitPrice, parent_uid= @ParentUID, is_promotion_applied= @IsPromotionApplied, 
                volume= @Volume, volume_unit= @VolumeUnit, weight= @Weight, weight_unit= @WeightUnit, 
                stock_type= @StockType, remarks= @Remarks, total_cash_discount = @TotalCashDiscount,
                total_excise_duty = @TotalExciseDuty,sku_uid = @SKUUID,approved_qty=@ApprovedQty,tax_data=@TaxData
                WHERE UID = @UID;";
            retValue = await ExecuteNonQueryAsync(query, salesOrderLines, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<ISalesOrder?> GetSalesOrderByUID(string salesOrderUID)
    {
        try
        {
            StringBuilder sql = new(@"
                SELECT SO.uid, SO.sales_order_number as SalesOrderNumber, 
                SO.draft_order_number DraftOrderNumber, SO.store_uid StoreUID, SO.[status] Status, 
                SO.order_type OrderType,SO.order_date OrderDate, SO.customer_po CustomerPO, 
                SO.currency_uid CurrencyUID, SO.payment_type PaymentType, SO.[source] Source, 
                so.expected_delivery_date ExpectedDeliveryDate, so.delivered_date_time DeliveryDateTime,
                SO.total_cash_discount TotalCashDiscount
                FROM sales_order SO 
                WHERE SO.[uid] = @SalesOrderUID");
            Dictionary<string, object?> parameters = new()
            {
                { "SalesOrderUID", salesOrderUID }
            };
            return await ExecuteSingleAsync<ISalesOrder>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        try
        {
            StringBuilder sql = new(@"
        SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy,
        modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime, 
        sales_order_uid AS SalesOrderUID, line_number AS LineNumber, item_code AS ItemCode, item_type AS ItemType, 
        base_price AS BasePrice, unit_price AS UnitPrice, fake_unit_price AS FakeUnitPrice, base_uom AS BaseUOM, uom AS UOM,
        uom_conversion_to_bu AS UOMConversionToBU, reco_uom AS RecoUOM, reco_qty AS RecoQty, 
        reco_uom_conversion_to_bu AS RecoUOMConversionToBU, reco_qty_bu AS RecoQtyBU, model_qty_bu AS ModelQtyBU,
        qty AS Qty, qty_bu AS QtyBU, van_qty_bu AS VanQtyBU, delivered_qty AS DeliveredQty, missed_qty AS MissedQty,
        returned_qty AS ReturnedQty, total_amount AS TotalAmount, total_discount AS TotalDiscount, 
        line_tax_amount AS LineTaxAmount, prorata_tax_amount AS ProrataTaxAmount, total_tax AS TotalTax, 
        net_amount AS NetAmount, net_fake_amount AS NetFakeAmount, sku_price_uid AS SKUPriceUID, 
        prorata_discount_amount AS ProrataDiscountAmount, line_discount_amount AS LineDiscountAmount, mrp AS MRP ,
        cost_unit_price AS CostUnitPrice, parent_uid AS ParentUID, is_promotion_applied AS IsPromotionApplied, 
        volume AS Volume, volume_unit AS VolumeUnit, weight AS Weight, weight_unit AS WeightUnit, stock_type AS StockType,
        remarks AS Remarks, total_cash_discount AS TotalCashDiscount, total_excise_duty AS TotalExciseDuty, sku_uid AS SKUUID,
        approved_qty AS ApprovedQty,tax_data as TaxData FROM sales_order_line 
        WHERE sales_order_uid = @SalesOrderUID");
            Dictionary<string, object?> parameters = new()
            {
                { "SalesOrderUID", salesOrderUID }
            };
            return await ExecuteQueryAsync<ISalesOrderLine>(sql.ToString(), parameters);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>> GetISalesSummaryItemViews(DateTime startDate, DateTime endDate, string storeUID = "")
    {
        /*
        StringBuilder sql = new(@"
                SELECT SO.[uid] AS SalesOrderUID, S.code AS StoreCode, S.name AS StoreName, 
                COALESCE(SO.sales_order_number, SO.draft_order_number) AS OrderNumber, 
                A.line1 ||  ' ' || A.line2 AS Address, SO.order_type, SO.[status] AS OrderStatus, 
                SO.order_date as OrderDate, SO.expected_delivery_date AS DeliveryDate,
                SO.net_amount AS OrderAmount, IFNULL(C.name, SO.currency_uid) AS CurrencyLabel, 
                CASE WHEN SO.ss = 0 THEN 1 ELSE 0 END AS IsPosted 
                FROM sales_order SO
                INNER JOIN store S ON S.[uid] = SO.store_uid
                LEFT JOIN currency C ON C.[UID] = SO.currency_uid
                LEFT JOIN address A ON A.linked_item_type = 'Store' 
                AND A.linked_item_uid = S.[uid] AND A.is_default = 1 
                WHERE DATE(so.expected_delivery_date)  BETWEEN DATE(@startDate) AND DATE(@endDate)");
        */
        StringBuilder sql = new(@"
                SELECT SO.[uid] AS SalesOrderUID, S.code AS StoreCode, S.name AS StoreName, 
                COALESCE(SO.sales_order_number, SO.draft_order_number) AS OrderNumber, 
                '' AS Address, SO.order_type, SO.[status] AS OrderStatus, 
                SO.order_date as OrderDate, SO.expected_delivery_date AS DeliveryDate,
                SO.net_amount AS OrderAmount, IFNULL(C.name, SO.currency_uid) AS CurrencyLabel, 
                CASE WHEN SO.ss = 0 THEN 1 ELSE 0 END AS IsPosted 
                FROM sales_order SO
                INNER JOIN store S ON S.[uid] = SO.store_uid
                LEFT JOIN currency C ON C.[UID] = SO.currency_uid
                WHERE DATE(so.expected_delivery_date)  BETWEEN DATE(@startDate) AND DATE(@endDate)");
        if (!storeUID.IsNullOrEmpty())
        {
            _ = sql.Append("AND store_uid = @storeUID");
        }
        Dictionary<string, object?> parameters = new()
        {
            { "startDate", startDate },
            { "endDate", endDate },
            { "storeUID", storeUID },
        };
        Type type = _serviceProvider.GetRequiredService<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>().GetType();
        List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView> salesOrders = await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>(sql.ToString(), parameters, type);
        return salesOrders;
    }

    public async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderPrintView(string SalesOrderUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
            {"SalesOrderUID" , SalesOrderUID}
        };
            string sql = @"SELECT IFNULL(SO.sales_order_number, SO.draft_order_number) AS SalesOrderNumber,  SO.[status] AS Status, 
                            SO.order_type AS OrderType, S.code AS StoreCode, S.number AS StoreNumber, S.name AS StoreName, SO.customer_po AS CustomerPO, 
                            SO.order_date AS OrderDate, so.expected_delivery_date AS ExpectedDeliveryDate, so.delivered_date_time AS DeliveredDateTime,
                            C.symbol AS CurrencySymbol, SO.total_amount AS TotalAmount, SO.total_discount AS TotalDiscount, SO.total_tax AS TotalTax, 
                            SO.net_amount AS NetAmount, SO.line_count AS LineCount, SO.qty_count AS QtyCount, SO.total_line_discount AS TotalLineDiscount, 
                            SO.total_cash_discount AS TotalCashDiscount, SO.total_header_discount AS TotalHeaderDiscount, SO.total_excise_duty AS TotalExciseDuty, 
                            SO.total_line_tax AS TotalLineTax, SO.total_header_tax AS TotalHeaderTax, A.line1 AS AddressLine1, A.line2 AS AddressLine2, 
                            A.line3 AS AddressLine3
                            FROM sales_order SO
                            INNER JOIN store S ON S.[uid] = SO.store_uid
                            INNER JOIN currency C ON C.[uid] = SO.currency_uid 
                            LEFT JOIN address A ON A.linked_item_uid = S.[uid] AND A.is_default = 1
                            WHERE SO.[uid] =@SalesOrderUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISalesOrderPrintView>().GetType();

            Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView salesOrderPrintViewDetails = await ExecuteSingleAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView>(sql, parameters, type);
            return salesOrderPrintViewDetails;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderLinePrintView(string SalesOrderUID)
    {
        try
        {
            Dictionary<string, object?> parameters = new()
            {
            {"SalesOrderUID" , SalesOrderUID}
        };
            string sql = @"SELECT SOL.line_number AS LineNumber, SOL.item_code AS SKUCode, S.[name] AS SKUDescription, SOL.item_type AS ItemType, 
                            SOL.unit_price AS UnitPrice, SOL.uom AS UOM, SOL.uom_conversion_to_bu AS UOMConversionToBU, SOL.reco_uom AS RecoUOM, 
                            SOL.reco_qty AS RecoQty, SOL.qty AS Qty, SOL.delivered_qty AS DeliveredQty, SOL.total_amount AS TotalAmount, 
                            SOL.total_discount AS TotalDiscount, SOL.total_tax AS TotalTax, SOL.net_amount AS NetAmount 
                            FROM sales_order_line SOL 
                            INNER JOIN sku S ON S.[uid] = SOL.sku_uid
                            WHERE sales_order_uid =@SalesOrderUID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISalesOrderLinePrintView>().GetType();

            IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> salesOrderPrintViewDetails = await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>(sql, parameters, type);
            return salesOrderPrintViewDetails;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<PagedResponse<IDeliveredPreSales>> SelectDeliveredPreSales(List<SortCriteria> sortCriterias,
        int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired,
        DateTime startDate, DateTime endDate, string Status)
    {
        throw new NotImplementedException();
    }

    public Task<IViewPreSales> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID)
    {
        throw new NotImplementedException();
    }

    public Task<int> CUD_SalesOrder(SalesOrderViewModelDCO salesOrderViewModel)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertorUpdate_SalesOrders(SalesOrderViewModelDCO salesOrderView)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdateSalesOrderStatus(Model.Classes.SalesOrderStatusModel salesOrderStatus)
    {
        int retValue = -1;
        try
        {
            if (salesOrderStatus != null)
            {
                string salesOrderQuery = @"UPDATE sales_order SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime, 
                                            server_modified_time = @ServerModifiedTime, 
                                            status = @Status
                                            WHERE uid = @UID";
                Dictionary<string, object?> salesOrderParameters = new()
                {
                { "@UID", salesOrderStatus.UID },
                { "@ModifiedBy", salesOrderStatus.ModifiedBy },
                { "@ModifiedTime", salesOrderStatus.ModifiedTime },
                { "@ServerModifiedTime", salesOrderStatus.ServerModifiedTime },
                { "@Status", salesOrderStatus.Status },
                };

                retValue = await ExecuteNonQueryAsync(salesOrderQuery, salesOrderParameters);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    #region Stock Update
    public async Task<int> UpdateStock(SalesOrderViewModelDCO salesOrderViewModel,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {            
            List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? iWHStockLedgers = ConvertToWHStockLedger(salesOrderViewModel);
            if (iWHStockLedgers == null || iWHStockLedgers.Count == 0)
            {
                return -1;
            }
            return await _stockUpdaterDL.UpdateStockAsync(iWHStockLedgers, connection, transaction);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public virtual List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? ConvertToWHStockLedger(SalesOrderViewModelDCO salesOrderViewModel)
    {
        if (salesOrderViewModel == null || salesOrderViewModel.SalesOrder == null)
        {
            return null;
        }
        if (salesOrderViewModel.SalesOrderLines == null || salesOrderViewModel.SalesOrderLines.Count == 0)
        {
            return null;
        }
        List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgerList = null;
        wHStockLedgerList = new List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>();
        foreach (SalesOrderLine salesOrderLine in salesOrderViewModel.SalesOrderLines)
        {
            Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger? wHStockLedger = ConvertToWHStockLedger(salesOrderViewModel.SalesOrder, salesOrderLine);
            if (wHStockLedger == null)
            {
                continue;
            }
            wHStockLedgerList.Add(wHStockLedger);
        }
        return wHStockLedgerList;
    }
    public virtual Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger? ConvertToWHStockLedger(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine salesOrderLine)
    {
        string warehouseUID = string.Empty;
        Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger wHStockLedger = _serviceProvider.GetRequiredService<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>();
        if (wHStockLedger == null)
        {
            return wHStockLedger;
        }
        wHStockLedger.Id = 0;
        wHStockLedger.UID = Guid.NewGuid().ToString();
        wHStockLedger.SS = 0;
        wHStockLedger.CreatedBy = salesOrder.CreatedBy;
        wHStockLedger.CreatedTime = DateTime.Now;
        wHStockLedger.ModifiedBy = salesOrder.ModifiedBy;
        wHStockLedger.ModifiedTime = DateTime.Now;
        wHStockLedger.ServerAddTime = DateTime.Now;
        wHStockLedger.ServerModifiedTime = DateTime.Now;
        wHStockLedger.CompanyUID = salesOrder.CompanyUID;
        wHStockLedger.WarehouseUID = salesOrder.VehicleUID;
        wHStockLedger.OrgUID = salesOrder.DeliveredByOrgUID;
        wHStockLedger.SKUUID = salesOrderLine.SKUUID;
        wHStockLedger.SKUCode = salesOrderLine.ItemCode;
        wHStockLedger.Type = -1;
        wHStockLedger.ReferenceType = LinkedItemType.SalesOrderLine;
        wHStockLedger.ReferenceUID = salesOrderLine.UID; // Later it will be taken from wh_stock_request_stock table
        wHStockLedger.BatchNumber = salesOrder.DefaultBatchNumber; // Hardcoded for now
        wHStockLedger.ExpiryDate = null;
        wHStockLedger.Qty = salesOrderLine.QtyBU;
        wHStockLedger.UOM = salesOrderLine.BaseUOM;
        wHStockLedger.StockType = Enum.GetName(StockType.Salable);
        wHStockLedger.SerialNo = null;
        wHStockLedger.VersionNo = salesOrder.DefaultStockVersion;
        wHStockLedger.ParentWhUID = null;
        wHStockLedger.YearMonth = CommonFunctions.GetIntValue(CommonFunctions.GetDateTimeInFormat(DateTime.Now, "yyMM"));
        return wHStockLedger;
    }
    public async Task<List<ISalesOrderInvoice>> GetAllSalesOrderInvoices(string? storeUID = null)
    {
        try
        {
            string sql = """
                        SELECT SO.uid AS SalesOrderUID, SO.sales_order_number AS SalesOrderNumber,
                        SO.expected_delivery_date AS DeliveryDate,
                        SUM(SOL.qty_bu) AS TotalQuantity, SUM(SOL.qty_bu - SOL.returned_qty) AS AvailableQty
                        FROM sales_order SO
                        INNER JOIN sales_order_line SOL ON SOL.sales_order_uid = SO.uid AND SO.store_uid = @StoreUID
                        where SO.order_type  = 'Vansales' AND SO.status = 'Delivered'
                        GROUP BY SO.uid, SO.sales_order_number, SO.expected_delivery_date
                        HAVING SUM(SOL.qty_bu - SOL.returned_qty) > 0
                        ORDER BY SO.expected_delivery_date
                        """;

            return await ExecuteQueryAsync<ISalesOrderInvoice>(sql,new { StoreUID = storeUID });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<List<ISalesOrderLineInvoice>> GetSalesOrderLineInvoiceItems(string salesOrderUID)
    {
        try
        {
            string sql = """
                         SELECT SOL.sales_order_uid AS SalesOrderUID, SOL.uid AS SalesOrderLineUID, S.uid AS SKUUID, S.code AS SKUCode, 
                         S.Name AS SKUName,
                         SOL.qty_bu - SOL.returned_qty AS AvailableQty
                         FROM sales_order_line SOL
                         INNER JOIN sku S ON S.uid = SOL.sku_uid
                         where SOL.sales_order_uid = @SalesOrderUID
                         ORDER BY SOL.line_number
                         """;
            return await ExecuteQueryAsync<ISalesOrderLineInvoice>(sql, new { SalesOrderUID = salesOrderUID });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<int> UpdateSalesOrderLinesReturnQty(List<ISalesOrderLine> salesOrderLines)
    {
        try
        {
            string sql = """
                         UPDATE Sales_Order_Line SET Returned_Qty = Returned_Qty + @ReturnedQty, SS = 2, Modified_Time = @ModifiedTime
                         WHERE uid = @UID
                         """;
            return await ExecuteNonQueryAsync(sql, salesOrderLines);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    #endregion
}
