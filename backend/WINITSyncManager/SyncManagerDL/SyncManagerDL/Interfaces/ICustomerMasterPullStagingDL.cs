using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomerMasterPullStagingDL
    {
        Task<int> InsertCustomerMasterPullDataIntoMonthTable(List<SyncManagerModel.Interfaces.ICustomerMasterPull> customers, IEntityDetails entityDetails);

    }
}
