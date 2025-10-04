using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class Customer_RefStagingDL : SqlServerDBManager, ICustomer_RefStagingDL
    {
        public Customer_RefStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertCustomerReferenceDataIntoMonthTable(List<ICustomer_Ref> customer_Refs, IEntityDetails entityDetails)
        {
            try
            {
                customer_Refs.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], [source], is_processed,  
                     customer_code,is_active,is_blocked,blocked_reason,inserted_on,processed_on,error_description,
                     common_attribute1,common_attribute2,custom_field_1,custom_field_2,custom_field_3,custom_field_4,custom_field_5)
                
                       values (@SyncLogId, @UID, @Source, @IsProcessed,  @CustomerCode, @IsActive,@IsBlocked,
                        @BlockedReason, @InsertedOn, @ProcessedOn, @ErrorDescription, @CommonAttribute1, @CommonAttribute2,
                        @CustomField1,@CustomField2,@CustomField3,@CustomField4,@CustomField5);"
                 );
                return await ExecuteNonQueryAsync(monthSql.ToString(), customer_Refs);

            }
            catch { throw; }

        }
    }
}
