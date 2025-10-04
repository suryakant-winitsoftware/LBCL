using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Winit.Modules.CollectionModule.BL.Interfaces;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class ConfirmBalance : ComponentBase
    {
        public string Amount { get; set; } = "";
        public string Date { get; set; } = "";
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            Amount = GetParameterValueFromURL("Amount");
            Date = GetParameterValueFromURL("GeneratedOn");
            _loadingService.HideLoading();
        }
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Balance Confirmation",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Primary Channel Partner Balance Confirmation", IsClickable = true, URL="balanceconfirmation" },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Balance Confirmation", IsClickable = false },
                }
            };
        }
        public string DateFormat(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("dd MMM yyyy, hh:mm tt");
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        public string FormatNumberInIndianStyle(decimal number, string symbol = "₹ ")
        {
            // Create a new NumberFormatInfo object
            NumberFormatInfo nfi = new NumberFormatInfo();

            // Define the grouping for the Indian numbering system
            nfi.NumberGroupSizes = new[] { 3, 2 }; // 3 for the first group, 2 for subsequent groups
            nfi.NumberGroupSeparator = ","; // Use comma as the separator
            nfi.CurrencyGroupSizes = new[] { 3, 2 }; // Same grouping for currency
            nfi.CurrencyGroupSeparator = ","; // Use comma as the separator for currency
            nfi.CurrencySymbol = symbol; // Set the rupee symbol

            // Format the number using the custom NumberFormatInfo
            // "{0:C}" formats as currency
            return number.ToString("C2", nfi); // "C2" specifies currency format with 2 decimal places
        }
        public async Task ConfirmApproval()
        {
            try
            {
                _navigationManager.NavigateTo("balanceconfirmationapproval?Amount=" + Amount + "&GeneratedOn=" + _balanceConfirmationViewmodel.BalanceConfirmationDetails.GeneratedOn);
                await Task.CompletedTask;
            }
            catch(Exception ex)
            {

            }
        }
        public async Task Dispute()
        {
            try
            {
                _navigationManager.NavigateTo("disputebalanceconfirmation?Amount=" + Amount + "&GeneratedOn=" + _balanceConfirmationViewmodel.BalanceConfirmationDetails.GeneratedOn);
                await Task.CompletedTask;
            }
            catch(Exception ex)
            {

            }
        }
    }
}
