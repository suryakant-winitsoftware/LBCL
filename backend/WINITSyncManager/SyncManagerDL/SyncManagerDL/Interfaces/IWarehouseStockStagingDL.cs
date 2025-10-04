using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IWarehouseStockStagingDL
    {
        Task<int> InsertWarehouseStockDataIntoMonthTable(List<SyncManagerModel.Interfaces.IWarehouseStock> warehouseStocks, IEntityDetails entityDetails);
    }
}
