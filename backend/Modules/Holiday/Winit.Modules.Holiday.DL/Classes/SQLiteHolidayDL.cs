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
    public class SQLiteHolidayDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager, IHolidayDL
    {
        public SQLiteHolidayDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>> GetHolidayDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT id AS Id, uid AS Uid, holiday_list_uid AS HolidayListUid, holiday_date AS HolidayDate, type AS Type, name AS Name, is_optional AS IsOptional, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                    FROM holiday) As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt (SELECT id AS Id, uid AS Uid, holiday_list_uid AS HolidayListUid, holiday_date AS HolidayDate, type AS Type, name AS Name, is_optional AS IsOptional, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                    FROM holiday) As SubQuery");
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
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IHoliday>().GetType();

                IEnumerable<Winit.Modules.Holiday.Model.Interfaces.IHoliday> HolidayDetails = await ExecuteQueryAsync<Winit.Modules.Holiday.Model.Interfaces.IHoliday>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday> pagedResponse = new PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHoliday>
                {
                    PagedData = HolidayDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
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
            var sql = @"SELECT id AS Id, uid AS Uid, holiday_list_uid AS HolidayListUid, holiday_date AS HolidayDate, type AS Type, name AS Name, is_optional AS IsOptional, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                    FROM holiday WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IHoliday>().GetType();

            Winit.Modules.Holiday.Model.Interfaces.IHoliday HolidayDetails = await ExecuteSingleAsync<Winit.Modules.Holiday.Model.Interfaces.IHoliday>(sql, parameters, type);
            return HolidayDetails;
        }
        public async Task<int> CreateHoliday(Winit.Modules.Holiday.Model.Interfaces.IHoliday createHoliday)
        {
            try
            {
                var sql = @"INSERT INTO HOLIDAY ( uid, holiday_list_uid, holiday_date, type, name, is_optional, year, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time)
                              VALUES (@UID,  @HolidayListUID,@HolidayDate ,@Type,@IsOptional,@Year,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                              @ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createHoliday.UID},
                   {"HolidayListUID", createHoliday.HolidayListUID},
                   {"HolidayDate", createHoliday.HolidayDate},
                   {"Type", createHoliday.Type},
                   {"Name", createHoliday.Name},
                   {"IsOptional", createHoliday.IsOptional},
                   {"Year", createHoliday.Year},
                   {"SS", createHoliday.SS},
                   {"CreatedBy", createHoliday.CreatedBy},
                   {"CreatedTime", createHoliday.CreatedTime},
                   {"ModifiedBy", createHoliday.ModifiedBy},
                   {"ModifiedTime", createHoliday.ModifiedTime},
                   {"ServerAddTime", createHoliday.ServerAddTime},
                   {"ServerModifiedTime", createHoliday.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
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
                var sql = @"UPDATE Holiday SET
                    holiday_date = @HolidayDate,
                    type = @Type,
                    name = @Name,
                    is_optional = @IsOptional,
                    year = @Year,
                    ss = @SS,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE id = @Id;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                //  {"UID", updateHoliday.UID},
                   {"HolidayDate", updateHoliday.HolidayDate},
                   {"Type", updateHoliday.Type},
                   {"Name", updateHoliday.Name},
                   {"IsOptional", updateHoliday.IsOptional},
                   {"Year", updateHoliday.Year},
                   {"SS", updateHoliday.SS},
                   {"ModifiedBy", updateHoliday.ModifiedBy},
                   {"ModifiedTime", updateHoliday.ModifiedTime},
                   {"ServerModifiedTime", updateHoliday.ServerModifiedTime},
                   {"Id", updateHoliday.Id},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
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
            var sql = "DELETE  FROM Holiday WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
