using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.DL.Classes;

public class PGSQLSelectionMapCriteriaDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISelectionMapCriteriaDL
{
    private readonly ISelectionMapDetailsDL _SelectionMapDetailsDL;
    private readonly ILogger<PGSQLSelectionMapCriteriaDL> _logger;
    
    public PGSQLSelectionMapCriteriaDL(IServiceProvider serviceProvider, IConfiguration config, ISelectionMapDetailsDL
        selectionMapDetailsDL, ILogger<PGSQLSelectionMapCriteriaDL> logger) : base(serviceProvider, config)
    {
        _SelectionMapDetailsDL = selectionMapDetailsDL;
        _logger = logger;
    }
    public async Task<int> CreateSelectionMapCriteria(List<ISelectionMapCriteria?> createSelectionMapCriterias, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            _logger.LogInformation("[DEBUG] CreateSelectionMapCriteria called with {Count} items", createSelectionMapCriterias.Count);
            foreach (var criteria in createSelectionMapCriterias)
            {
                _logger.LogInformation("[DEBUG] - UID: {UID}, LinkedItemUID: {LinkedItemUID}, IsActive: {IsActive}", 
                    criteria?.UID, criteria?.LinkedItemUID, criteria?.IsActive);
            }
            
            var queryAndParams = PrepareBulkInsertQuery(createSelectionMapCriterias);
            _logger.LogInformation("[DEBUG] SQL Query: {Query}", queryAndParams.Item1);
            _logger.LogInformation("[DEBUG] Parameters count: {Count}", queryAndParams.Item2.Count);
            
            var result = await ExecuteNonQueryAsync(queryAndParams.Item1, connection, transaction, queryAndParams.Item2);
            _logger.LogInformation("[DEBUG] CreateSelectionMapCriteria ExecuteNonQueryAsync returned: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DEBUG] ERROR in CreateSelectionMapCriteria: {Message}", ex.Message);
            _logger.LogError("[DEBUG] Stack trace: {StackTrace}", ex.StackTrace);
            throw;
        }
    }

    public async Task<int> DeleteSelectionMapCriteria(string UID, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                { "UID", UID }
            };

            var sql = @"DELETE FROM Selection_Map_Criteria WHERE UID = @UID";

            return await ExecuteNonQueryAsync(sql, connection, transaction,parameters);
        }
        catch (NpgsqlException ex)
        {
            if (ex.SqlState == "23503")
            {
                return -1;
            }
            throw new Exception("An error occurred while deleting the SelectionMapCriteria.", ex);
        }
    }

    public Task<ISelectionMapCriteria> GetSelectionMapCriteriaByUID(string UID)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<ISelectionMapCriteria>> SelectAllSelectionMapCriteria(List<SortCriteria> sortCriterias,
        int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT * FROM (
        SELECT
            id as Id,
            uid as UID,
            ss as SS,
            created_time as CreatedTime,
            modified_time as ModifiedTime,
            server_add_time as ServerAddTime,
            server_modified_time as ServerModifiedTime,
            linked_item_uid as LinkedItemUID,
            linked_item_type as LinkedItemType,
            has_organization as HasOrganization,
            has_location as HasLocation,
            has_customer as HasCustomer,
            has_sales_team as HasSalesTeam,
            has_item as HasItem,
            org_count as OrgCount,
            location_count as LocationCount,
            customer_count as CustomerCount,
            sales_team_count as SalesTeamCount,
            item_count as ItemCount,
            is_active as IsActive
        FROM selection_map_criteria
    ) AS subquery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM selection_map_criteria");
            }
            var parameters = new Dictionary<string, object?>();
            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" where ");
                AppendFilterCriteria<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria>(filterCriterias,
                    sbFilterCriteria, parameters);
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
                sql.Append($" OFFSET {((pageNumber - 1) * pageSize)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria> selectionMapCriteriaDetails =
                await ExecuteQueryAsync<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria>(sql.ToString(),
                parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria> pagedResponse = new
                PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria>
            {
                PagedData = selectionMapCriteriaDetails,
                TotalCount = totalCount
            };
            return pagedResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateSelectionMapCriteria(ISelectionMapCriteria updateSelectionMapCriteria, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {

            var sql = @"UPDATE Selection_Map_Criteria SET 
                        SS = @SS,
                        MODIFIED_TIME = @ModifiedTime,
                        SERVER_MODIFIED_TIME = @ServerModifiedTime,
                        LINKED_ITEM_UID = @LinkedItemUID, 
                        LINKED_ITEM_TYPE = @LinkedItemType,
                        HAS_ORGANIZATION = @HasOrganization,
                        HAS_LOCATION = @HasLocation,
                        HAS_CUSTOMER = @HasCustomer, 
                        HAS_SALES_TEAM = @HasSalesTeam, 
                        HAS_ITEM = @HasItem, 
                        ORG_COUNT = @OrgCount,
                        LOCATION_COUNT = @LocationCount, 
                        CUSTOMER_COUNT = @CustomerCount,
                        SALES_TEAM_COUNT = @SalesTeamCount,
                        ITEM_COUNT = @ItemCount,
                        IS_ACTIVE = @IsActive
                        WHERE UID = @UID";

            Dictionary<string, object?> parameters = new Dictionary<string, object?>
                {
                   {"ModifiedTime", updateSelectionMapCriteria.ModifiedTime},
                   {"ServerModifiedTime", updateSelectionMapCriteria.ServerModifiedTime},
                   {"SS", updateSelectionMapCriteria.SS},
                   {"LinkedItemUID", updateSelectionMapCriteria.LinkedItemUID},
                   {"LinkedItemType", updateSelectionMapCriteria.LinkedItemType},
                   {"HasOrganization", updateSelectionMapCriteria.HasOrganization},
                   {"HasLocation", updateSelectionMapCriteria.HasLocation},
                   {"HasCustomer", updateSelectionMapCriteria.HasCustomer},
                   {"HasSalesTeam", updateSelectionMapCriteria.HasSalesTeam},
                   {"HasItem", updateSelectionMapCriteria.HasItem},
                   {"OrgCount", updateSelectionMapCriteria.OrgCount},
                   {"LocationCount", updateSelectionMapCriteria.LocationCount},
                   {"CustomerCount", updateSelectionMapCriteria.CustomerCount},
                   {"SalesTeamCount", updateSelectionMapCriteria.SalesTeamCount},
                   {"ItemCount", updateSelectionMapCriteria.ItemCount},
                   {"IsActive", updateSelectionMapCriteria.IsActive},
                   {"UID", updateSelectionMapCriteria.UID},
                 };
            return await ExecuteNonQueryAsync(sql,connection,transaction, parameters);
        }
        catch (Exception)
        {
            throw;
        }
    }
    private (string, IDictionary<string, object?>) PrepareBulkInsertQuery(List<ISelectionMapCriteria> selectionMapCriterias)
    {
        StringBuilder sqlQuery = new StringBuilder();
        sqlQuery.Append(@"INSERT INTO SELECTION_MAP_CRITERIA (UID,SS,CREATED_TIME,MODIFIED_TIME,SERVER_ADD_TIME,SERVER_MODIFIED_TIME,
        LINKED_ITEM_UID, LINKED_ITEM_TYPE, HAS_ORGANIZATION, HAS_LOCATION, HAS_CUSTOMER, HAS_SALES_TEAM, HAS_ITEM, ORG_COUNT,
        LOCATION_COUNT, CUSTOMER_COUNT, SALES_TEAM_COUNT, ITEM_COUNT, IS_ACTIVE)");
        sqlQuery.Append(" VALUES ");

        IDictionary<string, object?> parameters = new Dictionary<string, object?>();
        for (var i = 0; i < selectionMapCriterias.Count; i++)
        {
            var selectionMapCriteria = selectionMapCriterias[i];
            sqlQuery.Append(@$"(@UID{i}, @SS{i}, @CreatedTime{i}, @ModifiedTime{i}, @ServerAddTime{i}, @ServerModifiedTime{i},
                @LinkedItemUID{i},@LinkedItemType{i},@HasOrganization{i}, @HasLocation{i}, @HasCustomer{i}, @HasSalesTeam{i}, 
                @HasItem{i}, @OrgCount{i}, @LocationCount{i}, @CustomerCount{i}, @SalesTeamCount{i}, @ItemCount{i}, @IsActive{i})");
            parameters.TryAdd($"@UID{i}", selectionMapCriteria.UID);
            parameters.TryAdd($"@SS{i}", selectionMapCriteria.SS);
            parameters.TryAdd($"@CreatedTime{i}", selectionMapCriteria.CreatedTime);
            parameters.TryAdd($"@ModifiedTime{i}", selectionMapCriteria.ModifiedTime);
            parameters.TryAdd($"@ServerAddTime{i}", selectionMapCriteria.ServerAddTime);
            parameters.TryAdd($"@ServerModifiedTime{i}", selectionMapCriteria.ServerModifiedTime);
            parameters.TryAdd($"@LinkedItemUID{i}", selectionMapCriteria.LinkedItemUID);
            parameters.TryAdd($"@LinkedItemType{i}", selectionMapCriteria.LinkedItemType);
            parameters.TryAdd($"@HasOrganization{i}", selectionMapCriteria.HasOrganization);
            parameters.TryAdd($"@HasLocation{i}", selectionMapCriteria.HasLocation);
            parameters.TryAdd($"@HasCustomer{i}", selectionMapCriteria.HasCustomer);
            parameters.TryAdd($"@HasSalesTeam{i}", selectionMapCriteria.HasSalesTeam);
            parameters.TryAdd($"@HasItem{i}", selectionMapCriteria.HasItem);
            parameters.TryAdd($"@OrgCount{i}", selectionMapCriteria.OrgCount);
            parameters.TryAdd($"@LocationCount{i}", selectionMapCriteria.LocationCount);
            parameters.TryAdd($"@CustomerCount{i}", selectionMapCriteria.CustomerCount);
            parameters.TryAdd($"@SalesTeamCount{i}", selectionMapCriteria.SalesTeamCount);
            parameters.TryAdd($"@ItemCount{i}", selectionMapCriteria.ItemCount);
            parameters.TryAdd($"@IsActive{i}", selectionMapCriteria.IsActive);
            if (i < selectionMapCriterias.Count - 1)
            {
                sqlQuery.Append(",");
            }
        }
        return (sqlQuery.ToString(), parameters);
    }
    public async Task<bool> CUDSelectionMapMaster(ISelectionMapMaster selectionMapMaster)
    {
        _logger.LogInformation("[DEBUG] CUDSelectionMapMaster called at {Time}", DateTime.Now);
        
        if (selectionMapMaster.SelectionMapCriteria == null || selectionMapMaster.SelectionMapDetails == null
           || !selectionMapMaster.SelectionMapDetails.Any())
        {
            _logger.LogError("[DEBUG] Invalid data - null criteria or details");
            throw new Exception("Invalid Data");
        }

        _logger.LogInformation("[DEBUG] Processing SelectionMapCriteria UID: {UID}", selectionMapMaster.SelectionMapCriteria.UID);
        _logger.LogInformation("[DEBUG] ActionType: {ActionType}", selectionMapMaster.SelectionMapCriteria.ActionType);
        _logger.LogInformation("[DEBUG] LinkedItemUID: {LinkedItemUID}", selectionMapMaster.SelectionMapCriteria.LinkedItemUID);
        _logger.LogInformation("[DEBUG] IsActive: {IsActive}", selectionMapMaster.SelectionMapCriteria.IsActive);
        _logger.LogInformation("[DEBUG] Details count: {Count}", selectionMapMaster.SelectionMapDetails.Count());

        try
        {
            using (var connection = PostgreConnection())
            {
                _logger.LogInformation("[DEBUG] Opening database connection...");
                await connection.OpenAsync();
                _logger.LogInformation("[DEBUG] Connection opened. State: {State}", connection.State);
                
                using (var transaction = connection.BeginTransaction())
                {
                    _logger.LogInformation("[DEBUG] Transaction started");
                    try
                    {
                        int selectionMapCriteriaResult = 0;

                        if (selectionMapMaster.SelectionMapCriteria.ActionType == ActionType.Add)
                        {
                            _logger.LogInformation("[DEBUG] Calling CreateSelectionMapCriteria for ADD operation");
                            selectionMapCriteriaResult = await CreateSelectionMapCriteria(new List<ISelectionMapCriteria?> { selectionMapMaster.SelectionMapCriteria }, connection, transaction);
                            _logger.LogInformation("[DEBUG] CreateSelectionMapCriteria result: {Result} rows affected", selectionMapCriteriaResult);
                        }
                        else if (selectionMapMaster.SelectionMapCriteria.ActionType == ActionType.Update)
                        {
                            _logger.LogInformation("[DEBUG] Calling UpdateSelectionMapCriteria for UPDATE operation");
                            selectionMapCriteriaResult = await UpdateSelectionMapCriteria(selectionMapMaster.SelectionMapCriteria, connection, transaction);
                            _logger.LogInformation("[DEBUG] UpdateSelectionMapCriteria result: {Result} rows affected", selectionMapCriteriaResult);
                        }

                        var selectionMapDetailsAddItems = selectionMapMaster.SelectionMapDetails.Where(e => e.ActionType == ActionType.Add).ToList();
                        if (selectionMapDetailsAddItems.Any())
                        {
                            int selectionMapDetailsCreateResult = await _SelectionMapDetailsDL.CreateSelectionMapDetails(selectionMapDetailsAddItems, connection, transaction);
                        }

                        var selectionMapDetailsUpdateItems = selectionMapMaster.SelectionMapDetails.Where(e => e.ActionType == ActionType.Update).ToList();
                        if (selectionMapDetailsUpdateItems.Any())
                        {
                            int selectionMapDetailsUpdateResult = await _SelectionMapDetailsDL.UpdateSelectionMapDetails(selectionMapDetailsUpdateItems, connection, transaction);
                        }

                        var selectionMapDetailsUids = selectionMapMaster.SelectionMapDetails.Where(e => e.ActionType == ActionType.Delete).Select(e => e.UID).ToList();
                        if (selectionMapDetailsUids.Any())
                        {
                            int selectionMapDetailsDeleteResult = await _SelectionMapDetailsDL.DeleteSelectionMapDetails(selectionMapDetailsUids, connection, transaction);
                        }

                        _logger.LogInformation("[DEBUG] About to commit transaction...");
                        transaction.Commit();
                        _logger.LogInformation("[DEBUG] Transaction committed successfully!");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[DEBUG] ERROR in transaction: {Message}", ex.Message);
                        _logger.LogError("[DEBUG] Stack trace: {StackTrace}", ex.StackTrace);
                        _logger.LogInformation("[DEBUG] Rolling back transaction...");
                        transaction.Rollback();
                        _logger.LogInformation("[DEBUG] Transaction rolled back");
                        throw;
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
       
    }

    public async Task<ISelectionMapMaster> GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
    {
        try
        {
            ISelectionMapMaster selectionMapMaster = new SelectionMapMaster();

            // Filter by LinkedItemUID to find the mapping for the given SKU Class Group or Price List
            List<FilterCriteria> selectionmapCriterisfilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria("LinkedItemUID",linkedItemUID,FilterType.Equal)
            };
            var selectionMapCriterials = await SelectAllSelectionMapCriteria
                (null, 0, 0, selectionmapCriterisfilterCriterias, false);

            // If not found by LinkedItemUID, return null
            if (selectionMapCriterials == null || selectionMapCriterials.PagedData == null ||
                !selectionMapCriterials.PagedData.Any())
            {
                return null;
            }
            selectionMapMaster.SelectionMapCriteria = selectionMapCriterials.PagedData.First();
            string selectionMapCriteriaUID = selectionMapMaster.SelectionMapCriteria.UID;
            List<FilterCriteria> selectionmapDetailsfilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria("SelectionMapCriteriaUID",selectionMapCriteriaUID,FilterType.Equal)
            };
            var selectionMapDetails = await _SelectionMapDetailsDL.SelectAllSelectionMapDetails
                (null, 0, 0, selectionmapDetailsfilterCriterias, false);
            if (selectionMapDetails != null && selectionMapDetails.PagedData != null)
            {
                selectionMapMaster.SelectionMapDetails = selectionMapDetails.PagedData.ToList<ISelectionMapDetails>();
            }
            else
            {
                selectionMapMaster.SelectionMapDetails = new List<ISelectionMapDetails>();
            }
            return selectionMapMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ISelectionMapMaster> GetSelectionMapMasterByCriteriaUID(string criteriaUID)
    {
        try
        {
            ISelectionMapMaster selectionMapMaster = new SelectionMapMaster();

            // Filter by UID to find the criteria directly
            List<FilterCriteria> selectionmapCriterisfilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria("UID",criteriaUID,FilterType.Equal)
            };
            var selectionMapCriterials = await SelectAllSelectionMapCriteria
                (null, 0, 0, selectionmapCriterisfilterCriterias, false);

            // If not found by UID, return null
            if (selectionMapCriterials == null || selectionMapCriterials.PagedData == null ||
                !selectionMapCriterials.PagedData.Any())
            {
                return null;
            }
            selectionMapMaster.SelectionMapCriteria = selectionMapCriterials.PagedData.First();

            // Fetch all details for this criteria
            List<FilterCriteria> selectionmapDetailsfilterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria("SelectionMapCriteriaUID",criteriaUID,FilterType.Equal)
            };
            var selectionMapDetails = await _SelectionMapDetailsDL.SelectAllSelectionMapDetails
                (null, 0, 0, selectionmapDetailsfilterCriterias, false);
            if (selectionMapDetails != null && selectionMapDetails.PagedData != null)
            {
                selectionMapMaster.SelectionMapDetails = selectionMapDetails.PagedData.ToList<ISelectionMapDetails>();
            }
            else
            {
                selectionMapMaster.SelectionMapDetails = new List<ISelectionMapDetails>();
            }
            return selectionMapMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task RollbackTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await transaction.RollbackAsync();
        await connection.CloseAsync();
    }
    public async Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        Dictionary<string, List<string>> storeLinkedItems = new Dictionary<string, List<string>>();

        string storeQuery = string.Empty;
        if (storeUIDs != null && storeUIDs.Count > 0)
        {
            storeQuery = "WHERE store_uid IN @StoreUID";
        }

        string query = $@"
            ;WITH Stores AS (
	            SELECT store_uid AS StoreUID, broad_classification AS BroadClassificationUID,  branch_uid AS BranchUID 
                FROM VW_StoreImpAttributes
	            {storeQuery}
            )
            SELECT s.StoreUID, smc.linked_item_uid AS LinkedItemUID
            FROM selection_map_criteria smc 
            JOIN Stores s ON 1=1  -- Ensures the Store CTE is available
            WHERE /*is_active = 1 AND*/ linked_item_type = @LinkedItemType AND
                -- Check if has_customer is true and valid customers exist
                (
                    smc.has_customer = 0 
                    OR (smc.customer_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'Customer'
                        AND smd.type_uid = 'Store'
                        AND smd.selection_value = s.StoreUID
                    ))
                )
                -- Check if has_sales_team is true and valid sales teams exist
                AND (
                    smc.has_sales_team = 0 
                    OR (smc.sales_team_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'SalesTeam'
                        AND smd.type_uid = 'BroadClassification'
                        AND smd.selection_value = s.BroadClassificationUID
                    ))
                )
                -- Check if has_location is true and valid locations exist
                AND (
                    smc.has_location = 0 
                    OR (smc.location_count > 0 AND EXISTS (
                        SELECT 1 FROM selection_map_details smd
                        WHERE smd.selection_map_criteria_uid = smc.uid
                        AND smd.selection_group = 'Location'
                        AND smd.type_uid = 'Branch'
                        AND smd.selection_value = s.BranchUID
                    ))
                )";

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
             {"LinkedItemType",  linkedItemType}
        };

        var result = await ExecuteQueryAsync<(string StoreUID, string LinkedItemUID)>(query, parameters);

        foreach (var row in result)
        {
            if (!storeLinkedItems.ContainsKey(row.StoreUID))
            {
                storeLinkedItems[row.StoreUID] = new List<string>();
            }
            storeLinkedItems[row.StoreUID].Add(row.LinkedItemUID);
        }
        return storeLinkedItems;
    }

}

