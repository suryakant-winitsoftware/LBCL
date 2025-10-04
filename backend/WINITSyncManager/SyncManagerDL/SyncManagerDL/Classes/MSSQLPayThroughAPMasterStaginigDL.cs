using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLPayThroughAPMasterStaginigDL : SqlServerDBManager, IPayThroughAPMasterStaginigDL
    {
        public MSSQLPayThroughAPMasterStaginigDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertPayThroughAPMasterDataIntoMonthTable(List<IPayThroughAPMaster> taxMasters, IEntityDetails entityDetails)
        {

            try
            {
                taxMasters.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (
                    sync_log_id, [UID], DIVISION, BRAND_CATEGORY, SKU_CODE, CONSUMER_FINANCE, SERVICE_COMMISSION, FREE_INSTALLATION, 
                    FTS, START_DATE, END_DATE, IS_PROCESSED, INSERTED_ON, PROCESSED_ON, ERROR_DESCRIPTION, 
                    COMMON_ATTRIBUTE1, COMMON_ATTRIBUTE2, CUSTOM_FIELD_1, CUSTOM_FIELD_2, CUSTOM_FIELD_3, 
                    CUSTOM_FIELD_4, CUSTOM_FIELD_5, S_RW_ID
                ) VALUES (
                    @SyncLogId, @UID, @Division, @BrandCategory, @SkuCode, @ConsumerFinance, @ServiceCommission, 
                    @FreeInstallation, @FTS, @StartDate, @EndDate, @IsProcessed,@InsertedOn,@ProcessedOn,@ErrorDescription,@CommonAttribute1,@CommonAttribute2,
                    @CustomField1, @CustomField2, @CustomField3, @CustomField4, @CustomField5, @S_RW_ID
                );");
                return await ExecuteNonQueryAsync(monthSql.ToString(), taxMasters);
            }
            catch
            {
                throw;
            }
        }
    }
}
