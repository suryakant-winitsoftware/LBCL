using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class Int_InvoiceSerialNo :SyncBaseModel, IInt_InvoiceSerialNo
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string DeliveryId { get; set; }
        public string ItemCode { get; set; }
        public string SerialNumbers { get; set; }
    }
}
