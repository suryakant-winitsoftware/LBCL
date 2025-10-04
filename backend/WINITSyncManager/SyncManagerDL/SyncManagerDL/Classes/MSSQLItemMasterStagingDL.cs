using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLItemMasterStagingDL : SqlServerDBManager, IItemMasterStagingDL
    {
        public MSSQLItemMasterStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertItemDataIntoMonthTable(List<IItemMaster> itemMaster, IEntityDetails entityDetails)
        {
            try
            {
                itemMaster.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,source,is_processed,
                inserted_on,processed_on,error_description,common_attribute1,common_attribute2,item_id,item_code,item_description,
                model_code,year,division,product_group,product_type,item_category,tonnage,capacity,star_rating,product_category,product_category_id,
                parent_item_code,parent_item_id,item_series,image_name,hsn_code,Item_Status)
              select @SyncLogId, @UID, @Source, @IsProcessed, @InsertedOn, @ProcessedOn,@ErrorDescription, @CommonAttribute1, @CommonAttribute2, @ItemId, @ItemCode, 
            @ItemDescription, @ModelCode, @Year, @Division, @ProductGroup, @ProductType,@ItemCategory, @Tonnage, @Capacity, @StarRating, @ProductCategory, 
            @ProductCategoryId, @ParentItemCode, @ParentItemId, @ItemSeries,@ImageName, @HsnCode,@ItemStatus");
                return await ExecuteNonQueryAsync(monthSql.ToString(), itemMaster);
            }
            catch
            {
                throw;
            }
        }
    }
}
