using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class OutstandingInvoiceView
    {
        public string OU { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SourceType { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string TaxInvoiceNumber { get; set; }
        public DateTime TaxInvoiceDate { get; set; }
        public DateTime InvoiceDueDate { get; set; }

    }
}
