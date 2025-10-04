using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.Scheme.DL.Interfaces;
using Winit.Modules.Scheme.Model.Constants;
using Winit.Modules.Scheme.Model.Interfaces;


using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Scheme.DL.Classes
{
    public class PGSQLSchemeBranchDL : PostgresDBManager, ISchemeBranchDL
    {
        public PGSQLSchemeBranchDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }


        public async Task<List<ISchemeBranch>> GetSchemeBranchesByLinkedItemUID(string linkedItemUID)
        {
            var param = new Dictionary<string, object>
            {
                { "LinkedItemUID",linkedItemUID}
            };
            var sql = @"select * from scheme_branch where linked_item_uid=@LinkedItemUID ";
            List<ISchemeBranch> standingProvisionBranches = await ExecuteQueryAsync<ISchemeBranch>(sql.ToString(), param);
            return standingProvisionBranches;
        }
        public async Task<int> CreateSchemeBranches(List<ISchemeBranch> schemeBranches)
        {

            var sql = @"INSERT INTO scheme_branch ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, linked_item_type, linked_item_uid, branch_code) 
                    VALUES ( @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @LinkedItemType, @LinkedItemUid, @BranchCode);";
            return await ExecuteNonQueryAsync(sql.ToString(), schemeBranches);
        }
        public async Task<int> DeleteSchemeBranches(List<string> uids)
        {
            var param = new Dictionary<string, object>
            {
                { "Uid",uids}
            };
            var sql = @"delete from scheme_branch where uid in @Uid";

            return await ExecuteNonQueryAsync(sql.ToString(), param);
        }
    }
}
