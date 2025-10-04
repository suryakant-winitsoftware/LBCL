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
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class PGSQLTaxSKUMapDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager, ITaxSkuMapDL
    {
        public PGSQLTaxSKUMapDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>> SelectAllTaxSkuMapDetails(List<SortCriteria> sortCriterias, int pageNumber,
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
                    company_uid AS CompanyUID,
                    sku_uid AS SKUUID,
                    tax_uid AS TaxUID
                FROM
                    tax_sku_map");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM tax_sku_map");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>(filterCriterias, sbFilterCriteria, parameters);

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
                else if (pageNumber > 0 && pageSize > 0)
                {
                    // Add default sorting only when pagination is needed
                    sql.Append(" ORDER BY id");
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    sql.Append($" LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
                }

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ITaxSkuMap>().GetType();
                IEnumerable<Model.Interfaces.ITaxSkuMap> taxSkuList = await ExecuteQueryAsync<Model.Interfaces.ITaxSkuMap>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>
                {
                    PagedData = taxSkuList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap> SelectTaxSkuMapByUID(string UID)
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
                    company_uid AS CompanyUID,
                    sku_uid AS SKUUID,
                    tax_uid AS TaxUID FROM tax_sku_map WHERE uid= @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKU>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMapDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap>(sql, parameters,type);
            return taxSkuMapDetails;
        }
        public async Task<int> CreateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap)
        {
            try
            {
                var sql = @"INSERT INTO tax_sku_map (uid, created_by, created_time, modified_by, modified_time, server_add_time,
                            server_modified_time, company_uid, sku_uid, tax_uid) Values(@UID,@CreatedBy,@CreatedTime
                           ,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@CompanyUID,@SKUUID,@TaxUID);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",taxSkuMap.UID},
                    {"CreatedBy",taxSkuMap.CreatedBy},
                    {"CreatedTime",taxSkuMap.CreatedTime},
                    {"ModifiedBy",taxSkuMap.ModifiedBy},
                    {"ModifiedTime",taxSkuMap.ModifiedTime},
                    {"ServerAddTime",taxSkuMap.ServerAddTime},
                    {"ServerModifiedTime",taxSkuMap.ServerModifiedTime},
                    {"CompanyUID",taxSkuMap.CompanyUID},
                    {"SKUUID",taxSkuMap.SKUUID},
                    {"TaxUID",taxSkuMap.TaxUID},
                    };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateTaxSkuMap(Winit.Modules.SKU.Model.Interfaces.ITaxSkuMap taxSkuMap)
        {
            try
            {
                var sql = @"UPDATE tax_sku_map SET
                    modified_by = @ModifiedBy,
                    modified_time = @ModifiedTime,
                    server_modified_time = @ServerModifiedTime,
                    company_uid = @CompanyUID,
                    tax_uid = @TaxUID
                WHERE uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",taxSkuMap.UID},
                   {"ModifiedBy",taxSkuMap.ModifiedBy},
                   {"ModifiedTime",taxSkuMap.ModifiedTime},
                   {"ServerModifiedTime",taxSkuMap.ServerModifiedTime},
                   {"CompanyUID",taxSkuMap.CompanyUID},
                   {"TaxUID",taxSkuMap.TaxUID},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteTaxSkuMapByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM tax_sku_map WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
