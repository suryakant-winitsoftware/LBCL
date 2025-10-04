using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Planogram.DL.Interfaces;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.Planogram.Model.Interfaces;

namespace Winit.Modules.Planogram.DL.Classes;

public class SQLitePlanogramDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, IPlanogramDL
{
    public SQLitePlanogramDL(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    public async Task<List<IPlanogramCategory>> GetPlanogramCategoriesAsync()
    {
        try
        {
            var sql = @"
            SELECT 
                SG.code AS CategoryCode,
                SG.name AS CategoryName,
                COUNT(*) AS SetupCount,
                FSC.relative_path || '/' || FSC.file_name AS CategoryImage
            FROM planogram_setup PS
            INNER JOIN sku_group SG ON SG.code = PS.category_code
            INNER JOIN sku_group_type SGT ON SGT.uid = SG.sku_group_type_uid AND SGT.code = 'Category'
            LEFT JOIN file_sys FSC ON FSC.linked_item_type = 'SKUGroup' AND FSC.linked_item_uid = SG.uid
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
                /*
                CASE 
                    WHEN PS.selection_type = 'IMAGE_PATH' THEN PS.selection_value
                    ELSE NULL
                END AS RecommendedImagePath,
                */
                FSP.relative_path || '/' || FSP.file_name AS RecommendedImagePath
            FROM planogram_setup PS
            INNER JOIN sku_group SG ON SG.code = PS.category_code
            INNER JOIN sku_group_type SGT ON SGT.uid = SG.sku_group_type_uid AND SGT.code = 'Category'
            LEFT JOIN file_sys FSP ON FSP.linked_item_type = 'Planogram' AND FSP.linked_item_uid = PS.uid
            WHERE PS.category_code = @CategoryCode
            ORDER BY PS.created_time DESC";

            var parameters = new Dictionary<string, object> { { "CategoryCode", categoryCode } };
            return await ExecuteQueryAsync<IPlanogramRecommendation>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching planogram recommendations by category", ex);
        }
    }


    public async Task<IPlanogramSetup> GetPlanogramSetupByUIDAsync(string uid)
    {
        try
        {
            var sql = @"
                SELECT 
                    id as Id,
                    uid as UID,
                    category_code as CategoryCode,
                    share_of_shelf_cm as ShareOfShelfCm,
                    selection_type as SelectionType,
                    selection_value as SelectionValue,
                    created_by as CreatedBy,
                    created_time as CreatedTime,
                    modified_by as ModifiedBy,
                    modified_time as ModifiedTime,
                    server_add_time as ServerAddTime,
                    server_modified_time as ServerModifiedTime
                FROM planogram_setup 
                WHERE uid = @UID";

            var parameters = new Dictionary<string, object> { { "UID", uid } };
            return await ExecuteSingleAsync<IPlanogramSetup>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching planogram setup by UID", ex);
        }
    }

    public async Task<string> CreatePlanogramExecutionHeaderAsync(IPlanogramExecutionHeader header)
    {
        try
        {
           // var uid = Guid.NewGuid().ToString();
            var sql = @"
                INSERT INTO planogram_execution_header 
                (uid, beat_history_uid, store_history_uid, store_uid, job_position_uid, 
                 route_uid, status,ss ,created_by, created_time,modified_by,modified_time)
                VALUES 
                (@UID, @BeatHistoryUID, @StoreHistoryUID, @StoreUID, @JobPositionUID, 
                 @RouteUID, @Status,@SS, @CreatedBy, @CreatedTime,@ModifiedBy,@ModifiedTime )";

            var parameters = new Dictionary<string, object>
            {
                { "UID", header.UID },
                { "BeatHistoryUID", header.BeatHistoryUID },
                { "StoreHistoryUID", header.StoreHistoryUID },
                { "StoreUID", header.StoreUID },
                { "JobPositionUID", header.JobPositionUID },
                { "RouteUID", header.RouteUID },
                { "Status", header.Status ?? "draft" },
                 { "SS", 1 },
                { "CreatedBy", header.CreatedBy },
                { "CreatedTime", header.CreatedTime},
                { "ModifiedBy", header.ModifiedBy },
                { "ModifiedTime", header.ModifiedTime}
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
           // var uid = Guid.NewGuid().ToString();
            var sql = @"
                INSERT INTO planogram_execution_detail 
                (uid, is_planogram_as_per_plan, planogram_execution_header_uid, planogram_setup_uid, executed_on, 
                 is_completed, ss,created_by, created_time,modified_by,modified_time)
                VALUES 
                (@UID, @IsPlanogramAsPerPlan, @PlanogramExecutionHeaderUID, @PlanogramSetupUID, @ExecutedOn, 
                 @IsCompleted, @SS,@CreatedBy, @CreatedTime ,@ModifiedBy,@ModifiedTime)";

            var parameters = new Dictionary<string, object>
            {
                { "UID", detail.UID },
                { "IsPlanogramAsPerPlan", detail.IsPlanogramAsPerPlan },
                { "PlanogramExecutionHeaderUID", detail.PlanogramExecutionHeaderUID },
                { "PlanogramSetupUID", detail.PlanogramSetupUID },
                { "ExecutedOn", detail.ExecutedOn },
                { "IsCompleted", detail.IsCompleted },
                { "SS", 1 },
                { "CreatedBy", detail.CreatedBy },
                { "CreatedTime", detail.CreatedTime },
                { "ModifiedBy", detail.ModifiedBy},
                { "ModifiedTime", detail.ModifiedTime}
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
                SET is_completed = @IsCompleted, 
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID";

            var parameters = new Dictionary<string, object>
            {
                { "UID", uid },
                { "IsCompleted", isCompleted },
                { "ModifiedTime", DateTime.Now },
                { "ServerModifiedTime", DateTime.Now }
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating planogram execution detail status", ex);
        }
    }

    public async Task<List<IPlanogramExecutionDetail>> GetPlanogramExecutionDetailsByHeaderUIDAsync(string headerUID)
    {
        try
        {
            var sql = @"
                SELECT 
                    id as Id,
                    uid as UID,
                    is_planogram_as_per_plan as IsPlanogramAsPerPlan,
                    planogram_execution_header_uid as PlanogramExecutionHeaderUID,
                    planogram_setup_uid as PlanogramSetupUID,
                    executed_on as ExecutedOn,
                    is_completed as IsCompleted,
                    created_by as CreatedBy,
                    created_time as CreatedTime,
                    modified_by as ModifiedBy,
                    modified_time as ModifiedTime,
                    server_add_time as ServerAddTime,
                    server_modified_time as ServerModifiedTime
                FROM planogram_execution_detail 
                WHERE planogram_execution_header_uid = @HeaderUID
                ORDER BY created_time DESC";

            var parameters = new Dictionary<string, object> { { "HeaderUID", headerUID } };
            return await ExecuteQueryAsync<IPlanogramExecutionDetail>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching planogram execution details by header UID", ex);
        }
    }

    // New CRUD methods for PlanogramSetup
    public Task<List<IPlanogramSetup>> GetAllPlanogramSetupsAsync(int pageNumber = 1, int pageSize = 50)
    {
        throw new NotImplementedException();
    }

    public Task<List<IPlanogramSetup>> GetPlanogramSetupsByCategoryAsync(string categoryCode)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreatePlanogramSetupAsync(IPlanogramSetup planogramSetup)
    {
        try
        {
            var sql = @"
                INSERT INTO planogram_setup 
                (uid, category_code, share_of_shelf_cm, selection_type, selection_value, 
                 created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, ss)
                VALUES 
                (@UID, @CategoryCode, @ShareOfShelfCm, @SelectionType, @SelectionValue, 
                 @CreatedBy, @CreatedTime, @ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SS)";

            var parameters = new Dictionary<string, object>
            {
                { "UID", planogramSetup.UID },
                { "CategoryCode", planogramSetup.CategoryCode },
                { "ShareOfShelfCm", planogramSetup.ShareOfShelfCm ?? 0 },
                { "SelectionType", planogramSetup.SelectionType ?? "Category" },
                { "SelectionValue", planogramSetup.SelectionValue ?? "" },
                { "CreatedBy", planogramSetup.CreatedBy ?? "SYSTEM" },
                { "CreatedTime", planogramSetup.CreatedTime ?? DateTime.Now },
                { "ModifiedBy", planogramSetup.ModifiedBy ?? planogramSetup.CreatedBy ?? "SYSTEM" },
                { "ModifiedTime", planogramSetup.ModifiedTime ?? DateTime.Now },
                { "ServerAddTime", planogramSetup.ServerAddTime ?? DateTime.Now },
                { "ServerModifiedTime", planogramSetup.ServerModifiedTime ?? DateTime.Now },
                { "SS", 0 }
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
                WHERE uid = @UID";

            var parameters = new Dictionary<string, object>
            {
                { "UID", planogramSetup.UID },
                { "CategoryCode", planogramSetup.CategoryCode },
                { "ShareOfShelfCm", planogramSetup.ShareOfShelfCm ?? 0 },
                { "SelectionType", planogramSetup.SelectionType ?? "Category" },
                { "SelectionValue", planogramSetup.SelectionValue ?? "" },
                { "ModifiedBy", planogramSetup.ModifiedBy ?? "SYSTEM" },
                { "ModifiedTime", planogramSetup.ModifiedTime ?? DateTime.Now },
                { "ServerModifiedTime", DateTime.Now }
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating planogram setup", ex);
        }
    }

    public async Task<bool> DeletePlanogramSetupAsync(string uid)
    {
        try
        {
            var sql = @"
                UPDATE planogram_setup 
                SET ss = 1,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime
                WHERE uid = @UID";

            var parameters = new Dictionary<string, object>
            {
                { "UID", uid },
                { "ModifiedTime", DateTime.Now },
                { "ServerModifiedTime", DateTime.Now }
            };

            var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("Error deleting planogram setup", ex);
        }
    }

    public Task<object> SearchPlanogramSetupsAsync(
        string searchText, 
        List<string> categoryCodes, 
        decimal? minShelfCm, 
        decimal? maxShelfCm, 
        string selectionType,
        int pageNumber,
        int pageSize)
    {
        throw new NotImplementedException();
    }

}
