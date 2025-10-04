using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccDocument : IAccDocument
    {
        public string AccCollectionUID { get; set; }
        public string TargetType { get; set; }
        public string TargetUID { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyUID { get; set; }
        public string DefaultCurrencyUID { get; set; }
        public decimal DefaultCurrencyExchangeRate { get; set; }
        public decimal DefaultCurrencyAmount { get; set; }
        public decimal EarlyPaymentDiscountPercentage { get; set; }
        public decimal EarlyPaymentDiscountAmount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }
        public string UID { get; set; }
    }
}
