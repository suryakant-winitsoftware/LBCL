using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.AwayPeriod.DL.Interfaces;
using Winit.Modules.AwayPeriod.Model.Classes;
using Winit.Modules.AwayPeriod.Model.Interfaces;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.AwayPeriod.DL.Classes
{
    public class PGSQLAwayPeriodDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IAwayPeriodDL
    {
        public PGSQLAwayPeriodDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }
        public async Task<PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>> GetAwayPeriodDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT id AS Id,uid AS UID,
                        created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,org_uid AS OrgUID,linked_item_type AS LinkedItemType,
                        linked_item_uid AS LinkedItemUID,description AS Description,from_date AS FromDate,to_date AS ToDate ,   is_active AS IsActive
                        FROM public.away_period)as SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT id AS Id,uid AS UID,
                        created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,org_uid AS OrgUID,linked_item_type AS LinkedItemType,
                        linked_item_uid AS LinkedItemUID,description AS Description,from_date AS FromDate,to_date AS ToDate ,   is_active AS IsActive
                        FROM public.away_period)as SubQuery");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>(filterCriterias, sbFilterCriteria, parameters);
                    sql.Append(sbFilterCriteria);
                    if (isCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql, true);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAwayPeriod>().GetType();
                IEnumerable<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> AwayPeriodDetails = await ExecuteQueryAsync<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>(sql.ToString(), parameters, type);
                int totalCount = -1;
                if (isCountRequired)
                {
                    // Get the total count of records
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> pagedResponse = new PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>
                {
                    PagedData = AwayPeriodDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> GetAwayPeriodDetailsByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"SELECT id AS Id,uid AS UID,
                        created_by AS CreatedBy,created_time AS CreatedTime,modified_by AS ModifiedBy,modified_time AS ModifiedTime,
                        server_add_time AS ServerAddTime,server_modified_time AS ServerModifiedTime,org_uid AS OrgUID,linked_item_type AS LinkedItemType,
                        linked_item_uid AS LinkedItemUID,description AS Description,from_date AS FromDate,to_date AS ToDate ,   is_active AS IsActive
                        FROM public.away_period WHERE uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.IAwayPeriod>().GetType();
            Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod AwayPeriodDetails = await ExecuteSingleAsync<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>(sql, parameters, type);
            return AwayPeriodDetails;
        }

        public async Task<int> CreateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod)
        {
            var sql = @"INSERT INTO away_period (
    uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, org_uid,
    linked_item_type, linked_item_uid, description, from_date, to_date, is_active) 
        VALUES (    @uid, @created_by, @created_time, @modified_by, @modified_time, @server_add_time, @server_modified_time, @org_uid,
    @linked_item_type, @linked_item_uid, @description, @from_date, @to_date, @is_active);";

            Dictionary<string, object> parameters = new Dictionary<string, object>
{
    {"uid", awayPeriod.UID},
    {"created_by", awayPeriod.CreatedBy},
    {"created_time", awayPeriod.CreatedTime},
    {"modified_by", awayPeriod.ModifiedBy},
    {"modified_time", awayPeriod.ModifiedTime},
    {"server_add_time", awayPeriod.ServerAddTime},
    {"server_modified_time", awayPeriod.ServerModifiedTime},
    {"org_uid", awayPeriod.OrgUID},
    {"linked_item_type", awayPeriod.LinkedItemType},
    {"linked_item_uid", awayPeriod.LinkedItemUID},
    {"description", awayPeriod.Description},
    {"from_date", awayPeriod.FromDate},
    {"to_date", awayPeriod.ToDate},
    {"is_active", awayPeriod.IsActive}
};

            return await ExecuteNonQueryAsync(sql, parameters);

        }

        public async Task<int> UpdateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod)
        {
            try
            {
                var sql = @"UPDATE away_period 
SET 
    modified_by = @modified_by, 
    modified_time = @modified_time, 
    server_modified_time = @server_modified_time, 
    linked_item_type = @linked_item_type, 
    linked_item_uid = @linked_item_uid, 
    description = @description, 
    is_active = @is_active 
WHERE 
    uid = @uid;
";

                Dictionary<string, object> parameters = new Dictionary<string, object>
{
    {"uid", awayPeriod.UID},
    {"modified_by", awayPeriod.ModifiedBy},
    {"modified_time", awayPeriod.ModifiedTime},
    {"server_modified_time", awayPeriod.ServerModifiedTime},
    {"linked_item_type", awayPeriod.LinkedItemType},
    {"linked_item_uid", awayPeriod.LinkedItemUID},
    {"description", awayPeriod.Description},
    {"is_active", awayPeriod.IsActive}
};

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteAwayPeriodDetail(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"DELETE  FROM away_period WHERE uid = @UID";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
