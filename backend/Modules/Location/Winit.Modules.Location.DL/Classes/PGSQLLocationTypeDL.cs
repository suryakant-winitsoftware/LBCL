using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class PGSQLLocationTypeDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ILocationTypeDL
    {
        public PGSQLLocationTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                                FROM public.location_type) as SUBQuery");
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
                                                FROM public.location_type) as SubQuery");
                }
                Dictionary<string, object> parameters = new();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new();
                    _ = sbFilterCriteria.Append(" WHERE ");
                    if (filterCriterias.Any(e => e.Name == "locationhierarchy_type_code_name"))
                    {
                        sbFilterCriteria.Append(" (code = @Code or name = @Name) ");
                        FilterCriteria filter = filterCriterias.Find(e => e.Name == "locationhierarchy_type_code_name")!;
                        parameters.Add("Code", filter.Value);
                        parameters.Add("Name", filter.Value);
                        filterCriterias.Remove(filter);
                        if (filterCriterias.Any()) sbFilterCriteria.Append(" AND ");
                    }
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
                    _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationType>().GetType();

                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocationType> LocationTypeDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
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
                        FROM public.location_type WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationType>().GetType();

            Winit.Modules.Location.Model.Interfaces.ILocationType? LocationTypeDetails =
                await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql, parameters, type);
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
                Dictionary<string, object?> parameters = new()
                {
                   {"UID", createLocationType.UID},
                   {"CreatedBy", createLocationType.CreatedBy},
                   {"ModifiedBy", createLocationType.ModifiedBy},
                   {"CreatedTime", createLocationType.CreatedTime},
                   {"CompanyUID", createLocationType.CompanyUID},
                   {"LevelNo", createLocationType.LevelNo},
                   {"Name", createLocationType.Name},
                   {"ParentUID",createLocationType.ParentUID },
                   {"Code",createLocationType.Code },
                   {"ModifiedTime", createLocationType.ModifiedTime},
                   {"ServerAddTime", createLocationType.ServerAddTime},
                   {"ServerModifiedTime", createLocationType.ServerModifiedTime},
                   {"ShowInUI", createLocationType.ShowInUI},
                   {"ShowInTemplate", createLocationType.ShowInTemplate},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
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

                Dictionary<string, object> parameters = new()
                {

                   {"UID", updateLocationType.UID},
                   {"CreatedBy", updateLocationType.CreatedBy},
                   {"ModifiedBy", updateLocationType.ModifiedBy},
                   {"CreatedTime", updateLocationType.CreatedTime},
                   {"CompanyUID", updateLocationType.CompanyUID},
                   {"LevelNo", updateLocationType.LevelNo},
                   {"Name", updateLocationType.Name},
                   {"ParentUID",updateLocationType.ParentUID },
                   {"Code",updateLocationType.Code },
                   {"ModifiedTime", updateLocationType.ModifiedTime},
                   {"ServerModifiedTime", updateLocationType.ServerModifiedTime},
                   {"ShowInUI", updateLocationType.ShowInUI},
                   {"ShowInTemplate", updateLocationType.ShowInTemplate},
                 };
                return await ExecuteNonQueryAsync(sql, parameters);
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
