using Microsoft.AspNetCore.Components;
using System.Net;
using Winit.Modules.Bank.Model.Classes;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using static Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentAppViewModel;
using Payment = Winit.Modules.CollectionModule.Model.Classes.Payment;

namespace Winit.Modules.CollectionModule.BL.Classes.CreatePayment
{
    public abstract class CreatePaymentBaseViewModel : ICreatePaymentViewModel
    {
        #region ReadOnly Variables
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        #endregion

        #region Class Variables
        public HashSet<AccPayable> _creditNoteRecords { get; set; }
        public HashSet<AccPayable> SelectedInvoiceRecords { get; set; }
        public HashSet<AccPayable> selectedItemsConstant { get; set; }
        public List<decimal> _enteredAmountInv { get; set; }
        public List<InvoiceInfo> _outstandingRecordsAmount { get; set; }
        public List<AccPayable> reaarangedList { get; set; }
        public List<AccPayable> ResponseDaystabList { get; set; }
        public List<SelectionItem> payments { get; set; }
        public List<ICollections> collectionListRecords { get; set; }

        public ICollectionAmount collectionAmount { get; set; }
        public IBank[] ReceiptBank { get; set; } = new IBank[1];
        public AccPayable recList { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<string> response { get; set; }
        public static Winit.Modules.CollectionModule.Model.Interfaces.ICollections onAccountcollection { get; set; }
        public AccCustomer[] Responsedata { get; set; }
        public AccCollectionPaymentMode[] bank { get; set; }
        public ExchangeRate[] CurrencyDetails { get; set; }
        public Payment[] ResponsedData { get; set; }
        public AccPayable[] ResponseDaystable { get; set; }
        public AccPayable[] ResponseDaystab { get; set; }
        public EarlyPaymentDiscountConfiguration[] EligibleRecords { get; set; }
        public List<AccPayable> OverDueRecords { get; set; }
        public List<AccPayable> OnTimeRecords { get; set; }
        public AccDocument[] Response { get; set; }
        public Payment[] Respon { get; set; }
        #endregion
        #region String Variables
        public string Consolidated_Current_Date { get; set; }
        public string TargetUID1 { get; set; }
        public string PaymentMode { get; set; }
        public string StoreUID { get; set; }
        public string InvoiceNo1 { get; set; }
        public string InvoiceNo { get; set; }
        public string CustCode { get; set; }
        public string CurrencyUID { get; set; }
        public string _date { get; set; }
        public string ReceiptID { get; set; }
        public string selectedValueText { get; set; }
        public string selectedValue1 { get; set; }
        public string ReceiptNumber { get; set; }
        public string _allRecords { get; set; }
        public string range { get; set; }
        public long _count { get; set; }
        public long _count1 { get; set; }
        public string CustomerName { get; set; }
        public string CheckListData { get; set; }
        #endregion
        #region Decimal Variables
        public decimal enteredAmount { get; set; }
        public decimal CreditNoteAmount { get; set; }
        public decimal CreditNoteAmountCopy { get; set; }
        public decimal CollectionAmount { get; set; }
        public decimal ExtraAmount { get; set; }
        public decimal EnteringAmount { get; set; }
        public decimal ExtraAmountCopy { get; set; }
        public decimal cheqAmount { get; set; }
        public decimal TotalAmountCN { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ExtraOnAccount { get; set; }
        public decimal Discountval { get; set; }
        public decimal Rate { get; set; }
        public static decimal OnAccountBalance { get; set; }
        public decimal TotalTableAmt { get; set; }
        public decimal ReceiptCount { get; set; } = 0;
        #endregion
        #region Int Variables
        public int _start { get; set; }
        public int _end { get; set; }
        public int AdvanceDays { get; set; }
        public int OnLoadOnAccount { get; set; }
        public int IsFirstEntry { get; set; }
        public int OnOnce { get; set; }
        public int OnAccountImp { get; set; }


        #endregion
        #region bool Variables
        public bool ExtraOnAcc { get; set; }
        public bool IsReceiptCreated { get; set; }
        public bool OnAccountSuccessFlag { get; set; }
        public bool CashOnAccount { get; set; }
        public bool ChequeOnAccount { get; set; }
        public bool POSOnAccount { get; set; }
        public bool OnlineOnAccount { get; set; }
        public bool Partial { get; set; }
        public bool ZeroReceiptPayment { get; set; }
        public bool IsAutoAllocate { get; set; }
        public bool IsExcelUpload { get; set; }
        #endregion
        #region Date Variables
        public DateTime? selectedDate { get; set; }
        public DateTime TripDate { get; set; }
        #endregion


        #region Mobile Variables
        public IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> _customersList { get; set; }
        public IEarlyPaymentDiscountConfiguration[] EligibleRecordsMobile { get; set; } = new IEarlyPaymentDiscountConfiguration[0];
        public List<IAccPayable> SelectedItems { get; set; } = new List<IAccPayable>();
        public List<ISetting> Settings { get; set; } = new List<ISetting>();
        public List<IExchangeRate> MultiCurrencyDetails { get; set; } = new List<IExchangeRate>();
        public Dictionary<string, List<IExchangeRate>> MultiCurrencyDetailsData { get; set; } = new Dictionary<string, List<IExchangeRate>>();
        public List<PaymentInfo> paymentInfos { get; set; } = new List<PaymentInfo>();
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _PopulateList { get; set; }
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> SelectedRecords { get; set; }
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _invoiceList { get; set; }
        public List<IAccCollection> _collectionList { get; set; }
        public List<IAccCollectionAllotment> _allotmentList { get; set; }

        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; } = new List<IFileSys>();
        public string CollectionsUID { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; } = new List<IFileSys>();
        public List<IBank> Banks { get; set; } = new List<IBank>();
        public string ConsolidatedReceiptNumber { get; set; }
        public List<ICollectionPrint> CollectionStoreDataForPrinter { get; set; } = new List<ICollectionPrint>();
        public List<ICollectionPrintDetails> AllotmentDataForPrinter { get; set; } = new List<ICollectionPrintDetails>();
        public List<IExchangeRate> CurrencyRateRecords { get; set; } = new List<IExchangeRate>();
        public List<IAccPayable> PendingPopupData { get; set; } = new List<IAccPayable>();
        public List<IAccCollectionCurrencyDetails> ViewMultiCurrencyDetailsData { get; set; } = new List<IAccCollectionCurrencyDetails>();
        public List<IAccCollectionPaymentMode> ShowPendingRecordsInPopUpData { get; set; } = new List<IAccCollectionPaymentMode>();
        public List<string> UIDForPrint { get; set; } = new List<string>();
        public DateTime _collectedDate { get; set; } = DateTime.Now;
        public bool IsOnAccount { get; set; } = false;
        public bool IsDirectOnAccountCreate { get; set; } = false;
        public string Discrepency { get; set; } = "0";
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int OnAccountResult { get; set; } = 0;
        public decimal CollectionLimit { get; set; } = 0;
        #endregion

        protected CreatePaymentBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
            _appUser = appUser;
            _creditNoteRecords = new HashSet<AccPayable>();
            SelectedInvoiceRecords = new HashSet<AccPayable>();
            selectedItemsConstant = new HashSet<AccPayable>();
            collectionListRecords = new List<ICollections>();
            _enteredAmountInv = new List<decimal>();
            _outstandingRecordsAmount = new List<InvoiceInfo>();
            reaarangedList = new List<AccPayable>();
            ResponseDaystabList = new List<AccPayable>();
            ReceiptBank = new Winit.Modules.Bank.Model.Classes.Bank[0];
            collectionAmount = new CollectionAmount();
            recList = new AccPayable();
            payments = new List<SelectionItem>();
            onAccountcollection = new Collections();
            Responsedata = new AccCustomer[0];
            bank = new AccCollectionPaymentMode[0];
            CurrencyDetails = new ExchangeRate[0];
            ResponsedData = new Payment[0];
            ResponseDaystable = new AccPayable[0];
            ResponseDaystab = new AccPayable[0];
            EligibleRecords = new EarlyPaymentDiscountConfiguration[0];
            OverDueRecords = new List<AccPayable>();
            OnTimeRecords = new List<AccPayable>();
            Response = new AccDocument[0];
            Respon = new Payment[0];
            MultiCurrencyDetailsData = new Dictionary<string, List<IExchangeRate>>();
            enteredAmount = 0; OnOnce = 1;
            CreditNoteAmount = 0; EnteringAmount = 0;
            CreditNoteAmountCopy = 0;
            TotalTableAmt = 0;
            CollectionAmount = 0;
            OnLoadOnAccount = 0;
            ExtraAmount = 0;
            ExtraAmountCopy = 0;
            OnAccountImp = 0;
            cheqAmount = 0;
            AdvanceDays = 0;
            TotalAmountCN = -1;
            TotalAmount = 0;
            Discountval = 0;
            OnAccountBalance = 0;
            ExtraOnAccount = 0;
            Rate = 0;
            IsFirstEntry = 0;
            _start = 0;
            _end = 0;
            _count = 0;
            _count1 = 0;
            _allRecords = "Total Outstanding Balance";
            range = "Total Outstanding Balance";
            TargetUID1 = "";
            selectedValue1 = "";
            ReceiptNumber = "";
            CustomerName = "";
            selectedValueText = "-- Select Customer --";
            InvoiceNo = "";
            InvoiceNo1 = "";
            CustCode = "";
            CurrencyUID = "";
            Consolidated_Current_Date = "";
            PaymentMode = "";
            ReceiptID = "";
            _date = DateTime.Now.ToString("yyyy-MM-dd");
            StoreUID = "";
            selectedDate = DateTime.Now;
            TripDate = DateTime.Now;
            Partial = false;
            IsAutoAllocate = false;
            ZeroReceiptPayment = false;
            ExtraOnAcc = false;
            IsReceiptCreated = false;
            OnAccountSuccessFlag = false;
        }

        public string CollectionUID()
        {
            string Uid = (Guid.NewGuid()).ToString();
            UIDForPrint.Add(Uid);
            return Uid;
        }
        public string sixGuidstring()
        {
            Guid newGuid = Guid.NewGuid();

            // Convert the GUID to a string and take the first 8 characters without hyphens
            string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);
            return eightDigitGuid;
        }
        public string Guidstring()
        {
            return (Guid.NewGuid()).ToString();
        }

        #region Collection Abstract Methods
        public abstract Task TableRowClickUI(AccPayable timePeriod, string totalRecords);
        public abstract Task ChangeRecords(string ReceiptID);
        public abstract Task CheckCustomerEligibleforDiscount();
        public abstract Task BindInvoiceTable();
        public abstract Task UnSettlePopUpRecords(bool _bankPop);
        public abstract Task Paymentmode(string ReceiptNum, string UID, ChangeEventArgs e);
        public abstract Task<List<SelectionItem>> PaymentModeSelectionItem(List<SelectionItem> Item);
        public abstract Task<string> PaymentModeSelection(string Mode);
        public abstract Task DocumentDataUI(string ReceiptNumber);
        public abstract Task CurrencyData(string ReceiptNumber);
        public abstract Task CurrencyChange();
        public abstract Task PopulateUISide(string CustomerCode);
        #endregion

        #region Receipt Abstract Methods
        public abstract Task<bool> Change(HashSet<object> hashSet);
        public abstract Task<bool> Product_AfterCheckBoxSelection(HashSet<object> hashSet);
        public abstract Task GetBank();
        public abstract Task<HttpStatusCode> CreateOnAccount(decimal _OnAccountBalance);
        public abstract Task UpdateDetails(decimal creditAmt, AccPayable CreditNoteRecords);
        public abstract Task CreateReceipt(bool AutoAllocate, List<ICollections> collectionListRecords);
        #endregion



        #region Mobile
        public abstract Task PopulateViewModel();
        public abstract Task OnSignatureProceedClick();
        public abstract Task PrepareSignatureFields();
        public abstract Task<List<IAccPayable>> GetInvoicesMobile(string AccCollectionUID);
        public abstract Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs);
        public abstract Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode);
        public abstract Task<IEnumerable<IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        public abstract Task<string> CreateReceipt(ICollections collection);
        public abstract Task<string> CreateOnAccount(ICollections collection, bool IsDirectOnAccount);
        public abstract Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment);
        public abstract Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID);
        public abstract Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID);
        public abstract Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID);
        public abstract Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID);
        public abstract Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID);
        public abstract Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID);
        public abstract Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID);
        public abstract Task<List<IStore>> GetCustomerCodeName();
        public abstract Task<List<ISetting>> GetSettings();
        public abstract Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate);
        public abstract Task<decimal> GetCollectionLimitForLoggedInUser(string EmpUID);
        public abstract Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action);
        #endregion
    }
}
