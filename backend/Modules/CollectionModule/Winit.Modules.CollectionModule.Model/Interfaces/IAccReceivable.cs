using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccReceivable : IBaseModel
    {
        public string SessionUserCode { get; set; }

        public string AccCollectionUID { get; set; }

        

        public string SourceType { get; set; }

        public string CurrencyUID { get; set; }
        public string DocumentType { get; set; }

        public string SourceUID { get; set; }

        public decimal UnSettledAmount { get; set; }

        public string ReferenceNumber { get; set; }
        public string TargetUID { get; set; }

        public string ChequeNo { get; set; }

        public string OrgUID { get; set; }

        public string Source { get; set; }

        public string JobPositionUID { get; set; }

        public bool IsManaged { get; set; }

        public decimal Amount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal BalanceAmount { get; set; }

        public bool IsActive { get; set; }

        public string StoreUID { get; set; }

        public DateTime? TransactionDate { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
