using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IStoreCheckItemView
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
        public decimal OrderUOMMultiplier { get; set; }
        public string BaseUOM { get; set; }
        public SKU.Model.UIInterfaces.ISKUUOMView? SelectedUOM { get; set; }
        public List<SKU.Model.UIInterfaces.ISKUUOMView> AllowedUOMs { get; set; }
        public HashSet<string> UsedUOMCodes { get; set; }
        public decimal BasePrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal BasePriceLabel { get; set; }
        public Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>? Attributes { get; set; }
        public ItemState ItemStatus { get; set; }
        public string Remarks { get; set; }
        public bool IsSelected { get; set; }
        public IStoreCheckItemView Clone();
        bool IsItemAVL { get; set; }
        string ItemCode { get; set; }
        QtyCaptureMode QtyCaptureMode { get; set; }
        string Description { get; set; }
        bool IsBackStoreBtnClick { get; set; }
        IItemUOMQuantities StoreQty { get; set; }
        IItemUOMQuantities BackStoreQty { get; set; }
        bool ToggleStatus { get; set; }
        bool ShowQtyDialog { get; set; }
        bool ShowBackStoreQtyDialog { get; set; }
        bool IsStoreDataSubmitted { get; set; }
        bool IsBackStoreDataSubmitted { get; set; }
        bool IsDRESubmitted { get; set; }
        IDamageReturnExpiry DamageReturnExpire { get; set; }
        string ImageUrl { get; set; }
        bool IsFocusSKU { get; set; }

        public Decimal TotalSKUQty { set; get; }
    }
}
