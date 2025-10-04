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
    public class OracleItemMasterDL : SyncManagerDL.Base.DBManager.OracleServerDBManager, IItemMasterDL
    {
        public OracleItemMasterDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<List<IItemMaster>> GetItemMasterDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IItemMaster> itemMaster = await ExecuteQueryAsync<IItemMaster>(sql.ToString(), parameters);
                return itemMaster;
            }
            catch
            {
                throw;
            }
        }
    }
}
