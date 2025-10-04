using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes;

public class PGSQLSKUAttributesDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ISKUAttributesDL
{
    public PGSQLSKUAttributesDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
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
                _ = sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
            }

            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUAttributes>().GetType();
            IEnumerable<Model.Interfaces.ISKUAttributes> skuAttributesList = await ExecuteQueryAsync<Model.Interfaces.ISKUAttributes>(sql.ToString(), parameters, type);
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
        Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUAttributes>().GetType();
        Winit.Modules.SKU.Model.Interfaces.ISKUAttributes skuAttributesDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes>(sql, parameters, type);
        return skuAttributesDetails;
    }
    public async Task<int> CreateSKUAttributes(Winit.Modules.SKU.Model.Interfaces.ISKUAttributes sKUAttributes)
    {
        try
        {
            string sql = @"INSERT INTO sku_attributes (uid,created_by,created_time,modified_by,modified_time,server_add_time,server_modified_time,
                                sku_uid,type,code,value,parent_type) Values (@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime
                            ,@ServerModifiedTime,@SKUUID,@Type,@Code,@Value,@ParentType);";
            Dictionary<string, object?> parameters = new()
            {
               {"UID",sKUAttributes.UID},
               {"CreatedBy",sKUAttributes.CreatedBy},
               {"CreatedTime",sKUAttributes.CreatedTime},
               {"ModifiedBy",sKUAttributes.ModifiedBy},
               {"ModifiedTime",sKUAttributes.ModifiedTime},
               {"ServerAddTime",sKUAttributes.ServerAddTime},
               {"ServerModifiedTime",sKUAttributes.ServerModifiedTime},
               {"SKUUID",sKUAttributes.SKUUID},
               {"Type",sKUAttributes.Type},
               {"Code",sKUAttributes.Code},
               {"Value",sKUAttributes.Value},
               {"ParentType",sKUAttributes.ParentType},

            };
            return await ExecuteNonQueryAsync(sql, parameters);
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
            _ = sql.Append(@"INSERT INTO sku_attributes (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                    sku_uid, type, code, value, parent_type) Values ");

            Dictionary<string, object?> parameters = new();
            int index = 0;

            foreach (Model.Interfaces.ISKUAttributes sKUAttributes in sKUAttributesList)
            {
                _ = sql.Append($@"(@UID{index}, @CreatedBy{index}, @CreatedTime{index}, @ModifiedBy{index}, @ModifiedTime{index}, @ServerAddTime{index},
                        @ServerModifiedTime{index}, @SKUUID{index}, @Type{index}, @Code{index}, @Value{index}, @ParentType{index}),");

                parameters.Add($"UID{index}", sKUAttributes.UID);
                parameters.Add($"CreatedBy{index}", sKUAttributes.CreatedBy);
                parameters.Add($"CreatedTime{index}", sKUAttributes.CreatedTime);
                parameters.Add($"ModifiedBy{index}", sKUAttributes.ModifiedBy);
                parameters.Add($"ModifiedTime{index}", DateTime.Now);
                parameters.Add($"ServerAddTime{index}", DateTime.Now);
                parameters.Add($"ServerModifiedTime{index}", sKUAttributes.ServerModifiedTime);
                parameters.Add($"SKUUID{index}", sKUAttributes.SKUUID);
                parameters.Add($"Type{index}", sKUAttributes.Type);
                parameters.Add($"Code{index}", sKUAttributes.Code);
                parameters.Add($"Value{index}", sKUAttributes.Value);
                parameters.Add($"ParentType{index}", sKUAttributes.ParentType);

                index++;
            }

            // Remove the last comma
            sql.Length--;

            return await ExecuteNonQueryAsync(sql.ToString(), parameters);
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
            count += await CreateBulkSKUAttributes(sKUAttributesList.FindAll(e => e.ActionType == ActionType.Add));
            foreach (Model.Interfaces.ISKUAttributes skuAttr in sKUAttributesList.FindAll(e => e.ActionType == ActionType.Update))
            {
                skuAttr.ServerModifiedTime = DateTime.Now;
                count += await UpdateSKUAttributes(skuAttr);
            }
            return count;
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
            Dictionary<string, object?> parameters = new()
            {
               {"UID",sKUAttributes.UID},
               {"ModifiedBy",sKUAttributes.ModifiedBy},
               {"ModifiedTime",sKUAttributes.ModifiedTime},
               {"ServerModifiedTime",sKUAttributes.ServerModifiedTime},
               {"SKUUID",sKUAttributes.SKUUID},
               {"Type",sKUAttributes.Type},
               {"Code",sKUAttributes.Code},
               {"Value",sKUAttributes.Value},
               {"ParentType",sKUAttributes.ParentType},

            };
            return await ExecuteNonQueryAsync(sql, parameters);
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
            string sql = "select uid as UID,name as DropDownTitle,  parent_uid as ParentUID from sku_group_type order by item_level ASC";

            return await ExecuteQueryAsync<SKUAttributeDropdownModel>(sql);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
