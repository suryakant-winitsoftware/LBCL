using Winit.Shared.Models.Enums;
 
namespace Winit.Modules.Int_CommonMethods.DL.Interfaces
{
    public interface IInt_CommonMethodsDL
    {
        Task<Winit.Modules.Int_CommonMethods.Model.Interfaces.IPrepareDB> GetTableScriptByEntity(List<FilterCriteria> filterCriterias);
        Task<Winit.Modules.Int_CommonMethods.Model.Interfaces.IPrepareDB> GetTableScriptByEntity(string Entity);
        Task<int> CheckCurrentRunningProcess(string EntityName);
        Task<string> PrepareDBByEntity(string EntityName);
        Task<int> UpdateLongRunningProcessStatusToFailure();
        Task<long> InitiateProcess(string EntityName);
        Task<int> ProcessCompletion(long SyncLogId, string Message, int Status);
        Task<Winit.Modules.Int_CommonMethods.Model.Interfaces.IEntityDetails> FetchEntityDetails(string EntityName, long SyncLogId);
        Task<Winit.Modules.Int_CommonMethods.Model.Interfaces.IEntityData> GetEntityData(string EntityName);
        Task<String> InsertPendingData(Model.Interfaces.IPendingDataRequest pendingData);
        Task<String> InsertPendingDataList(List<Model.Interfaces.IPendingDataRequest> pendingData);
        Task<int> UpdatePushDataStatusByUID(List<SyncManagerModel.Interfaces.IPushDataStatus> pushDataStatus);
        Task<int> InsertDataIntoQueue(SyncManagerModel.Interfaces.IApiRequest Request);
        Task<List<SyncManagerModel.Interfaces.IApiRequest>> GetDataFromQueue(string Entity);
        Task<int> UpdateQueueStatus(string UID);
        Task<int> IntegrationProcessStatusInsertion(Model.Interfaces.IIntegrationMessageProcess integrationMessage);
        Task<List<SyncManagerModel.Interfaces.IIntegrationProcessStatus>> GetAllOraclePendingProcessesByEntity(string Entity);
        Task<List<SyncManagerModel.Interfaces.IIntegrationProcessStatus>> GetAllPendingProcessByEntity(string Entity);
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
        Task<int> SaveOrUpdateInvoiceBlobFilePath(SyncManagerModel.Interfaces.IInt_InvoiceHeader invoiceHeader,string path);
        Task<int> InsertOutStandingInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertTemporaryCreditLimitsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertWarehouseStocksIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertProvisionsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<int> InsertProvisionCreditNotesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId);
        Task<bool> InvokePrepareStoreMasterAPI(List<string> uids);
        Task<bool> InvokePrepareSKUMasterAPI(List<string> uids);
    }
}
