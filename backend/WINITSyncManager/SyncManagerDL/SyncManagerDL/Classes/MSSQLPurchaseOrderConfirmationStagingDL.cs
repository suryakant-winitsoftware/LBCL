using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLPurchaseOrderConfirmationStagingDL : SqlServerDBManager, Iint_PurchaseOrderConfirmationStagingDL
    {
        public MSSQLPurchaseOrderConfirmationStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertPurchaseOrderCancellationDataIntoMonthTable(List<Iint_PurchaseOrderCancellation> purchaseOrderCancellations, IEntityDetails entityDetails)
        {
            try
            {
                purchaseOrderCancellations.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], is_processed, inserted_on, processed_on, 
                        error_description, common_attribute1, common_attribute2, erp_order_number,item_code,cancelled_qty)
                        values (@SyncLogId, @UID, @IsProcessed, @InsertedOn, @ProcessedOn, 
                        @ErrorDescription, @CommonAttribute1, @CommonAttribute2,@ErpOrderNumber  ,@ItemCode  ,@CancelledQty );");
                return await ExecuteNonQueryAsync(monthSql.ToString(), purchaseOrderCancellations);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> InsertPurchaseOrderStatusDataIntoMonthTable(List<Iint_PurchaseOrderStatus> purchaseOrderStatuses, IEntityDetails entityDetails)
        {
            try
            {
                purchaseOrderStatuses.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], is_processed, inserted_on, processed_on, 
      error_description, common_attribute1, common_attribute2, purchase_order_uid,erp_order_number,erp_order_date )
      VALUES (@SyncLogId, @UID, @IsProcessed, @InsertedOn, @ProcessedOn, 
      @ErrorDescription, @CommonAttribute1, @CommonAttribute2,@PurchaseOrderUid ,@ErpOrderNumber,@ErpOrderDate);");
                return await ExecuteNonQueryAsync(monthSql.ToString(), purchaseOrderStatuses);
            }
            catch
            {
                throw;
            }
        }
    }
}
