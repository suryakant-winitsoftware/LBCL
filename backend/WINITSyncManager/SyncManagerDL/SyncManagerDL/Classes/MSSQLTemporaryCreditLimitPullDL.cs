using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;

namespace SyncManagerDL.Classes
{
    public class MSSQLTemporaryCreditLimitPullDL : SqlServerDBManager, ITemporaryCreditLimitPullDL
    {
        public MSSQLTemporaryCreditLimitPullDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
       public async Task<int> InsertTemporaryCreditLimitDetailsIntoMonthTable(List<ITemporaryCreditLimit> temporaryCreditLimits, IEntityDetails entityDetails)
        {
            try
            {
                temporaryCreditLimits.ForEach(obj => { obj.SyncLogId = entityDetails.SyncLogDetailId; obj.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id,UID, req_uid,request_number,customer_code
                ,request_type,start_date,end_date,requested_credit_limit,requested_credit_days,remarks)
                 select @SyncLogId, @UID ,@ReqUID,@RequestNumber, @CustomerCode ,@RequestType ,@StartDate ,@EndDate ,@RequestedCreditLimit 
                ,@RequestedCreditDays ,@Remarks ");
                return await ExecuteNonQueryAsync(monthSql.ToString(), temporaryCreditLimits);
            }
            catch
            {
                throw;
            }
        }


    }
}
