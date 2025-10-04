using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.ServiceAndCallRegistration.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.ServiceAndCallRegistration
{
    public partial class CallRegistration : BaseComponentBase
    {
        private Winit.UIComponents.Common.FileUploader.FileUploader? fileUploader { get; set; }
        private string? FilePath { get; set; }
        private string MobileNumberValidationError { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }
        public string ErrorMessage;
        private bool IsMobileNumberValid { get; set; } = true;
        private bool IsPincodeValid { get; set; } = true;
        public bool IsSubmitSuccess { get; set; } = false;
        public bool OnInitialised { get; set; } = false;
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _serviceAndCallRegistrationViewModel.PopulateDropDowns();
            FilePath = FileSysTemplateControles.GetOnBoardImageCheckFolderPath("123123");
            OnInitialised = true;
            HideLoader();
        }
        private void HandleCallRegistrationCustTypeSelection(DropDownEvent eventArgs)
        {
            if (eventArgs.SelectionItems.Any(item => item.IsSelected))
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.CustomerType = _serviceAndCallRegistrationViewModel.CustTypeSelectionList
                                                                                        .Where(item => item.IsSelected == true)
                .Select(item => int.Parse(item.Code))
                                                                                        .SingleOrDefault();
            }
            else
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.CustomerType = null;
            }
        }
        private void HandleCallRegistrationProductCategorySelection(DropDownEvent eventArgs)
        {
            if (eventArgs.SelectionItems.Any(item => item.IsSelected))
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.ProductCategoryCode = _serviceAndCallRegistrationViewModel.ProductCategorySelectionList?
                                                                                                    .Where(item => item.IsSelected == true)
                                                                                                    .Select(item => item.Code)
                                                                                                    .SingleOrDefault() ?? string.Empty;
            }
            else
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.ProductCategoryCode = null;
            }
        }
        private void HandleCallRegistrationBrandCodeSelection(DropDownEvent eventArgs)
        {
            if (eventArgs.SelectionItems.Any(item => item.IsSelected))
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.BrandCode = _serviceAndCallRegistrationViewModel.BrandCodeSelectionList?
                                                                                        .Where(item => item.IsSelected == true)
                                                                                        .Select(item => item.Code)
                                                                                        .SingleOrDefault() ?? string.Empty;
            }
            else
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.BrandCode = null;
            }
        }
        private void HandleCallRegistrationServiceTypeCodeSelection(DropDownEvent eventArgs)
        {
            if (eventArgs.SelectionItems.Any(item => item.IsSelected))
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.ServiceType = _serviceAndCallRegistrationViewModel.ServiceTypeCodeSelectionList
                                                                                        .Where(item => item.IsSelected == true)
                                                                                        .Select(item => int.Parse(item.Code))
                                                                                        .SingleOrDefault();
            }
            else
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.ServiceType = null;
            }
        }
        private void HandleCallRegistrationWarrentyStatusSelection(DropDownEvent eventArgs)
        {
            if (eventArgs.SelectionItems.Any(item => item.IsSelected))
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.WarrantyStatus = _serviceAndCallRegistrationViewModel.WarrentyStatusSelectionList
                                                                                        .Where(item => item.IsSelected == true)
                                                                                        .Select(item => int.Parse(item.Code))
                                                                                        .SingleOrDefault();
            }
            else
            {
                _serviceAndCallRegistrationViewModel.CallRegistrationDetails.WarrantyStatus = null;
            }
        }
        public void OnDateChange(CalenderWrappedData calenderWrappedData)
        {
            _serviceAndCallRegistrationViewModel.CallRegistrationDetails.PurchaseDate = DateTime.Parse(calenderWrappedData.SelectedValue);
        }
        private void GetsavedImagePath(List<IFileSys> ImagePath)
        {
            _serviceAndCallRegistrationViewModel.FileSys = ImagePath;
        }
        private void AfterDeleteImage()
        {

        }
        private async Task SubmitCallRegistrationDetails()
        {
            ShowLoader();
            if (ValidateData())
            {
                await SaveCallRegistrationDetailsAndFileSys();

            }
            HideLoader();
        }
        private void OnMobileNumberInput(ChangeEventArgs e)
        {
            _serviceAndCallRegistrationViewModel.CallRegistrationDetails.MobileNumber = e.Value?.ToString()?.Trim();
            string mobileNumber = _serviceAndCallRegistrationViewModel.CallRegistrationDetails.MobileNumber;
            if (!string.IsNullOrWhiteSpace(mobileNumber))
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^\d{10}$"))
                {
                    if (mobileNumber[0] > '5')
                    {
                        IsMobileNumberValid = true;
                    }
                    else
                    {
                        IsMobileNumberValid = false;
                        MobileNumberValidationError = "Entered mobile number is Invalid .";
                    }
                }
                else
                {
                    IsMobileNumberValid = false;
                    MobileNumberValidationError = "Please enter a valid 10 - digit mobile number.";
                }
            }
            else
            {
                IsMobileNumberValid = false;
            }
            IsMobileNumberValid = System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^\d{10}$") && mobileNumber[0] > '5';
            StateHasChanged();
        }
        private void OnPincodeInput(ChangeEventArgs e)
        {
            _serviceAndCallRegistrationViewModel.CallRegistrationDetails.Pincode = e.Value?.ToString()?.Trim();
            string pincode = _serviceAndCallRegistrationViewModel.CallRegistrationDetails.Pincode;
            IsPincodeValid = !string.IsNullOrWhiteSpace(pincode) &&
                             System.Text.RegularExpressions.Regex.IsMatch(pincode, @"^\d{6}$");
            StateHasChanged();
        }
        private async Task SaveCallRegistrationDetailsAndFileSys()
        {
            await _serviceAndCallRegistrationViewModel.SaveCallRegistrationDetails();
            if (_serviceAndCallRegistrationViewModel.CallRegistrationResponce.Errors.Count == 0)
            {
                ShowAlert("Success", "Call Registration Successful.");
                _navigationManager.NavigateTo("MaintainCallRegistration");

            }
            else
            {
                ShowAlert("Error", string.Join(",", _serviceAndCallRegistrationViewModel.CallRegistrationResponce.Errors));
            }
        }

        private bool ValidateData()
        {
            ErrorMessage = string.Empty;
            if (_serviceAndCallRegistrationViewModel.CallRegistrationDetails.CustomerType == null)
            {
                SetErrorMessage("Customer type");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.BrandCode))
            {
                SetErrorMessage("Brand code");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.ProductCategoryCode))
            {
                SetErrorMessage("Product category name");
            }
            if (_serviceAndCallRegistrationViewModel.CallRegistrationDetails.ServiceType == null)
            {
                SetErrorMessage("Service type code");
            }
            if (_serviceAndCallRegistrationViewModel.CallRegistrationDetails.WarrantyStatus == null)
            {
                SetErrorMessage("Warranty status ");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.CustomerName))
            {
                SetErrorMessage("Customer name");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.ContactPerson))
            {
                SetErrorMessage("Contact person");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.MobileNumber))
            {
                SetErrorMessage("Mobile no.");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.Pincode))
            {
                SetErrorMessage("Pin code");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.EmailID))
            {
                SetErrorMessage("Email id");
            }
            if (string.IsNullOrWhiteSpace(_serviceAndCallRegistrationViewModel.CallRegistrationDetails.Address))
            {
                SetErrorMessage("Address");
            }
            //if (_serviceAndCallRegistrationViewModel.CallRegistrationDetails.PurchaseDate == null|| _serviceAndCallRegistrationViewModel.CallRegistrationDetails.PurchaseDate==DateTime.MinValue)
            //{
            //    SetErrorMessage("Purchase Date");
            //}
            if (!IsMobileNumberValid)
            {
                SetErrorMessage("Invalid mobile number");
            }
            if (!IsPincodeValid)
            {
                SetErrorMessage("Invalid pincode");
            }
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                if (ErrorMessage.EndsWith(","))
                {
                    ErrorMessage = ErrorMessage.TrimEnd(',');
                }
                ErrorMessage = "The following cannot be null or empty " + ErrorMessage;
                ShowAlert("Error", ErrorMessage);
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetErrorMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                ErrorMessage += msg;
            }
            else
            {
                ErrorMessage += msg + ",\n\n ";
            }
        }
    }
}
