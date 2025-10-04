using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLPriceLadderingStagingDL : SqlServerDBManager, IPriceLadderingStagingDL
    {
        public MSSQLPriceLadderingStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertPriceLadderingDataIntoMonthTable(List<IPriceLaddering> priceLaddering, IEntityDetails entityDetails)
        {
            try
            {
                priceLaddering.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], [source], is_processed, inserted_on, processed_on, 
                 error_description, common_attribute1, common_attribute2, laddering_id, operating_unit, division, 
                    product_category_id, branch, sales_office, broad_customer_classification, 
                    discount_type, percentage_discount, start_date, end_date)
                 VALUES (@SyncLogId, @UID,  @Source, @IsProcessed, @InsertedOn, @ProcessedOn, 
                 @ErrorDescription, @CommonAttribute1, @CommonAttribute2, @LadderingId,@OperatingUnit,@Division,
                @ProductCategoryId,@Branch,@SalesOffice,@BroadCustomerClassification,@DiscountType,@PercentageDiscount,
                @StartDate,@EndDate);");
                return await ExecuteNonQueryAsync(monthSql.ToString(), priceLaddering);
            }
            catch
            {
                throw;
            }
        }
    }
}
