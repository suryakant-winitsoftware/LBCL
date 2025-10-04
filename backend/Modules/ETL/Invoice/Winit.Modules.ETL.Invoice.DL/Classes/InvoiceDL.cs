using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.DL.Interfaces;
using Winit.Modules.ETL.Invoice.Model.Classes;
using Winit.Modules.ETL.Invoice.Model.Interfaces;
using Dapper;

namespace Winit.Modules.ETL.Invoice.DL.Classes
{
    public class InvoiceDL : IInvoiceDL
    {
        private readonly ILogger<InvoiceDL> _logger;

        public InvoiceDL(ILogger<InvoiceDL> logger)
        {
            _logger = logger;
        }

        public async Task ProcessInvoiceWithLines(IStgInvoice invoice, IDbConnection connection)
        {
            using var transaction = connection.BeginTransaction();
            try
            {
                // Check if the invoice exists
                const string invoiceExistsSql = "SELECT COUNT(*) FROM stg_invoice WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
                var invoiceExists = await connection.ExecuteScalarAsync<int>(invoiceExistsSql, new { invoice.InvoiceUid, invoice.InvoiceDate }, transaction) > 0;

                if (invoiceExists)
                {
                    // Update the invoice
                    const string invoiceUpdateSql = @"
                        UPDATE stg_invoice SET
                            invoice_number = @InvoiceNumber,
                            proforma_invoice_number = @ProformaInvoiceNumber,
                            dms_purchase_order_number = @DmsPurchaseOrderNumber,
                            erp_order_number = @ErpOrderNumber,
                            warehouse_uid = @WarehouseUid,
                            warehouse_code = @WarehouseCode,
                            warehouse_name = @WarehouseName,
                            org_uid = @OrgUid,
                            org_code = @OrgCode,
                            org_name = @OrgName,
                            org_unit_uid = @OrgUnitUid,
                            org_unit_code = @OrgUnitCode,
                            org_unit_name = @OrgUnitName,
                            store_uid = @StoreUid,
                            store_code = @StoreCode,
                            store_name = @StoreName,
                            branch_uid = @BranchUid,
                            branch_code = @BranchCode,
                            branch_name = @BranchName,
                            asm_emp_uid = @AsmEmpUid,
                            asm_emp_code = @AsmEmpCode,
                            asm_emp_name = @AsmEmpName,
                            dms_order_date = @DmsOrderDate,
                            dms_expected_delivery_date = @DmsExpectedDeliveryDate,
                            delivered_date_time = @DeliveredDateTime,
                            customer_po = @CustomerPo,
                            ar_number = @ArNumber,
                            shipping_address_code = @ShippingAddressCode,
                            billing_address_code = @BillingAddressCode,
                            total_amount = @TotalAmount,
                            total_discount = @TotalDiscount,
                            total_tax = @TotalTax,
                            net_amount = @NetAmount,
                            line_count = @LineCount,
                            qty_count = @QtyCount,
                            fiscal_year = @FiscalYear,
                            period_year = @PeriodYear,
                            period_num = @PeriodNum,
                            quarter_num = @QuarterNum,
                            modified_time = @ModifiedTime
                        WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate;";

                    await connection.ExecuteAsync(invoiceUpdateSql, invoice, transaction);
                }
                else
                {
                    // Insert the invoice
                    const string invoiceInsertSql = @"
                        INSERT INTO stg_invoice (
                            invoice_uid, invoice_number, proforma_invoice_number, dms_purchase_order_number,
                            erp_order_number, warehouse_uid, warehouse_code, warehouse_name,
                            org_uid, org_code, org_name, org_unit_uid, org_unit_code,
                            org_unit_name, store_uid, store_code, store_name, branch_uid,
                            branch_code, branch_name, asm_emp_uid, asm_emp_code, asm_emp_name,
                            dms_order_date, dms_expected_delivery_date, invoice_date,
                            delivered_date_time, customer_po, ar_number, shipping_address_code,
                            billing_address_code, total_amount, total_discount, total_tax,
                            net_amount, line_count, qty_count, fiscal_year, period_year,
                            period_num, quarter_num, created_time, modified_time
                        )
                        VALUES (
                            @InvoiceUid, @InvoiceNumber, @ProformaInvoiceNumber, @DmsPurchaseOrderNumber,
                            @ErpOrderNumber, @WarehouseUid, @WarehouseCode, @WarehouseName,
                            @OrgUid, @OrgCode, @OrgName, @OrgUnitUid, @OrgUnitCode,
                            @OrgUnitName, @StoreUid, @StoreCode, @StoreName, @BranchUid,
                            @BranchCode, @BranchName, @AsmEmpUid, @AsmEmpCode, @AsmEmpName,
                            @DmsOrderDate, @DmsExpectedDeliveryDate, @InvoiceDate,
                            @DeliveredDateTime, @CustomerPo, @ArNumber, @ShippingAddressCode,
                            @BillingAddressCode, @TotalAmount, @TotalDiscount, @TotalTax,
                            @NetAmount, @LineCount, @QtyCount, @FiscalYear, @PeriodYear,
                            @PeriodNum, @QuarterNum, @CreatedTime, @ModifiedTime
                        );";

                    await connection.ExecuteAsync(invoiceInsertSql, invoice, transaction);
                }

                // Process invoice lines (insert/update/delete)
                var existingLines = await connection.QueryAsync<StgInvoiceLine>("SELECT * FROM stg_invoice_line WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate", new { invoice.InvoiceUid, invoice.InvoiceDate }, transaction);
                var linesToInsert = new List<IStgInvoiceLine>();
                var linesToUpdate = new List<IStgInvoiceLine>();
                var linesToDelete = new List<IStgInvoiceLine>();

                foreach (var line in invoice.InvoiceLines)
                {
                    var existingLine = existingLines.FirstOrDefault(l => l.InvoiceLineUid == line.InvoiceLineUid);
                    if (existingLine != null)
                    {
                        // Update existing line
                        linesToUpdate.Add(line);
                    }
                    else
                    {
                        // Insert new line
                        linesToInsert.Add(line);
                    }
                }

                // Determine lines to delete
                foreach (var existingLine in existingLines)
                {
                    if (!invoice.InvoiceLines.Any(l => l.InvoiceLineUid == existingLine.InvoiceLineUid))
                    {
                        linesToDelete.Add(existingLine);
                    }
                }

                // Perform line updates
                if (linesToUpdate.Count > 0)
                {
                    const string lineUpdateSql = @"
                        UPDATE stg_invoice_line SET
                            invoice_number = @InvoiceNumber,
                            proforma_invoice_number = @ProformaInvoiceNumber,
                            dms_purchase_order_number = @DmsPurchaseOrderNumber,
                            erp_order_number = @ErpOrderNumber,
                            warehouse_uid = @WarehouseUid,
                            warehouse_code = @WarehouseCode,
                            warehouse_name = @WarehouseName,
                            org_uid = @OrgUid,
                            org_code = @OrgCode,
                            org_name = @OrgName,
                            org_unit_uid = @OrgUnitUid,
                            org_unit_code = @OrgUnitCode,
                            org_unit_name = @OrgUnitName,
                            store_uid = @StoreUid,
                            store_code = @StoreCode,
                            store_name = @StoreName,
                            branch_uid = @BranchUid,
                            branch_code = @BranchCode,
                            branch_name = @BranchName,
                            asm_emp_uid = @AsmEmpUid,
                            asm_emp_code = @AsmEmpCode,
                            asm_emp_name = @AsmEmpName,
                            dms_order_date = @DmsOrderDate,
                            dms_expected_delivery_date = @DmsExpectedDeliveryDate,
                            delivered_date_time = @DeliveredDateTime,
                            customer_po = @CustomerPo,
                            ar_number = @ArNumber,
                            fiscal_year = @FiscalYear,
                            period_year = @PeriodYear,
                            period_num = @PeriodNum,
                            quarter_num = @QuarterNum,
                            line_number = @LineNumber,
                            sku_uid = @SkuUid,
                            sku_code = @SkuCode,
                            sku_name = @SkuName,
                            sku_type = @SkuType,
                            unit_price = @UnitPrice,
                            qty = @Qty,
                            uom = @Uom,
                            base_uom = @BaseUom,
                            uom_multiplier = @UomMultiplier,
                            total_amount = @TotalAmount,
                            total_discount = @TotalDiscount,
                            total_tax = @TotalTax,
                            net_amount = @NetAmount,
                            volume = @Volume,
                            volume_unit = @VolumeUnit,
                            weight = @Weight,
                            weight_unit = @WeightUnit,
                            modified_time = @ModifiedTime,
                            attribute1_code = @Attribute1Code,
                            attribute1_name = @Attribute1Name,
                            attribute2_code = @Attribute2Code,
                            attribute2_name = @Attribute2Name,
                            attribute3_code = @Attribute3Code,
                            attribute3_name = @Attribute3Name,
                            attribute4_code = @Attribute4Code,
                            attribute4_name = @Attribute4Name,
                            attribute5_code = @Attribute5Code,
                            attribute5_name = @Attribute5Name,
                            attribute6_code = @Attribute6Code,
                            attribute6_name = @Attribute6Name,
                            attribute7_code = @Attribute7Code,
                            attribute7_name = @Attribute7Name
                        WHERE invoice_line_uid = @InvoiceLineUid AND invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
                    await connection.ExecuteAsync(lineUpdateSql, linesToUpdate, transaction);
                }

                // Perform line inserts
                if (linesToInsert.Count > 0)
                {
                    const string lineInsertSql = @"
                         INSERT INTO stg_invoice_line (
                            id, invoice_line_uid, invoice_uid, invoice_number, proforma_invoice_number,
                            dms_purchase_order_number, erp_order_number, warehouse_uid, warehouse_code,
                            warehouse_name, org_uid, org_code, org_name, org_unit_uid, org_unit_code,
                            org_unit_name, store_uid, store_code, store_name, branch_uid, branch_code,
                            branch_name, asm_emp_uid, asm_emp_code, asm_emp_name, dms_order_date,
                            dms_expected_delivery_date, invoice_date, delivered_date_time, customer_po,
                            ar_number, fiscal_year, period_year, period_num, quarter_num, line_number,
                            sku_uid, sku_code, sku_name, sku_type, unit_price, qty, uom, base_uom,
                            uom_multiplier, total_amount, total_discount, total_tax, net_amount, volume,
                            volume_unit, weight, weight_unit, created_time, modified_time, attribute1_code,
                            attribute1_name, attribute2_code, attribute2_name, attribute3_code, attribute3_name,
                            attribute4_code, attribute4_name, attribute5_code, attribute5_name, attribute6_code,
                            attribute6_name, attribute7_code, attribute7_name
                        )
                        VALUES (
                            @Id, @InvoiceLineUid, @InvoiceUid, @InvoiceNumber, @ProformaInvoiceNumber,
                            @DmsPurchaseOrderNumber, @ErpOrderNumber, @WarehouseUid, @WarehouseCode,
                            @WarehouseName, @OrgUid, @OrgCode, @OrgName, @OrgUnitUid, @OrgUnitCode,
                            @OrgUnitName, @StoreUid, @StoreCode, @StoreName, @BranchUid, @BranchCode,
                            @BranchName, @AsmEmpUid, @AsmEmpCode, @AsmEmpName, @DmsOrderDate,
                            @DmsExpectedDeliveryDate, @InvoiceDate, @DeliveredDateTime, @CustomerPo,
                            @ArNumber, @FiscalYear, @PeriodYear, @PeriodNum, @QuarterNum, @LineNumber,
                            @SkuUid, @SkuCode, @SkuName, @SkuType, @UnitPrice, @Qty, @Uom, @BaseUom,
                            @UomMultiplier, @TotalAmount, @TotalDiscount, @TotalTax, @NetAmount, @Volume,
                            @VolumeUnit, @Weight, @WeightUnit, @CreatedTime, @ModifiedTime, @Attribute1Code,
                            @Attribute1Name, @Attribute2Code, @Attribute2Name, @Attribute3Code, @Attribute3Name,
                            @Attribute4Code, @Attribute4Name, @Attribute5Code, @Attribute5Name, @Attribute6Code,
                            @Attribute6Name, @Attribute7Code, @Attribute7Name
                        );
                    ";
                    await connection.ExecuteAsync(lineInsertSql, linesToInsert, transaction);
                }

                // Perform line deletes
                if (linesToDelete.Count > 0)
                {
                    const string lineDeleteSql = "DELETE FROM stg_invoice_line WHERE invoice_line_uid = @InvoiceLineUid AND invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
                    await connection.ExecuteAsync(lineDeleteSql, linesToDelete, transaction);
                }

                transaction.Commit();
                _logger.LogInformation("Transaction committed.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Transaction rolled back. Error processing invoice and lines.");
                throw;
            }
        }

        public async Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, IDbConnection connection)
        {
            const string sql = @"
                SELECT
                    i.*,
                    il.id as LineId, il.invoice_line_uid, il.invoice_number, il.proforma_invoice_number,
                    il.dms_purchase_order_number, il.erp_order_number, il.warehouse_uid,
                    il.warehouse_code, il.warehouse_name, il.org_uid, il.org_code, il.org_name,
                    il.org_unit_uid, il.org_unit_code, il.org_unit_name, il.store_uid, il.store_code,
                    il.store_name, il.branch_uid, il.branch_code, il.branch_name, il.asm_emp_uid,
                    il.asm_emp_code, il.asm_emp_name, il.dms_order_date, il.dms_expected_delivery_date,
                    il.invoice_date, il.delivered_date_time, il.customer_po, il.ar_number, il.fiscal_year,
                    il.period_year, il.period_num, il.quarter_num, il.line_number, il.sku_uid,
                    il.sku_code, il.sku_name, il.sku_type, il.unit_price, il.qty, il.uom, il.base_uom,
                    il.uom_multiplier, il.total_amount, il.total_discount, il.total_tax, il.net_amount,
                    il.volume, il.volume_unit, il.weight, il.weight_unit, il.created_time, il.modified_time,
                    il.attribute1_code, il.attribute1_name, il.attribute2_code, il.attribute2_name,
                    il.attribute3_code, il.attribute3_name, il.attribute4_code, il.attribute4_name,
                    il.attribute5_code, il.attribute5_name, il.attribute6_code, il.attribute6_name,
                    il.attribute7_code, il.attribute7_name
                FROM stg_invoice i
                LEFT JOIN stg_invoice_line il ON i.invoice_uid = il.invoice_uid AND i.invoice_date = il.invoice_date
                WHERE i.invoice_uid = @InvoiceUid AND i.invoice_date = @InvoiceDate;";

            StgInvoice? invoice = null;
            var invoiceDictionary = new Dictionary<string, StgInvoice>();

            await connection.QueryAsync<StgInvoice, StgInvoiceLine, StgInvoice>(
                sql,
                (i, il) =>
                {
                    if (!invoiceDictionary.TryGetValue(i.InvoiceUid, out invoice))
                    {
                        invoice = i;
                        invoice.InvoiceLines = new List<IStgInvoiceLine>(); // Use the interface
                        invoiceDictionary.Add(i.InvoiceUid, invoice);
                    }

                    if (il != null)
                    {
                        invoice.InvoiceLines.Add(il);
                    }
                    return invoice;
                },
                param: new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate },
                splitOn: "LineId"
            );
            return invoiceDictionary.Values.FirstOrDefault();
        }
    }
}
