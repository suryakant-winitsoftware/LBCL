using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class MSSQLLocationMappingDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ILocationMappingDL
    {
        public MSSQLLocationMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>> SelectAllLocationMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                                                        id AS Id,
                                                        uid AS Uid,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime,
                                                        linked_item_uid AS LinkedItemUid,
                                                        linked_item_type AS LinkedItemType,
                                                        location_type_uid AS LocationTypeUid,
                                                        location_uid AS LocationUid
                                                    FROM location_mapping) as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                        id AS Id,
                                                        uid AS Uid,
                                                        created_by AS CreatedBy,
                                                        created_time AS CreatedTime,
                                                        modified_by AS ModifiedBy,
                                                        modified_time AS ModifiedTime,
                                                        server_add_time AS ServerAddTime,
                                                        server_modified_time AS ServerModifiedTime,
                                                        linked_item_uid AS LinkedItemUid,
                                                        linked_item_type AS LinkedItemType,
                                                        location_type_uid AS LocationTypeUid,
                                                        location_uid AS LocationUid
                                                    FROM location_mapping) as SUBQUERY");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.ILocationMapping>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocationMapping> locationMappingDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocationMapping>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationMapping>
                {
                    PagedData = locationMappingDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationMapping> GetLocationMappingByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT 
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            linked_item_uid AS LinkedItemUid,
                            linked_item_type AS LinkedItemType,
                            location_type_uid AS LocationTypeUid,
                            location_uid AS LocationUid
                        FROM location_mapping WHERE UID = @UID";
            Winit.Modules.Location.Model.Interfaces.ILocationMapping? locationMapping = await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocationMapping>(sql, parameters);
            return locationMapping;
        }
        public async Task<int> CreateLocationMapping(Winit.Modules.Location.Model.Interfaces.ILocationMapping createLocationMapping)
        {
            try
            {
                var sql = @"INSERT INTO location_mapping (
                                                        id, 
                                                        uid, 
                                                        created_by, 
                                                        created_time, 
                                                        modified_by, 
                                                        modified_time, 
                                                        server_add_time, 
                                                        server_modified_time, 
                                                        linked_item_uid, 
                                                        linked_item_type, 
                                                        location_type_uid, 
                                                        location_uid) 
                VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @LinkedItemUID,
                        @LocationTypeUID, @LinkedItemType, @LocationUID)";
                return await ExecuteNonQueryAsync(sql, createLocationMapping);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<int> InsertLocationHierarchy(string type, string uid)
        {
            int retValue = 0;
            try
            {
                string hierarchySql = @"
                                                WITH location_hierarchy AS (
                                -- Anchor member
                                SELECT 
                                    l1.id AS Id, 
                                    l1.uid AS UID, 
                                    l1.created_by AS CreatedBy, 
                                    l1.created_time AS CreatedTime, 
                                    l1.modified_by AS ModifiedBy,
                                    l1.modified_time AS ModifiedTime, 
                                    l1.server_add_time AS ServerAddTime, 
                                    l1.server_modified_time AS ServerModifiedTime,
                                    l1.company_uid AS CompanyUID, 
                                    l1.location_type_uid AS LocationTypeUID, 
                                    l1.code AS LocationCode, 
                                    l1.name AS LocationName, 
                                    l1.parent_uid AS LocationParentUID, 
                                    l1.item_level AS ItemLevel, 
                                    lt1.name AS LocationTypeName, 
                                    lt1.code AS LocationTypeCode, 
                                    lt1.parent_uid AS LocationTypeParentUID, 
                                    lt1.level_no AS LevelNo
                                FROM location l1
                                INNER JOIN location_type lt1 ON l1.location_type_uid = lt1.uid
                                WHERE l1.uid = @UID AND lt1.name = @Type
                            
                                UNION ALL
                            
                                -- Recursive member
                                SELECT 
                                    l2.id AS Id, 
                                    l2.uid AS UID, 
                                    l2.created_by AS CreatedBy, 
                                    l2.created_time AS CreatedTime, 
                                    l2.modified_by AS ModifiedBy,
                                    l2.modified_time AS ModifiedTime, 
                                    l2.server_add_time AS ServerAddTime, 
                                    l2.server_modified_time AS ServerModifiedTime,
                                    l2.company_uid AS CompanyUID, 
                                    l2.location_type_uid AS LocationTypeUID, 
                                    l2.code AS LocationCode, 
                                    l2.name AS LocationName, 
                                    l2.parent_uid AS LocationParentUID, 
                                    l2.item_level AS ItemLevel, 
                                    lt2.name AS LocationTypeName, 
                                    lt2.code AS LocationTypeCode, 
                                    lt2.parent_uid AS LocationTypeParentUID, 
                                    lt2.level_no AS LevelNo
                                FROM location l2
                                INNER JOIN location_hierarchy lh ON l2.uid = lh.LocationParentUID
                                INNER JOIN location_type lt2 ON l2.location_type_uid = lt2.uid AND lt2.show_in_ui = 1 
                            )
                            
                            SELECT * 
                            FROM location_hierarchy;";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID", uid},
                    {"Type", type }
                };

                List<ILocationHierarchy> locationHierarchy = await ExecuteQueryAsync<ILocationHierarchy>(hierarchySql, parameters);

                string insertSql = @"
                    INSERT INTO location_data (
                        uid, ss, created_by, created_time, modified_by, modified_time, 
                        server_add_time, server_modified_time, primary_uid, json_data, label, primary_type, primary_label,
                        l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11, l12, l13, l14, l15, l16, l17, l18, l19, l20
                    ) VALUES (
                        @UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, 
                        @ServerAddTime, @ServerModifiedTime, @PrimaryUid, @JsonData, @Label, @PrimaryType, @PrimaryLabel,
                        @L1, @L2, @L3, @L4, @L5, @L6, @L7, @L8, @L9, @L10, @L11, @L12, @L13, @L14, @L15, @L16, @L17, @L18, @L19, @L20
                    );";


                var insertParameters = new Dictionary<string, object>
                {
                    {"UID", Guid.NewGuid().ToString()},
                    {"SS", 0},
                    {"CreatedBy", locationHierarchy.FirstOrDefault()?.CreatedBy},
                    {"CreatedTime", DateTime.UtcNow},
                    {"ModifiedBy", locationHierarchy.FirstOrDefault()?.ModifiedBy},
                    {"ModifiedTime", DateTime.UtcNow},
                    {"ServerAddTime", DateTime.UtcNow},
                    {"ServerModifiedTime", DateTime.UtcNow},
                    {"PrimaryUid", uid},
                    {"PrimaryType", type},
                    {"PrimaryLabel", locationHierarchy.FirstOrDefault()?.LocationName }
                };

                for (int i = 1; i <= 20; i++)
                {
                    insertParameters[$"L{i}"] = null;
                }
                var customLocationDataList = new List<Dictionary<string, object>>();
                var labelBuilder = new StringBuilder();

                foreach (var location in locationHierarchy)
                {
                    if (location.LevelNo >= 1 && location.LevelNo <= 20)
                    {
                        insertParameters[$"L{location.LevelNo}"] = location.UID;
                    }

                    /*if (labelBuilder.Length > 0)
                    {
                        labelBuilder.Append(" -> ");
                    }
                    labelBuilder.Append(location.LocationName);*/

                    if (labelBuilder.Length > 0)
                    {
                        labelBuilder.Insert(0, " -> ");
                    }
                    labelBuilder.Insert(0, location.LocationName);

                    var customData = new Dictionary<string, object>
                    {
                        {"UID", location.UID},
                        {"LocationTypeName", location.LocationTypeName}, 
                        {"Level", location.LevelNo},
                        {"Label", $"[{location.LocationCode}] {location.LocationName}"}
                    };

                    customLocationDataList.Add(customData);
                }

                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(customLocationDataList);

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

        public async Task<int> UpdateLocationMappingDetails(Winit.Modules.Location.Model.Interfaces.ILocationMapping updateLocationMapping)
        {
            try
            {
                var sql = @"UPDATE location_mapping 
                                            SET 
                                                created_by = @CreatedBy, 
                                                created_time = @CreatedTime, 
                                                modified_by = @ModifiedBy, 
                                                modified_time = @ModifiedTime, 
                                                server_modified_time = @ServerModifiedTime, 
                                                linked_item_uid = @LinkedItemUID, 
                                                location_type_uid = @LocationTypeUID, 
                                                linked_item_type = @LinkedItemType, 
                                                location_uid = @LocationUID 
                                            WHERE uid = @UID;";

                return await ExecuteNonQueryAsync(sql, updateLocationMapping);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteLocationMappingDetails(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM location_mapping WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);

        }
        public async Task<List<ILocationData>> GetLocationMaster()
        {
            var sql = $@"SELECT 
                                id AS Id,
                                uid AS Uid,
                                ss AS Ss,
                                created_by AS CreatedBy,
                                created_time AS CreatedTime,
                                modified_by AS ModifiedBy,
                                modified_time AS ModifiedTime,
                                server_add_time AS ServerAddTime,
                                server_modified_time AS ServerModifiedTime,
                                l1 AS L1,
                                l2 AS L2,
                                l3 AS L3,
                                l4 AS L4,
                                l5 AS L5,
                                l6 AS L6,
                                l7 AS L7,
                                l8 AS L8,
                                l9 AS L9,
                                l10 AS L10,
                                l11 AS L11,
                                l12 AS L12,
                                l13 AS L13,
                                l14 AS L14,
                                l15 AS L15,
                                l16 AS L16,
                                l17 AS L17,
                                l18 AS L18,
                                l19 AS L19,
                                l20 AS L20,
                                primary_uid AS PrimaryUid,
                                json_data AS JsonData,
								label as Label,
                                Primary_Label as PrimaryLabel
                            FROM 
                                location_data ";

            List<ILocationData> LocationData = await ExecuteQueryAsync<ILocationData>(sql.ToString(), null);

            return LocationData;
        }
    }
}
