using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Classes
{
    public class OracleTemporaryCreditLimitPullDL: OracleServerDBManager,ITemporaryCreditLimitPullDL
    {
        public OracleTemporaryCreditLimitPullDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<ITemporaryCreditLimit>> PullTemporaryCreditLimitDetailsFromOracle(string sql)
        {
            try
            { 
                List<ITemporaryCreditLimit> temporaryCreditLimits = await ExecuteQueryAsync<ITemporaryCreditLimit>(sql.ToString(), null);
                return temporaryCreditLimits;
            }
            catch
            {
                throw;
            }
        }
    }
}
