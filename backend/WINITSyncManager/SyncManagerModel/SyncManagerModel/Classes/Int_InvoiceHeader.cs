using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class Int_InvoiceHeader : SyncBaseModel, IInt_InvoiceHeader
    {
        public Int_InvoiceHeader()
        {
            InvoiceLines = new List<Int_InvoiceLine>();
            InvoiceSerialNos = new List<Int_InvoiceSerialNo>();
        }
        public long SyncLogId { get; set; }
        public string UID { get; set; }
        public string OracleOrderNumber { get; set; }
        public string DeliveryId { get; set; }
        public string GstInvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public byte[] InvoiceFile { get; set; }
        public string ArNumber { get; set; }
        public List<Int_InvoiceLine> InvoiceLines { get; set; }
        public List<Int_InvoiceSerialNo> InvoiceSerialNos { get; set; }

        List<IInt_InvoiceLine> IInt_InvoiceHeader.InvoiceLines
        {
            get => InvoiceLines.Cast<IInt_InvoiceLine>().ToList();
            set => InvoiceLines = value.Cast<Int_InvoiceLine>().ToList();
        }
        List<IInt_InvoiceSerialNo> IInt_InvoiceHeader.InvoiceSerialNos
        {
            get => InvoiceSerialNos.Cast<IInt_InvoiceSerialNo>().ToList();
            set => InvoiceSerialNos = value.Cast<Int_InvoiceSerialNo>().ToList();
        }
    }
}
