using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Winit.Modules.PurchaseOrder.DL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.DL.Classes;

public class PGSQLPurchaseOrderLineDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPurchaseOrderLineDL
{
    private readonly IPurchaseOrderLineProvisionDL _purchaseOrderLineProvisionDL;
    private readonly ILogger<MSSQLPurchaseOrderLineDL> _logger;
    public Winit.Modules.Scheme.DL.Interfaces.IWalletDL _walletUpdaterDL;
    public PGSQLPurchaseOrderLineDL(IServiceProvider serviceProvider, IConfiguration config,
        IPurchaseOrderLineProvisionDL purchaseOrderLineProvisionDL,
        ILogger<MSSQLPurchaseOrderLineDL> logger) : base(serviceProvider, config)
    {
        _purchaseOrderLineProvisionDL = purchaseOrderLineProvisionDL;
        _logger = logger;
    }

    public async Task<PagedResponse<IPurchaseOrderLine>> GetAllPurchaseOrderLines(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(
            """
            SELECT  * FROM
            (SELECT 
            uid AS Uid,
            ss AS Ss,
            created_by AS CreatedBy,
            created_time AS CreatedTime,
            modified_by AS ModifiedBy,
            modified_time AS ModifiedTime,
            server_add_time AS ServerAddTime,
            server_modified_time AS ServerModifiedTime,
            purchase_order_header_uid AS PurchaseOrderHeaderUid,
            line_number AS LineNumber,
            sku_uid AS SkuUid,
            sku_code AS SkuCode,
            sku_type AS SkuType,
            uom AS Uom,
            base_uom AS BaseUom,
            uom_conversion_to_bu AS UomConversionToBu,
            available_qty AS AvailableQty,
            model_qty AS ModelQty,
            in_transit_qty AS InTransitQty,
            suggested_qty AS SuggestedQty,
            past_3_month_avg AS Past3MonthAvg,
            requested_qty AS RequestedQty,
            final_qty AS FinalQty,
            final_qty_bu AS FinalQtyBu,
            unit_price AS UnitPrice,
            base_price AS BasePrice,
            total_amount AS TotalAmount,
            total_discount AS TotalDiscount,
            line_discount AS LineDiscount,
            header_discount AS HeaderDiscount,
            total_tax_amount AS TotalTaxAmount,
            line_tax_amount AS LineTaxAmount,
            header_tax_amount AS HeaderTaxAmount,
            net_amount AS NetAmount,
            tax_data AS TaxData,
            app1_qty AS App1Qty,
            app2_qty AS App2Qty,
            app3_qty AS App3Qty,
            app4_qty AS App4Qty,
            app5_qty AS App5Qty,
            app6_qty AS App6Qty,
            mrp AS Mrp,
            dp_price AS DpPrice,
            laddering_percentage AS LadderingPercentage,
            laddering_discount AS LadderingDiscount,
            sell_in_discount_unit_value AS SellInDiscountUnitValue,
            sell_in_discount_unit_percentage AS SellInDiscountUnitPercentage,
            sell_in_discount_total_value AS SellInDiscountTotalValue,
            sell_in_cn_p1_unit_percentage AS SellInCnP1UnitPercentage,
            sell_in_cn_p1_unit_value AS SellInCnP1UnitValue,
            sell_in_cn_p1_value AS SellInCnP1Value,
            cash_discount_percentage AS CashDiscountPercentage,
            cash_discount_value AS CashDiscountValue,
            sell_in_p2_amount AS SellInP2Amount,
            sell_in_p3_amount AS SellInP3Amount,
            p3_standing_amount AS P3StandingAmount,
            promotion_uid as promotionuid,
            effective_unit_price,
            effective_unit_tax,
            cancelled_qty AS CancelledQty,
            billed_qty AS BilledQty,
            is_updated_from_erp as IsUpdatedFromErp,
            sell_in_scheme_code  AS SellInSchemeCode,
            standing_scheme_data As StandingSchemeData,
            qps_scheme_code As QPSSchemeCode,
            p2_qps_total_value As P2QPSTotalValue,
            p3_qps_total_value As P3QPSTotalValue,
            margin_unit_value As MarginUnitValue
            FROM purchase_order_line)
            AS  purchaseorderline
            """);

            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(
                """
                SELECT  * FROM
                (SELECT uid AS Uid, 
                ss AS Ss, 
                created_by AS CreatedBy, 
                created_time AS CreatedTime, 
                modified_by AS ModifiedBy, 
                modified_time AS ModifiedTime, 
                server_add_time AS ServerAddTime, 
                server_modified_time AS ServerModifiedTime, 
                purchase_order_header_uid AS PurchaseOrderHeaderUid,
                line_number AS LineNumber, 
                sku_uid AS SkuUid, 
                sku_code AS SkuCode, 
                sku_type AS SkuType, 
                uom AS Uom, 
                base_uom AS BaseUom, 
                uom_conversion_to_bu AS UomConversionToBu, 
                available_qty AS AvailableQty, 
                model_qty AS ModelQty, 
                in_transit_qty AS InTransitQty, 
                suggested_qty AS SuggestedQty, 
                past_3_month_avg AS Past3MonthAvg, 
                requested_qty AS RequestedQty, 
                final_qty AS FinalQty, 
                final_qty_bu AS FinalQtyBu, 
                unit_price AS UnitPrice, 
                base_price AS BasePrice, 
                total_amount AS TotalAmount, 
                total_discount AS TotalDiscount, 
                line_discount AS LineDiscount, 
                header_discount AS HeaderDiscount, 
                total_tax_amount AS TotalTaxAmount, 
                line_tax_amount AS LineTaxAmount, 
                header_tax_amount AS HeaderTaxAmount, 
                net_amount AS NetAmount, 
                tax_data AS TaxData, 
                app1_qty AS App1Qty, 
                app2_qty AS App2Qty, 
                app3_qty AS App3Qty, 
                app4_qty AS App4Qty, 
                app5_qty AS App5Qty, 
                app6_qty AS App6Qty ,
                mrp AS Mrp,
                dp_price AS DpPrice,
                laddering_percentage AS LadderingPercentage,
                laddering_discount AS LadderingDiscount,
                sell_in_discount_unit_value AS SellInDiscountUnitValue,
                sell_in_discount_unit_percentage AS SellInDiscountUnitPercentage,
                sell_in_discount_total_value AS SellInDiscountTotalValue,
                sell_in_cn_p1_unit_percentage AS SellInCnP1UnitPercentage,
                sell_in_cn_p1_unit_value AS SellInCnP1UnitValue,
                sell_in_cn_p1_value AS SellInCnP1Value,
                cash_discount_percentage AS CashDiscountPercentage,
                cash_discount_value AS CashDiscountValue,
                sell_in_p2_amount AS SellInP2Amount,
                sell_in_p3_amount AS SellInP3Amount,
                p3_standing_amount AS P3StandingAmount,
                promotion_uid as promotionuid,
                effective_unit_price,
                effective_unit_tax,
                cancelled_qty AS CancelledQty,
                billed_qty AS BilledQty,
                is_updated_from_erp as IsUpdatedFromErp,
                sell_in_scheme_code  AS SellInSchemeCode,
                standing_scheme_data As StandingSchemeData,
                qps_scheme_code As QPSSchemeCode,
                p2_qps_total_value As P2QPSTotalValue,
                p3_qps_total_value As P3QPSTotalValue,
                margin_unit_value As MarginUnitValue 
                FROM purchase_order_line)
                AS  purchaseorderline
                                        
                """);
            }
            Dictionary<string, object?> parameters = [];
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sql.Append(" WHERE ");
                AppendFilterCriteria<IPurchaseOrderLine>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(" WHERE ");
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
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<IPurchaseOrderLine> purchaseOrderLines = await ExecuteQueryAsync<IPurchaseOrderLine>(sql.ToString()
            , parameters);
            // Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<IPurchaseOrderLine> pagedResponse = new()
            {
                PagedData = purchaseOrderLines, TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreatePurchaseOrderLines(List<IPurchaseOrderLine> purchaseOrderLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            // Clean up the data - convert "string" placeholder values to null
            foreach (var line in purchaseOrderLines)
            {
                if (line.PurchaseOrderHeaderUID == "string") line.PurchaseOrderHeaderUID = null;
                if (line.SKUUID == "string") line.SKUUID = null;
                if (line.PromotionUID == "string") line.PromotionUID = null;
                if (line.TaxData == "string") line.TaxData = null;
                if (line.StandingSchemeData == "string") line.StandingSchemeData = null;

                // Fix invalid CreatedBy and ModifiedBy values - set to NULL and use SQL fallback
                if (line.CreatedBy == "string" || string.IsNullOrWhiteSpace(line.CreatedBy))
                    line.CreatedBy = null;
                if (line.ModifiedBy == "string" || string.IsNullOrWhiteSpace(line.ModifiedBy))
                    line.ModifiedBy = null;

                // Ensure timestamps are set
                if (line.CreatedTime == default) line.CreatedTime = DateTime.Now;
                if (line.ModifiedTime == default) line.ModifiedTime = DateTime.Now;
                if (line.ServerAddTime == default) line.ServerAddTime = DateTime.Now;
                if (line.ServerModifiedTime == default) line.ServerModifiedTime = DateTime.Now;
            }

            string sql = """
                         INSERT INTO purchase_order_line (
                             uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                             purchase_order_header_uid, line_number, sku_uid, sku_code, sku_type, uom, base_uom, uom_conversion_to_bu,
                             available_qty, model_qty, in_transit_qty, suggested_qty, past_3_month_avg, requested_qty, final_qty, final_qty_bu,
                             unit_price, base_price, total_amount, total_discount, line_discount, header_discount, total_tax_amount,
                             line_tax_amount, header_tax_amount, net_amount, tax_data, app1_qty, app2_qty, app3_qty, app4_qty, app5_qty,
                             app6_qty, mrp, dp_price, laddering_percentage, laddering_discount, sell_in_discount_unit_value,
                             sell_in_discount_unit_percentage, sell_in_discount_total_value, sell_in_cn_p1_unit_percentage,
                             sell_in_cn_p1_unit_value, sell_in_cn_p1_value, cash_discount_percentage, cash_discount_value,
                             sell_in_p2_amount, sell_in_p3_amount, p3_standing_amount, promotion_uid, effective_unit_price,
                             effective_unit_tax ,sell_in_scheme_code  ,standing_scheme_data ,qps_scheme_code ,p2_qps_total_value ,
                             p3_qps_total_value ,margin_unit_value
                         ) VALUES (
                             @UID, @SS,
                             CASE WHEN @CreatedBy IS NULL OR @CreatedBy = '' THEN (SELECT uid FROM emp LIMIT 1) ELSE @CreatedBy END,
                             @CreatedTime,
                             CASE WHEN @ModifiedBy IS NULL OR @ModifiedBy = '' THEN (SELECT uid FROM emp LIMIT 1) ELSE @ModifiedBy END,
                             @ModifiedTime, @ServerAddTime, @ServerModifiedTime,    
                             @PurchaseOrderHeaderUID, @LineNumber, @SKUUID, @SKUCode, @SKUType, @UOM, @BaseUOM, @UOMConversionToBU,
                             @AvailableQty, @ModelQty, @InTransitQty, @SuggestedQty, @Past3MonthAvg, @RequestedQty, @FinalQty, @FinalQtyBU,
                             @UnitPrice, @BasePrice, @TotalAmount, @TotalDiscount, @LineDiscount, @HeaderDiscount, @TotalTaxAmount,
                             @LineTaxAmount, @HeaderTaxAmount, @NetAmount, @TaxData::json, @App1Qty, @App2Qty, @App3Qty, @App4Qty, @App5Qty, 
                             @App6Qty, @Mrp, @DpPrice, @LadderingPercentage, @LadderingDiscount, @SellInDiscountUnitValue,
                             @SellInDiscountUnitPercentage, @SellInDiscountTotalValue, @SellInCnP1UnitPercentage,
                             @SellInCnP1UnitValue, @SellInCnP1Value, @CashDiscountPercentage, @CashDiscountValue,
                             @SellInP2Amount, @SellInP3Amount, @P3StandingAmount, @PromotionUID, @EffectiveUnitPrice, @EffectiveUnitTax,
                             @SellInSchemeCode, @StandingSchemeData::json, @QPSSchemeCode, @P2QPSTotalValue, @P3QPSTotalValue, @MarginUnitValue
                          );
                         """
                ;
            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderLines);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdatePurchaseOrderLines(List<IPurchaseOrderLine> purchaseOrderLines, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            // Fix invalid CreatedBy and ModifiedBy values
            foreach (var line in purchaseOrderLines)
            {
                if (line.CreatedBy == "string" || string.IsNullOrWhiteSpace(line.CreatedBy))
                    line.CreatedBy = null;
                if (line.ModifiedBy == "string" || string.IsNullOrWhiteSpace(line.ModifiedBy))
                    line.ModifiedBy = null;
            }
            string sql = """
                         UPDATE purchase_order_line SET
                             ss = @SS,
                             created_by = @CreatedBy,
                             created_time = @CreatedTime,
                             modified_by = @ModifiedBy,
                             modified_time = @ModifiedTime,
                             server_add_time = @ServerAddTime,
                             server_modified_time = @ServerModifiedTime,
                             purchase_order_header_uid = @PurchaseOrderHeaderUID,
                             line_number = @LineNumber,
                             sku_uid = @SKUUID,
                             sku_code = @SKUCode,
                             sku_type = @SKUType,
                             uom = @UOM,
                             base_uom = @BaseUOM,
                             uom_conversion_to_bu = @UOMConversionToBU,
                             available_qty = @AvailableQty,
                             model_qty = @ModelQty,
                             in_transit_qty = @InTransitQty,
                             suggested_qty = @SuggestedQty,
                             past_3_month_avg = @Past3MonthAvg,
                             requested_qty = @RequestedQty,
                             final_qty = @FinalQty,
                             final_qty_bu = @FinalQtyBU,
                             unit_price = @UnitPrice,
                             base_price = @BasePrice,
                             total_amount = @TotalAmount,
                             total_discount = @TotalDiscount,
                             line_discount = @LineDiscount,
                             header_discount = @HeaderDiscount,
                             total_tax_amount = @TotalTaxAmount,
                             line_tax_amount = @LineTaxAmount,
                             header_tax_amount = @HeaderTaxAmount,
                             net_amount = @NetAmount,
                             tax_data = @TaxData::json,
                             app1_qty = @App1Qty,
                             app2_qty = @App2Qty,
                             app3_qty = @App3Qty,
                             app4_qty = @App4Qty,
                             app5_qty = @App5Qty,
                             app6_qty = @App6Qty,
                             mrp = @Mrp,
                             dp_price = @DpPrice,
                             laddering_percentage = @LadderingPercentage,
                             laddering_discount = @LadderingDiscount,
                             sell_in_discount_unit_value = @SellInDiscountUnitValue,
                             sell_in_discount_unit_percentage = @SellInDiscountUnitPercentage,
                             sell_in_discount_total_value = @SellInDiscountTotalValue,
                             sell_in_cn_p1_unit_percentage = @SellInCnP1UnitPercentage,
                             sell_in_cn_p1_unit_value = @SellInCnP1UnitValue,
                             sell_in_cn_p1_value = @SellInCnP1Value,
                             cash_discount_percentage = @CashDiscountPercentage,
                             cash_discount_value = @CashDiscountValue,
                             sell_in_p2_amount = @SellInP2Amount,
                             sell_in_p3_amount = @SellInP3Amount,
                             p3_standing_amount = @P3StandingAmount,
                             promotion_uid = @PromotionUID,
                             effective_unit_price = @EffectiveUnitPrice,
                             effective_unit_tax = @EffectiveUnitTax,
                             sell_in_scheme_code  = @SellInSchemeCode,
                             standing_scheme_data = @StandingSchemeData::json,
                             qps_scheme_code = @QPSSchemeCode,
                             p2_qps_total_value = @P2QPSTotalValue,
                             p3_qps_total_value = @P3QPSTotalValue,
                             margin_unit_value = @MarginUnitValue
                         WHERE
                             uid = @UID;
                         """
                ;

            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, purchaseOrderLines);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> DeletePurchaseOrderLinesByUIDs(List<string> purchaseOrderLineUiDs, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            await _purchaseOrderLineProvisionDL.DeletePurchaseOrderLineProvisionsByPurchaseOrderLineUids(purchaseOrderLineUiDs, dbConnection, dbTransaction);
            string sql = "DELETE FROM purchase_order_line where uid in @PurchaseOrderUIDs";
            var parameters = new
            {
                PurchaseOrderUIDs = purchaseOrderLineUiDs
            };
            _ = await _purchaseOrderLineProvisionDL.DeletePurchaseOrderLineProvisionsByPurchaseOrderLineUids(purchaseOrderLineUiDs);
            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    public async Task<int> DeletePurchaseOrderLinesByPurchaseOrderHeaderUID(string purchaseOrderHeaderUID, IDbConnection? dbConnection = null,
        IDbTransaction? dbTransaction = null)
    {
        try
        {
            string sql = "DELETE FROM purchase_order_line where purchase_order_header_uid = @PurchaseOrderHeaderUID";
            var parameters = new
            {
                PurchaseOrderHeaderUID = purchaseOrderHeaderUID
            };
            return await ExecuteNonQueryAsync(sql, dbConnection, dbTransaction, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<int> UpdateWallet(List<IPurchaseOrderLine> purchaseOrderLines, string orgUid, string branchUId, string houid
        , IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? iWalletLedgers = ConvertToWalletLedger(purchaseOrderLines, orgUid);
            if (iWalletLedgers == null || iWalletLedgers.Count == 0)
            {
                return -1;
            }
            return await _walletUpdaterDL.UpdateWalletAsync(iWalletLedgers, connection, transaction);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public virtual List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? ConvertToWalletLedger(List<IPurchaseOrderLine> purchaseOrderLine, string OrgUid)
    {
        if (purchaseOrderLine == null || purchaseOrderLine.Count == 0)
        {
            return null;
        }
        var objpurchaseOrderLine = purchaseOrderLine
            .GroupBy(x => x.PurchaseOrderHeaderUID)
            .Select(g => new
            {
                PurchaseOrderHeaderUID = g.Key,
                SellInCnP1Value = g.Sum(x => x.SellInCnP1Value),
                SellInP2Amount = g.Sum(x => x.SellInP2Amount),
                SellInP3Amount = g.Sum(x => x.SellInP3Amount),
                P3StandingAmount = g.Sum(x => x.P3StandingAmount)
            })
            .ToList().FirstOrDefault();


        List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>? WalletLedgerList = null;
        WalletLedgerList = new List<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>();

        string sourceType;
        string sourceUid;
        string type;
        decimal amount;
        string CreatedBy;
        string CreditType;
        sourceType = "PurchaseInvoice";
        if (objpurchaseOrderLine != null)
        {
            sourceUid = objpurchaseOrderLine.PurchaseOrderHeaderUID;
            type = "CNP1";
            amount = objpurchaseOrderLine.SellInCnP1Value;
            CreatedBy = "";
            CreditType = "Debit";
            Winit.Modules.Scheme.Model.Interfaces.IWalletLedger? walletLedger = ConvertToWHStockLedger(OrgUid, sourceType, sourceUid
            , type, amount, CreatedBy, CreditType);
            if (walletLedger != null)
            {
                WalletLedgerList.Add(walletLedger);
            }

            type = "CNP2";
            amount = objpurchaseOrderLine.SellInP2Amount;
            CreatedBy = "";
            CreditType = "Debit";
            walletLedger = ConvertToWHStockLedger(OrgUid, sourceType, sourceUid
            , type, amount, CreatedBy, CreditType);
            if (walletLedger != null)
            {
                WalletLedgerList.Add(walletLedger);
            }

            type = "CNP3";
            amount = objpurchaseOrderLine.SellInP3Amount;
            CreatedBy = "";
            CreditType = "Debit";
            walletLedger = ConvertToWHStockLedger(OrgUid, sourceType, sourceUid
            , type, amount, CreatedBy, CreditType);
            if (walletLedger != null)
            {
                WalletLedgerList.Add(walletLedger);
            }

            type = "SOP";
            amount = objpurchaseOrderLine.P3StandingAmount;
            CreatedBy = "";
            CreditType = "Debit";
            walletLedger = ConvertToWHStockLedger(OrgUid, sourceType, sourceUid
            , type, amount, CreatedBy, CreditType);
            if (walletLedger != null)
            {
                WalletLedgerList.Add(walletLedger);
            }
        }

        return WalletLedgerList;
    }
    public virtual Winit.Modules.Scheme.Model.Interfaces.IWalletLedger? ConvertToWHStockLedger(string OrgUid, string sourceType, string sourceUid
        , string type, decimal amount, string CreatedBy, string CreditType)
    {
        string warehouseUID = string.Empty;
        Winit.Modules.Scheme.Model.Interfaces.IWalletLedger walletLedger = _serviceProvider.GetRequiredService<Winit.Modules.Scheme.Model.Interfaces.IWalletLedger>();
        if (OrgUid == "")
        {
            return walletLedger;
        }
        walletLedger.Id = 0;
        walletLedger.UID = Guid.NewGuid().ToString();
        walletLedger.SS = 0;
        walletLedger.CreatedBy = CreatedBy;
        walletLedger.CreatedTime = DateTime.Now;
        walletLedger.ModifiedBy = CreatedBy;
        walletLedger.ModifiedTime = DateTime.Now;
        walletLedger.ServerAddTime = DateTime.Now;
        walletLedger.ServerModifiedTime = DateTime.Now;
        walletLedger.OrgUid = OrgUid;
        walletLedger.TransactionDateTime = DateTime.Now;
        walletLedger.Type = type;
        walletLedger.Amount = amount;
        walletLedger.CreditType = CreditType;
        walletLedger.SourceType = sourceType;
        walletLedger.SourceUid = sourceUid;

        return walletLedger;
    }



}
