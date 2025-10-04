using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLPricingMasterStagingDL : SqlServerDBManager, IPricingMasterStagingDL
    {
        public MSSQLPricingMasterStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertPricingDataIntoMonthTable(List<IPricingMaster> pricingMaster, IEntityDetails entityDetails)
        {
            try
            {
                pricingMaster.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID],  [status], message, [source], is_processed, inserted_on, processed_on, 
                 error_description, common_attribute1, common_attribute2, price_master_id, item_code, mrp, dp,min_selling_price, [start_date], end_date, creation_date)
                 VALUES (@SyncLogId, @UID, @Status, @Message, @Source, @IsProcessed, @InsertedOn, @ProcessedOn, 
                 @ErrorDescription, @CommonAttribute1, @CommonAttribute2, @PriceMasterId, @ItemCode, @MRP, @DP, 
                 @MinSellingPrice, @StartDate, @EndDate, @CreationDate);");
                return await ExecuteNonQueryAsync(monthSql.ToString(), pricingMaster);
            }
            catch
            {
                throw;
            }
        }
    }
}
