using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class MSSQLSKUGroupDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUGroupDL
{
    public MSSQLSKUGroupDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> SelectAllSKUGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"select * from(SELECT 
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        sku_group_type_uid AS SkuGroupTypeUID,
                        code AS Code,
                        name AS Name,
                        parent_uid AS ParentUID,
                        item_level AS ItemLevel,
                        supplier_org_uid AS SupplierOrgUID
                FROM sku_group) as subquery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                        id AS Id,
                        uid AS UID,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime,
                        sku_group_type_uid AS SkuGroupTypeUID,
                        code AS Code,
                        name AS Name,
                        parent_uid AS ParentUID,
                        item_level AS ItemLevel,
                        supplier_org_uid AS SupplierOrgUID
                FROM sku_group) as subquery");
            }
            var parameters = new Dictionary<string, object?>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(filterCriterias, sbFilterCriteria, parameters);

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
            IEnumerable<Model.Interfaces.ISKUGroup> skuGroupList = await ExecuteQueryAsync<Model.Interfaces.ISKUGroup>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>
            {
                PagedData = skuGroupList,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> SelectSKUGroupByUID(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID" , UID}
        };
        var sql = @"SELECT 
                    sku_group_type_uid AS SkuGroupTypeUID,
                    code AS Code,
                    name AS Name,
                    parent_uid AS ParentUID,
                    item_level AS ItemLevel,
                    supplier_org_uid AS SupplierOrgUID
            FROM sku_group
            WHERE uid= @UID";
        Winit.Modules.SKU.Model.Interfaces.ISKUGroup skGroupDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(sql, parameters);
        return skGroupDetails;
    }
    public async Task<int> CreateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        try
        {
            var sql = @"INSERT INTO sku_group (uid, created_by, created_time, modified_by, modified_time,
                            server_add_time, server_modified_time, sku_group_type_uid, code, name, parent_uid, item_level,
                            supplier_org_uid) Values(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,
                            @ServerModifiedTime,@SKUGroupTypeUID,@Code,@Name,@ParentUID,@ItemLevel,@SupplierOrgUID);";
            return await ExecuteNonQueryAsync(sql, sKUGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> InsertSKUGroupHierarchy(string type, string uid)
    {
        int retValue = 0;
        try
        {
            string hierarchySql = @"WITH sku_group_hierarchy AS (
                            SELECT sg1.id AS Id, sg1.uid AS UID, sg1.created_by AS CreatedBy, sg1.created_time AS CreatedTime, 
                                   sg1.modified_by AS ModifiedBy, sg1.modified_time AS ModifiedTime, sg1.server_add_time AS ServerAddTime, 
                                   sg1.server_modified_time AS ServerModifiedTime, sg1.sku_group_type_uid AS GroupTypeUID, sg1.code AS GroupCode, 
                                   sg1.name AS GroupName, sg1.parent_uid AS GroupParentUID, sg1.item_level AS ItemLevel, sgt1.name AS GroupTypeName, 
                                   sgt1.code AS GroupTypeCode, sgt1.parent_uid AS GroupTypeParentUID, sgt1.item_level AS LevelNo
                            FROM sku_group sg1
                            INNER JOIN sku_group_type sgt1 ON sg1.sku_group_type_uid = sgt1.uid
                            WHERE sg1.uid = @UID AND sgt1.name = @Type
                        
                            UNION ALL
                        
                            SELECT sg2.id AS Id, sg2.uid AS UID, sg2.created_by AS CreatedBy, sg2.created_time AS CreatedTime, 
                                   sg2.modified_by AS ModifiedBy, sg2.modified_time AS ModifiedTime, sg2.server_add_time AS ServerAddTime, 
                                   sg2.server_modified_time AS ServerModifiedTime, sg2.sku_group_type_uid AS GroupTypeUID, sg2.code AS GroupCode, 
                                   sg2.name AS GroupName, sg2.parent_uid AS GroupParentUID, sg2.item_level AS ItemLevel, sgt2.name AS GroupTypeName, 
                                   sgt2.code AS GroupTypeCode, sgt2.parent_uid AS GroupTypeParentUID, sgt2.item_level AS LevelNo
                            FROM sku_group sg2
                            INNER JOIN sku_group_type sgt2 ON sgt2.uid = sg2.sku_group_type_uid
                            INNER JOIN sku_group_hierarchy sgh ON sg2.uid = sgh.GroupParentUID)
                        SELECT * FROM sku_group_hierarchy";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", uid},
                {"Type", type }
            };

            List<IGroupHierarchy> skuGroupHierarchy = await ExecuteQueryAsync<IGroupHierarchy>(hierarchySql, parameters);

            string insertSql = @"
                    INSERT INTO sku_group_data (
                        uid, ss, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, primary_uid, json_data, label, primary_type, primary_label,
                        l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11, l12, l13, l14, l15, l16, l17, l18, l19, l20
                    ) VALUES (
                        @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @PrimaryUid, CAST(@JsonData AS json), @Label, @PrimaryType, @PrimaryLabel,
                        @L1, @L2, @L3, @L4, @L5, @L6, @L7, @L8, @L9, @L10, @L11, @L12, @L13, @L14, @L15, @L16, @L17, @L18, @L19, @L20
                    );";
            var insertParameters = new Dictionary<string, object>
            {
                {"UID", Guid.NewGuid().ToString()},
                {"SS", 0},
                {"CreatedBy", skuGroupHierarchy.FirstOrDefault()?.CreatedBy},
                {"CreatedTime", DateTime.UtcNow},
                {"ModifiedBy", skuGroupHierarchy.FirstOrDefault()?.ModifiedBy},
                {"ModifiedTime", DateTime.UtcNow},
                {"ServerAddTime", DateTime.UtcNow},
                {"ServerModifiedTime", DateTime.UtcNow},
                {"PrimaryUid", uid},
                {"PrimaryType", type},
                {"PrimaryLabel", skuGroupHierarchy.FirstOrDefault()?.GroupName }
            };

            for (int i = 1; i <= 20; i++)
            {
                insertParameters[$"L{i}"] = null;
            }
            var customSKUGroupDataList = new List<Dictionary<string, object>>();
            var labelBuilder = new StringBuilder();

            foreach (var skuGroup in skuGroupHierarchy)
            {
                if (skuGroup.LevelNo >= 1 && skuGroup.LevelNo <= 20)
                {
                    insertParameters[$"L{skuGroup.LevelNo}"] = skuGroup.UID;
                }

                if (labelBuilder.Length > 0)
                {
                    labelBuilder.Insert(0, " -> ");
                }
                labelBuilder.Insert(0, skuGroup.GroupName);

                var customData = new Dictionary<string, object>
                {
                    {"UID", skuGroup.UID},
                    {"SKUGroupTypeName", skuGroup.GroupTypeName},
                    {"Level", skuGroup.LevelNo},
                    {"Label", $"[{skuGroup.GroupCode}] {skuGroup.GroupName}"}
                };

                customSKUGroupDataList.Add(customData);
            }

            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customSKUGroupDataList);

            insertParameters["JsonData"] = jsonData;
            insertParameters["Label"] = labelBuilder.ToString();
            retValue = await ExecuteNonQueryAsync(insertSql, insertParameters);

        }
        catch (Exception ex)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        try
        {
            var sql = @"UPDATE sku_group
                SET modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    sku_group_type_uid = @SKUGroupTypeUID,
                    code = @Code,
                    name = @Name,
                    parent_uid = @ParentUID,
                    item_level = @ItemLevel,
                    supplier_org_uid = @SupplierOrgUID
                WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, sKUGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKUGroup(string UID)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"UID" , UID}
        };
        var sql = @"DELETE FROM sku_group WHERE uid = @UID;";
        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>> SelectSKUGroupView()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        var sql = @"SELECT id AS Id,
                   uid AS UID,
                   sku_group_type_uid AS SkuGroupTypeUID,
                   code AS Code,
                   name AS Name,
                   parent_uid AS ParentUID,
                   item_level AS ItemLevel,
                   supplier_org_uid AS SupplierOrgUID
            FROM sku_group;";
        IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView> skGroupDetails = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>(sql, parameters);
        return skGroupDetails;
    }
    public async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string skuGroupTypeUID, string parentUID)
    {
        try
        {
            StringBuilder sql = new StringBuilder("select name as label, uid as UID, parent_uid as ParentCode, code as Code from sku_group");
            ;
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(skuGroupTypeUID))
            {
                parameters.Add("SKUGroupTypeUID", skuGroupTypeUID);
                sql.Append(" where sku_group_type_uid = @SKUGroupTypeUID");
            }
            else
            {
                parameters.Add("ParentUID", parentUID);
                sql.Append(" where parent_uid = @ParentUID");
            }
            return await ExecuteQueryAsync<SKUGroupSelectionItem>(sql.ToString(), parameters);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetSKUGroupBySKUGroupTypeUID(string skuGroupTypeUID)
    {
        try
        {
            StringBuilder sql = new StringBuilder(@"SELECT 
                        ch.id AS Id,
                        ch.uid AS UID,
                        ch.created_by AS CreatedBy,
                        ch.created_time AS CreatedTime,
                        ch.modified_by AS ModifiedBy,
                        ch.modified_time AS ModifiedTime,
                        ch.server_add_time AS ServerAddTime,
                        ch.server_modified_time AS ServerModifiedTime,
                        ch.sku_group_type_uid AS SkuGroupTypeUID,
                        ch.code AS Code,
                        ch.name AS Name,
                        ch.parent_uid AS ParentUID,
						PA.name as ParentName,
                        ch.item_level AS ItemLevel,
                        ch.supplier_org_uid AS SupplierOrgUID
                FROM sku_group ch
                LEFT JOIN sku_group PA on ch.parent_uid = PA.UID 
                WHERE ch.sku_group_type_uid=@SkuGroupTypeUID");
            
            Dictionary<string, object?> parameters = new Dictionary<string, object?>()
            {
                {"SkuGroupTypeUID",skuGroupTypeUID }
            };
            return await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(sql.ToString(), parameters);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<ISKUGroupItemView>> SelectAllSKUGroupItemViews(List<SortCriteria> sortCriterias, List<FilterCriteria> filterCriterias)
    {
        try
        {
            StringBuilder sql =
                new("""
                    SELECT * FROM (SELECT 
                    sg.id AS Id,
                    sg.uid AS UID,
                    sg.created_by AS CreatedBy,
                    sg.created_time AS CreatedTime,
                    sg.modified_by AS ModifiedBy,
                    sg.modified_time AS ModifiedTime,
                    sg.server_add_time AS ServerAddTime,
                    sg.server_modified_time AS ServerModifiedTime,
                    sg.sku_group_type_uid AS SkuGroupTypeUID,
                    sg.code AS Code,
                    sg.name AS Name,
                    sg.parent_uid AS ParentUID,
                    sg.item_level AS ItemLevel,
                    sg.supplier_org_uid AS SupplierOrgUID,
                    sgt.name AS skugrouptypename,
                    sgt.code AS skugrouptypecode,
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM sku_group ch WHERE ch.parent_uid = sg.uid) 
                        THEN 1 ELSE 0 
                    END AS HasChild
                    FROM sku_group sg Left JOIN sku_group_type sgt ON sg.sku_group_type_UID = sgt.uid ) as subquery
                """);

            Dictionary<string, object?> parameters = [];

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                if (filterCriterias.Any(e => e.Name == "product_hierarchylevel_code_name"))
                {
                    sbFilterCriteria.Append(" (CONVERT(varchar,code,120) Like '%' + @code + '%' OR CONVERT(varchar,name,120) LIKE '%' + @name + '%')  ");
                    FilterCriteria filter = filterCriterias.Find(e => e.Name == "product_hierarchylevel_code_name")!;
                    parameters.Add("code", filter.Value);
                    parameters.Add("name", filter.Value);
                    filterCriterias.Remove(filter);
                    if (filterCriterias.Any()) sbFilterCriteria.Append(" AND ");
                }
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            return await ExecuteQueryAsync<Model.UIInterfaces.ISKUGroupItemView>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
