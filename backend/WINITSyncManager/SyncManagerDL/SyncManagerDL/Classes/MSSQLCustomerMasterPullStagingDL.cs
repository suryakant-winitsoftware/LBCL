using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLCustomerMasterPullStagingDL : SqlServerDBManager,ICustomerMasterPullStagingDL
    {
        public MSSQLCustomerMasterPullStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertCustomerMasterPullDataIntoMonthTable(List<ICustomerMasterPull> customers, IEntityDetails entityDetails)
        {
            try
            {
                customers.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName}(sync_log_id,UID,source,is_processed,inserted_on,processed_on,error_description,common_attribute1,
                common_attribute2,AddressKey,OracleCustomerCode,OracleLocationCode,ReadFromOracle,Site_Number)
                values(@SyncLogId,@UID,@Source,@IsProcessed,@InsertedOn,@ProcessedOn,@ErrorDescription,@CommonAttribute1,
                @CommonAttribute2,@AddressKey,@OracleCustomerCode,@OracleLocationCode,@ReadFromOracle,@Site_Number);");
                return await ExecuteNonQueryAsync(monthSql.ToString(), customers);
            }
            catch
            {
                throw;
            }
        }

        
    }
}
