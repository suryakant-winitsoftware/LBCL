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
    public class OracleTaxMasterDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, ITaxMasterDL
    {
        public OracleTaxMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<ITaxMaster>> GetTaxMasterDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<ITaxMaster> taxMaster = await ExecuteQueryAsync<ITaxMaster>(sql.ToString(), parameters);
                return taxMaster;
            }
            catch
            {
                throw;
            }
        }
    }
}
