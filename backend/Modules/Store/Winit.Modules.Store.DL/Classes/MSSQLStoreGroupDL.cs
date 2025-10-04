using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.Store.DL.Classes;

public class MSSQLStoreGroupDL : Base.DL.DBManager.SqlServerDBManager, Interfaces.IStoreGroupDL
{
    public MSSQLStoreGroupDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Model.Interfaces.IStoreGroup>> SelectAllStoreGroup
   (List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new("""
                                    SELECT * FROM 
                ( SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime, store_group_type_uid AS StoreGroupTypeUID,
                code AS Code, name AS Name, parent_uid AS ParentUID, item_level AS ItemLevel, CASE 
                WHEN EXISTS (SELECT 1 FROM store_group ch WHERE ch.parent_uid = sg.uid) 
                THEN 1 ELSE 0 
                END AS HasChild FROM store_group sg) AS SUBQUERY
                """);
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder("""
                                              SELECT * FROM 
                     ( SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, 
                     modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime,
                     server_modified_time AS ServerModifiedTime, store_group_type_uid AS StoreGroupTypeUID,
                     code AS Code, name AS Name, parent_uid AS ParentUID, item_level AS ItemLevel, CASE 
                     WHEN EXISTS (SELECT 1 FROM store_group ch WHERE ch.parent_uid = sg.uid) 
                     THEN 1 ELSE 0 
                     END AS HasChild FROM store_group sg) AS SUBQUERY
                     """);
            }
            Dictionary<string, object?> parameters = [];

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                FilterCriteria? filterCriteria = filterCriterias.Find(e => e.Name == "store_group_hierarchy_level_code_name");

                if (filterCriteria != null)
                {
                    _ = sbFilterCriteria.Append(" (CONVERT(varchar,code,120) Like '%' + @code + '%' OR CONVERT(varchar,name,120) LIKE '%' + @name + '%') ");
                    parameters.Add("code", filterCriteria.Value);
                    parameters.Add("name", filterCriteria.Value);
                    _ = filterCriterias.Remove(filterCriteria);
                    if (filterCriterias.Any())
                    {
                        _ = sbFilterCriteria.Append(" AND ");
                    }
                }
                AppendFilterCriteria<Model.Interfaces.IStoreGroup>(filterCriterias, sbFilterCriteria, parameters); ;

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortCriterias, sql);
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    _ = sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }
            IEnumerable<Model.Interfaces.IStoreGroup> StoreGroups = await ExecuteQueryAsync<Model.Interfaces.IStoreGroup>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreGroup> pagedResponse = new()
            {
                PagedData = StoreGroups,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
    {
        try
        {
            string sql = @"INSERT INTO store_group (UID, created_by, created_time, modified_by, modified_time,
                         server_add_time, server_modified_time, store_group_type_UID, code, name, parent_UID, item_level) VALUES
                         (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 
                         @StoreGroupTypeUID, @Code, @Name, @ParentUID, @ItemLevel)";

            return await ExecuteNonQueryAsync(sql, storeGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> InsertStoreGroupHierarchy(string type, string uid)
    {
        int retValue;
        try
        {
            string hierarchySql = @"WITH store_group_hierarchy AS (
                                    SELECT 
                                        sg1.id AS Id, 
                                        sg1.uid AS UID, 
                                        sg1.created_by AS CreatedBy, 
                                        sg1.created_time AS CreatedTime, 
                                        sg1.modified_by AS ModifiedBy,
                                        sg1.modified_time AS ModifiedTime, 
                                        sg1.server_add_time AS ServerAddTime, 
                                        sg1.server_modified_time AS ServerModifiedTime,
                                        sg1.store_group_type_uid AS GroupTypeUID, 
                                        sg1.code AS GroupCode, 
                                        sg1.name AS GroupName, 
                                        sg1.parent_uid AS GroupParentUID, 
                                        sg1.item_level AS ItemLevel, 
                                        sgt1.name AS GroupTypeName, 
                                        sgt1.code AS GroupTypeCode, 
                                        sgt1.parent_uid AS GroupTypeParentUID, 
                                        sgt1.level_no AS LevelNo
                                    FROM 
                                        store_group sg1
                                    INNER JOIN 
                                        store_group_type sgt1 ON sg1.store_group_type_uid = sgt1.uid
                                    WHERE 
                                        sg1.uid = @UID 
                                        AND sgt1.name = @Type
                                
                                    UNION ALL
                                
                                    SELECT 
                                        sg2.id AS Id, 
                                        sg2.uid AS UID, 
                                        sg2.created_by AS CreatedBy, 
                                        sg2.created_time AS CreatedTime, 
                                        sg2.modified_by AS ModifiedBy,
                                        sg2.modified_time AS ModifiedTime, 
                                        sg2.server_add_time AS ServerAddTime, 
                                        sg2.server_modified_time AS ServerModifiedTime,
                                        sg2.store_group_type_uid AS GroupTypeUID, 
                                        sg2.code AS GroupCode, 
                                        sg2.name AS GroupName, 
                                        sg2.parent_uid AS GroupParentUID, 
                                        sg2.item_level AS ItemLevel, 
                                        sgt2.name AS GroupTypeName, 
                                        sgt2.code AS GroupTypeCode, 
                                        sgt2.parent_uid AS GroupTypeParentUID, 
                                        sgt2.level_no AS LevelNo
                                    FROM 
                                        store_group sg2
                                    INNER JOIN 
                                        store_group_type sgt2 ON sgt2.uid = sg2.store_group_type_uid
                                    INNER JOIN 
                                        store_group_hierarchy sgh ON sg2.uid = sgh.GroupParentUID)
                                            SELECT  * FROM store_group_hierarchy;";

            Dictionary<string, object> parameters = new()
            {
                {"UID", uid},
                {"Type", type }
            };

            List<IGroupHierarchy> storeGroupHierarchy = await ExecuteQueryAsync<IGroupHierarchy>(hierarchySql, parameters);

            string insertSql = @"
                    INSERT INTO store_group_data (
                        uid, ss, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, primary_uid, json_data, label, primary_type, primary_label,
                        l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11, l12, l13, l14, l15, l16, l17, l18, l19, l20
                    ) VALUES (
                        @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @PrimaryUid, @JsonData, @Label, @PrimaryType, @PrimaryLabel,
                        @L1, @L2, @L3, @L4, @L5, @L6, @L7, @L8, @L9, @L10, @L11, @L12, @L13, @L14, @L15, @L16, @L17, @L18, @L19, @L20
                    );";
            Dictionary<string, object> insertParameters = new()
            {
                {"UID", Guid.NewGuid().ToString()},
                {"SS", 0},
                {"CreatedBy", storeGroupHierarchy.FirstOrDefault()?.CreatedBy},
                {"CreatedTime", DateTime.UtcNow},
                {"ModifiedBy", storeGroupHierarchy.FirstOrDefault()?.ModifiedBy},
                {"ModifiedTime", DateTime.UtcNow},
                {"ServerAddTime", DateTime.UtcNow},
                {"ServerModifiedTime", DateTime.UtcNow},
                {"PrimaryUid", uid},
                {"PrimaryType", type},
                {"PrimaryLabel", storeGroupHierarchy.FirstOrDefault()?.GroupName }
            };

            for (int i = 1; i <= 20; i++)
            {
                insertParameters[$"L{i}"] = null;
            }
            List<Dictionary<string, object>> customStoreGroupDataList = new();
            StringBuilder labelBuilder = new();

            foreach (IGroupHierarchy storeGroup in storeGroupHierarchy)
            {
                if (storeGroup.LevelNo is >= 1 and <= 20)
                {
                    insertParameters[$"L{storeGroup.LevelNo}"] = storeGroup.UID;
                }

                if (labelBuilder.Length > 0)
                {
                    _ = labelBuilder.Insert(0, " -> ");
                }
                _ = labelBuilder.Insert(0, storeGroup.GroupName);

                Dictionary<string, object> customData = new()
                {
                    {"UID", storeGroup.UID},
                    {"StoreGroupTypeName", storeGroup.GroupTypeName},
                    {"Level", storeGroup.LevelNo},
                    {"Label", $"[{storeGroup.GroupCode}] {storeGroup.GroupName}"}
                };

                customStoreGroupDataList.Add(customData);
            }

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customStoreGroupDataList);

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
    public async Task<int> DeleteStoreGroup(string UID)
    {
        Dictionary<string, object?> parameters = new()
    {
        {"UID",UID}
    };
        string sql = @"DELETE  FROM store_group WHERE uid = @UID";

        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<Model.Interfaces.IStoreGroup?> SelectStoreGroupByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
    {
        {"UID",  UID}
    };
        string sql = @"SELECT id AS Id, uid AS UID, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, 
                           modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime,
                           store_group_type_uid AS StoreGroupTypeUID,code AS Code, name AS Name, parent_uid AS ParentUID, item_level AS ItemLevel
	                       FROM store_group WHERE uid = @UID";
        return await ExecuteSingleAsync<Model.Interfaces.IStoreGroup>(sql, parameters);
    }
    public async Task<int> UpdateStoreGroup(Model.Interfaces.IStoreGroup storeGroup)
    {
        try
        {
            string sql = @"UPDATE store_group SET 
                           modified_by=@ModifiedBy
                           ,modified_time=@ModifiedTime
                           ,server_modified_time=@ServerModifiedTime
                           ,store_group_type_uid=@StoreGroupTypeUID
                           ,code=@Code
                           ,name=@Name
                           ,parent_uid=@ParentUID
                            ,item_level=@ItemLevel
                            WHERE uid=@UID ";
            return await ExecuteNonQueryAsync(sql, storeGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }
}








