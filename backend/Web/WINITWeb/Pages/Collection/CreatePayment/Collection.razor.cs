using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.CustomControles;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Payment = Winit.Modules.CollectionModule.Model.Classes.Payment;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class Collection
    {
        private bool Dense_CheckBox = false;
        private bool resetValueOnEmptyText;
        private bool coerceText;
        private bool coerceValue;
        private bool resetValueOnEmpty;
        private bool coerce;
        private bool coerceval;
        private string value1, value2;
        Receipt receipt { get; set; }
        MultiCurrencyPopUp multi { get; set; }
        private AccCustomer[] ResponsedataCopy { get; set; } = new AccCustomer[0];
        private AccCustomer[] ResponsedataFiltered { get; set; } = new AccCustomer[0];
        private AccCustomer[] Responsedata { get; set; } = new AccCustomer[0];
        public Payment[] ResponsedData { get; set; } = new Payment[0];
        public ExchangeRate[] CurrencyDetails { get; set; } = new ExchangeRate[0];
        private Payment[] Respon { get; set; } = new Payment[0];
        private AccDocument[] Response { get; set; } = new AccDocument[0];
        private string PaymentUID { get; set; } = "";
        private string TargetUID { get; set; } = "";

        private string PaymentMode { get; set; } = "";
        private string DocumentType { get; set; } = "";
        private string ReceiptNumber { get; set; } = "";
        private bool ShowGetDetails = false;
        private bool ShowCashfields = false;
        private bool showAlert { get; set; } = false;
        private string AccCollectionUID { get; set; } = "";
        private string SessionUserCode { get; set; } = "";
        public string StoreUID { get; set; } = "";

        public string CurrencyUID { get; set; } = "";
        public decimal Rate { get; set; } = 0;

        private bool logged { get; set; } = false;
        private bool AutoAllocation { get; set; } = false;


        public AccDocument[] states =
        {
        new AccDocument { TargetUID = "INVOICE" },
        };

        //amount table data details

        private AccPayable[] ResponseDaystab { get; set; } = new AccPayable[0];
        private AccPayable[] ResponseDaystable { get; set; } = new AccPayable[0];
        private List<AccPayable> ResponseDaystabList { get; set; } = new List<AccPayable>();
        private AccCollectionPaymentMode[] bank { get; set; } = new AccCollectionPaymentMode[0];
        private List<AccCollectionPaymentMode> bankList { get; set; } = new List<AccCollectionPaymentMode>();
        List<string> ageGroups = new List<string> { "0-30 Days", "30-60 Days", "60-90 Days", "90+ Days" };
        private AccPayable allotment = new AccPayable();
        public string _allRecords { get; set; } = "Total Outstanding Balance";
        private string ReceiptID { get; set; } = "";
        public decimal ConvertAmount { get; set; } = 0;
        private decimal TotalTableAmt { get; set; } = 0;
        [Parameter] public EventCallback OnSelect { get; set; }

        private string _date = DateTime.Now.ToString("yyyy-MM-dd");
        private DateTime? selectedDate { get; set; } = DateTime.Now;
        private string? range { get; set; }
        private long _count { get; set; } = 0;
        private long _count1 { get; set; } = 0;
        private decimal _unsettleAmount { get; set; } = 0;
        private string empty { get; set; } = "";
        private string AlertMessage { get; set; } = "";
        private bool bankPop { get; set; } = false;
        private bool _bankPop { get; set; } = true;
        private string CustomerCode { get; set; } = "";
        private List<SelectionItem> payments { get; set; } = new List<SelectionItem>();
        private List<SelectionItem> paymentModes { get; set; } = new List<SelectionItem>() { new SelectionItem { Label = "Cash" }
                                                                ,new SelectionItem { Label = "Cheque" },new SelectionItem { Label = "POS" },new SelectionItem { Label = "Online" }};

        public static HashSet<object> ItemsChange = new HashSet<object>();
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public MultiSelect? _multi { get; set; }

        private string selectedValue1 { get; set; }

        public bool IsParentInitialised { get; set; }
        private string selectedValueText { get; set; } = "-- Select Customer --";
        List<DataGridColumn> bankColumns { get; set; } = new List<DataGridColumn>();
        List<DataGridColumn> customerColumns { get; set; } = new List<DataGridColumn>();
        List<ISelectionItem> customerData { get; set; } = new List<ISelectionItem>();
        public bool IsShow { get; set; } = false;
        public bool ShowCustomers { get; set; } = false;
        public bool ShowFeilds { get; set; } = false;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Add New Collection",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Add New Collection"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _loadingService.ShowLoading();
                if (_iAppUser.Emp.Code == "DIST001_Admin")
                {
                    ShowFeilds = false;
                }
                else
                {
                    ShowFeilds = true;
                }
                LoadResources(null, _languageService.SelectedCulture);
                range = @Localizer["total_outstanding_balance"];
                selectedDate = _createPaymentViewModel.selectedDate;
                logged = true;
                SessionUserCode = GetParameterValueFromURL("SessionUserCode");
                unsettlecolumns();
                await _createPaymentViewModel.PopulateUISide(CustomerCode);
                Responsedata = _createPaymentViewModel.Responsedata;
                ResponsedataCopy = Responsedata;
                var values = _iAppUser.Emp.Code + "|" + _iAppUser.Emp.AliasName + "|" + _iAppUser.Emp.Code;
                var changeEventArgs = new ChangeEventArgs();
                changeEventArgs.Value = values;
                await Paymentmode(_iAppUser.Emp.Code, changeEventArgs);
                selectedValue1 = _createPaymentViewModel.selectedValue1;
                selectedValueText = _createPaymentViewModel.selectedValueText;
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
                await SetHeaderName();
                StoreUID = Responsedata.First().UID;
                await Initialise();
                await RowClicked(allotment, @Localizer["_allRecords"]);
                await Paymentmode("", null);
                _multi.Reset();
                await CurrencyChange();
                receipt.heirarchy.FlagFalse();
                StateHasChanged();
                _loadingService.HideLoading();
            }
            catch (Exception ex)
            {
                IsParentInitialised = true;
                _loadingService.HideLoading();
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
        private async void OnSelected(DropDownEvent dropDownEvent, string type)
        {

            if (dropDownEvent != null)
            {
                if (dropDownEvent.SelectionMode == SelectionMode.Single && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Count > 0)
                {
                    var values = dropDownEvent.SelectionItems.First().UID + "|" + dropDownEvent.SelectionItems.First().Label + "|" + dropDownEvent.SelectionItems.First().Code;
                    var changeEventArgs = new ChangeEventArgs();
                    changeEventArgs.Value = values;
                    await Paymentmode(dropDownEvent.UID, changeEventArgs);
                    ShowCustomers = false;
                    StateHasChanged();
                }
            }
            else
            {
                ShowCustomers = false;
            }
        }

        private void Search(ChangeEventArgs e)
        {
            if (e.Value.ToString() != null)
            {
                ResponsedataFiltered = ResponsedataCopy.Where(item =>
            item.Code.Contains(e.Value.ToString(), StringComparison.OrdinalIgnoreCase) ||
            item.Name.Contains(e.Value.ToString(), StringComparison.OrdinalIgnoreCase)).ToArray();
                Responsedata = ResponsedataFiltered;

            }
            else
            {
                Responsedata = ResponsedataCopy;
            }
        }

        //if selects auto allocate for OnAccount shows error
        public async Task OpenCloseMultiCurrency(string PaymentMode)
        {
            try
            {
                IsShow = !IsShow;
                if (IsShow)
                {
                    await multi.OnInit(PaymentMode);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task OnSubmitAmount(decimal Amount)
        {
            try
            {
                _createPaymentViewModel.collectionAmount.CashAmount = Amount;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task SetHeaderName()
        {
            _IDataService.BreadcrumList = new();
            _IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["add_new_collection"], IsClickable = false });
            _IDataService.HeaderText = @Localizer["add_new_collection"];
            await CallbackService.InvokeAsync(_IDataService);
        }

        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(NavigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        //to bind payment mode data
        protected async Task Paymentmode(string UID, ChangeEventArgs e)
        {
            try
            {
                await _createPaymentViewModel.Paymentmode(ReceiptNumber, UID, e);
                selectedValueText = _createPaymentViewModel.CustomerName;
                CurrencyDetails = _createPaymentViewModel.CurrencyDetails;
                ResponsedData = _createPaymentViewModel.ResponsedData;
                ResponseDaystab = _createPaymentViewModel.ResponseDaystab;
                StoreUID = _createPaymentViewModel.StoreUID;

                ReceiptNumber = _createPaymentViewModel.ReceiptNumber;
                TotalTableAmt = 0;
                _count1 = 0;
                foreach (var list in ResponseDaystab)
                {
                    if (list.StoreUID != null)
                    {
                        TotalTableAmt += list.Balance;
                        _count1 = list.StoreUID != null || list.StoreUID != "" ? _count1 += Convert.ToInt64(list.Count) : 0;
                    }
                }
                await showUnsettled(false);
                await RowClicked(allotment, @Localizer["_allRecords"]);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                StateHasChanged();
            }
        }

        public async Task CheckCustomerEligibleforDiscount()
        {
            try
            {
                await _createPaymentViewModel.CheckCustomerEligibleforDiscount();
                if (_createPaymentViewModel.ResponseDaystabList != null && _createPaymentViewModel.ResponseDaystabList.Count != 0)
                {
                    ResponseDaystabList = _createPaymentViewModel.ResponseDaystabList;

                }
            }
            catch (Exception ex)
            {

            }
        }



        public async Task Initialise()
        {
            await _createPaymentViewModel.BindInvoiceTable();
            ResponseDaystab = _createPaymentViewModel.ResponseDaystab;
            TotalTableAmt = 0;
            _count1 = 0;
            foreach (var list in ResponseDaystab)
            {
                if (list.StoreUID != null)
                {
                    TotalTableAmt += list.Balance;
                    _count1 = list.StoreUID != null || list.StoreUID != "" ? _count1 += Convert.ToInt64(list.Count) : 0;
                }
            }
            //records of unsettled 
            await showUnsettled(false);
            StateHasChanged();

        }

        //rowclick
        async Task RowClicked(AccPayable timePeriod, string totalRecords)
        {
            try
            {
                if (timePeriod.DelayTime != null)
                {
                    range = timePeriod.DelayTime;
                    _count = timePeriod.Count;
                }
                else
                {
                    range = "Total Outstanding Balance";
                    _count = _count1;
                }
                await _createPaymentViewModel.TableRowClickUI(timePeriod, totalRecords);
                await OnSelect.InvokeAsync();
                ReceiptID = _createPaymentViewModel.ReceiptID;
                await _createPaymentViewModel.ChangeRecords(ReceiptID);
                ResponseDaystable = _createPaymentViewModel.ResponseDaystable;

                ResponseDaystabList = _createPaymentViewModel.ResponseDaystabList;

                await CheckCustomerEligibleforDiscount();
                IsParentInitialised = true;
                StateHasChanged();
                await JSRuntime.InvokeVoidAsync("focustotable", "bottom");
                if (_createPaymentViewModel.collectionAmount.Dense_CheckBox)
                {
                    var args = new ChangeEventArgs { Value = true };
                    await HandleCheckBoxChange(args);
                }

            }
            catch (Exception ex)
            {

            }

        }
        protected async void GetPaymentModes(List<SelectionItem> item)
        {
            try
            {
                if (item.Count > 0)
                {

                    payments = await _createPaymentViewModel.PaymentModeSelectionItem(item);
                    foreach (var list in item)
                    {

                        PaymentMode = await _createPaymentViewModel.PaymentModeSelection(list.Label);
                        if (list.Label == "Cash" && list.IsSelected)
                        {
                            ShowCashfields = true;
                        }
                        if (list.Label == "Cash" && !list.IsSelected)
                        {
                            ShowCashfields = false;
                            _createPaymentViewModel.collectionAmount.CashAmount = 0;
                            await receipt.AdjustAmountToZero("Cash");
                        }
                    }
                }
                else
                {
                    PaymentMode = "";
                    ShowCashfields = false;
                    _createPaymentViewModel.collectionAmount.CashAmount = 0;
                    await receipt.AdjustAmountToZero("Cash");
                }
            }
            catch (Exception ex)
            {

            }
        }

        //to bind document mode data
        protected async Task Documentmode()
        {
            try
            {
                await _createPaymentViewModel.DocumentDataUI(ReceiptNumber);
                Response = _createPaymentViewModel.Response;
                states = Response;
                List<AccDocument> updatedStates = Response.ToList(); // Convert to a List for flexibility
                updatedStates.Insert(0, new AccDocument { TargetUID = "AllInvoices" });
                updatedStates.Insert(0, new AccDocument { TargetUID = "OnAccount" });
                states = updatedStates.ToArray();
            }
            catch (Exception ex)
            {

            }

        }

        //to bind currency data
        protected async Task Currency(string UID, string e)
        {
            try
            {
                if (e != "AllInvoices")
                    TargetUID = e;
                else
                    AccCollectionUID = e;
                ReceiptNumber = ResponsedData.FirstOrDefault(item => "op" != "ji").ReceiptNumber;
                DocumentType = e;
                await _createPaymentViewModel.CurrencyData(ReceiptNumber);
                Respon = _createPaymentViewModel.Respon;
            }
            catch (Exception ex)
            {

            }

        }

        //hierarchical data binding
        protected async Task CurrencyChange()
        {
            try
            {
                await _createPaymentViewModel.CurrencyChange();
                CurrencyUID = _createPaymentViewModel.CurrencyUID;
                Rate = _createPaymentViewModel.Rate;
                ConvertAmount = 0;
                _createPaymentViewModel.collectionAmount.TotalAmount = 0;
            }
            catch (Exception ex)
            {

            }

        }

        public async void ConvertingToDefaultAmount(string e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e))
                {
                    _createPaymentViewModel.collectionAmount.TotalAmount = Convert.ToDecimal(e);
                    ConvertAmount = _createPaymentViewModel.collectionAmount.TotalAmount * Rate;
                    var args = new ChangeEventArgs();
                    args.Value = _createPaymentViewModel.collectionAmount.IsAutoAllocate;
                    _createPaymentViewModel.collectionAmount.CheckBoxState = _createPaymentViewModel.collectionAmount.IsAutoAllocate;
                    if (_createPaymentViewModel.collectionAmount.CheckBoxState)
                    {
                        await HandleCheckBoxChange(args);
                    }
                }
                else
                {
                    ConvertAmount = 0;
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void unsettlecolumns()
        {
            customerColumns = new List<DataGridColumn>
        {
            new DataGridColumn { Header = @Localizer["targetuid"], GetValue = s => ((AccCollectionAllotment)s).TargetUID },
            new DataGridColumn { Header = @Localizer["total_amount"], GetValue = s => ((AccCollectionAllotment)s).Amount},
            new DataGridColumn { Header = @Localizer["balance"], GetValue = s => ((AccCollectionAllotment)s).Remaining },
            new DataGridColumn { Header = @Localizer["trxdate"], GetValue = s => ((AccCollectionAllotment)s).TrxDate },
            new DataGridColumn { Header = @Localizer["due_date"], GetValue = s => ((AccCollectionAllotment)s).DueDate },
        };

            bankColumns = new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header =@Localizer["receipt_number"],
                IsButtonColumn = true,
                GetValue = s => ((AccCollectionPaymentMode)s).ReceiptNumber,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                       GetValue = s => ((AccCollectionPaymentMode)s).ReceiptNumber,
                        ButtonType=ButtonTypes.Url,
                        Action = async item => await HandleAction2_Product((AccCollectionPaymentMode)item)
                    },
                }
            },
            new DataGridColumn { Header = @Localizer["bank_name"], GetValue = s => ((AccCollectionPaymentMode)s).BankUID },
            new DataGridColumn { Header = @Localizer["branch_name"], GetValue = s => ((AccCollectionPaymentMode)s).Branch},
            new DataGridColumn { Header = @Localizer["payment_type"], GetValue = s => ((AccCollectionPaymentMode)s).Category },
            new DataGridColumn { Header = @Localizer["amount"], GetValue = s => ((AccCollectionPaymentMode)s).DefaultCurrencyAmount },
            new DataGridColumn { Header = @Localizer["status"], GetValue = s => ((AccCollectionPaymentMode)s).Status },
            new DataGridColumn { Header = @Localizer["date"], GetValue = s => ((AccCollectionPaymentMode)s).ChequeDate },
            new DataGridColumn { Header = @Localizer["ref_no"], GetValue = s => ((AccCollectionPaymentMode)s).ChequeNo },
        };
        }

        //public Task<string> ReceiptNumberTextProvider(string Receipt)
        //{
        //    if (Receipt == "")
        //    {

        //    }
        //    else
        //    {
        //        return Receipt;
        //    }
        //}

        private async Task HandleAction2_Product(AccCollectionPaymentMode item)
        {
            NavigationManager.NavigateTo("settlement");
        }
        protected async Task showUnsettled(bool _bankPop)
        {
            try
            {
                await _createPaymentViewModel.UnSettlePopUpRecords(_bankPop);
                bank = _createPaymentViewModel.bank;
                _unsettleAmount = 0;
                bankList = bank.ToList();
                foreach (var list in bankList)
                {
                    _unsettleAmount += Convert.ToDecimal(list.DefaultCurrencyAmount);
                }
                bankPop = _bankPop;
            }
            catch (Exception ex)
            {
                bankPop = _bankPop;
            }

        }
        protected void Close()
        {
            bankPop = false;
        }

        private async Task HandleCheckBoxChange(ChangeEventArgs args)
        {
            _createPaymentViewModel.collectionAmount.IsAutoAllocate = Convert.ToBoolean(args.Value);
            _createPaymentViewModel.collectionAmount.CheckBoxState = Convert.ToBoolean(args.Value);
            bool alert = receipt.heirarchy.GetCall(_createPaymentViewModel.collectionAmount.TotalAmount, Convert.ToBoolean(args.Value.ToString()));
            ItemsChange = receipt.heirarchy.SelectedItemsStatic;

            receipt.Changed(ItemsChange);

            if (!alert && Convert.ToBoolean(args.Value.ToString()))
            {
                await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["please_enter_amount"]);
            }
            StateHasChanged();

        }

    }
}
