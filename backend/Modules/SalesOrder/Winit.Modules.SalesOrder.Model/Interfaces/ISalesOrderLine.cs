using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.SalesOrder.Model.Interfaces
{
   

    public interface ISalesOrderLine : IBaseModelV2
    {
   
        public string SalesOrderUID { get; set; }
        public string SalesOrderLineUID { get; set; }
        public int LineNumber { get; set; }
        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public decimal BasePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal FakeUnitPrice { get; set; }
        public string BaseUOM { get; set; }
        public string UoM { get; set; }
        public decimal UOMConversionToBU { get; set; }
        public string RecoUOM { get; set; }
        public decimal RecoQty { get; set; }
        public decimal RecoUOMConversionToBU { get; set; }
        public decimal RecoQtyBU { get; set; }
        public decimal ModelQtyBU { get; set; }
        public decimal Qty { get; set; }
        public decimal QtyBU { get; set; }
        public decimal VanQtyBU { get; set; }
        public decimal DeliveredQty { get; set; }
        public decimal MissedQty { get; set; }
        public decimal ReturnedQty { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal LineTaxAmount { get; set; }
        public decimal ProrataTaxAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetFakeAmount { get; set; }
        public string? SKUPriceListUID { get; set; }
        public string? SKUPriceUID { get; set; }
        public decimal ProrataDiscountAmount { get; set; }
        public decimal LineDiscountAmount { get; set; }
        public decimal MRP { get; set; }
        public decimal CostUnitPrice { get; set; }
        public string ParentUID { get; set; }
        public bool IsPromotionApplied { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public decimal Weight { get; set; }
        public string WeightUnit { get; set; }
        public string StockType { get; set; }
        public string Remarks { get; set; }
        public decimal TotalCashDiscount { get; set; }
        public decimal TotalExciseDuty { get; set; }
        public string SKUUID { get; set; }
        public decimal ApprovedQty { get; set; }
        public string TaxData { get; set; }
        public ActionType ActionType { get; set; }
    }
}
