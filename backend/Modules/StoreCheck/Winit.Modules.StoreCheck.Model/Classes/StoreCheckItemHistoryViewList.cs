using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckItemHistoryViewList : IStoreCheckItemHistoryViewList
    {
        public int Id { get; set; }
        public string Uid { get; set; }
        public string StoreCheckHistoryUid { get; set; }
        public string GroupByCode { get; set; }
        public string GroupByValue { get; set; }
        public string SKUName { get; set; }
        public string BaseUOM { get; set; }
        public string SKUUID { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public decimal? SuggestedQty { get; set; }
        public decimal? StoreQty { get; set; }
        public decimal? BackStoreQty { get; set; }
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
        public List<IStoreCheckItemExpiryDREHistory> StoreCheckItemExpiryDREHistory { get; set; } = new List<IStoreCheckItemExpiryDREHistory>();

        public List<IStoreCheckItemUomQty> StoreCheckItemUomQty { get; set; }

        public List<SKU.Model.UIInterfaces.ISKUUOMView> AllowedUOMs { get; set; }

        public bool IsBackStoreBtnClick { get; set; }

        public string SKUImageURL { get; set; }
        public IStoreCheckItemUomQty StoreCheckBaseQtyDetails { get; set; } = new StoreCheckItemUomQty();

        public IStoreCheckItemUomQty StoreCheckOuterQtyDetails { get; set; } = new StoreCheckItemUomQty();

        public IItemUOMQuantities StoreQtyDetails { get; set; } = new ItemUOMQuantities();
        public IItemUOMQuantities BackStoreQtyDetails { get; set; } = new ItemUOMQuantities();
        public bool IsFocusSKU { get; set; }
        public IStoreCheckItemHistoryViewList Clone()
        {
            return new StoreCheckItemHistoryViewList
            {
                UOM = this.UOM,
                Id = this.Id,
                Uid = this.Uid,
                SKUImageURL = this.SKUImageURL,
                SKUUID = this.SKUUID,
                ItemCode = this.ItemCode,
                SKUName = this.SKUName,
                BaseUOM = this.BaseUOM,
                AllowedUOMs = this.AllowedUOMs != null ? new List<SKU.Model.UIInterfaces.ISKUUOMView>(this.AllowedUOMs) : new List<SKU.Model.UIInterfaces.ISKUUOMView>(), // Clone the list or initialize
                //StoreCheckItemExpiryDREHistory = this.StoreCheckItemExpiryDREHistory != null ? new List<IStoreCheckItemExpiryDREHistory>(this.StoreCheckItemExpiryDREHistory) : new List<IStoreCheckItemExpiryDREHistory>(), // Clone the list or initialize
                //StoreCheckItemUomQty = this.StoreCheckItemUomQty != null ? new List<IStoreCheckItemUomQty>(this.StoreCheckItemUomQty) : new List<IStoreCheckItemUomQty>(), // Clone the list or initialize
                StoreCheckItemUomQty = this.StoreCheckItemUomQty?.Select(uomQty => new StoreCheckItemUomQty
                {
                    BackStoreQty= uomQty.BackStoreQty,
                    StoreQty= uomQty.StoreQty,
                    StoreQtyBu= uomQty.StoreQtyBu,
                    BackStoreQtyBu= uomQty.BackStoreQtyBu,
                    BaseUom= uomQty.BaseUom,
                    IsBaseUOMTaken= uomQty.IsBaseUOMTaken,
                    IsOuterUOMTaken= uomQty.IsOuterUOMTaken,
                    UOM= uomQty.UOM,
                    UomMultiplier= uomQty.UomMultiplier,
                    Id = uomQty.Id,
                    UID = uomQty.UID,
                    StoreCheckItemHistoryUID = uomQty.StoreCheckItemHistoryUID,
                    SS = uomQty.SS,
                    CreatedTime = uomQty.CreatedTime,
                    ModifiedTime = uomQty.ModifiedTime,
                    ServerAddTime = uomQty.ServerAddTime,
                    ServerModifiedTime = uomQty.ServerModifiedTime,
                    IsRowModified = uomQty.IsRowModified,
                    ActionType = uomQty.ActionType
                }).Cast<IStoreCheckItemUomQty>().ToList(),

                IsBackStoreBtnClick = this.IsBackStoreBtnClick,
                IsRowModified = this.IsRowModified,
                SS = this.SS,
                IsDRESelected = this.IsDRESelected,
                IsAvailable = this.IsAvailable,
                StoreCheckHistoryUid = this.StoreCheckHistoryUid,
                CreatedTime = this.CreatedTime,
                ModifiedTime = this.ModifiedTime,
                ServerAddTime = this.ServerAddTime,
                ServerModifiedTime = this.ServerModifiedTime,
                ActionType = this.ActionType,
                StoreQty = this.StoreQty,
                BackStoreQty = this.BackStoreQty,
                GroupByCode = this.GroupByCode,
                GroupByValue = this.GroupByValue,
                SuggestedQty = this.SuggestedQty,
                ToFillQty = this.ToFillQty,
                StoreCheckBaseQtyDetails = new StoreCheckItemUomQty
                {
                    UID = this.StoreCheckBaseQtyDetails.UID,
                    StoreCheckItemHistoryUID = this.StoreCheckBaseQtyDetails.StoreCheckItemHistoryUID,
                    UOM = this.StoreCheckBaseQtyDetails.UOM,
                    BaseUom = this.StoreCheckBaseQtyDetails.BaseUom,
                    UomMultiplier = this.StoreCheckBaseQtyDetails.UomMultiplier,
                    StoreQty = this.StoreCheckBaseQtyDetails.StoreQty,
                    StoreQtyBu = this.StoreCheckBaseQtyDetails.StoreQtyBu,
                    BackStoreQty = this.StoreCheckBaseQtyDetails.BackStoreQty,
                    BackStoreQtyBu = this.StoreCheckBaseQtyDetails.BackStoreQtyBu,
                    SS = this.StoreCheckBaseQtyDetails.SS,
                    CreatedTime = this.StoreCheckBaseQtyDetails.CreatedTime,
                    ModifiedTime = this.StoreCheckBaseQtyDetails.ModifiedTime,
                    ServerAddTime = this.StoreCheckBaseQtyDetails.ServerAddTime,
                    ServerModifiedTime = this.StoreCheckBaseQtyDetails.ServerModifiedTime,
                    ActionType = this.StoreCheckBaseQtyDetails.ActionType,
                    IsBaseUOMTaken = this.StoreCheckBaseQtyDetails.IsBaseUOMTaken,
                    IsOuterUOMTaken = this.StoreCheckBaseQtyDetails.IsOuterUOMTaken,
                    IsRowModified = this.StoreCheckBaseQtyDetails.IsRowModified
                },
                 StoreCheckOuterQtyDetails = new StoreCheckItemUomQty
                {
                     UID = this.StoreCheckOuterQtyDetails.UID,
                     StoreCheckItemHistoryUID = this.StoreCheckOuterQtyDetails.StoreCheckItemHistoryUID,
                     UOM = this.StoreCheckOuterQtyDetails.UOM,
                     BaseUom = this.StoreCheckOuterQtyDetails.BaseUom,
                     UomMultiplier = this.StoreCheckOuterQtyDetails.UomMultiplier,
                     StoreQty = this.StoreCheckOuterQtyDetails.StoreQty,
                     StoreQtyBu = this.StoreCheckOuterQtyDetails.StoreQtyBu,
                     BackStoreQty = this.StoreCheckOuterQtyDetails.BackStoreQty,
                     BackStoreQtyBu = this.StoreCheckOuterQtyDetails.BackStoreQtyBu,
                     SS = this.StoreCheckOuterQtyDetails.SS,
                     CreatedTime = this.StoreCheckOuterQtyDetails.CreatedTime,
                     ModifiedTime = this.StoreCheckOuterQtyDetails.ModifiedTime,
                     ServerAddTime = this.StoreCheckOuterQtyDetails.ServerAddTime,
                     ServerModifiedTime = this.StoreCheckOuterQtyDetails.ServerModifiedTime,
                     ActionType = this.StoreCheckOuterQtyDetails.ActionType,
                     IsBaseUOMTaken = this.StoreCheckOuterQtyDetails.IsBaseUOMTaken,
                     IsOuterUOMTaken = this.StoreCheckOuterQtyDetails.IsOuterUOMTaken,
                     IsRowModified = this.StoreCheckOuterQtyDetails.IsRowModified
                 },
                StoreCheckItemExpiryDREHistory = this.StoreCheckItemExpiryDREHistory?.Select(dre => new StoreCheckItemExpiryDREHistory
                {
                    Id = dre.Id,
                    UID = dre.UID,
                    StoreCheckItemHistoryUID = dre.StoreCheckItemHistoryUID,
                    StockType = dre.StockType,
                    StockSubType = dre.StockSubType,
                    Notes = dre.Notes,
                    BatchNo = dre.BatchNo,
                    ExpiryDate = dre.ExpiryDate,
                    Reason = dre.Reason, // Uncomment this line if Reason is of type ISelectionItem
                    Qty = dre.Qty,
                    Uom = dre.Uom,
                    SS = dre.SS,
                    CreatedTime = dre.CreatedTime,
                    ModifiedTime = dre.ModifiedTime,
                    ServerAddTime = dre.ServerAddTime,
                    ServerModifiedTime = dre.ServerModifiedTime,
                    IsRowModified=dre.IsRowModified,
                    ActionType = dre.ActionType
                }).Cast<IStoreCheckItemExpiryDREHistory>().ToList(),
            };

        }
        public StoreCheckItemHistoryViewList()
        {
            StoreQty = null;
            BackStoreQty = null;
        }
    }

}


