using SyncManagerModel.Interfaces;
using Winit.Shared.Models.Enums;

namespace SyncManagerDL.Interfaces
{
    public interface IInt_CommonMethodsDL
    {
        Task<IPrepareDB> GetTableScriptByEntity(List<FilterCriteria> filterCriterias);
        Task<IPrepareDB> GetTableScriptByEntity(string Entity);
        Task<int> CheckCurrentRunningProcess(string EntityName);
        Task<string> PrepareDBByEntity(string EntityName);
        Task<int> UpdateLongRunningProcessStatusToFailure();
        Task<long> InitiateProcess(string EntityName);
        Task<int> ProcessCompletion(long SyncLogId, string Message, int Status);
        Task<IEntityDetails> FetchEntityDetails(string EntityName, long SyncLogId);
        Task<IEntityData> GetEntityData(string EntityName);
        Task<String> InsertPendingData(IPendingDataRequest pendingData);
        Task<String> InsertPendingDataList(List<IPendingDataRequest> pendingData);
        Task<int> UpdatePushDataStatusByUID(List<IPushDataStatus> pushDataStatus);
        Task<int> InsertDataIntoQueue(IApiRequest Request);
        Task<List<IApiRequest>> GetDataFromQueue(string Entity);
        Task<int> UpdateQueueStatus(string UID);
        Task<int> IntegrationProcessStatusInsertion(IIntegrationMessageProcess integrationMessage);
        Task<List<IIntegrationProcessStatus>> GetAllOraclePendingProcessesByEntity(string Entity);
        Task<List<IIntegrationProcessStatus>> GetAllPendingProcessByEntity(string Entity);
        Task<int> UpdateIntegrationProcessStatusByProcessId(long processId, int oracleStatus);
        Task<int> InsertItemDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertPriceDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertPriceLadderingDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertTaxDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertTemporaryCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertCustomerOracleCodeIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertPurchaseOrderConfirmationIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<List<SyncManagerModel.Interfaces.IInt_InvoiceHeader>> GetProcessingInvoiceBlobFiles(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> SaveOrUpdateInvoiceBlobFilePath(SyncManagerModel.Interfaces.IInt_InvoiceHeader invoiceHeader, string path);
        Task<int> InsertOutStandingInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertTemporaryCreditLimitsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertWarehouseStocksIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertProvisionsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertProvisionCreditNotesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertPayThroughAPMastersIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertCustomerReferenceIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> CheckisJobEnable(string Entity);
        Task<bool> InvokePrepareStoreMasterAPI(List<string> uids);
        Task<bool> InvokePrepareSKUMasterAPI(List<string> uids);
    }
}
