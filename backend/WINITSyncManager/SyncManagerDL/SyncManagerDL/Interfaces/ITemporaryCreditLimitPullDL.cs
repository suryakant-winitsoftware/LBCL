using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ITemporaryCreditLimitPullDL
    {
        Task<List<ITemporaryCreditLimit>> PullTemporaryCreditLimitDetailsFromOracle(string sql)
        {
            return Task.FromResult(new List<ITemporaryCreditLimit>());
        }
        Task<int> InsertTemporaryCreditLimitDetailsIntoMonthTable(List<ITemporaryCreditLimit> temporaryCreditLimits, IEntityDetails entityDetails)
        {
            return Task.FromResult(0);
        }
    }
}
