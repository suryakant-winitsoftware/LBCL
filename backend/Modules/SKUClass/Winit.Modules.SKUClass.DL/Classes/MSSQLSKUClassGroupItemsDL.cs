using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.DL.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.DL.Classes;

public class MSSQLSKUClassGroupItemsDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUClassGroupItemsDL
{
    public MSSQLSKUClassGroupItemsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }
    public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>> SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                id AS Id
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_class_group_items_uid AS SKUClassGroupUID,
                sku_code AS SKUCode,
                serial_number AS SerialNumber,
                model_qty AS ModelQty,
                model_uom AS ModelUoM,
                supplier_org_uid AS SupplierOrgUID,
                lead_time_in_days AS LeadTimeInDays,
                daily_cut_off_time AS DailyCutOffTime,
                is_exclusive AS IsExclusive,
                max_qty AS MaxQTY,
                min_qty AS MinQTY,
                is_excluded AS IsExcluded            
               FROM 
                sku_class_group_items)AS SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                id AS Id
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_class_group_items_uid AS SKUClassGroupUID,
                sku_code AS SKUCode,
                serial_number AS SerialNumber,
                model_qty AS ModelQty,
                model_uom AS ModelUoM,
                supplier_org_uid AS SupplierOrgUID,
                lead_time_in_days AS LeadTimeInDays,
                daily_cut_off_time AS DailyCutOffTime,
                is_exclusive AS IsExclusive,
                max_qty AS MaxQTY,
                min_qty AS MinQTY,
                is_excluded AS IsExcluded            
                             FROM 
                sku_class_group_items)AS SubQuery");
            }
            var parameters = new Dictionary<string, object>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>(filterCriterias, sbFilterCriteria, parameters);

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
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            IEnumerable<Model.Interfaces.ISKUClassGroupItems> sKUClassGroupItemss = await ExecuteQueryAsync<Model.Interfaces.ISKUClassGroupItems>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> pagedResponse = new PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>
            {
                PagedData = sKUClassGroupItemss,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> GetSKUClassGroupItemsByUID(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID", UID}
        };

        var sql = @"SELECT id AS Id
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_class_group_items_uid AS SKUClassGroupUID,
                sku_code AS SKUCode,
                serial_number AS SerialNumber,
                model_qty AS ModelQty,
                model_uom AS ModelUoM,
                supplier_org_uid AS SupplierOrgUID,
                lead_time_in_days AS LeadTimeInDays,
                daily_cut_off_time AS DailyCutOffTime,
                is_exclusive AS IsExclusive,
                max_qty AS MaxQTY,
                min_qty AS MinQTY,is_excluded as IsExcluded,
                action_type AS ActionType FROM sku_class_group_items WHERE uid = @UID";
        return await ExecuteSingleAsync<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>(sql, parameters);
    }
    public async Task<int> CreateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems createSKUClassGroupItems, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"INSERT INTO sku_class_group_items (uid, sku_class_group_uid, sku_code, serial_number, model_qty, model_uom, 
                        supplier_org_uid, lead_time_in_days, daily_cut_off_time, is_exclusive, created_time, modified_time, server_add_time, 
                        server_modified_time, created_by, modified_by, max_qty, min_qty,is_excluded,sku_uid) VALUES (@UID, @SKUClassGroupUID, @SKUCode, 
                        @SerialNumber,@ModelQty, @ModelUoM, @SupplierOrgUID,@LeadTimeInDays,@DailyCutOffTime,@IsExclusive, @CreatedTime, 
                        @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@CreatedBy,@ModifiedBy,@MaxQTY,@MinQTY,@IsExcluded,@SKUUID)";

            return await ExecuteNonQueryAsync(sql, connection, transaction, createSKUClassGroupItems);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreateSKUClassGroupItemsBulk(List<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems> createSKUClassGroupItemsBulk, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"INSERT INTO sku_class_group_items (uid, sku_class_group_uid, sku_code, serial_number, model_qty, model_uom, 
                        supplier_org_uid, lead_time_in_days, daily_cut_off_time, is_exclusive, created_time, modified_time, server_add_time, 
                        server_modified_time, created_by, modified_by, max_qty, min_qty,is_excluded,sku_uid) VALUES (@UID, @SKUClassGroupUID, @SKUCode, 
                        @SerialNumber,@ModelQty, @ModelUoM, @SupplierOrgUID,@LeadTimeInDays,@DailyCutOffTime,@IsExclusive, @CreatedTime, 
                        @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@CreatedBy,@ModifiedBy,@MaxQTY,@MinQTY,@IsExcluded,@SKUUID)";

            return await ExecuteNonQueryAsync(sql, connection, transaction, createSKUClassGroupItemsBulk);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems updateSKUClassGroupItems, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"UPDATE sku_class_group_items 
            SET sku_class_group_uid = @SKUClassGroupUID, sku_code = @SKUCode,
                serial_number = @SerialNumber, model_qty = @ModelQty,
                model_uom = @ModelUoM, supplier_org_uid = @SupplierOrgUID,
                lead_time_in_days = @LeadTimeInDays, daily_cut_off_time = @DailyCutOffTime,
                is_exclusive = @IsExclusive, modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime, min_qty = @MinQTY,
                max_qty = @MaxQTY,
                is_excluded = @IsExcluded,sku_uid=@SKUUID 
            WHERE uid = @UID;";

            return await ExecuteNonQueryAsync(sql, connection, transaction, updateSKUClassGroupItems);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKUClassGroupItems(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID", UID}
        };
        var sql = @"DELETE FROM sku_class_group_items WHERE uid = @UID;";

        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<int> DeleteSKUClassGroupItems(List<string> UIDs, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            if (UIDs == null || !UIDs.Any())
            {
                throw new Exception("Received O UIDs");
            }
            var parameters = new { UIDS = UIDs };


            var sql = @$"DELETE FROM sku_class_group_items WHERE uid IN @UIDS";

            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the SKUClassGroupItems.", ex);
        }
    }
    public async Task<PagedResponse<ISKUClassGroupItemView>> SelectAllSKUClassGroupItemView(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder("""
                                        Select * From(SELECT O.name as PlantName, S.name as SKUName,
                                        				SCGI.id AS Id,
                                                        SCGI.uid AS UID,
                                                        SCGI.created_by AS CreatedBy,
                                                        SCGI.created_time AS CreatedTime,
                                                        SCGI.modified_by AS ModifiedBy,
                                                        SCGI.modified_time AS ModifiedTime,
                                                        SCGI.server_add_time AS ServerAddTime,
                                                        SCGI.server_modified_time AS ServerModifiedTime,
                                                        SCGI.sku_class_group_uid AS SKUClassGroupUID,
                                                        SCGI.sku_code AS SKUCode,
                                                        SCGI.sku_uid AS SKUUID,
                                                        SCGI.serial_number AS SerialNumber,
                                                        SCGI.model_qty AS ModelQty,
                                                        SCGI.model_uom AS ModelUoM,
                                                        SCGI.supplier_org_uid AS SupplierOrgUID,
                                                        SCGI.lead_time_in_days AS LeadTimeInDays,
                                                        SCGI.daily_cut_off_time AS DailyCutOffTime,
                                                        SCGI.is_exclusive AS IsExclusive,
                                                        SCGI.max_qty AS MaxQTY,
                                                        SCGI.min_qty AS MinQTY,
                                                        SCGI.is_excluded AS IsExcluded FROM sku_class_group_items  
                                                        SCGI INNER JOIN sku S ON SCGI.sku_code = S.code LEFT JOIN  org O ON 
                                                        O.uid = SCGI.supplier_org_uid)As SubQuery
                                        """);
            var parameters = new Dictionary<string, object>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>
                    (filterCriterias, sbFilterCriteria, parameters);
                sql.Append(sbFilterCriteria);
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }

            IEnumerable<Model.UIInterfaces.ISKUClassGroupItemView> sKUClassGroupItemss =
                await ExecuteQueryAsync<Model.UIInterfaces.ISKUClassGroupItemView>(sql.ToString(), parameters);

            PagedResponse<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView> pagedResponse =
                new PagedResponse<Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView>
                {
                    PagedData = sKUClassGroupItemss,
                };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<IEnumerable<ISKUClassGroupItems>> PrepareSkuClassForCache(List<string> linkedItemUIDs)
    {
        string sql = """
                        select i.sku_class_group_uid,i.sku_uid from sku_class_group g
            inner join sku_class_group_items i on g.uid=i.sku_class_group_uid and g.is_active=1
            
            """;
        var parameters = new Dictionary<string, object>
        {
            {"LinkedItemUIDs",linkedItemUIDs }
        };
        if (linkedItemUIDs != null && linkedItemUIDs.Count > 0)
        {
            sql = new StringBuilder(sql).Append("where g.linked_item_uid in @LinkedItemUIDs").ToString();
        }
        IEnumerable<ISKUClassGroupItems> sKUClassGroupItemss =
                await ExecuteQueryAsync<ISKUClassGroupItems>(sql.ToString(), parameters);

        return sKUClassGroupItemss;
    }
    public async Task<List<string>> GetApplicableAllowedSKUGroupUIDs(string storeUID)
    {
        string sql = """
                       select uid from sku_class_group
            """;
        var parameters = new Dictionary<string, object>
        {

        };
        List<string> sKUClassGroupItemss =
                await ExecuteQueryAsync<string>(sql.ToString(), parameters);

        return sKUClassGroupItemss;
    }

    public async Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID)
    {
        string sql = """
                       SELECT sku_uid FROM sku_class_group_items where sku_class_group_uid = @SKUClassGroupUID
            """;
        var parameters = new Dictionary<string, object>
        {
            {"SKUClassGroupUID",skuClassGroupUID }
        };
        List<string> sKUClassGroupItemss =
                await ExecuteQueryAsync<string>(sql.ToString(), parameters);

        return sKUClassGroupItemss;
    }
}
