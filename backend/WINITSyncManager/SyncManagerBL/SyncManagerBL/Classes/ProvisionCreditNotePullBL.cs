using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class ProvisionCreditNotePullBL : IProvisionCreditNotePullBL
    {
        private readonly IProvisionCreditNotePullDL _mssqlProvisionCreditNotePull;
        private readonly IProvisionCreditNotePullDL _oracleProvisionCreditNotePull;
        public ProvisionCreditNotePullBL(Func<string, IProvisionCreditNotePullDL> provisionCreditNotePull)
        {
            this._mssqlProvisionCreditNotePull = provisionCreditNotePull(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            this._oracleProvisionCreditNotePull = provisionCreditNotePull(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
        }

        public async Task<int> InsertProvisionCreditNoteDetailsIntoMonthTable(List<IProvisionCreditNote> provisionCreditNotes, IEntityDetails entityDetails)
        {
            return await _mssqlProvisionCreditNotePull.InsertProvisionCreditNoteDetailsIntoMonthTable(provisionCreditNotes, entityDetails);
        }

        public async Task<List<IProvisionCreditNote>> PullProvisionCreditNoteDetailsFromOracle(string sql)
        {
            return await _oracleProvisionCreditNotePull.PullProvisionCreditNoteDetailsFromOracle(sql);
        }
    }
}
