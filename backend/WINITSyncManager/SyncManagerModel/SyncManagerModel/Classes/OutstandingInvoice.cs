using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class OutstandingInvoice : SyncBaseModel, IOutstandingInvoice
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string RecUid { get; set; }
        public int Ou { get; set; }
        public string CustomerNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string TaxInvoiceNum { get; set; }
        public DateTime? TaxInvoiceDate { get; set; }
        public decimal NetAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string InvoiceType { get; set; }
        public string Division { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
    }
}
