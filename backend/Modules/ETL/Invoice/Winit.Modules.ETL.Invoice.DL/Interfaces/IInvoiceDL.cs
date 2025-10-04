using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.DL.Interfaces
{
    public interface IInvoiceDL
    {
        // Read operations
        Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, IDbConnection sourceConnection)
        {
            return default;
        }
        Task<IEnumerable<IStgInvoice>> GetInvoicesByDateRange(DateTime startDate, DateTime endDate, IDbConnection sourceConnection)
        {
            return default;
        }

        // Write operations
        Task ProcessInvoiceWithLines(IStgInvoice invoice, IDbConnection destinationConnection)
        {
            return Task.CompletedTask;
        }
        Task ProcessInvoices(IEnumerable<IStgInvoice> invoices, IDbConnection destinationConnection)
        {
            return Task.CompletedTask;
        }
    }
}
