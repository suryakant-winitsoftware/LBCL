using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface ITemporaryCreditLimitDL
    {
        Task<List<SyncManagerModel.Interfaces.ITemporaryCreditLimit>> GetTemporaryCreditLimitDetails(IEntityDetails entityDetails)
        {
            return Task.FromResult(new List<SyncManagerModel.Interfaces.ITemporaryCreditLimit>());
        }
        Task<int> InsertCustomerdetailsIntoOracleStaging(List<ITemporaryCreditLimit> temporaryCreditLimits)
        {
            return Task.FromResult(0);
        }
        
    }
}

