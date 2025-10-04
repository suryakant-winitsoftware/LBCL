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
    public class MSSQLEntityScriptDL : SqlServerDBManager, IEntityScriptDL
    {
        public MSSQLEntityScriptDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<IEntityScript> GetEntityScriptDetailsByEntity(string Entity)
        {
            try
            {
                var sql = new StringBuilder($@"select [name] as EntityName ,entity_group_id as EntityGroup,SelectQuery,InsertQuery,MaxCount  from int_entity where [name]=@Entity ");
                var parameters = new Dictionary<string, object?>()
                {
                    {"Entity",Entity }
                };
                IEntityScript? itemMaster = await ExecuteSingleAsync<IEntityScript>(sql.ToString(), parameters);
                return itemMaster ==null  ?  new SyncManagerModel.Classes.EntityScript(): itemMaster;
            }
            catch
            {
                throw;
            }
        }
    }
}
