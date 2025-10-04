using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IProvisionCreditNotePullDL
    {
        Task<List<IProvisionCreditNote>> PullProvisionCreditNoteDetailsFromOracle(string sql)
        {
            return Task.FromResult(new List<IProvisionCreditNote>());
        }
        Task<int> InsertProvisionCreditNoteDetailsIntoMonthTable(List<IProvisionCreditNote> provisionCreditNotes, IEntityDetails entityDetails)
        {
            return Task.FromResult(0);
        }
    }
}
