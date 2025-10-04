using Microsoft.Extensions.Configuration;
using System.Text;
using SyncManagerDL.Base.DBManager;
using Winit.Shared.Models.Constants;
using SyncManagerModel.Interfaces;
using SyncManagerDL.DL.Interfaces;

namespace SyncManagerDL.Classes
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
