using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.Syncing.Model;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.DL.Interfaces
{
    public interface IMobileDataSyncDL
    {
        public Task<List<ITableGroupEntityView>> GetTablesToSync(string groupName, string tableName);
        public Task<List<T>> GetDataFromDatabase<T>(string sqlQuery, Dictionary<string, object?>? parameters);
        Task<List<ITableGroup>> GetTableGroupToSync(string groupName);
        Task<int> UpsertTableAsync(string tableName, List<dynamic> list, DateTime lastDownloadTime, string insertQuery, string updateQuery);
        Task UpdateLastDownloadTimeForTableGroup(string groupName, DateTime lastDownloadTime);
        Task UpdateLastUploadTimeForTableGroup(string groupName, DateTime lastUploadTime);
        Task<int> ExecuteQuery(string sqlQuery, List<object>? data, string sqliteFilePath);
        Task<List<SalesOrderViewModelDCO>?> PrepareInsertUpdateData_SalesOrder();
        Task<List<MerchandiserDTO>?> PrepareInsertUpdateData_Merchandiser();
        Task<List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>?> PrepareInsertUpdateData_WHStockRequest();
        Task<List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>?> PrepareInsertUpdateData_Return();
        Task<List<Winit.Modules.CollectionModule.Model.Classes.CollectionDTO>?> PrepareInsertUpdateData_Collection();
        Task<List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>?> PrepareInsertUpdateData_CollectionDeposit();
        Task<List<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>?> PrepareInsertUpdateData_Master();
        Task UpdateSSForUIDs(Dictionary<string, List<string>> requestUIDDictionary, int ss = 0);

        Task<List<Winit.Modules.Address.Model.Classes.Address>?> PrepareInsertUpdateData_Address();
        Task<List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckMaster>?> PrepareInsertUpdateData_StoreCheck();
        Task<List<Winit.Modules.FileSys.Model.Classes.FileSys>?> PrepareInsertUpdateData_FileSys();
        Task<int> CreateDynamicTable(bool action, string empCode, string tableName);
    }
}
