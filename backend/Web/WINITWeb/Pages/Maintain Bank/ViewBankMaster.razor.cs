using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Winit.Modules.Bank.Model.Classes;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.Vehicle.BL.Interfaces;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.Maintain_Bank
{
    public partial class ViewBankMaster
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        [Parameter]
        public string? SKUUID { get; set; }
        [Parameter]
        public string? Operation { get; set; }

        public bool IsEditBankDetails { get; set; } = false;
        public bool IsAddNewDetails { get; set; } = false;
        private bool IsIntialized { get; set; }
        private string? validationMessage { get; set; }
        private string Message { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            // _addEditMaintainWarehouseViewModel.ParentUID = "FR001";
            SKUUID = _commonFunctions.GetParameterValueFromURL("SKUUID");
            Operation = _commonFunctions.GetParameterValueFromURL("Operation");
            LoadResources(null, _languageService.SelectedCulture);
            await SetHeaderName();
            if (SKUUID != null)
            {
                await _ViewBankDetailsViewModel.PopulateBankViewDetailsByUID(SKUUID);
                IsEditBankDetails = Operation == "Edit" ? false : true;
            }
            else
            {
                IsAddNewDetails = true;
                _ViewBankDetailsViewModel.ViewBankDetails = new Bank();
            }
            IsIntialized = true;

            StateHasChanged();
        }
      
        
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_bank_master"], IsClickable = true, URL = "ViewBankDetails" });
            if (Operation == "Edit")
            {
                _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["edit_bank_master"], IsClickable = false });
                _IDataService.HeaderText = @Localizer["edit_bank_master"];
            }
            else if (Operation == "View")
            {
                _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["view_bank_master"], IsClickable = false });
                _IDataService.HeaderText = @Localizer["view_bank_master"];
            }
            else
            {
                _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["add_bank_master"], IsClickable = false });
                _IDataService.HeaderText = @Localizer["add_bank_master"];
            }
            _IDataService.HeaderText = @Localizer["view_bank_master"];
            await CallbackService.InvokeAsync(_IDataService);
        }

        public void OnCountrySelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.First();
                _ViewBankDetailsViewModel.ViewBankDetails.CountryUID = selecetedValue.UID;
            }
        }

        private async Task SaveBank()
        {
            if (_ViewBankDetailsViewModel.ViewBankDetails.BankCode == null ||
               string.IsNullOrWhiteSpace(_ViewBankDetailsViewModel.ViewBankDetails.BankName) )
               //string.IsNullOrWhiteSpace(_ViewBankDetailsViewModel.ViewBankDetails.CountryUID))
            {
                validationMessage = ""; //"The following field(s) have invalid value(s): ";
                if (_ViewBankDetailsViewModel.ViewBankDetails.BankCode == null)
                {
                    validationMessage += @Localizer["bank_code,"];
                }
                if (string.IsNullOrWhiteSpace(_ViewBankDetailsViewModel.ViewBankDetails.BankName))
                {
                    validationMessage += @Localizer["bank_name,"];
                }
                //if (string.IsNullOrWhiteSpace(_ViewBankDetailsViewModel.ViewBankDetails.CountryUID))
                //{
                //    validationMessage += "Country, ";
                //}
                validationMessage = validationMessage.TrimEnd(' ', ',');
                if (validationMessage.Length > 0)
                {
                    _tost.Add("", @Localizer["the_following_field(s)_are_mandatory_:"] + validationMessage, Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            else
            {
                if (Operation != "Edit")
                {
                    if (await _ViewBankDetailsViewModel.CreateUpdateBankDetailsData(_ViewBankDetailsViewModel.ViewBankDetails, IsAddNewDetails))
                    {
                        StatusMessage();
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_save.."]);
                    }
                }
                else
                {
                    if(await _ViewBankDetailsViewModel.CreateUpdateBankDetailsData(_ViewBankDetailsViewModel.ViewBankDetails, IsAddNewDetails))
                    {
                        StatusMessage();
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], @Localizer["failed_to_update.."]);
                    }
                }
                _navigationManager.NavigateTo("ViewBankDetails");
                StateHasChanged();
            }
        }
        public void StatusMessage()
        {
            Message = IsAddNewDetails == false ? @Localizer["updated"] : @Localizer["saved"];
            _tost.Add(@Localizer["currency"], @Localizer["currency_details"]  + Message + @Localizer["successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        private void BackBtnClicked()
        {
            _navigationManager.NavigateTo($"ViewBankDetails");
        }
    }
}
