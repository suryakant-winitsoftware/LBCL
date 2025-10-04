using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Constants.Notification;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Classes;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.Tax.BL.Interfaces;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Modules.User.Model.Constants;
using Winit.Modules.User.Model.Interface;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common;


namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class PurchaseOrderBaseViewModel : IPurchaseOrderViewModel
{
    public bool IsNewOrder { get; set; } = true;
    public string UserCode { get; set; }
    public bool IsDraftOrder { get; set; }

    public bool IsQtyDisabled =>
        !IsNewOrder && !IsDraftOrder && (PurchaseOrderHeader.Status == PurchaseOrderStatusConst.InProcessERP
        || PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Invoiced
        || PurchaseOrderHeader.Status ==
        PurchaseOrderStatusConst.CancelledByCMI);

    public IPurchaseOrderHeader PurchaseOrderHeader { get; private set; }
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; }
    public List<ISKUV1> SKUs { get; }
    public List<string> PurchaseOrderUIDs { get; set; } = [];
    public List<string> PurchaseOrderUIDsForNotification { get; set; } = [];
    public string PurchaseOrderUID { get; set; }
    private List<ISellInSchemePO> SellInSchemes { get; set; }
    private List<IQPSSchemePO> QPSSchemes { get; set; }
    public Dictionary<string, StandingSchemeResponse> StandingSchemes { get; set; }
    public List<ISelectionItem> WareHouseSelectionItems { get; set; }
    public List<ISelectionItem> TemplateSelectionItems { get; }
    public List<ISelectionItem> ShippingAddressSelectionItems { get; }
    public List<ISelectionItem> BillingAddressSelectionItems { get; }
    public List<ISelectionItem> OrgUnitSelectionItems { get; }
    public List<ISelectionItem> DivisionSelectionItems { get; }
    public List<ISelectionItem> ProductCategorySelectionItems { get; }
    public List<IPurchaseOrderItemView> PurchaseOrderItemViews { get; }
    public List<IPurchaseOrderItemView> FilteredPurchaseOrderItemViews { get; }
    public List<IPurchaseOrderItemView> FOCPurchaseOrderItemViews { get; }
    public List<string>? ApplicablePromotionList { get; set; }
    public List<string>? ApplicableSchemeList { get; set; }
    public IStoreItemView? SelectedStoreViewModel { get; set; }
    public string? CurrencyUID { get; set; }
    public string? CurrencyLabel { get; set; }
    #region Approval Fields
    public string ApprovalCreatedBy { get; set; }
    public int RuleId { get; set; }

    public int RequestId { get; set; }
    public ApprovalStatusUpdate ApprovalStatusUpdate { get; set; }
    #endregion
    public string UserRoleCode { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableLimit { get; set; }
    public decimal CurrentOutStanding { get; set; }
    public IStoreMaster? SelectedStoreMaster { get; set; }
    public bool IsPriceInclusiveVat { get; set; }
    public Dictionary<string, Winit.Modules.Promotion.Model.Classes.DmsPromotion>? DMSPromotionDictionary { get; set; }
    public List<IAppliedTax> AppliedTaxes { get; set; }
    public Dictionary<string, ITax> TaxDictionary { get; set; }
    public List<string> InvoiceApplicableTaxes { get; set; }
    public IPurchaseOrderTaxCalculator _purchaseOrderTaxCalculator { get; set; }
    public List<IStore> Stores { get; set; }
    public List<ISelectionItem> OrganizationUnitSelectionItems { get; set; }
    public string? SelectedDistributor { get; set; }
    public string StoreSearchString { get; set; } = string.Empty;
    public string ProductSearchString { get; set; } = string.Empty;
    public bool IsReassignButtonNeeded { get; set; }
    public bool IsPriceEditable { get; set; }

    public IEnumerable<IStore> FilteredStores
        => Stores.Where(e => string.IsNullOrEmpty(StoreSearchString) ||
            (e.Name?.Contains(StoreSearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.Code?.Contains(StoreSearchString, StringComparison.OrdinalIgnoreCase) ?? false));

    private List<string> TemplateSKUs { get; set; }
    protected bool IsCashDiscountExcluded { get; set; }
    private int DefaultDeliveryDay;
    private IPurchaseOrderMaster? PurchaseOrderMaster;
    public List<string> CreatedPurchaseOrderNumbers { get; set; }
    public int ApprovalLevel { get; set; }
    private Dictionary<string, string> DivisionEmpKVPair { get; set; }
    public Dictionary<string, string> PurchaseOrderUIDEmpKVPair { get; set; }
    private List<IPurchaseOrderLine> OgPurchaseOrderLines { get; set; }
    public IStoreCreditLimit? StoreCreditLimit { get; set; }
    public bool IsPoEdited { get; set; } = false;
    public bool IsPoCreatedByCP { get; set; } = false;
    public List<IPurchaseOrderCreditLimitBufferRange> PurchaseOrderCreditLimitBufferRanges { get; set; }
    public decimal CrediLimitBufferPercentage => PurchaseOrderCreditLimitBufferRanges?.Find(e => StoreCreditLimit?.CreditLimit >= e?.RangeFrom && StoreCreditLimit?.CreditLimit <= (e?.RangeTo == 0 ? decimal.MaxValue : e?.RangeTo))?.PercentageBuffer ?? default;
    private bool IsOrderEdited(List<IPurchaseOrderLine> purchaseOrderLines) => OgPurchaseOrderLines?.Exists(o =>
    {

        IPurchaseOrderLine actauPurchaseOrderLine = purchaseOrderLines?.Find(e => e.SKUUID == o.SKUUID && e.FinalQty == o.FinalQty && e.RequestedQty == o.RequestedQty);
        if (actauPurchaseOrderLine == null) return true;
        return false;

    }) ?? false && PurchaseOrderMaster?.PurchaseOrderLines.Count != OgPurchaseOrderLines?.Count;
    //Dependecy injection
    private readonly IServiceProvider _serviceProvider;
    private readonly IFilterHelper _filter;
    private readonly ISortHelper _sorter;
    private readonly IListHelper _listHelper;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    protected readonly IDataManager _dataManager;
    private readonly IAppConfig _appConfigs;
    private readonly IPurchaseOrderDataHelper _purchaseOrderDataHelper;
    private readonly IPurchaseOrderLevelCalculator _purchaseOrderLevelCalculator;
    private readonly Winit.Modules.SKU.BL.Interfaces.IAddProductPopUpDataHelper _addProductPopUpDataHelper;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public PurchaseOrderBaseViewModel(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IDataManager dataManager,
        IPurchaseOrderLevelCalculator purchaseOrderLevelCalculator,
        IAppConfig appConfigs,
        IPurchaseOrderDataHelper purchaseOrderDataHelper,
        SKU.BL.Interfaces.IAddProductPopUpDataHelper addProductPopUpDataHelper,
        Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings)
    {
        _serviceProvider = serviceProvider;
        _filter = filter;
        _sorter = sorter;
        _listHelper = listHelper;
        _appUser = appUser;
        _appSetting = appSetting;
        _dataManager = dataManager;
        _appConfigs = appConfigs;
        _purchaseOrderDataHelper = purchaseOrderDataHelper;
        PurchaseOrderHeader = _serviceProvider.GetRequiredService<IPurchaseOrderHeader>()!;
        PurchaseOrderHeader.CreatedBy = _appUser.Emp.UID;
        _purchaseOrderLevelCalculator = purchaseOrderLevelCalculator;
        _addProductPopUpDataHelper = addProductPopUpDataHelper;
        _jsonSerializerSettings = jsonSerializerSettings;
        PurchaseOrderHeader.OrderDate = DateTime.Now;
        SKUAttributeData = [];
        WareHouseSelectionItems = [];
        TaxDictionary = [];
        SKUs = [];
        PurchaseOrderItemViews = [];
        FilteredPurchaseOrderItemViews = [];
        ApplicablePromotionList = [];
        FOCPurchaseOrderItemViews = [];
        ShippingAddressSelectionItems = [];
        BillingAddressSelectionItems = [];
        OrgUnitSelectionItems = [];
        DivisionSelectionItems = [];
        ProductCategorySelectionItems = [];
        TemplateSelectionItems = [];
        TemplateSKUs = [];
        Stores = [];
        OrganizationUnitSelectionItems = [];
        CreatedPurchaseOrderNumbers = [];
        DivisionEmpKVPair = [];
        PurchaseOrderUIDEmpKVPair = [];
        PurchaseOrderCreditLimitBufferRanges = [];
        SellInSchemes = [];
        QPSSchemes = [];
        StandingSchemes = [];
        _purchaseOrderLevelCalculator.SetOrderViewModel(this);
        UserRoleCode = appUser?.Role?.Code;
        IsReassignButtonNeeded = _appSetting.IsReassignNeededInPurchaseOrder;
        UserCode = _appUser.Emp.Code;
        IsPriceInclusiveVat = _appSetting.IsPriceInclusiveVat;
    }

    public virtual async Task PopulateViewModel(string source, string orderUID = "")
    {
        TaxDictionary = _appUser.TaxDictionary;
        await SetSystemSettingValues();
        _ = PrepareOrgUnitSelectionItems();
        SetCurrency();
        await Task.Run(() =>
        {
            PrepareFilters();
            PreparePurchaseOrderCreditLimitBufferRanges();
        });
    }
    public async Task PreparePurchaseOrderCreditLimitBufferRanges()
    {
        PurchaseOrderCreditLimitBufferRanges.AddRange(await _purchaseOrderDataHelper.GetPurchaseOrderCreditLimitBufferRanges());
    }
    private async Task PrepareOrgUnitSelectionItems()
    {
        List<IOrg>? orgData = await _purchaseOrderDataHelper.GetOrgByOrgTypeUID("OU");
        if (orgData == null)
        {
            throw new CustomException(ExceptionStatus.Failed, "Failed to retrive org units");
        }

        OrgUnitSelectionItems.AddRange(
        CommonFunctions
            .ConvertToSelectionItems<IOrg>(orgData, e => e.UID, e => e.UID == "OU366" || e.Code == "95", e => e.Code, e => e.Name));

        PurchaseOrderHeader.OrgUnitUID = OrgUnitSelectionItems.Find(p => p.IsSelected)?.UID;
    }

    public async Task OnDistributorSelect()
    {
        if (_appUser.Role.IsDistributorRole)
        {
            SelectedDistributor = _appUser.SelectedJobPosition.OrgUID;
        }
        PurchaseOrderHeader.OrgUID = SelectedDistributor;
        PurchaseOrderItemViews.Clear();
        FilteredPurchaseOrderItemViews.Clear();
        if (!string.IsNullOrEmpty(PurchaseOrderHeader.UID))
        {
            PurchaseOrderMaster = await _purchaseOrderDataHelper.GetPurchaseOrderMasterByUID(PurchaseOrderHeader.UID);
            if (PurchaseOrderMaster is not null)
            {
                OgPurchaseOrderLines = PurchaseOrderMaster.PurchaseOrderLines.DeepCopy(null, _jsonSerializerSettings);
                PurchaseOrderHeader = PurchaseOrderMaster.PurchaseOrderHeader!;
                SelectedDistributor = PurchaseOrderHeader.OrgUID;
                if (PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Draft)
                {
                    IsDraftOrder = true;
                }
                else
                {
                    IsNewOrder = false;
                    _ = SetStoreCreditLimit(PurchaseOrderMaster.PurchaseOrderHeader!.OrgUID,
                    PurchaseOrderMaster.PurchaseOrderHeader.DivisionUID!);
                }
            }
        }
        await PrepareSelectedStoreMaster();

        await PopulateTemplates(SelectedStoreMaster.Store.UID, !_appUser.Role.IsDistributorRole ? _appUser.Emp.UID : null);

        List<string>? orgUIDs =
            await _purchaseOrderDataHelper.GetOrgHierarchyParentUIDsByOrgUID(SelectedStoreMaster!.Store.UID);
        if (orgUIDs == null || !orgUIDs.Any())
        {
            throw new CustomException(ExceptionStatus.Failed, "No org mapped to this customer");
        }
        _appUser.OrgUIDs = orgUIDs;
        IsCashDiscountExcluded = await _purchaseOrderDataHelper.CheckSchemeExcludeMappingExists(storeUID: SelectedDistributor!,
          currentDate: PurchaseOrderHeader.Status == PurchaseOrderStatusConst.Draft ? DateTime.Now : PurchaseOrderHeader.OrderDate) > 0 ? true : false;
        if (!IsNewOrder || IsDraftOrder || !string.IsNullOrEmpty(PurchaseOrderHeader.OrgUnitUID))
        {
            await OnOrgUnitSelect(PurchaseOrderHeader.OrgUnitUID);
        }


    }

    public void ClearOrgUnitSelection()
    {
        PurchaseOrderHeader.OrgUnitUID = default;
        BillingAddressSelectionItems.Clear();
        ShippingAddressSelectionItems.Clear();
        PurchaseOrderHeader.ShippingAddressUID = null;
        PurchaseOrderHeader.BillingAddressUID = null;
        TemplateSelectionItems.ForEach(e => e.IsSelected = false);
        PurchaseOrderHeader.HasTemplate = false;
        PurchaseOrderHeader.PurchaseOrderTemplateHeaderUID = null;
        PurchaseOrderItemViews.Clear();
        FilteredPurchaseOrderItemViews.Clear();
        SKUs.Clear();
        _ = UpdateHeader();
    }

    public async Task OnOrgUnitSelect(string orgUid, bool isDraftNotNeed = false)
    {
        PurchaseOrderHeader.OrgUnitUID = orgUid;
        if (SelectedStoreMaster is null)
        {
            return;
        }
        InitializeOptionalDependencies();
        SetInvoiceLevelApplicableTaxes();
        await PrepareWareHouseSelectionItems();
        SetStoreCredits();
        PopulateShipAndBillingAddressSelectionItems();
        if (!IsNewOrder || IsDraftOrder)
        {
            if (!isDraftNotNeed)
            {
                await BindDropDownsSelectedValues();
                await PopulateDraftOrder(PurchaseOrderMaster!.PurchaseOrderLines!, PurchaseOrderMaster.PurchaseOrderLineProvisions);
            }
        }

        await UpdateHeader();
    }

    public void ClearShippingAddressSelection()
    {
        PurchaseOrderHeader.ShippingAddressUID = string.Empty;
        TemplateSelectionItems.ForEach(e => e.IsSelected = false);
        if (!IsDraftOrder && IsNewOrder)
        {
            PurchaseOrderHeader.HasTemplate = false;
            PurchaseOrderHeader.PurchaseOrderTemplateHeaderUID = null;
        }
        PurchaseOrderItemViews.Clear();
        FilteredPurchaseOrderItemViews.Clear();
        SKUs.Clear();
        _ = UpdateHeader();
    }

    public async Task OnShipToAddressSelect(string uid)
    {
        try
        {
            PurchaseOrderHeader.ShippingAddressUID = uid;
            SKUs.Clear();
            List<IAsmDivisionMapping>? divisions;
            if (SelectedStoreMaster!.Store.IsAsmMappedByCustomer)
            {
                divisions = await _purchaseOrderDataHelper.GetAsmDivisionMappingByUID("store",
                SelectedStoreMaster.Store.UID, _appUser.Role.Code == "ASM" ? _appUser.Emp.UID : null);
            }
            else
            {
                divisions = await _purchaseOrderDataHelper.GetAsmDivisionMappingByUID("address", uid,
                _appUser.Role.Code == "ASM" ? _appUser.Emp.UID : null);
            }
            if (divisions is null || !divisions.Any())
            {
                throw new CustomException(ExceptionStatus.Failed, "No divisions found for this address");
            }

            DivisionEmpKVPair = divisions.ToDictionary(
            e => e.DivisionUID,// Key: Division UID
            f => f.AsmEmpUID// Value: Employee UID
            );
            PagingRequest pagingRequest = new()
            {
                FilterCriterias =
                [
                    new FilterCriteria("OrgUID", _appUser.OrgUIDs, FilterType.Equal),
                    new FilterCriteria("StoreUID", SelectedStoreMaster!.Store.UID, FilterType.Equal),
                    new FilterCriteria("Date", DateTime.Now, FilterType.Equal),
                    new FilterCriteria("SupplierOrgUIDs", DivisionEmpKVPair.Keys, FilterType.In),
                ]
            };
            SKUs.AddRange(await _addProductPopUpDataHelper.GetAllSKUs(pagingRequest));
            if (_appUser.Role.Code == "ASM")
            {
                SKUs.RemoveAll(e => !_appUser.AsmDivisions.Contains(e.SupplierOrgUID));
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task OnTemplateSelect(string templateUID)
    {
        PurchaseOrderHeader.PurchaseOrderTemplateHeaderUID = templateUID;
        PurchaseOrderHeader.HasTemplate = true;
        if (IsDraftOrder)
        {
            List<string> purchaseOrderLineUids = PurchaseOrderItemViews
                .Where(e => TemplateSKUs.Contains(e.SKUUID))
                .Select(i => i.UID)
                .ToList();
            _ = await _purchaseOrderDataHelper.DeletePurchaseOrderLinesByUIDs(purchaseOrderLineUids);
        }
        _ = PurchaseOrderItemViews.RemoveAll(e => TemplateSKUs.Contains(e.SKUUID));
        _ = FilteredPurchaseOrderItemViews.RemoveAll(e => TemplateSKUs.Contains(e.SKUUID));
        TemplateSKUs.Clear();
        List<IPurchaseOrderTemplateLine>? purchaseOrderTemplateLines = await _purchaseOrderDataHelper
            .GetAllPurchaseOrderTemplateLines(templateUID);
        if (purchaseOrderTemplateLines == null)
        {
            throw new CustomException(ExceptionStatus.Failed, "Failed to retrive the template data");
        }

        if (!purchaseOrderTemplateLines.Any())
        {
            throw new CustomException(ExceptionStatus.Failed, "No items found in the template");
        }
        List<string> actualSKUAvailable = SKUs.Select(e => e.UID).ToList();
        TemplateSKUs.AddRange(purchaseOrderTemplateLines.Select(e => e.SKUUID).ToList());
        await AddProductsToGridBySKUUIDs(TemplateSKUs.FindAll(actualSKUAvailable.Contains).ToList());
        foreach (IPurchaseOrderTemplateLine item in purchaseOrderTemplateLines)
        {
            IPurchaseOrderItemView? purchaseOrderItemView = PurchaseOrderItemViews.Find(e => e.SKUUID == item.SKUUID);
            if (purchaseOrderItemView != null)
            {
                purchaseOrderItemView.RequestedQty = item.Qty;
                purchaseOrderItemView.FinalQty = item.Qty;
                _ = OnQtyChange(purchaseOrderItemView);
            }
        }
    }

    public async Task ClearTemplateItems()
    {
        var skus = SKUs.Select(e => e.UID).ToList();
        _ = PurchaseOrderItemViews.RemoveAll(e => TemplateSKUs.Contains(e.SKUUID));
        _ = FilteredPurchaseOrderItemViews.RemoveAll(e => TemplateSKUs.Contains(e.SKUUID));
        await UpdateHeader();
    }

    public async Task PrepareDistributors()
    {
        Stores.Clear();
        List<IStore>? stores = await _purchaseOrderDataHelper.GetChannelPartner(_appUser.SelectedJobPosition.UID);
        if (stores != null && stores.Count > 0)
        {
            Stores.AddRange(stores);
        }
    }

    public async Task PrepareSelectedStoreMaster()
    {
        string distributorUID = SelectedDistributor ?? _appUser.SelectedJobPosition.OrgUID;
        IStoreMaster? storeMaster = await _purchaseOrderDataHelper.GetStoreMasterByStoreUID(distributorUID);
        if (storeMaster == null)
        {
            throw new CustomException(ExceptionStatus.Failed, "SKU master fetching failed..");
        }

        SelectedStoreMaster = storeMaster;
    }

    private async Task PopulateTemplates(string orgUID, string storeUid = null)
    {
        TemplateSelectionItems.Clear();
        List<IPurchaseOrderTemplateHeader>? data =
            await _purchaseOrderDataHelper.GetPurchaseOrderTemplateHeadersByStoreUidAndCreatedby(orgUID, storeUid);
        if (data != null)
        {
            TemplateSelectionItems
                .AddRange(CommonFunctions
                    .ConvertToSelectionItems(data, e => e.UID,
                    e => e.UID,
                    e => e.TemplateName,
                    e => e.CreatedBy == _appUser.Emp.UID ? "#ffe0e0" : null
                    ));
        }
    }

    private void SetCurrency()
    {
        CurrencyUID = _appUser.DefaultOrgCurrency?.CurrencyUID;
        CurrencyLabel = _appUser.DefaultOrgCurrency?.Symbol;
    }

    private void SetStoreCredits()
    {
        if (SelectedStoreMaster!.storeCredits == null || !SelectedStoreMaster.storeCredits.Any())
        {
            return;
        }

        CreditLimit = SelectedStoreMaster!.storeCredits?.First()?.CreditLimit ?? 0;
        AvailableLimit = SelectedStoreMaster!.storeCredits?.First()?.AvailableBalance ?? 0;
        CurrentOutStanding = SelectedStoreMaster!.storeCredits?.First()?.OverdueBalance ?? 0;
    }

    private void PopulateShipAndBillingAddressSelectionItems()
    {
        if (SelectedStoreMaster!.Addresses != null && SelectedStoreMaster.Addresses.Any())
        {
            ShippingAddressSelectionItems.Clear();
            ShippingAddressSelectionItems
                .AddRange(CommonFunctions
                    .ConvertToSelectionItems<IAddress>(SelectedStoreMaster.Addresses?
                        .Where(e => e.Type == "Shipping" && e.OrgUnitUID == PurchaseOrderHeader.OrgUnitUID &&
                            !string.IsNullOrEmpty(e.CustomField3))?
                        .ToList(), e => e.UID, e => e.Name,
                    e => $"[{e.CustomField3}] {e.Line1}, {e.Line2}, {e.City}, {e.State}, {e.ZipCode}"));
            BillingAddressSelectionItems.Clear();
            BillingAddressSelectionItems
                .AddRange(CommonFunctions
                    .ConvertToSelectionItems<IAddress>(SelectedStoreMaster.Addresses?
                        .Where(e => e.Type == "Billing" && e.OrgUnitUID == PurchaseOrderHeader.OrgUnitUID &&
                            !string.IsNullOrEmpty(e.CustomField3))?
                        .ToList(), e => e.UID, e => e.Name,
                    e => $"[{e.CustomField3}] {e.Line1}, {e.Line2}, {e.City}, {e.State}, {e.ZipCode}"));
        }
    }

    public virtual async Task SetSystemSettingValues()
    {
        SetDefaultDeliveryDay();
        //SetCurrency();
        //SetDefaultMaxQty();
        await Task.CompletedTask;
    }

    private void SetDefaultDeliveryDay()
    {
        DefaultDeliveryDay = _appSetting.DefaultDeliveryDay;
        PurchaseOrderHeader.ExpectedDeliveryDate = DateTime.Now.AddDays(DefaultDeliveryDay);
    }

    public virtual void InitializeOptionalDependencies()
    {
        AddTaxCalculatorDependency();
    }

    public void SetInvoiceLevelApplicableTaxes()
    {
        if (_purchaseOrderTaxCalculator == null)
        {
            return;
        }
        InvoiceApplicableTaxes = _purchaseOrderTaxCalculator.GetApplicableTaxesByApplicableAt(TaxDictionary, "Invoice");
    }

    protected virtual void AddTaxCalculatorDependency()
    {
        if (SelectedStoreMaster != null && SelectedStoreMaster.Store.IsTaxApplicable)
        {
            ITaxCalculator _taxCalculator = _serviceProvider.CreateInstance<ITaxCalculator>();
            _taxCalculator.SetTaxCalculator(_serviceProvider, _appSetting);
            _purchaseOrderTaxCalculator = new PurchaseOrderTaxCalculator(_taxCalculator, _appSetting);
            _purchaseOrderTaxCalculator.SetOrderViewModel(this);
        }
    }

    async private Task PrepareFilters()
    {
        await PopulateSKUAttributeData();
        await Task.Run(PrepareGridFilters);
    }

    public async Task PopulateSKUAttributeData()
    {
        SKUAttributeData.Clear();
        List<SKUAttributeDropdownModel>? skuAttibuteData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        if (skuAttibuteData != null && skuAttibuteData.Any())
        {
            SKUAttributeData.AddRange(skuAttibuteData);
        }
    }

    public bool FilterAction(List<FilterCriteria> filterCriterias, ISKUV1 sKUV1)
    {
        return _addProductPopUpDataHelper.FilterAction(filterCriterias, sKUV1);
    }


    public async Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID)
    {
        List<SKUGroupSelectionItem> data =
            await _purchaseOrderDataHelper.GetSKUGroupSelectionItemBySKUGroupTypeUID(null, selectedItemUID);
        return data != null && data.Any() ? data.ToList<ISelectionItem>() : [];
    }

    private async Task PrepareWareHouseSelectionItems()
    {
        WareHouseSelectionItems.Clear();
        List<IOrg>? wareHouses =
            await _purchaseOrderDataHelper.GetWareHouseData("FRWH", SelectedStoreMaster!.Store.UID);
        if (wareHouses != null && wareHouses.Any())
        {
            WareHouseSelectionItems.AddRange(
            CommonFunctions.ConvertToSelectionItems<IOrg>(wareHouses, ["UID", "Code", "Name"]));
            PurchaseOrderHeader.WareHouseUID = wareHouses.First().UID;
        }
        else
        {
            throw new CustomException(ExceptionStatus.Failed, "No warehouse available for this customer..");
        }
    }

    private async Task PrepareGridFilters()
    {
        OrganizationUnitSelectionItems.Clear();
        DivisionSelectionItems.Clear();
        ProductCategorySelectionItems.Clear();

        Task<List<ISelectionItem>?> orgUnits = _purchaseOrderDataHelper.GetProductOrgSelectionItems();
        Task<List<ISelectionItem>?> orgDivisions = _purchaseOrderDataHelper.GetProductDivisionSelectionItems();
        _ = await Task.WhenAll(orgUnits, orgDivisions);
        if (orgUnits.Result != null)
        {
            OrganizationUnitSelectionItems.AddRange(orgUnits.Result);
        }

        if (orgDivisions.Result != null)
        {
            DivisionSelectionItems.AddRange(orgDivisions.Result.Select(e =>
            {
                e.Code = e.UID;
                return e;
            }));
        }

        SKUAttributeDropdownModel? productCategorySelectionItems =
            SKUAttributeData.Find(e => e.DropDownTitle == "Product Category");
        if (productCategorySelectionItems != null)
        {
            ProductCategorySelectionItems.AddRange(productCategorySelectionItems.DropDownDataSource);
        }
    }

    public void ApplyGridFilter()
    {
        FilteredPurchaseOrderItemViews.Clear();
        List<IPurchaseOrderItemView> items = PurchaseOrderItemViews.Where(e =>
            string.IsNullOrEmpty(ProductSearchString) ||
            (e.SKUCode?.Contains(ProductSearchString, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.SKUName?.Contains(ProductSearchString, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        List<string>? selectedOrgs = new();//OrganizationUnitSelectionItems.Where(e => e.IsSelected && e.UID != null)
        //.Select(_ => _.UID!).ToList();
        List<string>? selectedDivisions = new();
        //DivisionSelectionItems.Where(e => e.IsSelected && e.UID != null)
        //  .Select(_ => _.UID!).ToList();
        List<string>? selectedProductCategories = new();//ProductCategorySelectionItems
        //.Where(e => e.IsSelected && e.Code != null).Select(_ => _.Code!).ToList();


        List<IPurchaseOrderItemView> rmItems = [];
        foreach (IPurchaseOrderItemView? item in items)
        {
            if (selectedOrgs is not null && selectedOrgs.Any() && !item.FilterKeys.Any(selectedOrgs.Contains))
            {
                rmItems.Add(item);
                continue;
            }
            if (selectedDivisions != null && selectedDivisions.Any() &&
                !item.FilterKeys.Any(selectedDivisions.Contains))
            {
                rmItems.Add(item);
                continue;
            }
            if (selectedProductCategories != null && selectedProductCategories.Any() &&
                !item.FilterKeys.Any(selectedProductCategories.Contains))
            {
                rmItems.Add(item);
                continue;
            }
        }
        _ = items.RemoveAll(rmItems.Contains);
        FilteredPurchaseOrderItemViews.AddRange(items);
    }
    protected bool IsExistingApply
    {
        get
        {
            return PurchaseOrderHeader.Status != PurchaseOrderStatusConst.Draft && !string.IsNullOrEmpty(PurchaseOrderHeader.UID);
        }
    }
    public async Task AddProductsToGridBySKUUIDs(List<string> sKUs)
    {
        _ = sKUs.RemoveAll(s => PurchaseOrderItemViews.Select(e => e.SKUUID).Contains(s));
        if (sKUs == null || !sKUs.Any())
        {
            return;
        }

        SKUMasterRequest sKUMasterRequest = new()
        {
            SKUUIDs = sKUs,
            OrgUIDs = _appUser.OrgUIDs
        };
        List<ISKUMaster> skuMasterData = await _purchaseOrderDataHelper.GetSKUsMasterBySKUUIDs(sKUMasterRequest);
        List<ISKUMaster> sKUsWithoutSupplier = skuMasterData.FindAll(e => string.IsNullOrEmpty(e.SKU.SupplierOrgUID));
        PreparePurchaseOrderItemViewsBySKUMaster(skuMasterData);
        List<string> schemeSKus = SellInSchemes.Select(e => e.SKUUID).ToList();
        List<string> requiredSchemeSkus = sKUs.FindAll(e => !schemeSKus.Contains(e));
        if (requiredSchemeSkus.Any())
        {
            if (IsExistingApply)
            {
                SellInSchemes.AddRange(await _purchaseOrderDataHelper.GetSellInSchemesByPOUid(PurchaseOrderHeader.UID));
            }
            else
            {
                SellInSchemes.AddRange(await _purchaseOrderDataHelper.GetSellInSchemesByOrgUidAndSKUUid(SelectedStoreMaster.Store.UID, requiredSchemeSkus));
            }
            //QPSSchemes.AddRange(await _purchaseOrderDataHelper.GetQPSSchemesByStoreUIDAndSKUUID(SelectedStoreMaster.Store.UID, requiredSchemeSkus));
        }
        Task a = PopulatePriceMaster(skuMasterData.Select(e => e.SKU.UID).ToList());
        Task standingSchemes = PopulateStandingSchemes(skuMasterData.Select(e => e.SKU.UID).ToList());
        Task qpsSchems = PopulateQPSSchemes(skuMasterData.Select(e => e.SKU.UID).ToList());
        await Task.WhenAll(a, standingSchemes, qpsSchems);
        ApplyGridFilter();
        if (sKUsWithoutSupplier != null && sKUsWithoutSupplier.Any())
        {
            throw new CustomException(ExceptionStatus.Failed,
            $"No supplier mapped for this items: {string.Join(", ", sKUsWithoutSupplier.Select(e => e.SKU.Code))}");
        }
    }

    private void PreparePurchaseOrderItemViewsBySKUMaster(List<ISKUMaster> skuMasterData)
    {
        foreach (ISKUMaster skuMaster in skuMasterData)
        {
            IPurchaseOrderItemView purchaseOrderItemView =
                ConvertToPurchaseOrderItemView(skuMaster, PurchaseOrderItemViews.Count + 1);
            if (IsNewOrder)
            {
                AddCreateFields(purchaseOrderItemView);
            }
            PurchaseOrderItemViews.Add(purchaseOrderItemView);
        }
    }

    // private void PopulateStandingSchemeSelectionItems(List<IPurchaseOrderItemView> purchaseOrderItemViews)
    // {
    //     foreach (var purchaseOrderItemView in purchaseOrderItemViews)
    //     {
    //         purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInCnP1UnitValueSelected = purchaseOrderItemView.SellInCnP1UnitValue > 0;
    //         purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInP2AmountSelected = purchaseOrderItemView.SellInP2Amount > 0;
    //         purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInP3AmountSelected = purchaseOrderItemView.SellInP3Amount > 0;
    //         purchaseOrderItemView.PurchaseOrderSchemeSelection.IsCashDiscountValueSelected = purchaseOrderItemView.CashDiscountValue > 0;
    //         purchaseOrderItemView.PurchaseOrderSchemeSelection.IsP2QPSTotalValueSelected = purchaseOrderItemView.P2QPSTotalValue > 0;
    //         if (!string.IsNullOrEmpty(purchaseOrderItemView.StandingSchemeData))
    //         {
    //
    //             List<StandingSchemeData> data = JsonConvert.DeserializeObject<List<StandingSchemeData>>(purchaseOrderItemView.StandingSchemeData);
    //             data.ForEach(e =>
    //             {
    //                 if (e.Amount == default)
    //                 {
    //                     purchaseOrderItemView.PurchaseOrderSchemeSelection.UnselectedStandingScheme.Add(e.SchemeCode);
    //                 }
    //             });
    //
    //             purchaseOrderItemView.PurchaseOrderSchemeSelection.UnselectedStandingScheme = purchaseOrderItemView.P3QPSTotalValue > 0;
    //         }
    //     }
    // }

    public virtual IPurchaseOrderItemView ConvertToPurchaseOrderItemView(ISKUMaster sKUMaster, int lineNumber,
        List<string>? skuImages = null)
    {
        Winit.Modules.SKU.Model.Interfaces.ISKUConfig? sKUConfig = sKUMaster.SKUConfigs?.FirstOrDefault();
        List<ISKUUOMView>? sKUUOMViews = ConvertToISKUUOMView(sKUMaster.SKUUOMs);
        ISKUUOMView? defaultUOM = sKUUOMViews
            ?.FirstOrDefault(e => e.Code == sKUConfig?.SellingUOM);
        ISKUUOMView? baseUOM = sKUUOMViews
            ?.FirstOrDefault(e => e.IsBaseUOM);
        IPurchaseOrderItemView purchaseOrderItemView = _serviceProvider.GetService<IPurchaseOrderItemView>() ??
            throw new Exception("IPurchaseOrderItemView is not registered");
        //purchaseOrderItemView.UID = Guid.NewGuid().ToString();
        purchaseOrderItemView.LineNumber = lineNumber;
        purchaseOrderItemView.SKUUID = sKUMaster.SKU.UID;
        purchaseOrderItemView.SKUCode = sKUMaster.SKU.Code;
        purchaseOrderItemView.SKUName = sKUMaster.SKU.Name;
        purchaseOrderItemView.IsPromo = false;
        purchaseOrderItemView.BaseUOM = baseUOM?.Code;
        purchaseOrderItemView.SelectedUOM = defaultUOM;
        purchaseOrderItemView.AllowedUOMs = sKUUOMViews;
        purchaseOrderItemView.BasePrice = 0;
        purchaseOrderItemView.UnitPrice = 0;
        //purchaseOrderItemView.IsTaxable = SelectedStoreViewModel?.IsTaxApplicable ?? false;
        purchaseOrderItemView.ApplicableTaxes = sKUMaster.ApplicableTaxUIDs;
        purchaseOrderItemView.TotalAmount = 0;
        purchaseOrderItemView.TotalDiscount = 0;
        purchaseOrderItemView.LineDiscount = 0;
        purchaseOrderItemView.HeaderDiscount = 0;
        //purchaseOrderItemView.TotalExciseDuty = 0;
        purchaseOrderItemView.LineTaxAmount = 0;
        purchaseOrderItemView.HeaderTaxAmount = 0;
        //purchaseOrderItemView.SKUPriceUID = null;
        //urchaseOrderItemView.SKUPriceListUID = null;
        purchaseOrderItemView.Attributes = ConvertToISKUAttributeView(sKUMaster.SKUAttributes);
        purchaseOrderItemView.ItemStatus = ItemState.Primary;
        purchaseOrderItemView.ApplicablePromotionUIDs = null;
        purchaseOrderItemView.SupplierOrgUID = sKUMaster.SKU.SupplierOrgUID;
        //purchaseOrderItemView. = string.Empty;
        //purchaseOrderItemView.CurrencyLabel = this.CurrencyLabel;
        purchaseOrderItemView.CatalogueURL = sKUMaster.SKU.CatalogueURL;
        purchaseOrderItemView.SKUImages = skuImages;
        purchaseOrderItemView.ModelDescription = $"[{sKUMaster.SKU.Code}]" + sKUMaster.SKU.Name;
        purchaseOrderItemView.PurchaseOrderLineProvisions = new System.Collections.Generic.List<IPurchaseOrderLineProvision>();
        if (sKUMaster.SKU is ISKUV1 sKuv1)
        {
            purchaseOrderItemView.FilterKeys = sKuv1.FilterKeys;
            purchaseOrderItemView.L2 = sKuv1.L2;
            purchaseOrderItemView.ProductCategoryId = sKuv1.ProductCategoryId;
            purchaseOrderItemView.ModelCode = sKuv1.ModelCode;
            purchaseOrderItemView.ModelDescription = (sKuv1.L2 ?? "") + $"[{sKuv1.ModelCode}]" + sKuv1.Name;
        }
        else
        {
            sKUMaster.SKUAttributes?.ForEach(e => purchaseOrderItemView.FilterKeys.Add(e.Code));
        }
        purchaseOrderItemView.SKUImage = !string.IsNullOrEmpty(sKUMaster.SKU.SKUImage)
            ? Path.Combine(_appConfigs.ApiDataBaseUrl, sKUMaster.SKU.SKUImage.ToString())
            : "/Data/SKU/no_image_available.jpg";

        return purchaseOrderItemView;
    }

    protected async Task PopulatePriceMaster(List<string> skuUIDs)
    {
        FilterCriteria filterCriteria = new("SKUUID", skuUIDs, FilterType.In);
        FilterCriteria orgfilterCriteria = new("OrgUID", SelectedStoreMaster!.Store.UID, FilterType.Equal);
        FilterCriteria datefilterCriteria = new("Date", PurchaseOrderHeader.OrderDate, FilterType.Equal);
        PagedResponse<ISKUPrice> pagedResponse = await _purchaseOrderDataHelper
            .PopulatePriceMaster(filterCriterias: [filterCriteria, orgfilterCriteria, datefilterCriteria],
            type: _appSetting.PriceApplicationModel);
        if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Any())
        {
            IEnumerable<ISKUPrice> sKUPrices = pagedResponse.PagedData;
            foreach (ISKUPrice sKUPrice in sKUPrices)
            {
                // Find the corresponding item in SalesOrderItemViews by matching some identifier, e.g., SKU or ID
                IPurchaseOrderItemView? purchaseOrderItemView =
                    PurchaseOrderItemViews.FirstOrDefault(item => item.SKUUID == sKUPrice.SKUUID);

                if (purchaseOrderItemView != null)
                {
                    // Update the price in SalesOrderItemViews
                    purchaseOrderItemView.BasePrice = CommonFunctions.RoundForSystem(sKUPrice.Price, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.UnitPrice = CommonFunctions.RoundForSystem(sKUPrice.Price, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.Mrp = CommonFunctions.RoundForSystem(sKUPrice.MRP, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.SKUPriceUID = sKUPrice.UID;
                    purchaseOrderItemView.SKUPriceListUID = sKUPrice.SKUPriceListUID;
                    purchaseOrderItemView.PriceVersionNo = sKUPrice.VersionNo;
                    purchaseOrderItemView.DummyPrice = CommonFunctions.RoundForSystem(sKUPrice.DummyPrice, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.DpPrice = CommonFunctions.RoundForSystem(sKUPrice.DummyPrice, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.LadderingPercentage = sKUPrice.LadderingPercentage;
                    purchaseOrderItemView.LadderingDiscount = CommonFunctions.RoundForSystem(sKUPrice.LadderingAmount, _appSetting.RoundOffDecimal);
                    purchaseOrderItemView.MinSellingPrice = CommonFunctions.RoundForSystem(sKUPrice.PriceLowerLimit, _appSetting.RoundOffDecimal);

                    //salesOrderItem.PriceLowerLimit = sKUPrice.PriceLowerLimit;
                    //salesOrderItem.PriceUpperLimit = sKUPrice.PriceUpperLimit;
                    //List<ISKUPriceLadderingData> skuPriceLaddering = await _ladderingCalculator
                    //.ApplyPriceLaddering(SelectedStoreMaster!.Store.BroadClassification, PurchaseOrderHeader.OrderDate,
                    //[purchaseOrderItemView.ProductCategoryId]);

                    //if (skuPriceLaddering != null && skuPriceLaddering.Any())
                    //{
                    //    purchaseOrderItemView.LadderingPercentage = skuPriceLaddering.First().PercentageDiscount;
                    //    purchaseOrderItemView.LadderingDiscount = purchaseOrderItemView.DummyPrice * skuPriceLaddering.First().PercentageDiscount * 0.01m;
                    //    purchaseOrderItemView.UnitPrice = purchaseOrderItemView.DummyPrice - purchaseOrderItemView.LadderingDiscount;
                    //}
                }
            }
        }
    }

    private async Task PopulateStandingSchemes(List<string> skuUIDs)
    {
        try
        {
            if (skuUIDs == null || skuUIDs.Count == 0) return;
            List<ISKUFilter> skuFilters = ConvertToSKUToSKUFilters(skuUIDs);
            Dictionary<string, StandingSchemeResponse>? standingSchemeResponses = IsExistingApply ?
                await _purchaseOrderDataHelper.GetStandingSchemesByPOUid(PurchaseOrderHeader.UID, skuFilters) :
                await _purchaseOrderDataHelper.GetStandingSchemesByOrgUidAndSKUUid(SelectedStoreMaster.Store.UID, PurchaseOrderHeader.OrderDate, skuFilters);
            if (standingSchemeResponses != null)
            {
                foreach (var standingSchemeResponse in standingSchemeResponses)
                {
                    StandingSchemes.TryAdd(standingSchemeResponse.Key, standingSchemeResponse.Value);
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private async Task PopulateQPSSchemes(List<string> skuUIDs)
    {
        try
        {
            if (skuUIDs == null || skuUIDs.Count == 0) return;
            List<ISKUFilter> skuFilters = ConvertToSKUToSKUFilters(skuUIDs);
            List<IQPSSchemePO>? qpsSchemes = IsExistingApply ?
                await _purchaseOrderDataHelper.GetQPSSchemesByPOUid(PurchaseOrderHeader.UID, skuFilters) :
                await _purchaseOrderDataHelper.GetQPSSchemesByStoreUIDAndSKUUID(SelectedStoreMaster.Store.UID, PurchaseOrderHeader.OrderDate, skuFilters);
            if (qpsSchemes != null && qpsSchemes.Any())
            {
                QPSSchemes.AddRange(qpsSchemes);
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private List<ISKUFilter> ConvertToSKUToSKUFilters(List<string> skuUIDs)
    {
        List<ISKUFilter> skuFilters = new List<ISKUFilter>();
        foreach (string skuUID in skuUIDs)
        {
            ISKUV1 sku = SKUs.Find(e => e.UID == skuUID);
            if (sku == null) continue;
            ISKUFilter skuFilter = new SKUFilter();
            skuFilter.SKUUID = sku.UID;
            skuFilter.DivisionUID = sku.SupplierOrgUID;
            skuFilter.FilterKeys = sku.FilterKeys;
            skuFilters.Add(skuFilter);
        }
        return skuFilters;
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
            ISKUAttributeViews = [];
            foreach (ISKUAttributes skuAttribute in sKUAttributes)
            {
                string key = skuAttribute.Type;
                ISKUAttributeViews[key] = ConvertToISKUAttributeView(skuAttribute);
            }
        }
        return ISKUAttributeViews;
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
        List<ISKUUOMView>? sKUUOMViews = null;
        if (sKUUOMs != null)
        {
            sKUUOMViews = [];
            foreach (ISKUUOM sKUUOM in sKUUOMs)
            {
                sKUUOMViews.Add(ConvertToISKUUOMView(sKUUOM));
            }
        }
        return sKUUOMViews;
    }

    public async Task OnQtyChange(IPurchaseOrderItemView purchaseOrderItemView)
    {
        ApplyQtyChange(purchaseOrderItemView);

        RemoveAppliedScheme(purchaseOrderItemView);
        ApplySellInScheme(purchaseOrderItemView);
        UpdateAmountAndTax(purchaseOrderItemView);
        ApplyCashDiscount(purchaseOrderItemView);
        ApplyQPSDiscount(purchaseOrderItemView);
        ApplyStandingScheme(purchaseOrderItemView);


        // Apply Promotion
        //await ApplyPromotion();
        // Recalculate Amount And Tax after promotion
        ApplyMargin(purchaseOrderItemView);
        // Update Header
        await UpdateHeader();
    }

    public async Task UpdateHeader()
    {
        await _purchaseOrderLevelCalculator.ComputeOrderLevelTaxesAndOrderSummary();
        PurchaseOrderHeader.NetAmount = IsPriceInclusiveVat
            ? PurchaseOrderHeader.TotalAmount - PurchaseOrderHeader.TotalDiscount
            : PurchaseOrderHeader.TotalAmount - PurchaseOrderHeader.TotalDiscount + PurchaseOrderHeader.TotalTaxAmount;
        //PurchaseOrderHeader.NetAmount = PurchaseOrderHeader.TotalAmount - PurchaseOrderHeader.TotalDiscount + PurchaseOrderHeader.TotalTaxAmount;
    }

    private PromotionHeaderView PreparePromotionHeaderView()
    {
        PromotionHeaderView promoHeaderView = new()
        {
            SalesOrderUID = PurchaseOrderHeader.UID,
            TotalAmount = PurchaseOrderHeader.TotalAmount,
            TotalQty = PurchaseOrderHeader.QtyCount,
            TotalDiscount = PurchaseOrderHeader.TotalDiscount,
            promotionItemView = []
        };
        foreach (IPurchaseOrderItemView purchaseOrderItemView in PurchaseOrderItemViews.Where(e =>
            e.IsCartItem && e.ItemType != ItemType.FOC))
        {
            PromotionItemView promotionItemView = new()
            {
                UniqueUId = purchaseOrderItemView.UID,
                SKUUID = purchaseOrderItemView.SKUUID,
                IsCartItem = purchaseOrderItemView.IsCartItem,
                UOM = purchaseOrderItemView.SelectedUOM?.Code,
                Multiplier = purchaseOrderItemView.SelectedUOM.Multiplier,
                //promotionItemView.Qty = purchaseOrderItemView.Qty;
                //promotionItemView.QtyBU = purchaseOrderItemView.QtyBU;
                TotalAmount = purchaseOrderItemView.TotalAmount,
                TotalDiscount = purchaseOrderItemView.TotalDiscount,
                ItemType = Enum.GetName(purchaseOrderItemView.ItemType),
                IsDiscountApplied = true,
                BasePrice = purchaseOrderItemView.BasePrice,
                ReplacePrice = purchaseOrderItemView.UnitPrice,
                UnitPrice = purchaseOrderItemView.UnitPrice,
                PromotionUID = string.Empty
            };
            promoHeaderView.promotionItemView.Add(promotionItemView);
        }
        return promoHeaderView;
    }

    public virtual async Task ApplyPromotion()
    {
        if (SelectedStoreViewModel!.IsPromotionsBlock || ApplicablePromotionList == null
            || ApplicablePromotionList.Count == 0 ||
            DMSPromotionDictionary == null ||
            DMSPromotionDictionary.Count == 0)
        {
            return;
        }
        RemoveAppliedPromotion();

        PromotionHeaderView promoHeaderView = PreparePromotionHeaderView();
        string applicablePromotionUIDsFromDictionary = string.Join(",", ApplicablePromotionList);
        List<AppliedPromotionView>?
            appliedPromotionViewList =
                null;//_purchaseOrderDataHelper.ApplyPromotion(applicablePromotionUIDsFromDictionary, promoHeaderView,
        //DMSPromotionDictionary, PromotionPriority.MinPriority);
        if (appliedPromotionViewList == null || appliedPromotionViewList.Count == 0)
        {
            return;
        }
        foreach (AppliedPromotionView appliedPromotionView in appliedPromotionViewList)
        {
            if (appliedPromotionView.IsFOC && appliedPromotionView.FOCItems != null &&
                appliedPromotionView.FOCItems.Count > 0)
            {
                //FOC Promotion
                foreach (FOCItem fOCItem in appliedPromotionView.FOCItems)
                {
                    // await AddOrUpdateFOCSalesOrderItem(fOCItem);
                }
            }
            else
            {
                IPurchaseOrderItemView? purchaseOrderItemView = PurchaseOrderItemViews
                    .FirstOrDefault(e => e.UID == appliedPromotionView.UniqueUID);

                if (purchaseOrderItemView == null)
                {
                    continue;
                }
                purchaseOrderItemView.AppliedPromotionUIDs ??= [];
                //salesOrderItemView.PromotionUID = appliedPromotionView.PromotionUID??"";
                _ = purchaseOrderItemView.AppliedPromotionUIDs.Add(appliedPromotionView.PromotionUID!);

                //Discount Promotion. Multiple promotion can apply so appending Disount Amount
                //purchaseOrderItemView.TotalLineDiscount += appliedPromotionView.DiscountAmount;
            }
            //TotalHeaderDiscount += appliedPromotionView.DiscountAmount;
        }
        await Task.CompletedTask;
    }

    private void RemoveAppliedPromotion()
    {
        foreach (IPurchaseOrderItemView purchaseOrderItemView in PurchaseOrderItemViews.Where(e => e.IsPromo == true))
        {
            //vk added on 5th Jul 2020
            purchaseOrderItemView.ItemType = ItemType.O;
            purchaseOrderItemView.AppliedPromotionUIDs?.Clear();
            purchaseOrderItemView.TotalDiscount = 0;
            ResetReplacePromotion(purchaseOrderItemView);
            // Delete all FOC Item
            //DeleteFOCItemForPromotion(SO, salesOrderItemView, "");
        }
        ResetFOCItems();
    }

    public void ResetFOCItems()
    {
        foreach (IPurchaseOrderItemView purchaseOrderItemView in FOCPurchaseOrderItemViews)
        {
            _ = PurchaseOrderItemViews.Remove(purchaseOrderItemView);
        }
        FOCPurchaseOrderItemViews.Clear();
    }

    public void ResetReplacePromotion(IPurchaseOrderItemView purchaseOrderItemView)
    {
        //if (purchaseOrderItemView.ReplacePrice > 0)
        //{
        //    purchaseOrderItemView.ReplacePrice = 0;
        //    purchaseOrderItemView.UnitPrice = purchaseOrderItemView.BasePrice;
        //}
    }

    public virtual void ApplyQtyChange(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (!IsNewOrder && !string.IsNullOrEmpty(purchaseOrderItemView.UID))
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
            purchaseOrderItemView.IsCartItem = purchaseOrderItemView.RequestedQty > 0;
        }
        ResetDiscount(purchaseOrderItemView);

        SetQtyBU(purchaseOrderItemView);
        //SetMissedOrderQty(purchaseOrderItemView);
        UpdateItemPrice(purchaseOrderItemView);
    }

    public virtual void ResetDiscount(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.AppliedPromotionUIDs?.Clear();
        purchaseOrderItemView.TotalDiscount = 0;
        purchaseOrderItemView.LineDiscount = 0;
    }

    public void SetQtyBU(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.SelectedUOM == null)
        {
            return;
        }
        purchaseOrderItemView.FinalQtyBU =
            purchaseOrderItemView.FinalQty * purchaseOrderItemView.SelectedUOM.Multiplier;
    }

    public virtual void SetMissedOrderQty(IPurchaseOrderItemView purchaseOrderItemView)
    {
        //if (purchaseOrderItemView.Qty > purchaseOrderItemView.VanQty)
        //{
        //    purchaseOrderItemView.MissedQty = purchaseOrderItemView.Qty - purchaseOrderItemView.VanQty;
        //}
        //else
        //{
        //    purchaseOrderItemView.MissedQty = 0;
        //}
    }

    public virtual void UpdateItemPrice(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.SelectedUOM == null)
        {
            return;
        }

        if (purchaseOrderItemView.ItemType == ItemType.FOC)
        {
            purchaseOrderItemView.UnitPrice = 0;
        }
        else if
            (!IsPriceEditable)// If price change not allowed then system should calculate else from external place it will be updated
        {
            purchaseOrderItemView.UnitPrice =
                CommonFunctions.RoundForSystem(purchaseOrderItemView.BasePrice *
                purchaseOrderItemView.SelectedUOM.Multiplier);
        }
        //purchaseOrderItemView.TotalAmount = purchaseOrderItemView.BasePrice * purchaseOrderItemView.SelectedUOM.Multiplier;
        UpdateAmountAndTax(purchaseOrderItemView);// This will be called after promotion calculation
    }

    public void UpdateAmountAndTax(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.TotalAmount =
            CommonFunctions.RoundForSystem(purchaseOrderItemView.FinalQty * purchaseOrderItemView.UnitPrice, _appSetting.RoundOffDecimal);
        //purchaseOrderItemView.EffectiveUnitPrice = purchaseOrderItemView.FinalQty == 0
        //    ? 0
        //    : (purchaseOrderItemView.TotalAmount - purchaseOrderItemView.TotalDiscount) /
        //    purchaseOrderItemView.FinalQty;

        //purchaseOrderItemView.EffectiveUnitPrice = purchaseOrderItemView.UnitPrice - purchaseOrderItemView.SellInDiscountUnitValue;
        purchaseOrderItemView.LineTaxAmount = 0;
        purchaseOrderItemView.TotalTaxAmount = 0;
        if (SelectedStoreMaster!.Store.IsTaxApplicable)
        {
            // Calculate Tax
            CalculateLineTax(purchaseOrderItemView);
        }
        UpdateNetAmount(purchaseOrderItemView);
    }

    public virtual void CalculateLineTax(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (_purchaseOrderTaxCalculator == null)
        {
            return;
        }
        _ = _purchaseOrderTaxCalculator.CalculateItemTaxes(purchaseOrderItemView);
        //purchaseOrderItemView.TotalLineTax = (purchaseOrderItemView.TotalAmount - purchaseOrderItemView.TotalDiscount) * .1m;
    }

    public void UpdateNetAmount(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.NetAmount = IsPriceInclusiveVat
            ? purchaseOrderItemView.TotalAmount - (purchaseOrderItemView.TotalDiscount)
            : purchaseOrderItemView.TotalAmount - (purchaseOrderItemView.TotalDiscount +
            purchaseOrderItemView.TotalTaxAmount);
    }

    public void Validate()
    {
        List<string> errorMSG = [];
        if (string.IsNullOrEmpty(PurchaseOrderHeader.OrgUID))
        {
            errorMSG.Add("Channel Partner");
        }
        if (string.IsNullOrEmpty(PurchaseOrderHeader.OrgUnitUID))
        {
            errorMSG.Add("Organization Unit");
        }
        if (string.IsNullOrEmpty(PurchaseOrderHeader.WareHouseUID))
        {
            //errorMSG.Add("WareHouse");
        }
        if (string.IsNullOrEmpty(PurchaseOrderHeader.ShippingAddressUID))
        {
            errorMSG.Add("ShippingAddress");
        }
        if (string.IsNullOrEmpty(PurchaseOrderHeader.BillingAddressUID))
        {
            errorMSG.Add("BillingAddress");
        }
        if (!PurchaseOrderItemViews.Any())
        {
            errorMSG.Add("Items");
        }
        foreach (IPurchaseOrderItemView item in PurchaseOrderItemViews)
        {
            if (item.FinalQty == 0 && !errorMSG.Contains("Quantity"))
            {
                errorMSG.Add("Quantity");
            }
        }
        if (errorMSG.Any())
        {
            throw new CustomException(ExceptionStatus.Failed,
            $"Please select the following fields: {string.Join(", ", errorMSG)}");
        }
    }

    public async Task DeleteSelectedItems()
    {
        IEnumerable<string> purchaseOrderLineUIDs = PurchaseOrderItemViews.Where(e => e.IsSelected)
            .Select(e => e.UID);
        if (IsDraftOrder)
        {
            _ = await _purchaseOrderDataHelper.DeletePurchaseOrderLinesByUIDs(purchaseOrderLineUIDs);
        }
        _ = PurchaseOrderItemViews.RemoveAll(e => e.IsSelected);
        _ = FilteredPurchaseOrderItemViews.RemoveAll(e => e.IsSelected);
        //_purchaseOrderLevelCalculator.
        int count = 1;
        foreach (IPurchaseOrderItemView item in PurchaseOrderItemViews)
        {
            item.LineNumber = count++;
        }
        _ = UpdateHeader();
        throw new CustomException(ExceptionStatus.Success, "Items deleted successfully..");
    }

    public virtual async Task SaveOrder(string purchaseOrderStatus = PurchaseOrderStatusConst.Draft)
    {
        string orderType = string.Empty;
        PurchaseOrderHeader.WareHouseUID = WareHouseSelectionItems.First().UID;
        IOrg? org = await _purchaseOrderDataHelper.GetOrgByUID(SelectedStoreMaster!.Store.UID);
        if (org == null)
        {
            throw new CustomException(ExceptionStatus.Failed, "Org retrving failed");
        }
        IAddress? address = SelectedStoreMaster!.Addresses.Find(e => e.UID == PurchaseOrderHeader.ShippingAddressUID);
        if (address == null) throw new CustomException(ExceptionStatus.Failed, "Select ship to address");
        PurchaseOrderHeader.BranchUID = address.BranchUID
            ?? throw new CustomException(ExceptionStatus.Failed,
            "Branch is not available for this shipto address.");
        PurchaseOrderHeader.HOOrgUID = org.ParentUID;
        List<IPurchaseOrderMaster> purchaseOrderMasters = [];
        if (purchaseOrderStatus == PurchaseOrderStatusConst.Draft)
        {
            orderType = "save";
            purchaseOrderMasters.Add(PreparePurchaseOrderMasterForDraft());
            if (IsDraftOrder)
            {
                purchaseOrderMasters.First().ActionType = Shared.Models.Enums.ActionType.Update;
            }
        }

        else
        {
            orderType = "place";
            string? sourceWareHouseUID = null;
            if (IsNewOrder)
            {
                sourceWareHouseUID =
                    await _purchaseOrderDataHelper.GetWareHouseUIDbySalesOfficeUID(address.SalesOfficeUID!);
            }
            Dictionary<string, List<IPurchaseOrderItemView>> purchaseOrdersItemsGroup = PurchaseOrderItemViews
                .Where(e => e.IsCartItem && !string.IsNullOrEmpty(e.SupplierOrgUID))
                .GroupBy(e => e.SupplierOrgUID)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (KeyValuePair<string, List<IPurchaseOrderItemView>> items in purchaseOrdersItemsGroup)
            {
                IPurchaseOrderMaster purchaseOrderMaster = PreparePurchaseOrderMasterByPurchaseOrderItemViews(
                items.Value,
                items.Key,
                purchaseOrderStatus,
                IsNewOrder);
                if (!string.IsNullOrEmpty(sourceWareHouseUID))
                    purchaseOrderMaster.PurchaseOrderHeader!.SourceWareHouseUID = sourceWareHouseUID;
                purchaseOrderMaster.ApprovalRequestItem = await GenerateApprovalRequestIdAndRuleId(purchaseOrderMaster);
                purchaseOrderMaster.ApprovalStatusUpdate = ApprovalStatusUpdate;
                purchaseOrderMasters.Add(purchaseOrderMaster);
            }
        }


        if (await _purchaseOrderDataHelper.SaveOrder(purchaseOrderMasters))
        {
            PurchaseOrderUIDsForNotification = purchaseOrderMasters.Select(e => e.PurchaseOrderHeader!.UID).ToList();
            if (purchaseOrderStatus == PurchaseOrderStatusConst.Draft && _appUser.Role.Code == PurchaseOrderStatusConst.ASM)
            {
                List<string> smsTemplates = new List<string>
                {
                    NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_CP_FOR_APPROVAL,
                    //NotificationTemplateNames.PO_CREATED_BY_ASM_SEND_TO_BM_FOR_INFO,
                };
                await SendEmail(smsTemplates);
                await SendSms(smsTemplates);
            }
            if (purchaseOrderStatus == PurchaseOrderStatusConst.PendingForApproval)
            {
                PurchaseOrderUIDs = purchaseOrderMasters.Select(e => e.PurchaseOrderHeader!.UID).ToList();
                IEnumerable<string>? ordernumbers = purchaseOrderMasters?
                    .Select(e =>
                        purchaseOrderStatus == PurchaseOrderStatusConst.Draft
                            ? e.PurchaseOrderHeader.DraftOrderNumber
                            : e.PurchaseOrderHeader.OrderNumber);

                if (ordernumbers != null && ordernumbers.Any())
                {
                    CreatedPurchaseOrderNumbers.AddRange(ordernumbers);
                }
            }
            else if (purchaseOrderStatus == PurchaseOrderStatusConst.CancelledByCMI)
            {
                throw new CustomException(ExceptionStatus.Success, $"Purchase order rejected successfully.");
            }
            throw new CustomException(ExceptionStatus.Success, $"Purchase order {orderType} successfully...");
        }
        throw new CustomException(ExceptionStatus.Failed, $"Purchase order {orderType} failed...");
    }

    public void ApplyApprovedQty()
    {
        switch (ApprovalLevel)
        {
            case 1:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App1Qty = e.FinalQty;
                    PurchaseOrderHeader.App1EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App1Date = DateTime.Now;
                });
                break;
            case 2:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App2Qty = e.FinalQty;
                    PurchaseOrderHeader.App2EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App2Date = DateTime.Now;
                });
                break;
            case 3:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App3Qty = e.FinalQty;
                    PurchaseOrderHeader.App3EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App3Date = DateTime.Now;
                });
                break;
            case 4:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App4Qty = e.FinalQty;
                    PurchaseOrderHeader.App4EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App4Date = DateTime.Now;
                });
                break;
            case 5:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App5Qty = e.FinalQty;
                    PurchaseOrderHeader.App5EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App5Date = DateTime.Now;
                });
                break;
            case 6:
                PurchaseOrderItemViews.ForEach(e =>
                {
                    e.App6Qty = e.FinalQty;
                    PurchaseOrderHeader.App6EmpUID = _appUser.Emp.UID;
                    PurchaseOrderHeader.App6Date = DateTime.Now;
                });
                break;
        }
    }

    private void AddCreateFields(Winit.Modules.Base.Model.IBaseModel baseModel, bool isUIDRequired = true)
    {
        baseModel.CreatedBy = _appUser?.Emp?.UID;
        baseModel.ModifiedBy = _appUser?.Emp?.UID;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        if (isUIDRequired)
        {
            baseModel.UID = Guid.NewGuid().ToString();
        }
    }

    private void AddUpdateFields(Winit.Modules.Base.Model.IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _appUser?.Emp?.UID;
        baseModel.ModifiedTime = DateTime.Now;
    }

    private async Task PopulateDraftOrder(List<IPurchaseOrderLine> purchaseOrderLines, List<IPurchaseOrderLineProvision>? purchaseOrderLineProvisions = null)
    {
        try
        {
            await AddProductsToGridBySKUUIDs(purchaseOrderLines.Select(e => e.SKUUID).ToList());
            foreach (IPurchaseOrderLine line in purchaseOrderLines)
            {
                List<IPurchaseOrderLineProvision> purchaseOrderLineProvisionsForItem = purchaseOrderLineProvisions?.FindAll(e => e.PurchaseOrderLineUID == line.UID);
                UpdatePurchaseOrderItemViewByPurchaseOrderLine(line, purchaseOrderLineProvisionsForItem);
            }
            PurchaseOrderItemViews.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
            FilteredPurchaseOrderItemViews.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
            if (!IsExistingApply)
            {
                foreach (var item in PurchaseOrderItemViews)
                {
                    //item.PurchaseOrderLineProvisions.Clear();
                    await CheckIfAnySchemeIsExpiredAndNewOneAvailable(item);
                   // await OnQtyChange(item);
                }
               // ApplyGridFilter();
            }

        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task CheckIfAnySchemeIsExpiredAndNewOneAvailable(IPurchaseOrderItemView view)
    {

        ISellInSchemePO? applicablesellInScheme = SellInSchemes.Find(p => p.SKUUID == view.SKUUID);
        IQPSSchemePO? applicableQpsScheme = QPSSchemes.Find(p => p.SKU_UID == view.SKUUID);
        if (applicablesellInScheme != null)
        {
            if (!view.PurchaseOrderLineProvisions.Any(p => p.SchemeCode == applicablesellInScheme.SchemeCode && view.UID == p.PurchaseOrderLineUID))
            {
                view.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.SellInCnP1, ProvisionTypeConst.SellInP2, ProvisionTypeConst.SellInP3 }).Contains(p.ProvisionType));
                RemoveSellInScheme(view);
                ApplySellInScheme(view);
            }
        }
        else
        {
            view.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.SellInCnP1, ProvisionTypeConst.SellInP2, ProvisionTypeConst.SellInP3 }).Contains(p.ProvisionType));
        }
        if (applicableQpsScheme != null)
        {
            if (!view.PurchaseOrderLineProvisions.Any(p => p.SchemeCode == applicableQpsScheme.Scheme_Code && view.UID == p.PurchaseOrderLineUID))
            {
                view.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.P2QPS, ProvisionTypeConst.P3QPS }).Contains(p.ProvisionType));
                RemoveQPSScheme(view);
                ApplyQPSDiscount(view);
            }
        }
        else
        {
            view.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.P2QPS, ProvisionTypeConst.P3QPS }).Contains(p.ProvisionType));
        }
        if (IsCashDiscountExcluded)
        {
            RemoveCashScheme(view);
        }
        else
        {
            RemoveCashScheme(view);
            ApplyCashDiscount(view);
        }
        CheckIfAnyStandingIsExpiredAndNewOneAvailable(view);

        ApplyMargin(view);
        await UpdateHeader();
    }
    private void CheckIfAnyStandingIsExpiredAndNewOneAvailable(IPurchaseOrderItemView view)
    {
        List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions = view.PurchaseOrderLineProvisions.
             FindAll(e => e.ProvisionType == ProvisionTypeConst.StandingScheme);
        StandingSchemes.TryGetValue(view.SKUUID, out var standingScheme);
        if(standingScheme != null  && standingScheme.Schemes != null && standingScheme.Schemes.Count > 0)
        {
            List<string> standingApplicableSchemeCodes = standingScheme.Schemes.Select(e => e.SchemeCode).ToList();
            purchaseOrderLineProvisions.RemoveAll(e => !standingApplicableSchemeCodes.Contains(e.SchemeCode));

            decimal totalAmount = 0;
           standingScheme.Schemes.FindAll(p => purchaseOrderLineProvisions.Any(q=>q.SchemeCode!=p.SchemeCode)).ForEach(e =>
            {
                IPurchaseOrderLineProvision scheme = purchaseOrderLineProvisions.Find(p => p.SchemeCode == e.SchemeCode);
                if (scheme != null)
                {
                    e.IsSelected = scheme.IsSelected;
                    if (e.IsSelected)
                    {
                        scheme.ActualProvisionUnitAmount = e.Amount;
                        scheme.ApprovedProvisionUnitAmount = e.Amount;
                    }

                }
                else
                {
                    IPurchaseOrderLineProvision purchaseOrderLineProvision = new PurchaseOrderLineProvision();
                    purchaseOrderLineProvision.SchemeCode = e.SchemeCode;
                    purchaseOrderLineProvision.ActualProvisionUnitAmount = e.Amount;
                    purchaseOrderLineProvision.ApprovedProvisionUnitAmount = e.Amount;
                    purchaseOrderLineProvision.PurchaseOrderLineUID = view.UID;
                    purchaseOrderLineProvision.ProvisionType = ProvisionTypeConst.StandingScheme;
                    view.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvision);
                }
                if (e.IsSelected)
                    totalAmount += e.Amount;
            });
            //decimal totalAmount = standingScheme.Schemes.Where(i => i.IsSelected).Sum(e => e.Amount);
            view.StandingUnitValue = totalAmount;
            view.P3StandingAmount = CommonFunctions.RoundForSystem(totalAmount * view.FinalQty, _appSetting.RoundOffDecimal);
            view.StandingSchemeData = JsonConvert.SerializeObject(standingScheme.Schemes);

        }
    }


    // private void SetSchemeSelectionDefaults(IPurchaseOrderItemView purchaseOrderItemView)
    // {
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInCnP1UnitValueSelected = purchaseOrderItemView.SellInCnP1UnitValue != default;
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInP2AmountSelected = purchaseOrderItemView.SellInP2Amount != default;
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsSellInP3AmountSelected = purchaseOrderItemView.SellInP3Amount != default;
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsCashDiscountValueSelected = purchaseOrderItemView.CashDiscountValue != default;
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsP2QPSTotalValueSelected = purchaseOrderItemView.P2QPSTotalValue != default;
    //     purchaseOrderItemView.PurchaseOrderSchemeSelection.IsP3QPSTotalValueSelected = purchaseOrderItemView.P3QPSTotalValue != default;
    //     if (!string.IsNullOrEmpty(purchaseOrderItemView.StandingSchemeData))
    //     {
    //         List<StandingSchemeData> data = JsonConvert.DeserializeObject<List<StandingSchemeData>>(purchaseOrderItemView.StandingSchemeData);
    //         data.ForEach(e =>
    //         {
    //             if (e.Amount == default)
    //             {
    //                 purchaseOrderItemView.PurchaseOrderSchemeSelection.UnselectedStandingScheme.Add(e.SchemeCode);
    //             }
    //         });
    //     }
    // }

    private async Task BindDropDownsSelectedValues()
    {
        ISelectionItem? wareHouse = WareHouseSelectionItems.Find(e => e.UID == PurchaseOrderHeader.WareHouseUID);
        if (wareHouse is not null)
        {
            wareHouse.IsSelected = true;
        }

        ISelectionItem? orgUnitSelectionItem = OrgUnitSelectionItems.Find(e => e.UID == PurchaseOrderHeader.OrgUnitUID);
        if (orgUnitSelectionItem is not null)
        {
            orgUnitSelectionItem.IsSelected = true;
        }

        ISelectionItem? billingAddress =
            BillingAddressSelectionItems.Find(e => e.UID == PurchaseOrderHeader.BillingAddressUID);
        if (billingAddress is not null)
        {
            billingAddress.IsSelected = true;
        }

        ISelectionItem? shippingAddress =
            ShippingAddressSelectionItems.Find(e => e.UID == PurchaseOrderHeader.ShippingAddressUID);
        if (shippingAddress is not null)
        {
            shippingAddress.IsSelected = true;
        }
        await OnShipToAddressSelect(PurchaseOrderHeader.ShippingAddressUID);
        ISelectionItem? templateItem =
            TemplateSelectionItems.Find(e => e.UID == PurchaseOrderHeader.PurchaseOrderTemplateHeaderUID);
        if (templateItem is not null)
        {
            templateItem.IsSelected = true;
        }
    }

    private async Task ClearAllProductsAndUpdateHeader()
    {
        PurchaseOrderItemViews.Clear();
        FilteredPurchaseOrderItemViews.Clear();
        await UpdateHeader();
    }

    private void UpdatePurchaseOrderItemViewByPurchaseOrderLine(IPurchaseOrderLine purchaseOrderLine, List<IPurchaseOrderLineProvision>? purchaseOrderLineProvisions = null)
    {
        IPurchaseOrderItemView? purchaseOrderItemView =
            PurchaseOrderItemViews.Find(e => e.SKUUID == purchaseOrderLine.SKUUID);
        if (purchaseOrderItemView is not null)
        {
            purchaseOrderItemView.UID = purchaseOrderLine.UID;
            purchaseOrderItemView.RequestedQty = purchaseOrderLine.RequestedQty;
            purchaseOrderItemView.FinalQty = purchaseOrderLine.FinalQty;
            purchaseOrderItemView.NetAmount = purchaseOrderLine.NetAmount;
            purchaseOrderItemView.TotalDiscount = purchaseOrderLine.TotalDiscount;
            purchaseOrderItemView.TotalAmount = purchaseOrderLine.TotalAmount;
            purchaseOrderItemView.CreatedBy = purchaseOrderLine.CreatedBy;
            purchaseOrderItemView.ModifiedBy = purchaseOrderLine.ModifiedBy;
            purchaseOrderItemView.CreatedTime = purchaseOrderLine.CreatedTime;
            purchaseOrderItemView.ModifiedTime = purchaseOrderLine.ModifiedTime;
            purchaseOrderItemView.PurchaseOrderHeaderUID = purchaseOrderLine.PurchaseOrderHeaderUID;
            purchaseOrderItemView.LineNumber = purchaseOrderLine.LineNumber;
            purchaseOrderItemView.AvailableQty = purchaseOrderLine.AvailableQty;
            purchaseOrderItemView.ModelQty = purchaseOrderLine.ModelQty;
            purchaseOrderItemView.InTransitQty = purchaseOrderLine.InTransitQty;
            purchaseOrderItemView.SuggestedQty = purchaseOrderLine.SuggestedQty;
            purchaseOrderItemView.Past3MonthAvg = purchaseOrderLine.Past3MonthAvg;
            purchaseOrderItemView.FinalQtyBU = purchaseOrderLine.FinalQtyBU;
            purchaseOrderItemView.LineDiscount = purchaseOrderLine.LineDiscount;
            purchaseOrderItemView.HeaderDiscount = purchaseOrderLine.HeaderDiscount;
            purchaseOrderItemView.TotalTaxAmount = purchaseOrderLine.TotalTaxAmount;
            purchaseOrderItemView.LineTaxAmount = purchaseOrderLine.LineTaxAmount;
            purchaseOrderItemView.HeaderTaxAmount = purchaseOrderLine.HeaderTaxAmount;
            purchaseOrderItemView.App1Qty = purchaseOrderLine.App1Qty;
            purchaseOrderItemView.App2Qty = purchaseOrderLine.App2Qty;
            purchaseOrderItemView.App3Qty = purchaseOrderLine.App3Qty;
            purchaseOrderItemView.App4Qty = purchaseOrderLine.App4Qty;
            purchaseOrderItemView.App5Qty = purchaseOrderLine.App5Qty;
            purchaseOrderItemView.App6Qty = purchaseOrderLine.App6Qty;
            purchaseOrderItemView.Mrp = purchaseOrderLine.Mrp;
            purchaseOrderItemView.DpPrice = purchaseOrderLine.DpPrice;
            purchaseOrderItemView.LadderingPercentage = purchaseOrderLine.LadderingPercentage;
            purchaseOrderItemView.LadderingDiscount = purchaseOrderLine.LadderingDiscount;
            purchaseOrderItemView.SellInDiscountUnitValue = purchaseOrderLine.SellInDiscountUnitValue;
            purchaseOrderItemView.SellInDiscountUnitPercentage = purchaseOrderLine.SellInDiscountUnitPercentage;
            purchaseOrderItemView.SellInDiscountTotalValue = purchaseOrderLine.SellInDiscountTotalValue;
            purchaseOrderItemView.SellInCnP1UnitPercentage = purchaseOrderLine.SellInCnP1UnitPercentage;
            purchaseOrderItemView.SellInCnP1UnitValue = purchaseOrderLine.SellInCnP1UnitValue;
            purchaseOrderItemView.SellInCnP1Value = purchaseOrderLine.SellInCnP1Value;
            purchaseOrderItemView.CashDiscountPercentage = purchaseOrderLine.CashDiscountPercentage;
            purchaseOrderItemView.CashDiscountValue = purchaseOrderLine.CashDiscountValue;
            purchaseOrderItemView.SellInP2Amount = purchaseOrderLine.SellInP2Amount;
            purchaseOrderItemView.SellInP3Amount = purchaseOrderLine.SellInP3Amount;
            purchaseOrderItemView.P3StandingAmount = purchaseOrderLine.P3StandingAmount;
            purchaseOrderItemView.PromotionUID = purchaseOrderLine.PromotionUID;
            //purchaseOrderItemView.EffectiveUnitPrice = purchaseOrderLine.EffectiveUnitPrice;
            purchaseOrderItemView.EffectiveUnitTax = purchaseOrderLine.EffectiveUnitTax;
            purchaseOrderItemView.IsCartItem = true;
            purchaseOrderItemView.BilledQty = purchaseOrderLine.BilledQty;
            purchaseOrderItemView.CancelledQty = purchaseOrderLine.CancelledQty;
            purchaseOrderItemView.IsUpdatedFromErp = purchaseOrderLine.IsUpdatedFromErp;
            purchaseOrderItemView.SellInSchemeCode = purchaseOrderLine.SellInSchemeCode;
            purchaseOrderItemView.StandingSchemeData = purchaseOrderLine.StandingSchemeData;
            purchaseOrderItemView.QPSSchemeCode = purchaseOrderLine.QPSSchemeCode;
            purchaseOrderItemView.P2QPSTotalValue = purchaseOrderLine.P2QPSTotalValue;
            purchaseOrderItemView.P3QPSTotalValue = purchaseOrderLine.P3QPSTotalValue;
            purchaseOrderItemView.MarginUnitValue = purchaseOrderLine.MarginUnitValue;
            purchaseOrderItemView.QPSOfferType = purchaseOrderLine.QPSOfferType;
            purchaseOrderItemView.QPSOfferValue = purchaseOrderLine.QPSOfferValue;
            purchaseOrderItemView.QPSUnitValue = purchaseOrderLine.QPSUnitValue;
            purchaseOrderItemView.StandingUnitValue = purchaseOrderLine.StandingUnitValue;
            purchaseOrderItemView.PurchaseOrderLineProvisions = purchaseOrderLineProvisions ?? new();
        }
    }

    private IPurchaseOrderMaster PreparePurchaseOrderMasterByPurchaseOrderItemViews(
        List<IPurchaseOrderItemView> purchaseOrderItemViews,
        string divisionUID, string status, bool isNew = false)
    {
        IPurchaseOrderMaster purchaseOrderMaster = _serviceProvider.GetRequiredService<IPurchaseOrderMaster>();
        IPurchaseOrderHeader purchaseOrderHeader =
            isNew ? _serviceProvider.GetRequiredService<IPurchaseOrderHeader>() : PurchaseOrderHeader;
        purchaseOrderHeader.Status = status;
        if (isNew)
        {
            ISelectionItem divSelectionItem = DivisionSelectionItems.Find(e => e.UID == divisionUID);
            purchaseOrderHeader.DraftOrderNumber = PurchaseOrderHeader.DraftOrderNumber;
            GenerateOrderNumber(purchaseOrderHeader, PurchaseOrderStatusConst.PendingForApproval, divSelectionItem?.Code);
            purchaseOrderHeader.OrgUID = SelectedStoreMaster!.Store.UID;
            purchaseOrderHeader.DivisionUID = divisionUID;
            purchaseOrderHeader.WareHouseUID = PurchaseOrderHeader.WareHouseUID;
            purchaseOrderHeader.BillingAddressUID = PurchaseOrderHeader.BillingAddressUID;
            purchaseOrderHeader.ShippingAddressUID = PurchaseOrderHeader.ShippingAddressUID;
            purchaseOrderHeader.OrderDate = DateTime.Now;
            purchaseOrderHeader.ExpectedDeliveryDate = PurchaseOrderHeader.ExpectedDeliveryDate;
            purchaseOrderHeader.BranchUID = PurchaseOrderHeader.BranchUID;
            purchaseOrderHeader.HOOrgUID = PurchaseOrderHeader.HOOrgUID;
            purchaseOrderHeader.OrgUnitUID = PurchaseOrderHeader.OrgUnitUID;
            purchaseOrderHeader.ReportingEmpUID = DivisionEmpKVPair[purchaseOrderHeader.DivisionUID];
            purchaseOrderHeader.HasTemplate = true;
            purchaseOrderHeader.PurchaseOrderTemplateHeaderUID = purchaseOrderHeader.PurchaseOrderTemplateHeaderUID;
        }
        UpdateHeaderAmount(purchaseOrderHeader, purchaseOrderItemViews);
        UpdateHeaderCount(purchaseOrderHeader, purchaseOrderItemViews);
        _ = _purchaseOrderTaxCalculator.CalculateInvoiceTaxes(purchaseOrderHeader, purchaseOrderItemViews);
        purchaseOrderHeader.TotalTaxAmount = purchaseOrderHeader.LineTaxAmount + purchaseOrderHeader.HeaderTaxAmount;
        //purchaseOrderHeader.NetAmount = purchaseOrderHeader.TotalAmount - purchaseOrderHeader.TotalDiscount + purchaseOrderHeader.TotalTaxAmount;
        purchaseOrderHeader.NetAmount = IsPriceInclusiveVat
            ? purchaseOrderHeader.TotalAmount - purchaseOrderHeader.TotalDiscount
            : purchaseOrderHeader.TotalAmount - purchaseOrderHeader.TotalDiscount + purchaseOrderHeader.TotalTaxAmount;
        //_purchaseOrderTaxCalculator.SetOrderViewModel
        purchaseOrderMaster.PurchaseOrderHeader = purchaseOrderHeader;
        if (isNew)
        {
            AddCreateFields(purchaseOrderHeader);
            if (IsDraftOrder)
            {
                purchaseOrderHeader.CreatedBy = PurchaseOrderMaster.PurchaseOrderHeader.CreatedBy;
                purchaseOrderHeader.CreatedTime = PurchaseOrderMaster.PurchaseOrderHeader.CreatedTime;
            }
        }
        else
        {
            purchaseOrderMaster.ActionType = ActionType.Update;
            AddUpdateFields(purchaseOrderHeader);
        }
        PurchaseOrderUIDEmpKVPair[purchaseOrderHeader.UID] = purchaseOrderHeader.ReportingEmpUID;
        purchaseOrderMaster.PurchaseOrderLines = purchaseOrderItemViews.OfType<IPurchaseOrderLine>().ToList();
        purchaseOrderMaster.PurchaseOrderLineProvisions = purchaseOrderItemViews.SelectMany(e => e.PurchaseOrderLineProvisions).ToList();

        int count = 1;
        purchaseOrderMaster.PurchaseOrderLines.ForEach((e) =>
        {
            if (isNew && string.IsNullOrEmpty(e.UID))
            {
                AddCreateFields(e);
            }
            else
            {
                AddUpdateFields(e);
            }
            e.LineNumber = count++;
            e.PurchaseOrderHeaderUID = purchaseOrderHeader.UID;
        });
        purchaseOrderMaster.PurchaseOrderLineProvisions.ForEach(e =>
        {
            if (isNew || string.IsNullOrEmpty(e.UID))
            {
                AddCreateFields(e);
            }
            else AddUpdateFields(e);
        });
        return purchaseOrderMaster;
    }

    private void UpdateHeaderAmount(IPurchaseOrderHeader purchaseOrderHeader,
        List<IPurchaseOrderItemView> purchaseOrderItemViews)
    {
        purchaseOrderHeader.TotalAmount =
            CommonFunctions.RoundForSystem(purchaseOrderItemViews.Sum(e => e.TotalAmount));
        purchaseOrderHeader.LineDiscount =
            CommonFunctions.RoundForSystem(purchaseOrderItemViews.Sum(e => e.TotalDiscount));
        purchaseOrderHeader.TotalDiscount =
            CommonFunctions.RoundForSystem(purchaseOrderItemViews.Sum(e => e.TotalDiscount));
        purchaseOrderHeader.LineTaxAmount =
            CommonFunctions.RoundForSystem(purchaseOrderItemViews.Sum(e => e.LineTaxAmount));
        purchaseOrderHeader.TotalTaxAmount = 0;
    }

    private void UpdateHeaderCount(IPurchaseOrderHeader purchaseOrderHeader,
        List<IPurchaseOrderItemView> purchaseOrderItemViews)
    {
        purchaseOrderHeader.LineCount = purchaseOrderItemViews.Where(e => e.IsCartItem).Count();
        purchaseOrderHeader.QtyCount = purchaseOrderItemViews.Sum(e => e.FinalQty);
    }

    private IPurchaseOrderMaster PreparePurchaseOrderMasterForDraft()
    {
        IPurchaseOrderMaster purchaseOrderMaster = _serviceProvider.GetService<IPurchaseOrderMaster>()!;
        PurchaseOrderHeader.Status = PurchaseOrderStatusConst.Draft;
        GenerateOrderNumber(PurchaseOrderHeader, PurchaseOrderStatusConst.Draft);
        if (!IsDraftOrder)
        {
            AddCreateFields(PurchaseOrderHeader);
        }
        else
        {
            AddUpdateFields(PurchaseOrderHeader);
        }
        PurchaseOrderHeader.OrgUID = SelectedStoreMaster!.Store.UID;
        PurchaseOrderHeader.DivisionUID = null;

        purchaseOrderMaster.PurchaseOrderHeader = PurchaseOrderHeader;
        purchaseOrderMaster.PurchaseOrderLines =
            PurchaseOrderItemViews.Where(e => e.IsCartItem).ToList<IPurchaseOrderLine>();
        purchaseOrderMaster.PurchaseOrderLines.ForEach(e =>
        {
            if (IsNewOrder && string.IsNullOrEmpty(e.UID))
            {
                AddCreateFields(e);
            }
            else
            {
                AddUpdateFields(e);
            }
            e.PurchaseOrderHeaderUID = PurchaseOrderHeader.UID;
        });
        purchaseOrderMaster.PurchaseOrderLineProvisions = PurchaseOrderItemViews.SelectMany(e => e.PurchaseOrderLineProvisions).ToList();
        purchaseOrderMaster.PurchaseOrderLineProvisions.ForEach(e =>
        {
            if (IsNewOrder && string.IsNullOrEmpty(e.UID)) AddCreateFields(e);
            else AddUpdateFields(e);
        });
        return purchaseOrderMaster;
    }

    private void GenerateOrderNumber(IPurchaseOrderHeader purchaseOrderHeader, string status, string? division = null)
    {
        if (status == PurchaseOrderStatusConst.Draft && string.IsNullOrEmpty(purchaseOrderHeader.DraftOrderNumber))
        {
            //CustomerCode/ddMMyy/HHMISS-DIVCode
            purchaseOrderHeader.DraftOrderNumber = SelectedStoreMaster.Store.Code + "/" + DateTime.Now.ToString("ddMMyy") + "/" + DateTime.Now.ToString("HHmmss");
        }
        else if (status == PurchaseOrderStatusConst.PendingForApproval &&
            string.IsNullOrEmpty(purchaseOrderHeader.OrderNumber))
        {
            if (string.IsNullOrEmpty(purchaseOrderHeader.DraftOrderNumber))
            {
                purchaseOrderHeader.OrderNumber = division + "-" + SelectedStoreMaster.Store.Code + "/" + DateTime.Now.ToString("ddMMyy") + "/" + DateTime.Now.ToString("HHmmss");
            }
            else
            {
                purchaseOrderHeader.OrderNumber = division + "-" + purchaseOrderHeader.DraftOrderNumber;
            }
        }
    }

    private async Task SetStoreCreditLimit(string storeUid, string divisionUID)
    {
        StoreCreditLimit = await _purchaseOrderDataHelper.GetCurrentLimitByStoreAndDivision(storeUid, divisionUID);
    }

    #region [Sell-In Scheme]
    private void RemoveAppliedScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        RemoveSellInScheme(purchaseOrderItemView);
        RemoveQPSScheme(purchaseOrderItemView);
        RemoveStandingScheme(purchaseOrderItemView);
        RemoveCashScheme(purchaseOrderItemView);
        purchaseOrderItemView.MarginUnitValue = 0;
    }
    private void RemoveSellInScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        //purchaseOrderItemView.LadderingDiscount = 0;
        //purchaseOrderItemView.LadderingPercentage = 0;
        purchaseOrderItemView.SellInDiscountUnitValue = 0;
        purchaseOrderItemView.SellInDiscountUnitPercentage = 0;
        purchaseOrderItemView.SellInDiscountTotalValue = 0;
        purchaseOrderItemView.SellInCnP1UnitPercentage = 0;
        purchaseOrderItemView.SellInCnP1UnitValue = 0;
        purchaseOrderItemView.SellInCnP1Value = 0;
        purchaseOrderItemView.SellInP2Amount = 0;
        purchaseOrderItemView.SellInP3Amount = 0;
        purchaseOrderItemView.SellInSchemeCode = string.Empty;
        purchaseOrderItemView.P3StandingAmount = 0;
        purchaseOrderItemView.AppliedPromotionUIDs?.Clear();
        purchaseOrderItemView.TotalDiscount = 0;
        purchaseOrderItemView.PromotionUID = default;
        //purchaseOrderItemView.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.SellInCnP1, ProvisionTypeConst.SellInP2, ProvisionTypeConst.SellInP3 }).Contains(p.ProvisionType));
    }
    private void RemoveQPSScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.QPSOfferType = default;
        purchaseOrderItemView.QPSOfferValue = default;
        purchaseOrderItemView.QPSUnitValue = default;
        purchaseOrderItemView.P3QPSTotalValue = default;
        //purchaseOrderLineProvisionQPSDiscount.ApprovedProvisionUnitAmount = 0;
        //purchaseOrderLineProvisionQPSDiscount.ActualProvisionUnitAmount = 0;
        purchaseOrderItemView.QPSSchemeCode = default;
        //purchaseOrderItemView.PurchaseOrderLineProvisions.RemoveAll(p => (new List<string>() { ProvisionTypeConst.P2QPS, ProvisionTypeConst.P3QPS }).Contains(p.ProvisionType));
    }
    private void RemoveStandingScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.StandingUnitValue = 0;
        purchaseOrderItemView.P3StandingAmount = 0;
        purchaseOrderItemView.StandingSchemeData = string.Empty;
        //purchaseOrderItemView.PurchaseOrderLineProvisions.RemoveAll(p => ProvisionTypeConst.StandingScheme == (p.ProvisionType));
    }
    private void RemoveCashScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        purchaseOrderItemView.CashDiscountValue = 0;
        purchaseOrderItemView.CashDiscountPercentage = 0;
        //purchaseOrderItemView.PurchaseOrderLineProvisions.RemoveAll(p => (p.ProvisionType) == ProvisionTypeConst.CashDiscount);
    }



    public void ApplyCashDiscount(IPurchaseOrderItemView purchaseOrderItemView)
    {

        if (purchaseOrderItemView.FinalQty == 0 || IsCashDiscountExcluded)
        {
            return;
        }
        var purchaseOrderLineProvisionCashDiscount = purchaseOrderItemView.PurchaseOrderLineProvisions.Find(e => e.ProvisionType == ProvisionTypeConst.CashDiscount);
        if (purchaseOrderLineProvisionCashDiscount == null)
        {
            purchaseOrderLineProvisionCashDiscount = new PurchaseOrderLineProvision();
            purchaseOrderItemView.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvisionCashDiscount);
        }
        if (purchaseOrderLineProvisionCashDiscount.IsSelected)
        {
            purchaseOrderItemView.CashDiscountValue = CommonFunctions.RoundForSystem(purchaseOrderItemView.EffectiveUnitPrice * purchaseOrderItemView.FinalQty
            * _appSetting.CashDiscountPercentage * 0.01m, _appSetting.RoundOffDecimal);
            purchaseOrderLineProvisionCashDiscount.ProvisionType ??= ProvisionTypeConst.CashDiscount;
            purchaseOrderLineProvisionCashDiscount.ActualProvisionUnitAmount = CommonFunctions.RoundForSystem(purchaseOrderItemView.FinalQty == 0 ? 0 : purchaseOrderItemView.CashDiscountValue / purchaseOrderItemView.FinalQty, _appSetting.RoundOffDecimal);
            purchaseOrderLineProvisionCashDiscount.ApprovedProvisionUnitAmount = purchaseOrderLineProvisionCashDiscount.ActualProvisionUnitAmount;

            purchaseOrderLineProvisionCashDiscount.PurchaseOrderLineUID = purchaseOrderItemView.UID;
            purchaseOrderLineProvisionCashDiscount.SchemeCode = purchaseOrderItemView.PromotionUID;
            purchaseOrderItemView.CashDiscountPercentage = _appSetting.CashDiscountPercentage;
        }
    }

    public virtual void ApplySellInScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.FinalQty == 0)
        {
            return;
        }

        ISellInSchemePO? sellInSchemePo = SellInSchemes.Where(x => x.SKUUID == purchaseOrderItemView.SKUUID)
            .FirstOrDefault();
        if (sellInSchemePo != null)
        {
            purchaseOrderItemView.SellInSchemeCode = sellInSchemePo.SchemeCode;

            //invoice discount
            if (sellInSchemePo.InvoiceDiscountType == "Value")
            {
                purchaseOrderItemView.SellInDiscountUnitValue = CommonFunctions.RoundForSystem(sellInSchemePo.InvoiceDiscount);

                purchaseOrderItemView.SellInDiscountTotalValue =
                CommonFunctions.RoundForSystem(purchaseOrderItemView.SellInDiscountUnitValue * purchaseOrderItemView.FinalQty,
                _appSetting.RoundOffDecimal);
            }
            else
            {
                purchaseOrderItemView.SellInDiscountUnitPercentage = sellInSchemePo.InvoiceDiscount;

                var sellInDiscountUnitValue = CommonFunctions.RoundForSystem(
            sellInSchemePo.InvoiceDiscount * purchaseOrderItemView.UnitPrice / 100, _appSetting.RoundOffDecimal);

                purchaseOrderItemView.SellInDiscountTotalValue =
               CommonFunctions.RoundForSystem(sellInDiscountUnitValue * purchaseOrderItemView.FinalQty,
               _appSetting.RoundOffDecimal);
            }
            //purchaseOrderItemView.SellInDiscountUnitValue = CommonFunctions.RoundForSystem(
            //sellInSchemePo.InvoiceDiscountType == "Value"
            //    ? sellInSchemePo.InvoiceDiscount
            //    : sellInSchemePo.InvoiceDiscount * purchaseOrderItemView.UnitPrice / 100, _appSetting.RoundOffDecimal);

            //purchaseOrderItemView.SellInDiscountUnitPercentage = CommonFunctions.RoundForSystem(
            //purchaseOrderItemView.SellInDiscountUnitValue * 100M / purchaseOrderItemView.UnitPrice,
            //_appSetting.RoundOffDecimal);

            //purchaseOrderItemView.SellInDiscountTotalValue =
            //    CommonFunctions.RoundForSystem(purchaseOrderItemView.SellInDiscountUnitValue * purchaseOrderItemView.FinalQty,
            //    _appSetting.RoundOffDecimal);
            purchaseOrderItemView.SellInCnP1UnitPercentage = 0;
            IPurchaseOrderLineProvision purchaseOrderLineProvisionSellInCnP1 = purchaseOrderItemView.PurchaseOrderLineProvisions.Find(e => e.ProvisionType == ProvisionTypeConst.SellInCnP1);
            if (purchaseOrderLineProvisionSellInCnP1 == null)
            {
                purchaseOrderLineProvisionSellInCnP1 = new PurchaseOrderLineProvision();
                purchaseOrderLineProvisionSellInCnP1.IsSelected = true;
                purchaseOrderItemView.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvisionSellInCnP1);
            }
            if (purchaseOrderLineProvisionSellInCnP1.IsSelected)
            {

                if (sellInSchemePo.CreditNoteDiscountType == "Value")
                {
                    purchaseOrderItemView.SellInCnP1UnitValue = sellInSchemePo.CnP1;
                }
                else
                {
                    purchaseOrderItemView.SellInCnP1UnitPercentage = sellInSchemePo.CnP1;
                }
                decimal creditnoteDiscount = CommonFunctions.RoundForSystem(sellInSchemePo.CreditNoteDiscountType == "Value"
                    ? sellInSchemePo.CnP1
                    : sellInSchemePo.CnP1 * purchaseOrderItemView.UnitPrice / 100
                , _appSetting.RoundOffDecimal);

                purchaseOrderLineProvisionSellInCnP1.ProvisionType ??= ProvisionTypeConst.SellInCnP1;
                purchaseOrderLineProvisionSellInCnP1.ActualProvisionUnitAmount = creditnoteDiscount;
                purchaseOrderLineProvisionSellInCnP1.ApprovedProvisionUnitAmount = creditnoteDiscount;
                purchaseOrderLineProvisionSellInCnP1.PurchaseOrderLineUID = purchaseOrderItemView.UID;
                purchaseOrderLineProvisionSellInCnP1.SchemeCode = sellInSchemePo.SchemeCode;

                purchaseOrderItemView.SellInCnP1Value = CommonFunctions.RoundForSystem(creditnoteDiscount * purchaseOrderItemView.FinalQty
                , _appSetting.RoundOffDecimal);
            }
            IPurchaseOrderLineProvision purchaseOrderLineProvisionSellInP2 = purchaseOrderItemView.PurchaseOrderLineProvisions.Find(e => e.ProvisionType == ProvisionTypeConst.SellInP2);
            if (purchaseOrderLineProvisionSellInP2 == null)
            {
                purchaseOrderLineProvisionSellInP2 = new PurchaseOrderLineProvision();
                purchaseOrderItemView.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvisionSellInP2);
            }
            if (purchaseOrderLineProvisionSellInP2.IsSelected)
            {
                purchaseOrderLineProvisionSellInP2.ProvisionType ??= ProvisionTypeConst.SellInP2;
                purchaseOrderLineProvisionSellInP2.ActualProvisionUnitAmount = sellInSchemePo.CnP2;
                purchaseOrderLineProvisionSellInP2.ApprovedProvisionUnitAmount = sellInSchemePo.CnP2;
                purchaseOrderLineProvisionSellInP2.PurchaseOrderLineUID = purchaseOrderItemView.UID;
                purchaseOrderLineProvisionSellInP2.SchemeCode = sellInSchemePo.SchemeCode;
                purchaseOrderItemView.SellInP2Amount = CommonFunctions.RoundForSystem(
                sellInSchemePo.CnP2 * purchaseOrderItemView.FinalQty
                , _appSetting.RoundOffDecimal);
            }
            IPurchaseOrderLineProvision purchaseOrderLineProvisionSellInP3 = purchaseOrderItemView.PurchaseOrderLineProvisions.Find(e => e.ProvisionType == ProvisionTypeConst.SellInP3);
            if (purchaseOrderLineProvisionSellInP3 == null)
            {
                purchaseOrderLineProvisionSellInP3 = new PurchaseOrderLineProvision();
                purchaseOrderItemView.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvisionSellInP3);
            }
            if (purchaseOrderLineProvisionSellInP3.IsSelected)
            {
                purchaseOrderLineProvisionSellInP3.ProvisionType ??= ProvisionTypeConst.SellInP3;
                purchaseOrderLineProvisionSellInP3.ActualProvisionUnitAmount = sellInSchemePo.CnP3;
                purchaseOrderLineProvisionSellInP3.ApprovedProvisionUnitAmount = sellInSchemePo.CnP3;
                purchaseOrderLineProvisionSellInP3.PurchaseOrderLineUID = purchaseOrderItemView.UID;
                purchaseOrderLineProvisionSellInP3.SchemeCode = sellInSchemePo.SchemeCode;
                purchaseOrderItemView.SellInP3Amount = CommonFunctions.RoundForSystem(
                sellInSchemePo.CnP3 * purchaseOrderItemView.FinalQty
                , _appSetting.RoundOffDecimal);
            }
            // purchaseOrderItemView.P3StandingAmount =
            //     applicableSellInSchemes.StandingProvisionAmount * purchaseOrderItemView.FinalQty;
            purchaseOrderItemView.AppliedPromotionUIDs = [];
            _ = purchaseOrderItemView.AppliedPromotionUIDs.Add(sellInSchemePo.SchemeUID!);

            purchaseOrderItemView.TotalDiscount = purchaseOrderItemView.SellInDiscountTotalValue;//+ purchaseOrderItemView.CashDiscountValue;
            purchaseOrderItemView.LineDiscount = purchaseOrderItemView.SellInDiscountUnitValue;
            purchaseOrderItemView.PromotionUID = sellInSchemePo.SchemeUID;
        }
    }

    private void ApplyStandingScheme(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.FinalQty == 0)
        {
            return;
        }
        List<IPurchaseOrderLineProvision> purchaseOrderLineProvisions = purchaseOrderItemView.PurchaseOrderLineProvisions.
            FindAll(e => e.ProvisionType == ProvisionTypeConst.StandingScheme);
        if (StandingSchemes.TryGetValue(purchaseOrderItemView.SKUUID, out var standingScheme))
        {
            standingScheme.Schemes.ForEach(e =>
            {
                IPurchaseOrderLineProvision scheme = purchaseOrderLineProvisions.Find(p => p.SchemeCode == e.SchemeCode);
                if (scheme != null)
                {
                    e.IsSelected = scheme.IsSelected;
                    if (e.IsSelected)
                    {
                        scheme.ActualProvisionUnitAmount = e.Amount;
                        scheme.ApprovedProvisionUnitAmount = e.Amount;
                    }

                }
                else
                {
                    IPurchaseOrderLineProvision purchaseOrderLineProvision = new PurchaseOrderLineProvision();
                    purchaseOrderLineProvision.SchemeCode = e.SchemeCode;
                    purchaseOrderLineProvision.ActualProvisionUnitAmount = e.Amount;
                    purchaseOrderLineProvision.ApprovedProvisionUnitAmount = e.Amount;
                    purchaseOrderLineProvision.PurchaseOrderLineUID = purchaseOrderItemView.UID;
                    purchaseOrderLineProvision.ProvisionType = ProvisionTypeConst.StandingScheme;
                    purchaseOrderItemView.PurchaseOrderLineProvisions.Add(purchaseOrderLineProvision);
                }
            });
            decimal totalAmount = standingScheme.Schemes.Where(i => i.IsSelected).Sum(e => e.Amount);
            purchaseOrderItemView.StandingUnitValue = totalAmount;
            purchaseOrderItemView.P3StandingAmount = CommonFunctions.RoundForSystem(totalAmount * purchaseOrderItemView.FinalQty, _appSetting.RoundOffDecimal);
            purchaseOrderItemView.StandingSchemeData = JsonConvert.SerializeObject(standingScheme.Schemes);
        }
    }
    public void ApplyQPSDiscount(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.FinalQty == 0)
        {
            return;
        }
        IQPSSchemePO qpsSchemePO = QPSSchemes.Find(e => e.SKU_UID == purchaseOrderItemView.SKUUID);


        if (qpsSchemePO == null) return;


        IPurchaseOrderLineProvision purchaseOrderLineProvisionQPSDiscount = purchaseOrderItemView.PurchaseOrderLineProvisions.Find(e => e.ProvisionType == ProvisionTypeConst.P3QPS);
        if (purchaseOrderLineProvisionQPSDiscount != null)
        {
            if (purchaseOrderLineProvisionQPSDiscount.IsSelected)
            {
                purchaseOrderItemView.QPSOfferType = qpsSchemePO.Offer_Type;
                purchaseOrderItemView.QPSOfferValue = qpsSchemePO.Offer_Value;
                purchaseOrderItemView.QPSUnitValue = CommonFunctions.RoundForSystem(qpsSchemePO.Offer_Type == "Percent" ? purchaseOrderItemView.UnitPrice * qpsSchemePO.Offer_Value * 0.01m : qpsSchemePO.Offer_Value, _appSetting.RoundOffDecimal);
                purchaseOrderItemView.P3QPSTotalValue = CommonFunctions.RoundForSystem(purchaseOrderItemView.QPSUnitValue * purchaseOrderItemView.FinalQty, _appSetting.RoundOffDecimal);
                purchaseOrderItemView.QPSSchemeCode = qpsSchemePO.Scheme_Code;
            }
            // else
            // {
            //     purchaseOrderItemView.QPSOfferType = default;
            //     purchaseOrderItemView.QPSOfferValue = default;
            //     purchaseOrderItemView.QPSUnitValue = default;
            //     purchaseOrderItemView.P3QPSTotalValue = default;
            //     purchaseOrderLineProvisionQPSDiscount.ApprovedProvisionUnitAmount = 0;
            //     purchaseOrderLineProvisionQPSDiscount.ActualProvisionUnitAmount = 0;
            //     purchaseOrderItemView.QPSSchemeCode = default;
            // }
        }
        else
        {
            IPurchaseOrderLineProvision newQPSPurchaseOrderLineProvision = new PurchaseOrderLineProvision();
            purchaseOrderItemView.QPSOfferType = qpsSchemePO.Offer_Type;
            purchaseOrderItemView.QPSOfferValue = qpsSchemePO.Offer_Value;
            purchaseOrderItemView.QPSUnitValue = CommonFunctions.RoundForSystem(qpsSchemePO.Offer_Type == "Percent" ? purchaseOrderItemView.UnitPrice * qpsSchemePO.Offer_Value * 0.01m : qpsSchemePO.Offer_Value, _appSetting.RoundOffDecimal);
            purchaseOrderItemView.P3QPSTotalValue = CommonFunctions.RoundForSystem(purchaseOrderItemView.QPSUnitValue * purchaseOrderItemView.FinalQty, _appSetting.RoundOffDecimal);
            purchaseOrderItemView.QPSSchemeCode = qpsSchemePO.Scheme_Code;
            newQPSPurchaseOrderLineProvision.ApprovedProvisionUnitAmount = purchaseOrderItemView.QPSUnitValue;
            newQPSPurchaseOrderLineProvision.ActualProvisionUnitAmount = purchaseOrderItemView.QPSUnitValue;
            newQPSPurchaseOrderLineProvision.SchemeCode = qpsSchemePO.Scheme_Code;
            newQPSPurchaseOrderLineProvision.PurchaseOrderLineUID = purchaseOrderItemView.UID;
            newQPSPurchaseOrderLineProvision.ProvisionType = ProvisionTypeConst.P3QPS;
            purchaseOrderItemView.PurchaseOrderLineProvisions.Add(newQPSPurchaseOrderLineProvision);
        }
    }

    public async Task<bool> SaveAllApprovalRequestDetails(string requestId)
    {
        return await _purchaseOrderDataHelper.SaveApprovalRequestDetails(requestId, PurchaseOrderUID);
    }

    public async Task<List<IAllApprovalRequest>> GetAllApproveListDetails(string uid)
    {
        return await _purchaseOrderDataHelper.GetAllApproveListDetailsFromAPIAsync(uid);
    }

    public async Task InsertDataInIntegrationDB()
    {
        try
        {
            IPendingDataRequest pendingDataRequest = new PendingDataRequest
            {
                LinkedItemUid = PurchaseOrderHeader.UID,
                Status = "Pending",
                LinkedItemType = "PurchaseOrder",
            };
            _ = await _purchaseOrderDataHelper.InsertDataInIntegrationDB(pendingDataRequest);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<List<IUserHierarchy>?> GetAllUserHierarchyFromAPIAsync(string hierarchyType, string hierarchyUID,
        int ruleId)
    {
        return _purchaseOrderDataHelper.GetAllUserHierarchyFromAPIAsync(hierarchyType, hierarchyUID, ruleId);
    }
    #endregion

    private ApprovalRequestItem? PrepareApprovalRequestItem()
    {
        ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
        approvalRequestItem.HierarchyType = UserHierarchyTypeConst.Emp;
        approvalRequestItem.HierarchyUid = PurchaseOrderHeader.ReportingEmpUID;
        approvalRequestItem.RuleId = RuleId;
        approvalRequestItem.RequestId = RequestId.ToString();
        approvalRequestItem.Payload = new Dictionary<string, object>
        {
            {
                "RequesterId", _appUser.Emp.Code
            },
            {
                "Remarks", "Need approval"
            },
            {
                "UserRoleCode", _appUser.Role.Code
            },
            {
                "Customer", new Winit.Modules.ApprovalEngine.Model.Classes.Customer
                {
                    CreatedBy = ApprovalCreatedBy,
                }
            }
        };
        return approvalRequestItem;
    }

    public async Task<ApprovalRequestItem> GenerateApprovalRequestIdAndRuleId(IPurchaseOrderMaster purchaseOrderMaster)
    {
        bool isNegative = CheckIsNegative(purchaseOrderMaster.PurchaseOrderLines);
        bool isEdited = IsOrderEdited(PurchaseOrderItemViews.Cast<IPurchaseOrderLine>().ToList());
        IsPoEdited = isEdited;
        if (purchaseOrderMaster.PurchaseOrderHeader.CreatedBy == _appUser.Emp.UID || isEdited)
        {
            IsPoCreatedByCP = true;
            if (isNegative)
            {
                //RuleId = 27;
                RuleId = GetRuleIdByName(ApprovalParameterConstants.RuleNameForCPCreatedNegativeMarginPO);
                ApprovalCreatedBy = ApprovalParameterConstants.CPCreatedNegativeMarginPO;
            }
            else
            {
                //RuleId = 10;
                RuleId = GetRuleIdByName(ApprovalParameterConstants.RuleNameForPOCreatedByChannelPartner);
                ApprovalCreatedBy = ApprovalParameterConstants.POCreatedByChannelPartner;
            }
        }
        else
        {
            if (isNegative)
            {
                //RuleId = 26;
                RuleId = GetRuleIdByName(ApprovalParameterConstants.RuleNameForASMCreatedNegativeMarginPO);
                ApprovalCreatedBy = ApprovalParameterConstants.ASMCreatedNegativeMarginPO;
            }
            else
            {
                //RuleId = 11;
                RuleId = GetRuleIdByName(ApprovalParameterConstants.RuleNameForPOCreatedByASM);
                ApprovalCreatedBy = ApprovalParameterConstants.POCreatedByASM;
            }
        }
        ApprovalRequestItem approvalRequestItem = new ApprovalRequestItem();
        approvalRequestItem.HierarchyType = UserHierarchyTypeConst.Emp;
        approvalRequestItem.HierarchyUid = purchaseOrderMaster.PurchaseOrderHeader.ReportingEmpUID;
        approvalRequestItem.RuleId = RuleId;
        approvalRequestItem.RequestId = RequestId.ToString();
        approvalRequestItem.Payload = new Dictionary<string, object>
        {
            {
                "RequesterId", _appUser.Emp.Code
            },
            {
                "Remarks", "Need approval"
            },
            {
                "UserRoleCode", _appUser.Role.Code
            },
            {
                "Customer", new Winit.Modules.ApprovalEngine.Model.Classes.Customer
                {
                    CreatedBy = ApprovalCreatedBy,
                }
            }
        };
        return approvalRequestItem;
    }
    public int GetRuleIdByName(string ruleName)
    {
        try
        {
            return _appUser.ApprovalRuleMaster.Where(item => item.RuleName == ruleName).Select(item => item.RuleId).FirstOrDefault();
        }
        catch (Exception ex)
        {
            return default;
        }
    }
    private bool CheckIsNegative(List<IPurchaseOrderLine> purchaseOrderLines)
    {
        return purchaseOrderLines.Exists(e => e.MarginUnitValue < 0);
    }
    public async Task CreateApproval()
    {
        if (await _purchaseOrderDataHelper.CreateApproval(PurchaseOrderHeader.UID, PrepareApprovalRequestItem()))
        {
            PurchaseOrderHeader.IsApprovalCreated = true;
            throw new CustomException(ExceptionStatus.Success, "Approval created successfully");
        }
        throw new CustomException(ExceptionStatus.Failed, "Failed to create approval");
    }

    public async Task ValidateCreditLimit()
    {
        if (StoreCreditLimit is null || StoreCreditLimit.CreditLimit == 0)
        {
            //throw new CustomException(ExceptionStatus.Success, "Cash Customer Validated Successfully");
            return;
        }
        else if (PurchaseOrderHeader.NetAmount > (StoreCreditLimit.TemporaryCreditLimit + StoreCreditLimit.CreditLimit + (StoreCreditLimit.CreditLimit * CrediLimitBufferPercentage * 0.01m) - StoreCreditLimit.BlockedLimit))
        {
            throw new CustomException(ExceptionStatus.Failed,
            """
            <p class="cls_alert_p">
                This order exceeds the available limit and cannot be submitted. Please clear your outstanding balance or adjust the order quantity to stay within your approved credit limit.
            If a temporary increase in the credit limit is required, please communicate with CMI management, providing justification for the request.
            Only then can you submit your order.
            </p>
            """);
        }
        else if ((StoreCreditLimit.DueDate != null && StoreCreditLimit.DueDate < DateTime.Now && StoreCreditLimit.TemporaryCreditDays == 0)
            || (StoreCreditLimit.TemporaryCreditApprovalDate != null &&
            StoreCreditLimit.TemporaryCreditDays > 0 &&
            StoreCreditLimit.TemporaryCreditApprovalDate.Value.AddDays(StoreCreditLimit.TemporaryCreditDays) < DateTime.Now))
        {
            throw new CustomException(ExceptionStatus.Failed,
            """
            <label class="cls_alert_label2">Due to exceeding the Aging Days, this order is currently "on
            hold."</label>
            <p class="cls_alert_p">
            Due to exceeding the Aging Days, this order is currently "On Hold".
            Please speak with the distributor to clear the outstanding balance. If a temporary increase in the credit days is required, please communicate with CMI management, providing justification for the request.
            </p>
            """);
        }
        else
        {
            throw new CustomException(ExceptionStatus.Success, "Credit limit has been valid");
        }
    }

    private void ApplyMargin(IPurchaseOrderItemView purchaseOrderItemView)
    {
        if (purchaseOrderItemView.FinalQty == 0)
        {
            return;
        }
        decimal p2QPSUnitValue = CommonFunctions.RoundForSystem((Math.Max(purchaseOrderItemView.P2QPSTotalValue ?? 0, 0) / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);
        decimal sellInP2UnitAmount = CommonFunctions.RoundForSystem((purchaseOrderItemView.SellInP2Amount / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);
        decimal sellInP3UnitAmount = CommonFunctions.RoundForSystem((purchaseOrderItemView.SellInP3Amount / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);
        decimal p3QPSUnitValue = CommonFunctions.RoundForSystem((Math.Max(purchaseOrderItemView.P3QPSTotalValue ?? 0, 0) / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);
        decimal cashDiscountUnitValue = CommonFunctions.RoundForSystem((purchaseOrderItemView.CashDiscountValue / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);
        decimal p3StandingUnitValue = CommonFunctions.RoundForSystem((purchaseOrderItemView.P3StandingAmount / purchaseOrderItemView.FinalQty), _appSetting.RoundOffDecimal);

        decimal billingPriceAfterProvision = purchaseOrderItemView.EffectiveUnitPrice - (purchaseOrderItemView.SellInCnP1UnitValue + sellInP2UnitAmount +
            sellInP3UnitAmount + p2QPSUnitValue + p3QPSUnitValue + cashDiscountUnitValue + p3StandingUnitValue);

        purchaseOrderItemView.MarginUnitValue = billingPriceAfterProvision - purchaseOrderItemView.MinSellingPrice;
    }
    public async Task<List<string>> CreateOrderNumbers()
    {
        try
        {
            if (CreatedPurchaseOrderNumbers.Count > 0)
            {
                return CreatedPurchaseOrderNumbers.ToList();
            }
            else
            {
                return new List<string> { PurchaseOrderHeader.DraftOrderNumber };
            }
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }

    public async Task SendEmail(List<string> smsTemplates)
    {
        try
        {
            List<string> UIDs = (PurchaseOrderUIDs.Count == 0) ? PurchaseOrderUIDsForNotification : PurchaseOrderUIDs;
            await _purchaseOrderDataHelper.InsertEmailIntoRabbitMQ(smsTemplates, UIDs);
        }
        catch (Exception ex)
        {
            // Log Exception
        }
    }
    public async Task SendSms(List<string> smsTemplates)
    {
        try
        {
            List<string> UIDs = (PurchaseOrderUIDs.Count == 0) ? PurchaseOrderUIDsForNotification : PurchaseOrderUIDs;
            await _purchaseOrderDataHelper.InsertSmsIntoRabbitMQ(smsTemplates, UIDs);
        }
        catch (Exception ex)
        {
            // Log Exception
        }
    }
}
