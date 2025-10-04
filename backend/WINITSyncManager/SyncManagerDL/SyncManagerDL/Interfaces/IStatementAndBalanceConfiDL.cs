using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Interfaces
{
    public interface IStatementAndBalanceConfiDL
    {
        Task<List<SyncManagerModel.Interfaces.IStatementAndBalanceConfi>> GetStatementAndBalanceConfiDetails(string sql);
    }
}
