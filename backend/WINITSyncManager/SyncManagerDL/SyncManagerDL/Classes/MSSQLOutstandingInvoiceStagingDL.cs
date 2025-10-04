using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;
using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Classes
{
    public class MSSQLOutstandingInvoiceStagingDL : SqlServerDBManager,IOutstandingInvoiceStagingDL
    {
        public MSSQLOutstandingInvoiceStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async  Task<int> InsertOutstandingInvoiceDataIntoMonthTable(List<SyncManagerModel.Interfaces.IOutstandingInvoice> outstandings, IEntityDetails entityDetails)
        {
            try
            {
                outstandings.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (
                sync_log_id, [UID], [source], is_processed, inserted_on, processed_on, error_description, common_attribute1, 
                common_attribute2, rec_uid,ou,customer_number,invoice_number,invoice_date,tax_invoice_num,tax_invoice_date,net_amount,balance_amount
                ,invoice_type,invoice_due_date,division) 
                values (@SyncLogId, @UID, @Source, @IsProcessed, @InsertedOn, @ProcessedOn, @ErrorDescription, @CommonAttribute1, 
                @CommonAttribute2,@RecUid, @Ou, @CustomerNumber, @InvoiceNumber, @InvoiceDate, @TaxInvoiceNum,
            @TaxInvoiceDate, @NetAmount, @BalanceAmount, @InvoiceType, @InvoiceDueDate,@Division  );");
                return await ExecuteNonQueryAsync(monthSql.ToString(), outstandings);
            }
            catch
            {
                throw;
            }

        }
    }
}
