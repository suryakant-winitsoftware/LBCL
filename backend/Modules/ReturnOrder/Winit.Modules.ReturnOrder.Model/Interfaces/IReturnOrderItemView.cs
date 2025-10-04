using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.Model.Interfaces
{
    public interface IReturnOrderItemView:IReturnOrderLine
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
        public HashSet<string> FilterKeys { get; set; }
        public ItemState ItemStatus { get; set; }
        public string InvoiceUID { get; set; }
        public string InvoiceLineUID { get; set; }
        public bool IsSelected { get; set; }
        public List<ISelectionItem> ReasonsList { get; set; }
        public string UOMLabel { get; set; }
        public bool IsReasonDDOpen { get; set; }
        public IReturnOrderItemView Clone(SKU.Model.UIInterfaces.ISKUUOMView newUOM, ItemState status, string uniqueIdentifier);
        public List<Shared.Models.Common.ISelectionItem> GetSelectionItems();
    }
}
