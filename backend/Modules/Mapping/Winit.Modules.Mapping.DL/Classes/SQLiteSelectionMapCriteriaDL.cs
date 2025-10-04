using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.DL.Classes;

public class SQLiteSelectionMapCriteriaDL : Winit.Modules.Base.DL.DBManager.SqliteDBManager, ISelectionMapCriteriaDL
{
    private readonly ISelectionMapDetailsDL _SelectionMapDetailsDL;
    public SQLiteSelectionMapCriteriaDL(IServiceProvider serviceProvider, IConfiguration config, ISelectionMapDetailsDL
        selectionMapDetailsDL) : base(serviceProvider)
    {
        _SelectionMapDetailsDL = selectionMapDetailsDL;
    }
    

    Task<int> ISelectionMapCriteriaDL.CreateSelectionMapCriteria(List<ISelectionMapCriteria?> createSelectionMapCriterias, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }

    Task<bool> ISelectionMapCriteriaDL.CUDSelectionMapMaster(ISelectionMapMaster selectionMapMaster)
    {
        throw new NotImplementedException();
    }

    Task<int> ISelectionMapCriteriaDL.DeleteSelectionMapCriteria(string UID, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
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
             {"LinkedItemType",  linkedItemType},
             {"StoreUID",  storeUIDs}
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

    Task<ISelectionMapCriteria> ISelectionMapCriteriaDL.GetSelectionMapCriteriaByUID(string UID)
    {
        throw new NotImplementedException();
    }

    Task<ISelectionMapMaster> ISelectionMapCriteriaDL.GetSelectionMapMasterByLinkedItemUID(string linkedItemUID)
    {
        throw new NotImplementedException();
    }

    Task<ISelectionMapMaster> ISelectionMapCriteriaDL.GetSelectionMapMasterByCriteriaUID(string criteriaUID)
    {
        throw new NotImplementedException();
    }

    Task<PagedResponse<ISelectionMapCriteria>> ISelectionMapCriteriaDL.SelectAllSelectionMapCriteria(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        throw new NotImplementedException();
    }

    Task<int> ISelectionMapCriteriaDL.UpdateSelectionMapCriteria(ISelectionMapCriteria updateSelectionMapCriteria, IDbConnection? connection, IDbTransaction? transaction)
    {
        throw new NotImplementedException();
    }
}

