using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Net;
using System.Resources;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
using AccCollectionPaymentMode = Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode;
using AccPayable = Winit.Modules.CollectionModule.Model.Classes.AccPayable;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Bank.Model.Classes;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class Receipt : BaseComponentBase
    {
        public static NavigationManager navigate { get; set; }
        private List<AccPayable> SelectedInvoiceRecords = new List<AccPayable>();
        private static HashSet<AccPayable> _creditNoteRecords = new HashSet<AccPayable>();
        public static Winit.Modules.Base.BL.ApiService apiService { get; set; }
        private HashSet<AccPayable> selectedItemsConstant = new HashSet<AccPayable>();

        public Heirarchy? heirarchy { get; set; }
        private List<AccElement> ElementsList { get; set; } = new List<AccElement>();

        private string ReceiptNumber { get; set; } = "";

        private bool ShowChequefields = false;
        private bool flag = false;
        private bool ShowPOSfields = false;
        private bool ShowOnlinefields = false;
        private bool ShowCashfields = false;


        private string SessionUserCode { get; set; } = "";

        private string BankUID = "";
        private IBank[] _bank =
    {
    };
        private IBank[] bank { get; set; } = new Bank[0];
        private int _currentPage = 1;
        private int _pageSize = 10;
        private bool _show123 { get; set; } = true;
        private bool _checkbox { get; set; } = true;

        private decimal InvoiceAmount { get; set; } = 0;
        private decimal CreditNoteAmount { get; set; } = 0;

        [Parameter] public string UserReceipt { get; set; } = "";
        [Parameter] public DateTime? _collectedDate { get; set; }
        [Parameter] public List<SelectionItem> SelectedPaymentMode { get; set; }
        [Parameter] public string PaymentMode { get; set; } = "";
        [Parameter] public string PaymentUID { get; set; } = "";

        [Parameter] public bool IsAutoAllocate { get; set; } = false;
        [Parameter] public decimal TotalAmt { get; set; } = 0;
        [Parameter] public decimal CashAmt { get; set; } = 0;
        [Parameter] public List<Winit.Modules.CollectionModule.Model.Classes.AccPayable> ResponseDaystabList { get; set; } = new List<Winit.Modules.CollectionModule.Model.Classes.AccPayable>();
        [Parameter] public List<Winit.Modules.CollectionModule.Model.Classes.AccPayable> ResponseDaystabList1 { get; set; }
        [Parameter] public List<DataGridColumn> Columns { get; set; } = new List<DataGridColumn>();




        [Parameter] public string DocumentType { get; set; } = "";

        private List<DataGridColumn> customerColumns;
        private decimal TotalAmtCopy { get; set; } = 0;

        [Parameter] public Collection _collection { get; set; }

        public bool ReceiptSuccessFlag { get; set; }


        public decimal OnAccountAmount { get; set; }
        public bool IsShow { get; set; } = false;
        MultiCurrencyPopUp _multi { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                //await base.OnInitializedAsync();
                LoadResources(null, _languageService.SelectedCulture);
                ColumnsData();
                await GetBank();
                SessionUserCode = "WINIT";
                apiService = _apiService;
                navigate = NavigationManager;
                string guid = Guid.NewGuid().ToString("N").Substring(0, 8);
                _createPaymentViewModel.collectionAmount = new CollectionAmount();
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }


        public async Task ApplyRate()
        {
            try
            {
                _createPaymentViewModel.collectionAmount.CashAmount *= _collection.Rate;
                _createPaymentViewModel.collectionAmount.ChequeAmount *= _collection.Rate;
                _createPaymentViewModel.collectionAmount.POSAmount *= _collection.Rate;
                _createPaymentViewModel.collectionAmount.OnlineAmount *= _collection.Rate;
                _createPaymentViewModel.collectionAmount.TotalAmount *= _collection.Rate;
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnDatePickerChanged(string newDate, string column)
        {
            switch (column)
            {
                case "Cheque":
                    _createPaymentViewModel.collectionAmount.ChequeTransferDate = Convert.ToDateTime(newDate);
                    break;
                case "POS":
                    _createPaymentViewModel.collectionAmount.POSTransferDate = Convert.ToDateTime(newDate);
                    break;
                case "Online":
                    _createPaymentViewModel.collectionAmount.OnlineTransferDate = Convert.ToDateTime(newDate);
                    break;
            }
        }

        protected override async void OnParametersSet()
        {
            ResponseDaystabList1 = ResponseDaystabList;
            ResponseDaystabList1 = ResponseDaystabList1.OrderBy(p => p.DueDate).ToList();
            ResponseDaystabList = ResponseDaystabList.OrderBy(p => p.DueDate).ToList();
            foreach (var list in SelectedPaymentMode)
            {
                PaymentMode = list.Label;
                if (list.Label == "Cheque" && !list.IsSelected)
                {
                    ShowChequefields = false;
                    _createPaymentViewModel.collectionAmount.ChequeAmount = 0;
                    await AdjustAmountToZero("Cheque");
                }
                if (list.Label == "POS" && !list.IsSelected)
                {
                    ShowPOSfields = false;
                    _createPaymentViewModel.collectionAmount.POSAmount = 0;
                    await AdjustAmountToZero("POS");
                }
                if (list.Label == "Online" && !list.IsSelected)
                {
                    ShowOnlinefields = false;
                    _createPaymentViewModel.collectionAmount.OnlineAmount = 0;
                    await AdjustAmountToZero("Online");
                }
                if (list.Label == "Cheque" && list.IsSelected)
                {
                    ShowChequefields = true;
                }
                if (list.Label == "POS" && list.IsSelected)
                {
                    ShowPOSfields = true;
                }
                if (list.Label == "Online" && list.IsSelected)
                {
                    ShowOnlinefields = true;
                }
                TotalAmtCopy = _createPaymentViewModel.collectionAmount.TotalAmount;
            }

        }
        //everytime when value is changed this methos gets hit
        public async void Changed(HashSet<object> hashSet)
        {
            try
            {
                bool Result = await _createPaymentViewModel.Change(hashSet);
                if (!Result)
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["entered_amount_not_equal_to_balance_of_invoice_or_creditnote"], null, null);
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //adding columns
        List<DataGridColumn> productColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = "DocumentType", GetValue = s => ((AccElement)s).CodeName },
            new DataGridColumn { Header = "DocumentType", GetValue = s => ((AccElement)s).DocumentType },
            new DataGridColumn { Header = "TransactionDate", GetValue = s => ((AccElement)s).TransactionDate },
            new DataGridColumn { Header = "DueDate", GetValue = s => ((AccElement)s).DueDate },

        };
        //getting data after checkbox is checked
        private async void Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {
            try
            {
                bool Result = await _createPaymentViewModel.Product_AfterCheckBoxSelection(hashSet);
                if (!Result)
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["entered_amount_not_equal_to_balance_of_invoice_or_creditnote"], null, null);
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }
        //converting hashSet object into elemental ^|
        public void ColumnsData()
        {
            //adding columns 
            customerColumns = new List<DataGridColumn>
        {
           new DataGridColumn { Header = "Invoice Number", GetValue = s => ((AccPayable)s).ReferenceNumber },
            new DataGridColumn { Header = @Localizer["invoice_date"], GetValue = s => GetFormattedDate(((AccPayable)s).TransactionDate) == null || GetFormattedDate(((AccPayable)s).TransactionDate) =="" ? "NA" : GetFormattedDate(((AccPayable)s).TransactionDate) },
            new DataGridColumn { Header = @Localizer["due_date"], GetValue = s => GetFormattedDate(((AccPayable)s).DueDate) == null || GetFormattedDate(((AccPayable)s).DueDate) =="" ? "NA" : GetFormattedDate(((AccPayable)s).DueDate) },
            new DataGridColumn { Header = "Original Amt", GetValue = s => FormatNumberInIndianStyle(((AccPayable)s).Amount)},
            new DataGridColumn { Header = "Due Amount", GetValue = s => FormatNumberInIndianStyle(((AccPayable)s).BalanceAmount) },
            new DataGridColumn { Header = "Payment Amount", IsTextBox = true },
            new DataGridColumn { Header = "Balance Amount", GetValue = s => FormatNumberInIndianStyle(((AccPayable)s).BalanceAmount) },
            new DataGridColumn{ Header = @Localizer["is_discount_applicable"], GetValue = s => ((AccPayable)s).Discount ? "Yes" : "No" },
            new DataGridColumn{ Header = @Localizer["discountvalue"], GetValue = s => ((AccPayable)s).DiscountValue + "%" },
        };
        }
        public string GetFormattedDate(DateTime? element)
        {
            return CommonFunctions.GetDateTimeInFormat(element);
        }
        private async void Product_OnSort(SortCriteria sortCriteria)
        {
        }
        static string FormatNumberInIndianStyle(decimal number)
        {
            // Create a new NumberFormatInfo object
            NumberFormatInfo nfi = new NumberFormatInfo();

            // Define the grouping for the Indian numbering system
            nfi.NumberGroupSizes = new[] { 3, 2 }; // 3 for the first group, 2 for subsequent groups
            nfi.NumberGroupSeparator = ","; // Use comma as the separator
            nfi.CurrencyGroupSizes = new[] { 3, 2 }; // Same grouping for currency
            nfi.CurrencyGroupSeparator = ","; // Use comma as the separator for currency
            nfi.CurrencySymbol = "₹ "; // Set the rupee symbol

            // Format the number using the custom NumberFormatInfo
            // "{0:C}" formats as currency
            return number.ToString("C2", nfi); // "C2" specifies currency format with 2 decimal places
        }
        protected async Task GetBank()
        {
            try
            {
                await _createPaymentViewModel.GetBank();
                bank = _createPaymentViewModel.ReceiptBank;
                _bank = bank;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<string> HasRequiredRecords()
        {
            try
            {
                var filteredRecords = _createPaymentViewModel._outstandingRecordsAmount.Where(record => record.SourceType.Contains("INVOICE") || record.SourceType.Contains("CREDITNOTE"))
            .ToList();
                if (filteredRecords.Count != 0)
                {
                    var invoices = filteredRecords
                        .Where(record => record.SourceType.Contains("INVOICE"))
                        .ToList();

                    var creditnotes = filteredRecords
                        .Where(record => record.SourceType.Contains("CREDITNOTE"))
                        .ToList();
                    decimal totalInvoiceAmount = invoices.Sum(invoice => invoice.EnteredAmount);
                    decimal totalCreditnoteAmount = creditnotes.Sum(creditnote => creditnote.EnteredAmount);
                    bool result = totalInvoiceAmount == totalCreditnoteAmount;
                    if ((!SelectedPaymentMode.Any(p => p.IsSelected)) && result)
                    {
                        return "HasRecords";
                    }
                    else if (!result)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], "Amount mismatch to adjust Invoice and Creditnote ");
                        _loadingService.HideLoading();
                        return "MisMatch";
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["while_adjusting_invoices_and_creditnotes_deselect_paymentmode."]);
                        _loadingService.HideLoading();
                        return "ModeSelected";
                    }
                }
                else
                {
                    return "NoRecords";
                }
            }
            catch (Exception ex)
            {
                return "Exception";
            }
        }
        //create button clicked
        protected async Task Routing(string ReceiptNumber)
        {
            try
            {
                _loadingService.ShowLoading();
                await ApplyRate();
                #region Adjusting invoice and creditnotes or Zero receipt
                if (_createPaymentViewModel.collectionAmount.TotalAmount == 0 && !_createPaymentViewModel.collectionAmount.Dense_CheckBox)
                {
                    string Result = await HasRequiredRecords();
                    if (Result == "HasRecords")
                    {
                        await CreditOrZeroAmount();
                        return;
                    }
                    if (Result == "ModeSelected" || Result == "MisMatch")
                    {
                        return;
                    }
                }
                #endregion
                bool Conditions = await CheckValidations();
                if (!Conditions)
                {
                    _loadingService.HideLoading();
                    return;
                }
                bool AmountValidation = await CheckAmountVariation();
                if (!AmountValidation)
                {
                    _loadingService.HideLoading();
                    return;
                }
                #region Auto-Allocate Logic

                if (_createPaymentViewModel.collectionAmount.IsAutoAllocate)
                {
                    bool InvValidation = await CheckInvoiceVariation();
                    if (!InvValidation)
                    {
                        _loadingService.HideLoading();
                        return;
                    }
                    foreach (var list in SelectedPaymentMode.Where(p => p.IsSelected))
                    {
                        PaymentMode = list.Label;
                        _createPaymentViewModel.PaymentMode = PaymentMode;

                        bool IsOnAccount = PaymentMode == "Cash" ? _createPaymentViewModel.collectionAmount.CashOnAccount :
                           PaymentMode == "Cheque" ? _createPaymentViewModel.collectionAmount.ChequeOnAccount :
                           PaymentMode == "POS" ? _createPaymentViewModel.collectionAmount.POSOnAccount :
                           PaymentMode == "Online" ? _createPaymentViewModel.collectionAmount.OnlineOnAccount : false;
                        if (IsOnAccount)
                        {
                            _createPaymentViewModel.ReceiptCount++;
                            bool OnAccountCreation = await CheckIfOnAccountOrNot();
                            if (!OnAccountCreation)
                            {
                                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["onaccount_creation_failed"]);
                                _loadingService.HideLoading();
                                return;
                            }
                        }
                    }
                    foreach (var list in SelectedPaymentMode.Where(p => p.IsSelected))
                    {
                        PaymentMode = list.Label;
                        _createPaymentViewModel.PaymentMode = PaymentMode;
                        await UpdateDetails(0, null);
                    }
                    return;
                }
                #endregion

                #region  OnAccount with Receipt Logic
                if (_createPaymentViewModel.SelectedInvoiceRecords.Count > 0 || _createPaymentViewModel._creditNoteRecords.Count != 0)
                {
                    bool InvoiceValidation = await CheckInvoiceVariation();
                    if (!InvoiceValidation)
                    {
                        _loadingService.HideLoading();
                        return;
                    }

                    if (!_createPaymentViewModel.collectionAmount.LockOnAccountReceipt)
                    {
                        foreach (var list in SelectedPaymentMode.Where(p => p.IsSelected))
                        {
                            PaymentMode = list.Label;
                            _createPaymentViewModel.PaymentMode = PaymentMode;

                            bool IsOnAccount = PaymentMode == "Cash" ? _createPaymentViewModel.collectionAmount.CashOnAccount :
                               PaymentMode == "Cheque" ? _createPaymentViewModel.collectionAmount.ChequeOnAccount :
                               PaymentMode == "POS" ? _createPaymentViewModel.collectionAmount.POSOnAccount :
                               PaymentMode == "Online" ? _createPaymentViewModel.collectionAmount.OnlineOnAccount : false;
                            if (IsOnAccount)
                            {
                                _createPaymentViewModel.ReceiptCount++;
                                bool OnAccountCreation = await CheckIfOnAccountOrNot();
                                if (!OnAccountCreation)
                                {
                                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["onaccount_creation_failed"]);
                                    _loadingService.HideLoading();
                                    return;
                                }
                            }
                        }
                        foreach (var list in SelectedPaymentMode.Where(p => p.IsSelected))
                        {
                            PaymentMode = list.Label;
                            _createPaymentViewModel.PaymentMode = PaymentMode;
                            bool IsReceipt = PaymentMode == "Cash" ? _createPaymentViewModel.collectionAmount.CashReceipt :
                                      PaymentMode == "Cheque" ? _createPaymentViewModel.collectionAmount.ChequeReceipt :
                                      PaymentMode == "POS" ? _createPaymentViewModel.collectionAmount.POSReceipt :
                                      PaymentMode == "Online" ? _createPaymentViewModel.collectionAmount.OnlineReceipt : false;
                            if (IsReceipt)
                            {
                                await UpdateDetails(0, null);
                            }
                        }
                    }
                    #endregion
                    #region Only Receipt Logic
                    if (_createPaymentViewModel.collectionAmount.LockReceipt)
                    {
                        foreach (var list in SelectedPaymentMode.Where(p => p.IsSelected))
                        {
                            PaymentMode = list.Label;
                            _createPaymentViewModel.PaymentMode = PaymentMode;
                            await UpdateDetails(0, null);
                        }
                    }
                    #endregion

                    await CreditOrZeroAmount();
                    StateHasChanged();
                }
                else
                {
                    #region OnAccount Logic
                    _loadingService.HideLoading();
                    bool result = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["invoices_are_not_selected._do_you_want_to_create_onaccount?"], @Localizer["yes"], @Localizer["no"]);
                    if (result)
                    {
                        _loadingService.ShowLoading();
                        _createPaymentViewModel.PaymentMode = SelectedPaymentMode.FirstOrDefault(p => p.IsSelected).Label;
                        await CreateOnAccount(_createPaymentViewModel.collectionAmount.TotalAmount);
                        if (!_createPaymentViewModel.OnAccountSuccessFlag)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["onaccount_failed"]);
                            _loadingService.HideLoading();
                            return;
                        }
                        else
                        {
                            await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["onaccount_created"]);
                            _loadingService.HideLoading();
                        }
                        await SuccessNavigation();
                    }
                    else
                    {
                        _loadingService.HideLoading();
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_select_invoices."]);
                        return;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _loadingService.HideLoading();
            }
        }


        public async Task CreditOrZeroAmount()
        {
            try
            {
                foreach (var list in _createPaymentViewModel._creditNoteRecords)
                {
                    PaymentMode = "Cash";
                    _createPaymentViewModel.PaymentMode = "Cash";
                    _createPaymentViewModel.recList = list;
                    await UpdateDetails(list.EnteredAmount, list);
                }
            }
            catch (Exception ex)
            {

            }
        }


        public async Task<bool> CheckIfOnAccountOrNot()
        {
            HttpStatusCode OnAccountResponse = await CreateOnAccount(OnAccountAmount);
            if (OnAccountResponse == HttpStatusCode.Created)
            {
                OnAccountAmount = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        protected async Task<HttpStatusCode> CreateOnAccount(decimal _OnAccountBalance)
        {
            await _createPaymentViewModel.CreateOnAccount(_OnAccountBalance);
            if (_createPaymentViewModel.OnAccountSuccessFlag)
            {
                return HttpStatusCode.Created;
            }
            else
            {
                return HttpStatusCode.NotFound;
            }
        }
        //Create Collection
        protected async Task UpdateDetails(decimal creditAmt, AccPayable CreditNoteRecords)
        {
            try
            {
                await _createPaymentViewModel.UpdateDetails(creditAmt, CreditNoteRecords);
                if (_createPaymentViewModel.IsReceiptCreated)
                {
                    if (_createPaymentViewModel.response.StatusCode == 201)
                    {
                        ReceiptSuccessFlag = true;
                        await CreatedMsgConditions();
                    }
                    if (_createPaymentViewModel.response.StatusCode == 400)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["badrequest"]);
                        ReceiptSuccessFlag = false;
                        await SuccessNavigation();
                    }
                    if (_createPaymentViewModel.response.StatusCode == 500)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["data_missing"]);
                        ReceiptSuccessFlag = false;
                        await SuccessNavigation();
                    }
                    _createPaymentViewModel.collectionListRecords = new List<ICollections>();
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _loadingService.HideLoading();
            }
        }
        //success msgs
        public async Task CreatedMsgConditions()
        {
            if (ReceiptSuccessFlag)
            {
                await _alertService.ShowErrorAlert(@Localizer["success"], @Localizer["receipt(s)_created_successfully"]);
            }
            else
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["creation_failed"]);
            }
            await SuccessNavigation();
        }

        public async Task SuccessNavigation()
        {
            heirarchy.FlagFalse();
            _collection._multi.Reset();
            await _multi.ClearDictionary();
            _loadingService.HideLoading();
            _createPaymentViewModel.OnLoadOnAccount = 0;
            _createPaymentViewModel.collectionAmount.TotalAmount = 0;
            _createPaymentViewModel.IsReceiptCreated = false;
            _createPaymentViewModel.OnAccountSuccessFlag = false;
            _createPaymentViewModel.ReceiptCount = 0;
            _createPaymentViewModel.response.StatusCode = 0;
            _createPaymentViewModel.SelectedInvoiceRecords.Clear();
            _createPaymentViewModel._outstandingRecordsAmount.Clear();
            _NavigationManager.NavigateTo("viewpayments");
        }
        //success msgs

        public async Task<bool> CheckValidations()
        {
            if (string.IsNullOrEmpty(_createPaymentViewModel.CustomerName) || string.IsNullOrEmpty(_createPaymentViewModel.StoreUID))
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], "Please select Customer");
                _loadingService.HideLoading();
                return false;
            }
            if (SelectedPaymentMode.Count(s => s.IsSelected) == 0)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_select_paymentmode"]);
                _loadingService.HideLoading();
                return false;
            }
            if (_createPaymentViewModel.collectionAmount.TotalAmount == 0)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_amount"]);
                _loadingService.HideLoading();
                return false;
            }
            foreach (var payment in SelectedPaymentMode.Where(s => s.IsSelected))
            {
                switch (payment.Label)
                {
                    case "Cash":
                        if (_createPaymentViewModel.collectionAmount.CashAmount == 0)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_cash_amount"]);
                            _loadingService.HideLoading();
                            return false;
                        }
                        break;
                    case "Cheque":
                        if (_createPaymentViewModel.collectionAmount.ChequeAmount == 0)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_cheque_amount"]);
                            _loadingService.HideLoading();
                            return false;
                        }
                        if (await ChequeConditions())
                        {
                            _loadingService.HideLoading();
                            return false;
                        }
                        break;
                    case "POS":
                        if (_createPaymentViewModel.collectionAmount.POSAmount == 0)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_pos_amount"]);
                            _loadingService.HideLoading();
                            return false;
                        }
                        if (await POSConditions())
                        {
                            _loadingService.HideLoading();
                            return false;
                        }
                        break;
                    case "Online":
                        if (_createPaymentViewModel.collectionAmount.OnlineAmount == 0)
                        {
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_online_amount"]);
                            _loadingService.HideLoading();
                            return false;
                        }
                        if (await OnlineConditions())
                        {
                            _loadingService.HideLoading();
                            return false;
                        }
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
        public async Task<bool> CheckAmountVariation()
        {
            try
            {
                if (_createPaymentViewModel.collectionAmount.TotalInputAmount == _createPaymentViewModel.collectionAmount.TotalAmount)
                {
                    return true;
                }
                if (_createPaymentViewModel.collectionAmount.TotalInputAmount != _createPaymentViewModel.collectionAmount.TotalAmount)
                {
                    if (_createPaymentViewModel.collectionAmount.TotalInputAmount > _createPaymentViewModel.collectionAmount.TotalAmount)
                    {
                        if (_createPaymentViewModel.SelectedInvoiceRecords.Count > 0)
                        {
                            _loadingService.HideLoading();
                            bool IsConfirmOnAccount = await _alertService.ShowConfirmationReturnType(@Localizer["confirmation"], @Localizer["as_amount_exceeds_on_account_will_be_created._are_you_sure_to_proceed?"], @Localizer["yes"], @Localizer["no"]);
                            if (!IsConfirmOnAccount)
                            {
                                return false;
                            }
                            else
                            {
                                _loadingService.ShowLoading();
                                decimal TotalAmount = _createPaymentViewModel.collectionAmount.TotalAmount;
                                AdjustAmountsForOnAccount(TotalAmount);
                                return true;
                            }
                        }
                        else
                        {
                            _loadingService.HideLoading();
                            await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_adjust_amount"]);
                            return false;
                        }
                    }
                    if (_createPaymentViewModel.collectionAmount.TotalInputAmount < _createPaymentViewModel.collectionAmount.TotalAmount)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["total_amount_is_more_than_paying_amount"]);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public void AdjustAmountsForOnAccount(decimal TotalAmount)
        {
            try
            {
                _createPaymentViewModel.collectionAmount.LockReceipt = false;
                _createPaymentViewModel.collectionAmount.LockOnAccountReceipt = false;
                if (_createPaymentViewModel.collectionAmount.CashAmount > TotalAmount)
                {
                    OnAccountAmount = _createPaymentViewModel.collectionAmount.CashAmount - TotalAmount;
                    _createPaymentViewModel.collectionAmount.CashAmount -= OnAccountAmount;
                    _createPaymentViewModel.collectionAmount.CashOnAccount = _createPaymentViewModel.collectionAmount.ChequeOnAccount
                        = _createPaymentViewModel.collectionAmount.POSOnAccount = _createPaymentViewModel.collectionAmount.OnlineOnAccount = true;
                    _createPaymentViewModel.collectionAmount.CashReceipt = true;
                    return;
                }
                else
                {
                    TotalAmount -= _createPaymentViewModel.collectionAmount.CashAmount;
                }
                if (_createPaymentViewModel.collectionAmount.ChequeAmount > TotalAmount)
                {
                    OnAccountAmount = _createPaymentViewModel.collectionAmount.ChequeAmount - TotalAmount;
                    _createPaymentViewModel.collectionAmount.ChequeAmount -= OnAccountAmount;
                    _createPaymentViewModel.collectionAmount.ChequeOnAccount = _createPaymentViewModel.collectionAmount.POSOnAccount
                        = _createPaymentViewModel.collectionAmount.OnlineOnAccount = true;
                    _createPaymentViewModel.collectionAmount.CashReceipt = true;
                    _createPaymentViewModel.collectionAmount.LockReceiptCondition = _createPaymentViewModel.collectionAmount.ChequeAmount == 0 ? true : false;
                    _createPaymentViewModel.collectionAmount.ChequeReceipt = _createPaymentViewModel.collectionAmount.ChequeAmount == 0 ? false : true;
                    return;
                }
                else
                {
                    TotalAmount -= _createPaymentViewModel.collectionAmount.ChequeAmount;
                }
                if (_createPaymentViewModel.collectionAmount.POSAmount > TotalAmount)
                {
                    OnAccountAmount = _createPaymentViewModel.collectionAmount.POSAmount - TotalAmount;
                    _createPaymentViewModel.collectionAmount.POSAmount -= OnAccountAmount;
                    _createPaymentViewModel.collectionAmount.POSOnAccount = _createPaymentViewModel.collectionAmount.OnlineOnAccount = true;
                    _createPaymentViewModel.collectionAmount.CashReceipt = _createPaymentViewModel.collectionAmount.ChequeReceipt = true;
                    _createPaymentViewModel.collectionAmount.POSReceipt = _createPaymentViewModel.collectionAmount.POSAmount == 0 ? false : true;
                    _createPaymentViewModel.collectionAmount.LockReceiptCondition = _createPaymentViewModel.collectionAmount.POSAmount == 0 ? true : false;
                    return;
                }
                else
                {
                    TotalAmount -= _createPaymentViewModel.collectionAmount.POSAmount;
                }
                if (_createPaymentViewModel.collectionAmount.OnlineAmount > TotalAmount)
                {
                    OnAccountAmount = _createPaymentViewModel.collectionAmount.OnlineAmount - TotalAmount;
                    _createPaymentViewModel.collectionAmount.OnlineAmount -= OnAccountAmount;
                    _createPaymentViewModel.collectionAmount.OnlineOnAccount = true;
                    _createPaymentViewModel.collectionAmount.CashReceipt = _createPaymentViewModel.collectionAmount.ChequeReceipt =
                        _createPaymentViewModel.collectionAmount.POSReceipt = true;
                    _createPaymentViewModel.collectionAmount.OnlineReceipt = _createPaymentViewModel.collectionAmount.OnlineAmount == 0 ? false : true;
                    _createPaymentViewModel.collectionAmount.LockReceiptCondition = _createPaymentViewModel.collectionAmount.OnlineAmount == 0 ? true : false;
                    return;
                }
                else
                {
                    TotalAmount -= _createPaymentViewModel.collectionAmount.OnlineAmount;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> CheckInvoiceVariation()
        {
            try
            {
                if (_createPaymentViewModel._outstandingRecordsAmount.Any())
                {
                    InvoiceAmount = _createPaymentViewModel._outstandingRecordsAmount.Where(p => p.SourceType.Contains("INVOICE")).Sum(p => p.EnteredAmount);
                    CreditNoteAmount = _createPaymentViewModel._outstandingRecordsAmount.Where(p => p.SourceType.Contains("CREDITNOTE")).Sum(p => p.EnteredAmount);
                    if (InvoiceAmount != (_createPaymentViewModel.collectionAmount.TotalAmount) && CreditNoteAmount == 0)
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["entered_amount_should_be_same_as_total_amount."]);
                        return false;
                    }
                    if (InvoiceAmount - CreditNoteAmount != (_createPaymentViewModel.collectionAmount.TotalAmount))
                    {
                        await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_adjust_invoice_&_creditnote_amount_to_total_amount."]);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_select_invoices."]);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["exception"]);
                return false;
            }
        }
        protected async Task<bool> ChequeConditions()
        {
            try
            {
                if (string.IsNullOrEmpty(_createPaymentViewModel.collectionAmount.ChequeBranchName) || _createPaymentViewModel.collectionAmount.ChequeAmount == 0 || (_createPaymentViewModel.collectionAmount.ChequeBank == "" || _createPaymentViewModel.collectionAmount.ChequeBank == "--Select Bank--") || (_createPaymentViewModel.collectionAmount.ChequeNo == "" || _createPaymentViewModel.collectionAmount.ChequeNo == null))
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_all_cheque_fields."]);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        protected async Task<bool> POSConditions()
        {
            try
            {
                if (string.IsNullOrEmpty(_createPaymentViewModel.collectionAmount.POSBranchName) || _createPaymentViewModel.collectionAmount.POSAmount == 0 || (_createPaymentViewModel.collectionAmount.POSBank == "" || _createPaymentViewModel.collectionAmount.POSBank == "--Select Bank--") || string.IsNullOrEmpty(_createPaymentViewModel.collectionAmount.POSNo))
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_all_pos_fields."]);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        protected async Task<bool> OnlineConditions()
        {
            try
            {
                if (string.IsNullOrEmpty(_createPaymentViewModel.collectionAmount.OnlineBranchName) || _createPaymentViewModel.collectionAmount.OnlineAmount == 0 || (_createPaymentViewModel.collectionAmount.OnlineBank == "" || _createPaymentViewModel.collectionAmount.OnlineBank == "--Select Bank--") || string.IsNullOrEmpty(_createPaymentViewModel.collectionAmount.OnlineNo))
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_fill_all_online_fields."]);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task OpenCloseMultiCurrency(string PaymentMode)
        {
            try
            {
                IsShow = !IsShow;
                if (IsShow)
                {
                    await _multi.OnInit(PaymentMode);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnSubmitAmount(Dictionary<string, decimal> Amount)
        {
            try
            {
                switch (Amount.First().Key)
                {
                    case "Cheque":
                        _createPaymentViewModel.collectionAmount.ChequeAmount = Amount.First().Value;
                        break;
                    case "POS":
                        _createPaymentViewModel.collectionAmount.POSAmount = Amount.First().Value;
                        break;
                    case "Online":
                        _createPaymentViewModel.collectionAmount.OnlineAmount = Amount.First().Value;
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task AdjustAmountToZero(string Mode)
        {
            try
            {
                await _multi.AdjustMultiCurrencyAmount(Mode);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
