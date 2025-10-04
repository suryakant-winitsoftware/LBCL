using Microsoft.AspNetCore.Components;
using System.Net;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Modules.FileSys.BL.Classes;
using Winit.Modules.FileSys.BL.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Winit.Modules.CollectionModule.BL.Classes.CreatePayment
{
    public class CreatePaymentAppViewModel : CreatePaymentBaseViewModel
    {
        protected IServiceProvider _serviceProvider { get; set; }
        protected ApiService _apiService { get; set; }
        private readonly IFileSysBL _fileSysBL;
        protected IAppConfig _appConfigs { get; set; }
        protected IAppUser _appUser { get; set; }
        protected IDataManager _dataManager { get; set; }
        protected readonly ICollectionModuleBL _collectionModuleBL;
        public CreatePaymentAppViewModel(IServiceProvider serviceProvider,
                IDataManager dataManager , IFileSysBL fileSysBL, IAppConfig appConfig, IAppUser appUser, ICollectionModuleBL collectionModuleBL, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            this._collectionModuleBL = collectionModuleBL;
            _serviceProvider = serviceProvider;
            _appConfigs = appConfig;
            _apiService = apiService;
            _appUser = appUser;
            _fileSysBL = fileSysBL;
            _dataManager = dataManager;
            MultiCurrencyDetailsData = new Dictionary<string, List<IExchangeRate>>();
        }


        public static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collection = new Collections();

        public List<ICollections> CollectionListRecords { get; set; } = new List<ICollections>();

        public decimal TotalAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<decimal> Discounts { get; set; } = new List<decimal>();
        private string _creditID { get; set; } = $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";

        public override async Task OnSignatureProceedClick()
        {
            CustomerSignatureFilePath = Path.Combine(CustomerSignatureFolderPath, CustomerSignatureFileName);
            UserSignatureFilePath = Path.Combine(UserSignatureFolderPath, UserSignatureFileName);
            int retValue = await _fileSysBL.CreateFileSysForBulk(SignatureFileSysList);
            if (retValue < 0) throw new Exception("Failed to save the Signatures");
        }
        public override async Task PrepareSignatureFields()
        {
            string baseFolder = Path.Combine(_appConfigs.BaseFolderPath, FileSysTemplateControles.GetSignatureFolderPath("Collection", _dataManager.GetData("ReceiptNumber").ToString()));
            CustomerSignatureFolderPath = baseFolder;
            UserSignatureFolderPath = baseFolder;
            CustomerSignatureFileName = $"Customer_{_dataManager.GetData("ReceiptNumber").ToString()}.png";
            UserSignatureFileName = $"User_{_dataManager.GetData("ReceiptNumber").ToString()}.png";
        }
        public override async Task PopulateViewModel()
        {
            await GetAllCustomersBySalesOrgCode(null, null);
        }

        public override async Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            EligibleRecordsMobile = (await _collectionModuleBL.CheckEligibleForDiscount(ApplicableCode)).ToArray();
            return EligibleRecordsMobile;
        }
        public async Task CreateInstances()
        {
            try
            {
                collection.AccCollection = new AccCollection();
                collection.AccPayable = new List<IAccPayable>();
                collection.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                collection.AccStoreLedger = new AccStoreLedger();
                collection.AccReceivable = new List<IAccReceivable>();
                collection.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                collection.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task<string> CreateReceipt(ICollections Data)
        {
            try
            {
                MultiCurrencyDetailsData = (Dictionary<string, List<IExchangeRate>>)_dataManager.GetData("MultiCurrencyDetails") ?? new Dictionary<string, List<IExchangeRate>>();
                foreach (var item in SelectedItems.Where(p => p.ReferenceNumber.Contains("CREDITNOTE", StringComparison.OrdinalIgnoreCase)))
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        PaymentType = item.ReferenceNumber,
                        IsChecked = true,
                        Amount = item.PayingAmount,
                        Type = item.ReferenceNumber,
                        // Set other properties as needed
                    });
                }
                foreach (var mode in paymentInfos.Where(p => p.IsChecked))
                {
                    await CreateInstances();
                    if (Convert.ToDecimal(Discrepency) > 0)
                    {
                        IsOnAccount = true;
                        PopulateCollection(mode, "OnAccount");
                        OnAccountResult = Convert.ToInt32(await CreateOnAccount(collection, false));
                        Discrepency = "0";
                        if (OnAccountResult != 1)
                        {
                            return "0";
                        }
                    }
                    await CreateInstances();
                    await CreateAllotmentJSON(mode);
                }
                string result = await _collectionModuleBL.CreateReceipt(CollectionListRecords.ToArray());
                CollectionListRecords.Clear();
                if (result.Contains("Success"))
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task CreateAllotmentJSON(PaymentInfo Info)
        {
            try
            {
                collection.AccCollection = new AccCollection();
                AccReceivable recev = new AccReceivable();
                AccPayable pay = new AccPayable();

                switch (Info.PaymentType)
                {
                    case "Cash":
                        TotalAmount = Info.Amount;
                        break;
                    case "Cheque":
                        TotalAmount = Info.Amount;
                        break;
                    case "POS":
                        TotalAmount = Info.Amount;
                        break;
                    case "Online":
                        TotalAmount = Info.Amount;
                        break;
                    default:
                        TotalAmount = 0;
                        break;
                }

                foreach (var item in SelectedItems.Where(p => p.edit))
                {

                    AccCollectionAllotment allotment = new AccCollectionAllotment();
                    allotment.UID = (Guid.NewGuid()).ToString();
                    allotment.StoreUID = _appUser.SelectedCustomer.StoreUID;
                    allotment.TargetType = item.SourceType;
                    allotment.ReferenceNumber = item.ReferenceNumber;
                    if (!Info.PaymentType.Contains("CREDITNOTE"))
                    {
                        await ApplyDiscount(item, Info.PaymentType);
                    }
                    if (Info.Amount > item.PayingAmount)
                    {
                        allotment.PaidAmount = item.PayingAmount;
                        allotment.DiscountAmount = DiscountAmount;
                        collection.AccCollectionAllotment.Add(allotment);
                        await CreateDiscountRecord(item, DiscountAmount);
                        await CreatePayableReceiveRecords(item, item.PayingAmount + DiscountAmount);
                        if (Info.Type.Contains("CREDITNOTE"))
                        {
                            await CreateCreditRecord(recev, Info);
                        }
                        Info.Amount -= item.PayingAmount;
                        item.PayingAmount = 0;
                        item.edit = false;
                    }
                    else
                    {
                        allotment.PaidAmount = Info.Amount;
                        allotment.DiscountAmount = DiscountAmount;
                        collection.AccCollectionAllotment.Add(allotment);
                        await CreateDiscountRecord(item, DiscountAmount);
                        await CreatePayableReceiveRecords(item, Info.Amount + DiscountAmount);
                        if (Info.Type.Contains("CREDITNOTE"))
                        {
                            await CreateCreditRecord(recev, Info);
                        }
                        item.PayingAmount -= Info.Amount;
                        Info.Amount = 0;
                        item.edit = item.PayingAmount == 0 ? false : true;
                        break;
                    }
                }
                DiscountAmount = 0;
                //collection.AccCollection.Amount = TotalAmount - Discounts.Sum();
                if (MultiCurrencyDetailsData.ContainsKey(Info.PaymentType))
                {
                    await CreateMultiCurrencyDetails(Info);
                }
                PopulateCollection(Info);
                CollectionListRecords.Add(collection);
                collection = new Collections();
                Discounts.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task ApplyDiscount(IAccPayable item, string PaymentMode)
        {
            try
            {
                if (SelectedItems.FirstOrDefault().Discount)
                {
                    DiscountAmount = 0;
                    if (SelectedItems.Where(p => p.SourceType.Contains("INVOICE")).Last() == item)
                    {
                        CreditAmount += SelectedItems.Where(p => p.ReferenceNumber.Contains("CREDITNOTE")).Sum(p => p.PayingAmount);
                    }
                    if (item.Amount == item.PayingAmount && paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentMode).Amount >= item.PayingAmount - CreditAmount)
                    {
                        paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentMode).Amount -= ((item.PayingAmount - CreditAmount) * item.DiscountValue / 100);
                        DiscountAmount = (item.PayingAmount - CreditAmount) * item.DiscountValue / 100;
                        Discounts.Add(DiscountAmount);
                        item.PayingAmount -= (item.PayingAmount - CreditAmount) * item.DiscountValue / 100;
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CreateDiscountRecord(IAccPayable accPayable, decimal DiscountAmount)
        {
            try
            {
                if (DiscountAmount != 0)
                {
                    AccCollectionAllotment allotment = new AccCollectionAllotment();
                    allotment.UID = (Guid.NewGuid()).ToString();
                    allotment.PaidAmount = DiscountAmount;
                    allotment.DiscountAmount = 0;
                    allotment.TargetType = "CREDITNOTE";
                    allotment.StoreUID = accPayable.StoreUID;
                    allotment.ReferenceNumber = "";
                    collection.AccCollectionAllotment.Add(allotment);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CreateCreditRecord(IAccReceivable recev, PaymentInfo info)
        {
            try
            {
                AccCollectionAllotment allotment = new AccCollectionAllotment();
                IAccPayable ReceiveRecords = SelectedItems.FirstOrDefault(item => item.ReferenceNumber == info.PaymentType);
                recev.ReferenceNumber = ReceiveRecords.ReferenceNumber;
                recev.StoreUID = ReceiveRecords.StoreUID;
                recev.PaidAmount = ReceiveRecords.PayingAmount;
                recev.SourceType = ReceiveRecords.SourceType;
                recev.SourceUID = ReceiveRecords.SourceUID;
                allotment.UID = (Guid.NewGuid()).ToString();
                allotment.StoreUID = ReceiveRecords.StoreUID;
                allotment.TargetType = ReceiveRecords.SourceType;
                allotment.ReferenceNumber = ReceiveRecords.ReferenceNumber;
                allotment.PaidAmount = ReceiveRecords.PayingAmount;
                collection.AccReceivable.Add(recev);
                collection.AccCollectionAllotment.Add(allotment);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CreatePayableReceiveRecords(IAccPayable accPayable, decimal PayingAmount)
        {
            try
            {
                if (accPayable.SourceType.Contains("INVOICE"))
                {
                    AccPayable pay = new AccPayable();
                    pay.ReferenceNumber = accPayable.ReferenceNumber;
                    pay.StoreUID = accPayable.StoreUID;
                    pay.SourceType = accPayable.SourceType;
                    pay.SourceUID = accPayable.SourceUID;
                    pay.PaidAmount = PayingAmount;
                    collection.AccPayable.Add(pay);
                }
                else
                {
                    AccReceivable receivable = new AccReceivable();
                    receivable.ReferenceNumber = accPayable.ReferenceNumber;
                    receivable.StoreUID = accPayable.StoreUID;
                    receivable.PaidAmount = PayingAmount;
                    receivable.SourceType = accPayable.SourceType;
                    receivable.SourceUID = accPayable.SourceUID;
                    collection.AccReceivable.Add(receivable);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string CreateReceiptNumber(string OnAccount)
        {
            try
            {
               return string.IsNullOrEmpty(OnAccount) ? _dataManager.GetData("ReceiptNumber").ToString() : "OA - " + _dataManager.GetData("ReceiptNumber").ToString();
            }
            catch(Exception ex)
            {
                throw new();
            }
        }
        public string CreateConsolidatedReceiptNumber(string OnAccount)
        {
            try
            {
               return string.IsNullOrEmpty(OnAccount) ? _dataManager.GetData("ConsolidatedReceiptNumber").ToString() : "OA - " + _dataManager.GetData("ConsolidatedReceiptNumber").ToString();
            }
            catch(Exception ex)
            {
                throw new();
            }
        }

        public void PopulateCollection(PaymentInfo list, string OnAccount = null)
        {
            collection.AccCollection.ReceiptNumber = paymentInfos.Count(p => p.IsChecked) == 1 ? CreateReceiptNumber(OnAccount) : Code + "_" + sixGuidstring();
            if (paymentInfos.Count(p => p.IsChecked) > 1)
            {
                collection.AccCollection.IsMultimode = true;
                collection.AccCollection.ConsolidatedReceiptNumber = CreateConsolidatedReceiptNumber(OnAccount);
            }
            else
            {
                collection.AccCollection.IsMultimode = false;
                collection.AccCollection.ConsolidatedReceiptNumber = collection.AccCollection.ReceiptNumber;
            }
            collection.AccCollection.CollectedDate = _collectedDate == null ? DateTime.Now : _collectedDate;
            collection.AccCollection.TripDate = _collectedDate;
            collection.AccCollection.DistributionChannelUID = "";
            collection.AccCollection.Amount = OnAccount == null ? TotalAmount - Discounts.Sum() : IsDirectOnAccountCreate ? list.Amount : Convert.ToDecimal(Discrepency);
            collection.AccCollection.CurrencyUID = "INR";
            collection.AccCollection.DefaultCurrencyUID = "INR";
            collection.AccCollection.DefaultCurrencyExchangeRate = 1;
            collection.AccCollection.DefaultCurrencyAmount = OnAccount == null ? collection.AccCollection.Amount : IsDirectOnAccountCreate ? list.Amount : Convert.ToDecimal(Discrepency);
            collection.AccCollection.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            collection.AccCollection.JobPositionUID = _appUser.SelectedJobPosition.UID;
            collection.AccCollection.StoreUID = SelectedItems.Any() ? SelectedItems.FirstOrDefault().StoreUID : _appUser.SelectedCustomer.StoreUID;
            collection.AccCollection.RouteUID = "DIST001_Route1";
            collection.AccCollection.EmpUID = _appUser.Emp.UID;
            collection.AccCollection.Status = (list.PaymentType != "Cash" ? (list.PaymentType.Contains("CREDITNOTE") ? "Collected" : "Submitted") : "Collected");
            collection.AccCollection.Remarks = "";
            if (OnAccount != null)
            {
                collection.AccCollection.ReferenceNumber = paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).PaymentType.Contains("CREDITNOTE") ? "" : paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).AccountNumber;
            }
            collection.AccCollection.IsRealized = false;
            collection.AccCollection.Longitude = "0";
            collection.AccCollection.Latitude = "0";
            collection.AccCollection.Source = "App";
            collection.AccCollection.IsRemoteCollection = _appUser.IsRemoteCollection;
            collection.AccCollection.RemoteCollectionReason = _appUser.RemoteCollectionReason;
            collection.AccCollection.Category = (paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).PaymentType.Contains("CREDITNOTE") ? "CREDITNOTE" : list.PaymentType);
            if (list.PaymentType != "Cash")
            {
                UpdateChequeDetails(list.PaymentType);
            }
            collection.AccCollection.UID = CollectionUID();
            collection.AccCollection.CreatedBy = "ADMIN";
            collection.AccCollection.ModifiedBy = "ADMIN";
            collection.AccCollection.IsEarlyPayment = Discounts.Sum() > 0;
            collection.AccCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
            collection.AccStoreLedger.UID = (Guid.NewGuid()).ToString();
            if (OnAccount != null)
            {
                AccCollectionAllotment allotment = new AccCollectionAllotment();
                AccReceivable recev = new AccReceivable();
                allotment.UID = (Guid.NewGuid()).ToString();
                allotment.StoreUID = SelectedItems.Any() ? SelectedItems.FirstOrDefault().StoreUID : _appUser.SelectedCustomer.StoreUID; 
                allotment.ReferenceNumber = SelectedItems.Any() ? SelectedItems.FirstOrDefault().StoreUID : "" ;
                allotment.TargetUID = (Guid.NewGuid()).ToString(); ;
                allotment.TargetType = "OA - CREDITNOTE";
                recev.UID = allotment.TargetUID;
                collection.AccReceivable.Add(recev);
                collection.AccCollectionAllotment.Add(allotment);
            }
        }

        public void UpdateChequeDetails(string PaymentType)
        {
            collection.AccCollectionPaymentMode.BankUID = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).BankName;
            collection.AccCollectionPaymentMode.Branch = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).BranchName;
            collection.AccCollectionPaymentMode.ChequeNo = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).AccountNumber;
            collection.AccCollectionPaymentMode.Amount = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).Amount;
            collection.AccCollectionPaymentMode.ChequeDate = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).PaymentDate;
            
            collection.AccCollectionPaymentMode.CheckListData = _dataManager.GetData("CheckList").ToString();
        }
        public override async Task<string> CreateOnAccount(ICollections collections, bool IsDirectOnAccount)
        {
            if (IsDirectOnAccount)
            {
                string DirectOnAccountresult = "";
                foreach (var mode in paymentInfos.Where(p => p.IsChecked))
                {
                    await CreateInstances();
                    PopulateCollection(mode, "OnAccount");
                    if (MultiCurrencyDetailsData.ContainsKey(mode.PaymentType))
                    {
                        await CreateMultiCurrencyDetails(mode);
                    }
                    DirectOnAccountresult = await _collectionModuleBL.CreateOnAccountReceipt(collection);
                }
                if (DirectOnAccountresult.Contains("Success"))
                {
                    Discrepency = "0";
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                string result = await _collectionModuleBL.CreateOnAccountReceipt(collections);
                if (result.Contains("Success"))
                {
                    Discrepency = "0";
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
        }

        public async Task CreateMultiCurrencyDetails(PaymentInfo info)
        {
            try
            {
                MultiCurrencyDetails = MultiCurrencyDetailsData[info.PaymentType];
                foreach (var data in MultiCurrencyDetails.Where(p => p.ConvertedAmount != 0))
                {
                    if (data.OriginalAmount != data.ConvertedAmount || data.ConvertedAmount != data.OriginalAmount)
                    {
                        await CreateRoundOffRecord(data);
                    }
                    AccCollectionCurrencyDetails currencyDetails = new AccCollectionCurrencyDetails();
                    currencyDetails.currency_uid = data.FromCurrencyUID;
                    currencyDetails.default_currency_uid = "INR";
                    currencyDetails.default_currency_exchange_rate = data.Rate;
                    currencyDetails.amount = data.CurrencyAmount;
                    currencyDetails.default_currency_amount = data.ConvertedAmount;
                    currencyDetails.final_default_currency_amount = data.OriginalAmount;
                    collection.AccCollectionCurrencyDetails.Add(currencyDetails);
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task CreateRoundOffRecord(IExchangeRate exchangeRate)
        {
            try
            {
                AccCollectionAllotment allotment = new AccCollectionAllotment();
                allotment.UID = (Guid.NewGuid()).ToString();
                allotment.StoreUID = SelectedItems.FirstOrDefault().StoreUID;
                allotment.PaidAmount =  exchangeRate.OriginalAmount > exchangeRate.ConvertedAmount_Temp ?
                        (exchangeRate.OriginalAmount - exchangeRate.ConvertedAmount_Temp)*-1 : exchangeRate.ConvertedAmount_Temp - exchangeRate.OriginalAmount; ;
                allotment.ReferenceNumber = "RO - " + _creditID;
                allotment.TargetType = "ROUNDOFF";
                collection.AccCollectionAllotment.Add(allotment);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public override async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            _customersList = await _collectionModuleBL.GetAllCustomersBySalesOrgCode(SessionUserCode, SalesOrgCode);
            return _customersList;
        }

        public override async Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            _PopulateList = await _collectionModuleBL.PopulateCollectionPage(CustomerCode, Tabs);
            return _PopulateList;
        }
        public override async Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
        {
            _invoiceList = await _collectionModuleBL.GetInvoicesMobile(AccCollectionUID);
            return _invoiceList;
        }

        public override async Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment)
        {
            _collectionList = await _collectionModuleBL.PaymentReceipts(FromDate, ToDate, Payment, "");

            return _collectionList;
        }
        public override async Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID)
        {
            _allotmentList = await _collectionModuleBL.AllotmentReceipts(AccCollectionUID);

            return _allotmentList;
        }

        public override async Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID)
        {
            CollectionStoreDataForPrinter = await _collectionModuleBL.GetCollectionStoreDataForPrinter(UID);
            return CollectionStoreDataForPrinter;
        }
        public override async Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID)
        {
            ShowPendingRecordsInPopUpData = await _collectionModuleBL.ShowPendingRecordsInPopUp(InvoiceNumber, StoreUID);
            return ShowPendingRecordsInPopUpData;
        }

        public override async Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID)
        {
            PendingPopupData = await _collectionModuleBL.GetPendingRecordsFromDB(StoreUID);
            return PendingPopupData;
        }

        public override async Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            AllotmentDataForPrinter = await _collectionModuleBL.GetAllotmentDataForPrinter(AccCollectionUID);
            return AllotmentDataForPrinter;
        }

        public override async Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID)
        {
            CurrencyRateRecords = await _collectionModuleBL.GetCurrencyRateRecords(StoreUID);
            return CurrencyRateRecords;
        }

        public override async Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID)
        {
            ViewMultiCurrencyDetailsData = await _collectionModuleBL.GetMultiCurrencyDetails(AccCollectionUID);
            return ViewMultiCurrencyDetailsData;
        }
        public List<IStore> CustomerName { get; set; } = new List<IStore>();
        public override async Task<List<IStore>> GetCustomerCodeName()
        {
            CustomerName = await _collectionModuleBL.GetCustomerCode("");
            return CustomerName;
        }
        
        public override async Task<List<ISetting>> GetSettings()
        {
            Settings = (await _collectionModuleBL.GetSettingByType("")).ToList();
            return Settings;
        }
        public override async Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate)
        {
            return await _collectionModuleBL.GetPaymentSummary(FromDate, ToDate);
        }
        public override async Task GetBank()
        {
            Banks = (await _collectionModuleBL.GetBankNames()).ToList();
        }
        public override async Task<decimal> GetCollectionLimitForLoggedInUser(string EmpUID)
        {
           return CollectionLimit = await _collectionModuleBL.CheckCollectionLimitForLoggedInUser(EmpUID);
        }
        public override async Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
        {
           return await _collectionModuleBL.UpdateCollectionLimit(Limit, EmpUID, Action);
        }

        #region NotImplementedMethods
        public override Task TableRowClickUI(AccPayable timePeriod, string totalRecords)
        {
            throw new NotImplementedException();
        }

        public override Task ChangeRecords(string ReceiptID)
        {
            throw new NotImplementedException();
        }

        public override Task CheckCustomerEligibleforDiscount()
        {
            throw new NotImplementedException();
        }

        public override Task BindInvoiceTable()
        {
            throw new NotImplementedException();
        }

        public override Task UnSettlePopUpRecords(bool _bankPop)
        {
            throw new NotImplementedException();
        }

        public override Task Paymentmode(string ReceiptNum, string UID, ChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override Task<List<SelectionItem>> PaymentModeSelectionItem(List<SelectionItem> Item)
        {
            throw new NotImplementedException();
        }

        public override Task<string> PaymentModeSelection(string Mode)
        {
            throw new NotImplementedException();
        }

        public override Task DocumentDataUI(string ReceiptNumber)
        {
            throw new NotImplementedException();
        }

        public override Task CurrencyData(string ReceiptNumber)
        {
            throw new NotImplementedException();
        }

        public override Task CurrencyChange()
        {
            throw new NotImplementedException();
        }

        public override Task PopulateUISide(string CustomerCode)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Change(HashSet<object> hashSet)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {
            throw new NotImplementedException();
        }

        

        public override Task<HttpStatusCode> CreateOnAccount(decimal _OnAccountBalance)
        {
            throw new NotImplementedException();
        }

        public override Task UpdateDetails(decimal creditAmt, AccPayable CreditNoteRecords)
        {
            throw new NotImplementedException();
        }

        public override Task CreateReceipt(bool AutoAllocate, List<ICollections> collectionListRecords)
        {
            throw new NotImplementedException();
        }
        #endregion
        public class PaymentInfo
        {
            public string PaymentType { get; set; }
            public string BranchName { get; set; }
            public string BankName { get; set; }
            public string AccountNumber { get; set; }
            public DateTime PaymentDate { get; set; } = DateTime.Now;
            public decimal Amount { get; set; }
            public bool IsChecked { get; set; }
            public string Type { get; set; } = "";
            public decimal DiscountValue { get; set; }
        }
    }
}

