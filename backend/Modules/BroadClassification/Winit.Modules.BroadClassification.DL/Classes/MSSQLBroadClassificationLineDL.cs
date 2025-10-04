using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.BroadClassification.DL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.DL.Classes
{
    public class MSSQLBroadClassificationLineDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, Winit.Modules.BroadClassification.DL.Interfaces.IBroadClassificationLineDL
    {
        public MSSQLBroadClassificationLineDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
              int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                StringBuilder sql = new(@"select * from(SELECT 
                                        id AS Id,
                                        uid AS UID,
                                        ss AS SS,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        broad_classification_header_uid AS BroadClassificationHeaderUID,
                                        line_number AS LineNumber,
                                        classification_code AS ClassificationCode
                                    FROM 
                                        broad_classification_line 
                                    )as sub_query");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                                        id AS Id,
                                        uid AS UID,
                                        ss AS SS,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        broad_classification_header_uid AS BroadClassificationHeaderUID,
                                        line_number AS LineNumber,
                                        classification_code AS ClassificationCode
                                    FROM 
                                        broad_classification_line  )as sub_query");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine> broadClassificationLines = await ExecuteQueryAsync<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine> pagedResponse = new PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>
                {
                    PagedData = broadClassificationLines,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>> GetBroadClassificationLineDetailsByUID(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                             var sql = @"SELECT 
                                        id AS Id,
                                        uid AS UID,
                                        ss AS SS,
                                        created_by AS CreatedBy,
                                        created_time AS CreatedTime,
                                        modified_by AS ModifiedBy,
                                        modified_time AS ModifiedTime,
                                        server_add_time AS ServerAddTime,
                                        server_modified_time AS ServerModifiedTime,
                                        broad_classification_header_uid AS BroadClassificationHeaderUID,
                                        line_number AS LineNumber,
                                        classification_code AS ClassificationCode
                                    FROM 
                                        broad_classification_line
                                     WHERE broad_classification_header_uid = @UID";
                return await ExecuteQueryAsync<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine>(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> CreateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationLine)
        {
            try
            {
                var sql = @"INSERT INTO broad_classification_line (uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, broad_classification_header_uid, line_number, classification_code)
                            VALUES 
                            (@UID, @SS, @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@BroadClassificationHeaderUID,
                            @LineNumber, @ClassificationCode);";
                return await ExecuteNonQueryAsync(sql, broadClassificationLine);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateBroadClassificationLine(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationLine broadClassificationLine)
        {
            try
            {
                var sql = @"UPDATE broad_classification_line SET  modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime,
                            line_number = @LineNumber,classification_code=@ClassificationCode;";
                return await ExecuteNonQueryAsync(sql, broadClassificationLine);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBroadClassificationLine(String UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = "DELETE  FROM broad_classification_line where uid = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
            
        }
    }


}
