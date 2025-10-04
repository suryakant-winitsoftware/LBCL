using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IProvisionCreditNotePushDL
    {
        Task<List<IProvisionCreditNote>> GetCreditNoteProvisionDetails(IEntityDetails entityDetails)
        {
            return Task.FromResult(new List<IProvisionCreditNote>());
        }
        Task<int> InsertCreditNoteProvisionsIntoOracleStaging(List<IProvisionCreditNote> creditNoteProvisions)
        {
            return Task.FromResult(0);
        }
    }
}
