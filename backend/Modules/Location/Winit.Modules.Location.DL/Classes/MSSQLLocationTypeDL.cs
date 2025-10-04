using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class MSSQLLocationTypeDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ILocationTypeDL
    {
        public MSSQLLocationTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType>> SelectAllLocationTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"SELECT * FROM (SELECT
                                                    id AS Id,
                                                    uid AS Uid,
                                                    created_by AS CreatedBy,
                                                    created_time AS CreatedTime,
                                                    modified_by AS ModifiedBy,
                                                    modified_time AS ModifiedTime,
                                                    server_add_time AS ServerAddTime,
                                                    server_modified_time AS ServerModifiedTime,
                                                    company_uid AS CompanyUID,
                                                    name AS Name,
                                                    parent_uid AS ParentUid,
                                                    level_no AS LevelNo,
                                                    code as Code,
                                                    show_in_ui AS ShowInUI,
                                                    show_in_template AS ShowInTemplate
                                FROM location_type) as SUBQuery");
                StringBuilder sqlCount = new();
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
                                                    company_uid AS CompanyUID,
                                                    name AS Name,
                                                    parent_uid AS ParentUid,
                                                    level_no AS LevelNo,
                                                    code as Code,
                                                    show_in_ui AS ShowInUI,
                                                    show_in_template AS ShowInTemplate
                                                FROM location_type) as SubQuery");
                }
                Dictionary<string, object> parameters = new();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    _ = sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Location.Model.Interfaces.ILocationType>(filterCriterias, sbFilterCriteria, parameters);
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
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocationType> LocationTypeDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType> pagedResponse = new()
                {
                    PagedData = LocationTypeDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationType?> GetLocationTypeByUID(string UID)
        {
            Dictionary<string, object?> parameters = new()
            {
                {"UID",  UID}
            };
            string sql = @"SELECT
                            id AS Id,
                            uid AS Uid,
                            created_by AS CreatedBy,
                            created_time AS CreatedTime,
                            modified_by AS ModifiedBy,
                            modified_time AS ModifiedTime,
                            server_add_time AS ServerAddTime,
                            server_modified_time AS ServerModifiedTime,
                            company_uid AS CompanyUID,
                            name AS Name,
                            parent_uid AS ParentUid,
                            level_no AS LevelNo,
                            code as Code,
                            show_in_ui AS ShowInUI,
                            show_in_template AS ShowInTemplate
                        FROM location_type WHERE UID = @UID";

            Winit.Modules.Location.Model.Interfaces.ILocationType? LocationTypeDetails =
                await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql, parameters);
            return LocationTypeDetails;
        }
        public async Task<int> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationType createLocationType)
        {
            try
            {
                string sql = $@"INSERT INTO location_type (
                                                uid,
                                                created_by,
                                                created_time,
                                                modified_by,
                                                modified_time,
                                                server_add_time,
                                                server_modified_time,
                                                company_uid,
                                                level_no,
                                                name,
                                                parent_uid,
                                                code,
                                                show_in_ui,
                                                show_in_template)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime,
                        @ServerModifiedTime, @CompanyUID, @LevelNo, @Name, @ParentUID, @Code, @ShowInUI, @ShowInTemplate)";
                
                return await ExecuteNonQueryAsync(sql, createLocationType);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateLocationTypeDetails(Winit.Modules.Location.Model.Interfaces.ILocationType updateLocationType)
        {
            try
            {
                string sql = @"UPDATE location_type
                        SET
                            created_by = @CreatedBy,
                            created_time = @CreatedTime,
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            company_uid = @CompanyUid,
                            level_no = @LevelNo,
                            name = @name,
                            parent_uid = @ParentUID,
                            code = @Code,
                            show_in_ui = @ShowInUI,
                            show_in_template = @ShowInTemplate
                        WHERE uid = @UID;";

                
                return await ExecuteNonQueryAsync(sql, updateLocationType);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteLocationTypeDetails(string UID)
        {
            Dictionary<string, object> parameters = new()
            {
                {"UID" , UID}
            };
            string sql = @"DELETE  FROM location_type WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
