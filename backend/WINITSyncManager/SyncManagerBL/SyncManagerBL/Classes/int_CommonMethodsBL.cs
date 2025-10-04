using Microsoft.Extensions.Configuration;
using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Enums;

namespace SyncManagerBL.Classes
{
    public class int_CommonMethodsBL : Iint_CommonMethodsBL
    {
        protected readonly IInt_CommonMethodsDL _int_CommonMethods;
        protected readonly IConfiguration _configuration;
        public int_CommonMethodsBL(IInt_CommonMethodsDL int_CommonMethods, IConfiguration configuration)
        {
            _int_CommonMethods = int_CommonMethods;
            _configuration = configuration;
        }
        public async Task<string> PrepareDBByEntity(string EntityName)
        {
            return await _int_CommonMethods.PrepareDBByEntity(EntityName);
        }
        public async Task<IPrepareDB> GetTableScriptByEntity(List<FilterCriteria> filterCriterias)
        {
            return await _int_CommonMethods.GetTableScriptByEntity(filterCriterias);
        }
        public async Task<IPrepareDB> GetTableScriptByEntity(string Entity)
        {
            return await _int_CommonMethods.GetTableScriptByEntity(Entity);
        }
        public async Task<int> CheckCurrentRunningProcess(string EntityName)
        {
            return await _int_CommonMethods.CheckCurrentRunningProcess(EntityName);
        }
        public async Task<int> UpdateLongRunningProcessStatusToFailure()
        {
            return await _int_CommonMethods.UpdateLongRunningProcessStatusToFailure();
        }
        public async Task<long> InitiateProcess(string EntityName)
        {
            return await _int_CommonMethods.InitiateProcess(EntityName);
        }
        public async Task<int> ProcessCompletion(long SyncLogId, string Message, int Status)
        {
            return await _int_CommonMethods.ProcessCompletion(SyncLogId, Message, Status);
        }
        public async Task<IEntityDetails> FetchEntityDetails(string EntityName, long SyncLogId)
        {
            return await _int_CommonMethods.FetchEntityDetails(EntityName, SyncLogId);
        }
        public async Task<IEntityData> GetEntityData(string EntityName)
        {
            return await _int_CommonMethods.GetEntityData(EntityName);
        }

        public async Task<string> InsertPendingData(IPendingDataRequest pendingData)
        {
            return await _int_CommonMethods.InsertPendingData(pendingData);
        }
        public async Task<string> InsertPendingDataList(List<IPendingDataRequest> pendingData)
        {
            return await _int_CommonMethods.InsertPendingDataList(pendingData);
        }

        public async Task<int> UpdatePushDataStatusByUID(List<IPushDataStatus> pushDataStatus)
        {
            return await _int_CommonMethods.UpdatePushDataStatusByUID(pushDataStatus);
        }
        public async Task<int> InsertDataIntoQueue(IApiRequest Request)
        {
            return await _int_CommonMethods.InsertDataIntoQueue(Request);
        }
        public async Task<int> IntegrationProcessStatusInsertion(IIntegrationMessageProcess integrationMessage)
        {
            return await _int_CommonMethods.IntegrationProcessStatusInsertion(integrationMessage);
        }

        public async Task<List<IApiRequest>> GetDataFromQueue(string Entity)
        {
            return await _int_CommonMethods.GetDataFromQueue(Entity);
        }

        public async Task<int> UpdateQueueStatus(string UID)
        {
            return await _int_CommonMethods.UpdateQueueStatus(UID);
        }

        public async Task<List<IIntegrationProcessStatus>> GetAllOraclePendingProcessesByEntity(string Entity)
        {
            return await _int_CommonMethods.GetAllOraclePendingProcessesByEntity(Entity);
        }

        public async Task<int> InsertItemDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertItemDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertPriceDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertPriceDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertTaxDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertTaxDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertPriceLadderingDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertPriceLadderingDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<List<IIntegrationProcessStatus>> GetAllPendingProcessByEntity(string Entity)
        {
            return await _int_CommonMethods.GetAllPendingProcessByEntity(Entity);
        }
        public async Task<int> InsertCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertCreditLimitDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertTemporaryCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertTemporaryCreditLimitDataIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertCustomerOracleCodeIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertCustomerOracleCodeIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertPurchaseOrderConfirmationIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertPurchaseOrderConfirmationIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                var blobFiles = await _int_CommonMethods.GetProcessingInvoiceBlobFiles(TableName, TablePrefix, SyncLogDetailId);
                // var pendingBlobFiles= 
                foreach (var blobFile in blobFiles)
                {
                    if (blobFile.InvoiceFile != null && blobFile.InvoiceFile.Length > 0)
                    {
                        string path = string.Empty;
                        path = await new Int_CommonFunctions(_configuration).SaveBlobToFile(blobFile.InvoiceFile, blobFile.DeliveryId, "Invoice", "Invoice");
                        await _int_CommonMethods.SaveOrUpdateInvoiceBlobFilePath(blobFile, path);
                    }
                }
                return await _int_CommonMethods.InsertInvoicesIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
            }
            catch { throw; }
        }
        public async Task<int> UpdateIntegrationProcessStatusByProcessId(long processId, int oracleStatus)
        {
            return await _int_CommonMethods.UpdateIntegrationProcessStatusByProcessId(processId, oracleStatus);
        }
        public async Task<int> InsertOutStandingInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertOutStandingInvoicesIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertTemporaryCreditLimitsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertTemporaryCreditLimitsIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertWarehouseStocksIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertWarehouseStocksIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertProvisionsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertProvisionsIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertProvisionCreditNotesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertProvisionCreditNotesIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertPayThroughAPMastersIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertPayThroughAPMastersIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> InsertCustomerReferenceIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            return await _int_CommonMethods.InsertCustomerReferenceIntoMainTable(TableName, TablePrefix, SyncLogDetailId);
        }
        public async Task<int> CheckisJobEnable(string Entity)
        {
            return await _int_CommonMethods.CheckisJobEnable(Entity);
        }
        public async Task<bool> InvokePrepareStoreMasterAPI(List<string> uids)
        {
            return await _int_CommonMethods.InvokePrepareStoreMasterAPI(uids);
        }
        public async Task<bool> InvokePrepareSKUMasterAPI(List<string> uids)
        {
            return await _int_CommonMethods.InvokePrepareSKUMasterAPI(uids);
        }

    }
}
