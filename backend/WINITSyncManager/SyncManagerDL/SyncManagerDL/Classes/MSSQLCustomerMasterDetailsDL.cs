using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class MSSQLCustomerMasterDetailsDL : SqlServerDBManager, ICustomerMasterDetailsDL
    {
        public MSSQLCustomerMasterDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails)
        {
            try
            {
                //await InsertIntoMonthAndQueueTable(entityDetails);
                List<ICustomerMasterDetails> customerMasterDetails = await GetCustomerDataToPush();
                await InsertIntoMonthTable(customerMasterDetails, entityDetails);
                await InsertIntoQueueTable(entityDetails);
                var parameters = new Dictionary<string, object?>()
                {
                    { "SyncLogDetailId",entityDetails.SyncLogDetailId}
                };
                var sql = new StringBuilder($@" select Store_Code,Store_UID,sync_log_id,uid,inserted_on,customer_name,sales_office,classification,address1
                ,address2,pincode,city,state,country,first_name,phn_no,mobile,email,pan_no_gst_no,warehouse,purpose,primary_customer,legal_name,
                addresskey,oraclecustomercode,oraclelocationcode,Site_Number,readfromoracle from {Int_DbTableName.CustomerMasterPush + Int_DbTableName.QueueTableSuffix}  where sync_log_id=@SyncLogDetailId");
                List<ICustomerMasterDetails> customerMasterPush = await ExecuteQueryAsync<ICustomerMasterDetails>(sql.ToString(), parameters);
                return customerMasterPush;
            }
            catch { throw; }
        }

        public Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers)
        {
            throw new NotImplementedException();
        }
        private async Task<List<ICustomerMasterDetails>> GetCustomerDataToPush()
        {
            try
            {
                string? db = await GetSettingValueByKey("DB");
                if (db == null)
                    throw new Exception("There is no value in setting table for DB key");
                var parameters = new Dictionary<string, object?>() { };
                var sql = new StringBuilder($@"select DISTINCT s.uid store_uid,s.code store_code,s.[name] as customer_name,c.[name] as firstname,
                s.classfication_type as classification,a.line1 as address1,a.line2 as address2,a.zip_code as pincode,a.city as city,a.state as state ,a.country_code as country
                ,a.mobile1 as mobile ,a.mobile2 as phn_no ,a.email,s.tax_doc_number as pan_no_gst_no,case when A.type='Billing' then'Bill To' when A.type='Shipping' then 'Ship To' else A.type end  as purpose,
                case when isnull(a.is_default,0)=1 then 'Primary' else 'Secondary' end primary_customer,s.legal_name,a.uid as addresskey,
                wh.code as warehouse, /*so.code as salesoffice */
                CASE 
                        WHEN a.type = 'Billing' AND a.sales_office_uid IS NULL THEN 
                            (
                                SELECT TOP 1 so2.code 
                                FROM {db}.address a2
                                LEFT JOIN {db}.sales_office so2 ON so2.uid = a2.sales_office_uid
                                WHERE 
                                    a2.linked_item_uid = s.uid 
                                    AND a2.type = 'Shipping' 
                                    AND ISNULL(a2.is_default, 0) = 1
                            )
                        ELSE so.code
                    END AS salesoffice,
                     A.custom_field1 as OracleCustomerCode ,A.custom_field3 as Site_Number,A.custom_field2 as OracleLocationCode  
                from {db}.store s 
                inner join {db}.store_additional_info si on si.store_uid=s.uid
                inner join {db}.address a on s.uid=a.linked_item_uid
                inner join {db}.int_pushed_data_status ps on (ps.linked_item_uid=s.uid and ps.linked_item_type='store') 
                or ( ps.linked_item_uid=a.uid and ps.linked_item_type='customeraddress')
                inner join {db}.contact c on c.linked_item_uid=s.uid AND c.is_default = 1
                inner join {db}.branch b on b.uid=a.branch_uid 
                left join {db}.sales_office so on so.uid=a.sales_office_uid
                left join {db}.org wh on wh.uid = so.warehouse_uid
                where isnull(s.code,'')<>''  and isnull(s.tax_doc_number,'')<>'' and  isnull(a.city,'')<>'' 
                and isnull(a.state,'')<>''  and isnull(a.zip_code,'')<>''  and isnull(ps.status,'')='pending'");
                List<ICustomerMasterDetails> customerMasterPush = await ExecuteQueryAsync<ICustomerMasterDetails>(sql.ToString(), parameters);
                return customerMasterPush;
            }
            catch
            {
                throw;
            }
        }
        private async Task<int> InsertIntoMonthTable(List<ICustomerMasterDetails> customerMasterDetails, IEntityDetails entityDetails)
        {
            try
            {
                customerMasterDetails.ForEach(item => { item.SyncLogDetailId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,uid,store_code,store_uid,inserted_on,customer_name,sales_office,classification,address1
            ,address2,pincode,city,state,country,first_name,phn_no,mobile,email,pan_no_gst_no,warehouse,purpose,primary_customer,legal_name,
                addresskey,oraclecustomercode,oraclelocationcode,Site_Number,readfromoracle)
              select @SyncLogDetailId, @UID ,@StoreCode,@StoreUID,GETDATE(),@CustomerName ,@SalesOffice ,@Classification ,@Address1 ,@Address2 ,@Pincode ,@City ,@State ,@Country
                ,@FirstName ,@PhnNo ,@Mobile ,@Email ,@PanNoGstNo ,@Warehouse ,@Purpose ,@PrimaryCustomer ,@LegalName ,@AddressKey 
                    ,@OracleCustomerCode ,@OracleLocationCode ,@Site_Number,@ReadFromOracle ");
                return await ExecuteNonQueryAsync(monthSql.ToString(), customerMasterDetails);

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
                var truncateQuery = new StringBuilder($@" truncate table  {Int_DbTableName.CustomerMasterPush + Int_DbTableName.QueueTableSuffix};");
                await ExecuteNonQueryAsync(truncateQuery.ToString(), null);
                var queueSql = new StringBuilder($@" Insert into {Int_DbTableName.CustomerMasterPush + Int_DbTableName.QueueTableSuffix} (sync_log_id,uid,store_code,store_uid,inserted_on,customer_name,sales_office,classification,address1
            ,address2,pincode,city,state,country,first_name,phn_no,mobile,email,pan_no_gst_no,warehouse,purpose,primary_customer,legal_name,
                addresskey,oraclecustomercode,oraclelocationcode,Site_Number,readfromoracle)
                select sync_log_id,uid,store_code,store_uid,inserted_on,customer_name,sales_office,classification,address1
            ,address2,pincode,city,state,country,first_name,phn_no,mobile,email,pan_no_gst_no,warehouse,purpose,primary_customer,legal_name,
                addresskey,oraclecustomercode,oraclelocationcode,Site_Number,readfromoracle from {entityDetails.TableName} where sync_log_id =@SyncLogDetailId");

                return await ExecuteNonQueryAsync(queueSql.ToString(), queueParameters);
            }
            catch { throw; }
        }
    }
}
