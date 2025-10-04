using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IPayThroughAPMasterBL
    {
        Task<int> InsertPayThroughAPMasterDataIntoMonthTable(List<SyncManagerModel.Interfaces.IPayThroughAPMaster> payThroughAPMasters, IEntityDetails entityDetails);
        Task<List<SyncManagerModel.Interfaces.IPayThroughAPMaster>> GetPayThroughAPMasterDetails(string sql);
    }
}
