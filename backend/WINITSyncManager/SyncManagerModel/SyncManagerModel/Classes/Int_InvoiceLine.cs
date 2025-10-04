using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class Int_InvoiceLine :SyncBaseModel, IInt_InvoiceLine
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string DeliveryId { get; set; }
        public string ItemCode { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal ShippedQty { get; set; }
        public decimal CancelledQty { get; set; }
    }
}
