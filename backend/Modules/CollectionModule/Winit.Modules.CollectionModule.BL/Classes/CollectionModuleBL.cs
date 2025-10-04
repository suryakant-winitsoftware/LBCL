using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.DL.Classes;
using Winit.Modules.CollectionModule.DL.Interfaces;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Setting.Model.Interfaces;
using Winit.Modules.Store.DL.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Winit.Modules.CollectionModule.BL.Classes
{
    public class CollectionModuleBL : Interfaces.ICollectionModuleBL
    {
        protected readonly DL.Interfaces.ICollectionModuleDL _CollectionModuleDL = null;

        public CollectionModuleBL(DL.Interfaces.ICollectionModuleDL collectionModuleDL)
        {
            _CollectionModuleDL = collectionModuleDL;
        }

        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByCustomerCode(string SessionUserCode, string CustomerCode, string SalesOrgCode)
        {
            return await _CollectionModuleDL.GetAllOutstandingTransactionsByCustomerCode(SessionUserCode, CustomerCode, SalesOrgCode);
        }
        public async Task<IEnumerable<Model.Interfaces.IExchangeRate>> GetAllConfiguredCurrencyDetailsBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            return await _CollectionModuleDL.GetAllConfiguredCurrencyDetailsBySalesOrgCode(SessionUserCode, SalesOrgCode);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllConfiguredDocumentTypesBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            return await _CollectionModuleDL.GetAllConfiguredDocumentTypesBySalesOrgCode(SessionUserCode, SalesOrgCode);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> GetAllConfiguredPaymentModesBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            return await _CollectionModuleDL.GetAllConfiguredPaymentModesBySalesOrgCode(SessionUserCode, SalesOrgCode);
        }
        public async Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode)
        {
            return await _CollectionModuleDL.GetAllCustomersBySalesOrgCode(SessionUserCode, SalesOrgCode);
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByMultipleFilters(string SessionUserCode, string SalesOrgCode, string CustomerCode, string StartDueDate, string EndDueDate, string StartInvoiceDate, string EndInvoiceDate)
        {
            return await _CollectionModuleDL.GetAllOutstandingTransactionsByMultipleFilters(SessionUserCode, SalesOrgCode, CustomerCode, StartDueDate, EndDueDate, StartInvoiceDate, EndInvoiceDate);
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> AllocateSelectedInvoiceswithCreditNotes(string SessionUserCode, string TrxCode, string TrxType, string PaidAmount)
        {
            return await _CollectionModuleDL.AllocateSelectedInvoiceswithCreditNotes(SessionUserCode, TrxCode, TrxType, PaidAmount);
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigurationsData(string SessionUserCode, string OrgUID)
        {
            return await _CollectionModuleDL.GetOrgwiseConfigurationsData(SessionUserCode, OrgUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> GetAllotmentAmount(string TargetUID, string AccCollectionUID)
        {
            return await _CollectionModuleDL.GetAllotmentAmount(TargetUID, AccCollectionUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollectionAllotment>> AllInvoices(string AccCollectionUID)
        {
            return await _CollectionModuleDL.AllInvoices(AccCollectionUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> DaysTable(string StoreUID)
        {
            return await _CollectionModuleDL.DaysTable(StoreUID);
        }
        public async Task<Model.Interfaces.IAccPayable> ExcelBalance(string ReceiptNumber, string StoreUID)
        {
            return await _CollectionModuleDL.ExcelBalance(ReceiptNumber, StoreUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccPayable>> DaysTableParent(string StoreUID, int startDay, int endDay)
        {
            return await _CollectionModuleDL.DaysTableParent(StoreUID, startDay, endDay);
        }
        public async Task<IEnumerable<IBank>> GetBankNames()
        {
            return await _CollectionModuleDL.GetBankNames();
        }
        public async Task<int> UpdateChequeDetails(IAccCollectionPaymentMode collection)
        {
            return await _CollectionModuleDL.UpdateChequeDetails(collection);
        }
        public async Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> IsReversal(string UID)
        {
            return await _CollectionModuleDL.IsReversal(UID);
        }
        public async Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> IsReversalCash(string UID)
        {
            return await _CollectionModuleDL.IsReversalCash(UID);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowPending(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowPending(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowSettled(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowApproved(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowApproved(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowRejected(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowRejected(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowBounced(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowBounced(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> PaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string AccCollectionUID)
        {
            return await _CollectionModuleDL.PaymentDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, AccCollectionUID);
        }
        public async Task<IEnumerable<Model.Interfaces.IAccCollection>> SettledDetails(string AccCollectionUID)
        {
            return await _CollectionModuleDL.SettledDetails(AccCollectionUID);
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> CashierSettlement(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.CashierSettlement(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashierSettlementVoid(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.CashierSettlementVoid(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashierSettlementSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.CashierSettlementSettled(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<PagedResponse<Model.Interfaces.IAccCollectionAllotment>> ShowPaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ShowPaymentDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<IEnumerable<Model.Interfaces.ICollectionModule>> GetOrgwiseConfigValueByConfigName(string SessionUserCode, string OrgUID, string Configname)
        {
            return await _CollectionModuleDL.GetOrgwiseConfigValueByConfigName(SessionUserCode, OrgUID, Configname);
        }
        public async Task<string> CreateReceipt(Model.Interfaces.ICollections[] collection)
        {
            return await _CollectionModuleDL.CreateReceipt(collection);
        }
        public async Task<string> CreateReceipt(Model.Interfaces.ICollections collection)
        {
            return await _CollectionModuleDL.CreateReceipt(collection);
        }
        public async Task<string> CreateReceiptWithZeroValue(Model.Interfaces.ICollections collection)
        {
            return await _CollectionModuleDL.CreateReceiptWithZeroValue(collection);
        }
        public async Task<int> CreateReceiptWithAutoAllocation(Model.Interfaces.ICollections[] collection)
        {
            return await _CollectionModuleDL.CreateReceiptWithAutoAllocation(collection);
        }
        public async Task<string> CreateOnAccountReceipt(Model.Interfaces.ICollections collection)
        {
            return await _CollectionModuleDL.CreateOnAccountReceipt(collection);
        }
        public async Task<string> CashCollectionSettlement(string collection)
        {
            return await _CollectionModuleDL.CashCollectionSettlement(collection);
        }
        public async Task<string> CreateCollectionSettlementByCashier(Model.Interfaces.IAccCollectionSettlement collection)
        {
            return await _CollectionModuleDL.CreateCollectionSettlementByCashier(collection);
        }
        public async Task<string> VOIDCollectionByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
        {
            return await _CollectionModuleDL.VOIDCollectionByReceiptNumber(ReceiptNumber, TargetUID, Amount, ChequeNo, SessionUserCode, ReasonforCancelation);
        }
        public async Task<string> CreateReversalReceiptByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode, string ReasonforCancelation)
        {
            return await _CollectionModuleDL.CreateReversalReceiptByReceiptNumber(ReceiptNumber, TargetUID, Amount, ChequeNo, SessionUserCode, ReasonforCancelation);
        }
        public async Task<string> UpdatePaymentModeDetails(IAccCollectionPaymentMode collection)
        {
            return await _CollectionModuleDL.UpdatePaymentModeDetails(collection);
        }
        public async Task<string> ValidateChequeReceiptByPaymentMode(string UID, string Button, string Comments, string SessionUserCode, string CashNumber)
        {
            return await _CollectionModuleDL.ValidateChequeReceiptByPaymentMode(UID, Button, Comments, SessionUserCode, CashNumber);
        }
        public async Task<string> ValidatePOSReceiptByPaymentMode(string UID, string Comments, string SessionUserCode)
        {
            return await _CollectionModuleDL.ValidatePOSReceiptByPaymentMode(UID, Comments, SessionUserCode);
        }
        public async Task<string> ValidateONLINEReceiptByPaymentMode(string UID, string Comments, string SessionUserCode)
        {
            return await _CollectionModuleDL.ValidateONLINEReceiptByPaymentMode(UID, Comments, SessionUserCode);
        }
        public async Task<string> ValidateChequeSettlement(string UID, string Comments, string Status, string SessionUserCode, string ReceiptUID, string ChequeNo)
        {
            return await _CollectionModuleDL.ValidateChequeSettlement(UID, Comments, Status, SessionUserCode, ReceiptUID, ChequeNo);
        }
        public async Task<string> ValidatePOSSettlement(string UID, string Comments, string Status, string SessionUserCode)
        {
            return await _CollectionModuleDL.ValidatePOSSettlement(UID, Comments, Status, SessionUserCode);
        }
        public async Task<string> ValidateONLINESettlement(string UID, string Comments, string Status, string SessionUserCode)
        {
            return await _CollectionModuleDL.ValidateONLINESettlement(UID, Comments, Status, SessionUserCode);
        }
        public async Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccUser>> GetUser()
        {
            return await _CollectionModuleDL.GetUser();
        }
        public async Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> GetChequeDetails(string UID, string TargetUID)
        {
            return await _CollectionModuleDL.GetChequeDetails(UID, TargetUID);
        }
        public async Task<IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetSettingByType(string UID)
        {
            return await _CollectionModuleDL.GetSettingByType(UID);
        }
        public async Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> GetUnSettleAmount(string UID)
        {
            return await _CollectionModuleDL.GetUnSettleAmount(UID);
        }

        public async Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs)
        {
            return await _CollectionModuleDL.PopulateCollectionPage(CustomerCode, Tabs);
        }
        public async Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID)
        {
            return await _CollectionModuleDL.GetInvoicesMobile(AccCollectionUID);
        }
        public async Task<PagedResponse<Model.Interfaces.IAccCollection>> ViewPayments(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _CollectionModuleDL.ViewPayments(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<List<IAccCollectionAllotment>> ViewPaymentsDetails(string UID)
        {
            return await _CollectionModuleDL.ViewPaymentsDetails(UID);
        }
        public async Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment, string Print)
        {
            return await _CollectionModuleDL.PaymentReceipts(FromDate, ToDate, Payment, Print);
        }
        public async Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID)
        {
            return await _CollectionModuleDL.AllotmentReceipts(AccCollectionUID);
        }
        public async Task<string> CreateReceiptWithEarlyPaymentDiscount(ICollections EarlyPaymentRecords)
        {
            return await _CollectionModuleDL.CreateReceiptWithEarlyPaymentDiscount(EarlyPaymentRecords);
        }
        public async Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode)
        {
            return await _CollectionModuleDL.CheckEligibleForDiscount(ApplicableCode);
        }
        public async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>> GetAccountStatement(string StoreUID, string FromDate, string ToDate)
        {
            return await _CollectionModuleDL.GetAccountStatement(StoreUID, FromDate, ToDate);
        }
        public async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> GetAccountStatementPay(string StoreUID)
        {
            return await _CollectionModuleDL.GetAccountStatementPay(StoreUID);
        }
        public async Task<List<IAccPayable>> GetInvoices(string StoreUID)
        {
            return await _CollectionModuleDL.GetInvoices(StoreUID);
        }
        public async Task<List<IStore>> GetCustomerCode(string CustomerCode)
        {
            return await _CollectionModuleDL.GetCustomerCode(CustomerCode);
        }
        public async Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment)
        {
            return await _CollectionModuleDL.AddEarlyPayment(EarlyPayment);
        }

        public async Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID)
        {
            return await _CollectionModuleDL.GetCollectionStoreDataForPrinter(UID);
        }
        public async Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID)
        {
            return await _CollectionModuleDL.GetAllotmentDataForPrinter(AccCollectionUID);
        }

        public async Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID)
        {
            return await _CollectionModuleDL.ShowPendingRecordsInPopUp(InvoiceNumber, StoreUID);
        }
        public async Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID)
        {
            return await _CollectionModuleDL.GetPendingRecordsFromDB(StoreUID);
        }
        public async Task<List<IAccCollectionPaymentMode>> CPOData(string AccCollectionUID)
        {
            return await _CollectionModuleDL.CPOData(AccCollectionUID);
        }
        public async Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID)
        {
            return await _CollectionModuleDL.GetCurrencyRateRecords(StoreUID);
        }
        public async Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID)
        {
            return await _CollectionModuleDL.GetMultiCurrencyDetails(AccCollectionUID);
        }
        public async Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate)
        {
            return await _CollectionModuleDL.GetPaymentSummary(FromDate, ToDate);
        }
        public async Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails()
        {
            return await _CollectionModuleDL.GetConfigurationDetails();
        }
        public async Task<List<IAccCollection>> GetReceipts()
        {
            return await _CollectionModuleDL.GetReceipts();
        }
        public async Task<IAccCollectionAndDeposit> ViewReceipts(string RequestNo)
        {
            return await _CollectionModuleDL.ViewReceipts(RequestNo);
        }
        public async Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status)
        {
            return await _CollectionModuleDL.GetRequestReceipts(Status);
        }
        public async Task<bool> CreateCashDepositRequest(IAccCollectionDeposit accCollectionDeposit)
        {
            return await _CollectionModuleDL.CreateCashDepositRequest(accCollectionDeposit);
        }
        public async Task<bool> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status)
        {
            return await _CollectionModuleDL.ApproveOrRejectDepositRequest(accCollectionDeposit, Status);
        }
        public async Task<decimal> CheckCollectionLimitForLoggedInUser(string EmpUID)
        {
            return await _CollectionModuleDL.CheckCollectionLimitForLoggedInUser(EmpUID);
        }
        public async Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action)
        {
            return await _CollectionModuleDL.UpdateCollectionLimit(Limit, EmpUID, Action);
        }
        public async Task<bool> UpdateBankDetails(string UID, string BankName, string Branch, string ReferenceNumber)
        {
            return await _CollectionModuleDL.UpdateBankDetails(UID, BankName, Branch, ReferenceNumber);
        }
        public async Task<PagedResponse<IAccCollection>> GetCollectionTabsDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, string PageName)
        {
            return await _CollectionModuleDL.GetCollectionTabsDetails(sortCriterias, pageNumber, pageSize, filterCriterias, PageName);
        }

        public async Task<PagedResponse<Model.Interfaces.IStoreStatement>> StoreStatementRecords(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StartDate, string EndDate)
        {
            return await _CollectionModuleDL.StoreStatementRecords(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,StartDate, EndDate);
        }
        public async Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation)
        {
            return await _CollectionModuleDL.UpdateBalanceConfirmation(balanceConfirmation);
        }
        public async Task<bool> UpdateBalanceConfirmationForResolvingDispute(IBalanceConfirmation balanceConfirmation)
        {
            return await _CollectionModuleDL.UpdateBalanceConfirmationForResolvingDispute(balanceConfirmation);
        }
        public async Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine)
        {
            return await _CollectionModuleDL.InsertDisputeRecords(balanceConfirmationLine);
        }
        public async Task<IBalanceConfirmation> GetBalanceConfirmationDetails(string StoreUID)
        {
            return await _CollectionModuleDL.GetBalanceConfirmationDetails(StoreUID);
        }
        public async Task<List<IBalanceConfirmation>> GetBalanceConfirmationListDetails()
        {
            return await _CollectionModuleDL.GetBalanceConfirmationListDetails();
        }
        public async Task<List<IBalanceConfirmationLine>> GetBalanceConfirmationLineDetails(string UID)
        {
            return await _CollectionModuleDL.GetBalanceConfirmationLineDetails(UID);
        }
        public async Task<Winit.Modules.Contact.Model.Interfaces.IContact> GetContactDetails(string EmpCode)
        {
            return await _CollectionModuleDL.GetContactDetails(EmpCode);
        }
    }

}
