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
    public class PGSQLSKUConfigDL : Winit.Modules.Base.DL.DBManager.PostgresDBManager,ISKUConfigDL
    {
        public PGSQLSKUConfigDL(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider, config)
        {
        }
        public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>> SelectAllSKUConfigDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            try
            {
                var sql = new StringBuilder(@"SELECT 
                    id AS Id,
                    uid AS UID,
                    ss AS SS,
                    created_by AS CreatedBy,
                    created_time AS CreatedTime,
                    modified_by AS ModifiedBy,
                    modified_time AS ModifiedTime,
                    server_add_time AS ServerAddTime,
                    server_modified_time AS ServerModifiedTime,
                    sc.org_uid AS OrgUID,
                    sc.name AS Name,
                    sc.distribution_channel_org_uid AS DistributionChannelOrgUid,
                    sc.sku_uid AS SKUUID,
                    sc.can_buy AS CanBuy,
                    sc.can_sell AS CanSell,
                    sc.buying_uom AS BuyingUOM,
                    sc.selling_uom AS SellingUOM,
                    sc.is_active AS IsActive,
                    o.name AS DistributionName
                FROM 
                    sku_config AS sc
                JOIN 
                    org AS o ON o.uid = sc.distribution_channel_org_uid");
                var sqlCount = new StringBuilder();
                if (isCountRequired)
                {
                    sqlCount = new StringBuilder(@"SELECT COUNT(1) AS Cnt FROM sku_config");
                }
                var parameters = new Dictionary<string, object>();

                if (filterCriterias != null && filterCriterias.Count > 0)
                {
                    StringBuilder sbFilterCriteria = new StringBuilder();
                    sbFilterCriteria.Append(" WHERE ");
                    AppendFilterCriteria<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>(filterCriterias, sbFilterCriteria, parameters);

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

                Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUConfig>().GetType();
                IEnumerable<Model.Interfaces.ISKUConfig> skuConfigList = await ExecuteQueryAsync<Model.Interfaces.ISKUConfig>(sql.ToString(), parameters, type);
                int totalCount = 0;
                if (isCountRequired)
                {
                    totalCount = await ExecuteScalarAsync<int>(sqlCount.ToString(), parameters);
                }

                PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> pagedResponse = new PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>
                {
                    PagedData = skuConfigList,
                    TotalCount = totalCount
                };

                return pagedResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> SelectSKUConfigByUID(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"SELECT 
                id AS Id,
                uid AS UID,
                ss AS SS,
                created_by AS CreatedBy,
                created_time AS CreatedTime,
                modified_by AS ModifiedBy,
                modified_time AS ModifiedTime,
                server_add_time AS ServerAddTime,
                server_modified_time AS ServerModifiedTime,
                org_uid AS OrgUID,
                name AS Name,
                distribution_channel_org_uid AS DistributionChannelOrgUID,
                sku_uid AS SKUUID,
                can_buy AS CanBuy,
                can_sell AS CanSell,
                buying_uom AS BuyingUOM,
                selling_uom AS SellingUOM,
                is_active AS IsActive
            FROM 
                sku_config
            WHERE 
                uid = @UID";
            Type type = _serviceProvider.GetRequiredService<Model.Interfaces.ISKUConfig>().GetType();
            Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfigDetails = await ExecuteSingleAsync<Winit.Modules.SKU.Model.Interfaces.ISKUConfig>(sql, parameters,type);
            return skuConfigDetails;
        }
        public async Task<int> CreateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig)
        {
            try
            {
                var sql   = @"INSERT INTO sku_config (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time, 
                            org_uid, distribution_channel_org_uid, sku_uid, can_buy, can_sell, buying_uom, selling_uom, is_active)
                            VALUES(@UID,@CreatedBy,@CreatedTime,@ModifiedBy,@ModifiedTime,@ServerAddTime,@ServerModifiedTime,@OrgUID,@DistributionChannelOrgUID,@SKUUID,@CanBuy,
                            @CanSell,@BuyingUOM,@SellingUOM,@IsActive);";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",skuConfig.UID},
                   {"CreatedBy",skuConfig.CreatedBy},
                   {"CreatedTime",skuConfig.CreatedTime},
                   {"ModifiedBy",skuConfig.ModifiedBy},
                   {"ModifiedTime",skuConfig.ModifiedTime},
                   {"ServerAddTime",skuConfig.ServerAddTime},
                   {"ServerModifiedTime",skuConfig.ServerModifiedTime},
                   {"OrgUID",skuConfig.OrgUID},
                   {"DistributionChannelOrgUID",skuConfig.DistributionChannelOrgUID},
                   {"SKUUID",skuConfig.SKUUID},
                   {"CanBuy",skuConfig.CanBuy},
                   {"CanSell",skuConfig.CanSell},
                   {"BuyingUOM",skuConfig.BuyingUOM},
                   {"SellingUOM",skuConfig.SellingUOM},
                   {"IsActive",skuConfig.IsActive},
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> UpdateSKUConfig(Winit.Modules.SKU.Model.Interfaces.ISKUConfig skuConfig)
        {
            try
            {
                var sql = @"UPDATE sku_config 
                SET 
                    modified_by = @ModifiedBy, 
                    modified_time = @ModifiedTime, 
                    server_modified_time = @ServerModifiedTime, 
                    org_uid = @OrgUID, 
                    distribution_channel_org_uid = @DistributionChannelOrgUID, 
                    sku_uid = @SKUUID, 
                    can_buy = @CanBuy, 
                    can_sell = @CanSell, 
                    buying_uom = @BuyingUOM, 
                    selling_uom = @SellingUOM, 
                    is_active = @IsActive
                WHERE 
                    uid = @UID;";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                   {"UID",skuConfig.UID},
                   {"ModifiedBy",skuConfig.ModifiedBy},
                   {"ModifiedTime",skuConfig.ModifiedTime},
                   {"ServerModifiedTime",skuConfig.ServerModifiedTime},
                   {"OrgUID",skuConfig.OrgUID},
                   {"DistributionChannelOrgUID",skuConfig.DistributionChannelOrgUID},
                   {"SKUUID",skuConfig.SKUUID},
                   {"CanBuy",skuConfig.CanBuy},
                   {"CanSell",skuConfig.CanSell},
                   {"BuyingUOM",skuConfig.BuyingUOM},
                   {"SellingUOM",skuConfig.SellingUOM},
                   {"IsActive",skuConfig.IsActive},
                  
                };
                return await ExecuteNonQueryAsync(sql, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteSKUConfig(string UID)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UID" , UID}
            };
            var sql = @"DELETE FROM sku_config WHERE uid = @UID;";
            return await ExecuteNonQueryAsync(sql, parameters);
        }
    }
}
