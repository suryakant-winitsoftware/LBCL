using Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Resources;
using Winit.Modules.Bank.Model.Classes;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.UIModels.Web.Store;
using static System.Net.WebRequestMethods;

namespace Winit.UIComponents.Web.Store.AddNewStore
{
    public partial class AdditionalInfo : ComponentBase
    {

        private ToggleModel toggle = new ToggleModel();
        [Parameter] public Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo? _StoreAdditionalInfo { get; set; }
        [Parameter] public List<Winit.Modules.ListHeader.Model.Classes.ListItem> ListItems { get; set; }
        bool isLoad { get; set; }
        [Parameter] public EventCallback<string> Response { get; set; }
        [Parameter] public List<ISelectionItem> Route { get; set; } = new List<ISelectionItem>();


        HttpClient http = new HttpClient();
        List<Winit.Modules.ListHeader.Model.Classes.ListItem> listItems { get; set; } = new List<Winit.Modules.ListHeader.Model.Classes.ListItem>();
        private List<ISelectionItem> InvoiceFrequency = new List<ISelectionItem>();
        private List<ISelectionItem> AgingCycle = new List<ISelectionItem>();
        private List<ISelectionItem> InvoiceFormat = new List<ISelectionItem>();
        private List<ISelectionItem> InvoiceDeliverymethod = new List<ISelectionItem>();
        private List<ISelectionItem> PaymentMode = new List<ISelectionItem>();
        private List<ISelectionItem> PriceType = new List<ISelectionItem>();
        private List<ISelectionItem> DeliveryMethod = new List<ISelectionItem>();
        private List<ISelectionItem> Bank = new List<ISelectionItem>();
        

        private string InvoiceFrequencyLabel { get; set; } ="Select Invoice Frequency";
        private string AgingCycleLabel { get; set; } = "Select AgingCycle";
        private string InvoiceFormatLabel { get; set; } = "Select Invoice Format";
        private string InvoiceDeliveryMethodLabel { get; set; } = "Select Invoice Delivery Method";
        private string PaymentModeLabel { get; set; } = "Select Payment Mode";
        private string PriceTypeLabel { get; set; } = "Select Price Type";
        private string DeliveryMethodLabel { get; set; } = "Select Delivery Method";
        private string BankLabel { get; set; } = "Select Bank";
        private string RouteLabel { get; set; } = "Select Route";

        private bool IsBankDisplay { get; set; }
        private bool IsInvoiceFrequencyDisplay { get; set; }
        private bool IsRouteDisplay { get; set; }
        private bool IsDeliveryMethodDisplay { get; set; }
        private bool IsPriceTypeDisplay { get; set; }
        private bool IsAgingCycleDisplay { get; set; }
        private bool IsInvoiceFormatDisplay { get; set; }
        private bool IsInvoiceDeliveryMethod { get; set; }
        private bool IsPaymentModeDisplay { get; set; }

        List<Bank> bank = new();
        List<Route> route = new();
        protected override void OnInitialized()
        {
            foreach (var item in ListItems)
            {
                SelectionItem selection = new SelectionItem()
                {
                    UID = item.UID,
                    Code = item.Code,
                    Label = item.Name,
                };
                if (item.ListHeaderUID == "InvoiceFrequency")
                {
                    selection.IsSelected = _StoreAdditionalInfo?.InvoiceFrequency == item.Code ? true : false;
                    InvoiceFrequency.Add(selection);
                    if (selection.IsSelected)
                    {
                        InvoiceFrequencyLabel = selection.Label;
                    }
                }
                else if (item.ListHeaderUID == "InvoiceFormat")
                {
                    selection.IsSelected = _StoreAdditionalInfo?.InvoiceFormat == item.Code ? true : false;
                    InvoiceFormat.Add(selection);
                    if (selection.IsSelected)
                    {
                        InvoiceFormatLabel = selection.Label;
                    }
                }
                else if (item.ListHeaderUID == "InvoiceDeliveryMethod")
                {
                    selection.IsSelected = _StoreAdditionalInfo?.InvoiceDeliveryMethod == item.Code ? true : false;
                    if (selection.IsSelected)
                    {
                        InvoiceDeliveryMethodLabel = selection.Label;
                    }
                    InvoiceDeliverymethod.Add(selection);
                }
            }
            isLoad = true;
            base.OnInitialized();
        }
       
        public bool isEdited { get; set; } = false;
        public bool IsComponentEdited()
        {
            return isEdited;
        }
        private void ChangeIsEditedOrNot()
        {
            isEdited = true;
        }

        private void ChangeAllowBadReturns()
        {

        }
        
        private void OnSelected(DropDownEvent dropDownEvent, string type)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {

                    ISelectionItem selectionItem = dropDownEvent.SelectionItems.FirstOrDefault();
                    if(selectionItem != null)
                    {
                        SetDropDownSelectedFields(type,selectionItem.Code,selectionItem.Label);
                    }
                }
                else
                {
                    SetDropDownSelectedFields(type, string.Empty);
                }
            }

            IsBankDisplay=false;
            IsInvoiceFrequencyDisplay = false;
            IsRouteDisplay = false;
            IsDeliveryMethodDisplay = false;
            IsPriceTypeDisplay = false;
            IsAgingCycleDisplay = false;
            IsInvoiceFormatDisplay = false;
            IsPaymentModeDisplay = false;
        }

        private void SetDropDownSelectedFields(string type,string value,string Label=null)
        {
            switch (type)
            {
                case "InvoiceFrequency":
                    _StoreAdditionalInfo.InvoiceFrequency = value;
                    InvoiceFrequencyLabel = string.IsNullOrEmpty(Label)?"Select Invoice Frequency":Label;
                    break;
               
                case "InvoiceFormat":
                    _StoreAdditionalInfo.InvoiceFormat=value;
                    InvoiceFormatLabel = string.IsNullOrEmpty(Label) ? "Select Invoice Format" : Label; ;
                    break;
                case "InvoiceDeliveryMethod":
                    _StoreAdditionalInfo.InvoiceDeliveryMethod= value;
                    InvoiceDeliveryMethodLabel = string.IsNullOrEmpty(Label) ? "Select Route" : Label;
                    break;
                case "Route":
                    _StoreAdditionalInfo.DefaultRouteUID= value;
                    RouteLabel = string.IsNullOrEmpty(Label) ? "Select Route" : Label;
                    break;
            }
        }

        protected void Set_StopDelivery(bool isSet)
        {
            _StoreAdditionalInfo.IsStopDelivery=isSet;
            ChangeIsEditedOrNot();
            StateHasChanged();
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
