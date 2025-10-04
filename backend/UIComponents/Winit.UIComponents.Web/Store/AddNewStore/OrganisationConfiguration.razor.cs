using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Web.Store;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class OrganisationConfiguration : ComponentBase
    {
        [Parameter] public OrganisationConfigurationModel _OrganisationConfiguration { get; set; }
        [Parameter] public EventCallback<OrganisationConfigurationModel> OnOrganisationSaved { get; set; }
        [Parameter] public EventCallback<StoreGroupData> OnStoreGroupDataSelected { get; set; }
        [Parameter] public string? StoreUID { get; set; }
        [Parameter] public List<ListItem> ListItems { get; set; }
        [Parameter] public List<ISelectionItem> SalesOrgList { get; set; }
        [Parameter] public bool IsNew{ get; set; }
        bool IsSalesOrgVisible { get; set; }

        List<ISelectionItem> PriceList { get; set; }
        string? PriceListLabel { get; set; }
        bool IsPriceListVisible { get; set; }
        List<ISelectionItem> ChainList { get; set; }
        string? ChainLabel { get; set; }
        bool IsChainVisible { get; set; }
        List<ISelectionItem> CustomerGroupList { get; set; }
        bool IsCustomerGroupVisible { get; set; }
        List<ISelectionItem> CustomerClassificationList { get; set; }
        string? CustomerClassificationLabel { get; set; }
        bool IsCustomerClassificationVisible { get; set; }
        List<ISelectionItem> PreferredPaymentMethodList { get; set; }
        bool IsPreferredPaymentMethodVisible { get; set; }
        List<ISelectionItem> PaymentTypeList { get; set; }
        bool IsPaymentTypeVisible { get; set; }
        List<ISelectionItem> PaymentMethodList { get; set; }
        bool IsPaymentMethodVisible { get; set; }
        List<ISelectionItem> PaymentTermList { get; set; }
        bool IsPaymentTermVisible { get; set; }
        bool IsLoad { get; set; } = true;
        List<StoreCredit>? StoreCreditList { get; set; }
        bool ViewLocation { get; set; }
        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(_OrganisationConfiguration.JsonData))
            {
                DeserializeStoregroupData();
            }
            Bind_Drop_DropDowns();
            base.OnInitialized();
        }
        protected void Bind_Drop_DropDowns()
        {
            PaymentMethodList = new();
            ChainList = new();
            CustomerGroupList = new();
            PaymentTermList = new();
            PaymentTypeList = new();
            CustomerClassificationList = new();
            foreach (var item in ListItems)
            {
                SelectionItem selection = new SelectionItem()
                {
                    UID = item.UID,
                    Code = item.Code,
                    Label = item.Name,
                    // IsSelected = additional.InvoiceFrequency == item.UID ? true : false
                };
                if (item.ListHeaderUID == "PaymentMethod")
                {
                    PaymentMethodList.Add(selection);
                }
                else if (item.ListHeaderUID == "CustomerChain")
                {
                    ChainList.Add(selection);
                }
                else if (item.ListHeaderUID == "CustomerGroup")
                {
                    CustomerGroupList.Add(selection);
                }
                else if (item.ListHeaderUID == "PaymentTerm")
                {
                    PaymentTermList.Add(selection);
                }
                else if (item.ListHeaderUID == "PaymentType")
                {
                    PaymentTypeList.Add(selection);
                }
                else if (item.ListHeaderUID == "CustomerClassifciation")
                {
                    CustomerClassificationList.Add(selection);
                }

            }
        }


        public (bool, string) IsValidate()
        {
            string Message = string.Empty;
            bool isval = true;
            if (string.IsNullOrEmpty(_OrganisationConfiguration.SalesOrg))
            {
                Message += @Localizer["sales_org"];
                isval = false;
            }
            if (isval && StoreCreditList != null && StoreCreditList.Count > 0)
            {
                bool isExist = StoreCreditList.Any(p => p.OrgUID == _OrganisationConfiguration.SalesOrg);
                if (isExist)
                {
                    //await _alertService.ShowErrorAlert("Error", "Organisation already exist");
                    _loadingService.HideLoading();
                    return (isval, @Localizer["organisation_already_exist"]);
                }
            }
            if (isval)
            {
                ResetDropdowns();
            }
            return (isval, Message);
        }
        protected void ResetDropdowns()
        {
            DesectedSelectionItem(SalesOrgList);
            DesectedSelectionItem(ChainList);
            DesectedSelectionItem(CustomerGroupList);
            DesectedSelectionItem(CustomerClassificationList);
            DesectedSelectionItem(PaymentTypeList);
            DesectedSelectionItem(PaymentMethodList);
            DesectedSelectionItem(PreferredPaymentMethodList);
            DesectedSelectionItem(PaymentTermList);
        }
        protected void DesectedSelectionItem(List<ISelectionItem> selectionItems)
        {
            if (selectionItems != null)
            {
                foreach (ISelectionItem item in selectionItems)
                {
                    item.IsSelected = false;
                }
            }

        }
        protected void OnDropDownSelected(DropDownEvent dropDownEvent, string dropDownType)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null)
                {
                    SetDropDownSelectedValue(dropDownEvent.SelectionItems, dropDownType);
                }
                else
                {
                    SetDropDownSelectedValue(null, dropDownType);
                }
            }

            IsSalesOrgVisible = false;
            IsPriceListVisible = false;
            IsChainVisible = false;
            IsCustomerGroupVisible = false;
            IsCustomerClassificationVisible = false;
            IsPreferredPaymentMethodVisible = false;
            IsPaymentTypeVisible = false;
            IsPaymentMethodVisible = false;
            IsPaymentTermVisible = false;
        }
        List<StoreGroupDataFromJson>? storeGroupDataFromJsons;
        protected void DeserializeStoregroupData()
        {
            _OrganisationConfiguration.StoreGroupDataFromJsons = JsonConvert.DeserializeObject<List<StoreGroupDataFromJson>>(_OrganisationConfiguration.JsonData);
            if (_OrganisationConfiguration.StoreGroupDataFromJsons != null)
            {
                _OrganisationConfiguration.StoreGroupDataFromJsons = _OrganisationConfiguration.StoreGroupDataFromJsons.OrderBy(p => p.Level).ToList();
            }
        }
        protected void StoreGroupDataSelected(StoreGroupData storeGroupData)
        {
            //SelectedStoreGroupData = storeGroupData;
            ViewLocation = false;
            _OrganisationConfiguration.ChannelSubChannelLabel = storeGroupData == null ? @Localizer["select_store_group"]  : storeGroupData.PrimaryType;
            _OrganisationConfiguration.ChannelSubChannelUID = storeGroupData == null ? @Localizer["select_store_group"] : storeGroupData.PrimaryUID;
            _OrganisationConfiguration.JsonData = storeGroupData?.JsonData;
            if (!string.IsNullOrEmpty(_OrganisationConfiguration.JsonData))
            {
                DeserializeStoregroupData();
            }
            OnStoreGroupDataSelected.InvokeAsync(storeGroupData);
        }
        protected void SetDropDownSelectedValue(List<ISelectionItem>? selectionItems, string dropDownType)
        {
            ISelectionItem? selectionItem = selectionItems?.FirstOrDefault();
            switch (dropDownType)
            {
                case UIModels.Web.Store.DropDownConstant.SalesOrg:
                    _OrganisationConfiguration.SalesOrgLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_sales_org"] ;
                    _OrganisationConfiguration.SalesOrg = selectionItem != null ? selectionItem.UID : string.Empty;
                    break;
                case UIModels.Web.Store.DropDownConstant.Chain:
                    _OrganisationConfiguration.ChainLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_customer_type"];
                    _OrganisationConfiguration.CustomerChain = selectionItem != null ? selectionItem.Code : string.Empty;
                    break;
                case UIModels.Web.Store.DropDownConstant.CustomerGroup:
                    _OrganisationConfiguration.CustomerGroupLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_customer_group"] ;
                    _OrganisationConfiguration.CustomerGroup = selectionItem != null ? selectionItem.Code : string.Empty;
                    break;
                case UIModels.Web.Store.DropDownConstant.CustomerClassification:
                    _OrganisationConfiguration.CustomerClassificationLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_customer_classification"];
                    _OrganisationConfiguration.CustomerClassification = selectionItem != null ? selectionItem.Code : string.Empty;
                    break;
                case UIModels.Web.Store.DropDownConstant.PaymentType:
                    _OrganisationConfiguration.PaymentTypeLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_payment_type"];
                    _OrganisationConfiguration.PaymentType = selectionItem != null ? selectionItem.Code : string.Empty;
                    if (selectionItem != null & selectionItem.Code != null)
                    {
                        if (selectionItem.Code.ToLower().Equals("cash"))
                        {
                            _OrganisationConfiguration.PaymentTermLabel = "0 days";
                            _OrganisationConfiguration.PaymentTerm = "0";
                            _OrganisationConfiguration.IsPaymentTermDisable = true;
                        }
                        else
                        {
                            _OrganisationConfiguration.IsPaymentTermDisable = false;
                        }
                    }
                    break;
                case UIModels.Web.Store.DropDownConstant.PaymentMethod:
                    PreferredPaymentMethodList = new();
                    foreach (ISelectionItem selection in selectionItems)
                    {
                        PreferredPaymentMethodList.Add(new SelectionItem() { UID = selection.UID, Code = selection.Code, Label = selection.Label, IsSelected = false });
                    }
                    _OrganisationConfiguration.PaymentMethodLabel = PreferredPaymentMethodList.Count == 0 ? @Localizer[""] : (selectionItems?.Count == 1 ? selectionItem?.Label : $"{selectionItems?.Count} {@Localizer["items_selected"]}");
                    _OrganisationConfiguration.PaymentMethod = selectionItems != null ? string.Join(", ", selectionItems.Select(o => o.Code)) : string.Empty;
                    break;
                case UIModels.Web.Store.DropDownConstant.PreferredPaymentMethod:
                    _OrganisationConfiguration.PreferredPaymentMethodLabel = selectionItems != null || selectionItems.Count != 0 ? (selectionItems.Count > 1 ? $"{selectionItems.Count} {@Localizer["items_selected"]}" : selectionItem.Label) : @Localizer["select_preferred_payment_method"];
                    _OrganisationConfiguration.PreferredPaymentMethod = selectionItem != null ? selectionItem.Code : string.Empty;
                    //IsPaymentTermIsDisabled = _OrganisationConfiguration.PreferredPaymentMethod == "Cash" ? true : false;
                    break;

                case UIModels.Web.Store.DropDownConstant.PaymentTerm:
                    _OrganisationConfiguration.PaymentTermLabel = selectionItem != null ? selectionItem.Label : @Localizer["select_payment_mode"];
                    _OrganisationConfiguration.PaymentTerm = selectionItem != null ? selectionItem.Code : string.Empty;
                    break;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Web.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }

    }
}
