using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccPayable : IBaseModel
    {
        public string SessionUserCode { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string DelayTime { get; set; }
        public long Count { get; set; }
        public string AccCollectionUID { get; set; }
        public bool IsCheckBox { get; set; }
        public string ChequeNo { get; set; }
        public string Mode { get; set; }
        public decimal EarlyPaymentDiscountAmount { get; set; }
        public string EarlyPaymentDiscountReferenceNo { get; set; }
        public decimal PayingAmount { get; set; }
        public string SourceType { get; set; }
        public string SourceUID { get; set; }
        public string ReferenceNumber { get; set; }
        public string OrgUID { get; set; }
        public string TargetUID { get; set; }
        public string JobPositionUID { get; set; }
        public bool IsManaged { get; set; }
        public decimal Amount { get; set; }
        public decimal UnSettledAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public bool Discount { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal Balance { get; set; }
        public decimal EnteredAmount { get; set; }
        public bool edit { get; set; } 
        public bool IsActive { get; set; }
        public bool IsChecked { get; set; } 
        public bool isButtonEnabled { get; set; } 
        public string StoreUID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string CurrencyUID { get; set; }
        public string Source { get; set; }
    }
}
