using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.UIInterfaces;
using Winit.Modules.SalesOrder.Model.UIInterfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;
namespace Winit.Modules.SalesOrder.BL.UIClasses;

public class SalesOrderAppViewModel : SalesOrderBaseViewModel, ISalesOrderAppViewModel
{
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    #region filter fields
    public string LeftAttributeCode { get; set; } = "Category";
    public string TopAttributeCode { get; set; } = "Sub Category";
    public bool IsLeftParentOfTop { get; set; }
    List<SKUGroupSelectionItem> SKUGroupTypeSelectionItems { get; set; }
    private List<string> FilterGroupCodes = new List<string>();
    public FilterCriteria LeftScrollFilterCriteria { get; set; }
    public List<FilterCriteria> TopScrollFilterCriterias { get; set; }
    Dictionary<string, List<SKUGroupSelectionItem>> SKUGroupSelectionItemsDict { get; set; }
    #endregion

    public SalesOrderAppViewModel(IServiceProvider serviceProvider,
        IFilterHelper filter,
        ISortHelper sorter,
        UIInterfaces.ISalesOrderAmountCalculator amountCalculator,
        IListHelper listHelper,
        IAppUser appUser,
        IAppSetting appSetting,
        IOrderLevelCalculator orderLevelCalculator,
        ICashDiscountCalculator cashDiscountCalculator, IAppConfig appConfig,
        IStringLocalizer<LanguageKeys> Localizer, ILanguageService languageService,
        IDataManager dataManager,
        ISalesOrderDataHelper salesOrderDataService) :
        base(serviceProvider, filter, sorter, amountCalculator, listHelper, appUser, appSetting,
             dataManager, orderLevelCalculator, cashDiscountCalculator, appConfig, salesOrderDataService)
    {
        IsPriceEditable = true;
        FilterDataList = new Dictionary<ISelectionItem, List<ISelectionItem>>();
        SKUGroupTypeSelectionItems = new List<SKUGroupSelectionItem>();
        SKUGroupSelectionItemsDict = new Dictionary<string, List<SKUGroupSelectionItem>>();
        TopScrollFilterCriterias = new List<FilterCriteria>();
        _localizer = Localizer;
        _languageService = languageService;
    }
    public async override Task PopulateViewModel(string source, Winit.Modules.Store.Model.Interfaces.IStoreItemView? storeViewModel, bool isNewOrder = true,
    string salesOrderUID = "")
    {
        LoadResources(null, _languageService.SelectedCulture);
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
        SetSystemSettingValues();
        PopulateSalesOrderSetting();
        await PopulateSKUClass();
        await PopulateStoreCheckData();
        await PopulateWHQty();
        await PopulateSKUMaster();
        var priceMaster = PopulatePriceMaster();
        SetPriceMaster(await priceMaster);
        await PopulatePromotion();
        PopulateFooter();
        await ApplyFilter(new List<Shared.Models.Enums.FilterCriteria>(), FilterMode.And);
        List<SortCriteria> sortCriteriaList = new List<SortCriteria>();
        sortCriteriaList.Add(new SortCriteria("SKUName", SortDirection.Asc));
        await ApplySort(sortCriteriaList);
        await PopulateSalesOrder();
        PrepareSignatureFields();
        SelectedStoreViewModel.IsCaptureSignatureRequired = true;
        await GetDistributorsList("FR");
    }


    protected async Task<IEnumerable<ISKUPrice>> PopulatePriceMaster()
    {
        PagedResponse<ISKUPrice> pagedResponse = await _salesOrderDataHelper.PopulatePriceMaster();
        if (pagedResponse != null && pagedResponse.PagedData != null && pagedResponse.PagedData.Count() > 0)
        {
            return pagedResponse.PagedData;
        }
        return [];
    }

    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
        _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    #region Logic
    public void PopulateFilterData()
    {
        FilterDataList.Add(
                new Winit.Shared.Models.Common.SelectionItemFilter
                {
                    Label = @_localizer["sort_by"],
                    Code = "Sort-By",
                    ActionType = FilterActionType.Sort,
                    Mode = Winit.Shared.Models.Enums.SelectionMode.Single,
                    ImgPath = ""
                },
                new List<Winit.Shared.Models.Common.ISelectionItem>
                {
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {
                        UID="NAME_ASC",  Label = @_localizer["name_ascending"],
                        Code = "SKULabel", Direction = SortDirection.Asc,
                        IsSelected = true, DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {UID="NAME_DESC",  Label = @_localizer["name_descending"], Code = "SKULabel",
                        Direction = SortDirection.Desc, DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter {UID="PRICE_ASC",
                        Label=@_localizer["price_ascending"],Code="UnitPrice", Direction = SortDirection.Asc,
                        DataType = typeof(string) },
                    new Winit.Shared.Models.Common.SelectionItemFilter
                    {UID="PRICE_DESC",  Label = @_localizer["price_descending"], Code = "UnitPrice",
                        Direction = SortDirection.Desc, DataType = typeof(string) },
                });
        if (DMSPromotionDictionary != null && DMSPromotionDictionary.Any())
        {
            var promos = DMSPromotionDictionary.Values.Select(e => new SelectionItemFilter
            {
                UID = e.UID,
                Code = e.Code,
                Label = e.Remarks,
                DataType = typeof(string),
                FilterGroup = FilterGroupType.Attribute
            }).ToList<ISelectionItem>();
            ISelectionItem selectionItem = new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label = @_localizer["promo"],
                Code = "Promo",
                ActionType = FilterActionType.Filter,
                Mode = Winit.Shared.Models.Enums.SelectionMode.Multiple,
                ImgPath = ""
            };
            FilterDataList.Add(selectionItem, promos);
        }
        var skuAttributesData = (List<SKUGroupSelectionItem>)_dataManager.GetData("skuAttributes");
        if (skuAttributesData != null) SKUGroupTypeSelectionItems.AddRange(skuAttributesData);
        SKUGroupSelectionItemsDict =
            (Dictionary<string, List<SKUGroupSelectionItem>>)_dataManager.GetData("skuTypes");
        if (SKUGroupTypeSelectionItems != null && SKUGroupTypeSelectionItems.Any() &&
            SKUGroupSelectionItemsDict != null && SKUGroupSelectionItemsDict.Any())
        {
            foreach (var filterLeft in SKUGroupTypeSelectionItems)
            {
                FilterGroupCodes.Add(filterLeft.Code);
                if (filterLeft.Code != null && SKUGroupSelectionItemsDict.ContainsKey(filterLeft.Code))
                {
                    FilterDataList.Add(ConvertToSKUGroupSelectionToSelectionItemFilter(filterLeft).First(),
                        ConvertToSKUGroupSelectionToSelectionItemFilter(SKUGroupSelectionItemsDict[filterLeft.Code].ToArray()));
                }
            }
        }
    }
    private List<ISelectionItem> ConvertToSKUGroupSelectionToSelectionItemFilter(params SKUGroupSelectionItem[] selectionItems)
    {
        return selectionItems.Select(e => new SelectionItemFilter()
        {
            UID = e.UID,
            Code = e.Code,
            Label = e.Label,
            ExtData = e.ExtData,
            ImgPath = e.ExtData?.ToString(),
            DataType = typeof(List<ISKUAttributes>),
            ActionType = FilterActionType.Filter,
            Mode = SelectionMode.Multiple,
            FilterGroup = FilterGroupType.Attribute
        }
        ).ToList<ISelectionItem>();
    }

    public override async Task ApplyFilter(List<Shared.Models.Enums.FilterCriteria> filterCriterias, Shared.Models.Enums.FilterMode filterMode)
    {
        // Clear existing filtered and displayed items
        FilteredSalesOrderItemViews.Clear();
        DisplayedSalesOrderItemViews.Clear();

        // Add SelectedPromotionUID to filter criteria if it exists
        if (!string.IsNullOrEmpty(SelectedPromotionUID))
        {
            filterCriterias.Add(new FilterCriteria("ApplicablePromotionUIDs", SelectedPromotionUID, FilterType.In, typeof(HashSet<string>)));
        }

        // Add LeftScrollFilterCriteria if it exists
        if (LeftScrollFilterCriteria != null)
        {
            filterCriterias.Add(LeftScrollFilterCriteria);
        }

        filterCriterias = filterCriterias.Union(TopScrollFilterCriterias).ToList();
        filterCriterias = filterCriterias.Union(FilterCriteriaList).ToList();
        // Apply field-based filters
        var fieldFilterCriteria = filterCriterias.Where(e => e.FilterGroup == FilterGroupType.Field).ToList();
        var filteredItems = await _filter.ApplyFilter<ISalesOrderItemView>(SalesOrderItemViews, fieldFilterCriteria, filterMode);

        // Group attribute filters by Name and get a list of values
        var attributeFilters = filterCriterias
            .Where(e => e.FilterGroup == FilterGroupType.Attribute)
            .GroupBy(e => e.Name)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Value.ToString()).ToList());

        // Apply attribute-based filters
        foreach (var attribute in attributeFilters)
        {
            var attributeKeys = new HashSet<string>(attribute.Value);
            filteredItems = filteredItems.Where(item => item.FilterKeys.Any(key => attributeKeys.Contains(key))).ToList();
        }

        // Update FilteredSalesOrderItemViews and DisplayedSalesOrderItemViews
        FilteredSalesOrderItemViews.AddRange(filteredItems);
        DisplayedSalesOrderItemViews.AddRange(FilteredSalesOrderItemViews);
    }
    #endregion


    public override async Task<bool> SaveSalesOrder(string StatusType = SalesOrderStatus.DRAFT)
    {
        int retValue = await _salesOrderDataHelper.CreateFileSysForBulk(SignatureFileSysList);
        if (retValue < 0) throw new Exception(@_localizer["failed_to_save_the_signatures"]);
        return await base.SaveSalesOrder(StatusType);
    }


    public List<SelectionItemFilter> GetLeftScrollSelectionItems()
    {
        List<SelectionItemFilter> selectionItemFilters = new List<SelectionItemFilter>
        {
            new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label = @_localizer["all"],
                Code = "All",
                ImgPath = "/Images/ic_sales_1.png"
            },
            new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label =@_localizer["recommended"] ,
                Code = "Recommended",
                ImgPath = "/Images/ic_sales_2.png"
            },
            new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label =@_localizer["top_selling"] ,
                Code = "TopSelling",
                ImgPath = "/Images/ic_sales_3.png"
            },
            new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label =@_localizer["top_trending"] ,
                Code = "TopTrending",
                ImgPath = "/Images/ic_sales_4.png"
            },
        };
        if (SKUGroupSelectionItemsDict != null && SKUGroupSelectionItemsDict.ContainsKey(LeftAttributeCode))
        {
            selectionItemFilters.AddRange(ConvertToSKUGroupSelectionToSelectionItemFilter
                (SKUGroupSelectionItemsDict[LeftAttributeCode].ToArray()).OfType<SelectionItemFilter>());
        }
        return selectionItemFilters;
    }

    public List<SelectionItemFilter> GetTopScrollSelectionItems(string? leftScrollItemCode = null)
    {
        List<SelectionItemFilter> selectionItemFilters = new List<SelectionItemFilter>()
        {

            new Winit.Shared.Models.Common.SelectionItemFilter
            {
                Label = "Focus",
                Code = "Focus",
                ImgPath = "/Images/icon_star.png"
            },
           //new Winit.Shared.Models.Common.SelectionItemFilter
           // {
           //     Label = @_localizer["promo"],
           //     Code = "IsPromo",
           //     ImgPath = "/Images/icon_tag.png"
           // },
           // new Winit.Shared.Models.Common.SelectionItemFilter
           // {
           //     Label = @_localizer["new"],
           //     Code = "New",
           //     ImgPath = "/Images/icon_tag.png"
           // }
        };
        if (SKUGroupSelectionItemsDict != null && SKUGroupSelectionItemsDict.ContainsKey(LeftAttributeCode) &&
            SKUGroupSelectionItemsDict.ContainsKey(TopAttributeCode))
        {
            var topScrollItems = SKUGroupSelectionItemsDict[TopAttributeCode].ToArray();
            if (!string.IsNullOrEmpty(leftScrollItemCode))
                topScrollItems = SKUGroupSelectionItemsDict[TopAttributeCode].FindAll(e => e.ParentCode == leftScrollItemCode).ToArray();
            selectionItemFilters.AddRange(ConvertToSKUGroupSelectionToSelectionItemFilter
                (topScrollItems).OfType<SelectionItemFilter>());
        }
        return selectionItemFilters;
    }
    public async Task<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderPrintView> GetSalesOrderHeaderDetails()
    {
        return await _salesOrderDataHelper.GetSalesOrderHeaderDetails(SalesOrderUID);
    }
    public async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLinePrintView>> GetSalesOrderlines()
    {
        return
            await _salesOrderDataHelper.GetSalesOrderPrintViewslines(SalesOrderUID);
    }
    public async Task GetDistributorsList(string type)
    {
        DistributorsList.Clear();

        var distributors = await _salesOrderDataHelper.GetDistributorslistByType(type);
        DistributorsList.AddRange(CommonFunctions.ConvertToSelectionItems<Winit.Modules.Org.Model.Interfaces.IOrg>
            (distributors.ToList(), ["UID", "Code", "Name"]));
    }
}
