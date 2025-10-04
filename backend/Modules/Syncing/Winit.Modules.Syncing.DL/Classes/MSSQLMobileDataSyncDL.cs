using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.Syncing.Model;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Syncing.DL.Classes
{
    public class MSSQLMobileDataSyncDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IMobileDataSyncDL
    {
        public MSSQLMobileDataSyncDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<List<ITableGroupEntityView>> GetTablesToSync(string groupName, string tableName)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"GroupName" , groupName},
                {"TableName" , tableName}
            };

            string groupNameQuery = "";
            string tableNameQuery = "";

            if (!string.IsNullOrEmpty(groupName))
            {
                groupNameQuery = " AND TG.group_name = @GroupName";
            }
            if (!string.IsNullOrEmpty(tableName))
            {
                tableNameQuery = " AND TGE.table_name = @TableName";
            }

            var sql = string.Format(@"SELECT TG.group_name AS GroupName, TGE.table_name AS TableName, TGE.serial_no AS SerialNo,
                TGE.masterdata_query AS MasterDataQuery, TGE.syncdata_query AS SyncDataQuery, 
                TGE.sqlite_insert_query AS SqliteInsertQuery, TGE.sqlite_insert_parameter AS SqliteInsertParameter,
                TGE.model_name AS ModelName, TGE.sqlite_update_query AS SqliteUpdateQuery, TGE.last_uploaded_time AS LastUploadedTime,
                TGE.last_downloaded_time AS LastDownloadedTime
                From table_group TG
                INNER JOIN table_group_entity TGE ON TGE.table_group_uid = TG.uid 
                AND TG.is_active = true AND TGE.is_active = true and TGE.has_download = true
                {0} 
                {1} 
                ORDER BY TG.serial_no, TGE.serial_no;", groupNameQuery, tableNameQuery);

            return await ExecuteQueryAsync<ITableGroupEntityView>(sql, parameters);
        }
        public async Task<List<T>> GetDataFromDatabase<T>(string sqlQuery, Dictionary<string, object?>? parameters)
        {
            return await ExecuteQueryAsync<T>(sqlQuery, parameters);
        }
        public async Task<List<ITableGroup>> GetTableGroupToSync(string groupName)
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"GroupName" , groupName}
            };

            string groupNameQuery = "";

            if (!string.IsNullOrEmpty(groupName))
            {
                groupNameQuery = " AND TG.name = @GroupName";
            }

            var sql = $@"SELECT TG.id AS Id, TG.uid AS UID, TG.group_name AS GroupName, TG.serial_no AS SerialNo, TG.last_upload_time AS LastUploadTime,
                TG.last_download_time AS LastDownloadTime
                FROM table_group TG
                WHERE TG.is_active = true {groupNameQuery}
                ORDER BY TG.serial_no;";

            return await ExecuteQueryAsync<ITableGroup>(sql, parameters);
        }
        public Task<int> UpsertTableAsync(string tableName, List<dynamic> list, DateTime lastDownloadTime, string insertQuery, string updateQuery)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLastDownloadTimeForTableGroup(string groupName, DateTime lastDownloadTime)
        {
            throw new NotImplementedException();
        }
        public Task UpdateLastUploadTimeForTableGroup(string groupName, DateTime lastUploadTime)
        {
            throw new NotImplementedException();
        }
        public async Task<int> ExecuteQuery(string sqlQuery, List<dynamic>? data, string sqliteFilePath)
        {
            int count = 0;
            try
            {
                using (var connection = SqliteConnection(GetSqliteConnectionString(sqliteFilePath)))
                {
                    await connection.OpenAsync();

                    count = await ExecuteNonQueryAsync(sqlQuery, connection, null, data);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return count;
        }
        #region Common
        public Task UpdateSSForUIDs(Dictionary<string, List<string>> requestUIDDictionary, int ss = 0)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region SalesOrder
        public Task<List<SalesOrderViewModelDCO>?> PrepareInsertUpdateData_SalesOrder()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region WHStockRequest
        public Task<List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>?> PrepareInsertUpdateData_WHStockRequest()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region ReturnOrder
        public Task<List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>?> PrepareInsertUpdateData_Return()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Collection
        public Task<List<Winit.Modules.CollectionModule.Model.Classes.CollectionDTO>?> PrepareInsertUpdateData_Collection()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region CollectionDeposit
        public Task<List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>?> PrepareInsertUpdateData_CollectionDeposit()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region Master
        public Task<List<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>?> PrepareInsertUpdateData_Master()
        {
            throw new NotImplementedException();
        }
        #endregion

        public async Task<List<Winit.Modules.FileSys.Model.Classes.FileSys>?> PrepareInsertUpdateData_FileSys()
        {
            throw new NotImplementedException();
        }

        public Task<List<Address.Model.Classes.Address>?> PrepareInsertUpdateData_Address()
        {
            throw new NotImplementedException();
        }
        public Task<List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckMaster>?> PrepareInsertUpdateData_StoreCheck()
        {
            throw new NotImplementedException();
        }

        public Task<List<MerchandiserDTO>?> PrepareInsertUpdateData_Merchandiser()
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateDynamicTable(bool action, string empCode, string tableName)
        {
            throw new NotImplementedException();
        }
    }
}

