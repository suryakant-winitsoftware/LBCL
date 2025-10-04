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

public class PGSQLSKUClassGroupItemsDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUClassGroupItemsDL
{
    public PGSQLSKUClassGroupItemsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }
    public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>> SelectAllSKUClassGroupItemsDetails(List<SortCriteria> sortCriterias, int pageNumber,
                      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT 
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_class_group_uid AS SKUClassGroupUID,
                sku_code AS SKUCode,
                sku_uid AS SKUUID,
                serial_number AS SerialNumber,
                model_qty AS ModelQty,
                model_uom AS ModelUoM,
                supplier_org_uid AS SupplierOrgUID,
                lead_time_in_days AS LeadTimeInDays,
                daily_cut_off_time AS DailyCutOffTime,
                is_exclusive AS IsExclusive,
                max_qty AS MaxQTY,
                min_qty AS MinQTY
            FROM 
                sku_class_group_items");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_class_group_items");
            }
            var parameters = new Dictionary<string, object>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                
                // Manual filter handling for sku_class_group_uid
                bool hasManualFilter = false;
                foreach (var filter in filterCriterias)
                {
                    if (filter.Name?.ToLower() == "sku_class_group_uid")
                    {
                        if (hasManualFilter) sbFilterCriteria.Append(" AND ");
                        sbFilterCriteria.Append($"sku_class_group_uid = @GroupUID");
                        parameters["GroupUID"] = filter.Value;
                        hasManualFilter = true;
                    }
                    else if (filter.Name?.ToLower() == "sku_code")
                    {
                        if (hasManualFilter) sbFilterCriteria.Append(" AND ");
                        sbFilterCriteria.Append($"sku_code LIKE @SKUCode");
                        parameters["SKUCode"] = $"%{filter.Value}%";
                        hasManualFilter = true;
                    }
                }
                
                // If no manual filters were added, use the base implementation
                if (!hasManualFilter)
                {
                    AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>(filterCriterias, sbFilterCriteria, parameters);
                }

                sql.Append(sbFilterCriteria);
                // If count required then add filters to count
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

            if (pageSize > 0)
            {
                // Ensure ORDER BY exists for PostgreSQL pagination
                if (sortCriterias == null || sortCriterias.Count == 0)
                {
                    sql.Append(" ORDER BY id");
                }
                // Use PostgreSQL pagination syntax (pageNumber is 0-based from API)
                int offset = pageNumber * pageSize;
                sql.Append($" LIMIT {pageSize} OFFSET {offset}");
            }

            //Data
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUClassGroupItems>().GetType();
            IEnumerable<Model.Interfaces.ISKUClassGroupItems> sKUClassGroupItemss = await ExecuteQueryAsync<Model.Interfaces.ISKUClassGroupItems>(sql.ToString(), parameters, type);
            //Count
            int totalCount = 0;
            if (isCountRequired)
            {
                // Get the total count of records
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
                {"UID",  UID}
            };

        var sql = @"SELECT id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_class_group_uid AS SKUClassGroupUID,
                sku_code AS SKUCode,
                sku_uid AS SKUUID,
                serial_number AS SerialNumber,
                model_qty AS ModelQty,
                model_uom AS ModelUoM,
                supplier_org_uid AS SupplierOrgUID,
                lead_time_in_days AS LeadTimeInDays,
                daily_cut_off_time AS DailyCutOffTime,
                is_exclusive AS IsExclusive,
                max_qty AS MaxQTY,
                min_qty AS MinQTY FROM sku_class_group_items WHERE uid = @UID";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUClassGroupItems>().GetType();
        Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems SKUClassGroupItemsDetails = await ExecuteSingleAsync<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>(sql, parameters, type);
        return SKUClassGroupItemsDetails;
    }
    public async Task<int> CreateSKUClassGroupItems(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems createSKUClassGroupItems, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"INSERT INTO sku_class_group_items (uid, sku_class_group_uid, sku_code, serial_number, model_qty, model_uom, 
                        supplier_org_uid, lead_time_in_days, daily_cut_off_time, is_exclusive, created_time, modified_time, server_add_time, 
                        server_modified_time, created_by, modified_by, max_qty, min_qty, sku_uid) VALUES (@UID, @SKUClassGroupUID, @SKUCode, 
                        @SerialNumber,@ModelQty, @ModelUoM, @SupplierOrgUID,@LeadTimeInDays,@DailyCutOffTime,@IsExclusive, @CreatedTime, 
                        @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@CreatedBy,@ModifiedBy,@MaxQTY,@MinQTY,@SKUUID)";

            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createSKUClassGroupItems.UID},
                   {"SKUClassGroupUID", createSKUClassGroupItems.SKUClassGroupUID},
                   {"SKUCode", createSKUClassGroupItems.SKUCode},
                   {"SKUUID", createSKUClassGroupItems.SKUUID},
                   {"SerialNumber", createSKUClassGroupItems.SerialNumber},
                   {"ModelQty",createSKUClassGroupItems.ModelQty},
                   {"ModelUoM",createSKUClassGroupItems.ModelUoM},
                   {"SupplierOrgUID",createSKUClassGroupItems.SupplierOrgUID},
                   {"LeadTimeInDays",createSKUClassGroupItems.LeadTimeInDays},
                   {"DailyCutOffTime",createSKUClassGroupItems.DailyCutOffTime},
                   {"IsExclusive",createSKUClassGroupItems.IsExclusive},
                   {"CreatedBy",createSKUClassGroupItems.CreatedBy},
                   {"ModifiedBy",createSKUClassGroupItems.ModifiedBy},
                   {"CreatedTime", createSKUClassGroupItems.CreatedTime},
                   {"ModifiedTime", createSKUClassGroupItems.ModifiedTime},
                   {"ServerAddTime", createSKUClassGroupItems.ServerAddTime},
                   {"ServerModifiedTime", createSKUClassGroupItems.ServerModifiedTime},
                   {"MaxQTY", createSKUClassGroupItems.MaxQTY},
                   {"MinQTY", createSKUClassGroupItems.MinQTY},
                };
            return await ExecuteNonQueryAsync(sql, connection,transaction,parameters);
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
                max_qty = @MaxQTY 
            WHERE uid = @UID;";

            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                 {"UID", updateSKUClassGroupItems.UID},
                   {"SKUClassGroupUID", updateSKUClassGroupItems.SKUClassGroupUID},
                   {"SKUCode", updateSKUClassGroupItems.SKUCode},
                   {"SerialNumber", updateSKUClassGroupItems.SerialNumber},
                   {"ModelQty",updateSKUClassGroupItems.ModelQty},
                   {"ModelUoM",updateSKUClassGroupItems.ModelUoM},
                   {"SupplierOrgUID",updateSKUClassGroupItems.SupplierOrgUID},
                   {"LeadTimeInDays",updateSKUClassGroupItems.LeadTimeInDays},
                   {"DailyCutOffTime",updateSKUClassGroupItems.DailyCutOffTime},
                   {"IsExclusive",updateSKUClassGroupItems.IsExclusive},
                   {"ModifiedTime", updateSKUClassGroupItems.ModifiedTime},
                   {"ServerModifiedTime", updateSKUClassGroupItems.ServerModifiedTime},
                   {"MinQTY", updateSKUClassGroupItems.MinQTY},
                   {"MaxQTY", updateSKUClassGroupItems.MaxQTY},
                 };
            return await ExecuteNonQueryAsync(sql, connection,transaction, parameters);
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
                {"UID" , UID}
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
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int i = 0;
            UIDs.ForEach((e) => { parameters.Add($"UID{i}", e); i++; });

            var sql = @$"DELETE FROM sku_class_group_items WHERE uid IN ({string.Join(",", UIDs.Select((uid, index) => $"@UID{index}"))})";

            return await ExecuteNonQueryAsync(sql,connection,transaction, parameters);
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
            var sql = new StringBuilder(@$"SELECT O.name as PlantName, S.name as SKUName,SCGI.id AS Id,
                SCGI.uid AS UID,
                SCGI.created_by AS CreatedBy,
                SCGI.created_time AS CreatedTime,
                SCGI.modified_by AS ModifiedBy,
                SCGI.modified_time AS ModifiedTime,
                SCGI.server_add_time AS ServerAddTime,
                SCGI.server_modified_time AS ServerModifiedTime,
                SCGI.sku_class_group_uid AS SKUClassGroupUID,
                SCGI.sku_code AS SKUCode,
                SCGI.serial_number AS SerialNumber,
                SCGI.model_qty AS ModelQty,
                SCGI.model_uom AS ModelUoM,
                SCGI.supplier_org_uid AS SupplierOrgUID,
                SCGI.lead_time_in_days AS LeadTimeInDays,
                SCGI.daily_cut_off_time AS DailyCutOffTime,
                SCGI.is_exclusive AS IsExclusive,
                SCGI.max_qty AS MaxQTY,
                SCGI.min_qty AS MinQTY FROM sku_class_group_items SCGI 
                INNER JOIN sku S ON SCGI.sku_code = S.code LEFT JOIN org O ON 
                O.uid = SCGI.supplier_org_uid");
            var parameters = new Dictionary<string, object>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                
                // Manual filter handling for sku_class_group_uid
                bool hasManualFilter = false;
                foreach (var filter in filterCriterias)
                {
                    if (filter.Name?.ToLower() == "sku_class_group_uid")
                    {
                        if (hasManualFilter) sbFilterCriteria.Append(" AND ");
                        sbFilterCriteria.Append($"SCGI.sku_class_group_uid = @GroupUID");
                        parameters["GroupUID"] = filter.Value;
                        hasManualFilter = true;
                    }
                    else if (filter.Name?.ToLower() == "sku_code")
                    {
                        if (hasManualFilter) sbFilterCriteria.Append(" AND ");
                        sbFilterCriteria.Append($"SCGI.sku_code LIKE @SKUCode");
                        parameters["SKUCode"] = $"%{filter.Value}%";
                        hasManualFilter = true;
                    }
                }
                
                // If no manual filters were added, use the base implementation
                if (!hasManualFilter)
                {
                    AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroupItems>
                        (filterCriterias, sbFilterCriteria, parameters);
                }
                
                sql.Append(sbFilterCriteria);
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql,true);
            }

            if (pageSize > 0)
            {
                // Ensure ORDER BY exists for PostgreSQL pagination
                if (sortCriterias == null || sortCriterias.Count == 0)
                {
                    sql.Append(" ORDER BY id");
                }
                // Use PostgreSQL pagination syntax (pageNumber is 0-based from API)
                int offset = pageNumber * pageSize;
                sql.Append($" LIMIT {pageSize} OFFSET {offset}");
            }

            Type type = _serviceProvider.GetRequiredService<Model.UIInterfaces.ISKUClassGroupItemView>().GetType();
            IEnumerable<Model.UIInterfaces.ISKUClassGroupItemView> sKUClassGroupItemss =
                await ExecuteQueryAsync<Model.UIInterfaces.ISKUClassGroupItemView>(sql.ToString(), parameters, type);

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
        throw new NotImplementedException();
    }

    public async Task<List<string>> GetApplicableAllowedSKUGroupUIDs(string storeUID)
    {
        throw new NotImplementedException();
    }

    public async Task<List<string>> GetApplicableAllowedSKUGBySKUClassGroupUID(string skuClassGroupUID)
    {
        throw new NotImplementedException();
    }
}



