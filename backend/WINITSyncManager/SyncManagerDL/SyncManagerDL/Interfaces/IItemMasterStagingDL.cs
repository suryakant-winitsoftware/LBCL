using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IItemMasterStagingDL
    {
        Task<int> InsertItemDataIntoMonthTable(List<IItemMaster> itemMaster, IEntityDetails entityDetails);
    }
}
