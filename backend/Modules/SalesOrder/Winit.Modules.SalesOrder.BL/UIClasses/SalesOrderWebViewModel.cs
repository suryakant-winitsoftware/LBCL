using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.SKU;

namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesOrderWebViewModel : SalesOrderBaseViewModel, ISalesOrderWebViewModel
{
    protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
    public List<IStoreCredit> StoreCredits { get; set; }
    public List<ISelectionItem> StoreCreditsSelectionItems { get; set; }
    private readonly List<IRoute> Routes = [];
    public List<ISelectionItem> DeliveryDistributor { get; set; } = [];
    public bool PresalesDeliveryDistributorEnabled { get { return true; } }
    public SalesOrderWebViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager,
            IOrderLevelCalculator orderLevelCalculator,
            ICashDiscountCalculator cashDiscountCalculator,
            Winit.Shared.Models.Common.IAppConfig appConfigs,
             ISalesOrderDataHelper salesOrderDataHelper)
        : base(serviceProvider, filter, sorter, amountCalculator, listHelper, appUser, appSetting,
               dataManager, orderLevelCalculator, cashDiscountCalculator, appConfigs, salesOrderDataHelper)
    {
        _appConfigs = appConfigs;
        RouteSelectionItems = [];
        StoreSelectionItems = [];
        StoreItemViews = [];
        StoreCredits = [];
        StoreCreditsSelectionItems = [];
    }

    public override async Task PopulateViewModel(string source,
        Winit.Modules.Store.Model.Interfaces.IStoreItemView? storeViewModel = null, bool isNewOrder = true,
        string salesOrderUID = "")
    {
        IsNewOrder = isNewOrder;
        SalesOrderUID = salesOrderUID;
        SetInitializationData();
        InitializeOptionalDependencies();
        TaxDictionary = _appUser.TaxDictionary;
        SetInvoiceLevelApplicableTaxes();
        Source = source;
        Routes.AddRange(await _salesOrderDataHelper.GetAllRoutesAPIAsync());
        List<ISelectionItem> routeSelectionItems = CommonFunctions.ConvertToSelectionItems
            <IRoute>(Routes, ["UID", "Code", "Name"]);
        RouteSelectionItems.Clear();
        RouteSelectionItems.AddRange(routeSelectionItems);
        SetSystemSettingValues();
        await PopulateSKUClass(); 
        await PopulateStoreCheckData();
        //await PopulateSKUMaster();
        //await PopulatePriceMaster();

        //await PopulateWHQty();
        if (!IsNewOrder)
        {
            SalesOrderViewModelDCO = await _salesOrderDataHelper.GetSalesOrderMasterDataBySalesOrderUID(SalesOrderUID);
            //await PopulateSalesOrder(); //used to load the existing order
            await SetEditMode();
        }
    }
    protected async Task SetEditMode()
    {

        List<ISalesOrderItemView> salesOrderItemViewList = SalesOrderItemViews;
        if (SalesOrderViewModelDCO != null && SalesOrderViewModelDCO.SalesOrderLines != null)
        {
            UpdateSalesOrderViewModelBySalesOrder(SalesOrderViewModelDCO!.SalesOrder!);
            if (OrderType == Winit.Shared.Models.Constants.OrderType.Presales)
            {

                DeliveredByOrgUID = SalesOrderViewModelDCO!.SalesOrder!.DeliveredByOrgUID;
            }

            await InitializeDropDownsEditPage(SalesOrderViewModelDCO.SalesOrder!);
            List<string> skuUids = SalesOrderViewModelDCO.SalesOrderLines.Select(e => e.SKUUID).ToList();
            List<string> skuPriceUids = SalesOrderViewModelDCO.SalesOrderLines.Select(e => e.SKUPriceUID).ToList();
            await PrepareProductViewMasterEditMode(skuUids, skuPriceUids);
            foreach (var salesOrderLine in SalesOrderViewModelDCO.SalesOrderLines)
            {
                await UpdateSalesOrderViewModelBySalesOrderLine(salesOrderLine);
            }
            SalesOrderItemViews.RemoveAll(e => e.BasePrice == 0 || e.Qty == 0);
        }
    }
    public async override Task UpdateSalesOrderViewModelBySalesOrderLine(Model.Interfaces.ISalesOrderLine salesOrderLine)
    {
        List<ISalesOrderItemView> salesOrderItemViewList = SalesOrderItemViews
            .Where(e => e.SKUUID == salesOrderLine.SKUUID) // Add SKUUID in DB and use that
            .ToList();
        if (salesOrderItemViewList != null)
        {
            // Take first item. If IsCartItem == false then good else any item
            ISalesOrderItemView? salesOrderItemView = salesOrderItemViewList
                .OrderBy(e => e.IsCartItem == false)
                .FirstOrDefault();
            if (salesOrderItemView != null)
            {
                ISKUUOMView? sKUUOM = salesOrderItemView.AllowedUOMs
                    .FirstOrDefault(u => u.Code == salesOrderLine.UoM);
                // If IsCartItem = true, it means the items already used so create a clone
                if (salesOrderItemView.IsCartItem)
                {
                    if (sKUUOM != null)
                    {
                        salesOrderItemView = salesOrderItemView.Clone(sKUUOM, ItemState.Cloned, sKUUOM.Code);
                        salesOrderItemView.IsCartItem = true;
                        salesOrderItemView.UsedUOMCodes.Add(sKUUOM.Code);
                        await AddClonedItemToList(salesOrderItemView);
                    }
                }
                else
                {
                    salesOrderItemView.IsCartItem = true;
                }
                if (sKUUOM != null)
                {
                    salesOrderItemView.SelectedUOM = sKUUOM;
                    salesOrderItemView.UID = salesOrderLine.UID;
                    salesOrderItemView.SalesOrderLineUID = salesOrderLine.UID;
                    salesOrderItemView.SKUPriceListUID = salesOrderLine.SKUPriceListUID;
                    salesOrderItemView.SKUPriceUID = salesOrderLine.SKUPriceUID;
                    salesOrderItemView.OrderQty = salesOrderLine.RecoQty;
                    salesOrderItemView.OrderUOM = salesOrderLine.RecoUOM;
                    salesOrderItemView.OrderUOMMultiplier = salesOrderLine.RecoUOMConversionToBU;
                    salesOrderItemView.Qty = salesOrderLine.Qty;
                    salesOrderItemView.IsCartItem = true;
                    salesOrderItemView.QtyBU = salesOrderLine.QtyBU;
                    salesOrderItemView.TotalCashDiscount = salesOrderLine.TotalCashDiscount;
                    if (Status == SalesOrderStatus.DELIVERED)
                    {
                        salesOrderItemView.DeliveredQty = salesOrderLine.DeliveredQty;
                    }
                    await UpdateItemPrice(salesOrderItemView);
                    // Calculate WHSalableQty for cloned item
                    SetWHSalableQty(salesOrderItemView);
                    await OnQtyChange(salesOrderItemView);
                    //SetAlreadySelectedUOMList(salesOrderItemPrimary);
                }
            }
        }
    }
    /// <summary>
    /// Override this method if you want something should be done before executing PopulateViewModel
    /// </summary>
    public virtual void SetInitializationData()
    {
        Status = Shared.Models.Constants.SalesOrderStatus.FINALIZED;
    }
    #region Business Logics
    public override async Task OnRouteSelect(string routeUID)
    {
        RouteUID = routeUID;
        VehicleUID = Routes.FirstOrDefault(e => e.UID == routeUID)!.VehicleUID;
        StoreItemViews.Clear();
        StoreItemViews.AddRange(await _salesOrderDataHelper.GetStoreItemViewsDataFromAPIAsync(routeUID));
        StoreSelectionItems.Clear();
        StoreSelectionItems.AddRange(Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems
            <IStoreItemView>(StoreItemViews, ["UID", "Code", "Name"]));
    }
    public override async Task OnStoreItemViewSelected(string storeItemViewUID)
    {
        IStoreItemView? storeItemView = StoreItemViews.Find(e => e.UID == storeItemViewUID);
        if (storeItemView != null)
        {
            SelectedStoreViewModel = storeItemView;
            await PopulateStoreData(SelectedStoreViewModel);
            AddTaxCalculatorDependency();
            ValidateSalesOrderViewModel();
            PopulateFooter();
            await PopulatePromotion();
            await PopulateSkuByOrgHierarchyandStore();
            List<SortCriteria> sortCriteriaList =
            [
                new SortCriteria("SKUName", SortDirection.Asc)
            ];
            await ApplySort(sortCriteriaList);
            if (PresalesDeliveryDistributorEnabled)
            {
                await GetDeliveryDistributorsByOrgUID();
            }
        }
    }

    public void OnStoreDistributionChannelSelect(string distributionChannelUID)
    {
        SelectedStoreViewModel.SelectedDistributionChannelUID = distributionChannelUID;
        DistributionChannelOrgUID = distributionChannelUID;
    }

    public void OnDeliveryDistributorSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            DeliveredByOrgUID = dropDownEvent.SelectionItems == null || dropDownEvent.SelectionItems.Count == 0 ?
                string.Empty : dropDownEvent.SelectionItems.FirstOrDefault()!.UID!;

        }
    }
    protected async Task GetDeliveryDistributorsByOrgUID()
    {
        DeliveryDistributor.Clear();
        List<IOrg> deliveryDistributors = await _salesOrderDataHelper.GetDeliveryDistributorsByOrgUID(OrgUID, FranchiseeOrgUID);
        if (deliveryDistributors != null && deliveryDistributors.Any())
        {
            DeliveryDistributor.AddRange(CommonFunctions.ConvertToSelectionItems<IOrg>(deliveryDistributors, s => s.UID, s => s.Code == DeliveredByOrgUID, s => s.Code, s => s.Name));
        }
    }
    protected override async Task PopulateStoreData(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel)
    {
        StoreCredits.Clear();
        StoreCreditsSelectionItems.Clear();
        List<StoreMasterDTO> storeMaster = await _salesOrderDataHelper.GetStoreMastersbyStoreUIDs([storeViewModel.UID]);
        if (storeMaster != null && storeMaster.Any())
        {
            StoreMasterDTO storeMasterDTO = storeMaster.FirstOrDefault()!;
            if (storeMasterDTO != null)
            {
                if (storeMasterDTO.storeCredits != null &&
                    storeMaster.First().storeCredits.Any())
                {
                    if (storeMasterDTO.storeCredits.Count == 1)
                    {
                        SelectedStoreViewModel.SelectedDistributionChannelUID = storeMasterDTO.storeCredits.First().DistributionChannelUID;
                    }
                    else
                    {
                        StoreCredits.AddRange(storeMasterDTO.storeCredits);
                        StoreCreditsSelectionItems.AddRange(CommonFunctions.ConvertToSelectionItems<IStoreCredit>
                            (StoreCredits, ["UID", "DistributionChannelUID", "CreditType"]));
                    }
                }
                if (storeMasterDTO.Store != null)
                {
                    FranchiseeOrgUID = storeMasterDTO.Store.FranchiseeOrgUID;
                }
            }
        }
        OrgUID = _appUser.SelectedJobPosition.OrgUID;
        DistributionChannelOrgUID = SelectedStoreViewModel.SelectedDistributionChannelUID;
        StoreUID = storeViewModel.UID;

        JobPositionUID = _appUser.SelectedJobPosition.UID;
        EmpUID = _appUser.Emp.UID;
    }
    public override async Task InitializeDropDownsEditPage(ISalesOrder salesOrder)
    {
        ISelectionItem? routeSelectectionItem = RouteSelectionItems?.Find(e => e.UID == salesOrder.RouteUID);
        if (routeSelectectionItem != null)
        {
            routeSelectectionItem.IsSelected = true;
            await OnRouteSelect(salesOrder.RouteUID);
            ISelectionItem? storeSelectionItem = StoreSelectionItems.Find(e => e.UID == salesOrder.StoreUID);
            await OnStoreItemViewSelected(salesOrder.StoreUID);

            if (storeSelectionItem != null)
            {
                storeSelectionItem.IsSelected = true;
            }
        }
    }
    public override async Task AddSelectedProducts(List<ISalesOrderItemView> salesOrderItemViews)
    {
        foreach (ISalesOrderItemView salesOrderItemView in salesOrderItemViews)
        {
            salesOrderItemView.Qty = 1;
            await OnQtyChange(salesOrderItemView);
        }
    }

    public async Task OnAddProductClick(List<ISKUV1> skus)
    {
        await PrepareProductViewMaster(skus.Select(e => e.UID).ToList());
    }


    #endregion
}
