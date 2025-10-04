using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSKUToGroupMappingDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager,ISKUToGroupMappingDL
    {
        public MSSQLSKUToGroupMappingDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>> SelectAllSKUToGroupMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sku_uid AS SKUUID,
                    sku_group_uid AS SKUGroupUID
                FROM sku_to_group_mapping;");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_to_group_mapping");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>(filterCriterias, sbFilterCriteria, parameters);

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
                    sql.Append($" OFFSET {(pageNumber - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUToGroupMapping>().GetType();
                IEnumerable<Model.Interfaces. ISKUToGroupMapping> skuToGroupMappingList = await ExecuteQueryAsync<Model.Interfaces.ISKUToGroupMapping>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>
                {
                    PagedData = skuToGroupMappingList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping> SelectSKUToGroupMappingByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT id AS Id,
                    uid AS UID,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sku_uid AS SKUUID,
                    sku_group_uid AS SKUGroupUID FROM sku_to_group_mapping WHERE uid= @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUToGroupMapping>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping skuGroupMappingToDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping>(sql, parameters,type);
            return skuGroupMappingToDetails;
        }
        public async Task<int> CreateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping skuToGroupMapping)
        {
            try
            {
                var sql   = @"INSERT INTO sku_to_group_mapping (uid, created_by, created_time, modified_by, modified_time,
                            server_add_time, server_modified_time, sku_uid, sku_group_uid) VALUES(@UID,@CreatedBy,@CreatedTime,
                            @ModifiedBy,@ModifiedTime, @ServerAddTime,@ServerModifiedTime,@SKUUID,@SKUGroupUID);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",skuToGroupMapping.UID},
                   {"CreatedBy",skuToGroupMapping.CreatedBy},
                   {"CreatedTime",skuToGroupMapping.CreatedTime},
                   {"ModifiedBy",skuToGroupMapping.ModifiedBy},
                   {"ModifiedTime",skuToGroupMapping.ModifiedTime},
                   {"ServerAddTime",skuToGroupMapping.ServerAddTime},
                   {"ServerModifiedTime",skuToGroupMapping.ServerModifiedTime},
                   {"SKUUID",skuToGroupMapping.SKUUID},
                   {"SKUGroupUID",skuToGroupMapping.SKUGroupUID},
                   
                   
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUToGroupMapping(Winit.Modules.SKU.Model.Interfaces.ISKUToGroupMapping skuToGroupMapping)
        {
            try
            {
                var sql = @"UPDATE sku_to_group_mapping
                SET modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    sku_uid = @SKUUID, 
                    sku_group_uid = @SKUGroupUID
                WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",skuToGroupMapping.UID},
                   {"ModifiedBy",skuToGroupMapping.ModifiedBy},
                   {"ModifiedTime",skuToGroupMapping.ModifiedTime},
                   {"ServerModifiedTime",skuToGroupMapping.ServerModifiedTime},
                   {"SKUUID",skuToGroupMapping.SKUUID},
                   {"SKUGroupUID",skuToGroupMapping.SKUGroupUID},
                  
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUToGroupMappingByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_to_group_mapping WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
