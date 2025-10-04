using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.UIInterfaces;

public interface ISalesOrderItemView
{
    public Int64 Id { get; set; }
    public string UID { get; set; }
    public string SalesOrderLineUID { get; set; }
    public bool IsCartItem { get; set; }
    public ItemType ItemType { get; set; }
    public int LineNumber { get; set; }
    public string? SKUImage { get; set; }
    public string SKUUID { get; set; }
    public string SKUCode { get; set; }
    public string SKUName { get; set; }
    public string SKULabel { get; set; }
    public bool IsMCL { get; set; }
    public bool IsPromo { get; set; }
    public bool IsNPD { get; set; }
    public decimal SCQty { get; set; }
    public decimal RCQty { get; set; }
    public decimal VanQty { get; set; }
    public decimal MaxQty { get; set; }
    public decimal OrderQty { get; set; }
    public string OrderUOM { get; set; }
    public decimal OrderUOMMultiplier { get; set; }
    public decimal DeliveredQty { get; set; }
    public decimal Qty { get; set; }
    public decimal QtyBU { get; set; }
    public decimal MissedQty { get; set; }
    public string BaseUOM { get; set; }
    public SKU.Model.UIInterfaces.ISKUUOMView? SelectedUOM { get; set; }
    public List<SKU.Model.UIInterfaces.ISKUUOMView> AllowedUOMs { get; set; }
    public HashSet<string> UsedUOMCodes { get; set; }
    public decimal BasePrice { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ReplacePrice { get; set; }
    public decimal BasePriceLabel { get; set; }
    public decimal PriceUpperLimit { get; set; }
    public decimal PriceLowerLimit { get; set; }
    public decimal MRP { get; set; }
    public bool IsTaxable { get; set; }
    public List<string>? ApplicableTaxes { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalLineDiscount { get; set; }
    public decimal TotalCashDiscount { get; set; }
    public decimal TotalHeaderDiscount { get; set; }
    public decimal TotalDiscount { get; }
    public decimal TotalExciseDuty { get; set; }
    public decimal TotalLineTax { get; set; }
    public decimal TotalHeaderTax { get; set; }
    public decimal TotalTax { get; }
    public decimal NetAmount { get; set; }
    public string? GSVText { get; }
    public string? SKUPriceUID { get; set; }
    public string? SKUPriceListUID { get; set; }
    public string? PriceVersionNo { get; set; }
    public Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>? Attributes { get; set; }
    public ItemState ItemStatus { get; set; }
    //public List<string> ApplicationPromotionUIDs { get; set; }
    public HashSet<string> ApplicablePromotionUIDs { get; set; }
    public HashSet<string> AppliedPromotionUIDs { get; set; }
    public string PromotionUID { get; set; }
    public string CurrencyLabel { get; set; }
    public string Remarks { get; set; }
    public bool IsSelected { get; set; }
    public string? CatalogueURL { get; set; }
    public List<string>? SKUImages { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> FOCSalesOrderItemViews { get; set; }
    public List<IAppliedTax> AppliedTaxes { get; set; }
    public HashSet<string> FilterKeys { get; set; }
    public ISalesOrderItemView Clone(SKU.Model.UIInterfaces.ISKUUOMView newUOM, ItemState status, string uniqueIdentifier);
    public List<Shared.Models.Common.SelectionItem> GetSelectionItems();
}

