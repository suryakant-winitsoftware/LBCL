using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using WINITMobile.Models.TopBar;
using ComponentBase = Microsoft.AspNetCore.Components.ComponentBase;
using Winit.UIComponents.Common.Language;
using System.Globalization;

namespace WINITMobile.Pages.Collection
{
    public partial class CreatePayment 
    {
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
        public string Textbox { get; set; } = "";
        //Tab
        private string selectedTab ="All";
        private string Tab1 = "All";
        private string Tab2 = "OverDue";
        //Tab
        private DateOnly InvDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        private DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        private string searchText { get; set; } = "";
        private decimal InvoiceAmount { get; set; } = 0;
        private decimal Discrepency { get; set; } = 0;
        private int _count { get; set; } = 0;
        private string UID { get; set; } = "";
        private string Name { get; set; } = "";
        private string Code { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private string CustomerCode { get; set; } = "";
        private bool flag { get; set; } = false;
        private decimal TotalDue { get; set; } = 0;
        private decimal OverDue { get; set; } = 0;
        private Dictionary<string, bool> checkboxStates = new Dictionary<string, bool>();
        private List<IAccPayable> searchList { get; set; } = new List<IAccPayable>();
        private List<IAccPayable> filteredList { get; set; } = new List<IAccPayable>();
        private List<IAccPayable> emptyList { get; set; } = new List<IAccPayable>();
        private List<IAccPayable> searchListCopy { get; set; } = new List<IAccPayable>();
        private List<IAccPayable> SelectedItems { get; set; } = new List<IAccPayable>();
        private List<IAccPayable> PendingPopUpData { get; set; } = new List<IAccPayable>();
        private List<IAccCollectionPaymentMode> ShowPendingRecordsPopUpData { get; set; } = new List<IAccCollectionPaymentMode>();
        public IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _populateList { get; set; }
        public IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _InvoicesList { get; set; } = new List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>();
        public IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _AllInvoicesList { get; set; }
        public IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _OverDueInvoicesList { get; set; }
        private IEarlyPaymentDiscountConfiguration[] EligibleRecords { get; set; } = new IEarlyPaymentDiscountConfiguration[0];
        public List<IAccPayable> OverDueRecords { get; set; } = new List<IAccPayable>();
        public List<IAccPayable> OnTimeRecords { get; set; } = new List<IAccPayable>();
        
        public decimal Discountval { get; set; } = 0;
        public int AdvanceDays { get; set; } = 0;
        public bool ShowPopUp { get; set; } = false;
        public bool OnAccount { get; set; } = false;
        public bool IsShowPendingRecords { get; set; } = false;
        public bool IsShowMultiCurrency { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            _backbuttonhandler.ClearCurrentPage();
            await PopulateData();
            LoadResources(null, _languageService.SelectedCulture);

        }

        public async Task PopulateData()
        {
            try
            {
                _dataManager.SetData("ConsolidatedReceiptNumber",_appUser.SelectedCustomer.Code + "_" + _createPaymentAppViewModel.sixGuidstring());
                _dataManager.SetData("ReceiptNumber", _appUser.SelectedCustomer.Code + "_" + _createPaymentAppViewModel.sixGuidstring());
                _dataManager.SetData("CustomerName", _appUser.SelectedCustomer.Name);
                UID = _appUser.SelectedCustomer.StoreUID;
                Name = "[" + _appUser.SelectedCustomer.Code + "]" + _appUser.SelectedCustomer.Name;
                Code = _appUser.SelectedCustomer.Code;
                _populateList = await _createPaymentAppViewModel.PopulateCollectionPage(UID, "All");
                _InvoicesList = await _createPaymentAppViewModel.GetInvoicesMobile(UID);
                _AllInvoicesList = _InvoicesList;
                await CheckForDiscount();
                TotalDue = _InvoicesList.Where(item => item.SourceType.Contains("INVOICE")).Sum(item => item.BalanceAmount);
                searchListCopy = (List<IAccPayable>)_InvoicesList;
                BindTotalandOverDue("All");
            }
            catch (Exception ex)
            {

            }
        }

        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            paramName = System.Web.HttpUtility.UrlDecode(paramName);
            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }

        public async Task CheckForDiscount()
        {
            EligibleRecords = (await _createPaymentAppViewModel.CheckEligibleForDiscount(UID)).ToArray();
            if (EligibleRecords != null)
            {
                bool IsOverDue = await CheckOverDue(EligibleRecords);
                if (!IsOverDue)
                {
                    await CheckAdvanceDays(EligibleRecords);
                }
            }
        }

        public async Task<bool> CheckOverDue(IEarlyPaymentDiscountConfiguration[] EligibleRecords)
        {
            DateTime today = DateTime.Now;
            OverDueRecords = _InvoicesList.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate < today).ToList();
            OnTimeRecords = _InvoicesList.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate > today).ToList();
            if (OverDueRecords.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task CheckAdvanceDays(IEarlyPaymentDiscountConfiguration[] EligibleRecords)
        {
            AdvanceDays = EligibleRecords[0].Advance_Paid_Days;
            Discountval = EligibleRecords[0].Discount_Value;
            foreach (var list in OnTimeRecords)
            {
                int diff = Convert.ToInt32((DateTime.Now - list.TransactionDate)?.TotalDays);
                if (diff > 0 && diff < AdvanceDays && list.SourceType.Contains("INVOICE"))
                {
                    foreach (var item in _InvoicesList)
                    {
                        if (item.ReferenceNumber == list.ReferenceNumber)
                        {
                            // Update the property as needed
                            item.Discount = true; // Update with the desired new value
                            item.DiscountValue = EligibleRecords[0].Discount_Value;
                        }
                    }
                }
            }
        }

        string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("MMM dd, yyyy") : string.Empty;
        }
        public void BindTotalandOverDue(string Tab)
        {
            TotalDue = _InvoicesList.Where(item => item.SourceType.Contains("INVOICE")).Sum(item => item.BalanceAmount);
            OverDue = _InvoicesList.Where(item => item.SourceType.Contains("INVOICE") && item.DueDate.Value < DateTime.Now).Sum(item => item.BalanceAmount);
        }

        public async void Redirect()
        {
            if (Textbox == "")
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_paying_amount"], null, @Localizer["ok"]);
            }
            else
            {
                if (Discrepency < 0 || InvoiceAmount < 0)
                {
                    if (Discrepency < 0)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["discrepency_can_not_be_negative"], null, @Localizer["ok"]);
                    }
                    if (InvoiceAmount < 0)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["invoice_amount_can_not_be_negative"],null, @Localizer["ok"]);
                    }
                }
                else
                {
                    if (SelectedItems.Count == _count && SelectedItems.Count != 0)
                    {
                        bool IsRecordsPresent = await DisplayPendingRecords();
                        if (IsRecordsPresent)
                        {
                            bool UserResponse = await _alertService.ShowConfirmationReturnType(@Localizer["alert"], @Localizer["there_are_old_invoice_to_collect_/_previous_cheque_/_bank_transfer_payments_are_not_settled.do_you_want_to_continue?"], @Localizer["yes"], @Localizer["no"]);
                            if (UserResponse)
                            {
                                await NavigationToCollectPayment();
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            await NavigationToCollectPayment();
                        }
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_select_records"],null, @Localizer["ok"]);
                    }

                }
            }
        }

        public async Task NavigationToCollectPayment()
        {
            try
            {
                if (Discrepency > 0)
                {
                    bool result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["as_amount_exceeds"]+" "+ "₹" + Discrepency + " " + @Localizer["on_account_will_be_created._are_you_sure_to_proceed?"], @Localizer["yes"], @Localizer["no"]);
                    if (result)
                    {
                        _NavigationManager.NavigateTo("/collectpayment?Discrepency=" + Discrepency + "&Code=" + Code + "&Name=" + Name);
                    }
                }
                else
                {
                    _NavigationManager.NavigateTo("/collectpayment?Code=" + Code + "&Name=" + Name + "&Discrepency=" + Discrepency);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task ShowOnAccountPopUp(EventArgs e)
        {
            try
            {
                if (SelectedItems.Count == 0)
                {
                    ShowPopUp = !ShowPopUp;
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["cannot_process_your_request!"],null, @Localizer["ok"]);
                }
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task Close()
        {
            try
            {
                ShowPopUp = !ShowPopUp;
                OnAccount = false;
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task ClosePendingRecords()
        {
            try
            {
                IsShowPendingRecords = !IsShowPendingRecords;
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task CloseMultiCurrency()
        {
            try
            {
                IsShowMultiCurrency = !IsShowMultiCurrency;
                
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task WinitTextBox_OnSearch(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                try
                {
                    filteredList = _AllInvoicesList.Where(item =>
                            item.SourceType.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                    _InvoicesList = filteredList;
                }
                catch (Exception ex)
                {
                    _InvoicesList = emptyList;
                }
            }
            else
            {
                _InvoicesList = _AllInvoicesList;
            }
            await Task.CompletedTask;
        }
        public async Task ShowPendingRecords(IAccPayable accPayable)
        {
            try
            {
                ShowPendingRecordsPopUpData = await _createPaymentAppViewModel.ShowPendingRecordsInPopUp(accPayable.StoreUID, accPayable.ReferenceNumber);
                IsShowPendingRecords = !IsShowPendingRecords;
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private void ToggleSelection(IAccPayable item, string Name = null)
        {
            if (Name == "CheckAll")
            {
                bool checkAllState = !GetCheckboxState("CheckAll");
                SetCheckboxState("CheckAll", checkAllState);

                foreach (var payableItem in _InvoicesList) // Assuming AllItems is a collection of all IAccPayable items
                {
                    SetCheckboxState(payableItem.UID, checkAllState);
                    if (checkAllState)
                    {
                        if (!SelectedItems.Contains(payableItem))
                        {
                            SelectedItems.Add(payableItem);
                        }
                        payableItem.PayingAmount = payableItem.BalanceAmount;
                        payableItem.IsCheckBox = true;
                    }
                    else
                    {
                        SelectedItems.Remove(payableItem);
                        payableItem.PayingAmount = 0;
                        payableItem.IsCheckBox = false;
                    }
                    OnChangeTextbox(item, "Toggle");
                    onCheckBoxChange(SelectedItems);
                }
            }
            else
            {
                bool currentState = GetCheckboxState(item.UID);
                flag = !currentState;
                SetCheckboxState(item.UID, !currentState);
                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                    item.PayingAmount = 0;
                    item.IsCheckBox = false;
                }
                else
                {
                    SelectedItems.Add(item);
                    item.PayingAmount = item.BalanceAmount;
                    item.IsCheckBox = true;
                }
                OnChangeTextbox(item, "Toggle");
                onCheckBoxChange(SelectedItems);
            }
        }
        public async void OnChangeTextbox(IAccPayable allotment, string value)
        {

            //after toggling if changed then it enters
            if (value != "Toggle")
            {
                allotment.PayingAmount = value != "" ? Convert.ToDecimal(value) : 0;
            }
            //after toggling if changed then it enters

            InvoiceAmount = 0;

            foreach (var list in _InvoicesList)
            {
                if (list.SourceType.Contains("INVOICE"))
                {
                    InvoiceAmount = InvoiceAmount + list.PayingAmount;
                }
                else
                {
                    InvoiceAmount = InvoiceAmount - list.PayingAmount;
                }
            }
            _count = _InvoicesList.Count(item => item.PayingAmount > 0);
            OnChangeTextboxDiscrepency();
            await Task.CompletedTask;
            //await JSRuntime.InvokeVoidAsync("adjustLayoutFunction", allotment.UID);
        }
        public void OnChangeTextboxDiscrepency(string value = "")
        {
            if (value.Length <= 10)
            {
                if(string.IsNullOrEmpty(value))
                {
                    value = Textbox;
                }
                Textbox = value ;
                Discrepency = (value == ""  ? 0 : Convert.ToDecimal(Textbox)) - InvoiceAmount;
            }
            else
            {
                Regex regex = new Regex(@"^[0-9]{1,10}$");
                if (!regex.IsMatch(value))
                {
                    Textbox = "0";
                    Discrepency = InvoiceAmount == 0 ? 0 : Discrepency;
                }
            }
            StateHasChanged();
        }
        public void onCheckBoxChange(List<IAccPayable> testing)
        {
            _dataManager.SetData(nameof(IAccPayable),testing);
        }
        public bool GetCheckboxState(string uid)
        {
            return checkboxStates.TryGetValue(uid, out bool state) ? state : false;
        }

        public void SetCheckboxState(string uid, bool state)
        {
            checkboxStates[uid] = state;
        }

        public async Task OpenMultiCurrency()
        {
            try
            {
                IsShowMultiCurrency = !IsShowMultiCurrency;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task OnAccountAmount(decimal e)
        {
            try
            {
                Discrepency = e;
                OnAccount = true;
                if (Discrepency > 0)
                {
                    bool result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["do_you_want_to_create_on_account?"],@Localizer["yes"],@Localizer["no"]);
                    if (result)
                    {
                        _NavigationManager.NavigateTo("/collectpayment?Discrepency=" + Discrepency + "&Code=" + Code + "&Name=" + Name);
                    }
                    else
                    {
                        Discrepency = 0;
                        OnAccount = false;
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_amount"],null, @Localizer["ok"]);
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task SyncData()
        {
            try
            {
                _loadingService.ShowLoading(@Localizer["syncing_data..."]);
                await Task.Delay(2000);
                foreach (var item in SelectedItems.Where(p => p.IsCheckBox))
                {
                    item.IsCheckBox = false;
                    SetCheckboxState(item.UID, false);
                }
                await PopulateData();
                await MakeZero();
                StateHasChanged();
                _loadingService.HideLoading();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task MakeZero()
        {
            try
            {
                InvoiceAmount = 0;
                Textbox = 0.ToString();
                Discrepency = 0;
                SelectedItems = new();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> DisplayPendingRecords()
        {
            try
            {
                PendingPopUpData = await _createPaymentAppViewModel.GetPendingRecordsFromDB(UID);
                if (PendingPopUpData.Count > 0)
                {
                    bool result = PendingPopUpData.Any(p => SelectedItems.Any(s => s.ReferenceNumber == p.ReferenceNumber));
                    if(result)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }


    }
}
