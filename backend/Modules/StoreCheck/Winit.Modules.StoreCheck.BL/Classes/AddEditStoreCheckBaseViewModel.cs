
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.StoreCheck.BL.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.BL.Classes
{
    public class AddEditStoreCheckBaseViewModel : IAddEditStoreCheck
    {

        #region Fields
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public IStoreCheckHistoryView DisplayStoreCheckHistoryView { get; set; }
        public List<IStoreCheckItemHistoryViewList> DisplayStoreCheckItemHistoryListView { get; set; }
        public List<IStoreCheckItemHistoryViewList> FilterStoreCheckItemHistoryListView { get; set; }

        public IStoreCheckItemUomQty DisplayStoreCheckItemUomQty { get; set; }

        public List<IStoreCheckItemExpiryDREHistory> DisplayStoreCheckItemExpiryDreHistory { get; set; }
        public IStoreCheckBL _storeCheckBL { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public IServiceProvider _serviceProvider;
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUMaster> SkuMasterData;
        public IFilterHelper _filter;
        public List<string> _propertiesToSearch = new List<string>();
        public ISortHelper _sorter;
        public IAppUser _appUser;
        public Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;

        public string BeatHistoryUID { get; set; }

        public string StoreHistoryUID { get; set; }

        public StoreCheckModel StorecheckModel = new StoreCheckModel();

        public StoreCheckItemUomQty NewStoreCheckItemUomQty = new StoreCheckItemUomQty();
        public StoreCheckItemExpiryDREHistory NewStoreCheckItemDREHistory = new StoreCheckItemExpiryDREHistory();

        //public StoreCheckGroupHistory StoreCheckGroupHistory = new StoreCheckGroupHistory();


        public QtyCaptureMode QtyMode { get; set; }
        public string BaseUOM { get; set; }
        public List<Winit.Modules.SKU.Model.UIClasses.SKUUOMView> sKUUOMViews { get; set; }

        public IListHelper _listHelper;
        private ISKUBL _ISKUBL { get; set; }

        public List<IStoreCheckItemView> DisplayStoreCheckItemView { get; set; }
        public List<IStoreCheckItemView> FilterStoreCheckItemView { get; set; }
        public bool IsFocusSelected { get; set; }
        public string SearchString { get; set; } = string.Empty;

        #endregion

        #region Constructors

        public AddEditStoreCheckBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IListHelper listHelper,
           IAppUser appUser,
               ISKUBL iSKUBL,
           Winit.Shared.Models.Common.IAppConfig appConfigs,
           Base.BL.ApiService apiService,
           Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
            IStoreCheckBL iStoreCheckBL)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _appUser = appUser;
            _ISKUBL = iSKUBL;
            _appSetting = appSetting;
            DisplayStoreCheckItemView = new List<IStoreCheckItemView>();
            FilterStoreCheckItemView = new List<IStoreCheckItemView>();
            _propertiesToSearch.Add("SKUCode");
            _propertiesToSearch.Add("SKUName");
            QtyMode = QtyCaptureMode.Single;
            StoreHistoryUID = _appUser?.SelectedCustomer?.StoreHistoryUID ?? "";
            BeatHistoryUID = _appUser?.SelectedBeatHistory?.UID ?? "";

        }

        #endregion

        #region Virtual Methods

        public virtual async Task PopulateViewModel(string apiParam = null)
        {
            throw new NotImplementedException();
        }

        public virtual async Task GetStoreCheckHistory(string beatHistoryUID, string storeHistoryUID)
        {
            throw new NotImplementedException();
        }
        public virtual async Task GetStoreCheckItemHistory(string storeCheckHistoryUID)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> CreateUpdateStoreCheck(string beatHistoryUID, string storeHistoryUID)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IStoreCheckItemUomQty> GetStoreCheckItemUomOuterQty(IStoreCheckItemHistoryViewList rowDetails)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<IStoreCheckItemUomQty> GetStoreCheckItemUomBaseQty(IStoreCheckItemHistoryViewList rowDetails)
        {
            throw new NotImplementedException();
        }
        //public virtual async Task<IStoreCheckItemUomQty> GetStoreCheckItemUomQty(IStoreCheckItemHistoryViewList rowDetails)
        //{
        //    throw new NotImplementedException();
        //}
        public virtual async Task<List<IStoreCheckItemExpiryDREHistory>> GetStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<IStoreCheckItemHistoryViewList>> ItemSearch(string searchString)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> CreateStoreCheckItemUomQty(string skuUid, string storeCheckItemHistoryUID)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> CreateStoreCheckItemDREHistory()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Create/UpdateData
        public async Task CreateUpdateStoreCheckHistoryModel(string beatHistoryUID, string storeHistoryUID)
        {

            if (DisplayStoreCheckHistoryView == null)
            {
                await CreateStoreCheckModel(beatHistoryUID, storeHistoryUID);
            }
            else
            {
                await UpdateStoreCheckModel();
            }
        }

        public async Task CreateStoreCheckModel(string beatHistoryUID, string storeHistoryUID)
        {
            await CreateStoreCheckHistory(beatHistoryUID, storeHistoryUID);
            await CreateStoreCheckItemHistory();
        }
        public async Task UpdateStoreCheckModel()
        {
            await UpdateStoreCheckHistory();
            await UpdateStoreCheckItemHistory();
        }

        public async Task CreateStoreCheckHistory(string beatHistoryUID, string storeHistoryUID)
        {
            StorecheckModel.StoreCheckHistoryView = new StoreCheckHistoryView();

            StorecheckModel.StoreCheckHistoryView.Id = 0;
            StorecheckModel.StoreCheckHistoryView.UID = Guid.NewGuid().ToString();
            StorecheckModel.StoreCheckHistoryView.StoreCheckDate = DateTime.Now;
            StorecheckModel.StoreCheckHistoryView.StoreUID = _appUser?.SelectedCustomer?.StoreUID ?? "";
            StorecheckModel.StoreCheckHistoryView.BeatHistoryUID = beatHistoryUID;
            StorecheckModel.StoreCheckHistoryView.StoreHistoryUID = storeHistoryUID;
            StorecheckModel.StoreCheckHistoryView.RouteUID = _appUser?.SelectedRoute?.UID ?? "";
            StorecheckModel.StoreCheckHistoryView.EmpUID = _appUser?.Emp?.UID ?? "";
            StorecheckModel.StoreCheckHistoryView.Level = 1;
            StorecheckModel.StoreCheckHistoryView.IsLastLevel = true;
            StorecheckModel.StoreCheckHistoryView.Status = "Pending";
            StorecheckModel.StoreCheckHistoryView.ActionType = ActionType.Add;
            StorecheckModel.StoreCheckHistoryView.OrgUID = _appUser.SelectedJobPosition.UID;
            StorecheckModel.StoreCheckHistoryView.CreatedTime = DateTime.Now;
            StorecheckModel.StoreCheckHistoryView.ModifiedTime = DateTime.Now;
            StorecheckModel.StoreCheckHistoryView.ServerAddTime = DateTime.Now;
            StorecheckModel.StoreCheckHistoryView.ServerModifiedTime = DateTime.Now;
            StorecheckModel.StoreCheckHistoryView.SS = 1;
            StorecheckModel.StoreCheckHistoryView.Level = 0;
            // StorecheckModel.StoreCheckHistoryView.GroupByName = "";
            // StorecheckModel.StoreCheckHistoryView.StoreCheckConfigUid = "";
            //StorecheckModel.StoreCheckHistoryView.SkuGroupUid = "";
            //StorecheckModel.StoreCheckHistoryView.StoreAssetUid = "";
            //StorecheckModel.StoreCheckHistoryView.SkuGroupTypeUid = "";

        }

        public async Task CreateStoreCheckItemHistory()
        {
            // await SKUOMStaticData();
            StorecheckModel.StoreCheckItemHistoryViewLists = new List<StoreCheckItemHistoryViewList>();
            foreach (var storeCheckItemView in DisplayStoreCheckItemView /*DisplayStoreCheckItemView*/)
            {
                
                var storeCheckItemHistoryView = new StoreCheckItemHistoryViewList
                {

                    Id = 0,
                    Uid = Guid.NewGuid().ToString(),
                    StoreCheckHistoryUid = StorecheckModel.StoreCheckHistoryView.UID,
                    // GroupByCode = " storeCheckItemView.GroupByCode",
                    //GroupByValue = "storeCheckItemView.GroupByValue",
                    ItemCode = storeCheckItemView.SKUCode,
                    SKUUID = storeCheckItemView.SKUUID,
                    UOM = storeCheckItemView.BaseUOM,
                    SuggestedQty = 0,
                    StoreQty = 0,
                    BackStoreQty = 0,
                    // ToFillQty = 0, //SuggestedQty-storeqty-backstoreqty
                    IsAvailable = false, //if selected true then true other wise false if false then store and backstore qty zero
                    IsDRESelected = false,
                    SS = 1,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    ActionType = ActionType.Add
                };

                StorecheckModel.StoreCheckItemHistoryViewLists.Add(storeCheckItemHistoryView);
            }
        }


        public async Task UpdateStoreCheckHistory()
        {

        }

        public async Task UpdateStoreCheckItemHistory()
        {

        }

        //public void CreateStoreCheckGroupHistory()
        //{
        //    StoreCheckGroupHistory.Id = 0;
        //    StoreCheckGroupHistory.Uid = Guid.NewGuid().ToString();
        //    StoreCheckGroupHistory.StoreCheckHistoryUid = StorecheckModel.StoreCheckHistoryView.Uid;
        //    //StoreCheckGroupHistory.GroupByCode = null;
        //    //  StoreCheckGroupHistory.GroupByValue = null;
        //    //StoreCheckGroupHistory.StoreCheckLevel = null;
        //    // StoreCheckGroupHistory.Status = null;
        //    //StoreCheckGroupHistory.Ss = null;
        //    StoreCheckGroupHistory.CreatedTime = DateTime.Now;
        //    StoreCheckGroupHistory.ModifiedTime = DateTime.Now;
        //    StoreCheckGroupHistory.ServerAddTime = DateTime.Now;
        //    StoreCheckGroupHistory.ServerModifiedTime = DateTime.Now;
        //}

        public async Task<IStoreCheckItemUomQty> CreateNewStoreCheckItemUomQty(IStoreCheckItemHistoryViewList rowDetails)
        {

            IStoreCheckItemUomQty storeCheckItemUomQty = new StoreCheckItemUomQty();
            storeCheckItemUomQty.Id = 0;
            storeCheckItemUomQty.UID = System.Guid.NewGuid().ToString();
            storeCheckItemUomQty.StoreCheckItemHistoryUID = rowDetails.Uid;
            storeCheckItemUomQty.UOM = "EA";
            storeCheckItemUomQty.BaseUom = "EA";
            storeCheckItemUomQty.UomMultiplier = 1;
            storeCheckItemUomQty.CreatedTime = DateTime.Now;
            storeCheckItemUomQty.ModifiedTime = DateTime.Now;
            storeCheckItemUomQty.ServerAddTime = DateTime.Now;
            storeCheckItemUomQty.ServerModifiedTime = DateTime.Now;
            storeCheckItemUomQty.ActionType = ActionType.Add;
            storeCheckItemUomQty.IsRowModified = true;
            storeCheckItemUomQty.SS = 1;
            return storeCheckItemUomQty;
        }
        public async Task<IStoreCheckItemUomQty> CreateNewStoreCheckItemOuterUomQty(IStoreCheckItemHistoryViewList rowDetails)
        {

            IStoreCheckItemUomQty storeCheckItemUomQty = new StoreCheckItemUomQty();
            storeCheckItemUomQty.Id = 0;
            storeCheckItemUomQty.UID = System.Guid.NewGuid().ToString();
            storeCheckItemUomQty.StoreCheckItemHistoryUID = rowDetails.Uid;
            storeCheckItemUomQty.UOM = "CASE";
            storeCheckItemUomQty.BaseUom = "EA";
            storeCheckItemUomQty.UomMultiplier = 4;
            storeCheckItemUomQty.CreatedTime = DateTime.Now;
            storeCheckItemUomQty.ModifiedTime = DateTime.Now;
            storeCheckItemUomQty.ServerAddTime = DateTime.Now;
            storeCheckItemUomQty.ServerModifiedTime = DateTime.Now;
            storeCheckItemUomQty.ActionType = ActionType.Add;
            storeCheckItemUomQty.IsRowModified = true;
            storeCheckItemUomQty.SS = 1;
            return storeCheckItemUomQty;
        }
        public void CreateNewStoreCheckItemDREHistory(String storeCheckItemHistoryUid, string skuUid)
        {
            var sKUOMStaticdata = SKUOMStaticdata.FirstOrDefault(item => item.SkuUid == skuUid);
            NewStoreCheckItemDREHistory.Id = 0;
            NewStoreCheckItemDREHistory.StoreCheckItemHistoryUID = storeCheckItemHistoryUid;
            NewStoreCheckItemDREHistory.UID = Guid.NewGuid().ToString();
            //  NewStoreCheckItemDREHistory.Qty = "EA";
            // NewStoreCheckItemDREHistory.ExpiryDate = sKUOMStaticdata.Name;
            // NewStoreCheckItemDREHistory.BatchNo = DateTime.Now;
            // NewStoreCheckItemDREHistory.ExpiryDate = DateTime.Now;
            NewStoreCheckItemDREHistory.SS = 1;
            // NewStoreCheckItemDREHistory.Uom = DateTime.Now;
            //  NewStoreCheckItemDREHistory.StockType = DateTime.Now;
            //  NewStoreCheckItemDREHistory.StockSubType = DateTime.Now;
            NewStoreCheckItemDREHistory.CreatedTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ModifiedTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ServerAddTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ServerModifiedTime = DateTime.Now;
        }

        public void UpdateStoreCheckItemUomQty(String storeCheckItemHistoryUid, string skuUid)
        {
            var sKUOMStaticdata = SKUOMStaticdata.FirstOrDefault(item => item.SkuUid == skuUid);
            NewStoreCheckItemUomQty.Id = 0;
            NewStoreCheckItemUomQty.StoreCheckItemHistoryUID = storeCheckItemHistoryUid;
            NewStoreCheckItemUomQty.UomMultiplier = sKUOMStaticdata.Multiplier;
            NewStoreCheckItemUomQty.UID = Guid.NewGuid().ToString();
            NewStoreCheckItemUomQty.BaseUom = "EA";
            NewStoreCheckItemUomQty.UOM = sKUOMStaticdata.Name;
            NewStoreCheckItemUomQty.CreatedTime = DateTime.Now;
            NewStoreCheckItemUomQty.ModifiedTime = DateTime.Now;
            NewStoreCheckItemUomQty.ServerAddTime = DateTime.Now;
            NewStoreCheckItemUomQty.ServerModifiedTime = DateTime.Now;
            NewStoreCheckItemUomQty.SS = 2;

        }
        public void UpdateStoreCheckItemDREHistory(String storeCheckItemHistoryUid, string skuUid)
        {
            var sKUOMStaticdata = SKUOMStaticdata.FirstOrDefault(item => item.SkuUid == skuUid);
            NewStoreCheckItemDREHistory.Id = 0;
            NewStoreCheckItemDREHistory.StoreCheckItemHistoryUID = storeCheckItemHistoryUid;
            NewStoreCheckItemDREHistory.UID = Guid.NewGuid().ToString();
            //  NewStoreCheckItemDREHistory.Qty = "EA";
            // NewStoreCheckItemDREHistory.ExpiryDate = sKUOMStaticdata.Name;
            // NewStoreCheckItemDREHistory.BatchNo = DateTime.Now;
            // NewStoreCheckItemDREHistory.ExpiryDate = DateTime.Now;
            NewStoreCheckItemDREHistory.SS = 2;
            // NewStoreCheckItemDREHistory.Uom = DateTime.Now;
            //  NewStoreCheckItemDREHistory.StockType = DateTime.Now;
            //  NewStoreCheckItemDREHistory.StockSubType = DateTime.Now;
            NewStoreCheckItemDREHistory.CreatedTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ModifiedTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ServerAddTime = DateTime.Now;
            NewStoreCheckItemDREHistory.ServerModifiedTime = DateTime.Now;
        }
        public List<IStoreCheckItemExpiryDREHistory> CreateStoreCheckItemExpiryDREHistory(IStoreCheckItemHistoryViewList rowDetails)
        {
            return new List<IStoreCheckItemExpiryDREHistory>
        {
            new StoreCheckItemExpiryDREHistory
            {
                Id=0,
                UID=Guid.NewGuid().ToString(),
                StoreCheckItemHistoryUID = rowDetails.Uid,
                StockType =StockType.NonSalable.ToString(),
                StockSubType = StoreCheckConst.Damage,
                Uom = rowDetails.BaseUOM,
                SS = 1,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            },
            new StoreCheckItemExpiryDREHistory
            {
                Id = 0,
                UID=Guid.NewGuid().ToString(),
                StoreCheckItemHistoryUID = rowDetails.Uid,
                 StockType = StockType.Salable.ToString(),
                StockSubType = StoreCheckConst.Expiry,
                 Uom = rowDetails.BaseUOM,
                SS = 1,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            },
            new StoreCheckItemExpiryDREHistory
            {
                Id = 0,
                UID=Guid.NewGuid().ToString(),
                StoreCheckItemHistoryUID = rowDetails.Uid,
                StockType = StockType.Salable.ToString(),
                StockSubType = StoreCheckConst.GoodReturn,
                Uom = rowDetails.BaseUOM,
                SS = 1,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            }
        };
        }
        #endregion

        public List<StoreCheckItem> SKUOMStaticdata = new List<StoreCheckItem>();
        List<StoreCheckItem> uniqueStoreCheckItems = new List<StoreCheckItem>();

        public async Task SKUOMStaticData()
        {

            var storeCheckItems = new List<StoreCheckItem>
           {
            new StoreCheckItem { Uid = "SKU001CASE", SkuUid = "SKU001", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU001EA", SkuUid = "SKU001", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU002CASE", SkuUid = "SKU002", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU002EA", SkuUid = "SKU002", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU003CASE", SkuUid = "SKU003", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU003EA", SkuUid = "SKU003", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU004CASE", SkuUid = "SKU004", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU004EA", SkuUid = "SKU004", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU005CASE", SkuUid = "SKU005", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU005EA", SkuUid = "SKU005", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU006CASE", SkuUid = "SKU006", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU006EA", SkuUid = "SKU006", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU007CASE", SkuUid = "SKU007", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU007EA", SkuUid = "SKU007", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU008CASE", SkuUid = "SKU008", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU008EA", SkuUid = "SKU008", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU009CASE", SkuUid = "SKU009", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU009EA", SkuUid = "SKU009", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU010CASE", SkuUid = "SKU010", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU010EA", SkuUid = "SKU010", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU011CASE", SkuUid = "SKU011", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU011EA", SkuUid = "SKU011", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU012CASE", SkuUid = "SKU012", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU012EA", SkuUid = "SKU012", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU013CASE", SkuUid = "SKU013", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU013EA", SkuUid = "SKU013", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU014CASE", SkuUid = "SKU014", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU014EA", SkuUid = "SKU014", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU015CASE", SkuUid = "SKU015", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU015EA", SkuUid = "SKU015", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU016CASE", SkuUid = "SKU016", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU016EA", SkuUid = "SKU016", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU017CASE", SkuUid = "SKU017", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU017EA", SkuUid = "SKU017", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU018CASE", SkuUid = "SKU018", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU018EA", SkuUid = "SKU018", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU019CASE", SkuUid = "SKU019", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU019EA", SkuUid = "SKU019", Code = "EA", Name = "EA", Multiplier = 1 },
            new StoreCheckItem { Uid = "SKU020CASE", SkuUid = "SKU020", Code = "CASE", Name = "CASE", Multiplier = 10 },
            new StoreCheckItem { Uid = "SKU020EA", SkuUid = "SKU020", Code = "EA", Name = "EA", Multiplier = 1 }
            };
            SKUOMStaticdata.AddRange(storeCheckItems);
            HashSet<string> uniqueSkuUids = new HashSet<string>();

            foreach (var item in storeCheckItems)
            {
                if (uniqueSkuUids.Add(item.SkuUid))
                {
                    uniqueStoreCheckItems.Add(item);
                }
            }
        }
    }
}
