using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class CustomerMasterDetailsBL : ICustomerMasterDetailsBL
    {
        private readonly ICustomerMasterDetailsDL _mssqlCustomerMasterDL;
        private readonly ICustomerMasterDetailsDL _oracleCustomerMasterDL;
        private readonly Iint_CommonMethodsBL _commonMethodsBL;
        public CustomerMasterDetailsBL(Func<string, ICustomerMasterDetailsDL> customerMaster, Iint_CommonMethodsBL iint_CommonMethodsBL)
        {
            _mssqlCustomerMasterDL = customerMaster(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            _oracleCustomerMasterDL = customerMaster(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
            _commonMethodsBL = iint_CommonMethodsBL;
        }
        public async Task<List<ICustomerMasterDetails>> GetCustomerMasterPushDetails(IEntityDetails entityDetails)
        {
            return await _mssqlCustomerMasterDL.GetCustomerMasterPushDetails(entityDetails);
        }
        public async Task<int> InsertCustomerdetailsIntoOracleStaging(List<ICustomerMasterDetails> customers)
        {
            return await _oracleCustomerMasterDL.InsertCustomerdetailsIntoOracleStaging(customers);
        }

    }
}
