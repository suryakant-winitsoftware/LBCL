using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class PGSQLSKUGroupDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUGroupDL
{
    public PGSQLSKUGroupDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> SelectAllSKUGroupDetails(List<SortCriteria>
        sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"select * from(SELECT 
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
            StringBuilder sqlCount = new();
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
            Dictionary<string, object?> parameters = [];

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql, true);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUGroup>().GetType();
            IEnumerable<Model.Interfaces.ISKUGroup> skuGroupList = await ExecuteQueryAsync<Model.Interfaces.ISKUGroup>(sql.ToString(), parameters, type);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroup> pagedResponse = new()
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
        Dictionary<string, object> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"SELECT 
                    sku_group_type_uid AS SkuGroupTypeUID,
                    code AS Code,
                    name AS Name,
                    parent_uid AS ParentUID,
                    item_level AS ItemLevel,
                    supplier_org_uid AS SupplierOrgUID
            FROM sku_group
            WHERE uid= @UID";
        return await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(sql, parameters);
    }
    public async Task<int> CreateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        try
        {
            string sql = @"INSERT INTO sku_group (uid, created_by, created_time, modified_by, modified_time,
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
        int retValue;
        try
        {
            string hierarchySql = @"
                    WITH RECURSIVE sku_group_hierarchy AS (
                    SELECT sg1.id AS Id, sg1.uid AS UID, sg1.created_by AS CreatedBy, sg1.created_time AS CreatedTime, sg1.modified_by as ModifiedBy,
                    sg1.modified_time AS ModifiedTime, sg1.server_add_time AS ServerAddTime, sg1.server_modified_time AS ServerModifiedTime,
                    sg1.sku_group_type_uid AS GroupTypeUID, sg1.code AS GroupCode, sg1.name As GroupName, sg1.parent_uid AS GroupParentUID, 
                    sg1.item_level AS ItemLevel, sgt1.name AS GroupTypeName, sgt1.code AS GroupTypeCode, 
                    sgt1.parent_uid AS GroupTypeParentUID, sgt1.item_level AS LevelNo
                    FROM sku_group sg1
                    INNER JOIN sku_group_type sgt1 on sg1.sku_group_type_uid = sgt1.uid
                    WHERE sg1.uid = @UID AND sgt1.name = @Type
                    UNION ALL
                    SELECT sg2.id AS Id, sg2.uid AS UID, sg2.created_by AS CreatedBy, sg2.created_time AS CreatedTime, sg2.modified_by AS ModifiedBy,
                    sg2.modified_time AS ModifiedTime, sg2.server_add_time AS ServerAddTime, sg2.server_modified_time AS ServerModifiedTime,
                    sg2.sku_group_type_uid AS GroupTypeUID, sg2.code AS GroupCode, sg2.name AS GroupName, sg2.parent_uid AS GroupParentUID, 
                    sg2.item_level AS ItemLevel, sgt2.name AS GroupTypeName, sgt2.code AS GroupTypeCode, 
                    sgt2.parent_uid AS GroupTypeParentUID, sgt2.item_level AS LevelNo
                    FROM sku_group sg2
                    INNER JOIN sku_group_type sgt2 ON sgt2.uid = sg2.sku_group_type_uid
                    INNER JOIN sku_group_hierarchy sgh ON sg2.uid = sgh.GroupParentUID
                    )
                    SELECT * FROM sku_group_hierarchy;";

            Dictionary<string, object> parameters = new()
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
            Dictionary<string, object> insertParameters = new()
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
            List<Dictionary<string, object>> customSKUGroupDataList = [];
            StringBuilder labelBuilder = new();

            foreach (IGroupHierarchy skuGroup in skuGroupHierarchy)
            {
                if (skuGroup.LevelNo is >= 1 and <= 20)
                {
                    insertParameters[$"L{skuGroup.LevelNo}"] = skuGroup.UID;
                }

                if (labelBuilder.Length > 0)
                {
                    _ = labelBuilder.Insert(0, " -> ");
                }
                _ = labelBuilder.Insert(0, skuGroup.GroupName);

                Dictionary<string, object> customData = new()
                {
                    {"UID", skuGroup.UID},
                    {"SKUGroupTypeName", skuGroup.GroupTypeName},
                    {"Level", skuGroup.LevelNo},
                    {"Label", $"[{skuGroup.GroupCode}] {skuGroup.GroupName}"}
                };

                customSKUGroupDataList.Add(customData);
            }

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customSKUGroupDataList);

            insertParameters["JsonData"] = jsonData;
            insertParameters["Label"] = labelBuilder.ToString();
            retValue = await ExecuteNonQueryAsync(insertSql, insertParameters);

        }
        catch (Exception)
        {
            throw;
        }
        return retValue;
    }
    public async Task<int> UpdateSKUGroup(Winit.Modules.SKU.Model.Interfaces.ISKUGroup sKUGroup)
    {
        try
        {
            string sql = @"UPDATE sku_group
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
            Dictionary<string, object> parameters = new()
            {
               {"UID",sKUGroup.UID},
               {"ModifiedBy",sKUGroup.ModifiedBy},
               {"ModifiedTime",sKUGroup.ModifiedTime},
               {"ServerModifiedTime",sKUGroup.ServerModifiedTime},
               {"Code",sKUGroup.Code},
               {"Name",sKUGroup.Name},
               {"ParentUID",sKUGroup.ParentUID},
               {"ItemLevel",sKUGroup.ItemLevel},
               {"SupplierOrgUID",sKUGroup.SupplierOrgUID},
               {"SKUGroupTypeUID",sKUGroup.SKUGroupTypeUID},
            };
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKUGroup(string UID)
    {
        Dictionary<string, object> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"DELETE FROM sku_group WHERE uid = @UID;";
        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>> SelectSKUGroupView()
    {
        Dictionary<string, object> parameters = [];
        string sql = @"SELECT id AS Id,
                   uid AS UID,
                   sku_group_type_uid AS SkuGroupTypeUID,
                   code AS Code,
                   name AS Name,
                   parent_uid AS ParentUID,
                   item_level AS ItemLevel,
                   supplier_org_uid AS SupplierOrgUID
            FROM sku_group;";
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUGroupView>().GetType();
        IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView> skGroupDetails = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroupView>(sql, parameters, type);
        return skGroupDetails;
    }
    public async Task<List<SKUGroupSelectionItem>> GetSKUGroupSelectionItemBySKUGroupTypeUID(string skuGroupTypeUID, string parentUID)
    {
        try
        {
            StringBuilder sql = new("select name as label, uid as UID, parent_uid as ParentCode, code as Code from sku_group");
            ;
            Dictionary<string, object?> parameters = [];

            if (!string.IsNullOrEmpty(skuGroupTypeUID))
            {
                parameters.Add("SKUGroupTypeUID", skuGroupTypeUID);
                _ = sql.Append(" where sku_group_type_uid = @SKUGroupTypeUID");
            }
            else
            {
                parameters.Add("ParentUID", parentUID);
                _ = sql.Append(" where parent_uid = @ParentUID");
            }
            return await ExecuteQueryAsync<SKUGroupSelectionItem>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>> GetSKUGroupBySKUGroupTypeUID(string skuGroupTypeUID)
    {
        try
        {
            StringBuilder sql = new(@"SELECT 
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
            ;
            Dictionary<string, object?> parameters = new()
            {
                { "SkuGroupTypeUID", skuGroupTypeUID },
                { "SKUGroupTypeUID", skuGroupTypeUID }
            };

            return await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<Winit.Modules.SKU.Model.UIInterfaces.ISKUGroupItemView>> SelectAllSKUGroupItemViews(List<SortCriteria>
        sortCriterias, List<FilterCriteria> filterCriterias)
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
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM sku_group ch WHERE ch.parent_uid = sg.uid) 
                        THEN 1 ELSE 0 
                    END AS HasChild
                    FROM sku_group sg ) as subquery
                """);

            Dictionary<string, object?> parameters = [];

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                if (filterCriterias.Any(e => e.Name == "product_hierarchylevel_code_name"))
                {
                    sbFilterCriteria.Append(" (code = @Code or name = @Name) ");
                    FilterCriteria filter = filterCriterias.Find(e => e.Name == "product_hierarchylevel_code_name")!;
                    parameters.Add("Code", filter.Value);
                    parameters.Add("Name", filter.Value);
                    filterCriterias.Remove(filter);
                    if (filterCriterias.Any()) sbFilterCriteria.Append(" AND ");
                }
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUGroup>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql, true);
            }

            return await ExecuteQueryAsync<Model.UIInterfaces.ISKUGroupItemView>(sql.ToString(), parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }

}
