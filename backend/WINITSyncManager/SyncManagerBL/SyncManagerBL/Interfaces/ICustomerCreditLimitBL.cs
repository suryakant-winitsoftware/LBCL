using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ICustomerCreditLimitBL
    {
        Task<List<SyncManagerModel.Interfaces.ICustomerCreditLimit>> GetCustomerCreditLimitDetails(string sql);
        Task<int> InsertCreditLimitDataIntoMonthTable(List<SyncManagerModel.Interfaces.ICustomerCreditLimit> creditLimits, IEntityDetails entityDetails);
    }
}
