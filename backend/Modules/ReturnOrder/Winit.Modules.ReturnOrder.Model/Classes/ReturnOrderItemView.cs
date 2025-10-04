using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.Model.Classes
{
    public class ReturnOrderItemView : ReturnOrderLine, IReturnOrderItemView
    {
        public string? SKUImage { get; set; }
        public string SKUName { get; set; }
        public string SKULabel { get; }
        public decimal MaxQty { get; set; }
        public decimal OrderQty { get; set; }
        public SKU.Model.UIInterfaces.ISKUUOMView? SelectedUOM { get; set; }
        public List<SKU.Model.UIInterfaces.ISKUUOMView> AllowedUOMs { get; set; }
        public List<string>? UsedUOMCodes { get; set; }
        public bool IsTaxable { get; set; }
        public List<Tax.Model.UIInterfaces.ITaxView>? ApplicableTaxes { get; set; }
        public decimal TotalLineDiscount { get; set; }
        public decimal TotalCashDiscount { get; set; }
        public decimal TotalLineTax { get; set; }
        public decimal TotalHeaderTax { get; set; }
        public string? GSVText { get; }
        public Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>? Attributes { get; set; }
        public HashSet<string> FilterKeys { get; set; } = new HashSet<string>();
        public ItemState ItemStatus { get; set; }
        public string InvoiceUID { get; set; }
        public string InvoiceLineUID { get; set; }
        public bool IsSelected { get; set; }
        public List<ISelectionItem> ReasonsList { get; set; }
        public string UOMLabel { get; set; }
        public bool IsReasonDDOpen { get; set; }
        public ReturnOrderItemView()
        {
            AllowedUOMs = new List<SKU.Model.UIInterfaces.ISKUUOMView>();
            ApplicableTaxes = new List<Tax.Model.UIInterfaces.ITaxView>();
            Attributes = new Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>();
        }
        public IReturnOrderItemView Clone(SKU.Model.UIInterfaces.ISKUUOMView newUOM, ItemState status, string uniqueIdentifier)
        {
            var clonedItem = new ReturnOrderItemView
            {
                UID = $"{this.UID}_{uniqueIdentifier}",
                SKUImage = this.SKUImage,
                SKUUID = this.SKUUID,
                SKUCode = this.SKUCode,
                SKUName = this.SKUName,
                SKUType = this.SKUType,
                MaxQty = this.MaxQty,
                OrderQty = 0,
                ApprovedQty = this.ApprovedQty,
                ReturnedQty = this.ReturnedQty,
                SelectedUOM = newUOM,
                AllowedUOMs = this.AllowedUOMs,
                UsedUOMCodes = this.UsedUOMCodes,
                BasePrice = this.BasePrice,
                UnitPrice = this.BasePrice * newUOM.Multiplier,
                IsTaxable = this.IsTaxable,
                ApplicableTaxes = this.ApplicableTaxes,
                TotalAmount = 0,
                TotalDiscount = 0,
                TotalExciseDuty = 0,
                TotalTax = this.TotalTax,
                SKUPriceUID = this.SKUPriceUID,
                SKUPriceListUID = this.SKUPriceListUID,
                Attributes = this.Attributes,
                ItemStatus = status,
                ReasonCode = this.ReasonCode,
                ReasonText = this.ReasonText,
                ExpiryDate = this.ExpiryDate,
                BatchNumber = this.BatchNumber,
                InvoiceUID = this.InvoiceUID,
                InvoiceLineUID = this.InvoiceLineUID,
                Remarks = this.Remarks,
                Volume = this.Volume,
                VolumeUnit = this.VolumeUnit,
                LineNumber = 0,
                SalesOrderUID = this.SalesOrderUID,
                SalesOrderLineUID = string.Empty,
                PromotionUID = string.Empty,
                TotalLineTax = 0,
                TotalHeaderTax = 0,
                TotalCashDiscount = 0,
                TotalLineDiscount = 0,
                ReasonsList = this.ReasonsList,
            };

            return clonedItem;
        }
        public List<Shared.Models.Common.ISelectionItem> GetSelectionItems()
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
                }).ToList<ISelectionItem>();
        }
    }
}
