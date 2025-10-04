using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Survey.DL.Classes
{
    public class PGSQLActivityModuleDL:Winit.Modules.Base.DL.DBManager.PostgresDBManager,IActivityModuleDL
    {
        private readonly string _surveyBasePath;

        public PGSQLActivityModuleDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
            _surveyBasePath = config["SurveyBasePath"];
        }
        public async Task<PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule>> GetAllActivityModuleDeatils(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"Select * from(SELECT 
                            sr.created_time AS Date,
                            s.code AS outletcode,
                            s.name AS outletname,
                            e.code AS usercode,
                            e.name AS username,
                            fs.relative_path as relativepath,
                            fs.linked_item_uid,
                            responseData->>'question_id' AS QuestionID,
                            responseData->>'question_label' AS QuestionLabel,
                            responseData->>'value' AS Answer,
                            -- Location info
                                    jp.location_value AS LocationValue,
                                    LR.code AS LocationCode,
                                    LR.name AS LocationName,R.code as Role
                        FROM survey_response sr
                        JOIN emp e ON e.uid = sr.emp_uid
                        JOIN store s ON s.uid = sr.linked_item_uid
                                    -- Join to get job and location
                                    LEFT JOIN job_position jp ON jp.emp_uid = e.uid
                                    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
                                    LEFT JOIN location LR ON LR.uid = LC.parent_uid
	                                INNER JOIN roles R ON R.uid = jp.user_role_uid

                        CROSS JOIN LATERAL jsonb_array_elements(sr.response_data::jsonb->'responses') AS responseData
                        LEFT JOIN file_sys fs 
                            ON LEFT(fs.linked_item_uid, 36) = sr.uid  
                            AND RIGHT(fs.linked_item_uid, LENGTH(fs.linked_item_uid) - 37) = responseData->>'question_id'  
                        WHERE sr.linked_item_type = 'Store'
                        AND sr.activity_type = 'ShopObservation')as subquery");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                            sr.created_time AS Date,
                            s.code AS outletcode,
                            s.name AS outletname,
                            e.code AS usercode,
                            e.name AS username,
                            fs.relative_path as relativepath,
                            fs.linked_item_uid,
                            responseData->>'question_id' AS QuestionID,
                            responseData->>'question_label' AS QuestionLabel,
                            responseData->>'value' AS Answer,
                            -- Location info
                                    jp.location_value AS LocationValue,
                                    LR.code AS LocationCode,
                                    LR.name AS LocationName,R.code as Role
                        FROM survey_response sr
                        JOIN emp e ON e.uid = sr.emp_uid
                        JOIN store s ON s.uid = sr.linked_item_uid
                         -- Join to get job and location
                                    LEFT JOIN job_position jp ON jp.emp_uid = e.uid
                                    LEFT JOIN location LC ON LC.location_type_uid = 'City' AND LC.code = jp.location_value
                                    LEFT JOIN location LR ON LR.uid = LC.parent_uid
	                                INNER JOIN roles R ON R.uid = jp.user_role_uid

                        CROSS JOIN LATERAL jsonb_array_elements(sr.response_data::jsonb->'responses') AS responseData
                        LEFT JOIN file_sys fs 
                            ON LEFT(fs.linked_item_uid, 36) = sr.uid  
                            AND RIGHT(fs.linked_item_uid, LENGTH(fs.linked_item_uid) - 37) = responseData->>'question_id'  
                        WHERE sr.linked_item_type = 'Store'
                        AND sr.activity_type = 'ShopObservation')as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.Survey.Model.Interfaces.IActivityModule>(filterCriterias, sbFilterCriteria, parameters);

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
                        sql.Append($" ORDER BY outletcode OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                    }
                }

                IEnumerable<Model.Interfaces.IActivityModule> activityModuleList = await ExecuteQueryAsync<Model.Interfaces.IActivityModule>(sql.ToString(), parameters);
                string fullpath = string.Empty;
                foreach (var item in activityModuleList)
                {
                    if(item!=null && item.Relativepath != null)
                    {
                        fullpath = _surveyBasePath + item.Relativepath;
                        item.Relativepath = fullpath;
                    }
                }
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule> pagedResponse = new PagedResponse<Winit.Modules.Survey.Model.Interfaces.IActivityModule>
                {
                    PagedData = activityModuleList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
