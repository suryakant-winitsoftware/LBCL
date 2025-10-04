using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ICustomerCreditLimitStagingDL
    {
         Task<int> InsertCreditLimitDataIntoMonthTable(List<SyncManagerModel.Interfaces.ICustomerCreditLimit> creditLimits, IEntityDetails entityDetails);
    }
}
