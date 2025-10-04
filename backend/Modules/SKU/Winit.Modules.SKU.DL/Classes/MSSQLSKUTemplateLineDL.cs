using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
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
using Winit.Modules.Location.Model.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.DL.Classes
{
    public class MSSQLSKUTemplateLineDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, ISKUTemplateLineDL
    {
        protected ISKUTemplateDL _skuTemplateDL;
        public MSSQLSKUTemplateLineDL(IServiceProvider serviceProvider, IConfiguration config, ISKUTemplateDL skuTemplateDL) : base(serviceProvider, config)
        {
            _skuTemplateDL = skuTemplateDL;
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>> SelectSKUTemplateLineDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder
                (@"SELECT * FROM (SELECT
                                        stl.id AS Id,
                                        stl.uid AS UID,
                                        stl.created_by AS CreatedBy,
                                        stl.created_time AS CreatedTime,
                                        stl.modified_by AS ModifiedBy,
                                        stl.modified_time AS ModifiedTime,
                                        stl.server_add_time AS ServerAddTime,
                                        stl.server_modified_time AS ServerModifiedTime,
                                        stl.sku_template_uid AS SKUTemplateUID,
                                        stl.sku_group_type_uid AS SKUGroupTypeUID,
                                        stl.sku_group_uid AS SKUGroupUID,
										sgt.name as SKUGroupTypeName,
										sg.Name as SKUGroupName,
										sgp.name as SKUGroupParentName,
                                        stl.is_excluded AS IsExcluded
                                    FROM
                                        sku_template_line stl
										Left join sku_group sg on sg.uid=stl.sku_group_uid
										Left Join sku_group_type sgt on sgt.uid=stl.sku_group_type_uid
										Left Join sku_group sgp on sgp.uid=sg.parent_uid
                                    )AS SUBQUERY");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT * FROM (SELECT
                                        stl.id AS Id,
                                        stl.uid AS UID,
                                        stl.created_by AS CreatedBy,
                                        stl.created_time AS CreatedTime,
                                        stl.modified_by AS ModifiedBy,
                                        stl.modified_time AS ModifiedTime,
                                        stl.server_add_time AS ServerAddTime,
                                        stl.server_modified_time AS ServerModifiedTime,
                                        stl.sku_template_uid AS SKUTemplateUID,
                                        stl.sku_group_type_uid AS SKUGroupTypeUID,
                                        stl.sku_group_uid AS SKUGroupUID,
										sgt.name as SKUGroupTypeName,
										sg.Name as SKUGroupName,
										sgp.name as SKUGroupParentName,
                                        stl.is_excluded AS IsExcluded
                                    FROM
                                        sku_template_line stl
										Left join sku_group sg on sg.uid=stl.sku_group_uid
										Left Join sku_group_type sgt on sgt.uid=stl.sku_group_type_uid
										Left Join sku_group sgp on sgp.uid=sg.parent_uid)AS SUBQUERY");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>(filterCriterias, sbFilterCriteria, parameters);

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
                IEnumerable<Model.Interfaces.ISKUTemplateLine> skuTemplateLineList = await ExecuteQueryAsync<Model.Interfaces.ISKUTemplateLine>(sql.ToString(), parameters);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }
                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>
                {
                    PagedData = skuTemplateLineList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine> SelectSKUTemplateLineByUID(string UID)
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
                                        sku_template_uid AS SKUTemplateUID,
                                        sku_group_type_uid AS SKUGroupTypeUID,
                                        sku_group_uid AS SKUGroupUID,
                                        is_excluded AS IsExcluded
                                        FROM
                                        sku_template_line
                                        WHERE uid = @UID";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "UID", UID }
                    };

                Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine? skuTemplateLineDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine>(sql, parameters);

                return skuTemplateLineDetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> CreateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine)
        {
            int retVal = -1;
            try
            {
                var sql = @"
                                    INSERT INTO sku_template_line (
                                        uid,
                                        created_by,
                                        created_time,
                                        modified_by,
                                        modified_time,
                                        server_add_time,
                                        server_modified_time,
                                        sku_template_uid,
                                        sku_group_type_uid,
                                        sku_group_uid,
                                        is_excluded
                                    ) VALUES (
                                        @UID,
                                        @CreatedBy,
                                        @CreatedTime,
                                        @ModifiedBy,
                                        @ModifiedTime,
                                        @ServerAddTime,
                                        @ServerModifiedTime,
                                        @SKUTemplateUID,
                                        @SKUGroupTypeUID,
                                        @SKUGroupUID,
                                        @IsExcluded
                                    )";

                retVal = await ExecuteNonQueryAsync(sql, sKUTemplateLine);
                return retVal;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUTemplateLine(Winit.Modules.SKU.Model.Interfaces.ISKUTemplateLine sKUTemplateLine)
        {
            try
            {
                var sql = @"UPDATE sku_template_line
                                SET
                                    modified_by = @ModifiedBy,
                                    modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,
                                    sku_template_uid = @SKUTemplateUID,
                                    sku_group_type_uid = @SKUGroupTypeUID,
                                    sku_group_uid = @SKUGroupUID,
                                    is_excluded = @IsExcluded
                                WHERE
                                    uid = @UID;";
                return await ExecuteNonQueryAsync(sql, sKUTemplateLine);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUTemplateLine(string UID)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "UID", UID }
                };
                var sql = @"
                    DELETE FROM sku_template_line
                    WHERE uid = @UID";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region CUDSKUTemplateAndLine

        public async Task<int> CUDSKUTemplateAndLine(SKUTemplateMaster sKUTemplateMaster)
        {
            List<ISKUTemplateLine> skuTemplateLines = null;
            if (sKUTemplateMaster.SKUTemplateLineList != null)
            {
                skuTemplateLines = sKUTemplateMaster.SKUTemplateLineList.ToList<ISKUTemplateLine>();
            }
            int count = -1;
            try
            {
                if (sKUTemplateMaster.SKUTemplate != null)
                {
                    count = await CUDSKUTemplate(sKUTemplateMaster.SKUTemplate);
                }
                if (sKUTemplateMaster.SKUTemplateLineList != null && sKUTemplateMaster.SKUTemplateLineList.Any())
                {
                    count = await CUDSKUTemplateLine(skuTemplateLines);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        #endregion
        #region CUDSKUTemplate
        private async Task<int> CUDSKUTemplate(Winit.Modules.SKU.Model.Interfaces.ISKUTemplate sKUTemplate)
        {
            int count = -1;
            try
            {
                var existingRec = await SelectSKUTemplateByUID(sKUTemplate.UID);
                if (existingRec == null)
                {
                    count = await _skuTemplateDL.CreateSKUTemplate(sKUTemplate);
                }
                else
                {
                    count = await _skuTemplateDL.UpdateSKUTemplate(sKUTemplate);
                }
            }
            catch
            {
                throw;
            }

            return count;
        }
        private async Task<string> SelectSKUTemplateByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UID",  UID}
                };
            var sql = @"SELECT  uid FROM sku_template WHERE uid = @UID;";
            return await ExecuteSingleAsync<string>(sql, parameters);

        }
        #endregion
        #region CUDSKUTemplateLine
        private async Task<int> CUDSKUTemplateLine(List<ISKUTemplateLine> skuTemplateLines)
        {
            int count = -1;
            if (skuTemplateLines == null || skuTemplateLines.Count == 0)
            {
                return count;
            }

            List<string> uidList = skuTemplateLines.Select(stl => stl.UID).ToList();

            try
            {
                List<string> existingUIDs = await CheckSKUTemplateLineByUIDs(uidList);
                List<ISKUTemplateLine> newSKUTemplateLines = skuTemplateLines.Where(e => !existingUIDs.Contains(e.UID)).ToList();
                List<ISKUTemplateLine> existingSKUTemplateLines = skuTemplateLines.Where(e => existingUIDs.Contains(e.UID)).ToList();
                if (existingSKUTemplateLines.Any())
                {
                    count = await UpdateSKUTemplateLine(existingSKUTemplateLines);
                }
                if (newSKUTemplateLines.Any())
                {
                    count = await InsertSKUTemplateLine(newSKUTemplateLines);
                }
            }
            catch
            {
                throw;
            }
            return count;
        }
        private async Task<List<string>> CheckSKUTemplateLineByUIDs(List<string> UIDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UIDs",  UIDs}
                };
            var sql = @"SELECT uid FROM  sku_template_line  WHERE  UID In (@UIDs)";
            return await ExecuteQueryAsync<string>(sql, parameters);
        }
        private async Task<int> InsertSKUTemplateLine(List<ISKUTemplateLine> skuTemplateLines)
        {
            int count = -1;
            try
            {
                string sql = @"INSERT INTO sku_template_line (
                                        uid,
                                        created_by,
                                        created_time,
                                        modified_by,
                                        modified_time,
                                        server_add_time,
                                        server_modified_time,
                                        sku_template_uid,
                                        sku_group_type_uid,
                                        sku_group_uid,
                                        is_excluded
                                    ) VALUES (
                                        @UID,
                                        @CreatedBy,
                                        @CreatedTime,
                                        @ModifiedBy,
                                        @ModifiedTime,
                                        @ServerAddTime,
                                        @ServerModifiedTime,
                                        @SKUTemplateUID,
                                        @SKUGroupTypeUID,
                                        @SKUGroupUID,
                                        @IsExcluded
                                    )";
                count = await ExecuteNonQueryAsync(sql, skuTemplateLines);
            }
            catch (Exception)
            {
                throw;
            }

            return count;
        }
        private async Task<int> UpdateSKUTemplateLine(List<ISKUTemplateLine> skuTemplateLines)
        {
            int count = -1;
            try
            {
                string sql = @"UPDATE sku_template_line
                                SET
                                    modified_by = @ModifiedBy,
                                    modified_time = @ModifiedTime,
                                    server_modified_time = @ServerModifiedTime,
                                    sku_template_uid = @SKUTemplateUID,
                                    sku_group_type_uid = @SKUGroupTypeUID,
                                    sku_group_uid = @SKUGroupUID,
                                    is_excluded = @IsExcluded
                                WHERE
                                    uid = @UID;";

                count = await ExecuteNonQueryAsync(sql, skuTemplateLines);

            }
            catch (Exception)
            {
                throw;
            }

            return count;
        }
        #endregion
        public async Task<int> DeleteSKUTemplateLines(List<string> uIDs)
        {
            try
            {
                var parameters = new { UIDs = uIDs };
                var sql = @" DELETE FROM sku_template_line WHERE uid IN @UIDs;";
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                throw;
            }
        }
    }
}
