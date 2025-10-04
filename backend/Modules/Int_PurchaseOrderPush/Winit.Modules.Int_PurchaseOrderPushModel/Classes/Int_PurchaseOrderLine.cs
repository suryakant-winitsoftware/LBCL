using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_PurchaseOrderPushModel.Interfaces;

namespace Winit.Modules.Int_PurchaseOrderPushModel.Classes
{
    public class Int_PurchaseOrderLine : SyncBaseModel, Iint_PurchaseOrderLine
    {
        public long SyncLogDetailId { get; set; }
        public string? UID { get; set; }
        public long? HeaderId { get; set; } 
        public string? PurchaseOrderLineUid { get; set; }
        public string? PurchaseOrderUid { get; set; }
        public string? ItemCode { get; set; }
        public decimal? OrderedQty { get; set; }
        public decimal? Mrp { get; set; }
        public decimal? Dp { get; set; }
        public decimal? LadderingPercentage { get; set; }
        public decimal? LadderingDiscount { get; set; }
        public decimal? SellInDiscountUnitValue { get; set; }
        public decimal? SellInDiscountUnitPercentage { get; set; }
        public decimal? SellInDiscountTotalValue { get; set; }
        public decimal? NetUnitPrice { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? SellInCnP1UnitPercentage { get; set; }
        public decimal? SellInCnP1UnitValue { get; set; }
        public decimal? SellInCnP1Value { get; set; }
        public decimal? CashDiscountPercentage { get; set; }
        public decimal? CashDiscountValue { get; set; }
        public decimal? SellInP2Amount { get; set; }
        public decimal? SellInP3Amount { get; set; }
        public decimal? P3StandingAmount { get; set; }
        public long? Id { get; set ; }
        public string SellInSchemeCode { get; set; }
        public string QpsSchemeCode { get; set; }
        public decimal? P2QpsUnitValue { get; set; }
        public decimal? P2QpsTotalValue { get; set; }
        public decimal? P3QpsUnitValue { get; set; }
        public decimal? P3QpsTotalValue { get; set; }
    }
}
