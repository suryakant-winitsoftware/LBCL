using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IPriceLadderingBL
    {
        Task<List<SyncManagerModel.Interfaces.IPriceLaddering>> GetPriceLadderingDetails(string sql);
        Task<int> InsertPriceLadderingDataIntoMonthTable(List<SyncManagerModel.Interfaces.IPriceLaddering> priceLaddering, IEntityDetails entityDetails);
    }
}
