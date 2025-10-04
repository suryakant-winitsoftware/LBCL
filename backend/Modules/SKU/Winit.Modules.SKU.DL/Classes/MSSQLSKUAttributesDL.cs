using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class MSSQLSKUAttributesDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUAttributesDL
{
    public MSSQLSKUAttributesDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
    {
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>> SelectAllSKUAttributesDetails(List<SortCriteria> sortCriterias, int pageNumber,
    int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        try
        {
            StringBuilder sql = new(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    ss AS SS,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sku_uid AS SKUUID,
                    type AS Type,
                    code AS Code,
                    value AS Value,
                    parent_type AS ParentType
                FROM 
                    sku_attributes");
            StringBuilder sqlCount = new();
            if (isCountRequired)
            {
                sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_attributes");
            }
            Dictionary<string, object?> parameters = new();

            if (filterCriterias != null && filterCriterias.Count > 0)
            {
                StringBuilder sbFilterCriteria = new();
                _ = sbFilterCriteria.Append(" WHERE ");
                AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(filterCriterias, sbFilterCriteria, parameters);

                _ = sql.Append(sbFilterCriteria);
                if (isCountRequired)
                {
                    _ = sqlCount.Append(sbFilterCriteria);
                }
            }

            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                _ = sql.Append(" ORDER BY ");
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

            IEnumerable<Model.Interfaces.ISKUAttributes> skuAttributesList = await ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(sql.ToString(), parameters);
            int totalCount = 0;
            if (isCountRequired)
            {
                totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
            }

            PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> pagedResponse = new()
            {
                PagedData = skuAttributesList,
                TotalCount = totalCount
            };

            return pagedResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SelectSKUAttributesByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"SELECT 
                id AS Id,
                uid AS UID,
                ss AS SS,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                sku_uid AS SKUUID,
                type AS Type,
                code AS Code,
                value AS Value,
                parent_type AS ParentType
            FROM 
                sku_attributes
            WHERE 
                uid = @UID";
        Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuAttributesDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(sql, parameters);
        return skuAttributesDetails;
    }
    public async Task<int> CreateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes)
    {
        try
        {
            string sql = @"INSERT INTO sku_attributes (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,
                                sku_uid,type,code,value,parent_type) Values (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime
                            ,@ServerModifiedTime,@SKUUID,@Type,@Code,@Value,@ParentType);";
            
            return await ExecuteNonQueryAsync(sql, sKUAttributes);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CreateBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributesList)
    {
        try
        {
            if(sKUAttributesList==null || !sKUAttributesList.Any())
            {
                return 0;
            }
            StringBuilder sql = new();
            _ = sql.Append(@"INSERT INTO sku_attributes (uid, created_by, created_time, modified_by, modified_time, server_add_time, 
                            server_modified_time, sku_uid, type, code, value, parent_type) 
                            VALUES 
                            (@UID, @CreatedBy, @CreatedTime,@ModifiedBy, @ModifiedTime, @ServerAddTime, @ServerModifiedTime, @SKUUID, @Type,
                            @Code, @Value, @ParentType)");
            return await ExecuteNonQueryAsync(sql.ToString(), sKUAttributesList);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> CUDBulkSKUAttributes(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributesList)
    {
        try
        {
            int count = 0;
            if(sKUAttributesList!=null && sKUAttributesList.Any() && sKUAttributesList.Any(e => e.ActionType == ActionType.Add))
            {
                count += await CreateBulkSKUAttributes(sKUAttributesList.FindAll(e => e.ActionType == ActionType.Add));
            }
            if(sKUAttributesList != null && sKUAttributesList.Any() && sKUAttributesList.Any(e => e.ActionType == ActionType.Update))
            {
                count += await UpdateSKUAttributesList(sKUAttributesList.FindAll(e => e.ActionType == ActionType.Update));
            }
            return count;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<int> UpdateSKUAttributesList(List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> sKUAttributes)
    {
        try
        {
            string sql = @"UPDATE sku_attributes 
                SET 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    sku_uid = @SKUUID, 
                    type = @Type, 
                    code = @Code, 
                    value = @Value, 
                    parent_type = @ParentType
                WHERE 
                    uid = @UID;";
            return await ExecuteNonQueryAsync(sql, sKUAttributes);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> UpdateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes)
    {
        try
        {
            string sql = @"UPDATE sku_attributes 
                SET 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    sku_uid = @SKUUID, 
                    type = @Type, 
                    code = @Code, 
                    value = @Value, 
                    parent_type = @ParentType
                WHERE 
                    uid = @UID;";
            return await ExecuteNonQueryAsync(sql, sKUAttributes);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<int> DeleteSKUAttributesByUID(string UID)
    {
        Dictionary<string, object?> parameters = new()
        {
            {"UID" , UID}
        };
        string sql = @"DELETE FROM sku_attributes WHERE uid = @UID;";
        return await ExecuteNonQueryAsync(sql, parameters);
    }
    public async Task<List<SKUAttributeDropdownModel>> GetSKUGroupTypeForSKuAttribute()
    {
        try
        {
            string sql = """
                select uid as UID,name as DropDownTitle, code as code,  parent_uid as ParentUID 
                from sku_group_type where show_in_ui = 1 order by item_level ASC
                """;

            return await ExecuteQueryAsync<SKUAttributeDropdownModel>(sql);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
