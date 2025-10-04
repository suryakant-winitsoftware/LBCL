using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StockAudit.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockAudit.Model.Classes
{
    public class StockAuditItemView: IStockAuditItemView
    {
        public string UOMQty { get; set; }
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
        public decimal BasePriceLabel { get; set; }
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
        public List<string> ApplicationPromotionUIDs { get; set; }
        public string PromotionUID { get; set; }
        public string CurrencyLabel { get; set; }
        public string Remarks { get; set; }
        public bool IsSelected { get; set; }
        public StockAuditItemView()
        {
            AllowedUOMs = new List<SKU.Model.UIInterfaces.ISKUUOMView>();
            ApplicableTaxes = new List<string>();
            Attributes = new Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>();
            UsedUOMCodes = new HashSet<string>();
        }

        public IStockAuditItemView Clone(SKU.Model.UIInterfaces.ISKUUOMView newUOM, ItemState status, string uniqueIdentifier)
        {
            var clonedItem = new StockAuditItemView
            {
                UID = $"{this.UID}_{uniqueIdentifier}",
                SalesOrderLineUID = string.Empty,
                IsCartItem = false,
                ItemType = ItemType.O,
                LineNumber = 0,
                SKUImage = this.SKUImage,
                SKUUID = this.SKUUID,
                SKUCode = this.SKUCode,
                SKUName = this.SKUName,
                SKULabel = this.SKULabel,
                IsMCL = this.IsMCL,
                IsPromo = this.IsPromo,
                IsNPD = this.IsNPD,
                SCQty = this.SCQty,
                RCQty = this.RCQty,
                VanQty = this.VanQty,
                MaxQty = this.MaxQty,
                OrderQty = this.OrderQty,
                OrderUOM = this.OrderUOM,
                OrderUOMMultiplier = this.OrderUOMMultiplier,
                Qty = 0,
                QtyBU = 0,
                //DeliveredQty = 0,
                MissedQty = 0,
                BaseUOM = this.BaseUOM,
                SelectedUOM = newUOM,
                AllowedUOMs = new List<SKU.Model.UIInterfaces.ISKUUOMView>() { newUOM },
                UsedUOMCodes = this.UsedUOMCodes,
                BasePrice = this.BasePrice,
                UnitPrice = this.BasePrice * newUOM.Multiplier,
                BasePriceLabel = this.BasePrice * newUOM.Multiplier,
                MRP = this.MRP,
                IsTaxable = this.IsTaxable,
                ApplicableTaxes = this.ApplicableTaxes,
                TotalAmount = 0,
                TotalLineDiscount = 0,
                TotalCashDiscount = 0,
                TotalHeaderDiscount = 0,
                TotalExciseDuty = 0,
                TotalLineTax = 0,
                TotalHeaderTax = 0,
                NetAmount = 0,
                SKUPriceUID = this.SKUPriceUID,
                SKUPriceListUID = this.SKUPriceListUID,
                PriceVersionNo = this.PriceVersionNo,
                Attributes = this.Attributes,
                ItemStatus = status,
                ApplicationPromotionUIDs = this.ApplicationPromotionUIDs,
                PromotionUID = string.Empty,
                CurrencyLabel = this.CurrencyLabel,
                Remarks = this.Remarks
            };

            return clonedItem;
        }

        public List<Shared.Models.Common.SelectionItem> GetSelectionItems()
        {
            // Transform AllowedUOMs to SelectionItem objects
            return AllowedUOMs
                .Where(uom => !UsedUOMCodes?.Contains(uom.Code) ?? true) // Filter out SKUUOMs with codes in UsedUOMCodes
                .Select(uom => new Shared.Models.Common.SelectionItem
                {
                    UID = uom.Code,
                    Code = uom.Code,
                    Label = uom.Label,
                    IsSelected = uom == SelectedUOM // Mark the currently selected SKUUOM as selected
                }).ToList();
        }
        // Retrieve a SKUAttributeView by Name
        public SKU.Model.UIInterfaces.ISKUAttributeView GetAttributeByName(string attributeName)
        {
            if (Attributes.TryGetValue(attributeName, out SKU.Model.UIInterfaces.ISKUAttributeView attribute))
            {
                return attribute;
            }
            return null; // Attribute not found
        }
       
    }
}
