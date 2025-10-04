using Microsoft.Extensions.Configuration;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncManagerDL.Base.DBManager;

namespace SyncManagerDL.Classes
{
    public class OracleStatementAndBalanceConfiDL : OracleServerDBManager, IStatementAndBalanceConfiDL
    {
        public OracleStatementAndBalanceConfiDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<List<IStatementAndBalanceConfi>> GetStatementAndBalanceConfiDetails(string sql)
        {
            var parameters = new Dictionary<string, object?>();
            List<IStatementAndBalanceConfi> statementAndBalances = await ExecuteQueryAsync<IStatementAndBalanceConfi>(sql.ToString(), parameters);
            return statementAndBalances;
        }
    }
}
