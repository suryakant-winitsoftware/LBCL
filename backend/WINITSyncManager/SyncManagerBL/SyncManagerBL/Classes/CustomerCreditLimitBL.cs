using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class CustomerCreditLimitBL : ICustomerCreditLimitBL
    {
        private readonly ICustomerCreditLimitDL _customerCreditLimitDL;
        private readonly ICustomerCreditLimitStagingDL _customerCreditLimitStagingDL;
        public CustomerCreditLimitBL(ICustomerCreditLimitDL customerCreditLimitDL, ICustomerCreditLimitStagingDL customerCreditLimitStagingDL)
        {
            _customerCreditLimitDL = customerCreditLimitDL;
            _customerCreditLimitStagingDL = customerCreditLimitStagingDL;
        }

        public async Task<List<ICustomerCreditLimit>> GetCustomerCreditLimitDetails(string sql)
        {
            return await _customerCreditLimitDL.GetCustomerCreditLimitDetails(sql);
        }

        public async Task<int> InsertCreditLimitDataIntoMonthTable(List<ICustomerCreditLimit> creditLimits, IEntityDetails entityDetails)
        {
            return await _customerCreditLimitStagingDL.InsertCreditLimitDataIntoMonthTable(creditLimits, entityDetails);
        }
    }
}
