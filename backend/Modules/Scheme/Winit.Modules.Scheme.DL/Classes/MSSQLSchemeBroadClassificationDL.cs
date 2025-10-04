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
    public class MSSQLSchemeBroadClassificationDL : SqlServerDBManager, ISchemeBroadClassificationDL
    {
        public MSSQLSchemeBroadClassificationDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }


        public async Task<List<ISchemeBroadClassification>> GetSchemeBroadClassificationByLinkedItemUID(string linkedItemUID)
        {
            var param = new Dictionary<string, object>
            {
                { "LinkedItemUID",linkedItemUID}
            };
            var sql = @"select * from scheme_broad_classification where linked_item_uid=@LinkedItemUID ";
            List<ISchemeBroadClassification> standingProvisionBranches = await ExecuteQueryAsync<ISchemeBroadClassification>(sql.ToString(), param);
            return standingProvisionBranches.ToList();
        }
        public async Task<int> CreateSchemeBroadClassifications(List<ISchemeBroadClassification> schemeBroadClassifications)
        {

            var sql = @"INSERT INTO scheme_broad_classification ( uid, created_by, created_time, modified_by, 
                modified_time, server_add_time, server_modified_time, ss, linked_item_type, linked_item_uid, 
                broad_classification_code) 
                VALUES (@Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, 
                @ServerModifiedTime, @Ss, @LinkedItemType, @LinkedItemUid, @BroadClassificationCode);
";
            return await ExecuteNonQueryAsync(sql.ToString(), schemeBroadClassifications);

        }
        public async Task<int> DeleteSchemeBroadClassifications(List<string> uids)
        {
            var param = new Dictionary<string, object>
     {
         { "Uid",uids}
     };
            var sql = @"delete from scheme_broad_classification where uid in @Uid";

            return await ExecuteNonQueryAsync(sql.ToString(), param);
        }
    }
}
