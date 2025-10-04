using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IInt_InvoiceSerialNo: ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string DeliveryId { get; set; }
        public string ItemCode { get; set; }
        public string SerialNumbers { get; set; }
    }
}
