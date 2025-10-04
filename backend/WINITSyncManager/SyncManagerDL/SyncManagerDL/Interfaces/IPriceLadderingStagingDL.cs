using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IPriceLadderingStagingDL
    {
        Task<int> InsertPriceLadderingDataIntoMonthTable(List<IPriceLaddering> priceLaddering, IEntityDetails entityDetails);
    }
}
