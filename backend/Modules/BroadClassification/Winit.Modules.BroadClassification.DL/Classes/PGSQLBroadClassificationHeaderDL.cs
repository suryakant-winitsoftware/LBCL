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
    public class PGSQLBroadClassificationHeaderDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IBroadClassificationHeaderDL
    {
        public PGSQLBroadClassificationHeaderDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>> GetBroadClassificationHeaderDetails(List<SortCriteria> sortCriterias, int pageNumber,
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
                                        name AS Name,
                                        classification_count AS ClassificationCount,
                                        is_active AS IsActive
                                    FROM 
                                        broad_classification_header 
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
                                        name AS Name,
                                        classification_count AS ClassificationCount,
                                        is_active AS IsActive
                                    FROM 
                                        broad_classification_header  )as sub_query");
                }
                var parameters = new Dictionary<string, object>();
                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>(filterCriterias, sbFilterCriteria, parameters);
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
                IEnumerable<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> broadClassificationHeaders = await ExecuteQueryAsync<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>(sql.ToString(), parameters);
                int totalCount = -1;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> pagedResponse = new PagedResponse<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>
                {
                    PagedData = broadClassificationHeaders,
                    TotalCount = totalCount
                };
                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader> GetBroadClassificationHeaderDetailsByUID(string UID)
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
                                        name AS Name,
                                        classification_count AS ClassificationCount,
                                        is_active AS IsActive
                                    FROM 
                                        broad_classification_header 
                                     WHERE uid = @UID";
                return await ExecuteSingleAsync<Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader>(sql, parameters);
            }
            catch
            {
                throw;
            }
           
        }
        public async Task<int> CreateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader)
        {
           
            try
            {
                var sql = @"INSERT INTO broad_classification_header (uid, ss, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, name, classification_count, is_active)
                            VALUES 
                            (UID, SS, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime, ServerAddTime, ServerModifiedTime, Name, ClassificationCount, IsActive);";
                return await ExecuteNonQueryAsync(sql, broadClassificationHeader);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateBroadClassificationHeader(Winit.Modules.BroadClassification.Model.Interfaces.IBroadClassificationHeader broadClassificationHeader)
        {
            try
            {
                var sql = @"UPDATE broad_classification_header SET  modified_time = @ModifiedTime, 
                            server_modified_time = @ServerModifiedTime,
                            name = @Name,classification_count=@ClassificationCount,is_active=@IsActive WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, broadClassificationHeader);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBroadClassificationHeader(String UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID",  UID}
            };
                var sql = "DELETE  FROM broad_classification_header uid = @UID";

                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
            
        }
    }


}
