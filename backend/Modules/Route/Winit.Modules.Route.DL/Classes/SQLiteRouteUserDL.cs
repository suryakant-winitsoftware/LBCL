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
    public class SQLiteRouteUserDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager,IRouteUserDL
    {
        public SQLiteRouteUserDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Route.Model.Interfaces.IRouteUser>> SelectAllRouteUserDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder("SELECT * FROM route_user");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder("SELECT COUNT(1) AS Cnt FROM route_user");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria(filterCriterias, sbFilterCriteria, parameters);

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
                    if (sortCriterias != null && sortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
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
            var sql = @"SELECT id, uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, route_uid, job_position_uid, from_date, to_date, is_active
	FROM public.route_user WHERE uid IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IRouteUser>().GetType();
            IEnumerable<Model.Interfaces.IRouteUser> RouteUserList = await ExecuteSingleAsync<IEnumerable<Model.Interfaces.IRouteUser>>(sql, parameters, type);
            return RouteUserList;
        }
        public async Task<int> CreateRouteUser(List<Winit.Modules.Route.Model.Classes.RouteUser> routeUserList)
        {
            int retVal = -1;
            try
            {
                if (routeUserList != null && routeUserList.Count > 0)
                {
                    foreach (RouteUser routeUser in routeUserList)
                    {
                        var sql = @"INSERT INTO route_user (uid AS UID,created_by AS CreatedBy,created_time AS CreatedTime,modify_by AS ModifiedBy,modify_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,route_uid AS RouteUID,job_position_uid AS JobPositionUID,from_date AS FromDate,
                        to_date AS ToDate,is_active As IsActive)
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
            catch (Exception)
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
                             modified_by AS ModifiedBy	=@ModifiedBy
                            ,modified_time AS ModifiedTime	=@ModifiedTime
                            ,server_add_time AS ServerModifiedTime	=@ServerModifiedTime
                            ,route_uid AS RouteUID=@RouteUID
                            ,job_position_uid AS JobPositionUID	=@JobPositionUID
                            ,from_date AS FromDate	=@FromDate
                            ,to_date AS ToDate	=@ToDate
                            ,is_active AS IsActive	=@IsActive
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
                        retVal = await ExecuteNonQueryAsync(sql, parameters);
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
                {"UID" , commaSeparatedUIDs}
            };
            var sql = @"DELETE FROM route_user WHERE uid IN (SELECT value FROM STRING_SPLIT(@UIDs, ','));";
            return await ExecuteNonQueryAsync(sql, parameters);

        }
    }
}
