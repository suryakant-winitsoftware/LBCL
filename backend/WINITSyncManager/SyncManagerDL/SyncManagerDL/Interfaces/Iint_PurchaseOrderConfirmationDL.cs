using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface Iint_PurchaseOrderConfirmationDL
    {
        Task<List<SyncManagerModel.Interfaces.Iint_PurchaseOrderCancellation>> GetPurchaseOrderCancellationDetails(string sql);
        Task<List<SyncManagerModel.Interfaces.Iint_PurchaseOrderStatus>> GetPurchaseOrderStatusDetails(string sql);
    }
}
