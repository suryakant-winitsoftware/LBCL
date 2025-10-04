using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.UIComponents.Mobile.Store;
using Winit.UIModels.Mobile.Store;

namespace WINITMobile.Pages.Store
{
    public partial class AddCustomer : ComponentBase
    {
        Toggle toggle = new Toggle();

        Winit.Modules.Address.Model.Interfaces.IAddress billingAddress;

        Winit.Modules.Address.Model.Interfaces.IAddress shippingAddress;

        string UID { get; set; }
        string status { get; set; }
        public bool isNewStore { get; set; }

        public bool showAlert {  get; set; }  
        
        public void ShowHideAlert()
        {
            showAlert = false;
        }

        protected override async Task OnInitializedAsync()
        {
            _backbuttonhandler.ClearCurrentPage();
            UID = GetParameterValueFromURL("UID");
            if (string.IsNullOrEmpty(UID))
            {
                isNewStore=true;
            }
            else
            {
                isNewStore=false;
            }
           

        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
        }
        private async void GetStoreInfoStatusAfterSaved(Winit.UIModels.Mobile.Store.StoreSavedAlert storeSavedAlert)
        {
            if(storeSavedAlert.IsSaved)
            {
                toggle.ContactDetails = true;
                if (isNewStore)
                {
                    this.UID = storeSavedAlert.Value;
                    
                }
                await _alertService.ShowSuccessAlert("Success", storeSavedAlert.Message);

            }
        }
        private void GetStoreAddressStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {
            toggle.BillToAddress = true;
        }
        private void GetBillAddressStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {
            toggle.ShipToAddress = true;
        }
        private void GetShipAddressStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {
            toggle.PaymentDetails = true;
        }
        private void GetPaymentDetailsStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {
            toggle.StoreDocuments = true;
        }
        private void GetDocumentsStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {
            //toggle.StoreDocuments=true;
            NavigationManager.NavigateTo("ManageCustomers");
        }

        private void GetContactsStatus(Winit.Modules.Store.Model.Classes.StoreSavedAlert storeSavedAlert)
        {

            toggle.StoreAddress = true;
        }

    }
}
