using Microsoft.Extensions.Configuration;
using System.Text;
using Winit.Modules.Mapping.DL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using System.Data;


namespace Winit.Modules.Mapping.DL.Classes;

public class PGSQLSelectionMapDetailsDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISelectionMapDetailsDL
{
    public PGSQLSelectionMapDetailsDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {

    }

    public async Task<int> CreateSelectionMapDetails(List<ISelectionMapDetails> createSelectionMapDetails, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            (string, IDictionary<string, object?>) queryandParams = PrepareBulkInsertQuery(createSelectionMapDetails);
            return await ExecuteNonQueryAsync(queryandParams.Item1, connection, transaction, queryandParams.Item2);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> DeleteSelectionMapDetails(List<string> UIDs, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            if (UIDs == null || !UIDs.Any())
            {
                throw new Exception("Received O UIDs");
            }
            Dictionary<string, object> parameters = new();
            int i = 0;
            UIDs.ForEach((e) => { parameters.Add($"UID{i}", e); i++; });

            string sql = @$"DELETE FROM Selection_Map_Details WHERE UID IN ({string.Join(",", UIDs.Select((uid, index) => $"@UID{index}"))})";

            return await ExecuteNonQueryAsync(sql, connection, transaction, parameters);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the SelectionMapDetails.", ex);
        }
    }

    public Task<ISelectionMapDetails> GetSelectionMapDetailsByUID(string UID)
    {
        throw new NotImplementedException();
    }

    public async Task<PagedResponse<ISelectionMapDetails>> SelectAllSelectionMapDetails
        (List<SortCriteria> sortDetailss, int pageNumber, int pageSize, List<FilterCriteria> filterDetailss,
        bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"select * from (select Id, UID,SS,CREATED_TIME as CreatedTime,MODIFIED_TIME as ModifiedTime
        ,SERVER_ADD_TIME as ServerAddTime,SERVER_MODIFIED_TIME as ServerModifiedTime,
        SELECTION_MAP_CRITERIA_UID as SelectionMapCriteriaUID ,SELECTION_GROUP as SelectionGroup,TYPE_UID as TypeUID,
        SELECTION_VALUE as SelectionValue ,IS_EXCLUDED  as IsExcluded from selection_map_Details) SubQ");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (select Id, UID,SS,CREATED_TIME as CreatedTime,MODIFIED_TIME as ModifiedTime
        ,SERVER_ADD_TIME as ServerAddTime,SERVER_MODIFIED_TIME as ServerModifiedTime,
        SELECTION_MAP_CRITERIA_UID as SelectionMapCriteriaUID ,SELECTION_GROUP as SelectionGroup,TYPE_UID as TypeUID,
        SELECTION_VALUE as SelectionValue ,IS_EXCLUDED  as IsExcluded from selection_map_Details)");
            }
            Dictionary<string, object> parameters = new();
            if (filterDetailss != null && filterDetailss.Count > 0)
            {
                StringBuilder sbFilterDetails = new();
                _ = sbFilterDetails.Append(" where ");
                AppendFilterCriteria<ISelectionMapDetails>(filterDetailss,
                    sbFilterDetails, parameters);
                _ = sql.Append(sbFilterDetails);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterDetails);
                }
            }
            if (sortDetailss != null && sortDetailss.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
                AppendSortCriteria(sortDetailss, sql);
            }
            if (pageNumber > 0 && pageSize > 0)
            {
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            IEnumerable<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> selectionMapDetailsDetails =
                await ExecuteQueryAsync<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails>(sql.ToString(),
                parameters);
            int totalCount = -1;
            if (isCountRequired)
            {
                // Get the total count of records
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }
            PagedResponse<Winit.Modules.Mapping.Model.Interfaces.ISelectionMapDetails> pagedResponse = new
()
            {
                PagedData = selectionMapDetailsDetails,
                TotalCount = totalCount
            };
            return pagedResponse;

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> UpdateSelectionMapDetails(List<ISelectionMapDetails> updateSelectionMapDetails, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {

            string sql = @"UPDATE Selection_Map_Details SET 
                        SS = @SS,
                        MODIFIED_TIME = @ModifiedTime,
                        SERVER_MODIFIED_TIME = @ServerModifiedTime,
                        SELECTION_MAP_CRITERIA_UID = @SelectionMapCriteriaUID,
                        SELECTION_GROUP = @SelectionGroup,
                        TYPE_UID = @TypeUID,
                        SELECTION_VALUE = @SelectionValue,
                        IS_EXCLUDED  = @IsExcluded
                        WHERE UID = @UID";

            
            return await ExecuteNonQueryAsync(sql, connection,transaction, updateSelectionMapDetails);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private (string, IDictionary<string, object?>) PrepareBulkInsertQuery(List<ISelectionMapDetails> selectionMapDetails)
    {
        StringBuilder sqlQuery = new();
        _ = sqlQuery.Append(@"INSERT INTO SELECTION_MAP_DETAILS (UID,SS,CREATED_TIME,MODIFIED_TIME,SERVER_ADD_TIME,SERVER_MODIFIED_TIME,
        SELECTION_MAP_CRITERIA_UID,SELECTION_GROUP,TYPE_UID,SELECTION_VALUE,IS_EXCLUDED)");
        _ = sqlQuery.Append(" VALUES ");

        IDictionary<string, object?> parameters = new Dictionary<string, object?>();
        for (int i = 0; i < selectionMapDetails.Count; i++)
        {
            ISelectionMapDetails selectionMapDetail = selectionMapDetails[i];
            _ = sqlQuery.Append(@$"(@UID{i}, @SS{i}, @CreatedTime{i}, @ModifiedTime{i}, @ServerAddTime{i}, @ServerModifiedTime{i},
                @SelectionMapCriteriaUID{i},@SelectionGroup{i},@TypeUID{i}, @SelectionValue{i}, @IsExcluded{i})");
            _ = parameters.TryAdd($"@UID{i}", selectionMapDetail.UID);
            _ = parameters.TryAdd($"@SS{i}", selectionMapDetail.SS);
            _ = parameters.TryAdd($"@CreatedTime{i}", selectionMapDetail.CreatedTime);
            _ = parameters.TryAdd($"@ModifiedTime{i}", selectionMapDetail.ModifiedTime);
            _ = parameters.TryAdd($"@ServerAddTime{i}", selectionMapDetail.ServerAddTime);
            _ = parameters.TryAdd($"@ServerModifiedTime{i}", selectionMapDetail.ServerModifiedTime);
            _ = parameters.TryAdd($"@SelectionMapCriteriaUID{i}", selectionMapDetail.SelectionMapCriteriaUID);
            _ = parameters.TryAdd($"@SelectionGroup{i}", selectionMapDetail.SelectionGroup);
            _ = parameters.TryAdd($"@TypeUID{i}", selectionMapDetail.TypeUID);
            _ = parameters.TryAdd($"@SelectionValue{i}", selectionMapDetail.SelectionValue);
            _ = parameters.TryAdd($"@IsExcluded{i}", selectionMapDetail.IsExcluded);
            if (i < selectionMapDetails.Count - 1)
            {
                _ = sqlQuery.Append(",");
            }
        }
        return (sqlQuery.ToString(), parameters);
    }
}

