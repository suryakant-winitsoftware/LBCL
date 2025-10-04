using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLStatementAndBalanceConfiStagingDL : SqlServerDBManager, IStatementAndBalanceConfiStagingDL
    {
        public MSSQLStatementAndBalanceConfiStagingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertStatementAndBalanceConfiIntoMonthTable(List<IStatementAndBalanceConfi> statementAndBalances, IEntityDetails entityDetails)
        {
            try
            {
                statementAndBalances.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,source,is_processed,
                inserted_on,processed_on,error_description,common_attribute1,common_attribute2,sales_office, branch, customer_number,
                customer_location_code,operating_unit, transaction_type, trx_number, document_number,description, trx_date, dr_amount, cr_amount)
                      select @SyncLogId, @UID, @Source, @IsProcessed, @InsertedOn, @ProcessedOn,@ErrorDescription, @CommonAttribute1,
                @CommonAttribute2,@SalesOffice, @Branch, @CustomerNumber, @CustomerLocationCode,@OperatingUnit, @TransactionType, @TrxNumber,
                @DocumentNumber,@Description, @TrxDate, @DrAmount, @CrAmount");
                return await ExecuteNonQueryAsync(monthSql.ToString(), statementAndBalances);
            }
            catch
            {
                throw;
            }
        }
    }
}
