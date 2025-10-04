using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using System.Text;
using Winit.Modules.SKUClass.DL.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKUClass.DL.Classes;

public class MSSQLSKUClassGroupDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUClassGroupDL
{
    private readonly ISKUClassGroupItemsDL _sKUClassGroupItemsDL;
    public MSSQLSKUClassGroupDL(IServiceProvider serviceProvider, IConfiguration config,
        ISKUClassGroupItemsDL sKUClassGroupItemsDL, ISKUClassGroupItems sKUClassGroupItems) : base(serviceProvider, config)
    {
        _sKUClassGroupItemsDL = sKUClassGroupItemsDL;
    }
    public async Task<PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>> SelectAllSKUClassGroupDetails(List<SortCriteria> sortCriterias, int pageNumber,
                      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            var sql = new StringBuilder(@"SELECT * FROM (SELECT 
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                sku_class_uid AS SKUClassUID,
                name AS Name,
                description AS Description,
                org_uid AS OrgUID,
                distribution_channel_uid AS DistributionChannelUID,
                franchisee_org_uid AS FranchiseeOrgUID,
                is_active AS IsActive,
                from_date AS FromDate,
                to_date AS ToDate,
                source_type AS SourceType,
                source_date AS SourceDate,
                priority AS Priority
            FROM sku_class_group)AS SubQuery");
            var sqlCount = new StringBuilder();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM (SELECT 
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                sku_class_uid AS SKUClassUID,
                name AS Name,
                description AS Description,
                org_uid AS OrgUID,
                distribution_channel_uid AS DistributionChannelUID,
                franchisee_org_uid AS FranchiseeOrgUID,
                is_active AS IsActive,
                from_date AS FromDate,
                to_date AS ToDate,
                source_type AS SourceType,
                source_date AS SourceDate,
                priority AS Priority
            FROM sku_class_group)AS SubQuery");
            }
            var parameters = new Dictionary<string, object?>();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new StringBuilder();
                sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>(filterCriterias, sbFilterCriteria, parameters);

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

            IEnumerable<Model.Interfaces.ISKUClassGroup> sKUClassGroups = await ExecuteQueryAsync<Model.Interfaces.ISKUClassGroup>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup> pagedResponse = new PagedResponse<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>
            {
                PagedData = sKUClassGroups,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup> GetSKUClassGroupByUID(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID",  UID}
            };

        var sql = @"SELECT id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                company_uid AS CompanyUID,
                sku_class_uid AS SKUClassUID,
                name AS Name,
                description AS Description,
                org_uid AS OrgUID,
                distribution_channel_uid AS DistributionChannelUID,
                franchisee_org_uid AS FranchiseeOrgUID,
                is_active AS IsActive,
                from_date AS FromDate,
                to_date AS ToDate,
                source_type AS SourceType,
                source_date AS SourceDate,
                priority AS Priority FROM sku_class_group WHERE uid = @UID";
        return await ExecuteSingleAsync<Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup>(sql, parameters);
    }
    public async Task<int> CreateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup createSKUClassGroup, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"INSERT INTO sku_class_group (
                uid, company_uid, sku_class_uid, description, name, org_uid, distribution_channel_uid, franchisee_org_uid, is_active,
                from_date, to_date, source_type, source_date, priority, created_time, modified_time, server_add_time, server_modified_time,
                created_by, modified_by) VALUES (@UID, @CompanyUID, @SKUClassUID , @Description,@Name ,@OrgUID,@DistributionChannelUID,
                @FranchiseeOrgUID,@IsActive,@FromDate,@ToDate,@SourceType,@SourceDate,@Priority, @CreatedTime, @ModifiedTime, @ServerAddTime, 
                @ServerModifiedTime,@CreatedBy,@ModifiedBy)";
            return await ExecuteNonQueryAsync(sql, connection, transaction, createSKUClassGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateSKUClassGroup(Winit.Modules.SKUClass.Model.Interfaces.ISKUClassGroup updateSKUClassGroup, IDbConnection? connection = null, IDbTransaction? transaction = null)
    {
        try
        {
            var sql = @"UPDATE sku_class_group 
            SET 
                company_uid = @CompanyUID,
                sku_class_uid = @SKUClassUID,
                name = @Name,
                org_uid = @OrgUID,
                distribution_channel_uid = @DistributionChannelUID,
                franchisee_org_uid = @FranchiseeOrgUID,
                is_active = @IsActive,
                from_date = @FromDate,
                to_date = @ToDate,
                source_type = @SourceType,
                source_date = @SourceDate,
                priority = @Priority,
                description = @Description,
                modified_time = @ModifiedTime,
                server_modified_time = @ServerModifiedTime 
            WHERE 
                uid = @UID;";

            return await ExecuteNonQueryAsync(sql, connection, transaction, updateSKUClassGroup);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKUClassGroup(string UID)
    {
        Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID" , UID}
            };
        var sql = @"DELETE FROM sku_class_group WHERE uid = @UID;";

        return await ExecuteNonQueryAsync(sql, parameters);

    }
    public async Task<int> DeleteSKUClassGroupMaster(string skuClassGroupUId)
    {
        try
        {
            Dictionary<string, object?> parameters = new Dictionary<string, object?>
            {
                {"UID" , skuClassGroupUId}
            };
            var sql = @"DELETE FROM  sku_class_group_items WHERE sku_class_group_uid = @UID;
                        delete d from selection_map_criteria c
                            inner join selection_map_details d on c.uid=d.selection_map_criteria_uid
                               and c.linked_item_type=@UID; 
                        delete from selection_map_criteria where linked_item_type=@UID;
                         DELETE  FROM sku_class_group WHERE uid = @UID
                        ";
            int status = await ExecuteNonQueryAsync(sql, parameters);
            return status;
        }
        catch
        {
            throw;
        }
    }
    public async Task<bool> CUD_SKUClassGroupMaster(ISKUClassGroupMaster sKUClassGroupMaster)
    {
        try
        {
            using (var connection = CreateConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (sKUClassGroupMaster.SKUClassGroup == null || sKUClassGroupMaster.SKUClassGroupItems == null
                            || !sKUClassGroupMaster.SKUClassGroupItems.Any()) throw new Exception("Invalid Data");
                        int SKUClassGroupresult = 0;
                        if (sKUClassGroupMaster.SKUClassGroup.ActionType == ActionType.Add)
                        {
                            bool exst = await CheckIfAnyRecordExistsInDBWithAnyColumn(DbTableName.SkuClassGroup, DbColumnName.SkuClassGroup_Name, sKUClassGroupMaster.SKUClassGroup.Name);
                            if (exst)
                                throw new DuplicateNameException($" Name {sKUClassGroupMaster.SKUClassGroup.Name} already exist");

                            SKUClassGroupresult = await CreateSKUClassGroup(sKUClassGroupMaster.SKUClassGroup, connection, transaction);
                        }
                        else if (sKUClassGroupMaster.SKUClassGroup.ActionType == ActionType.Update)
                        {
                            SKUClassGroupresult = await UpdateSKUClassGroup
                                (sKUClassGroupMaster.SKUClassGroup, connection, transaction);
                        }
                        if (SKUClassGroupresult <= 0)
                        {
                            await transaction.RollbackAsync();
                            await connection.CloseAsync();
                            return false;
                        }
                        var sKUClassGroupItemsAddItems = sKUClassGroupMaster.SKUClassGroupItems.FindAll(e => e.ActionType == ActionType.Add);
                        if (sKUClassGroupItemsAddItems != null && sKUClassGroupItemsAddItems.Any())
                        {
                            foreach (var sKUClassGroupItem in sKUClassGroupItemsAddItems)
                            {
                                int SKUClassGroupItemsCreateResult = await _sKUClassGroupItemsDL.CreateSKUClassGroupItems(sKUClassGroupItem, connection, transaction);
                                if (SKUClassGroupItemsCreateResult <= 0)
                                {
                                    await RollbackTransaction(connection, transaction);
                                    return false;
                                }
                            }
                        }

                        foreach (var selectionMapDetail in sKUClassGroupMaster.SKUClassGroupItems.FindAll(e => e.ActionType == ActionType.Update))
                        {
                            int SKUClassGroupItemsUpdateResult = await _sKUClassGroupItemsDL.UpdateSKUClassGroupItems(selectionMapDetail, connection, transaction);
                            if (SKUClassGroupItemsUpdateResult <= 0)
                            {
                                await RollbackTransaction(connection, transaction);
                                return false;
                            }
                        }
                        List<string> sKUClassGroupItemsUids = sKUClassGroupMaster.SKUClassGroupItems.Where
                            (_ => _.ActionType == ActionType.Delete).Select(e => e.UID).ToList();
                        if (sKUClassGroupItemsUids.Any())
                        {
                            int SKUClassGroupItemsDeleteResult = await _sKUClassGroupItemsDL.DeleteSKUClassGroupItems
                            (sKUClassGroupItemsUids, connection, transaction);
                            if (SKUClassGroupItemsDeleteResult <= 0)
                            {
                                await RollbackTransaction(connection, transaction);
                                return false;
                            }
                        }

                        transaction.Commit();
                        connection.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        await connection.CloseAsync();
                        throw;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<ISKUClassGroupMaster> GetSKUClassGroupMaster(string sKUClassGroupUID)
    {
        try
        {
            ISKUClassGroupMaster sKUClassGroupMaster = new SKUClassGroupMaster();
            sKUClassGroupMaster.SKUClassGroup = await GetSKUClassGroupByUID(sKUClassGroupUID);
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>
            {
                new FilterCriteria($@"SKUClassGroupUID",sKUClassGroupUID,FilterType.Equal)
            };
            List<SortCriteria> sortCriterias = new List<SortCriteria>
            {
                new SortCriteria($@"SKUCode",SortDirection.Asc)
            };
            sKUClassGroupMaster.SKUClassGroupItems = (await _sKUClassGroupItemsDL.SelectAllSKUClassGroupItemView(sortCriterias, 0, 0, filterCriterias, false)).PagedData.ToList();
            return sKUClassGroupMaster;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private async Task RollbackTransaction(SqlConnection connection, SqlTransaction transaction)
    {
        await transaction.RollbackAsync();
        await connection.CloseAsync();
    }

}

