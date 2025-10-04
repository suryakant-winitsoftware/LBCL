using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface Iint_PurchaseOrderStatus: ISyncBaseModel
    {
        public long? SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? PurchaseOrderUid { get; set; }
        public string? ErpOrderNumber { get; set; }
        public DateTime? ErpOrderDate { get; set; }
        public List<Iint_PurchaseOrderCancellation> iint_PurchaseOrderCancellations { get; set; }
    }
}
