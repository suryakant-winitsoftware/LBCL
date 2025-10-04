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
    public class OracleWarehouseStockDL : OracleServerDBManager, IWarehouseStockDL
    {
        public OracleWarehouseStockDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IWarehouseStock>> GetWarehouseStockDetails(string sql)
        {
            try
            {
                var parameters = new Dictionary<string, object?>();
                List<IWarehouseStock> warehouseStocks = await ExecuteQueryAsync<IWarehouseStock>(sql.ToString(), parameters);
                return warehouseStocks;
            }
            catch
            {
                throw;
            }
        }
    }
}
