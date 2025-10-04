using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface Iint_InvoiceDL
    {
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceHeader>> GetInvoiceHeaderDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceLine>> GetInvoiceLineDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceSerialNo>> GetInvoiceSerialNoDetails(string sql);
    }
}
