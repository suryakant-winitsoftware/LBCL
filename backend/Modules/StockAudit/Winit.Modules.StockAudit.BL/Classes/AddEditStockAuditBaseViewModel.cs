using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StockAudit.BL.Interfaces;
using Winit.Modules.StockAudit.DL.Interfaces;
using Winit.Modules.StockAudit.Model.Classes;
using Winit.Modules.StockAudit.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;



namespace Winit.Modules.StockAudit.BL.Classes
{
    public class AddEditStockAuditBaseViewModel: IAddEditStockAudit
    {
        
        public List<IStockAuditItemView> SaleableStockAuditItemView { get; set; }
        public List<IStockAuditItemView> StockAuditItemViews { get; set; }
        public List<IStockAuditItemView> DisplaySaleableStockAudit { get; set; }
        public List<IStockAuditItemView> DisplayFocStockAudit { get; set; }
        public List<IStockAuditItemView> FilteredStockAuditItemViews { get; set; }
        public IWHStockAuditBL _IWHStockAuditBL { get; set; }
        public string SelectedRouteUID { get; set; }
        public DateTime PageLoadTime { get; set; }
        public DateTime SaleConfirmTime { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SkuUOMList { get; set; }

        public List<ISelectionItem> UOMForSelection { get; set; }
        public List<ISelectionItem> FOCUOMForSelection { get; set; }
        public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SkuList { get; set; }
        public IServiceProvider _serviceProvider;
        public List<Winit.Modules.SKU.Model.Interfaces.ISKUMaster> SkuMasterData;
        public IFilterHelper _filter;
        public List<string> _propertiesToSearch = new List<string>();
        public ISortHelper _sorter;
      
        public IListHelper _listHelper;
        public List<Winit.Modules.Route.Model.Interfaces.IRoute> RouteList { get; set; }

        public WHStockAuditRequestTemplateModel PendingWHStockAuditRequest { get; set; } = new WHStockAuditRequestTemplateModel();
        public IEnumerable<ISKUMaster> SKUMasterList;

        public IAppUser _appUser;
        
        public Winit.Modules.Setting.BL.Interfaces.IAppSetting _appSetting;
        public Winit.Modules.Route.BL.Interfaces.IRouteBL _IRouteBL { get; set; }
        public string ActiveTab { get; set; }
        private ISKUBL _ISKUBL { get; set; }
        public AddEditStockAuditBaseViewModel(IServiceProvider serviceProvider,
    IFilterHelper filter,
    ISortHelper sorter,
    IListHelper listHelper,
    IAppUser appUser,  
        ISKUBL iSKUBL,
    Winit.Shared.Models.Common.IAppConfig appConfigs,
    IRouteBL iRouteBL,
    Base.BL.ApiService apiService,
    Winit.Modules.Setting.BL.Interfaces.IAppSetting appSetting,
             IWHStockAuditBL iStockAuditBL)
        {

            _filter = filter;
            _sorter = sorter;
            _serviceProvider = serviceProvider;
            _listHelper = listHelper;
            _appUser = appUser;
           
            _ISKUBL = iSKUBL;
            _IRouteBL = iRouteBL;
            _appSetting = appSetting;
            SaleableStockAuditItemView = new List<IStockAuditItemView>();
            StockAuditItemViews = new List<IStockAuditItemView>();
            DisplaySaleableStockAudit= new List<IStockAuditItemView>();
            DisplayFocStockAudit = new List<IStockAuditItemView>();
            FilteredStockAuditItemViews = new List<IStockAuditItemView>();
            _propertiesToSearch.Add("SKUCode");
            _propertiesToSearch.Add("SKUName");
        }

        public virtual async Task GetSKUMasterData()
        {
            throw new NotImplementedException();
        }

       
        public  virtual async Task<bool> AddStock()
        {
            throw new NotImplementedException();    
        }
        public virtual async Task<List<IStockAuditItemView>> ItemSearch(string searchString, List<IStockAuditItemView> StockAuditItemViews)
        {
            throw new NotImplementedException();
        }
     
        public virtual async Task PopulateViewModel(string apiParam=null)
        {
            throw new NotImplementedException();
        }
      
        public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForDDL(IStockAuditItemView salesOrderItemView)
        {
            return salesOrderItemView.AllowedUOMs
                .Where(e => !salesOrderItemView.UsedUOMCodes.Contains(e.Code))
                .Select(uom => new Shared.Models.Common.SelectionItem
                {
                    UID = uom.Code,
                    Code = uom.Code,
                    Label = uom.Label,
                    IsSelected = uom == salesOrderItemView.SelectedUOM // Mark the currently selected SKUUOM as selected
                })
                .ToList<ISelectionItem>();
        }

      
        public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForClone(IStockAuditItemView salesOrderItemView)
        {
            return salesOrderItemView.AllowedUOMs
                .Where(e => !salesOrderItemView.UsedUOMCodes.Contains(e.Code)
                && (salesOrderItemView.SelectedUOM == null || e.Code != salesOrderItemView.SelectedUOM.Code))
                .Select(uom => new Shared.Models.Common.SelectionItem
                {
                    UID = uom.Code,
                    Code = uom.Code,
                    Label = uom.Label,
                    IsSelected = false
                })
                .ToList<ISelectionItem>();
        }
        
        public async Task AddClonedItemToList(IStockAuditItemView item)
        {
            if(ActiveTab == StockAuditConst.Saleable)
            {
                await AddItemToList(SaleableStockAuditItemView, item, false);
                await AddItemToList(DisplaySaleableStockAudit, item, false);
            }
            else
            {
                await AddItemToList(StockAuditItemViews, item, false);
                await AddItemToList(DisplayFocStockAudit, item, false);
            }
            await AddItemToList(FilteredStockAuditItemViews, item, false);
           
        }
       
        public async Task RemoveItemFromList(IStockAuditItemView salesOrderItemView)
        {
            if (salesOrderItemView.SelectedUOM != null)
            {
                if(ActiveTab == StockAuditConst.Saleable)
                {
                    DisplaySaleableStockAudit.Remove(salesOrderItemView);
                    // FilteredStockAuditItemViews.Remove(salesOrderItemView);
                    SaleableStockAuditItemView.Remove(salesOrderItemView);
                }
                else
                {
                    DisplayFocStockAudit.Remove(salesOrderItemView);
                    // FilteredStockAuditItemViews.Remove(salesOrderItemView);
                    StockAuditItemViews.Remove(salesOrderItemView);
                }
               
                salesOrderItemView.UsedUOMCodes.Remove(salesOrderItemView.SelectedUOM.Code);
               
            }
        }
        private async Task AddItemToList(List<IStockAuditItemView> salesOrderItemViews, IStockAuditItemView item, bool addAtEnd = true)
        {
            if (addAtEnd)
            {
                await _listHelper.AddItemToList(salesOrderItemViews, item, null);
            }
            else
            {
                await _listHelper.AddItemToList(salesOrderItemViews, item, (T1, T2) => T1.SKUCode == T2.SKUCode);
            }

        }
      
        public async Task CreateWHStockAudit()  //
        {
            try
            {
                //PendingWHStockAuditRequest.WHStockAuditItemView = new Model.Classes.WHStockAuditItemView();
                var matchingRouteDetails = RouteList.First(item => item.UID == SelectedRouteUID);
                WHStockAuditItemView iWHStockAuditItemView = new WHStockAuditItemView
                {
                    Id = 0,
                    UID = System.Guid.NewGuid().ToString(),
                    CompanyUID = "WINIT",
                    WareHouseUID = matchingRouteDetails?.VehicleUID ?? string.Empty,
                    OrgUID = _appUser.SelectedJobPosition?.OrgUID ?? string.Empty,
                    AuditBy = _appUser.SelectedJobPosition?.EmpUID ?? string.Empty,
                    StartTime = PageLoadTime,
                    EndTime = System.DateTime.Now,
                    Status = StockAuditConst.Completed,
                    JobPositionUID = _appUser.SelectedJobPosition?.UID ?? string.Empty,
                    Remarks = "",
                    AuditNumber = $"{_appUser.SelectedJobPosition?.EmpUID ?? string.Empty}{(DateTime.Now).ToString("MMddyyyyHHmmss")}",
                    //ParentWarehouseUID = "123",
                   // ReferenceUID = "Salable",
                    UserJourneyUID = _appUser.UserJourney?.UID ?? string.Empty,
                    //AdjustmentStatus = "AdjsmntSt",
                    Source = "APP",
                    //WHStockAuditTemplateUID = "WINIT",
                    HasUnloadStock = false,
                    RouteUID = SelectedRouteUID,
                    SS = 0,
                    CreatedTime = System.DateTime.Now,
                    ModifiedTime = System.DateTime.Now,
                    ServerAddTime = System.DateTime.Now,
                    ServerModifiedTime = System.DateTime.Now,
                    IsLastRoute = false,
                    LastAuditTime = System.DateTime.Now,
                    CalculationStatus = "Pending",
                    //CalculationStartTime = System.DateTime.Now,
                    //CalculationEndTime = System.DateTime.Now,
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add
                };
                PendingWHStockAuditRequest.WHStockAuditItemView = iWHStockAuditItemView;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
        public int LineNumber = 0;
        public async Task CreateWHStockAuditLine()
        {
            try
            {
                   PendingWHStockAuditRequest.WHStockAuditDetailsItemView = new List<Model.Classes.WHStockAuditDetailsItemView>();
                   var selectedStoctAuditItem = ActiveTab == StockAuditConst.Saleable ? DisplaySaleableStockAudit
                                                                                     : DisplayFocStockAudit;
                    foreach (var stockAuditDisplay in selectedStoctAuditItem)
                    {
                        LineNumber = LineNumber + 1;
                    WHStockAuditDetailsItemView iWHStockAuditItemView = new WHStockAuditDetailsItemView
                    {
                        Id = 0,
                        UID = System.Guid.NewGuid().ToString(),
                        WHStockAuditUID = PendingWHStockAuditRequest.WHStockAuditItemView.UID,
                        LineNumber = LineNumber,
                        SKUUID = stockAuditDisplay.SKUUID,
                        SKUCode = stockAuditDisplay.SKUCode,
                        //BatchNumber = "",
                        // ExpiryDate = System.DateTime.Now,
                        // SerialNo = "",
                        BasePrice = 1,
                        ExpectedQty = 0,
                        ExpectedValue = 0,
                        UOM1 = stockAuditDisplay?.SelectedUOM?.Code,
                        UOM2 = stockAuditDisplay?.ItemStatus.ToString() == "Primary" ? "N/A" : stockAuditDisplay?.SelectedUOM?.Code,
                        Conversion1 = stockAuditDisplay?.ItemStatus.ToString() == "Primary" ? 1 : stockAuditDisplay?.SelectedUOM?.Multiplier ?? 1,
                        //Conversion2 = stockAuditDisplay?.ItemStatus.ToString() == "Primary" ? 1 : stockAuditDisplay?.SelectedUOM?.Multiplier ?? 1,
                        AvailableQty1 = stockAuditDisplay.UOMQty != null ? decimal.Parse(stockAuditDisplay.UOMQty) : 0,
                        // AvailableQty2 = stockAuditDisplay.SecondUOMQty != null ? decimal.Parse(stockAuditDisplay.SecondUOMQty) : 0,
                        StockType = ActiveTab,
                        OpenBalanceBU = 0,
                        StockReceiptQtyBU = 0,
                        DeliveredQtyBU = 0,
                        CreditsQtyBU = 0,
                        CreditCageQtyBU = 0,
                        CreditCustomerQtyBU = 0,
                        AdjustmentQtyBU = 0,
                        ClosingBalanceQtyBU = 0,
                        TotalStockCountBU = 0,
                        VarianceQtyBU = 0,
                        VarianceValue = 0,
                        FinalClosingQty = 0,
                        SS = 0,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        ServerAddTime = DateTime.Now,
                        ServerModifiedTime = DateTime.Now,
                        TotalStockCountValue = 0,

                        };
                        iWHStockAuditItemView.AvailableQty = iWHStockAuditItemView.AvailableQty1 * iWHStockAuditItemView.Conversion1;
                        iWHStockAuditItemView.AvailableValue = iWHStockAuditItemView.AvailableQty * iWHStockAuditItemView.BasePrice;
                        iWHStockAuditItemView.DiscrepencyQty = iWHStockAuditItemView.ExpectedQty * iWHStockAuditItemView.AvailableQty;
                        iWHStockAuditItemView.DiscrepencyValue = iWHStockAuditItemView.DiscrepencyQty;
                        PendingWHStockAuditRequest.WHStockAuditDetailsItemView.Add(iWHStockAuditItemView);
                    }
               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
