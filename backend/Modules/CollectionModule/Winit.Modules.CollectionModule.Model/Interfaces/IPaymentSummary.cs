using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IPaymentSummary
    {
        public string SalesManCode { get; set; }
        public string SalesManName { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string StoreName { get; set; }
        public string StoreCode { get; set; }
        public decimal CashAmount { get; set; }
        public decimal ChequeAmount { get; set; }
        public decimal POSAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public string CashTotalAmount { get; set; }
        public string ChequeTotalAmount { get; set; }
        public string POSTotalAmount { get; set; }
        public string OnlineTotalAmount { get; set; }
    }
}
