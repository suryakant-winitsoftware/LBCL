using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Classes;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCollectionSettlement : IBaseModel
    {
        public List<AccCollectionSettlementReceipts> Receipts { get; set; }
        public string SessionUserCode { get; set; }

        public int Id { get; set; }


        public string PaymentMode { get; set; }

        public string Route { get; set; }

        public string TargetType { get; set; }


        public DateTime? TransactionDate { get; set; }

        public DateTime? DueDate { get; set; }


        public string AccCollectionUID { get; set; }

        public decimal? CollectionAmount { get; set; }

        public decimal? ReceivedAmount { get; set; }

        public bool? HasDiscrepancy { get; set; }

        public decimal? DiscrepancyAmount { get; set; }

        public string DefaultCurrencyUID { get; set; }

        public DateTime? SettlementDate { get; set; }

        public string CashierJobPositionUID { get; set; }

        public string CashierEmpUID { get; set; }

        public string ReceiptNumber { get; set; }

        public string SettledBy { get; set; }
    }
}
