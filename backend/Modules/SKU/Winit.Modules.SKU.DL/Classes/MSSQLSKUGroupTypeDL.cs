using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSKUGroupTypeDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUGroupTypeDL
    {
        public MSSQLSKUGroupTypeDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>> SelectAllSKUGroupTypeDetails(List<SortCriteria> sortCriterias, int pageNumber,
      int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"select * from (SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    org_uid AS OrgUID,
                    code AS Code,
                    name AS Name,
                    parent_uid AS ParentUID,
                    item_level AS ItemLevel
                FROM sku_group_type) as SubQuery");
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
                    org_uid AS OrgUID,
                    code AS Code,
                    name AS Name,
                    parent_uid AS ParentUID,
                    item_level AS ItemLevel
                FROM sku_group_type) as subquery");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>(filterCriterias, sbFilterCriteria, parameters);

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

                IEnumerable<Model.Interfaces.ISKUGroupType> skuGroupTypeList = await ExecuteQueryAsync<Model.Interfaces.ISKUGroupType>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>
                {
                    PagedData = skuGroupTypeList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType> SelectSKUGroupTypeByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                org_uid AS OrgUID,
                code AS Code,
                name AS Name,
                parent_uid AS ParentUID,
                item_level AS ItemLevel
            FROM sku_group_type
            WHERE uid= @UID";
            Winit.Modules.SKU.Model.Interfaces.ISKUGroupType skGroupTypeDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroupType>(sql, parameters);
            return skGroupTypeDetails;
        }
        public async Task<int> CreateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType)
        {
            try
            {
                var sql = @"INSERT INTO sku_group_type (uid, created_by, created_time, modified_by, modified_time,
                            server_add_time, server_modified_time, org_uid, code, name, parent_uid, item_level)
                            Values(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime
                            ,@OrgUID,@Code,@Name,@ParentUID,@ItemLevel);";
               
                return await ExecuteNonQueryAsync(sql, sKUGroupType);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupType sKUGroupType)
        {
            try
            {
                var sql = @"UPDATE sku_group_type
                SET modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    code = @Code,
                    name = @Name,
                    parent_uid = @ParentUID,
                    item_level = @ItemLevel
                WHERE uid = @UID;";
                return await ExecuteNonQueryAsync(sql, sKUGroupType);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUGroupTypeByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_group_type
                WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }

        public async Task<IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView>> SelectSKUGroupTypeView()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT id AS Id,
                uid AS UID,
                org_uid AS OrgUID,
                code AS Code,
                name AS Name,
                parent_uid AS ParentUID,
                item_level AS ItemLevel
            FROM sku_group_type;";
            IEnumerable<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView> skGroupTypeDetails = await ExecuteQueryAsync<Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeView>(sql, parameters);
            return skGroupTypeDetails;
        }

        public async Task<ISKUAttributeLevel> SelectSKUAttributeDDL()
        {
            ISKUAttributeLevel result = null;
            Type type = _serviceProvider.GetRequiredService<ISelectionItem>().GetType();
            try
            {
                var sql = @"
                    SELECT uid AS UID,
                    code AS Code,
                    name AS Label
                FROM sku_group_type;
            
                    SELECT DISTINCT type AS Type,
                    code AS UID,
                    code AS Code,
                    value AS Label
                FROM sku_attributes
                ORDER BY 1;";


                var parameters = new Dictionary<string, object>();

                DataSet ds = await ExecuteQueryDataSetAsync(sql, parameters);
                if (ds != null && ds.Tables.Count == 2)
                {
                    result = _serviceProvider.CreateInstance<ISKUAttributeLevel>();
                    DataTable dataTable0 = ds.Tables[0];
                    DataTable dataTable1 = ds.Tables[1];

                    if (dataTable0.Rows.Count > 0)
                    {
                        result.SKUGroupTypes = new List<ISelectionItem>();
                        foreach (DataRow row in dataTable0.Rows)
                        {
                            ISelectionItem selectionItem = ConvertDataTableToObject<ISelectionItem>(row, null, type);
                            result.SKUGroupTypes.Add(selectionItem);
                        }
                    }
                    if (dataTable1.Rows.Count > 0)
                    {
                        result.SKUGroups = new Dictionary<string, List<ISelectionItem>>();

                        foreach (DataRow row in dataTable1.Rows)
                        {
                            ISelectionItem selectionItem = ConvertDataTableToObject<ISelectionItem>(row, null, type);
                            string groupType = ToString(row["Type"]);
                            if (!result.SKUGroups.ContainsKey(groupType))
                            {
                                result.SKUGroups[groupType] = new List<ISelectionItem>();
                            }
                            result.SKUGroups[groupType].Add(selectionItem);
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
