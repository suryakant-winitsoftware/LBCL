using SyncManagerModel.Interfaces;

namespace SyncManagerDL.Interfaces
{
    public interface IStatementAndBalanceConfiStagingDL
    {
        Task<int> InsertStatementAndBalanceConfiIntoMonthTable(List<IStatementAndBalanceConfi> statementAndBalances, IEntityDetails entityDetails);
    }
}
