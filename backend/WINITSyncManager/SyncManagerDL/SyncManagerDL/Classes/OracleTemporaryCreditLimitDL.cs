using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class OracleTemporaryCreditLimitDL : OracleServerDBManager, ITemporaryCreditLimitDL
    {
        public OracleTemporaryCreditLimitDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertCustomerdetailsIntoOracleStaging(List<ITemporaryCreditLimit> temporaryCreditLimits)
        {
            try
            {
                var sql = new StringBuilder($@"insert into  {Int_OracleTableNames.TemporaryCreditLimit} ( inserted_on, req_uid, request_number
                  , customer_code, request_type,div, start_date, end_date, requested_credit_limit, requested_credit_days, remarks, oracle_status,is_auto_approved) 
                  VALUES (TO_DATE(:InsertedOn, 'DD-MON-YYYY HH24:MI:SS') ,  :ReqUid, :RequestNumber, :CustomerCode, :RequestType,:division, :StartDate, :EndDate, :RequestedCreditLimit,
                  :RequestedCreditDays, :Remarks, :OracleStatus,:IsAutoApproved)"
                );

                return await ExecuteNonQueryAsync(sql.ToString(), temporaryCreditLimits);

            }
            catch { throw; }
        }

    }
}
