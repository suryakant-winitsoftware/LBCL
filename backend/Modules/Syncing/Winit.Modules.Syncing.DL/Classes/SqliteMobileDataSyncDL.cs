using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.CaptureCompetitor.Model.Classes;
using Winit.Modules.CaptureCompetitor.Model.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Classes;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.JourneyPlan.Model.Classes;
using Winit.Modules.JourneyPlan.Model.Interfaces;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Merchandiser.Model.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Modules.PO.Model.Classes;
using Winit.Modules.PO.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Classes;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.StockUpdater.Model.Classes;
using Winit.Modules.StockUpdater.Model.Interfaces;
using Winit.Modules.StoreActivity.Model.Classes;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Modules.Syncing.Model;
using Winit.Modules.Syncing.Model.Interfaces;
using Winit.Modules.WHStock.Model.Classes;
using Winit.Modules.WHStock.Model.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Syncing.DL.Classes
{
    public class SqliteMobileDataSyncDL : Base.DL.DBManager.SqliteDBManager, Interfaces.IMobileDataSyncDL
    {
        public SqliteMobileDataSyncDL(IServiceProvider serviceProvider) : base(serviceProvider)
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
                AND TG.is_active = 1 AND TGE.is_active = 1 and TGE.has_download = 1
                {0} 
                {1} 
                ORDER BY TG.serial_no, TGE.serial_no;", groupNameQuery, tableNameQuery);

            return await ExecuteQueryAsync<ITableGroupEntityView>(sql, parameters);
        }
        public async Task<List<T>> GetDataFromDatabase<T>(string sqlQuery, Dictionary<string, object> parameters)
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
                groupNameQuery = " AND TG.group_name = @GroupName";
            }

            var sql = $@"SELECT TG.id AS Id, TG.uid AS UID, TG.group_name AS GroupName, TG.serial_no AS SerialNo, TG.last_upload_time AS LastUploadTime,
                TG.last_download_time AS LastDownloadTime
                FROM table_group TG
                WHERE TG.is_active = 1 {groupNameQuery}
                ORDER BY TG.serial_no;";

            return await ExecuteQueryAsync<ITableGroup>(sql, parameters);
        }
        private async Task<List<string>> RecordExistsAsync(string tableName, List<string> uIDs, IDbConnection connection)
        {
            var query = $"SELECT uid AS UID FROM {tableName} WHERE uid IN @UIDs";
            var parameters = new { UIDs = uIDs };
            List<string> existingUIDs = await ExecuteQueryAsync<string>(query, parameters, null, connection);

            return existingUIDs;
        }

        public async Task<int> UpsertTableAsync(string tableName, List<dynamic> list, DateTime lastDownloadTime, string insertQuery, string updateQuery)
        {
            if (list == null || !list.Any())
            {
                return 0;
            }
            // Add Transaction code here
            int retValue = 0;

            using (var connection = SqliteConnection())
            {
                // Check Existing Records
                List<string> uIDs = list.Select(e => (string)e.UID).ToList();
                List<string> existingUIDs = await RecordExistsAsync(tableName, uIDs, connection);
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert/Update Operations
                        List<dynamic> existingRecords = list.Where(e => existingUIDs.Contains((string)e.UID)).ToList();
                        List<dynamic> newRecords = list.Where(e => !existingUIDs.Contains((string)e.UID)).ToList();
                        // Update
                        if (existingRecords != null && existingRecords.Any())
                        {
                            retValue += await ExecuteNonQueryAsync(updateQuery, existingRecords, connection, transaction);
                        }
                        // Insert 
                        if (newRecords != null && newRecords.Any())
                        {
                            //List<Winit.Modules.SKU.Model.Classes.SKU?> skuList = newRecords
                            //            .Select(jObject => ((JObject)jObject).ToObject<Winit.Modules.SKU.Model.Classes.SKU>())
                            //            .ToList();
                            //List<dynamic> skus = new List<dynamic>();
                            //var data = ((JObject)newRecords[0]).ToObject<Winit.Modules.SKU.Model.Classes.SKU>();
                            //var data1 = ((JObject)newRecords[1]).ToObject<Winit.Modules.SKU.Model.Classes.SKU>();
                            //skus.Add(data);
                            //skus.Add(data1);
                            //retValue += await ExecuteNonQueryAsync(insertQuery, newRecords, connection, transaction);
                            retValue += await ExecuteNonQueryAsync(insertQuery, newRecords, connection, transaction);
                        }
                        await UpdateLastDownloadTimeForTable(tableName, lastDownloadTime, connection, transaction);
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        RollbackTransaction(transaction);
                        throw;
                    }
                }
            }
            return retValue;
        }
        private async Task UpdateLastDownloadTimeForTable(string tableName, DateTime lastDownloadTime, IDbConnection connection, IDbTransaction transaction)
        {
            var query = $"UPDATE table_group_entity SET last_downloaded_time = @LastDownloadedTime WHERE table_name = @TableName";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"TableName",tableName },
                {"LastDownloadedTime",lastDownloadTime }
            };
            await ExecuteNonQueryAsync(query, parameters, connection, transaction);
        }
        public async Task UpdateLastDownloadTimeForTableGroup(string groupName, DateTime lastDownloadTime)
        {
            var query = $"UPDATE table_group SET last_download_time = @LastDownloadTime WHERE group_name = @GroupName";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"GroupName",groupName },
                {"LastDownloadTime",lastDownloadTime }
            };
            await ExecuteNonQueryAsync(query, parameters);
        }
        public async Task UpdateLastUploadTimeForTableGroup(string groupName, DateTime lastUploadTime)
        {
            var query = $"UPDATE table_group SET last_upload_time = @LastUploadTime WHERE group_name = @GroupName";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"GroupName",groupName },
                {"LastUploadTime",lastUploadTime }
            };
            await ExecuteNonQueryAsync(query, parameters);
        }
        public async Task<int> ExecuteQuery(string sqlQuery, List<object>? data, string sqliteFilePath)
        {
            int count = 0;
            try
            {
                using (var connection = SqliteConnection(GetSqliteConnectionString(sqliteFilePath)))
                {
                    await connection.OpenAsync();

                    count = await ExecuteNonQueryAsync(sqlQuery, connection, null, null);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return count;
        }

        #region Common
        private async Task<List<string>> GetUIDsToPushByTable(string tableName, int limit = 1000, IDbConnection? connection = null)
        {
            string limitQuery = string.Empty;
            if (limit > 0)
            {
                limitQuery = string.Format(" LIMIT {0}", limit);
            }
            var query = string.Format($"SELECT uid AS UID FROM {tableName} WHERE ss IN (1,2) ORDER BY modified_time {limitQuery}");
            List<string> uIDs = await ExecuteQueryAsync<string>(query, null, null, connection);
            return uIDs;
        }
        public async Task UpdateSSForUIDs(Dictionary<string, List<string>> requestUIDDictionary, int ss = 0)
        {
            if (requestUIDDictionary == null || requestUIDDictionary.Count == 0)
            {
                return;
            }
            using (IDbConnection connection = SqliteConnection())
            {
                foreach (string tableName in requestUIDDictionary.Keys)
                {
                    await UpdateSSForUIDs(tableName, requestUIDDictionary[tableName], ss, connection);
                }
            }
        }
        private async Task<int> UpdateSSForUIDs(string tableName, List<string> uIDs, int ss = 0, IDbConnection? connection = null)
        {
            var query = string.Format($"UPDATE {tableName} SET ss = {ss} WHERE uid IN @UIDs");
            var parameters = new { UIDs = uIDs };
            return await ExecuteNonQueryAsync(query, parameters, connection);
        }
        #endregion
        #region SalesOrder
        public async Task<List<SalesOrderViewModelDCO>?> PrepareInsertUpdateData_SalesOrder()
        {
            List<SalesOrderViewModelDCO>? salesOrderViewModelDCOList = null;
            List<Winit.Modules.SalesOrder.Model.Classes.SalesOrder>? salesOrderList = null;
            List<Winit.Modules.SalesOrder.Model.Classes.SalesOrderLine>? salesOrderLineList = null;
            List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? wHStockLedgerList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? storeHistoryList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccPayable>? accPayableList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.SalesOrder, 1000);
                if (uIDs == null)
                {
                    return null;
                }
                List<ISalesOrder>? salesOrders = await GetSalesOrderData(uIDs, connection);
                if (salesOrders == null || salesOrders.Count == 0)
                {
                    return null;
                }
                salesOrderList = salesOrders.Cast<Winit.Modules.SalesOrder.Model.Classes.SalesOrder>().ToList();

                List<ISalesOrderLine>? salesOrderLines = await GetSalesOrderLineData(uIDs, connection);
                if (salesOrderLines != null && salesOrderLines.Count > 0)
                {
                    salesOrderLineList = salesOrderLines.Cast<SalesOrderLine>().ToList();
                }

                List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgers = await GetWHStockLedgerDataForSales(uIDs, connection);
                if (wHStockLedgers != null && wHStockLedgers.Count > 0)
                {
                    wHStockLedgerList = wHStockLedgers.Cast<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>().ToList();
                }

                List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>? storeHistories = await GetStoreHistoryDataForSales(uIDs, connection);
                if (storeHistories != null && storeHistories.Count > 0)
                {
                    storeHistoryList = storeHistories.Cast<StoreHistory>().ToList();
                }

                List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>? accPayables = await GetAccPayableDataForSales(uIDs, connection);
                if (accPayables != null && accPayables.Count > 0)
                {
                    accPayableList = accPayables.Cast<AccPayable>().ToList();
                }
            }

            salesOrderViewModelDCOList = new List<SalesOrderViewModelDCO>();
            foreach (Winit.Modules.SalesOrder.Model.Classes.SalesOrder salesOrder in salesOrderList)
            {
                SalesOrderViewModelDCO salesOrderViewModelDCO = new SalesOrderViewModelDCO();
                salesOrderViewModelDCO.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // SalesOrder
                salesOrderViewModelDCO.SalesOrder = salesOrder;
                salesOrderViewModelDCO.RequestUIDDictionary[DbTableName.SalesOrder] = new List<string> { salesOrderViewModelDCO.SalesOrder.UID };
                // SalesOrderLine
                if (salesOrderLineList != null && salesOrderLineList.Count > 0)
                {
                    salesOrderViewModelDCO.SalesOrderLines = salesOrderLineList.Where(e => e.KeyUID == salesOrder.UID).ToList();
                    if (salesOrderViewModelDCO.SalesOrderLines != null && salesOrderViewModelDCO.SalesOrderLines.Count > 0)
                    {
                        salesOrderViewModelDCO.RequestUIDDictionary[DbTableName.SalesOrderLine] = salesOrderViewModelDCO.SalesOrderLines.Select(e => e.UID).ToList();
                    }
                }
                // WHStockLedger
                if (wHStockLedgerList != null && wHStockLedgerList.Count > 0)
                {
                    salesOrderViewModelDCO.WHStockLedgerList = wHStockLedgerList.Where(e => e.KeyUID == salesOrder.UID).ToList();
                    if (salesOrderViewModelDCO.WHStockLedgerList != null && salesOrderViewModelDCO.WHStockLedgerList.Count > 0)
                    {
                        salesOrderViewModelDCO.RequestUIDDictionary[DbTableName.WHStockLedger] = salesOrderViewModelDCO.WHStockLedgerList.Select(e => e.UID).ToList();
                    }
                }

                // StoreHistory
                if (storeHistoryList != null && storeHistoryList.Count > 0)
                {
                    salesOrderViewModelDCO.StoreHistory = storeHistoryList.Where(e => e.KeyUID == salesOrder.UID).FirstOrDefault();
                    if (salesOrderViewModelDCO.StoreHistory != null)
                    {
                        salesOrderViewModelDCO.RequestUIDDictionary[DbTableName.StoreHistory] = new List<string> { salesOrderViewModelDCO.StoreHistory.UID };
                    }
                }

                // AccPayable
                if (accPayableList != null && accPayableList.Count > 0)
                {
                    salesOrderViewModelDCO.AccPayable = accPayableList.Where(e => e.KeyUID == salesOrder.UID).FirstOrDefault();
                    if (salesOrderViewModelDCO.AccPayable != null)
                    {
                        salesOrderViewModelDCO.RequestUIDDictionary[DbTableName.AccPayable] = new List<string> { salesOrderViewModelDCO.AccPayable.UID };
                    }
                }
                salesOrderViewModelDCOList.Add(salesOrderViewModelDCO);
            }
            return salesOrderViewModelDCOList;
        }
        public async Task<List<MerchandiserDTO>?> PrepareInsertUpdateData_Merchandiser()
        {
            try
            {
                List<MerchandiserDTO>? merchandiserDCOList = null;
                List<Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>? captureCompitetorList = null;
                List<CategoryBrandMapping>? categoryBrandMappingList = null;
                List<CategoryBrandCompetitorMapping>? categoryBrandCompetitorMappingList = null;
                List<PlanogramSetup>? planogramSetupList = null;
                List<PlanogramExecutionHeader>? planogramExecutionHeaderList = null;
                List<PlanogramExecutionDetail>? planogramExecutionDetailList = null;
                List<StoreCheckGroupHistory>? storeCheckGroupHistoryList = null;
                List<StoreCheckItemHistory>? storyCheckItemHistoryList = null;
                List<StoreCheckItemUomQty>? storeCheckItemUomList = null;
                List<StoreCheckHistoryView>? storeCheckHistoryList = null;
                List<StoreCheckItemExpiryDREHistory>? storeCheckExpiryDREHistoryList = null;


                List<POExecution>? poExecutionList = null;
                List<POExecutionLine>? poExecutionLineList = null;
                List<ProductFeedback>? productFeedbackList = null;
                List<BroadcastInitiative>? broadcastInitiativeList = null;
                List<ExpiryCheckExecution>? expiryCheckExecutionList = null;
                List<ExpiryCheckExecutionLine>? expiryCheckExecutionLineList = null;
                List<PlanogramExecutionV1>? planogramExecutionV1List = null;
                List<ProductSampling>? productSamplingList = null;

                using (IDbConnection connection = SqliteConnection())
                {
                    List<string> uIDs = await GetUIDsToPushByTable(DbTableName.CaptureCompetitor, 1000);
                    if (uIDs == null)
                    {
                        return null;
                    }
                    List<ICaptureCompetitor>? captureCompetitor = await GetCaptureCompetitorData(uIDs, connection);
                    if (captureCompetitor != null && captureCompetitor.Count > 0)
                    {
                        captureCompitetorList = captureCompetitor.Cast<Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>().ToList();
                    }
                    

                    List<ICategoryBrandMapping>? categoryBrandMapping = await GetCategoryBrandMappingData(connection);
                    if (categoryBrandMapping != null && categoryBrandMapping.Count > 0)
                    {
                        categoryBrandMappingList = categoryBrandMapping.Cast<CategoryBrandMapping>().ToList();
                    }

                    List<ICategoryBrandCompetitorMapping>? categoryBrandCompetitorMapping = await GetCategoryBrandCompetitorMappingData(connection);
                    if (categoryBrandCompetitorMapping != null && categoryBrandCompetitorMapping.Count > 0)
                    {
                        categoryBrandCompetitorMappingList = categoryBrandCompetitorMapping.Cast<CategoryBrandCompetitorMapping>().ToList();
                    }

                    List<IPlanogramSetup>? planogramSetup = await GetPlanogramSetupData(connection);
                    if (planogramSetup != null && planogramSetup.Count > 0)
                    {
                        planogramSetupList = planogramSetup.Cast<PlanogramSetup>().ToList();
                    }

                    List<IPlanogramExecutionHeader>? planogramExecutionHeader = await GetPlanogramExecutionHeaderData(connection);
                    if (planogramExecutionHeader != null && planogramExecutionHeader.Count > 0)
                    {
                        planogramExecutionHeaderList = planogramExecutionHeader.Cast<PlanogramExecutionHeader>().ToList();
                    }

                    List<IPlanogramExecutionDetail>? planogramExecutionDetail = await GetPlanogramExecutionDetailData(connection);
                    if (planogramExecutionDetail != null && planogramExecutionDetail.Count > 0)
                    {
                        planogramExecutionDetailList = planogramExecutionDetail.Cast<PlanogramExecutionDetail>().ToList();
                    }

                    List<IStoreCheckGroupHistory>? storeCheckGroupHistory = await GetStoreCheckGroupHistoryData(connection);
                    if (storeCheckGroupHistory != null && storeCheckGroupHistory.Count > 0)
                    {
                        storeCheckGroupHistoryList = storeCheckGroupHistory.Cast<StoreCheckGroupHistory>().ToList();
                    }
                    
                    List<IStoreCheckItemHistory>? storeCheckItemHistory = await GetStoreCheckItemHistoryData(connection);
                    if (storeCheckItemHistory != null && storeCheckItemHistory.Count > 0)
                    {
                        storyCheckItemHistoryList  = storeCheckItemHistory.Cast<StoreCheckItemHistory>().ToList();
                    }

                    List<IStoreCheckItemUomQty>? storeCheckItemUomQty = await GetStoreCheckItemUomQtyData(connection);
                    if (storeCheckItemUomQty != null && storeCheckItemUomQty.Count > 0)
                    {
                        storeCheckItemUomList = storeCheckItemUomQty.Cast<StoreCheckItemUomQty>().ToList();
                    }
                    
                    List<IStoreCheckHistoryView>? storeCheckHistory = await GetStoreCheckHistoryData(connection);
                    if (storeCheckHistory != null && storeCheckHistory.Count > 0)
                    {
                        storeCheckHistoryList = storeCheckHistory.Cast<StoreCheckHistoryView>().ToList();
                    }
                    
                    List<IStoreCheckItemExpiryDREHistory>? storeCheckItemExpiryDREHistory = await GetStoreCheckItemExpiryDerHistoryData(connection);
                    if (storeCheckItemExpiryDREHistory != null && storeCheckItemExpiryDREHistory.Count > 0)
                    {
                        storeCheckExpiryDREHistoryList = storeCheckItemExpiryDREHistory.Cast<StoreCheckItemExpiryDREHistory>().ToList();
                    }
                    
                    List<IPOExecution>? poExecution = await GetPOExecutionData(connection);
                    if (poExecution != null && poExecution.Count > 0)
                    {
                        poExecutionList = poExecution.Cast<POExecution>().ToList();
                    }
                    
                    List<IPOExecutionLine>? poExecutionLine = await GetPOExecutionLineData(connection);
                    if (poExecutionLine != null && poExecutionLine.Count > 0)
                    {
                        poExecutionLineList = poExecutionLine.Cast<POExecutionLine>().ToList();
                    }
                    
                    List<IProductFeedback>? productFeedback = await GetProductFeedbackData(connection);
                    if (productFeedback != null && productFeedback.Count > 0)
                    {
                        productFeedbackList = productFeedback.Cast<ProductFeedback>().ToList();
                    }
                    
                    List<IBroadcastInitiative>? broadcastInitiative = await GetBroadcastInitiativeData(connection);
                    if (broadcastInitiative != null && broadcastInitiative.Count > 0)
                    {
                        broadcastInitiativeList = broadcastInitiative.Cast<BroadcastInitiative>().ToList();
                    }
                    
                    List<IExpiryCheckExecution>? expiryCheckExecution = await GetExpiryCheckExecutionData(connection);
                    if (expiryCheckExecution != null && expiryCheckExecution.Count > 0)
                    {
                        expiryCheckExecutionList = expiryCheckExecution.Cast<ExpiryCheckExecution>().ToList();
                    }
                    
                    List<IExpiryCheckExecutionLine>? expiryCheckExecutionLine = await GetExpiryCheckExecutionLineData(connection);
                    if (expiryCheckExecutionLine != null && expiryCheckExecutionLine.Count > 0)
                    {
                        expiryCheckExecutionLineList = expiryCheckExecutionLine.Cast<ExpiryCheckExecutionLine>().ToList();
                    }
                    
                    List<IPlanogramExecutionV1>? planogramExecutionV1 = await GetPlanogramExecutionV1Data(connection);
                    if (planogramExecutionV1 != null && planogramExecutionV1.Count > 0)
                    {
                        planogramExecutionV1List = planogramExecutionV1.Cast<PlanogramExecutionV1>().ToList();
                    }
                    
                    List<IProductSampling>? productSampling = await GetProductSamplingData(connection);
                    if (productSampling != null && productSampling.Count > 0)
                    {
                        productSamplingList = productSampling.Cast<ProductSampling>().ToList();
                    }
                    
                }
                merchandiserDCOList = new List<MerchandiserDTO>();
                MerchandiserDTO merchandiserDCO = new MerchandiserDTO();
                merchandiserDCO.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // CaptureCompetitor
                if (captureCompitetorList != null && captureCompitetorList.Count > 0)
                {
                    merchandiserDCO.captureCompetitor = captureCompitetorList;
                    if (merchandiserDCO.captureCompetitor != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.CaptureCompetitor] = merchandiserDCO.captureCompetitor.Select(p => p.UID).ToList();
                    }
                }
                // CategoryBrandMapping
                if (categoryBrandMappingList != null && categoryBrandMappingList.Count > 0)
                {
                    merchandiserDCO.categoryBrandMapping = categoryBrandMappingList;
                    if (merchandiserDCO.categoryBrandMapping != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.CategoryBrandMapping] = merchandiserDCO.categoryBrandMapping.Select(p => p.UID).ToList();
                    }
                }
                // CategoryBrandCompetitorMapping
                if (categoryBrandCompetitorMappingList != null && categoryBrandCompetitorMappingList.Count > 0)
                {
                    merchandiserDCO.categoryBrandCompetitorMapping = categoryBrandCompetitorMappingList;
                    if (merchandiserDCO.categoryBrandCompetitorMapping != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.CategoryBrandCompetitorMapping] = merchandiserDCO.categoryBrandCompetitorMapping.Select(p => p.UID).ToList();
                    }
                }
                // PlanogramSetup
                if (planogramSetupList != null && planogramSetupList.Count > 0)
                {
                    merchandiserDCO.planogramSetup = planogramSetupList;
                    if (merchandiserDCO.planogramSetup != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.PlanogramSetup] = merchandiserDCO.planogramSetup.Select(p => p.UID).ToList();
                    }
                }
                // PlanogramExecutionHeader
                if (planogramExecutionHeaderList != null && planogramExecutionHeaderList.Count > 0)
                {
                    merchandiserDCO.planogramExecutionHeader = planogramExecutionHeaderList;
                    if (merchandiserDCO.planogramExecutionHeader != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.PlanogramExecutionHeader] = merchandiserDCO.planogramExecutionHeader.Select(p => p.UID).ToList();
                    }
                }
                // PlanogramExecutionDetail
                if (planogramExecutionDetailList != null && planogramExecutionDetailList.Count > 0)
                {
                    merchandiserDCO.planogramExecutionDetails = planogramExecutionDetailList.ToList();
                    if (merchandiserDCO.planogramExecutionDetails != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.PlanogramExecutionDetail] = merchandiserDCO.planogramExecutionDetails.Select(p => p.UID).ToList();
                    }
                }
                // StoreCheckGroupHistory
                if (storeCheckGroupHistoryList != null && storeCheckGroupHistoryList.Count > 0)
                {
                    merchandiserDCO.storeCheckGroupHistory = storeCheckGroupHistoryList.ToList();
                    if (merchandiserDCO.storeCheckGroupHistory != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.StoreCheckGroupHistory] = merchandiserDCO.storeCheckGroupHistory.Select(p => p.UID).ToList();
                    }
                }
                // StoryCheckItemHistory
                if (storyCheckItemHistoryList != null && storyCheckItemHistoryList.Count > 0)
                {
                    merchandiserDCO.storeCheckItemHistory = storyCheckItemHistoryList.ToList();
                    if (merchandiserDCO.storeCheckItemHistory != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.StoreCheckItemHistory] = merchandiserDCO.storeCheckItemHistory.Select(p => p.UID).ToList();
                    }
                }
                // StoreCheckItemUom
                if (storeCheckItemUomList != null && storeCheckItemUomList.Count > 0)
                {
                    merchandiserDCO.storeCheckItemUom = storeCheckItemUomList.ToList();
                    if (merchandiserDCO.storeCheckItemUom != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.StoreCheckItemUOMQty] = merchandiserDCO.storeCheckItemUom.Select(p => p.UID).ToList();
                    }
                }
                // StoreCheckHistory
                if (storeCheckHistoryList != null && storeCheckHistoryList.Count > 0)
                {
                    merchandiserDCO.storeCheckHistory = storeCheckHistoryList.ToList();
                    if (merchandiserDCO.storeCheckHistory != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.StoreCheckHistory] = merchandiserDCO.storeCheckHistory.Select(p => p.UID).ToList();
                    }
                }
                // StoreCheckExpiryDREHistory
                if (storeCheckExpiryDREHistoryList != null && storeCheckExpiryDREHistoryList.Count > 0)
                {
                    merchandiserDCO.storeCheckExpiryDREHistory = storeCheckExpiryDREHistoryList.ToList();
                    if (merchandiserDCO.storeCheckExpiryDREHistory != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.StoreCheckItemExpiryDerHistory] = merchandiserDCO.storeCheckExpiryDREHistory.Select(p => p.UID).ToList();
                    }
                }

                if (poExecutionList != null && poExecutionList.Count > 0)
                {
                    merchandiserDCO.poExecution = poExecutionList.ToList();
                    if (merchandiserDCO.poExecution != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.POExecution] = merchandiserDCO.poExecution.Select(p => p.UID).ToList();
                    }
                }
                if (poExecutionLineList != null && poExecutionLineList.Count > 0)
                {
                    merchandiserDCO.poExecutionLine = poExecutionLineList.ToList();
                    if (merchandiserDCO.poExecutionLine != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.POExecutionLine] = merchandiserDCO.poExecutionLine.Select(p => p.UID).ToList();
                    }
                }
                if (productFeedbackList != null && productFeedbackList.Count > 0)
                {
                    merchandiserDCO.productFeedback = productFeedbackList.ToList();
                    if (merchandiserDCO.productFeedback != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.ProductFeedback] = merchandiserDCO.productFeedback.Select(p => p.UID).ToList();
                    }
                }
                if (broadcastInitiativeList != null && broadcastInitiativeList.Count > 0)
                {
                    merchandiserDCO.broadcastInitiative = broadcastInitiativeList.ToList();
                    if (merchandiserDCO.broadcastInitiative != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.BroadcastInitiative] = merchandiserDCO.broadcastInitiative.Select(p => p.UID).ToList();
                    }
                }
                if (expiryCheckExecutionList != null && expiryCheckExecutionList.Count > 0)
                {
                    merchandiserDCO.expiryCheckExecution = expiryCheckExecutionList.ToList();
                    if (merchandiserDCO.expiryCheckExecution != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.ExpiryCheckExecution] = merchandiserDCO.expiryCheckExecution.Select(p => p.UID).ToList();
                    }
                }
                if (expiryCheckExecutionLineList != null && expiryCheckExecutionLineList.Count > 0)
                {
                    merchandiserDCO.expiryCheckExecutionLine = expiryCheckExecutionLineList.ToList();
                    if (merchandiserDCO.expiryCheckExecutionLine != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.ExpiryCheckExecutionLine] = merchandiserDCO.expiryCheckExecutionLine.Select(p => p.UID).ToList();
                    }
                }
                if (planogramExecutionV1List != null && planogramExecutionV1List.Count > 0)
                {
                    merchandiserDCO.planogramExecutionV1 = planogramExecutionV1List.ToList();
                    if (merchandiserDCO.planogramExecutionV1 != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.PlanogramExecutionV1] = merchandiserDCO.planogramExecutionV1.Select(p => p.UID).ToList();
                    }
                }
                
                if (productSamplingList != null && productSamplingList.Count > 0)
                {
                    merchandiserDCO.productSampling = productSamplingList.ToList();
                    if (merchandiserDCO.productSampling != null)
                    {
                        merchandiserDCO.RequestUIDDictionary[DbTableName.ProductSampling] = merchandiserDCO.productSampling.Select(p => p.UID).ToList();
                    }
                }
                merchandiserDCOList.Add(merchandiserDCO);
                return merchandiserDCOList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder>?> GetSalesOrderData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT so.uid AS KeyUID, so.id AS Id, so.uid AS UID, so.created_by AS CreatedBy, so.created_time AS CreatedTime, 
                                so.modified_by AS ModifiedBy, so.modified_time AS ModifiedTime, so.server_add_time AS ServerAddTime, 
                                so.server_modified_time AS ServerModifiedTime, so.sales_order_number AS SalesOrderNumber, 
                                so.draft_order_number AS DraftOrderNumber, so.company_uid AS CompanyUID, so.org_uid AS OrgUID, 
                                so.distribution_channel_uid AS DistributionChannelUID, so.delivered_by_org_uid AS DeliveredByOrgUID, 
                                so.store_uid AS StoreUID, so.status AS Status, so.order_type AS OrderType, so.order_date AS OrderDate, 
                                so.customer_po AS CustomerPO, so.currency_uid AS CurrencyUID, so.payment_type AS PaymentType, 
                                so.total_amount AS TotalAmount, so.total_discount AS TotalDiscount, so.total_tax AS TotalTax, 
                                so.net_amount AS NetAmount, so.line_count AS LineCount, so.qty_count AS QtyCount, 
                                so.total_fake_amount AS TotalFakeAmount, so.reference_number AS ReferenceNumber, 
                                so.source AS Source, 0 AS SS, so.total_line_discount AS TotalLineDiscount, 
                                so.total_cash_discount AS TotalCashDiscount, so.total_header_discount AS TotalHeaderDiscount, 
                                so.total_excise_duty AS TotalExciseDuty, so.total_line_tax AS TotalLineTax, 
                                so.total_header_tax AS TotalHeaderTax, so.cash_sales_customer AS CashSalesCustomer, 
                                so.cash_sales_address AS CashSalesAddress, so.reference_uid AS ReferenceUID, 
                                so.reference_type AS ReferenceType, so.job_position_uid AS JobPositionUID, 
                                so.emp_uid AS EmpUID, so.beat_history_uid AS BeatHistoryUID, so.route_uid AS RouteUID, 
                                so.store_history_uid AS StoreHistoryUID, so.total_credit_limit AS TotalCreditLimit, 
                                so.available_credit_limit AS AvailableCreditLimit, so.expected_delivery_date AS ExpectedDeliveryDate, 
                                so.delivered_date_time AS DeliveredDateTime, so.latitude AS Latitude, so.longitude AS Longitude, 
                                so.is_offline AS IsOffline, so.credit_days AS CreditDays, so.notes AS Notes, 
                                so.delivery_instructions AS DeliveryInstructions, so.remarks AS Remarks, 
                                so.is_temperature_check_enabled AS IsTemperatureCheckEnabled, so.always_printed_flag AS AlwaysPrintedFlag, 
                                so.purchase_order_no_required_flag AS PurchaseOrderNoRequiredFlag, so.is_with_printed_invoices_flag AS IsWithPrintedInvoicesFlag, 
                                so.tax_data AS TaxData,so.vehicle_uid AS VehicleUID  
                                FROM sales_order AS so WHERE so.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrder>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine>?> GetSalesOrderLineData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT so.uid AS KeyUID, sol.id AS Id, sol.uid AS UID, sol.created_by AS CreatedBy, sol.created_time AS CreatedTime, 
                                sol.modified_by AS ModifiedBy, sol.modified_time AS ModifiedTime, sol.server_add_time AS ServerAddTime, 
                                sol.server_modified_time AS ServerModifiedTime, sol.sales_order_uid AS SalesOrderUID, 
                                sol.line_number AS LineNumber, sol.item_code AS ItemCode, sol.item_type AS ItemType, 
                                sol.base_price AS BasePrice, sol.unit_price AS UnitPrice, sol.fake_unit_price AS FakeUnitPrice, 
                                sol.base_uom AS BaseUOM, sol.uom AS UoM, sol.uom_conversion_to_bu AS UOMConversionToBU, 
                                sol.reco_uom AS RecoUOM, sol.reco_qty AS RecoQty, sol.reco_uom_conversion_to_bu AS RecoUOMConversionToBU, 
                                sol.reco_qty_bu AS RecoQtyBU, sol.model_qty_bu AS ModelQtyBU, sol.qty AS Qty, sol.qty_bu AS QtyBU, 
                                sol.van_qty_bu AS VanQtyBU, sol.delivered_qty AS DeliveredQty, sol.missed_qty AS MissedQty, 
                                sol.returned_qty AS ReturnedQty, sol.total_amount AS TotalAmount, sol.total_discount AS TotalDiscount, 
                                sol.line_tax_amount AS LineTaxAmount, sol.prorata_tax_amount AS ProrataTaxAmount, sol.total_tax AS TotalTax, 
                                sol.net_amount AS NetAmount, sol.net_fake_amount AS NetFakeAmount, sol.sku_price_uid AS SKUPriceUID, 
                                sol.prorata_discount_amount AS ProrataDiscountAmount, sol.line_discount_amount AS LineDiscountAmount, 
                                sol.mrp AS MRP, sol.cost_unit_price AS CostUnitPrice, sol.parent_uid AS ParentUID, sol.is_promotion_applied AS IsPromotionApplied, 
                                sol.volume AS Volume, sol.volume_unit AS VolumeUnit, sol.weight AS Weight, sol.weight_unit AS WeightUnit, 
                                sol.stock_type AS StockType, sol.remarks AS Remarks, sol.total_cash_discount AS TotalCashDiscount, 
                                sol.total_excise_duty AS TotalExciseDuty, sol.sku_uid AS SKUUID, sol.approved_qty AS ApprovedQty,
                                sol.tax_data AS TaxData,0 AS SS
                                FROM sales_order AS so 
                                INNER JOIN sales_order_line sol ON sol.sales_order_uid = so.uid 
                                WHERE so.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.SalesOrder.Model.Interfaces.ISalesOrderLine>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>?> GetWHStockLedgerDataForSales(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT so.uid AS KeyUID, whl.id AS Id, whl.uid AS UID, 0 AS SS, whl.created_by AS CreatedBy, 
                                whl.created_time AS CreatedTime, whl.modified_by AS ModifiedBy, whl.modified_time AS ModifiedTime, 
                                whl.server_add_time AS ServerAddTime, whl.server_modified_time AS ServerModifiedTime, 
                                whl.company_uid AS CompanyUID, whl.warehouse_uid AS WarehouseUID, whl.org_uid AS OrgUID, 
                                whl.sku_uid AS SKUUID, whl.sku_code AS SKUCode, whl.type AS Type, whl.reference_type AS ReferenceType, 
                                whl.reference_uid AS ReferenceUID, whl.batch_number AS BatchNumber, whl.expiry_date AS ExpiryDate, 
                                whl.qty AS Qty, whl.uom AS UOM, whl.stock_type AS StockType, whl.serial_no AS SerialNo, 
                                whl.version_no AS VersionNo, whl.parent_wh_uid AS ParentWHUID, whl.year_month AS YearMonth
                                FROM sales_order AS so 
                                INNER JOIN sales_order_line sol ON sol.sales_order_uid = so.uid 
                                INNER JOIN wh_stock_ledger whl ON whl.reference_type = '{LinkedItemType.SalesOrderLine}' AND whl.reference_uid = sol.uid 
                                WHERE so.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>?> GetStoreHistoryDataForSales(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT so.uid AS KeyUID, sh.id AS Id, sh.uid AS UID, sh.created_by AS CreatedBy, sh.created_time AS CreatedTime, 
                                sh.modified_by AS ModifiedBy, sh.modified_time AS ModifiedTime, sh.server_add_time AS ServerAddTime, 
                                sh.server_modified_time AS ServerModifiedTime, sh.user_journey_uid AS UserJourneyUID, sh.year_month AS YearMonth, 
                                sh.beat_history_uid AS BeatHistoryUID, sh.org_uid AS OrgUID, sh.route_uid AS RouteUID, sh.store_uid AS StoreUID, 
                                sh.is_planned AS IsPlanned, sh.serial_no AS SerialNo, sh.status AS Status, sh.visit_duration AS VisitDuration, 
                                sh.travel_time AS TravelTime, sh.planned_login_time AS PlannedLoginTime, sh.planned_logout_time AS PlannedLogoutTime, 
                                sh.login_time AS LoginTime, sh.logout_time AS LogoutTime, sh.no_of_visits AS NoOfVisits, 
                                sh.last_visit_date AS LastVisitDate, sh.is_skipped AS IsSkipped, sh.is_productive AS IsProductive, 
                                sh.is_green AS IsGreen, sh.target_value AS TargetValue, sh.target_volume AS TargetVolume, 
                                sh.target_lines AS TargetLines, sh.actual_value AS ActualValue, sh.actual_volume AS ActualVolume, 
                                sh.actual_lines AS ActualLines, sh.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes, 
                                sh.latitude AS Latitude, sh.longitude AS Longitude, sh.notes AS Notes, 0 AS SS 
                                FROM store_history AS sh
                                INNER JOIN sales_order AS so ON so.store_history_uid = sh.uid
                                WHERE so.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>?> GetAccPayableDataForSales(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT so.uid AS KeyUID, ap.id AS Id, ap.uid AS UID, 0 AS SS, ap.created_by AS CreatedBy, ap.created_time AS CreatedTime, 
                            ap.modified_by AS ModifiedBy, ap.modified_time AS ModifiedTime, ap.server_add_time AS ServerAddTime, 
                            ap.server_modified_time AS ServerModifiedTime, ap.source_type AS SourceType, ap.source_uid AS SourceUID, 
                            ap.reference_number AS ReferenceNumber, ap.org_uid AS OrgUID, ap.job_position_uid AS JobPositionUID, 
                            ap.amount AS Amount, ap.paid_amount AS PaidAmount, ap.store_uid AS StoreUID, ap.transaction_date AS TransactionDate, 
                            ap.due_date AS DueDate,ap.unsettled_amount AS UnsettledAmount, ap.source AS Source, ap.currency_uid AS CurrencyUID 
                            FROM acc_payable AS ap
                            INNER JOIN sales_order AS so ON ap.source_type = 'INVOICE' AND ap.source_uid = so.uid
                            WHERE so.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccPayable>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Merchandiser
        private async Task<List<ICaptureCompetitor>?> GetCaptureCompetitorData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT CC.uid AS KeyUID, CC.ss as SS, CC.id AS Id, CC.uid AS UID, CC.created_by AS CreatedBy, CC.created_time AS CreatedTime, 
                                CC.modified_by AS ModifiedBy, CC.modified_time AS ModifiedTime, CC.server_add_time AS ServerAddTime, 
                                CC.server_modified_time AS ServerModifiedTime,
                                CC.store_uid AS StoreUID,
                                CC.status AS Status,
                                CC.store_history_uid AS StoreHistoryUID,
                                CC.beat_history_uid AS BeatHistoryUID,
                                CC.route_uid AS RouteUID,
                                CC.activity_date AS ActivityDate,
                                CC.job_position_uid AS JobPositionUID,
                                CC.emp_uid AS EmpUID,
                                CC.our_brand AS OurBrand,
                                CC.our_price AS OurPrice,
                                CC.other_company AS OtherCompany,
                                CC.other_brand_name AS OtherBrandName,
                                CC.other_item_name AS OtherItemName,
                                CC.other_temperature AS OtherTemperature,
                                CC.other_price AS OtherPrice,
                                CC.other_promotion AS OtherPromotion,
                                CC.other_notes AS OtherNotes,
                                CC.other_selling_price AS OtherSellingPrice,
                                CC.other_uom AS OtherUOM
                                FROM capture_competitor as CC where CC.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<ICaptureCompetitor>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<ICategoryBrandMapping>?> GetCategoryBrandMappingData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT
                                cbm.id AS Id,
                                cbm.uid AS UID,cbm.ss as SS,
                                cbm.category_code AS CategoryCode,
                                cbm.brand_code AS BrandCode,
                                cbm.created_by AS CreatedBy,
                                cbm.created_time AS CreatedTime,
                                cbm.modified_by AS ModifiedBy,
                                cbm.modified_time AS ModifiedTime,
                                cbm.server_add_time AS ServerAddTime,
                                cbm.server_modified_time AS ServerModifiedTime
                                FROM category_brand_mapping as cbm where cbm.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<ICategoryBrandMapping>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<ICategoryBrandCompetitorMapping>?> GetCategoryBrandCompetitorMappingData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT
                                cbcm.id AS Id,
                                cbcm.uid AS UID,cbcm.ss as SS,
                                cbcm.category_brand_uid AS CategoryBrandUID,
                                cbcm.competitor_code AS CompetitorCode,
                                cbcm.created_by AS CreatedBy,
                                cbcm.created_time AS CreatedTime,
                                cbcm.modified_by AS ModifiedBy,
                                cbcm.modified_time AS ModifiedTime,
                                cbcm.server_add_time AS ServerAddTime,
                                cbcm.server_modified_time AS ServerModifiedTime
                                FROM category_brand_competitor_mapping as cbcm where cbcm.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<ICategoryBrandCompetitorMapping>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IPlanogramSetup>?> GetPlanogramSetupData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT 
                                ps.id AS Id,
                                ps.uid AS UID, ps.ss as SS,
                                ps.category_code AS CategoryCode,
                                ps.share_of_shelf_cm AS ShareOfShelfCm,
                                ps.selection_type AS SelectionType,
                                ps.selection_value AS SelectionValue,
                                ps.created_by AS CreatedBy,
                                ps.created_time AS CreatedTime,
                                ps.modified_by AS ModifiedBy,
                                ps.modified_time AS ModifiedTime,
                                ps.server_add_time AS ServerAddTime,
                                ps.server_modified_time AS ServerModifiedTime
                                FROM planogram_setup ps where ps.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<IPlanogramSetup>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IPlanogramExecutionHeader>?> GetPlanogramExecutionHeaderData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT 
                                peh.id AS Id,
                                peh.uid AS UID, peh.ss as SS,
                                peh.beat_history_uid AS BeatHistoryUID,
                                peh.store_history_uid AS StoreHistoryUID,
                                peh.store_uid AS StoreUID,
                                peh.job_position_uid AS JobPositionUID,
                                peh.route_uid AS RouteUID,
                                peh.status AS Status,
                                peh.created_by AS CreatedBy,
                                peh.created_time AS CreatedTime,
                                peh.modified_by AS ModifiedBy,
                                peh.modified_time AS ModifiedTime,
                                peh.server_add_time AS ServerAddTime,
                                peh.server_modified_time AS ServerModifiedTime
                                FROM planogram_execution_header peh where peh.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<IPlanogramExecutionHeader>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IPlanogramExecutionDetail>?> GetPlanogramExecutionDetailData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT peh.uid as KeyUID,
                                ped.id AS Id,
                                ped.uid AS UID, ped.ss as SS,
                                ped.planogram_execution_header_uid AS PlanogramExecutionHeaderUID,
                                ped.planogram_setup_uid AS PlanogramSetupUID,
                                ped.executed_on AS ExecutedOn,
                                ped.is_completed AS IsCompleted,
                                ped.created_by AS CreatedBy,
                                ped.created_time AS CreatedTime,
                                ped.modified_by AS ModifiedBy,
                                ped.modified_time AS ModifiedTime,
                                ped.server_add_time AS ServerAddTime,
                                ped.server_modified_time AS ServerModifiedTime
                                FROM planogram_execution_detail ped
                                INNER JOIN planogram_execution_header peh ON peh.uid = ped.planogram_execution_header_uid where ped.ss in (1 ,2)";
                //var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<IPlanogramExecutionDetail>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCheckGroupHistory>?> GetStoreCheckGroupHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT 
                scgh.id AS Id,
                scgh.uid AS UID,
                scgh.store_check_history_uid AS StoreCheckHistoryUID,
                scgh.group_by_code AS GroupByCode,
                scgh.group_by_value AS GroupByValue,
                scgh.store_check_level AS StoreCheckLevel,
                scgh.status AS Status,
                scgh.ss AS SS,
                scgh.created_time AS CreatedTime,
                scgh.modified_time AS ModifiedTime,
                scgh.server_add_time AS ServerAddTime,
                scgh.server_modified_time AS ServerModifiedTime
            FROM 
                store_check_group_history scgh
            WHERE 
                scgh.ss IN (1, 2)";

                return await ExecuteQueryAsync<IStoreCheckGroupHistory>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCheckItemHistory>?> GetStoreCheckItemHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT 
                scih.id AS Id,
                scih.uid AS UID,
                scih.store_check_history_uid AS StoreCheckHistoryUID,
                scih.group_by_code AS GroupByCode,
                scih.group_by_value AS GroupByValue,
                scih.sku_uid AS SkuUID,
                scih.sku_code AS SkuCode,
                scih.uom AS UOM,
                scih.suggested_qty AS SuggestedQty,
                scih.store_qty AS StoreQty,
                scih.backstore_qty AS BackstoreQty,
                scih.to_fill_qty AS ToFillQty,
                scih.is_available AS IsAvailable,
                scih.is_dre_selected AS IsDRESelected,
                scih.ss AS SS,
                scih.created_time AS CreatedTime,
                scih.modified_time AS ModifiedTime,
                scih.server_add_time AS ServerAddTime,
                scih.server_modified_time AS ServerModifiedTime
            FROM 
                store_check_item_history scih
            WHERE 
                scih.ss IN (1, 2)";

                return await ExecuteQueryAsync<IStoreCheckItemHistory>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCheckItemUomQty>?> GetStoreCheckItemUomQtyData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT 
                sciuy.id AS Id,
                sciuy.uid AS UID,
                sciuy.store_check_item_history_uid AS StoreCheckItemHistoryUID,
                sciuy.uom AS UOM,
                sciuy.base_uom AS BaseUOM,
                sciuy.uom_multiplier AS UomMultiplier,
                sciuy.store_qty AS StoreQty,
                sciuy.store_qty_bu AS StoreQtyBU,
                sciuy.back_store_qty AS BackStoreQty,
                sciuy.back_store_qty_bu AS BackStoreQtyBU,
                sciuy.ss AS SS,
                sciuy.created_time AS CreatedTime,
                sciuy.modified_time AS ModifiedTime,
                sciuy.server_add_time AS ServerAddTime,
                sciuy.server_modified_time AS ServerModifiedTime
            FROM 
                store_check_item_uom_qty sciuy
            WHERE 
                sciuy.ss IN (1, 2)";

                return await ExecuteQueryAsync<IStoreCheckItemUomQty>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCheckHistoryView>?> GetStoreCheckHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT 
                sch.id AS Id,
                sch.uid AS UID,
                sch.beat_history_uid AS BeatHistoryUID,
                sch.store_history_uid AS StoreHistoryUID,
                sch.store_asset_uid AS StoreAssetUID,
                sch.org_uid AS OrgUID,
                sch.store_check_date AS StoreCheckDate,
                sch.store_uid AS StoreUID,
                sch.store_check_config_uid AS StoreCheckConfigUID,
                sch.sku_group_type_uid AS SkuGroupTypeUID,
                sch.sku_group_uid AS SkuGroupUID,
                sch.group_by_name AS GroupByName,
                sch.status AS Status,
                sch.level AS Level,
                sch.is_last_level AS IsLastLevel,
                sch.ss AS SS,
                sch.created_time AS CreatedTime,
                sch.modified_time AS ModifiedTime,
                sch.server_add_time AS ServerAddTime,
                sch.server_modified_time AS ServerModifiedTime
            FROM 
                store_check_history sch
            WHERE 
                sch.ss IN (1, 2)";

                return await ExecuteQueryAsync<IStoreCheckHistoryView>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IStoreCheckItemExpiryDREHistory>?> GetStoreCheckItemExpiryDerHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT 
                sciedh.id AS Id,
                sciedh.uid AS UID,
                sciedh.store_check_item_history_uid AS StoreCheckItemHistoryUID,
                sciedh.stock_type AS StockType,
                sciedh.stock_sub_type AS StockSubType,
                sciedh.batch_no AS BatchNo,
                sciedh.expiry_date AS ExpiryDate,
                sciedh.reason AS Reason,
                sciedh.qty AS Qty,
                sciedh.uom AS UOM,
                sciedh.notes AS Notes,
                sciedh.ss AS SS,
                sciedh.created_time AS CreatedTime,
                sciedh.modified_time AS ModifiedTime,
                sciedh.server_add_time AS ServerAddTime,
                sciedh.server_modified_time AS ServerModifiedTime
            FROM 
                store_check_item_expiry_der_history sciedh
            WHERE 
                sciedh.ss IN (1, 2)";

                return await ExecuteQueryAsync<IStoreCheckItemExpiryDREHistory>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        
        private async Task<List<IPOExecution>?> GetPOExecutionData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT pe.id AS Id, pe.uid AS UID, pe.ss AS SS, pe.created_by AS CreatedBy, pe.created_time AS CreatedTime,
            pe.modified_by AS ModifiedBy, pe.modified_time AS ModifiedTime, pe.server_add_time AS ServerAddTime, pe.server_modified_time AS ServerModifiedTime,
            pe.beat_history_uid AS BeatHistoryUID, pe.store_history_uid AS StoreHistoryUID, pe.route_uid AS RouteUID, pe.store_uid AS StoreUID, 
            pe.job_position_uid AS JobPositionUID, pe.emp_uid AS EmpUID, pe.execution_time AS ExecutionTime, pe.po_number AS PONumber, pe.line_count AS LineCount,
            pe.qty_count AS QtyCount, pe.total_amount AS TotalAmount FROM po_execution pe 
                WHERE 
                pe.ss IN (1, 2)";

                return await ExecuteQueryAsync<IPOExecution>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IPOExecutionLine>?> GetPOExecutionLineData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT pl.id AS Id, pl.uid AS UID, pl.ss AS SS, pl.line_number AS LineNumber, pl.sku_uid AS SKUUID, pl.qty AS Qty, 
pl.price AS Price, pl.total_amount AS TotalAmount, pl.po_execution_uid AS POExecutionUID FROM po_execution_line pl
            WHERE 
                pl.ss IN (1, 2)";

                return await ExecuteQueryAsync<IPOExecutionLine>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IProductFeedback>?> GetProductFeedbackData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT pf.id AS Id, pf.uid AS UID, pf.ss AS SS, pf.created_by AS CreatedBy, pf.created_time AS CreatedTime, pf.modified_by AS ModifiedBy,
pf.modified_time AS ModifiedTime, pf.server_add_time AS ServerAddTime, pf.server_modified_time AS ServerModifiedTime, pf.beat_history_uid AS BeatHistoryUID, 
pf.route_uid AS RouteUID, pf.job_position_uid AS JobPositionUID, pf.emp_uid AS EmpUID, pf.execution_time AS ExecutionTime, pf.store_uid AS StoreUID, 
pf.sku_uid AS SKUUID, pf.feedback_on AS FeedbackOn, pf.end_customer_name AS EndCustomerName, pf.mobile_no AS MobileNo FROM product_feedback pf
            WHERE 
                pf.ss IN (1, 2)";

                return await ExecuteQueryAsync<IProductFeedback>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IBroadcastInitiative>?> GetBroadcastInitiativeData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT bi.id AS Id, bi.uid AS UID, bi.ss AS SS, bi.created_by AS CreatedBy, bi.created_time AS CreatedTime, bi.modified_by AS ModifiedBy, 
bi.modified_time AS ModifiedTime, bi.server_add_time AS ServerAddTime, bi.server_modified_time AS ServerModifiedTime, bi.beat_history_uid AS BeatHistoryUID, 
bi.route_uid AS RouteUID, bi.job_position_uid AS JobPositionUID, bi.emp_uid AS EmpUID, bi.execution_time AS ExecutionTime, bi.store_uid AS StoreUID,
bi.sku_uid AS SKUUID, bi.gender AS Gender, bi.end_customer_name AS EndCustomerName, bi.mobile_no AS MobileNo, bi.ftb_rc AS FtbRc FROM broadcast_initiative bi
            WHERE 
                bi.ss IN (1, 2)";

                return await ExecuteQueryAsync<IBroadcastInitiative>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IExpiryCheckExecution>?> GetExpiryCheckExecutionData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT ece.id AS Id, ece.uid AS UID, ece.ss AS SS, ece.created_by AS CreatedBy, ece.created_time AS CreatedTime, ece.modified_by AS ModifiedBy,
ece.modified_time AS ModifiedTime, ece.server_add_time AS ServerAddTime, ece.server_modified_time AS ServerModifiedTime, ece.beat_history_uid AS BeatHistoryUID,
ece.store_history_uid AS StoreHistoryUID, ece.route_uid AS RouteUID, ece.store_uid AS StoreUID, ece.job_position_uid AS JobPositionUID, ece.emp_uid AS EmpUID,
ece.execution_time AS ExecutionTime FROM expiry_check_execution ece
            WHERE 
                ece.ss IN (1, 2)";

                return await ExecuteQueryAsync<IExpiryCheckExecution>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IExpiryCheckExecutionLine>?> GetExpiryCheckExecutionLineData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT el.id AS Id, el.uid AS UID, el.ss AS SS, el.line_number AS LineNumber, el.sku_uid AS SKUUID, el.qty AS Qty, el.expiry_date AS ExpiryDate, 
el.expiry_check_execution_uid AS ExpiryCheckExecutionUID FROM expiry_check_execution_line el
            WHERE 
                el.ss IN (1, 2)";

                return await ExecuteQueryAsync<IExpiryCheckExecutionLine>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        private async Task<List<IPlanogramExecutionV1>?> GetPlanogramExecutionV1Data(IDbConnection? connection)
        {
            try
            {
                var query = $@"
            SELECT pe.id AS Id, pe.uid AS UID, pe.created_by AS CreatedBy, pe.created_time AS CreatedTime, pe.modified_by AS ModifiedBy, pe.modified_time AS ModifiedTime, 
pe.server_add_time AS ServerAddTime, pe.server_modified_time AS ServerModifiedTime, pe.beat_history_uid AS BeatHistoryUID, pe.store_history_uid AS StoreHistoryUID,
pe.route_uid AS RouteUID, pe.store_uid AS StoreUID, pe.job_position_uid AS JobPositionUID, pe.emp_uid AS EmpUID, pe.screen_name AS ScreenName,
pe.planogram_setup_v1_uid AS PlanogramSetupV1UID, pe.execution_time AS ExecutionTime FROM planogram_execution_v1 pe
            WHERE 
                pe.ss IN (1, 2)";

                return await ExecuteQueryAsync<IPlanogramExecutionV1>(query, null, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<IProductSampling>?> GetProductSamplingData(IDbConnection? connection)
        {
            try
            {
                var query = @"
        SELECT ps.id AS Id, ps.uid AS UID, ps.ss AS SS, ps.created_by AS CreatedBy, ps.created_time AS CreatedTime, 
               ps.modified_by AS ModifiedBy, ps.modified_time AS ModifiedTime, ps.server_add_time AS ServerAddTime, 
               ps.server_modified_time AS ServerModifiedTime, ps.beat_history_uid AS BeatHistoryUID, 
               ps.route_uid AS RouteUID, ps.job_position_uid AS JobPositionUID, ps.emp_uid AS EmpUID, 
               ps.execution_time AS ExecutionTime, ps.store_uid AS StoreUID, ps.sku_uid AS SKUUID, 
               ps.selling_price AS SellingPrice, ps.unit_used AS UnitUsed, ps.unit_sold AS UnitSold, 
               ps.no_of_customer_approached AS NoOfCustomerApproached 
        FROM product_sampling ps
        WHERE ps.ss IN (1, 2)";

                return await ExecuteQueryAsync<IProductSampling>(query, null, null, connection);
            }
            catch
            {
                throw;
            }
        }

        public Task<int> CreateDynamicTable(bool action, string empCode, string tableName)
        {
            return null;
        }
        #endregion
        #region WHStockRequest
        public async Task<List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>?> PrepareInsertUpdateData_WHStockRequest()
        {
            List<Winit.Modules.WHStock.Model.Classes.WHRequestTempleteModel>? wHRequestTempleteModelList = null;
            List<Winit.Modules.WHStock.Model.Classes.WHStockRequest>? wHStockRequestList = null;
            List<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine>? wHStockRequestLineList = null;
            List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? wHStockLedgerList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.WHStockRequest, 1000);
                if (uIDs == null)
                {
                    return null;
                }
                List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest>? wHStockRequests = await GetWHStockRequestData(uIDs, connection);
                if (wHStockRequests == null || wHStockRequests.Count == 0)
                {
                    return null;
                }
                wHStockRequestList = wHStockRequests.Cast<Winit.Modules.WHStock.Model.Classes.WHStockRequest>().ToList();

                List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>? wHStockRequestLines = await GetWHStockRequestLineData(uIDs, connection);
                if (wHStockRequestLines != null && wHStockRequestLines.Count > 0)
                {
                    wHStockRequestLineList = wHStockRequestLines.Cast<Winit.Modules.WHStock.Model.Classes.WHStockRequestLine>().ToList();
                }

                List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgers = await GetWHStockLedgerDataForWHStockRequest(uIDs, connection);
                if (wHStockLedgers != null && wHStockLedgers.Count > 0)
                {
                    wHStockLedgerList = wHStockLedgers.Cast<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>().ToList();
                }
            }

            wHRequestTempleteModelList = new List<WHRequestTempleteModel>();
            foreach (Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest wHStockRequest in wHStockRequestList)
            {
                WHRequestTempleteModel wHRequestTempleteModel = new WHRequestTempleteModel();
                wHRequestTempleteModel.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // WHStockRequest
                wHRequestTempleteModel.WHStockRequest = (Winit.Modules.WHStock.Model.Classes.WHStockRequest)wHStockRequest;
                wHRequestTempleteModel.RequestUIDDictionary[DbTableName.WHStockRequest] = new List<string> { wHRequestTempleteModel.WHStockRequest.UID };
                // WHStockRequestLines
                if (wHStockRequestLineList != null && wHStockRequestLineList.Count > 0)
                {
                    wHRequestTempleteModel.WHStockRequestLines = wHStockRequestLineList.Where(e => e.KeyUID == wHStockRequest.UID).ToList();
                    if (wHRequestTempleteModel.WHStockRequestLines != null && wHRequestTempleteModel.WHStockRequestLines.Count > 0)
                    {
                        wHRequestTempleteModel.RequestUIDDictionary[DbTableName.WHStockRequestLine] = wHRequestTempleteModel.WHStockRequestLines.Select(e => e.UID).ToList();
                    }
                }
                // WHStockLedger
                if (wHStockLedgerList != null && wHStockLedgerList.Count > 0)
                {
                    wHRequestTempleteModel.WHStockLedgerList = wHStockLedgerList.Where(e => e.KeyUID == wHStockRequest.UID).ToList();
                    if (wHRequestTempleteModel.WHStockLedgerList != null && wHRequestTempleteModel.WHStockLedgerList.Count > 0)
                    {
                        wHRequestTempleteModel.RequestUIDDictionary[DbTableName.WHStockLedger] = wHRequestTempleteModel.WHStockLedgerList.Select(e => e.UID).ToList();
                    }
                }

                wHRequestTempleteModelList.Add(wHRequestTempleteModel);
            }
            return wHRequestTempleteModelList;
        }
        private async Task<List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest>?> GetWHStockRequestData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT wr.uid AS KeyUID, wr.id AS Id, wr.uid AS UID, wr.company_uid AS CompanyUID, 
                            wr.source_org_uid AS SourceOrgUID, wr.source_wh_uid AS SourceWHUID,wr.target_org_uid AS TargetOrgUID, 
                            wr.target_wh_uid AS TargetWHUID, wr.route_uid AS RouteUID, wr.code AS Code, wr.request_type AS RequestType, 
                            wr.request_by_emp_uid AS RequestByEmpUID, wr.job_position_uid AS JobPositionUID, 
                            wr.required_by_date AS RequiredByDate, wr.status AS Status, wr.remarks AS Remarks, 
                            wr.stock_type AS StockType, 0 AS SS, wr.created_time AS CreatedTime, 
                            wr.modified_time AS ModifiedTime, wr.server_add_time AS ServerAddTime, 
                            wr.server_modified_time AS ServerModifiedTime, wr.org_uid AS OrgUID, 
                            wr.warehouse_uid AS WarehouseUID, wr.year_month AS YearMonth 
                            FROM wh_stock_request AS wr 
                            WHERE wr.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequest>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>?> GetWHStockRequestLineData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT wr.uid AS KeyUID, wsrl.id AS Id, wsrl.uid AS UID, wsrl.wh_stock_request_uid AS WhStockRequestUID, 
                                wsrl.line_number AS LineNumber, wsrl.stock_sub_type AS StockSubType, wsrl.sku_uid AS SkuUID, 
                                wsrl.sku_code AS SkuCode, wsrl.uom1 AS UOM1, wsrl.uom2 AS UOM2, wsrl.uom AS UOM, 
                                wsrl.uom1_cnf AS UOM1Cnf, wsrl.uom2_cnf AS UOM2Cnf, wsrl.requested_qty1 AS RequestedQty1, 
                                wsrl.requested_qty2 AS RequestedQty2, wsrl.requested_qty AS RequestedQty, 
                                wsrl.cpe_approved_qty1 AS CPEApprovedQty1, wsrl.cpe_approved_qty2 AS CPEApprovedQty2, 
                                wsrl.cpe_approved_qty AS CPEApprovedQty, wsrl.approved_qty1 AS ApprovedQty1, 
                                wsrl.approved_qty2 AS ApprovedQty2, wsrl.approved_qty AS ApprovedQty, 
                                wsrl.forward_qty1 AS ForwardQty1, wsrl.forward_qty2 AS ForwardQty2, 
                                wsrl.forward_qty AS ForwardQty, wsrl.collected_qty1 AS CollectedQty1, 
                                wsrl.collected_qty2 AS CollectedQty2, wsrl.collected_qty AS CollectedQty, 
                                wsrl.wh_qty AS WhQty, 0 AS SS, wsrl.created_time AS CreatedTime, 
                                wsrl.modified_time AS ModifiedTime, wsrl.server_add_time AS ServerAddTime, 
                                wsrl.server_modified_time AS ServerModifiedTime, wsrl.template_qty1 AS TemplateQty1, 
                                wsrl.template_qty2 AS TemplateQty2, wsrl.org_uid AS OrgUID, wsrl.warehouse_uid AS WarehouseUID, 
                                wsrl.year_month AS YearMonth 
                                FROM wh_stock_request AS wr  
                                INNER JOIN wh_stock_request_line wsrl ON wsrl.wh_stock_request_uid = wr.uid 
                                WHERE wr.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.WHStock.Model.Interfaces.IWHStockRequestLine>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>?> GetWHStockLedgerDataForWHStockRequest(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT wr.uid AS KeyUID, whl.id AS Id, whl.uid AS UID, 0 AS SS, whl.created_by AS CreatedBy, 
                                whl.created_time AS CreatedTime, whl.modified_by AS ModifiedBy, whl.modified_time AS ModifiedTime, 
                                whl.server_add_time AS ServerAddTime, whl.server_modified_time AS ServerModifiedTime, 
                                whl.company_uid AS CompanyUID, whl.warehouse_uid AS WarehouseUID, whl.org_uid AS OrgUID, 
                                whl.sku_uid AS SKUUID, whl.sku_code AS SKUCode, whl.type AS Type, whl.reference_type AS ReferenceType, 
                                whl.reference_uid AS ReferenceUID, whl.batch_number AS BatchNumber, whl.expiry_date AS ExpiryDate, 
                                whl.qty AS Qty, whl.uom AS UOM, whl.stock_type AS StockType, whl.serial_no AS SerialNo, 
                                whl.version_no AS VersionNo, whl.parent_wh_uid AS ParentWHUID, whl.year_month AS YearMonth
                                FROM wh_stock_request AS wr  
                                INNER JOIN wh_stock_request_line wsrl ON wsrl.wh_stock_request_uid = wr.uid 
                                INNER JOIN wh_stock_ledger whl ON whl.reference_type = '{LinkedItemType.WHStockRequestStock}' AND whl.reference_uid = wsrl.uid 
                                WHERE wr.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Return
        public async Task<List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>?> PrepareInsertUpdateData_Return()
        {
            List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>? returnOrderMasterDTOList = null;
            List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>? returnOrderList = null;
            List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine>? returnOrderLineList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? storeHistoryList = null;
            List<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>? wHStockLedgerList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.ReturnOrder, 1000);
                if (uIDs == null)
                {
                    return null;
                }
                List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>? ReturnOrders = await GetReturnOrderData(uIDs, connection);
                if (ReturnOrders == null || ReturnOrders.Count == 0)
                {
                    return null;
                }
                returnOrderList = ReturnOrders.Cast<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder>().ToList();

                List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>? returnOrderLineLines = await GetReturnOrderLineData(uIDs, connection);
                if (returnOrderLineLines != null && returnOrderLineLines.Count > 0)
                {
                    returnOrderLineList = returnOrderLineLines.Cast<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderLine>().ToList();
                }

                List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>? storeHistories = await GetStoreHistoryDataForReturn(uIDs, connection);
                if (storeHistories != null && storeHistories.Count > 0)
                {
                    storeHistoryList = storeHistories.Cast<StoreHistory>().ToList();
                }

                List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>? wHStockLedgers = await GetWHStockLedgerDataForReturn(uIDs, connection);
                if (wHStockLedgers != null && wHStockLedgers.Count > 0)
                {
                    wHStockLedgerList = wHStockLedgers.Cast<Winit.Modules.StockUpdater.Model.Classes.WHStockLedger>().ToList();
                }
            }

            returnOrderMasterDTOList = new List<Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO>();
            foreach (Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder returnOrder in returnOrderList)
            {
                Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO returnOrderMasterDTO = new Winit.Modules.ReturnOrder.Model.Classes.ReturnOrderMasterDTO();
                returnOrderMasterDTO.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // ReturnOrder
                returnOrderMasterDTO.ReturnOrder = (Winit.Modules.ReturnOrder.Model.Classes.ReturnOrder)returnOrder;
                returnOrderMasterDTO.RequestUIDDictionary[DbTableName.ReturnOrder] = new List<string> { returnOrderMasterDTO.ReturnOrder.UID };
                // ReturnOrderLine
                if (returnOrderLineList != null && returnOrderLineList.Count > 0)
                {
                    returnOrderMasterDTO.ReturnOrderLineList = returnOrderLineList.Where(e => e.KeyUID == returnOrder.UID).ToList();
                    if (returnOrderMasterDTO.ReturnOrderLineList != null && returnOrderMasterDTO.ReturnOrderLineList.Count > 0)
                    {
                        returnOrderMasterDTO.RequestUIDDictionary[DbTableName.WHStockRequestLine] = returnOrderMasterDTO.ReturnOrderLineList.Select(e => e.UID).ToList();
                    }
                }
                // StoreHistory
                if (storeHistoryList != null && storeHistoryList.Count > 0)
                {
                    returnOrderMasterDTO.StoreHistory = storeHistoryList.Where(e => e.KeyUID == returnOrder.UID).FirstOrDefault();
                    if (returnOrderMasterDTO.StoreHistory != null)
                    {
                        returnOrderMasterDTO.RequestUIDDictionary[DbTableName.StoreHistory] = new List<string> { returnOrderMasterDTO.StoreHistory.UID };
                    }
                }
                // WHStockLedger
                if (wHStockLedgerList != null && wHStockLedgerList.Count > 0)
                {
                    returnOrderMasterDTO.WHStockLedgerList = wHStockLedgerList.Where(e => e.KeyUID == returnOrder.UID).ToList();
                    if (returnOrderMasterDTO.WHStockLedgerList != null && returnOrderMasterDTO.WHStockLedgerList.Count > 0)
                    {
                        returnOrderMasterDTO.RequestUIDDictionary[DbTableName.WHStockLedger] = returnOrderMasterDTO.WHStockLedgerList.Select(e => e.UID).ToList();
                    }
                }
                returnOrderMasterDTOList.Add(returnOrderMasterDTO);
            }
            return returnOrderMasterDTOList;
        }
        private async Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>?> GetReturnOrderData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ro.uid AS KeyUID, ro.id AS Id, ro.uid AS UID, ro.created_by AS CreatedBy, 
                            ro.created_time AS CreatedTime, ro.modified_by AS ModifiedBy, ro.modified_time AS ModifiedTime, 
                            ro.server_add_time AS ServerAddTime, ro.server_modified_time AS ServerModifiedTime, 
                            ro.return_order_number AS ReturnOrderNumber, ro.draft_order_number AS DraftOrderNumber, 
                            ro.job_position_uid AS JobPositionUID, ro.emp_uid AS EmpUID, ro.org_uid AS OrgUID, 
                            ro.distribution_channel_uid AS DistributionChannelUID, ro.store_uid AS StoreUID, 
                            ro.is_tax_applicable AS IsTaxApplicable, ro.route_uid AS RouteUID, 
                            ro.beat_history_uid AS BeatHistoryUID, ro.store_history_uid AS StoreHistoryUID, 
                            ro.status AS Status, ro.order_type AS OrderType, ro.order_date AS OrderDate, 
                            ro.currency_uid AS CurrencyUID, ro.total_amount AS TotalAmount, 
                            ro.total_line_discount AS TotalLineDiscount, ro.total_cash_discount AS TotalCashDiscount, 
                            ro.total_header_discount AS TotalHeaderDiscount, ro.total_discount AS TotalDiscount, 
                            ro.total_excise_duty AS TotalExciseDuty, ro.line_tax_amount AS LineTaxAmount, 
                            ro.header_tax_amount AS HeaderTaxAmount, ro.total_tax AS TotalTax, 
                            ro.net_amount AS NetAmount, ro.total_fake_amount AS TotalFakeAmount, 
                            ro.line_count AS LineCount, ro.qty_count AS QtyCount, ro.notes AS Notes, 
                            ro.is_offline AS IsOffline, ro.latitude AS Latitude, ro.longitude AS Longitude, 
                            ro.delivered_by_org_uid AS DeliveredByOrgUID, 0 AS SS, ro.source AS Source, 
                            ro.promotion_uid AS PromotionUID, ro.total_line_tax AS TotalLineTax, 
                            ro.total_header_tax AS TotalHeaderTax,ro.sales_order_uid AS SalesOrderUID 
                            FROM return_order ro
                            WHERE ro.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrder>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>?> GetReturnOrderLineData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ro.uid AS KeyUID, rol.id AS Id, rol.uid AS UID, rol.created_by AS CreatedBy, 
                                rol.created_time AS CreatedTime, rol.modified_by AS ModifiedBy, 
                                rol.modified_time AS ModifiedTime, rol.server_add_time AS ServerAddTime, 
                                rol.server_modified_time AS ServerModifiedTime, 0 AS SS, 
                                rol.return_order_uid AS ReturnOrderUID, rol.line_number AS LineNumber, 
                                rol.sku_uid AS SKUUID, rol.sku_code AS SKUCode, rol.sku_type AS SKUType, 
                                rol.base_price AS BasePrice, rol.unit_price AS UnitPrice, 
                                rol.fake_unit_price AS FakeUnitPrice, rol.base_uom AS BaseUOM, 
                                rol.uom AS UoM, rol.multiplier AS Multiplier, rol.qty AS Qty, 
                                rol.qty_bu AS QtyBu, rol.approved_qty AS ApprovedQty, rol.returned_qty AS ReturnedQty, 
                                rol.total_amount AS TotalAmount, rol.total_discount AS TotalDiscount, 
                                rol.total_excise_duty AS TotalExciseDuty, rol.total_tax AS TotalTax, 
                                rol.net_amount AS NetAmount, rol.sku_price_uid AS SKUPriceUID, 
                                rol.sku_price_list_uid AS SKUPriceListUID, rol.reason_code AS ReasonCode, 
                                rol.reason_text AS ReasonText, rol.expiry_date AS ExpiryDate, rol.batch_number AS BatchNumber, 
                                rol.sales_order_uid AS SalesOrderUID, rol.sales_order_line_uid AS SalesOrderLineUID, 
                                rol.remarks AS Remarks, rol.volume AS Volume, rol.volume_unit AS VolumeUnit, 
                                rol.promotion_uid AS PromotionUID, rol.net_fake_amount AS NetFakeAmount, 
                                rol.po_number AS PONumber,rol.available_qty AS AvailableQty 
                                FROM return_order AS ro  
                                INNER JOIN return_order_line rol ON rol.return_order_uid = ro.uid 
                                WHERE ro.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderLine>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>?> GetWHStockLedgerDataForReturn(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ro.uid AS KeyUID, whl.id AS Id, whl.uid AS UID, 0 AS SS, whl.created_by AS CreatedBy, 
                                whl.created_time AS CreatedTime, whl.modified_by AS ModifiedBy, whl.modified_time AS ModifiedTime, 
                                whl.server_add_time AS ServerAddTime, whl.server_modified_time AS ServerModifiedTime, 
                                whl.company_uid AS CompanyUID, whl.warehouse_uid AS WarehouseUID, whl.org_uid AS OrgUID, 
                                whl.sku_uid AS SKUUID, whl.sku_code AS SKUCode, whl.type AS Type, whl.reference_type AS ReferenceType, 
                                whl.reference_uid AS ReferenceUID, whl.batch_number AS BatchNumber, whl.expiry_date AS ExpiryDate, 
                                whl.qty AS Qty, whl.uom AS UOM, whl.stock_type AS StockType, whl.serial_no AS SerialNo, 
                                whl.version_no AS VersionNo, whl.parent_wh_uid AS ParentWHUID, whl.year_month AS YearMonth
                                FROM return_order AS ro  
                                INNER JOIN return_order_line rol ON rol.return_order_uid = ro.uid 
                                INNER JOIN wh_stock_ledger whl ON whl.reference_type = '{LinkedItemType.ReturnOrderLine}' AND whl.reference_uid = rol.uid 
                                WHERE ro.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StockUpdater.Model.Interfaces.IWHStockLedger>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>?> GetStoreHistoryDataForReturn(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ro.uid AS KeyUID, sh.id AS Id, sh.uid AS UID, sh.created_by AS CreatedBy, sh.created_time AS CreatedTime, 
                                sh.modified_by AS ModifiedBy, sh.modified_time AS ModifiedTime, sh.server_add_time AS ServerAddTime, 
                                sh.server_modified_time AS ServerModifiedTime, sh.user_journey_uid AS UserJourneyUID, sh.year_month AS YearMonth, 
                                sh.beat_history_uid AS BeatHistoryUID, sh.org_uid AS OrgUID, sh.route_uid AS RouteUID, sh.store_uid AS StoreUID, 
                                sh.is_planned AS IsPlanned, sh.serial_no AS SerialNo, sh.status AS Status, sh.visit_duration AS VisitDuration, 
                                sh.travel_time AS TravelTime, sh.planned_login_time AS PlannedLoginTime, sh.planned_logout_time AS PlannedLogoutTime, 
                                sh.login_time AS LoginTime, sh.logout_time AS LogoutTime, sh.no_of_visits AS NoOfVisits, 
                                sh.last_visit_date AS LastVisitDate, sh.is_skipped AS IsSkipped, sh.is_productive AS IsProductive, 
                                sh.is_green AS IsGreen, sh.target_value AS TargetValue, sh.target_volume AS TargetVolume, 
                                sh.target_lines AS TargetLines, sh.actual_value AS ActualValue, sh.actual_volume AS ActualVolume, 
                                sh.actual_lines AS ActualLines, sh.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes, 
                                sh.latitude AS Latitude, sh.longitude AS Longitude, sh.notes AS Notes, 0 AS SS 
                                FROM store_history AS sh
                                INNER JOIN return_order AS ro ON ro.store_history_uid = sh.uid
                                WHERE ro.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region Collection
        public async Task<List<Winit.Modules.CollectionModule.Model.Classes.CollectionDTO>?> PrepareInsertUpdateData_Collection()
        {
            List<Winit.Modules.CollectionModule.Model.Classes.CollectionDTO>? collectionDTOList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollection>? accCollectionList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode>? accCollectionPaymentModeList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccStoreLedger>? accStoreLedgerList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionAllotment>? accCollectionAllotmentList = null;
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionCurrencyDetails>? accCollectionCurrencyList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? storeHistoryList = null;


            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.AccCollection, 1000);
                if (uIDs == null)
                {
                    return null;
                }
                List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>? accCollections = await GetAccCollectionData(uIDs, connection);
                if (accCollections == null || accCollections.Count == 0)
                {
                    return null;
                }
                accCollectionList = accCollections.Cast<Winit.Modules.CollectionModule.Model.Classes.AccCollection>().ToList();

                List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>? accCollectionPaymentModes = await GetAccCollectionPaymentModeData(uIDs, connection);
                if (accCollectionPaymentModes != null && accCollectionPaymentModes.Count > 0)
                {
                    accCollectionPaymentModeList = accCollectionPaymentModes.Cast<Winit.Modules.CollectionModule.Model.Classes.AccCollectionPaymentMode>().ToList();
                }

                List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>? accStoreLedgers = await GetAccStoreLedgerData(uIDs, connection);
                if (accStoreLedgers != null && accStoreLedgers.Count > 0)
                {
                    accStoreLedgerList = accStoreLedgers.Cast<AccStoreLedger>().ToList();
                }

                List<IAccCollectionAllotment>? accCollectionAllotments = await GetAccCollectionAllotmentData(uIDs, connection);
                if (accCollectionAllotments != null && accCollectionAllotments.Count > 0)
                {
                    accCollectionAllotmentList = accCollectionAllotments.Cast<Winit.Modules.CollectionModule.Model.Classes.AccCollectionAllotment>().ToList();
                }

                List<IAccCollectionCurrencyDetails>? accCollectionCurrencies = await GetAccCollectionCurrencyDetailsData(uIDs, connection);
                if (accCollectionCurrencies != null && accCollectionCurrencies.Count > 0)
                {
                    accCollectionCurrencyList = accCollectionCurrencies.Cast<Winit.Modules.CollectionModule.Model.Classes.AccCollectionCurrencyDetails>().ToList();
                }

                List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>? storeHistories = await GetStoreHistoryDataForCollection(uIDs, connection);
                if (storeHistories != null && storeHistories.Count > 0)
                {
                    storeHistoryList = storeHistories.Cast<StoreHistory>().ToList();
                }
            }

            collectionDTOList = new List<Winit.Modules.CollectionModule.Model.Classes.CollectionDTO>();
            foreach (Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection collection in accCollectionList)
            {
                Winit.Modules.CollectionModule.Model.Classes.CollectionDTO collectionDTO = new Winit.Modules.CollectionModule.Model.Classes.CollectionDTO();
                collectionDTO.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // AccCollection
                collectionDTO.AccCollection = (Winit.Modules.CollectionModule.Model.Classes.AccCollection)collection;
                collectionDTO.RequestUIDDictionary[DbTableName.AccCollection] = new List<string> { collectionDTO.AccCollection.UID };
                // AccCollectionPaymentMode
                collectionDTO.AccCollectionPaymentMode = accCollectionPaymentModeList.Where(e => e.KeyUID == collection.UID).FirstOrDefault();
                collectionDTO.RequestUIDDictionary[DbTableName.AccCollectionPaymentMode] = new List<string> { collectionDTO.AccCollectionPaymentMode.UID };
                // AccStoreLedger
                collectionDTO.AccStoreLedger = accStoreLedgerList.Where(e => e.KeyUID == collection.UID).FirstOrDefault();
                collectionDTO.RequestUIDDictionary[DbTableName.AccStoreLedger] = new List<string> { collectionDTO.AccStoreLedger.UID };
                // AccCollectionAllotment
                if (accCollectionAllotmentList != null && accCollectionAllotmentList.Count > 0)
                {
                    collectionDTO.AccCollectionAllotment = accCollectionAllotmentList.Where(e => e.KeyUID == collection.UID).ToList();
                    if (collectionDTO.AccCollectionAllotment != null && collectionDTO.AccCollectionAllotment.Count > 0)
                    {
                        collectionDTO.RequestUIDDictionary[DbTableName.AccCollectionAllotment] = collectionDTO.AccCollectionAllotment.Select(e => e.UID).ToList();
                    }
                }
                if (accCollectionCurrencyList != null && accCollectionCurrencyList.Count > 0)
                {
                    collectionDTO.AccCollectionCurrencyDetails = accCollectionCurrencyList.Where(e => e.KeyUID == collection.UID).ToList();
                    if (collectionDTO.AccCollectionCurrencyDetails != null && collectionDTO.AccCollectionCurrencyDetails.Count > 0)
                    {
                        collectionDTO.RequestUIDDictionary[DbTableName.AccCollectionCurrencyDetails] = collectionDTO.AccCollectionCurrencyDetails.Select(e => e.UID).ToList();
                    }
                }
                if (storeHistoryList != null && storeHistoryList.Count > 0)
                {
                    collectionDTO.StoreHistory = storeHistoryList.Where(e => e.KeyUID == collection.UID).FirstOrDefault();
                    if (collectionDTO.StoreHistory != null)
                    {
                        collectionDTO.RequestUIDDictionary[DbTableName.StoreHistory] = new List<string> { collectionDTO.StoreHistory.UID };
                    }
                }
                collectionDTOList.Add(collectionDTO);
            }
            return collectionDTOList;
        }

        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>?> GetAccCollectionData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, ac.id, ac.uid, ac.created_by, ac.created_time, ac.modified_by, ac.modified_time, 
                               ac.server_add_time, ac.server_modified_time, ac.receipt_number, ac.consolidated_receipt_number, 
                               ac.category, ac.amount, ac.currency_uid, ac.default_currency_uid, ac.default_currency_exchange_rate, 
                               ac.default_currency_amount, ac.org_uid, ac.distribution_channel_uid, ac.store_uid, 
                               ac.route_uid, ac.job_position_uid, ac.emp_uid, ac.collected_date, ac.status, ac.remarks, 
                               ac.reference_number, ac.is_realized, ac.latitude, ac.longitude, ac.source, ac.is_multimode, 
                               ac.trip_date, ac.comments, ac.salesman, ac.route, ac.reversal_receipt_uid, ac.cancelled_on, 
                               ac.is_remote_collection, ac.remote_collection_reason, ac.collection_deposit_status
                               FROM acc_collection ac WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollection>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>?> GetAccCollectionPaymentModeData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, acpm.id, acpm.uid, acpm.created_by, acpm.created_time, acpm.modified_by, acpm.modified_time, 
                               acpm.server_add_time, acpm.server_modified_time, acpm.acc_collection_uid, acpm.bank_uid, 
                               acpm.branch, acpm.cheque_no, acpm.amount, acpm.currency_uid, acpm.default_currency_uid, 
                               acpm.default_currency_exchange_rate, acpm.default_currency_amount, acpm.cheque_date, 
                               acpm.status, acpm.realization_date, acpm.comments, acpm.approve_comments, acpm.check_list_data
                               FROM acc_collection ac 
                               INNER JOIN acc_collection_payment_mode acpm ON acpm.acc_collection_uid = ac.uid 
                               WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionPaymentMode>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>?> GetAccStoreLedgerData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, asl.id, asl.uid, asl.created_by, asl.created_time, asl.modified_by, asl.modified_time, 
                               asl.server_add_time, asl.server_modified_time, asl.source_type, asl.source_uid, 
                               asl.credit_type, asl.org_uid, asl.store_uid, asl.default_currency_uid, asl.document_number, 
                               asl.default_currency_exchange_rate, asl.default_currency_amount, asl.transaction_date_time, 
                               asl.collected_amount, asl.currency_uid, asl.amount, asl.balance, asl.comments
                               FROM acc_store_ledger asl
                               INNER JOIN acc_collection ac ON asl.source_uid = ac.uid
                               WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccStoreLedger>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>?> GetAccCollectionAllotmentData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, aca.id, aca.uid, aca.created_by, aca.created_time, aca.modified_by, aca.modified_time, 
                                aca.server_add_time, aca.server_modified_time, aca.acc_collection_uid, aca.target_type, 
                                aca.target_uid, aca.reference_number, aca.amount, aca.currency_uid, aca.default_currency_uid, 
                                aca.default_currency_exchange_rate, aca.default_currency_amount, 
                                aca.early_payment_discount_percentage, aca.early_payment_discount_amount, 
                                aca.early_payment_discount_reference_no
                                FROM acc_collection_allotment aca  
                                INNER JOIN acc_collection ac ON aca.acc_collection_uid = ac.uid WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionAllotment>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionCurrencyDetails>?> GetAccCollectionCurrencyDetailsData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, accd.id, accd.uid, accd.created_by, accd.created_time, 
                               accd.modified_by, accd.modified_time, accd.server_add_time, 
                               accd.server_modified_time, accd.acc_collection_uid, 
                               accd.currency_uid, accd.default_currency_uid, 
                               accd.default_currency_exchange_rate, accd.amount, 
                               accd.default_currency_amount, accd.final_default_currency_amount
                               FROM acc_collection_currency_details accd  
                               INNER JOIN acc_collection ac ON accd.acc_collection_uid = ac.uid WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionCurrencyDetails>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>?> GetStoreHistoryDataForCollection(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT ac.uid AS KeyUID, sh.id AS Id, sh.uid AS UID, sh.created_by AS CreatedBy, sh.created_time AS CreatedTime, 
                                sh.modified_by AS ModifiedBy, sh.modified_time AS ModifiedTime, sh.server_add_time AS ServerAddTime, 
                                sh.server_modified_time AS ServerModifiedTime, sh.user_journey_uid AS UserJourneyUID, sh.year_month AS YearMonth, 
                                sh.beat_history_uid AS BeatHistoryUID, sh.org_uid AS OrgUID, sh.route_uid AS RouteUID, sh.store_uid AS StoreUID, 
                                sh.is_planned AS IsPlanned, sh.serial_no AS SerialNo, sh.status AS Status, sh.visit_duration AS VisitDuration, 
                                sh.travel_time AS TravelTime, sh.planned_login_time AS PlannedLoginTime, sh.planned_logout_time AS PlannedLogoutTime, 
                                sh.login_time AS LoginTime, sh.logout_time AS LogoutTime, sh.no_of_visits AS NoOfVisits, 
                                sh.last_visit_date AS LastVisitDate, sh.is_skipped AS IsSkipped, sh.is_productive AS IsProductive, 
                                sh.is_green AS IsGreen, sh.target_value AS TargetValue, sh.target_volume AS TargetVolume, 
                                sh.target_lines AS TargetLines, sh.actual_value AS ActualValue, sh.actual_volume AS ActualVolume, 
                                sh.actual_lines AS ActualLines, sh.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes, 
                                sh.latitude AS Latitude, sh.longitude AS Longitude, sh.notes AS Notes, 0 AS SS 
                                FROM store_history AS sh
                                INNER JOIN acc_collection AS ac ON ac.store_history_uid = sh.uid
                                WHERE ac.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region CollectionDeposit
        public async Task<List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>?> PrepareInsertUpdateData_CollectionDeposit()
        {
            List<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>? accCollectionDepositList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.AccCollectionDeposit, 1000);
                if (uIDs == null)
                {
                    return null;
                }

                List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionDeposit>? accCollectionDeposits = await GetAccCollectionDepositData(uIDs, connection);
                if (accCollectionDeposits == null || accCollectionDeposits.Count == 0)
                {
                    return null;
                }

                accCollectionDepositList = accCollectionDeposits.Cast<Winit.Modules.CollectionModule.Model.Classes.AccCollectionDeposit>().ToList();
            }

            foreach (AccCollectionDeposit accCollectionDeposit in accCollectionDepositList)
            {
                if (accCollectionDeposit.RequestUIDDictionary == null)
                {
                    accCollectionDeposit.RequestUIDDictionary = new Dictionary<string, List<string>>();
                }

                accCollectionDeposit.RequestUIDDictionary[DbTableName.AccCollectionDeposit] = new List<string> { accCollectionDeposit.UID };
            }

            return accCollectionDepositList;
        }

        private async Task<List<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionDeposit>?> GetAccCollectionDepositData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT acd.uid AS KeyUID, acd.id AS Id, acd.uid AS UID, acd.created_by AS CreatedBy, acd.created_time AS CreatedTime, 
                            acd.modified_by AS ModifiedBy, acd.modified_time AS ModifiedTime, acd.server_add_time AS ServerAddTime,
                            acd.server_modified_time AS ServerModifiedTime, 0 AS SS, acd.emp_uid AS EmpUID, acd.job_position_uid AS JobPositionUID, 
                            acd.request_no AS RequestNo, acd.request_date AS RequestDate, acd.amount AS Amount, acd.bank_uid AS BankUID, acd.branch AS Branch, 
                            acd.notes AS Notes, acd.comments AS Comments, acd.receipt_nos AS ReceiptNos, acd.approval_date AS ApprovalDate,
                            acd.approved_by_emp_uid AS ApprovedByEmpUID, acd.status AS Status FROM acc_collection_deposit AS acd
                            WHERE acd.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.CollectionModule.Model.Interfaces.IAccCollectionDeposit>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Master
        public async Task<List<MasterDTO>?> PrepareInsertUpdateData_Master()
        {
            List<Winit.Modules.JourneyPlan.Model.Classes.MasterDTO>? masterDTOList = null;
            List<Winit.Modules.JobPosition.Model.Classes.JobPositionAttendance>? jobPositionAttendanceList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.UserJourney>? userJourneyList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.ExceptionLog>? exceptionLogList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistory>? storeHistoryList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.StoreHistoryStats>? storeHistoryStatsList = null;
            List<Winit.Modules.JourneyPlan.Model.Classes.BeatHistory>? beatHistoryList = null;
            List<Winit.Modules.StoreActivity.Model.Classes.StoreActivityHistory>? storeActivityHistoryList = null;
            List<Winit.Modules.Address.Model.Classes.Address>? addressModelList = null;
            //List<Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>? captureCompetitorList = null;


            using (IDbConnection connection = SqliteConnection())
            {
                List<IJobPositionAttendance>? jobPositionAttendances = await GetJobPositionAttendanceData(connection);
                if (jobPositionAttendances == null && jobPositionAttendances?.Count > 0)
                {
                    jobPositionAttendanceList = jobPositionAttendances.Cast<Winit.Modules.JobPosition.Model.Classes.JobPositionAttendance>().ToList();
                }

                List<IUserJourney>? userJourneys = await GetUserJourneyData(connection);
                if (userJourneys != null && userJourneys.Count > 0)
                {
                    userJourneyList = userJourneys.Cast<UserJourney>().ToList();
                }
                List<IExceptionLog>? exceptionLogs = await GetExceptionLogData(connection);
                if (exceptionLogs != null && exceptionLogs.Count > 0)
                {
                    exceptionLogList = exceptionLogs.Cast<Winit.Modules.JourneyPlan.Model.Classes.ExceptionLog>().ToList();
                }

                List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>? storeHistories = await GetStoreHistoryData(connection);
                if (storeHistories != null && storeHistories.Count > 0)
                {
                    storeHistoryList = storeHistories.Cast<StoreHistory>().ToList();
                }
               
                List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistoryStats>? storeHistoryStats = await GetStoreHistoryStatsData(connection);
                if (storeHistoryStats != null && storeHistoryStats.Count > 0)
                {
                    storeHistoryStatsList = storeHistoryStats.Cast<StoreHistoryStats>().ToList();
                }

                List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>? beatHistorys = await GetBeatHistoryData(connection);
                if (beatHistorys != null && beatHistorys.Count > 0)
                {
                    beatHistoryList = beatHistorys.Cast<BeatHistory>().ToList();
                }
                List<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityHistory>? StoreActivityHistorys = await GetStoreActivityHistoryData(connection);
                if (StoreActivityHistorys != null && StoreActivityHistorys.Count > 0)
                {
                    storeActivityHistoryList = StoreActivityHistorys.Cast<StoreActivityHistory>().ToList();
                }
                List<Winit.Modules.Address.Model.Interfaces.IAddress>? addresses = await GetAddressData(connection);
                if (addresses != null && addresses.Count > 0)
                {
                    addressModelList = addresses.Cast<Winit.Modules.Address.Model.Classes.Address>().ToList();
                }
                //List<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>? captureCompetitors = await GetCaptureCompetitorData(connection);
                //if (captureCompetitors != null && captureCompetitors.Count > 0)
                //{
                //    captureCompetitorList = captureCompetitors.Cast<Winit.Modules.CaptureCompetitor.Model.Classes.CaptureCompetitor>().ToList();
                //}


            }
            MasterDTO masterDTO = new MasterDTO();
            masterDTOList = new List<MasterDTO>();
            masterDTO.RequestUIDDictionary = new Dictionary<string, List<string>>();

            if (jobPositionAttendanceList != null && jobPositionAttendanceList.Any())
            {
                masterDTO.JobPositionAttendenceList = jobPositionAttendanceList;
                masterDTO.RequestUIDDictionary[DbTableName.JobPositionAttendance] = masterDTO.JobPositionAttendenceList.Select(e => e.UID).ToList();
            }

            if (userJourneyList != null && userJourneyList.Any())
            {
                masterDTO.UserJourneyList = userJourneyList;
                masterDTO.RequestUIDDictionary[DbTableName.UserJourney] = masterDTO.UserJourneyList.Select(e => e.UID).ToList();
            }
            if (exceptionLogList != null && exceptionLogList.Any())
            {
                masterDTO.ExceptionLogList = exceptionLogList;
                masterDTO.RequestUIDDictionary[DbTableName.ExceptionLog] = masterDTO.ExceptionLogList.Select(e => e.UID).ToList();
            }
            if (storeHistoryList != null && storeHistoryList.Any())
            {
                masterDTO.StoreHistoryList = storeHistoryList;
                masterDTO.RequestUIDDictionary[DbTableName.StoreHistory] = masterDTO.StoreHistoryList.Select(e => e.UID).ToList();
            }
            if (storeHistoryStatsList != null && storeHistoryStatsList.Any())
            {
                masterDTO.StoreHistoryStatsList = storeHistoryStatsList;
                masterDTO.RequestUIDDictionary[DbTableName.StoreHistoryStats] = masterDTO.StoreHistoryStatsList.Select(e => e.UID).ToList();
            }
            if (beatHistoryList != null && beatHistoryList.Any())
            {
                masterDTO.BeatHistoryList = beatHistoryList;
                masterDTO.RequestUIDDictionary[DbTableName.BeatHistory] = masterDTO.BeatHistoryList.Select(e => e.UID).ToList();
            }
            if (storeActivityHistoryList != null && storeActivityHistoryList.Any())
            {
                masterDTO.StoreActivityHistoryList = storeActivityHistoryList;
                masterDTO.RequestUIDDictionary[DbTableName.StoreActivityHistory] = masterDTO.StoreActivityHistoryList.Select(e => e.UID).ToList();
            }
            if (addressModelList != null && addressModelList.Any())
            {
                masterDTO.AddressList = addressModelList;
                masterDTO.RequestUIDDictionary[DbTableName.Address] = masterDTO.AddressList.Select(e => e.UID).ToList();
            }
            //if (captureCompetitorList != null && captureCompetitorList.Any())
            //{
            //    masterDTO.CaptureCompetitorList = captureCompetitorList;
            //    masterDTO.RequestUIDDictionary[DbTableName.CaptureCompetitor] = masterDTO.CaptureCompetitorList.Select(e => e.UID).ToList();
            //}


            masterDTOList.Add(masterDTO);
            return masterDTOList;
        }
        //CaptureCompitataor
        private async Task<List<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>?> GetCaptureCompetitorData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT cc.uid AS KeyUID, cc.uid AS UID, 0 AS SS, cc.created_by AS CreatedBy, cc.created_time AS CreatedTime, 
                       cc.modified_by AS ModifiedBy, cc.modified_time AS ModifiedTime, cc.server_add_time AS ServerAddTime, 
                       cc.server_modified_time AS ServerModifiedTime, cc.org_uid AS OrgUID, cc.store_uid AS StoreUID, 
                       cc.status AS Status, cc.store_history_uid AS StoreHistoryUID, cc.beat_history_uid AS BeatHistoryUID, 
                       cc.route_uid AS RouteUID, cc.activity_date AS ActivityDate, cc.job_position_uid AS JobPositionUID, 
                       cc.emp_uid AS EmpUID, cc.our_brand AS OurBrand, cc.our_price AS OurPrice, cc.other_company AS OtherCompany, 
                       cc.other_brand_name AS OtherBrandName, cc.other_item_name AS OtherItemName, cc.other_temperature AS OtherTemperature, 
                       cc.other_price AS OtherPrice, cc.other_promotion AS OtherPromotion, cc.other_notes AS OtherNotes
                       FROM capture_competitor AS cc WHERE cc.ss IN (1,2)";

                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.CaptureCompetitor.Model.Interfaces.ICaptureCompetitor>(
                    query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        //JobPositionAttendance
        private async Task<List<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance>?> GetJobPositionAttendanceData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT jpa.uid AS KeyUID, jpa.uid AS UID, 0 AS SS, jpa.created_by AS CreatedBy, jpa.created_time AS CreatedTime, 
                               jpa.modified_by AS ModifiedBy, jpa.modified_time AS ModifiedTime, jpa.server_add_time AS ServerAddTime, 
                               jpa.server_modified_time AS ServerModifiedTime, jpa.org_uid AS OrgUID, jpa.job_position_uid AS JobPositionUID, 
                               jpa.emp_uid AS EmpUID, jpa.year AS Year, jpa.month AS Month, jpa.no_of_days AS NoOfDays, jpa.no_of_holidays AS NoOfHolidays, 
                               jpa.no_of_working_days AS NoOfWorkingDays, jpa.days_present AS DaysPresent, jpa.attendance_percentage AS AttendancePercentage,
                               jpa.last_update_date AS LastUpdateDate FROM job_position_attendance AS jpa where jpa.ss IN (1,2)";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JobPosition.Model.Interfaces.IJobPositionAttendance>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //UserJourney
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>?> GetUserJourneyData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT uj.uid AS KeyUID,uj.id AS Id, uj.uid AS UID, uj.created_by AS CreatedBy, uj.created_time AS CreatedTime, 
                                uj.modified_by AS ModifiedBy, uj.modified_time AS ModifiedTime, uj.server_add_time AS ServerAddTime, 
                                uj.server_modified_time AS ServerModifiedTime, uj.job_position_uid AS JobPositionUID, uj.emp_uid AS EmpUID, 
                                uj.journey_start_time AS JourneyStartTime, uj.journey_end_time AS JourneyEndTime, uj.start_odometer_reading AS StartOdometerReading,
                                uj.end_odometer_reading AS EndOdometerReading, uj.journey_time AS JourneyTime, uj.vehicle_uid AS VehicleUID, 
                                uj.eot_status AS EotStatus, uj.reopened_by AS ReopenedBy, uj.has_audit_completed AS HasAuditCompleted, 0 AS SS, 
                                uj.beat_history_uid AS BeatHistoryUID, uj.wh_stock_request_uid AS WhStockRequestUID, uj.is_synchronizing AS IsSynchronizing, 
                                uj.has_internet AS HasInternet, uj.internet_type AS InternetType, uj.download_speed AS DownloadSpeed, uj.upload_speed AS UploadSpeed,
                                uj.has_mobile_network AS HasMobileNetwork, uj.is_location_enabled AS IsLocationEnabled, 
                                uj.battery_percentage_target AS BatteryPercentageTarget, uj.battery_percentage_available AS BatteryPercentageAvailable, 
                                uj.attendance_status AS AttendanceStatus, uj.attendance_latitude AS AttendanceLatitude, 
                                uj.attendance_longitude AS AttendanceLongitude, uj.attendance_address AS AttendanceAddress FROM user_journey uj where uj.ss IN (1,2)";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IUserJourney>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //ExceptionLogData
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IExceptionLog>?> GetExceptionLogData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT el.uid AS KeyUID, el.id AS Id, el.uid AS UID, el.store_history_uid AS StoreHistoryUID, 
                               el.store_history_stats_uid AS StoreHistoryStatsUID, el.exception_type AS ExceptionType, el.exception_details AS ExceptionDetails, 
                               el.created_time AS CreatedTime, el.modified_time AS ModifiedTime, 0 AS SS, el.created_by AS CreatedBy, 
                               el.server_add_time AS ServerAddTime, el.server_modified_time AS ServerModifiedTime FROM exception_log el where el.ss IN (1,2)";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IExceptionLog>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //StoreHistory
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>?> GetStoreHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT sh.uid AS KeyUID, sh.id AS Id, sh.uid AS UID, sh.created_by AS CreatedBy, sh.created_time AS CreatedTime, 
                                sh.modified_by AS ModifiedBy, sh.modified_time AS ModifiedTime, sh.server_add_time AS ServerAddTime, 
                                sh.server_modified_time AS ServerModifiedTime, sh.user_journey_uid AS UserJourneyUID, sh.year_month AS YearMonth, 
                                sh.beat_history_uid AS BeatHistoryUID, sh.org_uid AS OrgUID, sh.route_uid AS RouteUID, sh.store_uid AS StoreUID, 
                                sh.is_planned AS IsPlanned, sh.serial_no AS SerialNo, sh.status AS Status, sh.visit_duration AS VisitDuration, 
                                sh.travel_time AS TravelTime, sh.planned_login_time AS PlannedLoginTime, sh.planned_logout_time AS PlannedLogoutTime, 
                                sh.login_time AS LoginTime, sh.logout_time AS LogoutTime, sh.no_of_visits AS NoOfVisits, 
                                sh.last_visit_date AS LastVisitDate, sh.is_skipped AS IsSkipped, sh.is_productive AS IsProductive, 
                                sh.is_green AS IsGreen, sh.target_value AS TargetValue, sh.target_volume AS TargetVolume, 
                                sh.target_lines AS TargetLines, sh.actual_value AS ActualValue, sh.actual_volume AS ActualVolume, 
                                sh.actual_lines AS ActualLines, sh.planned_time_spend_in_minutes AS PlannedTimeSpendInMinutes, 
                                sh.latitude AS Latitude, sh.longitude AS Longitude, sh.notes AS Notes, 0 AS SS 
                                FROM store_history AS sh where sh.ss In (1,2)
                                ";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        //StoreHistoryStats
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistoryStats>?> GetStoreHistoryStatsData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT 
                                shs.uid AS KeyUID,
                                shs.id AS Id,
                                shs.uid AS UID,
                                shs.created_by AS CreatedBy,
                                shs.created_time AS CreatedTime,
                                shs.modified_by AS ModifiedBy,
                                shs.modified_time AS ModifiedTime,
                                shs.server_add_time AS ServerAddTime,
                                shs.server_modified_time AS ServerModifiedTime,
                                shs.store_history_uid AS StoreHistoryUID,
                                shs.check_in_time AS CheckInTime,
                                shs.check_out_time AS CheckOutTime,
                                shs.total_time_in_min AS TotalTimeInMin,
                                shs.is_force_check_in AS IsForceCheckIn,
                                shs.latitude AS Latitude,
                                shs.longitude AS Longitude,
                                shs.ss AS SS
                            FROM 
                                store_history_stats AS shs
                            WHERE 
                                shs.ss IN (1, 2);";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IStoreHistoryStats>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //BeatHistory
        private async Task<List<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>?> GetBeatHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT bh.uid AS KeyUID, bh.id AS Id, bh.uid AS UID, bh.created_by AS CreatedBy,bh.org_uid as OrgUID, bh.created_time AS CreatedTime, 
                            bh.modified_by AS ModifiedBy, bh.modified_time AS ModifiedTime, bh.server_add_time AS ServerAddTime, 
                            bh.server_modified_time AS ServerModifiedTime, bh.user_journey_uid AS UserJourneyUID, bh.route_uid AS RouteUID, 
                            bh.start_time AS StartTime, bh.end_time AS EndTime, bh.job_position_uid AS JobPositionUID, bh.login_id AS LoginId, 
                            bh.visit_date AS VisitDate, bh.location_uid AS LocationUID, bh.planned_start_time AS PlannedStartTime, 
                            bh.planned_end_time AS PlannedEndTime, bh.planned_store_visits AS PlannedStoreVisits, bh.unplanned_store_visits AS UnPlannedStoreVisits,
                            bh.zero_sales_store_visits AS ZeroSalesStoreVisits, bh.msl_store_visits AS MSLStoreVisits, bh.skipped_store_visits 
                            AS SkippedStoreVisits, bh.actual_store_visits AS ActualStoreVisits, bh.coverage AS Coverage, bh.a_coverage AS ACoverage, 
                            bh.t_coverage AS TCoverage, bh.invoice_status AS InvoiceStatus, bh.notes AS Notes, bh.invoice_finalization_date AS
                            InvoiceFinalizationDate, bh.route_wh_org_uid AS RouteWHOrgUID, bh.cfd_time AS CFDTime, bh.has_audit_completed AS HasAuditCompleted,
                            bh.wh_stock_audit_uid AS WHStockAuditUID, 0 AS SS, bh.default_job_position_uid AS DefaultJobPositionUID, 
                            bh.user_journey_vehicle_uid AS UserJourneyVehicleUID,year_month AS YearMonth FROM beat_history bh where bh.ss In(1,2)
                           ";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.JourneyPlan.Model.Interfaces.IBeatHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //StoreActivityHistory
        private async Task<List<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityHistory>?> GetStoreActivityHistoryData(IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT sah.uid AS KeyUID, sah.uid AS UID, sah.store_history_uid AS StoreHistoryUID, sah.store_activity_uid AS StoreActivityUID, 
                              sah.serial_no AS SerialNo, sah.is_compulsory AS IsCompulsory, sah.is_locked AS IsLocked, sah.status AS Status, 0 AS SS,
                              sah.created_time AS CreatedTime, sah.modified_time AS ModifiedTime, sah.server_add_time AS ServerAddTime, sah.server_modified_time 
                              AS ServerModifiedTime  FROM store_activity_history sah where sah.ss In(1,2)";
                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.StoreActivity.Model.Interfaces.IStoreActivityHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //Address
        private async Task<List<Winit.Modules.Address.Model.Interfaces.IAddress>?> GetAddressData(IDbConnection? connection)
        {
            try
            {
                var query = $@"
                                SELECT 
                                    id AS Id, uid AS Uid, created_by AS CreatedBy, created_time AS CreatedTime,
                                    modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                                    server_modified_time AS ServerModifiedTime, type AS Type, name AS Name, line1 AS Line1,
                                    line2 AS Line2, line3 AS Line3, landmark AS Landmark, area AS Area, sub_area AS SubArea,
                                    zip_code AS ZipCode, city AS City, country_code AS CountryCode, region_code AS RegionCode,
                                    phone AS Phone, phone_extension AS PhoneExtension, mobile1 AS Mobile1, mobile2 AS Mobile2,
                                    email AS Email, fax AS Fax, latitude AS Latitude, longitude AS Longitude, altitude AS Altitude,
                                    linked_item_uid AS LinkedItemUid, linked_item_type AS LinkedItemType, status AS Status,
                                    state_code AS StateCode, territory_code AS TerritoryCode, pan AS Pan, aadhar AS Aadhar,
                                    ssn AS Ssn, is_editable AS IsEditable, is_default AS IsDefault, line4 AS Line4, info AS Info,
                                    depot AS Depot 
                                FROM address 
                                WHERE ss IN (1,2)";

                var parameters = new { };

                return await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region StoreCheck
        public async Task<List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckMaster>?> PrepareInsertUpdateData_StoreCheck()
        {
            List<StoreCheckMaster> storeCheckMasterList = null;
            List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView>? storeCheckHistoryList = null;
            List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory>? storeCheckItemHistoryList = null;
            List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory>? storeCheckItemExpiryDREHistoryList = null;
            List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty>? storeCheckItemUomQtyList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.StoreCheckHistory, 1000);
                if (uIDs == null)
                {
                    return null;
                }
                List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>? storeCheckHistories = await GetStoreCheckHistoryData(uIDs, connection);
                if (storeCheckHistories == null || storeCheckHistories.Count == 0)
                {
                    return null;
                }
                storeCheckHistoryList = storeCheckHistories.Cast<Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView>().ToList();

                List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistory>? storeCheckItemHistories = await GetStoreCheckItemHistoryData(uIDs, connection);
                if (storeCheckItemHistories != null && storeCheckItemHistories.Count > 0)
                {
                    storeCheckItemHistoryList = storeCheckItemHistories.Cast<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory>().ToList();
                }

                List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>? storeCheckItemExpiryDREHistories = await GetStoreCheckItemExpiryDREHistory(uIDs, connection);
                if (storeCheckItemExpiryDREHistories != null && storeCheckItemExpiryDREHistories.Count > 0)
                {
                    storeCheckItemExpiryDREHistoryList = storeCheckItemExpiryDREHistories.Cast<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory>().ToList();
                }
                List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemUomQty>? storeCheckItemUomQties = await GetStoreCheckItemUomQty(uIDs, connection);
                if (storeCheckItemUomQties != null && storeCheckItemUomQties.Count > 0)
                {
                    storeCheckItemUomQtyList = storeCheckItemUomQties.Cast<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty>().ToList();
                }
            }

            storeCheckMasterList = new List<StoreCheckMaster>();
            foreach (Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView storeCheckHistory in storeCheckHistoryList)
            {
                StoreCheckMaster storeCheckMaster = new StoreCheckMaster();
                storeCheckMaster.RequestUIDDictionary = new Dictionary<string, List<string>>();
                // storeCheckHistory
                storeCheckMaster.StoreCheckHistory = (Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView)storeCheckHistory;
                storeCheckMaster.RequestUIDDictionary[DbTableName.StoreCheckHistory] = new List<string> { storeCheckMaster.StoreCheckHistory.UID };
                // StoreCheckItemHistory
                if (storeCheckItemHistoryList != null && storeCheckItemHistoryList.Count > 0)
                {
                    storeCheckMaster.StoreCheckHistoryList = storeCheckItemHistoryList.Where(e => e.KeyUID == storeCheckHistory.UID).ToList();
                    if (storeCheckMaster.StoreCheckHistoryList != null && storeCheckMaster.StoreCheckHistoryList.Count > 0)
                    {
                        storeCheckMaster.RequestUIDDictionary[DbTableName.StoreCheckItemHistory] = storeCheckMaster.StoreCheckHistoryList.Select(e => e.UID).ToList();
                    }
                }
                // StoreCheckItemExpiryDREHistoriesList
                if (storeCheckItemExpiryDREHistoryList != null && storeCheckItemExpiryDREHistoryList.Count > 0)
                {
                    storeCheckMaster.StoreCheckItemExpiryDREHistoriesList = storeCheckItemExpiryDREHistoryList.Where(e => e.KeyUID == storeCheckHistory.UID).ToList();
                    if (storeCheckMaster.StoreCheckItemExpiryDREHistoriesList != null && storeCheckMaster.StoreCheckItemExpiryDREHistoriesList.Count > 0)
                    {
                        storeCheckMaster.RequestUIDDictionary[DbTableName.StoreCheckItemExpiryDerHistory] = storeCheckMaster.StoreCheckItemExpiryDREHistoriesList.Select(e => e.UID).ToList();
                    }
                }
                //StoreCheckItemUomQtyList
                if (storeCheckItemUomQtyList != null && storeCheckItemUomQtyList.Count > 0)
                {
                    storeCheckMaster.StoreCheckItemUomQtyList = storeCheckItemUomQtyList.Where(e => e.KeyUID == storeCheckHistory.UID).ToList();
                    if (storeCheckMaster.StoreCheckItemExpiryDREHistoriesList != null && storeCheckMaster.StoreCheckItemExpiryDREHistoriesList.Count > 0)
                    {
                        storeCheckMaster.RequestUIDDictionary[DbTableName.StoreCheckItemUOMQty] = storeCheckMaster.StoreCheckItemExpiryDREHistoriesList.Select(e => e.UID).ToList();
                    }
                }

                storeCheckMasterList.Add(storeCheckMaster);
            }
            return storeCheckMasterList;
        }
        private async Task<List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>?> GetStoreCheckHistoryData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT sch.id AS Id,sch.uid as KeyUID, sch.uid AS Uid, sch.beat_history_uid AS BeatHistoryUid, sch.store_history_uid AS StoreHistoryUid,
                            sch.store_asset_uid AS StoreAssetUid, sch.org_uid AS OrgUid, sch.store_check_date AS StoreCheckDate, sch.store_uid AS StoreUid,
                            sch.store_check_config_uid AS StoreCheckConfigUid, sch.sku_group_type_uid AS SkuGroupTypeUid, sch.sku_group_uid AS SkuGroupUid, 
                            sch.group_by_name AS GroupByName, sch.status AS Status, sch.level AS Level, sch.is_last_level AS IsLastLevel, 0 AS SS,
                            sch.created_time AS CreatedTime, sch.modified_time AS ModifiedTime, sch.server_add_time AS ServerAddTime, 
                            sch.server_modified_time AS ServerModifiedTime FROM store_check_history sch WHERE sch.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistory>?> GetStoreCheckItemHistoryData(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT scih.id AS Id,sch.uid as KeyUID, scih.uid AS Uid, scih.store_check_history_uid AS StoreCheckHistoryUid, scih.group_by_code AS GroupByCode,
                              scih.group_by_value AS GroupByValue, scih.sku_uid AS SkuUid, scih.sku_code AS SkuCode, scih.uom AS Uom, scih.suggested_qty AS SuggestedQty,
                              scih.store_qty AS StoreQty, scih.backstore_qty AS BackstoreQty, scih.to_fill_qty AS ToFillQty, scih.is_available AS IsAvailable, 
                              scih.is_dre_selected AS IsDreSelected, 0 AS SS, scih.created_time AS CreatedTime, scih.modified_time AS ModifiedTime,
                              scih.server_add_time AS ServerAddTime, scih.server_modified_time AS ServerModifiedTime
                              FROM store_check_history sch
                              JOIN store_check_item_history scih ON scih.store_check_history_uid = sch.uid
                              WHERE sch.uid IN @UIDs;";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>?> GetStoreCheckItemExpiryDREHistory(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT sciedh.id AS Id,sch.uid as KeyUID, sciedh.uid AS Uid, sciedh.store_check_item_history_uid AS StoreCheckItemHistoryUid, 
                            sciedh.stock_type AS StockType, sciedh.stock_sub_type AS StockSubType, sciedh.batch_no AS BatchNo, 
                            sciedh.expiry_date AS ExpiryDate, sciedh.reason AS Reason, sciedh.qty AS Qty, sciedh.uom AS Uom, sciedh.notes AS Notes, 
                            0 AS SS, sciedh.created_time AS CreatedTime, sciedh.modified_time AS ModifiedTime, 
                            sciedh.server_add_time AS ServerAddTime, sciedh.server_modified_time AS ServerModifiedTime
                            FROM store_check_history sch 
                            JOIN store_check_item_history scih ON sch.store_history_uid = sch.uid 
                            JOIN store_check_item_expiry_der_history sciedh ON sciedh.store_check_item_history_uid = scih.uid 
                            WHERE sch.uid IN @UIDs;";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private async Task<List<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemUomQty>?> GetStoreCheckItemUomQty(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT sciuq.id AS Id, sciuq.uid AS Uid,sch.uid as KeyUID, sciuq.store_check_item_history_uid AS StoreCheckItemHistoryUid, 
                               sciuq.uom AS Uom, sciuq.base_uom AS BaseUom, sciuq.uom_multiplier AS UomMultiplier, sciuq.store_qty AS StoreQty, 
                               sciuq.store_qty_bu AS StoreQtyBu, sciuq.back_store_qty AS BackStoreQty, sciuq.back_store_qty_bu AS BackStoreQtyBu, 
                               0 AS SS, sciuq.created_time AS CreatedTime, sciuq.modified_time AS ModifiedTime, 
                               sciuq.server_add_time AS ServerAddTime, sciuq.server_modified_time AS ServerModifiedTime
                               FROM store_check_history sch 
                               JOIN store_check_item_history scih ON sch.uid = scih.store_check_history_uid 
                               JOIN store_check_item_uom_qty sciuq ON sciuq.store_check_item_history_uid = scih.uid WHERE sch.uid IN @UIDs;";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemUomQty>(query, parameters, null, connection);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #endregion



        #region FileSys
        public async Task<List<Winit.Modules.FileSys.Model.Classes.FileSys>?> PrepareInsertUpdateData_FileSys()
        {
            List<Winit.Modules.FileSys.Model.Classes.FileSys>? fileSysList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.FileSys, 1000);
                if (uIDs == null)
                {
                    return null;
                }

                List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>? fileSys = await GetFileSysList(uIDs, connection);
                if (fileSys == null || fileSys.Count == 0)
                {
                    return null;
                }

                fileSysList = fileSys.Cast<Winit.Modules.FileSys.Model.Classes.FileSys>().ToList();
            }

            foreach (Winit.Modules.FileSys.Model.Classes.FileSys file in fileSysList)
            {
                if (file.RequestUIDDictionary == null)
                {
                    file.RequestUIDDictionary = new Dictionary<string, List<string>>();
                }

                file.RequestUIDDictionary[DbTableName.FileSys] = new List<string> { file.UID };
            }

            return fileSysList;
        }

        private async Task<List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>?> GetFileSysList(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT fs.uid AS KeyUID, fs.id AS Id, fs.uid AS UID, fs.created_by AS CreatedBy, fs.created_time AS CreatedTime, 
	                            fs.modified_by AS ModifiedBy, fs.modified_time AS ModifiedTime, 
	                            fs.server_add_time AS ServerAddTime, fs.server_modified_time AS ServerModifiedTime,
	                            fs.linked_item_type AS LinkedItemType, fs.linked_item_uid AS LinkedItemUID, 
	                            fs.file_sys_type AS FileSysType, fs.file_type AS FileType, 
	                            fs.parent_file_sys_uid AS ParentFileSysUID, fs.is_directory AS IsDirectory, 
	                            fs.file_name AS FileName, fs.display_name AS DisplayName, fs.file_size AS FileSize, 
	                            fs.relative_path AS RelativePath, fs.latitude AS Latitude, 
	                            fs.longitude AS Longitude, fs.created_by_job_position_uid AS CreatedByJobPositionUID, 
	                            fs.created_by_emp_uid AS CreatedByEmpUID, fs.is_default AS IsDefault,
	                            0 AS SS FROM file_sys fs  WHERE fs.uid IN @UIDs";
                var parameters = new { UIDs = uIDs };

                return await ExecuteQueryAsync<Winit.Modules.FileSys.Model.Interfaces.IFileSys>(query, parameters, null, connection);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Address

        public async Task<List<Winit.Modules.Address.Model.Classes.Address>?> PrepareInsertUpdateData_Address()
        {
            List<Winit.Modules.Address.Model.Classes.Address>? addressModelList = null;

            using (IDbConnection connection = SqliteConnection())
            {
                List<string> uIDs = await GetUIDsToPushByTable(DbTableName.Address, 1000);
                if (uIDs == null)
                {
                    return null;
                }

                List<Winit.Modules.Address.Model.Interfaces.IAddress>? addressModels = await GetAddressList(uIDs, connection);
                if (addressModels == null || addressModels.Count == 0)
                {
                    return null;
                }

                addressModelList = addressModels.Cast<Winit.Modules.Address.Model.Classes.Address>().ToList();
            }

            foreach (Winit.Modules.Address.Model.Classes.Address addressModel in addressModelList)
            {
                if (addressModel.RequestUIDDictionary == null)
                {
                    addressModel.RequestUIDDictionary = new Dictionary<string, List<string>>();
                }

                addressModel.RequestUIDDictionary[DbTableName.Address] = new List<string> { addressModel.UID };
            }

            return addressModelList;
        }
        private async Task<List<Winit.Modules.Address.Model.Interfaces.IAddress>?> GetAddressList(List<string> uIDs, IDbConnection? connection)
        {
            try
            {
                var query = $@"SELECT 
                        a.uid AS UID, a.id AS Id, a.created_time AS CreatedTime, a.modified_time AS ModifiedTime,
                        a.server_add_time AS ServerAddTime, a.server_modified_time AS ServerModifiedTime,
                        a.org_unit_uid AS OrgUID, a.line1 AS AddressLine1, a.line2 AS AddressLine2,
                        a.city AS City, a.state AS State, a.country_code AS Country, a.zip_code AS Pincode,
                        a.latitude AS Latitude, a.longitude AS Longitude, a.created_by AS CreatedBy, 
                        a.modified_by AS ModifiedBy
                    FROM address a 
                    WHERE a.uid IN @UIDs";

                var parameters = new { UIDs = uIDs };
                return await ExecuteQueryAsync<Winit.Modules.Address.Model.Interfaces.IAddress>(query, parameters, null, connection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
