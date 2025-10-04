using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomerMasterDetailsDL
    {
        Task<List<ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails);
        Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers);
        
    }
}
