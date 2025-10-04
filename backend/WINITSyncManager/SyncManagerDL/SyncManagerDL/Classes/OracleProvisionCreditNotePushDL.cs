using Microsoft.Extensions.Configuration;
using SyncManagerDL.Base.DBManager;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Constants;

namespace SyncManagerDL.Classes
{
    public class OracleProvisionCreditNotePushDL : OracleServerDBManager, IProvisionCreditNotePushDL
    {
        public OracleProvisionCreditNotePushDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<int> InsertCreditNoteProvisionsIntoOracleStaging(List<IProvisionCreditNote> creditNoteProvisions)
        {
            try
            {
                var sql = new StringBuilder($@"insert into  {Int_OracleTableNames.ProvisionCreditNotes} ( inserted_on, provision_id, dms_release_requested) 
                  VALUES ( TO_DATE(:InsertedOn, 'DD-MON-YYYY HH24:MI:SS'), :ProvisionId, :DmsReleaseRequested)"
                );

                return await ExecuteNonQueryAsync(sql.ToString(), creditNoteProvisions);

            }
            catch { throw; }
        }
    }
}
