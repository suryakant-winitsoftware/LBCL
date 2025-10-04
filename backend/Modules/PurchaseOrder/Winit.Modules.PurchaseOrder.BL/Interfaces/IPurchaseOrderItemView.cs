using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Interfaces;

public interface IPurchaseOrderItemView : IPurchaseOrderLine
{
    bool IsSelected { get; set; }
    bool IsCartItem { get; set; }
    decimal MaxQty { get; set; }
    decimal DummyPrice { get; set; }
    string? SKUName { get; set; }
    string? ModelCode { get; set; }
    string? L1 { get; set; }
    string? L2 { get; set; }
    string? L3 { get; set; }
    string? L4 { get; set; }
    string? L5 { get; set; }
    string? L6 { get; set; }
    bool IsPromo { get; set; }
    ItemType ItemType { get; set; }
    HashSet<string>? ApplicablePromotionUIDs { get; set; }
    HashSet<string>? AppliedPromotionUIDs { get; set; }
    HashSet<string> FilterKeys { get; set; }
    string? CatalogueURL { get; set; }
    string? SKUImage { get; set; }
    List<string>? SKUImages { get; set; }
    SKU.Model.UIInterfaces.ISKUUOMView? SelectedUOM { get; set; }
    List<SKU.Model.UIInterfaces.ISKUUOMView>? AllowedUOMs { get; set; }
    List<string>? ApplicableTaxes { get; set; }
    ItemState ItemStatus { get; set; }
    Dictionary<string, ISKUAttributeView>? Attributes { get; set; }
    string? SKUPriceUID { get; set; }
    string? SKUPriceListUID { get; set; }
    string? PriceVersionNo { get; set; }
    string SupplierOrgUID { get; set; }
    int ProductCategoryId { get; set; }
    public string ModelDescription { get; set; }
    List<IAppliedTax>? AppliedTaxes { get; set; }
    int? inputQty { get; set; }
    public decimal UnitDiscount { get; }
    List<IPurchaseOrderLineProvision> PurchaseOrderLineProvisions { get; set; }
}
