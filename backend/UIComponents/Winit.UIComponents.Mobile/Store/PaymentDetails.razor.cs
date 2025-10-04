using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Contact.BL.Classes;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Mobile.Store
{
    public partial class PaymentDetails : ComponentBase
    {
        [Parameter] public EventCallback<Winit.Modules.Store.Model.Classes.StoreSavedAlert> Status { get; set; }
        [Parameter] public string StoreUID { get; set; }
        [Parameter] public bool IsNewStore { get; set; }
        public string SaveOrUpdate { get; set; }    

        private List<Winit.Shared.Models.Common.ISelectionItem> _banks = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _paymentMode = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _priceType = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _invoiceDelivery = new List<ISelectionItem>();
        private List<Winit.Shared.Models.Common.ISelectionItem> _noOfCashCounters = new List<ISelectionItem>();

        private bool showInvoiceDelivery = false;
        private bool showPaymentMode = false;
        private bool showPriceType = false;
        private bool showBanks = false;


        private string InvoiceDeliveryLabel = "Select Invoice Delivery";
        private string PaymentModeLabel = "Select Payment Mode";
        private string PriceTypeLabel = "Select Price Type";
        private string BanksLabel = "Select Bank";


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

              

            }
            else
            {


            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadAfterParameterSet();
            LoadResources(null, _languageService.SelectedCulture);
        }
        protected void LoadResources(object sender, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            ResourceManager resourceManager = new ResourceManager("Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys", typeof(Winit.UIComponents.Common.LanguageResources.Mobile.LanguageKeys).Assembly);
            Localizer = new CustomStringLocalizer<LanguageKeys>(resourceManager, cultureInfo);
        }
        protected  async Task LoadAfterParameterSet()
        {
            if (IsNewStore)
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
            }
            else
            {
                SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Update;
                try
                {
                    payment = await PaymentBl.SelectPaymentByUID(StoreUID);
                    if (payment != null)
                    {
                        if (string.IsNullOrEmpty(payment.UID))
                        {
                            IsNewStore = true;
                            SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                        }
                    }
                    else
                    {
                        payment = new Winit.Modules.Store.Model.Classes.Payment();
                        IsNewStore = true;
                        SaveOrUpdate = Winit.Shared.Models.Constants.ListHeaderType.Save;
                    }
                }
                catch (Exception ex)
                {

                }
            }


            await getListItemsAsync();
            await GetBanks();
        }

        public async void Save()
        {
            Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert = new();
            payment.BankUID = "4cc15685-bb19-4ac5-8daf-86f08f41c1ab";
            payment.CreatedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
            payment.ModifiedBy = "2d893d92-dc1b-5904-934c-621103a900e39";
            payment.ModifiedTime = DateTime.Now;
            payment.ServerModifiedTime = DateTime.Now;
            var res = await PaymentBl.UpdatePaymentForMobile(payment);
            if(IsNewStore)
            await Status.InvokeAsync(storeSavedAlert);
            else
            await Status.InvokeAsync(storeSavedAlert);
        }
       
        private async Task GetBanks()
        {
            try
            {
                PagedResponse<Winit.Modules.Bank.Model.Interfaces.IBank> pagedResponse = await BankBl.GetBankDetails(null, 1, 10000, null, true);
                if (pagedResponse.TotalCount > 0)
                {
                    foreach (var item in pagedResponse.PagedData)
                    {
                        _banks.Add(new SelectionItem()
                        {
                            UID = item.UID,
                            Code = item.BankName,
                            Label = item.BankName,
                            IsSelected = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task getListItemsAsync()
        {
            List<string> codes = new List<string>()
            {
                Winit.Shared.Models.Constants.ListHeaderType.PaymentMode,
                Winit.Shared.Models.Constants.ListHeaderType.PriceType,
                Winit.Shared.Models.Constants.ListHeaderType.InvoiceDelivery,


            };

            try
            {
                PagedResponse<Winit.Modules.ListHeader.Model.Interfaces.IListItem> pagedResponse = await ListHeader.GetListItemsByCodes(codes, true);

                if (pagedResponse != null)
                {
                    if (pagedResponse.TotalCount > 0)
                    {
                        foreach (var item in pagedResponse.PagedData)
                        {


                            switch (item.Code)
                            {
                                case Winit.Shared.Models.Constants.ListHeaderType.PaymentMode:
                                    _paymentMode.Add(new SelectionItem()
                                    {
                                        Code = item.Code,
                                        UID = item.UID,
                                        Label = item.Name,
                                        IsSelected = false,
                                    });
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.PriceType:
                                    _priceType.Add(new SelectionItem()
                                    {
                                        Code = item.Code,
                                        UID = item.UID,
                                        Label = item.Name,
                                        IsSelected = false,
                                    });
                                    break;
                                case Winit.Shared.Models.Constants.ListHeaderType.InvoiceDelivery:
                                    _invoiceDelivery.Add(new SelectionItem()
                                    {
                                        Code = item.Code,
                                        UID = item.UID,
                                        Label = item.Name,
                                        IsSelected = false,
                                    });
                                    break;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        protected void OnSelected(DropDownEvent dropDownEvent, string type)
        {
            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    ChangeSelection(dropDownEvent.SelectionItems.FirstOrDefault(), type);
                }
                else
                {
                    ChangeSelection(new SelectionItem(), type);
                }

            }
            else
            {
                switch (type)
                {
                    case Winit.Shared.Models.Constants.ListHeaderType.PaymentMode:
                        showPaymentMode = false;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.PriceType:
                        showPriceType = false;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.InvoiceDelivery:
                        showInvoiceDelivery = false;
                        break;
                    case Winit.Shared.Models.Constants.ListHeaderType.Bank:
                        showBanks = false;
                        break;

                }
            }
        }

        public void ChangeSelection(Winit.Shared.Models.Common.ISelectionItem item, string Type)
        {

            switch (Type)
            {
                case Winit.Shared.Models.Constants.ListHeaderType.PaymentMode:
                    payment.PaymentMode = item.UID;
                    showPaymentMode = false;
                    PaymentModeLabel = string.IsNullOrEmpty(item.Label) ? PaymentModeLabel : item.Label;
                    break;

                case Winit.Shared.Models.Constants.ListHeaderType.PriceType:
                    payment.PriceType = item.UID;
                    showPriceType = false;
                    PriceTypeLabel = string.IsNullOrEmpty(item.Label) ? PriceTypeLabel : item.Label;
                    break;

                case Winit.Shared.Models.Constants.ListHeaderType.InvoiceDelivery:
                    payment.InvoiceDeliveryMethod = item.UID;
                    showInvoiceDelivery = false;
                    InvoiceDeliveryLabel = string.IsNullOrEmpty(item.Label) ? InvoiceDeliveryLabel : item.Label;
                    break;

                case Winit.Shared.Models.Constants.ListHeaderType.Bank:
                    payment.BankUID = item.UID;
                    showBanks = false;
                    BanksLabel = string.IsNullOrEmpty(item.Label) ? BanksLabel : item.Label;
                    break;
            }
        }

    }
}
