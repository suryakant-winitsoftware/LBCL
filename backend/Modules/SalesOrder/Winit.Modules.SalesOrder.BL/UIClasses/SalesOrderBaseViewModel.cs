using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.Data;
using System.Diagnostics;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.SalesOrder.BL.UIClasses;

public abstract class SalesOrderBaseViewModel : UIInterfaces.ISalesOrderViewModel
{
    public bool IsNewOrder = true;
    protected string SkuPriceList { get; set; } = "DefaultPriceList";
    protected SalesOrderViewModelDCO? SalesOrderViewModelDCO;
    public Int64 Id { get; set; }
    public string SalesOrderUID { get; set; }
    public string SalesOrderNumber { get; set; }
    public string DraftOrderNumber { get; set; }
    protected string DeliveredByOrgUID { get; set; }
    public string OrderType { get; set; }
    public string OrgUID { get; set; }
    public string FranchiseeOrgUID { get; set; }
    public string DistributionChannelOrgUID { get; set; }
    public string StoreUID { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerPO { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime DeliveredDateTime { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> SalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> FilteredSalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> DisplayedSalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> SelectedSalesOrderItemViews { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> DisplayedSalesOrderItemViews_Preview { get; set; }
    public List<Model.UIInterfaces.ISalesOrderItemView> FOCSalesOrderItemViews { get; set; }
    public string CurrencyUID { get; set; }
    public string RouteUID { get; set; }
    /// <summary>
    /// Will be used to show Currency Label
    /// </summary>
    public string CurrencyLabel { get; set; }
    public int LineCount { get; set; }
    public decimal QtyCount { get; set; }
    public decimal AvailableCreditLimit { get; set; }
    public int CreditDays { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalLineDiscount { get; set; }
    public decimal TotalCashDiscount { get; set; }
    public decimal TotalHeaderDiscount { get; set; }
    /// <summary>
    /// TotalDiscount = TotalLineDiscount + TotalCashDiscount + TotalHeaderDiscount
    /// </summary>
    public decimal TotalDiscount { get { return TotalLineDiscount + TotalCashDiscount + TotalHeaderDiscount; } }
    public decimal TotalExciseDuty { get; set; }
    public decimal TotalLineTax { get; set; }
    public decimal TotalHeaderTax { get; set; }
    /// <summary>
    /// TotalTax = LineTaxAmount + HeaderTaxAmount
    /// </summary>
    public decimal TotalTax { get { return TotalLineTax + TotalHeaderTax; } }
    /// <summary>
    /// NetAmount = TotalAmount - TotalDiscount + ExciseDuty + TotalTax
    /// </summary>
    public decimal NetAmount { get { return TotalAmount - TotalDiscount + TotalExciseDuty + TotalTax; } }
    public string Source { get; set; }
    public string ReferenceNumber { get; set; }
    public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreViewModel { get; set; }
    public List<FilterCriteria> FilterCriteriaList { get; set; }
    public List<SortCriteria> SortCriteriaList { get; set; }
    private List<string> _propertiesToSearch = new List<string>();
    public int DefaultDeliveryDay { get; set; } = 0;
    public Dictionary<string, decimal> VanStock { get; set; }
    public bool IsPriceInclusiveVat { get; set; } = false;
    public string Notes { get; set; }
    public string DeliveryInstructions { get; set; }
    public string Remarks { get; set; }
    public bool IsInitialized { get; set; } = false;
    public int DefaultMaxQty { get; set; }

    //Fields for Web
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
    public List<ISelectionItem> SalesTypeSelectionItems { get; set; }
    protected List<IStoreItemView> StoreItemViews { get; set; }
    public string CustomerPoNumber { get; set; }
    public string Address { get; set; }
    public string CustomerName { get; set; }
    public bool IsPriceEditable { get; set; }
    #region Variables for Signature
    public string CustomerSignatureFolderPath { get; set; }
    public string UserSignatureFolderPath { get; set; }
    public string CustomerSignatureFileName { get; set; }
    public string UserSignatureFileName { get; set; }
    public string CustomerSignatureFilePath { get; set; }
    public string UserSignatureFilePath { get; set; }
    public bool IsSignaturesCaptured { get; set; }
    public List<IFileSys> SignatureFileSysList { get; set; }
    public List<ISKUV1> SKUs { get; set; } = [];
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; } = [];
    #endregion
    // Injection
    private IServiceProvider _serviceProvider;
    protected readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    public ISalesOrderTaxCalculator _salesOrderTaxCalculator { get; set; }
    //private readonly IPromotionManager _promotionManager;
    private readonly UIInterfaces.ISalesOrderAmountCalculator _amountCalculator;
    private readonly IListHelper _listHelper;
    protected readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    protected readonly IDataManager _dataManager;
    private readonly IAppConfig _appConfig;
    private IOrderLevelCalculator _orderLevelCalculator;
    private ICashDiscountCalculator _cashDiscountCalculator;
    protected readonly ISalesOrderDataHelper _salesOrderDataHelper;
    // Promotion
    public List<string>? ApplicablePromotionList { get; set; }
    public Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>? DMSPromotionDictionary { get; set; }
    public Dictionary<string, List<ISelectionItem>>? PromotionItemMapDictionary { get; set; }
    public List<AppliedPromotionView>? AppliedPromotionViewList { get; set; }
    public string SelectedPromotionUID { get; set; }
    public Dictionary<string, ITax> TaxDictionary { get; set; }
    public List<string> InvoiceApplicableTaxes { get; set; }
    public List<IAppliedTax> AppliedTaxes { get; set; }
    public string VehicleUID { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public string BeatHistoryUID { get; set; }
    public string StoreHistoryUID { get; set; }
    public bool IsStockUpdateRequired { get; set; }
    public bool IsInvoiceGenerationRequired { get; set; }
    public List<ISelectionItem> DistributorsList { get; set; } =[];
    public ISelectionItem SelectedDistributor { get; set; }
    public IJSRuntime _JS
    {
        get; set;
    }
    //Constructor
    public SalesOrderBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
            IListHelper listHelper,
            IAppUser appUser,
            IAppSetting appSetting,
            IDataManager dataManager,
            IOrderLevelCalculator orderLevelCalculator,
            ICashDiscountCalculator cashDiscountCalculator,
            IAppConfig appConfig, ISalesOrderDataHelper salesOrderDataService
       )
    {

        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _amountCalculator = amountCalculator;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        _orderLevelCalculator = orderLevelCalculator;
        _cashDiscountCalculator = cashDiscountCalculator;
        _appConfig = appConfig;
        _salesOrderDataHelper = salesOrderDataService;

        // Initialize common properties or perform other common setup
        SalesOrderItemViews = new List<Model.UIInterfaces.ISalesOrderItemView>();
        FilteredSalesOrderItemViews = new List<Model.UIInterfaces.ISalesOrderItemView>();
        DisplayedSalesOrderItemViews = new List<Model.UIInterfaces.ISalesOrderItemView>();
        DisplayedSalesOrderItemViews_Preview = new List<Model.UIInterfaces.ISalesOrderItemView>();
        FOCSalesOrderItemViews = new List<Model.UIInterfaces.ISalesOrderItemView>();
        VanStock = new Dictionary<string, decimal>();
        SignatureFileSysList = new List<IFileSys>();
        FilterCriteriaList = new List<FilterCriteria>();
        // Initialization of dependent component
        _orderLevelCalculator.SetSalesOrderViewModel(this);
        _cashDiscountCalculator.SetSalesOrderViewModel(this);

        // Property set for Search
        _propertiesToSearch.Add("SKUCode");
        _propertiesToSearch.Add("SKUName");
    }
    public virtual void InitializeOptionalDependencies()
    {
        AddTaxCalculatorDependency();
    }
    protected virtual void AddTaxCalculatorDependency()
    {
        if (SelectedStoreViewModel != null && SelectedStoreViewModel.IsTaxApplicable)
        {
            ITaxCalculator _taxCalculator = _serviceProvider.CreateInstance<ITaxCalculator>();
            _taxCalculator.SetTaxCalculator(_serviceProvider, _appSetting);
            _salesOrderTaxCalculator = new SalesOrderTaxCalculator(_taxCalculator);
            _salesOrderTaxCalculator.SetSalesOrderViewModel(this);
        }
    }
    /// <summary>
    /// To be called after object creation
    /// </summary>
    public async virtual Task PopulateViewModel(string source, Winit.Modules.Store.Model.Interfaces.IStoreItemView? storeViewModel, bool isNewOrder = true,
        string salesOrderUID = "")
    {
        SelectedStoreViewModel = storeViewModel;
        await PopulateStoreData(SelectedStoreViewModel);
        ValidateSalesOrderViewModel();
        InitializeOptionalDependencies();
        TaxDictionary = _appUser.TaxDictionary;
        SetInvoiceLevelApplicableTaxes();
        IsNewOrder = isNewOrder;
        SalesOrderUID = salesOrderUID;
        if (isNewOrder) SalesOrderUID = Guid.NewGuid().ToString();
        OrderType = Winit.Shared.Models.Constants.OrderType.Presales;
        Status = Shared.Models.Constants.SalesOrderStatus.FINALIZED;
        Source = SourceType.APP;
        await SetSystemSettingValues();
        await PopulateSKUClass();
        await PopulateStoreCheckData();
        await PopulateSKUMaster();
        await PopulatePriceMaster();
        await PopulateWHQty();
        await PopulatePromotion();
        PopulateFooter();
        await ApplyFilter(new List<Shared.Models.Enums.FilterCriteria>(), FilterMode.And);
        List<SortCriteria> sortCriteriaList = new List<SortCriteria>();
        sortCriteriaList.Add(new SortCriteria("SKUName", SortDirection.Asc));
        await ApplySort(sortCriteriaList);
        await PopulateSalesOrder();
        PrepareSignatureFields();
    }

    #region Business Logic
    public void SetInvoiceLevelApplicableTaxes()
    {
        if (_salesOrderTaxCalculator == null)
        {
            return;
        }
        InvoiceApplicableTaxes = _salesOrderTaxCalculator.GetApplicableTaxesByApplicableAt(TaxDictionary, "Invoice");
    }
    protected async virtual Task PopulateStoreData(Winit.Modules.Store.Model.Interfaces.IStoreItemView storeViewModel)
    {
        if (storeViewModel != null)
        {
            OrgUID = storeViewModel.SelectedOrgUID;
            DistributionChannelOrgUID = storeViewModel.SelectedDistributionChannelUID;
            //if (storeViewModel.SelectedStoreDistributionChannelSource != null)
            //{
            //    DistributionChannelOrgUID = storeViewModel.SelectedStoreDistributionChannelSource.SourceUID;
            //}
            StoreUID = storeViewModel.StoreUID;

        }
        await Task.CompletedTask;
    }
    protected virtual void PopulateSalesOrderSetting()
    {
        JobPositionUID = _appUser.SelectedJobPosition.UID;
        EmpUID = _appUser.Emp.UID;
        BeatHistoryUID = _appUser.SelectedBeatHistory.UID;
        RouteUID = _appUser.SelectedRoute.UID;
        StoreHistoryUID = _appUser.SelectedCustomer.StoreHistoryUID;
        VehicleUID = _appUser.Vehicle?.UID;
        if (OrderType == Shared.Models.Constants.OrderType.Vansales)
        {
            IsStockUpdateRequired = true;
            IsInvoiceGenerationRequired = true;
        }
        else
        {
            IsStockUpdateRequired = false;
            IsInvoiceGenerationRequired = false;
        }
    }
    /// <summary>
    /// Load Setting Values for Sales Order
    /// </summary>
    public virtual async Task SetSystemSettingValues()
    {
        SetDefaultDeliveryDay();
        SetCurrency();
        SetDefaultMaxQty();
        await Task.CompletedTask;
    }
    /// <summary>
    /// If existing SalesOrder then populate the required field from SalesOrder
    /// </summary>
    protected async Task PopulateSalesOrder()
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(SalesOrderUID))
        {
            Model.Interfaces.ISalesOrder? salesOrder = await _salesOrderDataHelper.GetSalesOrderByUID(SalesOrderUID);
            if (salesOrder != null)
            {
                UpdateSalesOrderViewModelBySalesOrder(salesOrder);
                await InitializeDropDownsEditPage(salesOrder);
                await PopulateSalesOrderLine();
            }
        }
    }

    protected async Task PrepareProductViewMaster(List<string>? SKUUIDs)
    {
        List<FilterCriteria> filterCriterias = new()
        {
                new FilterCriteria("SKUUID", SKUUIDs, FilterType.In),
                new FilterCriteria("Price", 0, FilterType.NotEqual),
                new FilterCriteria("OrgUID", _appUser.SelectedJobPosition.OrgUID, FilterType.Equal),
        };
        Task<PagedResponse<ISKUPrice>> pagedResponse = _salesOrderDataHelper.
            PopulatePriceMaster(filterCriterias: filterCriterias, skuPriceList: SkuPriceList);


        Task task = PrepareProductMaster(SKUUIDs);
        await Task.WhenAll(pagedResponse, task);

        if (pagedResponse.Result.PagedData != null)
            PopulatePrice(pagedResponse.Result.PagedData);
    }
    protected async Task PrepareProductViewMasterEditMode(List<string>? SKUUIDs, List<string> skuPriceUids)
    {
        List<FilterCriteria> filterCriterias = new()
        {
                new FilterCriteria("SKUUID", SKUUIDs, FilterType.In),
                new FilterCriteria("Price", 0, FilterType.NotEqual),
                new FilterCriteria("OrgUID", _appUser.SelectedJobPosition.OrgUID, FilterType.Equal),
        };
        Task<PagedResponse<ISKUPrice>> pagedResponse = _salesOrderDataHelper.
            PopulatePriceMaster(filterCriterias: filterCriterias, skuPriceList: SkuPriceList);


        Task task = PrepareProductMaster(SKUUIDs);
        await Task.WhenAll(pagedResponse, task);

        if (pagedResponse.Result.PagedData != null)
            PopulatePrice(pagedResponse.Result.PagedData);
    }
    protected async Task PrepareProductMaster(List<string>? SKUUIDs)
    {
        SKUMasterRequest request = new();
        request.SKUUIDs = SKUUIDs?.Distinct().ToList();

        Task<List<ISKUMaster>> skuMasters = _salesOrderDataHelper.PopulateSKUMaster(request);
        Task<List<IFileSys>>? fileSysList = _salesOrderDataHelper.GetFileSys(LinkedItemType.SKU, FileSysType.Image);
        await Task.WhenAll(skuMasters, fileSysList);
        List<ISalesOrderItemView> salesOrderItemView = ConvertToISalesOrderItemView(skuMasters.Result, fileSysList.Result);
        if (salesOrderItemView != null)
        {
            salesOrderItemView.ForEach(p =>
            {
                p.IsCartItem = true;
                SalesOrderItemViews.Add(p);
            });
        }
    }
    public virtual void UpdateSalesOrderViewModelBySalesOrder(Model.Interfaces.ISalesOrder salesOrder)
    {
        Id = salesOrder.Id;
        SalesOrderNumber = salesOrder.SalesOrderNumber;
        DraftOrderNumber = salesOrder.DraftOrderNumber;
        OrderType = salesOrder.OrderType;
        Status = salesOrder.Status;
        OrderDate = salesOrder.OrderDate;
        CustomerPO = salesOrder.CustomerPO;
        CurrencyUID = salesOrder.CurrencyUID;
        Source = salesOrder.Source;
        ReferenceNumber = salesOrder.ReferenceNumber;
        TotalCashDiscount = salesOrder.TotalCashDiscount;
        CustomerName = salesOrder.CashSalesCustomer;
        Address = salesOrder.CashSalesAddress;
        ExpectedDeliveryDate = salesOrder.ExpectedDeliveryDate;
        Notes = salesOrder.Notes;
        DeliveryInstructions = salesOrder.DeliveryInstructions;
        Remarks = salesOrder.Remarks;
    }

    /// <summary>
    /// If existing SalesOrder then populate the required field from SalesOrderLine
    /// </summary>
    private async Task PopulateSalesOrderLine()
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(SalesOrderUID))
        {
            List<Model.Interfaces.ISalesOrderLine>? salesOrderLines = await _salesOrderDataHelper.GetSalesOrderLinesBySalesOrderUID(SalesOrderUID);
            if (salesOrderLines != null)
            {
                await PrepareProductMaster(salesOrderLines.Select(e => e.SKUUID).Distinct().ToList());
                foreach (ISalesOrderLine salesOrderLine in salesOrderLines)
                {
                    await UpdateSalesOrderViewModelBySalesOrderLine(salesOrderLine);
                }
            }
        }
    }
    public void SetWHSalableQty(ISalesOrderItemView salesOrderItemView)
    {
        try
        {
            ///*Need to take SUM because in case of Batchwise concept it can have multiple line for same item*/
            //if (salesOrderItem.WHStockAvailableViewListForWH.Any(e => e.UOM == salesOrderItem.SelectedUOM.Code && e.VersionNo == salesOrderItem.PriceVersionNumber))
            //{
            //    // Not using WHQty as we are going to use same column for both vansales & presales
            //    salesOrderItem.VanQty = salesOrderItem.WHStockAvailableViewListForWH
            //        .Where(e => e.UOM == salesOrderItem.SelectedUOM.Code && e.VersionNo == salesOrderItem.PriceVersionNumber)
            //        .Sum(e => e.Qty);
            //}
            //else
            //{
            //    salesOrderItem.VanQty = 0;
            //}
        }
        catch (System.Exception)
        {
            throw;
        }
    }
    public async virtual Task UpdateSalesOrderViewModelBySalesOrderLine(Model.Interfaces.ISalesOrderLine salesOrderLine)
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
                    // Calculate WHSalableQty for cloned item
                    SetWHSalableQty(salesOrderItemView);
                    await OnQtyChange(salesOrderItemView);
                    //SetAlreadySelectedUOMList(salesOrderItemPrimary);
                }
            }
        }
    }
    private void SetDefaultDeliveryDay()
    {
        if (_appUser.UserType == UserType.VanSales)
        {
            DefaultDeliveryDay = 0;
        }
        else
        {
            DefaultDeliveryDay = _appSetting.DefaultDeliveryDay;
        }
        ExpectedDeliveryDate = DateTime.Now.AddDays(DefaultDeliveryDay);
    }
    private void SetCurrency()
    {
        CurrencyUID = _appUser.OrgCurrencyList.FirstOrDefault()?.CurrencyUID;
        CurrencyLabel = _appUser.OrgCurrencyList.FirstOrDefault()?.Symbol;
    }
    private void SetDefaultMaxQty()
    {
        DefaultMaxQty = 999;// _appSetting.DefaultMaxQty; (Enable Setting code once login flow completes)
    }
    public virtual void Dispose()
    {
    }
    private async Task AddItemToList(List<Model.UIInterfaces.ISalesOrderItemView> salesOrderItemViews,
    Model.UIInterfaces.ISalesOrderItemView item, bool addAtEnd = true)
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
    public async Task AddClonedItemToList(Model.UIInterfaces.ISalesOrderItemView item)
    {
        await AddItemToList(SalesOrderItemViews, item, false);
        await AddItemToList(FilteredSalesOrderItemViews, item, false);
        await AddItemToList(DisplayedSalesOrderItemViews, item, false);
        await OnQtyChange(item);
    }
    protected async Task PopulateSKUMaster()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();

        // Get all SKUs first
        List<ISKUMaster> skuMasters = await _salesOrderDataHelper.PopulateSKUMaster(_appUser.OrgUIDs);
        
        // Filter SKUs based on AllowedSKUs if the list exists
        if (SelectedStoreViewModel?.AllowedSKUs != null && SelectedStoreViewModel.AllowedSKUs.Any())
        {
            skuMasters = skuMasters.Where(sku => SelectedStoreViewModel.AllowedSKUs.Contains(sku.SKU.UID)).ToList();
        }

        stopwatch1.Stop();
        
        List<IFileSys>? fileSysList = await _salesOrderDataHelper.GetFileSys(LinkedItemType.SKU, FileSysType.Image);
        List<ISalesOrderItemView> salesOrderItemView = ConvertToISalesOrderItemView(skuMasters, fileSysList);
        if (salesOrderItemView != null)
        {
            SalesOrderItemViews.AddRange(salesOrderItemView);
        }
        stopwatch.Stop();
    }

    protected async Task PopulatePriceMaster()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        PagedResponse<ISKUPrice> pagedResponse = await _salesOrderDataHelper.PopulatePriceMaster();
        if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Count() > 0)
        {
            PopulatePrice(pagedResponse.PagedData);

            stopwatch.Stop();
            //await _JS.InvokeVoidAsync("console.log", $"PopulatePriceMaster {stopwatch.ElapsedMilliseconds}");
        }
    }

    protected async Task PopulateSkuByOrgHierarchyandStore()
    {
        List<string> orgs = await _salesOrderDataHelper.GetOrgHierarchyParentUIDsByOrgUID(FranchiseeOrgUID);
        await PopulateSkuByOrgHierarchy(orgs);
    }
    protected async Task PopulateSkuByOrgHierarchy(List<string> orgs)
    {
        if (orgs == null || orgs.IsNullOrEmpty())
        {
            orgs = [FranchiseeOrgUID];
        }
        var orgList = new List<string> { FranchiseeOrgUID };
        PagingRequest pagingRequest = new PagingRequest()
        {
            FilterCriterias = [new(nameof(ISKUV1.OrgUID), orgs, FilterType.In)],
        };
        var skus = await _salesOrderDataHelper.GetSKUs(pagingRequest);
        SKUs.Clear();
        SKUs.AddRange(skus);

    }
    protected void PopulatePrice(IEnumerable<ISKUPrice> sKUPrices)
    {
        foreach (ISKUPrice sKUPrice in sKUPrices)
        {
            // Find the corresponding item in SalesOrderItemViews by matching some identifier, e.g., SKU or ID
            var salesOrderItem = SalesOrderItemViews.Find(item => item.SKUUID == sKUPrice.SKUUID);

            if (salesOrderItem != null)
            {
                // Update the price in SalesOrderItemViews
                salesOrderItem.BasePrice = sKUPrice.Price;
                salesOrderItem.UnitPrice = sKUPrice.Price;
                salesOrderItem.MRP = sKUPrice.MRP;
                salesOrderItem.SKUPriceUID = sKUPrice.UID;
                salesOrderItem.SKUPriceListUID = sKUPrice.SKUPriceListUID;
                salesOrderItem.PriceVersionNo = sKUPrice.VersionNo;
                salesOrderItem.PriceLowerLimit = sKUPrice.PriceLowerLimit;
                salesOrderItem.PriceUpperLimit = sKUPrice.PriceUpperLimit;
            }
        }
        //Remove items where price = 0
        SalesOrderItemViews.RemoveAll(item => item.BasePrice == 0);
    }
    protected async Task PopulateWHQty()
    {
        if(_appUser.Vehicle == null)
        {
            return;
        }
        IEnumerable<Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView> warehouseStockItemViewList = await _salesOrderDataHelper.GetVanStockItems(_appUser.Vehicle.UID, _appUser.SelectedJobPosition.OrgUID, StockType.Salable);
        VanStock.Clear();
        foreach (Winit.Modules.Org.Model.Interfaces.IWarehouseStockItemView warehouseStockItemView in warehouseStockItemViewList)
        {
            VanStock[warehouseStockItemView.SKUUID] = warehouseStockItemView.TotalEAQty;
        }
    }
    protected void PopulateFooter()
    {
        AvailableCreditLimit = SelectedStoreViewModel.AvailableCreditLimit;
    }
    protected async Task PopulatePromotion()
    {
        if (!SelectedStoreViewModel.IsPromotionsBlock)
        {
            ApplicablePromotionList = SelectedStoreViewModel.ApplicablePromotionList;
            DMSPromotionDictionary = SelectedStoreViewModel.DMSPromotionDictionary;
            await PopulatePromotionForItems();
        }
        await Task.CompletedTask;
    }
    protected async Task PopulatePromotionForItems()
    {
        if (SelectedStoreViewModel.ItemPromotionMapList == null || !SelectedStoreViewModel.ItemPromotionMapList.Any()) return;
        if (OrderType != Winit.Shared.Models.Constants.OrderType.Cashsales
            && OrderType != Winit.Shared.Models.Constants.OrderType.Presales
            && OrderType != Winit.Shared.Models.Constants.OrderType.FOC)
        {
            if (SalesOrderItemViews != null && SalesOrderItemViews.Count > 0)
            {
                PromotionItemMapDictionary = new Dictionary<string, List<ISelectionItem>>();
                // SKU Level
                foreach (IItemPromotionMap itemPromotionMap in SelectedStoreViewModel.ItemPromotionMapList
                    .Where(e => e.SKUType.Equals(ItemCriteriaType.SKU, StringComparison.OrdinalIgnoreCase)))
                {
                    if (SalesOrderItemViews.Any(e => e.SKUUID == itemPromotionMap.SKUTypeUID))
                    {
                        ISalesOrderItemView? salesOrderItemView = SalesOrderItemViews
                            .FirstOrDefault(e => e.SKUUID == itemPromotionMap.SKUTypeUID);
                        if (salesOrderItemView != null)
                        {
                            SetItemPromotion(itemPromotionMap.PromotionUID, salesOrderItemView);
                        }
                    }
                }
                // Attribue Level
                foreach (IItemPromotionMap itemPromotionMap in SelectedStoreViewModel.ItemPromotionMapList
                    .Where(e => !e.SKUType.Equals(ItemCriteriaType.SKU, StringComparison.OrdinalIgnoreCase)))
                {
                    List<ISalesOrderItemView> tempSalesOrderItemViewList = SalesOrderItemViews
                    .Where(e => e.Attributes != null
                                && e.Attributes.ContainsKey(itemPromotionMap.SKUType)
                                && e.Attributes.Values.Any(v => v.Code == itemPromotionMap.SKUTypeUID))
                    .ToList();
                    if (tempSalesOrderItemViewList != null)
                    {
                        foreach (ISalesOrderItemView salesOrderItemView in tempSalesOrderItemViewList)
                        {
                            SetItemPromotion(itemPromotionMap.PromotionUID, salesOrderItemView);

                        }
                    }
                }
            }
        }
        await Task.CompletedTask;
    }
    public void CreatePromotionDictionary(string promotionUID, ISalesOrderItemView salesOrderItemView)
    {
        if (PromotionItemMapDictionary != null)
        {
            if (!PromotionItemMapDictionary.ContainsKey(promotionUID))
            {
                PromotionItemMapDictionary[promotionUID] = new List<ISelectionItem>();
            }

            List<ISelectionItem> selectionItems = CommonFunctions.ConvertToSelectionItems<ISalesOrderItemView>(
                new List<ISalesOrderItemView> { salesOrderItemView }, new List<string> { "SKUUID", "SKUCode", "SKULabel", "SKUImage" });

            if (selectionItems != null && selectionItems.Any()
                && !PromotionItemMapDictionary[promotionUID].Any(si => si.UID == salesOrderItemView.SKUUID))
            {
                PromotionItemMapDictionary[promotionUID].Add(selectionItems.FirstOrDefault()!);
            }
        }
    }
    public void SetItemPromotion(string promotionUID, ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.ApplicablePromotionUIDs == null)
        {
            salesOrderItemView.ApplicablePromotionUIDs = new HashSet<string>();
        }
        salesOrderItemView.ApplicablePromotionUIDs.Add(promotionUID);
        salesOrderItemView.IsPromo = true;

        CreatePromotionDictionary(promotionUID, salesOrderItemView);
    }
    public List<ISelectionItem>? GetPromotionListForItem(ISalesOrderItemView salesOrderItemView)
    {
        if (DMSPromotionDictionary == null || DMSPromotionDictionary.Count == 0
            || salesOrderItemView.ApplicablePromotionUIDs == null
            || salesOrderItemView.ApplicablePromotionUIDs.Count == 0)
        {
            return null;
        }

        List<DmsPromotion> dmsPromotions = DMSPromotionDictionary
        .Where(kv => salesOrderItemView.ApplicablePromotionUIDs.Contains(kv.Key))
        .Select(kv => kv.Value)
        .ToList();

        if (dmsPromotions == null)
        {
            return null;
        }

        List<ISelectionItem> selectionItems = CommonFunctions.ConvertToSelectionItems<DmsPromotion>(
            dmsPromotions, new List<string> { "UID", "Code", "Remarks" });
        return selectionItems;
    }

    public async Task ApplySpecialPromotionFilter()
    {
        FilterCriteriaList.Add(new FilterCriteria("ApplicablePromotionUIDs", SelectedPromotionUID, FilterType.In, typeof(HashSet<string>)));
        await ApplyFilter(FilterCriteriaList, FilterMode.And);
    }
    public async Task RemoveSpecialPromotionFilter()
    {
        FilterCriteriaList.RemoveAll(e => e.Name == "ApplicablePromotionUIDs");
        await ApplyFilter(FilterCriteriaList, FilterMode.And);
    }
    protected async Task PopulateSKUClass()
    {
        //TODO for MSL, Focus, ETC
        await Task.CompletedTask;
    }
    protected async Task PopulateStoreCheckData()
    {
        //TODO for StoreCheck data population
        await Task.CompletedTask;
    }
    /// <summary>
    /// This will seach data from SalesOrderItemViews and store in FilteredSalesOrderItemViews & DisplayedSalesOrderItemViews
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    /// 
    public virtual async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias,
        Shared.Models.Enums.FilterMode filterMode)
    {
        FilteredSalesOrderItemViews.Clear();
        DisplayedSalesOrderItemViews.Clear();
        FilteredSalesOrderItemViews.AddRange(SalesOrderItemViews); // Assuming all item will be available. Then will call filter logic for final data
        List<Model.UIInterfaces.ISalesOrderItemView> filteredItemsList = await ApplyFilterLogic(FilteredSalesOrderItemViews, filterCriterias, filterMode);
        DisplayedSalesOrderItemViews.AddRange(filteredItemsList);
    }
    /// <summary>
    /// Override this method to implement your own filter logic
    /// </summary>
    /// <param name="filteredItemsList"></param>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    public virtual async Task<List<ISalesOrderItemView>> ApplyFilterLogic(List<Model.UIInterfaces.ISalesOrderItemView> filteredItemsList,
        List<Shared.Models.Enums.FilterCriteria> filterCriterias,
        Shared.Models.Enums.FilterMode filterMode)
    {
        // Field Level Filter
        List<Shared.Models.Enums.FilterCriteria> filterCriteriasFieldLevel = filterCriterias.Where(e => e.FilterGroup == FilterGroupType.Field).ToList();
        if (filterCriteriasFieldLevel.Any())
        {
            filteredItemsList = await _filter.ApplyFilter<Model.UIInterfaces.ISalesOrderItemView>(filteredItemsList, filterCriteriasFieldLevel, filterMode);
        }

        // Attribute Level  
        List<Shared.Models.Enums.FilterCriteria> filterCriteriasAttributeLevel = filterCriterias.Where(e => e.FilterGroup == FilterGroupType.Attribute).ToList();
        if (filterCriteriasAttributeLevel != null && filterCriteriasAttributeLevel.Count > 0)
        {
            List<string> attruteNames = filterCriteriasAttributeLevel.Select(e => e.Name).Distinct().ToList();
            foreach (string attributeName in attruteNames)
            {
                List<FilterCriteria> filterCriteriasSelectedForAttribute = filterCriteriasAttributeLevel.Where(e => e.Name == attributeName).ToList();
                List<ISalesOrderItemView> tempSalesOrderItemViews = ApplyAttributeFilter(filteredItemsList, attributeName, filterCriteriasSelectedForAttribute);
                filteredItemsList = filteredItemsList.Intersect(tempSalesOrderItemViews).ToList();
            }
        }
        return filteredItemsList;
    }
    private List<ISalesOrderItemView> ApplyAttributeFilter(List<ISalesOrderItemView> salesOrderItemView, string attributeName, List<FilterCriteria> attributeValues)
    {
        if (attributeValues != null && attributeValues.Count > 0)
        {
            List<object> selectedValues = attributeValues.Select(e => e.Value).ToList();
            return salesOrderItemView.Where(e => e.Attributes.ContainsKey(attributeName) && selectedValues.Contains(e.Attributes[attributeName].Code)).ToList();
        }
        return salesOrderItemView;
    }
    /// <summary>
    /// This will search data from FilteredSalesOrderItemViews and store in DisplayedSalesOrderItemViews
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task ApplySearch(string searchString)
    { 
        DisplayedSalesOrderItemViews = new List<ISalesOrderItemView>();
        DisplayedSalesOrderItemViews.AddRange(await _filter.ApplySearch<Model.UIInterfaces.ISalesOrderItemView>(
            FilteredSalesOrderItemViews, searchString, _propertiesToSearch)
            );
        DisplayedSalesOrderItemViews = DisplayedSalesOrderItemViews
            .OrderBy(e => e.SKULabel)
            .ToList();
    }
    /// <summary>
    /// This will sort data from FilteredSalesOrderItemViews and store in FilteredSalesOrderItemViews & DisplayedSalesOrderItemViews
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
    {
        DisplayedSalesOrderItemViews = await _sorter.Sort<Model.UIInterfaces.ISalesOrderItemView>(
            DisplayedSalesOrderItemViews, sortCriterias);
    }
    public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForDDL(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
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
    public List<Shared.Models.Common.ISelectionItem> GetAvailableUOMForClone(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
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
    public async Task RemoveItemFromList(ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.SelectedUOM != null)
        {
            DisplayedSalesOrderItemViews.Remove(salesOrderItemView);
            FilteredSalesOrderItemViews.Remove(salesOrderItemView);
            SalesOrderItemViews.Remove(salesOrderItemView);
            salesOrderItemView.UsedUOMCodes.Remove(salesOrderItemView.SelectedUOM.Code);
            await ApplyPromotion();
            await UpdateHeader();
        }
    }
    public virtual void SetVanQty(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (OrderType == Winit.Shared.Models.Constants.OrderType.Vansales)
        {
            if (VanStock.ContainsKey(salesOrderItemView.SKUUID))
            {
                salesOrderItemView.VanQty = VanStock[salesOrderItemView.SKUUID];
            }
            else
            {
                salesOrderItemView.VanQty = 0;
            }

            //decimal orderQtyBU = SalesOrderItemViews
            //    .Where(e => e.SKUUID == salesOrderItemView.SKUUID)
            //    .Sum(e1 => e1.QtyBU);
            //if (VanStock.ContainsKey(salesOrderItemView.SKUUID))
            //{
            //    decimal totalAvaialableStock = VanStock[salesOrderItemView.SKUUID];
            //    if (totalAvaialableStock < orderQtyBU)
            //    {
            //        salesOrderItemView.VanQty = 0;
            //    }
            //    else
            //    {
            //        salesOrderItemView.VanQty = totalAvaialableStock - orderQtyBU;
            //    }
            //}
        }
    }
    public async Task ClearCartItems(List<ISalesOrderItemView> salesOrderItemViews)
    {
        foreach (var salesOrderItemView in salesOrderItemViews)
        {
            if (salesOrderItemView.Qty > 0)
            {
                salesOrderItemView.Qty = 0;
                salesOrderItemView.IsSelected = false;
                salesOrderItemView.IsCartItem = false;
                await OnQtyChange(salesOrderItemView);
            }
        }
    }
    public async virtual Task<bool> SaveSalesOrder(string StatusType = SalesOrderStatus.DRAFT)
    {
        bool retValue = false;
        try
        {
            Status = StatusType;
            //if (Source != Winit.Shared.Models.Constants.SourceType.CPE)
            //{
            //    _appUser.Emp =  new Emp.Model.Classes.Emp { UID = "VK" };
            //    _appUser.SelectedJobPosition = new Winit.Modules.JobPosition.Model.Classes.JobPosition { UID = "VK" };
            //}
            this.OrderDate = DateTime.Now;

            if (IsNewOrder)
            {
                this.ExpectedDeliveryDate = this.OrderDate.Date;
            }
            // SalesOrder
            ISalesOrder salesOrder = ConvertToISalesOrder(this);
            // SalesOrderLine
            List<ISalesOrderItemView> salesOrderItemViewList = SalesOrderItemViews
                .Where(e => e.IsCartItem)
                .ToList();
            List<ISalesOrderLine> salesOrderLines = ConvertToISalesOrderLine(salesOrderItemViewList);

            SalesOrderViewModelDCO salesOrderViewModel = new SalesOrderViewModelDCO();
            salesOrderViewModel.IsNewOrder = IsNewOrder;
            salesOrderViewModel.SalesOrder = (Winit.Modules.SalesOrder.Model.Classes.SalesOrder)salesOrder;
            salesOrderViewModel.SalesOrderLines = salesOrderLines.OfType<SalesOrderLine>().ToList();

            // AccPayable
            if (IsInvoiceGenerationRequired)
            {
                IAccPayable accPayable = ConvertToIAccPayable(this);
                salesOrderViewModel.AccPayable = (AccPayable)accPayable;
            }

            return await _salesOrderDataHelper.SaveSalesOrder_Order(salesOrderViewModel);
        }
        catch (Exception)
        {
            return retValue;
        }
    }
    protected void PrepareSignatureFields()
    {
        string baseFolder = Path.Combine(_appConfig.BaseFolderPath, FileSysTemplateControles.GetSignatureFolderPath("SalesOrder", SalesOrderUID));
        CustomerSignatureFolderPath = baseFolder;
        UserSignatureFolderPath = baseFolder;
        CustomerSignatureFileName = $"Customer_{SalesOrderUID}.png";
        UserSignatureFileName = $"User_{SalesOrderUID}.png";
    }
    public void OnSignatureProceedClick()
    {
        CustomerSignatureFilePath = Path.Combine(CustomerSignatureFolderPath, CustomerSignatureFileName);
        UserSignatureFilePath = Path.Combine(UserSignatureFolderPath, UserSignatureFileName);
    }
    public List<ISelectionItem>? PrepareMissedPromotionSelectionItem(string skuUID, List<string> missedPromotionUIDs)
    {
        List<DmsPromotion> dmsPromotions = DMSPromotionDictionary
        .Where(kv => missedPromotionUIDs.Contains(kv.Key))
        .Select(kv => kv.Value)
        .ToList();

        if (dmsPromotions == null || !dmsPromotions.Any())
        {
            return null;
        }

        List<ISelectionItem> selectionItems = CommonFunctions.ConvertToSelectionItems<DmsPromotion>(
            dmsPromotions, new List<string> { "UID", "Code", "Remarks" });

        if (selectionItems == null || !selectionItems.Any())
        {
            return null;
        }
        return selectionItems;

    }
    public async Task<(List<ISelectionItem>, Dictionary<string, List<ISelectionItem>>?)> PrepareMissingPromotion()
    {
        List<ISelectionItem> selectionItemList = null;
        Dictionary<string, List<ISelectionItem>>? missedPromotionDictionary = null;
        if (!SelectedStoreViewModel.IsPromotionsBlock)
        {

            // Loop through each salesOrderItemView where IsPromo is true
            foreach (var salesOrderItemView in SalesOrderItemViews.Where(e => e.IsPromo))
            {
                // Filter ApplicablePromotionUIDs by excluding those that are in the appliedPromotionUIDs list
                List<string> missedPromotionList = salesOrderItemView.ApplicablePromotionUIDs
                                                              .Where(uid => salesOrderItemView.AppliedPromotionUIDs == null
                                                              || !salesOrderItemView.AppliedPromotionUIDs.Contains(uid))
                                                              .ToList();

                if (missedPromotionList == null || missedPromotionList.Count == 0)
                {
                    continue;
                }

                if (selectionItemList == null)
                {
                    selectionItemList = new List<ISelectionItem>();
                }
                if (missedPromotionDictionary == null)
                {
                    missedPromotionDictionary = new Dictionary<string, List<ISelectionItem>>();
                }

                List<ISelectionItem> selectionItems = CommonFunctions.ConvertToSelectionItems<ISalesOrderItemView>(
                    new List<ISalesOrderItemView> { salesOrderItemView }, new List<string> { "SKUUID", "SKUCode", "SKULabel", "SKUImage" });

                if (selectionItems != null && selectionItems.Any())
                {
                    List<ISelectionItem> selectionItemsPromotionList = PrepareMissedPromotionSelectionItem(salesOrderItemView.SKUUID, missedPromotionList);

                    if (selectionItemsPromotionList != null && selectionItemsPromotionList.Any())
                    {
                        if (!selectionItemList.Any(e => e.UID == selectionItems.FirstOrDefault().UID))
                        {
                            selectionItemList.Add(selectionItems.FirstOrDefault());
                        }

                        if (!missedPromotionDictionary.ContainsKey(salesOrderItemView.SKUUID))
                        {
                            missedPromotionDictionary[salesOrderItemView.SKUUID] = selectionItemsPromotionList; ;
                        }
                    }
                }
            }
        }
        await Task.CompletedTask;
        return (selectionItemList, missedPromotionDictionary);
    }
    #endregion
    #region Promotion
    public async Task AddOrUpdateFOCSalesOrderItem(FOCItem fOCItem)
    {
        // Initialize the dictionary if it is null
        if (FOCSalesOrderItemViews == null)
        {
            FOCSalesOrderItemViews = new List<Model.UIInterfaces.ISalesOrderItemView>();
        }

        ISalesOrderItemView? existingSalesOrderItemView = FOCSalesOrderItemViews
            .FirstOrDefault(e => e.SKUUID == fOCItem.ItemCode && e.SelectedUOM?.Code == fOCItem.UOM);

        if (existingSalesOrderItemView != null)
        {
            existingSalesOrderItemView.Qty += fOCItem.Qty;
            existingSalesOrderItemView.QtyBU += existingSalesOrderItemView.Qty;
        }
        else
        {
            ISalesOrderItemView? salesOrderItemView = SalesOrderItemViews
            .FirstOrDefault(e => e.SKUUID == fOCItem.ItemCode);

            if (salesOrderItemView == null)
            {
                return;
            }

            ISKUUOMView? sKUUOM = salesOrderItemView.AllowedUOMs
                        .FirstOrDefault(u => u.Code == fOCItem.UOM);
            if (sKUUOM == null)
            {
                return;
            }

            ISalesOrderItemView focSalesOrderItemView = salesOrderItemView.Clone(sKUUOM, ItemState.FOC,
                Enum.GetName(ItemState.FOC) + fOCItem.UOM);

            if (focSalesOrderItemView == null)
            {
                return;
            }

            focSalesOrderItemView.IsCartItem = true;
            focSalesOrderItemView.UnitPrice = 0;
            focSalesOrderItemView.IsPromo = false;
            focSalesOrderItemView.Qty += fOCItem.Qty;
            await UpdateAmountAndTax(salesOrderItemView);
            FOCSalesOrderItemViews.Add(focSalesOrderItemView);
            await AddItemToList(SalesOrderItemViews, focSalesOrderItemView, false);
            await AddItemToList(FilteredSalesOrderItemViews, focSalesOrderItemView, false);
            await AddItemToList(DisplayedSalesOrderItemViews, focSalesOrderItemView, false);
        }
        await Task.CompletedTask;
    }


    #endregion
    #region OnQtyChange
    public async Task OnQtyChange(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        await ApplyQtyChange(salesOrderItemView);
        // Apply Promotion
        await ApplyPromotion();
        // Recalculate Amount And Tax after promotion
        await UpdateAmountAndTax(salesOrderItemView);
        // Update Header
        await UpdateHeader();
    }
    private PromotionHeaderView PreparePromotionHeaderView()
    {
        PromotionHeaderView promoHeaderView = new PromotionHeaderView();
        promoHeaderView.SalesOrderUID = SalesOrderUID;
        promoHeaderView.TotalAmount = TotalAmount;
        promoHeaderView.TotalQty = QtyCount;
        promoHeaderView.TotalDiscount = TotalDiscount;
        promoHeaderView.promotionItemView = new List<PromotionItemView>();
        foreach (ISalesOrderItemView salesOrderItemView in SalesOrderItemViews.Where(e => e.IsCartItem && e.ItemType != ItemType.FOC))
        {
            PromotionItemView promotionItemView = new PromotionItemView();
            promotionItemView.UniqueUId = salesOrderItemView.UID;
            promotionItemView.SKUUID = salesOrderItemView.SKUUID;
            promotionItemView.IsCartItem = salesOrderItemView.IsCartItem;
            promotionItemView.UOM = salesOrderItemView.SelectedUOM?.Code;
            promotionItemView.Multiplier = salesOrderItemView.SelectedUOM.Multiplier;
            promotionItemView.Qty = salesOrderItemView.Qty;
            promotionItemView.QtyBU = salesOrderItemView.QtyBU;
            promotionItemView.TotalAmount = salesOrderItemView.TotalAmount;
            promotionItemView.TotalDiscount = salesOrderItemView.TotalDiscount;
            promotionItemView.ItemType = Enum.GetName(salesOrderItemView.ItemType);
            //promotionItemView.ChildType = salesOrderItemView.UID;
            promotionItemView.IsDiscountApplied = true;
            promotionItemView.BasePrice = salesOrderItemView.BasePrice;
            promotionItemView.ReplacePrice = salesOrderItemView.UnitPrice;
            promotionItemView.UnitPrice = salesOrderItemView.UnitPrice;
            promotionItemView.PromotionUID = string.Empty;
            //promotionItemView.Attributes = salesOrderItemView.Attributes;
            promoHeaderView.promotionItemView.Add(promotionItemView);
        }
        return promoHeaderView;
    }
    public void ResetReplacePromotion(ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.ReplacePrice > 0)
        {
            salesOrderItemView.ReplacePrice = 0;
            salesOrderItemView.UnitPrice = salesOrderItemView.BasePrice;
        }
    }
    public void ResetFOCItems()
    {
        foreach (ISalesOrderItemView salesOrderItemView in FOCSalesOrderItemViews)
        {
            SalesOrderItemViews.Remove(salesOrderItemView);
            FilteredSalesOrderItemViews.Remove(salesOrderItemView);
            DisplayedSalesOrderItemViews.Remove(salesOrderItemView);
        }
        FOCSalesOrderItemViews.Clear();
    }
    private void RemoveAppliedPromotion()
    {
        foreach (ISalesOrderItemView salesOrderItemView in SalesOrderItemViews.Where(e => e.IsPromo == true))
        {
            //vk added on 5th Jul 2020
            salesOrderItemView.ItemType = ItemType.O;
            if (salesOrderItemView.AppliedPromotionUIDs != null)
            {
                salesOrderItemView.AppliedPromotionUIDs.Clear();
            }
            salesOrderItemView.TotalLineDiscount = 0;
            ResetReplacePromotion(salesOrderItemView);
            // Delete all FOC Item
            //DeleteFOCItemForPromotion(SO, salesOrderItemView, "");
        }
        ResetFOCItems();
    }
    public virtual async Task ApplyPromotion()
    {
        if (SelectedStoreViewModel.IsPromotionsBlock || ApplicablePromotionList == null
            || ApplicablePromotionList.Count == 0 || DMSPromotionDictionary == null || DMSPromotionDictionary.Count == 0)
        {
            return;
        }
        RemoveAppliedPromotion();

        PromotionHeaderView promoHeaderView = PreparePromotionHeaderView();
        string applicablePromotionUIDsFromDictionary = string.Join(",", ApplicablePromotionList);
        List<AppliedPromotionView> appliedPromotionViewList = _salesOrderDataHelper.ApplyPromotion(applicablePromotionUIDsFromDictionary, promoHeaderView,
            DMSPromotionDictionary, PromotionPriority.MinPriority);
        if (appliedPromotionViewList == null || appliedPromotionViewList.Count == 0)
        {
            return;
        }
        foreach (AppliedPromotionView appliedPromotionView in appliedPromotionViewList)
        {
            if (appliedPromotionView.IsFOC && appliedPromotionView.FOCItems != null && appliedPromotionView.FOCItems.Count > 0)
            {
                //FOC Promotion
                foreach (FOCItem fOCItem in appliedPromotionView.FOCItems)
                {
                    await AddOrUpdateFOCSalesOrderItem(fOCItem);
                }
            }
            else
            {
                ISalesOrderItemView salesOrderItemView = SalesOrderItemViews
                .FirstOrDefault(e => e.UID == appliedPromotionView.UniqueUID)!;

                if (salesOrderItemView == null)
                {
                    continue;
                }
                if (salesOrderItemView.AppliedPromotionUIDs == null)
                {
                    salesOrderItemView.AppliedPromotionUIDs = new HashSet<string>();
                }
                //salesOrderItemView.PromotionUID = appliedPromotionView.PromotionUID??"";
                salesOrderItemView.AppliedPromotionUIDs.Add(appliedPromotionView.PromotionUID!);

                //Discount Promotion. Multiple promotion can apply so appending Disount Amount
                salesOrderItemView.TotalLineDiscount += appliedPromotionView.DiscountAmount;
            }
            //TotalHeaderDiscount += appliedPromotionView.DiscountAmount;
        }
        await Task.CompletedTask;
    }
    public virtual async Task UpdateHeader()
    {
        await _orderLevelCalculator.ComputeOrderLevelTaxesAndOrderSummary();
    }
    public virtual async Task ApplyQtyChange(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (!string.IsNullOrEmpty(salesOrderItemView.SalesOrderLineUID))
        {
            // Order created then don't mark 
            //If Qty zero and UID there then mark for deletion
            //if (selectedItem.Qty == 0 && selectedItem.RecoQty == 0)
            //{
            //    selectedItem.salesOrderLineDeleteUIDList.Add(SalesOrderLineUID);
            //}
        }
        else
        {
            if (salesOrderItemView.Qty >= 0)
            {
                salesOrderItemView.IsCartItem = true;
            }
            else
            {
                salesOrderItemView.IsCartItem = false;
            }
        }
        ResetDiscount(salesOrderItemView);
        SetQtyBU(salesOrderItemView);
        SetMissedOrderQty(salesOrderItemView);
        await UpdateItemPrice(salesOrderItemView);
    }
    public virtual void ResetDiscount(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        //if (salesOrderItemView.AppliedPromotionUIDs != null)
        //{
        //    salesOrderItemView.AppliedPromotionUIDs.Clear();
        //}
        //salesOrderItemView.TotalLineDiscount = 0;
        salesOrderItemView.TotalCashDiscount = 0;

    }
    public void SetQtyBU(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.SelectedUOM == null)
        {
            return;
        }
        salesOrderItemView.QtyBU = salesOrderItemView.Qty * salesOrderItemView.SelectedUOM.Multiplier;
    }
    public virtual void SetMissedOrderQty(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (OrderType == Winit.Shared.Models.Constants.OrderType.Vansales)
        {
            if (salesOrderItemView.Qty > salesOrderItemView.VanQty)
            {
                salesOrderItemView.MissedQty = salesOrderItemView.Qty - salesOrderItemView.VanQty;
            }
            else
            {
                salesOrderItemView.MissedQty = 0;
            }
        }
    }
    public virtual async Task UpdateItemPrice(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.SelectedUOM == null)
        {
            return;
        }

        if (salesOrderItemView.ItemType == ItemType.FOC)
        {
            salesOrderItemView.UnitPrice = 0;
        }
        else if (!IsPriceEditable) // If price change not allowed then system should calculate else from external place it will be updated
        {
            salesOrderItemView.UnitPrice = salesOrderItemView.BasePrice * salesOrderItemView.SelectedUOM.Multiplier;
        }
        salesOrderItemView.BasePriceLabel = salesOrderItemView.BasePrice * salesOrderItemView.SelectedUOM.Multiplier;
        await UpdateAmountAndTax(salesOrderItemView); // This will be called after promotion calculation
    }
    public async Task UpdateUnitPriceByUOMChange(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (salesOrderItemView.SelectedUOM == null)
        {
            return;
        }
        if (IsPriceEditable)
        {
            salesOrderItemView.UnitPrice = salesOrderItemView.BasePrice * salesOrderItemView.SelectedUOM.Multiplier;
        }
        await OnQtyChange(salesOrderItemView);
    }
    public async Task UpdateUnitPrice(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView, decimal unitPrice)
    {
        if (IsPriceEditable)
        {
            salesOrderItemView.UnitPrice = unitPrice;
        }
        await OnQtyChange(salesOrderItemView);
    }
    public async Task UpdateAmountAndTax(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        salesOrderItemView.TotalAmount = CommonFunctions.RoundForSystem(salesOrderItemView.Qty * salesOrderItemView.UnitPrice, 2);
        salesOrderItemView.TotalLineTax = 0;
        salesOrderItemView.TotalHeaderTax = 0;
        if (salesOrderItemView.IsTaxable)
        {
            // Calculate Tax
            CalculateLineTax(salesOrderItemView);
        }
        await UpdateNetAmount(salesOrderItemView);
        await Task.CompletedTask;
    }
    public async Task UpdateNetAmount(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (IsPriceInclusiveVat)
        {
            salesOrderItemView.NetAmount = salesOrderItemView.TotalAmount - salesOrderItemView.TotalDiscount + salesOrderItemView.TotalExciseDuty;
        }
        else
        {
            salesOrderItemView.NetAmount = salesOrderItemView.TotalAmount - salesOrderItemView.TotalDiscount + salesOrderItemView.TotalExciseDuty + salesOrderItemView.TotalTax;
        }
        await Task.CompletedTask;
    }
    public virtual void CalculateLineTax(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        if (_salesOrderTaxCalculator == null)
        {
            return;
        }
        _salesOrderTaxCalculator.CalculateItemTaxes(salesOrderItemView);
        //salesOrderItemView.TotalLineTax = (salesOrderItemView.TotalAmount - salesOrderItemView.TotalDiscount) * .1m;
    }
    public void SetSelectedSalesOrderItemViews()
    {
        SelectedSalesOrderItemViews = SalesOrderItemViews
            .Where(e => e.IsCartItem && e.Qty > 0)
            .ToList();
    }
    public async Task ApplySearch_Preview(string searchString)
    {
        DisplayedSalesOrderItemViews_Preview.Clear();
        DisplayedSalesOrderItemViews_Preview.AddRange(await _filter.ApplySearch<Model.UIInterfaces.ISalesOrderItemView>(
            SelectedSalesOrderItemViews, searchString, _propertiesToSearch)
            );
    }
    #endregion
    #region Notes
    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }
    #endregion
    #region LPO Number
    public void UpdateCustomerPO(string customerPO)
    {
        CustomerPO = customerPO;
    }
    #endregion
    #region CashDiscount
    public async Task CalculateCashDiscount(decimal discountValue)
    {
        await _cashDiscountCalculator.CalculateCashDiscount(discountValue);
    }
    #endregion
    #region Validation
    public virtual void ValidateSalesOrderViewModel()
    {
        if (SelectedStoreViewModel == null)
        {
            throw new Exception("storeViewModel is null");
        }
        else if (string.IsNullOrEmpty(SelectedStoreViewModel.SelectedDistributionChannelUID))
        {
            throw new Exception("Distribution Channel should not be null");
        }
    }
    private Dictionary<string, string> Validate(Winit.Modules.SalesOrder.Model.UIInterfaces.ISalesOrderItemView salesOrderItemView)
    {
        Dictionary<string, string> errorMessage = new Dictionary<string, string>();
        if (salesOrderItemView.SelectedUOM == null)
        {
            errorMessage["UOM"] = "UOM must be selected.";
        }
        return errorMessage;
    }
    #endregion
    #region Conversion
    private void UpdateSequenceNumber()
    {
        if (Status == SalesOrderStatus.DRAFT && string.IsNullOrEmpty(DraftOrderNumber))
        {
            DraftOrderNumber = "DO" + DateTime.Now.ToString("ddMMyyyyHHmmss");
        }
        else if (Status == SalesOrderStatus.FINALIZED && string.IsNullOrEmpty(DraftOrderNumber))
        {
            DraftOrderNumber = "T" + DateTime.Now.ToString("ddMMyyyyHHmmss");
        }
        else if (Status == SalesOrderStatus.DELIVERED && string.IsNullOrEmpty(SalesOrderNumber))
        {
            SalesOrderNumber = "SO" + DateTime.Now.ToString("ddMMyyyyHHmmss");
        }
    }
    public virtual Model.Interfaces.ISalesOrder ConvertToISalesOrder(Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModel viewModel)
    {
        Model.Interfaces.ISalesOrder salesOrder = _serviceProvider.CreateInstance<Model.Interfaces.ISalesOrder>();
        salesOrder.Id = viewModel.Id;
        if (IsNewOrder && string.IsNullOrEmpty(SalesOrderUID))
        {
            SalesOrderUID = Guid.NewGuid().ToString();
            salesOrder.SS = SSValues.One;
        }
        else
        {
            salesOrder.SS = SSValues.Two;
        }
        UpdateSequenceNumber();
        salesOrder.UID = viewModel.SalesOrderUID;
        salesOrder.CreatedBy = viewModel.EmpUID;
        salesOrder.CreatedTime = DateTime.Now;
        salesOrder.ModifiedBy = viewModel.EmpUID;
        salesOrder.ModifiedTime = DateTime.Now;
        salesOrder.ServerAddTime = null;
        salesOrder.ServerModifiedTime = null;
        salesOrder.SalesOrderNumber = viewModel.SalesOrderNumber;
        salesOrder.DraftOrderNumber = viewModel.DraftOrderNumber;
        salesOrder.CompanyUID = null;
        salesOrder.OrgUID = viewModel.OrgUID;
        salesOrder.DistributionChannelUID = viewModel.DistributionChannelOrgUID;
        if (OrderType == Winit.Shared.Models.Constants.OrderType.Presales)
        {

            salesOrder.DeliveredByOrgUID = DeliveredByOrgUID;
        }
        else
        {
            salesOrder.DeliveredByOrgUID = viewModel.OrgUID;
        }
        salesOrder.StoreUID = viewModel.StoreUID;
        salesOrder.Status = viewModel.Status;
        salesOrder.OrderType = viewModel.OrderType;
        salesOrder.OrderDate = viewModel.OrderDate;
        salesOrder.CustomerPO = viewModel.CustomerPO;
        salesOrder.CurrencyUID = viewModel.CurrencyUID;
        //salesOrder.PaymentType = viewModel.PaymentType;
        salesOrder.TotalAmount = viewModel.TotalAmount;
        salesOrder.TotalDiscount = viewModel.TotalDiscount;
        salesOrder.TotalTax = viewModel.TotalTax;
        salesOrder.NetAmount = viewModel.NetAmount;
        salesOrder.LineCount = viewModel.LineCount;
        salesOrder.QtyCount = (int)viewModel.QtyCount;
        salesOrder.TotalFakeAmount = viewModel.TotalAmount;
        salesOrder.ReferenceNumber = viewModel.ReferenceNumber;
        salesOrder.Source = viewModel.Source;
        salesOrder.ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate;
        salesOrder.TotalLineDiscount = viewModel.TotalLineDiscount;
        salesOrder.TotalCashDiscount = viewModel.TotalCashDiscount;
        salesOrder.TotalHeaderDiscount = viewModel.TotalHeaderDiscount;
        salesOrder.TotalExciseDuty = viewModel.TotalExciseDuty;
        salesOrder.TotalLineTax = viewModel.TotalLineTax;
        salesOrder.TotalHeaderTax = viewModel.TotalHeaderTax;
        salesOrder.CashSalesAddress = viewModel.Address;
        salesOrder.CashSalesCustomer = viewModel.CustomerName;
        salesOrder.JobPositionUID = viewModel.JobPositionUID;
        salesOrder.EmpUID = viewModel.EmpUID;
        salesOrder.BeatHistoryUID = viewModel.BeatHistoryUID; // Will be populated from Journey Plan View
        salesOrder.RouteUID = viewModel.RouteUID; // Will be populated from Journey Plan View
        salesOrder.StoreHistoryUID = viewModel.StoreHistoryUID; // Will be populated from Journey Plan View
        salesOrder.TotalCreditLimit = viewModel.SelectedStoreViewModel.TotalCreditLimit;
        salesOrder.AvailableCreditLimit = viewModel.SelectedStoreViewModel.AvailableCreditLimit;
        salesOrder.ExpectedDeliveryDate = viewModel.ExpectedDeliveryDate;
        salesOrder.DeliveredDateTime = DateTime.Now;
        salesOrder.Latitude = "0"; // Current latitude from location
        salesOrder.Longitude = "0"; // Current latitude from location
        salesOrder.IsOffline = true; // Will be updated based on device status
        salesOrder.CreditDays = viewModel.CreditDays;
        salesOrder.Notes = viewModel.Notes;
        salesOrder.DeliveryInstructions = "";// Will be populated from Journey Plan View
        salesOrder.Remarks = viewModel.Remarks;
        salesOrder.IsTemperatureCheckEnabled = viewModel.SelectedStoreViewModel.IsTemperatureCheckEnabled;
        salesOrder.AlwaysPrintedFlag = viewModel.SelectedStoreViewModel.AlwaysPrintedFlag;
        salesOrder.PurchaseOrderNoRequiredFlag = viewModel.SelectedStoreViewModel.PurchaseOrderNoRequiredFlag;
        salesOrder.IsWithPrintedInvoicesFlag = viewModel.SelectedStoreViewModel.IsWithPrintedInvoicesFlag;
        salesOrder.DefaultBatchNumber = _appUser.DefaultBatchNumber;
        salesOrder.DefaultStockVersion = _appUser.DefaultStockVersion;
        salesOrder.VehicleUID = viewModel.VehicleUID;
        salesOrder.IsStockUpdateRequired = viewModel.IsStockUpdateRequired;
        salesOrder.IsInvoiceGenerationRequired = viewModel.IsInvoiceGenerationRequired;
        return salesOrder;
    }

    public virtual Model.Interfaces.ISalesOrderLine ConvertToISalesOrderLine(ISalesOrderItemView salesOrderItemView, int lineNumber)
    {
        Model.Interfaces.ISalesOrderLine salesOrderLine = _serviceProvider.CreateInstance<Model.Interfaces.ISalesOrderLine>();
        salesOrderLine.Id = salesOrderItemView.Id;
        salesOrderLine.UID = salesOrderItemView.UID;
        salesOrderLine.SalesOrderLineUID = salesOrderItemView.SalesOrderLineUID;
        salesOrderLine.SS = SSValues.One;
        salesOrderLine.CreatedBy = EmpUID;
        salesOrderLine.CreatedTime = DateTime.Now;
        salesOrderLine.ModifiedBy = EmpUID;
        salesOrderLine.ModifiedTime = DateTime.Now;
        salesOrderLine.ServerAddTime = null;
        salesOrderLine.ServerModifiedTime = null;
        salesOrderLine.SalesOrderUID = SalesOrderUID;
        salesOrderLine.LineNumber = lineNumber;
        salesOrderLine.ItemCode = salesOrderItemView.SKUCode;
        salesOrderLine.ItemType = salesOrderItemView.ItemType.ToString();
        salesOrderLine.BasePrice = salesOrderItemView.BasePrice;
        salesOrderLine.UnitPrice = salesOrderItemView.UnitPrice;
        salesOrderLine.FakeUnitPrice = salesOrderItemView.UnitPrice;
        salesOrderLine.BaseUOM = salesOrderItemView.BaseUOM;
        salesOrderLine.UoM = salesOrderItemView.SelectedUOM.Code;
        salesOrderLine.UOMConversionToBU = salesOrderItemView.SelectedUOM.Multiplier;
        salesOrderLine.RecoUOM = salesOrderItemView.OrderUOM;
        salesOrderLine.RecoQty = salesOrderItemView.OrderQty;
        salesOrderLine.RecoUOMConversionToBU = salesOrderItemView.OrderUOMMultiplier;
        salesOrderLine.RecoQtyBU = salesOrderItemView.OrderQty * salesOrderItemView.OrderUOMMultiplier;
        salesOrderLine.ModelQtyBU = salesOrderItemView.RCQty;
        salesOrderLine.VanQtyBU = salesOrderItemView.VanQty;
        salesOrderLine.Qty = salesOrderItemView.Qty;
        salesOrderLine.QtyBU = salesOrderItemView.QtyBU;
        if (Status == SalesOrderStatus.DELIVERED)
        {
            salesOrderLine.DeliveredQty = salesOrderItemView.Qty;
        }
        salesOrderLine.MissedQty = salesOrderItemView.MissedQty;
        salesOrderLine.ReturnedQty = 0;
        salesOrderLine.TotalAmount = salesOrderItemView.TotalAmount;
        salesOrderLine.TotalDiscount = salesOrderItemView.TotalDiscount;
        salesOrderLine.LineTaxAmount = salesOrderItemView.TotalLineTax;
        salesOrderLine.ProrataTaxAmount = salesOrderItemView.TotalHeaderTax;
        salesOrderLine.TotalTax = salesOrderItemView.TotalTax;
        salesOrderLine.NetAmount = salesOrderItemView.NetAmount;
        salesOrderLine.NetFakeAmount = salesOrderItemView.NetAmount;
        salesOrderLine.SKUPriceUID = salesOrderItemView.SKUPriceUID;
        salesOrderLine.ProrataDiscountAmount = salesOrderItemView.TotalHeaderDiscount;
        //salesOrderLine.CashDiscount = salesOrderItemView.TotalCashDiscount;
        salesOrderLine.LineDiscountAmount = salesOrderItemView.TotalLineDiscount;
        salesOrderLine.MRP = salesOrderItemView.MRP;
        salesOrderLine.CostUnitPrice = 0;
        //salesOrderLine.ParentUID = string.Empty;
        salesOrderLine.IsPromotionApplied = !SelectedStoreViewModel.IsPromotionsBlock;
        salesOrderLine.Volume = 0;
        salesOrderLine.VolumeUnit = string.Empty;
        salesOrderLine.Weight = 0;
        salesOrderLine.WeightUnit = string.Empty;
        salesOrderLine.StockType = StockType.Salable.ToString();
        salesOrderLine.Remarks = salesOrderItemView.Remarks;
        salesOrderLine.TotalCashDiscount = salesOrderItemView.TotalCashDiscount;
        salesOrderLine.TotalExciseDuty = salesOrderItemView.TotalExciseDuty;
        salesOrderLine.SKUUID = salesOrderItemView.SKUUID;
        return salesOrderLine;
    }
    public virtual List<Model.Interfaces.ISalesOrderLine> ConvertToISalesOrderLine(List<ISalesOrderItemView> salesOrderItemViewList)
    {
        List<Model.Interfaces.ISalesOrderLine>? salesOrderLines = null;
        if (salesOrderItemViewList != null && salesOrderItemViewList.Count > 0)
        {
            salesOrderLines = new List<Model.Interfaces.ISalesOrderLine>();
            int linenumber = 1;
            foreach (SalesOrderItemView salesOrderItemView in salesOrderItemViewList)
            {
                Model.Interfaces.ISalesOrderLine salesOrderLine = ConvertToISalesOrderLine(salesOrderItemView, linenumber);
                //ToDo
                linenumber++;
                salesOrderLines.Add(salesOrderLine);
            }
        }
        return salesOrderLines;
    }
    public virtual IAccPayable ConvertToIAccPayable(Winit.Modules.SalesOrder.BL.UIInterfaces.ISalesOrderViewModel viewModel)
    {
        IAccPayable accPayable = _serviceProvider.CreateInstance<IAccPayable>();
        accPayable.Id = 0;
        accPayable.UID = SalesOrderUID;
        accPayable.SS = 1;
        accPayable.CreatedBy = viewModel.EmpUID;
        accPayable.CreatedTime = DateTime.Now;
        accPayable.ModifiedBy = viewModel.EmpUID;
        accPayable.ModifiedTime = DateTime.Now;
        accPayable.ServerAddTime = null;
        accPayable.ServerModifiedTime = null;
        accPayable.SourceType = InvoiceSourceType.Invoice;
        accPayable.SourceUID = viewModel.SalesOrderUID;
        accPayable.ReferenceNumber = viewModel.SalesOrderNumber;
        accPayable.OrgUID = viewModel.OrgUID;
        accPayable.JobPositionUID = viewModel.JobPositionUID;
        accPayable.Amount = viewModel.NetAmount;
        accPayable.PaidAmount = 0;
        accPayable.StoreUID = viewModel.StoreUID;
        accPayable.TransactionDate = viewModel.OrderDate;
        accPayable.DueDate = viewModel.OrderDate; // Will be calculated based on customer Payment term
        accPayable.BalanceAmount = viewModel.NetAmount;
        accPayable.UnSettledAmount = 0;
        accPayable.Source = SourceType.APP;
        accPayable.CurrencyUID = viewModel.CurrencyUID;
        return accPayable;
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
    public virtual ISKUAttributeView ConvertToISKUAttributeView(ISKUAttributes sKUAttribute)
    {
        ISKUAttributeView sKUAttributeView = _serviceProvider.CreateInstance<ISKUAttributeView>();
        sKUAttributeView.SKUUID = sKUAttribute.SKUUID;
        sKUAttributeView.Name = sKUAttribute.Type;
        sKUAttributeView.Code = sKUAttribute.Code;
        sKUAttributeView.Value = sKUAttribute.Value;
        return sKUAttributeView;
    }
    public virtual Dictionary<string, ISKUAttributeView>? ConvertToISKUAttributeView(List<ISKUAttributes> sKUAttributes)
    {
        Dictionary<string, ISKUAttributeView>? ISKUAttributeViews = null;
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
    public virtual ISalesOrderItemView ConvertToISalesOrderItemView(ISKUMaster sKUMaster, int lineNumber, List<string>? skuImages = null)
    {
        Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.FirstOrDefault();
        List<ISKUUOMView>? sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
        ISKUUOMView? defaultUOM = sKUUOMViews
                        ?.FirstOrDefault(e => e.Code == sKUConfig?.SellingUOM);
        ISKUUOMView? baseUOM = sKUUOMViews
                        ?.FirstOrDefault(e => e.IsBaseUOM);
        ISalesOrderItemView salesOrderItem = _serviceProvider.CreateInstance<ISalesOrderItemView>();
        salesOrderItem.UID = Guid.NewGuid().ToString();
        salesOrderItem.LineNumber = lineNumber;
        salesOrderItem.SKUUID = sKUMaster.SKU.UID;
        salesOrderItem.SKUCode = sKUMaster.SKU.Code;
        salesOrderItem.SKUName = sKUMaster.SKU.Name;
        salesOrderItem.SKULabel = sKUMaster.SKU.Name;
        salesOrderItem.IsMCL = false;
        salesOrderItem.IsPromo = false;
        salesOrderItem.IsNPD = false;
        salesOrderItem.SCQty = 0;
        salesOrderItem.RCQty = 0;
        salesOrderItem.VanQty = 0;
        salesOrderItem.MaxQty = DefaultMaxQty;
        salesOrderItem.OrderQty = 0;
        salesOrderItem.Qty = 0;
        salesOrderItem.QtyBU = 0;
        salesOrderItem.DeliveredQty = 0;
        salesOrderItem.MissedQty = 0;
        salesOrderItem.BaseUOM = baseUOM?.Code;
        salesOrderItem.SelectedUOM = defaultUOM;
        salesOrderItem.AllowedUOMs = sKUUOMViews;
        //UsedUOMCodes = new List<string>();
        salesOrderItem.BasePrice = 0;
        salesOrderItem.UnitPrice = 0;
        salesOrderItem.IsTaxable = SelectedStoreViewModel?.IsTaxApplicable ?? false;
        salesOrderItem.ApplicableTaxes = sKUMaster.ApplicableTaxUIDs;
        salesOrderItem.TotalAmount = 0;
        salesOrderItem.TotalLineDiscount = 0;
        salesOrderItem.TotalCashDiscount = 0;
        salesOrderItem.TotalHeaderDiscount = 0;
        salesOrderItem.TotalExciseDuty = 0;
        salesOrderItem.TotalLineTax = 0;
        salesOrderItem.TotalHeaderTax = 0;
        salesOrderItem.SKUPriceUID = null;
        salesOrderItem.SKUPriceListUID = null;
        salesOrderItem.Attributes = ConvertToISKUAttributeView(sKUMaster.SKUAttributes);
        salesOrderItem.ItemStatus = ItemState.Primary;
        salesOrderItem.ApplicablePromotionUIDs = null;
        salesOrderItem.PromotionUID = string.Empty;
        salesOrderItem.CurrencyLabel = this.CurrencyLabel;
        salesOrderItem.CatalogueURL = sKUMaster.SKU.CatalogueURL;
        salesOrderItem.SKUImages = skuImages;
        if (sKUMaster.SKUAttributes != null)
        {
            sKUMaster.SKUAttributes.ForEach(e => salesOrderItem.FilterKeys.Add(e.Code));
        }
        if (!string.IsNullOrEmpty(sKUMaster.SKU.SKUImage))
            salesOrderItem.SKUImage = Path.Combine(_appConfig.ApiDataBaseUrl, sKUMaster.SKU.SKUImage.ToString());
        else
        {
            salesOrderItem.SKUImage = "/Data/SKU/no_image_available.jpg";
        }

        return salesOrderItem;
    }
    public List<ISalesOrderItemView> ConvertToISalesOrderItemView(List<ISKUMaster> sKUMasters, List<IFileSys>? fileSysList = null)
    {
        List<ISalesOrderItemView> salesOrderItems = null;
        if (sKUMasters != null && sKUMasters.Count > 0)
        {
            salesOrderItems = new List<ISalesOrderItemView>();
            int lineNumber = 1;
            foreach (var sKUMaster in sKUMasters)
            {
                IEnumerable<string>? skuImages = null;
                if (fileSysList != null && fileSysList.Any())
                {
                    skuImages = fileSysList
                        .FindAll(e => e.LinkedItemUID == sKUMaster.SKU.UID)
                        .OrderByDescending(e1 => e1.IsDefault)
                        .Select(e2 => Path.Combine(e2.RelativePath, e2.FileName));
                }
                ISalesOrderItemView salesOrderItemView = ConvertToISalesOrderItemView(sKUMaster, lineNumber, skuImages?.ToList());
                SetVanQty(salesOrderItemView);
                salesOrderItems.Add(salesOrderItemView);
                lineNumber++;
            }
        }

        return salesOrderItems;
    }
    #endregion
    #region Virualmethods for Business Logics
    public virtual Task OnRouteSelect(string routeUID)
    {
        return Task.CompletedTask;
    }
    public virtual Task OnStoreItemViewSelected(string storeItemViewUID)
    {
        return Task.CompletedTask;
    }
    public virtual Task AddSelectedProducts(List<ISalesOrderItemView> salesOrderItemViews)
    {
        return Task.CompletedTask;
    }
    public virtual Task InitializeDropDownsEditPage(ISalesOrder salesOrder)
    {
        return Task.CompletedTask;
    }
    #endregion

    public virtual Task<bool> UpdateSalesOrderStatus(string status)
    {
        throw new NotImplementedException();
    }
    protected void SetPriceMaster(IEnumerable<ISKUPrice> sKUPrices)
    {
        var salesOrderItemDict = SalesOrderItemViews.ToDictionary(item => item.SKUUID);
        HashSet<string> updatedSkuUids = new();
        foreach (ISKUPrice sKUPrice in sKUPrices)
        {
            if (sKUPrice.Price != 0 && salesOrderItemDict.TryGetValue(sKUPrice.SKUUID, out var salesOrderItem))
            {
                salesOrderItem.BasePrice = sKUPrice.Price;
                salesOrderItem.UnitPrice = sKUPrice.Price;
                salesOrderItem.MRP = sKUPrice.MRP;
                salesOrderItem.SKUPriceUID = sKUPrice.UID;
                salesOrderItem.SKUPriceListUID = sKUPrice.SKUPriceListUID;
                salesOrderItem.PriceVersionNo = sKUPrice.VersionNo;
                salesOrderItem.PriceLowerLimit = sKUPrice.PriceLowerLimit;
                salesOrderItem.PriceUpperLimit = sKUPrice.PriceUpperLimit;

                updatedSkuUids.Add(sKUPrice.SKUUID);
            }
        }
        //foreach (ISKUPrice sKUPrice in sKUPrices)
        //{
        //    // Find the corresponding item in SalesOrderItemViews by matching some identifier, e.g., SKU or ID
        //    var salesOrderItem = SalesOrderItemViews.Find(item => item.SKUUID == sKUPrice.SKUUID);

        //    if (salesOrderItem != null)
        //    {
        //        // Update the price in SalesOrderItemViews
        //        salesOrderItem.BasePrice = sKUPrice.Price;
        //        salesOrderItem.UnitPrice = sKUPrice.Price;
        //        salesOrderItem.MRP = sKUPrice.MRP;
        //        salesOrderItem.SKUPriceUID = sKUPrice.UID;
        //        salesOrderItem.SKUPriceListUID = sKUPrice.SKUPriceListUID;
        //        salesOrderItem.PriceVersionNo = sKUPrice.VersionNo;
        //        salesOrderItem.PriceLowerLimit = sKUPrice.PriceLowerLimit;
        //        salesOrderItem.PriceUpperLimit = sKUPrice.PriceUpperLimit;
        //    }
        //}
        //Remove items where price = 0
        //SalesOrderItemViews.RemoveAll(item => item.BasePrice == 0);
        SalesOrderItemViews = SalesOrderItemViews
    .FindAll(item => updatedSkuUids.Contains(item.SKUUID));
    }

}
