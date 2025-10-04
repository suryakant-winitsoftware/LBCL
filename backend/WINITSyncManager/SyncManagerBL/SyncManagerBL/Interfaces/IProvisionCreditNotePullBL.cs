using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IProvisionCreditNotePullBL
    {
        Task<List<IProvisionCreditNote>> PullProvisionCreditNoteDetailsFromOracle(string sql);
        Task<int> InsertProvisionCreditNoteDetailsIntoMonthTable(List<IProvisionCreditNote> provisionCreditNotes, IEntityDetails entityDetails);
    }
}
