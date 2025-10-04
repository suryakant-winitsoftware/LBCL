using Microsoft.AspNetCore.Components;
using NPOI.HSSF.Record;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class DisputeBalanceConfirmation 
    {
        public string Amount { get; set; } = "";
        public int LineNumber { get; set; } = 1;
        public string Date { get; set; } = "";
        public List<IBalanceConfirmationLine> AddSchemes { get; set; } = new List<IBalanceConfirmationLine>();
        public int x = 0;
        public bool IsShowPopUp { get; set; } = false;
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            Amount = GetParameterValueFromURL("Amount");
            Date = GetParameterValueFromURL("GeneratedOn");
            BalanceConfirmationLine schemeDetails = new BalanceConfirmationLine();
            schemeDetails.UID = Guid.NewGuid().ToString();
            AddSchemes.Add(new BalanceConfirmationLine { UID = schemeDetails.UID.ToString(), LineNumber = LineNumber });
            _loadingService.HideLoading();
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Dispute Balance Confirmation",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Primary Channel Partner Balance Confirmation", IsClickable = true, URL="balanceconfirmation" },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Balance Confirmation", IsClickable = true, URL = "confirmbalance" },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Dispute Balance Confirmation", IsClickable = false },
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

        public async Task AddScheme()
        {
            try
            {
                ++LineNumber;
                BalanceConfirmationLine schemeDetails = new BalanceConfirmationLine();
                schemeDetails.UID = Guid.NewGuid().ToString();
                AddSchemes.Add(new BalanceConfirmationLine { UID = schemeDetails.UID.ToString(), LineNumber = LineNumber });
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task Cancel()
        {
            _navigationManager.NavigateTo("balanceconfirmation");
        }
        public async Task Close()
        {
            IsShowPopUp = false;
            _navigationManager.NavigateTo("disputedetails");
        }
        public bool AreAllPropertiesFilled<T>(T obj)
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                var value = property.GetValue(obj);
                if (value == null || (value is string && string.IsNullOrEmpty(value as string)))
                {
                    return false;
                }
            }
            return true;
        }
        public async Task ConfirmDispute()
        {
            if (!AddSchemes.Any(s =>
                string.IsNullOrEmpty(s.SchemeName) ||
                s.EligibleAmount == 0 ||
                s.ReceivedAmount ==0 ||
                string.IsNullOrEmpty(s.Description)))
            {
                AddSchemes.ForEach(p =>
                {
                    p.CreatedBy = "ADMIN";
                    p.ModifiedBy = "ADMIN";
                    p.JobPositionUID = _iAppUser.SelectedJobPosition.UID;
                    p.EmpUID = _iAppUser.Emp.UID;
                    p.BalanceConfirmationUID = _balanceConfirmationViewmodel.BalanceConfirmationDetails.UID;
                });
                bool Result = await _balanceConfirmationViewmodel.InsertDisputeRecords(AddSchemes);
                if (Result)
                {
                    IsShowPopUp = true;
                }
                else
                {
                    ShowErrorSnackBar("Error", "Dispute Failed...");
                }
            }
            else
            {
                ShowErrorSnackBar("Error", "Please fill all fields...");
            }
        }
        public async Task DeleteScheme(IBalanceConfirmationLine data)
        {
            try
            {
                var itemToRemove = AddSchemes.FirstOrDefault(s => s.UID == data.UID);
                if (itemToRemove != null)
                {
                    AddSchemes.Remove(itemToRemove);
                    --LineNumber;
                    StateHasChanged(); // Refresh the UI
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
