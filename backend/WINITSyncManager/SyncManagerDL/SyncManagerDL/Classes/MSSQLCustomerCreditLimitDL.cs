using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System.Text;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class MSSQLCustomerCreditLimitDL : SqlServerDBManager, ICustomerCreditLimitStagingDL
    {
        public MSSQLCustomerCreditLimitDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertCreditLimitDataIntoMonthTable(List<ICustomerCreditLimit> creditLimits, IEntityDetails entityDetails)
        {
            try
            {
                creditLimits.ForEach(item => { item.SyncLogId = entityDetails.SyncLogDetailId; item.UID = entityDetails.UID; });

                var monthSql = new StringBuilder($@"Insert into {entityDetails.TableName} (sync_log_id, [UID], [source], is_processed, 
                inserted_on, processed_on, error_description, common_attribute1,common_attribute2, org_uid, customer_number, division, sales_office, 
                credit_limit, marginal_credit_limit, effective_from, effective_upto
            ) values (@SyncLogId, @UID, @Source, @IsProcessed,@InsertedOn, @ProcessedOn, @ErrorDescription, @CommonAttribute1, 
                @CommonAttribute2, @OrgUid, @CustomerNumber, @Division, @SalesOffice, @CreditLimit, @MarginalCreditLimit, @EffectiveFrom, @EffectiveUpto
            );");
                return await ExecuteNonQueryAsync(monthSql.ToString(), creditLimits);
            }
            catch
            {
                throw;
            }
        }
    }
}
