using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.Model.Interfaces;
using Winit.Modules.ETL.Invoice.BL.Interfaces;
using Microsoft.Extensions.Logging;
using Winit.Modules.ETL.Invoice.DL.Interfaces;

namespace Winit.Modules.ETL.Invoice.BL.Classes
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IInvoiceDL _invoiceDL;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public InvoiceService(
            ILogger<InvoiceService> logger,
            IInvoiceDL invoiceDL,
            IDatabaseConnectionFactory connectionFactory)
        {
            _logger = logger;
            _invoiceDL = invoiceDL;
            _connectionFactory = connectionFactory;
        }

        public async Task InsertInvoiceWithLines(IStgInvoice invoice, string connectionString)
        {
            try
            {
                // No longer creating connection here, it's handled in the DL
                await _invoiceDL.InsertInvoiceWithLines(invoice, connectionString);
                _logger.LogInformation("Invoice and lines inserted successfully (via DL).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting invoice or retrieving invoice (via DL).");
                throw;
            }
        }

        public async Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, string connectionString)
        {
            try
            {
                return await _invoiceDL.GetInvoiceWithLines(invoiceUid, invoiceDate, connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice (via DL).");
                throw;
            }
        }

        public async Task ProcessInvoice(string invoiceUid, DateTime invoiceDate)
        {
            using var sourceConnection = _connectionFactory.CreateSourceConnection();
            using var destinationConnection = _connectionFactory.CreateDestinationConnection();

            try
            {
                // Read from source database
                var invoice = await _invoiceDL.GetInvoiceWithLines(invoiceUid, invoiceDate, sourceConnection);
                
                if (invoice != null)
                {
                    // Write to destination database
                    await _invoiceDL.ProcessInvoiceWithLines(invoice, destinationConnection);
                    _logger.LogInformation("Successfully processed invoice {InvoiceUid}", invoiceUid);
                }
                else
                {
                    _logger.LogWarning("Invoice {InvoiceUid} not found in source database", invoiceUid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoice {InvoiceUid}", invoiceUid);
                throw;
            }
        }

        public async Task ProcessInvoicesByDateRange(DateTime startDate, DateTime endDate)
        {
            using var sourceConnection = _connectionFactory.CreateSourceConnection();
            using var destinationConnection = _connectionFactory.CreateDestinationConnection();

            try
            {
                // Read from source database
                var invoices = await _invoiceDL.GetInvoicesByDateRange(startDate, endDate, sourceConnection);
                
                if (invoices.Any())
                {
                    // Write to destination database
                    await _invoiceDL.ProcessInvoices(invoices, destinationConnection);
                    _logger.LogInformation("Successfully processed {Count} invoices", invoices.Count());
                }
                else
                {
                    _logger.LogWarning("No invoices found in source database for date range {StartDate} to {EndDate}", startDate, endDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing invoices for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }
    }
}
