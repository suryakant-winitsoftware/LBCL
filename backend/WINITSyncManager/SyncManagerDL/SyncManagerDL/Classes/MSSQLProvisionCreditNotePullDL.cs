using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLProvisionCreditNotePullDL : SqlServerDBManager, IProvisionCreditNotePullDL
    {
        public MSSQLProvisionCreditNotePullDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertProvisionCreditNoteDetailsIntoMonthTable(List<IProvisionCreditNote> provisionCreditNotes, IEntityDetails entityDetails)
        {
            try
            {
                provisionCreditNotes.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID,provision_id, dms_release_requested, oracle_processed, cn_number, cn_date, cn_amount )
                 select @SyncLogId, @UID ,@ProvisionId, @DmsReleaseRequested, @OracleProcessed, @CnNumber, @CnDate, @CnAmount ");
                return await ExecuteNonQueryAsync(monthSql.ToString(), provisionCreditNotes);
            }
            catch
            {
                throw;
            }
        }
    }
}
