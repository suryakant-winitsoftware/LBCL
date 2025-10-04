using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLInt_InvoiceStagingDL : SqlServerDBManager, Iint_InvoiceStagingDL
    {
        public MSSQLInt_InvoiceStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertInvoiceHeaderDataIntoMonthTable(List<IInt_InvoiceHeader> invoiceHeaders, IEntityDetails entityDetails)
        {

            try
            {
                invoiceHeaders.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [uid], is_processed, 
                        inserted_on, processed_on, error_description, common_attribute1,common_attribute2, 
                        oracle_order_number, delivery_id, gst_invoice_number,invoice_date, invoice_file,ar_number)
                        values (@SyncLogId, @UID,   @IsProcessed,@InsertedOn,
                         @ProcessedOn, @ErrorDescription, @CommonAttribute1, @CommonAttribute2, @OracleOrderNumber,
                         @DeliveryId, @GstInvoiceNumber, @InvoiceDate, @InvoiceFile,@ArNumber)");
                return await ExecuteNonQueryAsync(monthSql.ToString(), invoiceHeaders);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> InsertInvoiceLineDataIntoMonthTable(List<IInt_InvoiceLine> invoiceLines, IEntityDetails entityDetails)
        {
            try
            {
                invoiceLines.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,  is_processed,
                            inserted_on,processed_on,error_description,common_attribute1,
                            common_attribute2,delivery_id,item_code,ordered_qty,shipped_qty,cancelled_qty)
                            values(@SyncLogId,@UID ,@IsProcessed,
                            @InsertedOn,@ProcessedOn,@ErrorDescription,@CommonAttribute1,
                            @CommonAttribute2,@DeliveryId,@ItemCode,@OrderedQty,@ShippedQty,@CancelledQty)");
                return await ExecuteNonQueryAsync(monthSql.ToString(), invoiceLines);
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> InsertInvoiceSerialNoDataIntoMonthTable(List<IInt_InvoiceSerialNo> invoiceSerialNos, IEntityDetails entityDetails)
        {
            try
            {
                invoiceSerialNos.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID, is_processed,
                            inserted_on,processed_on,error_description,common_attribute1,
                            common_attribute2,delivery_id,item_code,serial_numbers)
                            values(@SyncLogId,@UID ,@IsProcessed,
                            @InsertedOn,@ProcessedOn,@ErrorDescription,@CommonAttribute1,
                            @CommonAttribute2,@DeliveryId,@ItemCode,@SerialNumbers)");
                return await ExecuteNonQueryAsync(monthSql.ToString(), invoiceSerialNos);
            }
            catch
            {
                throw;
            }
        }
    }
}
