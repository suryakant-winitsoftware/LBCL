using Microsoft.AspNetCore.Components;
using Nest;
using NPOI.SS.Formula.Functions;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Printing.BL.Classes;
using Winit.Modules.Printing.BL.Interfaces;
using Winit.Modules.Printing.Model.Enum;
using Winit.Modules.Store.Model.Interfaces;
using WINITMobile.Models.TopBar;
using Winit.Modules.Printing.BL.Classes.CollectionOrder;
namespace WINITMobile.Pages.Collection
{
    public partial class PaymentSummary
    {
        private DateTime FromDate { get; set; } = DateTime.Now;
        private DateTime ToDate { get; set; } = DateTime.Now;
        private string searchText { get; set; }
        private string Name { get; set; } = "";
        [Parameter] public List<string> tabItems { get; set; } = new List<string>();
        private string selectedTab { get; set; } = "Cash";
        [CascadingParameter] public EventCallback<Models.TopBar.MainButtons> Btnname { get; set; }
        private List<IAccCollection> CollectionList { get; set; }
        private List<IAccCollection> CollectionListCopy { get; set; }
        public List<string> Status { get; set; } = new List<string> { "Collected", "OnAccount" };
        private List<IAccCollectionAllotment> AllotmentList { get; set; }
        private List<IAccCollectionPaymentMode> CPODetails { get; set; }
        private List<IExchangeRate> ViewMultiCurrencyDetails { get; set; } = new List<IExchangeRate>();
        private List<IAccCollectionCurrencyDetails> ConvertMultiCurrencyDetails { get; set; }
        public string FromPlaceHolder { get; set; } = "";
        public string ToPlaceHolder { get; set; } = "";
        private decimal CashAmount { get; set; } = 0;
        private decimal ChequeAmount { get; set; } = 0;
        private decimal POSAmount { get; set; } = 0;
        private decimal CreditAmount { get; set; } = 0;
        private decimal OnlineAmount { get; set; } = 0;
        private decimal TotalAmount { get; set; } = 0;
        public MultiCurrencyPopUp _multi { get; set; }
        public List<ICollectionPrint> CollectionOrderPaymentDetails { get; set; } = new List<ICollectionPrint>();
        private static Dictionary<string, string> storeData { get; set; } = new Dictionary<string, string>();
        public List<IStore> CustomerName { get; set; } = new List<IStore>();

        public bool IsShowMultiCurrency { get; set; } = false;




        protected override async Task OnInitializedAsync()
        {
            _backbuttonhandler.ClearCurrentPage();
            _backbuttonhandler.SetCurrentPage(this);
            tabItems.Add("Cash");
            tabItems.Add("Cheque");
            tabItems.Add("POS");
            tabItems.Add("Online");
            CustomerName = await _createPaymentAppViewModel.GetCustomerCodeName();
            foreach (var store in CustomerName)
            {
                storeData[store.UID] = store.Name;
            }
            OnTabSelect(null, "Cash", "");
            PopulateProperties();
            Name = GetParameterValueFromURL("Name");
            LoadResources(null, _languageService.SelectedCulture);

        }
        public override async Task OnBackClick()
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["confirm?"], "Do you want to exit?", @Localizer["yes"], @Localizer["no"]))
            {
                //_backbuttonhandler.ClearCurrentPage();
                _navigationManager.NavigateTo("/CustomerCall");
            }
        }
        static string BidirectionalLookup(string input)
        {
            if (storeData.ContainsKey(input))
            {
                // Look for the name corresponding to the GUID
                foreach (var pair in storeData)
                {
                    if (pair.Key == input)
                    {
                        return pair.Value;
                    }
                }
            }

            // Return the input if not found
            return input;
        }

        public decimal TabAmount(string tab)
        {
            try
            {
                return CollectionListCopy.Where(p => p.Category == tab).Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task PrintSummary()
        {
            try
            {
                List<IPaymentSummary> PaymentSummaryData = await _createPaymentAppViewModel.GetPaymentSummary(FromDate.ToString(), ToDate.ToString());
                decimal CashTotalAmount = PaymentSummaryData.Where(p => p.Category == "Cash").Sum(p => p.Amount);
                decimal ChequeTotalAmount = PaymentSummaryData.Where(p => p.Category == "Cheque").Sum(p => p.Amount);
                decimal POSTotalAmount = PaymentSummaryData.Where(p => p.Category == "POS").Sum(p => p.Amount);
                decimal OnlineTotalAmount = PaymentSummaryData.Where(p => p.Category == "Online").Sum(p => p.Amount);
                IPrint collectionSummaryPrint = new CollectionSummaryPrint();
                string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand");   //  "Zebra";         //
                string printerSizeString =  _storageHelper.GetStringFromPreferences("PrinterPaperSize");    // "FourInch";        //
                PrinterType printerType = (PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
                PrinterSize printerPaperSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);
                string collectionOrderPrintString = collectionSummaryPrint.CreatePrintString(printerType, printerPaperSize, (PaymentSummaryData, CashTotalAmount, ChequeTotalAmount, POSTotalAmount, OnlineTotalAmount));
                Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
                if (userPrinter.Type == PrinterType.Zebra)
                {
                    await ((ZebraPrinter)userPrinter).Print(collectionOrderPrintString);
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }

        public async Task WinitTextBox_OnSearch(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                try
                {

                }
                catch (Exception ex)
                {

                }
            }
            else
            {

            }
            await Task.CompletedTask;
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigate.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        private void ChangeTab(string tab)
        {
            selectedTab = tab;
            OnTabSelect(null, selectedTab, "");
        }
        //async Task SetTopBar()
        //{
        //    MainButtons buttons = new MainButtons()
        //    {
        //        UIButton1 = new Buttons()
        //        {
        //            ButtonType = ButtonType.Text,
        //            ButtonText = "Back",
        //            IsVisible = true,
        //            Action = Redirect
        //        },
        //        TopLabel = "Payment Summary"
        //    };
        //    await Btnname.InvokeAsync(buttons);
        //}


        private void Search()
        {
        }

        public void PopulateProperties()
        {
            CashAmount = CollectionListCopy.Where(c => c.Category.Contains("Cash")).Sum(c => c.Amount);
            ChequeAmount = CollectionListCopy.Where(c => c.Category.Contains("Cheque")).Sum(c => c.Amount);
            POSAmount = CollectionListCopy.Where(c => c.Category.Contains("POS")).Sum(c => c.Amount);
            CreditAmount = CollectionListCopy.Where(c => c.Category.Contains("CREDITNOTE")).Sum(c => c.Amount);
            OnlineAmount = CollectionListCopy.Where(c => c.Category.Contains("Online")).Sum(c => c.Amount);
            TotalAmount = CollectionListCopy.Where(p => !p.Status.Contains("OnAccount")).Sum(p => p.Amount);
        }
        public async void OnTabSelect(ChangeEventArgs e, string Payment, string date)
        {
            FromDate = date == "From" ? Convert.ToDateTime(e.Value) : FromDate;
            ToDate = date == "To" ? Convert.ToDateTime(e.Value) : ToDate;
            if (ToDate < FromDate || FromDate > ToDate)
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["please_check_date_range"]);
                return;
            }
            CollectionList = await _collectionModuleViewModel.PaymentReceipts(FromDate.ToString(), ToDate.ToString(), Payment);
            CollectionListCopy = CollectionList;
            if (Payment == "Cash")
            {
                CollectionList = CollectionListCopy.Where(p => p.Category == Payment || p.Category == "CREDITNOTE").ToList();
            }
            else
            {
                CollectionList = CollectionListCopy.Where(p => p.Category == Payment).ToList();
            }
            TotalAmount = CollectionListCopy.Where(p => !p.Status.Contains("OnAccount")).Sum(p => p.Amount);
            StateHasChanged();
        }

        public async void DivClicked(IAccCollection collection)
        {
            collection.flag = true;
            collection.IsExpanded = !collection.IsExpanded;
            if (!collection.IsExpanded)
            {
                collection.flag = !collection.flag;
            }
            AllotmentList = await _collectionModuleViewModel.AllotmentReceipts(collection.UID);
            CPODetails = await _collectionModuleViewModel.CPOData(collection.UID);
            StateHasChanged();
        }
        public static string GetDayInFormat(DateTime? dateTime)
        {
            return dateTime?.ToString("dd");
        }

        public static string GetMonthYearInFormat(DateTime? dateTime)
        {
            return dateTime?.ToString("MMM yyyy");
        }

        public async Task View(IAccCollection collection)
        {
            try
            {
                ViewMultiCurrencyDetails.Clear();
                ConvertMultiCurrencyDetails = await _createPaymentAppViewModel.GetMultiCurrencyDetails(collection.UID);
                for (int i = 0; i < ConvertMultiCurrencyDetails.Count; i++)
                {
                    ViewMultiCurrencyDetails.Add(new ExchangeRate()); // Example: Replace ExchangeRate with your concrete implementation of IExchangeRate
                }
                for (int i = 0; i < ConvertMultiCurrencyDetails.Count; i++)
                {
                    ViewMultiCurrencyDetails[i].FromCurrencyUID = ConvertMultiCurrencyDetails[i].currency_uid;
                    ViewMultiCurrencyDetails[i].Rate = (decimal)ConvertMultiCurrencyDetails[i].default_currency_exchange_rate;
                    ViewMultiCurrencyDetails[i].CurrencyAmount = (decimal)ConvertMultiCurrencyDetails[i].amount;
                    ViewMultiCurrencyDetails[i].ConvertedAmount = (decimal)ConvertMultiCurrencyDetails[i].default_currency_amount;
                }
                IsShowMultiCurrency = !IsShowMultiCurrency;
                if (IsShowMultiCurrency)
                {
                    await _multi.OnInit(collection.Category);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task Printer(IAccCollection accCollection)
        {
            try
            {
                List<string> UID = new List<string>();
                UID.Add(accCollection.UID);
                CollectionOrderPaymentDetails = await _createPaymentAppViewModel.GetCollectionStoreDataForPrinter(UID);
                //call printing method
                IPrint CollectionOrderPrint = new CollectionOrderPrint();
                string printerTypeString = _storageHelper.GetStringFromPreferences("PrinterTypeOrBrand"); // "Zebra";         //
                string printerSizeString = _storageHelper.GetStringFromPreferences("PrinterPaperSize");  //"FourInch";        //
                PrinterType printerType = (PrinterType)Enum.Parse(typeof(PrinterType), printerTypeString);
                PrinterSize printerPaperSize = (PrinterSize)Enum.Parse(typeof(PrinterSize), printerSizeString);
                string collectionOrderPrintString = CollectionOrderPrint.CreatePrintString(printerType, printerPaperSize, (CollectionOrderPaymentDetails));
                Winit.Modules.Printing.BL.Interfaces.IPrinter userPrinter = Winit.Modules.Printing.BL.Factory.PrinterFactory.CreatePrinter(_storageHelper.GetStringFromPreferences("PrinterMacAddresses"), printerType, _storageHelper.GetStringFromPreferences("PrinterMacAddresses"));
                if (userPrinter.Type == PrinterType.Zebra)
                {
                    await ((ZebraPrinter)userPrinter).Print(collectionOrderPrintString);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task Share()
        {
            try
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["will_be_available_in_phase_2!"]);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CloseMultiCurrency()
        {
            try
            {
                IsShowMultiCurrency = !IsShowMultiCurrency;
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
