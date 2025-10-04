using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncManagerDL.Base.DBManager;
using Winit.Modules.Int_InsertDataInQueue.DL.Interfaces;
using Winit.Modules.Int_InsertDataInQueue.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Int_InsertDataInQueue.DL.Classes
{
    public class MSSQLInsertDataInQueueDL : SqlServerDBManager, IInsertDataInQueueDL
    {
        public MSSQLInsertDataInQueueDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> InsertDataInQueue(IApiRequest Request)
        {
            try
            {
                var sql = new StringBuilder($@"insert into  {Int_DbTableName.IntegrationQueue + Int_DbTableName.MonthTableSuffix} (UID,entity_name,Content,Status) 
                              values (@UID,@EntityName,@JsonData,@Status)");

                return await ExecuteNonQueryAsync(sql.ToString(), Request);
            }
            catch { throw; }

        }
    }
}
