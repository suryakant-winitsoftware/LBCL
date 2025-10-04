using Microsoft.AspNetCore.Components;
using System.Globalization;
using Winit.UIComponents.Common.TimeTracker;

namespace WinIt.Pages.Collection.BalanceConfirmation
{
    public partial class BalanceConfirmationApproval
    {
        public bool IsShowTermsAndConditions { get; set; } = false;
        public bool IsShowSuccessPopUp { get; set; } = false;
        public bool EnableGenerateOtp { get; set; } = false;
        public ReverseTimer otpTimer { get; set; }
        public bool TermsAndConditions { get; set; } = false;
        public bool IsOtpSent { get; set; } = false;
        public bool Showtimer { get; set; } = false;
        public string Message { get; set; } = "Send Otp";
        public string OtpMessage { get; set; } = "sent";
        public string Amount { get; set; } = "";
        public string Date { get; set; } = "";
        public string OTP { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            SetHeaderName();
            Amount = GetParameterValueFromURL("Amount");
            Date = GetParameterValueFromURL("GeneratedOn");
            _loadingService.HideLoading();
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService? iDataBreadcrumbService;
        protected void SetHeaderName()
        {
            iDataBreadcrumbService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                HeaderText = "Confirm Balance",
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
        {
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Primary Channel Partner Balance Confirmation", IsClickable = true, URL="balanceconfirmation" },
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = "Confirm Balance", IsClickable = false },
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
        public async Task SendOTP()
        {
            try
            {
                Random random = new Random();
                OTP = random.Next(1000, 10000).ToString("D4");
                if (Message == "Re Generate")
                {
                    OtpMessage = "Re Sent";
                    otpTimer.StartTimer();
                }
                Message = "Re Generate";
                EnableGenerateOtp = true;
                IsOtpSent = true;
                Showtimer = true;
                _ = _balanceConfirmationViewmodel.SendSms(OTP ?? "", _balanceConfirmationViewmodel.ContactDetails.Mobile ?? "");
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnTimerCompleted(bool value)
        {
            try
            {
                EnableGenerateOtp = false;
                Showtimer = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public void ShowTermsAndConditions()
        {
            try
            {
                IsShowTermsAndConditions = true;
            }
            catch (Exception ex)
            {

            }
        }
        public void Close()
        {
            try
            {
                IsShowTermsAndConditions = false;
            }
            catch (Exception ex)
            {

            }
        }
        public void CloseSuccess()
        {
            try
            {
                IsShowSuccessPopUp = false;
                _navigationManager.NavigateTo("balanceconfirmation");
            }
            catch (Exception ex)
            {

            }
        }
        public async Task GetCapturedImage(string ImageCaptured)
        {
            try
            {
                _balanceConfirmationViewmodel.imageSrc = ImageCaptured;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task Submit()
        {
            try
            {
                if (!string.IsNullOrEmpty(_balanceConfirmationViewmodel.BalanceConfirmationDetails.OTPCode) && TermsAndConditions)
                {
                    //_balanceConfirmationViewmodel.BalanceConfirmationDetails.StartDate = Convert.ToDateTime(2024 - 08 - 06);
                    //_balanceConfirmationViewmodel.BalanceConfirmationDetails.EndDate = Convert.ToDateTime(2024 - 08 - 08);
                    _balanceConfirmationViewmodel.BalanceConfirmationDetails.Status = "Balance Approval Completed";
                    _balanceConfirmationViewmodel.BalanceConfirmationDetails.ConfirmationOrDisputeRequestTime = DateTime.Now;
                    _balanceConfirmationViewmodel.BalanceConfirmationDetails.ConfirmationRequestTimeOrDisputeConfirmationTime = DateTime.Now;
                    _balanceConfirmationViewmodel.BalanceConfirmationDetails.RequestByJobPositionUID = _iAppUser.SelectedJobPosition.UID;
                    _balanceConfirmationViewmodel.BalanceConfirmationDetails.RequestByEmpUID = _iAppUser.Emp.UID;
                    if (await _balanceConfirmationViewmodel.UpdateBalanceConfirmation(_balanceConfirmationViewmodel.BalanceConfirmationDetails))
                    {
                        IsShowSuccessPopUp = true;
                    }
                    else
                    {
                        ShowErrorSnackBar("Error", "Balance Confirmation Failed...");
                    }
                }
                else
                {
                    ShowErrorSnackBar("Error", "Please fill all Fields...");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
