using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Globalization;
using System.Linq;
using System.Net;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using AccCollectionPaymentMode = Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode;
using AccPayable = Winit.Modules.CollectionModule.Model.Classes.AccPayable;

namespace Winit.Modules.CollectionModule.BL.Classes.CreatePayment
{
    public class CreatePaymentWebViewModel : CreatePaymentBaseViewModel
    {
        private readonly ApiService _apiService;

        public CreatePaymentWebViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser, ApiService apiService) : base(serviceProvider, appConfig, appUser)
        {
            _apiService = apiService;
        }
        public override async Task<bool> Change(HashSet<object> hashSet)
        {
            try
            {
                _creditNoteRecords = new HashSet<Model.Classes.AccPayable>();
                HashSet<Model.Classes.AccPayable> accPayableHashSet = ConvertToaccPayableHashSet(hashSet);
                List<Model.Classes.AccPayable> selectedItems = new List<Model.Classes.AccPayable>();
                selectedItems = accPayableHashSet.ToList();
                SelectedInvoiceRecords = new HashSet<Model.Classes.AccPayable>();
                SelectedInvoiceRecords = accPayableHashSet;
                selectedItemsConstant = accPayableHashSet;
                _outstandingRecordsAmount = new List<InvoiceInfo>();
                _enteredAmountInv = new List<decimal>();
                foreach (var item in selectedItems)
                {
                    enteredAmount = 0;
                    TargetUID1 = item.ReferenceNumber;
                    SelectedInvoiceRecords = accPayableHashSet;
                    if (item.EnteredAmount > 0)
                    {
                        enteredAmount = item.EnteredAmount;
                        if (enteredAmount <= item.BalanceAmount && enteredAmount != 0)
                        {
                            item.isButtonEnabled = false;
                            _enteredAmountInv.Add(enteredAmount);
                            _outstandingRecordsAmount.Add(new InvoiceInfo { EnteredAmount = item.EnteredAmount, InvoiceDate = item.TransactionDate, InvoiceNo = item.ReferenceNumber, SourceType = item.SourceType });
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        item.EnteredAmount = item.BalanceAmount;
                        enteredAmount = item.BalanceAmount;
                        if (enteredAmount <= item.BalanceAmount && enteredAmount != 0)
                        {
                            item.isButtonEnabled = false;
                            _outstandingRecordsAmount.Add(new InvoiceInfo { EnteredAmount = item.EnteredAmount, InvoiceDate = item.TransactionDate, InvoiceNo = item.ReferenceNumber, SourceType = item.SourceType });
                        }
                    }
                }
                foreach (var cred in SelectedInvoiceRecords)
                {
                    if (cred.ReferenceNumber.Contains("CREDIT"))
                    {
                        SelectedInvoiceRecords.Remove(cred);
                        _creditNoteRecords.Add(cred);
                    }
                }
                reaarangedList = SelectedInvoiceRecords.OrderBy(r => r.TransactionDate).ToList();
                SelectedInvoiceRecords = new HashSet<Model.Classes.AccPayable>(reaarangedList);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public override async Task<bool> Product_AfterCheckBoxSelection(HashSet<object> hashSet)
        {
            try
            {
                _creditNoteRecords = new HashSet<Model.Classes.AccPayable>();
                HashSet<Model.Classes.AccPayable> accPayableHashSet = ConvertToaccPayableHashSet(hashSet);
                List<Model.Classes.AccPayable> selectedItems = new List<Model.Classes.AccPayable>();
                selectedItems = accPayableHashSet.ToList();
                SelectedInvoiceRecords = new HashSet<Model.Classes.AccPayable>();
                SelectedInvoiceRecords = accPayableHashSet;
                foreach (var cred in SelectedInvoiceRecords)
                {
                    if (cred.ReferenceNumber.Contains("CREDIT"))
                    {
                        SelectedInvoiceRecords.Remove(cred);
                        _creditNoteRecords.Add(cred);
                    }
                }
                reaarangedList = SelectedInvoiceRecords.OrderBy(r => r.TransactionDate).ToList();
                SelectedInvoiceRecords = new HashSet<Model.Classes.AccPayable>(reaarangedList);
                accPayableHashSet = SelectedInvoiceRecords;
                selectedItemsConstant = accPayableHashSet;
                _enteredAmountInv = new List<decimal>();
                _outstandingRecordsAmount = new List<InvoiceInfo>();
                var check = selectedItems.Count == 0 ? EmptyTextBoxes() : "";
                foreach (var item in selectedItems)
                {
                    enteredAmount = 0;
                    TargetUID1 = item.ReferenceNumber;
                    SelectedInvoiceRecords = accPayableHashSet;
                    if (item.EnteredAmount > 0)
                    {
                        enteredAmount = item.EnteredAmount;
                        if (enteredAmount <= item.BalanceAmount && enteredAmount != 0)
                        {
                            item.isButtonEnabled = false;
                            _enteredAmountInv.Add(enteredAmount);
                            _outstandingRecordsAmount.Add(new InvoiceInfo { EnteredAmount = item.EnteredAmount, InvoiceDate = item.TransactionDate, InvoiceNo = item.ReferenceNumber, SourceType = item.SourceType });
                        }
                        else
                        {
                            return false;
                        }
                        PopulateTextBoxes(selectedItems);
                    }
                    else
                    {
                        item.EnteredAmount = item.BalanceAmount;
                        enteredAmount = item.BalanceAmount;
                        if (enteredAmount <= item.BalanceAmount && enteredAmount != 0)
                        {
                            item.isButtonEnabled = false;
                            _outstandingRecordsAmount.Add(new InvoiceInfo { EnteredAmount = item.EnteredAmount, InvoiceDate = item.TransactionDate, InvoiceNo = item.ReferenceNumber, SourceType = item.SourceType });
                        }
                        foreach (var itm in ResponseDaystabList)
                        {
                            if (itm.ReferenceNumber == item.ReferenceNumber)
                            {
                                itm.EnteredAmount = item.EnteredAmount;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void PopulateTextBoxes(List<Model.Classes.AccPayable> selectedItems)
        {
            foreach (var itm in ResponseDaystabList)
            {
                if (selectedItems.Any(p => p.ReferenceNumber == itm.ReferenceNumber))
                {
                    if (itm.EnteredAmount == 0)
                    {
                        itm.EnteredAmount = itm.BalanceAmount;
                    }
                }
                else
                {
                    itm.EnteredAmount = 0;
                }
            }
        }
        public string EmptyTextBoxes()
        {
            foreach (var itm in ResponseDaystabList)
            {
                itm.EnteredAmount = 0;
            }
            return "";
        }
        public override async Task GetBank()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Winit.Modules.Bank.Model.Classes.Bank[]> response = await _apiService.FetchDataAsync<Winit.Modules.Bank.Model.Classes.Bank[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetBankNames", HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    ReceiptBank = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " recceipt.razor exception");
            }
        }
        public decimal SelectedPaymentAmount { get; set; } = 0;
        public List<decimal> Discounts { get; set; } = new List<decimal>();
        public decimal DiscountAmount { get; set; } = 0;
        public decimal CreditAmount { get; set; } = 0;
        public override async Task UpdateDetails(decimal Amount, AccPayable creditNoteRecords)
        {
            try
            {
                ICollections collection = new Collections();
                collection.AccCollection = new AccCollection();
                collection.AccPayable = new List<IAccPayable>();
                collection.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                collection.AccStoreLedger = new AccStoreLedger();
                collection.AccCollectionSettlement = new AccCollectionSettlement();
                collection.AccReceivable = new List<IAccReceivable>();
                collection.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                collection.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
                switch (PaymentMode)
                {
                    case "Cash":
                        SelectedPaymentAmount = Amount == 0 ? collectionAmount.CashAmount : Amount;
                        break;
                    case "Cheque":
                        SelectedPaymentAmount = collectionAmount.ChequeAmount;
                        break;
                    case "POS":
                        SelectedPaymentAmount = collectionAmount.POSAmount;
                        break;
                    case "Online":
                        SelectedPaymentAmount = collectionAmount.OnlineAmount;
                        break;
                    default:
                        SelectedPaymentAmount = Amount;
                        break;
                }

                foreach (var Inv in SelectedInvoiceRecords.Where(p => p.edit))
                {
                    if (Amount == 0)
                    {
                        await ApplyDiscount(Inv);
                    }
                    if (SelectedPaymentAmount > Inv.EnteredAmount)
                    {
                        AccCollectionAllotment allotment = new AccCollectionAllotment();
                        allotment.PaidAmount = Inv.EnteredAmount;
                        allotment.DiscountAmount = DiscountAmount;
                        allotment.TargetType = Inv.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE";
                        allotment.ReferenceNumber = Inv.ReferenceNumber;
                        allotment.TargetUID = Inv.ReferenceNumber;
                        allotment.StoreUID = Inv.StoreUID;
                        collection.AccCollectionAllotment.Add(allotment);
                        if (allotment.DiscountAmount > 0)
                        {
                            CreateDiscountRecord(collection, DiscountAmount, Inv);
                        }
                        await CreateJSONForPayableAndReceivable(collection, Inv, Inv.EnteredAmount + DiscountAmount, Amount, creditNoteRecords);
                        SelectedPaymentAmount -= Inv.EnteredAmount;
                        Inv.EnteredAmount = 0;
                        Inv.edit = false;
                    }
                    else
                    {
                        AccCollectionAllotment allotment = new AccCollectionAllotment();
                        allotment.PaidAmount = SelectedPaymentAmount;
                        allotment.DiscountAmount = DiscountAmount;
                        allotment.TargetType = Inv.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE";
                        allotment.ReferenceNumber = Inv.ReferenceNumber;
                        allotment.TargetUID = Inv.ReferenceNumber;
                        allotment.StoreUID = Inv.StoreUID;
                        collection.AccCollectionAllotment.Add(allotment);
                        if (allotment.DiscountAmount > 0)
                        {
                            CreateDiscountRecord(collection, DiscountAmount, Inv);
                        }
                        await CreateJSONForPayableAndReceivable(collection, Inv, SelectedPaymentAmount + DiscountAmount + CreditNoteAmount(Inv), Amount, creditNoteRecords);
                        Inv.EnteredAmount -= SelectedPaymentAmount;
                        Inv.edit = Inv.EnteredAmount == 0 ? false : true;
                        SelectedPaymentAmount = 0;
                        break;
                    }
                }
                collection.AccCollection.ReceiptNumber = CustCode + "_" + sixGuidstring();
                if (payments.Count(f => f.IsSelected) > 1)
                {
                    collection.AccCollection.IsMultimode = true;
                    collection.AccCollection.ConsolidatedReceiptNumber = CustCode + "_" + sixGuidstring();
                }
                else
                {
                    collection.AccCollection.IsMultimode = false;
                    collection.AccCollection.ConsolidatedReceiptNumber = collection.AccCollection.ReceiptNumber;
                }
                collection.AccCollection.CollectedDate = selectedDate == null ? DateTime.Now : selectedDate = selectedDate.Value.Date + DateTime.Now.TimeOfDay;
                collection.AccCollection.TripDate = TripDate;
                collection.AccCollection.DistributionChannelUID = "";
                collection.AccCollection.Amount = PaymentMode == "Cash" ? (Amount == 0 ? collectionAmount.CashAmount : Amount) : PaymentMode == "Cheque" ? collectionAmount.ChequeAmount : (PaymentMode == "POS" ? collectionAmount.POSAmount : (PaymentMode == "Online" ? collectionAmount.OnlineAmount : Amount));
                collection.AccCollection.Amount = Amount == 0 ? collection.AccCollection.Amount / Rate - Discounts.Sum() : 0;
                collection.AccCollection.CurrencyUID = string.IsNullOrEmpty(CurrencyUID) ? "INR" : CurrencyUID;
                collection.AccCollection.DefaultCurrencyUID = "INR";
                collection.AccCollection.DefaultCurrencyExchangeRate = 1;
                collection.AccCollection.DefaultCurrencyAmount = collection.AccCollection.Amount;
                collection.AccCollection.OrgUID = _appUser.SelectedJobPosition.OrgUID == null ? "" : _appUser.SelectedJobPosition.OrgUID;
                collection.AccCollection.JobPositionUID = _appUser.SelectedJobPosition.UID == null ? "" : _appUser.SelectedJobPosition.UID;
                collection.AccCollection.StoreUID = StoreUID ?? _appUser.SelectedCustomer.StoreUID ;
                collection.AccCollection.RouteUID = "DIST001_Route1";
                collection.AccCollection.EmpUID = _appUser.Emp.UID == null ? "" : _appUser.Emp.UID;
                collection.AccCollection.Status = PaymentMode == "Cash" ? "Collected" : "Submitted";
                collection.AccCollection.Remarks = "";
                collection.AccCollection.ReferenceNumber = PaymentMode == "Cheque" ? collectionAmount.ChequeNo : (PaymentMode == "POS" ? collectionAmount.POSNo : (PaymentMode == "Online" ? collectionAmount.OnlineNo : ""));
                collection.AccCollection.IsRealized = false;
                collection.AccCollection.Longitude = "0";
                collection.AccCollection.CreditNote = Amount != 0 ? "CREDITNOTE" : "";
                collection.AccCollection.Latitude = "0";
                collection.AccCollection.Source = "SFA";
                collection.AccCollection.Category = PaymentMode;
                collection.AccCollection.UID = Guidstring();
                collection.AccCollection.CreatedBy = _appUser.Emp.UID;
                collection.AccCollection.ModifiedBy = _appUser.Emp.UID;
                collection.AccCollection.AdvancePaidDays = AdvanceDays;
                collection.AccCollection.DiscountValue = Discountval;
                collection.AccCollection.IsActive = true;
                collection.AccCollection.DiscountAmount = Discounts.Sum();
                collection.AccCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
                collection.AccCollectionSettlement.UID = (Guid.NewGuid()).ToString();
                collection.AccStoreLedger.UID = (Guid.NewGuid()).ToString();
                collection.AccCollection.IsEarlyPayment = Discounts.Sum() > 0;
                if (PaymentMode != "Cash")
                {
                    await UpdateChequeDetails(collection.AccCollectionPaymentMode);
                }
                if (MultiCurrencyDetailsData.ContainsKey(PaymentMode))
                {
                    await CreateMultiCurrencyDetails(PaymentMode, collection);
                }
                collectionListRecords.Add(collection);
                CreditAmount = 0;
                DiscountAmount = 0;
                Discounts.Clear();
                if (payments.Count(f => f.IsSelected) + _creditNoteRecords.Count == collectionListRecords.Count + (IsExcelUpload ? 0 : ReceiptCountMethod()))
                {
                    await CreateReceipt(false, collectionListRecords);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CreateJSONForPayableAndReceivable(ICollections collection, AccPayable Inv, decimal Amount, decimal CreditAmount, AccPayable CreditNoteRecords)
        {
            try
            {
                if (CreditAmount == 0)
                {
                    if (Inv.SourceType.Contains("INVOICE"))
                    {
                        AccPayable accPayable = new AccPayable();
                        accPayable.PaidAmount = Amount;
                        accPayable.ReferenceNumber = Inv.ReferenceNumber;
                        accPayable.StoreUID = Inv.StoreUID;
                        collection.AccPayable.Add(accPayable);
                    }
                    else
                    {
                        AccReceivable accReceivable = new AccReceivable();
                        accReceivable.PaidAmount = Amount;
                        accReceivable.ReferenceNumber = Inv.ReferenceNumber;
                        accReceivable.StoreUID = Inv.StoreUID;
                        collection.AccReceivable.Add(accReceivable);
                    }
                }
                else
                {
                    AccCollectionAllotment allotment = new AccCollectionAllotment();
                    allotment.PaidAmount = CreditNoteRecords.EnteredAmount;
                    allotment.StoreUID = CreditNoteRecords.StoreUID;
                    allotment.TargetType = CreditNoteRecords.SourceType.Contains("INVOICE") ? "INVOICE" : "CREDITNOTE";
                    allotment.ReferenceNumber = CreditNoteRecords.ReferenceNumber;
                    AccReceivable accReceivable = new AccReceivable();
                    accReceivable.StoreUID = CreditNoteRecords.StoreUID;
                    accReceivable.ReferenceNumber = CreditNoteRecords.ReferenceNumber;
                    accReceivable.PaidAmount = CreditNoteRecords.EnteredAmount;
                    AccPayable accPayable = new AccPayable();
                    accPayable.StoreUID = Inv.StoreUID;
                    accPayable.ReferenceNumber = Inv.ReferenceNumber;
                    accPayable.PaidAmount = Amount;
                    collection.AccPayable.Add(accPayable);
                    collection.AccReceivable.Add(accReceivable);
                    collection.AccCollectionAllotment.Add(allotment);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public decimal CreditNoteAmount(AccPayable Inv)
        {
            if (SelectedInvoiceRecords.Where(p => p.SourceType.Contains("INVOICE")).Last() == Inv)
            {
                return 0;
            }
            else
            {
                return CreditAmount;
            }
        }
        public decimal ReceiptCountMethod()
        {
            if (ReceiptCount <= 1)
            {
                if (collectionAmount.LockReceiptCondition)
                {
                    return ReceiptCount;
                }
                else
                {
                    return ReceiptCount == 0 ? 0 : ReceiptCount - 1;
                }
            }
            else
            {
                return ReceiptCount - 1;
            }
        }
        public async Task ApplyDiscount(AccPayable Inv)
        {
            try
            {
                if (SelectedInvoiceRecords.FirstOrDefault().Discount)
                {
                    DiscountAmount = 0;
                    if (SelectedInvoiceRecords.Where(p => p.SourceType.Contains("INVOICE")).Last() == Inv)
                    {
                        CreditAmount = _creditNoteRecords.Where(p => p.ReferenceNumber.Contains("CREDITNOTE")).Sum(p => p.EnteredAmount);
                    }
                    if (SelectedPaymentAmount >= Inv.EnteredAmount - CreditAmount && Inv.EnteredAmount == Inv.Amount)
                    {
                        SelectedPaymentAmount = SelectedPaymentAmount - ((Inv.EnteredAmount - CreditAmount) * Inv.DiscountValue / 100);
                        DiscountAmount = (Inv.EnteredAmount - CreditAmount) * Inv.DiscountValue / 100;
                        Discounts.Add(DiscountAmount);
                        Inv.EnteredAmount = Inv.EnteredAmount - (Inv.EnteredAmount - CreditAmount) * Inv.DiscountValue / 100;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected async Task UpdateChequeDetails(IAccCollectionPaymentMode paymentMode)
        {
            try
            {
                paymentMode.BankUID = PaymentMode == "Cheque" ? collectionAmount.ChequeBank : (PaymentMode == "POS" ? collectionAmount.POSBank : (PaymentMode == "Online" ? collectionAmount.OnlineBank : ""));
                paymentMode.Branch = PaymentMode == "Cheque" ? collectionAmount.ChequeBranchName : (PaymentMode == "POS" ? collectionAmount.POSBranchName : (PaymentMode == "Online" ? collectionAmount.OnlineBranchName : ""));
                paymentMode.ChequeDate = PaymentMode == "Cheque" ? collectionAmount.ChequeTransferDate : (PaymentMode == "POS" ? collectionAmount.POSTransferDate : (PaymentMode == "Online" ? collectionAmount.OnlineTransferDate : DateTime.Now.Date));
                paymentMode.ChequeNo = PaymentMode == "Cheque" ? collectionAmount.ChequeNo : (PaymentMode == "POS" ? collectionAmount.POSNo : (PaymentMode == "Online" ? collectionAmount.OnlineNo : ""));
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CreateMultiCurrencyDetails(string info, ICollections collection)
        {
            try
            {
                MultiCurrencyDetails = MultiCurrencyDetailsData[info];
                foreach (var data in MultiCurrencyDetails.Where(p => p.ConvertedAmount != 0))
                {
                    if (data.OriginalAmount != data.ConvertedAmount || data.ConvertedAmount != data.OriginalAmount)
                    {
                        await CreateRoundOffRecord(data, collection);
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
        private string _creditID { get; set; } = $"{DateTime.Now.Minute:D2}{DateTime.Now.Second:D2}{DateTime.Now.Millisecond:D3}";

        public async Task CreateRoundOffRecord(IExchangeRate exchangeRate, ICollections collection)
        {
            try
            {
                AccCollectionAllotment allotment = new AccCollectionAllotment();
                allotment.UID = (Guid.NewGuid()).ToString();
                allotment.StoreUID = SelectedInvoiceRecords.FirstOrDefault().StoreUID;
                allotment.PaidAmount = exchangeRate.OriginalAmount > exchangeRate.ConvertedAmount_Temp ?
                        (exchangeRate.OriginalAmount - exchangeRate.ConvertedAmount_Temp) * -1 : exchangeRate.ConvertedAmount_Temp - exchangeRate.OriginalAmount; ;
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
        public void CreateDiscountRecord(ICollections collection, decimal Amount, Model.Classes.AccPayable item1)
        {
            AccCollectionAllotment allotment = new AccCollectionAllotment();
            allotment.UID = (Guid.NewGuid()).ToString();
            allotment.PaidAmount = Amount;
            allotment.DiscountAmount = 0;
            allotment.TargetType = "CREDITNOTE";
            allotment.StoreUID = item1.StoreUID;
            allotment.ReferenceNumber = "";
            collection.AccCollectionAllotment.Add(allotment);
        }
        public override async Task<HttpStatusCode> CreateOnAccount(decimal _OnAccountBalance)
        {
            onAccountcollection.AccPayable = new List<IAccPayable>();
            onAccountcollection.AccCollection = new AccCollection();
            onAccountcollection.AccCollectionPaymentMode = new AccCollectionPaymentMode();
            onAccountcollection.AccStoreLedger = new AccStoreLedger();
            onAccountcollection.AccCollectionSettlement = new AccCollectionSettlement();
            onAccountcollection.AccReceivable = new List<IAccReceivable>();
            onAccountcollection.AccCollectionAllotment = new List<IAccCollectionAllotment>();
            onAccountcollection.AccCollectionCurrencyDetails = new List<IAccCollectionCurrencyDetails>();
            onAccountcollection.AccCollection.ReceiptNumber = "OA - " + CustCode + "_" + sixGuidstring();
            if (payments.Count(f => f.IsSelected) > 1)
            {
                onAccountcollection.AccCollection.IsMultimode = true;
                onAccountcollection.AccCollection.ConsolidatedReceiptNumber = "OA - " + CustCode + "_" + Consolidated_Current_Date;
            }
            else
            {
                onAccountcollection.AccCollection.IsMultimode = false;
                onAccountcollection.AccCollection.ConsolidatedReceiptNumber = onAccountcollection.AccCollection.ReceiptNumber;
            }
            onAccountcollection.AccCollection.CollectedDate = selectedDate == null ? DateTime.Now : selectedDate;
            onAccountcollection.AccCollection.TripDate = TripDate;
            onAccountcollection.AccCollection.DistributionChannelUID = "";
            onAccountcollection.AccCollection.CurrencyUID = CurrencyUID;
            onAccountcollection.AccCollection.DefaultCurrencyUID = "INR";
            onAccountcollection.AccCollection.DefaultCurrencyExchangeRate = 1;
            onAccountcollection.AccCollection.Amount = _OnAccountBalance == 0 ? PaymentMode == "Cash" ? collectionAmount.CashAmount : PaymentMode == "Cheque" ? collectionAmount.ChequeAmount : (PaymentMode == "POS" ? collectionAmount.POSAmount : (PaymentMode == "Online" ? collectionAmount.OnlineAmount : 0)) : _OnAccountBalance / Rate;
            OnAccountBalance = onAccountcollection.AccCollection.Amount;
            onAccountcollection.AccCollection.DefaultCurrencyAmount = onAccountcollection.AccCollection.Amount * Rate;
            onAccountcollection.AccCollection.OrgUID = _appUser.SelectedJobPosition.OrgUID;
            onAccountcollection.AccCollection.JobPositionUID = _appUser.SelectedJobPosition.UID;
            onAccountcollection.AccCollection.StoreUID = StoreUID ?? _appUser.SelectedCustomer.StoreUID;
            onAccountcollection.AccCollection.RouteUID = _appUser.SelectedRoute == null ? null : _appUser.SelectedRoute.UID;
            onAccountcollection.AccCollection.EmpUID = _appUser.Emp.UID;
            onAccountcollection.AccCollection.Status = PaymentMode == "Cash" ? "Collected" : "Submitted";
            onAccountcollection.AccCollection.Remarks = "";
            onAccountcollection.AccCollection.ReferenceNumber = PaymentMode == "Cheque" ? collectionAmount.ChequeNo : (PaymentMode == "POS" ? collectionAmount.POSNo : (PaymentMode == "Online" ? collectionAmount.OnlineNo : ""));
            onAccountcollection.AccCollection.IsRealized = false;
            onAccountcollection.AccCollection.Longitude = "0";
            onAccountcollection.AccCollection.Latitude = "0";
            onAccountcollection.AccCollection.Source = "SFA";
            onAccountcollection.AccCollection.Category = PaymentMode;
            onAccountcollection.AccCollection.UID = Guidstring();
            onAccountcollection.AccCollection.CreatedBy = _appUser.Emp.UID;
            onAccountcollection.AccCollection.ModifiedBy = _appUser.Emp.UID;
            if (PaymentMode != "Cash")
            {
                await UpdateChequeDetails(onAccountcollection.AccCollectionPaymentMode);
            }
            if (MultiCurrencyDetailsData.ContainsKey(PaymentMode))
            {
                await CreateMultiCurrencyDetails(PaymentMode, onAccountcollection);
            }
            AccCollectionAllotment allotment = new AccCollectionAllotment();
            AccReceivable recev = new AccReceivable();
            allotment.UID = (Guid.NewGuid()).ToString();
            allotment.StoreUID = StoreUID;
            if (PaymentMode != "Cash")
            {
                allotment.DiscountAmount = onAccountcollection.AccCollection.Amount;
                allotment.EarlyPaymentDiscountAmount = onAccountcollection.AccCollection.Amount;
            }
            allotment.TargetUID = (Guid.NewGuid()).ToString(); ;
            allotment.TargetType = "OA - CREDITNOTE";
            recev.UID = allotment.TargetUID;
            onAccountcollection.AccReceivable.Add(recev);
            onAccountcollection.AccCollectionAllotment.Add(allotment);
            ++OnAccountImp;
            await OnAccountCreationUI(onAccountcollection);
            if ((HttpStatusCode)response.StatusCode == HttpStatusCode.Created)
            {
                OnAccountSuccessFlag = true;
                return HttpStatusCode.Created;
            }
            else
            {
                OnAccountSuccessFlag = false;
                return HttpStatusCode.NotFound;
            }
        }

        public async Task OnAccountCreationUI(ICollections onAccountcollection)
        {
            try
            {
                response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CreateOnAccountReceipt", HttpMethod.Post, onAccountcollection);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " recceipt.razor exception");
            }
        }
        private HashSet<Model.Classes.AccPayable> ConvertToaccPayableHashSet(HashSet<object> originalHashSet)
        {
            HashSet<Model.Classes.AccPayable> accPayableHashSet = new HashSet<Model.Classes.AccPayable>();
            foreach (var originalObject in originalHashSet)
            {
                Model.Classes.AccPayable accPayableObject = ConvertToAccPayableObject(originalObject);
                accPayableHashSet.Add(accPayableObject);
            }

            return accPayableHashSet;
        }
        private Model.Classes.AccPayable ConvertToAccPayableObject(object originalObject)
        {
            Model.Classes.AccPayable elementalObject = new Model.Classes.AccPayable();

            // Use reflection to copy properties with matching names and types
            foreach (var originalProperty in originalObject.GetType().GetProperties())
            {
                var elementalProperty = typeof(Model.Classes.AccPayable).GetProperty(originalProperty.Name);

                if (elementalProperty != null && elementalProperty.PropertyType == originalProperty.PropertyType)
                {
                    elementalProperty.SetValue(elementalObject, originalProperty.GetValue(originalObject));
                }
            }

            return elementalObject;
        }

        public override async Task CreateReceipt(bool AutoAllocate, List<ICollections> collectionListRecords)
        {
            try
            {
                if (AutoAllocate)
                {
                    response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CreateReceiptWithAutoAllocation", HttpMethod.Post, collectionListRecords.ToArray());
                    collectionListRecords.Clear();
                }
                else
                {
                    response = await _apiService.FetchDataAsync($"{_appConfig.ApiBaseUrl}CollectionModule/CreateReceipt", HttpMethod.Post, collectionListRecords.ToArray());
                    collectionListRecords.Clear();
                }
                if (response.StatusCode == 201)
                {
                    IsReceiptCreated = true;
                    payments.Clear();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " recceipt.razor exception");
            }
        }


        //Collection

        public override async Task PopulateUISide(string CustomerCode)
        {
            try
            {
                selectedDate = DateTime.ParseExact(_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                Winit.Shared.Models.Common.ApiResponse<AccCustomer[]> response = await _apiService.FetchDataAsync<AccCustomer[]>(
                    $"{_appConfig.ApiBaseUrl}CollectionModule/GetAllCustomersBySalesOrgCode?SessionUserCode=" + _appUser.SelectedJobPosition.OrgUID,
                    HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    Responsedata = response.Data;
                }
                if (Responsedata.Any()) // Check if Responsedata has elements
                {
                    selectedValue1 = Responsedata.First().UID + "|" + Responsedata.First().Name + "|" + Responsedata.First().Code;
                    selectedValueText = Responsedata.First().Name;
                    StoreUID = Responsedata.First().UID;
                }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task TableRowClickUI(AccPayable timePeriod, string totalRecords)
        {
            try
            {
                range = timePeriod.DelayTime;
                _count = timePeriod.StoreUID != null ? Convert.ToInt64(timePeriod.Count) : 0;
                _count = totalRecords.IsNullOrEmpty() ? _count : _count1;
                string input = string.IsNullOrEmpty(timePeriod.DelayTime) ? "" : timePeriod.DelayTime;
                ReceiptID = timePeriod.Count != 0 ? timePeriod.StoreUID : StoreUID;
                ReceiptID = ReceiptID != null ? ReceiptID : StoreUID;
                int start = 0;
                int end = 0;
                if (input.Contains("+"))
                {
                    string[] parts = input.Split('+');
                    int.TryParse(parts[0], out start);
                    _start = start;
                }
                else if (totalRecords != null && totalRecords != "")
                {
                    _start = 0;
                    _end = 0;
                }
                else
                {
                    string[] parts = input.Split('-');
                    int.TryParse(parts[0], out start);
                    _start = start;
                    string[] partsChild = parts[1].Split(" ");
                    int.TryParse(partsChild[0], out end);
                    _end = end;
                }

            }
            catch (Exception ex)
            {

            }

        }
        public override async Task ChangeRecords(string ReceiptID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccPayable[]> responseDays = await _apiService.FetchDataAsync<AccPayable[]>($"{_appConfig.ApiBaseUrl}CollectionModule/DaysTableParent?StoreUID=" + ReceiptID + "&startDay=" + _start + "&endDay=" + _end, HttpMethod.Get, null);
                if (responseDays != null && responseDays.Data != null)
                {
                    ResponseDaystable = responseDays.Data;
                }
                ResponseDaystabList = ResponseDaystable.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task BindInvoiceTable()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccPayable[]> responseDays = await _apiService.FetchDataAsync<AccPayable[]>($"{_appConfig.ApiBaseUrl}CollectionModule/DaysTable?StoreUID=" + StoreUID, HttpMethod.Get, null);
                if (responseDays != null && responseDays.Data != null)
                {
                    ResponseDaystab = responseDays.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task UnSettlePopUpRecords(bool _bankPop)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccCollectionPaymentMode[]> response = await _apiService.FetchDataAsync<AccCollectionPaymentMode[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetUnSettleAmount?AccCollectionUID=" + StoreUID, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    bank = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task CheckCustomerEligibleforDiscount()
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<EarlyPaymentDiscountConfiguration[]> eligibilityRecords = await _apiService.FetchDataAsync<EarlyPaymentDiscountConfiguration[]>($"{_appConfig.ApiBaseUrl}CollectionModule/CheckEligibleForDiscount?ApplicableCode=" + StoreUID, HttpMethod.Get, null);
                if (eligibilityRecords != null && eligibilityRecords.Data != null)
                {
                    EligibleRecords = eligibilityRecords.Data;
                }
                bool IsOverDue = await CheckOverDue(EligibleRecords);
                if (!IsOverDue)
                {
                    await CheckAdvanceDays(EligibleRecords);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }

        public async Task<bool> CheckOverDue(EarlyPaymentDiscountConfiguration[] EligibleRecords)
        {
            try
            {
                DateTime today = DateTime.Now;
                OverDueRecords = ResponseDaystabList.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate < today).ToList();
                OnTimeRecords = ResponseDaystabList.Where(t => t.SourceType.Contains("INVOICE") && t.DueDate > today).ToList();
                if (OverDueRecords.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }

        public async Task CheckAdvanceDays(EarlyPaymentDiscountConfiguration[] EligibleRecords)
        {
            try
            {
                AdvanceDays = EligibleRecords[0].Advance_Paid_Days;
                Discountval = EligibleRecords[0].Discount_Value;
                foreach (var list in OnTimeRecords)
                {
                    int diff = Convert.ToInt32((DateTime.Now - list.TransactionDate)?.TotalDays);
                    if (diff > 0 && diff < AdvanceDays && list.SourceType.Contains("INVOICE"))
                    {
                        foreach (var item in ResponseDaystabList)
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task Paymentmode(string ReceiptNum, string UID, ChangeEventArgs e)
        {
            try
            {
                ResponseDaystabList = new List<AccPayable>();
                Payment[] ResData = new Payment[0];
                ResponsedData = ResData;
                var selectedValue = "";
                if (e != null)
                {
                    selectedValue = e.Value.ToString();
                }
                else
                {
                    selectedValue = selectedValue1;
                }
                var values = selectedValue.Split('|');

                if (values.Length == 3)
                {
                    var selectedUID = values[0];
                    var selectedName = values[1];
                    var Code = values[2];
                    CustomerName = selectedName;
                    CustCode = Code;
                    UID = selectedUID;
                }
                else
                {
                    TotalTableAmt = 0;
                    ResponseDaystab = new AccPayable[0];
                    throw new Exception();
                }
                Winit.Shared.Models.Common.ApiResponse<ExchangeRate[]> apiResponse = await _apiService.FetchDataAsync<ExchangeRate[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAllConfiguredCurrencyDetailsBySalesOrgCode?SessionUserCode=" + ReceiptNum, HttpMethod.Get);
                if (apiResponse != null && apiResponse.Data != null)
                {
                    CurrencyDetails = apiResponse.Data;
                }
                else
                {
                    CurrencyDetails = Array.Empty<ExchangeRate>();
                }
                Winit.Shared.Models.Common.ApiResponse<Payment[]> response = await _apiService.FetchDataAsync<Payment[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAllConfiguredPaymentModesBySalesOrgCode?SessionUserCode=" + UID, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    ResponsedData = response.Data;
                }
                else
                {
                    ResponsedData = Array.Empty<Payment>();
                }
                StoreUID = ResponsedData.FirstOrDefault(item => ResponsedData != null).StoreUID;
                ReceiptNumber = ResponsedData.FirstOrDefault(item => "op" != "ji").ReceiptNumber;


                Winit.Shared.Models.Common.ApiResponse<AccPayable[]> responseDays = await _apiService.FetchDataAsync<AccPayable[]>($"{_appConfig.ApiBaseUrl}CollectionModule/DaysTable?StoreUID=" + StoreUID, HttpMethod.Get, null);
                if (responseDays != null && responseDays.Data != null)
                {
                    ResponseDaystab = responseDays.Data;
                }
                else
                {
                    ResponseDaystab = Array.Empty<AccPayable>();
                }
            }
            catch (Exception ex)
            {
                ResponseDaystabList = new List<AccPayable>();
                StoreUID = UID;
            }
        }
        public override async Task DocumentDataUI(string ReceiptNumber)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<AccDocument[]> response = await _apiService.FetchDataAsync<AccDocument[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAllConfiguredDocumentTypesBySalesOrgCode?SessionUserCode=" + ReceiptNumber, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    Response = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }

        public override async Task CurrencyData(string ReceiptNumber)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<Payment[]> response = await _apiService.FetchDataAsync<Payment[]>($"{_appConfig.ApiBaseUrl}CollectionModule/GetAllConfiguredCurrencyDetailsBySalesOrgCode?SessionUserCode=" + ReceiptNumber, HttpMethod.Get, null);
                if (response != null && response.Data != null)
                {
                    Respon = response.Data;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " collection.razor exception");
            }
        }
        public override async Task CurrencyChange()
        {
            try
            {
                CurrencyUID = "INR";
                Rate = 1;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }
        public override async Task<string> PaymentModeSelection(string Mode)
        {
            try
            {
                await Task.CompletedTask;
                return PaymentMode = Mode;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public override async Task<List<SelectionItem>> PaymentModeSelectionItem(List<SelectionItem> Item)
        {
            try
            {
                await Task.CompletedTask;
                return payments = Item;
            }
            catch (Exception ex)
            {
                return new List<SelectionItem>();
            }
        }
        #region NotImplementedMethods
        public override Task PopulateViewModel()
        {
            throw new NotImplementedException();
        }

        public override Task OnSignatureProceedClick()
        {
            throw new NotImplementedException();
        }

        public override Task PrepareSignatureFields()
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            throw new NotImplementedException();
        }

        public override Task<string> CreateReceipt(ICollections collection)
        {
            throw new NotImplementedException();
        }

        public override Task<string> CreateOnAccount(ICollections collection, bool IsDirectOnAccount)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<IExchangeRate>?> GetCurrencyRateRecords(string StoreUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<ExchangeRate>> apiResponse =
                    await _apiService.FetchDataAsync<List<ExchangeRate>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetCurrencyRateRecords?StoreUID="
                    , HttpMethod.Get);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IExchangeRate>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        public override async Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<List<AccCollectionCurrencyDetails>> apiResponse =
                    await _apiService.FetchDataAsync<List<AccCollectionCurrencyDetails>>($"{_appConfig.ApiBaseUrl}CollectionModule/GetMultiCurrencyDetails?AccCollectionUID=" + AccCollectionUID
                    , HttpMethod.Get);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data.ToList<IAccCollectionCurrencyDetails>();
                }
                return default;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }

        public override Task<List<IStore>> GetCustomerCodeName()
        {
            throw new NotImplementedException();
        }
        public override Task<List<ISetting>> GetSettings()
        {
            throw new NotImplementedException();
        }

        public override Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate)
        {
            throw new NotImplementedException();
        }

        public override Task<decimal> GetCollectionLimitForLoggedInUser(string EmpUID)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
        {
            try
            {
                Winit.Shared.Models.Common.ApiResponse<bool> apiResponse =
                        await _apiService.FetchDataAsync<bool>($"{_appConfig.ApiBaseUrl}CollectionModule/UpdateCollectionLimit?Limit=" + Limit + "&EmpUID=" + EmpUID + "&Action=" + Action
                        , HttpMethod.Post);

                if (apiResponse != null && apiResponse.Data != null)
                {
                    return apiResponse.Data;
                }
                return default;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        #endregion
    }
}
