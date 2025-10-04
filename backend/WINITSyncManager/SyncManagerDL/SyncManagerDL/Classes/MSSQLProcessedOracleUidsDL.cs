using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLProcessedOracleUidsDL : SqlServerDBManager, IProcessedOracleUidsDL
    {
        public MSSQLProcessedOracleUidsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingItemProcessStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,m.item_id as OracleUID,i.processstatus as IsProcessed ,i.errormessage  as ErrorDescription from {processDetails.MonthTableName} m 
                    inner join int_integrationmessageprocessstatus I on I.SyncLogDetailId=m.sync_log_id and I.monthtablename='{processDetails.MonthTableName}' 
                    where  I.SyncLogDetailId=@SyncLogDetailId");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingPriceProcessStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.price_master_id as OracleUID,I.ProcessStatus as IsProcessed ,I.ErrorMessage  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join  Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingPriceLadderingProcessStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.laddering_id as OracleUID,I.ProcessStatus as IsProcessed ,I.ErrorMessage  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join  Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingTaxProcessStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.hsn_code as OracleUID,I.ProcessStatus as IsProcessed ,I.ErrorMessage  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingCustomerPullProcessStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.addresskey as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingPOConfirmationStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.purchase_order_uid as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingInvoiceStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.delivery_id as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingCreditLimitStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.customer_number as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPendingOutstandingInvoiceStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,M.rec_uid as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetTemporaryCreditLimitsStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.req_uid as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetWarehouseStocksStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.sku_code as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }

        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetProvisionsStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.provision_id as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }

        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetProvisionCreditNotesStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.provision_id as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }

        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetPayThroughAPMastersStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.sku_code as OracleUID, FORMAT(M.start_date, 'dd-MM-yy')  as CommonAttribute1,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }

        }
        public async Task<List<SyncManagerModel.Interfaces.IIsProcessedStatusUids>> GetCustomerReferenceStatusAndUids(SyncManagerModel.Interfaces.IIntegrationProcessStatus processDetails)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId", processDetails.SyncLogId }
                };
                var sql = new StringBuilder($@" select FORMAT(I.ProcessedOn, 'dd-MMM-yyyy HH:mm:ss') as ProcessedOn,  M.customer_code as OracleUID,M.status as IsProcessed ,M.error_description  as ErrorDescription from {processDetails.MonthTableName} M 
            inner join   Int_IntegrationMessageProcessStatus I On I.SyncLogDetailId=M.sync_log_Id and I.MonthTableName='{processDetails.MonthTableName}' 
            where  I.SyncLogDetailId=@SyncLogDetailId and Isnull(I.ProcessStatus,0)<>0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIsProcessedStatusUids>(sql.ToString(), Parameters);
            }
            catch { throw; }

        }
    }
}
