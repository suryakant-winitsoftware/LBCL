using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Classes;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Classes
{
    public class OraclePricingMasterDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, IPricingMasterDL
    {
        public OraclePricingMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IPricingMaster>> GetPricingMasterDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IPricingMaster> pricingMaster = await ExecuteQueryAsync<IPricingMaster>(sql.ToString(), parameters);
                return pricingMaster;
            }
            catch
            {
                throw;
            }
        }
    }
}
