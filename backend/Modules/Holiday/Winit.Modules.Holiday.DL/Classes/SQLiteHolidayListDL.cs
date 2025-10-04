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
using Winit.Modules.Holiday.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Holiday.DL.Classes
{
    public class SQLiteHolidayListDL:Winit.Modules.Base.DL.DBManager.SqliteDBManager, IHolidayListDL
    {
        public SQLiteHolidayListDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>> SelectAllHolidayList(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT id AS Id, uid AS Uid, company_uid AS CompanyUid, org_uid AS OrgUid, name AS Name, description AS Description, location_uid AS LocationUid, is_active AS IsActive, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                FROM  holiday_list) As SubQuery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id, uid AS Uid, company_uid AS CompanyUid, org_uid AS OrgUid, name AS Name, description AS Description, location_uid AS LocationUid, is_active AS IsActive, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                FROM  holiday_list) As SubQuery ");
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

                IEnumerable<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> HolidayListDetails = await ExecuteQueryAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> pagedResponse = new PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>
                {
                    PagedData = HolidayListDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> GetHolidayListByHolidayListUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id, uid AS Uid, company_uid AS CompanyUid, org_uid AS OrgUid, name AS Name, description AS Description, location_uid AS LocationUid, is_active AS IsActive, year AS Year, ss AS Ss, created_by AS CreatedBy, created_time AS CreatedTime, modified_by AS ModifiedBy, modified_time AS ModifiedTime, server_add_time AS ServerAddTime, server_modified_time AS ServerModifiedTime
                FROM  holiday_list WHERE UID = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IHolidayList>().GetType();

            Winit.Modules.Holiday.Model.Interfaces.IHolidayList HolidayListDetails = await ExecuteSingleAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>(sql, parameters, type);
            return HolidayListDetails;
        }
        public async Task<int> CreateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList createHolidayList)
        {
            try
            {
                var sql = @"INSERT INTO HolidayList (uid, company_uid, org_uid, name, description, location_uid, is_active, year, ss, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time)
                    VALUES (@UID,  @CompanyUID,@OrgUID,@Name,@Description,@LocationUID,@IsActive,@Year,@SS,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,
                              @ServerAddTime,@ServerModifiedTime);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", createHolidayList ?.UID},
                   {"CompanyUID", createHolidayList ?.CompanyUID},
                   {"OrgUID", createHolidayList ?.OrgUID},
                   {"Name", createHolidayList ?.Name},
                   {"Description", createHolidayList ?.Description},
                   {"LocationUID", createHolidayList ?.LocationUID},
                   {"IsActive", createHolidayList ?.IsActive},
                   {"Year", createHolidayList ?.Year},
                   {"SS", createHolidayList ?.SS},
                   {"CreatedBy", createHolidayList ?.CreatedBy},
                   {"CreatedTime", createHolidayList ?.CreatedTime},
                   {"ModifiedBy", createHolidayList ?.ModifiedBy},
                   {"ModifiedTime", createHolidayList ?.ModifiedTime},
                   {"ServerAddTime", createHolidayList ?.ServerAddTime},
                   {"ServerModifiedTime", createHolidayList ?.ServerModifiedTime},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> UpdateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList updateHolidayList)
        {
            try
            {
                var sql = @"UPDATE HolidayList SET
                    company_uid = @CompanyUID,
                    name = @Name,
                    description = @Description,
                    location_uid = @LocationUID,
                    is_active = @IsActive,
                    year = @Year,
                    ss = @SS,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID", updateHolidayList ?.UID},
                   {"CompanyUID", updateHolidayList ?.CompanyUID},
                  // {"OrgUID", updateHolidayList ?.OrgUID},
                   {"Name", updateHolidayList ?.Name},
                   {"Description", updateHolidayList ?.Description},
                   {"LocationUID", updateHolidayList ?.LocationUID},
                   {"IsActive", updateHolidayList ?.IsActive},
                   {"Year", updateHolidayList ?.Year},
                   {"SS", updateHolidayList ?.SS},
                   {"ModifiedBy", updateHolidayList ?.ModifiedBy},
                   {"ModifiedTime", updateHolidayList ?.ModifiedTime},
                   {"ServerModifiedTime", updateHolidayList ?.ServerModifiedTime},
                 };
                 return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteHolidayList(string uID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"uID" , uID}
            };
            var sql = "DELETE  FROM HolidayList WHERE uID = @uID";
             return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
