using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CollectionModule.DL.Interfaces
{
    public interface ICollectionModuleDL
    {
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByCustomerCode(string SessionUserCode, string CustomerCode, string SalesOrgCode);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IExchangeRate>> GetAllConfiguredCurrencyDetailsBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>> GetAllConfiguredDocumentTypesBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> GetAllConfiguredPaymentModesBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>> GetAllCustomersBySalesOrgCode(string SessionUserCode, string SalesOrgCode);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>> GetAllotmentAmount(string TargetUID, string AccCollectionUID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>> AllInvoices(string AccCollectionUID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> DaysTable(string StoreUID);
        Task<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable> ExcelBalance(string ReceiptNumber, string StoreUID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> DaysTableParent(string StoreUID, int startDay, int endDay);
        Task<IEnumerable<IBank>> GetBankNames();
        Task<int> UpdateChequeDetails(IAccCollectionPaymentMode collection);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> IsReversal(string UID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> IsReversalCash(string UID);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowPending(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowApproved(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowRejected(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> ShowBounced(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> PaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string AccCollectionUID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> SettledDetails(string AccCollectionUID);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashierSettlement(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashierSettlementVoid(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>> CashierSettlementSettled(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<PagedResponse<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>> ShowPaymentDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>> GetAllOutstandingTransactionsByMultipleFilters(string SessionUserCode, string SalesOrgCode, string CustomerCode, string StartDueDate, string EndDueDate, string StartInvoiceDate, string EndInvoiceDate);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>> AllocateSelectedInvoiceswithCreditNotes(string SessionUserCode, string TrxCode, string TrxType, string PaidAmount);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>> GetOrgwiseConfigurationsData(string SessionUserCode, string OrgUID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.ICollectionModule>> GetOrgwiseConfigValueByConfigName(string SessionUserCode, string OrgUID, string Configname);
        Task<string> CreateReceipt(Model.Interfaces.ICollections[] collection);
        Task<string> CreateReceipt(Model.Interfaces.ICollections collection);
        Task<string> CreateReceiptWithZeroValue(Model.Interfaces.ICollections collection);
        Task<int> CreateReceiptWithAutoAllocation(Model.Interfaces.ICollections[] collection);
        Task<string> CreateOnAccountReceipt(Model.Interfaces.ICollections collection);
        Task<string> CashCollectionSettlement(string collection);
        Task<string> CreateCollectionSettlementByCashier(Model.Interfaces.IAccCollectionSettlement collection);
        Task<string> VOIDCollectionByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode, string ReasonforCancelation);
        Task<string> CreateReversalReceiptByReceiptNumber(string ReceiptNumber, string TargetUID, decimal Amount, string ChequeNo, string SessionUserCode, string ReasonforCancelation);
        Task<string> UpdatePaymentModeDetails(IAccCollectionPaymentMode collection);
        Task<string> ValidateChequeReceiptByPaymentMode(string UID, string Button, string Comments, string SessionUserCode, string CashNumber);
        Task<string> ValidatePOSReceiptByPaymentMode(string UID, string Comments, string SessionUserCode);
        Task<string> ValidateONLINEReceiptByPaymentMode(string UID, string Comments, string SessionUserCode);
        Task<string> ValidateChequeSettlement(string UID, string Comments, string Status, string SessionUserCode, string ReceiptUID, string ChequeNo);
        Task<string> ValidatePOSSettlement(string UID, string Comments, string Status, string SessionUserCode);
        Task<string> ValidateONLINESettlement(string UID, string Comments, string Status, string SessionUserCode);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccUser>> GetUser();
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> GetChequeDetails(string UID, string TargetUID);
        Task<IEnumerable<Winit.Modules.Setting.Model.Interfaces.ISetting>> GetSettingByType(string UID);
        Task<IEnumerable<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>> GetUnSettleAmount(string UID);
        //mobile
        Task<List<IAccPayable>> PopulateCollectionPage(string CustomerCode, string Tabs);
        Task<List<Model.Interfaces.IAccPayable>> GetInvoicesMobile(string AccCollectionUID);
        Task<PagedResponse<Model.Interfaces.IAccCollection>> ViewPayments(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<List<IAccCollectionAllotment>> ViewPaymentsDetails(string UID);
        Task<List<IAccCollection>> PaymentReceipts(string FromDate, string ToDate, string Payment, string Print);
        Task<List<IAccCollectionAllotment>> AllotmentReceipts(string AccCollectionUID);
        Task<string> CreateReceiptWithEarlyPaymentDiscount(ICollections EarlyPaymentRecords);
        Task<IEnumerable<IEarlyPaymentDiscountConfiguration>> CheckEligibleForDiscount(string ApplicableCode);
        Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>> GetAccountStatement(string StoreUID, string FromDate, string ToDate);
        Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>> GetAccountStatementPay(string StoreUID);
        Task<List<IAccPayable>> GetInvoices(string StoreUID);
        Task<List<IStore>> GetCustomerCode(string CustomerCode);
        Task<string> AddEarlyPayment(IEarlyPaymentDiscountConfiguration EarlyPayment);
        Task<List<ICollectionPrint>> GetCollectionStoreDataForPrinter(List<string> UID);
        Task<List<ICollectionPrintDetails>> GetAllotmentDataForPrinter(string AccCollectionUID);
        Task<List<IAccCollectionPaymentMode>> ShowPendingRecordsInPopUp(string InvoiceNumber, string StoreUID);
        Task<List<IAccPayable>> GetPendingRecordsFromDB(string StoreUID);
        Task<List<IAccCollectionPaymentMode>> CPOData(string AccCollectionUID);
        Task<List<IExchangeRate>> GetCurrencyRateRecords(string StoreUID);
        Task<List<IAccCollectionCurrencyDetails>> GetMultiCurrencyDetails(string AccCollectionUID);
        Task<List<IPaymentSummary>> GetPaymentSummary(string FromDate, string ToDate);
        Task<List<IEarlyPaymentDiscountConfiguration>> GetConfigurationDetails();
        Task<List<IAccCollection>> GetReceipts();
        Task<List<IAccCollectionDeposit>> GetRequestReceipts(string Status);
        Task<IAccCollectionAndDeposit> ViewReceipts(string RequestNo);
        Task<bool> CreateCashDepositRequest(IAccCollectionDeposit accCollectionDeposit);
        Task<int> CUDAccPayable(List<IAccPayable> accPayables, IDbConnection? connection = null, IDbTransaction? transaction = null);
        Task<bool> ApproveOrRejectDepositRequest(IAccCollectionDeposit accCollectionDeposit, string Status);
        Task<decimal> CheckCollectionLimitForLoggedInUser(string EmpUID);
        Task<bool> UpdateCollectionLimit(decimal Limit, string EmpUID, int Action);
        Task<bool> UpdateBankDetails(string UID, string BankName, string Branch, string ReferenceNumber);
        Task<PagedResponse<IAccCollection>> GetCollectionTabsDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, string PageName);
        Task<PagedResponse<Model.Interfaces.IStoreStatement>> StoreStatementRecords(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string StartDate, string EndDate);
        Task<bool> UpdateBalanceConfirmation(IBalanceConfirmation balanceConfirmation);
        Task<bool> UpdateBalanceConfirmationForResolvingDispute(IBalanceConfirmation balanceConfirmation);
        Task<bool> InsertDisputeRecords(List<IBalanceConfirmationLine> balanceConfirmationLine);
        Task<IBalanceConfirmation> GetBalanceConfirmationDetails(string StoreUID);
        Task<List<IBalanceConfirmation>> GetBalanceConfirmationListDetails();
        Task<List<IBalanceConfirmationLine>> GetBalanceConfirmationLineDetails(string UID);
        Task<Winit.Modules.Contact.Model.Interfaces.IContact> GetContactDetails(string EmpCode);
    }
    
}
