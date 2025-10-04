using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLTaxMasterStagingDL : SqlServerDBManager, ITaxMasterStagingDL
    {
        public MSSQLTaxMasterStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertTaxDataIntoMonthTable(List<ITaxMaster> taxMasters, IEntityDetails entityDetails)
        {
            try
            {
                taxMasters.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], is_processed, inserted_on, processed_on, 
                error_description, common_attribute1, common_attribute2,hsn_code,tax_percentage,start_date,end_date)
                VALUES (@SyncLogId, @UID, @IsProcessed, @InsertedOn, @ProcessedOn, 
                @ErrorDescription, @CommonAttribute1, @CommonAttribute2,@HsnCode,@TaxPercentage,@StartDate,@EndDate);");
                return await ExecuteNonQueryAsync(monthSql.ToString(), taxMasters);
            }
            catch
            {
                throw;
            }
        }
    }
}
