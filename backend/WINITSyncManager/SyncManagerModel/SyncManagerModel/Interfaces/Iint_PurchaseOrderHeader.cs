using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface Iint_PurchaseOrderHeader : ISyncBaseModel
    {
        public long Id { get; set; }
        public long SyncLogDetailId { get; set; }
        public string? UID { get; set; }
        public string? PurchaseOrderUid { get; set; }
        public string? DmsOrderNumber { get; set; }
        public string? DmsOrderDate { get; set; }
        public string? OrderType { get; set; }
        public string? SalesDivision { get; set; }
        public string? SalesOffice { get; set; }
        public string? DealerCode { get; set; }
        public string? BillTo { get; set; }
        public string? ShipTo { get; set; }
        public string? OperatingUnit { get; set; }
        public string? WarehouseCode { get; set; }
        public string? SalesPerson { get; set; }
    }
}
