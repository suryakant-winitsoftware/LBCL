using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Holiday.DL.Interfaces;
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.DL.Classes
{
    public class PGSQLHolidayListRoleDL:Winit.Modules.Base.DL.DBManager.PostgresDBManager, IHolidayListRoleDL
    {
        public PGSQLHolidayListRoleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>> GetHolidayListRoleDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT
                                                hr.id AS Id,
                                                hr.uid AS UID,
                                                hr.holiday_list_uid AS HolidayListUID,
                                                hr.user_role_uid AS UserRoleUID,
                                                hr.ss AS SS,
                                                hr.created_time AS CreatedTime,
                                                hr.modified_time AS ModifiedTime,
                                                hr.server_add_time AS ServerAddTime,
                                                hr.server_modified_time AS ServerModifiedTime
                                            FROM
                                                holiday_list_role hr ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM holiday_list_role");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);

                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql,true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IHoliday>().GetType();

                IEnumerable<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> HolidayListRoleDetails = await ExecuteQueryAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> pagedResponse = new PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>
                {
                    PagedData = HolidayListRoleDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole> GetHolidayListRoleByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT
                            hr.id AS Id,
                            hr.uid AS UID,
                            hr.holiday_list_uid AS HolidayListUID,
                            hr.user_role_uid AS UserRoleUID,
                            hr.ss AS SS,
                            hr.created_time AS CreatedTime,
                            hr.modified_time AS ModifiedTime,
                            hr.server_add_time AS ServerAddTime,
                            hr.server_modified_time AS ServerModifiedTime
                        FROM
                            holiday_list_role hr
                         WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IHolidayListRole>().GetType();

            Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole HolidayListRoleDetails = await ExecuteSingleAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole>(sql, parameters, type);
            return HolidayListRoleDetails;
        }
        public async Task<int> CreateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole CreateHolidayListRole)
        {
            try
            {
                var sql = @"INSERT INTO holiday_list_role (uid, holiday_list_uid, user_role_uid, ss, created_time, modified_time, server_add_time, server_modified_time)
VALUES (@UID, @HolidayListUID, @UserRoleUID, @SS, @CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);
";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", CreateHolidayListRole.UID},
                   {"HolidayListUID", CreateHolidayListRole.HolidayListUID},
                   {"UserRoleUID", CreateHolidayListRole.UserRoleUID},
                   {"SS", CreateHolidayListRole.SS},
                   {"CreatedTime", CreateHolidayListRole.CreatedTime},
                   {"ModifiedTime", CreateHolidayListRole.ModifiedTime},
                   {"ServerAddTime", CreateHolidayListRole.ServerAddTime},
                   {"ServerModifiedTime", CreateHolidayListRole.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateHolidayListRole(Winit.Modules.Holiday.Model.Interfaces.IHolidayListRole updateHolidayListRole)
        {
            try
            {
                var sql = @"UPDATE holiday_list_role
                            SET
                                holiday_list_uid = @HolidayListUID,
                                user_role_uid = @UserRoleUID,
                                ss = @SS,
                                modified_time = @ModifiedTime,
                                server_modified_time = @ServerModifiedTime
                            WHERE
                                uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",updateHolidayListRole.UID},
                    {"HolidayListUID",updateHolidayListRole.HolidayListUID},
                    {"UserRoleUID",updateHolidayListRole.UserRoleUID},
                    {"SS",updateHolidayListRole.SS},
                    {"ModifiedTime",updateHolidayListRole.ModifiedTime},
                    {"ServerModifiedTime",updateHolidayListRole.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw ;
            }
        }

        public async Task<int> DeleteHolidayListRole(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM holiday_list_role WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
