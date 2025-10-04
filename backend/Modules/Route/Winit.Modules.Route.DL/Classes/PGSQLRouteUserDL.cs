using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Route.DL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Route.DL.Classes
{
    public class PGSQLRouteUserDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IRouteUserDL
    {
        public PGSQLRouteUserDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectAllRouteUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"
                SELECT 
                    r.route_uid AS RouteUID, 
                    r.job_position_uid AS JobPositionUID, 
                    r.from_date AS FromDate, 
                    r.to_date AS ToDate, 
                    r.is_active AS IsActive, 
                    r.action_type AS ActionType,
                    r.ss AS SS,
                    r.created_by AS CreatedBy,
                    r.created_time AS CreatedTime,
                    r.modified_time AS ModifiedTime,
                    r.server_add_time AS ServerAddTime,
                    r.server_modified_time AS ServerModifiedTime
                FROM route_user r");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM route_user");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Route.Model.Interfaces.IRouteUser>(filterCriterias, sbFilterCriteria, parameters);

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

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }



                //Data
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteUser>().GetType();
                IEnumerable<Model.Interfaces.IRouteUser> routeUsers = await ExecuteQueryAsync<Model.Interfaces.IRouteUser>(sql.ToString(), parameters, type);
                //Count
                int totalCount = 0;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser> pagedResponse = new PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser>
                {
                    PagedData = routeUsers,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectRouteUserByUID(List<string> UIDs)
        {
            string commaSeparatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs" , commaSeparatedUIDs}
            };
            var sql = @"
                SELECT 
                    r.route_uid AS RouteUID, 
                    r.job_position_uid AS JobPositionUID, 
                    r.from_date AS FromDate, 
                    r.to_date AS ToDate, 
                    r.is_active AS IsActive, 
                    r.action_type AS ActionType,
                    r.ss AS SS,
                    r.created_by AS CreatedBy,
                    r.created_time AS CreatedTime,
                    r.modified_time AS ModifiedTime,
                    r.server_add_time AS ServerAddTime,
                    r.server_modified_time AS ServerModifiedTime
                    FROM route_user r WHERE uid= ANY(string_to_array(@UIDs, ','))";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteUser>().GetType();
           IEnumerable<Model.Interfaces.IRouteUser> RouteUserList = await ExecuteQueryAsync<Model.Interfaces.IRouteUser>(sql, parameters, type);
            return RouteUserList;
        }
        public async Task<int> CreateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            int retVal = -1;
            try {
                if (routeUserList != null && routeUserList.Count > 0)
                {
                    foreach (RouteUser routeUser in routeUserList)
                    {
                        var sql = @"INSERT INTO route_user (uid,created_by,created_time,modified_by,modified_time,
                        server_add_time,server_modified_time,route_uid, job_position_uid,from_date,to_date,is_active)
                        VALUES (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,
                         @RouteUID, @JobPositionUID, @FromDate, @ToDate, @IsActive);";

                        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID", routeUser.UID},
                {"CreatedBy", routeUser.CreatedBy},
                {"CreatedTime", routeUser.CreatedTime},
                {"ModifiedBy", routeUser.ModifiedBy},
                {"ModifiedTime", routeUser.ModifiedTime},
                {"ServerAddTime", routeUser.ServerAddTime},
                {"ServerModifiedTime", routeUser.ServerModifiedTime},
                {"RouteUID", routeUser.RouteUID},
                {"JobPositionUID", routeUser.JobPositionUID},
                {"FromDate", routeUser.FromDate},
                {"ToDate", routeUser.ToDate},
                {"IsActive", routeUser.IsActive},
            };

                        retVal = await ExecuteNonQueryAsync(sql, parameters);
                    }

                }
            }
            catch(Exception)
            {
                throw;
            }
          
            return retVal;
        }

        public async Task<int> UpdateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            int retVal = -1;
            try
            {
                if (routeUserList != null && routeUserList.Count > 0)
                {
                    foreach (var routeUser in routeUserList)
                    {
                        var sql = @"UPDATE route_user SET 
                            modified_by	=@ModifiedBy
                            ,modified_time	=@ModifiedTime
                            ,server_modified_time	=@ServerModifiedTime
                            ,route_uid	=@RouteUID
                            ,job_position_uid	=@JobPositionUID
                            ,from_date	=@FromDate
                            ,to_date	=@ToDate
                            ,is_active	=@IsActive
                             WHERE uid = @UID;";
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",routeUser.UID},
                   {"ModifiedBy",routeUser.ModifiedBy},
                   {"ModifiedTime",routeUser.ModifiedTime},
                   {"ServerModifiedTime",routeUser.ServerModifiedTime},
                   {"RouteUID",routeUser.RouteUID},
                   {"JobPositionUID",routeUser.JobPositionUID},
                   {"FromDate",routeUser.FromDate},
                   {"ToDate",routeUser.ToDate},
                   {"IsActive",routeUser.IsActive},
                };
                        retVal= await ExecuteNonQueryAsync(sql, parameters);
                    }
                }
            }
            catch (Exception)
            {
                throw;

            }
            return retVal;

        }
        public async Task<int> DeleteRouteUser(List<string> UIDs)
        {
            string commaSeparatedUIDs = string.Join(",", UIDs);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UIDs" , commaSeparatedUIDs}
            };
            var sql = @"DELETE  FROM route_user WHERE uid = ANY(string_to_array(@UIDs, ','))";
            return await ExecuteNonQueryAsync(sql, parameters);

        }
    }
}
