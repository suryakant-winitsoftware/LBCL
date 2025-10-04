using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface IOutstandingInvoiceDL
    {
        Task<List<SyncManagerModel.Interfaces.IOutstandingInvoice>> GetOutstandingInvoiceDetails(string sql);

    }
}
