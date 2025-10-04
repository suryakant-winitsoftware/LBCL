using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Classes
{
    public class OracleCustomer_RefDL : OracleServerDBManager, ICustomer_RefDL
    {
        public OracleCustomer_RefDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<ICustomer_Ref>> GetCustomerReferenceDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<ICustomer_Ref> customer_Refs = await ExecuteQueryAsync<ICustomer_Ref>(sql.ToString(), parameters);
                return customer_Refs;
            }
            catch
            {
                throw;
            }
        }
    }
}
