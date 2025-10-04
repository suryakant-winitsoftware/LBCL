using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Interfaces
{
    public interface IStatementAndBalanceConfiBL
    {
        Task<List<SyncManagerModel.Interfaces.IStatementAndBalanceConfi>> GetStatementAndBalanceConfiDetails(string sql);
        Task<int> InsertStatementAndBalanceConfiIntoMonthTable(List<IStatementAndBalanceConfi> statementAndBalances, IEntityDetails entityDetails);
    }
}
