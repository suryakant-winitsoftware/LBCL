using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class AddEarlyPaymentDiscount : BaseComponentBase
    {

        private AccCustomer[] Responsedata { get; set; } = new AccCustomer[0];
        public List<string> PaymentModes { get; set; } = new List<string>();
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public bool IsRendered { get; set; } = false;
        public bool ShowCustomers { get; set; } = false;
        public string PastDates { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string selectedValueText { get; set; } = "Select Customer";
        List<ISelectionItem> customerData { get; set; } = new List<ISelectionItem>();

        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            _earlyPaymentConfigurationViewModel.EarlyPayment = new EarlyPaymentDiscountConfiguration();
            await SetHeaderName();
            PaymentModes.Add("Cash");
            PaymentModes.Add("Cheque");
            PaymentModes.Add("POS");
            PaymentModes.Add("Online");
            IsRendered = true;
            _loadingService.HideLoading();
        }
        private async void OnSelected(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    _earlyPaymentConfigurationViewModel.EarlyPayment.Applicable_Code = dropDownEvent.SelectionItems.First().UID;
                    selectedValueText = dropDownEvent.SelectionItems.First().Code;
                    ShowCustomers = false;
                    StateHasChanged();
                }
            }
            else
            {
                ShowCustomers = false;
            }
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["add_early_payment"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["add_early_payment"];
            await CallbackService.InvokeAsync(_IDataService);
        }
        protected async Task OnGroupTypeChange(ChangeEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e.Value.ToString()))
                {
                    _earlyPaymentConfigurationViewModel.EarlyPayment.Applicable_Type = e.Value.ToString();
                    await _earlyPaymentConfigurationViewModel.GetCustomers(_appUser.SelectedJobPosition.OrgUID);
                    Responsedata = _earlyPaymentConfigurationViewModel.Responsedata;
                    foreach (var item in Responsedata)
                    {
                        SelectionItem type = new SelectionItem()
                        {
                            Code = item.Code,
                            UID = item.UID,
                            Label = item.Name,
                        };
                        customerData.Add(type);
                    }
                    _earlyPaymentConfigurationViewModel.EarlyPayment.Applicable_Type = e.Value.ToString();
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected async Task InsertOrUpdate()
        {
            try
            {
                if (ValidatePropertiesNotNull(_earlyPaymentConfigurationViewModel.EarlyPayment))
                {
                    string result = await _earlyPaymentConfigurationViewModel.AddEarlyPayment(_earlyPaymentConfigurationViewModel.EarlyPayment);
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (result == "2")
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], "Configuration Updated successfully");
                        }
                        else
                        {
                            await _alertService.ShowSuccessAlert(@Localizer["success"], @Localizer["configuration_added_successfully"]);
                        }
                        await NavigateToConfiguration();
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["configuration_failed"]);
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], "Please fill all fields");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static bool ValidatePropertiesNotNull(IEarlyPaymentDiscountConfiguration obj)
        {
            if(string.IsNullOrEmpty(obj.Sales_Org)|| string.IsNullOrEmpty(obj.Applicable_Type) || string.IsNullOrEmpty(obj.Applicable_Code)||
                string.IsNullOrEmpty(obj.Payment_Mode) || obj.Discount_Value == 0 || obj.Advance_Paid_Days == 0 )
            {
                return false;
            }
            return true;
        }
        public async Task NavigateToConfiguration()
        {
            try
            {
                _navigationManager.NavigateTo("configuration");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
