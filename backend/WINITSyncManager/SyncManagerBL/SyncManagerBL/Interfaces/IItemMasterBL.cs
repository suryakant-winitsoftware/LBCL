using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IItemMasterBL
    { 
         Task<List<SyncManagerModel.Interfaces.IItemMaster>> GetItemMasterDetails(string sql);
        Task<int> InsertItemDataIntoMonthTable(List<SyncManagerModel.Interfaces.IItemMaster> itemMaster, IEntityDetails entityDetails);
    }
}
