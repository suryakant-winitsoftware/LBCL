using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Classes;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.StoreCheck.BL.Classes
{
    public class AddEditStoreCheckAppViewModel: AddEditStoreCheckBaseViewModel
    {
        public ISKUBL _ISKUBL { get; set; }
        Winit.Modules.FileSys.BL.Interfaces.IFileSysBL _fileSysBL { set; get; }

        List<string> orgUIDs = new List<string>();
        List<string> DistributionChannelUIDs = new List<string>();

        List<string> skuUIDs = new List<string>();
        List<string> attributeTypes = new List<string>();

        public AddEditStoreCheckAppViewModel(IServiceProvider serviceProvider,
                    IFilterHelper filter,
                    ISortHelper sorter,
                    IListHelper listHelper,
                    IAppUser appUser,
                        ISKUBL iSKUBL,
                    Winit.Shared.Models.Common.IAppConfig appConfigs,
                    Base.BL.ApiService apiService,
                  Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
              Winit.Modules.StoreCheck.BL.Interfaces.IStoreCheckBL iStoreCheckBL,
              FileSys.BL.Interfaces.IFileSysBL fileSysBL) : base(serviceProvider, filter, sorter, listHelper, appUser, iSKUBL, appConfigs, apiService, appSetting, iStoreCheckBL)
        {
            _ISKUBL = iSKUBL;
            _storeCheckBL = iStoreCheckBL;
            _fileSysBL = fileSysBL;
        }

        #region Uploading Page

        public bool firstObjectcreate = true;

        public override async Task PopulateViewModel(string apiParam=null)
        {
            await GetSKUMasterData();
        }
        public  async Task GetSKUMasterData()
        {
            if (firstObjectcreate)
            {
                await FirstObjectCreate();
                firstObjectcreate = false;
            }
            if (SkuMasterData != null)
            {
                DisplayStoreCheckItemView = new List<IStoreCheckItemView>();
                FilterStoreCheckItemView = new List<IStoreCheckItemView>();
                List<IStoreCheckItemView> storeCheckItemView = ConvertToISalesOrderItemView(SkuMasterData);
                if (storeCheckItemView != null)
                {
                    DisplayStoreCheckItemView.AddRange(storeCheckItemView);
                    BaseUOM = DisplayStoreCheckItemView?.FirstOrDefault()?.BaseUOM?? "";
                   // var displayFocleStockAudit = await ItemSearch("", storeCheckItemView);
                    //DisplayStoreCheckItemView.AddRange(displayFocleStockAudit);
                }
                
            }
        }
        public async Task FirstObjectCreate()
        {
            try
            {

                SkuMasterData = await _ISKUBL.PrepareSKUMaster(
                        orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes);

                // Filter SKUs based on AllowedSKUs if the list exists
                if (_appUser?.SelectedCustomer?.AllowedSKUs != null && _appUser.SelectedCustomer.AllowedSKUs.Any())
                {
                    SkuMasterData = SkuMasterData.Where(sku => _appUser.SelectedCustomer.AllowedSKUs.Contains(sku.SKU.UID)).ToList();
                }

                List<IStoreCheckItemView> salesOrderItemView = ConvertToISalesOrderItemView(SkuMasterData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public List<IStoreCheckItemView> ConvertToISalesOrderItemView(List<ISKUMaster> sKUMasters)
        {
            List<IStoreCheckItemView> salesOrderItems = null;
            if (sKUMasters != null && sKUMasters.Count > 0)
            {
                salesOrderItems = new List<IStoreCheckItemView>();
                int lineNumber = 1;
                foreach (var sKUMaster in sKUMasters)
                {
                    salesOrderItems.Add(ConvertToISalesOrderItemView(sKUMaster, lineNumber));
                    lineNumber++;
                }
            }

            return salesOrderItems;
        }

        public virtual IStoreCheckItemView ConvertToISalesOrderItemView(ISKUMaster sKUMaster, int lineNumber)
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.FirstOrDefault();
            List<ISKUUOMView>? sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
            ISKUUOMView? defaultUOM = sKUUOMViews
                            .FirstOrDefault(e => e.Code == sKUConfig?.SellingUOM);
            ISKUUOMView? baseUOM = sKUUOMViews
                            .FirstOrDefault(e => e.IsBaseUOM);
            IStoreCheckItemView salesOrderItem = new StoreCheckItemView
            {
                UID = Guid.NewGuid().ToString(),
                LineNumber = lineNumber,
                SKUImage = sKUMaster.SKU.SKUImage,
                SKUUID = sKUMaster.SKU.UID,
                SKUCode = sKUMaster.SKU.Code,
                SKUName = sKUMaster.SKU.Name,
                SKULabel = sKUMaster.SKU.Name,
                IsFocusSKU = sKUMaster.SKU.IsFocusSKU,
                //IsMCL = false,
                //IsPromo = false,
                //IsNPD = false,
                //SCQty = 0,
                //RCQty = 0,
                //VanQty = 0,
                //MaxQty = DefaultMaxQty,
                //OrderQty = 0,
                //Qty = 0,
                //QtyBU = 0,
                //DeliveredQty = 0,
                //MissedQty = 0,
                BaseUOM = baseUOM?.Code,
                SelectedUOM = defaultUOM,
                AllowedUOMs = sKUUOMViews,
                //UsedUOMCodes = new List<string>(),
                BasePrice = 0,
                UnitPrice = 0,
                IsItemAVL = true,
                //IsTaxable = SelectedStoreViewModel.IsTaxApplicable,
                //ApplicableTaxes = sKUMaster.ApplicableTaxUIDs,
                //TotalAmount = 0,
                //TotalLineDiscount = 0,
                //TotalCashDiscount = 0,
                //TotalHeaderDiscount = 0,
                //TotalExciseDuty = 0,
                //TotalLineTax = 0,
                //TotalHeaderTax = 0,
                //SKUPriceUID = null,
                //SKUPriceListUID = null,
                Attributes = ConvertToISKUAttributeView(sKUMaster.SKUAttributes),
                ItemStatus = ItemState.Primary,
                //ApplicationPromotionUIDs = null,
                //PromotionUID = string.Empty,
                //CurrencyLabel = this.CurrencyLabel
            };
            return salesOrderItem;
        }

        public List<ISKUUOMView> ConvertToISKUUOMView(List<ISKUUOM> sKUUOMs)
        {
            List<ISKUUOMView> sKUUOMViews = null;
            if (sKUUOMs != null)
            {
                sKUUOMViews = new List<ISKUUOMView>();
                foreach (ISKUUOM sKUUOM in sKUUOMs)
                {
                    sKUUOMViews.Add(ConvertToISKUUOMView(sKUUOM));
                }
            }
            return sKUUOMViews;
        }
        public virtual ISKUUOMView ConvertToISKUUOMView(ISKUUOM sKUUOM)
        {
            ISKUUOMView sKUUOMView = _serviceProvider.CreateInstance<ISKUUOMView>();
            sKUUOMView.SKUUID = sKUUOM.SKUUID;
            sKUUOMView.Code = sKUUOM.Code;
            sKUUOMView.Name = sKUUOM.Name;
            sKUUOMView.Label = sKUUOM.Label;
            sKUUOMView.Barcode = sKUUOM.Barcodes;
            sKUUOMView.IsBaseUOM = sKUUOM.IsBaseUOM;
            sKUUOMView.IsOuterUOM = sKUUOM.IsOuterUOM;
            sKUUOMView.Multiplier = sKUUOM.Multiplier;
            return sKUUOMView;
        }
        public virtual ISKUAttributeView ConvertToISKUAttributeView(ISKUAttributes sKUAttribute)
        {
            ISKUAttributeView sKUAttributeView = _serviceProvider.CreateInstance<ISKUAttributeView>();
            sKUAttributeView.SKUUID = sKUAttribute.SKUUID;
            sKUAttributeView.Name = sKUAttribute.Type;
            sKUAttributeView.Code = sKUAttribute.Code;
            sKUAttributeView.Value = sKUAttribute.Value;
            return sKUAttributeView;
        }
        public virtual Dictionary<string, ISKUAttributeView> ConvertToISKUAttributeView(List<ISKUAttributes> sKUAttributes)
        {
            Dictionary<string, ISKUAttributeView> ISKUAttributeViews = null;
            if (sKUAttributes != null)
            {
                ISKUAttributeViews = new Dictionary<string, ISKUAttributeView>();
                foreach (ISKUAttributes skuAttribute in sKUAttributes)
                {
                    string key = skuAttribute.Type;
                    ISKUAttributeViews[key] = ConvertToISKUAttributeView(skuAttribute);
                }
            }
            return ISKUAttributeViews;
        }

        #endregion

        #region Fetching Data from SQLite
        public override async Task GetStoreCheckHistory(string beatHistoryUID, string storeHistoryUID)
        {

            try
            {

                if (_storeCheckBL != null)
                {
                    DisplayStoreCheckHistoryView = await _storeCheckBL.SelectStoreCheckHistoryData(beatHistoryUID, storeHistoryUID);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override async Task GetStoreCheckItemHistory(string storeCheckHistoryUID)
        {

            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                if (_storeCheckBL != null)
                {
                    var pagedResponse = await _storeCheckBL.SelectStoreCheckItemHistoryData(
                        pagingRequest?.SortCriterias,
                        pagingRequest?.PageNumber ?? 1,
                        pagingRequest?.PageSize ?? 10,
                        pagingRequest?.FilterCriterias,
                        pagingRequest?.IsCountRequired ?? false,
                        storeCheckHistoryUID
                    );
                    if (pagedResponse.PagedData != null)
                    {
                        DisplayStoreCheckItemHistoryListView = PrepreStoreCheckViewData(pagedResponse.PagedData.ToList());
                        if(DisplayStoreCheckItemHistoryListView != null)
                        {
                            FilterStoreCheckItemHistoryListView = DisplayStoreCheckItemHistoryListView
                            .Select(item => item.Clone() as IStoreCheckItemHistoryViewList)
                            .ToList();
                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public List<IStoreCheckItemHistoryViewList> PrepreStoreCheckViewData(List<IStoreCheckItemHistoryViewList> storeCheckitemHistoryViewLists)
        {
            var storeCheckItemHistoryViewLists = new List<IStoreCheckItemHistoryViewList>();
            foreach (var storeCheckItemHistoryViewList in storeCheckitemHistoryViewLists)
            {
                var matchSKUDetails = DisplayStoreCheckItemView.FirstOrDefault(item => item.SKUUID == storeCheckItemHistoryViewList.SKUUID);
                var StoreCheckItemHistoryViewList = new StoreCheckItemHistoryViewList
                {

                    StoreCheckHistoryUid= storeCheckItemHistoryViewList.StoreCheckHistoryUid,
                    UOM= storeCheckItemHistoryViewList.UOM,
                    Uid = storeCheckItemHistoryViewList.Uid,
                    SKUUID = storeCheckItemHistoryViewList.SKUUID,
                    ItemCode = storeCheckItemHistoryViewList.ItemCode,
                    SKUName =/*"",*/ matchSKUDetails.SKUName,
                    BackStoreQty = storeCheckItemHistoryViewList.BackStoreQty,
                    StoreQty = storeCheckItemHistoryViewList.StoreQty,
                    IsAvailable = true, //storeCheckItemHistoryViewList.IsAvailable, made default true for Farmely
                    IsDRESelected = storeCheckItemHistoryViewList.IsDRESelected,
                    SuggestedQty = storeCheckItemHistoryViewList.SuggestedQty ?? 0,
                    ToFillQty = storeCheckItemHistoryViewList.ToFillQty,
                    GroupByCode = storeCheckItemHistoryViewList.GroupByCode,
                    GroupByValue = storeCheckItemHistoryViewList.GroupByValue,
                    ServerModifiedTime= storeCheckItemHistoryViewList.ServerModifiedTime,
                    ModifiedTime= storeCheckItemHistoryViewList.ModifiedTime,
                    AllowedUOMs = matchSKUDetails.AllowedUOMs,
                    BaseUOM =/*"", */ matchSKUDetails.BaseUOM,
                    SKUImageURL=/* "",*/ matchSKUDetails.SKUImage,
                    IsFocusSKU = matchSKUDetails.IsFocusSKU
                };
                storeCheckItemHistoryViewLists.Add(StoreCheckItemHistoryViewList);
            }
            return storeCheckItemHistoryViewLists;
        }
        public override async Task<IStoreCheckItemUomQty> GetStoreCheckItemUomBaseQty(IStoreCheckItemHistoryViewList rowDetails) 
        {
            try
            {

                if (_storeCheckBL != null)
                {
                    
                        var storeCheckItemUomQty = await _storeCheckBL.SelectStoreCheckItemUomQty(rowDetails.Uid, "EA");

                    if (storeCheckItemUomQty == null)
                    {
                        return await CreateNewStoreCheckItemUomQty(rowDetails);
                    }

                    else
                    {
                        return storeCheckItemUomQty;
                    }
                }
                else
                {
                    Console.WriteLine("Buisness layer object have null value");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public override async Task<IStoreCheckItemUomQty> GetStoreCheckItemUomOuterQty(IStoreCheckItemHistoryViewList rowDetails)
        {
            try
            {

                if (_storeCheckBL != null)
                {

                    var storeCheckItemUomQty = await _storeCheckBL.SelectStoreCheckItemUomQty(rowDetails.Uid, "CASE");

                    if (storeCheckItemUomQty == null)
                    {
                        return await CreateNewStoreCheckItemOuterUomQty(rowDetails);
                    }

                    else
                    {
                        return storeCheckItemUomQty;
                    }
                }
                else
                {
                    Console.WriteLine("Buisness layer object have null value");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public override async Task<List<IStoreCheckItemExpiryDREHistory>> GetStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID)
        {
            try
            {

                if (_storeCheckBL != null)
                {
                     var storeCheckItemExpiryDreHistory = (await _storeCheckBL.SelectStoreCheckItemExpiryDREHistory(storeCheckItemHistoryUID)).ToList();
                    return storeCheckItemExpiryDreHistory;
                }
                else
                {
                    return null;
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

      


        #endregion

        #region CreateData


       
        public override async Task<bool> CreateUpdateStoreCheck(string beatHistoryUID, string storeHistoryUID)
        {

            try
            {
                StoreCheckModel storeCheckModel = new StoreCheckModel();
                if(DisplayStoreCheckHistoryView==null)
                {
                    await CreateUpdateStoreCheckHistoryModel(beatHistoryUID, storeHistoryUID);
                    storeCheckModel = StorecheckModel;
                }
                else
                {
                    DisplayStoreCheckHistoryView.Status = "Completed";
                    DisplayStoreCheckHistoryView.ModifiedTime = DateTime.Now;
                    DisplayStoreCheckHistoryView.ServerModifiedTime = DateTime.Now;
                    storeCheckModel.StoreCheckHistoryView = (StoreCheckHistoryView)DisplayStoreCheckHistoryView;
                    storeCheckModel.StoreCheckItemHistoryViewLists = (DisplayStoreCheckItemHistoryListView.Where(item=>item.IsRowModified)).Cast<StoreCheckItemHistoryViewList>().ToList();
                     
                }

                var retVal = await _storeCheckBL.CreateUpdateStoreCheckHistory(storeCheckModel);
                if (retVal > 0 && ImageFileSysList != null && ImageFileSysList.Any())
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(DisplayStoreCheckHistoryView?.UID))
                        {
                            ImageFileSysList?.ForEach(e => e.LinkedItemUID = $"{DisplayStoreCheckHistoryView?.UID}");
                        }
                        await SaveCapturedImagesAsync();
                    return true;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error while saving captured images: {ex.Message}");
                        throw;
                    }
                }

                return retVal > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        private async Task SaveCapturedImagesAsync()
        {
            // Save the images in bulk using the provided file system logic
            int saveResult = await _fileSysBL.CreateFileSysForBulk(ImageFileSysList);

            // Throw an exception if the image save operation failed
            if (saveResult < 0)
            {
                throw new Exception("Failed to save the captured images.");
            }
        }
        public override async Task<bool> CreateStoreCheckItemUomQty(string storeCheckItemHistoryUID,string skuUid)
        {

            try
            {
                StoreCheckItemUomQty storeCheckItemUomQty = new StoreCheckItemUomQty();
                if (DisplayStoreCheckItemUomQty == null)
                {
                    //CreateNewStoreCheckItemUomQty(storeCheckItemHistoryUID,skuUid);
                    storeCheckItemUomQty = NewStoreCheckItemUomQty;
                }
                else
                {
                  //  UpdateStoreCheckItemUomQty();
                }

                var retVal = await _storeCheckBL.CreateUpdateStoreCheckUomQty(storeCheckItemUomQty);
                if (retVal > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        //public override async Task<bool> CreateUpdateStoreCheckItemDREHistory(IStoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory)
        //{

        //    try
        //    {
        //        StoreCheckItemExpiryDREHistory newStoreCheckItemDREHistory = new StoreCheckItemExpiryDREHistory();
        //        if (DisplayStoreCheckItemUomQty == null)
        //        {
        //            CreateNewStoreCheckItemDREHistory(storeCheckItemHistoryUID,skuUid);
        //            newStoreCheckItemDREHistory = NewStoreCheckItemDREHistory;
        //        }
        //        else
        //        {
        //            //  UpdateStoreCheckItemUomQty();
        //        }

        //        var retVal = await _storeCheckBL.CreateUpdateStoreCheckExpireDreHistory(newStoreCheckItemDREHistory);
        //        if (retVal > 0)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return false;
        //    }
        //}
        //public override async Task<bool> CreateStoreCheckItemDREHistory(string storeCheckItemHistoryUID, string skuUid)
        //{

        //    try
        //    {
        //        StoreCheckItemExpiryDREHistory newStoreCheckItemDREHistory = new StoreCheckItemExpiryDREHistory();
        //        if (DisplayStoreCheckItemUomQty == null)
        //        {
        //            CreateNewStoreCheckItemDREHistory(storeCheckItemHistoryUID,skuUid);
        //            newStoreCheckItemDREHistory = NewStoreCheckItemDREHistory;
        //        }
        //        else
        //        {
        //            //  UpdateStoreCheckItemUomQty();
        //        }

        //        var retVal = await _storeCheckBL.CreateUpdateStoreCheckExpireDreHistory(newStoreCheckItemDREHistory);
        //        if (retVal > 0)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return false;
        //    }
        //}

        #endregion
        #region Search/Filter Logics
        public override async Task<List<IStoreCheckItemHistoryViewList>> ItemSearch(string searchString)
        {
            try
            {
                if (_filter != null)
                {
                    SearchString = searchString;
                    List<IStoreCheckItemHistoryViewList> applicableStoreCheckItemHistoryListViewl;
                    if (IsFocusSelected)
                    {
                        applicableStoreCheckItemHistoryListViewl = FilterStoreCheckItemHistoryListView.Where(e => e.IsFocusSKU)
                            .ToList();
                    }
                    else
                    {
                        applicableStoreCheckItemHistoryListViewl = FilterStoreCheckItemHistoryListView;
                    }
                        return await _filter.ApplySearch<IStoreCheckItemHistoryViewList>(
                                applicableStoreCheckItemHistoryListViewl, searchString, _propertiesToSearch);
                }
                // Add a default return statement here
                return new List<IStoreCheckItemHistoryViewList>(); // or null, depending on your logic
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Handle the exception appropriately, and return a value
                return new List<IStoreCheckItemHistoryViewList>(); // or null, depending on your logic
            }
        }
        #endregion
    }
}
