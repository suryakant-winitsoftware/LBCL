using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Web;
using Winit.UIComponents.Web.Store.AddNewStore;
using Winit.UIModels.Web.Store;
using WinIt.BreadCrum.Interfaces;
using WinIt.Pages.Base;
using Winit.Modules.Store.BL.Classes;
using Winit.Modules.Contact.Model.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Winit.Modules.Store.Model.Constants;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;
using Winit.Modules.AwayPeriod.Model.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.UIComponents.Common.LanguageResources.Web;
using Winit.Modules.Contact.Model.Classes;
using ClosedXML;
using Winit.Modules.Store.Model.Interfaces;


namespace WinIt.Pages.Store
{
    public partial class Store
    {
        private ToggleModel toggle = new ToggleModel();
        private StoreRendererModel render = new StoreRendererModel();

        HttpClient http = new HttpClient();
        PagingRequest paging = new PagingRequest();
        List<ListitemModel> listHeaders = new List<ListitemModel>();
        List<ListitemModel> listItems { get; set; }
        private string storeUid { get; set; }
        private bool isNewStore { get; set; } = true;
        private bool isDisabled { get; set; } = true;
        private bool isEdited { get; set; } = false;
        private int ScreenNo { get; set; } = 1;
        private int nextScreeen { get; set; } = 1;
        private bool showPopup = false;
        private bool ShowAllAddress { get; set; }
        public CustomerInformation<IStore>? customerInformation;

        public ContactDetails? contactDetails;
        public ContactPersonDetails? contactPersonDetails;

        public Address? billingAddress;
        public Address? shippingAddress;
        Winit.UIComponents.Web.Store.AddNewStore.OrganisationConfiguration organisationConfiguration;
        List<DataGridColumn> AddressColumns { get; set; }
        public AdditionalInfo? additionalInfo;

        public AwayPeriod? awayperiod;

        public Documents? documents;
        IContact? defaultContact;
        private void ClosePopup()
        {
            showPopup = false;
        }
        protected override async Task OnInitializedAsync()
        {
            ShowAllAddress = true;
            _loadingService.ShowLoading();
            await iStoreBase.PopulateViewModel();
            ////await getListItemsAsync();
            LoadResources(null, _languageService.SelectedCulture);
            //// await getListHeaderAsync();
            //if (storeUid != string.Empty && storeUid != null)
            //{
            //    isNewStore = false;
            //    isDisabled = false;
            //}
            await SetHeaderName();
            render.IsCustomerInformationRendered = true;
            _loadingService.HideLoading();
            //StateHasChanged();
            SetColumnHeaders();
            SetOrganisationConfigurationColumnsHeaders();
        }

        public async Task SaveStore()
        {

        }
        protected void SetColumnHeaders()
        {
            AddressColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header =@Localizer["address_line"] , GetValue = s => ((Winit.Modules.Address.Model.Classes.Address)s).Line1, IsSortable = false, SortField = "Line1" },
            new DataGridColumn { Header =@Localizer["postal_code"] , GetValue = s => ((Winit.Modules.Address.Model.Classes.Address)s).ZipCode, IsSortable = false, SortField = "" },
            new DataGridColumn { Header =@Localizer["email"] , GetValue = s => ((Winit.Modules.Address.Model.Classes.Address)s).Email, IsSortable = false, SortField = "" },

            new DataGridColumn
            {
                Header = @Localizer["actions"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType=ButtonTypes.Text,
                        Text =@Localizer["view/edit_address"],
                        Action = item => ViewEditShippingAddress((Winit.Modules.Address.Model.Classes.Address)item)
                    },

                }
            }
        };
        }
        protected void ViewEditShippingAddress(Winit.Modules.Address.Model.Classes.Address address)
        {
            iStoreBase.ViewEditShippingAddress(address);
            var add = iStoreBase.GetLocationLabelByPrimaryUID(address.LocationUID);
            address.LocationLabel = add.Item1;
            address.LocationJson = add.Item2;

            address.LocationMasters = JsonConvert.DeserializeObject<List<Winit.Modules.Location.Model.Classes.LocationMaster>>(address.LocationJson);
            if (address.LocationMasters != null)
            {
                address.LocationMasters = address.LocationMasters.OrderBy(p => p.Level).ToList();
            }
            iStoreBase.ShippingAddress = address;
            StateHasChanged();
        }
        protected void DeserializeLocationMaster(Winit.Modules.Address.Model.Classes.Address address)
        {

        }
        public async void ChangeViewState(int ComponentPosition)
        {
            //IsCurrentScreenStatus(ScreenNo);
            if (isEdited)
            {
                nextScreeen = ComponentPosition;
                showPopup = true;
            }
            else
            {
                await ChangeScreen(ComponentPosition);
            }


        }
        async Task ChangeScreen(int ComponentPosition)
        {
            _loadingService.ShowLoading();
            showPopup = false;
            isEdited = false;
            ScreenNo = ComponentPosition;
            switch (ComponentPosition)
            {
                case 1:
                    toggle.CustomerInformation = true;
                    break;
                case 2:
                    if (!render.IsContactDetailsRendered)
                    {
                        await ((StorWebViewModelForWeb)iStoreBase).GetContactsByStoreUID();
                    }
                    toggle.ContactDetails = true;
                    render.IsContactDetailsRendered = true;
                    break;
                case 3:
                    if (!render.IsContactPersonDetailsRendered)
                    {
                        await ((StorWebViewModelForWeb)iStoreBase).GetContactsPersonDetails();
                        defaultContact = iStoreBase.ContactPersonList.Find(contact => contact.IsDefault);
                    }
                    toggle.ContactPersonDetails = true;
                    render.IsContactPersonDetailsRendered = true;
                    break;
                case 4:
                    if (!render.IsBillToAddressRendered)
                    {
                        var address = iStoreBase?.Addresses?.Find(p => p.Type?.ToLower() == StoreConstants.BillingAddress.ToLower());
                        if (address != null)
                        {
                            var add = iStoreBase.GetLocationLabelByPrimaryUID(address.LocationUID);
                            address.LocationLabel = add.Item1;
                            address.LocationJson = add.Item2;
                            iStoreBase.BillingAddress = address;
                        }
                    }
                    toggle.BillToAddress = true;
                    render.IsBillToAddressRendered = true;
                    break;
                case 5:
                    if (!render.IsShipToAddressRendered)
                    {
                        iStoreBase.ShippingAddress = new Winit.Modules.Address.Model.Classes.Address();
                    }
                    toggle.ShipToAddress = true;
                    render.IsShipToAddressRendered = true;
                    break;
                case 6:
                    if (!render.IsOrganisationConfigurationRendered)
                    {
                        await ((StorWebViewModelForWeb)iStoreBase).GetOrgConfigurationByStoreUID();
                    }
                    toggle.OrganisationConfiguration = true;
                    render.IsOrganisationConfigurationRendered = true;
                    break;
                case 7:
                    if (!render.IsAwayPeriodRendered)
                    {
                        await ((StorWebViewModelForWeb)iStoreBase).GetAwayPeriodDetails();
                    }
                    toggle.ShowAwayPeriodDetails = true;
                    render.IsAwayPeriodRendered = true;
                    break;
                case 8:
                    toggle.StoreDocuments = true;
                    render.IsDocumentRendered = true;
                    break;
                case 9:
                    if (!render.IsAdditionalRendered)
                    {
                        await ((StorWebViewModelForWeb)iStoreBase).GetAdditionalInfo();
                    }
                    toggle.InvoiceInformation = true;
                    render.IsAdditionalRendered = true;
                    break;
            }
            _loadingService.HideLoading();
            StateHasChanged();

        }
        public void IsCurrentScreenStatus(int currentScreen)
        {

            switch (currentScreen)
            {
                case 1:
                    this.isEdited = customerInformation.IsComponentEdited();
                    break;
                case 2:
                    this.isEdited = customerInformation.IsComponentEdited();
                    break;
                case 3:
                    this.isEdited = contactPersonDetails.IsComponentEdited();
                    break;
                case 4:
                    this.isEdited = billingAddress.IsComponentEdited();
                    break;
                case 5:
                    this.isEdited = shippingAddress.IsComponentEdited();

                    break;
                case 6:
                    this.isEdited = customerInformation.IsComponentEdited();
                    break;
                case 7:
                    this.isEdited = customerInformation.IsComponentEdited();
                    break;

                case 8:
                    this.isEdited = customerInformation.IsComponentEdited();
                    break;
                case 9:
                    this.isEdited = additionalInfo.IsComponentEdited();
                    break;
            }

        }


        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_customer"], IsClickable = true, URL = "ManageCustomers" });
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = isNewStore == true ? @Localizer["add_new_customer"] : @Localizer["edit_customer_details"], IsClickable = false, URL = "Store" });
            _IDataService.HeaderText = isNewStore == true ? @Localizer["add_new_customer"] : @Localizer["edit_customer_details"];
            await CallbackService.InvokeAsync(_IDataService);
        }

        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
        }



        List<DataGridColumn> OrganisationConfigurationColumns { get; set; }

        protected void SetOrganisationConfigurationColumnsHeaders()
        {
            OrganisationConfigurationColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["organization"], GetValue = s => ((StoreCredit)s).OrgLabel},
                new DataGridColumn { Header = @Localizer["payment_type"], GetValue = s => ((StoreCredit)s).PaymentTypeLabel },
                new DataGridColumn { Header = @Localizer["credit_limit"], GetValue = s => ((StoreCredit)s).CreditLimit },
                new DataGridColumn { Header = @Localizer["payment_term"], GetValue = s => ((StoreCredit)s).PaymentTermLabel },
                new DataGridColumn { Header = @Localizer["is_active"], GetValue = s => ((StoreCredit)s).IsActive },
                new DataGridColumn { Header = @Localizer["is_blocked"], GetValue = s => CommonFunctions.GetBooleanValue(((StoreCredit)s).IsActive) },
                new DataGridColumn
                {
                    Header = @Localizer["actions"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType=ButtonTypes.Text,
                            Text = @Localizer["edit_customer"],
                            Action = item => ViewOrEdit((StoreCredit)item)
                        },

                    }
                }
            };
        }
        protected void ViewOrEdit(StoreCredit item)
        {
            iStoreBase.ViewOrEditOrganisationConfig(item);
            StateHasChanged();
        }


        #region Save Or Update data
        protected async Task SaveCustomerInfo()
        {
            _loadingService.ShowLoading();
            (bool isVal, string errorMessage) = customerInformation.IsValidated();
            if (isVal)
            {

                if (iStoreBase.IsNewStore)
                {
                    int retVal = await ((StorWebViewModelForWeb)iStoreBase).SaveCustomerInfo();
                    if (retVal > 0)
                    {
                        var response = await customerInformation?.fileUploader?.MoveFiles();
                        if (response != null && response.IsSuccess)
                        {
                            await ((StorWebViewModelForWeb)iStoreBase).CreateStoreImage(ModifiedStoreImages);
                        }
                        iStoreBase.IsNewStore = false;
                    }
                }
                else
                {
                    await ((StorWebViewModelForWeb)iStoreBase).UpdateCustomerInfo();
                }
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], errorMessage);
            }
            _loadingService.HideLoading();
        }
        protected async Task SaveOrUpdateContactPersonDetails(Winit.Modules.Contact.Model.Interfaces.IContact? ContactPerson)
        {
            _loadingService.ShowLoading();
            int val = 0;
            if (ContactPerson != null)
            {
                if (ContactPerson.IsDefault && defaultContact != null)
                {
                    defaultContact.IsDefault = false;
                    int retval = await ((StorWebViewModelForWeb)iStoreBase).UpdateContactPerson(defaultContact);
                    if (retval > 0)
                    {
                        defaultContact = ContactPerson;
                        foreach (var item in iStoreBase.ContactPersonList)
                        {
                            if (item.IsDefault && defaultContact.UID != item.UID)
                            {
                                item.IsDefault = false;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(ContactPerson.UID))
                {
                    val = await ((StorWebViewModelForWeb)iStoreBase).SaveContactPerson(ContactPerson);
                }
                else
                {
                    val = await ((StorWebViewModelForWeb)iStoreBase).UpdateContactPerson(ContactPerson);
                }
                if (val > 0)
                {
                    iStoreBase.ContactPerson = new Contact();
                }
            }
            _loadingService.HideLoading();
            StateHasChanged();
        }

        protected async Task SaveBillingAddress()
        {
            _loadingService.ShowLoading();
            var isval = billingAddress.IsValidated();
            if (isval.Item1)
            {
                if (string.IsNullOrEmpty(iStoreBase.BillingAddress?.UID))
                {
                    int retCount = await ((StorWebViewModelForWeb)iStoreBase).SaveAddress(iStoreBase.BillingAddress, "Billing");
                }
                else
                {
                    int retCount = await ((StorWebViewModelForWeb)iStoreBase).UpdateAddress(iStoreBase.BillingAddress, "Billing");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(isval.Item2))
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], isval.Item2);
                }
            }
            _loadingService.HideLoading();
        }
        protected async Task SaveShippingAddress()
        {
            _loadingService.ShowLoading();
            int retCount = 0;
            var isval = shippingAddress.IsValidated();
            if (isval.Item1)
            {
                if (string.IsNullOrEmpty(iStoreBase.ShippingAddress?.UID))
                {
                    retCount = await ((StorWebViewModelForWeb)iStoreBase).SaveAddress(iStoreBase.ShippingAddress, "Shipping");
                }
                else
                {
                    retCount = await ((StorWebViewModelForWeb)iStoreBase).UpdateAddress(iStoreBase.ShippingAddress, "Shipping");
                }
                if (retCount > 0)
                {
                    iStoreBase.ShippingAddress = new Winit.Modules.Address.Model.Classes.Address();
                }
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], isval.Item2);
            }
            _loadingService.HideLoading();
        }
        protected async Task SaveOrgConfiguration()
        {
            (bool, string) isval = organisationConfiguration.IsValidate();
            if (isval.Item1)
            {
                await ((StorWebViewModelForWeb)iStoreBase).SaveOrUpdateOrgConfig();
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], isval.Item2);
            }
        }

        protected async Task SaveorUpdateAwayPeriod(IAwayPeriod? awayPeriod)
        {
            _loadingService.ShowLoading();
            iStoreBase.AwayPeriod = awayPeriod;
            if (string.IsNullOrEmpty(awayPeriod?.UID))
            {
                await ((StorWebViewModelForWeb)iStoreBase).saveAwayPeriod();
            }
            else
            {
                await ((StorWebViewModelForWeb)iStoreBase).UpdateAwayPeriod();
            }
            _loadingService.HideLoading();
        }
        protected async Task SaveorUpdateAdditionalInfo()
        {
            _loadingService.ShowLoading();
            await ((StorWebViewModelForWeb)iStoreBase).SaveorUpdateStoreAdditionalinfo();
            _loadingService.HideLoading();
        }

        #endregion
        protected List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> ModifiedStoreImages;
        public void OnFilesUpload(List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> fileSys)
        {
            if (ModifiedStoreImages == null)
            {
                ModifiedStoreImages = fileSys;
            }
            else
            {
                ModifiedStoreImages.AddRange(fileSys);
            }
        }

    }
}
