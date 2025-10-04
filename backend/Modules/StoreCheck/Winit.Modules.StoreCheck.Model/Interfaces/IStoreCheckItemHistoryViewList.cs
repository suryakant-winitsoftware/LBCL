using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IStoreCheckItemHistoryViewList
    {
        public int Id { get; set; } 
        public string Uid { get; set; }
        public string StoreCheckHistoryUid { get; set; }
        public string GroupByCode { get; set; }
        public string GroupByValue { get; set; }
        public string SKUName { get; set; }
        public string SKUUID { get; set; }
        public string BaseUOM { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public decimal? SuggestedQty { get; set; }
        public Decimal? StoreQty { get; set; }
        public Decimal? BackStoreQty { get; set; }
        public Decimal? ToFillQty { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDRESelected { get; set; }
        public int? SS { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }
        public ActionType ActionType { get; set; }
        public bool IsRowModified { get; set; }
        public List<IStoreCheckItemExpiryDREHistory> StoreCheckItemExpiryDREHistory { get; set; }
        public List<IStoreCheckItemUomQty> StoreCheckItemUomQty { get; set; }
        public List<SKU.Model.UIInterfaces.ISKUUOMView> AllowedUOMs { get; set; }
        public bool IsBackStoreBtnClick { get; set; }
        public string SKUImageURL { get; set; }
        public IStoreCheckItemUomQty StoreCheckBaseQtyDetails { get; set; }
        public IStoreCheckItemUomQty StoreCheckOuterQtyDetails { get; set; }
        public IItemUOMQuantities StoreQtyDetails { get; set; } 
        public IItemUOMQuantities BackStoreQtyDetails { get; set; }
        public bool IsFocusSKU { get; set; }

        public IStoreCheckItemHistoryViewList Clone();

      

    }
}

