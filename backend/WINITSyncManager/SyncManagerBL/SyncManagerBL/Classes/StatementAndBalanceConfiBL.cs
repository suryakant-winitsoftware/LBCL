using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class StatementAndBalanceConfiBL : IStatementAndBalanceConfiBL
    {
        private readonly IStatementAndBalanceConfiDL _statementAndBalanceConfDL;
        private readonly IStatementAndBalanceConfiStagingDL _statementAndBalanceConfiStaging;
        public StatementAndBalanceConfiBL (IStatementAndBalanceConfiDL statementAndBalanceConfiDL, 
            IStatementAndBalanceConfiStagingDL statementAndBalanceConfiStaging)
        {
            _statementAndBalanceConfDL = statementAndBalanceConfiDL;
            _statementAndBalanceConfiStaging = statementAndBalanceConfiStaging;
        }
        public async Task<List<IStatementAndBalanceConfi>> GetStatementAndBalanceConfiDetails(string sql)
        {
          return await _statementAndBalanceConfDL.GetStatementAndBalanceConfiDetails(sql);
        }

        public async Task<int> InsertStatementAndBalanceConfiIntoMonthTable(List<IStatementAndBalanceConfi> statementAndBalances, IEntityDetails entityDetails)
        {
             return await _statementAndBalanceConfiStaging.InsertStatementAndBalanceConfiIntoMonthTable(statementAndBalances, entityDetails);
        }
    }
} 
