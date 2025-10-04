using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Holiday.DL.Interfaces;
using Winit.Modules.Holiday.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.DL.Classes
{
    public class MSSQLHolidayDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IHolidayDL
    {
        public MSSQLHolidayDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>> GetHolidayDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                                                id AS Id,
                                                uid AS UID,
                                                holiday_list_uid AS HolidayListUID,
                                                holiday_date AS HolidayDate,
                                                type AS Type,
                                                name AS Name,
                                                is_optional AS IsOptional,
                                                year AS Year,
                                                ss AS SS,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime
                                            FROM     holiday)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                id AS Id,
                                                uid AS UID,
                                                holiday_list_uid AS HolidayListUID,
                                                holiday_date AS HolidayDate,
                                                type AS Type,
                                                name AS Name,
                                                is_optional AS IsOptional,
                                                year AS Year,
                                                ss AS SS,
                                                created_by AS CreatedBy,
                                                created_time AS CreatedTime,
                                                modified_by AS ModifiedBy,
                                                modified_time AS ModifiedTime,
                                                server_add_time AS ServerAddTime,
                                                server_modified_time AS ServerModifiedTime
                                            FROM holiday)as subquery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Holiday.Model.Interfaces.IHoliday>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Holiday.Model.Interfaces.IHoliday> holidayDetails = await ExecuteQueryAsync<Winit.Modules.Holiday.Model.Interfaces.IHoliday>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday> pagedResponse = new PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>
                {
                    PagedData = holidayDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Holiday.Model.Interfaces.IHoliday> GetHolidayByOrgUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                                var sql = @"SELECT 
                        id AS Id,
                        uid AS UID,
                        holiday_list_uid AS HolidayListUID,
                        holiday_date AS HolidayDate,
                        type AS Type,
                        name AS Name,
                        is_optional AS IsOptional,
                        year AS Year,
                        ss AS SS,
                        created_by AS CreatedBy,
                        created_time AS CreatedTime,
                        modified_by AS ModifiedBy,
                        modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,
                        server_modified_time AS ServerModifiedTime
                    FROM 
                        holiday
                     WHERE uid = @UID";
            Winit.Modules.Holiday.Model.Interfaces.IHoliday? holidayDetails = await ExecuteSingleAsync<Winit.Modules.Holiday.Model.Interfaces.IHoliday>(sql, parameters);
            return holidayDetails;
        }
        public async Task<int> CreateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday createHoliday)
        {
            try
            {
                var sql = @"INSERT INTO holiday (uid, holiday_list_uid, holiday_date, type, name, is_optional, year, created_by,
                            created_time, modified_by, modified_time, server_add_time, server_modified_time) 
                            VALUES 
                            (@UID, @HolidayListUID, @HolidayDate, @Type, @Name, @IsOptional, @Year, @CreatedBy, @CreatedTime,
                            @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime);";
                return await ExecuteNonQueryAsync(sql, createHoliday);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday updateHoliday)
        {
            try
            {
                var sql = @"UPDATE holiday 
                            SET holiday_date = @HolidayDate, type = @Type, name = @Name, is_optional = @IsOptional, year = @Year,
                            ss = @SS, modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_modified_time = @ServerModifiedTime 
                            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updateHoliday);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteHoliday(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE  FROM holiday WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
