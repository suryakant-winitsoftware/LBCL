using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface ICollectionPrintDetails
    {
        public decimal Amount { get; set; }
        public string AccCollectionUID { get; set; }
        public string TargetType { get; set; }
        public string ReferenceNumber { get; set; }
        public string CurrencyUID { get; set; }
        public decimal EarlyPaymentDiscountPercentage { get; set; }
        public decimal EarlyPaymentDiscountAmount { get; set; }
        public string SourceType { get; set; }

        public string SourceUID { get; set; }
        public string OrgUID { get; set; }
        public string JobPositionUID { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal UnSettledAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal BalanceAmount { get; set; }
        public string Source { get; set; }
    }
}
