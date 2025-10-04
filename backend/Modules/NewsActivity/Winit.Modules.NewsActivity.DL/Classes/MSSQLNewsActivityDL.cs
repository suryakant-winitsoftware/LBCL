using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.NewsActivity.DL.Interfaces;
using Winit.Modules.NewsActivity.Models.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.NewsActivity.DL.Classes
{
    public class MSSQLNewsActivityDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, INewsActivityDL
    {
        public MSSQLNewsActivityDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<int> CreateNewsActivity(INewsActivity newsActivity)
        {
            try
            {
                var sql = @"INSERT INTO news_activity ( uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss, activity_type, title, description, publish_date, is_active)
                            VALUES ( @Uid, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @Ss, @ActivityType, @Title, @Description, @PublishDate, @IsActive);";
                return await ExecuteNonQueryAsync(sql, newsActivity);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PagedResponse<INewsActivity>> SelectAllNewsActivities(PagingRequest pagingRequest)
        {
            try
            {
                StringBuilder sql = new StringBuilder(@"select * from(SELECT
                                                                                id AS Id,
                                                                                uid AS Uid,
                                                                                created_by AS CreatedBy,
                                                                                created_time AS CreatedTime,
                                                                                modified_by AS ModifiedBy,
                                                                                modified_time AS ModifiedTime,
                                                                                server_add_time AS ServerAddTime,
                                                                                server_modified_time AS ServerModifiedTime,
                                                                                ss AS Ss,
                                                                                activity_type AS ActivityType,
                                                                                title AS Title,
                                                                                description AS Description,
                                                                                publish_date AS PublishDate,
                                                                                is_active AS IsActive
                                                                            FROM
                                                                                news_activity)as SUBQuery");

                var sqlCount = new StringBuilder();
                if (pagingRequest.IsCountRequired)
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
                                                            ss AS Ss,
                                                            activity_type AS ActivityType,
                                                            title AS Title,
                                                            description AS Description,
                                                            publish_date AS PublishDate,
                                                            is_active AS IsActive
                                                        FROM
                                                            news_activity)as SUBQuery");
                }
                var parameters = new Dictionary<string, object>();

                if (pagingRequest.FilterCriterias != null && pagingRequest.FilterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<INewsActivity>(pagingRequest.FilterCriterias, sbFilterCriteria, parameters);

                    sql.Append(sbFilterCriteria);
                    if (pagingRequest.IsCountRequired)
                    {
                        sqlCount.Append(sbFilterCriteria);
                    }
                }

                if (pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(pagingRequest.SortCriterias, sql);
                }

                if (pagingRequest.PageNumber > 0 && pagingRequest.PageSize > 0)
                {
                    if (pagingRequest.SortCriterias != null && pagingRequest.SortCriterias.Count > 0)
                    {
                        sql.Append($" OFFSET {(pagingRequest.PageNumber - 1) * pagingRequest.PageSize} ROWS FETCH NEXT {pagingRequest.PageSize} ROWS ONLY");
                    }
                    else
                    {
                        sql.Append($" ORDER BY Id OFFSET {(pagingRequest.PageNumber - 1) * pagingRequest.PageSize} ROWS FETCH NEXT {pagingRequest.PageSize} ROWS ONLY");
                    }
                }
                List<INewsActivity> roles = await ExecuteQueryAsync<INewsActivity>(sql.ToString(), parameters);
                int totalCount = -1;
                if (pagingRequest.IsCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<INewsActivity> pagedResponse = new PagedResponse<INewsActivity>
                {
                    PagedData = roles,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> UpdateNewsActivity(INewsActivity newsActivity)
        {
            try
            {
                var sql = @"UPDATE news_activity
                            SET uid = @Uid, created_by = @CreatedBy, created_time = @CreatedTime, 
                            modified_by = @ModifiedBy, modified_time = @ModifiedTime, server_add_time = @ServerAddTime, 
                            server_modified_time = @ServerModifiedTime, ss = @Ss, activity_type = @ActivityType, title = @Title, 
                            description = @Description,
                             publish_date = @PublishDate, is_active = @IsActive
                            WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, newsActivity);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteNewsActivityByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"delete from news_activity where uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
        public async Task<INewsActivity> GetNewsActivitysByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
            var sql = @"select 
                                                    * from news_activity WHERE UID = @UID";
            INewsActivity newsActivity = await ExecuteSingleAsync<INewsActivity>(sql, parameters);
            return newsActivity;
        }
    }
}

