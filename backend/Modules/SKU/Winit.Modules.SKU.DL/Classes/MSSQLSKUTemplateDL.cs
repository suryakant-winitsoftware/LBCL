using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSKUTemplateDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUTemplateDL
    {
        public MSSQLSKUTemplateDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>> SelectAllSKUTemplateDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                                    var sql = new StringBuilder
                                    (@"SELECT * FROM (SELECT
                                    id AS Id,
                                    uid AS UID,
                                    created_by AS CreatedBy,
                                    created_time AS CreatedTime,
                                    modified_by AS ModifiedBy,
                                    modified_time AS ModifiedTime,
                                    server_add_time AS ServerAddTime,
                                    server_modified_time AS ServerModifiedTime,
                                    template_code AS TemplateCode,
                                    template_name AS TemplateName,
                                    is_active AS IsActive,
                                    sku_template_data AS SKUTemplateData
                                FROM
                                   sku_template)AS SUBQUERY");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT * FROM (SELECT
                                    id AS Id,
                                    uid AS UID,
                                    created_by AS CreatedBy,
                                    created_time AS CreatedTime,
                                    modified_by AS ModifiedBy,
                                    modified_time AS ModifiedTime,
                                    server_add_time AS ServerAddTime,
                                    server_modified_time AS ServerModifiedTime,
                                    template_code AS TemplateCode,
                                    template_name AS TemplateName,
                                    is_active AS IsActive,
                                    sku_template_data AS SKUTemplateData
                                FROM
                                   sku_template)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>(filterCriterias, sbFilterCriteria, parameters);

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

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUTemplate>().GetType();
                IEnumerable<Model.Interfaces.ISKUTemplate> skuTemplateList = await ExecuteQueryAsync<Model.Interfaces.ISKUTemplate>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>
                {
                    PagedData = skuTemplateList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate> SelectSKUTemplateByUID(string UID)
        {
            try
            {
                var sql = @"
            SELECT
                id AS Id,
                uid AS UID,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                template_code AS TemplateCode,
                template_name AS TemplateName,
                is_active AS IsActive,
                sku_template_data AS SKUTemplateData
            FROM
                sku_template
            WHERE
                uid = @UID";

                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "UID", UID }
        };

                Type type = _serviceProvider.GetRequiredService<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>().GetType();
                Winit.Modules.SKU.Model.Interfaces.ISKUTemplate skuTemplateDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUTemplate>(sql, parameters, type);

                return skuTemplateDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate)
        {
            int retVal = -1;
            try
            {
                                                            var sql = @"INSERT INTO sku_template (
                                                            uid,
                                                            created_by,
                                                            created_time,
                                                            modified_by,
                                                            modified_time,
                                                            server_add_time,
                                                            server_modified_time,
                                                            template_code,
                                                            template_name,
                                                            is_active,
                                                            sku_template_data 
                                                        ) VALUES (
                                                            @UID,
                                                            @CreatedBy,
                                                            @CreatedTime,
                                                            @ModifiedBy,
                                                            @ModifiedTime,
                                                            @ServerAddTime,
                                                            @ServerModifiedTime,
                                                            @TemplateCode,
                                                            @TemplateName,
                                                            @IsActive,
                                                            CAST(@SKUTemplateData AS NVARCHAR(MAX))
                                                        )";
                string skuTemplateDataJson = JsonConvert.SerializeObject(sKUTemplate.SKUTemplateData);
                sKUTemplate.SKUTemplateData = skuTemplateDataJson;
                retVal = await ExecuteNonQueryAsync(sql, sKUTemplate);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async   Task<int> UpdateSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate)
        {
            try
            {
                       var sql = @"UPDATE sku_template
                        SET
                            modified_by = @ModifiedBy,
                            modified_time = @ModifiedTime,
                            server_modified_time = @ServerModifiedTime,
                            template_code = @TemplateCode,
                            template_name = @TemplateName,
                            is_active = @IsActive,
                            sku_template_data = CAST(@SKUTemplateData AS NVARCHAR(MAX))
                        WHERE
                            uid = @UID;";
                string skuTemplateDataJson = JsonConvert.SerializeObject(sKUTemplate.SKUTemplateData);
                sKUTemplate.SKUTemplateData = skuTemplateDataJson;
                return await ExecuteNonQueryAsync(sql, sKUTemplate);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUTemplate(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };
                var sql = @"
                    DELETE FROM sku_template WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
