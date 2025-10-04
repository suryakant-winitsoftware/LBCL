using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Classes
{
    public class OracleCustomerCreditLimitDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, ICustomerCreditLimitDL
    {
        public OracleCustomerCreditLimitDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<ICustomerCreditLimit>> GetCustomerCreditLimitDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<ICustomerCreditLimit> customerCreditLimit = await ExecuteQueryAsync<ICustomerCreditLimit>(sql.ToString(), parameters);
                return customerCreditLimit;
            }
            catch
            {
                throw;
            }
        }
    }
}
