using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class Int_PurchaseOrderStatus :SyncBaseModel, Iint_PurchaseOrderStatus
    {
        public Int_PurchaseOrderStatus() {
            this.iint_PurchaseOrderCancellations = new List<Int_PurchaseOrderCancellation>();
        }

        public long? SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? PurchaseOrderUid { get; set; }
        public string? ErpOrderNumber { get; set; }
        public DateTime? ErpOrderDate { get; set; }
        public List<Int_PurchaseOrderCancellation> iint_PurchaseOrderCancellations { get; set; }
        List<Iint_PurchaseOrderCancellation> Iint_PurchaseOrderStatus.iint_PurchaseOrderCancellations
        {
            get => iint_PurchaseOrderCancellations.Cast<Iint_PurchaseOrderCancellation>().ToList();
            set => iint_PurchaseOrderCancellations = value.Cast<Int_PurchaseOrderCancellation>().ToList();
        }
    }
}
