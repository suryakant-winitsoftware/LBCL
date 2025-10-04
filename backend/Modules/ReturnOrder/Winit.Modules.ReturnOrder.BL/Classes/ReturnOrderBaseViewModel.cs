using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;



namespace Winit.Modules.ReturnOrder.BL.Classes;

public abstract class ReturnOrderBaseViewModel : Winit.Modules.ReturnOrder.BL.Interfaces.IReturnOrderViewModel
{
    public int Id { get; set; }
    public string ReturnOrderUID { get; set; }
    public string ReturnOrderNumber { get; set; }
    public string DraftOrderNumber { get; set; }
    public string DistributionChannelOrgUID { get; set; }
    public string JobPositionUID { get; set; }
    public string OrderType { get; set; } = "withoutinvoice";
    public string StoreUID { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrgUID { get; set; }
    public string SalesOrderUID { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViews { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViewsRawdata { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> FilteredReturnOrderItemViews { get; set; }
    public List<Model.Interfaces.IReturnOrderItemView> DisplayedReturnOrderItemViews { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SkuAttributesList { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUList { get; set; }
    public List<IStoreItemView> StoreItemViews { get; set; }
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
    public bool IsTaxable { get; set; }
    public string Status { get; set; }
    public string Source { get; set; }
    public string CurrencyUID { get; set; }
    public bool IsTaxApplicable { get; set; }
    public bool IsViewMode { get; set; }
    public string RouteUID { get; set; }
    public string BeatHistoryUID { get; set; }
    public string StoreHistoryUID { get; set; }
    /// <summary>
    /// Will be used to show Currency Label
    /// </summary>
    public string CurrencyLabel { get; set; }
    public decimal LineCount { get; set; }
    public decimal QtyCount { get; set; }
    public string Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalLineDiscount { get; set; }
    public decimal TotalCashDiscount { get; set; }
    public decimal TotalHeaderDiscount { get; set; }
    /// <summary>
    /// TotalDiscount = TotalLineDiscount + TotalCashDiscount + TotalHeaderDiscount
    /// </summary>
    public decimal TotalDiscount { get { return TotalLineDiscount + TotalCashDiscount + TotalHeaderDiscount; } }
    public decimal TotalExciseDuty { get; set; }
    public decimal LineTaxAmount { get; set; }
    public decimal HeaderTaxAmount { get; set; }
    public decimal TotalLineTax { get; set; }
    public decimal TotalHeaderTax { get; set; }
    /// <summary>
    /// TotalTax = LineTaxAmount + HeaderTaxAmount
    /// </summary>
    public decimal TotalTax { get { return LineTaxAmount + HeaderTaxAmount; } }
    /// <summary>
    /// NetAmount = TotalAmount - TotalDiscount + ExciseDuty + TotalTax
    /// </summary>
    public decimal NetAmount { get { return TotalAmount - TotalDiscount + TotalExciseDuty + TotalTax; } }
    public Dictionary<StockType, List<ISelectionItem>> ReasonMap { get; set; } =
        new Dictionary<StockType, List<ISelectionItem>>();
    public Winit.Modules.Store.Model.Interfaces.IStoreItemView SelectedStoreViewModel { get; set; }
    public bool IsNewOrder = true;
    public bool IsPriceInclusiveVat = true;
    public bool IsPromotionBlocked { get; set; } = true;

    #region Signature Fields
    public List<IFileSys> SignatureFileSysList { get; set; }
    public bool IsSignaturesCaptured { get; set; }
    public string CustomerSignatureFileName { get; set; }
    public string CustomerSignatureFolderPath { get; set; }
    public string UserSignatureFileName { get; set; }
    public string UserSignatureFolderPath { get; set; }
    #endregion

    // Injection
    private IServiceProvider _serviceProvider;
    protected readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private ITaxCalculator _taxCalculator;
    //private readonly IPromotionManager _promotionManager;
    private readonly Interfaces.IReturnOrderAmountCalculator _amountCalculator;
    private readonly IListHelper _listHelper;
    IEnumerable<ISKUMaster> SKUMasterList;
    protected readonly IAppUser _appUser;
    private List<string> _propertiesToSearch = new List<string>();
    protected Winit.Shared.Models.Common.IAppConfig _appConfigs;
    private Winit.Modules.Base.BL.ApiService _apiService;


    //Constructor
    protected ReturnOrderBaseViewModel(IServiceProvider serviceProvider,
            IFilterHelper filter,
            ISortHelper sorter,
            Interfaces.IReturnOrderAmountCalculator amountCalculator,
            IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs,
            Winit.Modules.Base.BL.ApiService apiService,
            IAppUser appUser)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _amountCalculator = amountCalculator;
        _listHelper = listHelper;
        _apiService = apiService;
        _appConfigs = appConfigs;
        _appUser = appUser;

        // Initialize common properties or perform other common setup
        ReturnOrderItemViewsRawdata = new List<Model.Interfaces.IReturnOrderItemView>();
        ReturnOrderItemViews = new List<Model.Interfaces.IReturnOrderItemView>();
        FilteredReturnOrderItemViews = new List<Model.Interfaces.IReturnOrderItemView>();
        DisplayedReturnOrderItemViews = new List<Model.Interfaces.IReturnOrderItemView>();
        SkuAttributesList = new List<ISKUAttributes>();
        SKUList = new List<SKU.Model.Interfaces.ISKU>();
        RouteSelectionItems = new List<ISelectionItem>();
        StoreSelectionItems = new List<ISelectionItem>();
        StoreItemViews = new List<IStoreItemView>();
        OrderDate = DateTime.Now;
        SignatureFileSysList = new List<IFileSys>();
        // Property set for Search
        _propertiesToSearch.Add("SKUCode");
        _propertiesToSearch.Add("SKUName");
    }
    public virtual void InitializeOptionalDependencies()
    {
        if (IsTaxable)
        {
            _taxCalculator = _serviceProvider.CreateInstance<ITaxCalculator>();
        }
    }
    /// <summary>
    /// To be called after object creation
    /// </summary>
    /// 
    public virtual async Task PopulateViewModel(string source, bool isNewOrder = true, string returnOrderUID = "")
    {

        IsNewOrder = isNewOrder;
        ReturnOrderUID = returnOrderUID;
        Source = source;
        Status = Shared.Models.Constants.SalesOrderStatus.DRAFT;
        await PopulateStoreData(SelectedStoreViewModel);
        await PopulateReasons();
        await PopulateSKUMaster();
        InitializeOptionalDependencies();
        await SetSystemSettingValues();
        await PopulatePriceMaster();
        if (!returnOrderUID.IsNullOrEmpty())
        {
            await PopulateReturnOrder();
        }
        else
        {
            ReturnOrderUID = Guid.NewGuid().ToString();
        }
        await ApplyFilter(new List<Shared.Models.Enums.FilterCriteria>(), FilterMode.And);
        List<SortCriteria> sortCriteriaList = new List<SortCriteria>();
        sortCriteriaList.Add(new SortCriteria("SKUName", SortDirection.Asc));
        await ApplySort(sortCriteriaList);
        IsTaxable = true;
    }

    protected async virtual Task PopulateSKUMaster()
    {
        List<ISKUMaster> skuMasters = await SKUMasters_Data(_appUser.OrgUIDs);
        SkuAttributesList.AddRange(skuMasters.SelectMany(e => e.SKUAttributes.ToList()).Where(attr => attr != null));
        SKUList.AddRange(skuMasters.Select(e => e.SKU).Where(sku => sku != null));
        List<IReturnOrderItemView> returnOrderItemViews = ConvertToIReturnOrderItemView(skuMasters.ToList());
        if (returnOrderItemViews != null)
        {
            ReturnOrderItemViewsRawdata.AddRange(returnOrderItemViews);
        }
    }
    protected virtual async Task PopulateReturnOrder()
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(ReturnOrderUID))
        {
            Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderMaster returnOrderMaster = await GetReturnOrder_Data();
            if (returnOrderMaster != null)
            {
                ConvertReturnOrderToReturnOrderViewModel(returnOrderMaster.ReturnOrder);
                foreach (var returnOrderLine in returnOrderMaster.ReturnOrderLineList)
                {
                    //ReturnOrderItemViews.Add(ConvertReturnOrderLineToReturnOrderItemView(returnOrderLine));
                    OverideReturnOrderItemView(returnOrderLine);
                }
            }
        }
    }
    public void AddProductToGrid(List<IReturnOrderItemView> returnOrderItemViews)
    {
        ClearAllLists();
        ReturnOrderItemViews.AddRange(returnOrderItemViews);
        FilteredReturnOrderItemViews.AddRange(returnOrderItemViews);
        DisplayedReturnOrderItemViews.AddRange(returnOrderItemViews);
    }
    public void ClearAllLists()
    {
        ReturnOrderItemViews.Clear();
        FilteredReturnOrderItemViews.Clear();
        DisplayedReturnOrderItemViews.Clear();
    }
    public virtual async Task SetSystemSettingValues()
    {
        // SetDefaultDeliveryDay();
        SetCurrency();
        await Task.Delay(0);
    }

    private void SetCurrency()
    {
        CurrencyUID = _appUser.DefaultOrgCurrency?.CurrencyUID;
        CurrencyLabel = _appUser.DefaultOrgCurrency?.Symbol;
    }
    public async Task AddItemToList(List<Model.Interfaces.IReturnOrderItemView> ReturnOrderItemViews,
    ReturnOrder.Model.Interfaces.IReturnOrderItemView item, bool addAtEnd = true)
    {
        if (addAtEnd)
        {
            await _listHelper.AddItemToList(ReturnOrderItemViews, item, null);
        }
        else
        {
            await _listHelper.AddItemToList(ReturnOrderItemViews, item, (T1, T2) => T1.SKUCode == T2.SKUCode);
        }

    }
    /// <summary>
    /// This will seach data from ReturnOrderItemViews and store in FilteredReturnOrderItemViews & DisplayedReturnOrderItemViews
    /// </summary>
    /// <param name="filterCriterias"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    public async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias,
        Shared.Models.Enums.FilterMode filterMode)
    {
        FilteredReturnOrderItemViews.Clear();
        DisplayedReturnOrderItemViews.Clear();
        List<Model.Interfaces.IReturnOrderItemView> filteredItems = await _filter.ApplyFilter<Model.Interfaces.IReturnOrderItemView>(
            ReturnOrderItemViews, filterCriterias, filterMode);
        FilteredReturnOrderItemViews.AddRange(filteredItems);
        DisplayedReturnOrderItemViews.AddRange(filteredItems);
    }
    /// <summary>
    /// This will search data from FilteredReturnOrderItemViews and store in DisplayedReturnOrderItemViews
    /// </summary>
    /// <param name="searchString"></param>
    /// <returns></returns>
    public async Task ApplySearch(string searchString)
    {
        DisplayedReturnOrderItemViews = await _filter.ApplySearch<Model.Interfaces.IReturnOrderItemView>(
            FilteredReturnOrderItemViews, searchString, _propertiesToSearch);
    }
    /// <summary>
    /// This will sort data from FilteredReturnOrderItemViews and store in FilteredReturnOrderItemViews & DisplayedReturnOrderItemViews
    /// </summary>
    /// <param name="sortCriterias"></param>
    /// <returns></returns>
    public async Task ApplySort(List<Shared.Models.Enums.SortCriteria> sortCriterias)
    {
        List<Model.Interfaces.IReturnOrderItemView> sortedItems = await _sorter.Sort<Model.Interfaces.IReturnOrderItemView>(
            DisplayedReturnOrderItemViews, sortCriterias);
    }

    public async Task PopulateReasons()
    {
        List<IListItem>? salableReasons = await Reasons_Data("RETURN_REASON_SALEABLE");
        List<IListItem>? nonSalableReasons = await Reasons_Data("RETURN_REASON_NON_SALEABLE");
        if (salableReasons != null && salableReasons.Any())
        {
            ReasonMap.Add(StockType.Salable, CommonFunctions.ConvertToSelectionItems<IListItem>
                (salableReasons, new List<string> { "UID", "Code", "Name" }));
        }
        if (nonSalableReasons != null && nonSalableReasons.Any())
        {
            ReasonMap.Add(StockType.NonSalable, CommonFunctions.ConvertToSelectionItems<IListItem>
                (nonSalableReasons, new List<string> { "UID", "Code", "Name" }));
        }
    }
    public List<ISelectionItem> GetReasons(StockType stockType)
    {
        if (ReasonMap != null && ReasonMap.Count > 0)
        {
            if (ReasonMap.ContainsKey(stockType))
            {
                return ReasonMap[stockType];
            }
        }
        return new();
    }
    //Validation & Save
    private Dictionary<string, string> Validate(IReturnOrderItemView
        returnOrderItemView)
    {
        Dictionary<string, string> errorMessage = new Dictionary<string, string>();
        if (returnOrderItemView.SelectedUOM == null)
        {
            errorMessage["UOM"] = "UOM must be selected.";
        }
        return errorMessage;
    }
    public async virtual Task<bool> SaveOrder()
    {
        IReturnOrder returnOrder = ConvertToIReturnOrder(this);
        List<IReturnOrderItemView> SelectedReturnOrderItemViews = ReturnOrderItemViews.Where(item => item.IsSelected).ToList();
        List<IReturnOrderLine> returnOrderLines = ConvertToIReturnOrderLine(SelectedReturnOrderItemViews);
        Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderViewModel =
            new Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO();
        returnOrderViewModel.ReturnOrder = (Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder)returnOrder;
        returnOrderViewModel.ReturnOrderLineList = returnOrderLines.OfType<ReturnOrderLine>().ToList();
        returnOrderViewModel.ReturnOrderTaxList = new();
        return await PostData_ReturnOrder(returnOrderViewModel);
    }

    protected async Task PopulatePriceMaster()
    {
        List<ISKUPrice> sKUPrices = await GetSKuPrices_Data();
        foreach (ISKUPrice sKUPrice in sKUPrices)
        {
            // Find the corresponding item in ReturnOrderItemViews by matching some identifier, e.g., SKU or ID
            var returnOrderItem = ReturnOrderItemViewsRawdata.FirstOrDefault(item => item.SKUUID == sKUPrice.SKUUID);

            if (returnOrderItem != null)
            {
                // Update the price in ReturnOrderItemViews
                returnOrderItem.BasePrice = sKUPrice.Price;
                returnOrderItem.UnitPrice = sKUPrice.Price;
                returnOrderItem.SKUPriceUID = sKUPrice.UID;
                returnOrderItem.SKUPriceListUID = sKUPrice.SKUPriceListUID;
                returnOrderItem.SKUCode = sKUPrice.SKUCode;
            }
        }
        //Remove items where price = 0
        ReturnOrderItemViewsRawdata.RemoveAll(item => item.BasePrice == 0);
    }

    public List<ISelectionItem> GetAvailableUOMForDDL(IReturnOrderItemView returnOrderItemView)
    {
        return returnOrderItemView.AllowedUOMs
            .Where(e => !returnOrderItemView.UsedUOMCodes.Contains(e.Code))
            .Select(uom => new Shared.Models.Common.SelectionItem
            {
                UID = uom.Code,
                Code = uom.Code,
                Label = uom.Label,
                IsSelected = uom == returnOrderItemView.SelectedUOM // Mark the currently selected SKUUOM as selected
            })
            .ToList<ISelectionItem>();
    }
    public List<ISelectionItem> GetAvailableUOMForClone(IReturnOrderItemView returnOrderItemView)
    {
        return returnOrderItemView.AllowedUOMs
            .Where(e => !returnOrderItemView.UsedUOMCodes.Contains(e.Code)
            && (returnOrderItemView.SelectedUOM == null || e.Code != returnOrderItemView.SelectedUOM.Code))
            .Select(uom => new Shared.Models.Common.SelectionItem
            {
                UID = uom.Code,
                Code = uom.Code,
                Label = uom.Label,
                IsSelected = false
            })
            .ToList<ISelectionItem>();
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
    public void DeleteClonedItem(IReturnOrderItemView returnOrderItemView)
    {
        returnOrderItemView.UsedUOMCodes.Remove(returnOrderItemView.SelectedUOM.Code);
        ReturnOrderItemViews.Remove(returnOrderItemView);
        FilteredReturnOrderItemViews.Remove(returnOrderItemView);
        DisplayedReturnOrderItemViews.Remove(returnOrderItemView);
    }
    public void DeleteItem(IReturnOrderItemView returnOrderItemView)
    {
        ReturnOrderItemViews.Remove(returnOrderItemView);
        FilteredReturnOrderItemViews.Remove(returnOrderItemView);
        DisplayedReturnOrderItemViews.Remove(returnOrderItemView);
    }
    public async Task OnQtyChange(IReturnOrderItemView returnOrderItemView)
    {
        await ApplyQtyChange(returnOrderItemView);
        await ApplyPromotion();
        await UpdateHeader();
    }
    public virtual async Task ApplyPromotion()
    {
        if (IsPromotionBlocked == false)
        {

        }
        await Task.Delay(1);
    }
    public virtual async Task UpdateHeader()
    {
        Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderLevelCalculator returnOrderLevelCalculator = new Winit.Modules.ReturnOrder.BL.Classes.ReturnOrderLevelCalculator(this);
        await returnOrderLevelCalculator.ComputeOrderLevelTaxesAndOrderSummary();
    }
    public virtual async Task ApplyQtyChange(IReturnOrderItemView returnOrderItemView)
    {
        if (!string.IsNullOrEmpty(returnOrderItemView.SalesOrderLineUID))
        {
            if (returnOrderItemView.OrderQty > 0)
            {
                returnOrderItemView.IsSelected = true;
            }
            else
            {
                returnOrderItemView.IsSelected = false;
            }
        }
        else
        {
            if (returnOrderItemView.OrderQty > 0)
            {
                returnOrderItemView.IsSelected = true;
            }
            else
            {
                returnOrderItemView.IsSelected = false;
            }
        }
        ResetDiscount(returnOrderItemView);
        SetQtyBU(returnOrderItemView);
        UpdateItemPrice(returnOrderItemView);
        await Task.Delay(1);
    }
    public virtual void ResetDiscount(IReturnOrderItemView returnOrderItemView)
    {
        returnOrderItemView.PromotionUID = string.Empty;
        returnOrderItemView.TotalLineDiscount = 0;
        returnOrderItemView.TotalCashDiscount = 0;
    }
    public void SetQtyBU(IReturnOrderItemView returnOrderItemView)
    {
        if (returnOrderItemView.SelectedUOM == null)
        {
            return;
        }
        returnOrderItemView.QtyBU = returnOrderItemView.OrderQty * returnOrderItemView.SelectedUOM.Multiplier;
    }
    public virtual void UpdateItemPrice(IReturnOrderItemView returnOrderItemView)
    {
        if (returnOrderItemView.SelectedUOM == null)
        {
            return;
        }
        returnOrderItemView.UnitPrice = returnOrderItemView.BasePrice * returnOrderItemView.SelectedUOM.Multiplier;
        UpdateAmountAndTax(returnOrderItemView);
    }
    public void UpdateAmountAndTax(IReturnOrderItemView returnOrderItemView)
    {
        returnOrderItemView.TotalAmount = CommonFunctions.RoundForSystem(returnOrderItemView.OrderQty * returnOrderItemView.UnitPrice, 2);
        returnOrderItemView.TotalLineTax = 0;//database add
        returnOrderItemView.TotalHeaderTax = 0;// ''
        if (returnOrderItemView.IsTaxable)
        {
            // Calculate Tax
            CalculateLineTax(returnOrderItemView);
        }
        if (IsPriceInclusiveVat)
        {
            returnOrderItemView.NetAmount = returnOrderItemView.TotalAmount - returnOrderItemView.TotalDiscount + returnOrderItemView.TotalExciseDuty;
        }
        else
        {
            returnOrderItemView.NetAmount = returnOrderItemView.TotalAmount - returnOrderItemView.TotalDiscount + returnOrderItemView.TotalExciseDuty + returnOrderItemView.TotalTax;
        }
    }
    public virtual void CalculateLineTax(IReturnOrderItemView returnOrderItemView)
    {
        returnOrderItemView.TotalLineTax = (returnOrderItemView.TotalAmount - returnOrderItemView.TotalDiscount) * .1m;
    }
    public List<IReturnOrderItemView> ConvertToIReturnOrderItemView(List<ISKUMaster> sKUMasters)
    {
        List<IReturnOrderItemView> returnOrderItems = new List<IReturnOrderItemView>();
        if (sKUMasters != null && sKUMasters.Count > 0)
        {
            int lineNumber = 1;
            foreach (var sKUMaster in sKUMasters)
            {
                returnOrderItems.Add(ConvertToIReturnOrderItemView(sKUMaster, lineNumber));
                lineNumber++;
            }
        }

        return returnOrderItems;
    }
    public virtual IReturnOrderItemView ConvertToIReturnOrderItemView(ISKUMaster sKUMaster, int lineNumber)
    {
        Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.FirstOrDefault();
        List<ISKUUOMView> sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
        ISKUUOMView? defaultUOM = sKUUOMViews?.FirstOrDefault(e => e?.Code == sKUConfig?.SellingUOM);
        ISKUUOMView? baseUOM = sKUUOMViews
                        ?.FirstOrDefault(e => e.IsBaseUOM);
        IReturnOrderItemView returnOrderItem = new ReturnOrderItemView
        {
            UID = Guid.NewGuid().ToString(),
            LineNumber = lineNumber,
            SKUImage = string.Empty,
            SKUUID = sKUMaster?.SKU?.UID ?? string.Empty,
            SKUCode = sKUMaster?.SKU?.Code ?? string.Empty,
            SKUName = sKUMaster?.SKU?.Name ?? string.Empty,
            SKUType = "Salable",
            MaxQty = 0,
            OrderQty = 0,
            ApprovedQty = 0,
            ReturnedQty = 0,
            QtyBU = 0,
            SelectedUOM = defaultUOM,
            AllowedUOMs = sKUUOMViews,
            BaseUOM = defaultUOM?.Code,
            UsedUOMCodes = new List<string>(),
            Multiplier = 0,
            BasePrice = 0,
            UnitPrice = 0,
            IsTaxable = SelectedStoreViewModel?.IsTaxApplicable ?? false,
            ApplicableTaxes = null,//sKUMaster.TaxSKUMaps,
            TotalAmount = 0,
            TotalDiscount = 0,
            TotalExciseDuty = 0,
            TotalTax = 0,
            Attributes = ConvertToISKUAttributeView(sKUMaster?.SKUAttributes),
            ItemStatus = ItemState.Primary,
            ReasonCode = null,//will be re inistilized when selected reason code in the gridview
            ReasonText = null,//will be re inistilized when selected reason code in the gridview
            ExpiryDate = DateTime.MaxValue,
            BatchNumber = null,//not available in Sku
            InvoiceUID = null,
            InvoiceLineUID = null,
            Remarks = null,
            Volume = 0,
            VolumeUnit = string.Empty,
            SalesOrderUID = string.Empty,
            SalesOrderLineUID = string.Empty,
            ReasonsList = ReasonMap[StockType.NonSalable]
            // Note: Assuming it's nullable.
        };
        if (sKUMaster.SKUAttributes != null)
        {
            sKUMaster.SKUAttributes.ForEach(e => returnOrderItem.FilterKeys.Add(e.Code));
        }
        if (!string.IsNullOrEmpty(sKUMaster.SKU.SKUImage))
            returnOrderItem.SKUImage = Path.Combine(_appConfigs.ApiDataBaseUrl, sKUMaster.SKU.SKUImage.ToString());
        else
        {
            returnOrderItem.SKUImage = "/Data/SKU/no_image_available.jpg";
        }
        return returnOrderItem;
    }
    public List<ISKUUOMView> ConvertToISKUUOMView(List<ISKUUOM> sKUUOMs)
    {
        List<ISKUUOMView> sKUUOMViews = new List<ISKUUOMView>();
        if (sKUUOMs != null && sKUUOMs.Any())
        {
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
    public virtual Dictionary<string, ISKUAttributeView> ConvertToISKUAttributeView(List<ISKUAttributes> sKUAttributes)
    {
        Dictionary<string, ISKUAttributeView> ISKUAttributeViews = new Dictionary<string, ISKUAttributeView>();
        if (sKUAttributes != null)
        {
            foreach (ISKUAttributes skuAttribute in sKUAttributes)
            {
                string key = skuAttribute.Type;
                ISKUAttributeViews[key] = ConvertToISKUAttributeView(skuAttribute);
            }
        }
        return ISKUAttributeViews;
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
    public virtual List<Model.Interfaces.IReturnOrderLine> ConvertToIReturnOrderLine
        (List<IReturnOrderItemView> returnOrderItemViewList)
    {
        List<Model.Interfaces.IReturnOrderLine> returnOrderLines = new List<Model.Interfaces.IReturnOrderLine>();
        if (returnOrderItemViewList != null && returnOrderItemViewList.Count > 0)
        {
            foreach (ReturnOrderItemView returnOrderItemView in returnOrderItemViewList)
            {
                Model.Interfaces.IReturnOrderLine returnOrderLine = ConvertToIReturnOrderLine(returnOrderItemView);
                returnOrderLines.Add(returnOrderLine);
            }
        }
        return returnOrderLines;
    }
    public virtual Model.Interfaces.IReturnOrderLine ConvertToIReturnOrderLine(IReturnOrderItemView returnOrderItemView)
    {
        Model.Interfaces.IReturnOrderLine returnOrderLine =
            _serviceProvider.CreateInstance<Model.Interfaces.IReturnOrderLine>();
        returnOrderLine.Id = 0;
        returnOrderLine.ReturnOrderUID = ReturnOrderUID;
        returnOrderLine.LineNumber = returnOrderItemView.LineNumber;
        returnOrderLine.SKUUID = returnOrderItemView.SKUUID;
        returnOrderLine.SKUCode = returnOrderItemView.SKUCode;
        returnOrderLine.SKUType = returnOrderItemView.SKUType.ToString();
        returnOrderLine.BasePrice = returnOrderItemView.BasePrice;
        returnOrderLine.UnitPrice = returnOrderItemView.UnitPrice;
        returnOrderLine.FakeUnitPrice = returnOrderItemView.UnitPrice;
        returnOrderLine.BaseUOM = returnOrderItemView.BaseUOM;
        returnOrderLine.UoM = returnOrderItemView.SelectedUOM.Code;
        returnOrderLine.Multiplier = returnOrderItemView.Multiplier;
        returnOrderLine.Qty = returnOrderItemView.OrderQty;
        returnOrderLine.QtyBU = returnOrderItemView.QtyBU;
        returnOrderLine.ApprovedQty = returnOrderItemView.ApprovedQty;
        returnOrderLine.ReturnedQty = 0;
        returnOrderLine.TotalAmount = returnOrderItemView.TotalAmount;
        returnOrderLine.TotalDiscount = returnOrderItemView.TotalDiscount;
        returnOrderLine.TotalExciseDuty = returnOrderItemView.TotalExciseDuty;
        returnOrderLine.TotalTax = returnOrderItemView.TotalTax;
        returnOrderLine.NetAmount = returnOrderItemView.NetAmount;
        returnOrderLine.SKUPriceUID = returnOrderItemView.SKUPriceUID;
        returnOrderLine.SKUPriceListUID = returnOrderItemView.SKUPriceListUID;
        returnOrderLine.ReasonCode = returnOrderItemView.ReasonCode;
        returnOrderLine.ReasonText = returnOrderItemView.ReasonText;
        returnOrderLine.ExpiryDate = returnOrderItemView.ExpiryDate;
        returnOrderLine.BatchNumber = returnOrderItemView.BatchNumber;
        returnOrderLine.SalesOrderUID = returnOrderItemView.SalesOrderUID;
        returnOrderLine.SalesOrderLineUID = returnOrderItemView.SalesOrderLineUID;
        returnOrderLine.Remarks = returnOrderItemView.Remarks;
        returnOrderLine.Volume = returnOrderItemView.Volume;
        returnOrderLine.VolumeUnit = string.Empty;
        returnOrderLine.PromotionUID = string.Empty;
        returnOrderLine.NetFakeAmount = returnOrderItemView.NetAmount;
        returnOrderLine.UID = returnOrderItemView.UID;
        returnOrderLine.PONumber = returnOrderItemView.PONumber;
        AddCreateFields(returnOrderLine, false);
        return returnOrderLine;
    }
    public virtual Model.Interfaces.IReturnOrder ConvertToIReturnOrder(Interfaces.IReturnOrderViewModel viewModel)
    {
        if (Status == Winit.Shared.Models.Constants.SalesOrderStatus.APPROVED)
        {
            ReturnOrderNumber = "RO" + DateTime.Now.ToString("ddMMMyyyyHHmiss");
        }
        else
        {
            DraftOrderNumber = "DO" + DateTime.Now.ToString("ddMMMyyyyHHmiss");
        }
        Model.Interfaces.IReturnOrder returnOrder = _serviceProvider.CreateInstance<Model.Interfaces.IReturnOrder>();
        returnOrder.Id = viewModel.Id;
        returnOrder.UID = viewModel.ReturnOrderUID;
        returnOrder.ReturnOrderNumber = viewModel.ReturnOrderNumber;
        returnOrder.DraftOrderNumber = viewModel.DraftOrderNumber;
        returnOrder.JobPositionUID = viewModel.JobPositionUID;
        returnOrder.EmpUID = _appUser.Emp.UID;
        returnOrder.OrgUID = viewModel.OrgUID;
        returnOrder.DistributionChannelUID = viewModel.DistributionChannelOrgUID;
        returnOrder.StoreUID = StoreUID;
        returnOrder.IsTaxApplicable = viewModel.IsTaxApplicable;
        returnOrder.RouteUID = viewModel.RouteUID;
        returnOrder.BeatHistoryUID = viewModel.BeatHistoryUID;
        returnOrder.StoreHistoryUID = viewModel.StoreHistoryUID;
        returnOrder.Status = Status;
        returnOrder.OrderType = viewModel.OrderType;
        returnOrder.OrderDate = viewModel.OrderDate;
        returnOrder.CurrencyUID = viewModel.CurrencyUID;
        returnOrder.TotalAmount = viewModel.TotalAmount;
        returnOrder.TotalLineDiscount = viewModel.TotalLineDiscount;
        returnOrder.TotalCashDiscount = viewModel.TotalCashDiscount;
        returnOrder.TotalHeaderDiscount = viewModel.TotalHeaderDiscount;
        returnOrder.TotalDiscount = viewModel.TotalDiscount;
        returnOrder.TotalExciseDuty = viewModel.TotalExciseDuty;
        returnOrder.LineTaxAmount = viewModel.LineTaxAmount;
        returnOrder.HeaderTaxAmount = viewModel.HeaderTaxAmount;
        returnOrder.TotalTax = viewModel.TotalTax;
        returnOrder.NetAmount = viewModel.NetAmount;
        returnOrder.TotalFakeAmount = viewModel.TotalAmount;
        returnOrder.LineCount = (int)viewModel.LineCount;
        returnOrder.QtyCount = (int)viewModel.QtyCount;
        returnOrder.Notes = viewModel.Notes;
        returnOrder.IsOffline = false;
        returnOrder.Latitude = "0";
        returnOrder.Longitude = "0";
        returnOrder.DeliveredByOrgUID = viewModel.OrgUID;
        returnOrder.SS = 1;
        returnOrder.Source = Source;
        returnOrder.PromotionUID = string.Empty;
        returnOrder.TotalLineTax = viewModel.TotalLineTax;
        returnOrder.TotalHeaderTax = viewModel.TotalHeaderTax;
        returnOrder.SalesOrderUID = viewModel.SalesOrderUID;
        if (IsNewOrder)
        {
            AddCreateFields(returnOrder, false);
        }
        else
        {
            AddUpdateFields(returnOrder);
        }
        return returnOrder;
    }
    public void ConvertReturnOrderMasterDTOToReturnOrderBaseViewModel(ReturnOrderMasterDTO returnOrderMasterDTO)
    {
        ConvertReturnOrderToReturnOrderViewModel(returnOrderMasterDTO.ReturnOrder);
        ReturnOrderItemViews = new();
        foreach (var returnOrderLine in returnOrderMasterDTO.ReturnOrderLineList)
        {
            ReturnOrderItemViews.Add(ConvertReturnOrderLineToReturnOrderItemView(returnOrderLine));
        }
    }
    protected void ConvertReturnOrderToReturnOrderViewModel(IReturnOrder returnOrder)
    {
        this.Id = returnOrder.Id;
        this.ReturnOrderUID = returnOrder.UID;
        this.DraftOrderNumber = returnOrder.DraftOrderNumber;
        this.ReturnOrderNumber = returnOrder.ReturnOrderNumber;
        this.DistributionChannelOrgUID = returnOrder.DistributionChannelUID;
        this.OrderType = returnOrder.OrderType;
        this.JobPositionUID = returnOrder.JobPositionUID;
        this.IsTaxable = returnOrder.IsTaxApplicable;
        this.CurrencyUID = returnOrder.CurrencyUID;
        this.OrgUID = returnOrder.OrgUID;
        this.IsTaxApplicable = returnOrder.IsTaxApplicable;
        this.RouteUID = returnOrder.RouteUID;
        this.BeatHistoryUID = returnOrder.BeatHistoryUID;
        this.StoreHistoryUID = returnOrder.StoreHistoryUID;
        this.OrderDate = returnOrder.OrderDate;
        this.TotalLineTax = returnOrder.TotalLineTax;
        this.TotalHeaderTax = returnOrder.TotalHeaderTax;
        this.CurrencyLabel = returnOrder.CurrencyUID;
        this.LineCount = returnOrder.LineCount;
        this.QtyCount = returnOrder.QtyCount;
        this.Notes = returnOrder.Notes;
        this.TotalAmount = returnOrder.TotalAmount;
        this.TotalLineDiscount = returnOrder.TotalLineDiscount;
        this.TotalCashDiscount = returnOrder.TotalCashDiscount;
        this.TotalHeaderDiscount = returnOrder.TotalHeaderDiscount;
        //this.TotalDiscount = returnOrder.TotalDiscount; this is automatically calculated in model
        this.TotalExciseDuty = returnOrder.TotalExciseDuty;
        this.LineTaxAmount = returnOrder.LineTaxAmount;
        this.HeaderTaxAmount = returnOrder.HeaderTaxAmount;
        //this.TotalTax = returnOrder.TotalTax; this is automatically calculated
        //this.NetAmount = returnOrder.NetAmount;this is automatically calculated
    }
    private IReturnOrderItemView ConvertReturnOrderLineToReturnOrderItemView(IReturnOrderLine returnOrderLine)
    {
        IReturnOrderItemView returnOrderItemView = _serviceProvider.CreateInstance<IReturnOrderItemView>();
        returnOrderItemView.UID = returnOrderLine.UID;
        returnOrderItemView.LineNumber = returnOrderLine.LineNumber;
        // returnOrderItemView.SKUImage = returnOrderLine.SKUImage; fileld is not there in the returnorderline model
        returnOrderItemView.SKUUID = returnOrderLine.SKUUID;
        returnOrderItemView.SKUCode = returnOrderLine.SKUCode;
        //returnOrderItemView.SKUName = returnOrderLine.SKUName;fileld is not there in the returnorderline model
        //returnOrderItemView.SKULabel = returnOrderLine.SKULabel;//it is automatically set in class model
        returnOrderItemView.SKUType = returnOrderLine.SKUType;
        // returnOrderItemView.MaxQty = returnOrderLine.MaxQty; fileld is not there in the returnorderline model
        returnOrderItemView.OrderQty = returnOrderLine.Qty;
        returnOrderItemView.ApprovedQty = returnOrderLine.ApprovedQty;
        returnOrderItemView.ReturnedQty = returnOrderLine.ReturnedQty;
        returnOrderItemView.QtyBU = returnOrderLine.QtyBU;
        returnOrderItemView.BaseUOM = returnOrderLine.BaseUOM;
        // returnOrderItemView.UsedUOMCodes = returnOrderLine.UsedUOMCodes; it will be set from sku prices method
        returnOrderItemView.Multiplier = returnOrderLine.Multiplier;
        returnOrderItemView.BasePrice = returnOrderLine.BasePrice;
        returnOrderItemView.UnitPrice = returnOrderLine.UnitPrice;
        //returnOrderItemView.IsTaxable = returnOrderLine.IsTaxable; fileld is not there in the returnorderline model
        //returnOrderItemView.ApplicableTaxes = returnOrderLine.ApplicableTaxes; it s not present in both model and this class
        returnOrderItemView.TotalAmount = returnOrderLine.TotalAmount;
        returnOrderItemView.TotalDiscount = returnOrderLine.TotalDiscount;
        returnOrderItemView.TotalExciseDuty = returnOrderLine.TotalExciseDuty;
        //returnOrderItemView.TotalLineDiscount = returnOrderLine.TotalLineDiscount;fileld is not there in the returnorderline model
        //returnOrderItemView.TotalCashDiscount = returnOrderLine.TotalCashDiscount;fileld is not there in the returnorderline model
        returnOrderItemView.TotalTax = returnOrderLine.TotalTax;
        returnOrderItemView.NetAmount = returnOrderLine.NetAmount;
        //returnOrderItemView.TotalLineTax = returnOrderLine.TotalLineTax;    fileld is not there in the returnorderLine model
        //returnOrderItemView.TotalHeaderTax = returnOrderLine.TotalHeaderTax;fileld is not there in the returnorderLine model
        //returnOrderItemView.GSVText = returnOrderLine.GSVText; it will automatically set
        returnOrderItemView.SKUPriceUID = returnOrderLine.SKUPriceUID;
        returnOrderItemView.SKUPriceListUID = returnOrderLine.SKUPriceListUID;
        //returnOrderItemView.Attributes = returnOrderLine.Attributes; it will inistalized using method
        //returnOrderItemView.ItemStatus = returnOrderLine.ItemStatus;fileld is not there in the returnorderLine model
        returnOrderItemView.ReasonCode = returnOrderLine.ReasonCode;
        returnOrderItemView.ReasonText = returnOrderLine.ReasonText;
        returnOrderItemView.ExpiryDate = returnOrderLine.ExpiryDate;
        returnOrderItemView.BatchNumber = returnOrderLine.BatchNumber;
        //returnOrderItemView.InvoiceUID = returnOrderLine.InvoiceUID;        fileld is not there in the returnorderLine model
        //returnOrderItemView.InvoiceLineUID = returnOrderLine.InvoiceLineUID;fileld is not there in the returnorderLine model
        returnOrderItemView.Remarks = returnOrderLine.Remarks;
        returnOrderItemView.Volume = returnOrderLine.Volume;
        returnOrderItemView.VolumeUnit = returnOrderLine.VolumeUnit;
        returnOrderItemView.IsSelected = true;
        returnOrderItemView.PromotionUID = returnOrderLine.PromotionUID;
        //returnOrderItemView.ReasonsList = returnOrderLine.ReasonsList;//this will bw istalized using reasons method
        returnOrderItemView.SalesOrderUID = returnOrderLine.SalesOrderUID;
        returnOrderItemView.SalesOrderLineUID = returnOrderLine.SalesOrderLineUID;
        returnOrderItemView.PONumber = returnOrderLine.PONumber;
        return returnOrderItemView;
    }
    protected void OverideReturnOrderItemView(IReturnOrderLine returnOrderLine)
    {
        IReturnOrderItemView? returnOrderItemView = ReturnOrderItemViewsRawdata.Find
            (item => item.SKUCode == returnOrderLine.SKUCode);
        if (returnOrderItemView != null)
        {
            returnOrderItemView.UID = returnOrderLine.UID;
            returnOrderItemView.LineNumber = returnOrderLine.LineNumber;
            returnOrderItemView.SKUType = returnOrderLine.SKUType;
            returnOrderItemView.OrderQty = returnOrderLine.Qty;
            returnOrderItemView.ApprovedQty = returnOrderLine.ApprovedQty;
            returnOrderItemView.ReturnedQty = returnOrderLine.ReturnedQty;
            returnOrderItemView.QtyBU = returnOrderLine.QtyBU;
            returnOrderItemView.BaseUOM = returnOrderLine.BaseUOM;
            returnOrderItemView.UsedUOMCodes = new List<string>();
            returnOrderItemView.Multiplier = returnOrderLine.Multiplier;
            returnOrderItemView.BasePrice = returnOrderLine.BasePrice;
            returnOrderItemView.UnitPrice = returnOrderLine.UnitPrice;
            //returnOrderItemView.IsTaxable = returnOrderLine.IsTaxable; fileld is not there in the returnorderline model
            //returnOrderItemView.ApplicableTaxes = returnOrderLine.ApplicableTaxes; it s not present in both model and this class
            returnOrderItemView.TotalAmount = returnOrderLine.TotalAmount;
            returnOrderItemView.TotalDiscount = returnOrderLine.TotalDiscount;
            returnOrderItemView.TotalExciseDuty = returnOrderLine.TotalExciseDuty;
            //returnOrderItemView.TotalLineDiscount = returnOrderLine.TotalLineDiscount;fileld is not there in the returnorderline model
            //returnOrderItemView.TotalCashDiscount = returnOrderLine.TotalCashDiscount;fileld is not there in the returnorderline model
            returnOrderItemView.TotalTax = returnOrderLine.TotalTax;
            returnOrderItemView.NetAmount = returnOrderLine.NetAmount;
            //returnOrderItemView.TotalLineTax = returnOrderLine.TotalLineTax;    fileld is not there in the returnorderLine model
            //returnOrderItemView.TotalHeaderTax = returnOrderLine.TotalHeaderTax;fileld is not there in the returnorderLine model
            //returnOrderItemView.GSVText = returnOrderLine.GSVText; it will automatically set
            returnOrderItemView.SKUPriceUID = returnOrderLine.SKUPriceUID;
            returnOrderItemView.SKUPriceListUID = returnOrderLine.SKUPriceListUID;
            //returnOrderItemView.Attributes = returnOrderLine.Attributes; it will inistalized using method
            //returnOrderItemView.ItemStatus = returnOrderLine.ItemStatus;fileld is not there in the returnorderLine model
            returnOrderItemView.ReasonCode = returnOrderLine.ReasonCode;
            returnOrderItemView.ReasonText = returnOrderLine.ReasonText;
            returnOrderItemView.ExpiryDate = returnOrderLine.ExpiryDate;
            returnOrderItemView.BatchNumber = returnOrderLine.BatchNumber;
            //returnOrderItemView.InvoiceUID = returnOrderLine.InvoiceUID;        fileld is not there in the returnorderLine model
            //returnOrderItemView.InvoiceLineUID = returnOrderLine.InvoiceLineUID;fileld is not there in the returnorderLine model
            returnOrderItemView.Remarks = returnOrderLine.Remarks;
            returnOrderItemView.Volume = returnOrderLine.Volume;
            returnOrderItemView.VolumeUnit = returnOrderLine.VolumeUnit;
            returnOrderItemView.IsSelected = true;
            returnOrderItemView.PromotionUID = returnOrderLine.PromotionUID;
            //returnOrderItemView.ReasonsList = returnOrderLine.ReasonsList;//this will bw istalized using reasons method
            returnOrderItemView.SalesOrderUID = returnOrderLine.SalesOrderUID;
            returnOrderItemView.SalesOrderLineUID = returnOrderLine.SalesOrderLineUID;
            returnOrderItemView.SelectedUOM = returnOrderItemView.AllowedUOMs.Find(uom => uom.Code == returnOrderLine.UoM);
            var reason = returnOrderItemView.ReasonsList.First(reason => reason.Code == returnOrderLine.ReasonCode);
            if (reason != null) reason.IsSelected = true;
            ReturnOrderItemViews.Add(returnOrderItemView);
            if (Source == SourceType.CPE)
            {
                ReturnOrderItemViews.Add(returnOrderItemView);
                FilteredReturnOrderItemViews.Add(returnOrderItemView);
                DisplayedReturnOrderItemViews.Add(returnOrderItemView);
            }
        }
    }
    public async Task<List<ISelectionItem>> GetSkuListAsSelectionItems()
    {
        List<ISelectionItem> selectionItems = new List<ISelectionItem>();
        selectionItems = await Task.Run(() => SKUList.Select(sku => new SelectionItem
        {
            UID = sku.UID,
            Code = sku.Code,
            Label = sku.Name
        }).ToList<ISelectionItem>());
        return selectionItems;
    }
    private void AddCreateFields(IBaseModel baseModel, bool IsUIDRequired)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT";
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (IsUIDRequired) baseModel.UID = Guid.NewGuid().ToString();
    }
    private void AddUpdateFields(IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID ?? "WINIT"; //_appUser.Emp.UID;s
        baseModel.ModifiedTime = DateTime.Now;
    }

    public virtual Task OnRouteSelect(string routeUID)
    {
        throw new NotImplementedException();
    }
    public virtual Task OnStoreItemViewSelected(string storeItemViewUID)
    {
        throw new NotImplementedException();
    }
    public void OnSignatureProceedClick()
    {

    }

    public void Validate()
    {
        List<string> errorMSG = new List<string>();

        if (string.IsNullOrEmpty(RouteUID))
        {
            errorMSG.Add("Route");
        }
        if (string.IsNullOrEmpty(StoreUID))
        {
            errorMSG.Add("Store");
        }
        if (OrderDate == DateTime.MinValue)
        {
            errorMSG.Add("Order Date");
        }
        if (!DisplayedReturnOrderItemViews.Any())
        {
            errorMSG.Add("Items");
        }
        foreach (var item in DisplayedReturnOrderItemViews)
        {
            if (item.OrderQty == 0 && !errorMSG.Contains("Quantity")) errorMSG.Add("Quantity");
            if (string.IsNullOrEmpty(item.ReasonCode) && !errorMSG.Contains("Reason Code")) errorMSG.Add("Reason Code");
        }
        if (errorMSG.Any())
        {
            throw new CustomException(ExceptionStatus.Failed, $"Please select the following fields: {string.Join(", ", errorMSG)}");
        }
    }

    #region abstract methods
    protected abstract Task<bool> PostData_ReturnOrder(ReturnOrderMasterDTO returnOrderViewModel);
    protected abstract Task<List<ISKUPrice>> GetSKuPrices_Data();
    protected abstract Task<List<ISKUMaster>> SKUMasters_Data(List<string> orgs, List<string>? skuUIDs = null);
    protected abstract Task<List<IListItem>> Reasons_Data(string reason);
    protected abstract Task<IReturnOrderMaster> GetReturnOrder_Data();
    #endregion
}

