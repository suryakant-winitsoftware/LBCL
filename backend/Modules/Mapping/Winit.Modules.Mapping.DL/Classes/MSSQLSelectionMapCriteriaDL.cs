using Azure.Core;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.DL.Classes;

public class MSSQLSelectionMapCriteriaDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISelectionMapCriteriaDL
{
    private readonly ISelectionMapDetailsDL _SelectionMapDetailsDL;
    public MSSQLSelectionMapCriteriaDL(IServiceProvider serviceProvider, IConfiguration config, ISelectionMapDetailsDL
        selectionMapDetailsDL) : base(serviceProvider, config)
    {
        _SelectionMapDetailsDL = selectionMapDetailsDL;
    }
    public async Task<int> CreateSelectionMapCriteria(List<ISelectionMapCriteria?> createSelectionMapCriterias, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"INSERT INTO selection_map_criteria (uid, linked_item_uid, linked_item_type, has_organization, 
                        has_location, has_customer, has_sales_team, has_item, org_count, location_count, customer_count,
                        sales_team_count, item_count, ss, created_time, modified_time, server_add_time, server_modified_time,is_active) 
                        VALUES 
                        (@UID, @LinkedItemUID, @LinkedItemType, @HasOrganization, @HasLocation, @HasCustomer, @HasSalesTeam, 
                        @HasItem, @OrgCount, @LocationCount, @CustomerCount, @SalesTeamCount, @ItemCount, @SS, @CreatedTime,
                        @ModifiedTime, @ServerAddTime, @ServerModifiedTime,@IsActive)";
            return await ExecuteNonQueryAsync(sql, connection, transaction, createSelectionMapCriterias);

        }
        catch (Exception)
        {
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

            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        catch (SqlException ex)
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
            var sql = new StringBuilder(@"select * from(select Id,UID,SS,CREATED_TIME as CreatedTime,MODIFIED_TIME ModifiedTime,
                                          SERVER_ADD_TIME as ServerAddTime,SERVER_MODIFIED_TIME ServerModifiedTime,LINKED_ITEM_UID as LinkedItemUID, 
                                          LINKED_ITEM_TYPE as LinkedItemType, HAS_ORGANIZATION as HasOrganization, HAS_LOCATION as HasLocation, 
                                          HAS_CUSTOMER as HasCustomer, HAS_SALES_TEAM as HasSalesTeam, HAS_ITEM as HasItem, ORG_COUNT as OrgCount,
                                          LOCATION_COUNT as LocationCount, CUSTOMER_COUNT as CustomerCount, SALES_TEAM_COUNT as SalesTeamCount, 
                                          ITEM_COUNT as ItemCount from selection_map_criteria)as subquery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select Id,UID,SS,CREATED_TIME as CreatedTime,MODIFIED_TIME ModifiedTime,
                                          SERVER_ADD_TIME as ServerAddTime,SERVER_MODIFIED_TIME ServerModifiedTime,LINKED_ITEM_UID as LinkedItemUID, 
                                          LINKED_ITEM_TYPE as LinkedItemType, HAS_ORGANIZATION as HasOrganization, HAS_LOCATION as HasLocation, 
                                          HAS_CUSTOMER as HasCustomer, HAS_SALES_TEAM as HasSalesTeam, HAS_ITEM as HasItem, ORG_COUNT as OrgCount,
                                          LOCATION_COUNT as LocationCount, CUSTOMER_COUNT as CustomerCount, SALES_TEAM_COUNT as SalesTeamCount, 
                                          ITEM_COUNT as ItemCount from selection_map_criteria)as subquery");
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
                if (sortCriterias != null && sortCriterias.Count > 0)
                {
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
                else
                {
                    sql.Append($" ORDER BY Id OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }
            }
            IEnumerable<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria> selectionMapCriteriaDetails =
                await ExecuteQueryAsync<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapCriteria>(sql.ToString(),
                parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
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

            var sql = @"UPDATE selection_map_criteria SET 
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
                        ITEM_COUNT = @ItemCount,is_active=@IsActive
                        WHERE UID = @UID";
            return await ExecuteNonQueryAsync(sql, connection, transaction, updateSelectionMapCriteria);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<bool> CUDSelectionMapMaster(ISelectionMapMaster selectionMapMaster)
    {
        if (selectionMapMaster.SelectionMapCriteria == null || selectionMapMaster.SelectionMapDetails == null
            || !selectionMapMaster.SelectionMapDetails.Any())
        {
            throw new Exception("Invalid Data");
        }

        try
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        int selectionMapCriteriaResult = 0;

                        if (selectionMapMaster.SelectionMapCriteria.ActionType == ActionType.Add)
                        {
                            selectionMapCriteriaResult = await CreateSelectionMapCriteria(new List<ISelectionMapCriteria?> { selectionMapMaster.SelectionMapCriteria }, connection, transaction);
                        }
                        else if (selectionMapMaster.SelectionMapCriteria.ActionType == ActionType.Update)
                        {
                            selectionMapCriteriaResult = await UpdateSelectionMapCriteria(selectionMapMaster.SelectionMapCriteria, connection, transaction);
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

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
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

    public Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        throw new NotImplementedException();
    }
}

