using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class CustomerMasterPullBL : ICustomerMasterPullBL
    {
        private readonly ICustomerMasterPullDL _customerMasterPull;
        private readonly ICustomerMasterPullStagingDL _customerMasterPullStaging;
        public CustomerMasterPullBL(ICustomerMasterPullDL customerMasterPull,ICustomerMasterPullStagingDL customerMasterPullStaging)
        {
            _customerMasterPull = customerMasterPull;
            _customerMasterPullStaging=customerMasterPullStaging;
        }

        public async Task<List<ICustomerMasterPull>> GetCustomerMasterPullDetails(string sql)
        {
             return await _customerMasterPull.GetCustomerMasterPullDetails(sql);
        }

        public async Task<int> InsertCustomerMasterPullDataIntoMonthTable(List<ICustomerMasterPull> customers, IEntityDetails entityDetails)
        {
             return await _customerMasterPullStaging.InsertCustomerMasterPullDataIntoMonthTable(customers, entityDetails);
        }
    }
}
