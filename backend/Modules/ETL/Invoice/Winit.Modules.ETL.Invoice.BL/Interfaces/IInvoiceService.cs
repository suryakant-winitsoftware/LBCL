using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.BL.Interfaces
{
    public interface IInvoiceService
    {
        Task InsertInvoiceWithLines(IStgInvoice invoice, string connectionString);
        Task<IStgInvoice?> GetInvoiceWithLines(string invoiceUid, DateTime invoiceDate, string connectionString);
    }
}
