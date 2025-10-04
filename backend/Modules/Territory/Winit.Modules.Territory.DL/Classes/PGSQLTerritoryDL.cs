using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Territory.DL.Interfaces;
using Winit.Modules.Territory.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Territory.DL.Classes
{
    public class PGSQLTerritoryDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ITerritoryDL
    {
        public PGSQLTerritoryDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<PagedResponse<ITerritory>> SelectAllTerritories(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"
                    SELECT * FROM (
                        SELECT
                            t.id as Id,
                            t.uid as UID,
                            t.created_by as CreatedBy,
                            t.created_time as CreatedTime,
                            t.modified_by as ModifiedBy,
                            t.modified_time as ModifiedTime,
                            t.server_add_time as ServerAddTime,
                            t.server_modified_time as ServerModifiedTime,
                            t.org_uid as OrgUID,
                            t.territory_code as TerritoryCode,
                            t.territory_name as TerritoryName,
                            t.manager_emp_uid as ManagerEmpUID,
                            t.cluster_code as ClusterCode,
                            t.parent_uid as ParentUID,
                            t.item_level as ItemLevel,
                            CASE
                                WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                                THEN TRUE ELSE FALSE
                            END AS HasChild,
                            t.is_import as IsImport,
                            t.is_local as IsLocal,
                            t.is_non_license as IsNonLicense,
                            t.status as Status,
                            t.is_active as IsActive
                        FROM territory t
                    ) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"
                        SELECT COUNT(*) FROM (
                            SELECT
                                t.id as Id,
                                t.uid as UID,
                                t.territory_code as TerritoryCode,
                                t.territory_name as TerritoryName
                            FROM territory t
                        ) AS SubQuery");
                }

                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<ITerritory>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    StringBuilder sbSortCriteria = new StringBuilder();
                    AppendSortCriteria(sortCriterias, sbSortCriteria, true);
                    sql.Append(sbSortCriteria);
                }
                else
                {
                    sql.Append(" ORDER BY created_time DESC");
                }

                sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");

                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql.ToString(), parameters, type);

                return new PagedResponse<ITerritory>
                {
                    PagedData = result,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ITerritory> GetTerritoryByUID(string uid)
        {
            try
            {
                var sql = @"
                    SELECT
                        t.id as Id,
                        t.uid as UID,
                        t.created_by as CreatedBy,
                        t.created_time as CreatedTime,
                        t.modified_by as ModifiedBy,
                        t.modified_time as ModifiedTime,
                        t.server_add_time as ServerAddTime,
                        t.server_modified_time as ServerModifiedTime,
                        t.org_uid as OrgUID,
                        t.territory_code as TerritoryCode,
                        t.territory_name as TerritoryName,
                        t.manager_emp_uid as ManagerEmpUID,
                        t.cluster_code as ClusterCode,
                        t.parent_uid as ParentUID,
                        t.item_level as ItemLevel,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                            THEN TRUE ELSE FALSE
                        END AS HasChild,
                        t.is_import as IsImport,
                        t.is_local as IsLocal,
                        t.is_non_license as IsNonLicense,
                        t.status as Status,
                        t.is_active as IsActive
                    FROM territory t
                    WHERE t.uid = @uid";

                var parameters = new Dictionary<string, object?> { { "uid", uid } };
                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql, parameters, type);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ITerritory> GetTerritoryByCode(string territoryCode, string orgUID)
        {
            try
            {
                var sql = @"
                    SELECT
                        t.id as Id,
                        t.uid as UID,
                        t.created_by as CreatedBy,
                        t.created_time as CreatedTime,
                        t.modified_by as ModifiedBy,
                        t.modified_time as ModifiedTime,
                        t.server_add_time as ServerAddTime,
                        t.server_modified_time as ServerModifiedTime,
                        t.org_uid as OrgUID,
                        t.territory_code as TerritoryCode,
                        t.territory_name as TerritoryName,
                        t.manager_emp_uid as ManagerEmpUID,
                        t.cluster_code as ClusterCode,
                        t.parent_uid as ParentUID,
                        t.item_level as ItemLevel,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                            THEN TRUE ELSE FALSE
                        END AS HasChild,
                        t.is_import as IsImport,
                        t.is_local as IsLocal,
                        t.is_non_license as IsNonLicense,
                        t.status as Status,
                        t.is_active as IsActive
                    FROM territory t
                    WHERE t.territory_code = @territoryCode AND t.org_uid = @orgUID";

                var parameters = new Dictionary<string, object?>
                {
                    { "territoryCode", territoryCode },
                    { "orgUID", orgUID }
                };

                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql, parameters, type);
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ITerritory>> GetTerritoriesByOrg(string orgUID)
        {
            try
            {
                var sql = @"
                    SELECT
                        t.id as Id,
                        t.uid as UID,
                        t.created_by as CreatedBy,
                        t.created_time as CreatedTime,
                        t.modified_by as ModifiedBy,
                        t.modified_time as ModifiedTime,
                        t.server_add_time as ServerAddTime,
                        t.server_modified_time as ServerModifiedTime,
                        t.org_uid as OrgUID,
                        t.territory_code as TerritoryCode,
                        t.territory_name as TerritoryName,
                        t.manager_emp_uid as ManagerEmpUID,
                        t.cluster_code as ClusterCode,
                        t.parent_uid as ParentUID,
                        t.item_level as ItemLevel,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                            THEN TRUE ELSE FALSE
                        END AS HasChild,
                        t.is_import as IsImport,
                        t.is_local as IsLocal,
                        t.is_non_license as IsNonLicense,
                        t.status as Status,
                        t.is_active as IsActive
                    FROM territory t
                    WHERE t.org_uid = @orgUID
                    ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object?> { { "orgUID", orgUID } };
                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql, parameters, type);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ITerritory>> GetTerritoriesByManager(string managerEmpUID)
        {
            try
            {
                var sql = @"
                    SELECT
                        t.id as Id,
                        t.uid as UID,
                        t.created_by as CreatedBy,
                        t.created_time as CreatedTime,
                        t.modified_by as ModifiedBy,
                        t.modified_time as ModifiedTime,
                        t.server_add_time as ServerAddTime,
                        t.server_modified_time as ServerModifiedTime,
                        t.org_uid as OrgUID,
                        t.territory_code as TerritoryCode,
                        t.territory_name as TerritoryName,
                        t.manager_emp_uid as ManagerEmpUID,
                        t.cluster_code as ClusterCode,
                        t.parent_uid as ParentUID,
                        t.item_level as ItemLevel,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                            THEN TRUE ELSE FALSE
                        END AS HasChild,
                        t.is_import as IsImport,
                        t.is_local as IsLocal,
                        t.is_non_license as IsNonLicense,
                        t.status as Status,
                        t.is_active as IsActive
                    FROM territory t
                    WHERE t.manager_emp_uid = @managerEmpUID
                    ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object?> { { "managerEmpUID", managerEmpUID } };
                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql, parameters, type);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ITerritory>> GetTerritoriesByCluster(string clusterCode)
        {
            try
            {
                var sql = @"
                    SELECT
                        t.id as Id,
                        t.uid as UID,
                        t.created_by as CreatedBy,
                        t.created_time as CreatedTime,
                        t.modified_by as ModifiedBy,
                        t.modified_time as ModifiedTime,
                        t.server_add_time as ServerAddTime,
                        t.server_modified_time as ServerModifiedTime,
                        t.org_uid as OrgUID,
                        t.territory_code as TerritoryCode,
                        t.territory_name as TerritoryName,
                        t.manager_emp_uid as ManagerEmpUID,
                        t.cluster_code as ClusterCode,
                        t.parent_uid as ParentUID,
                        t.item_level as ItemLevel,
                        CASE
                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                            THEN TRUE ELSE FALSE
                        END AS HasChild,
                        t.is_import as IsImport,
                        t.is_local as IsLocal,
                        t.is_non_license as IsNonLicense,
                        t.status as Status,
                        t.is_active as IsActive
                    FROM territory t
                    WHERE t.cluster_code = @clusterCode
                    ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object?> { { "clusterCode", clusterCode } };
                Type type = _serviceProvider.GetRequiredService<ITerritory>().GetType();
                var result = await ExecuteQueryAsync<ITerritory>(sql, parameters, type);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> CreateTerritory(ITerritory territory)
        {
            try
            {
                var sql = @"
                    INSERT INTO territory (
                        uid, created_by, created_time, modified_by, modified_time,
                        server_add_time, server_modified_time,
                        org_uid, territory_code, territory_name, manager_emp_uid, cluster_code,
                        parent_uid, item_level, has_child,
                        is_import, is_local, is_non_license, status, is_active
                    ) VALUES (
                        @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                        NOW(), NOW(),
                        @OrgUID, @TerritoryCode, @TerritoryName, @ManagerEmpUID, @ClusterCode,
                        @ParentUID, @ItemLevel, @HasChild,
                        @IsImport, @IsLocal, @IsNonLicense, @Status, @IsActive
                    )";

                var parameters = new Dictionary<string, object?>
                {
                    { "UID", territory.UID },
                    { "CreatedBy", territory.CreatedBy },
                    { "CreatedTime", territory.CreatedTime },
                    { "ModifiedBy", territory.ModifiedBy },
                    { "ModifiedTime", territory.ModifiedTime },
                    { "OrgUID", territory.OrgUID },
                    { "TerritoryCode", territory.TerritoryCode },
                    { "TerritoryName", territory.TerritoryName },
                    { "ManagerEmpUID", territory.ManagerEmpUID },
                    { "ClusterCode", territory.ClusterCode },
                    { "ParentUID", territory.ParentUID },
                    { "ItemLevel", territory.ItemLevel },
                    { "HasChild", territory.HasChild },
                    { "IsImport", territory.IsImport },
                    { "IsLocal", territory.IsLocal },
                    { "IsNonLicense", territory.IsNonLicense },
                    { "Status", territory.Status },
                    { "IsActive", territory.IsActive }
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> UpdateTerritory(ITerritory territory)
        {
            try
            {
                var sql = @"
                    UPDATE territory SET
                        modified_by = @ModifiedBy,
                        modified_time = @ModifiedTime,
                        server_modified_time = NOW(),
                        territory_name = @TerritoryName,
                        manager_emp_uid = @ManagerEmpUID,
                        cluster_code = @ClusterCode,
                        parent_uid = @ParentUID,
                        item_level = @ItemLevel,
                        has_child = @HasChild,
                        is_import = @IsImport,
                        is_local = @IsLocal,
                        is_non_license = @IsNonLicense,
                        status = @Status,
                        is_active = @IsActive
                    WHERE uid = @UID";

                var parameters = new Dictionary<string, object?>
                {
                    { "UID", territory.UID },
                    { "ModifiedBy", territory.ModifiedBy },
                    { "ModifiedTime", territory.ModifiedTime },
                    { "TerritoryName", territory.TerritoryName },
                    { "ManagerEmpUID", territory.ManagerEmpUID },
                    { "ClusterCode", territory.ClusterCode },
                    { "ParentUID", territory.ParentUID },
                    { "ItemLevel", territory.ItemLevel },
                    { "HasChild", territory.HasChild },
                    { "IsImport", territory.IsImport },
                    { "IsLocal", territory.IsLocal },
                    { "IsNonLicense", territory.IsNonLicense },
                    { "Status", territory.Status },
                    { "IsActive", territory.IsActive }
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DeleteTerritory(string uid)
        {
            try
            {
                var sql = "DELETE FROM territory WHERE uid = @uid";
                var parameters = new Dictionary<string, object?> { { "uid", uid } };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
