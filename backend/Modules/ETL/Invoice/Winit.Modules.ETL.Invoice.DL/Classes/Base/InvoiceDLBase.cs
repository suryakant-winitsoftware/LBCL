using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.DL.Interfaces;
using Winit.Modules.ETL.Invoice.Model.Interfaces;
using Dapper;

namespace Winit.Modules.ETL.Invoice.DL.Classes.Base
{
    public abstract class InvoiceDLBase : IInvoiceDL
    {
        protected readonly ILogger _logger;

        protected InvoiceDLBase(ILogger logger)
        {
            _logger = logger;
        }

        // Read operations
        public abstract Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, IDbConnection sourceConnection);
        public abstract Task<IEnumerable<IStgInvoice>> GetInvoicesByDateRange(DateTime startDate, DateTime endDate, IDbConnection sourceConnection);

        // Write operations
        public abstract Task ProcessInvoiceWithLines(IStgInvoice invoice, IDbConnection destinationConnection);
        public abstract Task ProcessInvoices(IEnumerable<IStgInvoice> invoices, IDbConnection destinationConnection);

        // Common helper methods for source operations
        protected virtual async Task<IEnumerable<IStgInvoiceLine>> GetExistingInvoiceLinesFromSource(
            string invoiceUid, 
            DateTime invoiceDate, 
            IDbConnection sourceConnection, 
            IDbTransaction? transaction = null)
        {
            const string sql = "SELECT * FROM stg_invoice_line WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
            return await sourceConnection.QueryAsync<IStgInvoiceLine>(sql, new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate }, transaction);
        }

        // Common helper methods for destination operations
        protected virtual async Task<bool> InvoiceExistsInDestination(
            string invoiceUid, 
            DateTime invoiceDate, 
            IDbConnection destinationConnection, 
            IDbTransaction? transaction = null)
        {
            const string sql = "SELECT COUNT(*) FROM stg_invoice WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
            var count = await destinationConnection.ExecuteScalarAsync<int>(sql, new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate }, transaction);
            return count > 0;
        }

        protected virtual async Task<IEnumerable<IStgInvoiceLine>> GetExistingInvoiceLinesFromDestination(
            string invoiceUid, 
            DateTime invoiceDate, 
            IDbConnection destinationConnection, 
            IDbTransaction? transaction = null)
        {
            const string sql = "SELECT * FROM stg_invoice_line WHERE invoice_uid = @InvoiceUid AND invoice_date = @InvoiceDate";
            return await destinationConnection.QueryAsync<IStgInvoiceLine>(sql, new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate }, transaction);
        }

        protected virtual async Task ProcessInvoicesBatch(
            IEnumerable<IStgInvoice> invoices, 
            IDbConnection destinationConnection, 
            int batchSize = 100)
        {
            var invoiceList = invoices.ToList();
            for (int i = 0; i < invoiceList.Count; i += batchSize)
            {
                var batch = invoiceList.Skip(i).Take(batchSize);
                using var transaction = destinationConnection.BeginTransaction();
                try
                {
                    foreach (var invoice in batch)
                    {
                        await ProcessInvoiceWithLines(invoice, destinationConnection);
                    }
                    transaction.Commit();
                    _logger.LogInformation("Successfully processed batch of {Count} invoices", batch.Count());
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Error processing batch of invoices");
                    throw;
                }
            }
        }
    }
} 