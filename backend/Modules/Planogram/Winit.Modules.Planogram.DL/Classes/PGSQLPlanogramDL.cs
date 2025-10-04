using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Winit.Modules.Planogram.DL.Classes
{
    public class PGSQLPlanogramDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, IPlanogramDL
    {
        public PGSQLPlanogramDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }

        #region Existing Methods Implementation

        public async Task<List<IPlanogramCategory>> GetPlanogramCategoriesAsync()
        {
            try
            {
                var sql = @"
                SELECT 
                    SG.code AS CategoryCode,
                    SG.name AS CategoryName,
                    COUNT(PS.uid) AS SetupCount,
                    FSC.relative_path || '/' || FSC.file_name AS CategoryImage
                FROM planogram_setup PS
                INNER JOIN sku_group SG ON SG.code = PS.category_code
                INNER JOIN sku_group_type SGT ON SGT.uid = SG.sku_group_type_uid AND SGT.code = 'Category'
                LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKUGroup' AND FSC.linked_item_uid = SG.uid
                WHERE PS.ss = 0 OR PS.ss IS NULL
                GROUP BY SG.code, SG.name, FSC.relative_path, FSC.file_name
                ORDER BY SG.code";

                return await ExecuteQueryAsync<IPlanogramCategory>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching planogram categories", ex);
            }
        }

        public async Task<List<IPlanogramRecommendation>> GetPlanogramRecommendationsByCategoryAsync(string categoryCode)
        {
            try
            {
                var sql = @"
                SELECT 
                    PS.uid AS UID,
                    PS.category_code AS CategoryCode,
                    PS.share_of_shelf_cm AS ShareOfShelfCm,
                    PS.selection_type AS SelectionType,
                    PS.selection_value AS SelectionValue,
                    FSP.relative_path || '/' || FSP.file_name AS RecommendedImagePath
                FROM planogram_setup PS
                LEFT JOIN file_sys FSP ON FSP.linked_item_type = 'Planogram' AND FSP.linked_item_uid = PS.uid
                WHERE PS.category_code = @CategoryCode 
                  AND (PS.ss = 0 OR PS.ss IS NULL)
                ORDER BY PS.created_time DESC";

                var parameters = new { CategoryCode = categoryCode };
                return await ExecuteQueryAsync<IPlanogramRecommendation>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching planogram recommendations for category: {categoryCode}", ex);
            }
        }

        public async Task<IPlanogramSetup> GetPlanogramSetupByUIDAsync(string uid)
        {
            try
            {
                var sql = @"
                SELECT 
                    id,
                    uid,
                    category_code,
                    share_of_shelf_cm,
                    selection_type,
                    selection_value,
                    created_by,
                    created_time,
                    modified_by,
                    modified_time,
                    server_add_time,
                    server_modified_time,
                    ss
                FROM planogram_setup 
                WHERE uid = @UID 
                  AND (ss = 0 OR ss IS NULL)";

                var parameters = new { UID = uid };
                var results = await ExecuteQueryAsync<IPlanogramSetup>(sql, parameters);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching planogram setup by UID: {uid}", ex);
            }
        }

        public async Task<string> CreatePlanogramExecutionHeaderAsync(IPlanogramExecutionHeader header)
        {
            try
            {
                var sql = @"
                INSERT INTO planogram_execution_header (
                    uid, store_uid, beat_history_uid, store_history_uid, 
                    job_position_uid, route_uid, status,
                    created_by, created_time, server_add_time
                ) VALUES (
                    @UID, @StoreUID, @BeatHistoryUID, @StoreHistoryUID,
                    @JobPositionUID, @RouteUID, @Status,
                    @CreatedBy, @CreatedTime, @ServerAddTime
                )";

                var parameters = new
                {
                    UID = header.UID,
                    StoreUID = header.StoreUID,
                    BeatHistoryUID = header.BeatHistoryUID,
                    StoreHistoryUID = header.StoreHistoryUID,
                    JobPositionUID = header.JobPositionUID,
                    RouteUID = header.RouteUID,
                    Status = header.Status,
                    CreatedBy = header.CreatedBy,
                    CreatedTime = header.CreatedTime,
                    ServerAddTime = header.ServerAddTime
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return header.UID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating planogram execution header", ex);
            }
        }

        public async Task<string> CreatePlanogramExecutionDetailAsync(IPlanogramExecutionDetail detail)
        {
            try
            {
                var sql = @"
                INSERT INTO planogram_execution_detail (
                    uid, planogram_execution_header_uid, planogram_setup_uid,
                    executed_on, is_completed, is_planogram_as_per_plan,
                    created_by, created_time, server_add_time
                ) VALUES (
                    @UID, @HeaderUID, @SetupUID,
                    @ExecutedOn, @IsCompleted, @IsPlanogramAsPerPlan,
                    @CreatedBy, @CreatedTime, @ServerAddTime
                )";

                var parameters = new
                {
                    UID = detail.UID,
                    HeaderUID = detail.PlanogramExecutionHeaderUID,
                    SetupUID = detail.PlanogramSetupUID,
                    ExecutedOn = detail.ExecutedOn,
                    IsCompleted = detail.IsCompleted,
                    IsPlanogramAsPerPlan = detail.IsPlanogramAsPerPlan,
                    CreatedBy = detail.CreatedBy,
                    CreatedTime = detail.CreatedTime,
                    ServerAddTime = detail.ServerAddTime
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return detail.UID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating planogram execution detail", ex);
            }
        }

        public async Task<bool> UpdatePlanogramExecutionDetailStatusAsync(string uid, bool isCompleted)
        {
            try
            {
                var sql = @"
                UPDATE planogram_execution_detail 
                SET is_compliant = @IsCompleted, 
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UID", uid),
                    new NpgsqlParameter("@IsCompleted", isCompleted),
                    new NpgsqlParameter("@ModifiedTime", DateTime.Now),
                    new NpgsqlParameter("@ServerModifiedTime", DateTime.Now)
                };

                var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating planogram execution detail status for UID: {uid}", ex);
            }
        }

        public async Task<List<IPlanogramExecutionDetail>> GetPlanogramExecutionDetailsByHeaderUIDAsync(string headerUID)
        {
            try
            {
                var sql = @"
                SELECT 
                    ped.*,
                    ps.category_code,
                    ps.selection_type,
                    ps.selection_value
                FROM planogram_execution_detail ped
                INNER JOIN planogram_setup ps ON ps.uid = ped.planogram_setup_uid
                WHERE ped.planogram_execution_header_uid = @HeaderUID
                ORDER BY ped.created_time";

                var parameters = new { HeaderUID = headerUID };
                return await ExecuteQueryAsync<IPlanogramExecutionDetail>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching planogram execution details for header: {headerUID}", ex);
            }
        }

        #endregion

        #region New CRUD Methods for PlanogramSetup

        public async Task<List<IPlanogramSetup>> GetAllPlanogramSetupsAsync(int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var offset = (pageNumber - 1) * pageSize;
                var sql = @"
                SELECT 
                    id, uid, category_code, share_of_shelf_cm,
                    selection_type, selection_value,
                    created_by, created_time, modified_by, modified_time,
                    server_add_time, server_modified_time, ss
                FROM planogram_setup 
                WHERE ss = 0 OR ss IS NULL
                ORDER BY category_code, created_time DESC
                LIMIT @PageSize OFFSET @Offset";

                var parameters = new Dictionary<string, object>
                {
                    { "PageSize", pageSize },
                    { "Offset", offset }
                };

                var setups = await ExecuteQueryAsync<IPlanogramSetup>(sql, parameters);

                return setups;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all planogram setups", ex);
            }
        }

        public async Task<List<IPlanogramSetup>> GetPlanogramSetupsByCategoryAsync(string categoryCode)
        {
            try
            {
                var sql = @"
                SELECT 
                    id, uid, category_code, share_of_shelf_cm,
                    selection_type, selection_value,
                    created_by, created_time, modified_by, modified_time,
                    server_add_time, server_modified_time, ss
                FROM planogram_setup 
                WHERE category_code = @CategoryCode
                  AND (ss = 0 OR ss IS NULL)
                ORDER BY created_time DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "CategoryCode", categoryCode }
                };

                var setups = await ExecuteQueryAsync<IPlanogramSetup>(sql, parameters);

                return setups;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching planogram setups for category: {categoryCode}", ex);
            }
        }

        public async Task<string> CreatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            try
            {
                var sql = @"
                INSERT INTO planogram_setup (
                    uid, category_code, share_of_shelf_cm,
                    selection_type, selection_value,
                    created_by, created_time, modified_by, modified_time,
                    server_add_time, server_modified_time, ss
                ) VALUES (
                    @UID, @CategoryCode, @ShareOfShelfCm,
                    @SelectionType, @SelectionValue,
                    @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime,
                    @ServerAddTime, @ServerModifiedTime, 0
                )";

                var parameters = new
                {
                    UID = planogramSetup.UID,
                    CategoryCode = planogramSetup.CategoryCode,
                    ShareOfShelfCm = planogramSetup.ShareOfShelfCm ?? 0,
                    SelectionType = planogramSetup.SelectionType ?? "Category",
                    SelectionValue = planogramSetup.SelectionValue ?? "",
                    CreatedBy = planogramSetup.CreatedBy,
                    CreatedTime = planogramSetup.CreatedTime,
                    ModifiedBy = planogramSetup.ModifiedBy ?? planogramSetup.CreatedBy,
                    ModifiedTime = planogramSetup.ModifiedTime ?? planogramSetup.CreatedTime,
                    ServerAddTime = planogramSetup.ServerAddTime,
                    ServerModifiedTime = planogramSetup.ServerModifiedTime ?? planogramSetup.ServerAddTime
                };

                await ExecuteNonQueryAsync(sql, parameters);
                return planogramSetup.UID;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating planogram setup", ex);
            }
        }

        public async Task<bool> UpdatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
        {
            try
            {
                var sql = @"
                UPDATE planogram_setup 
                SET category_code = @CategoryCode,
                    share_of_shelf_cm = @ShareOfShelfCm,
                    selection_type = @SelectionType,
                    selection_value = @SelectionValue,
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID 
                  AND (ss = 0 OR ss IS NULL)";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UID", planogramSetup.UID),
                    new NpgsqlParameter("@CategoryCode", planogramSetup.CategoryCode),
                    new NpgsqlParameter("@ShareOfShelfCm", planogramSetup.ShareOfShelfCm ?? (object)DBNull.Value),
                    new NpgsqlParameter("@SelectionType", planogramSetup.SelectionType ?? (object)DBNull.Value),
                    new NpgsqlParameter("@SelectionValue", planogramSetup.SelectionValue ?? (object)DBNull.Value),
                    new NpgsqlParameter("@ModifiedBy", planogramSetup.ModifiedBy),
                    new NpgsqlParameter("@ModifiedTime", planogramSetup.ModifiedTime),
                    new NpgsqlParameter("@ServerModifiedTime", planogramSetup.ServerModifiedTime)
                };

                var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating planogram setup with UID: {planogramSetup.UID}", ex);
            }
        }

        public async Task<bool> DeletePlanogramSetupAsync(string uid)
        {
            try
            {
                // Soft delete by setting ss = 1
                var sql = @"
                UPDATE planogram_setup 
                SET ss = 1,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@UID", uid),
                    new NpgsqlParameter("@ModifiedTime", DateTime.Now),
                    new NpgsqlParameter("@ServerModifiedTime", DateTime.Now)
                };

                var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting planogram setup with UID: {uid}", ex);
            }
        }

        public async Task<object> SearchPlanogramSetupsAsync(
            string searchText,
            List<string> categoryCodes,
            decimal? minShelfCm,
            decimal? maxShelfCm,
            string selectionType,
            int pageNumber,
            int pageSize)
        {
            try
            {
                var offset = (pageNumber - 1) * pageSize;
                var whereClauses = new List<string> { "(ps.ss = 0 OR ps.ss IS NULL)" };
                var parameters = new List<NpgsqlParameter>();

                // Add search text filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    whereClauses.Add("(ps.category_code ILIKE @SearchText OR ps.selection_value ILIKE @SearchText)");
                    parameters.Add(new NpgsqlParameter("@SearchText", $"%{searchText}%"));
                }

                // Add category filter
                if (categoryCodes != null && categoryCodes.Any())
                {
                    whereClauses.Add("ps.category_code = ANY(@CategoryCodes)");
                    parameters.Add(new NpgsqlParameter("@CategoryCodes", categoryCodes.ToArray()));
                }

                // Add shelf cm filters
                if (minShelfCm.HasValue)
                {
                    whereClauses.Add("ps.share_of_shelf_cm >= @MinShelfCm");
                    parameters.Add(new NpgsqlParameter("@MinShelfCm", minShelfCm.Value));
                }

                if (maxShelfCm.HasValue)
                {
                    whereClauses.Add("ps.share_of_shelf_cm <= @MaxShelfCm");
                    parameters.Add(new NpgsqlParameter("@MaxShelfCm", maxShelfCm.Value));
                }

                // Add selection type filter
                if (!string.IsNullOrEmpty(selectionType))
                {
                    whereClauses.Add("ps.selection_type = @SelectionType");
                    parameters.Add(new NpgsqlParameter("@SelectionType", selectionType));
                }

                var whereClause = string.Join(" AND ", whereClauses);

                // Get total count
                var countSql = $@"
                SELECT COUNT(*) as total_count
                FROM planogram_setup ps
                WHERE {whereClause}";

                var parametersDict = parameters.ToDictionary(p => p.ParameterName.TrimStart('@'), p => p.Value);
                var countResults = await ExecuteQueryAsync<dynamic>(countSql, parametersDict);
                var totalCount = Convert.ToInt32(countResults.First().total_count);

                // Get paginated results
                var dataSql = $@"
                SELECT 
                    ps.*,
                    sg.name as category_name
                FROM planogram_setup ps
                LEFT JOIN sku_group sg ON sg.code = ps.category_code
                WHERE {whereClause}
                ORDER BY ps.category_code, ps.created_time DESC
                LIMIT @PageSize OFFSET @Offset";

                parametersDict.Add("PageSize", pageSize);
                parametersDict.Add("Offset", offset);

                var dataResults = await ExecuteQueryAsync<dynamic>(dataSql, parametersDict);
                var setups = dataResults.Select(row => new
                {
                    UID = row.uid?.ToString(),
                    CategoryCode = row.category_code?.ToString(),
                    CategoryName = row.category_name?.ToString(),
                    ShareOfShelfCm = row.share_of_shelf_cm != null ? Convert.ToDecimal(row.share_of_shelf_cm) : (decimal?)null,
                    SelectionType = row.selection_type?.ToString(),
                    SelectionValue = row.selection_value?.ToString(),
                    CreatedTime = row.created_time != null ? Convert.ToDateTime(row.created_time) : DateTime.MinValue,
                    ModifiedTime = row.modified_time != null ? Convert.ToDateTime(row.modified_time) : (DateTime?)null
                }).ToList();

                return new
                {
                    totalCount,
                    pageNumber,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    data = setups
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error searching planogram setups", ex);
            }
        }

        #endregion

        #region Helper Methods

        private IPlanogramSetup MapRowToPlanogramSetup(DataRow row)
        {
            return new PlanogramSetup
            {
                Id = Convert.ToInt32(row["id"]),
                UID = row["uid"]?.ToString(),
                CategoryCode = row["category_code"]?.ToString(),
                ShareOfShelfCm = row["share_of_shelf_cm"] != DBNull.Value ? Convert.ToDecimal(row["share_of_shelf_cm"]) : null,
                SelectionType = row["selection_type"]?.ToString(),
                SelectionValue = row["selection_value"]?.ToString(),
                CreatedBy = row["created_by"]?.ToString(),
                CreatedTime = row["created_time"] != DBNull.Value ? Convert.ToDateTime(row["created_time"]) : DateTime.MinValue,
                ModifiedBy = row["modified_by"]?.ToString(),
                ModifiedTime = row["modified_time"] != DBNull.Value ? Convert.ToDateTime(row["modified_time"]) : (DateTime?)null,
                ServerAddTime = row["server_add_time"] != DBNull.Value ? Convert.ToDateTime(row["server_add_time"]) : DateTime.MinValue,
                ServerModifiedTime = row["server_modified_time"] != DBNull.Value ? Convert.ToDateTime(row["server_modified_time"]) : (DateTime?)null
            };
        }

        #endregion
    }
}