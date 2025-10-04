using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.CollectionModule.BL.Classes
{
    public class CollectionAppViewModel : ICollectionModuleViewModel
    {
        private readonly IServiceProvider _serviceProvider;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private readonly Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL _collectionModuleBL;
        public IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore> _customersList { get; set; }
        public IEarlyPaymentDiscountConfiguration[] EligibleRecords { get; set; } = new IEarlyPaymentDiscountConfiguration[0];

        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _PopulateList { get; set; }
        public List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> _invoiceList { get; set; }
        public List<IAccCollection> _collectionList { get; set; }
        public List<IAccCollectionAllotment> _allotmentList { get; set; }
        public int Count { get; set; }
        private List<IAccPayable> SelectedItems { get; set; } = new List<IAccPayable>();
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collection = new Collections();
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections[] collectionArray { get; set; } = new Collections[1];
        public string CustomerSignatureFolderPath { get; set; }
        public string UserSignatureFolderPath { get; set; }
        public string CustomerSignatureFileName { get; set; }
        public string UserSignatureFileName { get; set; }
        public string CustomerSignatureFilePath { get; set; }
        public string UserSignatureFilePath { get; set; }
        public string ConsolidatedReceiptNumber { get; set; }
        public string ReceiptNumber { get; set; }
        public string SignatureFolderPath { get; set; }
        public string CustomerName { get; set; }
        public List<IFileSys> SignatureFileSysList { get; set; } = new List<IFileSys>();
        public List<ICollectionPrint> CollectionStoreDataForPrinter { get; set; } = new List<ICollectionPrint>();
        public List<ICollectionPrintDetails> AllotmentDataForPrinter { get; set; } = new List<ICollectionPrintDetails>();
        public List<IAccCollectionPaymentMode> CPODetails { get; set; } = new List<IAccCollectionPaymentMode>();

        public string Guidstring()
        {
            string random = (Guid.NewGuid()).ToString();
            return random;
        }
        public string sixGuidstring()
        {
            Guid newGuid = Guid.NewGuid();

            // Convert the GUID to a string and take the first 8 characters without hyphens
            string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);

            return eightDigitGuid;
        }
        public void OnSignatureProceedClick()
        {
            CustomerSignatureFilePath = Path.Combine(CustomerSignatureFolderPath, CustomerSignatureFileName);
            UserSignatureFilePath = Path.Combine(UserSignatureFolderPath, UserSignatureFileName);
        }
        public void PrepareSignatureFields()
        {
            string baseFolder = Path.Combine(_appConfigs.BaseFolderPath, FileSysTemplateControles.GetSignatureFolderPath("Collection", SignatureFolderPath));
            CustomerSignatureFolderPath = baseFolder;
            UserSignatureFolderPath = baseFolder;
            CustomerSignatureFileName = $"Customer_{Guidstring()}.png";
            UserSignatureFileName = $"User_{Guidstring()}.png";
        }
        public CollectionAppViewModel(IServiceProvider serviceProvider, Winit.Modules.CollectionModule.BL.Interfaces.ICollectionModuleBL collectionModuleBL, Winit.Modules.Base.BL.ApiService _apiService, Winit.Shared.Models.Common.IAppConfig _appConfigs)
        {
            _serviceProvider = serviceProvider;
            _collectionModuleBL = collectionModuleBL;
            this._apiService = _apiService;
            this._appConfigs = _appConfigs;
        }
        public async Task PopulateViewModel()
        {
            await GetAllCustomersBySalesOrgCode(null, null);
        }

        public async Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            EligibleRecords = (await _collectionModuleBL.CheckEligibleForDiscount(ApplicableCode)).ToArray();
            return EligibleRecords;
        }

        public async Task<string> CreateReceipt(ICollections collection)
        {
            try
            {
                //if (RemainingAmt == 0)
                //{
                //    foreach (var item in SelectedItems.Where(p => p.ReferenceNumber.Contains("CREDITNOTE", StringComparison.OrdinalIgnoreCase)))
                //    {
                //        paymentInfos.Add(new PaymentInfo
                //        {
                //            PaymentType = item.ReferenceNumber,
                //            IsChecked = true,
                //            Amount = item.PayingAmount,
                //            Type = item.ReferenceNumber,
                //            // Set other properties as needed
                //        });
                //    }
                //    //collection.Category = list.PaymentType;
                //    foreach (var list in paymentInfos.Where(p => p.IsChecked && p.Amount != 0))
                //    {
                //        collection.accPayable = new List<IAccCollectionNew.AccPayable>();
                //        collection.accCollectionPaymentMode = new IAccCollection.AccCollectionPaymentMode();
                //        collection.accStoreLedger = new IAccCollectionNew.AccStoreLedger();
                //        collection.accReceivable = new List<AcccReceivable>();
                //        collection.AccCollectionAllotment = new List<AccCollectionAllotments>();
                //        collection.ReceiptNumber = _collection.CustCode + "_" + sixGuidstring();
                //        if (paymentInfos.Count(p => p.IsChecked) > 1)
                //        {
                //            collection.IsMultimode = true;
                //            collection.ConsolidatedReceiptNumber = _collection.CustCode + "_" + sixGuidstring();
                //        }
                //        else
                //        {
                //            collection.IsMultimode = false;
                //            collection.ConsolidatedReceiptNumber = collection.ReceiptNumber;
                //        }
                //        collection.CollectedDate = _collectedDate == null ? DateTime.Now : _collectedDate;
                //        collection.TripDate = TripDate;
                //        collection.DistributionChannelUID = "";
                //        collection.Amount = Convert.ToDecimal(PayingAmount);
                //        collection.CurrencyUID = "INR";
                //        collection.DefaultCurrencyUID = "INR";
                //        collection.DefaultCurrencyExchangeRate = 1;
                //        collection.DefaultCurrencyAmount = Convert.ToDecimal(PayingAmount);
                //        collection.OrgUID = _iAppUser.SelectedJobPosition.OrgUID;
                //        collection.JobPositionUID = _iAppUser.SelectedJobPosition.UID;
                //        collection.StoreUID = SelectedItems.FirstOrDefault().StoreUID;
                //        collection.RouteUID = "";
                //        collection.EmpUID = _iAppUser.Emp.LoginId;
                //        collection.Status = list.PaymentType == "Cash" ? "Collected" : "Submitted";
                //        collection.Remarks = "";
                //        collection.ReferenceNumber = paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).AccountNumber;
                //        collection.IsRealized = false;
                //        collection.Longitude = "0";
                //        collection.Latitude = "0";
                //        collection.Source = "App";
                //        collection.Category = list.PaymentType;
                //        collection.UID = Guidstring();
                //        collection.CreatedBy = "WINIT";
                //        collection.ModifiedBy = "WINIT";
                //        collection.accCollectionPaymentMode.UID = (Guid.NewGuid()).ToString();
                //        collection.accCollectionSettlement.UID = (Guid.NewGuid()).ToString();
                //        collection.accStoreLedger.UID = (Guid.NewGuid()).ToString();
                //        while (list.Amount != 0)
                //        {
                //            PopulateAllotment(list, paymentInfos);
                //        }
                //        string res = "";
                //        //result = await _collectionModuleViewModel.CreateReceipt(collection);
                //    }
                //}


               // collectionArray[0] = collection;
                string result = await _collectionModuleBL.CreateReceipt(collection);
                if (result.Contains("Success"))
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
                //return "1";
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> CreateOnAccount(ICollections collection)
        {
            string result = await _collectionModuleBL.CreateOnAccountReceipt(collection);
            if (result.Contains("Success"))
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        //public string Guidstring()
        //{
        //    return (Guid.NewGuid()).ToString();
        //}

        //public string sixGuidstring()
        //{
        //    Guid newGuid = Guid.NewGuid();

        //    // Convert the GUID to a string and take the first 8 characters without hyphens
        //    string eightDigitGuid = newGuid.ToString("N").Substring(0, 8);

        //    return eightDigitGuid;
        //}
        //public void PopulateAllotment(PaymentInfo list, List<PaymentInfo> paymentInfos)
        //{
        //    foreach (var items in SelectedItems)
        //    {
        //        AccCollectionAllotments allotment = new AccCollectionAllotments();
        //        AcccReceivable recev = new AcccReceivable();
        //        IAccCollectionNew.AccPayable pay = new IAccCollectionNew.AccPayable();
        //        if (items.PayingAmount != 0)
        //        {
        //            allotment.UID = (Guid.NewGuid()).ToString();
        //            allotment.StoreUID = items.StoreUID;
        //            allotment.TargetType = items.SourceType;
        //            allotment.ReferenceNumber = items.ReferenceNumber;
        //            if (paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).Amount > items.PayingAmount)
        //            {
        //                allotment.PaidAmount = Convert.ToDouble(items.PayingAmount);
        //                collection.AccCollectionAllotment.Add(allotment);
        //                PayRec(items, pay, recev, items.PayingAmount);
        //                ReceiveRecords(list, recev, items);
        //                paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).Amount -= items.PayingAmount;
        //                items.PayingAmount = 0;
        //            }
        //            else
        //            {
        //                allotment.PaidAmount = Convert.ToDouble(paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).Amount);
        //                collection.AccCollectionAllotment.Add(allotment);
        //                PayRec(items, pay, recev, paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).Amount);
        //                ReceiveRecords(list, recev, items);
        //                items.PayingAmount -= paymentInfos.FirstOrDefault(p => p.PaymentType == list.PaymentType).Amount;
        //                list.Amount = 0;
        //                break;
        //            }
        //        }
        //    }
        //}
        //public void PayRec(IAccPayable items, IAccCollectionNew.AccPayable pay, AcccReceivable recev, decimal payingAmount)
        //{
        //    if (items.ReferenceNumber.Contains("INVOICE"))
        //    {
        //        pay.ReferenceNumber = items.ReferenceNumber;
        //        pay.StoreUID = items.StoreUID;
        //        pay.SourceType = items.SourceType;
        //        pay.SourceUID = items.SourceUID;
        //        pay.PaidAmount = payingAmount;
        //        collection.accPayable.Add(pay);
        //    }
        //    else
        //    {
        //        recev.ReferenceNumber = items.ReferenceNumber;
        //        recev.StoreUID = items.StoreUID;   
        //        recev.PaidAmount = payingAmount;
        //        recev.SourceType = items.SourceType;
        //        recev.SourceUID = items.SourceUID;
        //        collection.accReceivable.Add(recev);
        //    }
        //}

        //public void ReceiveRecords(PaymentInfo list, AcccReceivable recev, IAccPayable items)
        //{
        //    if (list.Type != null && list.Type.Contains("CREDITNOTE"))
        //    {
        //        AccCollectionAllotments allotment = new AccCollectionAllotments();
        //        IAccPayable ReceiveRecords = SelectedItems.FirstOrDefault(item => item.ReferenceNumber == list.Type);
        //        recev.ReferenceNumber = ReceiveRecords.ReferenceNumber;
        //        recev.StoreUID = ReceiveRecords.StoreUID;
        //        recev.PaidAmount = ReceiveRecords.PayingAmount;
        //        recev.SourceType = ReceiveRecords.SourceType;
        //        recev.SourceUID = ReceiveRecords.SourceUID;
        //        allotment.UID = (Guid.NewGuid()).ToString();
        //        allotment.StoreUID = ReceiveRecords.StoreUID;
        //        allotment.TargetType = ReceiveRecords.SourceType;
        //        allotment.ReferenceNumber = ReceiveRecords.ReferenceNumber;
        //        allotment.PaidAmount = Convert.ToDouble(ReceiveRecords.PayingAmount);
        //        collection.accReceivable.Add(recev);
        //        collection.AccCollectionAllotment.Add(allotment);
        //    }
        //}

        //public void UpdateChequeDetails(string PaymentType, List<PaymentInfo> paymentInfos)
        //{
        //    collection.accCollectionPaymentMode.BankUID = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).BankName;
        //    collection.accCollectionPaymentMode.Branch = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).BranchName;
        //    collection.accCollectionPaymentMode.ChequeNo = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).AccountNumber;
        //    collection.accCollectionPaymentMode.Amount = Convert.ToDouble(paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).Amount);
        //    collection.accCollectionPaymentMode.ChequeDate = paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentType).PaymentDate;
        //}

        public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            _customersList = await _collectionModuleBL.GetAllCustomersBySalesOrgCode(SessionUserCode, SalesOrgCode);
            return _customersList;
        }



        public async Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            _PopulateList = await _collectionModuleBL.PopulateCollectionPage(CustomerCode, Tabs);
            return _PopulateList;
        }
        public async Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
        {
            _invoiceList = await _collectionModuleBL.GetInvoicesMobile(AccCollectionUID);
            return _invoiceList;
        }

        public async Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment)
        {
            _collectionList = await _collectionModuleBL.PaymentReceipts(FromDate, ToDate, Payment,"");
            
            return _collectionList;
        }
        public async Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID)
        {
            _allotmentList = await _collectionModuleBL.AllotmentReceipts(AccCollectionUID);
            
            return _allotmentList;
        }

        public async Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID)
        {
            CollectionStoreDataForPrinter = await _collectionModuleBL.GetCollectionStoreDataForPrinter(UID);
            return CollectionStoreDataForPrinter;
        }

        public async Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            AllotmentDataForPrinter = await _collectionModuleBL.GetAllotmentDataForPrinter(AccCollectionUID);
            return AllotmentDataForPrinter;
        }

        public async Task<List<IAccCollectionPaymentMode>> CPOData(string AccCollectionUID)
        {
            CPODetails = await _collectionModuleBL.CPOData(AccCollectionUID);
            return CPODetails;
        }
        //protected override Task PopulateUI_Data(string CustomerCode)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task TableRowClick_Data(Model.Classes.AccPayable timePeriod, string totalRecords)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task ChangeRecords_Data(string ReceiptID)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task BindInvoiceTable_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task UnSettlePopUpRecords_Data(bool _bankPop)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task CheckCustomerEligibleforDiscount_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task Paymentmode_Data(string ReceiptNum, string UID, ChangeEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task DocumentData_Data(string ReceiptNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task CurrencyData_Data(string ReceiptNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task CurrencyChange_Data(string[] values)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<string> PaymentModeSelection_Data(string Mode)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<List<SelectionItem>> PaymentModeSelectionItem_Data(List<SelectionItem> Item)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<bool> Change_Data(HashSet<object> hashSet)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<bool> Product_AfterCheckBoxSelection_Data(HashSet<object> hashSet)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetBank_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task UpdateDetails_Data(string creditAmt)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task CreateReceipt_Data(bool AutoAllocate, List<IAccCollectionNew> collectionListRecords)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<HttpStatusCode> CreateOnAccount_Data(decimal _OnAccountBalance)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task OnAccountCreation_Data(IAccCollectionNew onAccountcollection)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetInvoiceDetails_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetReceiptDetails_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetCustomerCodeName_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task ViewReceiptDetails_Data(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetCashierDetails_Data()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> Clicked_Data(string Status)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> SettleRecords_Data(List<string> Multiple)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task ShowAllTabssRecords_Data(int Count)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task GetChequeDetails_Data(string UID, string TargetUID)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> OnClickSettleReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string CashNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> OnClickApproveReject_Data(string UID, string Button, string Comments1, string SessionUserCode, string ReceiptNumber, string ChequeNo1)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task CheckReversalPossible_Data(string UID)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> ReceiptReversal_Data(string UID, decimal ChequeAmount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> ReceiptReverseByCash_Data(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<ApiResponse<string>> ReceiptVOIDByCash_Data(string ReceiptNumber, string Amount, string ChequeNo, string ReasonforCancelation)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task StatementReportCustomers_Data(string CustomerCode)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task ViewAccountStatement_Data(string FromDate, string ToDate, string Customer)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<string> AddEarlyPayment_Data(IEarlyPaymentDiscountConfiguration EarlyPayment)
        //{
        //    throw new NotImplementedException();
        //}
        //public class PaymentInfo
        //{
        //    public string PaymentType { get; set; }
        //    public string BranchName { get; set; }
        //    public string BankName { get; set; }
        //    public string AccountNumber { get; set; }
        //    public DateTime PaymentDate { get; set; }
        //    public decimal Amount { get; set; }
        //    public bool IsChecked { get; set; }
        //    public string Type { get; set; }
        //}
    }

}
