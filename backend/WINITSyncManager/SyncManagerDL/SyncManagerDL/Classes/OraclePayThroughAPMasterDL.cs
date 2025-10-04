using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Classes
{
    public class OraclePayThroughAPMasterDL : OracleServerDBManager, IPayThroughAPMasterDL
    {
        public OraclePayThroughAPMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IPayThroughAPMaster>> GetPayThroughAPMasterDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IPayThroughAPMaster> payThroughAPMasters = await ExecuteQueryAsync<IPayThroughAPMaster>(sql.ToString(), parameters);
                return payThroughAPMasters;
            }
            catch
            {
                throw;
            }
        }
    }
}
