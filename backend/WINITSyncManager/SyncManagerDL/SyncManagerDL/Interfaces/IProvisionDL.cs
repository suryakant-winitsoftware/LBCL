using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IProvisionDL
    {
        Task<List<SyncManagerModel.Interfaces.IProvision>> GetProvisionDetails(string sql)
        {
            return Task.FromResult(new List<SyncManagerModel.Interfaces.IProvision>());
        }
        Task<int> InsertProvisionDataIntoMonthTable(List<SyncManagerModel.Interfaces.IProvision> provisions, IEntityDetails entityDetails)
        {
            return Task.FromResult(0);
        }

    }
}
