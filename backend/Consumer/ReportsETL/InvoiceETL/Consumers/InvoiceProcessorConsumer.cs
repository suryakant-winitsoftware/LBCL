using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.BL.Interfaces;
using Winit.Modules.ETL.Invoice.Model.Classes;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace InvoiceETL.Consumers
{
    public class InvoiceProcessorConsumer : IConsumer<IInvoiceDataChangeEvent>
    {
        private readonly ILogger<InvoiceProcessorConsumer> _logger;
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _configuration;
        public InvoiceProcessorConsumer(ILogger<InvoiceProcessorConsumer> logger, IInvoiceService invoiceService, IConfiguration configuration)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<IInvoiceDataChangeEvent> context)
        {
            IInvoiceDataChangeEvent invoiceDataChangeEvent = context.Message;
            try
            {
                if (invoiceDataChangeEvent == null)
                {
                    return;
                }
                _logger.LogInformation($"InvoiceProcessorConsumer started with: InvoiceUID: {invoiceDataChangeEvent.InvoiceUID}, ActionType = {invoiceDataChangeEvent.ActionType}");

                // Example: Insert an invoice and related lines
                try
                {
                    // Create an invoice
                    var invoice = new StgInvoice
                    {
                        InvoiceUid = invoiceDataChangeEvent.InvoiceUID,
                        InvoiceNumber = invoiceDataChangeEvent.InvoiceUID,
                        InvoiceDate = DateTime.Now,
                        TotalAmount = 100.00m,
                        TotalDiscount = 0,
                        TotalTax = 10,
                        NetAmount = 110,
                        LineCount = 2,
                        QtyCount = 2,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                    };

                    // Create invoice lines
                    var line1 = new StgInvoiceLine
                    {
                        Id = 1,
                        InvoiceLineUid = Guid.NewGuid().ToString(),
                        InvoiceUid = invoice.InvoiceUid,
                        InvoiceNumber = invoice.InvoiceNumber,
                        LineNumber = 1,
                        SkuUid = Guid.NewGuid().ToString(),
                        SkuCode = "SKU-001",
                        SkuName = "Product 1",
                        UnitPrice = 50.00m,
                        Qty = 1,
                        TotalAmount = 50.00m,
                        TotalDiscount = 0,
                        TotalTax = 5,
                        NetAmount = 55,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        InvoiceDate = invoice.InvoiceDate
                    };

                    var line2 = new StgInvoiceLine
                    {
                        Id = 2,
                        InvoiceLineUid = Guid.NewGuid().ToString(),
                        InvoiceUid = invoice.InvoiceUid,
                        InvoiceNumber = invoice.InvoiceNumber,
                        LineNumber = 2,
                        SkuUid = Guid.NewGuid().ToString(),
                        SkuCode = "SKU-002",
                        SkuName = "Product 2",
                        UnitPrice = 50.00m,
                        Qty = 1,
                        TotalAmount = 50.00m,
                        TotalDiscount = 0,
                        TotalTax = 5,
                        NetAmount = 55,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        InvoiceDate = invoice.InvoiceDate
                    };

                    invoice.InvoiceLines.Add(line1);
                    invoice.InvoiceLines.Add(line2);

                    // Insert the invoice and lines using the service
                    // Pass the connection string to the service
                    await _invoiceService.InsertInvoiceWithLines(invoice, _configuration.GetConnectionString("DestinationDB"));
                    _logger.LogInformation("Invoice and lines inserted/updated successfully.");

                    //Demonstrate GetInvoiceWithLines
                    var fetchedInvoice = await invoiceService.GetInvoiceWithLines(invoice.InvoiceUid, invoice.InvoiceDate, _configuration.GetConnectionString("DestinationDB"));
                    if (fetchedInvoice != null)
                    {
                        _logger.LogInformation($"Retrieved invoice: {fetchedInvoice.InvoiceNumber} with {fetchedInvoice.InvoiceLines.Count} lines.");
                    }
                    else
                    {
                        _logger.LogWarning($"Could not retrieve invoice: {invoice.InvoiceNumber}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting/updating invoice or retrieving invoice.");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InvoiceWorker is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvoiceWorker stopped due to an error.");
            }
            finally
            {
                _logger.LogInformation("InvoiceWorker stopped.");
            }
        }
    }
}
