using System.Text;
using System.Text.Json;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Modules.Survey.Model.Classes;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Classes
{
    public class SQLiteSurveyResponseDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISurveyResponseDL
    {
        public SQLiteSurveyResponseDL(IServiceProvider serviceProvider) : base(serviceProvider)
        {
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

                IEnumerable<ISurveyResponseModel> surveyResponseModelList = await ExecuteQueryAsync<ISurveyResponseModel>(sql.ToString(), parameters);
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
                var sql = @"INSERT INTO survey_response (id, created_time, modified_time, server_add_time, server_modified_time, 
                            ss, response_date,status, response_data, beat_history_uid, survey_uid, org_uid, linked_item_type, linked_item_uid, uid, created_by, 
                            job_position_uid, modified_by, emp_uid, route_uid, store_history_uid,activity_type) VALUES (@Id, @CreatedTime, @ModifiedTime, @ServerAddTime,
                            @ServerModifiedTime, @SS, @ResponseDate,@Status, @ResponseData, @BeatHistoryUID, @SurveyUID, @OrgUID, @LinkedItemType, @LinkedItemUID,
                            @UID, @CreatedBy, @JobPositionUid, @ModifiedBy, @EmpUID, @RouteUID, @StoreHistoryUID, @ActivityType)";
                return await ExecuteNonQueryAsync(sql, surveyResponseModel);
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
                             modified_by = @ModifiedBy ,remarks = @Remarks
                             WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, surveyResponseModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Capture Competitor", ex);
            }
        }

        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>> GetViewSurveyResponse(List<SortCriteria> sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                                                                        DATE(sr.created_time) AS CreatedDate,
                                                                        CAST(sr.created_time AS time(0)) AS CreatedTime,
                                                                        e.code AS UserCode,
                                                                        e.name AS UserName,
                                                                        s.code AS CustomerCode,
                                                                        s.name AS CustomerName,
                                                                        sr.uid AS SurveyResponseUID
                                                                        sr.status As Status
                                                                    FROM 
                                                                        survey_response sr 
                                                                    JOIN 
                                                                        emp e ON e.uid = sr.emp_uid
                                                                    JOIN 
                                                                        store s ON s.uid = sr.linked_item_uid AND sr.linked_item_type = 'Store'
                                                                    )as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (
                                                                        SELECT 
                                                                        DATE(sr.created_time) AS CreatedDate,
                                                                        CAST(sr.created_time AS time(0)) AS CreatedTime,
                                                                        e.code AS UserCode,
                                                                        e.name AS UserName,
                                                                        s.code AS CustomerCode,
                                                                        s.name AS CustomerName,
                                                                        sr.uid AS SurveyResponseUID,
                                                                        sr.status As Status
                                                                    FROM 
                                                                        survey_response sr 
                                                                    JOIN 
                                                                        emp e ON e.uid = sr.emp_uid
                                                                    JOIN 
                                                                        store s ON s.uid = sr.linked_item_uid AND sr.linked_item_type = 'Store')as subquery");
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

                IEnumerable<IViewSurveyResponse> viewSurveyResponseList = await ExecuteQueryAsync<IViewSurveyResponse>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IViewSurveyResponse>
                {
                    PagedData = viewSurveyResponseList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<ISurveyResponseViewDTO> ViewSurveyResponseByUID(string UID)
        {
            try
            {
                var sql = @"SELECT 
                        s.code AS Category, 
                        s.survey_data AS SurveyData, 
                        sr.response_data AS ResponseData
                        FROM 
                        survey s
                        JOIN 
                        survey_response sr 
                        ON 
                        sr.survey_uid = s.uid 
                        WHERE 
                        sr.uid = @UID";

                var parameters = new Dictionary<string, object> { { "UID", UID } };
                var result = await ExecuteSingleAsync<dynamic>(sql, parameters);
                if (result == null)
                    throw new Exception("No data found for the given UID");
                string surveyDataJson = result.surveydata;
                string responseDataJson = result.responsedata;

                var questionAnswers = MapQuestionsAndResponses(surveyDataJson, responseDataJson);

                return new SurveyResponseViewDTO
                {
                    // Category = result.category,
                    QuestionAnswers = questionAnswers
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching SurveyResponse UID", ex);
            }
        }
        private List<QuestionAnswer> MapQuestionsAndResponses(string surveyDataJson, string responseDataJson)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var surveyData = JsonSerializer.Deserialize<SurveyData>(surveyDataJson, options);
            var responseData = JsonSerializer.Deserialize<ResponseData>(responseDataJson, options);
            var questionAnswers = new List<QuestionAnswer>();
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
                        });
                    }
                }
            }

            return questionAnswers;
        }

        public async Task<ISurveySection> GetSurveySection(string UID)
        {
            try
            {
                var sql = @"SELECT * FROM survey WHERE uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", UID } };
                ISurveySection surveySection = await ExecuteSingleAsync<ISurveySection>(sql, parameters);
                return surveySection;
            }
            catch (Exception ex)
            {
                // Handle exception (log or rethrow)
                throw new Exception("Error fetching Capture Competitor by UID", ex);
            }
        }

        public async Task<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel> GetSurveyResponseByUID(string sectionUID, string StoreHistoryUID, DateTime? submmitedDate = null)
        {
            try
            {
                string sql;
                var parameters = new Dictionary<string, object>
                    {
                        { "UID", sectionUID },
                        { "StoreHistoryUID", StoreHistoryUID }
                    };

                if (submmitedDate.HasValue)
                {
                    // If SubmmitedDate is provided, include it in the SQL query to filter responses by that date
                    sql = @"SELECT sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                        sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, sr.ss AS SS, 
                        sr.response_date AS ResponseDate, sr.response_data AS ResponseData, sr.status AS Status, 
                        sr.beat_history_uid AS BeatHistoryUID, sr.survey_uid AS SurveyUID, sr.org_uid AS OrgUID, 
                        sr.linked_item_type AS LinkedItemType, sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, 
                        sr.created_by AS CreatedBy, sr.job_position_uid AS JobPositionUid, sr.modified_by AS ModifiedBy, 
                        sr.emp_uid AS EmpUID, sr.route_uid AS RouteUID, sr.store_history_uid AS StoreHistoryUID 
                    FROM survey_response sr 
                    WHERE sr.survey_uid = @UID 
                    AND sr.store_history_uid = @StoreHistoryUID 
                    AND sr.response_date = @SubmmitedDate";
                    parameters.Add("SubmmitedDate", submmitedDate.Value);
                }
                else
                {
                    // If SubmmitedDate is not provided, use the original SQL query without filtering by date
                    sql = @"SELECT sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                        sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, sr.ss AS SS, 
                        sr.response_date AS ResponseDate, sr.response_data AS ResponseData, sr.status AS Status, 
                        sr.beat_history_uid AS BeatHistoryUID, sr.survey_uid AS SurveyUID, sr.org_uid AS OrgUID, 
                        sr.linked_item_type AS LinkedItemType, sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, 
                        sr.created_by AS CreatedBy, sr.job_position_uid AS JobPositionUid, sr.modified_by AS ModifiedBy, 
                        sr.emp_uid AS EmpUID, sr.route_uid AS RouteUID, sr.store_history_uid AS StoreHistoryUID 
                    FROM survey_response sr 
                    WHERE sr.survey_uid = @UID 
                    AND sr.store_history_uid = @StoreHistoryUID ORDER BY sr.response_date DESC";
                }

                // Execute the SQL query
                return await ExecuteSingleAsync<ISurveyResponseModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching SurveyResponse UID", ex);
            }
        }


        public async Task<List<Winit.Modules.Survey.Model.Interfaces.ISurveyResponseModel>> GetSurveyResponse(string ActivityType, string LinkedItemUID)
        {
            try
            {
                var sql = @"
                            SELECT 
                                sr.id AS Id, sr.created_time AS CreatedTime, sr.modified_time AS ModifiedTime, 
                                sr.server_add_time AS ServerAddTime, sr.server_modified_time AS ServerModifiedTime, 
                                sr.ss AS SS, sr.response_date AS ResponseDate, sr.response_data AS ResponseData, 
                                sr.status AS Status, sr.beat_history_uid AS BeatHistoryUID, 
                                sr.survey_uid AS SurveyUID, sr.activity_type AS ActivityType, 
                                sr.org_uid AS OrgUID, sr.linked_item_type AS LinkedItemType, 
                                sr.linked_item_uid AS LinkedItemUID, sr.uid AS UID, 
                                sr.created_by AS CreatedBy, sr.job_position_uid AS JobPositionUid, 
                                sr.modified_by AS ModifiedBy, sr.emp_uid AS EmpUID, 
                                sr.route_uid AS RouteUID, sr.store_history_uid AS StoreHistoryUID 
                            FROM survey_response sr 
                            WHERE  
                                (@LinkedItemUID IS NULL OR sr.linked_item_uid = @LinkedItemUID) 
                                AND 
                                (
                                    (@ActivityType IS NOT NULL AND @ActivityType <> '' AND sr.activity_type = @ActivityType)
                                    OR 
                                    (@ActivityType IS NULL OR @ActivityType = '') AND sr.activity_type <> 'RaiseTicket'
                                )  
                        ";


                var parameters = new Dictionary<string, object>
                    {
                        { "ActivityType", ActivityType },
                        { "LinkedItemUID", LinkedItemUID }
                    };

                return await ExecuteQueryAsync<ISurveyResponseModel>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching SurveyResponse UID", ex);
            }
        }

        public async Task<List<IViewSurveyResponseExport>> GetViewSurveyResponseForExport(List<FilterCriteria> filterCriterias)
        {
            throw new NotImplementedException();
        }
        public async Task<int> TicketStatusUpdate(string uid, string status, string empUID)
        {
            throw new NotImplementedException();
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IStoreQuestionFrequency>> GetStoreQuestionFrequencyDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }
        public async Task<int> CreateSurveyResponseList()
        {
            throw new NotImplementedException();
        }
    }
}
