using Winit.Modules.Base.Model;
using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.PurchaseOrder.Model.Interfaces;

namespace Winit.Modules.PurchaseOrder.Model.Classes;

public class PurchaseOrderLine : BaseModel, IPurchaseOrderLine
{
    public string PurchaseOrderHeaderUID { get; set; } = string.Empty;
    [AuditTrail("S.No")] public int LineNumber { get; set; }
    public string SKUUID { get; set; } = string.Empty;
    [AuditTrail("Item Code")] public string SKUCode { get; set; } = string.Empty;
    public string SKUType { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public string BaseUOM { get; set; } = string.Empty;
    public decimal UOMConversionToBU { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal ModelQty { get; set; }
    public decimal InTransitQty { get; set; }
    public decimal SuggestedQty { get; set; }
    public decimal Past3MonthAvg { get; set; }
    public decimal RequestedQty { get; set; }
    [AuditTrail("Order Qty")] public decimal FinalQty { get; set; }
    public decimal FinalQtyBU { get; set; }
    [AuditTrail("Approved Price (L)")] public decimal UnitPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalAmount { get; set; }
    [AuditTrail] public decimal TotalDiscount { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal HeaderDiscount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal HeaderTaxAmount { get; set; }
    [AuditTrail("Total Amount")] public decimal NetAmount { get; set; }
    public string? TaxData { get; set; }
    public decimal App1Qty { get; set; }
    public decimal App2Qty { get; set; }
    public decimal App3Qty { get; set; }
    public decimal App4Qty { get; set; }
    public decimal App5Qty { get; set; }
    public decimal App6Qty { get; set; }
    public decimal Mrp { get; set; }
    [AuditTrail("Dealer Price")] public decimal DpPrice { get; set; }
    public decimal LadderingPercentage { get; set; }
    public decimal LadderingDiscount { get; set; }
    public decimal MinSellingPrice { get; set; }
    public decimal SellInDiscountUnitValue { get; set; }
    public decimal SellInDiscountUnitPercentage { get; set; }
    public decimal SellInDiscountTotalValue { get; set; }
    public decimal SellInCnP1UnitPercentage { get; set; }
    public decimal SellInCnP1UnitValue { get; set; }
    public decimal SellInCnP1Value { get; set; }
    public decimal CashDiscountPercentage { get; set; }
    public decimal CashDiscountValue { get; set; }
    public decimal SellInP2Amount { get; set; }
    public decimal SellInP3Amount { get; set; }
    public decimal P3StandingAmount { get; set; }
    public string? PromotionUID { get; set; }

    [AuditTrail("Billing Price")]
    public decimal EffectiveUnitPrice
    {
        get { return UnitPrice - (FinalQty == 0m ? 0m : SellInDiscountTotalValue / FinalQty); }
    }

    public decimal EffectiveUnitTax { get; set; }
    public decimal BilledQty { get; set; }
    public decimal CancelledQty { get; set; }
    public bool IsUpdatedFromErp { get; set; }
    public string? SellInSchemeCode { get; set; }
    public string? StandingSchemeData { get; set; }
    public string? QPSSchemeCode { get; set; }
    public decimal? P2QPSTotalValue { get; set; }
    public decimal? P3QPSTotalValue { get; set; }
    public decimal? MarginUnitValue { get; set; }
    public string QPSOfferType { get; set; }
    public decimal QPSOfferValue { get; set; }
    public decimal QPSUnitValue { get; set; }
    public decimal StandingUnitValue { get; set; }
}