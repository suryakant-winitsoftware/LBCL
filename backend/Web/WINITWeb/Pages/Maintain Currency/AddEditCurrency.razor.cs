using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Winit.Modules.Currency.Model.Classes;
using Winit.Modules.Currency.Model.Interfaces;
using Winit.Modules.ErrorHandling.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIComponents.Common.Language;


namespace WinIt.Pages.Maintain_Currency
{
    public partial class AddEditCurrency
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }

        [Parameter]
        public string? SKUUID { get; set; }
        [Parameter]
        public string? Operation { get; set; }

        public bool IsEditCurrencyDetails { get; set; } = false;
        public bool IsAddNewDetails { get; set; } = false;
        private bool IsIntialized { get; set; }
        private string? validationMessage { get; set; }
        private string Message { get; set; } = "";
        IDataService dataService = new DataServiceModel()
        {
            BreadcrumList = new List<IBreadCrum>()
      {
        //  new BreadCrumModel(){SlNo=1,Text="Maintain Currecy",URL="MaintainCurrency",IsClickable=true},
         // new BreadCrumModel(){SlNo=1,Text="Maintain Currency"},
      }
        };
        protected override async Task OnInitializedAsync()
        {
            // _addEditMaintainWarehouseViewModel.ParentUID = "FR001";
            SKUUID = _commonFunctions.GetParameterValueFromURL("SKUUID");
            Operation = _commonFunctions.GetParameterValueFromURL("Operation");
            LoadResources(null, _languageService.SelectedCulture);

             await SetHeaderName();
           // dataService.HeaderText = (IsAddNewDetails ? "Add Currency" : IsEditCurrencyDetails ? "Edit Currency" : "Default Text");

            if (SKUUID != null)
            {
                await _imaintainCurrencyViewModel.PopulateCurrencyViewDetailsByUID(SKUUID);
                IsEditCurrencyDetails = Operation == "Edit" ? false : true;
            }
            else
            {
                IsAddNewDetails = true;
                _imaintainCurrencyViewModel.ViewCurrencyDetails = new Currency();
                _imaintainCurrencyViewModel.DigitsSelectionItems[0].IsSelected = true;

            }
            IsIntialized = true;
            StateHasChanged();
        }
       
        public async Task SetHeaderName()
        {
            //_IDataService.BreadcrumList = new();
            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_currency"], IsClickable = true, URL = "MaintainCurrency" });
            //if (Operation == "Edit")
            //{
            //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["edit_currency_details"], IsClickable = false });
            //    _IDataService.HeaderText = @Localizer["edit_currency_details"];
            //}
            //else if (Operation == "View")
            //{
            //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["view_currency_details"], IsClickable = false });
            //    _IDataService.HeaderText = @Localizer["view_currency_details"];
            //}
            //else
            //{
            //    _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["add_currency_details"], IsClickable = false });
            //    _IDataService.HeaderText = @Localizer["add_currency_details"];
            //}
            //_IDataService.HeaderText = @Localizer["view_currency_details"];
            //await CallbackService.InvokeAsync(_IDataService);
            dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_currency"], IsClickable = true, URL = "MaintainCurrency" });

            if (Operation == "Edit")
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = @Localizer["edit_currency_details"], IsClickable = false });
                dataService.HeaderText = @Localizer["edit_currency_details"];
            }
            else if (Operation == "View")
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = @Localizer["view_currency_details"], IsClickable = false });
                dataService.HeaderText = @Localizer["view_currency_details"];
            }
            else
            {
                dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = @Localizer["add_currency_details"], IsClickable = false });
                dataService.HeaderText = @Localizer["add_currency_details"];
            }
        }

        public void OnDigitsSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.First();
                _imaintainCurrencyViewModel.ViewCurrencyDetails.Digits = int.Parse(selecetedValue.UID);
            }
            
        }
       
        private async Task SaveCurrency()
        {
            if (_imaintainCurrencyViewModel.ViewCurrencyDetails.Code == null ||
               string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Name) ||
               string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Symbol) ||
               string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Digits.ToString())||
               string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.FractionName))
            {
                validationMessage = ""; //"The following field(s) have invalid value(s): ";
                if (_imaintainCurrencyViewModel.ViewCurrencyDetails.Code == null)
                {
                    validationMessage += @Localizer["currency_code,"];
                }
                if (string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Name))
                {
                    validationMessage += @Localizer["currency_name,"];
                }
                if (string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Symbol))
                {
                    validationMessage += @Localizer["symbol,"];
                }
                if (string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails.Digits.ToString()))
                {
                    validationMessage += @Localizer["digits,"];
                }
                
                if (string.IsNullOrWhiteSpace(_imaintainCurrencyViewModel.ViewCurrencyDetails?.FractionName))
                {
                    validationMessage += @Localizer["fraction_name,"];
                }
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
                    if (await _imaintainCurrencyViewModel.CreateUpdateCurrencyDetailsData(_imaintainCurrencyViewModel.ViewCurrencyDetails, IsAddNewDetails))
                    {
                        StatusMessage();
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], "Failed to save");
                    }
                }
                else
                {
                    if (await _imaintainCurrencyViewModel.CreateUpdateCurrencyDetailsData(_imaintainCurrencyViewModel.ViewCurrencyDetails, IsAddNewDetails))
                    {
                        StatusMessage();
                    }
                    else
                    {
                        ShowErrorSnackBar(@Localizer["error"], "Failed to update");
                    }

                }
                _navigationManager.NavigateTo("MaintainCurrency");
                StateHasChanged();
               
            }
        }
        public void StatusMessage()
        {
            Message = IsAddNewDetails == false ? "updated" : "saved";
            _tost.Add(@Localizer["currency"], "Currency details" + Message + "successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
        }
        private void BackBtnClicked()
        {
            _navigationManager.NavigateTo($"MaintainCurrency");
        }
    }
}
