using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Territory.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Territory.DL.Classes
{
    public class MSSQLTerritoryDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ITerritoryDL
    {
        public MSSQLTerritoryDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        public async Task<PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory>> SelectAllTerritories(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (
                                                SELECT
                                                    t.id AS Id,
                                                    t.uid AS UID,
                                                    t.created_by AS CreatedBy,
                                                    t.created_time AS CreatedTime,
                                                    t.modified_by AS ModifiedBy,
                                                    t.modified_time AS ModifiedTime,
                                                    t.server_add_time AS ServerAddTime,
                                                    t.server_modified_time AS ServerModifiedTime,
                                                    t.org_uid AS OrgUID,
                                                    t.territory_code AS TerritoryCode,
                                                    t.territory_name AS TerritoryName,
                                                    t.manager_emp_uid AS ManagerEmpUID,
                                                    t.cluster_code AS ClusterCode,
                                                    t.parent_uid AS ParentUID,
                                                    t.item_level AS ItemLevel,
                                                    CASE
                                                        WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                                                        THEN 1 ELSE 0
                                                    END AS HasChild,
                                                    t.is_import AS IsImport,
                                                    t.is_local AS IsLocal,
                                                    t.is_non_license AS IsNonLicense,
                                                    t.status AS Status,
                                                    t.is_active AS IsActive
                                                FROM territory t
                                            ) AS SubQuery");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(*) FROM (
                                                    SELECT
                                                        t.id AS Id,
                                                        t.uid AS UID,
                                                        t.created_by AS CreatedBy,
                                                        t.created_time AS CreatedTime,
                                                        t.modified_by AS ModifiedBy,
                                                        t.modified_time AS ModifiedTime,
                                                        t.server_add_time AS ServerAddTime,
                                                        t.server_modified_time AS ServerModifiedTime,
                                                        t.org_uid AS OrgUID,
                                                        t.territory_code AS TerritoryCode,
                                                        t.territory_name AS TerritoryName,
                                                        t.manager_emp_uid AS ManagerEmpUID,
                                                        t.cluster_code AS ClusterCode,
                                                        t.parent_uid AS ParentUID,
                                                        t.item_level AS ItemLevel,
                                                        CASE
                                                            WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                                                            THEN 1 ELSE 0
                                                        END AS HasChild,
                                                        t.is_import AS IsImport,
                                                        t.is_local AS IsLocal,
                                                        t.is_non_license AS IsNonLicense,
                                                        t.status AS Status,
                                                        t.is_active AS IsActive
                                                    FROM territory t
                                                ) AS SubQuery");
                }

                var parameters = new Dictionary<string, object?>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Territory.Model.Interfaces.ITerritory>(filterCriterias, sbFilterCriteria, parameters);
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

                IEnumerable<Winit.Modules.Territory.Model.Interfaces.ITerritory> territories = await ExecuteQueryAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql.ToString(), parameters);

                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory> pagedResponse = new PagedResponse<Winit.Modules.Territory.Model.Interfaces.ITerritory>
                {
                    PagedData = territories,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.Territory.Model.Interfaces.ITerritory?> GetTerritoryByUID(string UID)
        {
            try
            {
                var sql = @"SELECT
                                t.id AS Id,
                                t.uid AS UID,
                                t.created_by AS CreatedBy,
                                t.created_time AS CreatedTime,
                                t.modified_by AS ModifiedBy,
                                t.modified_time AS ModifiedTime,
                                t.server_add_time AS ServerAddTime,
                                t.server_modified_time AS ServerModifiedTime,
                                t.org_uid AS OrgUID,
                                t.territory_code AS TerritoryCode,
                                t.territory_name AS TerritoryName,
                                t.manager_emp_uid AS ManagerEmpUID,
                                t.cluster_code AS ClusterCode,
                                t.parent_uid AS ParentUID,
                                t.item_level AS ItemLevel,
                                CASE
                                    WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                                    THEN 1 ELSE 0
                                END AS HasChild,
                                t.is_import AS IsImport,
                                t.is_local AS IsLocal,
                                t.is_non_license AS IsNonLicense,
                                t.status AS Status,
                                t.is_active AS IsActive
                            FROM territory t
                            WHERE t.uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                return await ExecuteSingleAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Winit.Modules.Territory.Model.Interfaces.ITerritory?> GetTerritoryByCode(string territoryCode, string orgUID)
        {
            try
            {
                var sql = @"SELECT
                                t.id AS Id,
                                t.uid AS UID,
                                t.created_by AS CreatedBy,
                                t.created_time AS CreatedTime,
                                t.modified_by AS ModifiedBy,
                                t.modified_time AS ModifiedTime,
                                t.server_add_time AS ServerAddTime,
                                t.server_modified_time AS ServerModifiedTime,
                                t.org_uid AS OrgUID,
                                t.territory_code AS TerritoryCode,
                                t.territory_name AS TerritoryName,
                                t.manager_emp_uid AS ManagerEmpUID,
                                t.cluster_code AS ClusterCode,
                                t.parent_uid AS ParentUID,
                                t.item_level AS ItemLevel,
                                CASE
                                    WHEN EXISTS (SELECT 1 FROM territory t1 WHERE t1.parent_uid = t.uid)
                                    THEN 1 ELSE 0
                                END AS HasChild,
                                t.is_import AS IsImport,
                                t.is_local AS IsLocal,
                                t.is_non_license AS IsNonLicense,
                                t.status AS Status,
                                t.is_active AS IsActive
                            FROM territory t
                            WHERE t.territory_code = @TerritoryCode AND t.org_uid = @OrgUID";

                var parameters = new Dictionary<string, object>
                {
                    { "TerritoryCode", territoryCode },
                    { "OrgUID", orgUID }
                };

                return await ExecuteSingleAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByOrg(string orgUID)
        {
            try
            {
                var sql = @"SELECT
                                t.id AS Id,
                                t.uid AS UID,
                                t.created_by AS CreatedBy,
                                t.created_time AS CreatedTime,
                                t.modified_by AS ModifiedBy,
                                t.modified_time AS ModifiedTime,
                                t.server_add_time AS ServerAddTime,
                                t.server_modified_time AS ServerModifiedTime,
                                t.org_uid AS OrgUID,
                                t.territory_code AS TerritoryCode,
                                t.territory_name AS TerritoryName,
                                t.manager_emp_uid AS ManagerEmpUID,
                                t.cluster_code AS ClusterCode,
                                t.is_import AS IsImport,
                                t.is_local AS IsLocal,
                                t.is_non_license AS IsNonLicense,
                                t.status AS Status,
                                t.is_active AS IsActive
                            FROM territory t
                            WHERE t.org_uid = @OrgUID
                            ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object>
                {
                    { "OrgUID", orgUID }
                };

                var result = await ExecuteQueryAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql, parameters);
                return result?.ToList() ?? new List<Winit.Modules.Territory.Model.Interfaces.ITerritory>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByManager(string managerEmpUID)
        {
            try
            {
                var sql = @"SELECT
                                t.id AS Id,
                                t.uid AS UID,
                                t.created_by AS CreatedBy,
                                t.created_time AS CreatedTime,
                                t.modified_by AS ModifiedBy,
                                t.modified_time AS ModifiedTime,
                                t.server_add_time AS ServerAddTime,
                                t.server_modified_time AS ServerModifiedTime,
                                t.org_uid AS OrgUID,
                                t.territory_code AS TerritoryCode,
                                t.territory_name AS TerritoryName,
                                t.manager_emp_uid AS ManagerEmpUID,
                                t.cluster_code AS ClusterCode,
                                t.is_import AS IsImport,
                                t.is_local AS IsLocal,
                                t.is_non_license AS IsNonLicense,
                                t.status AS Status,
                                t.is_active AS IsActive
                            FROM territory t
                            WHERE t.manager_emp_uid = @ManagerEmpUID
                            ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object>
                {
                    { "ManagerEmpUID", managerEmpUID }
                };

                var result = await ExecuteQueryAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql, parameters);
                return result?.ToList() ?? new List<Winit.Modules.Territory.Model.Interfaces.ITerritory>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Winit.Modules.Territory.Model.Interfaces.ITerritory>> GetTerritoriesByCluster(string clusterCode)
        {
            try
            {
                var sql = @"SELECT
                                t.id AS Id,
                                t.uid AS UID,
                                t.created_by AS CreatedBy,
                                t.created_time AS CreatedTime,
                                t.modified_by AS ModifiedBy,
                                t.modified_time AS ModifiedTime,
                                t.server_add_time AS ServerAddTime,
                                t.server_modified_time AS ServerModifiedTime,
                                t.org_uid AS OrgUID,
                                t.territory_code AS TerritoryCode,
                                t.territory_name AS TerritoryName,
                                t.manager_emp_uid AS ManagerEmpUID,
                                t.cluster_code AS ClusterCode,
                                t.is_import AS IsImport,
                                t.is_local AS IsLocal,
                                t.is_non_license AS IsNonLicense,
                                t.status AS Status,
                                t.is_active AS IsActive
                            FROM territory t
                            WHERE t.cluster_code = @ClusterCode
                            ORDER BY t.territory_code";

                var parameters = new Dictionary<string, object>
                {
                    { "ClusterCode", clusterCode }
                };

                var result = await ExecuteQueryAsync<Winit.Modules.Territory.Model.Interfaces.ITerritory>(sql, parameters);
                return result?.ToList() ?? new List<Winit.Modules.Territory.Model.Interfaces.ITerritory>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory createTerritory)
        {
            try
            {
                var sql = @"INSERT INTO territory (
                                uid, created_by, created_time, modified_by, modified_time,
                                server_add_time, server_modified_time, org_uid, territory_code,
                                territory_name, manager_emp_uid, cluster_code, parent_uid,
                                item_level, has_child, is_import, is_local, is_non_license,
                                status, is_active
                            ) VALUES (
                                @UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                                @ServerAddTime, @ServerModifiedTime, @OrgUID, @TerritoryCode,
                                @TerritoryName, @ManagerEmpUID, @ClusterCode, @ParentUID,
                                @ItemLevel, @HasChild, @IsImport, @IsLocal, @IsNonLicense,
                                @Status, @IsActive
                            )";

                return await ExecuteNonQueryAsync(sql, createTerritory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateTerritory(Winit.Modules.Territory.Model.Interfaces.ITerritory updateTerritory)
        {
            try
            {
                var sql = @"UPDATE territory SET
                                modified_by = @ModifiedBy,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime,
                                org_uid = @OrgUID,
                                territory_code = @TerritoryCode,
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

                return await ExecuteNonQueryAsync(sql, updateTerritory);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteTerritory(string UID)
        {
            try
            {
                var sql = @"DELETE FROM territory WHERE uid = @UID";

                var parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
