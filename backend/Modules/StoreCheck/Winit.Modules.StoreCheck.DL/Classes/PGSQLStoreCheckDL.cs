using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
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
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.DL.Classes
{
    public class PGSQLStoreCheckDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IStoreCheckDL
    {
        public PGSQLStoreCheckDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                                                 SCH.uid AS Uid,
                                                 SCH.beat_history_uid AS BeatHistoryUid,
                                                 SCH.store_history_uid AS StoreHistoryUid,
                                                 SCH.store_asset_uid AS StoreAssetUid,
                                                 SCH.org_uid AS OrgUid,
                                                 SCH.store_check_date AS StoreCheckDate,
                                                 SCH.store_uid AS StoreUid,
                                                 SCH.store_check_config_uid AS StoreCheckConfigUid,
                                                 SCH.sku_group_type_uid AS SkuGroupTypeUid,
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
                                                 WHERE SCH.beat_history_uid = @BeatHistoryUid AND SCH.store_history_uid = @StoreHistoryUid;");

                var parameters = new Dictionary<string, object>()
                {
                    { "@BeatHistoryUid", beatHistoryUID },
                    { "@StoreHistoryUid", storeHistoryUID }
                };
                return await ExecuteSingleAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>(sql.ToString(), parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while selecting store check history data.", ex);
            }
        }

        public async Task<IStoreCheckItemUomQty?> SelectStoreCheckItemUomQty(string storeCheckItemHistoryUID, string uom)
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
                return await ExecuteSingleAsync<IStoreCheckItemUomQty>(sql.ToString(), parameters);
            }
            catch 
            {
                throw;
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

                return await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory>(sql.ToString(), parameters);
            }
            catch 
            {
                throw;
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
                                             SCIH.uid AS Uid,
                                             SCIH.store_check_history_uid AS StoreCheckHistoryUid,
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
                                             SCIH.server_modified_time AS ServerModifiedTime
                                             FROM 
                                             store_check_item_history SCIH
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
                    AppendFilterCriteria<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList> StoreCheckItemHistoryViewList = await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>(sql.ToString(), parameters);
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
        public async Task<int> UpdateStoreCheck(Winit.Modules.StoreCheck.Model.Classes.StoreCheckModel storeCheckModel)
        {
            int count = 0;

            try
            {
                using (var connection = PostgreConnection())
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
                using (var connection = PostgreConnection())
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
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"@UID",  UID}
            };
                var sql = @"SELECT * FROM store_check_history WHERE uid  = @UID";
                return await ExecuteSingleAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckHistoryView>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }

        private async Task<int> UpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
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
                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, Parameters);
            }
            catch
            {
                throw;
            }
            return retVal;
            
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
                            if (storeCheckItemHistoryViewList.IsDRESelected && storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory != null && storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory.Count > 0)
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

            try
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
                return await ExecuteNonQueryAsync(Query, connection, transaction, Parameters);
            }
            catch
            {
                throw;
            }
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
                return await ExecuteNonQueryAsync(Query, connection, transaction, Parameters);
            }
            catch (Exception Ex)
            {
                throw;
            }
        }

        private async Task<IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>> SelectStoreCheckItemHistoryByUID(List<string> UIDs)
        {
            try
            {
                UIDs = UIDs.Select(code => code.Trim()).ToList();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                for (int i = 0; i < UIDs.Count; i++)
                {
                    parameters.Add($"@uid{i}", UIDs[i]);
                }
                var sql = $"SELECT * FROM store_check_item_history WHERE uid  IN  ({string.Join(",", UIDs.Select((_, i) => $"@uid{i}"))})";
                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList> WHStockRequestLineDetails = await ExecuteQueryAsync<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemHistoryViewList>(sql, parameters);
                return WHStockRequestLineDetails;
            }
            catch
            {
                throw;
            }
            
        }


        //StoreCheckHistory
        private async Task<int> CreateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
            IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var Query = @"INSERT INTO store_check_history (id,uid,beat_history_uid , store_history_uid , store_asset_uid , org_uid , 
                        store_check_date , store_uid , store_check_config_uid , sku_group_type_uid , sku_group_uid , group_by_name , status , level , 
                        is_last_level , ss ,created_time , modified_time , server_add_time , server_modified_time)
                        VALUES (@Id,@Uid, @BeatHistoryUid, @StoreHistoryUid, @StoreAssetUid, @OrgUid, @StoreCheckDate, @StoreUid,
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
                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, Parameters);
            }
            catch
            {
                throw;
            }
            return retVal;
           
        }

        private async Task<int> UpdateStoreChecUomQty(IStoreCheckItemUomQty storeCheckItemUomQty,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
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

                return await ExecuteNonQueryAsync(Query, connection, transaction, parameters);
            }
            catch
            {
                throw;
            }
           
        }

        private async Task<int> CreateStoreChecUomQty(IStoreCheckItemUomQty storeCheckItemUomQty,
     IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
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

                return await ExecuteNonQueryAsync(query, connection, transaction, parameters);
            }
            catch
            {
                throw;
            }
           
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
                IEnumerable<Winit.Modules.StoreCheck.Model.Interfaces.IStoreCheckItemExpiryDREHistory> existingRec = await SelectStoreCheckItemExpiryDREHistory(storeCheckItemHistoryViewList.Uid);

                foreach (var storeCheckItemExpiryDREHistory in storeCheckItemHistoryViewList.StoreCheckItemExpiryDREHistory)
                {
                    switch (storeCheckItemExpiryDREHistory.ActionType)
                    {
                        case Shared.Models.Enums.ActionType.Add:
                            if (existingRec != null)
                            {
                                exists = existingRec.Any(po => po.UID == storeCheckItemExpiryDREHistory.UID);

                            }
                            if (exists)
                            {
                                if (storeCheckItemExpiryDREHistory.IsRowModified)
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
        private async Task<int> UpdateStoreCheckExpireDreHistory(IStoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
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
                return await ExecuteNonQueryAsync(Query, connection, transaction, Parameters);
            }
            catch
            {
                throw;
            }
           
        }

        private async Task<int> CreateStoreCheckExpireDreHistory(IStoreCheckItemExpiryDREHistory storeCheckItemExpiryDREHistory,
      IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            try
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

                return await ExecuteNonQueryAsync(query, connection, transaction, parameters);
            }
            catch
            {
                throw;
            }
           
        }


        public async Task<int> InsertorUpdate_StoreCheck(StoreCheckMaster storeCheckMaster)
        {
            if (storeCheckMaster == null)
            {
                throw new Exception("StoreCheckMaster cannot be null.");
            }
            try
            {
                using (var connection = PostgreConnection())
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        int retVal = -1;

                        if (storeCheckMaster.StoreCheckHistory != null)
                        {
                            retVal = await CUDStoreCheckHistory(storeCheckMaster.StoreCheckHistory, connection, transaction);
                        }

                        if (storeCheckMaster.StoreCheckHistoryList != null && storeCheckMaster.StoreCheckHistoryList.Any())
                        {
                            retVal = await CUDStoreCheckItemHistory(storeCheckMaster.StoreCheckHistoryList, connection, transaction);
                        }

                        if (storeCheckMaster.StoreCheckItemExpiryDREHistoriesList != null && storeCheckMaster.StoreCheckItemExpiryDREHistoriesList.Any())
                        {
                            retVal = await CUDStoreCheckExpireDreHistory(storeCheckMaster.StoreCheckItemExpiryDREHistoriesList, connection, transaction);
                        }

                        if (storeCheckMaster.StoreCheckItemUomQtyList != null && storeCheckMaster.StoreCheckItemUomQtyList.Any())
                        {
                            retVal = await CUDStoreChecUomQty(storeCheckMaster.StoreCheckItemUomQtyList, connection, transaction);
                        }

                        if (retVal > 0)
                        {
                            await transaction.CommitAsync();
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                        }

                        return retVal;
                    }
                }
            }
            catch
            {
                throw;
            }
        }


        //StoreCheckHistory
        private async Task<int> CUDStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistory,
         IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var uid = storeCheckHistory.UID;
                string isExists = await CheckIfUIDExistsInDB(DbTableName.StoreCheckHistory, uid, connection, transaction);
                if (!string.IsNullOrEmpty(isExists))
                {
                    retVal= await UpdateStoreCheckHistory(storeCheckHistory, connection, transaction);
                }
                else
                {
                    retVal= await CreateStoreCheckHistory(storeCheckHistory, connection, transaction);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }

    
       


        private async Task<int> CUDStoreCheckItemHistory(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory> storeCheckItemHistoryList,
        IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var uidList = storeCheckItemHistoryList.Select(scih => scih.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreCheckItemHistory, uidList,connection,transaction);
                List<StoreCheckItemHistory>? newstoreCheckItemHistoryList = null;
                List<StoreCheckItemHistory>? existingstoreCheckItemHistoryList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    newstoreCheckItemHistoryList = storeCheckItemHistoryList.Where(scih => !existingUIDs.Contains(scih.UID)).ToList();
                    existingstoreCheckItemHistoryList = storeCheckItemHistoryList.Where(scih => existingUIDs.Contains(scih.UID)).ToList();
                }
                else
                {
                    newstoreCheckItemHistoryList = storeCheckItemHistoryList;
                }
                if (newstoreCheckItemHistoryList != null && newstoreCheckItemHistoryList.Any())
                {
                    retVal = await CreateStoreCheckItemHistoryList(newstoreCheckItemHistoryList,connection,transaction);

                }

                if (existingstoreCheckItemHistoryList != null && existingstoreCheckItemHistoryList.Any())
                {

                    retVal = await UpdateStoreCheckItemHistoryList(existingstoreCheckItemHistoryList,connection,transaction);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateStoreCheckItemHistoryList(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory> storeCheckItemHistoryList,
           IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var Query = @"INSERT INTO store_check_item_history (uid,store_check_history_uid,group_by_code ,group_by_value, 
                                 sku_uid,sku_code,uom,suggested_qty,store_qty,backstore_qty,to_fill_qty,is_available,is_dre_selected,ss ,created_time,
                                 modified_time ,server_add_time ,server_modified_time) VALUES (@Uid, @StoreCheckHistoryUid, @GroupByCode, 
                            @GroupByValue, @SKUUID,@SKUCode,@UOM,@SuggestedQty,@StoreQty,@BackStoreQty,@ToFillQty, @IsAvailable,@IsDRESelected, @SS, @CreatedTime, 
                            @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";

                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckItemHistoryList);
            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> UpdateStoreCheckItemHistoryList(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemHistory> storeCheckItemHistoryList,
           IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retval = -1;
            try
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

                retval= await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckItemHistoryList);
            }
            catch
            {
                throw;
            }
            return retval;
        }
        //StoreCheckExpireDreHistory


        private async Task<int> CUDStoreCheckExpireDreHistory(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemExpiryDREHistory> storeCheckItemExpiryDREHistoryList,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var uidList = storeCheckItemExpiryDREHistoryList.Select(scied => scied.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreCheckItemExpiryDerHistory, uidList, connection, transaction);
                List<StoreCheckItemExpiryDREHistory>? newstoreCheckItemExpiryDREHistoryList = null;
                List<StoreCheckItemExpiryDREHistory>? existingstoreCheckItemExpiryDREHistoryList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    newstoreCheckItemExpiryDREHistoryList = storeCheckItemExpiryDREHistoryList.Where(scied => !existingUIDs.Contains(scied.UID)).ToList();
                    existingstoreCheckItemExpiryDREHistoryList = storeCheckItemExpiryDREHistoryList.Where(scied => existingUIDs.Contains(scied.UID)).ToList();
                }
                else
                {
                    newstoreCheckItemExpiryDREHistoryList = storeCheckItemExpiryDREHistoryList;
                }
                if (newstoreCheckItemExpiryDREHistoryList != null && newstoreCheckItemExpiryDREHistoryList.Any())
                {
                    retVal = await CreateStoreCheckExpireDreHistoryList(newstoreCheckItemExpiryDREHistoryList, connection, transaction);

                }

                if (existingstoreCheckItemExpiryDREHistoryList != null && existingstoreCheckItemExpiryDREHistoryList.Any())
                {

                    retVal = await UpdateStoreCheckExpireDreHistoryList(existingstoreCheckItemExpiryDREHistoryList, connection, transaction);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateStoreCheckExpireDreHistoryList(List<StoreCheckItemExpiryDREHistory> storeCheckItemExpiryDREHistoryList,
     IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var query = @"INSERT INTO store_check_item_expiry_der_history
                 (id,uid, store_check_item_history_uid, stock_type, stock_sub_type, batch_no, 
                  expiry_date, qty, uom, ss, created_time, modified_time, 
                  server_add_time, server_modified_time)
                 VALUES 
                 (@Id,@Uid, @StoreCheckItemHistoryUid, @StockType, @StockSubType, @BatchNo, 
                  @ExpiryDate, @Qty, @Uom, @Ss, @CreatedTime, @ModifiedTime, 
                  @ServerAddTime, @ServerModifiedTime);";
                retVal= await ExecuteNonQueryAsync(query, connection, transaction, storeCheckItemExpiryDREHistoryList);
            }
            catch
            {
                throw;
            }
            return retVal;

        }
        private async Task<int> UpdateStoreCheckExpireDreHistoryList(List<StoreCheckItemExpiryDREHistory> storeCheckItemExpiryDREHistoryList,
IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
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
                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckItemExpiryDREHistoryList);
            }
            catch
            {
                throw;
            }
            return retVal;

        }
        //StoreChecUomQty

        private async Task<int> CUDStoreChecUomQty(List<Winit.Modules.StoreCheck.Model.Classes.StoreCheckItemUomQty> storeCheckItemUomQtyList,
       IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var uidList = storeCheckItemUomQtyList.Select(sciu => sciu.UID).ToList();
                List<string>? existingUIDs = await CheckIfUIDExistsInDB(DbTableName.StoreCheckItemUOMQty, uidList, connection, transaction);
                List<StoreCheckItemUomQty>? newstoreCheckItemUomQtyList = null;
                List<StoreCheckItemUomQty>? existingstoreCheckItemUomQtyList = null;
                if (existingUIDs != null && existingUIDs.Any())
                {
                    newstoreCheckItemUomQtyList = storeCheckItemUomQtyList.Where(sciu => !existingUIDs.Contains(sciu.UID)).ToList();
                    existingstoreCheckItemUomQtyList = storeCheckItemUomQtyList.Where(sciu => existingUIDs.Contains(sciu.UID)).ToList();
                }
                else
                {
                    newstoreCheckItemUomQtyList = storeCheckItemUomQtyList;
                }
                if (newstoreCheckItemUomQtyList != null && newstoreCheckItemUomQtyList.Any())
                {
                    retVal = await CreateStoreChecUomQtyList(newstoreCheckItemUomQtyList, connection, transaction);

                }

                if (existingstoreCheckItemUomQtyList != null && existingstoreCheckItemUomQtyList.Any())
                {

                    retVal = await UpdateStoreChecUomQtyList(existingstoreCheckItemUomQtyList, connection, transaction);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return retVal;
        }
        private async Task<int> CreateStoreChecUomQtyList(List<StoreCheckItemUomQty> storeCheckItemUomQtyList,
IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
            {
                var query = @"INSERT INTO store_check_item_uom_qty 
                 (uid, store_check_item_history_uid, uom, base_uom, uom_multiplier, 
                 store_qty, store_qty_bu, back_store_qty, back_store_qty_bu, ss, 
                 created_time, modified_time, server_add_time, server_modified_time)
                 VALUES 
                 (@UID, @StoreCheckItemHistoryUid, @UOM, @BaseUom, @UomMultiplier, 
                 @StoreQty, @StoreQtyBu, @BackStoreQty, @BackStoreQtyBu, @Ss, 
                 @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                retVal= await ExecuteNonQueryAsync(query, connection, transaction, storeCheckItemUomQtyList);
            }
            catch
            {
                throw;
            }
            return retVal;

        }

        private async Task<int> UpdateStoreChecUomQtyList(List<StoreCheckItemUomQty> storeCheckItemUomQtyList,
  IDbConnection? connection = null, IDbTransaction? transaction = null)
        {
            int retVal = -1;
            try
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
                retVal= await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckItemUomQtyList);
            }
            catch
            {
                throw;
            }
            return retVal;

        }


        // private async Task<int> CreateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistory,
        //  IDbConnection? connection = null, IDbTransaction? transaction = null)
        // {
        //     try
        //     {
        //         var Query = @"INSERT INTO store_check_history (id,uid,beat_history_uid , store_history_uid , store_asset_uid , org_uid , 
        //                 store_check_date , store_uid , store_check_config_uid , sku_group_type_uid , sku_group_uid , group_by_name , status , level , 
        //                 is_last_level , ss ,created_time , modified_time , server_add_time , server_modified_time)
        //                 VALUES (@Id,@Uid, @BeatHistoryUid, @StoreHistoryUid, @StoreAssetUid, @OrgUid, @StoreCheckDate, @StoreUid,
        //                 @StoreCheckConfigUid, @SkuGroupTypeUid, @SkuGroupUid, @GroupByName, @Status, @Level, @IsLastLevel, @SS, 
        //                 @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
        //         return await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckHistory);
        //     }
        //     catch
        //     {
        //         throw;
        //     }
        // }
        // private async Task<int> UpdateStoreCheckHistory(Winit.Modules.StoreCheck.Model.Classes.StoreCheckHistoryView storeCheckHistoryView,
        //IDbConnection? connection = null, IDbTransaction? transaction = null)
        // {
        //     try
        //     {
        //         var Query = @"UPDATE store_check_history
        //                 SET status = @Status, 
        //                 modified_time = @ModifiedTime, 
        //                 server_modified_time  = @ServerModifiedTime
        //                 WHERE uid = @Uid;";
        //         return await ExecuteNonQueryAsync(Query, connection, transaction, storeCheckHistoryViewList);
        //     }
        //     catch
        //     {
        //         throw;
        //     }

        // }

        //store_check_item_historyList



    }
}
