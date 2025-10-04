using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.SalesOrder.DL.Interfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.JourneyPlan.DL.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.DL.Classes;

public class MSSQLSalesOrderDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISalesOrderDL
{
    private readonly ILogger<MSSQLSalesOrderDL> _logger;
    protected IStoreHistoryDL _storeHistoryDL;
    protected ICollectionModuleDL _collectionModuleDL;
    public Winit.Modules.StockUpdater.DL.Interfaces.IStockUpdaterDL _stockUpdaterDL;
    public MSSQLSalesOrderDL(IServiceProvider serviceProvider, IConfiguration config, ILogger<MSSQLSalesOrderDL> logger, IStoreHistoryDL storeHistoryDL,
        ICollectionModuleDL collectionModuleDL, StockUpdater.DL.Interfaces.IStockUpdaterDL stockUpdaterDL) :
        base(serviceProvider, config)
    {
        _logger = logger;
        _storeHistoryDL = storeHistoryDL;
        _collectionModuleDL = collectionModuleDL;
        _stockUpdaterDL = stockUpdaterDL;
    }
    public async Task<List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>> GetISalesSummaryItemViews(DateTime startDate, DateTime endDate, string storeUID = "")
    {
        StringBuilder sql = new(@"SELECT
                                  s.code AS StoreCode,
                                  s.name AS StoreName,
                                  ISNULL(so.sales_order_number, so.draft_order_number) AS OrderNumber,
                                  a.line1 + ' ' + a.line2 AS Address,
                                  so.order_type AS OrderType,
                                  so.status AS OrderStatus,
                                  so.order_date AS OrderDate,
                                  so.expected_delivery_date AS DeliveryDate,
                                  so.net_amount AS OrderAmount,
                                  c.name AS CurrencyLabel,
                                  CASE WHEN so.ss = 0 THEN 1 ELSE 0 END AS IsPosted
                              FROM
                                  sales_order so
                              INNER JOIN
                                  store s ON s.uid = so.store_uid
                              INNER JOIN
                                  currency c ON c.uid = so.currency_uid
                              LEFT JOIN
                                  address a ON a.linked_item_type = 'Store' AND a.linked_item_uid = s.uid AND a.is_default = 1
                              WHERE
                                  so.expected_delivery_date BETWEEN @startDate AND @endDate");
        if (!storeUID.IsNullOrEmpty())
        {
            _ = sql.Append(@"AND store_uid = @storeUID");
        }
        Dictionary<string, object?> parameters = new()
        {
            { "startDate", startDate },
            { "endDate", endDate },
            { "storeUID", storeUID },
        };
        List<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView> salesOrders = await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesSummaryItemView>(sql.ToString(), parameters);
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
                  string sql = @"SELECT 
                                COALESCE(so.sales_order_number, so.draft_order_number) AS SalesOrderNumber,
                                so.status AS Status,
                                so.order_type AS OrderType,
                                s.code AS StoreCode,
                                s.number AS StoreNumber,
                                s.name AS StoreName,
                                so.customer_po AS CustomerPO,
                                so.order_date AS OrderDate,
                                so.expected_delivery_date AS ExpectedDeliveryDate,
                                so.delivered_date_time AS DeliveredDateTime,
                                c.symbol AS CurrencySymbol,
                                so.total_amount AS TotalAmount,
                                so.total_discount AS TotalDiscount,
                                so.total_tax AS TotalTax,
                                so.net_amount AS NetAmount,
                                so.line_count AS LineCount,
                                so.qty_count AS QtyCount,
                                so.total_line_discount AS TotalLineDiscount,
                                so.total_cash_discount AS TotalCashDiscount,
                                so.total_header_discount AS TotalHeaderDiscount,
                                so.total_excise_duty AS TotalExciseDuty,
                                so.total_line_tax AS TotalLineTax,
                                so.total_header_tax AS TotalHeaderTax,
                                a.line1 AS AddressLine1,
                                a.line2 AS AddressLine2,
                                a.line3 AS AddressLine3
                            FROM 
                                sales_order so
                            INNER JOIN 
                                store s ON s.uid = so.store_uid
                            INNER JOIN 
                                currency c ON c.uid = so.currency_uid
                            INNER JOIN 
                                address a ON a.linked_item_uid = s.uid AND a.is_default = 1
                            WHERE 
                                so.uid = @SalesOrderUID;";
            return await ExecuteSingleAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView>(sql, parameters);
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
            string sql = @"SELECT sol.line_number AS LineNumber, sol.item_code AS SKUCode, s.name AS SKUDescription, 
                            sol.item_type AS ItemType,sol.unit_price AS UnitPrice, sol.uom AS UOM, 
                            sol.uom_conversion_to_bu AS UOMConversionToBU,sol.reco_uom AS RecoUOM, 
                            sol.reco_qty AS RecoQty, sol.qty AS Qty, sol.delivered_qty AS DeliveredQty, 
                            sol.total_amount AS TotalAmount, sol.total_discount AS TotalDiscount, sol.total_tax AS TotalTax,
                            sol.net_amount As NetAmount FROM sales_order_line sol
                            INNER JOIN sku s ON s.uid = sol.sku_uid WHERE sol.sales_order_uid = @SalesOrderUID;";
            IEnumerable<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView> salesOrderPrintViewDetails = await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>(sql, parameters);
            return salesOrderPrintViewDetails;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<PagedResponse<IDeliveredPreSales>> SelectDeliveredPreSales(List<SortCriteria> sortCriterias,
        int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, DateTime startDate,
        DateTime endDate, string Status)
    {
        try
        {
            StringBuilder sql = new(@"SELECT * FROM (
                                                SELECT
                                                    '[' + o.code + ']' + o.name AS FranchiseCode,
                                                    o.uid AS OrgUID,
                                                    s.code AS StoreCode,
                                                    s.name AS StoreName,
                                                    so.uid AS SalesOrderUID, 
                                                    so.sales_order_number AS SalesOrderNumber, 
                                                    so.draft_order_number AS DraftOrderNumber,
                                                    s.number AS StoreNumber,
                                                    so.line_count AS SKUCount,
                                                    so.net_amount AS NetAmount,
                                                    so.order_type AS OrderType,
                                                    so.status AS Status,
                                                    '[' + r.code + '] ' + r.name AS RouteName,
                                                    --CONVERT(VARCHAR, so.order_date, 106) AS OrderDate,
                                                    --CONVERT(VARCHAR, so.expected_delivery_date, 106) AS DeliveryDate,
                                                    so.order_date AS OrderDate,
                                                    so.expected_delivery_date AS DeliveryDate,
                                                    e.name + ' [' + e.login_id + ']' AS EmpName,
                                                    e.uid AS EmpUID,
                                                    so.modified_time AS SalesOrderModifiedTime
                                                FROM
                                                    sales_order so
                                                    INNER JOIN store s ON s.uid = so.store_uid
                                                    INNER JOIN org o ON o.uid = s.franchisee_org_uid
                                                    INNER JOIN route r ON r.uid = so.route_uid
                                                    INNER JOIN emp e ON e.uid = so.emp_uid
                                                WHERE
                                                    so.expected_delivery_date BETWEEN @startDate AND @endDate
                                                    AND so.status = @Status) AS SubQuery");

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM
                                            (SELECT
                                                    '[' + o.code + ']' + o.name AS FranchiseCode,
                                                    o.uid AS OrgUID,
                                                    s.code AS StoreCode,
                                                    s.name AS StoreName,
                                                    so.uid AS SalesOrderUID, 
                                                    so.sales_order_number AS SalesOrderNumber, 
                                                    so.draft_order_number AS DraftOrderNumber,
                                                    s.number AS StoreNumber,
                                                    so.line_count AS SKUCount,
                                                    so.net_amount AS NetAmount,
                                                    so.order_type AS OrderType,
                                                    so.status AS Status,
                                                    '[' + r.code + '] ' + r.name AS RouteName,
                                                    --CONVERT(VARCHAR, so.order_date, 106) AS OrderDate,
                                                    --CONVERT(VARCHAR, so.expected_delivery_date, 106) AS DeliveryDate,
                                                    so.order_date AS OrderDate,
                                                    so.expected_delivery_date AS DeliveryDate,
                                                    e.name + ' [' + e.login_id + ']' AS EmpName,
                                                    e.uid AS EmpUID,
                                                    so.modified_time AS SalesOrderModifiedTime
                                                FROM
                                                    sales_order so
                                                    INNER JOIN store s ON s.uid = so.store_uid
                                                    INNER JOIN org o ON o.uid = s.franchisee_org_uid
                                                    INNER JOIN route r ON r.uid = so.route_uid
                                                    INNER JOIN emp e ON e.uid = so.emp_uid
                                                WHERE
                                                    so.expected_delivery_date BETWEEN @startDate AND @endDate
                                                    AND so.status = @Status)As SubQuery");
            }
            Dictionary<string, object?> parameters = new()
            {
                { "startDate", startDate },
                { "endDate", endDate },
                { "Status", Status },
            };
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
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

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IDeliveredPreSales>().GetType();
            IEnumerable<Model.Interfaces.IDeliveredPreSales> deliveredPreSales = await ExecuteQueryAsync<Model.Interfaces.IDeliveredPreSales>(sql.ToString(), parameters, type);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SalesOrder.Model.Interfaces.IDeliveredPreSales> pagedResponse = new()
            {
                PagedData = deliveredPreSales,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<IViewPreSales> SelectDeliveredPreSalesBySalesOrderUID(string SalesOrderUID)
    {
        IViewPreSales? result = null;
        try
        {
            string sql = @"
                            SELECT DISTINCT
                                s.number AS CustomerNumber,
                                s.name AS CustomerName,
                                so.customer_po AS PONumber,
                                so.sales_order_number AS SalesOrderNumber,
                                so.draft_order_number AS DraftOrderNumber,
                                e.name + '[' + e.login_id + ']' AS SalesREP,
                                so.order_type AS OrderType,
                                so.line_count AS TotalSKUCount,
                                so.status AS OrderStatus,
                                so.payment_type AS PaymentType,
                                r.name + '[' + r.code + ']' AS RouteName,
                                so.order_date AS OrderDate,
                                so.expected_delivery_date AS ExpectedDeliveryDate,
                                so.delivered_date_time AS DeliveredDateTime,
                                so.notes AS Notes,
                                so.qty_count AS QtyCount,
                                so.total_amount AS TotalAmount,
                                so.total_discount AS TotalDiscount,
                                so.total_tax AS TotalTax,
                                so.net_amount AS NetAmount,
                                so.source AS Source,
                                so.reference_number AS ReferenceNumber,
                                so.reference_uid AS ReferenceUID
                            FROM
                                sales_order so
                                INNER JOIN sales_order_line sol ON so.uid = sol.sales_order_uid
                                INNER JOIN store s ON s.uid = so.store_uid
                                INNER JOIN emp e ON e.uid = so.emp_uid
                                INNER JOIN route r ON r.uid = so.route_uid
                            WHERE
                                sol.sales_order_uid = @SalesOrderUID;
                            
                            SELECT
                                s.code AS SKUCode,
                                s.name AS SKUName,
                                sol.item_type AS ItemType,
                                sol.uom AS UOM,
                                CASE WHEN so.order_type IN ('PreSales', 'Draft') THEN sol.qty ELSE sol.reco_qty END AS Quantity,
                                sol.delivered_qty AS DeliveredQty,
                                sol.unit_price AS Price,
                                sol.total_discount AS Discount,
                                sol.total_amount - sol.total_discount AS AmountExcTax,
                                sol.total_tax AS Tax,
                                sol.net_amount AS AmountIncTax,
                                sol.approved_qty AS ApprovedQty
                            FROM
                                sales_order_line sol
                                INNER JOIN sales_order so ON so.uid = sol.sales_order_uid
                                INNER JOIN sku s ON sol.sku_uid = s.uid
                            WHERE
                                sol.sales_order_uid = @SalesOrderUID
                            ORDER BY
                                sol.line_number ASC;";

            Dictionary<string, object?> parameters = new()
    {
        { "SalesOrderUID", SalesOrderUID },
    };

            DataSet ds = await ExecuteQueryDataSetAsync(sql, parameters);
            if (ds != null && ds.Tables.Count == 2)
            {
                DataTable dataTable0 = ds.Tables[0];
                DataTable dataTable1 = ds.Tables[1];
                result = _serviceProvider.CreateInstance<Winit.Modules.SalesOrder.Model.Interfaces.IViewPreSales>();
                foreach (DataRow row0 in dataTable0.Rows)
                {
                    result = new ViewPreSales
                    {
                        CustomerNumber = row0["CustomerNumber"] is DBNull ? "N/A" : row0["CustomerNumber"].ToString(),
                        CustomerName = row0["CustomerName"] is DBNull ? "N/A" : row0["CustomerName"].ToString(),
                        PONumber = row0["PONumber"] is DBNull ? "N/A" : row0["PONumber"].ToString(),
                        SalesOrderNumber = row0["SalesOrderNumber"] is DBNull ? "N/A" : row0["SalesOrderNumber"].ToString(),
                        DraftOrderNumber = row0["DraftOrderNumber"] is DBNull ? "N/A" : row0["DraftOrderNumber"].ToString(),
                        SalesREP = row0["SalesREP"] is DBNull ? "N/A" : row0["SalesREP"].ToString(),
                        OrderType = row0["OrderType"] is DBNull ? "N/A" : row0["OrderType"].ToString(),
                        TotalSKUCount = row0["TotalSKUCount"] is DBNull ? 0 : Convert.ToInt32(row0["TotalSKUCount"]),
                        OrderStatus = row0["OrderStatus"] is DBNull ? "N/A" : row0["OrderStatus"].ToString(),
                        PaymentType = row0["PaymentType"] is DBNull ? "N/A" : row0["PaymentType"].ToString(),
                        RouteName = row0["RouteName"] is DBNull ? "N/A" : row0["RouteName"].ToString(),
                        OrderDate = Convert.ToDateTime(row0["OrderDate"]),
                        ExpectedDeliveryDate = Convert.ToDateTime(row0["ExpectedDeliveryDate"]),
                        DeliveredDateTime = Convert.ToDateTime(row0["DeliveredDateTime"]),
                        QtyCount = row0["QtyCount"] is DBNull ? 0 : Convert.ToDecimal(row0["QtyCount"]),
                        TotalAmount = row0["TotalAmount"] is DBNull ? 0 : Convert.ToDecimal(row0["TotalAmount"]),
                        TotalDiscount = row0["TotalDiscount"] is DBNull ? 0 : Convert.ToDecimal(row0["TotalDiscount"]),
                        TotalTax = row0["TotalTax"] is DBNull ? 0 : Convert.ToDecimal(row0["TotalTax"]),
                        NetAmount = row0["NetAmount"] is DBNull ? 0 : Convert.ToDecimal(row0["NetAmount"]),
                        Notes = row0["Notes"] is DBNull ? "N/A" : row0["Notes"].ToString(),
                        ReferenceNumber = row0["ReferenceNumber"] is DBNull ? "N/A" : row0["ReferenceNumber"].ToString(),
                        ReferenceUID = row0["ReferenceUID"] is DBNull ? "N/A" : row0["ReferenceUID"].ToString(),
                        Source = row0["Source"] is DBNull ? "N/A" : row0["Source"].ToString(),
                        sKUViewPreSalesList = new List<ISKUViewPreSales>()
                    };
                    foreach (DataRow row1 in dataTable1.Rows)
                    {

                        ISKUViewPreSales skuViewPreSales = new SKUViewPreSales
                        {
                            SKUCode = row1["SKUCode"] is DBNull ? "N/A" : row1["SKUCode"].ToString(),
                            SKUName = row1["SKUName"] is DBNull ? "N/A" : row1["SKUName"].ToString(),
                            ItemType = row1["ItemType"] is DBNull ? "N/A" : row1["ItemType"].ToString(),
                            UoM = row1["UoM"] is DBNull ? "N/A" : row1["UoM"].ToString(),
                            Qty = row1.Table.Columns.Contains("Qty") ? (row1["Qty"] is DBNull ? 0 : Convert.ToDecimal(row1["Qty"])) : 0,
                            RecoQty = row1.Table.Columns.Contains("RecoQty") ? (row1["RecoQty"] is DBNull ? 0 : Convert.ToDecimal(row1["RecoQty"])) : 0,
                            DeliveredQty = row1["DeliveredQty"] is DBNull ? 0 : Convert.ToDecimal(row1["DeliveredQty"]),
                            Price = row1["Price"] is DBNull ? 0 : Convert.ToDecimal(row1["Price"]),
                            Discount = row1["Discount"] is DBNull ? 0 : Convert.ToDecimal(row1["Discount"]),
                            AmountExcTax = row1["AmountExcTax"] is DBNull ? 0 : Convert.ToDecimal(row1["AmountExcTax"]),
                            Tax = row1["Tax"] is DBNull ? 0 : Convert.ToDecimal(row1["Tax"]),
                            AmountIncTax = row1["AmountIncTax"] is DBNull ? 0 : Convert.ToDecimal(row1["AmountIncTax"]),
                            ApprovedQty = row1["ApprovedQty"] is DBNull ? 0 : Convert.ToDecimal(row1["ApprovedQty"]),
                        };
                        result.sKUViewPreSalesList?.Add(skuViewPreSales);

                    }
                }
            }
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task<int> CUDSalesOrder_New(Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder salesOrder,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int count = 0;
        try
        {
            string? isExists = await CheckIfUIDExistsInDB(DbTableName.SalesOrder, salesOrder.UID, connection, transaction);
            if (!string.IsNullOrEmpty(isExists))
            {
                count += await UpdateSalesOrder(salesOrder, connection, transaction);
            }
            else
            {
                count += await CreateSalesOrder(salesOrder, connection, transaction);
            }
        }
        catch
        {
            throw;
        }
        return count;
    }
    private async Task<int> CUDSalesOrderLine_New(List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine> salesOrderLines,IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int count = 0;
        try
        {
            if (salesOrderLines == null || salesOrderLines.Count == 0)
            {
                return count;
            }
            List<string> uidList = salesOrderLines.Select(po => po.UID).ToList();
            List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.SalesOrderLine, uidList, connection, transaction);
            List<ISalesOrderLine>? newSalesOrderLines = null;
            List<ISalesOrderLine>? existingSalesOrderLines = null;
            if (existingUIDs != null && existingUIDs.Count > 0)
            {
                newSalesOrderLines = salesOrderLines.Where(sol => !existingUIDs.Contains(sol.UID)).ToList();
                existingSalesOrderLines = salesOrderLines.Where(e => existingUIDs.Contains(e.UID)).ToList();
            }
            else
            {
                newSalesOrderLines = salesOrderLines;
            }

            if (existingSalesOrderLines != null && existingSalesOrderLines.Any())
            {
                await UpdateSalesOrderLine(existingSalesOrderLines, connection, transaction);
            }
            if (newSalesOrderLines.Any())
            {
                await CreateSalesOrderLine(newSalesOrderLines, connection, transaction);
            }
        }
        catch
        {
            throw;
        }
        return count;
    }
    public async Task<int> InsertorUpdate_SalesOrders(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderView)
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
                        if (salesOrderView.SalesOrder != null)
                        {
                            count += await CUDSalesOrder_New(salesOrderView.SalesOrder, connection, transaction);
                        }
                        if (salesOrderView.SalesOrderLines != null && salesOrderView.SalesOrderLines.Count > 0)
                        {
                            count += await CUDSalesOrderLine_New(salesOrderView.SalesOrderLines.ToList<ISalesOrderLine>(),
                                connection, transaction);
                        }
                        if (salesOrderView.StoreHistory != null)
                        {
                            count += await _storeHistoryDL.CUDStoreHistory(salesOrderView.StoreHistory, connection, transaction);
                        }
                        if (salesOrderView.AccPayable != null)
                        {
                            count += await _collectionModuleDL.CUDAccPayable(new List<IAccPayable> { salesOrderView.AccPayable }, connection, transaction);
                        }
                        if (salesOrderView.WHStockLedgerList != null && salesOrderView.WHStockLedgerList.Count > 0)
                        {
                            count += await _stockUpdaterDL.UpdateStockAsync(salesOrderView.WHStockLedgerList.ToList<IWHStockLedger>(), connection, transaction);
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
    public async Task<ISalesOrder> GetSalesOrderByUID(string salesOrderUID)
    {
        StringBuilder sql = new(@"SELECT *  from sales_order WHERE uid = @SalesOrderUID");
        Dictionary<string, object?> parameters = new()
        {
            { "SalesOrderUID", salesOrderUID }
        };
        return await ExecuteSingleAsync<ISalesOrder>(sql.ToString(), parameters);
    }
    public async Task<List<ISalesOrderLine>?> GetSalesOrderLinesBySalesOrderUID(string salesOrderUID)
    {
        StringBuilder sql = new(@"Select id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime,
                                  modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                  server_modified_time AS ServerModifiedTime, sales_order_uid AS SalesOrderUID, 
                                  line_number AS LineNumber, item_code AS ItemCode, item_type AS ItemType, 
                                  base_price AS BasePrice, unit_price AS UnitPrice, fake_unit_price AS FakeUnitPrice, 
                                  base_uom AS BaseUOM, uom AS UoM, uom_conversion_to_bu AS UOMConversionToBU, 
                                  reco_uom AS RecoUOM, reco_qty AS RecoQty, reco_uom_conversion_to_bu AS RecoUOMConversionToBU,
                                  reco_qty_bu AS RecoQtyBU, model_qty_bu AS ModelQtyBU, qty AS Qty, qty_bu AS QtyBU, 
                                  van_qty_bu AS VanQtyBU, delivered_qty AS DeliveredQty, missed_qty AS MissedQty, 
                                  returned_qty AS ReturnedQty, total_amount AS TotalAmount, total_discount AS TotalDiscount, 
                                  line_tax_amount AS LineTaxAmount, prorata_tax_amount AS ProrataTaxAmount, total_tax AS TotalTax, 
                                  net_amount AS NetAmount, net_fake_amount AS NetFakeAmount, sku_price_uid AS SKUPriceUID, 
                                  prorata_discount_amount AS ProrataDiscountAmount, line_discount_amount AS LineDiscountAmount,
                                  mrp AS MRP, cost_unit_price AS CostUnitPrice, parent_uid AS ParentUID, 
                                  is_promotion_applied AS IsPromotionApplied, volume AS Volume, volume_unit AS VolumeUnit, 
                                  weight AS Weight, weight_unit AS WeightUnit, stock_type AS StockType, remarks AS Remarks,
                                  total_cash_discount AS TotalCashDiscount, total_excise_duty AS TotalExciseDuty, 
                                  sku_uid AS SKUUID,tax_data as TaxData From sales_order_line where sales_order_uid = @SalesOrderUID");
        Dictionary<string, object?> parameters = new()
        {
            { "SalesOrderUID", salesOrderUID }
        };
        List<ISalesOrderLine> salesOrders = await ExecuteQueryAsync<ISalesOrderLine>(sql.ToString(), parameters);
        return salesOrders;
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

            return await ExecuteQueryAsync<ISalesOrderInvoice>(sql, new { StoreUID = storeUID });
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




    #region UnUsed Methods
    private async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder?> SelectSalesOrder_ByUID(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID",  UID}
        };
        string sql = @"SELECT * FROM sales_order WHERE uid = @UID";
        return await ExecuteSingleAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder>(sql, parameters, null, connection, transaction);
    }
    private async Task<List<string>> SelectSalesOrderLineByUID(List<string> uids,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        var parameters = new { UIDs = uids };
        string sql = @"SELECT  uid FROM sales_order_line WHERE uid In @UIDs";
        return await ExecuteQueryAsync<string>(sql, parameters, null, connection);
    }
    public async Task<IEnumerable<ISalesOrderViewModel>> SelectSalesOrderDetailsAll(List<SortCriteria> sortCriterias, int pageNumber,
 int pageSize, List<FilterCriteria> filterCriterias)
    {
        try
        {
            StringBuilder sql = new(@"SELECT so.id AS SalesOrderId, so.uid AS SalesOrderUID, 
                            so.created_by AS SalesOrderCreatedBy, so.created_time AS SalesOrderCreatedTime, 
                            so.modified_by AS SalesOrderModifiedBy, so.modified_time AS SalesOrderModifiedTime, 
                            so.server_add_time AS SalesOrderServerAddTime, so.server_modified_time AS SalesOrderServerModifiedTime, 
                            so.sales_order_number AS SalesOrderNumber, so.draft_order_number AS DraftOrderNumber, so.org_uid AS OrgUID, 
                            so.distribution_channel_uid AS DistributionChannelUID, so.delivered_by_org_uid AS DeliveredByOrgUID, 
                            so.store_uid AS StoreUID, so.status AS Status, so.order_type AS OrderType, so.order_date AS OrderDate, 
                            so.customer_po AS CustomerPO, so.currency_uid AS CurrencyUID, so.payment_type AS PaymentType, so.total_amount AS TotalAmount,
                            so.total_discount AS TotalDiscount, so.total_tax AS TotalTax, so.net_amount AS NetAmount, so.line_count AS LineCount,
                            so.qty_count AS QtyCount, so.total_fake_amount AS TotalFakeAmount, so.reference_number AS ReferenceNumber, 
                            so.source AS Source, so.total_line_discount AS TotalLineDiscount,so.total_cash_discount AS TotalCashDiscount,
                            so.total_header_discount AS TotalHeaderDiscount,so.total_excise_duty AS TotalExciseDuty,so.total_line_tax AS TotalLineTax,
                            so.total_header_tax AS TotalHeaderTax, so.id AS SalesOrderInfoId, so.uid AS SalesOrderInfoUID, 
                            so.created_by AS SalesOrderInfoCreatedBy, so.created_time AS SalesOrderInfoCreatedTime, 
                            so.modified_by AS SalesOrderInfoModifiedBy, so.modified_time AS SalesOrderInfoModifiedTime, 
                            so.server_add_time AS SalesOrderInfoServerAddTime,so.server_modified_time AS SalesOrderInfoServerModifiedTime,
                            so.sales_order_uid AS SalesOrderUID, so.job_position_uid AS JobPositionUID, so.emp_uid AS EmpUID, 
                            so.beat_history_uid AS BeatHistoryUID, so.route_uid AS RouteUID, so.store_history_uid AS StoreHistoryUID, 
                            so.total_credit_limit AS TotalCreditLimit, so.available_credit_limit AS AvailableCreditLimit, 
                            so.expected_delivery_date AS ExpectedDeliveryDate, so.delivered_date_time AS DeliveredDateTime, 
                            so.latitude AS Latitude, so.longitude AS Longitude, so.is_offline AS IsOffline, so.credit_days AS CreditDays, 
                            so.notes AS Notes, so.delivery_instructions AS DeliveryInstructions, so.remarks AS Remarks, 
                            so.is_temperature_check_enabled AS IsTemperatureCheckEnabled, so.always_printed_flag AS AlwaysPrintedFlag, 
                            so.purchase_order_no_required_flag AS PurchaseOrderNoRequiredFlag, so.is_with_printed_invoices_flag AS IsWithPrintedInvoicesFlag
                            FROM sales_order AS so ");

            Dictionary<string, object?> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                _ = sql.Append(" WHERE ");
                AppendFilterCriteria<ISalesOrderViewModel>(filterCriterias, sql, parameters);
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
                    ISalesOrder salesOrder = ConvertDataTableToObject1<SalesOrder.Model.Interfaces.ISalesOrder>(row, salesOrderFactory, salesOrderColumnMappings);
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
            string sql = @"SELECT so.id AS SalesOrderId, so.uid AS SalesOrderUID, so.created_by AS SalesOrderCreatedBy, 
                        so.created_time AS SalesOrderCreatedTime, so.modified_by AS SalesOrderModifiedBy, 
                        so.modified_time AS SalesOrderModifiedTime, so.server_add_time AS SalesOrderServerAddTime,
                        so.server_modified_time AS SalesOrderServerModifiedTime, so.company_uid AS CompanyUID, so.sales_order_number AS SalesOrderNumber, 
                        so.draft_order_number AS DraftOrderNumber, so.org_uid AS OrgUID, so.distribution_channel_uid AS DistributionChannelUID, 
                        so.delivered_by_org_uid AS DeliveredByOrgUID, so.store_uid AS StoreUID, so.status AS Status, so.order_type AS OrderType, 
                        so.order_date AS OrderDate, so.customer_po AS CustomerPO, so.currency_uid AS CurrencyUID, so.payment_type AS PaymentType, 
                        so.total_amount AS TotalAmount, so.total_discount AS TotalDiscount, so.total_tax AS TotalTax, so.net_amount AS NetAmount,
                        so.line_count AS LineCount, so.qty_count AS QtyCount, so.total_fake_amount AS TotalFakeAmount, so.reference_number AS ReferenceNumber,
                        so.source AS Source, so.total_line_discount AS TotalLineDiscount,so.total_cash_discount AS TotalCashDiscount,
                        so.total_header_discount AS TotalHeaderDiscount,so.total_excise_duty AS TotalExciseDuty,so.total_line_tax AS TotalLineTax,
                        so.total_header_tax AS TotalHeaderTax,so.cash_sales_customer AS CashSalesCustomer,so.cash_sales_address AS CashSalesAddress,
                        so.id AS SalesOrderInfoId, so.uid AS SalesOrderInfoUID, so.created_by AS SalesOrderInfoCreatedBy,
                        so.created_time AS SalesOrderInfoCreatedTime, so.modified_by AS SalesOrderInfoModifiedBy,
                        so.modified_time AS SalesOrderInfoModifiedTime, so.server_add_time AS SalesOrderInfoServerAddTime,
                        so.server_modified_time AS SalesOrderInfoServerModifiedTime, so.sales_order_uid AS SalesOrderUID, 
                        so.job_position_uid AS JobPositionUID, so.emp_uid AS EmpUID, so.beat_history_uid AS BeatHistoryUID, 
                        so.route_uid AS RouteUID, so.store_history_uid AS StoreHistoryUID, so.total_credit_limit AS TotalCreditLimit, 
                        so.available_credit_limit AS AvailableCreditLimit, so.expected_delivery_date AS ExpectedDeliveryDate, 
                        so.delivered_date_time AS DeliveredDateTime, so.latitude AS Latitude, so.longitude AS Longitude, 
                        so.is_offline AS IsOffline, so.credit_days AS CreditDays, so.notes AS Notes, so.delivery_instructions AS DeliveryInstructions, 
                        so.remarks AS Remarks, so.is_temperature_check_enabled AS IsTemperatureCheckEnabled, so.always_printed_flag AS AlwaysPrintedFlag,                      
                        so.is_with_printed_invoices_flag AS IsWithPrintedInvoicesFlag, sol.id AS SalesOrderLineId, sol.uid AS SalesOrderLineUID, 
                        sol.created_by AS SalesOrderLineCreatedBy, sol.created_time AS SalesOrderLineCreatedTime, sol.modified_by AS SalesOrderLineModifiedBy,
                        sol.modified_time AS SalesOrderLineModifiedTime, sol.server_add_time AS SalesOrderLineServerAddTime,
                        sol.server_modified_time AS SalesOrderLineServerModifiedTime, sol.sales_order_uid AS SalesOrderUID, 
                        sol.line_number AS LineNumber, sol.item_code AS ItemCode, sol.item_type AS ItemType, sol.base_price AS BasePrice, 
                        sol.unit_price AS UnitPrice, sol.fake_unit_price AS FakeUnitPrice, sol.base_uom AS BaseUOM, sol.uom AS UOM, 
                        sol.uom_conversion_to_bu AS UOMConversionToBU, sol.reco_uom AS RecoUOM, sol.reco_qty AS RecoQty, 
                        sol.reco_uom_conversion_to_bu AS RecoUOMConversionToBU, sol.reco_qty_bu AS RecoQtyBU, sol.model_qty_bu AS ModelQtyBU, 
                        sol.qty AS Qty, sol.qty_bu AS QtyBU, sol.van_qty_bu AS VanQtyBU, sol.delivered_qty AS DeliveredQty,
                        sol.missed_qty AS MissedQty, sol.returned_qty AS ReturnedQty, sol.total_amount AS TotalAmount, 
                        sol.total_discount AS TotalDiscount, sol.line_tax_amount AS LineTaxAmount, sol.prorata_tax_amount AS ProrataTaxAmount, 
                        sol.total_tax AS TotalTax, sol.net_amount AS NetAmount, sol.net_fake_amount AS NetFakeAmount, sol.sku_price_uid AS SKUPriceUID,
                        sol.prorata_discount_amount AS ProrataDiscountAmount, sol.line_discount_amount AS LineDiscountAmount, sol.mrp AS MRP, 
                        sol.cost_unit_price AS CostUnitPrice, sol.parent_uid AS ParentUID, sol.is_promotion_applied AS IsPromotionApplied, 
                        sol.volume AS Volume, sol.volume_unit AS VolumeUnit, sol.weight AS Weight, sol.weight_unit AS WeightUnit,
                        sol.stock_type AS StockType, sol.remarks AS Remarks
                        FROM sales_order so 
                        INNER JOIN sales_order_line sol ON sol.sales_order_uid = so.sales_order_uid 
                        WHERE sol.sales_order_uid = @SalesOrderUID";
            // so.purchase_order_no_required_flag, 
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
                    ISalesOrder salesOrder = ConvertDataTableToObject1<SalesOrder.Model.Interfaces.ISalesOrder>(row, salesOrderFactory, salesOrderColumnMappings);
                    ISalesOrderLine salesOrderLine = ConvertDataTableToObject1<SalesOrder.Model.Interfaces.ISalesOrderLine>(row, salesOrderLineFactory, salesOrderLineColumnMappings);
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
    public Task<int> SaveSalesOrder(SalesOrderViewModelDCO salesOrderViewModel)
    {
        throw new NotImplementedException();
    }
    public async Task<int> CUD_SalesOrder(Winit.Modules.SalesOrder.Model.Classes.SalesOrderViewModelDCO salesOrderViewModel)
    {
        try
        {
            //DataTable dtLine = new DataTable();
            //DataRow drLine=null;
            //dtLine.Columns.Add(new DataColumn("UID", typeof(string)));
            //dtLine.Columns.Add(new DataColumn("CreatedBy", typeof(string)));
            //dtLine.Columns.Add(new DataColumn("CreatedTime", typeof(DateTime)));
            //dtLine.Columns.Add(new DataColumn("ModifiedBy", typeof(string)));
            //dtLine.Columns.Add(new DataColumn("ModifiedTime", typeof(DateTime)));
            //dtLine.Columns.Add(new DataColumn("ServerAddTime", typeof(DateTime)));
            //dtLine.Columns.Add(new DataColumn("ServerModifiedTime", typeof(DateTime)));
            //dtLine.Columns.Add(new DataColumn("SalesOrderUID", typeof(string)));
            //foreach (SalesOrderLine objSalesOrderLine in salesOrderViewModel.SalesOrderLines)
            //{
            //    drLine = dtLine.NewRow();
            //    drLine["UID"] = objSalesOrderLine.UID;
            //    drLine["CreatedBy"] = objSalesOrderLine.CreatedBy;
            //    drLine["CreatedTime"] = objSalesOrderLine.CreatedTime;
            //    drLine["ModifiedBy"] = objSalesOrderLine.ModifiedBy;
            //    drLine["ModifiedTime"] = objSalesOrderLine.ModifiedTime;
            //    drLine["ServerAddTime"] = objSalesOrderLine.ServerAddTime;
            //    drLine["ServerModifiedTime"] = objSalesOrderLine.ServerModifiedTime;
            //    drLine["SalesOrderUID"] = objSalesOrderLine.SalesOrderUID;
            //    dtLine.Rows.Add(drLine);
            //}
            //var salesOrderProc = @"call public.sp_salesorderdatachecking (:in_uid,:in_createdby,:in_createdtime,:in_modifiedby,
            //:in_modifiedtime,:in_serveraddtime,:in_servermodifiedtime,:in_salesordernumber,:in_draftordernumber,:in_companyuid,
            //:in_orguid,:in_distributionchanneluid,:in_deliveredbyorguid,:in_storeuid,:in_status,:in_ordertype,:in_orderdate,
            //:in_customerpo,:in_totalamount,:in_totaldiscount,:in_netamount,:in_totaltax,:in_linecount,:in_qtycount,
            //:in_totalfakeamount,:in_referencenumber,:in_source,:in_ss,:in_totallinediscount,:in_totalcashdiscount,:in_totalheaderdiscount,
            //:in_totalexciseduty,:in_totallinetax,:in_totalheadertax,:in_cashsalescustomer,:in_cashsalesaddress,:in_salesorderinfouid,
            //:in_salesorderinfocreatedby,:in_salesorderinfocreatedtime,:in_salesorderinfomodifiedby,
            //:in_salesorderinfomodifiedtime,:in_salesorderinfoserveraddtime,:in_salesorderinfoservermodifiedtime,:in_salesorderinfosalesorderuid,
            //:in_jobpositionuid,:in_empuid,:in_beathistoryuid,:in_routeuid,:in_storehistoryuid,:in_totalcreditlimit,:in_availablecreditlimit,
            //:in_expecteddeliverydate,:in_delivereddatetime,:in_latitude,:in_longitude,:in_isoffline,:in_creditdays,:in_notes,:in_deliveryinstructions,
            //:in_remarks,:in_istemperaturecheckenabled,:in_alwaysprintedflag,:in_purchaseordernorequiredflag,:in_iswithprintedinvoicesflag)";
            string query = @"call public.sp_salesorderdatachecking (:in_uid,:in_createdby,:in_createdtime,
                        :in_modifiedby,:in_modifiedtime,:in_serveraddtime,:in_servermodifiedtime,:in_salesordernumber,:in_draftordernumber,
                        :in_companyuid,:in_orguid,:in_distributionchanneluid,:in_deliveredbyorguid,:in_storeuid,:in_status,
                        :in_ordertype,:in_orderdate)";
            Dictionary<string, object?> Parameters = new()
            {
   //     { "in_salesorderlines", ConvertToJsondrLine },
        { "in_uid", salesOrderViewModel.SalesOrder.UID },
        { "in_createdby", salesOrderViewModel.SalesOrder.CreatedBy },
        { "in_createdtime", salesOrderViewModel.SalesOrder.CreatedTime },
        { "in_modifiedby", salesOrderViewModel.SalesOrder.ModifiedBy },
        { "in_modifiedtime", salesOrderViewModel.SalesOrder.ModifiedTime },
        { "in_serveraddtime", salesOrderViewModel.SalesOrder.ServerAddTime },
        { "in_servermodifiedtime", salesOrderViewModel.SalesOrder.ServerModifiedTime },
        { "in_salesordernumber", salesOrderViewModel.SalesOrder.SalesOrderNumber },
        { "in_draftordernumber", salesOrderViewModel.SalesOrder.DraftOrderNumber },
        { "in_companyuid",salesOrderViewModel.SalesOrder.CompanyUID },
        { "in_orguid", salesOrderViewModel.SalesOrder.OrgUID },
        { "in_distributionchanneluid", salesOrderViewModel.SalesOrder.DistributionChannelUID },
        { "in_deliveredbyorguid", salesOrderViewModel.SalesOrder.DeliveredByOrgUID },
        { "in_storeuid", salesOrderViewModel.SalesOrder.StoreUID },
        { "in_status", salesOrderViewModel.SalesOrder.Status },
        { "in_ordertype", salesOrderViewModel.SalesOrder.OrderType },
        { "in_orderdate", salesOrderViewModel.SalesOrder.OrderDate },
       // { "in_customerpo", salesOrderViewModel.SalesOrder.CustomerPO },
       //// { "CurrencyUID", salesOrderViewModel.SalesOrder.CurrencyUID },
       // { "in_totalamount", salesOrderViewModel.SalesOrder.TotalAmount },
       // { "in_totaldiscount", salesOrderViewModel.SalesOrder.TotalDiscount },
       // { "in_netamount", salesOrderViewModel.SalesOrder.NetAmount },
       // { "in_totaltax", salesOrderViewModel.SalesOrder.TotalTax },
       // { "in_linecount", salesOrderViewModel.SalesOrder.LineCount },
       // { "in_qtycount", salesOrderViewModel.SalesOrder.QtyCount },
       // { "in_totalfakeamount", salesOrderViewModel.SalesOrder.TotalFakeAmount },
       // { "in_referencenumber", salesOrderViewModel.SalesOrder.ReferenceNumber },
       // { "in_source", salesOrderViewModel.SalesOrder.Source },
       // { "in_ss", salesOrderViewModel.SalesOrder.SS },
       // { "in_totallinediscount", salesOrderViewModel.SalesOrder.TotalLineDiscount },
       // { "in_totalcashdiscount", salesOrderViewModel.SalesOrder.TotalCashDiscount },
       // { "in_totalheaderdiscount", salesOrderViewModel.SalesOrder.TotalHeaderDiscount },
       // { "in_totalexciseduty", salesOrderViewModel.SalesOrder.TotalExciseDuty },
       // { "in_totallinetax", salesOrderViewModel.SalesOrder.TotalLineDiscount },
       // { "in_totalheadertax", salesOrderViewModel.SalesOrder.TotalHeaderDiscount },
       // { "in_cashsalescustomer", salesOrderViewModel.SalesOrder.CashSalesCustomer },
       // { "in_cashsalesaddress", salesOrderViewModel.SalesOrder.CashSalesAddress },
       // { "in_salesorderinfouid",salesOrderViewModel.SalesOrderInfo.UID},
       // { "in_salesorderinfocreatedby", salesOrderViewModel.SalesOrderInfo.CreatedBy },
       // { "in_salesorderinfocreatedtime", salesOrderViewModel.SalesOrderInfo.CreatedTime },
       // { "in_salesorderinfomodifiedby", salesOrderViewModel.SalesOrderInfo.ModifiedBy },
       // { "in_salesorderinfomodifiedtime", salesOrderViewModel.SalesOrderInfo.ModifiedTime },
       // { "in_salesorderinfoserveraddtime", salesOrderViewModel.SalesOrderInfo.ServerAddTime },
       // { "in_salesorderinfoservermodifiedtime", salesOrderViewModel.SalesOrderInfo.ServerModifiedTime },
       // { "in_salesorderinfosalesorderuid", salesOrderViewModel.SalesOrderInfo.SalesOrderUID },
       // { "in_jobpositionuid", salesOrderViewModel.SalesOrderInfo.JobPositionUID },
       // { "in_empuid", salesOrderViewModel.SalesOrderInfo.EmpUID },
       // { "in_beathistoryuid", salesOrderViewModel.SalesOrderInfo.BeatHistoryUID },
       // { "in_routeuid", salesOrderViewModel.SalesOrderInfo.RouteUID },
       // { "in_storehistoryuid", salesOrderViewModel.SalesOrderInfo.StoreHistoryUID },
       // { "in_totalcreditlimit", salesOrderViewModel.SalesOrderInfo.TotalCreditLimit },
       // { "in_availablecreditlimit", salesOrderViewModel.SalesOrderInfo.AvailableCreditLimit },
       // { "in_expecteddeliverydate", salesOrderViewModel.SalesOrderInfo.ExpectedDeliveryDate },
       // { "in_delivereddatetime", salesOrderViewModel.SalesOrderInfo.DeliveredDateTime },
       // { "in_latitude", salesOrderViewModel.SalesOrderInfo.Latitude },
       // { "in_longitude", salesOrderViewModel.SalesOrderInfo.Longitude },
       // { "in_isoffline", salesOrderViewModel.SalesOrderInfo.IsOffline },
       // { "in_creditdays", salesOrderViewModel.SalesOrderInfo.CreditDays },
       // { "in_notes", salesOrderViewModel.SalesOrderInfo.Notes },
       // { "in_deliveryinstructions", salesOrderViewModel.SalesOrderInfo.DeliveryInstructions },
       // { "in_remarks", salesOrderViewModel.SalesOrderInfo.Remarks },
       // { "in_istemperaturecheckenabled", salesOrderViewModel.SalesOrderInfo.IsTemperatureCheckEnabled },
       // { "in_alwaysprintedflag", salesOrderViewModel.SalesOrderInfo.AlwaysPrintedFlag },
       // { "in_purchaseordernorequiredflag", salesOrderViewModel.SalesOrderInfo.PurchaseOrderNoRequiredFlag },
       // { "in_iswithprintedinvoicesflag", salesOrderViewModel.SalesOrderInfo.IsWithPrintedInvoicesFlag },
      //  { "in_salesorderlines", ConvertToJson(salesOrderViewModel.SalesOrderLines) },
  

// BaseModelV2
//{ "in_uid", "6037ce60afhggs5a540D0d76ss1s33ss11" },
//{ "in_createdby", "WINIT" },
//{ "in_createdtime", DateTime.Now },
//{ "in_modifiedby", "WINIT" },
//{ "in_modifiedtime", DateTime.Now },
//{ "in_serveraddtime", DateTime.Now },
//{ "in_servermodifiedtime", DateTime.Now },

//// SalesOrders
//{ "in_salesordernumber", "sales_ossssssrdDseegrf111gdd1wQ411" },
//{ "in_draftordernumber", "draft_ordesesssDsssrg1ssf111gs4Q11" },
//{ "in_companyuid", "WINIT" },
//{ "in_orguid", "WINIT" },
//{ "in_distributionchanneluid", "WINIT" },
//{ "in_deliveredbyorguid", "WINIT" },
//{ "in_storeuid", "0EDD3D28-0C02-48CF-B98D-2529DA691F68" },
//{ "in_status", "status_value" },
//{ "in_ordertype", "order_type_value" },
//{ "in_orderdate", DateTime.Now },
//{ "in_customerpo", "customer_po_value" },
//{ "in_currencyuid", "INR" },
//{ "in_paymenttype", "payment_type_value" },
//{ "in_totalamount", 100.00 },
//{ "in_totaldiscount", 10.00 },
//{ "in_totaltax", 5.00 },
//{ "in_netamount", 85.00 },
//{ "in_linecount", 2 },
//{ "in_qtycount", 50 },
//{ "in_totalfakeamount", 90.00 },
//{ "in_referencenumber", "reference_number_value" },
//{ "in_source", "source" },
//{ "in_ss", 1 },
//{ "in_totallinediscount", 8.00 },
//{ "in_totalcashdiscount", 6.00 },
//{ "in_totalheaderdiscount", 12.00 },
//{ "in_totalexciseduty", 3.00 },
//{ "in_totallinetax", 7.00 },
//{ "in_totalheadertax", 10.00 },
//{ "in_cashsalescustomer", "cash_sales_customer_value" },
//{ "in_cashsalesaddress", "cash_sales_address_value" },

//// SalesOrderInfo
//{ "in_salesorderinfouid", "6037ce60-fea3-54efa-bf2c-afhg5a5400d761331" },
//{ "in_salesorderinfocreatedby", "WINIT" },
//{ "in_salesorderinfocreatedtime", DateTime.Now },
//{ "in_salesorderinfomodifiedby", "WINIT" },
//{ "in_salesorderinfomodifiedtime", DateTime.Now },
//{ "in_salesorderinfoserveraddtime", DateTime.Now },
//{ "in_salesorderinfoservermodifiedtime", DateTime.Now },
//{ "in_salesorderinfosalesorderuid", "6037ce60-fea3-54efa-bf2c-afhg5a5400d761331" },
//{ "in_jobpositionuid", "Driver1" },
//{ "in_empuid", "WINIT" },
//{ "in_beathistoryuid", "6037ce60-fea3-4efa-bf2c-afhg5a5400d761331" },
//{ "in_routeuid", "97f62e65-73fa-4fe5-97de-21c59c8c6b4d" },
//{ "in_storehistoryuid", "97f62e65-73fa-4fe5-97de-21c59c8c6b4d" },
//{ "in_totalcreditlimit", 500.00 },
//{ "in_availablecreditlimit", 300.00 },
//{ "in_expecteddeliverydate", DateTime.Now },
//{ "in_delivereddatetime", DateTime.Now },
//{ "in_latitude", "latitude_value" },
//{ "in_longitude", "longitude_value" },
//{ "in_isoffline", true },
//{ "in_creditdays", 15 },
//{ "in_notes", "notes_value" },
//{ "in_deliveryinstructions", "delivery_instructions_value" },
//{ "in_remarks", "remarks_value" },
//{ "in_istemperaturecheckenabled", true },
//{ "in_alwaysprintedflag", 1 },
//{ "in_purchaseordernorequiredflag", 1 },
//{ "in_iswithprintedinvoicesflag", true },
};



            int count = /*await ExecuteStoredProcedureAsync(query, Parameters)*/ 1;

            return count == 0 ? throw new Exception("Operation Failed.") : count;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreateSalesOrder(Model.Interfaces.ISalesOrder salesOrder, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrder != null)
            {
                string salesOrderQuery = @"INSERT INTO sales_order (uid, ss, created_by, created_time, modified_by, 
                modified_time,server_add_time, server_modified_time, sales_order_number, draft_order_number, 
                company_uid, org_uid, distribution_channel_uid, delivered_by_org_uid, store_uid, 
                status, order_type, order_date, customer_po, currency_uid, 
                total_amount, total_discount, total_tax, line_count, qty_count, 
                total_fake_amount, reference_number, source,cash_sales_customer,cash_sales_address,reference_uid,
                reference_type,job_position_uid, emp_uid, beat_history_uid, route_uid, store_history_uid, total_credit_limit, available_credit_limit, 
                expected_delivery_date, delivered_date_time, latitude, longitude, is_offline, credit_days, notes, delivery_instructions, remarks, 
                is_temperature_check_enabled, always_printed_flag, purchase_order_no_required_flag, is_with_printed_invoices_flag, tax_data)
                VALUES (
                @UID, 0, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                @ServerAddTime, @ServerModifiedTime, @SalesOrderNumber, @DraftOrderNumber, 
                @CompanyUID, @OrgUID, @DistributionChannelUID, @DeliveredByOrgUID, @StoreUID, 
                @Status, @OrderType, @OrderDate, @CustomerPO, @CurrencyUID, 
                @TotalAmount, @TotalDiscount, @TotalTax, @LineCount, @QtyCount, 
                @TotalFakeAmount, @ReferenceNumber, @Source,@CashSalesCustomer,@CashSalesAddress,@ReferenceUID,@ReferenceType,
                @JobPositionUID, @EmpUID, @BeatHistoryUID, @RouteUID, @StoreHistoryUID, @TotalCreditLimit, @AvailableCreditLimit, 
                @ExpectedDeliveryDate, @DeliveredDateTime, @Latitude, @Longitude, @IsOffline, @CreditDays, @Notes, @DeliveryInstructions, 
                @Remarks, @IsTemperatureCheckEnabled, @AlwaysPrintedFlag, @PurchaseOrderNoRequiredFlag, @IsWithPrintedInvoicesFlag, @TaxData::json)";

                retValue = await ExecuteNonQueryAsync(salesOrderQuery, connection, transaction, salesOrder);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateSalesOrder(Model.Interfaces.ISalesOrder salesOrder, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrder != null)
            {
                string salesOrderQuery = @"UPDATE sales_order SET 
                                            modified_by = @ModifiedBy, 
                                            modified_time = @ModifiedTime, 
                                            server_modified_time = @ServerModifiedTime, 
                                            sales_order_number = @SalesOrderNumber, 
                                            draft_order_number = @DraftOrderNumber, 
                                            status = @Status, 
                                            order_type = @OrderType, 
                                            order_date = @OrderDate, 
                                            customer_po = @CustomerPO, 
                                            currency_uid = @CurrencyUID, 
                                            payment_type = @PaymentType, 
                                            total_amount = @TotalAmount, 
                                            total_discount = @TotalDiscount, 
                                            total_tax = @TotalTax, 
                                            net_amount = @NetAmount, 
                                            line_count = @LineCount, 
                                            qty_count = @QtyCount, 
                                            total_fake_amount = @TotalFakeAmount, 
                                            reference_number = @ReferenceNumber, 
                                            source = @Source, 
                                            ss = 0, 
                                            total_line_discount = @TotalLineDiscount, 
                                            total_cash_discount = @TotalCashDiscount, 
                                            total_header_discount = @TotalHeaderDiscount, 
                                            total_excise_duty = @TotalExciseDuty, 
                                            total_line_tax = @TotalLineTax, 
                                            total_header_tax = @TotalHeaderTax, 
                                            cash_sales_customer = @CashSalesCustomer, 
                                            cash_sales_address = @CashSalesAddress,
                                            reference_uid = @ReferenceUID,
                                            reference_type = @ReferenceType,
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
                                            tax_data = JSON_QUERY(@TaxData)
                                             WHERE uid = @UID";
                retValue = await ExecuteNonQueryAsync(salesOrderQuery, connection, transaction, salesOrder);
            }
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> CreateSalesOrderLine(List<ISalesOrderLine> salesOrderLines, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrderLines == null || salesOrderLines.Count == 0)
            {
                return retValue;
            }
            string salesOrderLineQuery = @"INSERT INTO sales_order_line (uid, ss,created_by, created_time,
                                     modified_by, modified_time, server_add_time, server_modified_time, sales_order_uid, 
                                     line_number, item_code, item_type, base_price, unit_price, fake_unit_price, 
                                     base_uom, uom, uom_conversion_to_bu, reco_uom, reco_qty, reco_uom_conversion_to_bu, 
                                     reco_qty_bu, model_qty_bu, qty, qty_bu,van_qty_bu, delivered_qty, missed_qty, 
                                     returned_qty, total_amount, total_discount, line_tax_amount, prorata_tax_amount, 
                                     total_tax, net_amount, net_fake_amount, sku_price_uid, prorata_discount_amount,
                                     line_discount_amount, mrp, cost_unit_price, parent_uid, is_promotion_applied,
                                     volume, volume_unit, weight, stock_type, remarks, 
                                     total_cash_discount, total_excise_duty, sku_uid,approved_qty,tax_data)
                                      VALUES 
                                      (@UID, 0, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                                      @SalesOrderUID, @LineNumber, @ItemCode, @ItemType, @BasePrice, @UnitPrice, @FakeUnitPrice, 
                                      @BaseUOM, @UoM, @UOMConversionToBU, @RecoUOM, @RecoQty, @RecoUOMConversionToBU, @RecoQtyBU, 
                                      @ModelQtyBU, @Qty, @QtyBU, @VanQtyBU, @DeliveredQty, @MissedQty, @ReturnedQty, @TotalAmount,
                                      @TotalDiscount, @LineTaxAmount, @ProrataTaxAmount, @TotalTax, @NetAmount, @NetFakeAmount,
                                      @SKUPriceUID, @ProrataDiscountAmount, @LineDiscountAmount, @MRP, @CostUnitPrice, @ParentUID, 
                                      @IsPromotionApplied, @Volume, @VolumeUnit, @Weight, @StockType, @Remarks, 
                                      @TotalCashDiscount, @TotalExciseDuty, @SKUUID,@ApprovedQty,JSON_QUERY(@TaxData);";

            retValue = await ExecuteNonQueryAsync(salesOrderLineQuery, connection, transaction, salesOrderLines);

        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateSalesOrderLine(List<Model.Interfaces.ISalesOrderLine> salesOrderLines, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        int retValue = 0;
        try
        {
            if (salesOrderLines == null || salesOrderLines.Count == 0)
            {
                return retValue;
            }
            string salesOrderLineQuery = @"UPDATE sales_order_line SET 
                                                ss = 0, 
                                                modified_by = @ModifiedBy, 
                                                modified_time = @ModifiedTime, 
                                                server_modified_time = @ServerModifiedTime, 
                                                line_number = @LineNumber, 
                                                item_code = @ItemCode, 
                                                item_type = @ItemType, 
                                                base_price = @BasePrice, 
                                                unit_price = @UnitPrice, 
                                                fake_unit_price = @FakeUnitPrice, 
                                                base_uom = @BaseUOM, 
                                                uom = @UoM, 
                                                uom_conversion_to_bu = @UOMConversionToBU, 
                                                reco_uom = @RecoUOM, 
                                                reco_qty = @RecoQty, 
                                                reco_uom_conversion_to_bu = @RecoUOMConversionToBU, 
                                                reco_qty_bu = @RecoQtyBU, 
                                                model_qty_bu = @ModelQtyBU, 
                                                qty = @Qty, 
                                                qty_bu = @QtyBU, 
                                                van_qty_bu = @VanQtyBU, 
                                                delivered_qty = @DeliveredQty, 
                                                missed_qty = @MissedQty, 
                                                returned_qty = @ReturnedQty, 
                                                total_amount = @TotalAmount, 
                                                total_discount = @TotalDiscount, 
                                                line_tax_amount = @LineTaxAmount, 
                                                prorata_tax_amount = @ProrataTaxAmount, 
                                                total_tax = @TotalTax, 
                                                net_amount = @NetAmount, 
                                                net_fake_amount = @NetFakeAmount, 
                                                sku_price_uid = @SKUPriceUID, 
                                                prorata_discount_amount = @ProrataDiscountAmount, 
                                                line_discount_amount = @LineDiscountAmount, 
                                                mrp = @MRP, 
                                                cost_unit_price = @CostUnitPrice, 
                                                parent_uid = @ParentUID, 
                                                is_promotion_applied = @IsPromotionApplied, 
                                                volume = @Volume, 
                                                volume_unit = @VolumeUnit, 
                                                weight = @Weight, 
                                                stock_type = @StockType, 
                                                remarks = @Remarks, 
                                                total_cash_discount = @TotalCashDiscount, 
                                                total_excise_duty = @TotalExciseDuty, 
                                                approved_qty = @ApprovedQty, 
                                                tax_data = JSON_QUERY(@TaxData) 
                                                 WHERE uid = @UID";

            retValue = await ExecuteNonQueryAsync(salesOrderLineQuery, connection, transaction, salesOrderLines);
        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    #endregion
}
