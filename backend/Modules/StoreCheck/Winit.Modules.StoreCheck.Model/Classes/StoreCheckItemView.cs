using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckItemView: IStoreCheckItemView
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
        public string? SKUPriceUID { get; set; }
        public Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>? Attributes { get; set; }
        public ItemState ItemStatus { get; set; }
        public string Remarks { get; set; }
        public bool IsSelected { get; set; }
        public StoreCheckItemView()
        {
            AllowedUOMs = new List<SKU.Model.UIInterfaces.ISKUUOMView>();
            Attributes = new Dictionary<string, SKU.Model.UIInterfaces.ISKUAttributeView>();
            UsedUOMCodes = new HashSet<string>();
        }

        
            public IStoreCheckItemView Clone()
            {
                return new StoreCheckItemView
                {
                    UOMQty = this.UOMQty,
                    Id = this.Id,
                    UID = this.UID,
                    SalesOrderLineUID = this.SalesOrderLineUID,
                    IsCartItem = this.IsCartItem,
                    ItemType = this.ItemType,
                    LineNumber = this.LineNumber,
                    SKUImage = this.SKUImage,
                    SKUUID = this.SKUUID,
                    SKUCode = this.SKUCode,
                    SKUName = this.SKUName,
                    SKULabel = this.SKULabel,
                    OrderUOMMultiplier = this.OrderUOMMultiplier,
                    BaseUOM = this.BaseUOM,
                    SelectedUOM = this.SelectedUOM, // Assuming this is a reference type, you might want to clone this as well if it's mutable
                    AllowedUOMs = new List<SKU.Model.UIInterfaces.ISKUUOMView>(this.AllowedUOMs), // Clone the list
                    UsedUOMCodes = new HashSet<string>(this.UsedUOMCodes), // Clone the set
                    BasePrice = this.BasePrice,
                    UnitPrice = this.UnitPrice,
                    BasePriceLabel = this.BasePriceLabel,
                    SKUPriceUID = this.SKUPriceUID,
                    Attributes = this.Attributes.ToDictionary(entry => entry.Key, entry => entry.Value), // Deep copy of dictionary
                    ItemStatus = this.ItemStatus,
                    Remarks = this.Remarks,
                    IsSelected = this.IsSelected,
                    IsItemAVL = this.IsItemAVL,
                    ItemCode = this.ItemCode,
                    QtyCaptureMode = this.QtyCaptureMode,
                    Description = this.Description,
                    IsBackStoreBtnClick = this.IsBackStoreBtnClick,
                    StoreQty = new ItemUOMQuantities
                    {
                        TotalQty = this.StoreQty.TotalQty,
                        pcQty = this.StoreQty.pcQty,
                        CaseQty = this.StoreQty.CaseQty,
                    },
                    BackStoreQty = new ItemUOMQuantities
                    {
                        TotalQty = this.StoreQty.TotalQty,
                        pcQty = this.StoreQty.pcQty,
                        CaseQty = this.StoreQty.CaseQty,
                    },
                    ToggleStatus = this.ToggleStatus,
                    ShowQtyDialog = this.ShowQtyDialog,
                    ShowBackStoreQtyDialog = this.ShowBackStoreQtyDialog,
                    IsStoreDataSubmitted = this.IsStoreDataSubmitted,
                    IsBackStoreDataSubmitted = this.IsBackStoreDataSubmitted,
                    IsDRESubmitted = this.IsDRESubmitted,
                    DamageReturnExpire = new DamageReturnExpiry
                    {
                        DamageQty = this.DamageReturnExpire.DamageQty,
                        DamageNotes = this.DamageReturnExpire.DamageNotes,
                        DamageExpiryDate = this.DamageReturnExpire.DamageExpiryDate,
                        ExpireQty = this.DamageReturnExpire.ExpireQty,
                        ExpireNotes = this.DamageReturnExpire.ExpireNotes,
                        ExpiryDate = this.DamageReturnExpire.ExpiryDate,
                        ReturnQty = this.DamageReturnExpire.ReturnQty,
                        ReturnNotes = this.DamageReturnExpire.ReturnNotes,
                        GoodReturnExpiryDate = this.DamageReturnExpire.GoodReturnExpiryDate,
                        ReturenReason = this.DamageReturnExpire.ReturenReason
                    },
                    ImageUrl = this.ImageUrl,
                    IsFocusSKU = this.IsFocusSKU,
                    TotalSKUQty = this.TotalSKUQty
                };
            }
        
        public bool IsItemAVL { get; set; } = false;
        public string ItemCode { get; set; }
        public QtyCaptureMode QtyCaptureMode { get; set; }
        public string Description { get; set; }
        public bool IsBackStoreBtnClick { get; set; } = false;
        public IItemUOMQuantities StoreQty { get; set; } = new ItemUOMQuantities();
        public IItemUOMQuantities BackStoreQty { get; set; } = new ItemUOMQuantities();
        public bool ToggleStatus { get; set; }
        public bool ShowQtyDialog { get; set; } = false;
        public bool ShowBackStoreQtyDialog { get; set; }
        public bool IsStoreDataSubmitted { get; set; }
        public bool IsBackStoreDataSubmitted { get; set; }
        public bool IsDRESubmitted { get; set; }
        public IDamageReturnExpiry DamageReturnExpire { get; set; } = new DamageReturnExpiry();
        public string ImageUrl { get; set; }
        public bool IsFocusSKU { get; set; }

        public Decimal TotalSKUQty { set; get; }
    }
}
