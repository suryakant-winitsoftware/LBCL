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


namespace Winit.Modules.Survey.DL.Classes
{
    public class PGSQLSurveyDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISurveyDL
    {
        public PGSQLSurveyDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey>> GetAllSurveyDeatils(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from (SELECT s.id AS Id, s.uid AS UID, s.created_by AS CreatedBy,
                                            s.created_time AS CreatedTime, s.modified_by AS ModifiedBy, s.modified_time AS ModifiedTime,
                                            s.server_add_time AS ServerAddTime, s.server_modified_time AS ServerModifiedTime, s.ss AS SS, s.code AS Code, 
                                            s.description AS Description, s.start_date AS StartDate, s.end_date AS EndDate, s.is_active::bool AS IsActive, 
                                            s.survey_data AS SurveyData FROM survey s)as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT s.id AS Id, s.uid AS UID, s.created_by AS CreatedBy,
                                            s.created_time AS CreatedTime, s.modified_by AS ModifiedBy, s.modified_time AS ModifiedTime,
                                            s.server_add_time AS ServerAddTime, s.server_modified_time AS ServerModifiedTime, s.ss AS SS, s.code AS Code, 
                                            s.description AS Description, s.start_date AS StartDate, s.end_date AS EndDate, s.is_active::bool AS IsActive, 
                                            s.survey_data AS SurveyData FROM survey s)as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.ISurvey>(filterCriterias, sbFilterCriteria, parameters);

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

                IEnumerable<Model.Interfaces.ISurvey> surveyList = await ExecuteQueryAsync<Model.Interfaces.ISurvey>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.ISurvey>
                {
                    PagedData = surveyList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            try
            {
                var sql = @"INSERT INTO survey (uid, created_by, created_time, modified_by, modified_time, 
                            server_add_time, server_modified_time, ss, code, description, start_date, end_date, is_active, survey_data) 
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS, 
                            @Code, @Description, @StartDate, @EndDate, @IsActive, @SurveyData::json)";
                return await ExecuteNonQueryAsync(sql, survey);
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Survey", ex);
            }
        }

        public async Task<int> CUDSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            int retval = -1;
            try
            {
                var existing = await GetSurveyByUID(survey.UID);
                if(existing!=null)
                {
                    retval=await UpdateSurvey(survey);
                }
                else
                {
                    retval= await CreateSurvey(survey);
                }
               
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating or updating Survey", ex);
            }
            return retval;
        }
        public async Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByUID(string UID)
        {
            try
            {
                var sql = @"SELECT s.id AS Id, s.uid AS UID, s.created_by AS CreatedBy,
                                            s.created_time AS CreatedTime, s.modified_by AS ModifiedBy, s.modified_time AS ModifiedTime,
                                            s.server_add_time AS ServerAddTime, s.server_modified_time AS ServerModifiedTime, s.ss AS SS, s.code AS Code, 
                                            s.description AS Description, s.start_date AS StartDate, s.end_date AS EndDate, s.is_active AS IsActive, 
                                            s.survey_data AS SurveyData FROM survey s WHERE s.uid = @UID";
                var parameters = new Dictionary<string, object> { { "UID", UID } };
                return await ExecuteSingleAsync<ISurvey>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.Survey.Model.Interfaces.ISurvey> GetSurveyByCode(string code)
        {
            try
            {
                var sql = @"SELECT s.id AS Id, s.uid AS UID, s.created_by AS CreatedBy,
                                            s.created_time AS CreatedTime, s.modified_by AS ModifiedBy, s.modified_time AS ModifiedTime,
                                            s.server_add_time AS ServerAddTime, s.server_modified_time AS ServerModifiedTime, s.ss AS SS, s.code AS Code, 
                                            s.description AS Description, s.start_date AS StartDate, s.end_date AS EndDate, s.is_active AS IsActive, 
                                            s.survey_data AS SurveyData FROM survey s WHERE s.Code = @Code";
                var parameters = new Dictionary<string, object> { { "Code", code } };
                return await ExecuteSingleAsync<ISurvey>(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSurvey(Winit.Modules.Survey.Model.Interfaces.ISurvey survey)
        {
            try
            {
                var sql = @"UPDATE survey SET   modified_by = @ModifiedBy, modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime, ss = @SS, code = @Code, description = @Description, 
                            start_date = @StartDate, end_date = @EndDate, is_active = @IsActive, survey_data = @SurveyData::json WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, survey);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSurvey(string uID)
        {
            try
            {
                var parametes = new Dictionary<string, object>()
                {
                    {
                        "UID",uID
                    }
                };
                var sql = @"DELETE FROM Survey WHERE UID=@UID";
                return await ExecuteNonQueryAsync(sql, parametes);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
