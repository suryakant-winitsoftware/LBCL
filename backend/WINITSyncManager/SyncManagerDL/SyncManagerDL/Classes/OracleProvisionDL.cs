using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Classes
{
    public class OracleProvisionDL : OracleServerDBManager, IProvisionDL
    {
        public OracleProvisionDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<IProvision>> GetProvisionDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IProvision> provisions = await ExecuteQueryAsync<IProvision>(sql.ToString(), parameters);
                return provisions;
            }
            catch
            {
                throw;
            }
        }
    }
}
