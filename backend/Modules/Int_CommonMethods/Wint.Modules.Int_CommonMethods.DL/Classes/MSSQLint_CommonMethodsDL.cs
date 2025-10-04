using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Modules.Int_CommonMethods.DL.Interfaces;
using Winit.Modules.Int_CommonMethods.Model.Classes;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.Int_CommonMethods.DL.Classes
{
    public class MSSQLint_CommonMethodsDL : SyncManagerDL.Base.DBManager.SqlServerDBManager, IInt_CommonMethodsDL
    {
        private readonly Winit.Modules.Int_CommonMethods.DL.Classes.Int_ApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly string apiBaseUrl;
        public MSSQLint_CommonMethodsDL(IServiceProvider serviceProvider, IConfiguration config, Int_ApiService apiService) : base(serviceProvider, config)
        {
            _apiService = apiService;
            _configuration = config;
            apiBaseUrl = CommonFunctions.GetStringValue(_configuration["AppSettings:ApiBaseUrl"] ?? string.Empty);
        }
        /// <summary>
        /// It checks if the entity exists or not and checks a month and queue table script, and if present, it will create accoding the script. 
        /// </summary>
        /// <param name="EntityName"></param>
        /// <returns></returns>
        public async Task<string> PrepareDBByEntity(string EntityName)
        {
            try
            {
                await CreateTablesForEntity(Int_EntityNames.SyncLog);
                await CreateTablesForEntity(Int_EntityNames.SyncLogDetail);
                await CreateTablesForEntity(Int_EntityNames.IntegrationQueue);
                await CreateTablesForEntity(EntityName);
                return "Success";
            }
            catch
            {
                throw;
            }
        }
        private async Task<string> CreateTablesForEntity(string EntityName)
        {
            try
            {
                int Entity = await CheckEntity(EntityName);
                if (Entity <= 0)
                    throw new Exception($"Entity Does Not Exist Please Check EntityName: {EntityName}");
                IPrepareDB prepareDB = await GetTableScriptByEntity(EntityName);
                if (prepareDB == null)
                    throw new Exception($"Table Script Does Not Exist Please Check EntityName: {EntityName}");
                string? CheckMonthTable = await DoesTableExist(prepareDB.TablePrefix + Int_DbTableName.MonthTableSuffix);
                if (string.IsNullOrEmpty(CheckMonthTable))
                    await CreateMonthTable(prepareDB.TablePrefix + Int_DbTableName.MonthTableSuffix, prepareDB.Script);
                if (EntityName != Int_EntityNames.SyncLog || EntityName != Int_EntityNames.SyncLogDetail || EntityName != Int_EntityNames.IntegrationQueue)
                {
                    string? CheckQueueTable = await DoesTableExist(prepareDB.TablePrefix + Int_DbTableName.QueueTableSuffix);
                    if (string.IsNullOrEmpty(CheckQueueTable))
                        await CreateQueueTable(prepareDB.TablePrefix + Int_DbTableName.QueueTableSuffix, prepareDB.Script);
                }
                return "Success";
            }
            catch
            {
                throw;
            }
        }
        public async Task<IPrepareDB> GetTableScriptByEntity(List<FilterCriteria> filterCriterias)
        {
            try
            {
                var sql = new StringBuilder(@" Select id,uid,created_by,created_time,modified_by,
                 modified_time,server_add_time,server_modified_time,script,entity_name,entity_group,table_prefix from int_prepare_db_table_script ");
                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IPrepareDB>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                }
                IPrepareDB? prepareDB = await ExecuteSingleAsync<IPrepareDB>(sql.ToString(), parameters);
                return prepareDB ?? new PrepareDB();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// It fetches the month and queue table script based on entity
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task<IPrepareDB> GetTableScriptByEntity(string Entity)
        {
            try
            {
                var parameters = new Dictionary<string, object?>()
                {
                    {"Entity",Entity }
                };
                var sql = new StringBuilder(@" Select id,uid,created_by,created_time,modified_by,
                 modified_time,server_add_time,server_modified_time,script,entity_name,entity_group,table_prefix from int_prepare_db_table_script 
                    where entity_name=@Entity");
                return await ExecuteSingleAsync<IPrepareDB>(sql.ToString(), parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// It will check whether the entity exists or not. 
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task<int> CheckEntity(string Entity)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"Entity",Entity}
                };
                var sql = new StringBuilder($@"select count(*) from int_entity where name =@Entity ");
                return await ExecuteScalarAsync<int>(sql.ToString(), parameters);

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// It will create the current month table. 
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Script"></param>
        /// <returns></returns>
        private async Task<int> CreateMonthTable(string TableName, string Script)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"TableName",TableName}
                };
                var sql = new StringBuilder($@"create table {TableName} {Script}");
                var sqlCheck = new StringBuilder($@"select count(*) from sys.tables where name =@TableName ");
                await ExecuteNonQueryAsync(sql.ToString(), null);
                return await ExecuteScalarAsync<int>(sqlCheck.ToString(), parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// It will create Queue table.
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Script"></param>
        /// <returns></returns>
        private async Task<int> CreateQueueTable(string TableName, string Script)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"TableName",TableName}
                };
                var sql = new StringBuilder($@"create table {TableName} {Script}");
                var sqlCheck = new StringBuilder($@"select count(*) from sys.tables where name =@TableName ");
                await ExecuteNonQueryAsync(sql.ToString(), null);
                return await ExecuteScalarAsync<int>(sqlCheck.ToString(), parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// It will check if any process is currently running or not,
        /// and it will return the current running process count of the difficult entity.
        /// </summary>
        /// <param name="EntityName"></param>
        /// <returns></returns>
        public async Task<int> CheckCurrentRunningProcess(string EntityName)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "entity_name",EntityName}
                };
                var sql = new StringBuilder($@" SELECT COUNT(1) AS ProcessCount  FROM {Int_DbTableName.SyncLogDetail + Int_DbTableName.MonthTableSuffix}        
                    WHERE Status NOT IN (200, -100) AND [entity_name] =  @entity_name   ");

                return await ExecuteScalarAsync<int>(sql.ToString(), parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// The status will be marked as failed if any process runs for more than 60 minutes.
        /// </summary>
        /// <returns></returns>
        public async Task<int> UpdateLongRunningProcessStatusToFailure()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object> { };
            var sql = new StringBuilder($@"  Update   {Int_DbTableName.SyncLogDetail + Int_DbTableName.MonthTableSuffix} 
                     Set Status=-100,message='Long running process'  Where DateDiff(MI,start_time,GetDate())>5 AND isnull(Status,0)=0   ");
            return await ExecuteNonQueryAsync(sql.ToString(), parameters);
        }
        /// <summary>
        /// The process will begin with a status of 0.
        /// </summary>
        /// <param name="EntityName"></param>
        /// <returns></returns>
        public async Task<long> InitiateProcess(string EntityName)
        {
            long SyncLogId = -1;
            try
            {
                try
                {
                    string? syncType = string.Empty;
                    /* get syncType Push Or Pull */
                    Dictionary<string, object> parameters = new Dictionary<string, object> {
                                { "EntityName", EntityName }
                            };
                    var SyncTypeSql = new StringBuilder($@" SELECT TOP 1   action from int_entity where name = @EntityName");
                    syncType = await ExecuteScalarAsync<string>(SyncTypeSql.ToString(), parameters);
                    /* get new SynclogId */
                    Dictionary<string, object> SyncLogParameters = new Dictionary<string, object> {
                                { "EntityName", EntityName },{"SyncType",syncType}
                            };
                    var SyncLogInsert = new StringBuilder($@" INSERT INTO {Int_DbTableName.SyncLog + Int_DbTableName.MonthTableSuffix} (no_of_tables, Status, entity_name, action,start_time) 
                                VALUES(0, 0,  @EntityName ,@SyncType,GETDATE())   
                            select SCOPE_IDENTITY()  as SyncLogId ");
                    SyncLogId = await ExecuteScalarAsync<long>(SyncLogInsert.ToString(), SyncLogParameters);
                    if (SyncLogId <= 0)
                        throw new Exception("Not able to generate SyncLogId");

                    Dictionary<string, object> SyncLogDetailParameters = new Dictionary<string, object> {
                                { "EntityName", EntityName },{"SyncLogId",SyncLogId} };

                    var SyncLogDetailInsert = new StringBuilder($@"INSERT INTO {Int_DbTableName.SyncLogDetail + Int_DbTableName.MonthTableSuffix} (sync_log_id, [entity_name], [action], status)  
                                 SELECT @SyncLogId, name, [Action], 0  FROM int_entity WHERE  name= @EntityName ");

                    await ExecuteNonQueryAsync(SyncLogDetailInsert.ToString(), SyncLogDetailParameters);
                }
                catch
                {
                    throw;
                }
                return SyncLogId;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEntityDetails> FetchEntityDetails(string EntityName, long SyncLogId)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object> {
                    {"EntityName",EntityName },
                    { "SyncLogId",SyncLogId}
                };
                var sql = new StringBuilder($@"select  d.sync_log_id as SyncLogDetailId, d.[entity_name] as Entity, e.table_prefix + SUBSTRING(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 3, 2)  + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2)  as TableName, 
         SUBSTRING(CAST(YEAR(GETDATE()) AS VARCHAR(4)), 3, 2)  + RIGHT('00' + CAST(MONTH(GETDATE()) AS VARCHAR(2)), 2)  as TablePrefix, e.last_sync_time_stamp as lastsynctimestamp
         ,e.action from int_entity e inner join {Int_DbTableName.SyncLogDetail + Int_DbTableName.MonthTableSuffix} d on d.[entity_name] = e.[name] and d.[action] = e.[action]
         where d.sync_log_id =  @SyncLogId   and  e.[name] = @EntityName");

                return await ExecuteSingleAsync<IEntityDetails>(sql.ToString(), parameters)?? new EntityDetails();
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEntityData> GetEntityData(string EntityName)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object> {
                    {"EntityName",EntityName }
                };
                var sql = new StringBuilder($@"  select   name,action,table_prefix,sequence,is_mandatory,created_time,last_sync_time_stamp,description,SelectQuery,InsertQuery,MaxCount from int_entity where  name = @EntityName");
                return await ExecuteSingleAsync<Winit.Modules.Int_CommonMethods.Model.Interfaces.IEntityData>(sql.ToString(), parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> ProcessCompletion(long SyncLogId, string Message, int Status)
        {
            try
            {
                // string db = await GetSettingValueByKey("DB");
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                { "SyncLogId", SyncLogId },{"Message",Message},{"Status",Status}
                            };
                var syncLogDetailUpdate = new StringBuilder($@" UPDATE {Int_DbTableName.SyncLogDetail + Int_DbTableName.MonthTableSuffix} SET Message =@Message,Status =@Status
                        WHERE sync_log_id = @SyncLogId  ");

                /*
                 *  var syncLogUpdate = new StringBuilder($@" UPDATE {Int_DbTableName.SyncLog+Int_DbTableName.MonthTableSuffix} SET Message =@Message,Status =@Status
                WHERE SyncLogDetailId = @SyncLogDetailId  ");

                  var syncTimeUpdate = new StringBuilder($@" UPDATE {Int_DbTableName.SyncLog + Int_DbTableName.MonthTableSuffix} SET Message =@Message,Status =@Status
                  WHERE SyncLogDetailId = @SyncLogDetailId  ");
                  */
                int syncLogDetailCount = await ExecuteNonQueryAsync(syncLogDetailUpdate.ToString(), Parameters);
                //int syncLogCount = await ExecuteNonQueryAsync(syncLogUpdate.ToString(), Parameters);
                return syncLogDetailCount;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> InsertPendingData(IPendingDataRequest pendingData)
        {
            try
            {
                var syncLogDetailUpdate = new StringBuilder($@"insert into int_pushed_data_status (uid,linked_item_uid,status,linked_item_type)
                        values (newid(),@LinkedItemUid,@Status,@LinkedItemType)");
                int syncLogDetailCount = await ExecuteNonQueryAsync(syncLogDetailUpdate.ToString(), pendingData);
                return "success";
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> InsertPendingDataList(List<IPendingDataRequest> pendingData)
        {
            try
            {
                var syncLogDetailUpdate = new StringBuilder($@"insert into int_pushed_data_status (uid,linked_item_uid,status,linked_item_type)
                        values (newid(),@LinkedItemUid,@Status,@LinkedItemType)");
                int syncLogDetailCount = await ExecuteNonQueryAsync(syncLogDetailUpdate.ToString(), pendingData);
                return "success";
            }
            catch
            {
                throw;
            }
        }

        //public async Task<int> UpdatePushDataStatusByUID(List<string> Uids)
        //{
        //    try
        //    {
        //        string db = await GetSettingValueByKey("DB");
        //        var sql = new StringBuilder($@"update {db}.int_pushed_data_status  set status='success' where linked_item_uid in @Uids ");
        //        return await ExecuteNonQueryAsync(sql.ToString(), new { Uids });
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        public async Task<int> UpdatePushDataStatusByUID(List<SyncManagerModel.Interfaces.IPushDataStatus> pushDataStatus)
        {
            try
            {
                string db = await GetSettingValueByKey("DB");
                var sql = new StringBuilder($@"update {db}.int_pushed_data_status  set status=@Status,[error_message]=@ErrorMessage where linked_item_uid=@LinkedItemUid ");
                return await ExecuteNonQueryAsync(sql.ToString(), pushDataStatus);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> InsertDataIntoQueue(SyncManagerModel.Interfaces.IApiRequest Request)
        {
            try
            {
                var sql = new StringBuilder($@"insert into  {Int_DbTableName.IntegrationQueue + Int_DbTableName.MonthTableSuffix} (UID,entity_name,Content,Status) 
                              values (@UID,@EntityName,@JsonData,@Status)");

                return await ExecuteNonQueryAsync(sql.ToString(), Request);
            }
            catch { throw; }

        }

        public async Task<int> IntegrationProcessStatusInsertion(IIntegrationMessageProcess integrationMessage)
        {
            try
            {
                var sql = new StringBuilder($@"insert into Int_IntegrationMessageProcessStatus (interfacename,monthtablename,tableprefix,synclogdetailid,processstatus,errormessage,reqbatchnumber)
            values(@InterfaceName,@MonthTableName,@TablePrefix,@SyncLogDetailId,@ProcessStatus,@ErrorMessage,@ReqBatchNumber)");

                return await ExecuteNonQueryAsync(sql.ToString(), integrationMessage);
            }
            catch { throw; }
        }

        public async Task<List<SyncManagerModel.Interfaces.IApiRequest>> GetDataFromQueue(string Entity)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                { "Entity", Entity }
                };
                var sql = new StringBuilder($@"select  UID,[entity_name],Content as JsonData,Status from {Int_DbTableName.IntegrationQueue + Int_DbTableName.MonthTableSuffix} 
                    where [entity_name]=@Entity and isnull(Status,0)=0");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IApiRequest>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> UpdateQueueStatus(string UID)
        {
            try
            {
                var sql = new StringBuilder($@"update {Int_DbTableName.IntegrationQueue + Int_DbTableName.MonthTableSuffix}  set status=1 where UID = @UID ");
                return await ExecuteNonQueryAsync(sql.ToString(), new { UID });
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SyncManagerModel.Interfaces.IIntegrationProcessStatus>> GetAllOraclePendingProcessesByEntity(string Entity)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                { "Entity", Entity }
                };
                var sql = new StringBuilder($@"select ProcessId,InterfaceName,MonthTableName,TablePrefix,SyncLogDetailId as SyncLogId,CreatedOn,
                ProcessedOn,ProcessStatus,ErrorMessage,ReqBatchNumber,OracleStatus from Int_IntegrationMessageProcessStatus 
                where Isnull(ProcessStatus,0)=1 and Isnull(OracleStatus,0)=0 and (InterfaceName=@Entity or Isnull(@Entity,'')='')");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIntegrationProcessStatus>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IIntegrationProcessStatus>> GetAllPendingProcessByEntity(string Entity)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                { "Entity", Entity }
                };
                var sql = new StringBuilder($@"select ProcessId,InterfaceName,MonthTableName,TablePrefix,SyncLogDetailId as SyncLogId,CreatedOn,
                ProcessedOn,ProcessStatus,ErrorMessage,ReqBatchNumber,OracleStatus from Int_IntegrationMessageProcessStatus 
                where Isnull(ProcessStatus,0)=0 and (InterfaceName=@Entity or Isnull(@Entity,'')='')");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IIntegrationProcessStatus>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertItemDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_SKU_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                // return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
                var uids = await ExecuteQueryAsync<string>(sql.ToString(), Parameters);
                bool prepareSKU = true;
                if(uids.Count > 0)
                prepareSKU= await InvokePrepareSKUMasterAPI(uids); 
                return prepareSKU ? 1 : -1;
            }
            catch { throw; }
        }
        public async Task<int> InsertPriceDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_SKU_Price_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertPriceLadderingDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_Price_Laddering_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertTaxDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_Tax_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_credit_Limit_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertTemporaryCreditLimitDataIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_credit_Limit_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertCustomerOracleCodeIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_Customer_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                await ExecuteNonQueryAsync(sql.ToString(), Parameters);
                var uids = await ExecuteQueryAsync<string>(sql.ToString(), Parameters);
                bool prepareStore =true;
                 if (uids.Count>0)
                    prepareStore= await InvokePrepareStoreMasterAPI(uids);

                //Console.WriteLine(prepareStore ? "Prepare Store Master Api was invoked successfully" : "failed");
                return prepareStore ? 1 : -1;
            }
            catch { throw; }
        }

        public async Task<int> InsertPurchaseOrderConfirmationIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {

            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_PurchaseOrder_Confirmation_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> InsertInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_invoice_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertOutStandingInvoicesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_OutStanding_Invoice_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertTemporaryCreditLimitsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_CMI_OutStanding_Invoice_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertWarehouseStocksIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_Warehouse_Stock_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertProvisionsIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_provision_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<int> InsertProvisionCreditNotesIntoMainTable(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" exec usp_provision_creditnote_Process 
                              @strSyncLogDetailId = @SyncLogDetailId, 
                              @strTableName = @TableName, 
                              @strTablePrefix = @TablePrefix");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }
        public async Task<List<SyncManagerModel.Interfaces.IInt_InvoiceHeader>> GetProcessingInvoiceBlobFiles(string TableName, string TablePrefix, long SyncLogDetailId)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"SyncLogDetailId",SyncLogDetailId },{"TableName",TableName },{"TablePrefix",TablePrefix }
                };
                var sql = new StringBuilder($@" select oracle_order_number,delivery_id,gst_invoice_number,invoice_date,invoice_file 
                        from {Int_DbTableName.InvoiceHeader}{TablePrefix}  where sync_log_id=@SyncLogDetailId");
                return await ExecuteQueryAsync<SyncManagerModel.Interfaces.IInt_InvoiceHeader>(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> UpdateIntegrationProcessStatusByProcessId(long processId, int oracleStatus)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object>
                {
                    {"ProcessId",processId },
                    {"OracleStatus",oracleStatus}
                };
                var sql = new StringBuilder($@"update int_integrationmessageprocessstatus set OracleStatus=@OracleStatus  where ProcessId=@ProcessId ");
                return await ExecuteNonQueryAsync(sql.ToString(), Parameters);
            }
            catch { throw; }
        }

        public async Task<int> SaveOrUpdateInvoiceBlobFilePath(SyncManagerModel.Interfaces.IInt_InvoiceHeader invoiceHeader, string path)
        {
            try
            {
                Dictionary<string, object> Parameters = new Dictionary<string, object> {
                                 {"DeliveryId",invoiceHeader.DeliveryId },{"Path",path },{"FileSize",invoiceHeader.InvoiceFile.Length }
                };
                string db = await GetSettingValueByKey("DB") ?? "";
                var insertSql = new StringBuilder($@"
                        IF NOT EXISTS (SELECT 1 FROM {db}.file_sys WHERE linked_item_uid = @DeliveryId AND file_name = 'Invoice_' + @DeliveryId + '.pdf')
                        BEGIN
                    insert into {db}.file_sys (uid,created_by,created_time,modified_by,modified_time,server_add_time
                    ,server_modified_time,linked_item_type,linked_item_uid,file_sys_type,file_type,file_name,display_name,file_size,relative_path)
                    select newid(),'Admin', getdate(),'Admin', getdate(),getdate(),getdate(),'Invoice',@DeliveryId,'Invoice File','Pdf'
                        ,'Invoice_'+@DeliveryId+'.pdf','Invoice_'+@DeliveryId+'.pdf',@FileSize,@Path  
                            end;
                            ");
                var updateSql = new StringBuilder($@" update  {db}.file_sys  set relative_path=@Path,file_size=@FileSize
                ,modified_time=,server_modified_time=getdate() where linked_item_uid=@DeliveryId");

                await ExecuteNonQueryAsync(updateSql.ToString(), Parameters);
                return await ExecuteNonQueryAsync(insertSql.ToString(), Parameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> InvokePrepareStoreMasterAPI(List<string> uids)
        {
            Winit.Shared.Models.Common.ApiResponse<string> response = await
           _apiService.FetchDataAsync
              ($"{apiBaseUrl}DataPreparation/PrepareStoreMaster", HttpMethod.Post, uids);
            return response.IsSuccess;
        }

        public async Task<bool> InvokePrepareSKUMasterAPI(List<string> uids)
        {
            Winit.Shared.Models.Common.ApiResponse<string> response = await
          _apiService.FetchDataAsync
             ($"{apiBaseUrl}DataPreparation/PrepareSKUMaster", HttpMethod.Post, new { SKUUIDs = uids });
            return response.IsSuccess;
        }
    }
}
