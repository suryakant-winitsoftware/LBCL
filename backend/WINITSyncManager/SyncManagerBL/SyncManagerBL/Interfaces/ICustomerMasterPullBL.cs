using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ICustomerMasterPullBL
    {
        Task<List<SyncManagerModel.Interfaces.ICustomerMasterPull>> GetCustomerMasterPullDetails(string sql);
        Task<int> InsertCustomerMasterPullDataIntoMonthTable(List<SyncManagerModel.Interfaces.ICustomerMasterPull> customers, IEntityDetails entityDetails);

    }
}
