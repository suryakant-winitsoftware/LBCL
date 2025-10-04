using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLWarehouseStockDL : SqlServerDBManager, IWarehouseStockStagingDL
    {
        public MSSQLWarehouseStockDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertWarehouseStockDataIntoMonthTable(List<IWarehouseStock> warehouseStocks, IEntityDetails entityDetails)
        {
            try
            {
                warehouseStocks.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,source,is_processed,
                inserted_on,processed_on,error_description,common_attribute1,common_attribute2,organisation_unit, warehouse_code, sub_warehouse_code, sku_code, qty)
              select @SyncLogId, @UID, @Source, @IsProcessed, @InsertedOn, @ProcessedOn,@ErrorDescription, @CommonAttribute1, @CommonAttribute2
                    ,@OrganisationUnit, @WarehouseCode, @SubWarehouseCode, @SkuCode, @Qty");
                return await ExecuteNonQueryAsync(monthSql.ToString(), warehouseStocks);
            }
            catch
            {
                throw;
            }
        }
    }
}
