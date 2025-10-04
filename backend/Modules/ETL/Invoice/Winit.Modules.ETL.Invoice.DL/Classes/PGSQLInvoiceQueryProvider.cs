using Winit.Modules.ETL.Invoice.DL.Interfaces;

namespace Winit.Modules.ETL.Invoice.DL.Classes
{
    public class PGSQLInvoiceQueryProvider : IInvoiceQueryProvider
    {
        public string GetInvoiceQuery() => @"
            SELECT i.*, il.*
            FROM stg_invoice i
            LEFT JOIN stg_invoice_line il ON i.invoice_uid = il.invoice_uid AND i.invoice_date = il.invoice_date
            WHERE i.invoice_uid = @InvoiceUid AND i.invoice_date = @InvoiceDate;";

        public string GetInvoicesByDateRangeQuery() => @"
            SELECT i.*, il.*
            FROM stg_invoice i
            LEFT JOIN stg_invoice_line il ON i.invoice_uid = il.invoice_uid AND i.invoice_date = il.invoice_date
            WHERE i.invoice_date BETWEEN @StartDate AND @EndDate;";

        public string GetUpdateInvoiceQuery() => @"
            UPDATE stg_invoice
            SET 
                invoice_number = @InvoiceNumber,
                invoice_date = @InvoiceDate,
                customer_id = @CustomerId,
                total_amount = @TotalAmount,
                status = @Status,
                updated_at = CURRENT_TIMESTAMP
            WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate;";

        public string GetInsertInvoiceQuery() => @"
            INSERT INTO stg_invoice (
                invoice_uid,
                invoice_number,
                invoice_date,
                customer_id,
                total_amount,
                status,
                created_at,
                updated_at
            )
            VALUES (
                @InvoiceUid,
                @InvoiceNumber,
                @InvoiceDate,
                @CustomerId,
                @TotalAmount,
                @Status,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            );";

        public string GetUpdateInvoiceLinesQuery() => @"
            UPDATE stg_invoice_line
            SET 
                product_id = @ProductId,
                quantity = @Quantity,
                unit_price = @UnitPrice,
                total_price = @TotalPrice,
                updated_at = CURRENT_TIMESTAMP
            WHERE invoice_line_uid = @InvoiceLineUid 
            AND invoice_uid = @InvoiceUid 
            AND invoice_date = @InvoiceDate;";

        public string GetInsertInvoiceLinesQuery() => @"
            INSERT INTO stg_invoice_line (
                invoice_line_uid,
                invoice_uid,
                invoice_date,
                product_id,
                quantity,
                unit_price,
                total_price,
                created_at,
                updated_at
            )
            VALUES (
                @InvoiceLineUid,
                @InvoiceUid,
                @InvoiceDate,
                @ProductId,
                @Quantity,
                @UnitPrice,
                @TotalPrice,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            );";

        public string GetDeleteInvoiceLinesQuery() => @"
            DELETE FROM stg_invoice_line
            WHERE invoice_line_uid = @InvoiceLineUid 
            AND invoice_uid = @InvoiceUid 
            AND invoice_date = @InvoiceDate;";

        public string GetInvoiceExistsQuery() => @"
            SELECT COUNT(1)
            FROM stg_invoice
            WHERE invoice_uid = @InvoiceUid 
            AND invoice_date = @InvoiceDate;";

        public string GetExistingInvoiceLinesQuery() => @"
            SELECT *
            FROM stg_invoice_line
            WHERE invoice_uid = @InvoiceUid 
            AND invoice_date = @InvoiceDate;";
    }
} 