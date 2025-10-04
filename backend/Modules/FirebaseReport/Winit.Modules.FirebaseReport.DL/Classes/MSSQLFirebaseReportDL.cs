using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.Base.DL.DBManager;
using Winit.Modules.FirebaseReport.DL.Interfaces;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.FirebaseReport.DL.Classes
{
    public class MSSQLFirebaseReportDL : SqlServerDBManager, IFirebaseReportDL
    {
        public MSSQLFirebaseReportDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {

        }

        public async Task<PagedResponse<IFirebaseReport>> SelectAllFirebaseReportDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                                                uid AS UID,
                                                linked_item_uid AS LinkedItemUID,
                                                linked_item_type AS LinkedItemType,
                                                request_body AS RequestBody,
                                                user_code AS UserCode,
                                                customer_code AS CustomerCode,
                                                request_created_time AS RequestCreatedTime,
                                               -- request_posted_2_api_time AS RequestPosted2ApiTime,
                                                request_received_by_api_time AS RequestReceivedByApiTime,
                                               -- request_sent_2_service_time AS RequestSent2ServiceTime,
                                                request_received_by_service_time AS RequestReceivedByServiceTime,
                                                request_posted_to_db_time AS RequestPostedToDBTime,
                                                notification_sent_time AS NotificationSentTime,
                                                notification_received_time AS NotificationReceivedTime,
                                              --  request_sent_2_log_api_time AS RequestSent2LogApiTime,
                                                is_failed AS IsFailed,
                                                comments AS Comments,
                                                modified_time AS ModifiedTime,
                                                status AS Status,
                                                app_comments AS AppComments,
                                                next_uid AS NextUID
                                            FROM 
                                                tbl_logs)as subquery ");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                                uid AS UID,
                                                linked_item_uid AS LinkedItemUID,
                                                linked_item_type AS LinkedItemType,
                                                request_body AS RequestBody,
                                                user_code AS UserCode,
                                                customer_code AS CustomerCode,
                                                request_created_time AS RequestCreatedTime,
                                               -- request_posted_2_api_time AS RequestPosted2ApiTime,
                                                request_received_by_api_time AS RequestReceivedByApiTime,
                                               -- request_sent_2_service_time AS RequestSent2ServiceTime,
                                                request_received_by_service_time AS RequestReceivedByServiceTime,
                                                request_posted_to_db_time AS RequestPostedToDBTime,
                                                notification_sent_time AS NotificationSentTime,
                                                notification_received_time AS NotificationReceivedTime,
                                              --  request_sent_2_log_api_time AS RequestSent2LogApiTime,
                                                is_failed AS IsFailed,
                                                comments AS Comments,
                                                modified_time AS ModifiedTime,
                                                status AS Status,
                                                app_comments AS AppComments,
                                                next_uid AS NextUID
                                            FROM 
                                                tbl_logs)as subquery ");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<IFirebaseReport>(filterCriterias, sbFilterCriteria, parameters);;
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
                IEnumerable<IFirebaseReport> firebaseReportDetails = await ExecuteQueryAsync<IFirebaseReport>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<IFirebaseReport> pagedResponse = new PagedResponse<IFirebaseReport>
                {
                    PagedData = firebaseReportDetails,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IFirebaseReport> SelectFirebaseDetailsData(string UID)
        {
            try
            {
                var sql = new StringBuilder(@" SELECT 
                                                uid AS UID,
                                                linked_item_uid AS LinkedItemUID,
                                                linked_item_type AS LinkedItemType,
                                                request_body AS RequestBody,
                                                user_code AS UserCode,
                                                customer_code AS CustomerCode,
                                                request_created_time AS RequestCreatedTime,
                                               -- request_posted_2_api_time AS RequestPosted2ApiTime,
                                                request_received_by_api_time AS RequestReceivedByApiTime,
                                               -- request_sent_2_service_time AS RequestSent2ServiceTime,
                                                request_received_by_service_time AS RequestReceivedByServiceTime,
                                                request_posted_to_db_time AS RequestPostedToDBTime,
                                                notification_sent_time AS NotificationSentTime,
                                                notification_received_time AS NotificationReceivedTime,
                                              --  request_sent_2_log_api_time AS RequestSent2LogApiTime,
                                                is_failed AS IsFailed,
                                                comments AS Comments,
                                                modified_time AS ModifiedTime,
                                                status AS Status,
                                                app_comments AS AppComments,
                                                next_uid AS NextUID
                                            FROM 
                                                tbl_logs WHERE linked_item_uid = @UID");
                var parameters = new Dictionary<string, object> { { "@UID", UID } };
                IEnumerable<IFirebaseReport> FirebaseReportDetails = await ExecuteQueryAsync<IFirebaseReport>(sql.ToString(), parameters);

                return FirebaseReportDetails.FirstOrDefault()!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IFirebaseReport> GetFirebaseReportByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                            var sql = @"SELECT 
                                                uid AS UID,
                                                linked_item_uid AS LinkedItemUID,
                                                linked_item_type AS LinkedItemType,
                                                request_body AS RequestBody,
                                                user_code AS UserCode,
                                                customer_code AS CustomerCode,
                                                request_created_time AS RequestCreatedTime,
                                               -- request_posted_2_api_time AS RequestPosted2ApiTime,
                                                request_received_by_api_time AS RequestReceivedByApiTime,
                                               -- request_sent_2_service_time AS RequestSent2ServiceTime,
                                                request_received_by_service_time AS RequestReceivedByServiceTime,
                                                request_posted_to_db_time AS RequestPostedToDBTime,
                                                notification_sent_time AS NotificationSentTime,
                                                notification_received_time AS NotificationReceivedTime,
                                              --  request_sent_2_log_api_time AS RequestSent2LogApiTime,
                                                is_failed AS IsFailed,
                                                comments AS Comments,
                                                modified_time AS ModifiedTime,
                                                status AS Status,
                                                app_comments AS AppComments,
                                                next_uid AS NextUID
                                            FROM 
                                                tbl_logs  WHERE uid = @UID";
            IFirebaseReport? FirebaseReportDetails = await ExecuteSingleAsync<IFirebaseReport>(sql, parameters);
            return FirebaseReportDetails;
        }

        public async Task<IEnumerable<string>> BindFilterValues(string Case)
        {
            try
            {
                Dictionary<string, string> columnMappings = new Dictionary<string, string>
                {
                    {"Type", "LinkedItemType"},
                    {"UID", "UID"},
                };

                if (!columnMappings.ContainsKey(Case))
                {
                    throw new ArgumentException("Unsupported case", nameof(Case));
                }
                string columnName = columnMappings[Case];

                var sql = $"SELECT DISTINCT {columnName} FROM tbl_logs";

                IEnumerable<string> distinctValues = await ExecuteQueryAsync<string>(sql, null);

                return distinctValues;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
