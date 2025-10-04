using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class ItemMasterBL : IItemMasterBL
    {
        private readonly IItemMasterDL _ItemMasterDL;
        private readonly IItemMasterStagingDL _ItemMasterStagingDL;
        public ItemMasterBL(IItemMasterDL itemMasterDL,IItemMasterStagingDL itemMasterStaging)
        {
            _ItemMasterDL = itemMasterDL;
            _ItemMasterStagingDL = itemMasterStaging;
        } 
        

        public async Task<List<IItemMaster>> GetItemMasterDetails(string sql)
        {
            return await _ItemMasterDL.GetItemMasterDetails(sql);
        }

        public  async Task<int> InsertItemDataIntoMonthTable(List<IItemMaster> itemMaster, IEntityDetails entityDetails)
        {
             return await _ItemMasterStagingDL.InsertItemDataIntoMonthTable(itemMaster, entityDetails);
        }
    }
}
