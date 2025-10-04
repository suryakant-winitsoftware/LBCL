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
    public class MSSQLHolidayListDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IHolidayListDL
    {
        public MSSQLHolidayListDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>> SelectAllHolidayList(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT hl.id AS Id, hl.uid AS UID, hl.company_uid AS CompanyUID, hl.org_uid AS OrgUID,
                                            hl.name AS Name, hl.description AS Description, hl.location_uid AS LocationUID, 
                                            hl.is_active AS IsActive, hl.year AS Year, hl.ss AS SS, hl.created_by AS CreatedBy,
                                            hl.created_time AS CreatedTime, hl.modified_by AS ModifiedBy, hl.modified_time AS ModifiedTime,
                                            hl.server_add_time AS ServerAddTime, hl.server_modified_time AS ServerModifiedTime FROM holiday_list AS hl)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT hl.id AS Id, hl.uid AS UID, hl.company_uid AS CompanyUID, hl.org_uid AS OrgUID,
                                            hl.name AS Name, hl.description AS Description, hl.location_uid AS LocationUID, 
                                            hl.is_active AS IsActive, hl.year AS Year, hl.ss AS SS, hl.created_by AS CreatedBy,
                                            hl.created_time AS CreatedTime, hl.modified_by AS ModifiedBy, hl.modified_time AS ModifiedTime,
                                            hl.server_add_time AS ServerAddTime, hl.server_modified_time AS ServerModifiedTime FROM holiday_list AS hl)");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> HolidayListDetails = await ExecuteQueryAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList> pagedResponse = new PagedResponse<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>
                {
                    PagedData = HolidayListDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
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
            var sql = @"SELECT hl.id AS Id, hl.uid AS UID, hl.company_uid AS CompanyUID, hl.org_uid AS OrgUID,
                                            hl.name AS Name, hl.description AS Description, hl.location_uid AS LocationUID, 
                                            hl.is_active AS IsActive, hl.year AS Year, hl.ss AS SS, hl.created_by AS CreatedBy,
                                            hl.created_time AS CreatedTime, hl.modified_by AS ModifiedBy, hl.modified_time AS ModifiedTime,
                                            hl.server_add_time AS ServerAddTime, hl.server_modified_time AS ServerModifiedTime FROM 
                                            holiday_list AS hl WHERE hl.uid = @UID";
            Winit.Modules.Holiday.Model.Interfaces.IHolidayList? HolidayListDetails = await ExecuteSingleAsync<Winit.Modules.Holiday.Model.Interfaces.IHolidayList>(sql, parameters);
            return HolidayListDetails;
        }
        public async Task<int> CreateHolidayList(Winit.Modules.Holiday.Model.Interfaces.IHolidayList createHolidayList)
        {
            try
            {
                     var sql = @"INSERT INTO holiday_list 
                               (uid, company_uid, org_uid, name, description, location_uid, is_active, year, ss, created_by, 
                                created_time, modified_by, modified_time, server_add_time, server_modified_time) 
                                VALUES
                                (@UID, @CompanyUID, @OrgUID, @Name, @Description, @LocationUID, @IsActive, @Year,
                                @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime)";
                return await ExecuteNonQueryAsync(sql, createHolidayList);
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
                var sql = @"UPDATE holiday_list 
                                    SET 
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
                                    WHERE 
                                        uid = @UID;";
                return await ExecuteNonQueryAsync(sql, updateHolidayList);
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
            var sql = "DELETE  FROM holiday_list WHERE uid = @uID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
