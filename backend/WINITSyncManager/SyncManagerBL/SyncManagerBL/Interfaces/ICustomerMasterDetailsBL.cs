using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ICustomerMasterDetailsBL
    {
        Task<List<ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails);
        Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers);
    }
}
