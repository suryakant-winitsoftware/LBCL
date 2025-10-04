using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IProvisionBL
    {
        Task<List<SyncManagerModel.Interfaces.IProvision>> GetProvisionDetails(string sql);
        Task<int> InsertProvisionDataIntoMonthTable(List<SyncManagerModel.Interfaces.IProvision> provisions, IEntityDetails entityDetails);
    }
}
