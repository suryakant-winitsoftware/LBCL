using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.DL.Classes
{
    public class SQLiteLocationTypeDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ILocationTypeDL
    {
        public SQLiteLocationTypeDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType>> SelectAllLocationTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
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
                        company_uid AS CompanyUid,
                        name AS Name,
                        parent_uid AS ParentUid,
                        level_no AS LevelNo,
                        code AS Code,
                        show_in_ui AS ShowInUi
                    FROM 
                        location_type) As SubQuery");
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
                        company_uid AS CompanyUid,
                        name AS Name,
                        parent_uid AS ParentUid,
                        level_no AS LevelNo,
                        code AS Code,
                        show_in_ui AS ShowInUi
                    FROM 
                        location_type) As SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);
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
                //if (pageNumber > 0 && pageSize > 0)
                //{
                //    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                //}
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationType>().GetType();

                IEnumerable<Winit.Modules.Location.Model.Interfaces.ILocationType> LocationTypeDetails = await ExecuteQueryAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType> pagedResponse = new PagedResponse<Winit.Modules.Location.Model.Interfaces.ILocationType>
                {
                    PagedData = LocationTypeDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Location.Model.Interfaces.ILocationType> GetLocationTypeByUID(string UID)
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
                        company_uid AS CompanyUid,
                        name AS Name,
                        parent_uid AS ParentUid,
                        level_no AS LevelNo,
                        code AS Code,
                        show_in_ui AS ShowInUi
                    FROM 
                        location_type) WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ILocationType>().GetType();

            Winit.Modules.Location.Model.Interfaces.ILocationType LocationTypeDetails = await ExecuteSingleAsync<Winit.Modules.Location.Model.Interfaces.ILocationType>(sql, parameters, type);
            return LocationTypeDetails;
        }
        public async Task<int> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationType createLocationType)
        {
            try
            {
                var sql = @"INSERT INTO location_type (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, company_uid, level_no,  name, parent_uid,code) " +
          "VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @CompanyUID, @LevelNo, @Name, @ParentUID,@Code)";
                Dictionary<string, object> parameters = new Dictionary<string, object>
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
                        var sql = @"UPDATE location_type SET 
                     created_by = @CreatedBy, 
                     created_time = @CreatedTime, 
                     modified_by = @ModifiedBy, 
                     modified_time = @ModifiedTime, 
                     server_modified_time = @ServerModifiedTime, 
                     company_uid = @CompanyUID, 
                     level_no = @LevelNo, 
                     name = @Name, 
                     parent_uid = @ParentUID 
                     WHERE uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {

                   {"UID", updateLocationType.UID},
                   {"CreatedBy", updateLocationType.CreatedBy},
                   {"ModifiedBy", updateLocationType.ModifiedBy},
                   {"CreatedTime", updateLocationType.CreatedTime},
                   {"CompanyUID", updateLocationType.CompanyUID},
                   {"LevelNo", updateLocationType.LevelNo},
                   {"Name", updateLocationType.Name},
                   {"ParentUID",updateLocationType.ParentUID },
                   {"ModifiedTime", updateLocationType.ModifiedTime},
                   {"ServerModifiedTime", updateLocationType.ServerModifiedTime},
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
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM location_type WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        } 
    }
}
