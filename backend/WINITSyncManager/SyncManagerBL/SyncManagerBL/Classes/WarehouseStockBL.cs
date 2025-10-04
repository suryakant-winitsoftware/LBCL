using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class WarehouseStockBL:IWarehouseStockBL
    {
        private readonly IWarehouseStockDL _warehouseStockDL;
        private readonly IWarehouseStockStagingDL _warehouseStockStagingDL;
        public WarehouseStockBL(IWarehouseStockDL warehouseStockDL, IWarehouseStockStagingDL warehouseStockStagingDL)
        {
            _warehouseStockDL = warehouseStockDL;
            _warehouseStockStagingDL = warehouseStockStagingDL;
        }

        public async Task<List<IWarehouseStock>> GetWarehouseStockDetails(string sql)
        {
           return await _warehouseStockDL.GetWarehouseStockDetails(sql);
        }

        public async Task<int> InsertWarehouseStockDataIntoMonthTable(List<IWarehouseStock> warehouseStocks, IEntityDetails entityDetails)
        {
            return await _warehouseStockStagingDL.InsertWarehouseStockDataIntoMonthTable(warehouseStocks, entityDetails);
        }
    }
}
