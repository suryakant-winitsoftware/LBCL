using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IWarehouseStockBL
    {
        Task<List<SyncManagerModel.Interfaces.IWarehouseStock>> GetWarehouseStockDetails(string sql);
        Task<int> InsertWarehouseStockDataIntoMonthTable(List<SyncManagerModel.Interfaces.IWarehouseStock> warehouseStocks, IEntityDetails entityDetails);

    }
}
