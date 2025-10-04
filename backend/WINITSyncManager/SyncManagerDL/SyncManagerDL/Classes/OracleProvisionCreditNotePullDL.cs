using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerDL.Classes
{
    public class OracleProvisionCreditNotePullDL : OracleServerDBManager, IProvisionCreditNotePullDL
    {
        public OracleProvisionCreditNotePullDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
       public async Task<List<IProvisionCreditNote>> PullProvisionCreditNoteDetailsFromOracle(string sql)
        {
            try
            {
                List<IProvisionCreditNote> provisionCreditNotes = await ExecuteQueryAsync<IProvisionCreditNote>(sql.ToString(), null);
                return provisionCreditNotes;
            }
            catch
            {
                throw;
            }
        }
    }
}
