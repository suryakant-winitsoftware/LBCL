using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.DL.Classes.Base;
using Winit.Modules.ETL.Invoice.DL.Interfaces;
using Winit.Modules.ETL.Invoice.Model.Classes;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.DL.Classes
{
    public class CompositeInvoiceDL : InvoiceDLBase
    {
        private readonly IInvoiceQueryProvider _sourceQueryProvider;
        private readonly IInvoiceQueryProvider _destinationQueryProvider;
        private readonly ILogger<CompositeInvoiceDL> _logger;

        public CompositeInvoiceDL(
            ILogger<CompositeInvoiceDL> logger,
            IInvoiceQueryProviderFactory queryProviderFactory,
            string sourceDbType,
            string destinationDbType) : base(logger)
        {
            _logger = logger;
            _sourceQueryProvider = queryProviderFactory.GetSourceProvider(sourceDbType);
            _destinationQueryProvider = queryProviderFactory.GetDestinationProvider(destinationDbType);
        }

        public override async Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, IDbConnection sourceConnection)
        {
            var invoiceDictionary = new Dictionary<string, StgInvoice>();

            await sourceConnection.QueryAsync<StgInvoice, StgInvoiceLine, StgInvoice>(
                _sourceQueryProvider.GetInvoiceQuery(),
                (invoice, line) =>
                {
                    if (!invoiceDictionary.TryGetValue(invoice.InvoiceUid, out var existingInvoice))
                    {
                        existingInvoice = invoice;
                        existingInvoice.InvoiceLines = new List<IStgInvoiceLine>();
                        invoiceDictionary.Add(invoice.InvoiceUid, existingInvoice);
                    }

                    if (line != null)
                    {
                        existingInvoice.InvoiceLines.Add(line);
                    }

                    return existingInvoice;
                },
                new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate },
                splitOn: "InvoiceLineUid"
            );

            return invoiceDictionary.Values.FirstOrDefault();
        }

        public override async Task<IEnumerable<IStgInvoice>> GetInvoicesByDateRange(DateTime startDate, DateTime endDate, IDbConnection sourceConnection)
        {
            var invoiceDictionary = new Dictionary<string, StgInvoice>();

            await sourceConnection.QueryAsync<StgInvoice, StgInvoiceLine, StgInvoice>(
                _sourceQueryProvider.GetInvoicesByDateRangeQuery(),
                (invoice, line) =>
                {
                    if (!invoiceDictionary.TryGetValue(invoice.InvoiceUid, out var existingInvoice))
                    {
                        existingInvoice = invoice;
                        existingInvoice.InvoiceLines = new List<IStgInvoiceLine>();
                        invoiceDictionary.Add(invoice.InvoiceUid, existingInvoice);
                    }

                    if (line != null)
                    {
                        existingInvoice.InvoiceLines.Add(line);
                    }

                    return existingInvoice;
                },
                new { StartDate = startDate, EndDate = endDate },
                splitOn: "InvoiceLineUid"
            );

            return invoiceDictionary.Values;
        }

        public override async Task ProcessInvoiceWithLines(IStgInvoice invoice, IDbConnection destinationConnection)
        {
            using var transaction = destinationConnection.BeginTransaction();
            try
            {
                var invoiceExists = await InvoiceExistsInDestination(invoice.InvoiceUid, invoice.InvoiceDate, destinationConnection, transaction);

                if (invoiceExists)
                {
                    await UpdateInvoice(invoice, destinationConnection, transaction);
                }
                else
                {
                    await InsertInvoice(invoice, destinationConnection, transaction);
                }

                await ProcessInvoiceLines(invoice, destinationConnection, transaction);

                transaction.Commit();
                _logger.LogInformation("Transaction committed successfully for invoice {InvoiceUid}", invoice.InvoiceUid);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error processing invoice {InvoiceUid}", invoice.InvoiceUid);
                throw;
            }
        }

        public override async Task ProcessInvoices(IEnumerable<IStgInvoice> invoices, IDbConnection destinationConnection)
        {
            await ProcessInvoicesBatch(invoices, destinationConnection);
        }

        private async Task UpdateInvoice(IStgInvoice invoice, IDbConnection connection, IDbTransaction transaction)
        {
            await connection.ExecuteAsync(_destinationQueryProvider.GetUpdateInvoiceQuery(), invoice, transaction);
        }

        private async Task InsertInvoice(IStgInvoice invoice, IDbConnection connection, IDbTransaction transaction)
        {
            await connection.ExecuteAsync(_destinationQueryProvider.GetInsertInvoiceQuery(), invoice, transaction);
        }

        private async Task ProcessInvoiceLines(IStgInvoice invoice, IDbConnection connection, IDbTransaction transaction)
        {
            var existingLines = await GetExistingInvoiceLinesFromDestination(invoice.InvoiceUid, invoice.InvoiceDate, connection, transaction);
            var linesToInsert = new List<IStgInvoiceLine>();
            var linesToUpdate = new List<IStgInvoiceLine>();
            var linesToDelete = new List<IStgInvoiceLine>();

            foreach (var line in invoice.InvoiceLines)
            {
                var existingLine = existingLines.FirstOrDefault(l => l.InvoiceLineUid == line.InvoiceLineUid);
                if (existingLine != null)
                {
                    linesToUpdate.Add(line);
                }
                else
                {
                    linesToInsert.Add(line);
                }
            }

            foreach (var existingLine in existingLines)
            {
                if (!invoice.InvoiceLines.Any(l => l.InvoiceLineUid == existingLine.InvoiceLineUid))
                {
                    linesToDelete.Add(existingLine);
                }
            }

            if (linesToUpdate.Any())
            {
                await UpdateInvoiceLines(linesToUpdate, connection, transaction);
            }

            if (linesToInsert.Any())
            {
                await InsertInvoiceLines(linesToInsert, connection, transaction);
            }

            if (linesToDelete.Any())
            {
                await DeleteInvoiceLines(linesToDelete, connection, transaction);
            }
        }

        private async Task UpdateInvoiceLines(IEnumerable<IStgInvoiceLine> lines, IDbConnection connection, IDbTransaction transaction)
        {
            await connection.ExecuteAsync(_destinationQueryProvider.GetUpdateInvoiceLinesQuery(), lines, transaction);
        }

        private async Task InsertInvoiceLines(IEnumerable<IStgInvoiceLine> lines, IDbConnection connection, IDbTransaction transaction)
        {
            await connection.ExecuteAsync(_destinationQueryProvider.GetInsertInvoiceLinesQuery(), lines, transaction);
        }

        private async Task DeleteInvoiceLines(IEnumerable<IStgInvoiceLine> lines, IDbConnection connection, IDbTransaction transaction)
        {
            await connection.ExecuteAsync(_destinationQueryProvider.GetDeleteInvoiceLinesQuery(), lines, transaction);
        }

        private async Task<bool> InvoiceExistsInDestination(string invoiceUid, DateTime invoiceDate, IDbConnection connection, IDbTransaction transaction)
        {
            var count = await connection.ExecuteScalarAsync<int>(
                _destinationQueryProvider.GetInvoiceExistsQuery(),
                new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate },
                transaction
            );
            return count > 0;
        }

        private async Task<IEnumerable<IStgInvoiceLine>> GetExistingInvoiceLinesFromDestination(string invoiceUid, DateTime invoiceDate, IDbConnection connection, IDbTransaction transaction)
        {
            return await connection.QueryAsync<IStgInvoiceLine>(
                _destinationQueryProvider.GetExistingInvoiceLinesQuery(),
                new { InvoiceUid = invoiceUid, InvoiceDate = invoiceDate },
                transaction
            );
        }
    }
} 