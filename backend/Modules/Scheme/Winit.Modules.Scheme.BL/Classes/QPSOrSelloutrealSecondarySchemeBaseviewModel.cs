using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common;
using Winit.UIComponents.Common.Services;
using Winit.UIComponents.SnackBar;

namespace Winit.Modules.Scheme.BL.Classes;

public abstract class QPSOrSelloutrealSecondarySchemeBaseviewModel : SchemeViewModelBase,
    IQPSOrSelloutrealSecondarySchemeViewModel
{
    public QPSOrSelloutrealSecondarySchemeBaseviewModel(IAppConfig appConfig, ApiService apiService,
        IServiceProvider serviceProvider, IAppUser appUser, ILoadingService loadingService, IAlertService alertService,
        IAppSetting appSetting, CommonFunctions commonFunctions, IAddProductPopUpDataHelper addProductPopUpDataHelper,
        IToast toast) : base(appConfig
        , apiService, serviceProvider, appUser, loadingService, alertService, appSetting, commonFunctions,
        addProductPopUpDataHelper, toast)
    {
    }

    ~QPSOrSelloutrealSecondarySchemeBaseviewModel()
    {
    }

    protected IDataManager _dataManager;

    protected List<Winit.Modules.ListHeader.Model.Interfaces.IListItem>? ListItems { get; set; }
    public PromoMasterView PromoMasterView { get; set; }
    public bool IsNoEndDate { get; set; }
    public bool IsInitialize { get; set; }
    public bool IsFOCType { get; set; }
    public bool IsQPSScheme { get; set; }
    public List<ISelectionItem> SlabOrderTypeList { get; set; }
    public List<ISelectionItem> SKUGroupDDL { get; set; } = [];
    public List<ISKUGroup> SKUGroupList { get; set; }
    public List<ISKUGroupType> SKUGroupTypeList { get; set; }
    public List<ISelectionItem> SKUGroupTypeDDL { get; set; }
    public List<ISelectionItem> OrderTypeDDL { get; set; }
    public List<IQPSSchemeProducts> SchemeProducts { get; set; }
    public List<ISelectionItem> OfferTypeDDL { get; set; }
    public List<ISchemeSlab> SchemeSlabs { get; set; }
    public ISchemeSlab SchemeSlab { get; set; }
    public string FreeSKULabel { get; set; }
    public string SelectedValueSkuType { get; set; } = string.Empty;
    public bool IsGroupTypeSKU { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKU> SKUList { get; set; }
    public List<SKUAttributeDropdownModel>? SKUAttributeData { get; private set; }

    private ISelectionItem SelectedOrderType { get; set; }
    private ISelectionItem SelectedGroupType { get; set; }
    private ISelectionItem SelectedGroup { get; set; }
    private ISelectionItem SelectedOfferType { get; set; }
    private List<ISelectionItem> Selectedvalues { get; set; }
    private Winit.Modules.SKU.Model.Interfaces.ISKU SelectedSKU;
    private string QualifiCationLevel = string.Empty;
    private string ApplicationLevel = string.Empty;

    public async Task PopulateViewModel()
    {
        List<Task> tasks = new List<Task>();
        PromoMasterView.IsNew = IsNew = PageType.New.Equals(_commonFunctions.GetParameterValueFromURL(PageType.Page));
        tasks.Add(GetSKuGroup());
        tasks.Add(GetSKuGroupType());
        tasks.Add(GetSKUAttributeData());
        tasks.Add(GetListItemsByCodes());
        tasks.Add(PopulateApplicableToCustomersAndSKU());
        if (IsNew)
        {
            PromoMasterView.PromotionView.Code =
                GetSchemeCodeBySchemeName(IsQPSScheme ? SchemeConstants.QPS_Code : SchemeConstants.SOA);
            PromoMasterView.PromotionView.UID = Guid.NewGuid().ToString();
        }
        else
        {
            string UID = _commonFunctions.GetParameterValueFromURL("UID");
            tasks.Add(PopulateApprovalEngine(UID));
            tasks.Add(GetSchemeDetailsBYUID(UID));
        }

        await Task.WhenAll(tasks);
        IsInitialize = true;
    }

    private async Task GetSKuGroup()
    {
        SKUGroupList = await _addProductPopUpDataHelper.GetSKUGroup(new());
    }

    private async Task GetSKUAttributeData()
    {
        try
        {
            SKUAttributeData = await _addProductPopUpDataHelper.GetSKUAttributeData();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task GetSKuGroupType()
    {
        SKUGroupTypeList = await _addProductPopUpDataHelper.GetSKUGroupType(new());
        SKUGroupTypeDDL = new()
        {
            new SelectionItem() { Code = QpsConstants.SKU, UID = QpsConstants.SKU, Label = QpsConstants.SKU }
        };
        if (SKUGroupTypeList != null)
        {
            foreach (var item in SKUGroupTypeList)
            {
                SKUGroupTypeDDL.Add(new SelectionItem()
                { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
            }
        }
    }

    #region UI Logic

    public void OnOrderTypeSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null)
        {
            SelectedOrderType = dropDownEvent?.SelectionItems?.FirstOrDefault();
            SchemeProducts.Clear();
        }
    }

    public void OnOfferTypeSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
            {
                SelectedOfferType = dropDownEvent?.SelectionItems?.FirstOrDefault();
                if (SelectedOfferType != null)
                {
                    SchemeSlab.OfferType = SelectedOfferType.Code;
                    if (SelectedOfferType.Code?.ToLower() == "foc")
                    {
                        SchemeSlab.IsFOCType = true;
                        FreeSKULabel = "Select SKU";
                    }
                    else
                    {
                        SchemeSlab.IsFOCType = false;
                    }
                }
            }
            else
            {
                SelectedOfferType = null;
                SchemeSlab.OfferType = string.Empty;
                SchemeSlab.IsFOCType = false;
            }
        }
    }

    public void OnGroupTypeSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            if (dropDownEvent.SelectionItems != null)
            {
                SelectedGroupType = dropDownEvent.SelectionItems.FirstOrDefault();
                if (SelectedGroupType != null)
                {
                    if (SelectedGroupType.Code == QpsConstants.SKU)
                    {
                        IsGroupTypeSKU = true;
                        SelectedValueSkuType = "Select SKU";
                    }
                    else
                    {
                        IsGroupTypeSKU = false;
                        SKUGroupDDL.Clear();
                        foreach (var item in SKUGroupList)
                        {
                            if (item.SKUGroupTypeUID.Equals(SelectedGroupType?.UID))
                            {
                                SKUGroupDDL.Add(new Winit.Shared.Models.Common.SelectionItem()
                                { Code = item.Code, UID = item.UID, Label = item.Name, IsSelected = false });
                            }
                        }
                    }
                }
            }
        }
    }


    public void OnGroupSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
            Selectedvalues = dropDownEvent.SelectionItems;
    }

    List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> SelectedSKUProducts;

    public void AddSelectedItems()
    {
        if (SelectedOrderType == null)
            return;
        if (IsGroupTypeSKU)
        {
            if (SelectedSKUProducts == null)
                return;

            foreach (var item in SelectedSKUProducts)
            {
                if (!SchemeProducts.Any(p => p.SelectedValue == item.UID))
                {
                    SchemeProducts.Add(new QPSSchemeProducts()
                    {
                        OrderType = SelectedOrderType.Label,
                        OrderTypeCode = SelectedOrderType.Code,
                        SKUGroupType = SelectedGroupType.Label,
                        SKUGroupTypeCode = SelectedGroupType.Code,
                        SelectedValue = item.Name,
                        SelectedValueCode = item.UID,
                        IsNewProduct = true,
                        IsSKU = true,
                    });
                }
            }

            return;
        }

        if (SelectedOrderType != null && SelectedGroupType != null && Selectedvalues != null)
        {
            string errorMessage = string.Empty;
            foreach (ISelectionItem? ISelectionItem in Selectedvalues)
            {
                if (!SchemeProducts.Any(p => p.SelectedValueCode == ISelectionItem.Code))
                
                {
                    SchemeProducts.Add(new QPSSchemeProducts()
                    {
                        OrderType = SelectedOrderType.Label,
                        OrderTypeCode = SelectedOrderType?.Code,
                        SKUGroupType = SelectedGroupType?.Label,
                        SKUGroupTypeCode = SelectedGroupType?.Code,
                        SelectedValue = ISelectionItem.Label,
                        SelectedValueCode = ISelectionItem.Code,
                        IsNewProduct = true
                    });
                }
                else
                {
                    errorMessage += ISelectionItem.Code + ",";
                }
            }
        }
    }

    public void GetSelectedItems(List<Winit.Modules.SKU.Model.Interfaces.ISKUV1> sKUs)
    {
        if (sKUs != null)
        {
            if (SchemeSlab.IsFOCType)
            {
                SelectedSKU = sKUs.FirstOrDefault();
                FreeSKULabel = SelectedSKU == null ? "Select SKU" : SelectedSKU.Name;
                SchemeSlab.OfferItem = SelectedSKU!.Code;
                SchemeSlab.OfferItemUID = SelectedSKU.UID;
            }
            else
            {
                SelectedSKUProducts = sKUs;
                SelectedValueSkuType = $"{sKUs.Count} record selected";
            }

            //to remove selection in sku pop up
            SelectedSKUs = [];
        }
    }

    public void AddSchemeSlab()
    {
        SchemeSlab.IsNewSlab = true;
        SchemeSlab.PromoOrderUID = Guid.NewGuid().ToString();
        SchemeSlab.PromoOfferUID = Guid.NewGuid().ToString();
        var isVal = IsSlabValidated(SchemeSlab);
        if (!isVal.Item1)
        {
            _toast.Add("Error", isVal.Item2, UIComponents.SnackBar.Enum.Severity.Error);
            return;
        }


        foreach (var item in OfferTypeDDL)
        {
            item.IsSelected = false;
        }
    }

    #endregion

    #region Validation

    public void IsDetailsValidated(out bool isVal, out string message)
    {
        isVal = true;
        message = string.Empty;
        //if (SelectedChannelPartner == null && IsNew)
        //{
        //    isVal = false;
        //    message += "Channel Partner, ";
        //}
        if (string.IsNullOrEmpty(PromoMasterView.PromotionView.Code) ||
            string.IsNullOrWhiteSpace(PromoMasterView.PromotionView.Code))
        {
            isVal = false;
            message += "Scheme code, ";
        }

        if (string.IsNullOrEmpty(PromoMasterView.PromotionView.Name) ||
            string.IsNullOrWhiteSpace(PromoMasterView.PromotionView.Name))
        {
            isVal = false;
            message += "Scheme name, ";
        }

        if (PromoMasterView.PromotionView.ValidFrom == null ||
            PromoMasterView.PromotionView.ValidFrom == DateTime.MinValue)
        {
            isVal = false;
            message += "Valid from, ";
        }

        if (PromoMasterView.PromotionView.ValidUpto == null ||
            PromoMasterView.PromotionView.ValidUpto == DateTime.MinValue)
        {
            if (!IsNoEndDate)
            {
                isVal = false;
                message += "Valid upto, ";
            }
        }

        if (isVal)
        {
            if (PromoMasterView.PromotionView.ValidUpto <= PromoMasterView.PromotionView.ValidFrom)
            {
                isVal = false;
                message = "Valid upto should be greater than valid from ";
            }
        }
        else
        {
            message = "Following fields should not be empty : " + message.Substring(0, message.Length - 2);
        }
    }

    public void IsSchemeProductsValidated(out bool isVal, out string message)
    {
        isVal = true;
        message = string.Empty;
        IsDetailsValidated(out isVal, out message);
        if (!isVal)
        {
            return;
        }

        if (SelectedOrderType == null)
        {
            isVal = false;
            message += "Please select order type, ";
        }

        if (SchemeProducts.Count == 0)
        {
            isVal = false;
            message += "Please add atleast one SKU group or SKU !";
        }
    }

    private (bool, string) IsSlabValidated(ISchemeSlab schemeSlab)
    {
        bool isVal = true;
        string message = "Following fields shouldn't be empty :";

        if (schemeSlab.Minimum == 0)
        {
            isVal = false;
            message += "minimum ,";
        }

        if (schemeSlab.Maximum == 0)
        {
            isVal = false;
            message += "maximum ,";
        }

        if (string.IsNullOrEmpty(schemeSlab.OfferType))
        {
            isVal = false;
            message += nameof(SchemeSlab.OfferType) + " ,";
        }

        if (schemeSlab.OfferValue == 0)
        {
            isVal = false;
            message += "offer value ,";
        }

        if (schemeSlab.IsFOCType && string.IsNullOrEmpty(schemeSlab.OfferItemUID))
        {
            isVal = false;
            message += "free SKU ,";
        }


        if (!isVal)
        {
            message = message.Substring(0, message.Length - 2);
        }

        if (isVal && schemeSlab.Maximum <= schemeSlab.Minimum)
        {
            isVal = false;
            message = "Maximum value should not be less than or equal to minimum value ,";
        }

        if (isVal)
        {
            if (SchemeSlabs.Count > 0)
            {
                ISchemeSlab? minSlab = null;
                ISchemeSlab? maxSlab = null;
                foreach (var slab in SchemeSlabs)
                {
                    if (slab.Maximum < schemeSlab.Minimum)
                    {
                        minSlab = slab;
                    }
                    else
                    {
                        maxSlab = slab;
                        break;
                    }
                }

                if (minSlab is not null && maxSlab is not null)
                {
                    if (minSlab.Maximum < schemeSlab.Minimum && schemeSlab.Maximum < maxSlab.Minimum)
                    {
                        SchemeSlabs.Add(SchemeSlab);
                        SchemeSlab = new SchemeSlab();
                    }
                    else
                    {
                        isVal = false;
                        message = "Please add valid slab";
                    }
                }
                else
                {
                    decimal previousSlabMaxValue = schemeSlab.IsNewSlab
                            ? SchemeSlabs[SchemeSlabs.Count - 1].Maximum
                            : SchemeSlabs[SchemeSlabs.IndexOf(schemeSlab) - 1].Maximum
                        ;
                    if (schemeSlab.Minimum <= previousSlabMaxValue)
                    {
                        isVal = false;
                        message = "Please add valid slab";
                    }
                    else
                    {
                        SchemeSlabs.Add(SchemeSlab);
                        SchemeSlab = new SchemeSlab();
                    }
                }
            }
            else
            {
                SchemeSlabs.Add(SchemeSlab);
                SchemeSlab = new SchemeSlab();
            }
        }


        return (isVal, message);
    }

    #endregion

    #region Saving Logic

    bool isSaveClicked { get; set; }

    protected void SaveOrUpdateSchemeDetails()
    {
        if (IsNew)
        {
            PromoMasterView.PromotionView.CreatedBy = _appUser.Emp.UID;
            PromoMasterView.PromotionView.CreatedByEmpUID = _appUser.Emp.UID;
            PromoMasterView.PromotionView.CompanyUID = _appUser.Emp.CompanyUID;
            //PromoMasterView.PromotionView.OrgUID = SelectedChannelPartner?.UID;
            PromoMasterView.PromotionView.CreatedTime = DateTime.Now;
            PromoMasterView.PromotionView.IsActive = true;
            PromoMasterView.PromotionView.SS = 0;
            PromoMasterView.PromotionView.HasSlabs = SchemeSlabs.Any();
            PromoMasterView.PromotionView.Type = IsQPSScheme ? QpsConstants.QPS : QpsConstants.SellOutActualSecondary;
            if (IsNoEndDate)
            {
                PromoMasterView.PromotionView.ValidUpto = QpsConstants.MaxDate;
            }
        }

        PromoMasterView.PromotionView.ModifiedBy = _appUser.Emp.UID;
        PromoMasterView.PromotionView.ModifiedTime = DateTime.Now;
        PrePareApplicabletoCustomers(PromoMasterView.PromotionView.UID,
            IsQPSScheme ? SchemeConstants.QPS : SchemeConstants.SellOutRealSecondary);
        PromoMasterView.SchemeBranches = SchemeBranches;
        PromoMasterView.SchemeOrgs = SchemeOrgs;
        PromoMasterView.SchemeBroadClassifications = SchemeBroadClassifications;
    }


    protected void SaveOrUpdateSlabTypePromotion_V1()
    {
        if (!isSaveClicked)
        {
            SaveOrUpdateSchemeDetails();


            foreach (ISchemeSlab promoOrderForSlabs in SchemeSlabs)
            {
                if (promoOrderForSlabs.IsNewSlab)
                {
                    SaveSlabType(promoOrderForSlabs);
                }
                else
                {
                    AddNewProductsToExistingSlabs(promoOrderForSlabs);
                }
            }
        }

        isSaveClicked = true;
    }

    private void AddNewProductsToExistingSlabs(ISchemeSlab promoOrderForSlabs)
    {
        SchemeProducts.ForEach(p =>
        {
            if (p.IsNewProduct)
            {
                AddProductGroup(p, promoOrderForSlabs.PromoOrderUID);
            }
        });
    }

    private void DeleteSchemeProducts()
    {
        SchemeProducts.ForEach(p =>
        {
            if (p.ActionType == Shared.Models.Enums.ActionType.Delete)
            {
                PromoMasterView.PromoOrderItemViewList.ForEach(q =>
                {
                    if (q.UID.Equals(p.PromoOrderItemUID))
                    {
                        p.ActionType = Shared.Models.Enums.ActionType.Delete;
                    }
                });
            }
        });
    }

    private void DeleteSchemeSlabs(ISchemeSlab promoOrderForSlabs)
    {
        PromoMasterView.PromoOrderViewList.ForEach(p =>
        {
            if (p.UID.Equals(promoOrderForSlabs.PromoOrderUID))
            {
                p.ActionType = Shared.Models.Enums.ActionType.Delete;
            }
        });
        PromoMasterView.PromoOrderItemViewList.ForEach(p =>
        {
            if (p.PromoOrderUID.Equals(promoOrderForSlabs.PromoOrderUID))
            {
                p.ActionType = Shared.Models.Enums.ActionType.Delete;
            }
        });
        if (promoOrderForSlabs.IsFOCType)
        {
            PromoMasterView.PromoOfferItemViewList.ForEach(p =>
            {
                if (p.UID.Equals(promoOrderForSlabs.OfferItemUID))
                {
                    p.ActionType = Shared.Models.Enums.ActionType.Delete;
                }
            });
        }

        PromoMasterView.PromoConditionViewList.ForEach(p =>
        {
            if (p.ReferenceUID.Equals(promoOrderForSlabs.PromoOrderUID))
            {
                p.ActionType = Shared.Models.Enums.ActionType.Delete;
            }

            if (p.ReferenceUID.Equals(promoOrderForSlabs.PromoOfferUID))
            {
                p.ActionType = Shared.Models.Enums.ActionType.Delete;
            }

            if (promoOrderForSlabs.IsFOCType && p.ReferenceUID.Equals(promoOrderForSlabs.OfferItemUID))
            {
                p.ActionType = Shared.Models.Enums.ActionType.Delete;
            }
        });
    }

    private void UpdatePromotion(ISchemeSlab promoOrderForSlabs)
    {
    }

    private void AddProductGroup(IQPSSchemeProducts schemeProducts, string promoOrderUID)
    {
        PromoMasterView.PromoOrderItemViewList.Add(new PromoOrderItemView
        {
            UID = Guid.NewGuid().ToString(),
            PromoOrderUID = promoOrderUID,
            PromotionUID = PromoMasterView.PromotionView.UID,
            ItemCriteriaType = schemeProducts.SKUGroupTypeCode,
            ItemCriteriaSelected = schemeProducts.SelectedValueCode,
            ActionType = Shared.Models.Enums.ActionType.Add,
            CreatedBy = _appUser.Emp.UID,
            ModifiedBy = _appUser.Emp.UID,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            IsCompulsory = true,
            ItemUOM = "EA",
        });
    }

    private void SaveSlabType(ISchemeSlab promoOrderForSlabs)
    {
        //Promo Order
        PromoMasterView.PromoOrderViewList.Add(new PromoOrderView
        {
            UID = promoOrderForSlabs.PromoOrderUID,
            PromotionUID = PromoMasterView.PromotionView.UID,
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
            ReferenceUID = promoOrderForSlabs.PromoOrderUID,
            PromotionUID = PromoMasterView.PromotionView.UID,
            ConditionType = SelectedOrderType?.Code,
            Min = promoOrderForSlabs.Minimum,
            Max = promoOrderForSlabs.Maximum,
            UOM = "EA",
            AllUOMConversion = true,
            SS = 0,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
        });
        //Promo Order item
        foreach (IQPSSchemeProducts schemeProducts in SchemeProducts)
        {
            if (schemeProducts.IsNewProduct)
            {
                AddProductGroup(schemeProducts, promoOrderForSlabs.PromoOrderUID);
            }
        }

        //Promo  Offer  
        PromoMasterView.PromoOfferViewList.Add(new()
        {
            UID = promoOrderForSlabs.PromoOfferUID,
            PromoOrderUID = promoOrderForSlabs.PromoOrderUID,
            PromotionUID = PromoMasterView.PromotionView.UID,
            Type = "IXF",
            ApplicationLevel = this.ApplicationLevel,
            QualificationLevel = this.QualifiCationLevel,
            CreatedBy = _appUser.Emp.UID,
            ModifiedBy = _appUser.Emp.UID,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            SelectionModel = "All",
            HasOfferItemSelection = promoOrderForSlabs.IsFOCType,
            ActionType = Winit.Shared.Models.Enums.ActionType.Add,
        });
        //Promo  Offer  Condition
        PromoMasterView.PromoConditionViewList.Add(new()
        {
            UID = Guid.NewGuid().ToString(),
            ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOffer,
            ReferenceUID = promoOrderForSlabs.PromoOfferUID,
            PromotionUID = PromoMasterView.PromotionView.UID,
            ConditionType = promoOrderForSlabs.OfferType,
            Min = promoOrderForSlabs.OfferValue,
            Max = promoOrderForSlabs.OfferValue,
            CreatedBy = _appUser.Emp.CreatedBy,
            ModifiedBy = _appUser.Emp.ModifiedBy,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now,
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            ActionType = Winit.Shared.Models.Enums.ActionType.Add,
            UOM = "EA",
            AllUOMConversion = true,
        });
        if (promoOrderForSlabs.IsFOCType)
        {
            //Promo Offer Item
            string promoOfferitemUID = Guid.NewGuid().ToString();
            PromoMasterView.PromoOfferItemViewList.Add(new()
            {
                UID = promoOfferitemUID,
                PromoOfferUID = promoOrderForSlabs.PromoOfferUID,
                PromotionUID = PromoMasterView.PromotionView.UID,
                ActionType = Shared.Models.Enums.ActionType.Add,
                ItemCriteriaType = QpsConstants.SKU,
                ItemCriteriaSelected = promoOrderForSlabs.OfferItemUID,
                ItemUOM = "EA",
                IsCompulsory = true,
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now,
                ServerAddTime = DateTime.Now,
                ServerModifiedTime = DateTime.Now,
            });

            //Promo Offer item Condition
            PromoMasterView.PromoConditionViewList.Add(
                new PromoConditionView()
                {
                    ActionType = Shared.Models.Enums.ActionType.Add,
                    UID = Guid.NewGuid().ToString(),
                    ReferenceUID = promoOfferitemUID,
                    PromotionUID = PromoMasterView.PromotionView.UID,
                    ReferenceType = Winit.Shared.Models.Constants.Promotions.PromoOfferItem,
                    CreatedBy = _appUser.Emp.CreatedBy,
                    ModifiedBy = _appUser.Emp.ModifiedBy,
                    ModifiedTime = DateTime.Now,
                    ServerAddTime = DateTime.Now,
                    ServerModifiedTime = DateTime.Now,
                    UOM = "EA",
                    Min = promoOrderForSlabs.OfferValue,
                    Max = promoOrderForSlabs.OfferValue,
                });
        }
    }

    protected void SetEditMode(List<ISKUV1> skus)
    {
        string? selectedGroupTypeUID = string.Empty;
        SchemeSlabs = new();
        SchemeProducts = new();
        if (QpsConstants.MaxDate == PromoMasterView.PromotionView.ValidUpto)
        {
            IsNoEndDate = true;
        }

        PromoOrderView? promoOrderView = PromoMasterView.PromoOrderViewList?.FirstOrDefault();
        if (promoOrderView == null)
        {
            return;
        }

        if (PromoMasterView.PromoConditionViewList != null && PromoMasterView.PromoConditionViewList.Count > 0)
        {
            string? selectedOrderTypeCode = PromoMasterView.PromoConditionViewList
                .Find(p => p.ReferenceUID == promoOrderView.UID)?.ConditionType;

            OrderTypeDDL.ForEach(item =>
            {
                if (selectedOrderTypeCode == item.Code)
                {
                    item.IsSelected = true;
                    SelectedOrderType = item;
                }
            });
        }

        if (PromoMasterView.PromoOrderItemViewList != null)
        {
            PromoMasterView.PromoOrderItemViewList.ForEach(item =>
            {
                IQPSSchemeProducts qPSSchemeProducts = new QPSSchemeProducts()
                {
                    OrderType = SelectedOrderType?.Label,
                    OrderTypeCode = SelectedOrderType?.Code,
                    ActionType = Shared.Models.Enums.ActionType.Update,
                    Id = item.Id,
                };
                if (QpsConstants.SKU.Equals(item.ItemCriteriaType))
                {
                    ISKUV1? sKUV1 = skus?.Find(p => p.UID == item.ItemCriteriaSelected);
                    if (sKUV1 != null)
                    {
                        qPSSchemeProducts.SKUGroupType = QpsConstants.SKU;
                        qPSSchemeProducts.SKUGroupTypeCode = QpsConstants.SKU;
                        qPSSchemeProducts.SelectedValueCode = sKUV1.UID;
                        qPSSchemeProducts.SelectedValue = sKUV1.Name;
                    }
                }
                else
                {
                    ISKUGroup? skuGroup = SKUGroupList.Find(p => p.Code == item.ItemCriteriaSelected);
                    ISKUGroupType? skuGroupType = SKUGroupTypeList.Find(p => p.UID == skuGroup?.SKUGroupTypeUID);

                    qPSSchemeProducts.SKUGroupType = skuGroupType?.Name;
                    qPSSchemeProducts.SKUGroupTypeCode = skuGroupType?.Code;
                    qPSSchemeProducts.SelectedValueCode = skuGroup?.Name;
                    qPSSchemeProducts.SelectedValue = skuGroup?.Code;
                }

                if (!SchemeProducts.Any(p =>
                        qPSSchemeProducts.SKUGroupTypeCode == p.SKUGroupTypeCode &&
                        qPSSchemeProducts.SelectedValue == p.SelectedValue))
                {
                    SchemeProducts.Add(qPSSchemeProducts);
                }
            });


            foreach (PromoOrderItemView promoOrderItemView in PromoMasterView.PromoOrderItemViewList)
            {
                if (promoOrderView.UID == promoOrderItemView.PromoOrderUID)
                {
                }
            }
        }

        foreach (PromoOrderView promoOrderView1 in PromoMasterView.PromoOrderViewList)
        {
            PromoOffer? promoOffer =
                PromoMasterView.PromoOfferViewList?.Find(p => p.PromoOrderUID == promoOrderView1.UID);
            PromoConditionView? promoOrderCondition =
                PromoMasterView.PromoConditionViewList!.Find(p => p.ReferenceUID == promoOrderView1.UID);
            PromoConditionView? promoOfferCondition =
                PromoMasterView.PromoConditionViewList!.Find(p => p.ReferenceUID == promoOffer?.UID);
            if (promoOrderCondition != null && promoOffer != null)
            {
                ISchemeSlab schemeSlab = new SchemeSlab()
                {
                    PromoOrderUID = promoOrderView1.UID,
                    Maximum = promoOrderCondition.Max,
                    Minimum = promoOrderCondition.Min,
                    OfferValue = promoOfferCondition!.Min,
                    OfferType = promoOfferCondition!.ConditionType,
                    IsFOCType = "FOC".Equals(promoOfferCondition?.ConditionType),
                    Id = promoOrderView1.Id,
                };
                if (schemeSlab.IsFOCType)
                {
                    PromoOfferItemView? promoOfferItemView =
                        PromoMasterView.PromoOfferItemViewList?.Find(p => p.PromoOfferUID == promoOffer.UID);
                    if (promoOfferItemView != null)
                    {
                        schemeSlab.OfferItemUID = promoOfferItemView.ItemCriteriaSelected;
                        schemeSlab.OfferItem = skus?.Find(p => p.UID == promoOfferItemView.ItemCriteriaSelected)?.Name;
                    }
                }

                SchemeSlabs.Add(schemeSlab);
            }
        }

        if (PromoMasterView != null && PromoMasterView.PromoOfferItemViewList == null)
        {
            PromoMasterView.PromoOfferItemViewList = [];
        }
    }

    protected void SetEditModeForSlabTypePromotion()
    {
        string? selectedGroupTypeUID = string.Empty;
        SchemeSlabs = new();
        PromoOrderView? promoOrderView = PromoMasterView.PromoOrderViewList.FirstOrDefault();
        if (promoOrderView == null)
        {
            return;
        }

        if (PromoMasterView.PromoConditionViewList != null && PromoMasterView.PromoConditionViewList.Count > 0)
        {
            string? selectedOrderTypeCode = PromoMasterView.PromoConditionViewList
                .Find(p => p.ReferenceUID == promoOrderView.UID)?.ConditionType;
            foreach (var item in OrderTypeDDL)
            {
                if (selectedOrderTypeCode == item.Code)
                {
                    item.IsSelected = true;
                    SelectedOrderType = item;
                }
            }
        }

        if (PromoMasterView.PromoOrderItemViewList != null && SelectedOrderType != null)
        {
            SchemeProducts = new();
            foreach (PromoOrderItemView promoOrderItem in PromoMasterView.PromoOrderItemViewList)
            {
                ISKUGroup? sKUGroup = SKUGroupList.Find(p => p.Code == promoOrderItem.ItemCriteriaSelected);
                if (sKUGroup != null)
                {
                    ISKUGroupType? sKUGroupType = SKUGroupTypeList.Find(p => p.UID == sKUGroup.SKUGroupTypeUID);
                    if (sKUGroupType != null)
                    {
                        IQPSSchemeProducts schemeProducts = new QPSSchemeProducts()
                        {
                            PromoOrderItemUID = promoOrderItem.UID,
                            OrderType = SelectedOrderType.Code ?? "NA",
                            OrderTypeCode = SelectedOrderType.Label ?? "NA",
                            SKUGroupType = sKUGroupType.Name,
                            SKUGroupTypeCode = sKUGroupType.Code,
                            SelectedValue = sKUGroup.Name,
                            SelectedValueCode = sKUGroup.Code,
                        };
                        SchemeProducts.Add(schemeProducts);
                    }
                }
            }
        }

        var schemeSlabs = PromoMasterView?.PromoConditionViewList?
            .Where(promoCondition => promoCondition != null &&
                                     promoCondition.ReferenceType ==
                                     Winit.Shared.Models.Constants.Promotions.PromoOffer)
            .Select(promoCondition =>
            {
                var schemeSlab = _serviceProvider.CreateInstance<ISchemeSlab>();
                schemeSlab.Minimum = promoCondition.Min;
                schemeSlab.Maximum = promoCondition.Max;
                schemeSlab.PromoOfferUID = promoCondition.UID;
                schemeSlab.OfferType = promoCondition.ConditionType;
                schemeSlab.IsFOCType = "FOC".Equals(schemeSlab.OfferType);
                if (schemeSlab.IsFOCType)
                {
                    PromoOfferItemView? promoOfferItem =
                        PromoMasterView?.PromoOfferItemViewList?.Find(p =>
                            p.PromoOfferUID.Equals(schemeSlab.PromoOfferUID));
                    if (promoOfferItem != null)
                    {
                        schemeSlab.OfferItemUID = promoOfferItem.ItemCriteriaSelected;
                        Winit.Modules.SKU.Model.Interfaces.ISKU? sKU = SKUList.Find(p =>
                            p.UID.Equals(promoOfferItem.ItemCriteriaSelected));
                        if (sKU != null)
                        {
                            schemeSlab.OfferItem = sKU.Name;
                        }
                    }
                }

                return schemeSlab;
            })
            .ToList();
        if (schemeSlabs != null)
        {
            SchemeSlabs = schemeSlabs;
        }
    }

    #endregion

    #region

    protected abstract Task GetListItemsByCodes();

    protected abstract Task GetSchemeDetailsBYUID(string UID);

    #endregion
}