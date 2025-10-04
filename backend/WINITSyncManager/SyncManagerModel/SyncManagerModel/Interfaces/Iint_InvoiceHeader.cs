using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IInt_InvoiceHeader : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string OracleOrderNumber { get; set; }
        public string DeliveryId { get; set; }
        public string GstInvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public byte[] InvoiceFile { get; set; }
        public string ArNumber { get; set; }
        public List<IInt_InvoiceLine> InvoiceLines { get; set; }
        public List<IInt_InvoiceSerialNo> InvoiceSerialNos { get; set; }
    }
}
