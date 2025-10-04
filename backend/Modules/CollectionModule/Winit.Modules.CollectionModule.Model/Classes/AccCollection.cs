using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccCollection : BaseModel, IAccCollection
    {
        public string SalesOrg { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string ApplicableType { get; set; }
        public string CashDepositStatus { get; set; }
        public string ApplicableCode { get; set; }
        public int AdvancePaidDays { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool ApplicableOnPartialPayments { get; set; }
        public bool ApplicableOnOverDueCustomers { get; set; }
        public string SessionUserCode { get; set; }
        public bool flag { get; set; } = true;
        public bool IsExpanded { get; set; } = false;
        public string Customer { get; set; }
        public string Salesman { get; set; }
        public string Route { get; set; }
        public string ChequeNo { get; set; }
        public bool Excel { get; set; } = false;
        public string ReceiptNumber { get; set; }
        public string CashNumber { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string PaymentMode { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string CurrencyUID { get; set; }
        public bool IsEarlyPayment { get; set; }
        public string DefaultCurrencyUID { get; set; }
        public decimal DefaultCurrencyExchangeRate { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public string OrgUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string StoreUID { get; set; }
        public string? RouteUID { get; set; }
        public string JobPositionUID { get; set; }
        public string CreditNote { get; set; }
        public string EmpUID { get; set; }
        public DateTime? CollectedDate { get; set; }
        public string Status { get; set; }
        public string ReversalReceiptUID { get; set; }
        public string Remarks { get; set; }
        public string ReferenceNumber { get; set; }
        public bool IsRealized { get; set; }
        public bool IsSettled { get; set; }
        public bool IsVoid { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Source { get; set; }
        public bool IsReversal { get; set; }
        public bool IsMultimode { get; set; }
        public DateTime? TripDate { get; set; }
        public DateTime? CancelledOn { get; set; }
        public string CancelledBy { get; set; }
        public string Comments { get; set; }
        public string TrxType { get; set; }
        public bool IsRemoteCollection { get; set; }
        public string RemoteCollectionReason { get; set; }
    }
}
