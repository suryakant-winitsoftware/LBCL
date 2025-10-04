using iTextSharp.text;
using Microsoft.AspNetCore.Components;
using System.Net;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using static Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentAppViewModel;
using Payment = Winit.Modules.CollectionModule.Model.Classes.Payment;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface ICreatePaymentViewModel
    {

        public ICollectionAmount collectionAmount { get; set; }
        public DateTime? selectedDate { get; set; }
        public AccCustomer[] Responsedata { get; set; }
        public AccPayable[] ResponseDaystable { get; set; }
        public AccPayable[] ResponseDaystab { get; set; }
        public Payment[] Respon { get; set; }
        public List<AccPayable> ResponseDaystabList { get; set; }
        public List<SelectionItem> payments { get; set; }
        public string selectedValueText { get; set; }
        public string selectedValue1 { get; set; }
        public string ReceiptID { get; set; }
        public string CurrencyUID { get; set; }
        public decimal Rate { get; set; }
        public decimal ReceiptCount { get; set; }
        public AccCollectionPaymentMode[] bank { get; set; }
        public ExchangeRate[] CurrencyDetails { get; set; }
        public Payment[] ResponsedData { get; set; }
        public string StoreUID { get; set; }
        public string ReceiptNumber { get; set; }
        public AccDocument[] Response { get; set; }



        public IBank[] ReceiptBank { get; set; }
        public HashSet<AccPayable> SelectedInvoiceRecords { get; set; }
        public HashSet<AccPayable> _creditNoteRecords { get; set; }
        public List<InvoiceInfo> _outstandingRecordsAmount { get; set; }
        public Winit.Shared.Models.Common.ApiResponse<string> response { get; set; }
        public List<ICollections> collectionListRecords { get; set; }
        public AccPayable recList { get; set; }
        public string PaymentMode { get; set; }
        public int OnLoadOnAccount { get; set; }
        public bool OnAccountSuccessFlag { get; set; }
        public bool IsReceiptCreated { get; set; }
        public bool ExtraOnAcc { get; set; }
        public int IsFirstEntry { get; set; }
        public bool CashOnAccount { get; set; }
        public bool IsExcelUpload { get; set; }
        public bool ChequeOnAccount { get; set; }
        public bool POSOnAccount { get; set; }
        public bool OnlineOnAccount { get; set; }

        #region Collection Methods
        Task UnSettlePopUpRecords(bool _bankPop);
        Task TableRowClickUI(AccPayable timePeriod, string totalRecords);
        Task ChangeRecords(string ReceiptID);
        Task Paymentmode(string ReceiptNum, string UID, ChangeEventArgs e);
        Task CheckCustomerEligibleforDiscount();
        Task BindInvoiceTable();
        Task<List<SelectionItem>> PaymentModeSelectionItem(List<SelectionItem> Item);
        Task<string> PaymentModeSelection(string Mode);
        Task DocumentDataUI(string ReceiptNumber);
        Task CurrencyData(string ReceiptNumber);
        Task CurrencyChange();
        Task PopulateUISide(string CustomerCode);
        #endregion

        #region Receipt Methods
        Task<bool> Change(HashSet<object> hashSet);
        Task<bool> Product_AfterCheckBoxSelection(HashSet<object> hashSet);
        Task GetBank();
        Task<HttpStatusCode> CreateOnAccount(decimal _OnAccountBalance);
        Task UpdateDetails(decimal creditAmt, AccPayable CreditNoteRecords);
        Task CreateReceipt(bool AutoAllocate, List<ICollections> collectionListRecords);
        #endregion




        #region Mobile
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _PopulateList { get; set; }
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> SelectedRecords { get; set; }
        public IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> _customersList { get; set; }
        public List<IAccPayable> SelectedItems { get; set; }
        public List<ISetting> Settings { get; set; }
        public List<IExchangeRate> MultiCurrencyDetails { get; set; }
        public Dictionary<string, List<IExchangeRate>> MultiCurrencyDetailsData { get; set; }
        public List<PaymentInfo> paymentInfos { get; set; }
        public List<string> UIDForPrint { get; set; }
        public string Discrepency { get; set; }
        public bool IsOnAccount { get; set; }
        public bool IsDirectOnAccountCreate { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int OnAccountResult { get; set; }
        public decimal CollectionLimit { get; set; }
        Task PopulateViewModel();
        Task OnSignatureProceedClick();
        Task PrepareSignatureFields();
        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string CollectionsUID { get; set; }
        public string CustomerName { get; set; }
        public List<IFileSys> ImageFileSysList { get; set; }
        public string CheckListData { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; }
        public List<IBank> Banks { get; set; }
        public string Guidstring();
        public string sixGuidstring();
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _invoiceList { get; set; }
        Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID);
        Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs);
        Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode);
        public List<IAccCollection> _collectionList { get; set; }
        Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<string> CreateReceipt(ICollections collection);
        Task<string> CreateOnAccount(ICollections collection, bool IsDirectOnAccount);
        Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment);
        Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID);
        Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID);
        Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID);
        Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID);
        Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID);
        Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID);
        Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID);
        Task<List<IStore>> GetCustomerCodeName();
        Task<List<ISetting>> GetSettings();
        Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate);
        Task<decimal> GetCollectionLimitForLoggedInUser(string EmpUID);
        Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action);
        #endregion
    }
}
