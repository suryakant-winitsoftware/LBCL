using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Interfaces;


namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class ViewDisputeDetails 
    {
        public bool IsShowPopUp { get; set; } = false;
        public bool IsShowSuccessPopUp { get; set; } = false;
        public bool IsInitialised { get; set; } = false;
        public string UID { get; set; } = "";
        public IBalanceConfirmation DisputeResolvedRecords { get; set; } = new Winit.Modules.CollectionModule.Model.Classes.BalanceConfirmation();

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            UID = GetParameterValueFromURL("UID");
            await _balanceConfirmationViewmodel.GetBalanceConfirmationLineTableDetails(UID);
            IsInitialised = true;
            _loadingService.HideLoading();
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "View Dispute Details",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
                        new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Balance Confirmation Disputes", IsClickable = true, URL = "disputedetails" },
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "View Dispute Balance Confirmation", IsClickable = false },
        }
            };
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
        public async Task DisputeResolved()
        {
            try
            {
                IsShowPopUp = true;
            }
            catch (Exception ex)
            {

            }
        }
        public void Close()
        {
            try
            {
                IsShowPopUp = false;
                IsShowSuccessPopUp = false;
                _navigationManager.NavigateTo("disputedetails");
            }
            catch (Exception ex)
            {

            }
        }
        public void Cancel()
        {
            try
            {
                IsShowPopUp = false;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task Submit()
        {
            try
            {
                if (!string.IsNullOrEmpty(DisputeResolvedRecords.Comments))
                {
                    DisputeResolvedRecords.UID = UID;
                    DisputeResolvedRecords.Status = "Resolved";
                    DisputeResolvedRecords.DisputeConfirmationByJobPositionUID = _iAppUser.SelectedJobPosition.UID;
                    DisputeResolvedRecords.DisputeconfirmationByEmpUID = _iAppUser.Emp.UID;
                    if (await _balanceConfirmationViewmodel.UpdateDisputeResolved(DisputeResolvedRecords))
                    {
                        IsShowPopUp = false;
                        IsShowSuccessPopUp = true;
                    }
                    else
                    {
                        IsShowPopUp = false;
                        ShowErrorSnackBar("Error", "Dispute Resolving Failed...");

                    }
                }
                else
                {
                    //_tost.Add("Contact", "Customer Details Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    ShowErrorSnackBar("Error", "Please fill all Fields...");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void Back()
        {
            try
            {
                _navigationManager.NavigateTo("disputedetails");
            }
            catch (Exception ex)
            {

            }
        }

    }
}
