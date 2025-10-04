using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCollectionAllotment : IBaseModel
    {
        public int Id { get; set; }
        public bool IsCheckBox { get; set; } 
        public string Customer { get; set; }
        public string Route { get; set; }
        public string StoreUID { get; set; }
        public string ReceiptNumber { get; set; }
        public string Salesmen { get; set; }
        public decimal EnteredAmount { get; set; }
        public long Count { get; set; }
        public int Range1 { get; set; }
        public int Range2 { get; set; }
        public int Range3 { get; set; }
        public int Range4 { get; set; }
        public string SessionUserCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AccCollectionUID { get; set; }
        public string CNUID { get; set; }
        public decimal CNAmount { get; set; }
        public string TargetType { get; set; }
        public string PaymentType { get; set; }
        public string CashNumber { get; set; }
        public string TargetUID { get; set; }
        public string ChequeNo { get; set; }
        public string ReferenceNumber { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public decimal Amount { get; set; }
        public string DelayTime { get; set; }//in days
        public decimal PaidAmount { get; set; }
        public decimal PayingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal Remaining { get; set; }
        public string CurrencyUID { get; set; }
        public string DefaultCurrencyUID { get; set; }
        public decimal DefaultCurrencyExchangeRate { get; set; }
        public string EarlyPaymentDiscountReferenceNo { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public decimal EarlyPaymentDiscountPercentage { get; set; }
        public decimal EarlyPaymentDiscountAmount { get; set; }
        public bool Discount { get; set; }
        public decimal PaidAmounts { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? TrxDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Usercode { get; set; }
    }
}
