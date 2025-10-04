using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface ITemporaryCreditLimitBL
    {
        Task<List<ITemporaryCreditLimit>> GetTemporaryCreditLimitPushDetails(IEntityDetails entityDetails);
        Task<int> InsertTemporaryCreditLimitsIntoOracleStaging(List<ITemporaryCreditLimit> temporaryCreditLimits);
        Task<List<ITemporaryCreditLimit>> PullTemporaryCreditLimitDetailsFromOracle(string sql);
        Task<int> InsertTemporaryCreditLimitDetailsIntoMonthTable(List<ITemporaryCreditLimit> temporaryCreditLimits, IEntityDetails entityDetails);
    }
}
