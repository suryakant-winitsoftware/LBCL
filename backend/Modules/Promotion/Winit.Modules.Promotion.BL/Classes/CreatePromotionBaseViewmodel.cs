
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.Model;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Promotion.BL.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Common;
using Winit.UIModels.Web.Promotions;


namespace Winit.Modules.Promotion.BL.Classes;

public class CreatePromotionBaseViewmodel : ICreatepromotionBaseViewModel
{
    public CreatePromotionBaseViewmodel(ApiService apiService, Winit.Shared.Models.Common.IAppConfig appConfigs,
        CommonFunctions commonFunctions, NavigationManager navigationManager,
        Winit.Modules.Common.Model.Interfaces.IDataManager dataManager, IAlertService alertService,
        IAppUser iAppUser, Winit.UIComponents.Common.Services.ILoadingService loadingService, IStringLocalizer<LanguageKeys> Localizer,
        ILanguageService languageService, IAddProductPopUpDataHelper addProductPopUpDataHelper)
    {
        _apiService = apiService;
        _appConfigs = appConfigs;
        _commonFunctions = commonFunctions;
        _navigationManager = navigationManager;
        _dataManager = dataManager;
        _alertService = alertService;
        _iAppUser = iAppUser;
        _loadingService = loadingService;
        _localizer = Localizer;
        _languageService = languageService;
        _addProductPopUpDataHelper = addProductPopUpDataHelper;
    }
    public IAddProductPopUpDataHelper _addProductPopUpDataHelper { get; }
    private readonly ILanguageService _languageService;
    private IStringLocalizer<LanguageKeys> _localizer;
    Winit.UIComponents.Common.Services.ILoadingService _loadingService;
    IAppUser _iAppUser { get; set; }
    public ApiService _apiService { get; set; }
    public List<SKUAttributeDropdownModel>? SKUAttributeData { get; set; }
    public Winit.Shared.Models.Common.IAppConfig _appConfigs { get; set; }
    public CommonFunctions _commonFunctions { get; set; }
    public NavigationManager _navigationManager { get; set; }
    public Common.Model.Interfaces.IDataManager _dataManager { get; set; }
    public IAlertService _alertService { get; set; }
    public string PromotionUID { get; set; }
    public bool IsAssorted { get; set; }
    public bool IsLine { get; set; }
    public bool IsInvoice { get; set; }
    public bool IsGroupTypeSKU { get; set; }

    public Winit.Modules.Promotion.Model.Classes.PromotionView PromotionView { get; set; }
    public bool IsLoad { get; set; } = false;
    public List<ListHeader.Model.Classes.ListItem> PROMOTION_TYPE { get; set; } = new();
    public List<ListHeader.Model.Classes.ListItem> PROMOTION_CATEGORY { get; set; } = new();
    public List<ListHeader.Model.Classes.ListItem> PROMO_INSTANT_FORMATS { get; set; } = new();
    public List<Winit.Modules.ListHeader.Model.Classes.ListItem> PromotionsDropDowns { get; set; }
    public string str1 { get; set; }
    public string DiscountLabel { get; set; } = "Discount";
    public bool IsDiscountShow { get; set; } = true;
    protected Winit.Modules.Promotion.Model.Classes.PromoMasterView PromoMasterView { get; set; } = new();
    public string SelectedGroupLabel { get; set; } = "Select Group";
    public string SelectedGroupTypeLabel { get; set; }
    public List<DataGridColumn> Columns { get; set; }

    public List<PromoOrderForSlabs> promoOrderForSlabs { get; set; } = new();

    public List<PromotionItemsModel> PromotionItemsModelList { get; set; } = new();

    public List<ISelectionItem> SKUGroupList { get; set; } = new();
    public List<ISelectionItem> SKUGroupTypeList { get; set; } = [];
    public List<ISelectionItem> SelectedItems { get; set; } = [];

    public List<IPromoOrderForSlabs> PromoOrderForSlabsList { get; set; } = new();
    Winit.Shared.Models.Common.ISelectionItem SelectedGroupType { get; set; }
    List<ISKUGroup> SKUGroup { get; set; } = new();
    public decimal EligibleOrBundleQty { get; set; }
    public int MaxDealCount { get; set; }
    public decimal Discount { get; set; }
    public bool IsGroupTypeVisible { get; set; }
    public bool IsGroupVisible { get; set; }
    string PromotionOrderUID { get; set; } = Guid.NewGuid().ToString();
    string PromoOfferUID { get; set; } = Guid.NewGuid().ToString();
    public bool IsNoEndDate { get; set; }
    public bool IsCategoryDisplay { get; set; }
    public string CategoryLabel { get; set; } = "Select Category";
    public List<ISelectionItem> Category { get; set; }
    public bool IsInstantFormatsDisplay { get; set; }
    public string InstantFormatsLabel { get; set; } = "Select  Format";
    public List<ISelectionItem> InstantFormats { get; set; }
    public bool IsOfferTypeDisplay { get; set; }
    public string OfferTypeLabel { get; set; } = "Select  Offer Type";
    public List<ISelectionItem> OfferTypeList { get; set; }
    public List<ISelectionItem> SlabOfferTypeList { get; set; }
    public List<ISelectionItem> DisplaySlabOfferTypeList { get; set; }
    public bool IsOrderTypeDisplay { get; set; }
    public bool IsSlabOrderTypeDisplay { get; set; }
    public string SlabOrderTypeLabel { get; set; } = "Select Order Type";
    public string SlabOrderTypeUID { get; set; }
    public string SlabSkuUID { get; set; }
    public string SlabSKULabel { get; set; } = "Please Select Product";
    public string OrderTypeLabel { get; set; } = "Select  Buy Criteria";
    public List<ISelectionItem> OrderTypeList { get; set; }
    public List<ISelectionItem> SlabOrderTypeList { get; set; }
    public bool IsTypeDisplay { get; set; }
    public string TypeLabel { get; set; }
    public List<ISelectionItem> Type { get; set; }
    public bool IsOfferFOCType { get; set; }
    public bool IsSlabTypePromotion { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKUList { get; set; } = [];
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SelectedSKU { get; set; } = [];
    public bool IsNewPromotion { get; set; }
    public bool ShowBundleQty { get; set; }
    public decimal BundleQty { get; set; }
    public string SelectionModel { get; set; } = "Any";
    public bool IsGroupTypeSKUType { get; set; }
    public bool IsFOCSKU { get; set; }
    public List<ISelectionItem> SelectionModelList { get; set; } = new() { new SelectionItem { Code = "All", Label = "All", UID = "All" }, new SelectionItem { Code = "Any", Label = "Any", UID = "Any" } };
    public string SelectionModelLabel { get; set; } = "Select Selection Model";
    public bool IsSelectionModelDisplay { get; set; }
    string PromotionType { get; set; }
    public string OfferTypeCode { get; set; }
    string OrderTypeCode { get; set; }
    public string SelectedOrderType { get; set; }
    string QualifiCationLevel
    {
        get
        {
            return IsInvoice ? Shared.Models.Constants.QualifiCationLevel.Invoice :
              Shared.Models.Constants.QualifiCationLevel.Line;
        }
    }
    string ApplicationLevel
    {
        get
        {
            return IsInvoice ? Shared.Models.Constants.ApplicationLevel.Invoice :
              Shared.Models.Constants.ApplicationLevel.Line;
        }
    }
    protected void LoadResources(object sender, string culture)
    {
        CultureInfo cultureInfo = new CultureInfo(culture);
        ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
        _localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
    }
    public void PopulateViewModel()
    {
        LoadResources(null, _languageService.SelectedCulture);

        IsNewPromotion = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
        if (IsNewPromotion)
        {
            PromotionUID = Guid.NewGuid().ToString();
            PromotionView = new()
            {
                UID = PromotionUID,
                ValidFrom = DateTime.Now.Date,
                ValidUpto = new DateTime(2099, 12, 31),
                SS = 0,
                PromoMessage = "Pending",
                Type = _commonFunctions.GetParameterValueFromURL("promo")
            };
            PromotionView.Type = _commonFunctions.GetParameterValueFromURL("promo");
            SetPromotionType();
        }
        else
        {
            PromotionUID = _commonFunctions.GetParameterValueFromURL("UID");
        }
    }
    public async Task PopulateViewmodelAsync()
    {
        _loadingService.ShowLoading();
        try
        {
            SKUAttributeData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        await GetSKUFromDatabase();
        if (!IsNewPromotion)
        {
            await GetPromotionDetailsBYUID(PromotionUID);
        }
        await GetListItemsByCodes_Promotions();
        await SetDropDownLists();
        _loadingService.HideLoading();
    }

    #region Drop Down Events
    protected async Task SetDropDownLists()
    {
        try
        {
            SKUGroupTypeList = new() { new SelectionItem() { UID = "sku", Code = "sku", Label = "SKU" } };
            bool isSkuGroupTypeAvailable = DatamanagerGeneric<List<ISKUGroupType>>.TryGet(CommonMasterDataConstants.SKUGroupType,
                out List<ISKUGroupType> skuGroupTypelst);
            if (DatamanagerGeneric<List<ISKUGroup>>.TryGet(CommonMasterDataConstants.SKUGroup, out List<ISKUGroup> skuGroup))
            {
                SKUGroup.Clear();
                SKUGroup.AddRange(skuGroup);
            }
            //List<SelectionItem> sKUGroupTypeList = new() { new() { UID = "sku", Code = "sku", Label = "SKU" } };
            //SKUGroupTypes = (List<SKUGroupType>)_dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroupType);
            //SKUGroup = (List<SKUGroup>)_dataManager.GetData(Winit.Shared.Models.Constants.CommonMasterDataConstants.SKUGroup);
            //foreach (var item in SKUGroupTypes)
            //{
            //    SKUGroupTypeList.Add(new SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
            //}
            // this.SKUGroupTypeList = sKUGroupTypeList.ToList<ISelectionItem>();
            if (isSkuGroupTypeAvailable)
            {
                foreach (var item in skuGroupTypelst)
                {
                    SKUGroupTypeList.Add(new SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
                }
            }
        }
        catch (Exception ex)
        {
            // await _alertService.ShowErrorAlert("Error", ex.Message);
        }

        Category = new();
        InstantFormats = new();
        Type = new();
        OfferTypeList = new();
        SlabOfferTypeList = new();
        SlabOrderTypeList = new();
        OrderTypeList = new();
        foreach (ListHeader.Model.Classes.ListItem item in PromotionsDropDowns)
        {
            SelectionItem selectionItem = new SelectionItem
            {
                UID = item.UID,
                Label = item.Name,
                Code = item.Code,
            };
            if (item.ListHeaderUID.Equals("PROMOTION_CATEGORY"))
            {
                Category.Add(selectionItem);
                if (PromotionView != null)
                {
                    if (selectionItem.Code.Equals(PromotionView.Category))
                    {
                        CategoryLabel = selectionItem.Label;
                        selectionItem.IsSelected = true;
                    }
                }

            }
            else if (item.ListHeaderUID.Equals("PROMOTION_TYPE"))
            {
                if (item.Code.Equals(PromotionView?.Type))
                {
                    TypeLabel = selectionItem.Label; selectionItem.IsSelected = true;
                }

            }
            else if (item.ListHeaderUID.Equals("PROMO_BUNDLE_FORMATS") || item.ListHeaderUID.Equals("PROMO_INSTANT_FORMATS") || item.ListHeaderUID.Equals("PROMO_INVOICE_FORMATS"))
            {

                if (PromotionView != null)
                {
                    if (selectionItem.Code.Equals(PromotionView.PromoFormat))
                    {
                        InstantFormatsLabel = selectionItem.Label;
                        selectionItem.IsSelected = true;
                    }
                }
                InstantFormats.Add(selectionItem);
            }
            else if (item.ListHeaderUID.Equals("PROMO_INVOICE_OFFERTYPE"))
            {
                OfferTypeList.Add(selectionItem);
                SlabOfferTypeList.Add(selectionItem);
                if (item.Code.Equals(OfferTypeCode))
                {
                    OfferTypeLabel = selectionItem.Label;
                    selectionItem.IsSelected = true;
                }
            }
            else if (item.ListHeaderUID.Equals("PROMO_INVOICE_ORDERTYPE"))
            {
                OrderTypeList.Add(selectionItem);
                if (item.Code.Equals(OrderTypeCode))
                {
                    OrderTypeLabel = selectionItem.Label;
                    SelectedOrderType = OrderTypeLabel;
                    selectionItem.IsSelected = true;
                }
            }
            else if (item.ListHeaderUID.Equals("PROMO_INSTANT_ORDERTYPE"))
            {
                SlabOrderTypeList.Add(selectionItem);
                if (item.Code.Equals(SlabOrderTypeUID))
                {
                    SlabOrderTypeLabel = selectionItem.Label;
                    selectionItem.IsSelected = true;
                }
            }
        }
    }
    public void OnPromotionchange()
    {
        DiscountLabel = @_localizer["discount"];
        IsDiscountShow = true;
        IsOfferFOCType = false;
        IsSlabTypePromotion = false;
        switch (PromotionView.PromoFormat)
        {
            case "IQFD" or "BQFD":
                PromotionType = "Value";
                break;
            case "IQPD" or "BQPD":
                PromotionType = "Percentage";
                DiscountLabel = @_localizer["discount_percentage"];
                break;
            case "IQXF" or "BQXF":
                PromotionType = "FOC";
                IsDiscountShow = false;
                IsOfferFOCType = true;
                break;
            case "INSTANTSLAB" or "BUNDLESLAB":
                PromotionType = PromotionView.PromoFormat;
                IsSlabTypePromotion = true;
                break;
            case "IQRF" or "BQRF":
                PromotionType = "Percentage";
                DiscountLabel = @_localizer["replace_unit_price"];
                break;
        }
    }
    public void OnGroupTypeSelected(DropDownEvent dropDownEvent)
    {
        List<SelectionItem> sKUGroupList = new List<SelectionItem>();
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                SelectedGroupType = dropDownEvent.SelectionItems.FirstOrDefault();
                if (SelectedGroupType != null)
                {
                    if (SelectedGroupType.Code.Equals("sku"))
                    {
                        IsGroupTypeSKUType = true;
                        SelectedGroupTypeLabel = @_localizer["sku"];
                    }
                    else
                    {
                        IsGroupTypeSKUType = false;
                        SelectedGroupTypeLabel = SelectedGroupType.Label;
                        foreach (var item in SKUGroup)
                        {
                            if (item.SKUGroupTypeUID.Equals(SelectedGroupType.UID))
                            {
                                sKUGroupList.Add(new Winit.Shared.Models.Common.SelectionItem() { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
                            }
                        }
                        this.SKUGroupList = sKUGroupList.ToList<ISelectionItem>();
                    }
                }
            }
            else
            {
                IsGroupTypeSKUType = false;
                SelectedGroupTypeLabel = string.Empty;
            }
        }

        IsGroupTypeVisible = false;

    }
    public void OnGroupItemsSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            SelectedItems.Clear();
            if (dropDownEvent.SelectionItems != null)
            {
                SelectedItems.AddRange(dropDownEvent.SelectionItems);
                SelectedGroupLabel = SelectedItems.Count >= 1 ? (SelectedItems.Count == 1 ?
                    SelectedItems.FirstOrDefault().Label : $"{SelectedItems.Count} items selected") : "Select Group";

            }
        }

        IsGroupVisible = false;
    }
    public void OnSelectionModelSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                ISelectionItem? SelectedItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (SelectedItem != null)
                {
                    SelectionModelLabel = SelectedItem.Label;
                    SelectionModel = SelectedItem.Code;

                    if (SelectionModel.Equals("All"))
                    {
                        foreach (var item in PromotionItemsModelList)
                        {
                            item.IsCompulsary = true;
                        }
                        ShowBundleQty = false;
                    }
                    else
                    {
                        ShowBundleQty = true;
                    }
                }
                else
                {
                    SelectionModelLabel = @_localizer["select_selection_model"];
                    SelectionModel = string.Empty;
                    ShowBundleQty = false;
                }
            }
            else
            {
                SelectionModelLabel = @_localizer["select_selection_model"];
                SelectionModel = string.Empty;
                ShowBundleQty = false;
            }
        }

        IsSelectionModelDisplay = false;
    }
    public void OnCategorySelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                ISelectionItem selectedItems = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectedItems != null)
                {
                    CategoryLabel = selectedItems.Label;
                    PromotionView.Category = selectedItems.Code;
                }
                else
                {
                    PromotionView.Category = string.Empty;
                    CategoryLabel = @_localizer["select_category"];
                }
            }
        }

        IsCategoryDisplay = false;
    }
    public void OnInstantFormatsSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                ISelectionItem selectedItems = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectedItems != null)
                {
                    InstantFormatsLabel = selectedItems.Label;
                    PromotionView.PromoFormat = selectedItems.Code;
                }
                else
                {
                    PromotionView.PromoFormat = string.Empty;
                    InstantFormatsLabel = @_localizer["select_promotion_format"];
                }
            }
        }
        OnPromotionchange();
        IsInstantFormatsDisplay = false;
    }
    public void OnOfferOrorderTypeSelected(DropDownEvent dropDownEvent)
    {
        if (IsOrderTypeDisplay)
        {
            SelectedOrderType = string.Empty;
        }
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                ISelectionItem? selectedItems = dropDownEvent?.SelectionItems?.FirstOrDefault<ISelectionItem>();
                if (selectedItems != null)
                {
                    if (IsOfferTypeDisplay)
                    {
                        OfferTypeCode = selectedItems.Code;
                        OfferTypeLabel = selectedItems.Label;
                        if (OfferTypeCode.Equals("FOC"))
                        {
                            IsOfferFOCType = true;
                        }
                        else
                        {
                            IsOfferFOCType = false;
                        }
                    }
                    else if (IsOrderTypeDisplay)
                    {
                        OrderTypeCode = selectedItems.Code;
                        OrderTypeLabel = selectedItems.Label;
                        SelectedOrderType = selectedItems.Label;
                    }
                }
                else
                {
                    if (IsOfferTypeDisplay)
                    {
                        OfferTypeCode = string.Empty;
                        OfferTypeLabel = @_localizer["select_offer_type"];
                    }
                    else if (IsOrderTypeDisplay)
                    {
                        OrderTypeCode = string.Empty;
                        OrderTypeLabel = @_localizer["select_buy_criteria"];
                    }
                }
            }
        }
        IsOfferTypeDisplay = false;
        IsOrderTypeDisplay = false;
    }

    #region For Slab Promotions

    public void ShowFreeSlabSKU(IPromoOrderForSlabs promoOrderForSlabs)
    {
        promoOrderForSlabs.IsOfferTypeDisplay = true;
        DisplaySlabOfferTypeList = new();
        bool isNew = string.IsNullOrEmpty(promoOrderForSlabs.OfferTypeUID);

        foreach (ISelectionItem selectionItem in SlabOfferTypeList)
        {
            ISelectionItem iSelectionItem = new SelectionItem()
            {
                UID = selectionItem.UID,
                Code = selectionItem.Code,
                Label = selectionItem.Label,
            };
            if (!isNew)
            {
                iSelectionItem.IsSelected = iSelectionItem.UID.Equals(promoOrderForSlabs.OfferTypeUID) ? true : false;
            }
            DisplaySlabOfferTypeList.Add(iSelectionItem);
        }
    }
    public void OnSlabOrderTypeChanaged(DropDownEvent dropDownEvent)
    {
        IsSlabOrderTypeDisplay = false;
        if (dropDownEvent != null)
        {
            SlabOrderTypeLabel = @_localizer["select_order_type"];
            SlabOrderTypeUID = string.Empty;
            if (dropDownEvent.SelectionItems != null)
            {
                ISelectionItem? selectedItems = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectedItems != null)
                {
                    SlabOrderTypeLabel = selectedItems.Label;
                    SlabOrderTypeUID = selectedItems.Code;
                }
            }
        }
    }
    public void OnSlabOfferTypeChanaged(DropDownEvent dropDownEvent, IPromoOrderForSlabs promoOrderForSlabs)
    {
        promoOrderForSlabs.IsOfferTypeDisplay = false;

        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                ISelectionItem? selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                if (selectionItem != null)
                {
                    promoOrderForSlabs.OfferTypeUID = selectionItem.Code;
                    promoOrderForSlabs.OfferTypeLabel = selectionItem.Label;
                }
                else
                {
                    promoOrderForSlabs.OfferTypeUID = string.Empty;
                    promoOrderForSlabs.OfferTypeLabel = @_localizer["select_offer_type"];
                }
            }
            else
            {
                promoOrderForSlabs.OfferTypeUID = string.Empty;
                promoOrderForSlabs.OfferTypeLabel = @_localizer["select_offer_type"];
            }

            if (string.IsNullOrEmpty(promoOrderForSlabs.OfferTypeUID))
            {
                promoOrderForSlabs.IsDiscountVisible = false;
                promoOrderForSlabs.IsFreeSKUVisible = false;
            }
            else
            {
                promoOrderForSlabs.IsDiscountVisible = true;
                promoOrderForSlabs.IsFreeSKUVisible = false;
                if (promoOrderForSlabs.OfferTypeUID.Equals("FOC"))
                {
                    promoOrderForSlabs.IsFreeSKUVisible = true;
                    promoOrderForSlabs.DiscountLabel = @_localizer["qty"];
                }
                else if (promoOrderForSlabs.OfferTypeUID.Equals("Value"))
                {
                    promoOrderForSlabs.DiscountLabel = @_localizer["value"];
                }
                else if (promoOrderForSlabs.OfferTypeUID.Equals("Percent"))
                {
                    promoOrderForSlabs.DiscountLabel = @_localizer["percent"];
                }
            }
        }


    }

    #endregion

    #endregion

    #region UI logic
    private void SetPromotionType()
    {
        IsAssorted = PromotionView.Type.Equals(Winit.Shared.Models.Constants.Promotions.Assorted);
        IsInvoice = PromotionView.Type.Equals(Winit.Shared.Models.Constants.Promotions.Invoice);
        IsLine = ShowBundleQty = PromotionView.Type.Equals(Winit.Shared.Models.Constants.Promotions.Line);
    }
    public void AddTierOrSlab()
    {
        string message = string.Empty;
        bool isval = true;
        if (string.IsNullOrEmpty(SlabOrderTypeUID))
        {
            message += @_localizer["order_type"] + " ,";
            isval = false;
        }
        if (string.IsNullOrEmpty(SlabSkuUID))
        {
            message += @_localizer["sku"] + " ,";
            isval = false;
        }
        if (isval)
        {
            PromoOrderForSlabsList.Add(new PromoOrderForSlabs()
            {
                UID = Guid.NewGuid().ToString(),
                OfferTypeLabel = @_localizer["select_offer_type"],
                FreeSKULabel = @_localizer["select_free_sku"],
                IsNewOfferType = true,
            });
        }
        else
        {
            _alertService.ShowErrorAlert(@_localizer["error"], $"{@_localizer["following_field(s)_having_invalid_values_:"]}{message}");
        }
    }
    public void AddSKUButtonType(bool isGroupTypeSKU)
    {
        if (isGroupTypeSKU)
        {
            IsGroupTypeSKU = true;
            IsFOCSKU = false;
        }
        else
        {
            IsGroupTypeSKU = false;
            IsFOCSKU = true;
        }
    }
    public void GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKUs)
    {
        string Message = string.Empty;
        if (SKUs != null)
        {
            if (IsSlabTypePromotion)
            {
                Winit.Modules.SKU.Model.Interfaces.ISKUV1? sKU = SKUs.FirstOrDefault();
                if (sKU != null)
                {
                    SlabSkuUID = sKU.UID;
                    SlabSKULabel = sKU.Name;
                }
            }
            else
            {
                foreach (Winit.Modules.SKU.Model.Interfaces.ISKUV1 sku in SKUs)
                {
                    if (IsGroupTypeSKU)
                    {
                        PromotionItemsModel? promotionItems = PromotionItemsModelList.Find(p => p.GroupUID == sku.UID);
                        if (promotionItems == null)
                        {
                            PromotionItemsModelList.Add(new()
                            {
                                GroupTypeName = "sku",
                                GroupCode = sku.Code,
                                GroupUID = sku.UID,
                                GroupName = sku.Name,
                                ActionType = Shared.Models.Enums.ActionType.Add,
                                IsCompulsary = !ShowBundleQty,
                            });
                        }
                        else
                        {
                            Message += sku.Code + " ,";
                        }
                    }
                    else if (IsFOCSKU)
                    {
                        Winit.Modules.SKU.Model.Interfaces.ISKUV1? existIngSKu = SelectedSKU.Find(p => p.UID == sku.UID & p.ActionType != Shared.Models.Enums.ActionType.Delete);
                        if (existIngSKu == null)
                        {
                            SelectedSKU.Add(sku);
                        }

                        else
                        {
                            Message += sku.Code + " ,";
                        }
                    }

                }
            }
            if (!string.IsNullOrEmpty(Message))
            {
                Message = Message.Substring(0, Message.Length - 2);
                _alertService.ShowErrorAlert(@_localizer["error"], Message + @_localizer["items_already_exists"]);
            }
        }
    }
    public void GetSelectedItemForSlab(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SKUList, IPromoOrderForSlabs promoOrderForSlabs)
    {
        if (SKUList != null)
        {
            Winit.Modules.SKU.Model.Interfaces.ISKUV1? sKU = SKUList.FirstOrDefault();
            if (sKU != null)
            {
                promoOrderForSlabs.FreeSkuUID = sKU.UID;
                promoOrderForSlabs.FreeSKULabel = sKU.Name;
            }
        }
    }

    public void AddSelectedItems()
    {
        if (IsValidate(out string errorMessage))
        {
            foreach (ISelectionItem ISelectionItem in SelectedItems)
            {
                PromotionItemsModel? model = PromotionItemsModelList.FirstOrDefault(p => p.GroupUID == ISelectionItem.UID);

                if (model == null)
                {
                    ISelectionItem.IsSelected = false;
                    PromotionItemsModelList.Add(new PromotionItemsModel()
                    {
                        GroupTypeUID = SelectedGroupType?.UID,
                        GroupTypeName = SelectedGroupType?.Label,
                        GroupUID = ISelectionItem.UID,
                        GroupCode = ISelectionItem.Code,
                        GroupName = ISelectionItem.Label,
                        IsCompulsary = !ShowBundleQty,
                    });
                }
            }
        }
        else
        {
            _alertService.ShowErrorAlert(@_localizer["error"], errorMessage);
        }
        SelectedGroupLabel = @_localizer["select_group"];
        SelectedItems.Clear();
    }
    #endregion

    #region API Calling Methods
    public async Task GetSKUFromDatabase()
    {
        Winit.Shared.Models.Common.PagingRequest pagingRequest = new Winit.Shared.Models.Common.PagingRequest();
        pagingRequest.IsCountRequired = true;
        Winit.Shared.Models.Common.ApiResponse<Winit.Shared.Models.Common.PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUV1>> apiResponse =
            await _apiService.FetchDataAsync<Winit.Shared.Models.Common.PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUV1>>(
                $"{_appConfigs.ApiBaseUrl}SKU/SelectAllSKUDetails",
                HttpMethod.Post, pagingRequest);


        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
        {
            SKUList.Clear();
            if (apiResponse.Data.PagedData != null)
            {
                SKUList = apiResponse.Data.PagedData.ToList<Winit.Modules.SKU.Model.Interfaces.ISKUV1>();
            }
        }
    }
    public async Task GetListItemsByCodes_Promotions()
    {
        Winit.Modules.ListHeader.Model.Classes.ListItemRequest listItemRequest = new ListItemRequest()
        {
            isCountRequired = true,
            Codes = new List<string>()
            {
                 "PROMOTION_CATEGORY",
                 "PROMOTION_TYPE",
                "PROMO_INVOICE_OFFERTYPE",
                "PROMO_INSTANT_ORDERTYPE"
            }
        };

        if (IsInvoice)
        {
            listItemRequest.Codes.Add("PROMO_INVOICE_FORMATS");
            listItemRequest.Codes.Add("PROMO_INVOICE_ORDERTYPE");
        }
        else
        {
            if (IsAssorted)
            {
                listItemRequest.Codes.Add("PROMO_BUNDLE_FORMATS");
            }
            else
            {
                listItemRequest.Codes.Add("PROMO_INSTANT_FORMATS");
            }

        }

        Shared.Models.Common.ApiResponse<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>> apiResponse = await
            _apiService.FetchDataAsync<PagedResponse<Winit.Modules.ListHeader.Model.Classes.ListItem>>($"{_appConfigs.ApiBaseUrl}ListItemHeader/GetListItemsByCodes", HttpMethod.Post, listItemRequest);

        if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null && apiResponse.Data.PagedData != null)
        {
            PromotionsDropDowns = apiResponse.Data.PagedData.OrderBy(p => p.SerialNo).ToList();
        }
    }
    public async Task GetPromotionDetailsBYUID(string UID)
    {
        Shared.Models.Common.ApiResponse<Winit.Modules.Promotion.Model.Classes.PromoMasterView> apiResponse =
            await _apiService.FetchDataAsync<Winit.Modules.Promotion.Model.Classes.PromoMasterView>($"{_appConfigs.ApiBaseUrl}Promotion/GetPromotionDetailsByUID?PromotionUID={UID}", HttpMethod.Get);

        if (apiResponse.IsSuccess && apiResponse.StatusCode == 200)
        {
            if (apiResponse.StatusCode == 200 && apiResponse.Data != null)
            {
                PromoMasterView = apiResponse.Data;
                SetEditModeForAllFieldsOfPromotion();
            }
        }

    }

    #endregion

    #region Set View/Edit 
    protected void SetEditModeForAllFieldsOfPromotion()
    {
        if (PromoMasterView.PromotionView != null)
        {
            PromotionView = PromoMasterView.PromotionView;
            if (PromotionView != null)
            {
                SetPromotionType();
                OnPromotionchange();
                IsNoEndDate = PromotionView.ValidUpto == null ? true : false;
            }
        }
        if (IsSlabTypePromotion)
        {
            SetEditModeForSlabTypePromotion();
        }
        else
        {
            SetEditModeForAllExeptSlab();
        }
    }
    protected void SetEditModeForSlabTypePromotion()
    {
        if (PromoMasterView.PromoOrderItemViewList != null)
        {
            PromoOrderItemView promoOrderItem = PromoMasterView.PromoOrderItemViewList.FirstOrDefault();
            if (promoOrderItem != null)
            {
                SlabSkuUID = promoOrderItem.ItemCriteriaSelected;
                Winit.Modules.SKU.Model.Interfaces.ISKUV1 sKU = SKUList.Find(p => p.UID.Equals(SlabSkuUID));
                if (sKU != null)
                {
                    SlabSKULabel = sKU.Name;
                }
            }
        }
        if (PromoMasterView.PromoOrderViewList != null && PromoMasterView.PromoOrderViewList.Any())
        {

            foreach (PromoOrderView promoOrder in PromoMasterView.PromoOrderViewList)
            {
                PromoOrderForSlabs promoOrderForSlabs = new PromoOrderForSlabs()
                {
                    PromoOrderUID = promoOrder.UID,
                };

                if (PromoMasterView.PromoConditionViewList != null)
                {
                    PromoConditionView? promoCondition = PromoMasterView.PromoConditionViewList.Find(p => p.ReferenceUID == promoOrder.UID);
                    if (promoCondition != null)
                    {
                        SlabOrderTypeUID = promoCondition.ConditionType;
                        promoOrderForSlabs.Min = promoCondition.Min;
                        promoOrderForSlabs.Max = promoCondition.Max;
                    }
                }
                if (PromoMasterView.PromoOfferViewList != null)
                {
                    PromoOfferView? promoOffer = PromoMasterView.PromoOfferViewList.Find(p => p.PromoOrderUID == promoOrder.UID);
                    if (promoOffer != null)
                    {
                        PromoConditionView? promoCondition = PromoMasterView.PromoConditionViewList.Find(p => p.ReferenceUID == promoOffer.UID);
                        if (promoCondition != null)
                        {
                            promoOrderForSlabs.OfferTypeUID = promoCondition.ConditionType;
                            promoOrderForSlabs.OfferTypeLabel = promoCondition.ConditionType;
                            promoOrderForSlabs.Discount = promoCondition.Min;

                            if (promoOrderForSlabs.OfferTypeUID.Equals("FOC"))
                            {
                                promoOrderForSlabs.IsFreeSKUVisible = true;
                                promoOrderForSlabs.DiscountLabel = @_localizer["qty"];
                            }
                            else if (promoOrderForSlabs.OfferTypeUID.Equals("Value"))
                            {
                                promoOrderForSlabs.DiscountLabel = @_localizer["value"];
                            }
                            else if (promoOrderForSlabs.OfferTypeUID.Equals("Percent"))
                            {
                                promoOrderForSlabs.DiscountLabel = @_localizer["percent"];
                            }
                            promoOrderForSlabs.IsDiscountVisible = true;
                        }
                        if (PromoMasterView.PromoOfferItemViewList != null)
                        {
                            PromoOfferItemView? promoOfferItem = PromoMasterView.PromoOfferItemViewList.Find(p => p.PromoOfferUID == promoOffer.UID);
                            if (promoOfferItem != null)
                            {
                                promoOrderForSlabs.FreeSkuUID = promoOfferItem.ItemCriteriaSelected;
                                Winit.Modules.SKU.Model.Interfaces.ISKUV1? sKU = SKUList.Find(p => p.UID.Equals(promoOrderForSlabs.FreeSkuUID));
                                if (sKU != null)
                                {
                                    promoOrderForSlabs.FreeSKULabel = sKU.Name;
                                }
                            }
                        }
                    }
                }
                PromoOrderForSlabsList.Add(promoOrderForSlabs);
            }
        }
    }
    protected void SetEditModeForAllExeptSlab()
    {
        if (PromoMasterView.PromoOrderViewList != null)
        {
            PromoOrderView? promoOrder = PromoMasterView.PromoOrderViewList.FirstOrDefault();
            if (promoOrder != null)
            {
                PromotionOrderUID = promoOrder.UID;
                MaxDealCount = promoOrder.MaxDealCount;
                SelectionModelLabel = promoOrder.SelectionModel;
                SelectionModel = promoOrder.SelectionModel;
                if (!string.IsNullOrEmpty(SelectionModel))
                {
                    if (SelectionModel.Equals("All"))
                    {
                        ShowBundleQty = false;
                    }
                    else
                    {
                        ShowBundleQty = true;
                    }
                }
            }
        }
        if (PromoMasterView.PromoOfferViewList != null)
        {
            PromoOfferView? promoOffer = PromoMasterView.PromoOfferViewList.FirstOrDefault();
            if (promoOffer != null)
            {
                PromoOfferUID = promoOffer.UID;
            }
        }
        if (PromoMasterView.PromoConditionViewList != null)
        {
            foreach (PromoConditionView promoCondition in PromoMasterView.PromoConditionViewList)
            {
                if (promoCondition.ReferenceUID == PromotionOrderUID)
                {
                    EligibleOrBundleQty = promoCondition.Min;
                    OrderTypeCode = promoCondition.ConditionType;
                }
                else if (promoCondition.ReferenceUID == PromoOfferUID)
                {
                    OfferTypeCode = promoCondition.ConditionType;
                    Discount = promoCondition.Min;
                    if (!string.IsNullOrEmpty(OfferTypeCode))
                    {
                        if (OfferTypeCode.Equals("FOC"))
                        {
                            IsOfferFOCType = true;
                        }
                    }
                }
            }
        }

        if (PromoMasterView.PromoOrderItemViewList != null && !IsInvoice)
        {
            PromotionItemsModelList = new();
            foreach (var item in PromoMasterView.PromoOrderItemViewList)
            {
                PromoConditionView? promoCondition = PromoMasterView.PromoConditionViewList?.Find(p => p.ReferenceUID == item.UID);
                bool isSkuType = item.ItemCriteriaType.ToLower() == "sku";
                ISKUGroup sKUGroup = null;
                Winit.Modules.SKU.Model.Interfaces.ISKUV1? sKU = null;

                if (isSkuType)
                {
                    sKU = SKUList?.Find(p => p.UID.Equals(item.ItemCriteriaSelected));
                }
                else
                {
                    sKUGroup = SKUGroup?.Find(itm => itm.Code == item.ItemCriteriaSelected);
                }
                PromotionItemsModel promotionItemsModel = new()
                {
                    GroupTypeName = item.ItemCriteriaType,
                    GroupUID = item.ItemCriteriaSelected,
                    IsCompulsary = item.IsCompulsory,

                };
                if (promoCondition != null)
                {
                    promotionItemsModel.Qty = promoCondition.Min;
                }
                if (sKUGroup != null)
                {
                    promotionItemsModel.GroupName = sKUGroup.Name;
                }
                else if (sKU != null)
                {
                    promotionItemsModel.GroupName = sKU.Name;
                    promotionItemsModel.GroupCode = sKU.Code;
                }
                if (!string.IsNullOrEmpty(promotionItemsModel.GroupName))
                {
                    PromotionItemsModelList.Add(promotionItemsModel);
                }

            }
        }
        if (PromoMasterView.PromoOfferItemViewList != null && IsOfferFOCType)
        {
            foreach (PromoOfferItemView promoOfferItem in PromoMasterView.PromoOfferItemViewList)
            {
                PromoConditionView? promoCondition = PromoMasterView.PromoConditionViewList?.Find(p => p.ReferenceUID == promoOfferItem.UID);
                Winit.Modules.SKU.Model.Interfaces.ISKUV1? sku = SKUList?.Find(p => p.UID == promoOfferItem.ItemCriteriaSelected);
                if (sku != null && promoCondition != null)
                {
                    sku.Qty = promoCondition.Min;
                    SelectedSKU.Add(sku);
                }
            }
        }
    }

    #endregion

    #region Validation
    private bool IsValidate(out string errorMessage)
    {
        bool isVal = true;
        errorMessage = @_localizer["the_following_field(s)_have_invalid_value(s)"] + ":";
        if (IsAssorted && string.IsNullOrEmpty(SelectionModel))
        {
            isVal = false;
            errorMessage += @_localizer["selection_model"] + " ,";
        }
        if (MaxDealCount == 0)
        {
            isVal = false;
            errorMessage += nameof(MaxDealCount) + " ,";
        }
        if (EligibleOrBundleQty == 0)
        {
            //Errormessage += nameof(EligibleQty) + " ,";
        }
        if (isVal == false)
        {
            SelectedItems = null;
            SelectedGroupLabel = @_localizer["select_group"] + " ,";
            errorMessage = errorMessage.Substring(0, errorMessage.Length - 2);

        }

        if ((SelectedItems == null || SelectedItems.Count == 0) && isVal)
        {
            isVal = false;
            errorMessage += @_localizer["select_group"];
        }

        return isVal;
    }
    protected Validation IsPromoorderForSlabsValidated()
    {
        string message = string.Empty;
        bool isval = true;
        if (string.IsNullOrEmpty(SlabOrderTypeUID))
        {
            message += @_localizer["order_type"] + " ,";
            isval = false;
        }
        if (string.IsNullOrEmpty(SlabSkuUID))
        {
            message += @_localizer["sku"] + " ,";
            isval = false;
        }
        if (!string.IsNullOrEmpty(message))
        {
            message = $"{@_localizer["the_following_field(s)_have_invalid_value(s)"]}:{message.Substring(0, message.Length - 2)}";
        }
        if (isval)
        {
            string errorMessage = string.Empty;
            int prevSlab = 0;
            int currentSlab = 1;
            decimal prevSlabMax = 0;
            if (PromoOrderForSlabsList.Any())
            {
                foreach (PromoOrderForSlabs promoOrderForSlabs in PromoOrderForSlabsList)
                {
                    bool isValMandatory = true;

                    if (promoOrderForSlabs.Min == 0)
                    {
                        errorMessage += $"{@_localizer["min"]},";
                        isValMandatory = false;
                    }
                    if (promoOrderForSlabs.Max == 0)
                    {
                        isValMandatory = false;
                        errorMessage += $"{@_localizer["max"]},";
                    }
                    if (string.IsNullOrEmpty(promoOrderForSlabs.OfferTypeUID))
                    {
                        isValMandatory = false;
                        errorMessage += $"{@_localizer["offer_type"]} ,";
                    }
                    else
                    {
                        if (promoOrderForSlabs.Discount == 0)
                        {
                            errorMessage += $"{promoOrderForSlabs.DiscountLabel} ,";
                            isValMandatory = false;
                        }
                        if (string.IsNullOrEmpty(promoOrderForSlabs.FreeSkuUID))
                        {
                            errorMessage += @_localizer["sku"] + " ,";
                            isValMandatory = false;
                        }
                    }
                    if (isValMandatory && prevSlab != 0)
                    {
                        if (prevSlabMax >= promoOrderForSlabs.Min)
                        {
                            isval = false;
                            isValMandatory = false;
                            message += $"{@_localizer["max_value_of_slab"]}{prevSlab} {@_localizer["should_not_be_greater_than_min_value_of"]} {currentSlab} ;";
                        }

                    }
                    if (!isValMandatory)
                    {
                        isval = isValMandatory;
                        message += $"{@_localizer["slab"]}{currentSlab} {@_localizer["has_invalid_value(s)_:"]}{errorMessage} ;";
                    }

                    prevSlabMax = promoOrderForSlabs.Max;
                    prevSlab++;
                    currentSlab++;
                }
            }
            else
            {
                isval = false;
                message = @_localizer["please_add_atleast_one_tier_or_slab"];
            }
        }
        return new Validation(isval, message);
    }
    public Validation IsPromotionItemsValidated()
    {
        bool isVal = true;
        string Errormessage = @_localizer["the_following_field(s)_have_invalid_value(s)"] + ":";

        if (MaxDealCount == 0 && !IsInvoice)
        {
            isVal = false;
            Errormessage += nameof(MaxDealCount) + " ,";
        }
        if (EligibleOrBundleQty == 0)
        {
            if (IsLine)
                Errormessage += @_localizer["eligible_qty"] + ",";
            else if (IsAssorted && ShowBundleQty)
                Errormessage += @_localizer["bundle_qty"] + ",";
            else if (IsInvoice)
                Errormessage += $"{@_localizer["buy"]} {SelectedOrderType},";

        }
        if (Discount == 0 && !IsAssorted)
        {
            Errormessage += DiscountLabel + ",";
        }
        if (isVal == false)
        {
            SelectedItems = new();
            SelectedGroupLabel = @_localizer["select_group"] + " ,";
            Errormessage = Errormessage.Substring(0, Errormessage.Length - 2);
            Errormessage += @_localizer["should_be_greater_than_0"];
        }
        if (isVal && !IsInvoice)
        {
            if (PromotionItemsModelList == null || PromotionItemsModelList.Count == 0)
            {
                isVal = false;
                Errormessage += @_localizer["select_group"] + ",";
            }
            else if (IsAssorted)
            {
                string msg = string.Empty;
                foreach (var item in PromotionItemsModelList)
                {
                    if (item.Qty <= 0 && item.IsCompulsary)
                    {

                        msg += item.GroupCode + " ,";
                    }
                }
                if (!string.IsNullOrEmpty(msg))
                {
                    isVal = false;
                    Errormessage += @_localizer["promotions_items:"] + msg.Substring(0, msg.Length - 2) + ";";
                }
            }
        }
        if (IsOfferFOCType)
        {
            if (SelectedSKU == null || SelectedSKU.Count == 0)
            {
                isVal = false;
                Errormessage += @_localizer["foc_product"] + " ,";
            }
            else
            {
                string msg = string.Empty;
                foreach (var item in SelectedSKU)
                {
                    if (item.Qty <= 0)
                    {

                        msg += item.Code + " ,";
                    }
                }
                if (!string.IsNullOrEmpty(msg))
                {
                    isVal = false;
                    Errormessage += @_localizer["promotions_offer_items_:"] + msg.Substring(0, msg.Length - 2) + ";";
                }
            }
        }

        return new Validation(isVal, Errormessage);
    }
    public async Task<Validation> IsPromotionDetailsValidated()
    {
        bool isVal = true;
        string Message = string.Empty;
        if (string.IsNullOrEmpty(PromotionView.Code))
        {
            Message = @_localizer["code"] + " ,";
            isVal = false;
        }
        if (string.IsNullOrEmpty(PromotionView.Name))
        {
            Message += @_localizer["promotion_name"] + " ,";
            isVal = false;
        }

        if (string.IsNullOrEmpty(PromotionView.PromoFormat))
        {
            Message += @_localizer["promotion_format"] + " ,";
            isVal = false;
        }
        if (PromotionView.Priority <= 0)
        {
            Message += @_localizer["priority"] + " ,";
            isVal = false;
        }

        if (IsNoEndDate)
        {
            PromotionView.ValidUpto = null;
        }
        else if (PromotionView.ValidFrom >= PromotionView.ValidUpto)
        {
            Message += @_localizer["valid_from_should_not_be_greater_than_or_equal_to_valid_upto"] + ",";
            isVal = false;
        }
        if (string.IsNullOrEmpty(PromotionView.Remarks))
        {
            isVal = false;
            Message += @_localizer["promotion_description"] + " ,";
        }
        Message = $"{@_localizer["the_following_field(s)_have_invalid_value(s)"]}: {Message}";
        if (isVal && IsNewPromotion)
        {
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync<string>(
                $"{_appConfigs.ApiBaseUrl}Promotion/GetPromotionDetailsValidated?PromotionUID={PromotionView.UID}&OrgUID={PromotionView.OrgUID}&PromotionCode={PromotionView.Code}&PriorityNo={PromotionView.Priority}&isNew={IsNewPromotion}", HttpMethod.Get);
            if (apiResponse != null && apiResponse.IsSuccess)
            {
                if (apiResponse != null)
                {
                    int retval = CommonFunctions.GetIntValue(apiResponse.Data);

                    if (retval != Winit.Shared.Models.Constants.Promotions.None)
                    {
                        isVal = false;
                        if (retval == Winit.Shared.Models.Constants.Promotions.Code_Priority)
                        {
                            Message = @_localizer["code_and_priority_already_exists"];
                        }
                        else if (retval == Winit.Shared.Models.Constants.Promotions.Code)
                        {
                            Message = @_localizer["code_already_exist"];
                        }
                        else if (retval == Winit.Shared.Models.Constants.Promotions.Priority)
                        {
                            Message = @_localizer["priority_already_exist"];
                        }
                    }
                }
            }
        }
        return new Validation(isVal, Message);
    }

    #endregion

    #region Promotion Saving and Saving Logic
    public async Task<Validation> SaveAsync()
    {


        Validation validation = await IsPromotionDetailsValidated();
        if (validation.IsValidated)
        {
            validation = IsSlabTypePromotion ? IsPromoorderForSlabsValidated() : IsPromotionItemsValidated();
        }
        if (validation.IsValidated)
        {
            if (IsNewPromotion)
            {
                SavePromotionDetails();
            }
            else
            {
                UpdatePromotionDetails();
            }
            ApiResponse<string> apiResponse = await _apiService.FetchDataAsync($"{_appConfigs.ApiBaseUrl}Promotion/CUDPromotionMaster", HttpMethod.Post, PromoMasterView);
            if (apiResponse != null)
            {
                if (!apiResponse.IsSuccess)
                {
                    validation.ErrorMessage = apiResponse.ErrorMessage!;
                }
            }

        }
        return validation;
    }

    protected void SavePromotionDetails()
    {
        PromotionView.UID = PromotionUID;
        PromotionView.CompanyUID = _iAppUser.Emp.CompanyUID;
        PromotionView.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
        PromotionView.IsActive = true;
        CreateFields(PromotionView);
        PromoMasterView = new();
        PromoMasterView.PromoOrderViewList = new();
        PromoMasterView.PromoConditionViewList = [];
        PromoMasterView.PromoOfferItemViewList = [];
        PromoMasterView.PromoOrderItemViewList = [];
        PromoMasterView.PromoOfferViewList = [];
        PromoMasterView.PromoOfferItemViewList = [];
        PromoMasterView.PromotionView = PromotionView;

        if (IsSlabTypePromotion)
        {
            SaveOrUpdateSlabTypePromotion();
        }
        else
        {
            if (IsInvoice)
            {
                SavePromoOrderInvoice();
                SavePromoOfferInvoiceType();
            }
            else
            {
                PromoMasterView.PromoOrderViewList = SavePromotionOrder(PromotionUID);
                SaveOrUpdatePromoOrderItems();
                SavePromoOffer();

            }
            if (IsOfferFOCType)
            {
                SaveOrUpdatePromoOfferItem();
            }
        }

    }
    protected void UpdatePromotionDetails()
    {
        UpdateFields(PromotionView);
        if (IsSlabTypePromotion)
        {
            SaveOrUpdateSlabTypePromotion();
        }
        else
        {
            if (IsNoEndDate)
            {
                PromotionView.ValidUpto = null;
            }
            PromoMasterView.PromotionView = PromotionView;
            UpdatePromotionOrder();
            if (!IsInvoice)
            {
                SaveOrUpdatePromoOrderItems();
            }
            if (IsOfferFOCType)
            {
                SaveOrUpdatePromoOfferItem();
            }
        }
    }

    protected List<PromoOrderView> SavePromotionOrder(string PromotionUID)
    {
        var orderView = new PromoOrderView
        {
            UID = PromotionOrderUID,
            PromotionUID = PromotionUID,
            ActionType = Shared.Models.Enums.ActionType.Add,
            MaxDealCount = this.MaxDealCount,
            SelectionModel = this.SelectionModel,
            QualificationLevel = this.QualifiCationLevel,
        };
        CreateFields(orderView);
        PromoMasterView.PromoConditionViewList.Add(PromoConditionViewConverter(PromotionOrderUID, Promotions.PromoOrder, EligibleOrBundleQty));
        return [orderView];
    }
    protected void UpdatePromotionOrder()
    {
        if (!IsInvoice)
        {
            foreach (PromoOrderView promoOrderView1 in PromoMasterView.PromoOrderViewList)
            {
                PromotionOrderUID = promoOrderView1.UID;
                promoOrderView1.ActionType = Shared.Models.Enums.ActionType.Add;
                promoOrderView1.MaxDealCount = this.MaxDealCount;
                UpdateFields(promoOrderView1);
            }
        }
        foreach (PromoConditionView view in PromoMasterView.PromoConditionViewList)
        {
            view.ModifiedTime = DateTime.Now;
            view.ActionType = Shared.Models.Enums.ActionType.Add;
            if (view.ReferenceUID == PromotionOrderUID)
            {
                view.Min = EligibleOrBundleQty;
                if (IsInvoice)
                {
                    view.ConditionType = OrderTypeCode;
                }
            }
            else if (view.ReferenceUID == PromoOfferUID)
            {
                view.Min = Discount;
            }
        }
    }
    protected void SaveOrUpdatePromoOrderItems()
    {
        foreach (PromotionItemsModel item in PromotionItemsModelList)
        {
            string PromoOrderItemUID = Guid.NewGuid().ToString();
            bool isNewPromoOrderItemView = true;
            if (!IsNewPromotion)
            {
                foreach (PromoOrderItemView promoOrderItemView in PromoMasterView.PromoOrderItemViewList)
                {
                    if (promoOrderItemView.ItemCriteriaSelected != item.GroupUID) continue;

                    UpdateFields(promoOrderItemView);
                    promoOrderItemView.ActionType = item.ActionType;
                    promoOrderItemView.IsCompulsory = item.IsCompulsary;

                    foreach (PromoConditionView promoCondition in PromoMasterView.PromoConditionViewList)
                    {
                        if (promoOrderItemView.UID != promoCondition.ReferenceUID) continue;

                        promoCondition.ActionType = item.ActionType;
                        promoCondition.Min = item.Qty;
                        promoCondition.ModifiedTime = DateTime.Now;
                        isNewPromoOrderItemView = false;
                    }
                }
            }

            if (isNewPromoOrderItemView)
            {
                PromoMasterView.PromoOrderItemViewList.Add(PromoOrderItemViewConverter(PromoOrderItemUID, item));
                PromoMasterView.PromoConditionViewList.Add(
                    PromoConditionViewConverter(PromoOrderItemUID, Promotions.PromoOrderItem, item.Qty)
               );
            }
        }

    }
    private PromoConditionView PromoConditionViewConverter(string referenceUID, string referenceType, decimal min = 0)
    {
        var condition = new PromoConditionView()
        {
            UID = Guid.NewGuid().ToString(),
            ReferenceUID = referenceUID,
            PromotionUID = PromotionUID,
            ReferenceType = referenceType,//,
            Min = min,
            ConditionType = "Qty",
            ActionType = Winit.Shared.Models.Enums.ActionType.Add,
            UOM = "EA",
            AllUOMConversion = true,
        };
        CreateFields(condition);
        return condition;
    }
    private PromoOrderItemView PromoOrderItemViewConverter(string promoOrderItemUID, PromotionItemsModel item)
    {
        PromoOrderItemView promoOrderItem = new PromoOrderItemView
        {
            UID = promoOrderItemUID,
            PromoOrderUID = PromotionOrderUID,
            PromotionUID = PromotionUID,
            ItemCriteriaType = item.GroupTypeName,
            ItemCriteriaSelected = item.GroupUID,
            ActionType = Shared.Models.Enums.ActionType.Add,
            IsCompulsory = item.IsCompulsary,
            ItemUOM = "EA",
        };
        CreateFields(promoOrderItem);
        return promoOrderItem;
    }
    private void CreateFields(IBaseModel baseModel)
    {
        baseModel.CreatedBy = _iAppUser.Emp.CreatedBy;
        baseModel.ModifiedBy = _iAppUser.Emp.ModifiedBy;
        baseModel.CreatedTime = DateTime.Now;
        baseModel.ModifiedTime = DateTime.Now;
        baseModel.ServerAddTime = DateTime.Now;
        baseModel.ServerModifiedTime = DateTime.Now;
        baseModel.SS = 0;
    }
    private void UpdateFields(IBaseModel baseModel)
    {
        baseModel.ModifiedBy = _iAppUser.Emp.ModifiedBy;
        baseModel.ModifiedTime = DateTime.Now;
        baseModel.ServerModifiedTime = DateTime.Now;
    }
    protected void SavePromoOffer()
    {

        PromoOfferView promoOfferView = new()
        {
            UID = PromoOfferUID,
            PromoOrderUID = PromotionOrderUID,
            PromotionUID = PromotionUID,
            Type = "IXF",
            ApplicationLevel = this.ApplicationLevel,
            QualificationLevel = this.QualifiCationLevel,
            SelectionModel = "All",
            ActionType = Winit.Shared.Models.Enums.ActionType.Add,
        };
        CreateFields(promoOfferView);
        PromoMasterView.PromoOfferViewList = [promoOfferView];

        PromoMasterView.PromoConditionViewList.Add(PromoConditionViewConverter(PromoOfferUID, Promotions.PromoOffer, Discount));
    }
    protected void SaveOrUpdatePromoOfferItem()
    {
        foreach (Winit.Modules.SKU.Model.Interfaces.ISKUV1 sku in SelectedSKU)
        {
            bool isNewSKU = true;

            if (!IsNewPromotion)
            {
                foreach (PromoOfferItemView promoOfferItem in PromoMasterView.PromoOfferItemViewList)
                {
                    if (sku.Code == promoOfferItem.ItemCriteriaSelected)
                    {
                        promoOfferItem.ModifiedTime = DateTime.Now;
                        promoOfferItem.ActionType = sku.ActionType;

                        foreach (PromoConditionView promoCondition in PromoMasterView.PromoConditionViewList)
                        {
                            if (promoCondition.ReferenceUID == promoOfferItem.UID)
                            {
                                promoCondition.Min = sku.Qty;
                                promoCondition.ActionType = sku.ActionType;
                                promoCondition.ModifiedTime = DateTime.Now;
                                promoCondition.ActionType = sku.ActionType;
                            }
                        }
                        isNewSKU = false;
                    }
                }
            }
            if (isNewSKU)
            {
                string PromoOfferitemUID = Guid.NewGuid().ToString();
                PromoMasterView.PromoOfferItemViewList.Add(new()
                {
                    UID = PromoOfferitemUID,
                    PromoOfferUID = PromoOfferUID,
                    PromotionUID = PromotionUID,
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    ItemCriteriaType = "sku",
                    ItemCriteriaSelected = sku.Code,
                    ItemUOM = "EA",
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                });


                PromoMasterView.PromoConditionViewList.Add(
                   new PromoConditionView()
                   {
                       ActionType = Shared.Models.Enums.ActionType.Add,
                       UID = Guid.NewGuid().ToString(),
                       ReferenceUID = PromoOfferitemUID,
                       PromotionUID = PromotionUID,
                       ReferenceType = "PromoOfferItem",
                       Min = sku.Qty,
                       ConditionType = "Qty",
                       CreatedBy = _iAppUser.Emp.CreatedBy,
                       ModifiedBy = _iAppUser.Emp.ModifiedBy,
                       ModifiedTime = DateTime.Now,
                       ServerAddTime = DateTime.Now,
                       ServerModifiedTime = DateTime.Now,
                   });

            }
        }
    }



    //Invoice Type Promotion
    protected void SavePromoOrderInvoice()
    {
        PromoMasterView.PromoOrderViewList = new()
        {
            new PromoOrderView
             {
             UID=PromotionOrderUID,
             PromotionUID=PromotionUID,
             ActionType=Shared.Models.Enums.ActionType.Add,
             SelectionModel="Any",
             QualificationLevel=this.QualifiCationLevel,
             CreatedTime=DateTime.Now,
             ModifiedTime=DateTime.Now,
             ServerModifiedTime=DateTime.Now,
             ServerAddTime=DateTime.Now,
             }
        };
        PromoMasterView.PromoConditionViewList.Add(new()
        {
            UID = Guid.NewGuid().ToString(),
            ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOrder,
            ReferenceUID = PromotionOrderUID,
            PromotionUID = PromotionUID,
            ConditionType = OrderTypeCode,
            Min = EligibleOrBundleQty,
            UOM = "EA",
            AllUOMConversion = true,
            SS = 0,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
        });
    }
    protected void SavePromoOfferInvoiceType()
    {
        PromoMasterView.PromoOfferViewList = new()
        {
          new()
           {
            UID = PromoOfferUID,
            PromoOrderUID = PromotionOrderUID,
            PromotionUID = PromotionUID,
            ApplicationLevel = this.ApplicationLevel,
            QualificationLevel=this.QualifiCationLevel,
            CreatedTime=DateTime.Now,
            ServerAddTime=DateTime.Now,
           },
        };
        PromoMasterView.PromoConditionViewList.Add(new()
        {
            UID = Guid.NewGuid().ToString(),
            CreatedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
            ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOffer,
            ReferenceUID = PromoOfferUID,
            PromotionUID = PromotionUID,
            ConditionType = OfferTypeCode,
            Min = Discount,
        });
    }

    //Slab Type Promotion
    protected void SaveOrUpdateSlabTypePromotion()
    {
        foreach (IPromoOrderForSlabs promoOrderForSlabs in PromoOrderForSlabsList)
        {
            if (promoOrderForSlabs.IsNewOfferType)
            {
                string promoOrderUID = Guid.NewGuid().ToString();
                string promoOfferUID = Guid.NewGuid().ToString();
                string promoOrderItemUID = Guid.NewGuid().ToString();
                string promoOfferitemUID = Guid.NewGuid().ToString();

                //Promo Order
                PromoMasterView.PromoOrderViewList.Add(new PromoOrderView
                {
                    UID = promoOrderUID,
                    PromotionUID = PromotionUID,
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    SelectionModel = "All",
                    QualificationLevel = this.QualifiCationLevel,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                });
                //Promo Order Condition
                PromoMasterView.PromoConditionViewList.Add(new()
                {
                    UID = Guid.NewGuid().ToString(),
                    ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOrder,
                    ReferenceUID = promoOrderUID,
                    PromotionUID = PromotionUID,
                    ConditionType = SlabOrderTypeUID,
                    Min = promoOrderForSlabs.Min,
                    Max = promoOrderForSlabs.Max,
                    UOM = "EA",
                    AllUOMConversion = true,
                    SS = 0,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                });
                //Promo Order item
                PromoMasterView.PromoOrderItemViewList.Add(new PromoOrderItemView
                {
                    UID = promoOrderItemUID,
                    PromoOrderUID = promoOrderUID,
                    PromotionUID = PromotionUID,
                    ItemCriteriaType = "sku",
                    ItemCriteriaSelected = SlabSkuUID,
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    CreatedBy = _iAppUser.Emp.CreatedBy,
                    ModifiedBy = _iAppUser.Emp.ModifiedBy,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    IsCompulsory = true,
                    ItemUOM = "EA",
                });
                //Promo Order Item Condition
                PromoMasterView.PromoConditionViewList.Add(new PromoConditionView()
                {
                    UID = Guid.NewGuid().ToString(),
                    ReferenceUID = promoOrderItemUID,
                    PromotionUID = PromotionUID,
                    ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOrderItem,
                    ConditionType = SlabOrderTypeUID,
                    CreatedBy = _iAppUser.Emp.CreatedBy,
                    ModifiedBy = _iAppUser.Emp.ModifiedBy,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add,
                    UOM = "EA",
                    AllUOMConversion = true,
                });
                //Promo  Offer  
                PromoMasterView.PromoOfferViewList.Add(new()
                {
                    UID = promoOfferUID,
                    PromoOrderUID = promoOrderUID,
                    PromotionUID = PromotionUID,
                    Type = "IXF",
                    ApplicationLevel = this.ApplicationLevel,
                    QualificationLevel = this.QualifiCationLevel,
                    CreatedBy = _iAppUser.Emp.CreatedBy,
                    ModifiedBy = _iAppUser.Emp.ModifiedBy,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    SelectionModel = "All",
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add,
                });
                //Promo  Offer  Condition
                PromoMasterView.PromoConditionViewList.Add(new()
                {
                    UID = Guid.NewGuid().ToString(),
                    ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOffer,
                    ReferenceUID = promoOfferUID,
                    PromotionUID = PromotionUID,
                    ConditionType = promoOrderForSlabs.OfferTypeUID,
                    Min = promoOrderForSlabs.Discount,
                    CreatedBy = _iAppUser.Emp.CreatedBy,
                    ModifiedBy = _iAppUser.Emp.ModifiedBy,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    ActionType = Winit.Shared.Models.Enums.ActionType.Add,
                    UOM = "EA",
                    AllUOMConversion = true,
                });
                //Promo Offer Item
                PromoMasterView.PromoOfferItemViewList.Add(new()
                {
                    UID = promoOfferitemUID,
                    PromoOfferUID = promoOfferUID,
                    PromotionUID = PromotionUID,
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    ItemCriteriaType = "sku",
                    ItemCriteriaSelected = promoOrderForSlabs.IsFreeSKUVisible ? promoOrderForSlabs.FreeSkuUID : null,
                    ItemUOM = "EA",
                    IsCompulsory = true,
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                });

                //Promo Offer item Condition

                PromoMasterView.PromoConditionViewList.Add(
                  PromoConditionViewConverter(promoOfferitemUID, Promotions.PromoOfferItem));
            }
            else
            {
                PromoOfferView? promoOffer = PromoMasterView.PromoOfferViewList.Find(p => p.PromoOrderUID.Equals(promoOrderForSlabs.PromoOrderUID));
                foreach (PromoConditionView promoConditionView in PromoMasterView.PromoConditionViewList)
                {
                    if (promoConditionView.ReferenceUID.Equals(promoOrderForSlabs.PromoOrderUID))
                    {
                        promoConditionView.Min = promoOrderForSlabs.Min;
                        promoConditionView.Max = promoOrderForSlabs.Max;
                    }
                    if (promoOffer != null)
                    {
                        if (promoConditionView.ReferenceUID.Equals(promoOffer.UID))
                        {
                            promoConditionView.Min = promoOrderForSlabs.Discount;
                        }
                    }
                }

                foreach (PromoOrderItemView promoOrderItemView in PromoMasterView.PromoOrderItemViewList)
                {
                    if (promoOrderItemView.PromoOrderUID.Equals(promoOrderForSlabs.PromoOrderUID))
                    {
                        promoOrderItemView.ItemCriteriaSelected = SlabSkuUID;
                    }
                }
                foreach (PromoOfferItemView promoOfferItemView in PromoMasterView.PromoOfferItemViewList)
                {
                    if (promoOffer != null)
                    {
                        if (promoOfferItemView.PromoOfferUID.Equals(promoOffer.UID))
                        {
                            promoOfferItemView.ItemCriteriaSelected = promoOrderForSlabs.FreeSkuUID;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Delete Operation
    public async Task DeleteOrderItems(PromotionItemsModel promotionItemsModel)
    {
        if (await _alertService.ShowConfirmationReturnType(@_localizer["confirm"], @_localizer["are_you_sure_you_want_to_delete?"]))
        {
            promotionItemsModel.ActionType = Shared.Models.Enums.ActionType.Delete;
        }
    }
    public async Task DeleteOfferItem(Winit.Modules.SKU.Model.Interfaces.ISKUV1 sKU)
    {
        if (await _alertService.ShowConfirmationReturnType(@_localizer["confirm"], @_localizer["are_you_sure_you_want_to_delete?"]))
        {
            sKU.ActionType = Shared.Models.Enums.ActionType.Delete;
        }
    }
    public async Task DeleteOrderItem(PromotionItemsModel promotionItemsModel)
    {
        if (await _alertService.ShowConfirmationReturnType(@_localizer["confirm"], @_localizer["are_you_sure_you_want_to_delete?"]))
        {
            promotionItemsModel.ActionType = Shared.Models.Enums.ActionType.Delete;
        }
    }

    #endregion

}
