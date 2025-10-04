using Winit.Modules.Common.Model.Classes.AuditTrail;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderItemView : PurchaseOrderLine, IPurchaseOrderItemView
{
    public bool IsSelected { get; set; }
    public bool IsCartItem { get; set; }
    public decimal MaxQty { get; set; }
    public decimal DummyPrice { get; set; }
    [AuditTrail]
    public string? SKUName { get; set; }
    public string? ModelCode { get; set; }
    public string? L1 { get; set; }
    public string? L2 { get; set; }
    public string? L3 { get; set; }
    public string? L4 { get; set; }
    public string? L5 { get; set; }
    public string? L6 { get; set; }
    public bool IsPromo { get; set; }
    public ItemType ItemType { get; set; }
    public HashSet<string>? ApplicablePromotionUIDs { get; set; }
    public HashSet<string>? AppliedPromotionUIDs { get; set; }
    public HashSet<string> FilterKeys { get; set; } = new HashSet<string>();
    public string? CatalogueURL { get; set; }
    public string? SKUImage { get; set; }
    public List<string>? SKUImages { get; set; }
    public SKU.Model.UIInterfaces.ISKUUOMView? SelectedUOM { get; set; }
    public List<SKU.Model.UIInterfaces.ISKUUOMView>? AllowedUOMs { get; set; }
    public List<string>? ApplicableTaxes { get; set; }
    public ItemState ItemStatus { get; set; }
    public Dictionary<string, ISKUAttributeView>? Attributes { get; set; }
    public string? SKUPriceUID { get; set; }
    public string? SKUPriceListUID { get; set; }
    public string? PriceVersionNo { get; set; }
    public string SupplierOrgUID { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string ModelDescription { get; set; }
    public List<IAppliedTax>? AppliedTaxes { get; set; }
    public int? inputQty
    {
        get
        {
            return (int?)FinalQty;
        }
        set
        {
            RequestedQty = value.GetValueOrDefault();
        }
    }
    public decimal UnitDiscount
    {
        get
        {
            return FinalQty == 0 ? 0 : TotalDiscount / FinalQty;
        }
    }
    public List<IPurchaseOrderLineProvision> PurchaseOrderLineProvisions { get; set; }
}
