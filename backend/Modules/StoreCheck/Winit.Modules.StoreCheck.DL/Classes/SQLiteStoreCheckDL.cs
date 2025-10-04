using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.DL.Interfaces;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.DL.Classes
{
    public class SQLiteStoreCheckDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IStoreCheckDL
    {

        public SQLiteStoreCheckDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<int> CUDStoreCheck(/*Winit.Modules.StockAudit.Model.Classes.WHStockAuditRequestTemplateModel storeCheckModel*/)
        {
            //int count = 0;

            //try
            //{
            //    using (var connection = SqliteConnection())
            //    {
            //        await connection.OpenAsync();

            //        using (var transaction = connection.BeginTransaction())
            //        {
            //            try
            //            {

            //                if (storeCheckModel != null && storeCheckModel.WHStockAuditItemView != null)
            //                {
            //                    count += await CUDWHStockAudit(storeCheckModel.WHStockAuditItemView, connection, transaction);

            //                }
            //                if (storeCheckModel != null && storeCheckModel.WHStockAuditDetailsItemView != null)
            //                {
            //                    count += await CUDWHStockAuditLine(storeCheckModel.WHStockAuditDetailsItemView, connection, transaction);
            //                }

            //                transaction.Commit();

            //            }
            //            catch
            //            {
            //                transaction.Rollback();
            //                throw;
            //            }
            //            finally { connection.Close(); }
            //        }
            //    }
            //}
            //catch
            //{
            //    throw;
            //}
            //return count;

            return 0; 
        }
        public async Task<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView> SelectStoreCheckHistoryData(string beatHistoryUID, string storeHistoryUID)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                SCH.id AS Id,
                SCH.uid AS UID,
                SCH.beat_history_uid AS BeatHistoryUID,
                SCH.store_history_uid AS StoreHistoryUID,
                SCH.store_asset_uid AS StoreAssetUID,
                SCH.org_uid AS OrgUID,
                SCH.store_check_date AS StoreCheckDate,
                SCH.store_uid AS StoreUID,
                SCH.store_uid AS EmpUID,
                SCH.store_uid AS RouteUID,
                SCH.store_check_config_uid AS StoreCheckConfigUID,
                SCH.sku_group_type_uid AS SkuGroupTypeUID,
                SCH.sku_group_uid AS SkuGroupUid,
                SCH.group_by_name AS GroupByName,
                SCH.status AS Status,
                SCH.level AS Level,
                SCH.is_last_level AS IsLastLevel,
                SCH.ss AS SS,
                SCH.created_time AS CreatedTime,
                SCH.modified_time AS ModifiedTime,
                SCH.server_add_time AS ServerAddTime,
                SCH.server_modified_time AS ServerModifiedTime
                FROM store_check_history SCH
                WHERE SCH.beat_history_uid = @BeatHistoryUID AND SCH.store_history_uid = @StoreHistoryUID;");

                var parameters = new Dictionary<string, object>()
                {
                    { "@BeatHistoryUID", beatHistoryUID },
                    { "@StoreHistoryUID", storeHistoryUID }
                };


                var type = _serviceProvider.GetRequiredService<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>().GetType();

                Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView storeCheckHistoryView = await ExecuteSingleAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>(sql.ToString(), parameters, type);
                return storeCheckHistoryView;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while selecting store check history data.", ex);
            }
        }

        public async Task<IStoreCheckItemUomQty> SelectStoreCheckItemUomQty(string storeCheckItemHistoryUID,string uom)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
            SCIUQ.id AS Id,
            SCIUQ.uid AS UID,
            SCIUQ.store_check_item_history_uid AS StoreCheckItemHistoryUid,
            SCIUQ.uom AS UOM,
            SCIUQ.base_uom AS BaseUom,
            SCIUQ.uom_multiplier AS UomMultiplier,
            SCIUQ.store_qty AS StoreQty,
            SCIUQ.store_qty_bu AS StoreQtyBu,
            SCIUQ.back_store_qty AS BackStoreQty,
            SCIUQ.back_store_qty_bu AS BackStoreQtyBu,
            SCIUQ.ss AS Ss,
            SCIUQ.created_time AS CreatedTime,
            SCIUQ.modified_time AS ModifiedTime,
            SCIUQ.server_add_time AS ServerAddTime,
            SCIUQ.server_modified_time AS ServerModifiedTime
        FROM 
            store_check_item_uom_qty SCIUQ
            WHERE SCIUQ.store_check_item_history_uid = @StoreCheckItemHistoryUid AND SCIUQ.uom = @UOM;");

                var parameters = new Dictionary<string, object>()
                    {
                        { "@StoreCheckItemHistoryUid", storeCheckItemHistoryUID },
                        { "@UOM", uom }
                    };

                var type = _serviceProvider.GetRequiredService<IStoreCheckItemUomQty>().GetType();

                IStoreCheckItemUomQty storeCheckItemUomQty = await ExecuteSingleAsync<IStoreCheckItemUomQty>(sql.ToString(), parameters, type);
                return storeCheckItemUomQty;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while selecting store check history data.", ex);
            }
        }

        public async Task<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>> SelectStoreCheckItemExpiryDREHistory(string storeCheckItemHistoryUID)
        {
            try
            {
                var sql = new StringBuilder(@"
                        SELECT 
                SCIED.id AS Id,
                SCIED.uid AS Uid,
                SCIED.store_check_item_history_uid AS StoreCheckItemHistoryUid,
                SCIED.stock_type AS StockType,
                SCIED.stock_sub_type AS StockSubType,
                SCIED.batch_no AS BatchNo,
                SCIED.expiry_date AS ExpiryDate,
                SCIED.reason AS Reason,
                SCIED.qty AS Qty,
                SCIED.uom AS Uom,
                SCIED.modified_time AS ModifiedTime,
                SCIED.server_modified_time AS ServerModifiedTime
                        FROM store_check_item_expiry_der_history SCIED
                        WHERE SCIED.store_check_item_history_uid = @storeCheckItemHistoryUID;
                    ");

              
                var parameters = new Dictionary<string, object>()
                    {
                        { "@storeCheckItemHistoryUID", storeCheckItemHistoryUID }
                    };
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreCheckItemExpiryDREHistory>().GetType();
               // var type = _serviceProvider.GetRequiredService<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>>().GetType();

                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory> storeCheckItemExpiryDREHistory = await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>(sql.ToString(), parameters, type);
                return storeCheckItemExpiryDREHistory;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while selecting store check history data.", ex);
            }
        }

        public async Task<PagedResponse<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>> SelectStoreCheckItemHistoryData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string storeCheckHistoryUID)
        {
            try
            {
                var sql = new StringBuilder(@"
       SELECT 
        SCIH.id AS Id,
        SCIH.uid AS UID,
        SCIH.store_check_history_uid AS StoreCheckHistoryUID,
        SCIH.group_by_code AS GroupByCode,
        SCIH.group_by_value AS GroupByValue,
        SCIH.sku_uid AS SkuUid,
        SCIH.sku_code AS SkuCode,
        SCIH.uom AS Uom,
        SCIH.suggested_qty AS SuggestedQty,
        SCIH.store_qty AS StoreQty,
        SCIH.backstore_qty AS BackStoreQty,
        SCIH.to_fill_qty AS ToFillQty,
        SCIH.is_available AS IsAvailable,
        SCIH.is_dre_selected AS IsDreSelected,
        SCIH.modified_time AS ModifiedTime,
        SCIH.server_modified_time AS ServerModifiedTime,
        FSC.relative_path || '/' || FSC.file_name AS SKUImageURL,
        CASE WHEN FO.sku_uid IS NOT NULL THEN 1 ELSE 0 END AS IsFocusSKU
        FROM 
        store_check_item_history SCIH
        LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKU' AND FSC.linked_item_uid = SCIH.sku_uid
                                                AND FSC.file_sys_type = 'SKU'
        LEFT JOIN (
                SELECT DISTINCT SCGI.sku_uid 
                FROM sku_class_group SCG
                INNER JOIN sku_class_group_items SCGI ON SCGI.sku_class_group_uid = SCG.uid
                AND SCG.sku_class_uid = 'FOCUS'
            ) FO ON FO.sku_uid = SCIH.sku_uid
                             WHERE 
                                SCIH.store_check_history_uid = @storeCheckHistoryUID;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt  
                                                   FROM store_check_history SCIH
                                                   
                                                   WHERE 
                                                    SCIH.StoreCheckHistoryUid = @storeCheckHistoryUID;");
                }
                var parameters = new Dictionary<string, object>()
                {
                    { "@storeCheckHistoryUID", storeCheckHistoryUID }
                };
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IStoreCheckItemHistoryViewList>().GetType();

                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList> StoreCheckItemHistoryViewList = await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList> pagedResponse = new PagedResponse<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>
                {
                    PagedData = StoreCheckItemHistoryViewList,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<int> InsertorUpdate_StoreCheck(StoreCheckMaster storeCheckMaster)
        {
        throw new NotImplementedException();
        }


            #region Insert Data/Update Data

            public async Task<int> UpdateStoreCheck(Winit.Modules.StoreCheck.Model.Classes.StoreCheckModel storeCheckModel)
        {

            int count = 0;

            try
            {
                using (var connection = SqliteConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            if (storeCheckModel != null && storeCheckModel.StoreCheckHistoryView != null)
                            {
                                count += await CreateUpdateStoreCheckHistory(storeCheckModel.StoreCheckHistoryView, connection, transaction);

                            }
                            if (storeCheckModel != null && storeCheckModel.StoreCheckItemHistoryViewLists != null)
                            {
                                count += await CreateUpdateStoreCheckItemHistory(storeCheckModel.StoreCheckItemHistoryViewLists, connection, transaction);

                            }
                           

                            transaction.Commit();

                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally { connection.Close(); }
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;

        }


            public async Task<int> CreateUpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckModel storeCheckModel)
            {
            int count = 0;

            try
            {
                using (var connection = SqliteConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            if (storeCheckModel != null && storeCheckModel.StoreCheckHistoryView != null)
                            {
                                count += await CreateUpdateStoreCheckHistory(storeCheckModel.StoreCheckHistoryView, connection, transaction);

                            }
                            if (storeCheckModel != null && storeCheckModel.StoreCheckItemHistoryViewLists != null)
                            {
                                count += await CreateUpdateStoreCheckItemHistory(storeCheckModel.StoreCheckItemHistoryViewLists, connection, transaction);

                            }

                            transaction.Commit();

                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                        finally { connection.Close(); }
                    }
                }
            }
            catch
            {
                throw;
            }
            return count;
        }

        public async Task<int> CreateUpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;

            try
            {
                bool exists = false;
                var existingRec = await SelectStoreCheckHistoryByUID(storeCheckHistoryView.UID);


                switch (storeCheckHistoryView.ActionType)
                {
                    case Shared.Models.Enums.ActionType.Add:
                        if (existingRec != null)
                        {
                            exists = (existingRec.UID == storeCheckHistoryView.UID);
                        }

                        count += exists ? await UpdateStoreCheckHistory(storeCheckHistoryView, connection, transaction) : await CreateStoreCheckHistory(storeCheckHistoryView, connection, transaction);
                        break;

                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView> SelectStoreCheckHistoryByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"@UID",  UID}
            };
            var sql = @"SELECT * FROM store_check_history WHERE uid  = @UID";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>().GetType();
            Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView whStockRequestDetails = await ExecuteSingleAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>(sql, parameters, type);
            return whStockRequestDetails;
        }

        private async Task<int> UpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"UPDATE store_check_history
                        SET status = @Status, 
                        modified_time = @ModifiedTime, 
                        server_modified_time  = @ServerModifiedTime
                        WHERE uid = @Uid;";
            var Parameters = new Dictionary<string, object>
            {
                { "@Uid", storeCheckHistoryView.UID },
                { "@Status", storeCheckHistoryView.Status },
                { "@ModifiedTime", storeCheckHistoryView.ModifiedTime },
                { "@ServerModifiedTime", storeCheckHistoryView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        }

        public async Task<int> CreateUpdateStoreCheckItemHistory(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistoryViewList> storeCheckItemHistoryViewLists,
           IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            if (storeCheckItemHistoryViewLists == null || storeCheckItemHistoryViewLists.Count == 0)
            {
                return count;
            }
            List<string> uidList = storeCheckItemHistoryViewLists.Select(po => po.Uid).ToList();
            // List<string> deletedUidList = wHStockRequestLines.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<IStoreCheckItemHistoryViewList> existingRec = await SelectStoreCheckItemHistoryByUID(uidList);

                foreach (StoreCheckItemHistoryViewList storeCheckItemHistoryViewList in storeCheckItemHistoryViewLists)
                {
                    switch (storeCheckItemHistoryViewList.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            bool exists = existingRec.Any(po => po.Uid == storeCheckItemHistoryViewList.Uid);
                            count += exists ?
                                await UpdateStoreCheckItemHistory(storeCheckItemHistoryViewList, connection, transaction) :
                                await CreateStoreCheckItemHistory(storeCheckItemHistoryViewList, connection, transaction);
                                if(storeCheckItemHistoryViewList.IsDRESelected && storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory != null && storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory.Count >0)
                                {
                                    await CreateUpdateStoreCheckExpireDreHistory(storeCheckItemHistoryViewList, connection, transaction);
                                }
                                if (storeCheckItemHistoryViewList.StoreCheckBaseQtyDetails.StoreQty != null || storeCheckItemHistoryViewList.StoreCheckBaseQtyDetails.BackStoreQty != null)
                                {
                                    await CreateUpdateStoreCheckUomQty(storeCheckItemHistoryViewList.StoreCheckBaseQtyDetails, storeCheckItemHistoryViewList.StoreCheckBaseQtyDetails.UOM, connection, transaction);
                                }
                                if (storeCheckItemHistoryViewList.StoreCheckOuterQtyDetails.StoreQty != null || storeCheckItemHistoryViewList.StoreCheckOuterQtyDetails.BackStoreQty != null)
                                {
                                    await CreateUpdateStoreCheckUomQty(storeCheckItemHistoryViewList.StoreCheckOuterQtyDetails, storeCheckItemHistoryViewList.StoreCheckOuterQtyDetails.UOM, connection, transaction);
                                }

                            break;

                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        private async Task<int> UpdateStoreCheckItemHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistoryViewList storeCheckItemHistoryViewList,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {

            var Query = @"UPDATE store_check_item_history
                        SET 
                            backstore_qty=@BackStoreQty,
                            store_qty=@StoreQty,
                           suggested_qty = @SuggestedQty,
                            to_fill_qty =@ToFillQty,
                            is_available =@IsAvailable,
                           modified_time = @ModifiedTime,
                           is_dre_selected=@IsDRESelected,
                           server_modified_time = @ServerModifiedTime
                          WHERE uid  = @Uid;";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@Uid", storeCheckItemHistoryViewList.Uid },
                           { "@StoreQty", storeCheckItemHistoryViewList.StoreQty },
                           { "@BackStoreQty", storeCheckItemHistoryViewList.BackStoreQty },
                           { "@SuggestedQty", storeCheckItemHistoryViewList.SuggestedQty },
                           { "@ToFillQty", storeCheckItemHistoryViewList.ToFillQty },
                           { "@IsAvailable", storeCheckItemHistoryViewList.IsAvailable },
                           { "@ModifiedTime", storeCheckItemHistoryViewList.ModifiedTime },
                           { "@IsDRESelected", storeCheckItemHistoryViewList.IsDRESelected },
                           { "@ServerModifiedTime", storeCheckItemHistoryViewList.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        }

        private async Task<int> CreateStoreCheckItemHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistoryViewList storeCheckItemHistoryViewList,
           IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
            {
                var Query = @"INSERT INTO store_check_item_history (id,uid,store_check_history_uid,group_by_code ,group_by_value, 
                                 sku_uid,sku_code,uom,suggested_qty,store_qty,backstore_qty,to_fill_qty,is_available,is_dre_selected,ss ,created_time,
                                 modified_time ,server_add_time ,server_modified_time) VALUES (@Id,@Uid, @StoreCheckHistoryUid, @GroupByCode, 
                            @GroupByValue, @SKUUID,@ItemCode,@UOM,@SuggestedQty,@StoreQty,@BackStoreQty,@ToFillQty, @IsAvailable,@IsDRESelected, @SS, @CreatedTime, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime);
";
                var Parameters = new Dictionary<string, object>
                        {
                 
                           { "@Id", storeCheckItemHistoryViewList.Id },
                           { "@Uid", storeCheckItemHistoryViewList.Uid },
                           { "@StoreCheckHistoryUid", storeCheckItemHistoryViewList.StoreCheckHistoryUid },
                           { "@GroupByCode", storeCheckItemHistoryViewList.GroupByCode },
                           { "@GroupByValue", storeCheckItemHistoryViewList.GroupByValue },
                           { "@SKUUID", storeCheckItemHistoryViewList.SKUUID },
                           { "@ItemCode", storeCheckItemHistoryViewList.ItemCode },
                           { "@UOM", storeCheckItemHistoryViewList.UOM },
                           { "@SuggestedQty", storeCheckItemHistoryViewList.SuggestedQty },
                           { "@StoreQty", storeCheckItemHistoryViewList.StoreQty },
                           { "@BackStoreQty", storeCheckItemHistoryViewList.BackStoreQty },
                           { "@ToFillQty", storeCheckItemHistoryViewList.ToFillQty },
                           { "@IsAvailable", storeCheckItemHistoryViewList.IsAvailable },
                           { "@IsDRESelected", storeCheckItemHistoryViewList.IsDRESelected },
                           { "@SS", storeCheckItemHistoryViewList.SS },
                           { "@CreatedTime", storeCheckItemHistoryViewList.CreatedTime },
                           { "@ModifiedTime", storeCheckItemHistoryViewList.ModifiedTime },
                           { "@ServerAddTime", storeCheckItemHistoryViewList.ServerAddTime },
                           { "@ServerModifiedTime", storeCheckItemHistoryViewList.ServerModifiedTime },

            };
                return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
            }
            catch (Exception Ex)
            {
                throw;
            }

        }

        private async Task<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>> SelectStoreCheckItemHistoryByUID(List<string> UIDs)
        {
            UIDs = UIDs.Select(code => code.Trim()).ToList();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            for (int i = 0; i < UIDs.Count; i++)
            {
                parameters.Add($"@uid{i}", UIDs[i]);
            }
            var sql = $"SELECT * FROM store_check_item_history WHERE uid  IN  ({string.Join(",", UIDs.Select((_, i) => $"@uid{i}"))})";
            Type type = _serviceProvider.GetRequiredService<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>().GetType();
            IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList> WHStockRequestLineDetails = await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>(sql, parameters, type);
            return WHStockRequestLineDetails;
        }

        private async Task<int> CreateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"INSERT INTO store_check_history (id,uid,beat_history_uid , store_history_uid , store_asset_uid , org_uid , 
                        store_check_date , store_uid ,emp_uid,route_uid, store_check_config_uid , sku_group_type_uid , sku_group_uid , group_by_name , status , level , 
                        is_last_level , ss ,created_time , modified_time , server_add_time , server_modified_time)
                        VALUES (@Id,@Uid, @BeatHistoryUid, @StoreHistoryUid, @StoreAssetUid, @OrgUid, @StoreCheckDate, @StoreUid,@EmpUID,@RouteUID,
                        @StoreCheckConfigUid, @SkuGroupTypeUid, @SkuGroupUid, @GroupByName, @Status, @Level, @IsLastLevel, @SS, 
                        @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
            var Parameters = new Dictionary<string, object>
                        {
                           { "@Id", storeCheckHistoryView.Id },
                           { "@Uid", storeCheckHistoryView.UID },
                           { "@BeatHistoryUid", storeCheckHistoryView.BeatHistoryUID },
                           { "@StoreHistoryUid", storeCheckHistoryView.StoreHistoryUID },
                           { "@StoreAssetUid", storeCheckHistoryView.StoreAssetUID },
                           { "@OrgUid", storeCheckHistoryView.OrgUID },
                           { "@StoreCheckDate", storeCheckHistoryView.StoreCheckDate },
                           { "@StoreUid", storeCheckHistoryView.StoreUID },
                           { "@EmpUID", storeCheckHistoryView.EmpUID },
                           { "@RouteUID", storeCheckHistoryView.RouteUID },
                           { "@StoreCheckConfigUid", storeCheckHistoryView.StoreCheckConfigUID },
                           { "@SkuGroupTypeUid", storeCheckHistoryView.SkuGroupTypeUID },
                           { "@SkuGroupUid", storeCheckHistoryView.SkuGroupUID },
                           { "@GroupByName", storeCheckHistoryView.GroupByName },
                           { "@Status", storeCheckHistoryView.Status },
                           { "@Level", storeCheckHistoryView.Level },
                           { "@IsLastLevel", storeCheckHistoryView.IsLastLevel },
                           { "@SS", storeCheckHistoryView.SS },
                           { "@CreatedTime", storeCheckHistoryView.CreatedTime },
                           { "@ModifiedTime", storeCheckHistoryView.ModifiedTime },
                           { "@ServerAddTime", storeCheckHistoryView.ServerAddTime },
                           { "@ServerModifiedTime", storeCheckHistoryView.ServerModifiedTime },
                        };
            return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        }

        //public async Task<int> CreateUpdateStoreCheckUomQty(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty storeCheckItemUomQty)
        //{
        //    int count = 0;

        //    try
        //    {
        //        using (var connection = SqliteConnection())
        //        {
        //            await connection.OpenAsync();

        //            using (var transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {

        //                    bool exists = false;
        //                    var existingRec = await SelectStoreCheckHistoryByUID(storeCheckItemUomQty.UID);


        //                    switch (storeCheckItemUomQty.ActionType)
        //                    {
        //                        case Shared.Models.Enums.ActionType.Add:
        //                            if (existingRec != null)
        //                            {
        //                                exists = (existingRec.Uid == storeCheckItemUomQty.UID);
        //                            }

        //                            count += exists ? await UpdateStoreChecUomQty(storeCheckItemUomQty, connection, transaction) : await CreateStoreChecUomQty(storeCheckItemUomQty, connection, transaction);
        //                            break;

        //                    }

        //                    transaction.Commit();

        //                }
        //                catch
        //                {
        //                    transaction.Rollback();
        //                    throw;
        //                }
        //                finally { connection.Close(); }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    return count;
        //}
        private async Task<int> UpdateStoreChecUomQty(IStoreCheckItemUomQty storeCheckItemUomQty,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"UPDATE store_check_item_uom_qty
             SET 
             store_qty = @StoreQty,
             store_qty_bu = @StoreQtyBu,
             back_store_qty = @BackStoreQty,
             back_store_qty_bu = @BackStoreQtyBu,
             modified_time = @ModifiedTime,
             server_modified_time = @ServerModifiedTime
             WHERE uid = @UID;";

            var parameters = new Dictionary<string, object>
{
   
    { "@StoreQty", storeCheckItemUomQty.StoreQty },
    { "@StoreQtyBu", storeCheckItemUomQty.StoreQtyBu },
    { "@BackStoreQty", storeCheckItemUomQty.BackStoreQty },
    { "@BackStoreQtyBu", storeCheckItemUomQty.BackStoreQtyBu },
    { "@ModifiedTime", storeCheckItemUomQty.ModifiedTime },
    { "@ServerModifiedTime", storeCheckItemUomQty.ServerModifiedTime },
                 { "@UID", storeCheckItemUomQty.UID }
};

            return await ExecuteNonQueryAsync(Query, parameters, connection, transaction);
        }

        private async Task<int> CreateStoreChecUomQty(IStoreCheckItemUomQty storeCheckItemUomQty,
     IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var query = @"INSERT INTO store_check_item_uom_qty 
                 (id,uid, store_check_item_history_uid, uom, base_uom, uom_multiplier, 
                 store_qty, store_qty_bu, back_store_qty, back_store_qty_bu, ss, 
                 created_time, modified_time, server_add_time, server_modified_time)
                 VALUES 
                 (@Id,@UID, @StoreCheckItemHistoryUid, @UOM, @BaseUom, @UomMultiplier, 
                 @StoreQty, @StoreQtyBu, @BackStoreQty, @BackStoreQtyBu, @Ss, 
                 @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

            var parameters = new Dictionary<string, object>
    {
         { "@Id", storeCheckItemUomQty.UID },
         { "@UID", storeCheckItemUomQty.UID },
        { "@StoreCheckItemHistoryUid", storeCheckItemUomQty.StoreCheckItemHistoryUID },
        { "@UOM", storeCheckItemUomQty.UOM },
        { "@BaseUom", storeCheckItemUomQty.BaseUom },
        { "@UomMultiplier", storeCheckItemUomQty.UomMultiplier },
        { "@StoreQty", storeCheckItemUomQty.StoreQty },
        { "@StoreQtyBu", storeCheckItemUomQty.StoreQtyBu },
        { "@BackStoreQty", storeCheckItemUomQty.BackStoreQty },
        { "@BackStoreQtyBu", storeCheckItemUomQty.BackStoreQtyBu },
        { "@Ss", storeCheckItemUomQty.SS },
        { "@CreatedTime", storeCheckItemUomQty.CreatedTime },
        { "@ModifiedTime", storeCheckItemUomQty.ModifiedTime },
        { "@ServerAddTime", storeCheckItemUomQty.ServerAddTime },
        { "@ServerModifiedTime", storeCheckItemUomQty.ServerModifiedTime }
    };

            return await ExecuteNonQueryAsync(query, parameters, connection, transaction);
        }
        
        public async Task<int> CreateUpdateStoreCheckExpireDreHistory(IStoreCheckItemHistoryViewList storeCheckItemHistoryViewList,
         IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            bool exists = false;
            int count = 0;
            if (storeCheckItemHistoryViewList == null)
            {
                return count;
            }
           // List<string> uidList = storeCheckItemExpiryDREHistory.Select(po => po.Uid).ToList();
            // List<string> deletedUidList = wHStockRequestLines.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>  existingRec = await SelectStoreCheckItemExpiryDREHistory(storeCheckItemHistoryViewList.Uid);

                foreach (var storeCheckItemExpiryDREHistory in storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory)
                {
                    switch (storeCheckItemExpiryDREHistory.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            if(existingRec != null)
                            {
                                exists = existingRec.Any(po => po.UID == storeCheckItemExpiryDREHistory.UID);

                            }
                            if (exists)
                            {
                                if(storeCheckItemExpiryDREHistory.IsRowModified)
                                {
                                    count += await UpdateStoreCheckExpireDreHistory(storeCheckItemExpiryDREHistory, connection, transaction);

                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                count += await CreateStoreCheckExpireDreHistory(storeCheckItemExpiryDREHistory, connection, transaction);
                            }
                            break;

                    }
                }
            }
            catch
            {
                throw;
            }

            return count;
        }

        public async Task<int> CreateUpdateStoreCheckUomQty(IStoreCheckItemUomQty StoreCheckItemUomQty, string uom,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int count = 0;
            if (StoreCheckItemUomQty == null || StoreCheckItemUomQty.StoreCheckItemHistoryUID == null)
            {
                return count;
            }
            // List<string> uidList = storeCheckItemExpiryDREHistory.Select(po => po.Uid).ToList();
            // List<string> deletedUidList = wHStockRequestLines.Where(S => S.ActionType == Winit.Shared.Models.Enums.ActionType.Delete).Select(S => S.UID).ToList();

            try
            {
                Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemUomQty existingRec = await SelectStoreCheckItemUomQty(StoreCheckItemUomQty.StoreCheckItemHistoryUID, uom);

                
                    switch (StoreCheckItemUomQty.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            //bool exists = existingRec.UID == StoreCheckItemUomQty.UID;
                            if (existingRec != null)
                            {
                                if (StoreCheckItemUomQty.IsRowModified)
                                {
                                    count += await UpdateStoreChecUomQty(StoreCheckItemUomQty, connection, transaction);

                                }
                                
                            }
                            else
                            {
                                count += await CreateStoreChecUomQty(StoreCheckItemUomQty, connection, transaction);
                            }
                            break;

                    }
                
            }
            catch
            {
                throw;
            }

            return count;
        }
        //public async Task<int> CreateUpdateStoreCheckExpireDreHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistoryViewList storeCheckItemHistoryViewList,
        //   object connection = null, object transaction = null)
        //{
        //    int count = 0;

        //    try
        //    {
        //        using (var connection = SqliteConnection())
        //        {
        //            await connection.OpenAsync();

        //            using (var transaction = connection.BeginTransaction())
        //            {
        //                try
        //                {

        //                    bool exists = false;
        //                    var existingRec = await SelectStoreCheckHistoryByUID(storeCheckItemExpiryDREHistory.Uid);


        //                    switch (storeCheckItemExpiryDREHistory.ActionType)
        //                    {
        //                        case Shared.Models.Enums.ActionType.Add:
        //                            if (existingRec != null)
        //                            {
        //                                exists = (existingRec.Uid == storeCheckItemExpiryDREHistory.Uid);
        //                            }

        //                            count += exists ? await UpdateStoreCheckExpireDreHistory(storeCheckItemExpiryDREHistory, connection, transaction) : await CreateStoreCheckExpireDreHistory(storeCheckItemExpiryDREHistory, connection, transaction);
        //                            break;

        //                    }

        //                    transaction.Commit();

        //                }
        //                catch
        //                {
        //                    transaction.Rollback();
        //                    throw;
        //                }
        //                finally { connection.Close(); }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    return count;
        //}
        private async Task<int> UpdateStoreCheckExpireDreHistory(IStoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var Query = @"UPDATE store_check_item_expiry_der_history
                 SET
                 stock_type = @StockType,
                 stock_sub_type = @StockSubType,
                 batch_no = @BatchNo,
                 expiry_date = @ExpiryDate,
                 qty = @Qty,
                 modified_time = @ModifiedTime,
                 server_modified_time = @ServerModifiedTime
                 WHERE uid = @Uid;";

            var Parameters = new Dictionary<string, object>
    {
        { "@StockType", storeCheckItemExpiryDREHistory.StockType },
        { "@StockSubType", storeCheckItemExpiryDREHistory.StockSubType },
        { "@BatchNo", storeCheckItemExpiryDREHistory.BatchNo },
        { "@ExpiryDate", storeCheckItemExpiryDREHistory.ExpiryDate },
        { "@Qty", storeCheckItemExpiryDREHistory.Qty },
        { "@ModifiedTime", storeCheckItemExpiryDREHistory.ModifiedTime },
        { "@ServerModifiedTime", storeCheckItemExpiryDREHistory.ServerModifiedTime },
        { "@Uid", storeCheckItemExpiryDREHistory.UID }
    };
            return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        }

        private async Task<int> CreateStoreCheckExpireDreHistory(IStoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory,
      IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            var query = @"INSERT INTO store_check_item_expiry_der_history
                 (id,uid, store_check_item_history_uid, stock_type, stock_sub_type, batch_no, 
                  expiry_date, qty, uom, ss, created_time, modified_time, 
                  server_add_time, server_modified_time)
                 VALUES 
                 (@Id,@Uid, @StoreCheckItemHistoryUid, @StockType, @StockSubType, @BatchNo, 
                  @ExpiryDate, @Qty, @Uom, @Ss, @CreatedTime, @ModifiedTime, 
                  @ServerAddTime, @ServerModifiedTime);";

            var parameters = new Dictionary<string, object>
    {
                  { "@Id", storeCheckItemExpiryDREHistory.UID },
        { "@Uid", storeCheckItemExpiryDREHistory.UID },
        { "@StoreCheckItemHistoryUid", storeCheckItemExpiryDREHistory.StoreCheckItemHistoryUID },
        { "@StockType", storeCheckItemExpiryDREHistory.StockType },
        { "@StockSubType", storeCheckItemExpiryDREHistory.StockSubType },
        { "@BatchNo", storeCheckItemExpiryDREHistory.BatchNo },
        { "@ExpiryDate", storeCheckItemExpiryDREHistory.ExpiryDate },
       
        { "@Qty", storeCheckItemExpiryDREHistory.Qty },
        { "@Uom", storeCheckItemExpiryDREHistory.Uom },
        { "@Ss", storeCheckItemExpiryDREHistory.SS },
        { "@CreatedTime", storeCheckItemExpiryDREHistory.CreatedTime },
        { "@ModifiedTime", storeCheckItemExpiryDREHistory.ModifiedTime },
        { "@ServerAddTime", storeCheckItemExpiryDREHistory.ServerAddTime },
        { "@ServerModifiedTime", storeCheckItemExpiryDREHistory.ServerModifiedTime }
    };

            return await ExecuteNonQueryAsync(query, parameters, connection, transaction);
        }

        //private async Task<int> CreateStoreCheckGroupHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckGroupHistory StoreCheckGroupHistory,
        //         object connection = null, object transaction = null)
        //{
        //    var Query = @"INSERT INTO store_check_history (id ,uid,StoreCheckHistoryUid  , group_by_code  , group_by_value  , store_check_level  , 
        //                status  , ss ,created_time , modified_time , server_add_time , server_modified_time)
        //                VALUES (@Id ,@Uid , @StoreCheckHistoryUid, @GroupByCode , @GroupByValue , @StoreCheckLevel  , @Status , @Ss , @StoreUid,
        //                @StoreCheckConfigUid, @SkuGroupTypeUid, @SkuGroupUid, @GroupByName, @Status, @Level, @IsLastLevel, @SS, 
        //                @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
        //    var Parameters = new Dictionary<string, object>
        //                {
        //                   { "@Id", StoreCheckGroupHistory.Id },
        //                   { "@Uid", StoreCheckGroupHistory.Uid },
        //                   { "@StoreCheckHistoryUid ", StoreCheckGroupHistory.StoreCheckHistoryUid  },
        //                   { "@GroupByCode ", StoreCheckGroupHistory.GroupByCode  },
        //                   { "@GroupByValue ", StoreCheckGroupHistory.GroupByValue  },
        //                   { "@StoreCheckLevel  ", StoreCheckGroupHistory.StoreCheckLevel   },
        //                   { "@Status ", StoreCheckGroupHistory.Status  },
        //                   { "@Ss ", StoreCheckGroupHistory.Ss  },
        //                   { "@CreatedTime", StoreCheckGroupHistory.CreatedTime },
        //                   { "@ModifiedTime", StoreCheckGroupHistory.ModifiedTime },
        //                   { "@ServerAddTime", StoreCheckGroupHistory.ServerAddTime },
        //                   { "@ServerModifiedTime", StoreCheckGroupHistory.ServerModifiedTime },
        //                };
        //    return await ExecuteNonQueryAsync(Query, Parameters, connection, transaction);
        //}
        #endregion
    }
}
