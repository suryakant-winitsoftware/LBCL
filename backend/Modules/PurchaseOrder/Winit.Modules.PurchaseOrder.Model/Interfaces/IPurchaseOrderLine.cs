using Winit.Modules.Base.Model;
using Winit.Modules.PurchaseOrder.Model.Classes;

namespace Winit.Modules.PurchaseOrder.Model.Interfaces;

public interface IPurchaseOrderLine : IBaseModel
{
    public string PurchaseOrderHeaderUID { get; set; }
    public int LineNumber { get; set; }
    public string SKUUID { get; set; }
    public string SKUCode { get; set; }
    public string SKUType { get; set; }
    public string UOM { get; set; }
    public string BaseUOM { get; set; }
    public decimal UOMConversionToBU { get; set; }
    public decimal AvailableQty { get; set; }
    public decimal ModelQty { get; set; }
    public decimal InTransitQty { get; set; }
    public decimal SuggestedQty { get; set; }
    public decimal Past3MonthAvg { get; set; }
    public decimal RequestedQty { get; set; }
    public decimal FinalQty { get; set; }
    public decimal FinalQtyBU { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal HeaderDiscount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal HeaderTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? TaxData { get; set; }
    public decimal App1Qty { get; set; }
    public decimal App2Qty { get; set; }
    public decimal App3Qty { get; set; }
    public decimal App4Qty { get; set; }
    public decimal App5Qty { get; set; }
    public decimal App6Qty { get; set; }
    public decimal Mrp { get; set; }
    public decimal DpPrice { get; set; }
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
    public decimal EffectiveUnitPrice { get; }
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
    public decimal StandingUnitValue {get; set;}
}
