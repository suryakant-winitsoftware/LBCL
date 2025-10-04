using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class MSSQLTemporaryCreditLimitDL : SqlServerDBManager, ITemporaryCreditLimitDL
    {

        public MSSQLTemporaryCreditLimitDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<ITemporaryCreditLimit>> GetTemporaryCreditLimitDetails(IEntityDetails entityDetails)
        {

            try
            {
                //await InsertIntoMonthAndQueueTable(entityDetails);
                List<ITemporaryCreditLimit> temporaryCreditLimits = await GetTemporaryCreditLimitDataToPush();
                await InsertIntoMonthTable(temporaryCreditLimits, entityDetails);
                await InsertIntoQueueTable(entityDetails);
                var parameters = new Dictionary<string, object?>()
        {
            { "SyncLogDetailId",entityDetails.SyncLogDetailId}
        };
                var sql = new StringBuilder($@" select sync_log_id,UID,req_uid,request_number,customer_code,request_type,Division
                ,start_date,end_date,requested_credit_limit,requested_credit_days,remarks,oracle_status,read_from_oracle,is_auto_approved from {Int_DbTableName.TemporaryCreditLimit + Int_DbTableName.QueueTableSuffix}  where sync_log_id=@SyncLogDetailId");
                List<ITemporaryCreditLimit> customerMasterPush = await ExecuteQueryAsync<ITemporaryCreditLimit>(sql.ToString(), parameters);
                return customerMasterPush;
            }
            catch { throw; }

        }
        private async Task<List<ITemporaryCreditLimit>> GetTemporaryCreditLimitDataToPush()
        {
            try
            {
                string? db = await GetSettingValueByKey("DB");
                if (db == null)
                    throw new Exception("There is no value in setting table for DB key");
                var parameters = new Dictionary<string, object?>() { };
                var sql = new StringBuilder($@" select  DISTINCT TC.uid  ,TC.uid ReqUID,order_number request_number, S.code customer_code
                            ,request_type,o.code as division  ,effective_from start_date,effective_upto end_date
                            ,case when  request_type ='Aging Days'then 0 else TC.request_amount_days end  requested_credit_limit
                            ,case when  request_type ='Aging Days' then TC.request_amount_days  else 0  end  requested_credit_days
                            ,remarks,case when tc.is_auto_approved=1 then 'Y' else 'N' end is_auto_approved from {db}.temporary_credit TC 
                            inner join {db}.store s on TC.store_uid=S.uid
							 inner join {db}.org o on TC.division_org_uid=o.uid 
                            inner join {db}.int_pushed_data_status ps on ps.linked_item_uid=TC.uid 
                            and ps.linked_item_type='TemporaryCreditLimit'and isnull(ps.status,'')='pending' ");
                List<ITemporaryCreditLimit> customerMasterPush = await ExecuteQueryAsync<ITemporaryCreditLimit>(sql.ToString(), parameters);
                return customerMasterPush;
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> InsertIntoMonthTable(List<ITemporaryCreditLimit> temporaryCreditLimits, IEntityDetails entityDetails)
        {
            try
            {
                temporaryCreditLimits.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID, req_uid,request_number,customer_code
                ,request_type,division,start_date,end_date,requested_credit_limit,requested_credit_days,remarks,is_auto_approved)
                select @SyncLogId, @UID ,@ReqUID,@RequestNumber, @CustomerCode ,@RequestType,@Division ,@StartDate ,@EndDate ,@RequestedCreditLimit 
                ,@RequestedCreditDays ,@Remarks ,@IsAutoApproved  ");
                return await ExecuteNonQueryAsync(monthSql.ToString(), temporaryCreditLimits);

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
                var truncateQuery = new StringBuilder($@" truncate table  {Int_DbTableName.TemporaryCreditLimit + Int_DbTableName.QueueTableSuffix};");
                await ExecuteNonQueryAsync(truncateQuery.ToString(), null);
                var queueSql = new StringBuilder($@" Insert into {Int_DbTableName.TemporaryCreditLimit + Int_DbTableName.QueueTableSuffix} (sync_log_id,UID,name,status
                ,message,source,is_processed,inserted_on,processed_on,error_description,common_attribute1,common_attribute2,req_uid,request_number
                ,customer_code,request_type,division,start_date,end_date,requested_credit_limit,requested_credit_days,remarks,oracle_status,read_from_oracle,is_auto_approved)
                select sync_log_id,UID,name,status,message,source,is_processed,inserted_on,processed_on,error_description,common_attribute1
                ,common_attribute2,req_uid,request_number,customer_code,request_type,division,start_date,end_date,requested_credit_limit
                ,requested_credit_days,remarks,oracle_status,read_from_oracle,is_auto_approved from {entityDetails.TableName} where sync_log_id =@SyncLogDetailId");

                return await ExecuteNonQueryAsync(queueSql.ToString(), queueParameters);
            }
            catch { throw; }
        }
    }
}
