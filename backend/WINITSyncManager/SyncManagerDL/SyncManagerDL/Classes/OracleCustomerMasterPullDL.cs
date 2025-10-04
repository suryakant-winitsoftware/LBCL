using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class OracleCustomerMasterPullDL : OracleServerDBManager, ICustomerMasterPullDL
    {
        public OracleCustomerMasterPullDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<ICustomerMasterPull>> GetCustomerMasterPullDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<ICustomerMasterPull> customers = await ExecuteQueryAsync<ICustomerMasterPull>(sql.ToString(), parameters);
                return customers;
            }
            catch
            {
                throw;
            }
        }
    }
}
