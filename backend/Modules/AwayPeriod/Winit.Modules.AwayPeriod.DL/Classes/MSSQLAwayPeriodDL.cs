using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.AwayPeriod.DL.Interfaces;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Common;

namespace Winit.Modules.AwayPeriod.DL.Classes
{
    public class MSSQLAwayPeriodDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IAwayPeriodDL
    {
        public MSSQLAwayPeriodDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>> GetAwayPeriodDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (SELECT ap.id AS Id, ap.uid AS UID, ap.created_by AS CreatedBy, ap.created_time AS CreatedTime,
                                            ap.modified_by AS ModifiedBy, ap.modified_time AS ModifiedTime, ap.server_add_time AS ServerAddTime, 
                                            ap.server_modified_time AS ServerModifiedTime, ap.org_uid AS OrgUID, ap.linked_item_type AS LinkedItemType, 
                                            ap.linked_item_uid AS LinkedItemUID, ap.description AS Description, ap.from_date AS FromDate, 
                                            ap.to_date AS ToDate, ap.is_active AS IsActive FROM away_period AS ap)As SubQuery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM 
                                            (SELECT ap.id AS Id, ap.uid AS UID, ap.created_by AS CreatedBy, ap.created_time AS CreatedTime,
                                            ap.modified_by AS ModifiedBy, ap.modified_time AS ModifiedTime, ap.server_add_time AS ServerAddTime, 
                                            ap.server_modified_time AS ServerModifiedTime, ap.org_uid AS OrgUID, ap.linked_item_type AS LinkedItemType, 
                                            ap.linked_item_uid AS LinkedItemUID, ap.description AS Description, ap.from_date AS FromDate, 
                                            ap.to_date AS ToDate, ap.is_active AS IsActive FROM away_period AS ap)As SubQuery");
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
                IEnumerable<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod> AwayPeriodDetails = await ExecuteQueryAsync<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
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
        { "UID", UID }
    };

            var sql = @"SELECT ap.id AS Id, ap.uid AS UID, ap.created_by AS CreatedBy, ap.created_time AS CreatedTime,
                ap.modified_by AS ModifiedBy, ap.modified_time AS ModifiedTime, ap.server_add_time AS ServerAddTime, 
                ap.server_modified_time AS ServerModifiedTime, ap.org_uid AS OrgUID, ap.linked_item_type AS LinkedItemType, 
                ap.linked_item_uid AS LinkedItemUID, ap.description AS Description, ap.from_date AS FromDate, 
                ap.to_date AS ToDate, ap.is_active AS IsActive 
                FROM away_period AS ap 
                WHERE ap.UID = @UID";

            Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod? awayPeriodDetails =
                await ExecuteSingleAsync<Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod>(sql, parameters);

            return awayPeriodDetails;
        }


        public async Task<int> CreateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod)
        {
            var sql = @"
                        INSERT INTO away_period (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                        server_modified_time, org_uid, linked_item_type, linked_item_uid, description, from_date, to_date, is_active)
                        VALUES 
                        (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @OrgUID,
                        @LinkedItemType, @LinkedItemUID, @Description, @FromDate, @ToDate, @IsActive);";

            return await ExecuteNonQueryAsync(sql, awayPeriod);
        }
        public async Task<int> UpdateAwayPeriodDetails(Winit.Modules.AwayPeriod.Model.Interfaces.IAwayPeriod awayPeriod)
        {
            var sql = @"
                        UPDATE away_period 
                        SET modified_by = @ModifiedBy, 
                            modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime, 
                            linked_item_type = @LinkedItemType, 
                            linked_item_uid = @LinkedItemUID, 
                            description = @Description, 
                            is_active = @IsActive 
                        WHERE uid = @UID;";

            return await ExecuteNonQueryAsync(sql, awayPeriod);
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
