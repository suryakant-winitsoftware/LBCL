using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IProvisionCreditNotePushBL
    {
        Task<List<IProvisionCreditNote>> GetCreditNoteProvisionDetails(IEntityDetails entityDetails);
        Task<int> InsertCreditNoteProvisionsIntoOracleStaging(List<IProvisionCreditNote> creditNoteProvisions);
    }
}
