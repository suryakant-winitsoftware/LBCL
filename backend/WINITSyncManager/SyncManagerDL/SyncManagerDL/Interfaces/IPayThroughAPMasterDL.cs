namespace SyncManagerDL.Interfaces
{
    public interface IPayThroughAPMasterDL
    {
        Task<List<SyncManagerModel.Interfaces.IPayThroughAPMaster>> GetPayThroughAPMasterDetails(string sql);
    }
}
