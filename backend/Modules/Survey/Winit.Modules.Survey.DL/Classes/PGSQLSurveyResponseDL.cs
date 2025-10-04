using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Collections.Specialized;
using Winit.Modules.Survey.Model.Classes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Spreadsheet;
using WINIT.Shared.Models.Models;
using Nest;
using Azure;
using Winit.Shared.Models.Constants;


namespace Winit.Modules.Survey.DL.Classes
{
    public class PGSQLSurveyResponseDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISurveyResponseDL
    {
        private readonly string _surveyBasePath;

        public PGSQLSurveyResponseDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _surveyBasePath = config["SurveyBasePath"];
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>> GetAllSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT sr.id AS Id, sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                            sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, sr.ss AS SS, sr.response_date AS ResponseDate, 
                            sr.response_data AS ResponseData, sr.beat_history_uid AS BeatHistoryUID, sr.survey_uid AS SurveyUID, 
                            sr.org_uid AS OrgUID, sr.linked_item_type AS LinkedItemType, sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, sr.created_by AS CreatedBy,
                            sr.job_position_uid AS JobPositionUid, sr.modified_by AS ModifiedBy, sr.emp_uid AS EmpUID, sr.route_uid AS RouteUID,
                            sr.store_history_uid AS StoreHistoryUID FROM survey_response sr)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT sr.id AS Id, sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                            sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, sr.ss AS SS, sr.response_date AS ResponseDate, 
                            sr.response_data AS ResponseData, sr.beat_history_uid AS BeatHistoryUID, sr.survey_uid AS SurveyUID, 
                            sr.org_uid AS OrgUID, sr.linked_item_type AS LinkedItemType, sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, sr.created_by AS CreatedBy,
                            sr.job_position_uid AS JobPositionUid, sr.modified_by AS ModifiedBy, sr.emp_uid AS EmpUID, sr.route_uid AS RouteUID,
                            sr.store_history_uid AS StoreHistoryUID FROM survey_response sr)as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>(filterCriterias, sbFilterCriteria, parameters);

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

                IEnumerable<Model.Interfaces.ISurveyResponseModel> surveyResponseModelList = await ExecuteQueryAsync<Model.Interfaces.ISurveyResponseModel>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>
                {
                    PagedData = surveyResponseModelList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel)
        {
            try
            {
                string? result = await CheckIfUIDExistsInDB(DbTableName.SurveyResponse, surveyResponseModel.UID);
                if (string.IsNullOrEmpty(result))
                {
                    var sql = @"INSERT INTO survey_response ( created_time, modified_time, server_add_time, server_modified_time, 
                            ss, response_date, response_data, beat_history_uid, survey_uid, org_uid, linked_item_type, linked_item_uid, uid, created_by, 
                            job_position_uid, modified_by, emp_uid, route_uid, store_history_uid,activity_type) VALUES ( @CreatedTime, @ModifiedTime, @ServerAddTime,
                            @ServerModifiedTime, 0, @ResponseDate, @ResponseData::jsonb, @BeatHistoryUID, @SurveyUID, @OrgUID, @LinkedItemType, @LinkedItemUID,
                            @UID, @CreatedBy, @JobPositionUid, @ModifiedBy, @EmpUID, @RouteUID, @StoreHistoryUID,@ActivityType)";
                    return await ExecuteNonQueryAsync(sql, surveyResponseModel);
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Survey Response", ex);
            }
        }

        public async Task<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> GetSurveyResponseByUID(string UID)
        {
            try
            {
                var sql = @"SELECT sr.id AS Id, sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                            sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, sr.ss AS SS, sr.response_date AS ResponseDate, 
                            sr.response_data AS ResponseData, sr.beat_history_uid AS BeatHistoryUID, sr.survey_uid AS SurveyUID, 
                            sr.org_uid AS OrgUID, sr.linked_item_type AS LinkedItemType, sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, sr.created_by AS CreatedBy,
                            sr.job_position_uid AS JobPositionUid, sr.modified_by AS ModifiedBy, sr.emp_uid AS EmpUID, sr.route_uid AS RouteUID,
                            sr.store_history_uid AS StoreHistoryUID FROM survey_response sr WHERE sr.uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", UID } };
                return await ExecuteSingleAsync<ISurveyResponseModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching SurveyResponse UID", ex);
            }
        }


        public async Task<int> UpdateSurveyResponse(Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel surveyResponseModel)
        {
            try
            {
                var sql = @"UPDATE survey_response SET   modified_time = @ModifiedTime, 
                             server_modified_time = @ServerModifiedTime, ss = @SS, 
                             response_date = @ResponseDate, 
                             response_data = @ResponseData, 
                             modified_by = @ModifiedBy 
                             WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, surveyResponseModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Capture Competitor", ex);
            }
        }

        public async Task<int> TicketStatusUpdate(string uid, string status, string empUID)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"UID",uid},
                    {"status",status},
                    {"ModifiedTime",DateTime.Now},
                    {"ServerModifiedTime",DateTime.Now},
                    {"ModifiedBy",empUID}
                };
                var sql = @"UPDATE survey_response SET   modified_time = @ModifiedTime, 
                             server_modified_time = @ServerModifiedTime, 
                             modified_by = @ModifiedBy,
                             status=@Status
                             WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ", ex);
            }
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>> GetViewSurveyResponse(
     List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM (
                                   SELECT 
                                       TO_CHAR(sr.created_time, 'YYYY-MM-DD') AS CreatedDate,
                                       CAST(sr.created_time AS time(0)) AS CreatedTime,
                                       sr.survey_uid AS SurveyName,
                                       e.code AS UserCode,
                                       e.name AS UserName,
                                         '[' ||  e.code || '] ' ||e.name AS Users,

                                       s.code AS CustomerCode,
                                       s.name AS CustomerName,
                                    '[' ||  s.code || '] ' ||s.name AS Stores_Customers,
                                       sr.uid AS SurveyResponseUID,
                                       sr.activity_type AS ActivityType,
                                        CASE 
                                        WHEN sr.activity_type = 'RaiseTicket' AND sr.status IS NULL THEN 'Open'
                                        ELSE COALESCE(sr.status, '') END AS Status,
                                       sr.created_time AS CreatedDateTime,
                                                       CASE
                                        WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 minute' THEN 'just now'
                                        WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 hour' 
                                            THEN CONCAT(EXTRACT(MINUTE FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' min(s) ago')
                                        WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 day' 
                                            THEN CONCAT(EXTRACT(HOUR FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' hour(s) ago')
                                        ELSE CONCAT(EXTRACT(DAY FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' day(s) ago')
                                    END AS SurveyAge,
                              -- Location info
                                    jp.location_value AS LocationValue,
                                    LR.code AS LocationCode,
                                    LR.name AS LocationName,
                                    R.code as Role
                                   FROM survey_response sr 
                                   JOIN emp e ON e.uid = sr.emp_uid
                                  Left JOIN store s ON s.uid = sr.linked_item_uid 
                                 -- Join to get job and location
                                    LEFT JOIN job_position jp ON jp.emp_uid = e.uid
                                    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
                                    LEFT JOIN location LR ON LR.uid = LC.parent_uid
	                                INNER JOIN roles R ON R.uid = jp.user_role_uid

                                   WHERE sr.linked_item_type = 'Store'  
                                    --AND sr.created_time BETWEEN @StartDate AND @EndDate

                               ) AS subquery WHERE 1=1 ");

                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount.Append(@"SELECT COUNT(1) AS Cnt FROM (
                           SELECT 
                               TO_CHAR(sr.created_time, 'YYYY-MM-DD') AS CreatedDate,
                               CAST(sr.created_time AS time(0)) AS CreatedTime,
                               sr.survey_uid AS SurveyName,
                               e.code AS UserCode,
                               e.name AS UserName,
                               '[' ||  e.code || '] ' ||e.name AS Users,
                               s.code AS CustomerCode,
                               s.name AS CustomerName,
                                '[' ||  s.code || '] ' ||s.name AS Stores_Customers,
                               sr.uid AS SurveyResponseUID,
                               sr.activity_type AS ActivityType,
                                  CASE 
                                        WHEN sr.activity_type = 'RaiseTicket' AND sr.status IS NULL THEN 'Open'
                                        ELSE COALESCE(sr.status, '') END AS Status,
                                sr.created_time AS CreatedDateTime,
                                    CASE
                                WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 minute' THEN 'just now'
                                WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 hour' 
                                    THEN CONCAT(EXTRACT(MINUTE FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' min(s) ago')
                                WHEN CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time < INTERVAL '1 day' 
                                    THEN CONCAT(EXTRACT(HOUR FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' hour(s) ago')
                                ELSE CONCAT(EXTRACT(DAY FROM CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata' - sr.created_time)::INT, ' day(s) ago')
                            END AS SurveyAge,
                                -- Location info
                                    jp.location_value AS LocationValue,
                                    LR.code AS LocationCode,
                                    LR.name AS LocationName,
                                    R.code as Role
                           FROM survey_response sr 
                           JOIN emp e ON e.uid = sr.emp_uid
                          Left JOIN store s ON s.uid = sr.linked_item_uid 

                         -- Join to get job and location
                            LEFT JOIN job_position jp ON jp.emp_uid = e.uid
                            LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
                            LEFT JOIN location LR ON LR.uid = LC.parent_uid
	                        INNER JOIN roles R ON R.uid = jp.user_role_uid

                           WHERE sr.linked_item_type = 'Store' 
                            -- AND sr.created_time BETWEEN @StartDate AND @EndDate

                       ) AS subquery WHERE 1=1");
                }

                var parameters = new Dictionary<string, object>();
                var activityTypeFilter = filterCriterias?
                    .FirstOrDefault(f => string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(f.Value?.ToString(), "Others", StringComparison.OrdinalIgnoreCase));
                StringBuilder sbFilterCriteria = new(" AND  ");
                filterCriterias?.RemoveAll(f =>
                    string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(f.Value?.ToString(), "Others", StringComparison.OrdinalIgnoreCase));
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>(filterCriterias, sbFilterCriteria, parameters);
                }
                if (activityTypeFilter != null && filterCriterias != null && filterCriterias.Count > 0)
                {
                    sbFilterCriteria.Append(" AND ");
                }
                if (activityTypeFilter != null)
                {
                    sbFilterCriteria.Append("  ActivityType NOT IN ('ShopObservation', 'RaiseTicket')");
                }
                sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    sqlCount.Append(sbFilterCriteria);
                }
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append(" ORDER BY ");
                    AppendSortCriteria(sortCriterias, sql);
                }
                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                IEnumerable<Model.Interfaces.IViewSurveyResponse> viewSurveyResponseList =
                    await ExecuteQueryAsync<Model.Interfaces.IViewSurveyResponse>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                return new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>
                {
                    PagedData = viewSurveyResponseList,
                    TotalCount = totalCount
                };
            }
            catch (Exception)
            {
                throw;
            }
        }



        //public async Task<ISurveyResponseViewDTO> ViewSurveyResponseByUID(string UID)
        //{
        //    try
        //    {
        //        var sql = @"SELECT 
        //                s.code AS Category, 
        //                s.survey_data AS SurveyData, 
        //                sr.response_data AS ResponseData,
        //                fs.linked_item_uid As LinkedItemUID,
        //                fs.relative_path As RelativePath
        //                FROM 
        //                survey s
        //                JOIN 
        //                survey_response sr 
        //                ON 
        //                sr.survey_uid = s.uid 
        //                JOIN 
        //               file_sys fs ON LEFT(fs.linked_item_uid, 36) = sr.uid
        //                WHERE 
        //                sr.uid = @UID";

        //        var parameters = new Dictionary<string, object> { { "UID", UID } };
        //        var result = await ExecuteSingleAsync<dynamic>(sql, parameters);
        //        if (result == null)
        //            throw new Exception("No data found for the given UID");
        //        string surveyDataJson = result.surveydata;
        //        string responseDataJson = result.responsedata;
        //        var resultList = result.ToList();


        //        var questionAnswers = MapQuestionsAndResponses(surveyDataJson, responseDataJson);

        //        return new SurveyResponseViewDTO
        //        {
        //           // Category = result.category,
        //            QuestionAnswers = questionAnswers
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error fetching SurveyResponse UID", ex);
        //    }
        //}
        //    public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>> GetViewSurveyResponse(
        //List<SortCriteria> sortCriterias,
        //int pageNumber,
        //int pageSize,
        //List<FilterCriteria> filterCriterias,
        //bool isCountRequired)
        //    {
        //        try
        //        {
        //            var sql = new StringBuilder(@"SELECT * FROM (
        //                    SELECT 
        //                        TO_CHAR(sr.created_time, 'YYYY-MM-DD') AS CreatedDate,
        //                        CAST(sr.created_time AS time(0)) AS CreatedTime,
        //                        sr.survey_uid AS SurveyName,
        //                        e.code AS UserCode,
        //                        e.name AS UserName,
        //                        s.code AS CustomerCode,
        //                        s.name AS CustomerName,
        //                        sr.uid AS SurveyResponseUID,
        //                        sr.activity_type AS ActivityType,
        //                        sr.status AS Status,
        //                        sr.created_time AS CreatedDateTime 
        //                    FROM survey_response sr 
        //                    JOIN emp e ON e.uid = sr.emp_uid
        //                    LEFT JOIN store s ON s.uid = sr.linked_item_uid 
        //                    WHERE sr.linked_item_type = 'Store'
        //                ) AS subquery WHERE 1=1");

        //            var sqlCount = new StringBuilder();
        //            if (isCountRequired)
        //            {
        //                sqlCount.Append(@"SELECT COUNT(1) AS Cnt FROM (
        //                SELECT 
        //                    TO_CHAR(sr.created_time, 'YYYY-MM-DD') AS CreatedDate,
        //                    CAST(sr.created_time AS time(0)) AS CreatedTime,
        //                    sr.survey_uid AS SurveyName,
        //                    e.code AS UserCode,
        //                    e.name AS UserName,
        //                    s.code AS CustomerCode,
        //                    s.name AS CustomerName,
        //                    sr.uid AS SurveyResponseUID,
        //                    sr.activity_type AS ActivityType
        //                FROM survey_response sr 
        //                JOIN emp e ON e.uid = sr.emp_uid
        //                LEFT JOIN store s ON s.uid = sr.linked_item_uid 
        //                WHERE sr.linked_item_type = 'Store'
        //            ) AS subquery WHERE 1=1");
        //            }

        //            var parameters = new Dictionary<string, object>();

        //            // Handle ActivityType filter exclusion
        //            var activityTypeFilter = filterCriterias?
        //                .FirstOrDefault(f => string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase)
        //                                  && string.Equals(f.Value?.ToString(), "Others", StringComparison.OrdinalIgnoreCase));

        //            // Remove handled filter to prevent duplication
        //            filterCriterias?.RemoveAll(f =>
        //                string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase) &&
        //                string.Equals(f.Value?.ToString(), "Others", StringComparison.OrdinalIgnoreCase));

        //            // ✅ Handle CreatedDate filter (default to today if not provided)
        //            var createdDateFilter = filterCriterias?
        //                .FirstOrDefault(f => string.Equals(f.Name?.ToString(), "CreatedDate", StringComparison.OrdinalIgnoreCase));

        //            if (createdDateFilter == null)
        //            {
        //                createdDateFilter = new FilterCriteria(
        //                       name: "CreatedDate",
        //                       value: DateTime.Now.ToString("yyyy-MM-dd"),
        //                       type: FilterType.Equal,
        //                       dataType: typeof(string), // or typeof(DateTime) if your logic supports it
        //                       filterGroup: FilterGroupType.Field,
        //                       filterMode: FilterMode.And
        //                   );
        //            }

        //            StringBuilder sbFilterCriteria = new(" AND ");

        //            if (filterCriterias != null && filterCriterias.Count > 0)
        //            {
        //                AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>(
        //                    filterCriterias, sbFilterCriteria, parameters);
        //            }

        //            if (activityTypeFilter != null && filterCriterias.Count > 0)
        //            {
        //                sbFilterCriteria.Append(" AND ");
        //            }

        //            if (activityTypeFilter != null)
        //            {
        //                sbFilterCriteria.Append(" ActivityType NOT IN ('ShopObservation', 'RaiseTicket')");
        //            }

        //            sql.Append(sbFilterCriteria);
        //            if (isCountRequired)
        //            {
        //                sqlCount.Append(sbFilterCriteria);
        //            }

        //            if (sortCriterias != null && sortCriterias.Count > 0)
        //            {
        //                sql.Append(" ORDER BY ");
        //                AppendSortCriteria(sortCriterias, sql);
        //            }

        //            if (pageNumber > 0 && pageSize > 0)
        //            {
        //                sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
        //            }

        //            IEnumerable<Model.Interfaces.IViewSurveyResponse> viewSurveyResponseList =
        //                await ExecuteQueryAsync<Model.Interfaces.IViewSurveyResponse>(sql.ToString(), parameters);

        //            int totalCount = 0;
        //            if (isCountRequired)
        //            {
        //                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
        //            }

        //            return new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>
        //            {
        //                PagedData = viewSurveyResponseList,
        //                TotalCount = totalCount
        //            };
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }

        private List<QuestionAnswer> MapQAndR_ImageExport(string surveyDataJson, string responseDataJson, string ImagePath, string RelativePathVideo, string RelativePathRaiseTkt)
        {
            var questionAnswers = new List<QuestionAnswer>();
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var surveyData = System.Text.Json.JsonSerializer.Deserialize<SurveyData>(surveyDataJson, options);
                var responseData = System.Text.Json.JsonSerializer.Deserialize<ResponseData>(responseDataJson, options);
                questionAnswers = new List<QuestionAnswer>();
                foreach (var section in surveyData.Sections)
                {
                    foreach (var question in section.Questions)
                    {
                        var response = responseData.Responses.FirstOrDefault(r => r.QuestionId == question.Id);
                        if (response != null)
                        {
                            questionAnswers.Add(new QuestionAnswer
                            {
                                Question = question.Label,
                                Value = response.Value,
                                Values = response.Values,
                                Points = response.Points,
                                Category = section.SectionTitle,
                                ActivityType = response.ActivityType,
                                ImagePath = ImagePath,
                                RelativePathVideo = RelativePathVideo,
                                RelativePathRaiseTkt = RelativePathRaiseTkt,
                            });
                        }
                    }
                }
            }

            catch (Exception ex)
            {

            }


            return questionAnswers;
        }

        private List<QuestionAnswer> MapQuestionsAndResponses_old(string surveyDataJson, string responseDataJson)
        {
            var questionAnswers = new List<QuestionAnswer>();
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var surveyData = System.Text.Json.JsonSerializer.Deserialize<SurveyData>(surveyDataJson, options);
                var responseData = System.Text.Json.JsonSerializer.Deserialize<ResponseData>(responseDataJson, options);
                questionAnswers = new List<QuestionAnswer>();
                foreach (var section in surveyData.Sections)
                {
                    foreach (var question in section.Questions)
                    {
                        var response = responseData.Responses.FirstOrDefault(r => r.QuestionId == question.Id);
                        if (response != null)
                        {
                            questionAnswers.Add(new QuestionAnswer
                            {
                                Question = question.Label,
                                Value = response.Value,
                                Values = response.Values,
                                Points = response.Points,
                                Category = section.SectionTitle,
                                ActivityType = response.ActivityType,
                            });
                        }
                    }
                }
            }

            catch (Exception ex)
            {

            }


            return questionAnswers;
        }

        public async Task<ISurveyResponseViewDTO> ViewSurveyResponseByUID(string UID)
        {
            try
            {
                var sql = @"SELECT 
                    s.code AS Category, 
                    s.survey_data AS SurveyData, 
                    sr.response_data AS ResponseData,
                    --fs.linked_item_uid AS LinkedItemUID,
                    COALESCE(fs.linked_item_uid, fsss.linked_item_uid) AS LinkedItemUID,
                    fs.relative_path AS RelativePath,
					fss.relative_path AS RelativePathVideo,
                    fsss.relative_path AS RelativePathRaiseTkt,
                    st.code AS CustomerCode,
                    st.name AS CustomerName
                    FROM survey s
                    JOIN survey_response sr ON sr.survey_uid = s.uid 
                    LEFT JOIN file_sys fs ON LEFT(fs.linked_item_uid, 36) = sr.uid AND fs.linked_item_uid LIKE '%-q11'
    				LEFT JOIN file_sys fss ON LEFT(fss.linked_item_uid, 36) = sr.uid AND fss.linked_item_uid LIKE '%-capture_competitor'
                    LEFT JOIN file_sys fsss ON LEFT(fsss.linked_item_uid, 36) = sr.uid AND fsss.linked_item_uid LIKE '%-imageCapture'
                    Left JOIN store st ON st.uid = sr.linked_item_uid
                    WHERE sr.uid = @UID";
                var sql1 = @"SELECT 
                    s.code AS Category, 
                    s.survey_data AS SurveyData, 
                    sr.response_data AS ResponseData,
                    fs.linked_item_uid AS LinkedItemUID,
                    fs.relative_path AS RelativePath,
                    st.code AS CustomerCode,
                    st.name AS CustomerName
                    FROM survey s
                    JOIN survey_response sr ON sr.survey_uid = s.uid 
                    LEFT JOIN file_sys fs ON LEFT(fs.linked_item_uid, 36) = sr.uid
                    Left JOIN store st ON st.uid = sr.linked_item_uid
                    WHERE sr.uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", UID } };

                var results = await ExecuteQueryAsync<SurveyResponseResult>(sql, parameters);
                if (results == null || !results.Any())
                    throw new Exception("No data found for the given UID");

                var firstResult = results.First();
                string customerName = firstResult.CustomerName;
                string customerCode = firstResult.CustomerCode;

                var fileSysData = results
    .Where(r => !string.IsNullOrEmpty(r.LinkedItemUID))
    .ToDictionary(
        r =>
        {
            string surveyResponseUID = r.LinkedItemUID.Length >= 36 ? r.LinkedItemUID.Substring(0, 36) : r.LinkedItemUID;

            string questionUID = r.LinkedItemUID.Length > 37 ? r.LinkedItemUID.Substring(37) : string.Empty;

            return new Tuple<string, string>(surveyResponseUID, questionUID);
        },
        r => new Tuple<string, string, string>(r.RelativePath, r.RelativePathVideo, r.RelativePathRaiseTkt)
    );
                var surveyDataJson = results.First().SurveyData;
                var responseDataJson = results.First().ResponseData;

                var surveyData = System.Text.Json.JsonSerializer.Deserialize<SurveyData>(surveyDataJson);
                var responseData = System.Text.Json.JsonSerializer.Deserialize<ResponseData>(responseDataJson);

                var questionAnswers = MapQuestionsAndResponsesForSurvey(responseData, fileSysData, customerCode, customerName);

                return new SurveyResponseViewDTO
                {
                    QuestionAnswers = questionAnswers,
                    CustomerName = customerName,
                    CustomerCode = customerCode
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching SurveyResponse UID", ex);
            }
        }

        private List<QuestionAnswer> MapQuestionsAndResponsesForSurvey(
    ResponseData responseData,
    Dictionary<Tuple<string, string>, Tuple<string, string, string>> fileSysData,
    string customerCode,
    string customerName)
        {
            var questionAnswers = new List<QuestionAnswer>();

            // First, let's find the paths for q11
            var pathsForQ11 = fileSysData
                .Where(kv => kv.Key.Item2 == "q11" || kv.Key.Item2 == "imageCapture")
                .Select(kv => kv.Value)
                .FirstOrDefault();

            foreach (var response in responseData.Responses)
            {
                string imagePath = null;
                string videoPath = null;
                string imagePathRaiseTkt = null;  

                // For "Capture Photo of Competitor" question
                if (response.QuestionLabel == "Capture Photo of Competitor")
                {
                    if (pathsForQ11 != null)
                    {
                        imagePath = !string.IsNullOrEmpty(pathsForQ11.Item1)
                            ? $"{_surveyBasePath}{pathsForQ11.Item1}"
                            : null;
                    }
                }
                // For "Capture Video of Competitor Location (30 sec)" question
                else if (response.QuestionLabel == "Capture Video of Competitor Location (30 sec)")
                {
                    if (pathsForQ11 != null)
                    {
                        videoPath = !string.IsNullOrEmpty(pathsForQ11.Item2)
                            ? $"{_surveyBasePath}{pathsForQ11.Item2}"
                            : null;
                    }
                }
                else if (response.QuestionLabel == "Capture Image")
                {
                    if (pathsForQ11 != null)
                    {
                        imagePathRaiseTkt = !string.IsNullOrEmpty(pathsForQ11.Item3)
                            ? $"{_surveyBasePath}{pathsForQ11.Item3}"
                            : null;
                    }
                }

                var questionAnswer = new QuestionAnswer
                {
                    Question = response.QuestionLabel,
                    Value = response.Value,
                    Values = response.Values,
                    Points = response.Points,
                    RelativePath = imagePath,
                    RelativePathVideo = videoPath,
                    RelativePathRaiseTkt = imagePathRaiseTkt,
                    Category = responseData.SectionId,
                    CustomerName = customerName
                };

                questionAnswers.Add(questionAnswer);
            }

            return questionAnswers;
        }

        private List<QuestionAnswer> MapQuestionsAndResponses(
    ResponseData responseData,
    Dictionary<Tuple<string, string>, string> fileSysData)
        {
            var questionAnswers = new List<QuestionAnswer>();

            foreach (var response in responseData.Responses)
            {
                var relativePath = fileSysData
                    .Where(kv => kv.Key.Item2 == response.QuestionId)
                    .Select(kv => kv.Value)
                    .FirstOrDefault();

                string fullPath = !string.IsNullOrEmpty(relativePath)
                    ? $"{_surveyBasePath}{relativePath}"
                    : null;

                questionAnswers.Add(new QuestionAnswer
                {
                    Question = response.QuestionLabel,  // Using QuestionLabel directly from ResponseData
                    Value = response.Value,
                    Values = response.Values,
                    Points = response.Points,
                    RelativePath = fullPath,
                    Category = responseData.SectionId
                });
            }

            return questionAnswers;
        }

        public async Task<List<IViewSurveyResponseExport>> GetViewSurveyResponseForExport(List<FilterCriteria> filterCriterias)
        {
            try
            {
                // Your SQL query to fetch data
                var sql = new StringBuilder(@"select * from (SELECT 
                                TO_CHAR(sr.created_time, 'YYYY-MM-DD') AS CreatedDate,
                                COALESCE('https://switzgroup-live.winitsoftware.com/' || fs.relative_path,'No Image') AS ImagePath,
COALESCE('https://switzgroup-live.winitsoftware.com/' || fss.relative_path ,'No Video') AS RelativePathVideo,
COALESCE('https://switzgroup-live.winitsoftware.com/' || fsss.relative_path,'No Image') AS RelativePathRaiseTkt,
                                CAST(sr.created_time AS TIME(0)) AS CreatedTime,
                                sr.survey_uid AS SurveyName,
                                e.code AS UserCode,
                                e.name AS UserName,
                                st.code AS CustomerCode,
                                st.name AS CustomerName,
                                sr.uid AS SurveyResponseUID,
                                sr.created_time AS CreatedDateTime,  
                                s.code AS Category,
                                s.survey_data AS SurveyData,
                                --sr.status AS Status,
                                sr.response_data AS ResponseData,
                                sr.activity_type as ActivityType,
                                CASE 
                                        WHEN sr.activity_type = 'RaiseTicket' AND sr.status IS NULL THEN 'Open'
                                        ELSE COALESCE(sr.status, '') END AS Status,
                           -- Location info
                                    jp.location_value AS LocationValue,
                                    LR.code AS LocationCode,
                                    LR.name AS LocationName,
                                    R.code as Role
                                FROM survey s
                                JOIN survey_response sr ON sr.survey_uid = s.uid
                                JOIN emp e ON e.uid = sr.emp_uid
                                LEFT JOIN file_sys fs ON LEFT(fs.linked_item_uid, 36) = sr.uid AND fs.linked_item_uid LIKE '%-q11'
    				LEFT JOIN file_sys fss ON LEFT(fss.linked_item_uid, 36) = sr.uid AND fss.linked_item_uid LIKE '%-capture_competitor'
                    LEFT JOIN file_sys fsss ON LEFT(fsss.linked_item_uid, 36) = sr.uid AND fsss.linked_item_uid LIKE '%-imageCapture'
                                Left JOIN store st ON st.uid = sr.linked_item_uid AND sr.linked_item_type = 'Store'
 -- Join to get job and location
 LEFT JOIN job_position jp ON jp.emp_uid = e.uid
 LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
 LEFT JOIN location LR ON LR.uid = LC.parent_uid
 INNER JOIN roles R ON R.uid = jp.user_role_uid
                                ORDER BY sr.created_time DESC, CAST(sr.created_time AS TIME(0)) DESC)as subquery 
                                where 1=1");

                var parameters = new Dictionary<string, object>();

                var ShopObservationTypeFilter = filterCriterias?
                    .FirstOrDefault(f => string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(f.Value?.ToString(), "ShopObservation", StringComparison.OrdinalIgnoreCase));

                var RaiseTicketTypeFilter = filterCriterias?
                    .FirstOrDefault(f => string.Equals(f.Name?.ToString(), "ActivityType", StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(f.Value?.ToString(), "RaiseTicket", StringComparison.OrdinalIgnoreCase));

                if (ShopObservationTypeFilter != null)
                {
                    parameters["ActivityType"] = ShopObservationTypeFilter.Value;
                    filterCriterias.RemoveAll(p => p.Name == "ActivityType");
                }

                if (RaiseTicketTypeFilter != null)
                {
                    parameters["ActivityType"] = RaiseTicketTypeFilter.Value;
                    filterCriterias.RemoveAll(p => p.Name == "ActivityType");
                }

                if ((ShopObservationTypeFilter != null && ShopObservationTypeFilter.Value.ToString() == "ShopObservation") || (RaiseTicketTypeFilter != null
                    && RaiseTicketTypeFilter.Value.ToString() == "RaiseTicket"))

                {
                    sql.Append(" And ActivityType=@ActivityType  ");
                }
                else
                {
                    sql.Append(" And ActivityType NOT IN ('ShopObservation', 'RaiseTicket')   ");
                }
                filterCriterias?.RemoveAll(f => string.Equals(f.Value?.ToString()?.Trim(), "Others", StringComparison.OrdinalIgnoreCase));
                if (filterCriterias.Any())
                {
                    sql.Append(" and ");
                }
                AppendFilterCriteria<IViewSurveyResponseExport>(filterCriterias, sql, parameters);
                var result = await ExecuteQueryAsync<dynamic>(sql.ToString(), parameters);

                if (result == null || !result.Any())
                    throw new Exception("No data found for the given UID");

                var exportDataList = new List<IViewSurveyResponseExport>();
                foreach (var row in result)
                {
                    string ImagePath = row.imagepath;
                    string RelativePathVideo = row.relativepathvideo;
                    string RelativePathRaiseTkt = row.relativepathraisetkt;
                    string surveyDataJson = row.surveydata;
                    string responseDataJson = row.responsedata;

                    var questionAnswers = MapQAndR_ImageExport(surveyDataJson, responseDataJson, ImagePath, RelativePathVideo, RelativePathRaiseTkt);
                    foreach (var questionAnswer in questionAnswers)
                    {
                        var exportData = new ViewSurveyResponseExport
                        {

                            UserCode = row.usercode,
                            UserName = row.username,
                            CreatedDate = row.createddate,
                            CreatedTime = row.createdtime,
                            CustomerCode = row.customercode,
                            CustomerName = row.customername,
                            ActivityType = row.activitytype,
                            Status = row.status,
                            SurveyName = row.surveyname,
                            SurveyResponseUID = row.surveyresponseuid,
                            //SurveyName = row.surveyname,
                            // SurveyName = questionAnswer.ActivityType,
                            Category = questionAnswer.Category,
                            Question = questionAnswer.Question,
                            Value = questionAnswer.Value,
                            Values = questionAnswer.Values,
                            Points = questionAnswer.Points,
                            ImagePath = questionAnswer.ImagePath,
                            RelativePathVideo = questionAnswer.RelativePathVideo,
                            RelativePathRaiseTkt = questionAnswer.RelativePathRaiseTkt
                        };

                        exportDataList.Add(exportData);

                    }
                }

                return exportDataList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching survey export data.", ex);
            }
        }






        public Task<ISurveySection> GetSurveySection(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<ISurveyResponseModel> GetSurveyResponseByUID(string sectionUID, string StoreHistoryUID, DateTime? submmitedDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<ISurveyResponseModel>> GetSurveyResponse(string ActivityType, string LinkedItemUID)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> GetStoreQuestionFrequencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT * FROM
                                                                (WITH RepeatedStores AS (
                                                                SELECT linked_item_uid
                                                                FROM survey_response
                                                                GROUP BY linked_item_uid
                                                                HAVING COUNT(*) > 1
                                                            ),
                                                            ExtractedQuestions AS (
                                                                SELECT 
                                                                    sr.linked_item_uid, sr.response_date,
                                                                    jsonb_array_elements(sr.response_data::jsonb->'responses')->>'question_id' AS question_id,
                                                                    jsonb_array_elements(sr.response_data::jsonb->'responses')->>'question_label' AS question_label,
                                                                    sr.emp_uid, 
                                                                    sr.response_data
                                                                FROM survey_response sr
                                                                JOIN RepeatedStores rs ON rs.linked_item_uid = sr.linked_item_uid
                                        WHERE DATE(sr.response_date) BETWEEN @StartDate AND @EndDate

                                                            )
                                                            SELECT 
                                                                s.name AS customername,
                                                              eq.linked_item_uid AS customercode, 
                                                                e.name AS username,
                                                                e.code AS usercode,
                                                                
                                                              eq.response_date As ResponseDate, 
                                                                eq.question_id as questionid, 
                                                                eq.question_label as questions,
                                                                COUNT(*) AS questioncount,  
-- Location info
        jp.location_value AS LocationValue,
        LR.code AS LocationCode,
        LR.name AS LocationName,R.code as Role
                                                            FROM ExtractedQuestions eq
                                                            JOIN emp e ON e.uid = eq.emp_uid
                                                            JOIN store s ON s.uid = eq.linked_item_uid
  -- Join to get job and location
    LEFT JOIN job_position jp ON jp.emp_uid = e.uid
    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
    LEFT JOIN location LR ON LR.uid = LC.parent_uid
	INNER JOIN roles R ON R.uid = jp.user_role_uid

                                                            GROUP BY 
                                                                eq.linked_item_uid, 
                                                                eq.question_id, 
                                                                eq.question_label,
                                                                eq.response_date,
                                                                s.name, 
                                                               -- s.code, 
                                                                e.name, 
                                                                e.code, 
jp.location_value,
        LR.code,
        LR.name,R.code
                                                                
                                                           -- HAVING COUNT(*) > 5 
                                                            ORDER BY eq.linked_item_uid, questioncount DESC,ResponseDate)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (
                                                          WITH RepeatedStores AS (
                                                            SELECT linked_item_uid
                                                            FROM survey_response
                                                            GROUP BY linked_item_uid
                                                            HAVING COUNT(*) > 1
                                                        ),
                                                        ExtractedQuestions AS (
                                                            SELECT 
                                                                sr.linked_item_uid, sr.response_date, 
                                                                jsonb_array_elements(sr.response_data::jsonb->'responses')->>'question_id' AS question_id,
                                                                jsonb_array_elements(sr.response_data::jsonb->'responses')->>'question_label' AS question_label,
                                                                sr.emp_uid, 
                                                                sr.response_data
                                                            FROM survey_response sr
                                                            JOIN RepeatedStores rs ON rs.linked_item_uid = sr.linked_item_uid
                                    WHERE DATE(sr.response_date) BETWEEN @StartDate AND @EndDate

                                                        )
                                                        SELECT 
                                                            s.name AS customername,
                                                          eq.linked_item_uid AS customercode, 
                                                            e.name AS username,
                                                            e.code AS usercode,
                                                                
                                                              eq.response_date As ResponseDate,
                                                            eq.question_id as questionid, 
                                                            eq.question_label as questions,
                                                            COUNT(*) AS questioncount,
-- Location info
        jp.location_value AS LocationValue,
        LR.code AS LocationCode,
        LR.name AS LocationName,R.code as Role
                                  
                                                        FROM ExtractedQuestions eq
                                                        JOIN emp e ON e.uid = eq.emp_uid
                                                        JOIN store s ON s.uid = eq.linked_item_uid
  -- Join to get job and location
    LEFT JOIN job_position jp ON jp.emp_uid = e.uid
    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
    LEFT JOIN location LR ON LR.uid = LC.parent_uid
	INNER JOIN roles R ON R.uid = jp.user_role_uid

                                                        GROUP BY 
                                                            eq.linked_item_uid, 
                                                            eq.question_id, 
                                                            eq.question_label,
                                                                eq.response_date, 
                                                            s.name, 
                                                           -- s.code, 
                                                            e.name, 
                                                            e.code,
jp.location_value,
        LR.code,
        LR.name,R.code
                                                            
                                                       -- HAVING COUNT(*) > 5 
                                                        ORDER BY eq.linked_item_uid, questioncount DESC,ResponseDate)as subquery");
                }
                var parameters = new Dictionary<string, object>
{
    { "StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) },
    { "EndDate", DateTime.Now }
};


                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>(filterCriterias, sbFilterCriteria, parameters);

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
                        sql.Append($" ORDER BY usercode OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Model.Interfaces.IStoreQuestionFrequency> storeQuestionFrequencyList = await ExecuteQueryAsync<Model.Interfaces.IStoreQuestionFrequency>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>
                {
                    PagedData = storeQuestionFrequencyList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSurveyResponseList()
        {
            List<SurveyResponseModel> surveyResponseModels = new List<SurveyResponseModel>(); // Initialize list
            List<SurveyResponseModel> surveyResponseModelsUnique = new List<SurveyResponseModel>(); // Initialize list
            List<string> existingsurveyuids = null;
            try
            {
                var surveyreponsequery = @"select uid from survey_response";
                existingsurveyuids = await ExecuteQueryAsync<string>(surveyreponsequery);
                var apprequestquery = @"select * from app_request WHERE CAST(request_created_time AS DATE) = '2025-03-03'";
                var apprequestList = await ExecuteQueryAsync<Winit.Modules.Syncing.Model.Interfaces.IAppRequest>(apprequestquery);

                foreach (var appRequest in apprequestList)
                {
                    // Deserialize each request body and add to the list
                    var surveyResponse = JsonConvert.DeserializeObject<Winit.Modules.Survey.Model.Classes.SurveyResponseModel>(appRequest.RequestBody);
                    surveyResponseModels.Add(surveyResponse);
                }
                surveyResponseModelsUnique = surveyResponseModels
    .Where(srm => !existingsurveyuids.Contains(srm.UID)) // Keep only new UIDs
    .ToList();




                var sql = @"INSERT INTO survey_response ( created_time, modified_time, server_add_time, server_modified_time, 
                     ss, response_date, response_data, beat_history_uid, survey_uid, org_uid, linked_item_type, linked_item_uid, uid, created_by, 
                     job_position_uid, modified_by, emp_uid, route_uid, store_history_uid, activity_type) 
                     VALUES (@CreatedTime, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, 0, @ResponseDate, @ResponseData::jsonb, @BeatHistoryUID, 
                             @SurveyUID, @OrgUID, @LinkedItemType, @LinkedItemUID, @UID, @CreatedBy, @JobPositionUid, @ModifiedBy, @EmpUID, @RouteUID, 
                             @StoreHistoryUID, @ActivityType)";

                // Assuming ExecuteNonQueryAsync can handle a list of parameters for the bulk insert
                return await ExecuteNonQueryAsync(sql, surveyResponseModelsUnique); // Pass the populated list
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Survey Response", ex);
            }
        }

    }
}
