using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class MSSQLProvisionCreditNotePushDL : SqlServerDBManager, IProvisionCreditNotePushDL
    {
        public MSSQLProvisionCreditNotePushDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
       public async Task<List<IProvisionCreditNote>> GetCreditNoteProvisionDetails(IEntityDetails entityDetails)
        {
            try
            { 
                List<IProvisionCreditNote> creditNoteProvisions = await GetCreditNoteProvisionDataToPush();
                await InsertIntoMonthTable(creditNoteProvisions, entityDetails);
                await InsertIntoQueueTable(entityDetails);
                var parameters = new Dictionary<string, object?>()
        {
            { "SyncLogDetailId",entityDetails.SyncLogDetailId}
        };
                var sql = new StringBuilder($@" select sync_log_id,UID,provision_id, dms_release_requested from {Int_DbTableName.CreditNoteProvisionPush + Int_DbTableName.QueueTableSuffix}  where sync_log_id=@SyncLogDetailId");
                List<IProvisionCreditNote> creditNoteProvisionsPushData = await ExecuteQueryAsync<IProvisionCreditNote>(sql.ToString(), parameters);
                return creditNoteProvisionsPushData;
            }
            catch { throw; }
        }

        private async Task<List<IProvisionCreditNote>> GetCreditNoteProvisionDataToPush()
        {
            try
            {
                string? db = await GetSettingValueByKey("DB");
                if (db == null)
                    throw new Exception("There is no value in setting table for DB key");
                var parameters = new Dictionary<string, object?>() { };
                var sql = new StringBuilder($@" select DISTINCT ps.linked_item_uid  UID,p.provision_id ProvisionId ,p.is_dms_release_requested  DmsReleaseRequested
                            from {db}.provision_data p inner join {db}.int_pushed_data_status ps on ps.linked_item_uid=CAST(p.id as varchar) 
                            and ps.linked_item_type='Provision'and isnull(ps.status,'')='pending'");
                List<IProvisionCreditNote> creditNoteProvisions = await ExecuteQueryAsync<IProvisionCreditNote>(sql.ToString(), parameters);

                return creditNoteProvisions;
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> InsertIntoMonthTable(List<IProvisionCreditNote> creditNoteProvisions, IEntityDetails entityDetails)
        {
            try
            {
                creditNoteProvisions.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,provision_id, dms_release_requested)
                select @SyncLogId, @UID , @ProvisionId, @DmsReleaseRequested   ");
                return await ExecuteNonQueryAsync(monthSql.ToString(), creditNoteProvisions);

            }
            catch { throw; }
        }
        private async Task<int> InsertIntoQueueTable(IEntityDetails entityDetails)
        {
            try
            {
                var queueParameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var truncateQuery = new StringBuilder($@" truncate table  {Int_DbTableName.CreditNoteProvisionPush + Int_DbTableName.QueueTableSuffix};");
                await ExecuteNonQueryAsync(truncateQuery.ToString(), null);
                var queueSql = new StringBuilder($@" Insert into {Int_DbTableName.CreditNoteProvisionPush + Int_DbTableName.QueueTableSuffix} (sync_log_id,UID,name,status
                ,message,source,is_processed,inserted_on,processed_on,error_description,common_attribute1,common_attribute2, provision_id, dms_release_requested)
                select sync_log_id,UID,name,status,message,source,is_processed,inserted_on,processed_on,error_description,common_attribute1
                ,common_attribute2,provision_id, dms_release_requested from {entityDetails.TableName} where sync_log_id =@SyncLogDetailId");

                return await ExecuteNonQueryAsync(queueSql.ToString(), queueParameters);
            }
            catch { throw; }
        }

    }
}
