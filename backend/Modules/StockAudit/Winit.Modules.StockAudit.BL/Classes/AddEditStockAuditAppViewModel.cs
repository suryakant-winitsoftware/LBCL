using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.StockAudit.BL.Interfaces;
using Winit.Modules.StockAudit.DL.Interfaces;
using Winit.Modules.StockAudit.Model.Classes;
using Winit.Modules.StockAudit.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;



namespace Winit.Modules.StockAudit.BL.Classes
{
    public class AddEditStockAuditAppViewModel : AddEditStockAuditBaseViewModel
    {
        public ISKUBL _ISKUBL { get; set; }

        List<string> orgUIDs = new List<string>();
        List<string> DistributionChannelUIDs = new List<string>();

        List<string> skuUIDs = new List<string>();
        List<string> attributeTypes = new List<string>();

        public AddEditStockAuditAppViewModel(IServiceProvider serviceProvider,
    IFilterHelper filter,
    ISortHelper sorter,
    IListHelper listHelper,
    IAppUser appUser,
        ISKUBL iSKUBL,
    Winit.Shared.Models.Common.IAppConfig appConfigs,
    IRouteBL iRouteBL,
    Base.BL.ApiService apiService,
  Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
              IWHStockAuditBL iStockAuditBL) : base(serviceProvider, filter, sorter, listHelper, appUser, iSKUBL, appConfigs, iRouteBL, apiService, appSetting, iStockAuditBL)
        {
            _ISKUBL = iSKUBL;
            _IWHStockAuditBL = iStockAuditBL;
        }
        public bool firstObjectcreate = true;
        public override async Task GetSKUMasterData()
        {
           if(firstObjectcreate)
            {
                await FirstObjectCreate();
                firstObjectcreate = false;
            }
            if (SkuMasterData != null)
            {
                SaleableStockAuditItemView = new List<IStockAuditItemView>();
                StockAuditItemViews = new List<IStockAuditItemView>();
                DisplaySaleableStockAudit = new List<IStockAuditItemView>();
                DisplayFocStockAudit = new List<IStockAuditItemView>();
                FilteredStockAuditItemViews = new List<IStockAuditItemView>();
                List<IStockAuditItemView> salesOrderItemView = ConvertToISalesOrderItemView(SkuMasterData);
                if (salesOrderItemView != null)
                {
                    StockAuditItemViews.AddRange(salesOrderItemView);
                    var displayFocleStockAudit = await ItemSearch("", StockAuditItemViews);
                    DisplayFocStockAudit.AddRange(displayFocleStockAudit);
                }
                List<IStockAuditItemView> salesOrderItemView1 = ConvertToISalesOrderItemView(SkuMasterData);
                if (salesOrderItemView != null)
                {
                    SaleableStockAuditItemView.AddRange(salesOrderItemView1);
                    var displaySaleableStockAudit = await ItemSearch("", SaleableStockAuditItemView);
                    DisplaySaleableStockAudit.AddRange(displaySaleableStockAudit);
                }
            }
        }
        public async Task FirstObjectCreate()
        {
            try
            {

                SkuMasterData = await _ISKUBL.PrepareSKUMaster(
                        orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes);
                List<IStockAuditItemView> salesOrderItemView = ConvertToISalesOrderItemView(SkuMasterData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public List<IStockAuditItemView> ConvertToISalesOrderItemView(List<ISKUMaster> sKUMasters)
        {
            List<IStockAuditItemView> salesOrderItems = null;
            if (sKUMasters != null && sKUMasters.Count > 0)
            {
                salesOrderItems = new List<IStockAuditItemView>();
                int lineNumber = 1;
                foreach (var sKUMaster in sKUMasters)
                {
                    salesOrderItems.Add(ConvertToISalesOrderItemView(sKUMaster, lineNumber));
                    lineNumber++;
                }
            }

            return salesOrderItems;
        }

        public virtual IStockAuditItemView ConvertToISalesOrderItemView(ISKUMaster sKUMaster, int lineNumber)
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.First();
            List<ISKUUOMView>? sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
            ISKUUOMView? defaultUOM = sKUUOMViews
                            .First(e => e.Code == sKUConfig?.SellingUOM);
            ISKUUOMView? baseUOM = sKUUOMViews
                            .First(e => e.IsBaseUOM);
            IStockAuditItemView salesOrderItem = new StockAuditItemView
            {
                UID = Guid.NewGuid().ToString(),
                LineNumber = lineNumber,
                SKUImage = string.Empty,
                SKUUID = sKUMaster.SKU.UID,
                SKUCode = sKUMaster.SKU.Code,
                SKUName = sKUMaster.SKU.Name,
                SKULabel = sKUMaster.SKU.Name,
                IsMCL = false,
                IsPromo = false,
                IsNPD = false,
                SCQty = 0,
                RCQty = 0,
                VanQty = 0,
                //MaxQty = DefaultMaxQty,
                OrderQty = 0,
                Qty = 0,
                QtyBU = 0,
                DeliveredQty = 0,
                MissedQty = 0,
                BaseUOM = baseUOM?.Code,
                SelectedUOM = defaultUOM,
                AllowedUOMs = sKUUOMViews,
                //UsedUOMCodes = new List<string>(),
                BasePrice = 0,
                UnitPrice = 0,
                //IsTaxable = SelectedStoreViewModel.IsTaxApplicable,
                ApplicableTaxes = sKUMaster.ApplicableTaxUIDs,
                TotalAmount = 0,
                TotalLineDiscount = 0,
                TotalCashDiscount = 0,
                TotalHeaderDiscount = 0,
                TotalExciseDuty = 0,
                TotalLineTax = 0,
                TotalHeaderTax = 0,
                SKUPriceUID = null,
                SKUPriceListUID = null,
                Attributes = ConvertToISKUAttributeView(sKUMaster.SKUAttributes),
                ItemStatus = ItemState.Primary,
                ApplicationPromotionUIDs = null,
                PromotionUID = string.Empty,
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
        
        public override async Task PopulateViewModel(string apiParam = null)
        {
            await GetRouteByOrgUID(apiParam);
        }
        public override async Task<bool> AddStock()
        {
            PendingWHStockAuditRequest.WHStockAuditDetailsItemView = new List<Model.Classes.WHStockAuditDetailsItemView>();
            await CreateWHStockAudit();
            await CreateWHStockAuditLine();
            var retVal = await _IWHStockAuditBL.CUDWHStock(PendingWHStockAuditRequest);
            if (retVal > 0)
            {
                return true;
            }
            return false;
        }
        public async Task GetRouteByOrgUID(string OrgUID)
        {
            try
            {
                PagingRequest pagingRequest = new PagingRequest();

                if (_IRouteBL != null)
                {
                    var pagedResponse = await _IRouteBL.SelectRouteAllDetails(
                        pagingRequest?.SortCriterias,
                        pagingRequest?.PageNumber ?? 1,
                        pagingRequest?.PageSize ?? 10,
                        pagingRequest?.FilterCriterias,
                        pagingRequest?.IsCountRequired ?? false,
                        OrgUID
                    );
                    if (pagedResponse.PagedData != null)
                    {
                        RouteList = pagedResponse.PagedData.ToList();

                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public override async Task<List<IStockAuditItemView>> ItemSearch(string searchString, List<IStockAuditItemView> StockAuditItemViews)
        {
            try
            {
               
                if (_filter != null)
                {
                    return await _filter.ApplySearch<IStockAuditItemView>(
                            StockAuditItemViews, searchString, _propertiesToSearch);
                }
                // Add a default return statement here
                return new List<IStockAuditItemView>(); // or null, depending on your logic
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Handle the exception appropriately, and return a value
                return new List<IStockAuditItemView>(); // or null, depending on your logic
            }
        }


    }
}
