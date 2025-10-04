using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLProvisionDL : SqlServerDBManager, IProvisionDL
    {
        public MSSQLProvisionDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertProvisionDataIntoMonthTable(List<IProvision> provisions, IEntityDetails entityDetails)
        {
            try
            {
                provisions.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,source,is_processed,
                inserted_on,processed_on,error_description,common_attribute1,common_attribute2
                    ,provision_id, customer_code, branch, sales_office, oracle_order_number,delivery_id 
                    , gst_invoice_number, ar_no, invoice_date, item_code, qty,scheme_type, scheme_amount, naration)
              select @SyncLogId, @UID, @Source, @IsProcessed, @InsertedOn, @ProcessedOn,@ErrorDescription, @CommonAttribute1, @CommonAttribute2
                    , @ProvisionId, @CustomerCode, @Branch, @SalesOffice, @OracleOrderNumber,@DeliveryId 
              ,@GstInvoiceNumber, @ArNo, @InvoiceDate, @ItemCode, @Qty,@SchemeType, @SchemeAmount, @Naration");
                return await ExecuteNonQueryAsync(monthSql.ToString(), provisions);
            }
            catch
            {
                throw;
            }
        }


    }
}
