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
    public class OraclePriceLadderingDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, IPriceLadderingDL
    {
        public OraclePriceLadderingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IPriceLaddering>> GetPriceLadderingDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IPriceLaddering> priceLaddering = await ExecuteQueryAsync<IPriceLaddering>(sql.ToString(), parameters);
                return priceLaddering;
            }
            catch
            {
                throw;
            }
        }
    }
}
