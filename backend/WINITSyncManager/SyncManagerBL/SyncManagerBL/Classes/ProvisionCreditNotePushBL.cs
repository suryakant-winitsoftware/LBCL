using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class ProvisionCreditNotePushBL : IProvisionCreditNotePushBL
    {
        private readonly IProvisionCreditNotePushDL _mssqlCreditNoteProvisionPushDL;
        private readonly IProvisionCreditNotePushDL _oracleCreditNoteProvisionPushDL;
        public ProvisionCreditNotePushBL(Func<string, IProvisionCreditNotePushDL> creditNoteProvisionPushDL)
        {
            _mssqlCreditNoteProvisionPushDL = creditNoteProvisionPushDL(Winit.Shared.Models.Constants.ConnectionStringName.SqlServer);
            _oracleCreditNoteProvisionPushDL = creditNoteProvisionPushDL(Winit.Shared.Models.Constants.ConnectionStringName.OracleServer);
        }

        public async Task<List<IProvisionCreditNote>> GetCreditNoteProvisionDetails(IEntityDetails entityDetails)
        {
            return await _mssqlCreditNoteProvisionPushDL.GetCreditNoteProvisionDetails(entityDetails);
        }

        public async Task<int> InsertCreditNoteProvisionsIntoOracleStaging(List<IProvisionCreditNote> creditNoteProvisions)
        {
           return await _oracleCreditNoteProvisionPushDL.InsertCreditNoteProvisionsIntoOracleStaging(creditNoteProvisions);
        }
    }
}
