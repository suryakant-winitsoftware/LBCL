using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccElement 
    {
        public string ReceiptNumber { get; set; }
        public string Salesman { get; set; }
        public string Status { get; set; }
        public string Route { get; set; }
        public bool IsSettled { get; set; }
        public bool IsVoid { get; set; }
        public bool IsReversal { get; set; }
        public string Category { get; set; }
        public string CashNumber { get; set; }
        public string? UID { get; set; }
        public decimal Amount { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public string CurrencyUID { get; set; }
        public decimal PaidAmount { get; set; }
        public string StoreUID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? CollectedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Comments { get; set; }
        public string CodeName { get; set; }
        public string DocumentType { get; set; }
    }
}
